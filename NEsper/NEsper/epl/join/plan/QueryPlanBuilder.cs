///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>Build a query plan based on filtering information.</summary>
    public class QueryPlanBuilder
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Build query plan using the filter.
        /// </summary>
        /// <param name="typesPerStream">- event types for each stream</param>
        /// <param name="outerJoinDescList">- list of outer join criteria, or null if there are no outer joins</param>
        /// <param name="queryGraph">- relationships between streams based on filter expressions and outer-join on-criteria</param>
        /// <param name="streamNames">- names of streams</param>
        /// <param name="dependencyGraph">- dependencies between historical streams</param>
        /// <param name="historicalStreamIndexLists">- index management, populated for the query plan</param>
        /// <param name="streamJoinAnalysisResult">stream join analysis metadata</param>
        /// <param name="historicalViewableDesc">historicals</param>
        /// <param name="isQueryPlanLogging">for logging</param>
        /// <param name="exprEvaluatorContext">context</param>
        /// <param name="annotations">annotations</param>
        /// <exception cref="ExprValidationException">if the query plan fails</exception>
        /// <returns>query plan</returns>
        public static QueryPlan GetPlan(
            EventType[] typesPerStream,
            OuterJoinDesc[] outerJoinDescList,
            QueryGraph queryGraph,
            string[] streamNames,
            HistoricalViewableDesc historicalViewableDesc,
            DependencyGraph dependencyGraph,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            StreamJoinAnalysisResult streamJoinAnalysisResult,
            bool isQueryPlanLogging,
            Attribute[] annotations,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            const string methodName = ".GetPlan ";

            int numStreams = typesPerStream.Length;
            if (numStreams < 2)
            {
                throw new ArgumentException("Number of join stream types is less then 2");
            }
            if (outerJoinDescList.Length >= numStreams)
            {
                throw new ArgumentException("Too many outer join descriptors found");
            }

            if (numStreams == 2)
            {
                OuterJoinType? outerJoinType = null;
                if (outerJoinDescList.Length > 0)
                {
                    outerJoinType = outerJoinDescList[0].OuterJoinType;
                }

                QueryPlan queryPlan = TwoStreamQueryPlanBuilder.Build(
                    typesPerStream, queryGraph, outerJoinType, streamJoinAnalysisResult.UniqueKeys,
                    streamJoinAnalysisResult.TablesPerStream);
                RemoveUnidirectionalAndTable(queryPlan, streamJoinAnalysisResult);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(methodName + "2-Stream queryPlan=" + queryPlan);
                }
                return queryPlan;
            }

            bool hasPreferMergeJoin = HintEnum.PREFER_MERGE_JOIN.GetHint(annotations) != null;
            bool hasForceNestedIter = HintEnum.FORCE_NESTED_ITER.GetHint(annotations) != null;
            bool isAllInnerJoins = outerJoinDescList.Length == 0 ||
                                   OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList);

            if (isAllInnerJoins && !hasPreferMergeJoin)
            {
                QueryPlan queryPlan = NStreamQueryPlanBuilder.Build(
                    queryGraph, typesPerStream,
                    historicalViewableDesc, dependencyGraph, historicalStreamIndexLists,
                    hasForceNestedIter, streamJoinAnalysisResult.UniqueKeys,
                    streamJoinAnalysisResult.TablesPerStream);

                if (queryPlan != null)
                {
                    RemoveUnidirectionalAndTable(queryPlan, streamJoinAnalysisResult);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(methodName + "N-Stream inner-join queryPlan=" + queryPlan);
                    }
                    return queryPlan;
                }

                if (isQueryPlanLogging && QUERY_PLAN_LOG.IsInfoEnabled)
                {
                    Log.Info("Switching to Outer-NStream algorithm for query plan");
                }
            }

            QueryPlan queryPlanX = NStreamOuterQueryPlanBuilder.Build(
                queryGraph, outerJoinDescList, streamNames, typesPerStream,
                historicalViewableDesc, dependencyGraph, historicalStreamIndexLists, exprEvaluatorContext,
                streamJoinAnalysisResult.UniqueKeys,
                streamJoinAnalysisResult.TablesPerStream);
            RemoveUnidirectionalAndTable(queryPlanX, streamJoinAnalysisResult);
            return queryPlanX;
        }

        // Remove plans for non-unidirectional streams
        private static void RemoveUnidirectionalAndTable(
            QueryPlan queryPlan,
            StreamJoinAnalysisResult streamJoinAnalysisResult)
        {
            bool allUnidirectional = streamJoinAnalysisResult.IsUnidirectionalAll;
            for (int streamNum = 0; streamNum < queryPlan.ExecNodeSpecs.Length; streamNum++)
            {
                if (allUnidirectional)
                {
                    queryPlan.ExecNodeSpecs[streamNum] = new QueryPlanNodeAllUnidirectionalOuter(streamNum);
                }
                else
                {
                    bool unidirectional = streamJoinAnalysisResult.IsUnidirectional &&
                                          !streamJoinAnalysisResult.UnidirectionalInd[streamNum];
                    bool table = streamJoinAnalysisResult.TablesPerStream[streamNum] != null;
                    if (unidirectional || table)
                    {
                        queryPlan.ExecNodeSpecs[streamNum] = new QueryPlanNodeNoOp();
                    }
                }
            }
        }
    }
} // end of namespace
