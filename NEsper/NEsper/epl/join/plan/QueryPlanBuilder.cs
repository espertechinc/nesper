///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
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
	/// <summary>
	/// Build a query plan based on filtering information.
	/// </summary>
	public class QueryPlanBuilder
	{
	    private static readonly ILog queryPlanLog = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        /// <summary>
        /// Build query plan using the filter.
        /// </summary>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="outerJoinDescList">list of outer join criteria, or null if there are no outer joins</param>
        /// <param name="queryGraph">relationships between streams based on filter expressions and outer-join on-criteria</param>
        /// <param name="streamNames">names of streams</param>
        /// <param name="historicalViewableDesc">The historical viewable desc.</param>
        /// <param name="dependencyGraph">dependencies between historical streams</param>
        /// <param name="historicalStreamIndexLists">index management, populated for the query plan</param>
        /// <param name="streamJoinAnalysisResult">The stream join analysis result.</param>
        /// <param name="isQueryPlanLogging">if set to <c>true</c> [is query plan logging].</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns>
        /// query plan
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Number of join stream types is less then 2
        /// or
        /// Too many outer join descriptors found
        /// </exception>
        /// <throws>ExprValidationException if the query plan fails</throws>
	    public static QueryPlan GetPlan(EventType[] typesPerStream,
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
	        string methodName = ".getPlan ";

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

	            QueryPlan queryPlanX = TwoStreamQueryPlanBuilder.Build(typesPerStream, queryGraph, outerJoinType, streamJoinAnalysisResult.UniqueKeys, streamJoinAnalysisResult.TablesPerStream);
	            RemoveUnidirectionalAndTable(queryPlanX, streamJoinAnalysisResult);

	            if (log.IsDebugEnabled)
	            {
	                log.Debug(methodName + "2-Stream queryPlan=" + queryPlanX);
	            }
	            return queryPlanX;
	        }

	        bool hasPreferMergeJoin = HintEnum.PREFER_MERGE_JOIN.GetHint(annotations) != null;
	        bool hasForceNestedIter = HintEnum.FORCE_NESTED_ITER.GetHint(annotations) != null;
	        bool isAllInnerJoins = outerJoinDescList.Length == 0 || OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList);

	        if (isAllInnerJoins && !hasPreferMergeJoin)
	        {
	            QueryPlan queryPlanX = NStreamQueryPlanBuilder.Build(queryGraph, typesPerStream,
	                                    historicalViewableDesc, dependencyGraph, historicalStreamIndexLists,
	                                    hasForceNestedIter, streamJoinAnalysisResult.UniqueKeys,
	                                    streamJoinAnalysisResult.TablesPerStream);

	            if (queryPlanX != null) {
	                RemoveUnidirectionalAndTable(queryPlanX, streamJoinAnalysisResult);

	                if (log.IsDebugEnabled)
	                {
	                    log.Debug(methodName + "N-Stream inner-join queryPlan=" + queryPlanX);
	                }
	                return queryPlanX;
	            }

	            if (isQueryPlanLogging && queryPlanLog.IsInfoEnabled) {
	                log.Info("Switching to Outer-NStream algorithm for query plan");
	            }
	        }

	        QueryPlan queryPlan = NStreamOuterQueryPlanBuilder.Build(queryGraph, outerJoinDescList, streamNames, typesPerStream,
	                                    historicalViewableDesc, dependencyGraph, historicalStreamIndexLists, exprEvaluatorContext, streamJoinAnalysisResult.UniqueKeys,
	                streamJoinAnalysisResult.TablesPerStream);
	        RemoveUnidirectionalAndTable(queryPlan, streamJoinAnalysisResult);
	        return queryPlan;
	    }

	    // Remove plans for non-unidirectional streams
	    private static void RemoveUnidirectionalAndTable(QueryPlan queryPlan, StreamJoinAnalysisResult streamJoinAnalysisResult) {
	        for (int streamNum = 0; streamNum < queryPlan.ExecNodeSpecs.Length; streamNum++) {
	            bool unidirectional = streamJoinAnalysisResult.IsUnidirectional && !streamJoinAnalysisResult.UnidirectionalInd[streamNum];
	            bool table = streamJoinAnalysisResult.TablesPerStream[streamNum] != null;
	            if (unidirectional || table) {
	                queryPlan.ExecNodeSpecs[streamNum] = new QueryPlanNodeNoOp();
	            }
	        }
	    }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
