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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPatternEventProperties  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }
    
        [Test]
        public void TestWildcardSimplePattern() {
            SetupSimplePattern("*");
    
            Object theEvent = new SupportBean();
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
        }
    
        [Test]
        public void TestWildcardOrPattern() {
            SetupOrPattern("*");
    
            Object theEvent = new SupportBean();
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
            Assert.IsNull(eventBean.Get("b"));
    
            theEvent = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(theEvent);
            eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("b"));
            Assert.IsNull(eventBean.Get("a"));
        }
    
        [Test]
        public void TestPropertiesSimplePattern() {
            SetupSimplePattern("a, a as myEvent, a.IntPrimitive as MyInt, a.TheString");
    
            SupportBean theEvent = new SupportBean();
            theEvent.IntPrimitive = 1;
            theEvent.TheString = "test";
            _epService.EPRuntime.SendEvent(theEvent);
    
            EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
            Assert.AreSame(theEvent, eventBean.Get("myEvent"));
            Assert.AreEqual(1, eventBean.Get("MyInt"));
            Assert.AreEqual("test", eventBean.Get("a.TheString"));
        }
    
        [Test]
        public void TestPropertiesOrPattern() {
            SetupOrPattern("a, a as myAEvent, b, b as myBEvent, a.IntPrimitive as MyInt, " +
                    "a.TheString, b.SimpleProperty as Simple, b.Indexed[0] as Indexed, b.Nested.NestedValue as NestedVal");
    
            Object theEvent = SupportBeanComplexProps.MakeDefaultBean();
            _epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("b"));
            Assert.AreEqual("Simple", eventBean.Get("Simple"));
            Assert.AreEqual(1, eventBean.Get("Indexed"));
            Assert.AreEqual("NestedValue", eventBean.Get("NestedVal"));
            Assert.IsNull(eventBean.Get("a"));
            Assert.IsNull(eventBean.Get("myAEvent"));
            Assert.IsNull(eventBean.Get("MyInt"));
            Assert.IsNull(eventBean.Get("a.TheString"));
    
            SupportBean eventTwo = new SupportBean();
            eventTwo.IntPrimitive = 2;
            eventTwo.TheString = "test2";
            _epService.EPRuntime.SendEvent(eventTwo);
            eventBean = _updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, eventBean.Get("MyInt"));
            Assert.AreEqual("test2", eventBean.Get("a.TheString"));
            Assert.IsNull(eventBean.Get("b"));
            Assert.IsNull(eventBean.Get("myBEvent"));
            Assert.IsNull(eventBean.Get("Simple"));
            Assert.IsNull(eventBean.Get("Indexed"));
            Assert.IsNull(eventBean.Get("NestedVal"));
        }
    
        private void SetupSimplePattern(String selectCriteria) {
            String stmtText = "select " + selectCriteria + " from pattern [a=" + typeof(SupportBean).FullName + "]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _updateListener.Update;
        }
    
        private void SetupOrPattern(String selectCriteria) {
            String stmtText = "select " + selectCriteria + " from pattern [every(a=" + typeof(SupportBean).FullName +
                    " or b=" + typeof(SupportBeanComplexProps).FullName + ")]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _updateListener.Update;
        }
    }
}
