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
    /// MapIndex for filter parameter constants to match using the equals (=) operator. The 
    /// implementation is based on a regular HashMap.
    /// </summary>
    public sealed class FilterParamIndexNotEqualsIs : FilterParamIndexNotEqualsBase
    {
        public FilterParamIndexNotEqualsIs(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock, FilterOperator.IS_NOT)
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
                // Look up in hashtable
                using (ConstantsMapRwLock.AcquireReadLock())
                {
                    foreach (var entry in ConstantsMap)
                    {
                        if (entry.Key == null)
                        {
                            if (attributeValue != null)
                            {
                                entry.Value.MatchEvent(theEvent, matches);
                            }
                            continue;
                        }

                        if (!entry.Key.Equals(attributeValue))
                        {
                            entry.Value.MatchEvent(theEvent, matches);
                        }
                    }
                }

                returnValue.Value = null;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
