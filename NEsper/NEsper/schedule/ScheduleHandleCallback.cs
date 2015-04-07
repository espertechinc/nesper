///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service;

namespace com.espertech.esper.schedule
{
    /// <summary>Interface for scheduled callbacks. </summary>
    public interface ScheduleHandleCallback 
    {
        /// <summary>Callback that is invoked as indicated by a schedule added to the scheduling service. </summary>
        /// <param name="extensionServicesContext">is a marker interface for providing custom extension servicespassed to the triggered class </param>
        void ScheduledTrigger(ExtensionServicesContext extensionServicesContext);
    }

    public class ProxyScheduleHandleCallback : ScheduleHandleCallback
    {
        public Action<ExtensionServicesContext> ProcScheduledTrigger { get; set; }

        public ProxyScheduleHandleCallback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyScheduleHandleCallback"/> class.
        /// </summary>
        /// <param name="dg">The dg.</param>
        public ProxyScheduleHandleCallback(Action<ExtensionServicesContext> dg)
        {
            ProcScheduledTrigger = dg;
        }

        /// <summary>
        /// Callback that is invoked as indicated by a schedule added to the scheduling service.
        /// </summary>
        /// <param name="extensionServicesContext">is a marker interface for providing custom extension services
        /// passed to the triggered class</param>
        public void ScheduledTrigger(ExtensionServicesContext extensionServicesContext)
        {
            ProcScheduledTrigger(extensionServicesContext);
        }
    }
}
