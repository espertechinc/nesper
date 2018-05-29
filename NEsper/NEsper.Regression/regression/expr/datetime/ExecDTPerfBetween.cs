///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTPerfBetween : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA));
            epService.EPAdministrator.Configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
    
            epService.EPAdministrator.CreateEPL("create window AWindow#keepall as A");
            epService.EPAdministrator.CreateEPL("insert into AWindow select * from A");
    
            // preload
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A" + i, "2002-05-30T09:00:00.000", 100));
            }
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("AEarlier", "2002-05-30T08:00:00.000", 100));
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("ALater", "2002-05-30T10:00:00.000", 100));
    
            string epl = "select a.key as c0 from SupportDateTime unidirectional, AWindow as a where Longdate.between(longdateStart, longdateEnd, false, true)";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // query
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30T08:00:00.050"));
                Assert.AreEqual("AEarlier", listener.AssertOneGetNewAndReset().Get("c0"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "Delta=" + delta / 1000d);
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make("2002-05-30T10:00:00.050"));
            Assert.AreEqual("ALater", listener.AssertOneGetNewAndReset().Get("c0"));
    
            stmt.Dispose();
        }
    }
} // end of namespace
