///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    public class ExecVariablesTimer : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1", typeof(long), "12");
            epService.EPAdministrator.Configuration.AddVariable("var2", typeof(long), "2");
            epService.EPAdministrator.Configuration.AddVariable("var3", typeof(long), null);
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            string stmtTextSet = "on pattern [every timer:interval(100 milliseconds)] set var1 = current_timestamp, var2 = var1 + 1, var3 = var1 + var2";
            EPStatement stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
    
            Thread.Sleep(1000);
            stmtSet.Dispose();
    
            EventBean[] received = listenerSet.GetNewDataListFlattened();
            Assert.IsTrue(received.Length >= 5, "received : " + received.Length);
    
            for (int i = 0; i < received.Length; i++) {
                long var1 = (long) received[i].Get("var1");
                long var2 = (long) received[i].Get("var2");
                long var3 = (long) received[i].Get("var3");
                Assert.IsTrue(var1 >= startTime);
                Assert.AreEqual(var1, var2 - 1);
                Assert.AreEqual(var3, var2 + var1);
            }
        }
    }
} // end of namespace
