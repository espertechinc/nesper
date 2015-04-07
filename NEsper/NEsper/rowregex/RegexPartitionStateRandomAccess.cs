///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Interface for random access to a previous event.
    /// </summary>
    public interface RegexPartitionStateRandomAccess
    {
        /// <summary>
        /// Returns an new data event given an index.
        /// </summary>
        /// <param name="index">to return new data for</param>
        /// <returns>
        /// new data event
        /// </returns>
        EventBean GetPreviousEvent(int index);
    }
}
