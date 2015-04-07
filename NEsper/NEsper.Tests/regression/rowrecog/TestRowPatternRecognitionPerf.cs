///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionPerf 
    {
        [Test]
        public void TestPerfDisregardedMultimatches()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            String text = "select * from MyEvent " +
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
            //"  measures A.TheString as a_string, count(B.TheString) as cntb, C.TheString as c_string " +
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listener = new SupportUpdateListener();
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
}
