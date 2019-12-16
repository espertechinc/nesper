///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.support;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplan;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlannerUtil
    {
        public static SubordinateQueryPlannerIndexPropListPair ToListOfHashedAndBtreeProps(
            string[] hashIndexPropsProvided,
            Type[] hashIndexCoercionType,
            string[] rangeIndexPropsProvided,
            Type[] rangeIndexCoercionType)
        {
            IList<IndexedPropDesc> hashedProps = new List<IndexedPropDesc>();
            IList<IndexedPropDesc> btreeProps = new List<IndexedPropDesc>();
            for (var i = 0; i < hashIndexPropsProvided.Length; i++) {
                hashedProps.Add(new IndexedPropDesc(hashIndexPropsProvided[i], hashIndexCoercionType[i]));
            }

            for (var i = 0; i < rangeIndexPropsProvided.Length; i++) {
                btreeProps.Add(new IndexedPropDesc(rangeIndexPropsProvided[i], rangeIndexCoercionType[i]));
            }

            return new SubordinateQueryPlannerIndexPropListPair(hashedProps, btreeProps);
        }

        public static void QueryPlanLogOnExpr(
            bool queryPlanLogging,
            ILog queryPlanLog,
            SubordinateWMatchExprQueryPlanForge strategy,
            Attribute[] annotations,
            ImportService importService)
        {
            QueryPlanIndexHook hook = QueryPlanIndexHookUtil.GetHook(annotations, importService);
            if (queryPlanLogging && (queryPlanLog.IsInfoEnabled || hook != null)) {
                var prefix = "On-Expr ";
                queryPlanLog.Info(prefix + "strategy " + strategy.Strategy.ToQueryPlan());
                if (strategy.Indexes == null) {
                    queryPlanLog.Info(prefix + "full table scan");
                }
                else {
                    for (var i = 0; i < strategy.Indexes.Length; i++) {
                        string indexName = strategy.Indexes[i].IndexName;
                        var indexText = indexName != null ? "index " + indexName + " " : "(implicit) (" + i + ")";
                        queryPlanLog.Info(prefix + indexText);
                    }
                }

                if (hook != null) {
                    var pairs = GetPairs(strategy.Indexes);
                    SubordTableLookupStrategyFactoryForge inner = strategy.Strategy.OptionalInnerStrategy;
                    hook.InfraOnExpr(
                        new QueryPlanIndexDescOnExpr(
                            pairs,
                            strategy.Strategy.GetType().GetSimpleName(),
                            inner == null ? null : inner.GetType().GetSimpleName()));
                }
            }
        }

        public static void QueryPlanLogOnSubq(
            bool queryPlanLogging,
            ILog queryPlanLog,
            SubordinateQueryPlanDescForge plan,
            int subqueryNum,
            Attribute[] annotations,
            ImportService importService)
        {
            QueryPlanIndexHook hook = QueryPlanIndexHookUtil.GetHook(annotations, importService);
            if (queryPlanLogging && (queryPlanLog.IsInfoEnabled || hook != null)) {
                var prefix = "Subquery " + subqueryNum + " ";
                var strategy = plan == null || plan.LookupStrategyFactory == null
                    ? "table scan"
                    : plan.LookupStrategyFactory.ToQueryPlan();
                queryPlanLog.Info(prefix + "strategy " + strategy);
                if (plan != null) {
                    if (plan.IndexDescs != null) {
                        for (var i = 0; i < plan.IndexDescs.Length; i++) {
                            var indexName = plan.IndexDescs[i].IndexName;
                            var indexText = indexName != null ? "index " + indexName + " " : "(implicit) ";
                            queryPlanLog.Info(prefix + "shared index");
                            queryPlanLog.Info(prefix + indexText);
                        }
                    }
                }

                if (hook != null) {
                    var pairs = plan == null ? new IndexNameAndDescPair[0] : GetPairs(plan.IndexDescs);
                    var factory = plan?.LookupStrategyFactory.GetType().GetSimpleName();
                    hook.Subquery(new QueryPlanIndexDescSubquery(pairs, subqueryNum, factory));
                }
            }
        }

        private static IndexNameAndDescPair[] GetPairs(SubordinateQueryIndexDescForge[] indexDescs)
        {
            if (indexDescs == null) {
                return null;
            }

            var pairs = new IndexNameAndDescPair[indexDescs.Length];
            for (var i = 0; i < indexDescs.Length; i++) {
                var index = indexDescs[i];
                pairs[i] = new IndexNameAndDescPair(index.IndexName, index.IndexMultiKey.ToQueryPlan());
            }

            return pairs;
        }

        /// <summary>
        ///     Given an index with a defined set of hash(equals) and range(btree) props and uniqueness flag,
        ///     and given a list of indexable properties and accessAccessors for both hash and range,
        ///     return the ordered keys and coercion information.
        /// </summary>
        /// <param name="indexMultiKey">index definition</param>
        /// <param name="hashIndexPropsProvided">hash indexable properties</param>
        /// <param name="hashJoinedProps">keys for hash indexable properties</param>
        /// <param name="rangeIndexPropsProvided">btree indexable properties</param>
        /// <param name="rangeJoinedProps">keys for btree indexable properties</param>
        /// <returns>ordered set of key information</returns>
        public static IndexKeyInfo CompileIndexKeyInfo(
            IndexMultiKey indexMultiKey,
            string[] hashIndexPropsProvided,
            SubordPropHashKeyForge[] hashJoinedProps,
            string[] rangeIndexPropsProvided,
            SubordPropRangeKeyForge[] rangeJoinedProps)
        {
            // map the order of indexed columns (key) to the key information available
            var indexedKeyProps = indexMultiKey.HashIndexedProps;
            var isCoerceHash = false;
            var hashesDesc = new SubordPropHashKeyForge[indexedKeyProps.Length];
            var hashPropCoercionTypes = new Type[indexedKeyProps.Length];

            for (var i = 0; i < indexedKeyProps.Length; i++) {
                var indexField = indexedKeyProps[i].IndexPropName;
                var index = CollectionUtil.FindItem(hashIndexPropsProvided, indexField);
                if (index == -1) {
                    throw new IllegalStateException("Could not find index property for lookup '" + indexedKeyProps[i]);
                }

                hashesDesc[i] = hashJoinedProps[index];
                hashPropCoercionTypes[i] = indexedKeyProps[i].CoercionType;
                var keyForge = hashesDesc[i].HashKey.KeyExpr.Forge;
                var evaluatorHashkey = keyForge.ExprEvaluator;
                if (evaluatorHashkey != null &&
                    indexedKeyProps[i].CoercionType.GetBoxedType() !=
                    keyForge.EvaluationType.GetBoxedType()) { // we allow null evaluator
                    isCoerceHash = true;
                }
            }

            // map the order of range columns (range) to the range information available
            indexedKeyProps = indexMultiKey.RangeIndexedProps;
            var rangesDesc = new SubordPropRangeKeyForge[indexedKeyProps.Length];
            var rangePropCoercionTypes = new Type[indexedKeyProps.Length];
            var isCoerceRange = false;
            for (var i = 0; i < indexedKeyProps.Length; i++) {
                var indexField = indexedKeyProps[i].IndexPropName;
                var index = CollectionUtil.FindItem(rangeIndexPropsProvided, indexField);
                if (index == -1) {
                    throw new IllegalStateException("Could not find range property for lookup '" + indexedKeyProps[i]);
                }

                rangesDesc[i] = rangeJoinedProps[index];
                rangePropCoercionTypes[i] = rangeJoinedProps[index].CoercionType;
                if (indexedKeyProps[i].CoercionType.GetBoxedType() != rangePropCoercionTypes[i].GetBoxedType()) {
                    isCoerceRange = true;
                }
            }

            return new IndexKeyInfo(
                hashesDesc,
                new CoercionDesc(isCoerceHash, hashPropCoercionTypes),
                rangesDesc,
                new CoercionDesc(isCoerceRange, rangePropCoercionTypes));
        }

        public static EventTable[] RealizeTables(
            SubordinateQueryIndexDesc[] indexDescriptors,
            EventType eventType,
            EventTableIndexRepository indexRepository,
            IEnumerable<EventBean> contents,
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var tables = new EventTable[indexDescriptors.Length];
            for (var i = 0; i < tables.Length; i++) {
                var desc = indexDescriptors[i];
                var table = indexRepository.GetIndexByDesc(desc.IndexMultiKey);
                if (table == null) {
                    table = EventTableUtil.BuildIndex(
                        agentInstanceContext,
                        0,
                        desc.QueryPlanIndexItem,
                        eventType,
                        true,
                        desc.QueryPlanIndexItem.IsUnique,
                        desc.IndexName,
                        null,
                        false);

                    // fill table since its new
                    if (!isRecoveringResilient) {
                        var events = new EventBean[1];
                        foreach (EventBean prefilledEvent in contents) {
                            events[0] = prefilledEvent;
                            table.Add(events, agentInstanceContext);
                        }
                    }

                    indexRepository.AddIndex(desc.IndexMultiKey, new EventTableIndexRepositoryEntry(null, null, table));
                }

                tables[i] = table;
            }

            return tables;
        }

        public static void AddIndexMetaAndRef(
            SubordinateQueryIndexDesc[] indexDescs,
            EventTableIndexMetadata repo,
            string deploymentId,
            string statementName)
        {
            foreach (var desc in indexDescs) {
                if (desc.IndexName != null) {
                    // this is handled by the create-index as it is an explicit index
                }
                else {
                    repo.AddIndexNonExplicit(desc.IndexMultiKey, deploymentId, desc.QueryPlanIndexItem);
                }
            }
        }
    }
} // end of namespace