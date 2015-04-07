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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestObjectArrayEvent 
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            String[] names = {"myInt", "myString", "beanA"};
            Object[] types = {typeof(int), typeof(String), typeof(SupportBeanComplexProps)};
    
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("MyObjectArrayEvent", names, types);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestObjectArrayNestedMapEventType() {
            var eventAdapterService = ((EPServiceProviderSPI) _epService).EventAdapterService;
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            var supportBeanType = _epService.EPAdministrator.Configuration.GetEventType("SupportBean");
    
            IDictionary<String, Object> lev2def = new Dictionary<String, Object>();
            lev2def["sb"] = "SupportBean";
            IDictionary<String, Object> lev1def = new Dictionary<String, Object>();
            lev1def["lev1name"] = lev2def;
            _epService.EPAdministrator.Configuration.AddEventType("MyMapNestedObjectArray", new String[]{"lev0name"}, new Object[]{lev1def});
            Assert.AreEqual(typeof(object[]), _epService.EPAdministrator.Configuration.GetEventType("MyMapNestedObjectArray").UnderlyingType);
    
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select lev0name.lev1name.sb.TheString as val from MyMapNestedObjectArray").Events += listener.Update;
    
            IDictionary<String, Object> lev2data = new Dictionary<String, Object>();
            lev2data["sb"] = eventAdapterService.AdapterForTypedObject(new SupportBean("E1", 0), supportBeanType);
            IDictionary<String, Object> lev1data = new Dictionary<String, Object>();
            lev1data["lev1name"] = lev2data;
    
            _epService.EPRuntime.SendEvent(new Object[] {lev1data}, "MyMapNestedObjectArray");
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("val"));
            
            try {
                _epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMapNestedObjectArray");
                Assert.Fail();
            }
            catch (EPException ex) {
                Assert.AreEqual("Event type named 'MyMapNestedObjectArray' has not been defined or is not a Map event type, the name 'MyMapNestedObjectArray' refers to a System.Object(Array) event type", ex.Message);
            }
        }
    
        [Test]
        public void TestMetadata()
        {
            var type = (EventTypeSPI) ((EPServiceProviderSPI)_epService).EventAdapterService.GetEventTypeByName("MyObjectArrayEvent");
            Assert.AreEqual(ApplicationType.OBJECTARR, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyObjectArrayEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("MyObjectArrayEvent", type.Metadata.PublicName);
            Assert.AreEqual("MyObjectArrayEvent", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            var types = ((EPServiceProviderSPI)_epService).EventAdapterService.AllTypes;
            Assert.AreEqual(1, types.Count);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("myInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("myString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("beanA", typeof(SupportBeanComplexProps), null, false, false, false, false, true),
            }, type.PropertyDescriptors);
        }
    
        [Test]
        public void TestAddRemoveType()
        {
            // test remove type with statement used (no force)
            var configOps = _epService.EPAdministrator.Configuration;
            var stmt = _epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").ToArray(), new String[]{"stmtOne"});
            
            Assert.AreEqual(1, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual(typeof(object[]), _epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent").UnderlyingType);
    
            try {
                configOps.RemoveEventType("MyObjectArrayEvent", false);
            }
            catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyObjectArrayEvent"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyObjectArrayEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyObjectArrayEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.AreEqual(0, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual(null, _epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent"));
            try {
                _epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
    
            // add back the type
            configOps.AddEventType("MyObjectArrayEvent", new String[] {"p01"}, new Object[] {typeof(String)});
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
            Assert.AreEqual(1, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual("MyObjectArrayEvent", _epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent").Name);
    
            // compile
            _epService.EPAdministrator.CreateEPL("select p01 from MyObjectArrayEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").ToArray(), new String[]{"stmtTwo"});
            try {
                _epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyObjectArrayEvent", false);
            }
            catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyObjectArrayEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyObjectArrayEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
    
            // add back the type
            configOps.AddEventType("MyObjectArrayEvent", new String[] {"newprop"}, new Object[] {typeof(String)});
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
    
            // compile
            _epService.EPAdministrator.CreateEPL("select newprop from MyObjectArrayEvent");
            try {
                _epService.EPAdministrator.CreateEPL("select p01 from MyObjectArrayEvent");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestNestedObjects()
        {
            var statementText = "select beanA.simpleProperty as simple," +
                        "beanA.nested.nestedValue as nested," +
                        "beanA.indexed[1] as indexed," +
                        "beanA.nested.nestedNested.nestedNestedValue as nestednested " +
                        "from MyObjectArrayEvent.win:length(5)";
            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new Object[] {3, "some string", SupportBeanComplexProps.MakeDefaultBean()}, "MyObjectArrayEvent");
            Assert.AreEqual("NestedValue", listener.LastNewData[0].Get("nested"));
            Assert.AreEqual(2, listener.LastNewData[0].Get("indexed"));
            Assert.AreEqual("NestedNestedValue", listener.LastNewData[0].Get("nestednested"));
            statement.Stop();
        }
    
        [Test]
        public void TestQueryFields()
        {
            var statementText = "select myInt + 2 as intVal, 'x' || myString || 'x' as stringVal from MyObjectArrayEvent.win:length(5)";
            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // send IDictionary<String, Object> event
            _epService.EPRuntime.SendEvent(new Object[] {3, "some string", SupportBeanComplexProps.MakeDefaultBean()}, "MyObjectArrayEvent");
            Assert.AreEqual(5, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("xsome stringx", listener.LastNewData[0].Get("stringVal"));
    
            // send Map base event
            _epService.EPRuntime.SendEvent(new Object[] {4, "string2", null}, "MyObjectArrayEvent");
            Assert.AreEqual(6, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("xstring2x", listener.LastNewData[0].Get("stringVal"));
    
            statement.Stop();
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                var configuration = SupportConfigFactory.GetConfiguration();
                configuration.AddEventType("MyInvalidEvent", new String[] {"p00"}, new Object[] {typeof(int), typeof(String)});
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Number of property names and property types do not match, found 1 property names and 2 property types", ex.Message);
            }
    
            TryInvalid("select XXX from MyObjectArrayEvent.win:length(5)");
            TryInvalid("select myString * 2 from MyObjectArrayEvent.win:length(5)");
            TryInvalid("select String.Trim(myInt) from MyObjectArrayEvent.win:length(5)");

            var invalidOAConfig = new ConfigurationEventTypeObjectArray();
            invalidOAConfig.SuperTypes = new HashSet<String>() { "A", "B" };
            var invalidOANames = new String[] {"p00"};
            var invalidOATypes = new Object[] {typeof(int)};
            try
            {
                var configuration = SupportConfigFactory.GetConfiguration();
                configuration.AddEventType("MyInvalidEventTwo", invalidOANames, invalidOATypes, invalidOAConfig);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Object-array event types only allow a single supertype", ex.Message);
            }

            try {
                _epService.EPAdministrator.Configuration.AddEventType("MyInvalidOA", invalidOANames, invalidOATypes, invalidOAConfig);
                Assert.Fail();
            }
            catch (ConfigurationException ex)
            {
                Assert.AreEqual("Object-array event types only allow a single supertype", ex.Message);
            }

            try {
                _epService.EPAdministrator.CreateEPL("create objectarray schema InvalidOA () inherits A, B");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Object-array event types only allow a single supertype [create objectarray schema InvalidOA () inherits A, B]", ex.Message);
            }
        }
    
        [Test]
        public void TestSendMapNative()
        {
            var statementText = "select * from MyObjectArrayEvent.win:length(5)";
            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // send event
            var theEvent = new Object[] {3, "some string", SupportBeanComplexProps.MakeDefaultBean()};
            _epService.EPRuntime.SendEvent(theEvent, "MyObjectArrayEvent");
    
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreSame(theEvent, listener.LastNewData[0].Underlying);
            Assert.AreEqual(3, listener.LastNewData[0].Get("myInt"));
            Assert.AreEqual("some string", listener.LastNewData[0].Get("myString"));
    
            // send event
            theEvent = new Object[] {4, "string2", null};
            _epService.EPRuntime.SendEvent(theEvent, "MyObjectArrayEvent");
    
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(theEvent, listener.LastNewData[0].Underlying);
            Assert.AreEqual(4, listener.LastNewData[0].Get("myInt"));
            Assert.AreEqual("string2", listener.LastNewData[0].Get("myString"));
        }
    
        [Test]
        public void TestPerformanceOutput() {
    
            // Comment-in for manual testing.
            // PONO input, Map output, 10M, 23.45 sec
            // PONO input, Object[] output, 10M, 17.204 sec
            // Map input, Map output, 10M, 25.33 sec
            // Map input, Object[] output, 10M, 19.797 sec
            // Object[] input, Map output, 10M, 22.2 sec
            // Object[] input, Object[] output, 10M, 16.97 sec
            //
            // memory: 608000 PONO for 32m, 521000 Object[] for 32m, 41000 Map for 32m
            //
    
            // type prep
            /*
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyPONOEvent));
            IDictionary<String, Object> mapdef = new Dictionary<String, Object>();
            IDictionary<String, Object> mapval = new Dictionary<String, Object>();
            List<String> propertyNames = new List<String>();
            List<Object> propertyTypes = new List<Object>();
            for (int i = 0; i < 10; i++) {
                mapdef.Put("p" + i, typeof(String));
                mapval.Put("p" + i, "p" + i);
                propertyNames.Add("p" + i);
                propertyTypes.Add(typeof(String));
            }
            epService.EPAdministrator.Configuration.AddEventType("MyMapEvent", mapdef);
            epService.EPAdministrator.Configuration.AddEventType("MyObjectArray", propertyNames.ToArray(), propertyTypes.ToArray());
    
            // stmt prep
            EPStatement stmtPONO = epService.EPAdministrator.CreateEPL(OutputTypeEnum.ARRAY.GetAnnotationText() + " select p0,p1,p2,p3,p4,p5,p6,p7,p8,p9 from MyObjectArray");
            Assert.AreEqual(OutputTypeEnum.ARRAY.GetOutputClass(), stmtPONO.EventType.UnderlyingType);
            stmtPONO.AddListener(new MyDiscardListener());
    
            // event prep
            //Object event = new MyPONOEvent("p0", "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p8");
            Object[] objectArrayEvent = {"p0", "p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p8"};
    
            // loop
            long start = Environment.TickCount;
            for (int i = 0; i < 10000000; i++) {
                //epService.EPRuntime.SendEvent(event);
                //epService.EPRuntime.SendEvent(mapval, "MyMapEvent");
                epService.EPRuntime.SendEvent(objectArrayEvent, "MyObjectArray");
            }
            long end = Environment.TickCount;
            double deltaSec = (end - start) / 1000d;
    
            Console.WriteLine("Delta: " + deltaSec);
            */
        }
    
        private void TryInvalid(String statementText)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
        }
    
        public class MyPONOEvent
        {
            public MyPONOEvent(String p0, String p1, String p2, String p3, String p4, String p5, String p6, String p7, String p8, String p9)
            {
                P0 = p0;
                P1 = p1;
                P2 = p2;
                P3 = p3;
                P4 = p4;
                P5 = p5;
                P6 = p6;
                P7 = p7;
                P8 = p8;
                P9 = p9;
            }

            public string P0 { get; private set; }

            public string P1 { get; private set; }

            public string P2 { get; private set; }

            public string P3 { get; private set; }

            public string P4 { get; private set; }

            public string P5 { get; private set; }

            public string P6 { get; private set; }

            public string P7 { get; private set; }

            public string P8 { get; private set; }

            public string P9 { get; private set; }
        }

        public class MyDiscardListener
        {
            public void Update(EventBean[] newEvents, EventBean[] oldEvents)
            {
            }
        }
    }
}
