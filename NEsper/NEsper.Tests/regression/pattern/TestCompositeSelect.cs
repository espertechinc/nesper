///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestCompositeSelect
    {
        [Test]
        public void TestFollowedByFilter()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("A", typeof(SupportBean_A).FullName);
            config.AddEventType("B", typeof(SupportBean_B).FullName);
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var stmtTxtOne = "insert into StreamOne select * from pattern [a=A -> b=B]";
            epService.EPAdministrator.CreateEPL(stmtTxtOne);
    
            var listener = new SupportUpdateListener();
            var stmtTxtTwo = "select *, 1 as code from StreamOne";
            var stmtTwo = epService.EPAdministrator.CreateEPL(stmtTxtTwo);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            var theEvent = listener.AssertOneGetNewAndReset();
    
            var values = new Object[stmtTwo.EventType.PropertyNames.Length];
            var count = 0;
            foreach (var name in stmtTwo.EventType.PropertyNames) {
                values[count++] = theEvent.Get(name);
            }
    
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("a", typeof(SupportBean_A), null, false, false, false, false, true),
                    new EventPropertyDescriptor("b", typeof(SupportBean_B), null, false, false, false, false, true)
            }, ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("StreamOne").PropertyDescriptors);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestFragment()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("A", typeof(SupportBean_A).FullName);
            config.AddEventType("B", typeof(SupportBean_B).FullName);
    
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

    
            var stmtTxtOne = "select * from pattern [[2] a=A -> b=B]";
            var stmt = epService.EPAdministrator.CreateEPL(stmtTxtOne);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("a", typeof(SupportBean_A[]), typeof(SupportBean_A), false, false, true, false, true),
                    new EventPropertyDescriptor("b", typeof(SupportBean_B), null, false, false, false, false, true)
            }, stmt.EventType.PropertyDescriptors);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
    
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent.Underlying is IDictionary<string,object>);
    
            // test fragment B type and theEvent
            var typeFragB = theEvent.EventType.GetFragmentType("b");
            Assert.IsFalse(typeFragB.IsIndexed);
            Assert.AreEqual("B", typeFragB.FragmentType.Name);
            Assert.AreEqual(typeof(string), typeFragB.FragmentType.GetPropertyType("id"));
    
            var eventFragB = (EventBean) theEvent.GetFragment("b");
            Assert.AreEqual("B", eventFragB.EventType.Name);
    
            // test fragment A type and event
            var typeFragA = theEvent.EventType.GetFragmentType("a");
            Assert.IsTrue(typeFragA.IsIndexed);
            Assert.AreEqual("A", typeFragA.FragmentType.Name);
            Assert.AreEqual(typeof(string), typeFragA.FragmentType.GetPropertyType("id"));
    
            Assert.IsTrue(theEvent.GetFragment("a") is EventBean[]);
            var eventFragA1 = (EventBean) theEvent.GetFragment("a[0]");
            Assert.AreEqual("A", eventFragA1.EventType.Name);
            Assert.AreEqual("A1", eventFragA1.Get("id"));
            var eventFragA2 = (EventBean) theEvent.GetFragment("a[1]");
            Assert.AreEqual("A", eventFragA2.EventType.Name);
            Assert.AreEqual("A2", eventFragA2.Get("id"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
