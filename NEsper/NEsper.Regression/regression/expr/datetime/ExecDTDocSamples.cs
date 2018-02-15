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
    
            epService.EPAdministrator.CreateEPL("select TimeTaken.Format() as timeTakenStr from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Get('month') as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.MonthOfYear as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Minus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Minus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Plus(2 minutes) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Plus(2*60*1000) as timeTakenMinus2Min from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.RoundCeiling('min') as timeTakenRounded from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.RoundFloor('min') as timeTakenRounded from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.Set('month', 3) as timeTakenMonth from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.WithDate(2002, 4, 30) as timeTakenDated from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.WithMax('sec') as timeTakenMaxSec from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.ToCalendar() as timeTakenCal from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.ToDate() as timeTakenDate from RFIDEvent");
            epService.EPAdministrator.CreateEPL("select TimeTaken.ToMillisec() as timeTakenLong from RFIDEvent");
    
            // test pattern use
            var leg = new ConfigurationEventTypeLegacy();
            leg.StartTimestampPropertyName = "longdateStart";
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).Name, leg);
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).Name, leg);
    
            TryRun(epService, "a.longdateStart.After(b)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.After(b.longdateStart)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.After(b)", "2002-05-30T09:00:00.000", "2002-05-30T08:59:59.999", true);
            TryRun(epService, "a.After(b)", "2002-05-30T08:59:59.999", "2002-05-30T09:00:00.000", false);
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
