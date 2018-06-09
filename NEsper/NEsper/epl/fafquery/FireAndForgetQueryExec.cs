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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.index.quadtree;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.exec.composite;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.fafquery
{
    /// <summary>
    /// Defines the <see cref="FireAndForgetQueryExec" />
    /// </summary>
    public class FireAndForgetQueryExec
    {
        /// <summary>
        /// The snapshot
        /// </summary>
        /// <param name="queryGraph">The <see cref="QueryGraph"/></param>
        /// <param name="attributes">The <see cref="Attribute" /> array</param>
        /// <param name="virtualDataWindow">The <see cref="VirtualDWView"/></param>
        /// <param name="indexRepository">The <see cref="EventTableIndexRepository"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <returns>The <see cref="ICollection{EventBean}"/></returns>
        public static ICollection<EventBean> Snapshot(
            QueryGraph queryGraph,
            Attribute[] attributes,
            VirtualDWView virtualDataWindow,
            EventTableIndexRepository indexRepository,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName,
            AgentInstanceContext agentInstanceContext)
        {
            var queryGraphValue = queryGraph == null ? null : queryGraph.GetGraphValue(QueryGraph.SELF_STREAM, 0);
            if (queryGraphValue == null || queryGraphValue.Items.IsEmpty())
            {
                if (virtualDataWindow != null)
                {
                    var pair = virtualDataWindow.GetFireAndForgetDesc(Collections.GetEmptySet<string>(), Collections.GetEmptySet<string>());
                    return virtualDataWindow.GetFireAndForgetData(pair.Second, new Object[0], new RangeIndexLookupValue[0], attributes);
                }
                return null;
            }

            // determine custom index
            var customResult = SnapshotCustomIndex(
                queryGraphValue, indexRepository, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);
            if (customResult != null)
            {
                return customResult.Value;
            }

            // determine lookup based on hash-keys and ranges
            var keysAvailable = queryGraphValue.HashKeyProps;
            var keyNamesAvailable = keysAvailable.Indexed.Count == 0
                ? Collections.GetEmptySet<string>()
                : new HashSet<string>(keysAvailable.Indexed);
            var rangesAvailable = queryGraphValue.RangeProps;
            var rangeNamesAvailable = rangesAvailable.Indexed.Count == 0
                ? Collections.GetEmptySet<string>()
                : new HashSet<string>(rangesAvailable.Indexed);

            // find index that matches the needs
            var tablePair = FindIndex(keyNamesAvailable, rangeNamesAvailable, indexRepository, virtualDataWindow, attributes);

            // regular index lookup
            if (tablePair != null)
            {
                return SnapshotIndex(keysAvailable, rangesAvailable, tablePair, virtualDataWindow, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);
            }

            // in-keyword lookup
            var inkwResult = SnapshotInKeyword(queryGraphValue, indexRepository, virtualDataWindow, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);
            if (inkwResult != null)
            {
                return inkwResult.Value;
            }

            QueryPlanReportTableScan(attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);
            return null;
        }

        /// <summary>
        /// The FindIndex
        /// </summary>
        /// <param name="keyNamesAvailable">The key names available.</param>
        /// <param name="rangeNamesAvailable">The range names available.</param>
        /// <param name="indexRepository">The <see cref="EventTableIndexRepository" /></param>
        /// <param name="virtualDataWindow">The <see cref="VirtualDWView" /></param>
        /// <param name="attributes">The <see cref="Attribute" /> array</param>
        /// <returns></returns>
        private static Pair<IndexMultiKey, EventTableAndNamePair> FindIndex(
            ISet<string> keyNamesAvailable,
            ISet<string> rangeNamesAvailable,
            EventTableIndexRepository indexRepository,
            VirtualDWView virtualDataWindow,
            Attribute[] attributes)
        {
            if (virtualDataWindow != null)
            {
                var tablePairNoName = virtualDataWindow.GetFireAndForgetDesc(keyNamesAvailable, rangeNamesAvailable);
                return new Pair<IndexMultiKey, EventTableAndNamePair>(tablePairNoName.First, new EventTableAndNamePair(tablePairNoName.Second, null));
            }
            var indexHint = IndexHint.GetIndexHint(attributes);
            var optionalIndexHintInstructions = indexHint != null ? indexHint.InstructionsFireAndForget : null;
            return indexRepository.FindTable(keyNamesAvailable, rangeNamesAvailable, optionalIndexHintInstructions);
        }

        /// <summary>
        /// The SnapshotInKeyword
        /// </summary>
        /// <param name="queryGraphValue">The <see cref="QueryGraphValue"/></param>
        /// <param name="indexRepository">The <see cref="EventTableIndexRepository"/></param>
        /// <param name="virtualDataWindow">The <see cref="VirtualDWView"/></param>
        /// <param name="attributes">The <see cref="Attribute" /> array</param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        /// <returns>The collection of event beans</returns>
        private static NullableObject<ICollection<EventBean>> SnapshotInKeyword(
            QueryGraphValue queryGraphValue,
            EventTableIndexRepository indexRepository,
            VirtualDWView virtualDataWindow,
            Attribute[] attributes,
            AgentInstanceContext agentInstanceContext,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName)
        {
            var inkwSingles = queryGraphValue.InKeywordSingles;
            if (inkwSingles.Indexed.Length == 0)
            {
                return null;
            }

            var tablePair = FindIndex(
                new HashSet<string>(inkwSingles.Indexed),
                Collections.GetEmptySet<string>(),
                indexRepository,
                virtualDataWindow,
                attributes);
            if (tablePair == null)
            {
                return null;
            }

            queryPlanReport(tablePair.Second.IndexName, tablePair.Second.EventTable, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);

            var evaluateParamsTrue = new EvaluateParams(null, true, agentInstanceContext);

            // table lookup with in-clause: determine combinations
            var tableHashProps = tablePair.First.HashIndexedProps;
            var combinations = new Object[tableHashProps.Length][];
            for (var tableHashPropNum = 0; tableHashPropNum < tableHashProps.Length; tableHashPropNum++)
            {
                for (var i = 0; i < inkwSingles.Indexed.Length; i++)
                {
                    if (inkwSingles.Indexed[i].Equals(tableHashProps[tableHashPropNum].IndexPropName))
                    {
                        var keysExpressions = inkwSingles.Key[i];
                        var values = new Object[keysExpressions.KeyExprs.Count];
                        combinations[tableHashPropNum] = values;
                        for (var j = 0; j < keysExpressions.KeyExprs.Count; j++)
                        {
                            values[j] = keysExpressions.KeyExprs[j].ExprEvaluator.Evaluate(evaluateParamsTrue);
                        }
                    }
                }
            }

            // enumerate combinations
            var enumeration = CombinationEnumeration.New(combinations);
            var events = new HashSet<EventBean>();

            foreach (Object[] keys in enumeration)
            {
                var result = FafTableLookup(virtualDataWindow, tablePair.First, tablePair.Second.EventTable, keys, null, attributes);
                events.AddAll(result);
            }

            return new NullableObject<ICollection<EventBean>>(events);
        }

        /// <summary>
        /// The SnapshotIndex
        /// </summary>
        /// <param name="keysAvailable">The <see cref="QueryGraphValuePairHashKeyIndex"/></param>
        /// <param name="rangesAvailable">The <see cref="QueryGraphValuePairRangeIndex"/></param>
        /// <param name="tablePair">The <see cref="Pair{IndexMultiKey, EventTableAndNamePair}"/></param>
        /// <param name="virtualDataWindow">The <see cref="VirtualDWView"/></param>
        /// <param name="attributes">The <see cref="Attribute"/> array</param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        /// <returns>The <see cref="ICollection{EventBean}"/></returns>
        private static ICollection<EventBean> SnapshotIndex(
            QueryGraphValuePairHashKeyIndex keysAvailable,
            QueryGraphValuePairRangeIndex rangesAvailable,
            Pair<IndexMultiKey, EventTableAndNamePair> tablePair,
            VirtualDWView virtualDataWindow,
            Attribute[] attributes,
            AgentInstanceContext agentInstanceContext,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName)
        {
            var evaluateParamsTrue = new EvaluateParams(null, true, agentInstanceContext);

            // report plan
            queryPlanReport(tablePair.Second.IndexName, tablePair.Second.EventTable, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);

            // compile hash lookup values
            var tableHashProps = tablePair.First.HashIndexedProps;
            var keyValues = new Object[tableHashProps.Length];
            for (var tableHashPropNum = 0; tableHashPropNum < tableHashProps.Length; tableHashPropNum++)
            {
                var tableHashProp = tableHashProps[tableHashPropNum];
                for (var i = 0; i < keysAvailable.Indexed.Count; i++)
                {
                    if (keysAvailable.Indexed[i].Equals(tableHashProp.IndexPropName))
                    {
                        var key = keysAvailable.Keys[i];
                        var value = key.KeyExpr.ExprEvaluator.Evaluate(evaluateParamsTrue);
                        if (value != null)
                        {
                            value = MayCoerceNonNull(value, tableHashProp.CoercionType);
                            keyValues[tableHashPropNum] = value;
                        }
                    }
                }
            }

            // compile range lookup values
            var tableRangeProps = tablePair.First.RangeIndexedProps;
            var rangeValues = new RangeIndexLookupValue[tableRangeProps.Length];
            for (var tableRangePropNum = 0; tableRangePropNum < tableRangeProps.Length; tableRangePropNum++)
            {
                var tableRangeProp = tableRangeProps[tableRangePropNum];
                for (var i = 0; i < rangesAvailable.Indexed.Count; i++)
                {
                    if (rangesAvailable.Indexed[i].Equals(tableRangeProp.IndexPropName))
                    {
                        var range = rangesAvailable.Keys[i];
                        if (range is QueryGraphValueEntryRangeIn)
                        {
                            var between = (QueryGraphValueEntryRangeIn)range;
                            var start = between.ExprStart.ExprEvaluator.Evaluate(evaluateParamsTrue);
                            var end = between.ExprEnd.ExprEvaluator.Evaluate(evaluateParamsTrue);
                            Range rangeValue;
                            if (tableRangeProp.CoercionType.IsNumeric())
                            {
                                double? startDouble = null;
                                if (start != null)
                                {
                                    startDouble = start.AsDouble();
                                }
                                double? endDouble = null;
                                if (end != null)
                                {
                                    endDouble = end.AsDouble();
                                }
                                rangeValue = new DoubleRange(startDouble, endDouble);
                            }
                            else
                            {
                                rangeValue = new StringRange(start == null ? null : start.ToString(), end == null ? null : end.ToString());
                            }

                            rangeValues[tableRangePropNum] = new RangeIndexLookupValueRange(rangeValue, between.RangeType, between.IsAllowRangeReversal);
                        }
                        else
                        {
                            var relOp = (QueryGraphValueEntryRangeRelOp)range;
                            var value = relOp.Expression.ExprEvaluator.Evaluate(evaluateParamsTrue);
                            if (value != null)
                            {
                                value = MayCoerceNonNull(value, tableRangeProp.CoercionType);
                            }
                            rangeValues[tableRangePropNum] = new RangeIndexLookupValueRange(value, relOp.RangeType, true);
                        }
                    }
                }
            }

            // perform lookup
            return FafTableLookup(virtualDataWindow, tablePair.First, tablePair.Second.EventTable, keyValues, rangeValues, attributes);
        }

        /// <summary>
        /// The MayCoerceNonNull
        /// </summary>
        /// <param name="value">The <see cref="Object"/></param>
        /// <param name="coercionType">The <see cref="Type"/></param>
        /// <returns>The <see cref="Object"/></returns>
        private static Object MayCoerceNonNull(Object value, Type coercionType)
        {
            if (value.GetType() == coercionType)
            {
                return value;
            }
            if (TypeHelper.IsNumber(value))
            {
                return CoercerFactory.CoerceBoxed(value, coercionType);
            }
            return value;
        }

        /// <summary>
        /// The SnapshotCustomIndex
        /// </summary>
        /// <param name="queryGraphValue">The <see cref="QueryGraphValue"/></param>
        /// <param name="indexRepository">The <see cref="EventTableIndexRepository"/></param>
        /// <param name="attributes">The <see cref="Attribute"/> array</param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        /// <returns>The <see cref="ICollection{EventBean}"/></returns>
        private static NullableObject<ICollection<EventBean>> SnapshotCustomIndex(
            QueryGraphValue queryGraphValue,
            EventTableIndexRepository indexRepository,
            Attribute[] attributes,
            AgentInstanceContext agentInstanceContext,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName)
        {
            EventTable table = null;
            string indexName = null;
            QueryGraphValueEntryCustomOperation values = null;

            // find matching index
            var found = false;
            foreach (var valueDesc in queryGraphValue.Items)
            {
                if (valueDesc.Entry is QueryGraphValueEntryCustom)
                {
                    var customIndex = (QueryGraphValueEntryCustom)valueDesc.Entry;

                    foreach (var entry in indexRepository.TableIndexesRefCount)
                    {
                        if (entry.Key.AdvancedIndexDesc == null)
                        {
                            continue;
                        }
                        var metadata = indexRepository.EventTableIndexMetadata.Indexes.Get(entry.Key);
                        if (metadata == null || metadata.ExplicitIndexNameIfExplicit == null)
                        {
                            continue;
                        }
                        EventAdvancedIndexProvisionDesc provision = metadata.QueryPlanIndexItem.AdvancedIndexProvisionDesc;
                        if (provision == null)
                        {
                            continue;
                        }
                        foreach (var op in customIndex.Operations)
                        {
                            if (!provision.Factory.ProvidesIndexForOperation(op.Key.OperationName, op.Value.PositionalExpressions))
                            {
                                continue;
                            }
                            if (ExprNodeUtility.DeepEquals(entry.Key.AdvancedIndexDesc.IndexedExpressions, op.Key.ExprNodes, true))
                            {
                                values = op.Value;
                                table = entry.Value.Table;
                                indexName = metadata.ExplicitIndexNameIfExplicit;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (table == null)
            {
                return null;
            }

            // report
            queryPlanReport(indexName, table, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);

            // execute
            EventTableQuadTree index = (EventTableQuadTree)table;
            var x = Eval(values.PositionalExpressions.Get(0).ExprEvaluator, agentInstanceContext, "x");
            var y = Eval(values.PositionalExpressions.Get(1).ExprEvaluator, agentInstanceContext, "y");
            var width = Eval(values.PositionalExpressions.Get(2).ExprEvaluator, agentInstanceContext, "width");
            var height = Eval(values.PositionalExpressions.Get(3).ExprEvaluator, agentInstanceContext, "height");
            var result = index.QueryRange(x, y, width, height);
            return new NullableObject<ICollection<EventBean>>(result);
        }

        /// <summary>
        /// The ToQueryPlan
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public string ToQueryPlan()
        {
            return this.GetType().Name;
        }

        /// <summary>
        /// The FafTableLookup
        /// </summary>
        /// <param name="virtualDataWindow">The <see cref="VirtualDWView"/></param>
        /// <param name="indexMultiKey">The <see cref="IndexMultiKey"/></param>
        /// <param name="eventTable">The <see cref="EventTable"/></param>
        /// <param name="keyValues">The <see cref="Object"/> array</param>
        /// <param name="rangeValues">The <see cref="RangeIndexLookupValue" /> array</param>
        /// <param name="attributes">The <see cref="Attribute" /> array</param>
        /// <returns>The <see cref="ICollection{EventBean}"/></returns>
        private static ICollection<EventBean> FafTableLookup(
            VirtualDWView virtualDataWindow,
            IndexMultiKey indexMultiKey,
            EventTable eventTable,
            Object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] attributes)
        {
            if (virtualDataWindow != null)
            {
                return virtualDataWindow.GetFireAndForgetData(eventTable, keyValues, rangeValues, attributes);
            }

            ISet<EventBean> result;
            if (indexMultiKey.HashIndexedProps.Length > 0 && indexMultiKey.RangeIndexedProps.Length == 0)
            {
                if (indexMultiKey.HashIndexedProps.Length == 1)
                {
                    var table = (PropertyIndexedEventTableSingle)eventTable;
                    result = table.Lookup(keyValues[0]);
                }
                else
                {
                    var table = (PropertyIndexedEventTable)eventTable;
                    result = table.Lookup(keyValues);
                }
            }
            else if (indexMultiKey.HashIndexedProps.Length == 0 && indexMultiKey.RangeIndexedProps.Length == 1)
            {
                var table = (PropertySortedEventTable)eventTable;
                result = table.LookupConstants(rangeValues[0]);
            }
            else
            {
                var table = (PropertyCompositeEventTable)eventTable;
                var rangeCoercion = table.OptRangeCoercedTypes;
                var lookup = CompositeIndexLookupFactory.Make(keyValues, rangeValues, rangeCoercion);
                result = new HashSet<EventBean>();
                lookup.Lookup(table.MapIndex, result, table.PostProcessor);
            }
            if (result != null)
            {
                return result;
            }

            return Collections.GetEmptyList<EventBean>();
        }

        /// <summary>
        /// The Eval
        /// </summary>
        /// <param name="eval">The <see cref="ExprEvaluator"/></param>
        /// <param name="context">The <see cref="ExprEvaluatorContext"/></param>
        /// <param name="name">The <see cref="string"/></param>
        /// <returns>The <see cref="double"/></returns>
        private static double Eval(ExprEvaluator eval, ExprEvaluatorContext context, string name)
        {
            var number = eval.Evaluate(new EvaluateParams(null, true, context));
            if (number == null)
            {
                throw new EPException("Invalid null value for '" + name + "'");
            }
            return number.AsDouble();
        }

        /// <summary>
        /// The QueryPlanReportTableScan
        /// </summary>
        /// <param name="attributes">The <see cref="Attribute"/> array</param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        private static void QueryPlanReportTableScan(
            Attribute[] attributes,
            AgentInstanceContext agentInstanceContext,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName)
        {
            queryPlanReport(null, null, attributes, agentInstanceContext, queryPlanLogging, queryPlanLogDestination, objectName);
        }

        /// <summary>
        /// The queryPlanReport
        /// </summary>
        /// <param name="indexNameOrNull">The <see cref="string"/></param>
        /// <param name="eventTableOrNull">The <see cref="EventTable"/></param>
        /// <param name="attributes">The <see cref="Attribute"/> array</param>
        /// <param name="agentInstanceContext">The <see cref="AgentInstanceContext"/></param>
        /// <param name="queryPlanLogging">The <see cref="bool"/></param>
        /// <param name="queryPlanLogDestination">The <see cref="ILog"/></param>
        /// <param name="objectName">The <see cref="string"/></param>
        private static void queryPlanReport(
            string indexNameOrNull,
            EventTable eventTableOrNull,
            Attribute[] attributes,
            AgentInstanceContext agentInstanceContext,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName)
        {
            var hook = QueryPlanIndexHookUtil.GetHook(attributes, agentInstanceContext.StatementContext.EngineImportService);
            if (queryPlanLogging && (queryPlanLogDestination.IsInfoEnabled || hook != null))
            {
                var prefix = "Fire-and-forget from " + objectName + " ";
                var indexText = indexNameOrNull != null ? "index " + indexNameOrNull + " " : "full table scan ";
                indexText += "(snapshot only, for join see separate query plan) ";
                if (eventTableOrNull == null)
                {
                    queryPlanLogDestination.Info(prefix + indexText);
                }
                else
                {
                    queryPlanLogDestination.Info(prefix + indexText + eventTableOrNull.ToQueryPlan());
                }

                if (hook != null)
                {
                    hook.FireAndForget(new QueryPlanIndexDescFAF(
                        new IndexNameAndDescPair[]{
                                new IndexNameAndDescPair(indexNameOrNull, eventTableOrNull != null ? eventTableOrNull.ProviderClass.Name : null)
                        }));
                }
            }
        }
    }
}
