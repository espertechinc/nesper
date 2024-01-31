///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    /// <summary>
    /// Build a query plan based on filtering information.
    /// </summary>
    public class QueryPlanBuilder
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        public static QueryPlanForgeDesc GetPlan(
            EventType[] typesPerStream,
            OuterJoinDesc[] outerJoinDescList,
            QueryGraphForge queryGraph,
            string[] streamNames,
            HistoricalViewableDesc historicalViewableDesc,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexListForge[] historicalStreamIndexLists,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            bool isQueryPlanLogging,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var methodName = ".getPlan ";

            var numStreams = typesPerStream.Length;
            if (numStreams < 2) {
                throw new ArgumentException("Number of join stream types is less then 2");
            }

            if (outerJoinDescList.Length >= numStreams) {
                throw new ArgumentException("Too many outer join descriptors found");
            }

            if (numStreams == 2) {
                OuterJoinType? outerJoinType = null;
                if (outerJoinDescList.Length > 0) {
                    outerJoinType = outerJoinDescList[0].OuterJoinType;
                }

                var queryPlan = TwoStreamQueryPlanBuilder.Build(
                    typesPerStream,
                    queryGraph,
                    outerJoinType,
                    streamJoinAnalysisResult,
                    statementRawInfo);
                RemoveUnidirectionalAndTable(queryPlan.Forge, streamJoinAnalysisResult);

                if (Log.IsDebugEnabled) {
                    Log.Debug(methodName + "2-Stream queryPlan=" + queryPlan);
                }

                return queryPlan;
            }

            var hasPreferMergeJoin = HintEnum.PREFER_MERGE_JOIN.GetHint(statementRawInfo.Annotations) != null;
            var hasForceNestedIter = HintEnum.FORCE_NESTED_ITER.GetHint(statementRawInfo.Annotations) != null;
            var isAllInnerJoins = outerJoinDescList.Length == 0 ||
                                  OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList);

            if (isAllInnerJoins && !hasPreferMergeJoin) {
                var queryPlan = NStreamQueryPlanBuilder.Build(
                    queryGraph,
                    typesPerStream,
                    historicalViewableDesc,
                    dependencyGraph,
                    historicalStreamIndexLists,
                    hasForceNestedIter,
                    streamJoinAnalysisResult.UniqueKeys,
                    streamJoinAnalysisResult.TablesPerStream,
                    streamJoinAnalysisResult,
                    statementRawInfo,
                    services.SerdeResolver);

                if (queryPlan != null) {
                    RemoveUnidirectionalAndTable(queryPlan.Forge, streamJoinAnalysisResult);

                    if (Log.IsDebugEnabled) {
                        Log.Debug(methodName + "N-Stream inner-join queryPlan=" + queryPlan);
                    }

                    return queryPlan;
                }

                if (isQueryPlanLogging && QUERY_PLAN_LOG.IsInfoEnabled) {
                    Log.Info("Switching to Outer-NStream algorithm for query plan");
                }
            }

            var queryPlanX = NStreamOuterQueryPlanBuilder.Build(
                queryGraph,
                outerJoinDescList,
                streamNames,
                typesPerStream,
                historicalViewableDesc,
                dependencyGraph,
                historicalStreamIndexLists,
                streamJoinAnalysisResult.UniqueKeys,
                streamJoinAnalysisResult.TablesPerStream,
                streamJoinAnalysisResult,
                statementRawInfo,
                services);
            RemoveUnidirectionalAndTable(queryPlanX.Forge, streamJoinAnalysisResult);
            return queryPlanX;
        }

        // Remove plans for non-unidirectional streams
        private static void RemoveUnidirectionalAndTable(
            QueryPlanForge queryPlan,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult)
        {
            var allUnidirectional = streamJoinAnalysisResult.IsUnidirectionalAll;
            for (var streamNum = 0; streamNum < queryPlan.ExecNodeSpecs.Length; streamNum++) {
                if (allUnidirectional) {
                    queryPlan.ExecNodeSpecs[streamNum] = new QueryPlanNodeForgeAllUnidirectionalOuter(streamNum);
                }
                else {
                    var unidirectional = streamJoinAnalysisResult.IsUnidirectional &&
                                         !streamJoinAnalysisResult.UnidirectionalInd[streamNum];
                    var table = streamJoinAnalysisResult.TablesPerStream[streamNum] != null;
                    if (unidirectional || table) {
                        queryPlan.ExecNodeSpecs[streamNum] = QueryPlanNodeNoOpForge.INSTANCE;
                    }
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace