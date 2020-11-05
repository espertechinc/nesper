///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.concurrency;

namespace com.espertech.esperio.ext
{
    /// <summary>
    /// Implementation of Sender to send to esper engine using threads
    /// </summary>
    public class ThreadedSender : DirectSender
    {
        private readonly IExecutorService _executorService;
        private readonly string _eventTypeName;

        /// <summary>Ctor. </summary>
        /// <param name="threadPoolSize">size of pool</param>
        public ThreadedSender(int threadPoolSize)
        {
            _executorService = new DedicatedExecutorService("threaded-sender", threadPoolSize);
        }

        /// <summary>Ctor. </summary>
        /// <param name="executorService">threadpool to use</param>
        public ThreadedSender(IExecutorService executorService)
        {
            this._executorService = executorService;
        }

        /// <summary>Send an event. </summary>
        /// <param name="beanToSend">event to send</param>
        public void SendEvent(object beanToSend)
        {
            _executorService.Submit(
                () => Runtime
                    .SendEventBean(beanToSend, _eventTypeName));
        }

        /// <summary>Send an event. </summary>
        /// <param name="mapToSend">event to send</param>
        /// <param name="eventTypeName">name of event</param>
        public void SendEvent(IDictionary<string, object> mapToSend,
            string eventTypeName)
        {
            _executorService.Submit(
                () => Runtime
                    .SendEventMap(mapToSend, eventTypeName));
        }
    }
}
