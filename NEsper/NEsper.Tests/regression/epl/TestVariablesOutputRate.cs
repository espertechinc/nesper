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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestVariablesOutputRate 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestOutputRateEventsAll()
        {
            _epService.EPAdministrator.Configuration.AddVariable("var_output_limit", typeof(long), "3");
    
            String stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.Events += _listener.Update;
    
            RunAssertionOutputRateEventsAll();
        }
    
        [Test]
        public void TestOutputRateEventsAll_OM()
        {
            _epService.EPAdministrator.Configuration.AddVariable("var_output_limit", typeof(long), "3");
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CountStar(), "cnt");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model.OutputLimitClause = OutputLimitClause.Create(OutputLimitSelector.LAST, "var_output_limit");
    
            String stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatement stmtSelect = _epService.EPAdministrator.Create(model);
            stmtSelect.Events += _listener.Update;
            Assert.AreEqual(stmtTextSelect, model.ToEPL());
    
            RunAssertionOutputRateEventsAll();
        }
    
        [Test]
        public void TestOutputRateEventsAll_Compile()
        {
            _epService.EPAdministrator.Configuration.AddVariable("var_output_limit", typeof(long), "3");
    
            String stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtTextSelect);
            EPStatement stmtSelect = _epService.EPAdministrator.Create(model);
            stmtSelect.Events += _listener.Update;
            Assert.AreEqual(stmtTextSelect, model.ToEPL());
    
            RunAssertionOutputRateEventsAll();
        }
    
        private void RunAssertionOutputRateEventsAll()
        {
            SendSupportBeans("E1", "E2");   // varargs: sends 2 events
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBeans("E3");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {3L});
            _listener.Reset();
    
            // set output limit to 5
            String stmtTextSet = "on " + typeof(SupportMarketDataBean).FullName + " set var_output_limit = Volume";
            _epService.EPAdministrator.CreateEPL(stmtTextSet);
            SendSetterBean(5L);
    
            SendSupportBeans("E4", "E5", "E6", "E7"); // send 4 events
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBeans("E8");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {8L});
            _listener.Reset();
    
            // set output limit to 2
            SendSetterBean(2L);
    
            SendSupportBeans("E9"); // send 1 events
            Assert.IsFalse(_listener.IsInvoked);
    
            SendSupportBeans("E10");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {10L});
            _listener.Reset();
    
            // set output limit to 1
            SendSetterBean(1L);
    
            SendSupportBeans("E11");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {11L});
            _listener.Reset();
    
            SendSupportBeans("E12");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {12L});
            _listener.Reset();
    
            // set output limit to null -- this continues at the current rate
            SendSetterBean(null);
    
            SendSupportBeans("E13");
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {13L});
            _listener.Reset();
        }
    
        [Test]
        public void TestOutputRateTimeAll()
        {
            _epService.EPAdministrator.Configuration.AddVariable("var_output_limit", typeof(long), "3");
            SendTimer(0);
    
            String stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output snapshot every var_output_limit seconds";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.Events += _listener.Update;
    
            SendSupportBeans("E1", "E2");   // varargs: sends 2 events
            SendTimer(2999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(3000);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {2L});
            _listener.Reset();
    
            // set output limit to 5
            String stmtTextSet = "on " + typeof(SupportMarketDataBean).FullName + " set var_output_limit = Volume";
            _epService.EPAdministrator.CreateEPL(stmtTextSet);
            SendSetterBean(5L);
    
            // set output limit to 1 second
            SendSetterBean(1L);
    
            SendTimer(3200);
            SendSupportBeans("E3", "E4");
            SendTimer(3999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(4000);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {4L});
            _listener.Reset();
    
            // set output limit to 4 seconds (takes effect next time rescheduled, and is related to reference point which is 0)
            SendSetterBean(4L);
    
            SendTimer(4999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(5000);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {4L});
            _listener.Reset();
    
            SendTimer(7999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(8000);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {4L});
            _listener.Reset();
    
            SendSupportBeans("E5", "E6");   // varargs: sends 2 events
    
            SendTimer(11999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(12000);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], new String[] {"cnt"}, new Object[] {6L});
            _listener.Reset();
    
            SendTimer(13000);
            // set output limit to 2 seconds (takes effect next time event received, and is related to reference point which is 0)
            SendSetterBean(2L);
            SendSupportBeans("E7", "E8");   // varargs: sends 2 events
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(13999);
            Assert.IsFalse(_listener.IsInvoked);
            // set output limit to null : should stay at 2 seconds
            SendSetterBean(null);
            try
            {
                SendTimer(14000);
                Assert.Fail();
            }
            catch
            {
                // expected
            }
        }
    
        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendSupportBeans(params string[] strings)
        {
            foreach (String stringValue in strings)
            {
                SendSupportBean(stringValue);
            }
        }
    
        private SupportBean SendSupportBean(String stringValue)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportMarketDataBean SendSetterBean(long? longValue)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean("", 0, longValue, "");
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
