///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternComplexPropertyAccess : RegressionExecution {
        private static readonly string EVENT_COMPLEX = typeof(SupportBeanComplexProps).FullName;
        private static readonly string EVENT_NESTED = typeof(SupportBeanCombinedProps).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionComplexProperties(epService);
            RunAssertionIndexedFilterProp(epService);
            RunAssertionIndexedValueProp(epService);
            RunAssertionIndexedValuePropOM(epService);
            RunAssertionIndexedValuePropCompile(epService);
        }
    
        private void RunAssertionComplexProperties(EPServiceProvider epService) {
            EventCollection events = EventCollectionFactory.GetSetSixComplexProperties();
            var testCaseList = new CaseList();
            EventExpressionCase testCase;
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Mapped('keyOne') = 'valueOne')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(indexed[1] = 2)");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(indexed[0] = 2)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(arrayProperty[1] = 20)");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(arrayProperty[1] in (10:30))");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(arrayProperty[2] = 20)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(nested.nestedValue = 'nestedValue')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(nested.nestedValue = 'dummy')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(nested.nestedNested.nestedNestedValue = 'nestedNestedValue')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(nested.nestedNested.nestedNestedValue = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(indexed[1].Mapped('1mb').value = '1ma1')");
            testCase.Add("e2", "s", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(indexed[0].Mapped('1ma').value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(array[0].Mapped('0ma').value = '0ma0')");
            testCase.Add("e2", "s", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(array[2].Mapped('x').value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(array[879787].Mapped('x').value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(array[0].Mapped('xxx').value = 'x')");
            testCaseList.AddTest(testCase);
    
            var util = new PatternTestHarness(events, testCaseList, this.GetType());
            util.RunTest(epService);
        }
    
        private void RunAssertionIndexedFilterProp(EPServiceProvider epService) {
            var testListener = new SupportUpdateListener();
            string type = typeof(SupportBeanComplexProps).FullName;
            string pattern = "every a=" + type + "(indexed[0]=3)";
    
            EPStatement stmt = epService.EPAdministrator.CreatePattern(pattern);
            stmt.Events += testListener.Update;
    
            var theEvent = new SupportBeanComplexProps(new int[]{3, 4});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("a"));
    
            theEvent = new SupportBeanComplexProps(new int[]{6});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(testListener.IsInvoked);
    
            theEvent = new SupportBeanComplexProps(new int[]{3});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("a"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionIndexedValueProp(EPServiceProvider epService) {
            string type = typeof(SupportBeanComplexProps).FullName;
            string pattern = "every a=" + type + " -> b=" + type + "(indexed[0] = a.indexed[0])";
    
            EPStatement stmt = epService.EPAdministrator.CreatePattern(pattern);
            RunIndexedValueProp(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertionIndexedValuePropOM(EPServiceProvider epService) {
            string type = typeof(SupportBeanComplexProps).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.FollowedBy(Patterns.EveryFilter(type, "a"),
                    Patterns.Filter(Filter.Create(type, Expressions.EqProperty("indexed[0]", "a.indexed[0]")), "b"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            string patternText = "select * from pattern [every a=" + type + " -> b=" + type + "(indexed[0]=a.indexed[0])]";
            Assert.AreEqual(patternText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RunIndexedValueProp(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertionIndexedValuePropCompile(EPServiceProvider epService) {
            string type = typeof(SupportBeanComplexProps).FullName;
    
            string patternText = "select * from pattern [every a=" + type + " -> b=" + type + "(indexed[0]=a.indexed[0])]";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(patternText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(patternText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RunIndexedValueProp(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunIndexedValueProp(EPServiceProvider epService, EPStatement stmt) {
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            var eventOne = new SupportBeanComplexProps(new int[]{3});
            epService.EPRuntime.SendEvent(eventOne);
            Assert.IsFalse(testListener.IsInvoked);
    
            var theEvent = new SupportBeanComplexProps(new int[]{6});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(testListener.IsInvoked);
    
            var eventTwo = new SupportBeanComplexProps(new int[]{3});
            epService.EPRuntime.SendEvent(eventTwo);
            EventBean eventBean = testListener.AssertOneGetNewAndReset();
            Assert.AreSame(eventOne, eventBean.Get("a"));
            Assert.AreSame(eventTwo, eventBean.Get("b"));
        }
    }
    
    
} // end of namespace
