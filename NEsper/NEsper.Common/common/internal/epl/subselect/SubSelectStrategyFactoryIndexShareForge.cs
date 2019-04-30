///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.hint;
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
        private readonly AggregationServiceForgeDesc aggregationServiceForgeDesc;
        private readonly ExprForge filterExprEval;
        private readonly ExprNode[] groupKeys;
        private readonly NamedWindowMetaData namedWindow;
        private readonly SubordinateQueryPlanDescForge queryPlan;

        private readonly int subqueryNumber;
        private readonly TableMetaData table;

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
            this.subqueryNumber = subqueryNumber;
            this.namedWindow = namedWindow;
            this.table = table;
            this.filterExprEval = filterExprEval;
            this.groupKeys = groupKeys;
            this.aggregationServiceForgeDesc = aggregationServiceForgeDesc;

            var queryPlanLogging = services.Configuration.Common.Logging.IsEnableQueryPlan;

            // We only use existing indexes in all cases. This means "create index" is required.
            if (table != null) {
                queryPlan = SubordinateQueryPlanner.PlanSubquery(
                    outerEventTypesSelect, joinedPropPlan, false, fullTableScan, indexHint, true, subqueryNumber,
                    false, table.IndexMetadata, table.UniquenessAsSet, true,
                    table.InternalEventType, statement.StatementRawInfo, services);

                if (queryPlan != null && queryPlan.IndexDescs != null) {
                    for (var i = 0; i < queryPlan.IndexDescs.Length; i++) {
                        var index = queryPlan.IndexDescs[i];
                        if (table.TableVisibility == NameAccessModifier.PUBLIC) {
                            services.ModuleDependenciesCompileTime.AddPathIndex(
                                false, table.TableName, table.TableModuleName, index.IndexName, index.IndexModuleName,
                                services.NamedWindowCompileTimeRegistry, services.TableCompileTimeRegistry);
                        }
                    }
                }
            }
            else {
                queryPlan = SubordinateQueryPlanner.PlanSubquery(
                    outerEventTypesSelect, joinedPropPlan, false, fullTableScan, indexHint, true, subqueryNumber,
                    namedWindow.IsVirtualDataWindow, namedWindow.IndexMetadata, namedWindow.UniquenessAsSet, true,
                    namedWindow.EventType, statement.StatementRawInfo, services);

                if (queryPlan != null && queryPlan.IndexDescs != null) {
                    for (var i = 0; i < queryPlan.IndexDescs.Length; i++) {
                        var index = queryPlan.IndexDescs[i];
                        if (namedWindow.EventType.Metadata.AccessModifier == NameAccessModifier.PUBLIC) {
                            services.ModuleDependenciesCompileTime.AddPathIndex(
                                true, namedWindow.EventType.Name, namedWindow.NamedWindowModuleName, index.IndexName,
                                index.IndexModuleName, services.NamedWindowCompileTimeRegistry,
                                services.TableCompileTimeRegistry);
                        }
                    }
                }
            }

            SubordinateQueryPlannerUtil.QueryPlanLogOnSubq(
                queryPlanLogging, QUERY_PLAN_LOG, queryPlan, subqueryNumber, statement.StatementRawInfo.Annotations,
                services.ImportServiceCompileTime);
        }

        public IList<ViewFactoryForge> ViewForges => Collections.GetEmptyList<ViewFactoryForge>();

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(SubSelectStrategyFactoryIndexShare), GetType(), classScope);

            var groupKeyEval = ConstantNull();
            if (groupKeys != null) {
                groupKeyEval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    ExprNodeUtilityQuery.GetForges(groupKeys), null, method, GetType(), classScope);
            }

            method.Block
                .DeclareVar(
                    typeof(SubSelectStrategyFactoryIndexShare), "s",
                    NewInstance(typeof(SubSelectStrategyFactoryIndexShare)))
                .SetProperty(Ref("s"), "Table",
                    table == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("s"), "NamedWindow",
                    namedWindow == null
                        ? ConstantNull()
                        : NamedWindowDeployTimeResolver.MakeResolveNamedWindow(
                            namedWindow, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("s"), "AggregationServiceFactory",
                    SubSelectStrategyFactoryLocalViewPreloadedForge.MakeAggregationService(
                        subqueryNumber, aggregationServiceForgeDesc, classScope, method, symbols))
                .SetProperty(Ref("s"), "FilterExprEval",
                    filterExprEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorNoCoerce(
                            filterExprEval, method, GetType(), classScope))
                .SetProperty(Ref("s"), "GroupKeyEval", groupKeyEval)
                .SetProperty(Ref("s"), "QueryPlan",
                    queryPlan == null ? ConstantNull() : queryPlan.Make(method, symbols, classScope))
                .MethodReturn(Ref("s"));
            return LocalMethod(method);
        }

        public bool HasAggregation {
            get { return aggregationServiceForgeDesc != null; }
        }

        public bool HasPrior {
            get { return false; }
        }

        public bool HasPrevious {
            get { return false; }
        }
    }
} // end of namespace