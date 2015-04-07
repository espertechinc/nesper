///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.events.vaevent
{
    /// <summary>Strategy for merging updates or additional properties. </summary>
    public interface UpdateStrategy
    {
        /// <summary>Merge properties. </summary>
        /// <param name="isBaseEventType">true if the event is a base event type</param>
        /// <param name="revisionState">the current state, to be updated.</param>
        /// <param name="revisionEvent">the new event to merge</param>
        /// <param name="typesDesc">descriptor for event type of the new event to merge</param>
        void HandleUpdate(bool isBaseEventType,
                          RevisionStateMerge revisionState,
                          RevisionEventBeanMerge revisionEvent,
                          RevisionTypeDesc typesDesc);
    }
}
