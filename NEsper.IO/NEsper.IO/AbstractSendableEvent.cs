///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esperio
{
    /// <summary>
    /// Base class for sendable event, provides timestamp and schedule slot.
    /// </summary>
    public abstract class AbstractSendableEvent : SendableEvent
    {
        /// <summary>Ctor. </summary>
        /// <param name="timestamp">to send</param>
        /// <param name="scheduleSlot">the schedule slot assigned by scheduling service</param>
        protected AbstractSendableEvent(long timestamp, long scheduleSlot)
        {
            if (scheduleSlot == -1)
            {
                throw new ArgumentNullException("scheduleSlot");
            }

            SendTime = timestamp;
            ScheduleSlot = scheduleSlot;
        }

        public abstract void Send(AbstractSender runtime);

        public long ScheduleSlot { get; }

        public long SendTime { get; }
    }
}
