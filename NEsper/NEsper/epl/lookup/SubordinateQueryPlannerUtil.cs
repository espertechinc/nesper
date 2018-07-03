///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.lookup
{
    public static class SubordinateQueryPlannerUtil
    {
        public static void QueryPlanLogOnExpr(
            bool queryPlanLogging,
            ILog queryPlanLog,
            SubordinateWMatchExprQueryPlanResult strategy,
            Attribute[] annotations,
            EngineImportService engineImportService)
        {
            var hook = QueryPlanIndexHookUtil.GetHook(annotations, engineImportService);
            if (queryPlanLogging && (queryPlanLog.IsInfoEnabled || hook != null))
            {
                var prefix = "On-Expr ";
                queryPlanLog.Info(prefix + "strategy " + strategy.Factory.ToQueryPlan());
                if (strategy.IndexDescs == null)
                {
                    queryPlanLog.Info(prefix + "full table scan");
                }
                else
                {
                    for (var i = 0; i < strategy.IndexDescs.Length; i++)
                    {
                        var indexName = strategy.IndexDescs[i].IndexName;
                        var indexText = indexName != null ? "index " + indexName + " " : "(implicit) (" + i + ")";
                        queryPlanLog.Info(prefix + indexText);
                    }
                }
                if (hook != null)
                {
                    var pairs = GetPairs(strategy.IndexDescs);
                    var inner = strategy.Factory.OptionalInnerStrategy;
                    hook.InfraOnExpr(new QueryPlanIndexDescOnExpr(pairs,
                            strategy.Factory.GetType().Name,
                            inner == null ? null : inner.GetType().Name));
                }
            }
        }

        public static void QueryPlanLogOnSubq(
            bool queryPlanLogging,
            ILog queryPlanLog,
            SubordinateQueryPlanDesc plan,
            int subqueryNum,
            Attribute[] annotations,
            EngineImportService engineImportService)
        {
            var hook = QueryPlanIndexHookUtil.GetHook(annotations, engineImportService);
            if (queryPlanLogging && (queryPlanLog.IsInfoEnabled || hook != null))
            {
                var prefix = "Subquery " + subqueryNum + " ";
                var strategy = (plan == null || plan.LookupStrategyFactory == null) ? "table scan" : plan.LookupStrategyFactory.ToQueryPlan();
                queryPlanLog.Info(prefix + "strategy " + strategy);
                if (plan != null)
                {
                    if (plan.IndexDescs != null)
                    {
                        for (var i = 0; i < plan.IndexDescs.Length; i++)
                        {
                            var indexName = plan.IndexDescs[i].IndexName;
                            var indexText = indexName != null ? "index " + indexName + " " : "(implicit) ";
                            queryPlanLog.Info(prefix + "shared index");
                            queryPlanLog.Info(prefix + indexText);
                        }
                    }
                }
                if (hook != null)
                {
                    var pairs = plan == null ? new IndexNameAndDescPair[0] : GetPairs(plan.IndexDescs);
                    string factory = plan == null ? null : plan.LookupStrategyFactory.GetType().Name;
                    hook.Subquery(new QueryPlanIndexDescSubquery(pairs, subqueryNum, factory));
                }
            }
        }

        private static IndexNameAndDescPair[] GetPairs(SubordinateQueryIndexDesc[] indexDescs)
        {
            if (indexDescs == null)
            {
                return null;
            }
            var pairs = new IndexNameAndDescPair[indexDescs.Length];
            for (var i = 0; i < indexDescs.Length; i++)
            {
                var index = indexDescs[i];
                pairs[i] = new IndexNameAndDescPair(index.IndexName, index.IndexMultiKey.ToQueryPlan());
            }
            return pairs;
        }

        public static SubordinateQueryPlannerIndexPropListPair ToListOfHashedAndBtreeProps(
            string[] hashIndexPropsProvided,
            Type[] hashIndexCoercionType,
            string[] rangeIndexPropsProvided,
            Type[] rangeIndexCoercionType)
        {
            IList<IndexedPropDesc> hashedProps = new List<IndexedPropDesc>();
            IList<IndexedPropDesc> btreeProps = new List<IndexedPropDesc>();
            for (var i = 0; i < hashIndexPropsProvided.Length; i++)
            {
                hashedProps.Add(new IndexedPropDesc(hashIndexPropsProvided[i], hashIndexCoercionType[i]));
            }
            for (var i = 0; i < rangeIndexPropsProvided.Length; i++)
            {
                btreeProps.Add(new IndexedPropDesc(rangeIndexPropsProvided[i], rangeIndexCoercionType[i]));
            }
            return new SubordinateQueryPlannerIndexPropListPair(hashedProps, btreeProps);
        }

        /// <summary>
        /// Given an index with a defined set of hash(equals) and range(btree) props and uniqueness flag,
        /// and given a list of indexable properties and accessors for both hash and range,
        /// return the ordered keys and coercion information.
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
            SubordPropHashKey[] hashJoinedProps,
            string[] rangeIndexPropsProvided,
            SubordPropRangeKey[] rangeJoinedProps)
        {
            // map the order of indexed columns (key) to the key information available
            var indexedKeyProps = indexMultiKey.HashIndexedProps;
            var isCoerceHash = false;
            var hashesDesc = new SubordPropHashKey[indexedKeyProps.Length];
            var hashPropCoercionTypes = new Type[indexedKeyProps.Length];

            for (var i = 0; i < indexedKeyProps.Length; i++)
            {
                var indexField = indexedKeyProps[i].IndexPropName;
                var index = CollectionUtil.FindItem(hashIndexPropsProvided, indexField);
                if (index == -1)
                {
                    throw new IllegalStateException("Could not find index property for lookup '" + indexedKeyProps[i]);
                }
                hashesDesc[i] = hashJoinedProps[index];
                hashPropCoercionTypes[i] = indexedKeyProps[i].CoercionType;
                var evaluatorHashkey = hashesDesc[i].HashKey.KeyExpr.ExprEvaluator;
                if (evaluatorHashkey != null && TypeHelper.GetBoxedType(indexedKeyProps[i].CoercionType) != TypeHelper.GetBoxedType(evaluatorHashkey.ReturnType))
                {   // we allow null evaluator
                    isCoerceHash = true;
                }
            }

            // map the order of range columns (range) to the range information available
            indexedKeyProps = indexMultiKey.RangeIndexedProps;
            var rangesDesc = new SubordPropRangeKey[indexedKeyProps.Length];
            var rangePropCoercionTypes = new Type[indexedKeyProps.Length];
            var isCoerceRange = false;
            for (var i = 0; i < indexedKeyProps.Length; i++)
            {
                var indexField = indexedKeyProps[i].IndexPropName;
                var index = CollectionUtil.FindItem(rangeIndexPropsProvided, indexField);
                if (index == -1)
                {
                    throw new IllegalStateException("Could not find range property for lookup '" + indexedKeyProps[i]);
                }
                rangesDesc[i] = rangeJoinedProps[index];
                rangePropCoercionTypes[i] = rangeJoinedProps[index].CoercionType;
                if (TypeHelper.GetBoxedType(indexedKeyProps[i].CoercionType) != TypeHelper.GetBoxedType(rangePropCoercionTypes[i]))
                {
                    isCoerceRange = true;
                }
            }

            return new IndexKeyInfo(hashesDesc,
                    new CoercionDesc(isCoerceHash, hashPropCoercionTypes), rangesDesc, new CoercionDesc(isCoerceRange, rangePropCoercionTypes));
        }

        private static IndexNameAndDescPair[] GetTableClassNamePairs(EventTableAndNamePair[] pairs)
        {
            if (pairs == null)
            {
                return new IndexNameAndDescPair[0];
            }
            var names = new IndexNameAndDescPair[pairs.Length];
            for (var i = 0; i < pairs.Length; i++)
            {
                names[i] = new IndexNameAndDescPair(pairs[i].IndexName, pairs[i].EventTable.ProviderClass.Name);
            }
            return names;
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
            for (var i = 0; i < tables.Length; i++)
            {
                var desc = indexDescriptors[i];
                var table = indexRepository.GetIndexByDesc(desc.IndexMultiKey);
                if (table == null)
                {
                    table = EventTableUtil.BuildIndex(agentInstanceContext, 0, desc.QueryPlanIndexItem, eventType, true, desc.IndexMultiKey.IsUnique, desc.IndexName, null, false);

                    // fill table since its new
                    if (!isRecoveringResilient)
                    {
                        var events = new EventBean[1];
                        foreach (var prefilledEvent in contents)
                        {
                            events[0] = prefilledEvent;
                            table.Add(events, agentInstanceContext);
                        }
                    }

                    indexRepository.AddIndex(desc.IndexMultiKey, new EventTableIndexRepositoryEntry(null, table));
                }
                tables[i] = table;
            }
            return tables;
        }

        public static void AddIndexMetaAndRef(SubordinateQueryIndexDesc[] indexDescs, EventTableIndexMetadata repo, string statementName)
        {
            foreach (var desc in indexDescs)
            {
                if (desc.IndexName != null)
                {
                    repo.AddIndexReference(desc.IndexName, statementName);
                }
                else
                {
                    repo.AddIndexNonExplicit(desc.IndexMultiKey, statementName, desc.QueryPlanIndexItem);
                    repo.AddIndexReference(desc.IndexMultiKey, statementName);
                }
            }
        }
    }
}
