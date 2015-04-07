///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
        /// <returns>implementation</returns>
        public static SchedulingServiceSPI NewService(TimeSourceService timeSourceService)
        {
            return new SchedulingServiceImpl(timeSourceService);
        }
	}
}
