///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     Interface for random access to a previous event.
    /// </summary>
    public interface RowRecogStateRandomAccess
    {
        /// <summary>
        ///     Returns an new data event given an index.
        /// </summary>
        /// <param name="index">to return new data for</param>
        /// <returns>new data event</returns>
        EventBean GetPreviousEvent(int index);

        void NewEventPrepare(EventBean newEvent);

        void ExistingEventPrepare(EventBean theEvent);

        void Remove(EventBean[] oldEvents);

        void Remove(EventBean oldEvent);

        bool IsEmpty();
    }
} // end of namespace