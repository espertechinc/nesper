///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public class ContextControllerHashFactory : ContextControllerFactoryBase, ContextControllerFactory
    {
        private readonly ContextDetailHash _hashedSpec;
        private readonly IList<FilterSpecCompiled> _filtersSpecsNestedContexts;
        private readonly ContextStateCache _stateCache;
        private readonly ContextStatePathValueBinding _binding;
    
        private IDictionary<String, Object> _contextBuiltinProps;
    
        public ContextControllerHashFactory(ContextControllerFactoryContext factoryContext, ContextDetailHash hashedSpec, IList<FilterSpecCompiled> filtersSpecsNestedContexts, ContextStateCache stateCache)
            : base(factoryContext)
        {
            _hashedSpec = hashedSpec;
            _filtersSpecsNestedContexts = filtersSpecsNestedContexts;
            _stateCache = stateCache;
            _binding = stateCache.GetBinding(typeof(int));
        }
    
        public bool HasFiltersSpecsNestedContexts() {
            return _filtersSpecsNestedContexts != null && !_filtersSpecsNestedContexts.IsEmpty();
        }

        public override ContextStateCache StateCache
        {
            get { return _stateCache; }
        }

        public ContextStatePathValueBinding Binding
        {
            get { return _binding; }
        }

        public override void ValidateFactory()
        {
            ValidatePopulateContextDesc();
            _contextBuiltinProps = ContextPropertyEventType.GetHashType();
        }
    
        public override ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement)
        {
            var streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(statement.StatementSpec);
            ContextControllerPartitionedUtil.ValidateStatementForContext(FactoryContext.ContextName, statement, streamAnalysis, GetItemEventTypes(_hashedSpec), FactoryContext.ServicesContext.NamedWindowService);
            return new ContextControllerStatementCtxCacheFilters(streamAnalysis.Filters);
        }
    
        public override void PopulateFilterAddendums(IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum, ContextControllerStatementDesc statement, Object key, int contextId)
        {
            var statementInfo = (ContextControllerStatementCtxCacheFilters) statement.Caches[FactoryContext.NestingLevel - 1];
            var assignedContextPartition = key.AsInt();
            var code = assignedContextPartition % _hashedSpec.Granularity;
            GetAddendumFilters(filterAddendum, code, statementInfo.FilterSpecs, _hashedSpec, statement);
        }
    
        public void PopulateContextInternalFilterAddendums(ContextInternalFilterAddendum filterAddendum, Object key)
        {
            var assignedContextPartition = key.AsInt();
            var code = assignedContextPartition % _hashedSpec.Granularity;
            GetAddendumFilters(filterAddendum.FilterAddendum, code, _filtersSpecsNestedContexts, _hashedSpec, null);
        }
    
        public override FilterSpecLookupable GetFilterLookupable(EventType eventType) {
            foreach (var hashItem in _hashedSpec.Items) {
                if (hashItem.FilterSpecCompiled.FilterForEventType == eventType) {
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
                    return () => new StatementAIResourceRegistry(new AIRegistryAggregationMultiPerm(), new AIRegistryExprMultiPerm());
                }
                else
                {
                    return () => new StatementAIResourceRegistry(new AIRegistryAggregationMap(), new AIRegistryExprMap());
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

        public override ContextController CreateNoCallback(int pathId, ContextControllerLifecycleCallback callback)
        {
            return new ContextControllerHash(pathId, callback, this);
        }
    
        public override ContextPartitionIdentifier KeyPayloadToIdentifier(Object payload)
        {
            return new ContextPartitionIdentifierHash(payload.AsInt());
        }
    
        private ICollection<EventType> GetItemEventTypes(ContextDetailHash hashedSpec)
        {
            return hashedSpec.Items.Select(item => item.FilterSpecCompiled.FilterForEventType).ToList();
        }

        private void ValidatePopulateContextDesc()
        {
            if (_hashedSpec.Items.IsEmpty()) {
                throw new ExprValidationException("Empty list of hash items");
            }
    
            foreach (var item in _hashedSpec.Items) {
                if (item.Function.Parameters.IsEmpty()) {
                    throw new ExprValidationException("For context '" + FactoryContext.ContextName + "' expected one or more parameters to the hash function, but found no parameter list");
                }
    
                // determine type of hash to use
                var hashFuncName = item.Function.Name;
                var hashFunction = HashFunctionEnumExtensions.Determine(FactoryContext.ContextName, hashFuncName);
                Pair<Type, EngineImportSingleRowDesc> hashSingleRowFunction = null;
                if (hashFunction == null) {
                    try {
                        hashSingleRowFunction = FactoryContext.AgentInstanceContextCreate.StatementContext.MethodResolutionService.ResolveSingleRow(hashFuncName);
                    }
                    catch (Exception) {
                        // expected
                    }
    
                    if (hashSingleRowFunction == null) {
                        throw new ExprValidationException("For context '" + FactoryContext.ContextName + "' expected a hash function that is any of {" + HashFunctionEnumExtensions.StringList +
                            "} or a plug-in single-row function or script but received '" + hashFuncName + "'");
                    }
                }
    
                // get first parameter
                var paramExpr = item.Function.Parameters[0];
                var eval = paramExpr.ExprEvaluator;
                var paramType = eval.ReturnType;
                EventPropertyGetter getter;
    
                if (hashFunction == HashFunctionEnum.CONSISTENT_HASH_CRC32) {
                    if (item.Function.Parameters.Count > 1 || paramType != typeof(String)) {
                        getter = new ContextControllerHashedGetterCRC32Serialized(FactoryContext.AgentInstanceContextCreate.StatementContext.StatementName, item.Function.Parameters, _hashedSpec.Granularity);
                    }
                    else {
                        getter = new ContextControllerHashedGetterCRC32Single(eval, _hashedSpec.Granularity);
                    }
                }
                else if (hashFunction == HashFunctionEnum.HASH_CODE) {
                    if (item.Function.Parameters.Count > 1) {
                        getter = new ContextControllerHashedGetterHashMultiple(item.Function.Parameters, _hashedSpec.Granularity);
                    }
                    else {
                        getter = new ContextControllerHashedGetterHashSingle(eval, _hashedSpec.Granularity);
                    }
                }
                else if (hashSingleRowFunction != null)
                {
                    getter = new ContextControllerHashedGetterSingleRow(
                        FactoryContext.AgentInstanceContextCreate.StatementContext.StatementName, hashFuncName,
                        hashSingleRowFunction, item.Function.Parameters, _hashedSpec.Granularity,
                        FactoryContext.AgentInstanceContextCreate.StatementContext.MethodResolutionService,
                        item.FilterSpecCompiled.FilterForEventType,
                        FactoryContext.AgentInstanceContextCreate.StatementContext.EventAdapterService,
                        FactoryContext.AgentInstanceContextCreate.StatementId,
                        FactoryContext.ServicesContext.TableService);
                }
                else {
                    throw new ArgumentException("Unrecognized hash code function '" + hashFuncName + "'");
                }

                var expression = item.Function.Name + "(" + paramExpr + ")";
                var lookupable = new FilterSpecLookupable(expression, getter, typeof(int));
                item.Lookupable = lookupable;
            }
        }
    
        // Compare filters in statement with filters in segmented context, addendum filter compilation
        /// <summary>
        /// Gets the addendum filters.
        /// </summary>
        /// <param name="addendums">The addendums.</param>
        /// <param name="agentInstanceId">The agent instance identifier.</param>
        /// <param name="filtersSpecs">The filters specs.</param>
        /// <param name="hashSpec">The hash spec.</param>
        /// <param name="statementDesc">The statement desc.</param>
        private static void GetAddendumFilters(
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums,
            int agentInstanceId,
            IList<FilterSpecCompiled> filtersSpecs,
            ContextDetailHash hashSpec,
            ContextControllerStatementDesc statementDesc)
        {
    
            // determine whether create-named-window
            var isCreateWindow = statementDesc != null && statementDesc.Statement.StatementSpec.CreateWindowDesc != null;
            if (!isCreateWindow) {
                foreach (var filtersSpec in filtersSpecs) {
    
                    var foundPartition = FindHashItemSpec(hashSpec, filtersSpec);
                    if (foundPartition == null) {
                        continue;
                    }
    
                    FilterValueSetParam filter = new FilterValueSetParamImpl(foundPartition.Lookupable, FilterOperator.EQUAL, agentInstanceId);

                    var addendum = new FilterValueSetParam[1][];
                    addendum[0] = new FilterValueSetParam[] { filter };

                    var partitionFilters = foundPartition.ParametersCompiled;
                    if (partitionFilters != null)
                    {
                        addendum = ContextControllerAddendumUtil.AddAddendum(partitionFilters, filter);
                    }

                    FilterValueSetParam[][] existing = addendums.Get(filtersSpec);
                    if (existing != null)
                    {
                        addendum = ContextControllerAddendumUtil.MultiplyAddendum(existing, addendum);
                    }

                    addendums[filtersSpec] = addendum;
                }
            }
            // handle segmented context for create-window
            else {
                var declaredAsName = statementDesc.Statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
                if (declaredAsName != null) {
                    foreach (var filterSpec in filtersSpecs) {
    
                        ContextDetailHashItem foundPartition = null;
                        foreach (var partitionItem in hashSpec.Items) {
                            if (partitionItem.FilterSpecCompiled.FilterForEventType.Name.Equals(declaredAsName)) {
                                foundPartition = partitionItem;
                                break;
                            }
                        }
    
                        if (foundPartition == null) {
                            continue;
                        }
    
                        FilterValueSetParam filter = new FilterValueSetParamImpl(foundPartition.Lookupable, FilterOperator.EQUAL, agentInstanceId);

                        var addendum = new FilterValueSetParam[1][];
                        addendum[0] = new FilterValueSetParam[] { filter };

                        var existing = addendums.Get(filterSpec);
                        if (existing != null)
                        {
                            addendum = ContextControllerAddendumUtil.MultiplyAddendum(existing, addendum);
                        }

                        addendums[filterSpec] = addendum;
                    }
                }
            }
        }
    
        public static ContextDetailHashItem FindHashItemSpec(ContextDetailHash hashSpec, FilterSpecCompiled filterSpec) {
            ContextDetailHashItem foundPartition = null;
            foreach (var partitionItem in hashSpec.Items) {
                var typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(filterSpec.FilterForEventType, partitionItem.FilterSpecCompiled.FilterForEventType);
                if (typeOrSubtype) {
                    foundPartition = partitionItem;
                }
            }
    
            return foundPartition;
        }

        public enum HashFunctionEnum
        {
            CONSISTENT_HASH_CRC32,
            HASH_CODE
        }

        public class HashFunctionEnumExtensions
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
}
