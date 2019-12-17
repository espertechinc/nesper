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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants for the not range operators (range open/closed/half).
    ///     The implementation is based on the SortedMap implementation of TreeMap and stores only expression
    ///     parameter values of type StringRange.
    /// </summary>
    public class FilterParamIndexStringRangeInverted : FilterParamIndexStringRangeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FilterParamIndexStringRangeInverted));

        public FilterParamIndexStringRangeInverted(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock,
            FilterOperator filterOperator)
            : base(lookupable, readWriteLock, filterOperator)
        {
            if (!filterOperator.IsInvertedRangeOperator()) {
                throw new ArgumentException("Invalid filter operator " + filterOperator);
            }
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            object objAttributeValue = Lookupable.Getter.Get(theEvent);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, objAttributeValue);
            }

            if (objAttributeValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            var attributeValue = (string) objAttributeValue;

            if (FilterOperator == FilterOperator.NOT_RANGE_CLOSED) {
                // include all endpoints
                foreach (var entry in Ranges) {
                    if (string.Compare(entry.Key.Min, attributeValue, StringComparison.Ordinal) > 0 || 
                        string.Compare(entry.Key.Max, attributeValue, StringComparison.Ordinal) < 0) {
                        entry.Value.MatchEvent(theEvent, matches);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_OPEN) { // include neither endpoint
                foreach (var entry in Ranges) {
                    if (string.Compare(entry.Key.Min, attributeValue, StringComparison.Ordinal) >= 0 || 
                        string.Compare(entry.Key.Max, attributeValue, StringComparison.Ordinal) <= 0) {
                        entry.Value.MatchEvent(theEvent, matches);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_HALF_CLOSED) {
                // include high endpoint not low endpoint
                foreach (var entry in Ranges) {
                    if (string.Compare(entry.Key.Min, attributeValue, StringComparison.Ordinal) >= 0 || 
                        string.Compare(entry.Key.Max, attributeValue, StringComparison.Ordinal) < 0) {
                        entry.Value.MatchEvent(theEvent, matches);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_HALF_OPEN) {
                // include low endpoint not high endpoint
                foreach (var entry in Ranges) {
                    if (string.Compare(entry.Key.Min, attributeValue, StringComparison.Ordinal) > 0 || 
                        string.Compare(entry.Key.Max, attributeValue, StringComparison.Ordinal) <= 0) {
                        entry.Value.MatchEvent(theEvent, matches);
                    }
                }
            }
            else {
                throw new IllegalStateException("Invalid filter operator " + FilterOperator);
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(null);
            }
        }
    }
} // end of namespace