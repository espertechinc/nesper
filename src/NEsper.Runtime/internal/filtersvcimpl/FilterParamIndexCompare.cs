///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants for the comparison operators (less, greater, etc).
    ///     The implementation is based on the SortedMap implementation of TreeMap.
    ///     The index only accepts numeric constants. It keeps a lower and upper bounds of all constants in the index
    ///     for fast range checking, since the assumption is that frequently values fall within a range.
    /// </summary>
    public class FilterParamIndexCompare : FilterParamIndexLookupableBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FilterParamIndexCompare));

        private readonly IOrderedDictionary<object, EventEvaluator> constantsMap;
        private readonly IReaderWriterLock constantsMapRWLock;

        private double? lowerBounds;
        private double? upperBounds;

        public FilterParamIndexCompare(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock,
            FilterOperator filterOperator)
            : base(filterOperator, lookupable)
        {
            constantsMap = new OrderedListDictionary<object, EventEvaluator>();
            constantsMapRWLock = readWriteLock;

            if (filterOperator != FilterOperator.GREATER &&
                filterOperator != FilterOperator.GREATER_OR_EQUAL &&
                filterOperator != FilterOperator.LESS &&
                filterOperator != FilterOperator.LESS_OR_EQUAL) {
                throw new ArgumentException("Invalid filter operator for index of " + filterOperator);
            }
        }

        public override bool IsEmpty => constantsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock => constantsMapRWLock;

        public override EventEvaluator Get(object filterConstant)
        {
            return constantsMap.Get(filterConstant);
        }

        public override void Put(
            object filterConstant,
            EventEvaluator matcher)
        {
            constantsMap.Put(filterConstant, matcher);

            // Update bounds
            double constant = filterConstant.AsDouble();
            if (lowerBounds == null || constant < lowerBounds) {
                lowerBounds = constant;
            }

            if (upperBounds == null || constant > upperBounds) {
                upperBounds = constant;
            }
        }

        public override void Remove(object filterConstant)
        {
            if (constantsMap.Delete(filterConstant) == null) {
                return;
            }

            UpdateBounds();
        }

        public override int CountExpensive {
            get { return constantsMap.Count; }
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            object propertyValue = Lookupable.Eval.Eval(theEvent, ctx);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, propertyValue);
            }

            if (propertyValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            // A undefine lower bound indicates an empty index
            if (lowerBounds == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            var filterOperator = FilterOperator;
            double propertyValueDouble = propertyValue.AsDouble();

            // Based on current lower and upper bounds check if the property value falls outside - shortcut submap generation
            if (filterOperator == FilterOperator.GREATER && propertyValueDouble <= lowerBounds) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            if (filterOperator == FilterOperator.GREATER_OR_EQUAL && propertyValueDouble < lowerBounds) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            if (filterOperator == FilterOperator.LESS && propertyValueDouble >= upperBounds) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            if (filterOperator == FilterOperator.LESS_OR_EQUAL && propertyValueDouble > upperBounds) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            // Look up in table
            using (constantsMapRWLock.ReadLock.Acquire())
            {
                // Get the head or tail end of the map depending on comparison type
                IDictionary<object, EventEvaluator> subMap;

                if (filterOperator == FilterOperator.GREATER ||
                    filterOperator == FilterOperator.GREATER_OR_EQUAL) {
                    // At the head of the map are those with a lower numeric constants
                    subMap = constantsMap.Head(propertyValue);
                }
                else {
                    subMap = constantsMap.Tail(propertyValue);
                }

                // All entries in the subMap are eligible, with an exception
                EventEvaluator exactEquals = null;
                if (filterOperator == FilterOperator.LESS) {
                    exactEquals = constantsMap.Get(propertyValue);
                }

                foreach (EventEvaluator matcher in subMap.Values) {
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
                    matcher?.MatchEvent(theEvent, matches, ctx);
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

        private void UpdateBounds()
        {
            if (constantsMap.IsEmpty()) {
                lowerBounds = null;
                upperBounds = null;
                return;
            }

            lowerBounds = constantsMap.Keys.First().AsDouble();
            upperBounds = constantsMap.Keys.Last().AsDouble();
        }
    }
} // end of namespace