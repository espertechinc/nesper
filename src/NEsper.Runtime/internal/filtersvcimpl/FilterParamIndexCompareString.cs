///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    /// Index for filter parameter constants for the comparison operators (less, greater, etc).
    /// The implementation is based on the SortedMap implementation of TreeMap.
    /// The index only accepts String constants. It keeps a lower and upper bounds of all constants in the index
    /// for fast range checking, since the assumption is that frequently values fall within a range.
    /// </summary>
    public class FilterParamIndexCompareString : FilterParamIndexLookupableBase
    {
        private readonly IOrderedDictionary<object, EventEvaluator> constantsMap;
        private readonly IReaderWriterLock constantsMapRWLock;

        public FilterParamIndexCompareString(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock,
            FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {

            constantsMap = new OrderedListDictionary<object, EventEvaluator>();
            constantsMapRWLock = readWriteLock;

            if ((filterOperator != FilterOperator.GREATER) &&
                (filterOperator != FilterOperator.GREATER_OR_EQUAL) &&
                (filterOperator != FilterOperator.LESS) &&
                (filterOperator != FilterOperator.LESS_OR_EQUAL)) {
                throw new ArgumentException("Invalid filter operator for index of " + filterOperator);
            }
        }

        public override EventEvaluator Get(object filterConstant)
        {
            return constantsMap.Get(filterConstant);
        }

        public override void Put(
            object filterConstant,
            EventEvaluator matcher)
        {
            constantsMap.Put(filterConstant, matcher);
        }

        public override void Remove(object filterConstant)
        {
            constantsMap.Remove(filterConstant);
        }

        public override int CountExpensive => constantsMap.Count;

        public override bool IsEmpty => constantsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock => constantsMapRWLock;

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            var propertyValue = Lookupable.Eval.Eval(theEvent, ctx);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, propertyValue);
            }

            if (propertyValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            var filterOperator = this.FilterOperator;

            // Look up in table
            using (constantsMapRWLock.ReadLock.Acquire())
            {
                // Get the head or tail end of the map depending on comparison type
                IDictionary<object, EventEvaluator> subMap;

                if ((filterOperator == FilterOperator.GREATER) ||
                    (filterOperator == FilterOperator.GREATER_OR_EQUAL)) {
                    // At the head of the map are those with a lower numeric constants
                    subMap = constantsMap.Head(propertyValue);
                }
                else {
                    subMap = constantsMap.Tail(propertyValue);
                }

                // All entries in the subMap are elgibile, with an exception
                EventEvaluator exactEquals = null;
                if (filterOperator == FilterOperator.LESS) {
                    exactEquals = constantsMap.Get(propertyValue);
                }

                foreach (var matcher in subMap.Values) {
                    // For the LESS comparison type we ignore the exactly equal case
                    // The subMap is sorted ascending, thus the exactly equals case is the first
                    if (exactEquals != null) {
                        exactEquals = null;
                        continue;
                    }

                    matcher.MatchEvent(theEvent, matches, ctx);
                }

                if (filterOperator == FilterOperator.GREATER_OR_EQUAL) {
                    var matcher = constantsMap.Get(propertyValue);
                    if (matcher != null) {
                        matcher.MatchEvent(theEvent, matches, ctx);
                    }
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(null);
            }
        }

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (var entry in constantsMap) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Key, this));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
} // end of namespace