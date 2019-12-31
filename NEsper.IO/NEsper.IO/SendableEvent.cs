///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esperio
{
	/// <summary>
	/// A wrapper that packages an event up so that it can be
	/// sent into the caller-specified runtime. It also provides
	/// the scheduling information for this event (send time and
	/// schedule slot), so the user can send this event on schedule.
	/// </summary>
	public interface SendableEvent
	{
        /// <summary> Send the event into the runtime.</summary>
        /// <param name="sender">the sender to send an event</param>
        void Send(AbstractSender sender);

		/// <summary>
		/// Get the send time of this event, relative to all the other events sent or read by the same entity
		/// </summary>
		/// <returns>timestamp</returns>
        long SendTime { get; }

		/// <summary>
		/// Get the schedule slot for the entity that created this event
		/// </summary>
		/// <returns>schedule slot</returns>
        long ScheduleSlot { get; }
	}
}
