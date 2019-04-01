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

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogIntervalMicrosecondResolution : RegressionExecution {
    
        public override void Run(EPServiceProvider defaultService) {
            IDictionary<TimeUnit, EPServiceProvider> epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
    
            foreach (EPServiceProvider epService in epServices.Values) {
                epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            }
    
            RunAssertionWithTime(epServices.Get(TimeUnit.MILLISECONDS), 0, 10000);
            RunAssertionWithTime(epServices.Get(TimeUnit.MICROSECONDS), 0, 10000000);
        }
    
        private void RunAssertionWithTime(EPServiceProvider epService, long startTime, long flipTime) {
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));
    
            string text = "select * from SupportBean " +
                    "match_recognize (" +
                    " measures A as a" +
                    " pattern (A*)" +
                    " interval 10 seconds" +
                    ")";
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL(text, "s0", null);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            isolated.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            isolated.Dispose();
        }
    }
} // end of namespace
