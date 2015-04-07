///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestAndOperator : SupportBeanConstants
    {
        [Test]
        public void TestOp()
        {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            CaseList testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every(b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.Every(Patterns.And(Patterns.Filter(EVENT_B_CLASS, "b"), Patterns.Filter(EVENT_D_CLASS, "d")));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual("select * from pattern [every (b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")]", model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every( b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every( every b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + EVENT_A_CLASS + " and d=" + EVENT_D_CLASS + " and b=" + EVENT_B_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every( every b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            for (int i = 0; i < 3; i++)
            {
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            }
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            for (int i = 0; i < 5; i++)
            {
                testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            }
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("a=" + EVENT_A_CLASS + " and d=" + EVENT_D_CLASS + " and b=" + EVENT_B_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + EVENT_A_CLASS + " and every d=" + EVENT_D_CLASS + " and b=" + EVENT_B_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " and b=" + EVENT_B_CLASS);
            testCase.Add("B1", "b", events.GetEvent("B1"), "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every a=" + EVENT_A_CLASS + " and every d=" + EVENT_D_CLASS + " and every b=" + EVENT_B_CLASS);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"), "a", events.GetEvent("A2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"), "a", events.GetEvent("A2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (a=" + EVENT_A_CLASS + " and every d=" + EVENT_D_CLASS + " and b=" + EVENT_B_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"), "a", events.GetEvent("A1"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"), "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " and b=" + EVENT_B_CLASS + ")");
            testCase.Add("B1", "b", events.GetEvent("B1"), "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"), "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            PatternTestHarness util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        [Test]
        public void TestAndNotDefaultTrue()
        {
            // ESPER-402
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            SupportUpdateListener listener = new SupportUpdateListener();
            engine.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(engine, GetType(), GetType().FullName); }

            engine.EPAdministrator.Configuration.AddEventType("CallWaiting", typeof(SupportBean_A));
            engine.EPAdministrator.Configuration.AddEventType("CallFinished", typeof(SupportBean_B));
            engine.EPAdministrator.Configuration.AddEventType("CallPickedUp", typeof(SupportBean_C));
            String pattern =
                    " insert into NumberOfWaitingCalls(calls) " +
                    " select count(*)" +
                    " from pattern[every call=CallWaiting ->" + 
                            " (not CallFinished(id=call.id) and" +
                            " not CallPickedUp(id=call.id))]";
            engine.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
            engine.EPRuntime.SendEvent(new SupportBean_A("A1"));
            engine.EPRuntime.SendEvent(new SupportBean_B("B1"));
            engine.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(listener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }

        }
    }
}
