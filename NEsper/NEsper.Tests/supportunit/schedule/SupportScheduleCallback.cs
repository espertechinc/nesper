///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.schedule;

namespace com.espertech.esper.supportunit.schedule
{
    public class SupportScheduleCallback : ScheduleHandle, ScheduleHandleCallback 
    {
        private static int _orderAllCallbacks;
    
        private int _orderTriggered = 0;
    
        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext)
        {
            log.Debug(".scheduledTrigger");
            _orderAllCallbacks++;
            _orderTriggered = _orderAllCallbacks;
        }
    
        public int ClearAndGetOrderTriggered()
        {
            int result = _orderTriggered;
            _orderTriggered = 0;
            return result;
        }
    
        public static void SetCallbackOrderNum(int orderAllCallbacks) {
            SupportScheduleCallback._orderAllCallbacks = orderAllCallbacks;
        }

        public int StatementId
        {
            get { return 1; }
        }

        public int AgentInstanceId
        {
            get { return 0; }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
