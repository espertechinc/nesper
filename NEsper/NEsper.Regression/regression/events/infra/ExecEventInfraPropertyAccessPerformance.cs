///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    public class ExecEventInfraPropertyAccessPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string methodName = ".testPerfPropertyAccess";
    
            string joinStatement = "select * from " +
                    typeof(SupportBeanCombinedProps).FullName + "#length(1)" +
                    " where indexed[0].Mapped('a').value = 'dummy'";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            var updateListener = new SupportUpdateListener();
            joinView.Events += updateListener.Update;
    
            // Send events for each stream
            SupportBeanCombinedProps theEvent = SupportBeanCombinedProps.MakeDefaultBean();
            Log.Info(methodName + " Sending events");
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 10000; i++) {
                SendEvent(epService, theEvent);
            }
            Log.Info(methodName + " Done sending events");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stays at 250, below 500ms
            Assert.IsTrue((endTime - startTime) < 1000);
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
