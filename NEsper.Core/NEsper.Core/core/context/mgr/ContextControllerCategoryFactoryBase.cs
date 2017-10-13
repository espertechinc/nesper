///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public abstract class ContextControllerCategoryFactoryBase
        : ContextControllerFactoryBase
        , ContextControllerFactory
    {
        private readonly ContextDetailCategory _categorySpec;
        private readonly IList<FilterSpecCompiled> _filtersSpecsNestedContexts;

        private IDictionary<string, object> _contextBuiltinProps;

        protected ContextControllerCategoryFactoryBase(ContextControllerFactoryContext factoryContext, ContextDetailCategory categorySpec, IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext)
        {
            _categorySpec = categorySpec;
            _filtersSpecsNestedContexts = filtersSpecsNestedContexts;
        }

        public bool HasFiltersSpecsNestedContexts
        {
            get { return _filtersSpecsNestedContexts != null && !_filtersSpecsNestedContexts.IsEmpty(); }
        }

        public override void ValidateFactory()
        {
            if (_categorySpec.Items.IsEmpty())
            {
                throw new ExprValidationException("EmptyFalse list of partition items");
            }
            _contextBuiltinProps = ContextPropertyEventType.GetCategorizedType();
        }

        public override ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement)
        {
            var streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(statement.StatementSpec);
            ValidateStatementForContext(statement, streamAnalysis);
            return new ContextControllerStatementCtxCacheFilters(streamAnalysis.Filters);
        }

        public override void PopulateFilterAddendums(
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum,
            ContextControllerStatementDesc statement,
            object categoryIndex,
            int contextId)
        {
            var statementInfo = (ContextControllerStatementCtxCacheFilters)statement.Caches[FactoryContext.NestingLevel - 1];
            var category = _categorySpec.Items[categoryIndex.AsInt()];
            GetAddendumFilters(filterAddendum, category, _categorySpec, statementInfo.FilterSpecs, statement);
        }

        public void PopulateContextInternalFilterAddendums(ContextInternalFilterAddendum filterAddendum, object categoryIndex)
        {
            var category = _categorySpec.Items[categoryIndex.AsInt()];
            GetAddendumFilters(filterAddendum.FilterAddendum, category, _categorySpec, _filtersSpecsNestedContexts, null);
        }

        public override FilterSpecLookupable GetFilterLookupable(EventType eventType)
        {
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
                return () => new StatementAIResourceRegistry(
                    new AIRegistryAggregationMultiPerm(), new AIRegistryExprMultiPerm());
            }
        }

        public override IList<ContextDetailPartitionItem> ContextDetailPartitionItems
        {
            get { return Collections.GetEmptyList<ContextDetailPartitionItem>(); }
        }

        public override ContextDetail ContextDetail
        {
            get { return _categorySpec; }
        }

        public ContextDetailCategory CategorySpec
        {
            get { return _categorySpec; }
        }

        public override IDictionary<string, object> ContextBuiltinProps
        {
            get { return _contextBuiltinProps; }
        }

        public override ContextPartitionIdentifier KeyPayloadToIdentifier(object payload)
        {
            var index = payload.AsInt();
            return new ContextPartitionIdentifierCategory(_categorySpec.Items[index].Name);
        }

        private void ValidateStatementForContext(ContextControllerStatementBase statement, StatementSpecCompiledAnalyzerResult streamAnalysis)
        {
            var filters = streamAnalysis.Filters;

            var isCreateWindow = statement.StatementSpec.CreateWindowDesc != null;
            var message = "Category context '" + FactoryContext.ContextName + "' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement";

            // if no create-window: at least one of the filters must match one of the filters specified by the context
            if (!isCreateWindow)
            {
                foreach (var filter in filters)
                {
                    var stmtFilterType = filter.FilterForEventType;
                    var contextType = _categorySpec.FilterSpecCompiled.FilterForEventType;
                    if (Equals(stmtFilterType, contextType))
                    {
                        return;
                    }
                    if (EventTypeUtility.IsTypeOrSubTypeOf(stmtFilterType, contextType))
                    {
                        return;
                    }
                }

                if (!filters.IsEmpty())
                {
                    throw new ExprValidationException(message);
                }
                return;
            }

            // validate create-window
            var declaredAsName = statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
            if (declaredAsName != null)
            {
                if (_categorySpec.FilterSpecCompiled.FilterForEventType.Name.Equals(declaredAsName))
                {
                    return;
                }
                throw new ExprValidationException(message);
            }
        }

        // Compare filters in statement with filters in segmented context, addendum filter compilation
        private static void GetAddendumFilters(
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums,
            ContextDetailCategoryItem category,
            ContextDetailCategory categorySpec,
            IList<FilterSpecCompiled> filters,
            ContextControllerStatementDesc statement)
        {
            // determine whether create-named-window
            var isCreateWindow = statement != null && statement.Statement.StatementSpec.CreateWindowDesc != null;
            if (!isCreateWindow)
            {
                foreach (var filtersSpec in filters)
                {

                    var typeOrSubtype = EventTypeUtility.IsTypeOrSubTypeOf(filtersSpec.FilterForEventType, categorySpec.FilterSpecCompiled.FilterForEventType);
                    if (!typeOrSubtype)
                    {
                        continue;   // does not apply
                    }
                    AddAddendums(addendums, filtersSpec, category, categorySpec);
                }
            }
            else
            {
                // handle segmented context for create-window
                var declaredAsName = statement.Statement.StatementSpec.CreateWindowDesc.AsEventTypeName;
                if (declaredAsName != null)
                {
                    foreach (var filtersSpec in filters)
                    {
                        AddAddendums(addendums, filtersSpec, category, categorySpec);
                    }
                }
            }
        }

        private static void AddAddendums(
            IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums,
            FilterSpecCompiled filtersSpec,
            ContextDetailCategoryItem category,
            ContextDetailCategory categorySpec)
        {
            var categoryEventFilters = categorySpec.FilterParamsCompiled;
            var categoryItemFilters = category.CompiledFilterParam;

            var addendum = ContextControllerAddendumUtil.MultiplyAddendum(
                categoryEventFilters, categoryItemFilters);

            var existingFilters = addendums.Get(filtersSpec);
            if (existingFilters != null)
            {
                addendum = ContextControllerAddendumUtil.MultiplyAddendum(existingFilters, addendum);
            }

            addendums.Put(filtersSpec, addendum);
        }
    }
} // end of namespace
