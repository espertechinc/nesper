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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitFirstHaving : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
            configuration.EngineDefaults.Logging.IsEnableTimerDebug = false;
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
    
            RunAssertionHavingNoAvgOutputFirstEvents(epService);
            RunAssertionHavingNoAvgOutputFirstMinutes(epService);
            RunAssertionHavingAvgOutputFirstEveryTwoMinutes(epService);
        }
    
        private void RunAssertionHavingNoAvgOutputFirstEvents(EPServiceProvider epService) {
            string query = "select DoublePrimitive from SupportBean having DoublePrimitive > 1 output first every 2 events";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            TryAssertion2Events(epService, listener);
            statement.Dispose();
    
            // test joined
            query = "select DoublePrimitive from SupportBean#lastevent,SupportBean_ST0#lastevent st0 having DoublePrimitive > 1 output first every 2 events";
            statement = epService.EPAdministrator.CreateEPL(query);
            epService.EPRuntime.SendEvent(new SupportBean_ST0("ID", 1));
            statement.Events += listener.Update;
            TryAssertion2Events(epService, listener);
        }
    
        private void RunAssertionHavingNoAvgOutputFirstMinutes(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            string[] fields = "val0".Split(',');
            string query = "select sum(DoublePrimitive) as val0 from SupportBean#length(5) having sum(DoublePrimitive) > 100 output first every 2 seconds";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendBeanEvent(epService, 10);
            SendBeanEvent(epService, 80);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            SendBeanEvent(epService, 11);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{101d});
    
            SendBeanEvent(epService, 1);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2999));
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, 100);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{114d});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(4999));
            SendBeanEvent(epService, 0);
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            SendBeanEvent(epService, 0);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{102d});
        }
    
        private void RunAssertionHavingAvgOutputFirstEveryTwoMinutes(EPServiceProvider epService) {
            string query = "select DoublePrimitive, avg(DoublePrimitive) from SupportBean having DoublePrimitive > 2*avg(DoublePrimitive) output first every 2 minutes";
            EPStatement statement = epService.EPAdministrator.CreateEPL(query);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, 2);
            Assert.IsFalse(listener.IsInvoked);
    
            SendBeanEvent(epService, 9);
            Assert.IsTrue(listener.IsInvoked);
        }
    
    
        private void TryAssertion2Events(EPServiceProvider epService, SupportUpdateListener listener) {
    
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 9);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 2);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendBeanEvent(epService, 2);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
        }
    
        private void SendBeanEvent(EPServiceProvider epService, double doublePrimitive) {
            var b = new SupportBean();
            b.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(b);
        }
    }
    
} // end of namespace
