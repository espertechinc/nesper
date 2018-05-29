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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternObserverTimerAt : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddVariable("VMIN", typeof(int), 0);
            configuration.AddVariable("VHOUR", typeof(int), 8);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionAtWeekdays(epService);
            RunAssertionAtWeekdaysPrepared(epService);
            RunAssertionAtWeekdaysVariable(epService);
            RunAssertionExpression(epService);
            RunAssertionPropertyAndSODAAndTimezone(epService);
            RunAssertionEvery15thMonth(epService);
        }

        private DateTimeOffset MakeDateTime(int year, int month, int dayOfMonth, int hour, int minute, int second, int millisecond)
        {
            var dateTime = new DateTimeOffset(
                year, month, dayOfMonth, hour, minute, second, millisecond, TimeZoneInfo.Local.BaseUtcOffset);
            if (TimeZoneInfo.Local.SupportsDaylightSavingTime)
            {
                dateTime = dateTime.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dateTime));
            }

            return dateTime;
        }

        private void RunAssertionEvery15thMonth(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("select * from pattern[every timer:at(*,*,*,*/15,*)]").Dispose();
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            var dateTime = MakeDateTime(2005, 3, 9, 8, 0, 0, 0);
            var startTime = dateTime.TimeInMillis();

            // Start a 2004-12-9 8:00:00am and send events every 10 minutes
            //          "A1"    8:10
            //          "B1"    8:20
            //          "C1"    8:30
            //          "B2"    8:40
            //          "A2"    8:50
            //          "D1"    9:00
            //          "E1"    9:10
            //          "F1"    9:20
            //          "D2"    9:30
            //          "B3"    9:40
            //          "G1"    9:50
            //          "D3"   10:00

            var testData = EventCollectionFactory.GetEventSetOne(startTime, 1000 * 60 * 10);
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("timer:at(10, 8, *, *, *)");
            testCase.Add("A1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(10, 8, *, *, *, 1)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(5, 8, *, *, *)");
            testCase.Add("A1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(10, 8, *, *, *, *)");
            testCase.Add("A1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(25, 9, *, *, *)");
            testCase.Add("D2");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(11, 8, *, *, *)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(19, 8, *, *, *, 59)");
            testCase.Add("B1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(* / 5, *, *, *, *, *)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(*, *, *, *, *, * / 10)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(20, 8, *, *, *, 20)");
            testCase.Add("C1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(*, *, *, *, *)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(*, *, *, *, *, *)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(* / 9, *, *, *, *, *)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(* / 10, *, *, *, *, *)");
            AddAll(testCase);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every timer:at(* / 30, *, *, *, *)");
            testCase.Add("C1");
            testCase.Add("D1");
            testCase.Add("D2");
            testCase.Add("D3");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(10, 9, *, *, *, 10) or timer:at(30, 9, *, *, *, *)");
            testCase.Add("F1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id='B3') -> timer:at(20, 9, *, *, *, *)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id='B3') -> timer:at(45, 9, *, *, *, *)");
            testCase.Add("G1", "b", testData.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(59, 8, *, *, *, 59) -> d=" + EVENT_D_CLASS);
            testCase.Add("D1", "d", testData.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(*, 9, *, *, *, 59) -> d=" + EVENT_D_CLASS);
            testCase.Add("D2", "d", testData.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(22, 8, *, *, *) -> b=" + EVENT_B_CLASS + "(id='B3') -> timer:at(55, *, *, *, *)");
            testCase.Add("D3", "b", testData.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(40, *, *, *, *, 1) and b=" + EVENT_B_CLASS);
            testCase.Add("A2", "b", testData.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(40, 9, *, *, *, 1) or d=" + EVENT_D_CLASS + "(id=\"D3\")");
            testCase.Add("G1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(22, 8, *, *, *) -> b=" + EVENT_B_CLASS + "() -> timer:at(55, 8, *, *, *)");
            testCase.Add("D1", "b", testData.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(22, 8, *, *, *, 1) where timer:within(1 second)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(22, 8, *, *, *, 1) where timer:within(31 minutes)");
            testCase.Add("C1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(*, 9, *, *, *) and timer:at(55, *, *, *, *)");
            testCase.Add("D1");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("timer:at(40, 8, *, *, *, 1) and b=" + EVENT_B_CLASS);
            testCase.Add("A2", "b", testData.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            var text = "select * from pattern [timer:at(10,8,*,*,*,*)]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.TimerAt(10, 8, null, null, null, null);
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("A1");
            testCaseList.AddTest(testCase);
    
            // As of release 1.6 this no longer updates listeners when the statement is started.
            // The reason is that the dispatch view only gets attached after a pattern started, therefore
            // ZeroDepthEventStream looses the event.
            // There should be no use case requiring this
            // 
            //          testCase = new EventExpressionCase("not timer:at(22, 8, *, *, *, 1)");
            //          testCase.Add(EventCollection.ON_START_EVENT_ID);
            //          testCaseList.AddTest(testCase);
    
            // Run all tests
            var util = new PatternTestHarness(testData, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionAtWeekdays(EPServiceProvider epService) {
            const string expression = "select * from pattern [every timer:at(0,8,*,*,[1,2,3,4,5])]";

            var dateTime = MakeDateTime(2008, 7, 3, 10, 0, 0, 0); // start on a Sunday at 6am, August 3 2008
            SendTimer(dateTime.TimeInMillis(), epService);

            var statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            if (!InstrumentationHelper.ENABLED) {
                TryAssertion(epService, listener);
            }
    
            statement.Dispose();
        }
    
        private void RunAssertionAtWeekdaysPrepared(EPServiceProvider epService) {
            var expression = "select * from pattern [every timer:at(?,?,*,*,[1,2,3,4,5])]";

            var cal = MakeDateTime(2008, 7, 3, 10, 0, 0, 0); // start on a Sunday at 6am, August 3 2008
            SendTimer(cal.TimeInMillis(), epService);

            var prepared = epService.EPAdministrator.PrepareEPL(expression);
            prepared.SetObject(1, 0);
            prepared.SetObject(2, 8);
            var statement = epService.EPAdministrator.Create(prepared);
    
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            } // excluding assertion, too many steps
    
            TryAssertion(epService, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionAtWeekdaysVariable(EPServiceProvider epService) {
            var expression = "select * from pattern [every timer:at(VMIN,VHOUR,*,*,[1,2,3,4,5])]";

            var cal = MakeDateTime(2008, 7, 3, 10, 0, 0, 0); // start on a Sunday at 6am, August 3 2008
            SendTimer(cal.TimeInMillis(), epService);

            var prepared = epService.EPAdministrator.PrepareEPL(expression);
            var statement = epService.EPAdministrator.Create(prepared);
    
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            if (!InstrumentationHelper.ENABLED) {
                TryAssertion(epService, listener);
            }
    
            statement.Dispose();
        }
    
        private void RunAssertionExpression(EPServiceProvider epService) {
            var expression = "select * from pattern [every timer:at(7+1-8,4+4,*,*,[1,2,3,4,5])]";

            var cal = MakeDateTime(2008, 7, 3, 10, 0, 0, 0); // start on a Sunday at 6am, August 3 2008
            SendTimer(cal.TimeInMillis(), epService);

            var prepared = epService.EPAdministrator.PrepareEPL(expression);
            var statement = epService.EPAdministrator.Create(prepared);
    
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.EndTest();
            } // excluding assertion, too many steps
    
            TryAssertion(epService, listener);
    
            statement.Dispose();
        }
    
        private void RunAssertionPropertyAndSODAAndTimezone(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            SendTimeEvent("2008-08-3T06:00:00.000", epService);
            var expression = "select * from pattern [a=SupportBean -> every timer:at(2*a.IntPrimitive,*,*,*,*)]";
            var statement = epService.EPAdministrator.CreateEPL(expression);
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
    
            SendTimeEvent("2008-08-3T06:39:59.000", epService);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimeEvent("2008-08-3T06:40:00.000", epService);
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            statement.Dispose();
    
            // test SODA
            var epl = "select * from pattern [every timer:at(*/VFREQ,VMIN:VMAX,1 last,*,[8,2:VMAX,*/VREQ])]";
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());

            // test timezone
            if (TimeZoneInfo.Local.BaseUtcOffset == TimeSpan.FromHours(-5)) {    // asserting only in EST timezone, see schedule util tests
                SendTimeEvent("2008-01-4T06:50:00.000", epService);
                epService.EPAdministrator.CreateEPL("select * from pattern [timer:at(0, 5, 4, 1, *, 0, 'PST')]").Events += listener.Update;
    
                SendTimeEvent("2008-01-4T07:59:59.999", epService);
                Assert.IsFalse(listener.GetAndClearIsInvoked());
    
                SendTimeEvent("2008-01-4T08:00:00.000", epService);
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }
            epService.EPAdministrator.CreateEPL("select * from pattern [timer:at(0, 5, 4, 8, *, 0, 'xxx')]").Events += listener.Update;
            epService.EPAdministrator.CreateEPL("select * from pattern [timer:at(0, 5, 4, 8, *, 0, *)]").Events += listener.Update;
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener) {
            var dateTime = MakeDateTime(2008, 7, 3, 10, 0, 0, 0); // start on a Sunday at 6am, August 3 2008

            var invocations = new List<string>();
            for (var i = 0; i < 24 * 60 * 7; i++) { // run for 1 week
                dateTime = dateTime.AddUsingField(DateTimeFieldEnum.MINUTE, 1);
                SendTimer(dateTime.TimeInMillis(), epService);
    
                if (listener.GetAndClearIsInvoked()) {
                    // Log.Info("invoked at calendar " + cal.Time.ToString());
                    invocations.Add(dateTime.ToShortTimeString());
                }
            }
            var expectedResult = new string[5];
            dateTime = MakeDateTime(2008, 7, 4, 8, 0, 0, 0); //"Mon Aug 04 08:00:00 EDT 2008"
            expectedResult[0] = dateTime.ToShortTimeString();
            dateTime = MakeDateTime(2008, 7, 5, 8, 0, 0, 0); //"Tue Aug 05 08:00:00 EDT 2008"
            expectedResult[1] = dateTime.ToShortTimeString();
            dateTime = MakeDateTime(2008, 7, 6, 8, 0, 0, 0); //"Wed Aug 06 08:00:00 EDT 2008"
            expectedResult[2] = dateTime.ToShortTimeString();
            dateTime = MakeDateTime(2008, 7, 7, 8, 0, 0, 0); //"Thu Aug 07 08:00:00 EDT 2008"
            expectedResult[3] = dateTime.ToShortTimeString();
            dateTime = MakeDateTime(2008, 7, 8, 8, 0, 0, 0); //"Fri Aug 08 08:00:00 EDT 2008"
            expectedResult[4] = dateTime.ToShortTimeString();
            EPAssertionUtil.AssertEqualsExactOrder(expectedResult, invocations.ToArray());
        }
    
        private void SendTimeEvent(string time, EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    
        private void SendTimer(long timeInMSec, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void AddAll(EventExpressionCase desc) {
            desc.Add("A1");
            desc.Add("B1");
            desc.Add("C1");
            desc.Add("B2");
            desc.Add("A2");
            desc.Add("D1");
            desc.Add("E1");
            desc.Add("F1");
            desc.Add("D2");
            desc.Add("B3");
            desc.Add("G1");
            desc.Add("D3");
        }
    }
} // end of namespace
