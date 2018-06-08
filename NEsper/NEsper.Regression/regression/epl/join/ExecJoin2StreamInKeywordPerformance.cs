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
    public class ExecJoin2StreamInKeywordPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInKeywordSingleIndexLookup(epService);
            RunAssertionInKeywordMultiIndexLookup(epService);
        }
    
        private void RunAssertionInKeywordSingleIndexLookup(EPServiceProvider epService) {
            string epl = "select IntPrimitive as val from SupportBean#keepall sb, SupportBean_S0 s0 unidirectional " +
                    "where sb.TheString in (s0.p00, s0.p01)";
            string[] fields = "val".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E645", "E8975"));
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {645}, new object[] {8975}});
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInKeywordMultiIndexLookup(EPServiceProvider epService) {
            string epl = "select id as val from SupportBean_S0#keepall s0, SupportBean sb unidirectional " +
                    "where sb.TheString in (s0.p00, s0.p01)";
            string[] fields = "val".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_S0(i, "p00_" + i, "p01_" + i));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("p01_645", 0));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{645});
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);
    
            stmt.Dispose();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
