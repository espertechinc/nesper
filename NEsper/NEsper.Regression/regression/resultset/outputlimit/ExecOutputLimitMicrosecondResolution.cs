///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitMicrosecondResolution : RegressionExecution {
    
        public override void Run(EPServiceProvider defaultEPService) {
            IDictionary<TimeUnit, EPServiceProvider> epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
            foreach (EPServiceProvider epService in epServices.Values) {
                epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            }
    
            RunAssertionOutputLimit(epServices.Get(TimeUnit.MILLISECONDS), 0, "1", 1000, 1000);
            RunAssertionOutputLimit(epServices.Get(TimeUnit.MICROSECONDS), 0, "1", 1000000, 1000000);
            RunAssertionOutputLimit(epServices.Get(TimeUnit.MILLISECONDS), 789123456789L, "0.1", 789123456789L + 100, 100);
            RunAssertionOutputLimit(epServices.Get(TimeUnit.MICROSECONDS), 789123456789L, "0.1", 789123456789L + 100000, 100000);
        }
    
        private void RunAssertionOutputLimit(EPServiceProvider epService, long startTime, string size, long flipTime, long repeatTime) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));
    
            var listener = new SupportUpdateListener();
            EPStatement stmt = isolated.EPAdministrator.CreateEPL("select * from SupportBean output every " + size + " seconds", "s0", null);
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportBean("E1", 10));
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
            Assert.IsFalse(listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            isolated.EPRuntime.SendEvent(new SupportBean("E2", 10));
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(repeatTime + flipTime - 1));
            Assert.IsFalse(listener.IsInvoked);
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(repeatTime + flipTime));
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            isolated.Dispose();
        }
    }
} // end of namespace
