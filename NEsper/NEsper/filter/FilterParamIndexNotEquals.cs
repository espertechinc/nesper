///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Index for filter parameter constants to match using the equals (=) operator.
    /// The implementation is based on a regular dictionary.
    /// </summary>
    public sealed class FilterParamIndexNotEquals : FilterParamIndexNotEqualsBase
    {
        public FilterParamIndexNotEquals(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock, FilterOperator.NOT_EQUAL)
        {
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            var attributeValue = Lookupable.Getter.Get(theEvent);
            var returnValue = new Mutable<bool?>(false);

            using (Instrument.With(
                i => i.QFilterReverseIndex(this, attributeValue),
                i => i.AFilterReverseIndex(returnValue.Value)))
            {
                if (attributeValue == null)
                {
                    // null cannot match any other value, not even null (use "is" or "is not", i.e. null != null returns null)
                    return;
                }

                // Look up in hashtable
                using (ConstantsMapRwLock.ReadLock.Acquire())
                {
                    foreach (var entry in ConstantsMap)
                    {
                        if (entry.Key == null)
                        {
                            continue;
                            // null-value cannot match, not even null (use "is" or "is not", i.e. null != null returns null)
                        }

                        if (!entry.Key.Equals(attributeValue))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }

                    returnValue.Value = null;
                }
            }
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
