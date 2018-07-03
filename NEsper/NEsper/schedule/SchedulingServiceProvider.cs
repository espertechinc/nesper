///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.threading;
using com.espertech.esper.timer;

namespace com.espertech.esper.schedule
{
	/// <summary>
    /// Static factory for implementations of the SchedulingService interface.
    /// </summary>

    public sealed class SchedulingServiceProvider
	{
        /// <summary>
        /// Creates an implementation of the SchedulingService interface.
        /// </summary>
        /// <param name="timeSourceService">time source provider</param>
        /// <param name="lockManager">The lock manager.</param>
        /// <returns>
        /// implementation
        /// </returns>
        public static SchedulingServiceSPI NewService(
            TimeSourceService timeSourceService,
            ILockManager lockManager)
        {
            return new SchedulingServiceImpl(timeSourceService, lockManager);
        }
	}
}
