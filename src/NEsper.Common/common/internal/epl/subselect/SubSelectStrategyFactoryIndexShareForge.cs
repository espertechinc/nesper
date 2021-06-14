///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectStrategyFactoryIndexShareForge : SubSelectStrategyFactoryForge
	{
		private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

		private readonly int _subqueryNumber;
		private readonly NamedWindowMetaData _namedWindow;
		private readonly TableMetaData _table;
		private readonly ExprForge _filterExprEval;
		private readonly ExprNode[] _groupKeys;
		private readonly AggregationServiceForgeDesc _aggregationServiceForgeDesc;
		private readonly SubordinateQueryPlanDescForge _queryPlan;
		private readonly IList<StmtClassForgeableFactory> _additionalForgeables = new List<StmtClassForgeableFactory>();
		private readonly MultiKeyClassRef _groupByMultiKey;

		public SubSelectStrategyFactoryIndexShareForge(
			int subqueryNumber,
			SubSelectActivationPlan subselectActivation,
			EventType[] outerEventTypesSelect,
			NamedWindowMetaData namedWindow,
			TableMetaData table,
			bool fullTableScan,
			IndexHint indexHint,
			SubordPropPlan joinedPropPlan,
			ExprForge filterExprEval,
			ExprNode[] groupKeys,
			AggregationServiceForgeDesc aggregationServiceForgeDesc,
			StatementBaseInfo statement,
			StatementCompileTimeServices services)
		{
			_subqueryNumber = subqueryNumber;
			_namedWindow = namedWindow;
			_table = table;
			_filterExprEval = filterExprEval;
			_groupKeys = groupKeys;
			_aggregationServiceForgeDesc = aggregationServiceForgeDesc;

			bool queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;

			// We only use existing indexes in all cases. This means "create index" is required.
			SubordinateQueryPlan plan;
			if (table != null) {
				plan = SubordinateQueryPlanner.PlanSubquery(
					outerEventTypesSelect,
					joinedPropPlan,
					false,
					fullTableScan,
					indexHint,
					true,
					subqueryNumber,
					false,
					table.IndexMetadata,
					table.UniquenessAsSet,
					true,
					table.InternalEventType,
					statement.StatementRawInfo,
					services);
			}
			else {
				plan = SubordinateQueryPlanner.PlanSubquery(
					outerEventTypesSelect,
					joinedPropPlan,
					false,
					fullTableScan,
					indexHint,
					true,
					subqueryNumber,
					namedWindow.IsVirtualDataWindow,
					namedWindow.IndexMetadata,
					namedWindow.UniquenessAsSet,
					true,
					namedWindow.EventType,
					statement.StatementRawInfo,
					services);
			}

			_queryPlan = plan == null ? null : plan.Forge;
			if (plan != null) {
				_additionalForgeables.AddAll(plan.AdditionalForgeables);
			}

			if (_queryPlan != null && _queryPlan.IndexDescs != null) {
				for (int i = 0; i < _queryPlan.IndexDescs.Length; i++) {
					SubordinateQueryIndexDescForge index = _queryPlan.IndexDescs[i];

					if (table != null) {
						if (table.TableVisibility == NameAccessModifier.PUBLIC) {
							services.ModuleDependenciesCompileTime.AddPathIndex(
								false,
								table.TableName,
								table.TableModuleName,
								index.IndexName,
								index.IndexModuleName,
								services.NamedWindowCompileTimeRegistry,
								services.TableCompileTimeRegistry);
						}
					}
					else {
						if (namedWindow.EventType.Metadata.AccessModifier == NameAccessModifier.PUBLIC) {
							services.ModuleDependenciesCompileTime.AddPathIndex(
								true,
								namedWindow.EventType.Name,
								namedWindow.NamedWindowModuleName,
								index.IndexName,
								index.IndexModuleName,
								services.NamedWindowCompileTimeRegistry,
								services.TableCompileTimeRegistry);
						}
					}
				}
			}

			SubordinateQueryPlannerUtil.QueryPlanLogOnSubq(
				queryPlanLogging,
				QUERY_PLAN_LOG,
				_queryPlan,
				subqueryNumber,
				statement.StatementRawInfo.Annotations,
				services.ImportServiceCompileTime);

			if (groupKeys == null || groupKeys.Length == 0) {
				_groupByMultiKey = null;
			}
			else {
				MultiKeyPlan mkplan = MultiKeyPlanner.PlanMultiKey(groupKeys, false, statement.StatementRawInfo, services.SerdeResolver);
				_additionalForgeables.AddAll(mkplan.MultiKeyForgeables);
				_groupByMultiKey = mkplan.ClassRef;
			}
		}

		public CodegenExpression MakeCodegen(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenMethod method = parent.MakeChild(typeof(SubSelectStrategyFactoryIndexShare), GetType(), classScope);

			CodegenExpression groupKeyEval = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(_groupKeys, null, _groupByMultiKey, method, classScope);

			var tableExpr = _table == null
				? ConstantNull()
				: TableDeployTimeResolver.MakeResolveTable(_table, symbols.GetAddInitSvc(method));
			var namedWindowExpr = _namedWindow == null
				? ConstantNull()
				: NamedWindowDeployTimeResolver.MakeResolveNamedWindow(_namedWindow, symbols.GetAddInitSvc(method));
			var aggregationServiceFactoryExpr = SubSelectStrategyFactoryLocalViewPreloadedForge.MakeAggregationService(
				_subqueryNumber,
				_aggregationServiceForgeDesc,
				classScope,
				method,
				symbols);
			var filterExprEvalExpr = _filterExprEval == null
				? ConstantNull()
				: ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(_filterExprEval, method, GetType(), classScope);
			var queryPlanExpr = _queryPlan == null
				? ConstantNull()
				: _queryPlan.Make(method, symbols, classScope);
			
			method.Block
				.DeclareVar<SubSelectStrategyFactoryIndexShare>("s", NewInstance(typeof(SubSelectStrategyFactoryIndexShare)))
				.SetProperty(Ref("s"), "Table", tableExpr)
				.SetProperty(Ref("s"), "NamedWindow", namedWindowExpr)
				.SetProperty(Ref("s"), "AggregationServiceFactory", aggregationServiceFactoryExpr)
				.SetProperty(Ref("s"), "FilterExprEval", filterExprEvalExpr)
				.SetProperty(Ref("s"), "GroupKeyEval", groupKeyEval)
				.SetProperty(Ref("s"), "QueryPlan", queryPlanExpr)
				.MethodReturn(Ref("s"));
			return LocalMethod(method);
		}

		public IList<ViewFactoryForge> ViewForges => EmptyList<ViewFactoryForge>.Instance;

		public bool HasAggregation => _aggregationServiceForgeDesc != null;

		public bool HasPrior => false;

		public bool HasPrevious => false;

		public IList<StmtClassForgeableFactory> AdditionalForgeables => _additionalForgeables;
	}
} // end of namespace
