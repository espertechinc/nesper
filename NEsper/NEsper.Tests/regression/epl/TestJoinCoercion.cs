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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestJoinCoercion  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestJoinCoercionRange() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            String[] fields = "sbs,sbi,sbri".Split(',');
            String epl = "select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.id as sbri from SupportBean.win:length(10) sb, SupportBeanRange.win:length(10) sbr " +
                    "where IntPrimitive between rangeStartLong and rangeEndLong";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 100, "R1"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R2", "G", 90L, 100L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2", 100, "R2"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1", 10, "R3"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1000));
            Assert.IsFalse(_listener.IsInvoked);
    
            stmt.Dispose();
            epl = "select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.id as sbri from SupportBean.win:length(10) sb, SupportBeanRange.win:length(10) sbr " +
                    "where sbr.key = sb.TheString and IntPrimitive between rangeStartLong and rangeEndLong";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 10));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("G", 101));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G", 101, "R1"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R2", "G", 90L, 102L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G", 101, "R2"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"G", 10, "R3"});
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
            _epService.EPRuntime.SendEvent(new SupportBean("G", 1000));
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestWithJoinCoercion() {
            String joinStatement = "select Volume from " +
                    typeof(SupportMarketDataBean).FullName + ".win:length(3) as s0," +
                    typeof(SupportBean).FullName + "().win:length(3) as s1 " +
                    " where s0.Volume = s1.IntPrimitive";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinStatement);
            stmt.Events += _listener.Update;
    
            SendBeanEvent(100);
            SendMarketEvent(100);
            Assert.AreEqual(100L, _listener.AssertOneGetNewAndReset().Get("Volume"));
        }
    
        private void SendBeanEvent(int intPrimitive) {
            SupportBean bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketEvent(long volume) {
            SupportMarketDataBean bean = new SupportMarketDataBean("", 0, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
