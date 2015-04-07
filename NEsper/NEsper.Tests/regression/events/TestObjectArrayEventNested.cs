///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;
    
    [TestFixture]
    public class TestObjectArrayEventNested 
    {
        [Test]
        public void TestConfiguredViaPropsAndXML()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.EventMetaConfig.DefaultEventRepresentation = 
                EventRepresentation.OBJECTARRAY;
            configuration.AddEventType(
                "MyOAType", 
                "bean,TheString,map".Split(','), 
                new Object[]
                {
                    typeof(SupportBean).FullName, 
                    typeof(String).FullName,
                    typeof(Map).FullName
                });
    
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var eventType = epService.EPAdministrator.Configuration.GetEventType("MyOAType");
            Assert.AreEqual(typeof(Object[]), eventType.UnderlyingType);
            Assert.AreEqual(typeof(String), eventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("map"));
            Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("bean"));
    
            var stmt = epService.EPAdministrator.CreateEPL("select bean, TheString, map('key'), bean.TheString from MyOAType");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(Object[]), stmt.EventType.UnderlyingType);
    
            var bean = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(new Object[] {bean, "abc", Collections.SingletonDataMap("key", "value")}, "MyOAType");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), "bean,TheString,map('key'),bean.TheString".Split(','), new Object[] {bean, "abc", "value", "E1"});
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestObjectArrayTypeUpdate()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.EventMetaConfig.DefaultEventRepresentation = EventRepresentation.OBJECTARRAY;
    
            String[] names = {"base1", "base2"};
            Object[] types = {typeof(String), MakeMap(new Object[][] { new Object[] {"n1", typeof(int)}})};
            configuration.AddEventType("MyOAEvent", names, types);
    
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var statementOne = epService.EPAdministrator.CreateEPL(
                    "select base1 as v1, base2.n1 as v2, base3? as v3, base2.n2? as v4 from MyOAEvent");
            Assert.AreEqual(typeof(Object[]), statementOne.EventType.UnderlyingType);
            var statementOneSelectAll = epService.EPAdministrator.CreateEPL("select * from MyOAEvent");
            Assert.AreEqual("[base1, base2]", statementOneSelectAll.EventType.PropertyNames.Render(", ", "[]"));
            var listenerOne = new SupportUpdateListener();
            statementOne.Events += listenerOne.Update;
            var fields = "v1,v2,v3,v4".Split(',');
    
            epService.EPRuntime.SendEvent(new Object[] {"abc", MakeMap(new Object[][] { new Object[] {"n1", 10}}), ""}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"abc", 10, null, null});
    
            // Update type
            String[] namesNew = {"base3", "base2"};
            var typesNew = new Object[] {typeof(long), MakeMap(new Object[][] { new Object[] {"n2", typeof(String)}})};
            epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("MyOAEvent", namesNew, typesNew);
    
            var statementTwo = epService.EPAdministrator.CreateEPL("select base1 as v1, base2.n1 as v2, base3 as v3, base2.n2 as v4 from MyOAEvent");
            var statementTwoSelectAll = epService.EPAdministrator.CreateEPL("select * from MyOAEvent");
            var listenerTwo = new SupportUpdateListener();
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new Object[] {"def", MakeMap(new Object[][] { new Object[] {"n1", 9}, new Object[] {"n2", "xyz"}}), 20L}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"def", 9, 20L, "xyz"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new Object[]{"def", 9, 20L, "xyz"});
    
            // assert event type
            Assert.AreEqual("[base1, base2, base3]", statementOneSelectAll.EventType.PropertyNames.Render(", ", "[]"));
            Assert.AreEqual("[base1, base2, base3]", statementTwoSelectAll.EventType.PropertyNames.Render(", ", "[]"));
    
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("base3", typeof(long), null, false, false, false, false, false),
                    new EventPropertyDescriptor("base2", typeof(Map), null, false, false, false, true, false),
                    new EventPropertyDescriptor("base1", typeof(string), typeof(char), false, false, true, false, false),
            }, statementTwoSelectAll.EventType.PropertyDescriptors);
    
            try
            {
                epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("dummy", new String[0], new Object[0]);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Error updating Object-array event type: Event type named 'dummy' has not been declared", ex.Message);
            }
    
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            try
            {
                epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("SupportBean", new String[0], new Object[0]);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Error updating Object-array event type: Event type by name 'SupportBean' is not an Object-array event type", ex.Message);
            }
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestObjectArrayInheritanceInitTime()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
    
            configuration.AddEventType("RootEvent", new String[] {"base"}, new Object[] {typeof(String)});
            configuration.AddEventType("Sub1Event", new String[]{"sub1"}, new Object[]{typeof(String)});
            configuration.AddEventType("Sub2Event", new String[] {"sub2"}, new Object[] {typeof(String)});
            configuration.AddEventType("SubAEvent", new String[] {"suba"}, new Object[] {typeof(String)});
            configuration.AddEventType("SubBEvent", new String[] {"subb"}, new Object[] {typeof(String)});
    
            configuration.AddObjectArraySuperType("Sub1Event", "RootEvent");
            configuration.AddObjectArraySuperType("Sub2Event", "RootEvent");
            configuration.AddObjectArraySuperType("SubAEvent", "Sub1Event");
            configuration.AddObjectArraySuperType("SubBEvent", "SubAEvent");
    
            try {
                configuration.AddObjectArraySuperType("SubBEvent", "Sub2Event");
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Object-array event types may not have multiple supertypes", ex.Message);
            }
    
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            EPAssertionUtil.AssertEqualsExactOrder(new []{
                    new EventPropertyDescriptor("base", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("sub1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("suba", typeof(string), typeof(char), false, false, true, false, false),
            }, ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("SubAEvent").PropertyDescriptors);
    
            RunObjectArrInheritanceAssertion(epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestObjectArrayInheritanceRuntime()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
    
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var configOps = epService.EPAdministrator.Configuration;
            configOps.AddEventType("RootEvent", new String[] {"base"}, new Object[] {typeof(String)});
            configOps.AddEventType("Sub1Event", new String[]{"sub1"}, new Object[]{typeof(String)}, new ConfigurationEventTypeObjectArray("RootEvent".AsSingleton()));
            configOps.AddEventType("Sub2Event", new String[]{"sub2"}, new Object[]{typeof(String)}, new ConfigurationEventTypeObjectArray("RootEvent".AsSingleton()));
            configOps.AddEventType("SubAEvent", new String[]{"suba"}, new Object[]{typeof(String)}, new ConfigurationEventTypeObjectArray("Sub1Event".AsSingleton()));
            configOps.AddEventType("SubBEvent", new String[]{"subb"}, new Object[]{typeof(String)}, new ConfigurationEventTypeObjectArray("SubAEvent".AsSingleton()));

            RunObjectArrInheritanceAssertion(epService);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private static void RunObjectArrInheritanceAssertion(EPServiceProvider epService)
        {
            var listeners = new SupportUpdateListener[5];
            String[] statements = {
                    "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb? as vb from RootEvent",  // 0
                    "select base as vbase, sub1 as v1, sub2? as v2, suba? as va, subb? as vb from Sub1Event",   // 1
                    "select base as vbase, sub1? as v1, sub2 as v2, suba? as va, subb? as vb from Sub2Event",   // 2
                    "select base as vbase, sub1 as v1, sub2? as v2, suba as va, subb? as vb from SubAEvent",    // 3
                    "select base as vbase, sub1? as v1, sub2? as v2, suba? as va, subb as vb from SubBEvent"     // 4
            };
            for (var i = 0; i < statements.Length; i++)
            {
                var statement = epService.EPAdministrator.CreateEPL(statements[i]);
                listeners[i] = new SupportUpdateListener();
                statement.Events += listeners[i].Update;
            }
            var fields = "vbase,v1,v2,va,vb".Split(',');
    
            var type = epService.EPAdministrator.Configuration.GetEventType("SubAEvent");
            Assert.AreEqual("base", type.PropertyDescriptors[0].PropertyName);
            Assert.AreEqual("sub1", type.PropertyDescriptors[1].PropertyName);
            Assert.AreEqual("suba", type.PropertyDescriptors[2].PropertyName);
            Assert.AreEqual(3, type.PropertyDescriptors.Count);
    
            type = epService.EPAdministrator.Configuration.GetEventType("SubBEvent");
            Assert.AreEqual("[base, sub1, suba, subb]", type.PropertyNames.Render(", ", "[]"));
            Assert.AreEqual(4, type.PropertyDescriptors.Count);
    
            type = epService.EPAdministrator.Configuration.GetEventType("Sub1Event");
            Assert.AreEqual("[base, sub1]", type.PropertyNames.Render(", ", "[]"));
            Assert.AreEqual(2, type.PropertyDescriptors.Count);
    
            type = epService.EPAdministrator.Configuration.GetEventType("Sub2Event");
            Assert.AreEqual("[base, sub2]", type.PropertyNames.Render(", ", "[]"));
            Assert.AreEqual(2, type.PropertyDescriptors.Count);
    
            epService.EPRuntime.SendEvent(new Object[] {"a","b","x"}, "SubAEvent");    // base, sub1, suba
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new Object[]{"a", "b", null, "x", null});
    
            epService.EPRuntime.SendEvent(new Object[] {"f1", "f2", "f4"}, "SubAEvent");
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
            Assert.IsFalse(listeners[2].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, new Object[]{"f1", "f2", null, "f4", null});
    
            epService.EPRuntime.SendEvent(new Object[] {"XBASE", "X1", "X2", "XY"}, "SubBEvent");
            var values = new Object[] {"XBASE","X1",null,"X2","XY"};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(), fields, values);
            EPAssertionUtil.AssertProps(listeners[4].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[] {"YBASE","Y1"}, "Sub1Event");
            values = new Object[] {"YBASE","Y1", null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[] {"YBASE", "Y2"}, "Sub2Event");
            values = new Object[] {"YBASE",null, "Y2", null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(), fields, values);
    
            epService.EPRuntime.SendEvent(new Object[] {"ZBASE"}, "RootEvent");
            values = new Object[] {"ZBASE",null, null, null, null};
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(), fields, values);
            Assert.IsFalse(listeners[1].IsInvoked || listeners[2].IsInvoked || listeners[3].IsInvoked || listeners[4].IsInvoked);
    
            // try property not available
            try
            {
                epService.EPAdministrator.CreateEPL("select suba from Sub1Event");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'suba': Property named 'suba' is not valid in any stream (did you mean 'sub1'?) [select suba from Sub1Event]", ex.Message);
            }
    
            // try supertype not exists
            try
            {
                epService.EPAdministrator.Configuration.AddEventType("Sub1Event", MakeMap(""), new String[] {"doodle"});
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Supertype by name 'doodle' could not be found",ex.Message);
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            var epService = GetEngineInitialized(null, null, null);
    
            // can add the same nested type twice
            epService.EPAdministrator.Configuration.AddEventType("ABC", new String[] {"p0"}, new Type[] {typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("ABC", new String[] {"p0"}, new Type[] {typeof(int)});
            try
            {
                // changing the definition however stops the compatibility
                epService.EPAdministrator.Configuration.AddEventType("ABC", new String[] {"p0"}, new Type[] {typeof(long)});
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Event type named 'ABC' has already been declared with differing column name or type information: Type by name 'ABC' in property 'p0' expected " + typeof(int?) + " but receives " + typeof(long?), ex.Message);
            }
            
            TryInvalid(epService, new String[] {"a"}, new Object[] {new SupportBean()}, "Nestable type configuration encountered an unexpected property type of 'SupportBean' for property 'a', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type");
        }
    
        [Test]
        public void TestNestedPono()
        {
            var pair = GetTestDefTwo();
            var epService = GetEngineInitialized("NestedObjectArr", pair.First, pair.Second);
    
            var statementText = "select " +
                                    "simple, object, nodefmap, map, " +
                                    "object.id as a1, nodefmap.key1? as a2, nodefmap.key2? as a3, nodefmap.key3?.key4 as a4, " +
                                    "map.objectOne as b1, map.simpleOne as b2, map.nodefmapOne.key2? as b3, map.mapOne.simpleTwo? as b4, " +
                                    "map.objectOne.indexed[1] as c1, map.objectOne.nested.nestedValue as c2," +
                                    "map.mapOne.simpleTwo as d1, map.mapOne.objectTwo as d2, map.mapOne.nodefmapTwo as d3, " +
                                    "map.mapOne.mapTwo as e1, map.mapOne.mapTwo.simpleThree as e2, map.mapOne.mapTwo.objectThree as e3, " +
                                    "map.mapOne.objectTwo.array[1].Mapped('1ma').value as f1, map.mapOne.mapTwo.objectThree.id as f2" +
                                    " from NestedObjectArr";
            var statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            var testdata = GetTestDataTwo();
            epService.EPRuntime.SendEvent(testdata, "NestedObjectArr");
    
            // test all properties exist
            var received = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, "simple,object,nodefmap,map".Split(','),
                    new Object[]{"abc", new SupportBean_A("A1"), testdata[2], testdata[3]});
            EPAssertionUtil.AssertProps(received, "a1,a2,a3,a4".Split(','),
                    new Object[]{"A1", "val1", null, null});
            EPAssertionUtil.AssertProps(received, "b1,b2,b3,b4".Split(','),
                    new Object[]{GetNestedKey(testdata, 3, "objectOne"), 10, "val2", 300});
            EPAssertionUtil.AssertProps(received, "c1,c2".Split(','), new Object[]{2, "NestedValue"});
            EPAssertionUtil.AssertProps(received, "d1,d2,d3".Split(','),
                    new Object[]{300, GetNestedKey(testdata, 3, "mapOne", "objectTwo"), GetNestedKey(testdata, 3, "mapOne", "nodefmapTwo")});
            EPAssertionUtil.AssertProps(received, "e1,e2,e3".Split(','),
                    new Object[]{GetNestedKey(testdata, 3, "mapOne", "mapTwo"), 4000L, new SupportBean_B("B1")});
            EPAssertionUtil.AssertProps(received, "f1,f2".Split(','),
                    new Object[]{"1ma0", "B1"});
    
            // assert type info
            var stmt = epService.EPAdministrator.CreateEPL(("select * from NestedObjectArr"));
            var eventType = stmt.EventType;
    
            var propertiesReceived = eventType.PropertyNames;
            var propertiesExpected = new String[] {"simple", "object", "nodefmap", "map"};
            EPAssertionUtil.AssertEqualsAnyOrder(propertiesReceived, propertiesExpected);
            Assert.AreEqual(typeof(String), eventType.GetPropertyType("simple"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("map"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("nodefmap"));
            Assert.AreEqual(typeof(SupportBean_A), eventType.GetPropertyType("object"));
    
            Assert.IsNull(eventType.GetPropertyType("map.mapOne.simpleOne"));

            // nested PONO with generic return type
            listener.Reset();
            epService.EPAdministrator.Configuration.AddEventType("MyNested", new String[] {"bean"}, new Object[] { typeof(MyNested) });
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from MyNested(bean.Insides.anyOf(i=>Id = 'A'))");
            stmtTwo.Events += listener.Update;

            epService.EPRuntime.SendEvent(new Object[] {new MyNested(new MyInside[] {new MyInside("A")})}, "MyNested");
            Assert.IsTrue(listener.IsInvoked);
        }
    
        [Test]
        public void TestArrayProperty()
        {
            var epService = GetEngineInitialized(null, null, null);
    
            // test map containing first-level property that is an array of primitive or Class
            String[] props = {"p0", "p1"};
            Object[] types = {typeof(int[]), typeof(SupportBean[])};
            epService.EPAdministrator.Configuration.AddEventType("MyArrayOA", props, types);
    
            var stmt = epService.EPAdministrator.CreateEPL("select p0[0] as a, p0[1] as b, p1[0].IntPrimitive as c, p1[1] as d, p0 as e from MyArrayOA");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var p0 = new int[] {1, 2, 3};
            var beans = new SupportBean[] {new SupportBean("e1", 5), new SupportBean("e2", 6)};
            var eventData = new Object[] {p0, beans};
            epService.EPRuntime.SendEvent(eventData, "MyArrayOA");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[]{1, 2, 5, beans[1], p0});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or Class
            epService.EPAdministrator.Configuration.AddEventType("MyArrayOAMapOuter", new String[] {"outer"}, new Object[] {"MyArrayOA"});
    
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0[0] as a, outer.p0[1] as b, outer.p1[0].IntPrimitive as c, outer.p1[1] as d, outer.p0 as e from MyArrayOAMapOuter");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new Object[] {eventData}, "MyArrayOAMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d".Split(','), new Object[]{1, 2, 5, beans[1]});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(int[]), stmt.EventType.GetPropertyType("e"));
        }
    
        [Test]
        public void TestMappedProperty()
        {
            var epService = GetEngineInitialized(null, null, null);
    
            // test map containing first-level property that is an array of primitive or Class
            var mappedDef = MakeMap(new Object[][] { new Object[] {"p0", typeof(Map)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMap", mappedDef);
    
            var stmt = epService.EPAdministrator.CreateEPL("select p0('k1') as a from MyMappedPropertyMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            IDictionary<String, Object> eventVal = new Dictionary<String, Object>();
            eventVal["k1"] = "v1";
            var theEvent = MakeMap(new Object[][] { new Object[] {"p0", eventVal}});
            epService.EPRuntime.SendEvent(theEvent, "MyMappedPropertyMap");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[] {"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
            stmt.Dispose();
    
            // test map at the second level of a nested map that is an array of primitive or Class
            var mappedDefOuter = MakeMap(new Object[][] { new Object[] {"outer", mappedDef}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuter", mappedDefOuter);
    
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0('k1') as a from MyMappedPropertyMapOuter");
            stmt.Events += listener.Update;
    
            var eventOuter = MakeMap(new Object[][] { new Object[] {"outer", theEvent}});
            epService.EPRuntime.SendEvent(eventOuter, "MyMappedPropertyMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[] {"v1"});
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("a"));
    
            // test map that contains a bean which has a map property
            var mappedDefOuterTwo = MakeMap(new Object[][] { new Object[] {"outerTwo", typeof(SupportBeanComplexProps)}});
            epService.EPAdministrator.Configuration.AddEventType("MyMappedPropertyMapOuterTwo", mappedDefOuterTwo);
    
            stmt = epService.EPAdministrator.CreateEPL("select outerTwo.MapProperty('xOne') as a from MyMappedPropertyMapOuterTwo");
            stmt.Events += listener.Update;
    
            var eventOuterTwo = MakeMap(new Object[][] { new Object[] {"outerTwo", SupportBeanComplexProps.MakeDefaultBean()}});
            epService.EPRuntime.SendEvent(eventOuterTwo, "MyMappedPropertyMapOuterTwo");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a".Split(','), new Object[] {"yOne"});
            Assert.AreEqual(typeof(String), stmt.EventType.GetPropertyType("a"));        
        }
    
        [Test]
        public void TestMapNamePropertyNested()
        {
            var epService = GetEngineInitialized(null, null, null);
    
            // create a named map
            var namedDef = MakeMap(new Object[][] { new Object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            var eventDef = MakeMap(new Object[][] { new Object[] {"p0", "MyNamedMap"}, new Object[] {"p1", "MyNamedMap[]"}});
            epService.EPAdministrator.Configuration.AddEventType("MyMapWithAMap", eventDef);
    
            // test named-map at the second level of a nested map
            epService.EPAdministrator.Configuration.AddEventType("MyObjectArrayMapOuter", new String[] {"outer"}, new Object[] {eventDef});
    
            var listener = new SupportUpdateListener();
            var stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0 as a, outer.p1[0].n0 as b, outer.p1[1].n0 as c, outer.p0 as d, outer.p1 as e from MyObjectArrayMapOuter");
            stmt.Events += listener.Update;
    
            var n0_1 = MakeMap(new Object[][] { new Object[] {"n0", 1}});
            var n0_21 = MakeMap(new Object[][] { new Object[] {"n0", 2}});
            var n0_22 = MakeMap(new Object[][] { new Object[] {"n0", 3}});
            var n0_2 = new Map[] {n0_21, n0_22};
            var theEvent = MakeMap(new Object[][] { new Object[] {"p0", n0_1}, new Object[] {"p1", n0_2 }});
            epService.EPRuntime.SendEvent(new Object[] {theEvent}, "MyObjectArrayMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[] {1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select outer.p0.n0? as a, outer.p1[0].n0? as b, outer.p1[1]?.n0 as c, outer.p0? as d, outer.p1? as e from MyObjectArrayMapOuter");
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new Object[] {theEvent}, "MyObjectArrayMapOuter");
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "a,b,c,d,e".Split(','), new Object[] {1, 2, 3, n0_1, n0_2});
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
        }
    
        [Test]
        public void TestMapNameProperty()
        {
            var epService = GetEngineInitialized(null, null, null);
    
            // create a named map
            var namedDef = MakeMap(new Object[][] { new Object[] {"n0", typeof(int)}});
            epService.EPAdministrator.Configuration.AddEventType("MyNamedMap", namedDef);
    
            // create a map using the name
            epService.EPAdministrator.Configuration.AddEventType("MyOAWithAMap", new String[] {"p0", "p1"}, new Object[] {"MyNamedMap", "MyNamedMap[]"});
    
            var stmt = epService.EPAdministrator.CreateEPL("select p0.n0 as a, p1[0].n0 as b, p1[1].n0 as c, p0 as d, p1 as e from MyOAWithAMap");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var n0_1 = MakeMap(new Object[][] { new Object[] {"n0", 1}});
            var n0_21 = MakeMap(new Object[][] { new Object[] {"n0", 2}});
            var n0_22 = MakeMap(new Object[][] { new Object[] {"n0", 3}});
            var n0_2 = new Map[] {n0_21, n0_22};
            epService.EPRuntime.SendEvent(new Object[] {n0_1, n0_2}, "MyOAWithAMap");
    
            var eventResult = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(eventResult, "a,b,c,d".Split(','), new Object[] {1, 2, 3, n0_1});
            var valueE = (Map[]) eventResult.Get("e");
            Assert.AreSame(valueE[0], n0_2[0]);
            Assert.AreSame(valueE[1], n0_2[1]);

            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("b"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c"));
            Assert.AreEqual(typeof(Map), stmt.EventType.GetPropertyType("d"));
            Assert.AreEqual(typeof(Map[]), stmt.EventType.GetPropertyType("e"));
        }
    
        [Test]
        public void TestObjectArrayNested()
        {
            var epService = GetEngineInitialized(null, null, null);
            epService.EPAdministrator.Configuration.AddEventType("TypeLev1", new String[] {"p1id"}, new Object[] {typeof(int)});
            epService.EPAdministrator.Configuration.AddEventType("TypeLev0", new String[] {"p0id", "p1"}, new Object[] {typeof(int), "TypeLev1"});
            epService.EPAdministrator.Configuration.AddEventType("TypeRoot", new String[] {"rootId", "p0"}, new Object[] {typeof(int), "TypeLev0"});

            var stmt = epService.EPAdministrator.CreateEPL("select * from TypeRoot.std:lastevent()");
            Object[] dataLev1 = {1000};
            Object[] dataLev0 = {100, dataLev1};
            epService.EPRuntime.SendEvent(new Object[] {10, dataLev0}, "TypeRoot");
            var theEvent = stmt.FirstOrDefault();
            EPAssertionUtil.AssertProps(theEvent, "rootId,p0.p0id,p0.p1.p1id".Split(','), new Object[] {10, 100, 1000});
        }
    
        private static void TryInvalid(EPServiceProvider epService, String[] names, Object[] types, String message)
        {
            try
            {
                epService.EPAdministrator.Configuration.AddEventType("NestedMap", names, types);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Assert.IsTrue(ex.Message.Contains(message), "expected '" + message + "' but received '" + ex.Message);
            }
        }
    
        private static Object GetNestedKey(Object[] array, int index, String keyTwo)
        {
            var map = (Map) array[index];
            return map.Get(keyTwo);
        }
    
        private static Object GetNestedKey(Object[] array, int index, String keyTwo, String keyThree)
        {
            var map = (Map) array[index];
            map = (Map) map.Get(keyTwo);
            return map.Get(keyThree);
        }
    
        private Object[] GetTestDataTwo()
        {
            var levelThree = MakeMap(new Object[][] {
                    new Object[] {"simpleThree", 4000L},
                    new Object[] {"objectThree", new SupportBean_B("B1")},
            });
    
            var levelTwo = MakeMap(new Object[][] {
                    new Object[] {"simpleTwo", 300},
                    new Object[] {"objectTwo", SupportBeanCombinedProps.MakeDefaultBean()},
                    new Object[] {"nodefmapTwo", MakeMap(new Object[][] { new Object[] {"key3", "val3"}})},
                    new Object[] {"mapTwo", levelThree},
            });
    
            var levelOne = MakeMap(new Object[][] {
                    new Object[] {"simpleOne", 10},
                    new Object[] {"objectOne", SupportBeanComplexProps.MakeDefaultBean()},
                    new Object[] {"nodefmapOne", MakeMap(new Object[][] { new Object[] {"key2", "val2"}})},
                    new Object[] {"mapOne", levelTwo}
            });
    
            var levelZero = new Object[] {"abc", new SupportBean_A("A1"), MakeMap(new Object[][] { new Object[] {"key1", "val1"}}), levelOne};
            return levelZero;
        }
    
        private Pair<String[], Object[]> GetTestDefTwo()
        {
            var levelThree= MakeMap(new Object[][] {
                    new Object[] {"simpleThree", typeof(long)},
                    new Object[] {"objectThree", typeof(SupportBean_B)},
            });
    
            var levelTwo= MakeMap(new Object[][] {
                    new Object[] {"simpleTwo", typeof(int)},
                    new Object[] {"objectTwo", typeof(SupportBeanCombinedProps)},
                    new Object[] {"nodefmapTwo", typeof(Map)},
                    new Object[] {"mapTwo", levelThree},
            });
    
            var levelOne = MakeMap(new Object[][] {
                    new Object[] {"simpleOne", typeof(int)},
                    new Object[] {"objectOne", typeof(SupportBeanComplexProps)},
                    new Object[] {"nodefmapOne", typeof(Map)},
                    new Object[] {"mapOne", levelTwo}
            });
    
            var levelZeroProps = new String[] {"simple", "object", "nodefmap", "map"};
            var levelZeroTypes = new Object[] {typeof(String), typeof(SupportBean_A), typeof(Map), levelOne};
            return new Pair<String[], Object[]>(levelZeroProps, levelZeroTypes);
        }
    
        private static EPServiceProvider GetEngineInitialized(String name, String[] propertyNames, Object[] propertyTypes)
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            if (name != null) {
                configuration.AddEventType(name, propertyNames, propertyTypes);
            }
            
            var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            return epService;
        }
    
        private static IDictionary<String, Object> MakeMap(String nameValuePairs)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            var elements = nameValuePairs.Split(',');
            for (var i = 0; i < elements.Length; i++)
            {
                var pair = elements[i].Split('=');
                if (pair.Length == 2)
                {
                    result.Put(pair[0], pair[1]);
                }
            }
            return result;
        }
    
        private static IDictionary<String, Object> MakeMap(Object[][] entries)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            if (entries == null)
            {
                return result;
            }
            for (var i = 0; i < entries.Length; i++)
            {
                result.Put((String) entries[i][0], entries[i][1]);
            }
            return result;
        }

        private class MyNested
        {
            public IList<MyInside> Insides { get; private set; }
            public MyNested(IList<MyInside> insides)
            {
                Insides = insides;
            }
        }

        private class MyInside
        {
            public string Id { get; private set; }
            public MyInside(String id)
            {
                Id = id;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
