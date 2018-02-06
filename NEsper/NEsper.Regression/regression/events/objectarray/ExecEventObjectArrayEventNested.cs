///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.map.ExecEventMap;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    using Map = IDictionary<string, object>;

    public class ExecEventObjectArrayEventNested : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionArrayProperty(epService);
            RunAssertionMappedProperty(epService);
            RunAssertionMapNamePropertyNested(epService);
            RunAssertionMapNameProperty(epService);
            RunAssertionObjectArrayNested(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // can add the same nested type twice
            epService.EPAdministrator.Configuration.AddEventType("ABC", new string[]{"p0"}, new Type[]{typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("ABC", new string[]{"p0"}, new Type[]{typeof(int)});
            try {
                // changing the definition however stops the compatibility
                epService.EPAdministrator.Configuration.AddEventType("ABC", new string[]{"p0"}, new Type[]{typeof(long)});
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Event type named 'ABC' has already been declared with differing column name or type information: Type by name 'ABC' in property 'p0' expected " + Name.Clean<int>() + " but receives " + Name.Clean<long>(), ex.Message);
            }
    
            TryInvalid(epService, new string[]{"a"}, new object[]{new SupportBean()}, "Nestable type configuration encountered an unexpected property type of 'SupportBean' for property 'a', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
        }
    
        private void RunAssertionArrayProperty(EPServiceProvider epService) {
    
            // test map containing first-level property that is an array of primitive or .GetType()
            string[] props = {"p0", "p1"};
            object[] types = {typeof(int[]), typeof(SupportBean[])};
            epService.EPAdministrator.Configuration.AddEventType("MyArrayOA", props, types);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0[0] as a, p0[1] as b, p1[0].IntPrimitive as c, p1[1] as d, p0 as e from MyArrayOA");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var p0 = new int[]{1, 2, 3};
            var beans = new SupportBean[]{new SupportBean("e1", 5), new SupportBean("e2", 6)};
            var eventData = new object[]{p0, beans};
            epService.EPRuntime.SendEvent(eventData, "MyArrayOA");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new object[]{1, 2, 5, beans[1], p0});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or .GetType()
            epService.EPAdministrator.Configuration.AddEventType("MyArrayOAMapOuter", new string[]{"outer"}, new object[]{"MyArrayOA"});
    
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0[0] as a, outer.p0[1] as b, outer.p1[0].IntPrimitive as c, outer.p1[1] as d, outer.p0 as e from MyArrayOAMapOuter");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{eventData}, "MyArrayOAMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d".Split(','), new object[]{1, 2, 5, beans[1]});
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMappedProperty(EPServiceProvider epService) {
    
            // test map containing first-level property that is an array of primitive or .GetType()
            IDictionary<string, Object> mappedDef = MakeMap(new object[][]{new object[] {"p0", typeof(Map)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMap", mappedDef);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0('k1') as a from MyMappedPropertyMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var eventVal = new Dictionary<string, Object>();
            eventVal.Put("k1", "v1");
            IDictionary<string, Object> theEvent = MakeMap(new object[][]{new object[] {"p0", eventVal}});
            epService.EPRuntime.SendEvent(theEvent, "MyMappedPropertyMap");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new object[]{"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or .GetType()
            IDictionary<string, Object> mappedDefOuter = MakeMap(new object[][]{new object[] {"outer", mappedDef}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);
    
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0('k1') as a from MyMappedPropertyMapOuter");
            stmt.Events += listener.Update;
    
            IDictionary<string, Object> eventOuter = MakeMap(new object[][]{new object[] {"outer", theEvent}});
            epService.EPRuntime.SendEvent(eventOuter, "MyMappedPropertyMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new object[]{"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
    
            // test map that contains a bean which has a map property
            IDictionary<string, Object> mappedDefOuterTwo = MakeMap(new object[][]{new object[] {"outerTwo", typeof(SupportBeanComplexProps)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);
    
            stmt = epService.EPAdministrator.CreateEPL("select outerTwo.MapProperty('xOne') as a from MyMappedPropertyMapOuterTwo");
            stmt.Events += listener.Update;
    
            IDictionary<string, Object> eventOuterTwo = MakeMap(new object[][]{new object[] {"outerTwo", SupportBeanComplexProps.MakeDefaultBean()}});
            epService.EPRuntime.SendEvent(eventOuterTwo, "MyMappedPropertyMapOuterTwo");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new object[]{"yOne"});
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("a"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMapNamePropertyNested(EPServiceProvider epService) {
            // create a named map
            IDictionary<string, Object> namedDef = MakeMap(new object[][]{new object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            IDictionary<string, Object> eventDef = MakeMap(new object[][]{new object[] {"p0", "MyNamedMap"}, new object[] {"p1", "MyNamedMap[]"}});
            epService.EPAdministrator.Configuration.AddEventType("MyMapWithAMap", eventDef);
    
            // test named-map at the second level of a nested map
            epService.EPAdministrator.Configuration.AddEventType("MyObjectArrayMapOuter", new string[]{"outer"}, new object[]{eventDef});
    
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0 as a, outer.p1[0].n0 as b, outer.p1[1].n0 as c, outer.p0 as d, outer.p1 as e from MyObjectArrayMapOuter");
            stmt.Events += listener.Update;
    
            IDictionary<string, Object> n0_1 = MakeMap(new object[][]{new object[] {"n0", 1}});
            IDictionary<string, Object> n0_21 = MakeMap(new object[][]{new object[] {"n0", 2}});
            IDictionary<string, Object> n0_22 = MakeMap(new object[][]{new object[] {"n0", 3}});
            var n0_2 = new Map[]{n0_21, n0_22};
            IDictionary<string, Object> theEvent = MakeMap(new object[][]{new object[] {"p0", n0_1}, new object[] {"p1", n0_2}});
            epService.EPRuntime.SendEvent(new object[]{theEvent}, "MyObjectArrayMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new object[]{1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0? as a, outer.p1[0].n0? as b, outer.p1[1]?.n0 as c, outer.p0? as d, outer.p1? as e from MyObjectArrayMapOuter");
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new object[]{theEvent}, "MyObjectArrayMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new object[]{1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMapNameProperty(EPServiceProvider epService) {
    
            // create a named map
            IDictionary<string, Object> namedDef = MakeMap(new object[][]{new object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            epService.EPAdministrator.Configuration.AddEventType("MyOAWithAMap", new string[]{"p0", "p1"}, new object[]{"MyNamedMap", "MyNamedMap[]"});
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p0.n0 as a, p1[0].n0 as b, p1[1].n0 as c, p0 as d, p1 as e from MyOAWithAMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            IDictionary<string, Object> n0_1 = MakeMap(new object[][]{new object[] {"n0", 1}});
            IDictionary<string, Object> n0_21 = MakeMap(new object[][]{new object[] {"n0", 2}});
            IDictionary<string, Object> n0_22 = MakeMap(new object[][]{new object[] {"n0", 3}});
            var n0_2 = new Map[]{n0_21, n0_22};
            epService.EPRuntime.SendEvent(new object[]{n0_1, n0_2}, "MyOAWithAMap");
    
            EventBean eventResult = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(eventResult, "a,b,c,d".Split(','), new object[]{1, 2, 3, n0_1});
            Map[] valueE = (Map[]) eventResult.Get("e");
            Assert.AreEqual(valueE[0], n0_2[0]);
            Assert.AreEqual(valueE[1], n0_2[1]);
    
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionObjectArrayNested(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("TypeLev1", new string[]{"p1id"}, new object[]{typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("TypeLev0", new string[]{"p0id", "p1"}, new object[]{typeof(int), "TypeLev1"});
            epService.EPAdministrator.Configuration.AddEventType("TypeRoot", new string[]{"rootId", "p0"}, new object[]{typeof(int), "TypeLev0"});
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from TypeRoot#lastevent");
            object[] dataLev1 = {1000};
            object[] dataLev0 = {100, dataLev1};
            epService.EPRuntime.SendEvent(new object[]{10, dataLev0}, "TypeRoot");
            EventBean theEvent = stmt.First();
            EPAssertionUtil.AssertProps(theEvent, "rootId,p0.p0id,p0.p1.p1id".Split(','), new object[]{10, 100, 1000});
    
            stmt.Dispose();
        }
    
        private void TryInvalid(EPServiceProvider epService, string[] names, object[] types, string message) {
            try {
                epService.EPAdministrator.Configuration.AddEventType("NestedMap", names, types);
                Assert.Fail();
            } catch (Exception ex) {
                // Comment-in: Log.Error(ex.Message, ex);
                Assert.IsTrue(ex.Message.Contains(message), "expected '" + message + "' but received '" + ex.Message);
            }
        }
    }
} // end of namespace
