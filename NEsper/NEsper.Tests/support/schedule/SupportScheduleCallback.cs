///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.schedule;

namespace com.espertech.esper.support.schedule
{
    public class SupportScheduleCallback : ScheduleHandle, ScheduleHandleCallback 
    {
        private static int orderAllCallbacks;
    
        private int orderTriggered = 0;
    
        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext)
        {
            log.Debug(".scheduledTrigger");
            orderAllCallbacks++;
            orderTriggered = orderAllCallbacks;
        }
    
        public int ClearAndGetOrderTriggered()
        {
            int result = orderTriggered;
            orderTriggered = 0;
            return result;
        }
    
        public static void SetCallbackOrderNum(int orderAllCallbacks) {
            SupportScheduleCallback.orderAllCallbacks = orderAllCallbacks;
        }

        public string StatementId
        {
            get { return null; }
        }

        public int AgentInstanceId
        {
            get { return 0; }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
