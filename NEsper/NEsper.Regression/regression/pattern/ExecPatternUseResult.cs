///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternUseResult : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionNumeric(epService);
            RunAssertionObjectId(epService);
            RunAssertionFollowedByFilter(epService);
            RunAssertionPatternTypeCacheForRepeat(epService);
        }
    
        private void RunAssertionPatternTypeCacheForRepeat(EPServiceProvider epService) {
            // UEJ-229-28464 bug fix for type reuse for dissimilar types
            string epl = "create objectarray schema TypeOne(symbol string, price double);\n" +
                    "create objectarray schema TypeTwo(symbol string, market string, price double);\n" +
                    "\n" +
                    "@Name('Out2') select a[0].symbol from pattern [ [2] a=TypeOne ]\n;" +
                    "@Name('Out3') select a[0].market from pattern [ [2] a=TypeTwo ];";
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            var listenerOut2 = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("Out2").Events += listenerOut2.Update;
    
            var listenerOut3 = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("Out3").Events += listenerOut3.Update;
    
            epService.EPRuntime.SendEvent(new object[]{"GE", 10}, "TypeOne");
            epService.EPRuntime.SendEvent(new object[]{"GE", 10}, "TypeOne");
            Assert.IsTrue(listenerOut2.IsInvokedAndReset());
    
            epService.EPRuntime.SendEvent(new object[]{"GE", 10, 5}, "TypeTwo");
            epService.EPRuntime.SendEvent(new object[]{"GE", 10, 5}, "TypeTwo");
            Assert.IsTrue(listenerOut3.IsInvokedAndReset());
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionNumeric(EPServiceProvider epService) {
            string @event = typeof(SupportBean_N).FullName;
    
            EventCollection events = EventCollectionFactory.GetSetThreeExternalClock(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("na=" + @event + " -> nb=" + @event + "(DoublePrimitive = na.DoublePrimitive)");
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=87) -> nb=" + @event + "(IntPrimitive > na.IntPrimitive)");
            testCase.Add("N8", "na", events.GetEvent("N3"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=87) -> nb=" + @event + "(IntPrimitive < na.IntPrimitive)");
            testCase.Add("N4", "na", events.GetEvent("N3"), "nb", events.GetEvent("N4"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=66) -> every nb=" + @event + "(IntPrimitive >= na.IntPrimitive)");
            testCase.Add("N3", "na", events.GetEvent("N2"), "nb", events.GetEvent("N3"));
            testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(BoolBoxed=false) -> every nb=" + @event + "(BoolPrimitive = na.BoolPrimitive)");
            testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + @event + " -> every nb=" + @event + "(IntPrimitive=na.IntPrimitive)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + @event + "() -> every nb=" + @event + "(DoublePrimitive=na.DoublePrimitive)");
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + @event + "(BoolBoxed=false) -> every nb=" + @event + "(BoolBoxed=na.BoolBoxed)");
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCase.Add("N8", "na", events.GetEvent("N5"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(BoolBoxed=false) -> nb=" + @event + "(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=" + @event + "(IntPrimitive > nb.IntPrimitive)");
            testCase.Add("N6", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"), "nc", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=86) -> nb=" + @event + "(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=" + @event + "(IntPrimitive > na.IntPrimitive)");
            testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=86) -> (nb=" + @event + "(IntPrimitive<na.IntPrimitive)" +
                    " or nc=" + @event + "(IntPrimitive > na.IntPrimitive))");
            testCase.Add("N5", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", null);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=86) -> (nb=" + @event + "(IntPrimitive>na.IntPrimitive)" +
                    " or nc=" + @event + "(IntBoxed < na.IntBoxed))");
            testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N8"), "nc", null);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "(IntPrimitive=86) -> (nb=" + @event + "(IntPrimitive>na.IntPrimitive)" +
                    " and nc=" + @event + "(IntBoxed < na.IntBoxed))");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "() -> every nb=" + @event + "(DoublePrimitive in [0:na.DoublePrimitive])");
            testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "() -> every nb=" + @event + "(DoublePrimitive in (0:na.DoublePrimitive))");
            testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "() -> every nb=" + @event + "(IntPrimitive in (na.IntPrimitive:na.DoublePrimitive))");
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + @event + "() -> every nb=" + @event + "(IntPrimitive in (na.IntPrimitive:60))");
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionObjectId(EPServiceProvider epService) {
            string @event = typeof(SupportBean_S0).FullName;
    
            EventCollection events = EventCollectionFactory.GetSetFourExternalClock(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("X1=" + @event + "() -> X2=" + @event + "(p00=X1.p00)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("X1=" + @event + "(p00='B') -> X2=" + @event + "(p00=X1.p00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("X1=" + @event + "(p00='B') -> every X2=" + @event + "(p00=X1.p00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + @event + "(p00='B') -> every X2=" + @event + "(p00=X1.p00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + @event + "() -> X2=" + @event + "(p00=X1.p00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
            testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + @event + "() -> every X2=" + @event + "(p00=X1.p00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
            testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionFollowedByFilter(EPServiceProvider epService) {
            // Test for ESPER-121
            epService.EPAdministrator.Configuration.AddEventType("FxTradeEvent", typeof(SupportTradeEvent));
    
            string expression = "every tradeevent1=FxTradeEvent(userId in ('U1000','U1001','U1002') ) -> " +
                    "(tradeevent2=FxTradeEvent(userId in ('U1000','U1001','U1002') and " +
                    "  userId != tradeevent1.userId and " +
                    "  ccypair = tradeevent1.ccypair and " +
                    "  direction = tradeevent1.direction) -> " +
                    " tradeevent3=FxTradeEvent(userId in ('U1000','U1001','U1002') and " +
                    "  userId != tradeevent1.userId and " +
                    "  userId != tradeevent2.userId and " +
                    "  ccypair = tradeevent1.ccypair and " +
                    "  direction = tradeevent1.direction)" +
                    ") where timer:within(600 sec)";
    
            EPStatement statement = epService.EPAdministrator.CreatePattern(expression);
            var listener = new MyUpdateListener();
            statement.Events += listener.Update;
    
            var random = new Random();
            string[] users = {"U1000", "U1001", "U1002"};
            string[] ccy = {"USD", "JPY", "EUR"};
            string[] direction = {"B", "S"};
    
            for (int i = 0; i < 100; i++) {
                var theEvent = new SupportTradeEvent(i,
                    users[random.Next(users.Length)],
                    ccy[random.Next(ccy.Length)], direction[random.Next(direction.Length)]);
                epService.EPRuntime.SendEvent(theEvent);
            }
    
            Assert.AreEqual(0, listener.BadMatchCount);
            statement.Dispose();
        }
    
        private class MyUpdateListener
        {
            private int _badMatchCount;

            public int BadMatchCount => _badMatchCount;

            public void Update(object sender, UpdateEventArgs args)
            {
                if (args.NewEvents != null) {
                    foreach (EventBean @eventBean in args.NewEvents) {
                        HandleEvent(eventBean);
                    }
                }
            }
    
            private void HandleEvent(EventBean eventBean) {
                SupportTradeEvent tradeevent1 = (SupportTradeEvent)
                        eventBean.Get("tradeevent1");
                SupportTradeEvent tradeevent2 = (SupportTradeEvent)
                        eventBean.Get("tradeevent2");
                SupportTradeEvent tradeevent3 = (SupportTradeEvent)
                        eventBean.Get("tradeevent3");
    
                if (tradeevent1.UserId.Equals(tradeevent2.UserId) ||
                        tradeevent1.UserId.Equals(tradeevent3.UserId) ||
                        tradeevent2.UserId.Equals(tradeevent3.UserId)) {
                    /*
                    Log.Info("Bad Match : ");
                    Log.Info(tradeevent1);
                    Log.Info(tradeevent2);
                    Log.Info(tradeevent3 + "\n");
                    */
                    _badMatchCount++;
                }
            }
        }
    }
} // end of namespace
