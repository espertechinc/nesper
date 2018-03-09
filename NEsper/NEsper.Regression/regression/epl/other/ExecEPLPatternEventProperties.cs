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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLPatternEventProperties : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionWildcardSimplePattern(epService);
            RunAssertionWildcardOrPattern(epService);
            RunAssertionPropertiesSimplePattern(epService);
            RunAssertionPropertiesOrPattern(epService);
        }
    
        private void RunAssertionWildcardSimplePattern(EPServiceProvider epService) {
            var updateListener = SetupSimplePattern(epService, "*");
    
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
    
            var eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWildcardOrPattern(EPServiceProvider epService) {
            var updateListener = SetupOrPattern(epService, "*");
    
            object theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            var eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
            Assert.IsNull(eventBean.Get("b"));
    
            theEvent = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(theEvent);
            eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("b"));
            Assert.IsNull(eventBean.Get("a"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPropertiesSimplePattern(EPServiceProvider epService) {
            var updateListener = SetupSimplePattern(epService, "a, a as myEvent, a.IntPrimitive as myInt, a.TheString");
    
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = 1;
            theEvent.TheString = "test";
            epService.EPRuntime.SendEvent(theEvent);
    
            var eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("a"));
            Assert.AreSame(theEvent, eventBean.Get("myEvent"));
            Assert.AreEqual(1, eventBean.Get("myInt"));
            Assert.AreEqual("test", eventBean.Get("a.TheString"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPropertiesOrPattern(EPServiceProvider epService) {
            var updateListener = SetupOrPattern(epService,
                "a, a as myAEvent, b, b as myBEvent, a.IntPrimitive as myInt, " +
                "a.TheString, b.SimpleProperty as Simple, b.Indexed[0] as Indexed, b.Nested.NestedValue as NestedVal");
    
            Object theEvent = SupportBeanComplexProps.MakeDefaultBean();
            epService.EPRuntime.SendEvent(theEvent);
            var eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Get("b"));
            Assert.AreEqual("Simple", eventBean.Get("Simple"));
            Assert.AreEqual(1, eventBean.Get("Indexed"));
            Assert.AreEqual("NestedValue", eventBean.Get("NestedVal"));
            Assert.IsNull(eventBean.Get("a"));
            Assert.IsNull(eventBean.Get("myAEvent"));
            Assert.IsNull(eventBean.Get("myInt"));
            Assert.IsNull(eventBean.Get("a.TheString"));
    
            var eventTwo = new SupportBean();
            eventTwo.IntPrimitive = 2;
            eventTwo.TheString = "test2";
            epService.EPRuntime.SendEvent(eventTwo);
            eventBean = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, eventBean.Get("myInt"));
            Assert.AreEqual("test2", eventBean.Get("a.TheString"));
            Assert.IsNull(eventBean.Get("b"));
            Assert.IsNull(eventBean.Get("myBEvent"));
            Assert.IsNull(eventBean.Get("Simple"));
            Assert.IsNull(eventBean.Get("Indexed"));
            Assert.IsNull(eventBean.Get("NestedVal"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportUpdateListener SetupSimplePattern(EPServiceProvider epService, string selectCriteria) {
            var stmtText = "select " + selectCriteria + " from pattern [a=" + typeof(SupportBean).FullName + "]";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            return listener;
        }
    
        private SupportUpdateListener SetupOrPattern(EPServiceProvider epService, string selectCriteria) {
            var stmtText = "select " + selectCriteria + " from pattern [Every(a=" + typeof(SupportBean).FullName +
                    " or b=" + typeof(SupportBeanComplexProps).FullName + ")]";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
            return updateListener;
        }
    }
} // end of namespace
