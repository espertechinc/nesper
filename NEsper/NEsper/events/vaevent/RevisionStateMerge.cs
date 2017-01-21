///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>State for merge stratgies. </summary>
    public class RevisionStateMerge
    {
        private EventBean baseEventUnderlying;
        private NullableObject<Object>[] overlays;
        private RevisionEventBeanMerge lastEvent;
    
        /// <summary>Ctor. </summary>
        /// <param name="baseEventUnderlying">base event</param>
        /// <param name="overlays">merged values</param>
        /// <param name="lastEvent">last event</param>
        public RevisionStateMerge(EventBean baseEventUnderlying, NullableObject<Object>[] overlays, RevisionEventBeanMerge lastEvent)
        {
            this.baseEventUnderlying = baseEventUnderlying;
            this.overlays = overlays;
            this.lastEvent = lastEvent;
        }

        /// <summary>Gets or sets base event. </summary>
        /// <returns>base event</returns>
        public EventBean BaseEventUnderlying
        {
            get { return baseEventUnderlying; }
            set { this.baseEventUnderlying = value; }
        }

        /// <summary>Gets or sets the merged values. </summary>
        /// <returns>merged values</returns>
        public NullableObject<Object>[] Overlays
        {
            get { return overlays; }
            set { this.overlays = value; }
        }

        /// <summary>Gets or sets the last event. </summary>
        /// <returns>last event</returns>
        public RevisionEventBeanMerge LastEvent
        {
            get { return lastEvent; }
            set { this.lastEvent = value; }
        }
    }
}
