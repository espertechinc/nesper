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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogPerf : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
    
            string text = "select * from MyEvent " +
                    "match_recognize (" +
                    "  partition by value " +
                    "  measures A.TheString as a_string, C.TheString as c_string " +
                    "  all matches " +
                    "  pattern (A B*? C) " +
                    "  define A as A.cat = '1'," +
                    "         B as B.cat = '2'," +
                    "         C as C.cat = '3'" +
                    ")";
            // When testing aggregation:
            //"  measures A.string as a_string, count(B.string) as cntb, C.string as c_string " +
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            long start = PerformanceObserver.MilliTime;
    
            for (int partition = 0; partition < 2; partition++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "1", partition));
                for (int i = 0; i < 25000; i++) {
                    epService.EPRuntime.SendEvent(new SupportRecogBean("E2_" + i, "2", partition));
                }
                Assert.IsFalse(listener.IsInvoked);
    
                epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "3", partition));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }
    
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 2000, "delta=" + delta);
        }
    }
} // end of namespace
