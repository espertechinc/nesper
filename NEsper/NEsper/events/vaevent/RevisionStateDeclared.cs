///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// State for the overlay (non-merge) strategy.
    /// </summary>
    public class RevisionStateDeclared
    {
        private long revisionNumber;
        private EventBean baseEventUnderlying;
        private RevisionBeanHolder[] holders;
        private RevisionEventBeanDeclared lastEvent;

        /// <summary>Ctor. </summary>
        /// <param name="baseEventUnderlying">base event</param>
        /// <param name="holders">revisions</param>
        /// <param name="lastEvent">prior event</param>
        public RevisionStateDeclared(EventBean baseEventUnderlying,
                                     RevisionBeanHolder[] holders,
                                     RevisionEventBeanDeclared lastEvent)
        {
            this.baseEventUnderlying = baseEventUnderlying;
            this.holders = holders;
            this.lastEvent = lastEvent;
        }

        /// <summary>Returns revision number. </summary>
        /// <returns>version number</returns>
        public long RevisionNumber
        {
            get { return revisionNumber; }
        }

        /// <summary>Increments version number. </summary>
        /// <returns>incremented version number</returns>
        public long IncRevisionNumber()
        {
            return ++revisionNumber;
        }

        /// <summary>Gets or sets base event. </summary>
        /// <returns>base event</returns>
        public EventBean BaseEventUnderlying
        {
            get { return baseEventUnderlying; }
            set { this.baseEventUnderlying = value; }
        }

        /// <summary>Gets or sets versions. </summary>
        /// <returns>versions</returns>
        public RevisionBeanHolder[] Holders
        {
            get { return holders; }
            set { this.holders = value; }
        }

        /// <summary>Gets or sets the last event. </summary>
        /// <returns>last event</returns>
        public RevisionEventBeanDeclared LastEvent
        {
            get { return lastEvent; }
            set { this.lastEvent = value; }
        }
    }
}
