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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestComplexPropertyAccess 
    {
        private static readonly string EVENT_COMPLEX = typeof(SupportBeanComplexProps).FullName;
        private static readonly string EVENT_NESTED = typeof(SupportBeanCombinedProps).FullName;
    
        [Test]
        public void TestComplexProperties()
        {
            var events = EventCollectionFactory.GetSetSixComplexProperties();
            var testCaseList = new CaseList();
            EventExpressionCase testCase = null;
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Mapped('keyOne') = 'valueOne')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Indexed[1] = 2)");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Indexed[0] = 2)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(ArrayProperty[1] = 20)");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(ArrayProperty[1] in (10:30))");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(ArrayProperty[2] = 20)");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Nested.NestedValue = 'NestedValue')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Nested.NestedValue = 'dummy')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Nested.NestedNested.NestedNestedValue = 'NestedNestedValue')");
            testCase.Add("e1", "s", events.GetEvent("e1"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_COMPLEX + "(Nested.NestedNested.NestedNestedValue = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Indexed[1].Mapped('1mb').Value = '1ma1')");
            testCase.Add("e2", "s", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Indexed[0].Mapped('1ma').Value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Array[0].Mapped('0ma').Value = '0ma0')");
            testCase.Add("e2", "s", events.GetEvent("e2"));
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Array[2].Mapped('x').Value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Array[879787].Mapped('x').Value = 'x')");
            testCaseList.AddTest(testCase);
    
            testCase = new EventExpressionCase("s=" + EVENT_NESTED + "(Array[0].Mapped('xxx').Value = 'x')");
            testCaseList.AddTest(testCase);

            var util = new PatternTestHarness(events, testCaseList, GetType(), GetType().FullName);
            util.RunTest();
        }
    
        [Test]
        public void TestIndexedFilterProp()
        {
            var testListener = new SupportUpdateListener();
            var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var type = typeof(SupportBeanComplexProps).FullName;
            var pattern = "every a=" + type + "(Indexed[0]=3)";
    
            var stmt = epService.EPAdministrator.CreatePattern(pattern);
            stmt.Events += testListener.Update;
    
            Object theEvent = new SupportBeanComplexProps(new int[] { 3, 4});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("a"));
    
            theEvent = new SupportBeanComplexProps(new int[] { 6});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(testListener.IsInvoked);
    
            theEvent = new SupportBeanComplexProps(new int[] { 3});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreSame(theEvent, testListener.AssertOneGetNewAndReset().Get("a"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIndexedValueProp()
        {
            var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var type = typeof(SupportBeanComplexProps).FullName;
            var pattern = "every a=" + type + " -> b=" + type + "(Indexed[0] = a.Indexed[0])";
    
            var stmt = epService.EPAdministrator.CreatePattern(pattern);
            RunIndexedValueProp(epService, stmt);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIndexedValuePropOM()
        {
            var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            
            var type = typeof(SupportBeanComplexProps).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            PatternExpr pattern = Patterns.FollowedBy(Patterns.EveryFilter(type, "a"),
                    Patterns.Filter(Filter.Create(type, Expressions.EqProperty("Indexed[0]", "a.Indexed[0]")), "b"));
            model.FromClause = FromClause.Create(PatternStream.Create(pattern));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            var patternText = "select * from pattern [every a=" + type + " -> b=" + type + "(Indexed[0]=a.Indexed[0])]";
            Assert.AreEqual(patternText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            RunIndexedValueProp(epService, stmt);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestIndexedValuePropCompile()
        {
            var epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
            
            var type = typeof(SupportBeanComplexProps).FullName;

            var patternText = "select * from pattern [every a=" + type + " -> b=" + type + "(Indexed[0]=a.Indexed[0])]";
            var model = epService.EPAdministrator.CompileEPL(patternText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(patternText, model.ToEPL());
    
            var stmt = epService.EPAdministrator.Create(model);
            RunIndexedValueProp(epService, stmt);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunIndexedValueProp(EPServiceProvider epService, EPStatement stmt)
        {
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            Object eventOne = new SupportBeanComplexProps(new int[] {3});
            epService.EPRuntime.SendEvent(eventOne);
            Assert.IsFalse(testListener.IsInvoked);
    
            Object theEvent = new SupportBeanComplexProps(new int[] { 6});
            epService.EPRuntime.SendEvent(theEvent);
            Assert.IsFalse(testListener.IsInvoked);
    
            Object eventTwo = new SupportBeanComplexProps(new int[] { 3});
            epService.EPRuntime.SendEvent(eventTwo);
            var eventBean = testListener.AssertOneGetNewAndReset();
            Assert.AreSame(eventOne, eventBean.Get("a"));
            Assert.AreSame(eventTwo, eventBean.Get("b"));
        }
    }
}