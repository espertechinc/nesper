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
    public class ExecJoinCoercion : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionJoinCoercionRange(epService);
            RunAssertionJoinCoercion(epService);
        }
    
        private void RunAssertionJoinCoercionRange(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            string[] fields = "sbs,sbi,sbri".Split(',');
            string epl = "select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where IntPrimitive between rangeStartLong and rangeEndLong";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 100, "R1"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R2", "G", 90L, 100L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", 100, "R2"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "R3"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1000));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
            epl = "select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where sbr.key = sb.TheString and IntPrimitive between rangeStartLong and rangeEndLong";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
            epService.EPRuntime.SendEvent(new SupportBean("G", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("G", 101));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G", 101, "R1"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R2", "G", 90L, 102L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G", 101, "R2"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"G", 10, "R3"});
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
            epService.EPRuntime.SendEvent(new SupportBean("G", 1000));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinCoercion(EPServiceProvider epService) {
            string joinStatement = "select volume from " +
                    typeof(SupportMarketDataBean).FullName + "#length(3) as s0," +
                    typeof(SupportBean).FullName + "()#length(3) as s1 " +
                    " where s0.volume = s1.IntPrimitive";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendBeanEvent(epService, 100);
            SendMarketEvent(epService, 100);
            Assert.AreEqual(100L, listener.AssertOneGetNewAndReset().Get("volume"));
        }
    
        private void SendBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketEvent(EPServiceProvider epService, long volume) {
            var bean = new SupportMarketDataBean("", 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
