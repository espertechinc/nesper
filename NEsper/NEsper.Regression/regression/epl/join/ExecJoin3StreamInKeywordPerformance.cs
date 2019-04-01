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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoin3StreamInKeywordPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddEventType("S2", typeof(SupportBean_S2));
    
            string epl = "select s0.id as val from " +
                    "S0#keepall s0, " +
                    "S1#keepall s1, " +
                    "S2#keepall s2 " +
                    "where p00 in (p10, p20)";
            string[] fields = "val".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "P00_" + i));
            }
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "x"));
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S2(1, "P00_6541"));
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {6541}});
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
