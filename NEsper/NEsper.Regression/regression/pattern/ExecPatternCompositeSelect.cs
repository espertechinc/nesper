///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternCompositeSelect : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("A", typeof(SupportBean_A));
            configuration.AddEventType("B", typeof(SupportBean_B));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFollowedByFilter(epService);
            RunAssertionFragment(epService);
        }
    
        private void RunAssertionFollowedByFilter(EPServiceProvider epService) {
            string stmtTxtOne = "insert into StreamOne select * from pattern [a=A -> b=B]";
            epService.EPAdministrator.CreateEPL(stmtTxtOne);
    
            var listener = new SupportUpdateListener();
            string stmtTxtTwo = "select *, 1 as code from StreamOne";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtTxtTwo);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
    
            var values = new Object[stmtTwo.EventType.PropertyNames.Length];
            int count = 0;
            foreach (string name in stmtTwo.EventType.PropertyNames) {
                values[count++] = theEvent.Get(name);
            }
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("a", typeof(SupportBean_A), null, false, false, false, false, true),
                    new EventPropertyDescriptor("b", typeof(SupportBean_B), null, false, false, false, false, true)
            }, ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("StreamOne").PropertyDescriptors);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFragment(EPServiceProvider epService) {
            string stmtTxtOne = "select * from pattern [[2] a=A -> b=B]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTxtOne);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("a", typeof(SupportBean_A[]), typeof(SupportBean_A), false, false, true, false, true),
                    new EventPropertyDescriptor("b", typeof(SupportBean_B), null, false, false, false, false, true)
            }, stmt.EventType.PropertyDescriptors);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.IsTrue(theEvent.Underlying is IDictionary<string, object>);
    
            // test fragment B type and event
            FragmentEventType typeFragB = theEvent.EventType.GetFragmentType("b");
            Assert.IsFalse(typeFragB.IsIndexed);
            Assert.AreEqual("B", typeFragB.FragmentType.Name);
            Assert.AreEqual(typeof(string), typeFragB.FragmentType.GetPropertyType("id"));
    
            EventBean eventFragB = (EventBean) theEvent.GetFragment("b");
            Assert.AreEqual("B", eventFragB.EventType.Name);
    
            // test fragment A type and event
            FragmentEventType typeFragA = theEvent.EventType.GetFragmentType("a");
            Assert.IsTrue(typeFragA.IsIndexed);
            Assert.AreEqual("A", typeFragA.FragmentType.Name);
            Assert.AreEqual(typeof(string), typeFragA.FragmentType.GetPropertyType("id"));
    
            Assert.IsTrue(theEvent.GetFragment("a") is EventBean[]);
            EventBean eventFragA1 = (EventBean) theEvent.GetFragment("a[0]");
            Assert.AreEqual("A", eventFragA1.EventType.Name);
            Assert.AreEqual("A1", eventFragA1.Get("id"));
            EventBean eventFragA2 = (EventBean) theEvent.GetFragment("a[1]");
            Assert.AreEqual("A", eventFragA2.EventType.Name);
            Assert.AreEqual("A2", eventFragA2.Get("id"));
    
            stmt.Dispose();
        }
    }
} // end of namespace
