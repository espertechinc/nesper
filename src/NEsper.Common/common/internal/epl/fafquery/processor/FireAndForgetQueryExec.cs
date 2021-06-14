///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.join.exec.composite;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.hint;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.support;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using Range = com.espertech.esper.common.@internal.filterspec.Range;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetQueryExec
    {
        private static readonly ILog QUERY_PLAN_LOG = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        public static ICollection<EventBean> Snapshot(
            QueryGraph filterQueryGraph,
            Attribute[] annotations,
            VirtualDWView virtualDataWindow,
            EventTableIndexRepository indexRepository,
            string objectName,
            AgentInstanceContext agentInstanceContext)
        {
            var queryGraphValue = filterQueryGraph == null
                ? null
                : filterQueryGraph.GetGraphValue(QueryGraphForge.SELF_STREAM, 0);
            if (queryGraphValue == null || queryGraphValue.Items.IsEmpty()) {
                if (virtualDataWindow != null) {
                    Pair<IndexMultiKey, EventTable> pair = VirtualDWQueryPlanUtil.GetFireAndForgetDesc(
                        virtualDataWindow.EventType,
                        new EmptySet<string>(),
                        new EmptySet<string>());
                    return virtualDataWindow.GetFireAndForgetData(
                        pair.Second,
                        CollectionUtil.OBJECTARRAY_EMPTY,
                        new RangeIndexLookupValue[0],
                        annotations);
                }

                return null;
            }

            // determine custom index
            var customResult = SnapshotCustomIndex(
                queryGraphValue,
                indexRepository,
                annotations,
                agentInstanceContext,
                objectName);
            if (customResult != null) {
                return customResult.Value;
            }

            // determine lookup based on hash-keys and ranges
            var keysAvailable = queryGraphValue.HashKeyProps;
            ISet<string> keyNamesAvailable = keysAvailable.Indexed.Length == 0
                ? (ISet<string>) new EmptySet<string>()
                : (ISet<string>) new HashSet<string>(keysAvailable.Indexed);
            var rangesAvailable = queryGraphValue.RangeProps;
            ISet<string> rangeNamesAvailable = rangesAvailable.Indexed.Length == 0
                ? (ISet<string>) new EmptySet<string>()
                : (ISet<string>) new HashSet<string>(rangesAvailable.Indexed);
            Pair<IndexMultiKey, EventTableAndNamePair> tablePair;

            // find index that matches the needs
            tablePair = FindIndex(
                keyNamesAvailable,
                rangeNamesAvailable,
                indexRepository,
                virtualDataWindow,
                annotations);

            // regular index lookup
            if (tablePair != null) {
                return SnapshotIndex(
                    keysAvailable,
                    rangesAvailable,
                    tablePair,
                    virtualDataWindow,
                    annotations,
                    agentInstanceContext,
                    objectName);
            }

            // in-keyword lookup
            var inkwResult = SnapshotInKeyword(
                queryGraphValue,
                indexRepository,
                virtualDataWindow,
                annotations,
                agentInstanceContext,
                objectName);
            if (inkwResult != null) {
                return inkwResult.Value;
            }

            QueryPlanReportTableScan(annotations, agentInstanceContext, objectName);
            return null;
        }

        private static Pair<IndexMultiKey, EventTableAndNamePair> FindIndex(
            ISet<string> keyNamesAvailable,
            ISet<string> rangeNamesAvailable,
            EventTableIndexRepository indexRepository,
            VirtualDWView virtualDataWindow,
            Attribute[] annotations)
        {
            if (virtualDataWindow != null) {
                var tablePairNoName = VirtualDWQueryPlanUtil.GetFireAndForgetDesc(
                    virtualDataWindow.EventType,
                    keyNamesAvailable,
                    rangeNamesAvailable);
                return new Pair<IndexMultiKey, EventTableAndNamePair>(
                    tablePairNoName.First,
                    new EventTableAndNamePair(tablePairNoName.Second, null));
            }

            var indexHint = IndexHint.GetIndexHint(annotations);
            IList<IndexHintInstruction> optionalIndexHintInstructions =
                indexHint != null ? indexHint.GetInstructionsFireAndForget() : null;
            return indexRepository.FindTable(keyNamesAvailable, rangeNamesAvailable, optionalIndexHintInstructions);
        }

        private static NullableObject<ICollection<EventBean>> SnapshotInKeyword(
            QueryGraphValue queryGraphValue,
            EventTableIndexRepository indexRepository,
            VirtualDWView virtualDataWindow,
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext,
            string objectName)
        {
            var inkwSingles = queryGraphValue.InKeywordSingles;
            if (inkwSingles.Indexed.Length == 0) {
                return null;
            }

            Pair<IndexMultiKey, EventTableAndNamePair> tablePair = FindIndex(
                new HashSet<string>(inkwSingles.Indexed),
                new EmptySet<string>(),
                indexRepository,
                virtualDataWindow,
                annotations);
            if (tablePair == null) {
                return null;
            }

            QueryPlanReport(
                tablePair.Second.IndexName,
                tablePair.Second.EventTable,
                annotations,
                agentInstanceContext,
                objectName);

            // table lookup with in-clause: determine combinations
            var tableHashProps = tablePair.First.HashIndexedProps;
            var combinations = new object[tableHashProps.Length][];
            for (var tableHashPropNum = 0; tableHashPropNum < tableHashProps.Length; tableHashPropNum++) {
                for (var i = 0; i < inkwSingles.Indexed.Length; i++) {
                    if (inkwSingles.Indexed[i].Equals(tableHashProps[tableHashPropNum].IndexPropName)) {
                        QueryGraphValueEntryInKeywordSingleIdx keysExpressions = inkwSingles.Key[i];
                        var values = new object[keysExpressions.KeyExprs.Length];
                        combinations[tableHashPropNum] = values;
                        for (var j = 0; j < keysExpressions.KeyExprs.Length; j++) {
                            values[j] = keysExpressions.KeyExprs[j].Evaluate(null, true, agentInstanceContext);
                        }
                    }
                }
            }

            // enumerate combinations
            var enumeration = new CombinationEnumeration(combinations);
            var events = new HashSet<EventBean>();
            while (enumeration.MoveNext()) {
                object[] keys = enumeration.Current;
                var result = FafTableLookup(
                    virtualDataWindow,
                    tablePair.First,
                    tablePair.Second.EventTable,
                    keys,
                    null,
                    annotations,
                    agentInstanceContext);
                events.AddAll(result);
            }

            return new NullableObject<ICollection<EventBean>>(events);
        }

        private static ICollection<EventBean> SnapshotIndex(
            QueryGraphValuePairHashKeyIndex keysAvailable,
            QueryGraphValuePairRangeIndex rangesAvailable,
            Pair<IndexMultiKey, EventTableAndNamePair> tablePair,
            VirtualDWView virtualDataWindow,
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext,
            string objectName)
        {
            // report plan
            QueryPlanReport(
                tablePair.Second.IndexName,
                tablePair.Second.EventTable,
                annotations,
                agentInstanceContext,
                objectName);

            // compile hash lookup values
            var tableHashProps = tablePair.First.HashIndexedProps;
            var keyValues = new object[tableHashProps.Length];
            for (var tableHashPropNum = 0; tableHashPropNum < tableHashProps.Length; tableHashPropNum++) {
                var tableHashProp = tableHashProps[tableHashPropNum];
                for (var i = 0; i < keysAvailable.Indexed.Length; i++) {
                    if (keysAvailable.Indexed[i].Equals(tableHashProp.IndexPropName)) {
                        var key = keysAvailable.Keys[i];
                        var value = key.KeyExpr.Evaluate(null, true, agentInstanceContext);
                        if (value != null) {
                            value = MayCoerceNonNull(value, tableHashProp.CoercionType);
                            keyValues[tableHashPropNum] = value;
                        }
                    }
                }
            }

            // compile range lookup values
            var tableRangeProps = tablePair.First.RangeIndexedProps;
            var rangeValues = new RangeIndexLookupValue[tableRangeProps.Length];
            for (var tableRangePropNum = 0; tableRangePropNum < tableRangeProps.Length; tableRangePropNum++) {
                var tableRangeProp = tableRangeProps[tableRangePropNum];
                for (var i = 0; i < rangesAvailable.Indexed.Length; i++) {
                    if (rangesAvailable.Indexed[i].Equals(tableRangeProp.IndexPropName)) {
                        var range = rangesAvailable.Keys[i];
                        if (range is QueryGraphValueEntryRangeIn) {
                            var between = (QueryGraphValueEntryRangeIn) range;
                            var start = between.ExprStart.Evaluate(null, true, agentInstanceContext);
                            var end = between.ExprEnd.Evaluate(null, true, agentInstanceContext);
                            Range rangeValue;
                            if (tableRangeProp.CoercionType.IsNumeric()) {
                                double? startDouble = null;
                                if (start != null) {
                                    startDouble = start.AsDouble();
                                }

                                double? endDouble = null;
                                if (end != null) {
                                    endDouble = end.AsDouble();
                                }

                                rangeValue = new DoubleRange(startDouble, endDouble);
                            }
                            else {
                                rangeValue = new StringRange(
                                    start == null ? null : start.ToString(),
                                    end == null ? null : end.ToString());
                            }

                            rangeValues[tableRangePropNum] = new RangeIndexLookupValueRange(
                                rangeValue,
                                between.Type,
                                between.IsAllowRangeReversal);
                        }
                        else {
                            var relOp = (QueryGraphValueEntryRangeRelOp) range;
                            var value = relOp.Expression.Evaluate(null, true, agentInstanceContext);
                            if (value != null) {
                                value = MayCoerceNonNull(value, tableRangeProp.CoercionType);
                            }

                            rangeValues[tableRangePropNum] = new RangeIndexLookupValueRange(value, relOp.Type, true);
                        }
                    }
                }
            }

            // perform lookup
            return FafTableLookup(
                virtualDataWindow,
                tablePair.First,
                tablePair.Second.EventTable,
                keyValues,
                rangeValues,
                annotations,
                agentInstanceContext);
        }

        private static object MayCoerceNonNull(
            object value,
            Type coercionType)
        {
            if (value.GetType() == coercionType) {
                return value;
            }

            if (value.IsNumber()) {
                return TypeHelper.CoerceBoxed(value, coercionType);
            }

            return value;
        }

        private static ICollection<EventBean> FafTableLookup(
            VirtualDWView virtualDataWindow,
            IndexMultiKey indexMultiKey,
            EventTable eventTable,
            object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext)
        {
            if (virtualDataWindow != null) {
                return virtualDataWindow.GetFireAndForgetData(eventTable, keyValues, rangeValues, annotations);
            }

            ISet<EventBean> result;
            if (indexMultiKey.HashIndexedProps.Length > 0 && indexMultiKey.RangeIndexedProps.Length == 0) {
                var table = (PropertyHashedEventTable) eventTable;
                var lookupKey = table.MultiKeyTransform.From(keyValues);
                result = table.Lookup(lookupKey);
            }
            else if (indexMultiKey.HashIndexedProps.Length == 0 && indexMultiKey.RangeIndexedProps.Length == 1) {
                var table = (PropertySortedEventTable) eventTable;
                result = table.LookupConstants(rangeValues[0]);
            }
            else {
                var table = (PropertyCompositeEventTable) eventTable;
                var rangeCoercion = table.OptRangeCoercedTypes;
                var lookup = CompositeIndexLookupFactory.Make(keyValues, table.MultiKeyTransform, rangeValues, rangeCoercion);
                result = new HashSet<EventBean>();
                lookup.Lookup(table.Index, result, table.PostProcessor);
            }

            if (result != null) {
                return result;
            }

            return EmptyList<EventBean>.Instance;
        }

        private static NullableObject<ICollection<EventBean>> SnapshotCustomIndex(
            QueryGraphValue queryGraphValue,
            EventTableIndexRepository indexRepository,
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext,
            string objectName)
        {
            EventTable table = null;
            string indexName = null;
            QueryGraphValueEntryCustomOperation values = null;

            // find matching index
            var found = false;
            foreach (var valueDesc in queryGraphValue.Items) {
                if (valueDesc.Entry is QueryGraphValueEntryCustom) {
                    var customIndex = (QueryGraphValueEntryCustom) valueDesc.Entry;

                    foreach (var entry in indexRepository.TableIndexesRefCount) {
                        if (entry.Key.AdvancedIndexDesc == null) {
                            continue;
                        }

                        var metadata = indexRepository.EventTableIndexMetadata.Indexes.Get(entry.Key);
                        if (metadata == null || metadata.ExplicitIndexNameIfExplicit == null) {
                            continue;
                        }

                        var provision = metadata.OptionalQueryPlanIndexItem.AdvancedIndexProvisionDesc;
                        if (provision == null) {
                            continue;
                        }

                        foreach (var op in customIndex.Operations) {
                            if (!provision.Factory.Forge.ProvidesIndexForOperation(op.Key.OperationName)) {
                                continue;
                            }

                            var indexProperties = entry.Key.AdvancedIndexDesc.IndexExpressions;
                            var expressions = op.Key.Expressions;
                            if (Arrays.AreEqual(indexProperties, expressions)) {
                                values = op.Value;
                                table = entry.Value.Table;
                                indexName = metadata.ExplicitIndexNameIfExplicit;
                                found = true;
                                break;
                            }
                        }

                        if (found) {
                            break;
                        }
                    }
                }

                if (found) {
                    break;
                }
            }

            if (table == null) {
                return null;
            }

            // report
            QueryPlanReport(indexName, table, annotations, agentInstanceContext, objectName);

            // execute
            var index = (EventTableQuadTree) table;
            var x = Eval(values.PositionalExpressions.Get(0), agentInstanceContext, "x");
            var y = Eval(values.PositionalExpressions.Get(1), agentInstanceContext, "y");
            var width = Eval(values.PositionalExpressions.Get(2), agentInstanceContext, "width");
            var height = Eval(values.PositionalExpressions.Get(3), agentInstanceContext, "height");
            return new NullableObject<ICollection<EventBean>>(
                index.QueryRange(x, y, width, height));
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        private static double Eval(
            ExprEvaluator eval,
            ExprEvaluatorContext context,
            string name)
        {
            var number = eval.Evaluate(null, true, context);
            if (number == null) {
                throw new EPException("Invalid null value for '" + name + "'");
            }

            return number.AsDouble();
        }

        private static void QueryPlanReportTableScan(
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext,
            string objectName)
        {
            QueryPlanReport(null, null, annotations, agentInstanceContext, objectName);
        }

        private static void QueryPlanReport(
            string indexNameOrNull,
            EventTable eventTableOrNull,
            Attribute[] annotations,
            AgentInstanceContext agentInstanceContext,
            string objectName)
        {
            var hook = QueryPlanIndexHookUtil.GetHook(annotations, agentInstanceContext.ImportServiceRuntime);
            var queryPlanLogging = agentInstanceContext.RuntimeSettingsService.ConfigurationCommon.Logging
                .IsEnableQueryPlan;
            if (queryPlanLogging && (QUERY_PLAN_LOG.IsInfoEnabled || hook != null)) {
                var prefix = "Fire-and-forget or init-time-query from " + objectName + " ";
                var indexText = indexNameOrNull != null ? "index " + indexNameOrNull + " " : "full table scan ";
                indexText += "(snapshot only, for join see separate query plan) ";
                if (eventTableOrNull == null) {
                    QUERY_PLAN_LOG.Info(prefix + indexText);
                }
                else {
                    QUERY_PLAN_LOG.Info(prefix + indexText + eventTableOrNull.ToQueryPlan());
                }

                hook?.FireAndForget(
                    new QueryPlanIndexDescFAF(
                        new[] {
                            new IndexNameAndDescPair(
                                indexNameOrNull,
                                eventTableOrNull != null ? eventTableOrNull.ProviderClass.Name : null)
                        }));
            }
        }
    }
} // end of namespace