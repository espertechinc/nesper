///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    /// Interface for a service that allows to add and remove handles (typically storing callbacks)
    /// for a certain time which are returned when
    /// the evaluate method is invoked and the current time is on or after the handle's registered time.
    /// It is the expectation that the setTime method is called
    /// with same or ascending values for each subsequent call. Handles with are triggered are automatically removed
    /// by implementations.
    /// </summary>
    public interface SchedulingService : TimeProvider,
        IDisposable
    {
        /// <summary>
        /// Add a callback for after the given milliseconds from the current time.
        /// If the same callback (equals) was already added before, the method will not add a new
        /// callback or change the existing callback to a new time, but throw an exception.
        /// </summary>
        /// <param name="afterTime">number of millisec to get a callback</param>
        /// <param name="handle">to add</param>
        /// <param name="slot">allows ordering of concurrent callbacks</param>
        /// <throws>  ScheduleServiceException thrown if the add operation did not complete </throws>
        void Add(
            long afterTime,
            ScheduleHandle handle,
            long slot);

        /// <summary>
        /// Remove a callback.
        /// If the callback to be removed was not found an exception is thrown.
        /// </summary>
        /// <param name="handle">to remove</param>
        /// <param name="scheduleSlot">for which the callback was added</param>
        /// <throws>  ScheduleServiceException thrown if the callback was not located </throws>
        void Remove(
            ScheduleHandle handle,
            long scheduleSlot);

        /// <summary>
        /// Evaluate the current time and perform any callbacks.
        /// </summary>
        /// <param name="handles">The handles.</param>
        void Evaluate(ICollection<ScheduleHandle> handles);

        /// <summary>Returns time handle count.</summary>
        /// <returns>count</returns>
        int TimeHandleCount { get; }

        /// <summary>Returns furthest in the future handle.</summary>
        /// <returns>future handle</returns>
        long? FurthestTimeHandle { get; }

        /// <summary>Returns count of handles.</summary>
        /// <returns>count</returns>
        int ScheduleHandleCount { get; }

        /// <summary>
        /// Returns true if the handle has been scheduled already.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// 	<c>true</c> if the specified handle is scheduled; otherwise, <c>false</c>.
        /// </returns>
        bool IsScheduled(ScheduleHandle handle);
    }
}