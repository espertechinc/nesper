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
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubSelectStrategyFactoryLocalViewPreloaded), GetType(), classScope);

            var groupKeyEval = ConstantNull();
            if (groupKeys != null) {
                groupKeyEval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    ExprNodeUtilityQuery.GetForges(groupKeys), null, method, GetType(), classScope);
            }

            method.Block
                .DeclareVar(
                    typeof(SubSelectStrategyFactoryLocalViewPreloaded), "factory",
                    NewInstance(typeof(SubSelectStrategyFactoryLocalViewPreloaded)))
                .ExprDotMethod(Ref("factory"), "setSubqueryNumber", Constant(subqueryNumber))
                .ExprDotMethod(
                    Ref("factory"), "setViewFactories",
                    ViewFactoryForgeUtil.CodegenForgesWInit(ViewForges, 0, subqueryNumber, method, symbols, classScope))
                .ExprDotMethod(Ref("factory"), "setViewResourceDelegate", viewResourceDelegateDesc.ToExpression())
                .ExprDotMethod(
                    Ref("factory"), "setEventTableFactoryFactory",
                    lookupStrategy.First.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("factory"), "setLookupStrategyFactory", lookupStrategy.Second.Make(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("factory"), "setAggregationServiceFactory",
                    MakeAggregationService(subqueryNumber, aggregationServiceForgeDesc, classScope, method, symbols))
                .ExprDotMethod(Ref("factory"), "setCorrelatedSubquery", Constant(correlatedSubquery))
                .ExprDotMethod(Ref("factory"), "setGroupKeyEval", groupKeyEval)
                .ExprDotMethod(
                    Ref("factory"), "setFilterExprEval",
                    filterExprNode == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                            filterExprNode.Forge, method, GetType(), classScope));
            if (namedWindow != null) {
                method.Block.ExprDotMethod(
                    Ref("factory"), "setNamedWindow",
                    NamedWindowDeployTimeResolver.MakeResolveNamedWindow(namedWindow, symbols.GetAddInitSvc(method)));
                if (namedWindowFilterExpr != null) {
                    method.Block
                        .ExprDotMethod(
                            Ref("factory"), "setNamedWindowFilterQueryGraph",
                            namedWindowFilterQueryGraph.Make(method, symbols, classScope))
                        .ExprDotMethod(
                            Ref("factory"), "setNamedWindowFilterExpr",
                            ExprNodeUtilityCodegen.CodegenEvaluator(
                                namedWindowFilterExpr.Forge, method, GetType(), classScope));
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
            int subqueryNumber, AggregationServiceForgeDesc aggregationServiceForgeDesc, CodegenClassScope classScope,
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols)
        {
            if (aggregationServiceForgeDesc == null) {
                return ConstantNull();
            }

            var aggregationClassNames = new AggregationClassNames(
                CodegenPackageScopeNames.ClassPostfixAggregationForSubquery(subqueryNumber));
            var aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
                false, aggregationServiceForgeDesc.AggregationServiceFactoryForge, parent, classScope,
                classScope.OutermostClassName, aggregationClassNames);
            classScope.AddInnerClasses(aggResult.InnerClasses);
            return LocalMethod(aggResult.InitMethod, symbols.GetAddInitSvc(parent));
        }
    }
} // end of namespace