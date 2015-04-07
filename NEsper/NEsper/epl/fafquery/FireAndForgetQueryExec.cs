///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.exec.composite;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.join.util;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.fafquery
{
    public class FireAndForgetQueryExec
    {
        public static ICollection<EventBean> Snapshot(
            FilterSpecCompiled optionalFilter,
            Attribute[] annotations,
            VirtualDWView virtualDataWindow,
            EventTableIndexRepository indexRepository,
            bool queryPlanLogging,
            ILog queryPlanLogDestination,
            string objectName,
            AgentInstanceContext agentInstanceContext)
        {
            if (optionalFilter == null || optionalFilter.Parameters.Length == 0) {
                if (virtualDataWindow != null) {
                    var pair = virtualDataWindow.GetFireAndForgetDesc(Collections.GetEmptySet<string>(), Collections.GetEmptySet<string>());
                    return virtualDataWindow.GetFireAndForgetData(pair.Second, new object[0], new RangeIndexLookupValue[0], annotations);
                }
                return null;
            }
    
            // Determine what straight-equals keys and which ranges are available.
            // Widening/Coercion is part of filter spec compile.
            ISet<string> keysAvailable = new HashSet<string>();
            ISet<string> rangesAvailable = new HashSet<string>();
            if (optionalFilter.Parameters.Length == 1)
            {
                foreach (FilterSpecParam param in optionalFilter.Parameters[0])
                {
                    if (!(param is FilterSpecParamConstant ||
                          param is FilterSpecParamRange ||
                          param is FilterSpecParamIn))
                    {
                        continue;
                    }

                    if (param.FilterOperator == FilterOperator.EQUAL ||
                        param.FilterOperator == FilterOperator.IS ||
                        param.FilterOperator == FilterOperator.IN_LIST_OF_VALUES)
                    {
                        keysAvailable.Add(param.Lookupable.Expression);
                    }
                    else if (param.FilterOperator.IsRangeOperator() ||
                             param.FilterOperator.IsInvertedRangeOperator() ||
                             param.FilterOperator.IsComparisonOperator())
                    {
                        rangesAvailable.Add(param.Lookupable.Expression);
                    }
                    else if (param.FilterOperator.IsRangeOperator())
                    {
                        rangesAvailable.Add(param.Lookupable.Expression);
                    }
                }
            }

            // Find an index that matches the needs
            Pair<IndexMultiKey, EventTableAndNamePair> tablePair;
            if (virtualDataWindow != null) {
                var tablePairNoName = virtualDataWindow.GetFireAndForgetDesc(keysAvailable, rangesAvailable);
                tablePair = new Pair<IndexMultiKey, EventTableAndNamePair>(tablePairNoName.First, new EventTableAndNamePair(tablePairNoName.Second, null));
            }
            else {
                var indexHint = IndexHint.GetIndexHint(annotations);
                IList<IndexHintInstruction> optionalIndexHintInstructions = null;
                if (indexHint != null) {
                    optionalIndexHintInstructions = indexHint.InstructionsFireAndForget;
                }
                tablePair = indexRepository.FindTable(keysAvailable, rangesAvailable, optionalIndexHintInstructions);
            }
    
            var hook = QueryPlanIndexHookUtil.GetHook(annotations);
            if (queryPlanLogging && (queryPlanLogDestination.IsInfoEnabled || hook != null)) {
                var prefix = "Fire-and-forget from " + objectName + " ";
                var indexName = tablePair != null && tablePair.Second != null ? tablePair.Second.IndexName : null;
                var indexText = indexName != null ? "index " + indexName + " " : "full table scan ";
                indexText += "(snapshot only, for join see separate query plan)";
                if (tablePair == null) {
                    queryPlanLogDestination.Info(prefix + indexText);
                }
                else {
                    queryPlanLogDestination.Info(prefix + indexText + tablePair.Second.EventTable.ToQueryPlan());
                }
    
                if (hook != null) {
                    hook.FireAndForget(new QueryPlanIndexDescFAF(
                            new IndexNameAndDescPair[] {
                                    new IndexNameAndDescPair(indexName, tablePair != null ?
                                            tablePair.Second.EventTable.GetType().Name : null)
                            }));
                }
            }
    
            if (tablePair == null) {
                return null;    // indicates table scan
            }
    
            // Compile key sets which contain key index lookup values
            var keyIndexProps = IndexedPropDesc.GetIndexProperties(tablePair.First.HashIndexedProps);
            var hasKeyWithInClause = false;
            var keyValues = new object[keyIndexProps.Length];
            for (var keyIndex = 0; keyIndex < keyIndexProps.Length; keyIndex++) {
                foreach (var param in optionalFilter.Parameters[0]) {
                    if (param.Lookupable.Expression.Equals(keyIndexProps[keyIndex])) {
                        if (param.FilterOperator == FilterOperator.IN_LIST_OF_VALUES) {
                            var keyValuesList = ((MultiKeyUntyped) param.GetFilterValue(null, agentInstanceContext)).Keys;
                            if (keyValuesList.Length == 0) {
                                continue;
                            }
                            else if (keyValuesList.Length == 1) {
                                keyValues[keyIndex] = keyValuesList[0];
                            }
                            else {
                                keyValues[keyIndex] = keyValuesList;
                                hasKeyWithInClause = true;
                            }
                        }
                        else {
                            keyValues[keyIndex] = param.GetFilterValue(null, agentInstanceContext);
                        }
                        break;
                    }
                }
            }
    
            // Analyze ranges - these may include key lookup value (EQUALS semantics)
            var rangeIndexProps = IndexedPropDesc.GetIndexProperties(tablePair.First.RangeIndexedProps);
            RangeIndexLookupValue[] rangeValues;
            if (rangeIndexProps.Length > 0) {
                rangeValues = CompileRangeLookupValues(rangeIndexProps, optionalFilter.Parameters[0], agentInstanceContext);
            }
            else {
                rangeValues = new RangeIndexLookupValue[0];
            }
    
            var eventTable = tablePair.Second.EventTable;
            var indexMultiKey = tablePair.First;
    
            // table lookup without in-clause
            if (!hasKeyWithInClause) {
                return FafTableLookup(virtualDataWindow, indexMultiKey, eventTable, keyValues, rangeValues, annotations);
            }
    
            // table lookup with in-clause: determine combinations
            var combinations = new object[keyIndexProps.Length][];
            for (var i = 0; i < keyValues.Length; i++) {
                if (keyValues[i] is object[]) {
                    combinations[i] = (object[]) keyValues[i];
                }
                else {
                    combinations[i] = new object[] {keyValues[i]};
                }
            }
    
            // enumerate combinations
            var enumeration = new CombinationEnumeration(combinations);
            var events = new HashSet<EventBean>();
            for (;enumeration.MoveNext();) {
                object[] keys = enumeration.Current;
                var result = FafTableLookup(virtualDataWindow, indexMultiKey, eventTable, keys, rangeValues, annotations);
                events.AddAll(result);
            }
            return events;
        }
    
        private static ICollection<EventBean> FafTableLookup(VirtualDWView virtualDataWindow, IndexMultiKey indexMultiKey, EventTable eventTable, object[] keyValues, RangeIndexLookupValue[] rangeValues, Attribute[] annotations) {
            if (virtualDataWindow != null) {
                return virtualDataWindow.GetFireAndForgetData(eventTable, keyValues, rangeValues, annotations);
            }
    
            ISet<EventBean> result;
            if (indexMultiKey.HashIndexedProps.Length > 0 && indexMultiKey.RangeIndexedProps.Length == 0) {
                if (indexMultiKey.HashIndexedProps.Length == 1) {
                    var table = (PropertyIndexedEventTableSingle) eventTable;
                    result = table.Lookup(keyValues[0]);
                }
                else {
                    var table = (PropertyIndexedEventTable) eventTable;
                    result = table.Lookup(keyValues);
                }
            }
            else if (indexMultiKey.HashIndexedProps.Length == 0 && indexMultiKey.RangeIndexedProps.Length == 1) {
                var table = (PropertySortedEventTable) eventTable;
                result = table.LookupConstants(rangeValues[0]);
            }
            else {
                var table = (PropertyCompositeEventTable) eventTable;
                var rangeCoercion = table.OptRangeCoercedTypes;
                var lookup = CompositeIndexLookupFactory.Make(keyValues, rangeValues, rangeCoercion);
                result = new HashSet<EventBean>();
                lookup.Lookup(table.IndexTable, result);
            }
            if (result != null) {
                return result;
            }
            return Collections.GetEmptyList<EventBean>();
        }
    
        private static RangeIndexLookupValue[] CompileRangeLookupValues(string[] rangeIndexProps, FilterSpecParam[] parameters, AgentInstanceContext agentInstanceContext)
        {
            var result = new RangeIndexLookupValue[rangeIndexProps.Length];
    
            for (var rangeIndex = 0; rangeIndex < rangeIndexProps.Length; rangeIndex++) {
                foreach (var param in parameters) {
                    if (!(param.Lookupable.Expression.Equals(rangeIndexProps[rangeIndex]))) {
                        continue;
                    }
    
                    if (param.FilterOperator == FilterOperator.EQUAL || param.FilterOperator == FilterOperator.IS) {
                        result[rangeIndex] = new RangeIndexLookupValueEquals(param.GetFilterValue(null, agentInstanceContext));
                    }
                    else if (param.FilterOperator.IsRangeOperator() || param.FilterOperator.IsInvertedRangeOperator()) {
                        QueryGraphRangeEnum opAdd = param.FilterOperator.MapFrom();
                        result[rangeIndex] = new RangeIndexLookupValueRange(param.GetFilterValue(null, agentInstanceContext), opAdd, true);
                    }
                    else if (param.FilterOperator.IsComparisonOperator()) {
    
                        var existing = result[rangeIndex];
                        QueryGraphRangeEnum opAdd = param.FilterOperator.MapFrom();
                        if (existing == null) {
                            result[rangeIndex] = new RangeIndexLookupValueRange(param.GetFilterValue(null, agentInstanceContext), opAdd, true);
                        }
                        else {
                            if (!(existing is RangeIndexLookupValueRange)) {
                                continue;
                            }
                            var existingRange = (RangeIndexLookupValueRange) existing;
                            var opExist = existingRange.Operator;
                            var desc = QueryGraphRangeUtil.GetCanConsolidate(opExist, opAdd);
                            if (desc != null) {
                                var doubleRange = GetDoubleRange(desc.IsReverse, existing.Value, param.GetFilterValue(null, agentInstanceContext));
                                result[rangeIndex] = new RangeIndexLookupValueRange(doubleRange, desc.RangeType, false);
                            }
                        }
                    }
                }
            }
            return result;
        }
    
        private static DoubleRange GetDoubleRange(bool reverse, object start, object end) {
            if (start == null || end == null) {
                return null;
            }
            double startDbl = start.AsDouble();
            double endDbl = end.AsDouble();
            if (reverse) {
                return new DoubleRange(startDbl, endDbl);
            }
            else {
                return new DoubleRange(endDbl, startDbl);
            }
        }
    }
}
