///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>Interface for scheduled callbacks. </summary>
    public interface ScheduleHandleCallback
    {
        /// <summary>Callback that is invoked as indicated by a schedule added to the scheduling service. </summary>
        void ScheduledTrigger();
    }

    public class ProxyScheduleHandleCallback : ScheduleHandleCallback
    {
        public ProxyScheduleHandleCallback()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyScheduleHandleCallback" /> class.
        /// </summary>
        /// <param name="dg">The dg.</param>
        public ProxyScheduleHandleCallback(Action dg)
        {
            ProcScheduledTrigger = dg;
        }

        public Action ProcScheduledTrigger { get; set; }

        /// <summary>
        ///     Callback that is invoked as indicated by a schedule added to the scheduling service.
        /// </summary>
        public void ScheduledTrigger()
        {
            ProcScheduledTrigger();
        }
    }
}