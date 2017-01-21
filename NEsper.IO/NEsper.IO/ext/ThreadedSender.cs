///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.threading;

namespace com.espertech.esperio.ext
{
    /// <summary>
    /// Implementation of Sender to send to esper engine using threads
    /// </summary>
    public class ThreadedSender : DirectSender
    {
    	private readonly IExecutorService executorService;
    
        /// <summary>Ctor. </summary>
        /// <param name="threadPoolSize">size of pool</param>
        public ThreadedSender(int threadPoolSize) {
    		executorService = new DedicatedExecutorService("threaded-sender", threadPoolSize);
    	}
    
        /// <summary>Ctor. </summary>
        /// <param name="executorService">threadpool to use</param>
        public ThreadedSender(IExecutorService executorService)
        {
            this.executorService = executorService;
        }
    
        /// <summary>Send an event. </summary>
        /// <param name="beanToSend">event to send</param>
        public void SendEvent(Object beanToSend) {
            executorService.Submit(() => Runtime.SendEvent(beanToSend));
    	}
    
        /// <summary>Send an event. </summary>
        /// <param name="mapToSend">event to send</param>
        /// <param name="eventTypeName">name of event</param>
        public void SendEvent(IDictionary<string,object> mapToSend, String eventTypeName) {
            executorService.Submit(() => Runtime.SendEvent(mapToSend, eventTypeName));
    	}
    }
}
