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
    public class PatternOperatorOperatorMix : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;

            testCase = new EventExpressionCase(
                "(b=SupportBean_B -> d=SupportBean_D) " +
                " and " +
                "(a=SupportBean_A -> e=SupportBean_E)"
            );
            testCase.Add(
                "E1",
                "b",
                events.GetEvent("B1"),
                "d",
                events.GetEvent("D1"),
                "a",
                events.GetEvent("A1"),
                "e",
                events.GetEvent("E1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("b=SupportBean_B -> (d=SupportBean_D() or a=SupportBean_A)");
            testCase.Add("A2", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);

            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.FollowedBy(
                Patterns.Filter("SupportBean_B", "b"),
                Patterns.Or(Patterns.Filter("SupportBean_D", "d"), Patterns.Filter("SupportBean_A", "a")));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = env.CopyMayFail(model);
            var text = "select * from pattern [b=SupportBean_B -> d=SupportBean_D or a=SupportBean_A]";
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("A2", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "b=SupportBean_B() -> (" +
                "(d=SupportBean_D() -> a=SupportBean_A())" +
                " or " +
                "(a=SupportBean_A() -> e=SupportBean_E()))"
            );
            testCase.Add("E1", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"), "e", events.GetEvent("E1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("b=SupportBean_B() and d=SupportBean_D or a=SupportBean_A");
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase("(b=SupportBean_B() -> d=SupportBean_D()) or a=SupportBean_A");
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);

            testCase = new EventExpressionCase(
                "(b=SupportBean_B() and " +
                "d=SupportBean_D()) or " +
                "a=SupportBean_A");
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);

            var util = new PatternTestHarness(events, testCaseList);
            util.RunTest(env);
        }
    }
} // end of namespace