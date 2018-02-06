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

namespace com.espertech.esper.regression.resultset.outputlimit
{
    public class ExecOutputLimitParameterizedByContext : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MySimpleScheduleEvent));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T09:00:00.000")));
            epService.EPAdministrator.CreateEPL("create context MyCtx start MySimpleScheduleEvent as sse");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyCtx\n" +
                    "select count(*) as c \n" +
                    "from SupportBean_S0\n" +
                    "output last At(context.sse.atminute, context.sse.athour, *, *, *, *) and when terminated\n");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new MySimpleScheduleEvent(10, 15));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T10:14:59.000")));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T10:15:00.000")));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
        }
    
        public class MySimpleScheduleEvent {
            private int athour;
            private int atminute;
    
            public MySimpleScheduleEvent(int athour, int atminute) {
                this.athour = athour;
                this.atminute = atminute;
            }
    
            public int GetAthour() {
                return athour;
            }
    
            public int GetAtminute() {
                return atminute;
            }
        }
    }
} // end of namespace
