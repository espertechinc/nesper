///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestUseResultPattern : SupportBeanConstants
    {
        [Test]
        public void TestNumeric()
        {
            string EVENT = typeof(SupportBean_N).FullName;
    
            EventCollection events = EventCollectionFactory.GetSetThreeExternalClock(0, 1000);
            CaseList testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("na=" + EVENT + " -> nb=" + EVENT + "(DoublePrimitive = na.DoublePrimitive)");
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=87) -> nb=" + EVENT + "(IntPrimitive > na.IntPrimitive)");
            testCase.Add("N8", "na", events.GetEvent("N3"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=87) -> nb=" + EVENT + "(IntPrimitive < na.IntPrimitive)");
            testCase.Add("N4", "na", events.GetEvent("N3"), "nb", events.GetEvent("N4"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=66) -> every nb=" + EVENT + "(IntPrimitive >= na.IntPrimitive)");
            testCase.Add("N3", "na", events.GetEvent("N2"), "nb", events.GetEvent("N3"));
            testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(BoolBoxed=false) -> every nb=" + EVENT + "(BoolPrimitive = na.BoolPrimitive)");
            testCase.Add("N4", "na", events.GetEvent("N2"), "nb", events.GetEvent("N4"));
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + EVENT + " -> every nb=" + EVENT + "(IntPrimitive=na.IntPrimitive)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + EVENT + "() -> every nb=" + EVENT + "(DoublePrimitive=na.DoublePrimitive)");
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every na=" + EVENT + "(BoolBoxed=false) -> every nb=" + EVENT + "(BoolBoxed=na.BoolBoxed)");
            testCase.Add("N5", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"));
            testCase.Add("N8", "na", events.GetEvent("N2"), "nb", events.GetEvent("N8"));
            testCase.Add("N8", "na", events.GetEvent("N5"), "nb", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(BoolBoxed=false) -> nb=" + EVENT + "(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=" + EVENT + "(IntPrimitive > nb.IntPrimitive)");
            testCase.Add("N6", "na", events.GetEvent("N2"), "nb", events.GetEvent("N5"), "nc", events.GetEvent("N6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=86) -> nb=" + EVENT + "(IntPrimitive<na.IntPrimitive)" +
                    " -> nc=" + EVENT + "(IntPrimitive > na.IntPrimitive)");
            testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", events.GetEvent("N8"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=86) -> (nb=" + EVENT + "(IntPrimitive<na.IntPrimitive)" +
                    " or nc=" + EVENT + "(IntPrimitive > na.IntPrimitive))");
            testCase.Add("N5", "na", events.GetEvent("N4"), "nb", events.GetEvent("N5"), "nc", null);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=86) -> (nb=" + EVENT + "(IntPrimitive>na.IntPrimitive)" +
                    " or nc=" + EVENT + "(IntBoxed < na.IntBoxed))");
            testCase.Add("N8", "na", events.GetEvent("N4"), "nb", events.GetEvent("N8"), "nc", null);
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "(IntPrimitive=86) -> (nb=" + EVENT + "(IntPrimitive>na.IntPrimitive)" +
                    " and nc=" + EVENT + "(IntBoxed < na.IntBoxed))");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "() -> every nb=" + EVENT + "(DoublePrimitive in [0:na.DoublePrimitive])");
            testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "() -> every nb=" + EVENT + "(DoublePrimitive in (0:na.DoublePrimitive))");
            testCase.Add("N4", "na", events.GetEvent("N1"), "nb", events.GetEvent("N4"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "() -> every nb=" + EVENT + "(IntPrimitive in (na.IntPrimitive:na.DoublePrimitive))");
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("na=" + EVENT + "() -> every nb=" + EVENT + "(IntPrimitive in (na.IntPrimitive:60))");
            testCase.Add("N6", "na", events.GetEvent("N1"), "nb", events.GetEvent("N6"));
            testCase.Add("N7", "na", events.GetEvent("N1"), "nb", events.GetEvent("N7"));
            testCaseList.AddTest(testCase);

            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        [Test]
        public void TestObjectId()
        {
            string EVENT = typeof(SupportBean_S0).FullName;
    
            EventCollection events = EventCollectionFactory.GetSetFourExternalClock(0, 1000);
            CaseList testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("X1=" + EVENT + "() -> X2=" + EVENT + "(p00=X1.P00)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("X1=" + EVENT + "(p00='B') -> X2=" + EVENT + "(p00=X1.P00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("X1=" + EVENT + "(p00='B') -> every X2=" + EVENT + "(p00=X1.P00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + EVENT + "(p00='B') -> every X2=" + EVENT + "(p00=X1.P00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + EVENT + "() -> X2=" + EVENT + "(p00=X1.P00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
            testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every X1=" + EVENT + "() -> every X2=" + EVENT + "(p00=X1.P00)");
            testCase.Add("e6", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e6"));
            testCase.Add("e8", "X1", events.GetEvent("e3"), "X2", events.GetEvent("e8"));
            testCase.Add("e10", "X1", events.GetEvent("e9"), "X2", events.GetEvent("e10"));
            testCase.Add("e11", "X1", events.GetEvent("e2"), "X2", events.GetEvent("e11"));
            testCase.Add("e11", "X1", events.GetEvent("e6"), "X2", events.GetEvent("e11"));
            testCase.Add("e12", "X1", events.GetEvent("e7"), "X2", events.GetEvent("e12"));
            testCaseList.AddTest(testCase);

            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        [Test]
        public void TestFollowedByFilter()
        {
            // Test for ESPER-121
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportTradeEvent>("FxTradeEvent");
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(
                    "testRFIDZoneEnter", config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            String expression = "every tradeevent1=FxTradeEvent(userId in ('U1000','U1001','U1002') ) -> " +
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
            MyUpdateListener listener = new MyUpdateListener();
            statement.Events += (sender, e) => listener.Update(e.NewEvents, e.OldEvents);
    
            Random random = new Random();
            String[] users = {"U1000", "U1001", "U1002"};
            String[] ccy = {"USD", "JPY", "EUR"};
            String[] direction = {"B", "S"};
    
            for (int i = 0; i < 100; i++)
            {
                SupportTradeEvent theEvent = new SupportTradeEvent(
                    i,
                    users[random.Next(0, users.Length)],
                    ccy[random.Next(0, ccy.Length)],
                    direction[random.Next(0, direction.Length)]);
                epService.EPRuntime.SendEvent(theEvent);
            }
    
            Assert.AreEqual(0, listener.BadMatchCount);
    
            epService.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        public class MyUpdateListener
        {
            public int BadMatchCount { get; private set; }

            public int GoodMatchCount { get; private set; }

            public void Update(EventBean[] newEvents, EventBean[]
                    oldEvents)
            {
                if (newEvents != null)
                {
                    foreach (EventBean eventBean in newEvents)
                    {
                        HandleEvent(eventBean);
                    }
                }
            }
    
            private void HandleEvent(EventBean eventBean)
            {
                SupportTradeEvent tradeevent1 = (SupportTradeEvent)
                        eventBean.Get("tradeevent1");
                SupportTradeEvent tradeevent2 = (SupportTradeEvent)
                        eventBean.Get("tradeevent2");
                SupportTradeEvent tradeevent3 = (SupportTradeEvent)
                        eventBean.Get("tradeevent3");
    
                if ((
                        tradeevent1.UserId.Equals(tradeevent2.UserId) ||
                                tradeevent1.UserId.Equals(tradeevent3.UserId) ||
                                tradeevent2.UserId.Equals(tradeevent3.UserId)))
                {
                    /*
                    Console.Out.WriteLine("Bad Match : ");
                    Console.Out.WriteLine(tradeevent1);
                    Console.Out.WriteLine(tradeevent2);
                    Console.Out.WriteLine(tradeevent3 + "\n");
                    */
                    BadMatchCount++;
                }
                else
                {
                    GoodMatchCount++;
                }
            }
        }
    }
}
