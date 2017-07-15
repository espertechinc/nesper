///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public abstract class ContextControllerHashFactoryBase
        : ContextControllerFactoryBase
        , ContextControllerFactory
    {
        private readonly ContextDetailHash _hashedSpec;
        private readonly IList<FilterSpecCompiled> _filtersSpecsNestedContexts;
        private IDictionary<string, Object> _contextBuiltinProps;

        private readonly IDictionary<EventType, FilterSpecLookupable> _nonPropertyExpressions =
            new Dictionary<EventType, FilterSpecLookupable>();

        protected ContextControllerHashFactoryBase(
            ContextControllerFactoryContext factoryContext,
            ContextDetailHash hashedSpec,
            IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext)
        {
            _hashedSpec = hashedSpec;
            _filtersSpecsNestedContexts = filtersSpecsNestedContexts;
        }

        // Compare filters in statement with filters in segmented context, addendum filter compilation
        public static void GetAddendumFilters(
            IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums,
            int hashCode,
            IList<FilterSpecCompiled> filtersSpecs,
            ContextDetailHash hashSpec,
            ContextControllerStatementDesc statementDesc)
        {
            foreach (var filtersSpec in filtersSpecs)
            {
                var addendum = GetAddendumFilters(filtersSpec, hashCode, hashSpec, statementDesc);
                if (addendum == null)
                {
                    continue;
                }

                var existing = addendums.Get(filtersSpec);
                if (existing != null)
                {
                    addendum = ContextControllerAddendumUtil.MultiplyAddendum(existing, addendum);
                }
                addendums.Put(filtersSpec, addendum);
            }
        }

        public static FilterValueSetParam[][] GetAddendumFilters(
            FilterSpecCompiled filterSpecCompiled,
            int hashCode,
            ContextDetailHash hashSpec,
            ContextControllerStatementDesc statementDesc)
        {

            // determine whether create-named-window
            var isCreateWindow = statementDesc != null && statementDesc.Statement.StatementSpec.CreateWindowDesc != null;
            ContextDetailHashItem foundPartition = null;

            if (!isCreateWindow)
            {
                foundPartition = FindHashItemSpec(hashSpec, filterSpecCompiled);
            }
            else
            {
                string declaredAsName = statementDesc.Statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
                foreach (var partitionItem in hashSpec.Items)
                {
                    if (partitionItem.FilterSpecCompiled.FilterForEventType.Name.Equals(declaredAsName))
                    {
                        foundPartition = partitionItem;
                        break;
                    }
                }
            }

            if (foundPartition == null)
            {
                return null;
            }

            var filter = new FilterValueSetParamImpl(foundPartition.Lookupable, FilterOperator.EQUAL, hashCode);

            var addendum = new FilterValueSetParam[1][];
            addendum[0] = new FilterValueSetParam[]
            {
                filter
            };

            var partitionFilters = foundPartition.ParametersCompiled;
            if (partitionFilters != null)
            {
                addendum = ContextControllerAddendumUtil.AddAddendum(partitionFilters, filter);
            }
            return addendum;
        }

        public static ContextDetailHashItem FindHashItemSpec(ContextDetailHash hashSpec, FilterSpecCompiled filterSpec)
        {
            ContextDetailHashItem foundPartition = null;
            foreach (var partitionItem in hashSpec.Items)
            {
                var typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(
                    filterSpec.FilterForEventType, partitionItem.FilterSpecCompiled.FilterForEventType);
                if (typeOrSubtype)
                {
                    foundPartition = partitionItem;
                }
            }

            return foundPartition;
        }

        public bool HasFiltersSpecsNestedContexts
        {
            get { return _filtersSpecsNestedContexts != null && !_filtersSpecsNestedContexts.IsEmpty(); }
        }

        public override void ValidateFactory()
        {
            ValidatePopulateContextDesc();
            _contextBuiltinProps = ContextPropertyEventType.GetHashType();
        }

        public override ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement)
        {
            var factoryContext = base.FactoryContext;
            var streamAnalysis =
                StatementSpecCompiledAnalyzer.AnalyzeFilters(statement.StatementSpec);
            ContextControllerPartitionedUtil.ValidateStatementForContext(
                factoryContext.ContextName, statement, streamAnalysis, GetItemEventTypes(_hashedSpec),
                factoryContext.ServicesContext.NamedWindowMgmtService);
            // register non-property expression to be able to recreated indexes
            foreach (var entry in _nonPropertyExpressions)
            {
                factoryContext.ServicesContext.FilterNonPropertyRegisteryService.RegisterNonPropertyExpression(
                    statement.StatementContext.StatementName, entry.Key, entry.Value);
            }
            return new ContextControllerStatementCtxCacheFilters(streamAnalysis.Filters);
        }

        public void PopulateFilterAddendums(
            IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum,
            ContextControllerStatementDesc statement,
            Object key,
            int contextId)
        {
            var factoryContext = base.FactoryContext;
            var statementInfo = (ContextControllerStatementCtxCacheFilters) statement.Caches[factoryContext.NestingLevel - 1];
            var assignedContextPartition = (int) key;
            var code = assignedContextPartition%_hashedSpec.Granularity;
            GetAddendumFilters(filterAddendum, code, statementInfo.FilterSpecs, _hashedSpec, statement);
        }

        public void PopulateContextInternalFilterAddendums(ContextInternalFilterAddendum filterAddendum, Object key)
        {
            var assignedContextPartition = (int) key;
            var code = assignedContextPartition%_hashedSpec.Granularity;
            GetAddendumFilters(filterAddendum.FilterAddendum, code, _filtersSpecsNestedContexts, _hashedSpec, null);
        }

        public override FilterSpecLookupable GetFilterLookupable(EventType eventType)
        {
            foreach (var hashItem in _hashedSpec.Items)
            {
                if (hashItem.FilterSpecCompiled.FilterForEventType == eventType)
                {
                    return hashItem.Lookupable;
                }
            }
            return null;
        }

        public override bool IsSingleInstanceContext
        {
            get { return false; }
        }

        public override StatementAIResourceRegistryFactory StatementAIResourceRegistryFactory
        {
            get
            {
                if (_hashedSpec.Granularity <= 65536)
                {
                    return () => new StatementAIResourceRegistry(
                        new AIRegistryAggregationMultiPerm(), new AIRegistryExprMultiPerm());
                }
                else
                {
                    return () => new StatementAIResourceRegistry(
                        new AIRegistryAggregationMap(), new AIRegistryExprMap());
                }
            }
        }

        public override IList<ContextDetailPartitionItem> ContextDetailPartitionItems
        {
            get { return Collections.GetEmptyList<ContextDetailPartitionItem>(); }
        }

        public override ContextDetail ContextDetail
        {
            get { return _hashedSpec; }
        }

        public ContextDetailHash HashedSpec
        {
            get { return _hashedSpec; }
        }

        public override IDictionary<string, object> ContextBuiltinProps
        {
            get { return _contextBuiltinProps; }
        }

        public override ContextPartitionIdentifier KeyPayloadToIdentifier(Object payload)
        {
            return new ContextPartitionIdentifierHash(payload.AsInt());
        }

        private ICollection<EventType> GetItemEventTypes(ContextDetailHash hashedSpec)
        {
            var itemEventTypes = new List<EventType>();
            foreach (var item in hashedSpec.Items)
            {
                itemEventTypes.Add(item.FilterSpecCompiled.FilterForEventType);
            }
            return itemEventTypes;
        }

        private void ValidatePopulateContextDesc()
        {

            if (_hashedSpec.Items.IsEmpty())
            {
                throw new ExprValidationException("Empty list of hash items");
            }

            var factoryContext = base.FactoryContext;
            foreach (var item in _hashedSpec.Items)
            {
                if (item.Function.Parameters.IsEmpty())
                {
                    throw new ExprValidationException(
                        "For context '" + factoryContext.ContextName +
                        "' expected one or more parameters to the hash function, but found no parameter list");
                }

                // determine type of hash to use
                var hashFuncName = item.Function.Name;
                var hashFunction = HashFunctionEnumExtensions.Determine(factoryContext.ContextName, hashFuncName);
                Pair<Type, EngineImportSingleRowDesc> hashSingleRowFunction = null;
                if (hashFunction == null)
                {
                    try
                    {
                        hashSingleRowFunction =
                            factoryContext.AgentInstanceContextCreate.StatementContext.EngineImportService
                                .ResolveSingleRow(hashFuncName);
                    }
                    catch (Exception)
                    {
                        // expected
                    }

                    if (hashSingleRowFunction == null)
                    {
                        throw new ExprValidationException(
                            "For context '" + factoryContext.ContextName + "' expected a hash function that is any of {" +
                            HashFunctionEnumExtensions.StringList +
                            "} or a plug-in single-row function or script but received '" + hashFuncName + "'");
                    }
                }

                // get first parameter
                var paramExpr = item.Function.Parameters[0];
                var eval = paramExpr.ExprEvaluator;
                var paramType = eval.ReturnType;
                EventPropertyGetter getter;

                if (hashFunction == HashFunctionEnum.CONSISTENT_HASH_CRC32)
                {
                    if (item.Function.Parameters.Count > 1 || paramType != typeof (string))
                    {
                        getter =
                            new ContextControllerHashedGetterCRC32Serialized(
                                factoryContext.AgentInstanceContextCreate.StatementContext.StatementName,
                                item.Function.Parameters, _hashedSpec.Granularity);
                    }
                    else
                    {
                        getter = new ContextControllerHashedGetterCRC32Single(eval, _hashedSpec.Granularity);
                    }
                }
                else if (hashFunction == HashFunctionEnum.HASH_CODE)
                {
                    if (item.Function.Parameters.Count > 1)
                    {
                        getter = new ContextControllerHashedGetterHashMultiple(
                            item.Function.Parameters, _hashedSpec.Granularity);
                    }
                    else
                    {
                        getter = new ContextControllerHashedGetterHashSingle(eval, _hashedSpec.Granularity);
                    }
                }
                else if (hashSingleRowFunction != null)
                {
                    getter =
                        new ContextControllerHashedGetterSingleRow(
                            factoryContext.AgentInstanceContextCreate.StatementContext.StatementName, hashFuncName,
                            hashSingleRowFunction, item.Function.Parameters, _hashedSpec.Granularity,
                            factoryContext.AgentInstanceContextCreate.StatementContext.EngineImportService,
                            item.FilterSpecCompiled.FilterForEventType,
                            factoryContext.AgentInstanceContextCreate.StatementContext.EventAdapterService,
                            factoryContext.AgentInstanceContextCreate.StatementId,
                            factoryContext.ServicesContext.TableService,
                            factoryContext.ServicesContext.EngineURI);
                }
                else
                {
                    throw new ArgumentException("Unrecognized hash code function '" + hashFuncName + "'");
                }

                // create and register expression
                var expression = item.Function.Name + "(" +
                                    ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(paramExpr) + ")";
                var lookupable = new FilterSpecLookupable(expression, getter, typeof (int?), true);
                item.Lookupable = lookupable;
                factoryContext.ServicesContext.FilterNonPropertyRegisteryService.RegisterNonPropertyExpression(
                    factoryContext.AgentInstanceContextCreate.StatementName, item.FilterSpecCompiled.FilterForEventType,
                    lookupable);
                _nonPropertyExpressions.Put(item.FilterSpecCompiled.FilterForEventType, lookupable);
            }
        }

        public enum HashFunctionEnum
        {
            CONSISTENT_HASH_CRC32,
            HASH_CODE
        }

        public static class HashFunctionEnumExtensions
        {
            public static HashFunctionEnum? Determine(String contextName, String name)
            {
                return EnumHelper.ParseBoxed<HashFunctionEnum>(name, true);
            }

            public static string StringList
            {
                get
                {
                    var message = new StringWriter();
                    var delimiter = "";
                    foreach (var name in EnumHelper.GetNames<HashFunctionEnum>())
                    {
                        message.Write(delimiter);
                        message.Write(name.ToLower());
                        delimiter = ", ";
                    }
                    return message.ToString();
                }
            }
        }
    }

} // end of namespace
