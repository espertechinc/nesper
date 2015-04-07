///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Index for filter parameter constants for the range operators (range open/closed/half). 
    /// The implementation is based on the SortedMap implementation of TreeMap and stores only 
    /// expression parameter values of type DoubleRange.
    /// </summary>
    public sealed class FilterParamIndexDoubleRange : FilterParamIndexDoubleRangeBase
    {
        public FilterParamIndexDoubleRange(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock, FilterOperator filterOperator)
            : base(lookupable, readWriteLock, filterOperator)
        {
            if (!(filterOperator.IsRangeOperator()))
            {
                throw new ArgumentException("Invalid filter operator " + filterOperator);
            }
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            var objAttributeValue = Lookupable.Getter.Get(theEvent);
            var returnValue = new Mutable<bool?>(false);

            using (Instrument.With(
                i => i.QFilterReverseIndex(this, objAttributeValue),
                i => i.AFilterReverseIndex(returnValue.Value)))
            {
                if (objAttributeValue == null)
                {
                    return;
                }

                var attributeValue = objAttributeValue.AsDouble();

                var rangeStart = new DoubleRange(attributeValue - LargestRangeValueDouble, attributeValue);
                var rangeEnd = new DoubleRange(attributeValue, Double.MaxValue);

                var subMap = Ranges.Between(rangeStart, true, rangeEnd, true);

                // For not including either endpoint
                // A bit awkward to duplicate the loop code, however better than checking the bool many times over
                // This may be a bit of an early performance optimization - the optimizer after all may do this better
                if (FilterOperator == FilterOperator.RANGE_OPEN) // include neither endpoint
                {
                    foreach (var entry in subMap)
                    {
                        if ((attributeValue > entry.Key.Min) &&
                            (attributeValue < entry.Key.Max))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }
                }
                else if (FilterOperator == FilterOperator.RANGE_CLOSED) // include all endpoints
                {
                    foreach (var entry in subMap)
                    {
                        if ((attributeValue >= entry.Key.Min) &&
                            (attributeValue <= entry.Key.Max))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }
                }
                else if (FilterOperator == FilterOperator.RANGE_HALF_CLOSED) // include high endpoint not low endpoint
                {
                    foreach (var entry in subMap)
                    {
                        if ((attributeValue > entry.Key.Min) &&
                            (attributeValue <= entry.Key.Max))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }
                }
                else if (FilterOperator == FilterOperator.RANGE_HALF_OPEN) // include low endpoint not high endpoint
                {
                    foreach (var entry in subMap)
                    {
                        if ((attributeValue >= entry.Key.Min) &&
                            (attributeValue < entry.Key.Max))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }
                }
                else
                {
                    throw new IllegalStateException("Invalid filter operator " + FilterOperator);
                }

                returnValue.Value = null;
            }
        }

        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
