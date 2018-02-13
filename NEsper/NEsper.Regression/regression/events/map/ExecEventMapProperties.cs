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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapProperties : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionArrayProperty(epService);
            RunAssertionMappedProperty(epService);
            RunAssertionsMapNamePropertyNested(epService);
            RunAssertionMapNameProperty(epService);
        }
    
        private void RunAssertionArrayProperty(EPServiceProvider epService) {
            // test map containing first-level property that is an array of primitive or Class
            IDictionary<string, Object> arrayDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", typeof(int[])}, new object[] {"p1", typeof(SupportBean[])}});
            epService.EPAdministrator.Configuration.AddEventType("MyArrayMap", arrayDef);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0[0] as a, p0[1] as b, p1[0].intPrimitive as c, p1[1] as d, p0 as e from MyArrayMap");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var p0 = new int[]{1, 2, 3};
            var beans = new SupportBean[]{new SupportBean("e1", 5), new SupportBean("e2", 6)};
            IDictionary<string, Object> theEvent = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", p0}, new object[] {"p1", beans}});
            epService.EPRuntime.SendEvent(theEvent, "MyArrayMap");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[]{1, 2, 5, beans[1], p0});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or Class
            IDictionary<string, Object> arrayDefOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", arrayDef}});
            epService.EPAdministrator.Configuration.AddEventType("MyArrayMapOuter", arrayDefOuter);
    
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0[0] as a, outer.p0[1] as b, outer.p1[0].intPrimitive as c, outer.p1[1] as d, outer.p0 as e from MyArrayMapOuter");
            stmt.AddListener(listener);
    
            IDictionary<string, Object> eventOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", theEvent}});
            epService.EPRuntime.SendEvent(eventOuter, "MyArrayMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d".Split(','), new Object[]{1, 2, 5, beans[1]});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMappedProperty(EPServiceProvider epService) {
            // test map containing first-level property that is an array of primitive or Class
            IDictionary<string, Object> mappedDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", typeof(Map)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMap", mappedDef);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select P0('k1') as a from MyMappedPropertyMap");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var eventVal = new Dictionary<string, Object>();
            eventVal.Put("k1", "v1");
            IDictionary<string, Object> theEvent = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", eventVal}});
            epService.EPRuntime.SendEvent(theEvent, "MyMappedPropertyMap");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[]{"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or Class
            IDictionary<string, Object> mappedDefOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", mappedDef}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);
    
            stmt = epService.EPAdministrator.CreateEPL("select Outer.P0('k1') as a from MyMappedPropertyMapOuter");
            stmt.AddListener(listener);
    
            IDictionary<string, Object> eventOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", theEvent}});
            epService.EPRuntime.SendEvent(eventOuter, "MyMappedPropertyMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[]{"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
    
            // test map that contains a bean which has a map property
            IDictionary<string, Object> mappedDefOuterTwo = ExecEventMap.MakeMap(new Object[][]{new object[] {"outerTwo", typeof(SupportBeanComplexProps)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);
    
            stmt = epService.EPAdministrator.CreateEPL("select OuterTwo.MapProperty('xOne') as a from MyMappedPropertyMapOuterTwo");
            stmt.AddListener(listener);
    
            IDictionary<string, Object> eventOuterTwo = ExecEventMap.MakeMap(new Object[][]{new object[] {"outerTwo", SupportBeanComplexProps.MakeDefaultBean()}});
            epService.EPRuntime.SendEvent(eventOuterTwo, "MyMappedPropertyMapOuterTwo");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[]{"yOne"});
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("a"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionsMapNamePropertyNested(EPServiceProvider epService) {
            // create a named map
            IDictionary<string, Object> namedDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            IDictionary<string, Object> eventDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", "MyNamedMap"}, new object[] {"p1", "MyNamedMap[]"}});
            epService.EPAdministrator.Configuration.AddEventType("MyMapWithAMap", eventDef);
    
            // test named-map at the second level of a nested map
            IDictionary<string, Object> arrayDefOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", eventDef}});
            epService.EPAdministrator.Configuration.AddEventType("MyArrayMapTwo", arrayDefOuter);
    
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0 as a, outer.p1[0].n0 as b, outer.p1[1].n0 as c, outer.p0 as d, outer.p1 as e from MyArrayMapTwo");
            stmt.AddListener(listener);
    
            IDictionary<string, Object> n0_1 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 1}});
            IDictionary<string, Object> n0_21 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 2}});
            IDictionary<string, Object> n0_22 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 3}});
            var n0_2 = new Map[]{n0_21, n0_22};
            IDictionary<string, Object> theEvent = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", n0_1}, new object[] {"p1", n0_2}});
            IDictionary<string, Object> eventOuter = ExecEventMap.MakeMap(new Object[][]{new object[] {"outer", theEvent}});
            epService.EPRuntime.SendEvent(eventOuter, "MyArrayMapTwo");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[]{1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0? as a, outer.p1[0].n0? as b, outer.p1[1]?.n0 as c, outer.p0? as d, outer.p1? as e from MyArrayMapTwo");
            stmt.AddListener(listener);
            epService.EPRuntime.SendEvent(eventOuter, "MyArrayMapTwo");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[]{1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMapNameProperty(EPServiceProvider epService) {
            // create a named map
            IDictionary<string, Object> namedDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            IDictionary<string, Object> eventDef = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", "MyNamedMap"}, new object[] {"p1", "MyNamedMap[]"}});
            epService.EPAdministrator.Configuration.AddEventType("MyMapWithAMap", eventDef);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0.n0 as a, p1[0].n0 as b, p1[1].n0 as c, p0 as d, p1 as e from MyMapWithAMap");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            IDictionary<string, Object> n0_1 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 1}});
            IDictionary<string, Object> n0_21 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 2}});
            IDictionary<string, Object> n0_22 = ExecEventMap.MakeMap(new Object[][]{new object[] {"n0", 3}});
            var n0_2 = new Map[]{n0_21, n0_22};
            IDictionary<string, Object> theEvent = ExecEventMap.MakeMap(new Object[][]{new object[] {"p0", n0_1}, new object[] {"p1", n0_2}});
            epService.EPRuntime.SendEvent(theEvent, "MyMapWithAMap");
    
            EventBean eventResult = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(eventResult, "a,b,c,d".Split(','), new Object[]{1, 2, 3, n0_1});
            Map[] valueE = (Map[]) eventResult.Get("e");
            Assert.AreEqual(valueE[0], n0_2[0]);
            Assert.AreEqual(valueE[1], n0_2[1]);
    
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
        }
    }
} // end of namespace
