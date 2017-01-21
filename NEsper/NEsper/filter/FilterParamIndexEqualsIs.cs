///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// MapIndex for filter parameter constants to match using the equals (=) operator.
    /// The implementation is based on a regular HashMap.
    /// </summary>
    public sealed class FilterParamIndexEqualsIs : FilterParamIndexEqualsBase
    {
        public FilterParamIndexEqualsIs(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock, FilterOperator.IS)
        {
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            var attributeValue = Lookupable.Getter.Get(theEvent);
            var returnValue = new Mutable<bool>(false);

            using (Instrument.With(
                i => i.QFilterReverseIndex(this, attributeValue),
                i => i.AFilterReverseIndex(returnValue.Value)))
            {
                EventEvaluator evaluator = null;
                using (ConstantsMapRwLock.AcquireReadLock())
                {
                    evaluator = ConstantsMap.Get(attributeValue);
                }

                // No listener found for the value, return
                if (evaluator == null)
                {
                    returnValue.Value = false;
                }
                else
                {
                    evaluator.MatchEvent(theEvent, matches);
                    returnValue.Value = true;
                }
            }
        }
    }
}