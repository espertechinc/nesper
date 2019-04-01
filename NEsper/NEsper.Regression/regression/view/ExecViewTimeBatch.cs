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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeBatch : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionMonthScoped(epService);
            RunAssertionStartEagerForceUpdate(epService);
        }
    
        private void RunAssertionMonthScoped(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime(epService, "2002-02-01T09:00:00.000");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean#time_batch(1 month)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendCurrentTimeWithMinus(epService, "2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            SendCurrentTimeWithMinus(epService, "2002-04-01T09:00:00.000", 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendCurrentTime(epService, "2002-04-01T09:00:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            SendCurrentTime(epService, "2002-05-01T09:00:00.000");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartEagerForceUpdate(EPServiceProvider epService) {
            SendTimer(epService, 1000);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from SupportBean#time_batch(1, \"START_EAGER,FORCE_UPDATE\")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 1999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 2000);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 2999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 3000);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 4000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E1"});
    
            SendTimer(epService, 5000);
            EPAssertionUtil.AssertProps(listener.AssertOneGetOldAndReset(), "TheString".Split(','), new object[]{"E1"});
    
            SendTimer(epService, 5999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 6000);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendTimer(epService, 7000);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendCurrentTime(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendCurrentTimeWithMinus(EPServiceProvider epService, string time, long minus) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
} // end of namespace
