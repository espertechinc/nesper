///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
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
		private readonly IList<ViewFactoryForge> _viewForges;
		private readonly ViewResourceDelegateDesc _viewResourceDelegateDesc;
		private readonly Pair<EventTableFactoryFactoryForge, SubordTableLookupStrategyFactoryForge> _lookupStrategy;
		private readonly ExprNode _filterExprNode;
		private readonly bool _correlatedSubquery;
		private readonly AggregationServiceForgeDesc _aggregationServiceForgeDesc;
		private readonly int _subqueryNumber;
		private readonly ExprNode[] _groupKeys;
		private readonly NamedWindowMetaData _namedWindow;
		private readonly ExprNode _namedWindowFilterExpr;
		private readonly QueryGraphForge _namedWindowFilterQueryGraph;
		private readonly MultiKeyClassRef _groupByMultiKeyClasses;

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
			QueryGraphForge namedWindowFilterQueryGraph,
			MultiKeyClassRef groupByMultiKeyClasses)
		{
			_viewForges = viewForges;
			_viewResourceDelegateDesc = viewResourceDelegateDesc;
			_lookupStrategy = lookupStrategy;
			_filterExprNode = filterExprNode;
			_correlatedSubquery = correlatedSubquery;
			_aggregationServiceForgeDesc = aggregationServiceForgeDesc;
			_subqueryNumber = subqueryNumber;
			_groupKeys = groupKeys;
			_namedWindow = namedWindow;
			_namedWindowFilterExpr = namedWindowFilterExpr;
			_namedWindowFilterQueryGraph = namedWindowFilterQueryGraph;
			_groupByMultiKeyClasses = groupByMultiKeyClasses;
		}

		public IList<ViewFactoryForge> ViewForges => _viewForges;

		public CodegenExpression MakeCodegen(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenMethod method = parent.MakeChild(typeof(SubSelectStrategyFactoryLocalViewPreloaded), GetType(), classScope);

			CodegenExpression groupKeyEval = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(_groupKeys, null, _groupByMultiKeyClasses, method, classScope);

			method.Block
				.DeclareVar(typeof(SubSelectStrategyFactoryLocalViewPreloaded), "factory", NewInstance(typeof(SubSelectStrategyFactoryLocalViewPreloaded)))
				.SetProperty(Ref("factory"), "SubqueryNumber", Constant(_subqueryNumber))
				.SetProperty(Ref("factory"), "ViewFactories",  ViewFactoryForgeUtil.CodegenForgesWInit(_viewForges, 0, _subqueryNumber, method, symbols, classScope))
				.SetProperty(Ref("factory"), "ViewResourceDelegate", _viewResourceDelegateDesc.ToExpression())
				.SetProperty(Ref("factory"), "EventTableFactoryFactory", _lookupStrategy.First.Make(method, symbols, classScope))
				.SetProperty(Ref("factory"), "LookupStrategyFactory", _lookupStrategy.Second.Make(method, symbols, classScope))
				.SetProperty(Ref("factory"), "AggregationServiceFactory", MakeAggregationService(_subqueryNumber, _aggregationServiceForgeDesc, classScope, method, symbols))
				.SetProperty(Ref("factory"), "CorrelatedSubquery", Constant(_correlatedSubquery))
				.SetProperty(Ref("factory"), "GroupKeyEval", groupKeyEval)
				.SetProperty(Ref("factory"), "FilterExprEval", _filterExprNode == null
						? ConstantNull()
						: ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(_filterExprNode.Forge, method, GetType(), classScope));

			if (_namedWindow != null) {
				method.Block.ExprDotMethod(
					Ref("factory"), "NamedWindow", NamedWindowDeployTimeResolver.MakeResolveNamedWindow(_namedWindow, symbols.GetAddInitSvc(method)));
				if (_namedWindowFilterExpr != null) {
					method.Block
						.SetProperty(Ref("factory"), "NamedWindowFilterQueryGraph",
							_namedWindowFilterQueryGraph.Make(method, symbols, classScope))
						.SetProperty(Ref("factory"), "NamedWindowFilterExpr",
							ExprNodeUtilityCodegen.CodegenEvaluator(_namedWindowFilterExpr.Forge, method, GetType(), classScope));
				}
			}

			method.Block.MethodReturn(Ref("factory"));
			return LocalMethod(method);
		}

		public bool HasAggregation => _aggregationServiceForgeDesc != null;

		public bool HasPrior => _viewResourceDelegateDesc.PriorRequests != null && !_viewResourceDelegateDesc.PriorRequests.IsEmpty();

		public bool HasPrevious => _viewResourceDelegateDesc.HasPrevious;

		internal static CodegenExpression MakeAggregationService(
			int subqueryNumber,
			AggregationServiceForgeDesc aggregationServiceForgeDesc,
			CodegenClassScope classScope,
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols)
		{
			if (aggregationServiceForgeDesc == null) {
				return ConstantNull();
			}

			AggregationClassNames aggregationClassNames =
				new AggregationClassNames(CodegenNamespaceScopeNames.ClassPostfixAggregationForSubquery(subqueryNumber));
			AggregationServiceFactoryMakeResult aggResult = AggregationServiceFactoryCompiler.MakeInnerClassesAndInit(
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
