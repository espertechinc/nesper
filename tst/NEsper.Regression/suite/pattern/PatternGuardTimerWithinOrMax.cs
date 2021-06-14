///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternGuardTimerWithinOrMax : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;

            testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:withinmax(2 sec,100)");
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:withinmax(2001 msec,1)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("b=SupportBean_B(Id='B1') where timer:withinmax(1999 msec,10)");
            testCaseList.AddTest(testCase);

            var text = "select * from pattern [b=SupportBean_B(Id='B3') where timer:withinmax(10.001d,1)]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model = env.CopyMayFail(model);
            Expression filter = Expressions.Eq("Id", "B3");
            PatternExpr pattern = Patterns.TimerWithinMax(
                10.001,
                1,
                Patterns.Filter(Filter.Create("SupportBean_B", filter), "b"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            Assert.AreEqual(text, model.ToEPL().Replace("\"", "'"));
            testCase = new EventExpressionCase(model);
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:withinmax(4.001, 0)");
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:withinmax(4.001, 1)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("(every b=SupportBean_B) where timer:withinmax(4.001, 2)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("every b=SupportBean_B where timer:withinmax(2.001, 4)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            // Note how every restarts the max
            testCase = new EventExpressionCase("every (b=SupportBean_B where timer:withinmax(2001 msec, 2))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("every (b=SupportBean_B where timer:withinmax(2001 msec, 3))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("every (b=SupportBean_B where timer:withinmax(2001 msec, 1))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("every (b=SupportBean_B where timer:withinmax(2001 msec, 0))");
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "every b=SupportBean_B -> d=SupportBean_D where timer:withinmax(4000 msec, 1)");
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "every b=SupportBean_B() -> every d=SupportBean_D where timer:withinmax(4000 msec, 1)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "every b=SupportBean_B() -> (every d=SupportBean_D) where timer:withinmax(1 day, 3)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "every b=SupportBean_B() -> (every d=SupportBean_D) where timer:withinmax(1 day, 2)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "every b=SupportBean_B() -> (every d=SupportBean_D) where timer:withinmax(1 day, 1)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);

            var util = new PatternTestHarness(events, testCaseList, GetType());
            util.RunTest(env);
        }
    }
} // end of namespace