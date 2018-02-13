///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.assertFalse;
// using static org.junit.Assert.assertTrue;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTMicrosecondResolution : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            IDictionary<TimeUnit, EPServiceProvider> epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
            RunAssertionEventTime(epServices);
            RunAssertionLongProperty(epServices);
        }
    
        public void RunAssertionEventTime(IDictionary<TimeUnit, EPServiceProvider> epServices) {
            foreach (EPServiceProvider epService in epServices.Values()) {
                epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(id string, sts long, ets long) starttimestamp sts endtimestamp ets");
            }
    
            long time = DateTimeParser.ParseDefaultMSec("2002-05-30T09:00:00.000");
            RunAssertionEventTime(epServices.Get(TimeUnit.MILLISECONDS), time, time);
            RunAssertionEventTime(epServices.Get(TimeUnit.MICROSECONDS), time * 1000, time * 1000);
        }
    
        private void RunAssertionLongProperty(IDictionary<TimeUnit, EPServiceProvider> epServices) {
            foreach (EPServiceProvider epService in epServices.Values()) {
                epService.EPAdministrator.Configuration.AddEventType(typeof(SupportDateTime));
            }
    
            long time = DateTimeParser.ParseDefaultMSec("2002-05-30T09:05:06.007");
            Calendar calTime = GregorianCalendar.Instance;
            calTime.TimeInMillis = time;
    
            Calendar calMod = GregorianCalendar.Instance;
            calMod.TimeInMillis = time;
            calMod.Set(Calendar.HOUR_OF_DAY, 1);
            calMod.Set(Calendar.MINUTE, 2);
            calMod.Set(Calendar.SECOND, 3);
            calMod.Set(Calendar.MILLISECOND, 4);
    
            string select =
                    "longdate.WithTime(1, 2, 3, 4) as c0," +
                            "longdate.Set('hour', 1).Set('minute', 2).Set('second', 3).Set('millisecond', 4).ToCalendar() as c1," +
                            "longdate.Get('month') as c2," +
                            "current_timestamp.Get('month') as c3," +
                            "current_timestamp.MinuteOfHour as c4," +
                            "current_timestamp.ToDate() as c5," +
                            "current_timestamp.ToCalendar() as c6," +
                            "current_timestamp.Minus(1) as c7";
            string[] fields = "c0,c1,c2,c3,c4,c5,c6,c7".Split(',');
    
            RunAssertionLongProperty(epServices.Get(TimeUnit.MILLISECONDS), time, new SupportDateTime(time, null, null, null, null), select, fields,
                    new Object[]{calMod.TimeInMillis, calMod, 4, 4, 5, calTime.Time, calTime, time - 1});
            RunAssertionLongProperty(epServices.Get(TimeUnit.MICROSECONDS), time * 1000, new SupportDateTime(time * 1000 + 123, null, null, null, null), select, fields,
                    new Object[]{calMod.TimeInMillis * 1000 + 123, calMod, 4, 4, 5, calTime.Time, calTime, time * 1000 - 1000});
        }
    
        private static void RunAssertionEventTime(EPServiceProvider epService, long tsB, long flipTimeEndtsA) {
    
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL("select * from MyEvent(id='A') as a unidirectional, MyEvent(id='B')#lastevent as b where A.WithDate(2002, 4, 30).Before(b)", "s0", null);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            isolated.EPRuntime.SendEvent(new Object[]{"B", tsB, tsB}, "MyEvent");
    
            isolated.EPRuntime.SendEvent(new Object[]{"A", flipTimeEndtsA - 1, flipTimeEndtsA - 1}, "MyEvent");
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            isolated.EPRuntime.SendEvent(new Object[]{"A", flipTimeEndtsA, flipTimeEndtsA}, "MyEvent");
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            isolated.Destroy();
        }
    
        private void RunAssertionLongProperty(EPServiceProvider epService, long startTime, SupportDateTime @event, string select, string[] fields, Object[] expected) {
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
            isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));
    
            EPStatement stmt = isolated.EPAdministrator.CreateEPL("select " + select + " from SupportDateTime", "s0", null);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            isolated.EPRuntime.SendEvent(@event);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            isolated.Destroy();
        }
    }
} // end of namespace
