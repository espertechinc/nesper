///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    /// <summary>
    /// Service provider interface for scheduling service.
    /// </summary>
    public interface SchedulingServiceSPI : SchedulingService
    {
        /// <summary>
        /// Take a statement's schedules out of the currently active set of schedules.
        /// </summary>
        /// <param name="statementId">statements to take out</param>
        /// <returns>schedules</returns>
        ScheduleSet Take(ICollection<int> statementId);

        /// <summary>
        /// Apply the set of schedules.
        /// </summary>
        /// <param name="scheduleSet">to apply</param>
        void Apply(ScheduleSet scheduleSet);

        long? NearestTimeHandle { get; }

        void VisitSchedules(ScheduleVisitor visitor);

        /// <summary>
        /// Initialization is optional and provides a chance to preload things after statements are available.
        /// </summary>
        void Init();
    }
}