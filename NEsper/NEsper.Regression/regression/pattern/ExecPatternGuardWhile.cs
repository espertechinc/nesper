///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternGuardWhile : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionOp(epService);
            RunAssertionVariable(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("a=A -> (every b=B) While(b.id != 'B2')");
            testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("a=A -> (every b=B) While(b.id != 'B3')");
            testCase.Add("B1", "a", events.GetEvent("A1"), "b", events.GetEvent("B1"));
            testCase.Add("B2", "a", events.GetEvent("A1"), "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=B) While(b.id != 'B3')");
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            string text = "select * from pattern [(every b=" + EVENT_B_CLASS + ") while (b.id!=\"B3\")]";
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Expression guardExpr = Expressions.Neq("b.id", "B3");
            PatternExpr every = Patterns.Every(Patterns.Filter(Filter.Create(EVENT_B_CLASS), "b"));
            PatternExpr patternGuarded = Patterns.WhileGuard(every, guardExpr);
            model.FromClause = FromClause.Create(PatternStream.Create(patternGuarded));
            Assert.AreEqual(text, model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("B1", "b", events.GetEvent("B1"));
            testCase.Add("B2", "b", events.GetEvent("B2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("(every b=B) While(b.id != 'B1')");
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionVariable(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("myVariable", "bool", true);
    
            string expression =
                    "select * from pattern [every a=SupportBean(TheString like 'A%') -> (every b=SupportBean(TheString like 'B%')) while (myVariable)]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 100));
            Assert.AreEqual(2, listener.GetAndResetLastNewData().Length);
    
            epService.EPRuntime.SetVariableValue("myVariable", false);
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 3));
            epService.EPRuntime.SendEvent(new SupportBean("A4", 4));
            epService.EPRuntime.SendEvent(new SupportBean("B2", 200));
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from pattern [every SupportBean while ('abc')]",
                    "Invalid parameter for pattern guard 'SupportBean while (\"abc\")': Expression pattern guard requires a single expression as a parameter returning a true or false (bool) value [select * from pattern [every SupportBean while ('abc')]]");
            TryInvalid(epService, "select * from pattern [every SupportBean while (abc)]",
                    "Failed to validate pattern guard expression 'abc': Property named 'abc' is not valid in any stream [select * from pattern [every SupportBean while (abc)]]");
        }
    }
} // end of namespace
