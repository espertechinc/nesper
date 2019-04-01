///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorOperatorMix : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + ") " +
                    " and " +
                    "(a=" + EVENT_A_CLASS + " -> e=" + EVENT_E_CLASS + ")"
            );
            testCase.Add("E1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"), "a", events.GetEvent("A1"), "e", events.GetEvent("E1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + " -> (d=" + EVENT_D_CLASS + "() or a=" + EVENT_A_CLASS + ")");
            testCase.Add("A2", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.FollowedBy(
                    Patterns.Filter(EVENT_B_CLASS, "b"),
                    Patterns.Or(Patterns.Filter(EVENT_D_CLASS, "d"), Patterns.Filter(EVENT_A_CLASS, "a")));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            string text = "select * from pattern [b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + " or a=" + EVENT_A_CLASS + "]";
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("A2", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() -> (" +
                    "(d=" + EVENT_D_CLASS + "() -> a=" + EVENT_A_CLASS + "())" +
                    " or " +
                    "(a=" + EVENT_A_CLASS + "() -> e=" + EVENT_E_CLASS + "()))"
            );
            testCase.Add("E1", "b", events.GetEvent("B1"), "a", events.GetEvent("A2"), "e", events.GetEvent("E1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "() and d=" +
                    EVENT_D_CLASS + "() or a=" +
                    EVENT_A_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + "() -> d=" + EVENT_D_CLASS + "()) or a=" + EVENT_A_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(b=" + EVENT_B_CLASS + "() and " +
                    "d=" + EVENT_D_CLASS + "()) or " +
                    "a=" + EVENT_A_CLASS);
            testCase.Add("A1", "a", events.GetEvent("A1"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    }
} // end of namespace
