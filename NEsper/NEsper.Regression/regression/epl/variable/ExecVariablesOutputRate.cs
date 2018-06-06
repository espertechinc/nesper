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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    public class ExecVariablesOutputRate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var_output_limit", typeof(long), "3");
    
            RunAssertionOutputRateEventsAll(epService);
            RunAssertionOutputRateEventsAll_OM(epService);
            RunAssertionOutputRateEventsAll_Compile(epService);
            RunAssertionOutputRateTimeAll(epService);
        }
    
        private void RunAssertionOutputRateEventsAll(EPServiceProvider epService) {
            epService.EPRuntime.SetVariableValue("var_output_limit", 3L);
            string stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            TryAssertionOutputRateEventsAll(epService, listener);
    
            stmtSelect.Dispose();
        }
    
        private void RunAssertionOutputRateEventsAll_OM(EPServiceProvider epService) {
            epService.EPRuntime.SetVariableValue("var_output_limit", 3L);
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CountStar(), "cnt");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            model.OutputLimitClause = OutputLimitClause.Create(OutputLimitSelector.LAST, "var_output_limit");
    
            string stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatement stmtSelect = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            Assert.AreEqual(stmtTextSelect, model.ToEPL());
    
            TryAssertionOutputRateEventsAll(epService, listener);
    
            stmtSelect.Dispose();
        }
    
        private void RunAssertionOutputRateEventsAll_Compile(EPServiceProvider epService) {
            epService.EPRuntime.SetVariableValue("var_output_limit", 3L);
            string stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output last every var_output_limit events";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtTextSelect);
            EPStatement stmtSelect = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            Assert.AreEqual(stmtTextSelect, model.ToEPL());
    
            TryAssertionOutputRateEventsAll(epService, listener);
    
            stmtSelect.Dispose();
        }
    
        private void TryAssertionOutputRateEventsAll(EPServiceProvider epService, SupportUpdateListener listener) {
            SendSupportBeans(epService, "E1", "E2");   // varargs: sends 2 events
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeans(epService, "E3");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{3L});
            listener.Reset();
    
            // set output limit to 5
            string stmtTextSet = "on " + typeof(SupportMarketDataBean).FullName + " set var_output_limit = volume";
            epService.EPAdministrator.CreateEPL(stmtTextSet);
            SendSetterBean(epService, 5L);
    
            SendSupportBeans(epService, "E4", "E5", "E6", "E7"); // send 4 events
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeans(epService, "E8");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{8L});
            listener.Reset();
    
            // set output limit to 2
            SendSetterBean(epService, 2L);
    
            SendSupportBeans(epService, "E9"); // send 1 events
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeans(epService, "E10");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{10L});
            listener.Reset();
    
            // set output limit to 1
            SendSetterBean(epService, 1L);
    
            SendSupportBeans(epService, "E11");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{11L});
            listener.Reset();
    
            SendSupportBeans(epService, "E12");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{12L});
            listener.Reset();
    
            // set output limit to null -- this continues at the current rate
            SendSetterBean(epService, null);
    
            SendSupportBeans(epService, "E13");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{13L});
            listener.Reset();
        }
    
        private void RunAssertionOutputRateTimeAll(EPServiceProvider epService) {
            epService.EPRuntime.SetVariableValue("var_output_limit", 3L);
            SendTimer(epService, 0);
    
            string stmtTextSelect = "select count(*) as cnt from " + typeof(SupportBean).FullName + " output snapshot every var_output_limit seconds";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            SendSupportBeans(epService, "E1", "E2");   // varargs: sends 2 events
            SendTimer(epService, 2999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 3000);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{2L});
            listener.Reset();
    
            // set output limit to 5
            string stmtTextSet = "on " + typeof(SupportMarketDataBean).FullName + " set var_output_limit = volume";
            epService.EPAdministrator.CreateEPL(stmtTextSet);
            SendSetterBean(epService, 5L);
    
            // set output limit to 1 second
            SendSetterBean(epService, 1L);
    
            SendTimer(epService, 3200);
            SendSupportBeans(epService, "E3", "E4");
            SendTimer(epService, 3999);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 4000);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{4L});
            listener.Reset();
    
            // set output limit to 4 seconds (takes effect next time rescheduled, and is related to reference point which is 0)
            SendSetterBean(epService, 4L);
    
            SendTimer(epService, 4999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 5000);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{4L});
            listener.Reset();
    
            SendTimer(epService, 7999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 8000);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{4L});
            listener.Reset();
    
            SendSupportBeans(epService, "E5", "E6");   // varargs: sends 2 events
    
            SendTimer(epService, 11999);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(epService, 12000);
            EPAssertionUtil.AssertProps(listener.LastNewData[0], new[]{"cnt"}, new object[]{6L});
            listener.Reset();
    
            SendTimer(epService, 13000);
            // set output limit to 2 seconds (takes effect next time event received, and is related to reference point which is 0)
            SendSetterBean(epService, 2L);
            SendSupportBeans(epService, "E7", "E8");   // varargs: sends 2 events
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 13999);
            Assert.IsFalse(listener.IsInvoked);
            // set output limit to null : should stay at 2 seconds
            SendSetterBean(epService, null);
            try {
                SendTimer(epService, 14000);
                Assert.Fail();
            } catch (Exception) {
                // expected
            }
            stmtSelect.Dispose();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendSupportBeans(EPServiceProvider epService, params string[] strings) {
            foreach (string theString in strings) {
                SendSupportBean(epService, theString);
            }
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString) {
            var bean = new SupportBean();
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSetterBean(EPServiceProvider epService, long? longValue) {
            var bean = new SupportMarketDataBean("", 0, longValue, "");
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
