///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.queryplanbuild;
using com.espertech.esper.common.@internal.epl.join.support;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeForgeFactory
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static JoinSetComposerPrototypeDesc MakeComposerPrototype(
            StatementSpecCompiled spec,
            StreamJoinAnalysisResultCompileTime joinAnalysisResult,
            StreamTypeService typeService,
            HistoricalViewableDesc historicalViewableDesc,
            bool isOnDemandQuery,
            bool hasAggregations,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            var streamTypes = typeService.EventTypes;
            var streamNames = typeService.StreamNames;
            var whereClause = spec.Raw.WhereClause;
            var queryPlanLogging = compileTimeServices.Configuration.Common.Logging.IsEnableQueryPlan;
            var additionalForgeables = new List<StmtClassForgeableFactory>();

            // Determine if there is a historical stream, and what dependencies exist
            var historicalDependencyGraph = new DependencyGraph(streamTypes.Length, false);
            for (var i = 0; i < streamTypes.Length; i++) {
                if (historicalViewableDesc.Historical[i]) {
                    var streamsThisStreamDependsOn = historicalViewableDesc.DependenciesPerHistorical[i];
                    historicalDependencyGraph.AddDependency(i, streamsThisStreamDependsOn);
                }
            }

            if (Log.IsDebugEnabled) {
                Log.Debug("Dependency graph: " + historicalDependencyGraph);
            }

            // Handle a join with a database or other historical data source for 2 streams
            var outerJoinDescs = OuterJoinDesc.ToArray(spec.Raw.OuterJoinDescList);
            if (historicalViewableDesc.IsHistorical && streamTypes.Length == 2) {
                var desc = MakeComposerHistorical2Stream(
                    outerJoinDescs,
                    whereClause,
                    streamTypes,
                    streamNames,
                    historicalViewableDesc,
                    queryPlanLogging,
                    statementRawInfo,
                    compileTimeServices);
                return new JoinSetComposerPrototypeDesc(desc.Forge, desc.AdditionalForgeables);
            }

            var isOuterJoins = !OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescs);

            // Query graph for graph relationships between streams/historicals
            // For outer joins the query graph will just contain outer join relationships
            var hint = ExcludePlanHint.GetHint(
                typeService.StreamNames,
                statementRawInfo,
                compileTimeServices);
            var queryGraph = new QueryGraphForge(streamTypes.Length, hint, false);
            if (outerJoinDescs.Length > 0) {
                OuterJoinAnalyzer.Analyze(outerJoinDescs, queryGraph);
                if (Log.IsDebugEnabled) {
                    Log.Debug(".makeComposer After outer join filterQueryGraph=\n" + queryGraph);
                }
            }

            // Let the query graph reflect the where-clause
            if (whereClause != null) {
                // Analyze relationships between streams using the optional filter expression.
                // Relationships are properties in AND and EQUALS nodes of joins.
                FilterExprAnalyzer.Analyze(whereClause, queryGraph, isOuterJoins);
                if (Log.IsDebugEnabled) {
                    Log.Debug(".makeComposer After filter expression filterQueryGraph=\n" + queryGraph);
                }

                // Add navigation entries based on key and index property equivalency (a=b, b=c follows a=c)
                QueryGraphForge.FillEquivalentNav(streamTypes, queryGraph);
                if (Log.IsDebugEnabled) {
                    Log.Debug(".makeComposer After fill equiv. nav. filterQueryGraph=\n" + queryGraph);
                }
            }

            // Historical index lists
            var historicalStreamIndexLists =
                new HistoricalStreamIndexListForge[streamTypes.Length];

            var queryPlanDesc = QueryPlanBuilder.GetPlan(
                streamTypes,
                outerJoinDescs,
                queryGraph,
                typeService.StreamNames,
                historicalViewableDesc,
                historicalDependencyGraph,
                historicalStreamIndexLists,
                joinAnalysisResult,
                queryPlanLogging,
                statementRawInfo,
                compileTimeServices);
            QueryPlanForge queryPlan = queryPlanDesc.Forge;
            additionalForgeables.AddAll(queryPlanDesc.AdditionalForgeables);

            // remove unused indexes - consider all streams or all unidirectional
            var usedIndexes = new HashSet<TableLookupIndexReqKey>();
            var indexSpecs = queryPlan.IndexSpecs;
            for (var streamNum = 0; streamNum < queryPlan.ExecNodeSpecs.Length; streamNum++) {
                var planNode = queryPlan.ExecNodeSpecs[streamNum];
                planNode?.AddIndexes(usedIndexes);
            }

            foreach (var indexSpec in indexSpecs) {
                if (indexSpec == null) {
                    continue;
                }

                var items = indexSpec.Items;
                var indexNames = items.Keys.ToArray();
                foreach (var indexName in indexNames) {
                    if (!usedIndexes.Contains(indexName)) {
                        items.Remove(indexName);
                    }
                }
            }

            // plan multikeys
            IList<StmtClassForgeableFactory> multikeyForgeables = PlanMultikeys(
                indexSpecs, statementRawInfo, compileTimeServices);
            additionalForgeables.AddAll(multikeyForgeables);
            
            // plan objectsettings
            PlanStateMgmtSettings(indexSpecs, statementRawInfo, compileTimeServices);

            QueryPlanIndexHook hook = QueryPlanIndexHookUtil.GetHook(
                spec.Annotations,
                compileTimeServices.ImportServiceCompileTime);
            if (queryPlanLogging && (QUERY_PLAN_LOG.IsInfoEnabled || hook != null)) {
                QUERY_PLAN_LOG.Info("Query plan: " + queryPlan.ToQueryPlan());
                hook?.Join(queryPlan);
            }

            var selectsRemoveStream =
                spec.Raw.SelectStreamSelectorEnum.IsSelectsRStream() || spec.Raw.OutputLimitSpec != null;
            var joinRemoveStream = selectsRemoveStream || hasAggregations;

            ExprNode postJoinEvaluator;
            if (JoinSetComposerUtil.IsNonUnidirectionalNonSelf(
                isOuterJoins,
                joinAnalysisResult.IsUnidirectional,
                joinAnalysisResult.IsPureSelfJoin)) {
                postJoinEvaluator = GetFilterExpressionInclOnClause(
                    spec.Raw.WhereClause,
                    outerJoinDescs,
                    statementRawInfo,
                    compileTimeServices);
            }
            else {
                postJoinEvaluator = spec.Raw.WhereClause;
            }
            
            JoinSetComposerPrototypeGeneralForge forge = new JoinSetComposerPrototypeGeneralForge(
                typeService.EventTypes,
                postJoinEvaluator, 
                outerJoinDescs.Length > 0,
                queryPlan, 
                joinAnalysisResult, 
                typeService.StreamNames,
                joinRemoveStream, 
                historicalViewableDesc.IsHistorical);
            return new JoinSetComposerPrototypeDesc(forge, additionalForgeables);
        }

        private static List<StmtClassForgeableFactory> PlanMultikeys(
            QueryPlanIndexForge[] indexSpecs,
            StatementRawInfo raw,
            StatementCompileTimeServices compileTimeServices)
        {
            List<StmtClassForgeableFactory> multiKeyForgeables = new List<StmtClassForgeableFactory>();
            foreach (QueryPlanIndexForge spec in indexSpecs) {
                if (spec == null) {
                    continue;
                }
                foreach (var entry in spec.Items) {
                    QueryPlanIndexItemForge forge = entry.Value;

                    MultiKeyPlan plan = MultiKeyPlanner.PlanMultiKey(
                        forge.HashTypes, false, raw, compileTimeServices.SerdeResolver);
                    multiKeyForgeables.AddAll(plan.MultiKeyForgeables);
                    forge.HashMultiKeyClasses = plan.ClassRef;

                    DataInputOutputSerdeForge[] rangeSerdes = new DataInputOutputSerdeForge[forge.RangeTypes.Length];
                    for (int i = 0; i < forge.RangeTypes.Length; i++) {
                        rangeSerdes[i] = compileTimeServices.SerdeResolver.SerdeForIndexBtree(forge.RangeTypes[i], raw);
                    }
                    forge.RangeSerdes = rangeSerdes;
                }
            }
            return multiKeyForgeables;
        }

        private static void PlanStateMgmtSettings(
            QueryPlanIndexForge[] indexSpecs,
            StatementRawInfo raw,
            StatementCompileTimeServices compileTimeServices)
        {
            foreach (QueryPlanIndexForge spec in indexSpecs) {
                if (spec == null) {
                    continue;
                }

                foreach (var entry in spec.Items) {
                    var forge = entry.Value;
                    forge.PlanStateMgmtSettings(raw, compileTimeServices);
                }
            }
        }

        private static JoinSetComposerPrototypeHistorical2StreamDesc MakeComposerHistorical2Stream(
            OuterJoinDesc[] outerJoinDescs,
            ExprNode whereClause,
            EventType[] streamTypes,
            string[] streamNames,
            HistoricalViewableDesc historicalViewableDesc,
            bool queryPlanLogging,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var polledViewNum = 0;
            var streamViewNum = 1;
            if (historicalViewableDesc.Historical[1]) {
                streamViewNum = 0;
                polledViewNum = 1;
            }

            // if all-historical join, check dependency
            var isAllHistoricalNoSubordinate = false;
            if (historicalViewableDesc.Historical[0] && historicalViewableDesc.Historical[1]) {
                var graph = new DependencyGraph(2, false);
                graph.AddDependency(0, historicalViewableDesc.DependenciesPerHistorical[0]);
                graph.AddDependency(1, historicalViewableDesc.DependenciesPerHistorical[1]);
                if (graph.FirstCircularDependency != null) {
                    throw new ExprValidationException("Circular dependency detected between historical streams");
                }

                // if both streams are independent
                if (graph.RootNodes.Count == 2) {
                    isAllHistoricalNoSubordinate = true; // No parameters used by either historical
                }
                else {
                    if (graph.GetDependenciesForStream(0).Count == 0) {
                        streamViewNum = 0;
                        polledViewNum = 1;
                    }
                    else {
                        streamViewNum = 1;
                        polledViewNum = 0;
                    }
                }
            }

            // Build an outer join expression node
            var isOuterJoin = false;
            ExprNode outerJoinEqualsNode = null;
            var isInnerJoinOnly = false;
            var outerJoinPerStream = new bool[2];
            if (outerJoinDescs != null && outerJoinDescs.Length > 0) {
                var outerJoinDesc = outerJoinDescs[0];
                isInnerJoinOnly = outerJoinDesc.OuterJoinType.Equals(OuterJoinType.INNER);

                if (isAllHistoricalNoSubordinate) {
                    if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.FULL)) {
                        isOuterJoin = true;
                        outerJoinPerStream[0] = true;
                        outerJoinPerStream[1] = true;
                    }
                    else if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.LEFT)) {
                        isOuterJoin = true;
                        outerJoinPerStream[0] = true;
                    }
                    else if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.RIGHT)) {
                        isOuterJoin = true;
                        outerJoinPerStream[1] = true;
                    }
                }
                else {
                    if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.FULL)) {
                        isOuterJoin = true;
                        outerJoinPerStream[0] = true;
                        outerJoinPerStream[1] = true;
                    }
                    else if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.LEFT) &&
                             streamViewNum == 0) {
                        isOuterJoin = true;
                        outerJoinPerStream[0] = true;
                    }
                    else if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.RIGHT) &&
                             streamViewNum == 1) {
                        isOuterJoin = true;
                        outerJoinPerStream[1] = true;
                    }
                }

                outerJoinEqualsNode = outerJoinDesc.MakeExprNode(statementRawInfo, services);
            }

            // Determine filter for indexing purposes
            ExprNode filterForIndexing = null;
            if (outerJoinEqualsNode != null && whereClause != null && isInnerJoinOnly) {
                // both filter and outer join, add
                filterForIndexing = new ExprAndNodeImpl();
                filterForIndexing.AddChildNode(whereClause);
                filterForIndexing.AddChildNode(outerJoinEqualsNode);
            }
            else if (outerJoinEqualsNode == null && whereClause != null) {
                filterForIndexing = whereClause;
            }
            else if (outerJoinEqualsNode != null) {
                filterForIndexing = outerJoinEqualsNode;
            }

            var indexStrategies =
                DetermineIndexing(
                    filterForIndexing,
                    streamTypes[polledViewNum],
                    streamTypes[streamViewNum],
                    polledViewNum,
                    streamViewNum,
                    streamNames,
                    statementRawInfo,
                    services);

            QueryPlanIndexHook hook = QueryPlanIndexHookUtil.GetHook(
                statementRawInfo.Annotations,
                services.ImportServiceCompileTime);
            if (queryPlanLogging && (QUERY_PLAN_LOG.IsInfoEnabled || hook != null)) {
                QUERY_PLAN_LOG.Info("historical lookup strategy: " + indexStrategies.LookupForge.ToQueryPlan());
                QUERY_PLAN_LOG.Info("historical index strategy: " + indexStrategies.IndexingForge.ToQueryPlan());
                hook?.Historical(
                    new QueryPlanIndexDescHistorical(
                        indexStrategies.LookupForge.GetType().GetSimpleName(),
                        indexStrategies.IndexingForge.GetType().GetSimpleName()));
            }

            JoinSetComposerPrototypeHistorical2StreamForge forge = new JoinSetComposerPrototypeHistorical2StreamForge(
                streamTypes,
                whereClause,
                isOuterJoin,
                polledViewNum,
                streamViewNum,
                outerJoinEqualsNode,
                indexStrategies.LookupForge,
                indexStrategies.IndexingForge,
                isAllHistoricalNoSubordinate,
                outerJoinPerStream);
            return new JoinSetComposerPrototypeHistorical2StreamDesc(
                forge, indexStrategies.AdditionalForgeables);
        }

        private static ExprNode GetFilterExpressionInclOnClause(
            ExprNode whereClause,
            OuterJoinDesc[] outerJoinDescList,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            if (whereClause == null) { // no need to add as query planning is fully based on on-clause
                return null;
            }

            if (outerJoinDescList.Length == 0) { // not an outer-join syntax
                return whereClause;
            }

            if (!OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList)) { // all-inner joins
                return whereClause;
            }

            var hasOnClauses = OuterJoinDesc.HasOnClauses(outerJoinDescList);
            if (!hasOnClauses) {
                return whereClause;
            }

            IList<ExprNode> expressions = new List<ExprNode>();
            expressions.Add(whereClause);

            foreach (var outerJoinDesc in outerJoinDescList) {
                if (outerJoinDesc.OptLeftNode != null) {
                    expressions.Add(outerJoinDesc.MakeExprNode(rawInfo, services));
                }
            }

            var andNode = ExprNodeUtilityMake.ConnectExpressionsByLogicalAnd(expressions);
            try {
                andNode.Validate(null);
            }
            catch (ExprValidationException ex) {
                throw new EPRuntimeException("Unexpected exception validating expression: " + ex.Message, ex);
            }

            return andNode;
        }

        private static JoinSetComposerPrototypeHistoricalDesc DetermineIndexing(
            ExprNode filterForIndexing,
            EventType polledViewType,
            EventType streamViewType,
            int polledViewStreamNum,
            int streamViewStreamNum,
            string[] streamNames,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            // No filter means unindexed event tables
            if (filterForIndexing == null) {
                return new JoinSetComposerPrototypeHistoricalDesc(
                    HistoricalIndexLookupStrategyNoIndexForge.INSTANCE,
                    PollResultIndexingStrategyNoIndexForge.INSTANCE,
                    EmptyList<StmtClassForgeableFactory>.Instance);
            }

            // analyze query graph; Whereas stream0=named window, stream1=delete-expr filter
            var hint = ExcludePlanHint.GetHint(streamNames, rawInfo, services);
            var queryGraph = new QueryGraphForge(2, hint, false);
            FilterExprAnalyzer.Analyze(filterForIndexing, queryGraph, false);

            return DetermineIndexing(
                queryGraph,
                polledViewType,
                streamViewType,
                polledViewStreamNum,
                streamViewStreamNum,
                rawInfo,
                services.SerdeResolver);
        }

        /// <summary>
        ///     Constructs indexing and lookup strategy for a given relationship that a historical stream may have with another
        ///     stream (historical or not) that looks up into results of a poll of a historical stream.
        ///     <para />
        ///     The term "polled" refers to the assumed-historical stream.
        /// </summary>
        /// <param name="queryGraph">relationship representation of where-clause filter and outer join on-expressions</param>
        /// <param name="polledViewType">the event type of the historical that is indexed</param>
        /// <param name="streamViewType">the event type of the stream looking up in indexes</param>
        /// <param name="polledViewStreamNum">the stream number of the historical that is indexed</param>
        /// <param name="streamViewStreamNum">the stream number of the historical that is looking up</param>
        /// <param name="raw"></param>
        /// <param name="serdeResolver"></param>
        /// <returns>indexing and lookup strategy pair</returns>
        public static JoinSetComposerPrototypeHistoricalDesc DetermineIndexing(
            QueryGraphForge queryGraph,
            EventType polledViewType,
            EventType streamViewType,
            int polledViewStreamNum,
            int streamViewStreamNum,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
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
            if (hashKeys.IsEmpty() && rangeKeys.IsEmpty()) {
                var inKeywordSingles = queryGraphValue.InKeywordSingles;
                if (inKeywordSingles != null && inKeywordSingles.Indexed.Length != 0) {
                    var indexed = inKeywordSingles.Indexed[0];
                    var lookup = inKeywordSingles.Key[0];
                    var strategy =
                        new HistoricalIndexLookupStrategyInKeywordSingleForge(streamViewStreamNum, lookup.KeyExprs);
                    var indexing = new PollResultIndexingStrategyHashForge(
                        polledViewStreamNum,
                        polledViewType,
                        new string[]{ indexed },
                        null,
                        null);
                    return new JoinSetComposerPrototypeHistoricalDesc(strategy, indexing, EmptyList<StmtClassForgeableFactory>.Instance);
                }

                var multis = queryGraphValue.InKeywordMulti;
                if (!multis.IsEmpty()) {
                    var multi = multis[0];
                    var strategy =
                        new HistoricalIndexLookupStrategyInKeywordMultiForge(streamViewStreamNum, multi.Key.KeyExpr);
                    var indexing =
                        new PollResultIndexingStrategyInKeywordMultiForge(
                            polledViewStreamNum,
                            polledViewType,
                            ExprNodeUtilityQuery.GetIdentResolvedPropertyNames(multi.Indexed));
                    return new JoinSetComposerPrototypeHistoricalDesc(strategy, indexing, EmptyList<StmtClassForgeableFactory>.Instance);
                }

                return new JoinSetComposerPrototypeHistoricalDesc(
                    HistoricalIndexLookupStrategyNoIndexForge.INSTANCE,
                    PollResultIndexingStrategyNoIndexForge.INSTANCE,
                    EmptyList<StmtClassForgeableFactory>.Instance);
            }

            CoercionDesc keyCoercionTypes = CoercionUtil.GetCoercionTypesHash(
                new[] {streamViewType, polledViewType},
                0,
                1,
                hashKeys,
                hashIndexes);

            if (rangeKeys.IsEmpty()) {
                var hashEvals = QueryGraphValueEntryHashKeyedForge.GetForges(hashKeys.ToArray());
                var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(hashEvals, false, raw, serdeResolver);
                var lookup = new HistoricalIndexLookupStrategyHashForge(
                    streamViewStreamNum, hashEvals, keyCoercionTypes.CoercionTypes, multiKeyPlan.ClassRef);
                var indexing = new PollResultIndexingStrategyHashForge(
                    polledViewStreamNum,
                    polledViewType,
                    hashIndexes,
                    keyCoercionTypes.CoercionTypes,
                    multiKeyPlan.ClassRef);
                return new JoinSetComposerPrototypeHistoricalDesc(lookup, indexing, multiKeyPlan.MultiKeyForgeables);
            }

            CoercionDesc rangeCoercionTypes = CoercionUtil.GetCoercionTypesRange(
                new[] {streamViewType, polledViewType},
                1,
                rangeIndexes,
                rangeKeys);

            if (rangeKeys.Count == 1 && hashKeys.Count == 0) {
                var rangeCoercionType = rangeCoercionTypes.CoercionTypes[0];
                var indexing = new PollResultIndexingStrategySortedForge(
                    polledViewStreamNum,
                    polledViewType,
                    rangeIndexes[0],
                    rangeCoercionType);
                var lookup = new HistoricalIndexLookupStrategySortedForge(
                    streamViewStreamNum,
                    rangeKeys[0],
                    rangeCoercionType);
                return new JoinSetComposerPrototypeHistoricalDesc(lookup, indexing, EmptyList<StmtClassForgeableFactory>.Instance);
            }
            else {
                var hashEvals = QueryGraphValueEntryHashKeyedForge.GetForges(hashKeys.ToArray());
                var multiKeyPlan = MultiKeyPlanner.PlanMultiKey(hashEvals, false, raw, serdeResolver);
                var strategy = new HistoricalIndexLookupStrategyCompositeForge(
                    streamViewStreamNum,
                    hashEvals,
                    multiKeyPlan.ClassRef,
                    rangeKeys.ToArray());
                var indexing = new PollResultIndexingStrategyCompositeForge(
                    polledViewStreamNum,
                    polledViewType,
                    hashIndexes,
                    keyCoercionTypes.CoercionTypes,
                    multiKeyPlan.ClassRef,
                    rangeIndexes,
                    rangeCoercionTypes.CoercionTypes);
                return new JoinSetComposerPrototypeHistoricalDesc(strategy, indexing, multiKeyPlan.MultiKeyForgeables);
            }
        }
    }
} // end of namespace