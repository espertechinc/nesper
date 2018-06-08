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
    public class ExecPatternGuardTimerWithinOrMax : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:withinmax(2 sec,100)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:withinmax(2001 msec,1)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("b=" + EVENT_B_CLASS + "(id=\"B1\") where timer:withinmax(1999 msec,10)");
            testCaseList.AddTest(testCase);
    
            string text = "select * from pattern [b=" + EVENT_B_CLASS + "(id=\"B3\") where timer:withinmax(10.001,1)]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Expression filter = Expressions.Eq("id", "B3");
            PatternExpr pattern = Patterns.TimerWithinMax(10.001, 1, Patterns.Filter(Filter.Create(EVENT_B_CLASS, filter), "b"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:withinmax(4.001, 0)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:withinmax(4.001, 1)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=" + EVENT_B_CLASS + ") where timer:withinmax(4.001, 2)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " where timer:withinmax(2.001, 4)");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            // Note how every restarts the max
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " where timer:withinmax(2001 msec, 2))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " where timer:withinmax(2001 msec, 3))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " where timer:withinmax(2001 msec, 1))");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCase.Add("B3", "b", events.GetEvent("B3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every (b=" + EVENT_B_CLASS + " where timer:withinmax(2001 msec, 0))");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + " -> d=" + EVENT_D_CLASS + " where timer:withinmax(4000 msec, 1)");
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> every d=" + EVENT_D_CLASS + " where timer:withinmax(4000 msec, 1)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> (every d=" + EVENT_D_CLASS + ") where timer:withinmax(1 day, 3)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> (every d=" + EVENT_D_CLASS + ") where timer:withinmax(1 day, 2)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("every b=" + EVENT_B_CLASS + "() -> (every d=" + EVENT_D_CLASS + ") where timer:withinmax(1 day, 1)");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D3", "b", events.GetEvent("B3"), "d", events.GetEvent("D3"));
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    }
} // end of namespace
