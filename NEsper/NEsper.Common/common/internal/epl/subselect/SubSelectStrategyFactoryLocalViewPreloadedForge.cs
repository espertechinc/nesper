///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectStrategyFactoryLocalViewPreloadedForge : SubSelectStrategyFactoryForge
    {
        private readonly AggregationServiceForgeDesc aggregationServiceForgeDesc;
        private readonly bool correlatedSubquery;
        private readonly ExprNode filterExprNode;
        private readonly ExprNode[] groupKeys;
        private readonly Pair<EventTableFactoryFactoryForge, SubordTableLookupStrategyFactoryForge> lookupStrategy;
        private readonly NamedWindowMetaData namedWindow;
        private readonly ExprNode namedWindowFilterExpr;
        private readonly QueryGraphForge namedWindowFilterQueryGraph;
        private readonly int subqueryNumber;
        private readonly ViewResourceDelegateDesc viewResourceDelegateDesc;

        public SubSelectStrategyFactoryLocalViewPreloadedForge(
            IList<ViewFactoryForge> viewForges,
            ViewResourceDelegateDesc viewResourceDelegateDesc,
            Pair<EventTableFactoryFactoryForge, SubordTableLookupStrategyFactoryForge> lookupStrategy,
            ExprNode filterExprNode,
            bool correlatedSubquery,
            AggregationServiceForgeDesc aggregationServiceForgeDesc,
            int subqueryNumber,
            ExprNode[] groupKeys,
            NamedWindowMetaData namedWindow,
            ExprNode namedWindowFilterExpr,
            QueryGraphForge namedWindowFilterQueryGraph)
        {
            ViewForges = viewForges;
            this.viewResourceDelegateDesc = viewResourceDelegateDesc;
            this.lookupStrategy = lookupStrategy;
            this.filterExprNode = filterExprNode;
            this.correlatedSubquery = correlatedSubquery;
            this.aggregationServiceForgeDesc = aggregationServiceForgeDesc;
            this.subqueryNumber = subqueryNumber;
            this.groupKeys = groupKeys;
            this.namedWindow = namedWindow;
            this.namedWindowFilterExpr = namedWindowFilterExpr;
            this.namedWindowFilterQueryGraph = namedWindowFilterQueryGraph;
        }

        public IList<ViewFactoryForge> ViewForges { get; }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubSelectStrategyFactoryLocalViewPreloaded), GetType(), classScope);

            var groupKeyEval = ConstantNull();
            if (groupKeys != null) {
                groupKeyEval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    ExprNodeUtilityQuery.GetForges(groupKeys),
                    null,
                    method,
                    GetType(),
                    classScope);
            }

            method.Block
                .DeclareVar<SubSelectStrategyFactoryLocalViewPreloaded>(
                    "factory",
                    NewInstance(typeof(SubSelectStrategyFactoryLocalViewPreloaded)))
                .SetProperty(Ref("factory"), "SubqueryNumber", Constant(subqueryNumber))
                .SetProperty(
                    Ref("factory"),
                    "ViewFactories",
                    ViewFactoryForgeUtil.CodegenForgesWInit(ViewForges, 0, subqueryNumber, method, symbols, classScope))
                .SetProperty(Ref("factory"), "ViewResourceDelegate", viewResourceDelegateDesc.ToExpression())
                .SetProperty(
                    Ref("factory"),
                    "EventTableFactoryFactory",
                    lookupStrategy.First.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "LookupStrategyFactory",
                    lookupStrategy.Second.Make(method, symbols, classScope))
                .SetProperty(
                    Ref("factory"),
                    "AggregationServiceFactory",
                    MakeAggregationService(subqueryNumber, aggregationServiceForgeDesc, classScope, method, symbols))
                .SetProperty(Ref("factory"), "CorrelatedSubquery", Constant(correlatedSubquery))
                .SetProperty(Ref("factory"), "GroupKeyEval", groupKeyEval)
                .SetProperty(
                    Ref("factory"),
                    "FilterExprEval",
                    filterExprNode == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                            filterExprNode.Forge,
                            method,
                            GetType(),
                            classScope));
            if (namedWindow != null) {
                method.Block.SetProperty(
                    Ref("factory"),
                    "NamedWindow",
                    NamedWindowDeployTimeResolver.MakeResolveNamedWindow(namedWindow, symbols.GetAddInitSvc(method)));
                if (namedWindowFilterExpr != null) {
                    method.Block
                        .SetProperty(
                            Ref("factory"),
                            "NamedWindowFilterQueryGraph",
                            namedWindowFilterQueryGraph.Make(method, symbols, classScope))
                        .SetProperty(
                            Ref("factory"),
                            "NamedWindowFilterExpr",
                            ExprNodeUtilityCodegen.CodegenEvaluator(
                                namedWindowFilterExpr.Forge,
                                method,
                                GetType(),
                                classScope));
                }
            }

            method.Block.MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public bool HasAggregation => aggregationServiceForgeDesc != null;

        public bool HasPrior => viewResourceDelegateDesc.PriorRequests != null &&
                                !viewResourceDelegateDesc.PriorRequests.IsEmpty();

        public bool HasPrevious => viewResourceDelegateDesc.HasPrevious;

        protected internal static CodegenExpression MakeAggregationService(
            int subqueryNumber,
            AggregationServiceForgeDesc aggregationServiceForgeDesc,
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            if (aggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames = new AggregationClassNames(
                CodegenPackageScopeNames.ClassPostfixAggregationForSubquery(subqueryNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                false,
                aggregationServiceForgeDesc.AggregationServiceFactoryForge,
                parent,
                classScope,
                classScope.OutermostClassName,
                aggregationClassNames);
            classScope.AddInnerClasses(aggResult.InnerClasses);
            return LocalMethod(aggResult.InitMethod, symbols.GetAddInitSvc(parent));
        }
    }
} // end of namespace