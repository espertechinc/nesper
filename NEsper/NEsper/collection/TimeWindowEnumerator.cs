///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.collection
{
	/// <summary>
    /// GetEnumerator for <see cref="TimeWindow"/> to iterate over a timestamp slots that hold events.
    /// </summary>

    public sealed class TimeWindowEnumerator : MixedEventBeanAndCollectionEnumeratorBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="window">is the time-slotted collection</param>

        public TimeWindowEnumerator(IEnumerable<TimeWindowPair> window)
            : base(window)
        {
        }

	    protected override Object GetValue(Object keyValue)
        {
            return ((TimeWindowPair)keyValue).EventHolder;
        }
    }
}
