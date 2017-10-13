///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.pollindex;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Factory for building a <seealso cref="JoinSetComposer" /> from analyzing filter nodes, for
    /// fast join tuple result set composition.
    /// </summary>
    public class JoinSetComposerPrototypeFactory
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static JoinSetComposerPrototype MakeComposerPrototype(
            string statementName,
            int statementId,
            OuterJoinDesc[] outerJoinDescList,
            ExprNode optionalFilterNode,
            EventType[] streamTypes,
            string[] streamNames,
            StreamJoinAnalysisResult streamJoinAnalysisResult,
            bool queryPlanLogging,
            StatementContext statementContext,
            HistoricalViewableDesc historicalViewableDesc,
            ExprEvaluatorContext exprEvaluatorContext,
            bool selectsRemoveStream,
            bool hasAggregations,
            TableService tableService,
            bool isOnDemandQuery,
            bool allowIndexInit)
        {
            // Determine if there is a historical stream, and what dependencies exist
            var historicalDependencyGraph = new DependencyGraph(streamTypes.Length, false);
            for (var i = 0; i < streamTypes.Length; i++)
            {
                if (historicalViewableDesc.Historical[i])
                {
                    var streamsThisStreamDependsOn = historicalViewableDesc.DependenciesPerHistorical[i];
                    historicalDependencyGraph.AddDependency(i, streamsThisStreamDependsOn);
                }
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Dependency graph: " + historicalDependencyGraph);
            }

            // Handle a join with a database or other historical data source for 2 streams
            if ((historicalViewableDesc.HasHistorical) && (streamTypes.Length == 2))
            {
                return MakeComposerHistorical2Stream(
                    outerJoinDescList, optionalFilterNode, streamTypes, historicalViewableDesc, queryPlanLogging,
                    exprEvaluatorContext, statementContext, streamNames, allowIndexInit);
            }

            var isOuterJoins = !OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList);

            // Query graph for graph relationships between streams/historicals
            // For outer joins the query graph will just contain outer join relationships
            var hint = ExcludePlanHint.GetHint(streamNames, statementContext);
            var queryGraph = new QueryGraph(streamTypes.Length, hint, false);
            if (outerJoinDescList.Length > 0)
            {
                OuterJoinAnalyzer.Analyze(outerJoinDescList, queryGraph);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".MakeComposer After outer join queryGraph=\n" + queryGraph);
                }
            }

            // Let the query graph reflect the where-clause
            if (optionalFilterNode != null)
            {
                // Analyze relationships between streams using the optional filter expression.
                // Relationships are properties in AND and EQUALS nodes of joins.
                FilterExprAnalyzer.Analyze(optionalFilterNode, queryGraph, isOuterJoins);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".MakeComposer After filter expression queryGraph=\n" + queryGraph);
                }

                // Add navigation entries based on key and index property equivalency (a=b, b=c follows a=c)
                QueryGraph.FillEquivalentNav(streamTypes, queryGraph);
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".MakeComposer After fill equiv. nav. queryGraph=\n" + queryGraph);
                }
            }

            // Historical index lists
            var historicalStreamIndexLists = new HistoricalStreamIndexList[streamTypes.Length];

            var queryPlan = QueryPlanBuilder.GetPlan(
                streamTypes, outerJoinDescList, queryGraph, streamNames,
                historicalViewableDesc, historicalDependencyGraph, historicalStreamIndexLists,
                streamJoinAnalysisResult, queryPlanLogging, statementContext.Annotations, exprEvaluatorContext);

            // remove unused indexes - consider all streams or all unidirectional
            var usedIndexes = new HashSet<TableLookupIndexReqKey>();
            var indexSpecs = queryPlan.IndexSpecs;
            for (var streamNum = 0; streamNum < queryPlan.ExecNodeSpecs.Length; streamNum++)
            {
                var planNode = queryPlan.ExecNodeSpecs[streamNum];
                if (planNode != null)
                {
                    planNode.AddIndexes(usedIndexes);
                }
            }
            foreach (var indexSpec in indexSpecs)
            {
                if (indexSpec == null)
                {
                    continue;
                }
                var items = indexSpec.Items;
                var indexNames = items.Keys.ToArray();
                foreach (var indexName in indexNames)
                {
                    if (!usedIndexes.Contains(indexName))
                    {
                        items.Remove(indexName);
                    }
                }
            }

            var hook = QueryPlanIndexHookUtil.GetHook(
                statementContext.Annotations, statementContext.EngineImportService);
            if (queryPlanLogging && (QUERY_PLAN_LOG.IsInfoEnabled || hook != null))
            {
                QUERY_PLAN_LOG.Info("Query plan: " + queryPlan.ToQueryPlan());
                if (hook != null)
                {
                    hook.Join(queryPlan);
                }
            }

            // register index-use references for tables
            if (!isOnDemandQuery)
            {
                foreach (var usedIndex in usedIndexes)
                {
                    if (usedIndex.TableName != null)
                    {
                        tableService.GetTableMetadata(usedIndex.TableName)
                            .AddIndexReference(usedIndex.Name, statementName);
                    }
                }
            }

            var joinRemoveStream = selectsRemoveStream || hasAggregations;
            return new JoinSetComposerPrototypeImpl(
                statementName,
                statementId,
                outerJoinDescList,
                optionalFilterNode,
                streamTypes,
                streamNames,
                streamJoinAnalysisResult,
                statementContext.Annotations,
                historicalViewableDesc,
                exprEvaluatorContext,
                indexSpecs,
                queryPlan,
                historicalStreamIndexLists,
                joinRemoveStream,
                isOuterJoins,
                tableService, statementContext.EventTableIndexService);
        }

        private static JoinSetComposerPrototype MakeComposerHistorical2Stream(
            OuterJoinDesc[] outerJoinDescList,
            ExprNode optionalFilterNode,
            EventType[] streamTypes,
            HistoricalViewableDesc historicalViewableDesc,
            bool queryPlanLogging,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContext statementContext,
            string[] streamNames,
            bool allowIndexInit)
        {
            var polledViewNum = 0;
            var streamViewNum = 1;
            if (historicalViewableDesc.Historical[1])
            {
                streamViewNum = 0;
                polledViewNum = 1;
            }

            // if all-historical join, check dependency
            var isAllHistoricalNoSubordinate = false;
            if ((historicalViewableDesc.Historical[0]) && historicalViewableDesc.Historical[1])
            {
                var graph = new DependencyGraph(2, false);
                graph.AddDependency(0, historicalViewableDesc.DependenciesPerHistorical[0]);
                graph.AddDependency(1, historicalViewableDesc.DependenciesPerHistorical[1]);
                if (graph.FirstCircularDependency != null)
                {
                    throw new ExprValidationException("Circular dependency detected between historical streams");
                }

                // if both streams are independent
                if (graph.RootNodes.Count == 2)
                {
                    isAllHistoricalNoSubordinate = true; // No parameters used by either historical
                }
                else
                {
                    if (graph.GetDependenciesForStream(0).Count == 0)
                    {
                        streamViewNum = 0;
                        polledViewNum = 1;
                    }
                    else
                    {
                        streamViewNum = 1;
                        polledViewNum = 0;
                    }
                }
            }

            // Build an outer join expression node
            var isOuterJoin = false;
            ExprNode outerJoinEqualsNode = null;
            var isInnerJoinOnly = false;
            if (outerJoinDescList.Length > 0)
            {
                var outerJoinDesc = outerJoinDescList[0];
                isInnerJoinOnly = outerJoinDesc.OuterJoinType.Equals(OuterJoinType.INNER);

                if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.FULL))
                {
                    isOuterJoin = true;
                }
                else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.LEFT)) &&
                         (streamViewNum == 0))
                {
                    isOuterJoin = true;
                }
                else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.RIGHT)) &&
                         (streamViewNum == 1))
                {
                    isOuterJoin = true;
                }

                outerJoinEqualsNode = outerJoinDesc.MakeExprNode(exprEvaluatorContext);
            }

            // Determine filter for indexing purposes
            ExprNode filterForIndexing = null;
            if ((outerJoinEqualsNode != null) && (optionalFilterNode != null) && isInnerJoinOnly)
            {
                // both filter and outer join, add
                filterForIndexing = new ExprAndNodeImpl();
                filterForIndexing.AddChildNode(optionalFilterNode);
                filterForIndexing.AddChildNode(outerJoinEqualsNode);
            }
            else if ((outerJoinEqualsNode == null) && (optionalFilterNode != null))
            {
                filterForIndexing = optionalFilterNode;
            }
            else if (outerJoinEqualsNode != null)
            {
                filterForIndexing = outerJoinEqualsNode;
            }

            var indexStrategies =
                DetermineIndexing(
                    filterForIndexing, streamTypes[polledViewNum], streamTypes[streamViewNum], polledViewNum,
                    streamViewNum, statementContext, streamNames);

            var hook = QueryPlanIndexHookUtil.GetHook(
                statementContext.Annotations, statementContext.EngineImportService);
            if (queryPlanLogging && (QUERY_PLAN_LOG.IsInfoEnabled || hook != null))
            {
                QUERY_PLAN_LOG.Info("historical lookup strategy: " + indexStrategies.First.ToQueryPlan());
                QUERY_PLAN_LOG.Info("historical index strategy: " + indexStrategies.Second.ToQueryPlan());
                if (hook != null)
                {
                    hook.Historical(
                        new QueryPlanIndexDescHistorical(
                            indexStrategies.First.GetType().Name, indexStrategies.Second.GetType().Name));
                }
            }

            return new JoinSetComposerPrototypeHistorical2StreamImpl(
                optionalFilterNode,
                streamTypes,
                exprEvaluatorContext,
                polledViewNum,
                streamViewNum,
                isOuterJoin,
                outerJoinEqualsNode,
                indexStrategies,
                isAllHistoricalNoSubordinate,
                outerJoinDescList, allowIndexInit);
        }

        private static Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> DetermineIndexing(
            ExprNode filterForIndexing,
            EventType polledViewType,
            EventType streamViewType,
            int polledViewStreamNum,
            int streamViewStreamNum,
            StatementContext statementContext,
            string[] streamNames)
        {
            // No filter means unindexed event tables
            if (filterForIndexing == null)
            {
                return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(
                    new HistoricalIndexLookupStrategyNoIndex(), new PollResultIndexingStrategyNoIndex());
            }

            // analyze query graph; Whereas stream0=named window, stream1=delete-expr filter
            var hint = ExcludePlanHint.GetHint(streamNames, statementContext);
            var queryGraph = new QueryGraph(2, hint, false);
            FilterExprAnalyzer.Analyze(filterForIndexing, queryGraph, false);

            return DetermineIndexing(
                queryGraph, polledViewType, streamViewType, polledViewStreamNum, streamViewStreamNum);
        }

        /// <summary>
        /// Constructs indexing and lookup strategy for a given relationship that a historical stream may have with another
        /// stream (historical or not) that looks up into results of a poll of a historical stream.
        /// <para>
        /// The term "polled" refers to the assumed-historical stream.
        /// </para>
        /// </summary>
        /// <param name="queryGraph">relationship representation of where-clause filter and outer join on-expressions</param>
        /// <param name="polledViewType">the event type of the historical that is indexed</param>
        /// <param name="streamViewType">the event type of the stream looking up in indexes</param>
        /// <param name="polledViewStreamNum">the stream number of the historical that is indexed</param>
        /// <param name="streamViewStreamNum">the stream number of the historical that is looking up</param>
        /// <returns>indexing and lookup strategy pair</returns>
        public static Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> DetermineIndexing(
            QueryGraph queryGraph,
            EventType polledViewType,
            EventType streamViewType,
            int polledViewStreamNum,
            int streamViewStreamNum)
        {
            var queryGraphValue = queryGraph.GetGraphValue(streamViewStreamNum, polledViewStreamNum);
            var hashKeysAndIndes = queryGraphValue.HashKeyProps;
            var rangeKeysAndIndex = queryGraphValue.RangeProps;

            // index and key property names
            var hashKeys = hashKeysAndIndes.Keys;
            var hashIndexes = hashKeysAndIndes.Indexed;
            var rangeKeys = rangeKeysAndIndex.Keys;
            var rangeIndexes = rangeKeysAndIndex.Indexed;

            // If the analysis revealed no join columns, must use the brute-force full table scan
            if (hashKeys.IsEmpty() && rangeKeys.IsEmpty())
            {
                var inKeywordSingles = queryGraphValue.InKeywordSingles;
                if (inKeywordSingles != null && inKeywordSingles.Indexed.Length != 0)
                {
                    var indexed = inKeywordSingles.Indexed[0];
                    var lookup = inKeywordSingles.Key[0];
                    var strategy = new HistoricalIndexLookupStrategyInKeywordSingle(
                        streamViewStreamNum, lookup.KeyExprs);
                    var indexing = new PollResultIndexingStrategyIndexSingle(
                        polledViewStreamNum, polledViewType, indexed);
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }

                var multis = queryGraphValue.InKeywordMulti;
                if (!multis.IsEmpty())
                {
                    var multi = multis[0];
                    var strategy = new HistoricalIndexLookupStrategyInKeywordMulti(
                        streamViewStreamNum, multi.Key.KeyExpr);
                    var indexing = new PollResultIndexingStrategyIndexSingleArray(
                        polledViewStreamNum, polledViewType,
                        ExprNodeUtility.GetIdentResolvedPropertyNames(multi.Indexed));
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }

                return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(
                    new HistoricalIndexLookupStrategyNoIndex(), new PollResultIndexingStrategyNoIndex());
            }

            var keyCoercionTypes = CoercionUtil.GetCoercionTypesHash(
                new EventType[]
                {
                    streamViewType,
                    polledViewType
                }, 0, 1, hashKeys, hashIndexes);

            if (rangeKeys.IsEmpty())
            {
                // No coercion
                if (!keyCoercionTypes.IsCoerce)
                {
                    if (hashIndexes.Count == 1)
                    {
                        var indexing = new PollResultIndexingStrategyIndexSingle(
                            polledViewStreamNum, polledViewType, hashIndexes[0]);
                        var strategy = new HistoricalIndexLookupStrategyIndexSingle(streamViewStreamNum, hashKeys[0]);
                        return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                    }
                    else
                    {
                        var indexing = new PollResultIndexingStrategyIndex(
                            polledViewStreamNum, polledViewType, hashIndexes);
                        var strategy = new HistoricalIndexLookupStrategyIndex(
                            streamViewType, streamViewStreamNum, hashKeys);
                        return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                    }
                }

                // With coercion, same lookup strategy as the index coerces
                if (hashIndexes.Count == 1)
                {
                    var indexing = new PollResultIndexingStrategyIndexCoerceSingle(
                        polledViewStreamNum, polledViewType, hashIndexes[0], keyCoercionTypes.CoercionTypes[0]);
                    var strategy = new HistoricalIndexLookupStrategyIndexSingle(streamViewStreamNum, hashKeys[0]);
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }
                else
                {
                    var indexing = new PollResultIndexingStrategyIndexCoerce(
                        polledViewStreamNum, polledViewType, hashIndexes, keyCoercionTypes.CoercionTypes);
                    var strategy = new HistoricalIndexLookupStrategyIndex(streamViewType, streamViewStreamNum, hashKeys);
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }
            }
            else
            {
                var rangeCoercionTypes = CoercionUtil.GetCoercionTypesRange(
                    new EventType[]
                    {
                        streamViewType,
                        polledViewType
                    }, 1, rangeIndexes, rangeKeys);
                if (rangeKeys.Count == 1 && hashKeys.Count == 0)
                {
                    var rangeCoercionType = rangeCoercionTypes.IsCoerce ? rangeCoercionTypes.CoercionTypes[0] : null;
                    var indexing = new PollResultIndexingStrategySorted(
                        polledViewStreamNum, polledViewType, rangeIndexes[0], rangeCoercionType);
                    var strategy = new HistoricalIndexLookupStrategySorted(streamViewStreamNum, rangeKeys[0]);
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }
                else
                {
                    var indexing = new PollResultIndexingStrategyComposite(
                        polledViewStreamNum, polledViewType, hashIndexes, keyCoercionTypes.CoercionTypes, rangeIndexes,
                        rangeCoercionTypes.CoercionTypes);
                    var strategy = new HistoricalIndexLookupStrategyComposite(
                        streamViewStreamNum, hashKeys, keyCoercionTypes.CoercionTypes, rangeKeys,
                        rangeCoercionTypes.CoercionTypes);
                    return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy>(strategy, indexing);
                }
            }
        }
    }
} // end of namespace
