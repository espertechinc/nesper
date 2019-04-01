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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapInheritanceInitTime : RegressionExecution {
        public override void Configure(Configuration configuration) {
            IDictionary<string, Object> root = ExecEventMap.MakeMap(new object[][]{new object[] {"base", typeof(string)}});
            IDictionary<string, Object> sub1 = ExecEventMap.MakeMap(new object[][]{new object[] {"sub1", typeof(string)}});
            IDictionary<string, Object> sub2 = ExecEventMap.MakeMap(new object[][]{new object[] {"sub2", typeof(string)}});
            Properties suba = ExecEventMap.MakeProperties(new object[][]{new object[] {"suba", typeof(string)}});
            IDictionary<string, Object> subb = ExecEventMap.MakeMap(new object[][]{new object[] {"subb", typeof(string)}});
            configuration.AddEventType("RootEvent", root);
            configuration.AddEventType("Sub1Event", sub1);
            configuration.AddEventType("Sub2Event", sub2);
            configuration.AddEventType("SubAEvent", suba);
            configuration.AddEventType("SubBEvent", subb);
    
            configuration.AddMapSuperType("Sub1Event", "RootEvent");
            configuration.AddMapSuperType("Sub2Event", "RootEvent");
            configuration.AddMapSuperType("SubAEvent", "Sub1Event");
            configuration.AddMapSuperType("SubBEvent", "Sub1Event");
            configuration.AddMapSuperType("SubBEvent", "Sub2Event");
        }
    
        public override void Run(EPServiceProvider epService) {
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("base", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("sub1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("suba", typeof(string), typeof(char), false, false, true, false, false),
            }, ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("SubAEvent").PropertyDescriptors);
    
            RunAssertionMapInheritance(epService);
        }
    
        internal static void RunAssertionMapInheritance(EPServiceProvider epService) {
            var listeners = new SupportUpdateListener[5];
            string[] statements = {
                    "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb? as vb from RootEvent",  // 0
                    "select base as vbase, sub1 as v1, sub2? as v2, suba? as va, subb? as vb from Sub1Event",   // 1
                    "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb? as vb from Sub2Event",   // 2
                    "select base as vbase, sub1 as v1, sub2? as v2, suba as va, subb? as vb from SubAEvent",    // 3
                    "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb as vb from SubBEvent"     // 4
            };
            for (int i = 0; i < statements.Length; i++) {
                EPStatement statement = epService.EPAdministrator.CreateEPL(statements[i]);
                listeners[i] = new SupportUpdateListener();
                statement.Events += listeners[i].Update;
            }
            string[] fields = "vbase,v1,v2,va,vb".Split(',');
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=a,sub1=b,sub2=x,suba=c,subb=y"), "SubAEvent");
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"a", "b", "x", "c", "y"});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new object[]{"a", "b", "x", "c", "y"});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new object[]{"a", "b", "x", "c", "y"});
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=f1,sub1=f2,sub2=f3,suba=f4,subb=f5"), "SubAEvent");
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new object[]{"f1", "f2", "f3", "f4", "f5"});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new object[]{"f1", "f2", "f3", "f4", "f5"});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new object[]{"f1", "f2", "f3", "f4", "f5"});
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=XBASE,sub1=X1,sub2=X2,subb=XY"), "SubBEvent");
            var values = new object[]{"XBASE", "X1", "X2", null, "XY"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=YBASE,sub1=Y1"), "Sub1Event");
            values = new object[]{"YBASE", "Y1", null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=YBASE,sub2=Y2"), "Sub2Event");
            values = new object[]{"YBASE", null, "Y2", null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap("base=ZBASE"), "RootEvent");
            values = new object[]{"ZBASE", null, null, null, null};
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
                epService.EPAdministrator.Configuration.AddEventType("Sub1Event", ExecEventMap.MakeMap(""), new string[]{"doodle"});
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Supertype by name 'doodle' could not be found", ex.Message);
            }
        }
    }
} // end of namespace
