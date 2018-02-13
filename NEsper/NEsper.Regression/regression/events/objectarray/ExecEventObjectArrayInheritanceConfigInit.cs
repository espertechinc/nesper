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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.map.ExecEventMap;
// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    public class ExecEventObjectArrayInheritanceConfigInit : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("RootEvent", new string[]{"base"}, new Object[]{typeof(string)});
            configuration.AddEventType("Sub1Event", new string[]{"sub1"}, new Object[]{typeof(string)});
            configuration.AddEventType("Sub2Event", new string[]{"sub2"}, new Object[]{typeof(string)});
            configuration.AddEventType("SubAEvent", new string[]{"suba"}, new Object[]{typeof(string)});
            configuration.AddEventType("SubBEvent", new string[]{"subb"}, new Object[]{typeof(string)});
    
            configuration.AddObjectArraySuperType("Sub1Event", "RootEvent");
            configuration.AddObjectArraySuperType("Sub2Event", "RootEvent");
            configuration.AddObjectArraySuperType("SubAEvent", "Sub1Event");
            configuration.AddObjectArraySuperType("SubBEvent", "SubAEvent");
    
            try {
                configuration.AddObjectArraySuperType("SubBEvent", "Sub2Event");
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Object-array event types may not have multiple supertypes", ex.Message);
            }
        }
    
        public override void Run(EPServiceProvider epService) {
            RunObjectArrInheritanceAssertion(epService);
        }
    
        internal static void RunObjectArrInheritanceAssertion(EPServiceProvider epService) {
            var listeners = new SupportUpdateListener[5];
            string[] statements = {
                    "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb? as vb from RootEvent",  // 0
                    "select base as vbase, sub1 as v1, sub2? as v2, suba? as va, subb? as vb from Sub1Event",   // 1
                    "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb? as vb from Sub2Event",   // 2
                    "select base as vbase, sub1 as v1, sub2? as v2, suba as va, subb? as vb from SubAEvent",    // 3
                    "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb as vb from SubBEvent"     // 4
            };
            for (int i = 0; i < statements.Length; i++) {
                EPStatement statement = epService.EPAdministrator.CreateEPL(statements[i]);
                listeners[i] = new SupportUpdateListener();
                statement.AddListener(listeners[i]);
            }
            string[] fields = "vbase,v1,v2,va,vb".Split(',');
    
            EventType type = epService.EPAdministrator.Configuration.GetEventType("SubAEvent");
            Assert.AreEqual("base", type.PropertyDescriptors[0].PropertyName);
            Assert.AreEqual("sub1", type.PropertyDescriptors[1].PropertyName);
            Assert.AreEqual("suba", type.PropertyDescriptors[2].PropertyName);
            Assert.AreEqual(3, type.PropertyDescriptors.Length);
    
            type = epService.EPAdministrator.Configuration.GetEventType("SubBEvent");
            Assert.AreEqual("[base, sub1, suba, subb]", Arrays.ToString(type.PropertyNames));
            Assert.AreEqual(4, type.PropertyDescriptors.Length);
    
            type = epService.EPAdministrator.Configuration.GetEventType("Sub1Event");
            Assert.AreEqual("[base, sub1]", Arrays.ToString(type.PropertyNames));
            Assert.AreEqual(2, type.PropertyDescriptors.Length);
    
            type = epService.EPAdministrator.Configuration.GetEventType("Sub2Event");
            Assert.AreEqual("[base, sub2]", Arrays.ToString(type.PropertyNames));
            Assert.AreEqual(2, type.PropertyDescriptors.Length);
    
            epService.EPRuntime.SendEvent(new Object[]{"a", "b", "x"}, "SubAEvent");    // base, sub1, suba
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
    
            epService.EPRuntime.SendEvent(new Object[]{"f1", "f2", "f4"}, "SubAEvent");
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
    
            epService.EPRuntime.SendEvent(new Object[]{"XBASE", "X1", "X2", "XY"}, "SubBEvent");
            var values = new Object[]{"XBASE", "X1", null, "X2", "XY"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[]{"YBASE", "Y1"}, "Sub1Event");
            values = new Object[]{"YBASE", "Y1", null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[]{"YBASE", "Y2"}, "Sub2Event");
            values = new Object[]{"YBASE", null, "Y2", null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[]{"ZBASE"}, "RootEvent");
            values = new Object[]{"ZBASE", null, null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
    
            // try property not available
            try {
                epService.EPAdministrator.CreateEPL("select suba from Sub1Event");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'suba': Property named 'suba' is not valid in any stream (did you mean 'sub1'?) [select suba from Sub1Event]", ex.Message);
            }
    
            // try supertype not Exists
            try {
                epService.EPAdministrator.Configuration.AddEventType("Sub1Event", MakeMap(""), new string[]{"doodle"});
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Supertype by name 'doodle' could not be found", ex.Message);
            }
        }
    }
} // end of namespace
