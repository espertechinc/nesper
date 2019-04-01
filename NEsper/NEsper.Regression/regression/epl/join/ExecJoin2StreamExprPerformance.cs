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
    public class ExecJoin2StreamExprPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            string epl;
    
            epl = "select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = 'E6750'";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6750);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = 'E6749'";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6749);
    
            epService.EPAdministrator.CreateEPL("create variable string myconst = 'E6751'");
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = myconst";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6751);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = (id || '6752')";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6752);
    
            epl = "select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = (id || '6753')";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6753);
    
            epl = "select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = 'E6754' and sb.IntPrimitive=6754";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6754);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = (id || '6755') and sb.IntPrimitive=6755";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6755);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between 6756 and 6756";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6756);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= 6757 and IntPrimitive <= 6757";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6757);
    
            epl = "select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = 'E6758' and sb.IntPrimitive >= 6758 and IntPrimitive <= 6758";
            TryAssertion(epService, epl, new SupportBean_ST0("E", -1), 6758);
    
            epl = "select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= (rangeStart + 1) and IntPrimitive <= (rangeEnd - 1)";
            TryAssertion(epService, epl, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);
    
            epl = "select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= 6001 and IntPrimitive <= (rangeEnd - 1)";
            TryAssertion(epService, epl, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);
    
            epl = "select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between (rangeStart + 1) and (rangeEnd - 1)";
            TryAssertion(epService, epl, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);
    
            epl = "select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between (rangeStart + 1) and 6004";
            TryAssertion(epService, epl, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);
    
            epl = "select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive in (6001 : (rangeEnd - 1)]";
            TryAssertion(epService, epl, new SupportBeanRange("R1", 6000, 6005), 6002 + 6003 + 6004);
        }
    
        private void TryAssertion(EPServiceProvider epService, string epl, Object theEvent, Object expected) {
    
            string[] fields = "val".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // preload
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
    
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(theEvent);
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{expected});
            }
            long delta = DateTimeHelper.CurrentTimeMillis - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
