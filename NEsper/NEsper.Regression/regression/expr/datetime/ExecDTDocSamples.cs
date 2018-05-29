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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTDocSamples : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            var meta = new Dictionary<string, object>();
            meta.Put("timeTaken", typeof(DateTime));
            epService.EPAdministrator.Configuration.AddEventType("RFIDEvent", meta);
    
            epService.EPAdministrator.CreateEPL("select timeTaken.Format() as timeTakenStr from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Get('month') as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.GetMonthOfYear() as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Minus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Minus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Plus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Plus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.RoundCeiling('min') as timeTakenRounded from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.RoundFloor('min') as timeTakenRounded from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.Set('month', 3) as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.withDate(2002, 4, 30) as timeTakenDated from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.WithMax('sec') as timeTakenMaxSec from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.ToCalendar() as timeTakenCal from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.ToDate() as timeTakenDate from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select timeTaken.ToMillisec() as timeTakenLong from RFIDEvent");
    
            // test pattern use
            var leg = new ConfigurationEventTypeLegacy();
            leg.StartTimestampPropertyName = "longdateStart";
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA), leg);
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB), leg);
    
            TryRun(epService, "a.longdateStart.after(b)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.after(b.longdateStart)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.after(b)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.after(b)", "2002-05-30T08:59:59.999", "2002-05-30T09:00:00.000", false);
        }
    
        private void TryRun(EPServiceProvider epService, string condition, string tsa, string tsb, bool isInvoked) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern [a=A -> b=B] as abc where " + condition);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", tsa, 0));
            epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("E2", tsb, 0));
            Assert.AreEqual(isInvoked, listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        public class MyEvent {
    
            public string Get() {
                return "abc";
            }
        }
    }
} // end of namespace
