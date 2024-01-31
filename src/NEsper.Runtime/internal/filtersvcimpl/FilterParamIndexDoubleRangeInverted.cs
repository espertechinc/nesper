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
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants for the not range operators (range open/closed/half).
    ///     The implementation is based on the SortedMap implementation of TreeMap and stores only expression
    ///     parameter values of type DoubleRange.
    /// </summary>
    public class FilterParamIndexDoubleRangeInverted : FilterParamIndexDoubleRangeBase
    {
        public FilterParamIndexDoubleRangeInverted(
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
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            var objAttributeValue = Lookupable.Eval.Eval(theEvent, ctx);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, objAttributeValue);
            }

            if (objAttributeValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            double attributeValue = objAttributeValue.AsDouble();

            if (FilterOperator == FilterOperator.NOT_RANGE_CLOSED) { // include all endpoints
                foreach (var entry in Ranges) {
                    if (attributeValue < entry.Key.Min ||
                        attributeValue > entry.Key.Max) {
                        entry.Value.MatchEvent(theEvent, matches, ctx);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_OPEN) { // include neither endpoint
                foreach (var entry in Ranges) {
                    if (attributeValue <= entry.Key.Min ||
                        attributeValue >= entry.Key.Max) {
                        entry.Value.MatchEvent(theEvent, matches, ctx);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_HALF_CLOSED) { // include high endpoint not low endpoint
                foreach (var entry in Ranges) {
                    if (attributeValue <= entry.Key.Min ||
                        attributeValue > entry.Key.Max) {
                        entry.Value.MatchEvent(theEvent, matches, ctx);
                    }
                }
            }
            else if (FilterOperator == FilterOperator.NOT_RANGE_HALF_OPEN) { // include low endpoint not high endpoint
                foreach (var entry in Ranges) {
                    if (attributeValue < entry.Key.Min ||
                        attributeValue >= entry.Key.Max) {
                        entry.Value.MatchEvent(theEvent, matches, ctx);
                    }
                }
            }
            else {
                throw new IllegalStateException("Invalid filter operator " + FilterOperator);
            }

            RangesNullEndpoints?.MatchEvent(theEvent, matches, ctx);

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(null);
            }
        }
    }
} // end of namespace