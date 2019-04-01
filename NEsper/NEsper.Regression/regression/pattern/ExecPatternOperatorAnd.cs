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

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorAnd : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOp(epService);
            RunAssertionAndNotDefaultTrue(epService);
            RunAssertionAndWithEveryAndTerminationOptimization(epService);
        }
    
        private void RunAssertionOp(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetEventSetOne(0, 1000);
            var testCaseList = new CaseList();
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
    
            testCase = new EventExpressionCase("Every(b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.Every(Patterns.And(Patterns.Filter(EVENT_B_CLASS, "b"), Patterns.Filter(EVENT_D_CLASS, "d")));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual("select * from pattern [every (b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")]", model.ToEPL());
            testCase = new EventExpressionCase(model);
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("Every( b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS + ")");
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
    
            testCase = new EventExpressionCase("Every( every b=" + EVENT_B_CLASS + " and d=" + EVENT_D_CLASS + ")");
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
    
            testCase = new EventExpressionCase("Every( every b=" + EVENT_B_CLASS + " and every d=" + EVENT_D_CLASS + ")");
            testCase.Add("D1", "b", events.GetEvent("B1"), "d", events.GetEvent("D1"));
            testCase.Add("D1", "b", events.GetEvent("B2"), "d", events.GetEvent("D1"));
            testCase.Add("D2", "b", events.GetEvent("B1"), "d", events.GetEvent("D2"));
            testCase.Add("D2", "b", events.GetEvent("B2"), "d", events.GetEvent("D2"));
            testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D1"));
            for (int i = 0; i < 3; i++) {
                testCase.Add("B3", "b", events.GetEvent("B3"), "d", events.GetEvent("D2"));
            }
            testCase.Add("D3", "b", events.GetEvent("B1"), "d", events.GetEvent("D3"));
            testCase.Add("D3", "b", events.GetEvent("B2"), "d", events.GetEvent("D3"));
            for (int i = 0; i < 5; i++) {
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
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionAndNotDefaultTrue(EPServiceProvider epService) {
    
            // ESPER-402
            epService.EPAdministrator.Configuration.AddEventType("CallWaiting", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("CallFinished", typeof(SupportBean_B));
            epService.EPAdministrator.Configuration.AddEventType("CallPickedUp", typeof(SupportBean_C));
            string pattern =
                    " insert into NumberOfWaitingCalls(calls) " +
                            " select count(*)" +
                            " from pattern[every call=CallWaiting ->" +
                            " (not CallFinished(id=call.id) and" +
                            " not CallPickedUp(id=call.id))]";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAndWithEveryAndTerminationOptimization(EPServiceProvider epService) {
            // When all other sub-expressions to an AND are gone,
            // then there is no need to retain events of the subexpression still active
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_B));
    
            string epl = "select * from pattern [a=SupportBean_A and every b=SupportBean_B]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            for (int i = 0; i < 10; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_B("B" + i));
            }
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_B("B_last"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a.id,b.id".Split(','), new object[]{"A1", "B_last"});
    
            stmt.Dispose();
        }
    }
} // end of namespace
