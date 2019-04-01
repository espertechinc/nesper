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
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    using Map = IDictionary<string, object>;

    public class ExecEventMap : RegressionExecution {
        private readonly Properties _properties;
        private readonly IDictionary<string, object> _map;
    
        public ExecEventMap() {
            _properties = new Properties();
            _properties.Put("myInt", "int");
            _properties.Put("myString", "string");
            _properties.Put("beanA", typeof(SupportBeanComplexProps).FullName);
            _properties.Put("myStringArray", "string[]");
    
            _map = new Dictionary<string, object>();
            _map.Put("myInt", 3);
            _map.Put("myString", "some string");
            _map.Put("beanA", SupportBeanComplexProps.MakeDefaultBean());
        }
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("myMapEvent", _properties);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMapNestedEventType(epService);
            RunAssertionMetadata(epService);
            RunAssertionNestedObjects(epService);
            RunAssertionQueryFields(epService);
            RunAssertionInvalidStatement(epService);
            RunAssertionAddRemoveType(epService);
        }
    
        public static IDictionary<string, object> MakeMap(string nameValuePairs) {
            var result = new Dictionary<string, object>();
            string[] elements = nameValuePairs.Split(',');
            for (int i = 0; i < elements.Length; i++) {
                string[] pair = elements[i].Split('=');
                if (pair.Length == 2) {
                    result.Put(pair[0], pair[1]);
                }
            }
            return result;
        }
    
        public static IDictionary<string, object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, object>();
            if (entries == null) {
                return result;
            }
            for (int i = 0; i < entries.Length; i++) {
                result.Put(entries[i][0].ToString(), entries[i][1]);
            }
            return result;
        }
    
        public static Properties MakeProperties(object[][] entries) {
            var result = new Properties();
            for (int i = 0; i < entries.Length; i++) {
                Type clazz = (Type) entries[i][1];
                result.Put((string)entries[i][0], clazz.Name);
            }
            return result;
        }
    
        public static object GetNestedKeyMap(IDictionary<string, object> root, string keyOne, string keyTwo) {
            Map map = (Map) root.Get(keyOne);
            return map.Get(keyTwo);
        }
    
        internal static object GetNestedKeyMap(IDictionary<string, object> root, string keyOne, string keyTwo, string keyThree) {
            Map map = (Map) root.Get(keyOne);
            map = (Map) map.Get(keyTwo);
            return map.Get(keyThree);
        }
    
        private void RunAssertionMapNestedEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EventType supportBeanType = epService.EPAdministrator.Configuration.GetEventType("SupportBean");
    
            var lev2def = new Dictionary<string, object>();
            lev2def.Put("sb", "SupportBean");
            var lev1def = new Dictionary<string, object>();
            lev1def.Put("lev1name", lev2def);
            var lev0def = new Dictionary<string, object>();
            lev0def.Put("lev0name", lev1def);
    
            epService.EPAdministrator.Configuration.AddEventType("MyMap", lev0def);
            Assert.IsNotNull(epService.EPAdministrator.Configuration.GetEventType("MyMap"));
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select lev0name.lev1name.sb.TheString as val from MyMap").Events += listener.Update;
    
            EventAdapterService eventAdapterService = ((EPServiceProviderSPI) epService).EventAdapterService;
            var lev2data = new Dictionary<string, object>();
            lev2data.Put("sb", eventAdapterService.AdapterForTypedObject(new SupportBean("E1", 0), supportBeanType));
            var lev1data = new Dictionary<string, object>();
            lev1data.Put("lev1name", lev2data);
            var lev0data = new Dictionary<string, object>();
            lev0data.Put("lev0name", lev1data);
    
            epService.EPRuntime.SendEvent(lev0data, "MyMap");
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("val"));
    
            try {
                epService.EPRuntime.SendEvent(new object[0], "MyMap");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Event type named 'MyMap' has not been defined or is not a Object-array event type, the name 'MyMap' refers to a " +
                                typeof(IDictionary<string, object>).GetCleanName() +
                                " event type", ex.Message);
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMetadata(EPServiceProvider epService) {
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("myMapEvent");
            Assert.AreEqual(ApplicationType.MAP, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("myMapEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("myMapEvent", type.Metadata.PublicName);
            Assert.AreEqual("myMapEvent", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("myInt", typeof(int), null, false, false, false, false, false),
                    new EventPropertyDescriptor("myString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("beanA", typeof(SupportBeanComplexProps), null, false, false, false, false, true),
                    new EventPropertyDescriptor("myStringArray", typeof(string[]), typeof(string), false, false, true, false, false),
            }, type.PropertyDescriptors);
        }
    
        private void RunAssertionAddRemoveType(EPServiceProvider epService) {
            epService.EPAdministrator.DestroyAllStatements();
    
            // test remove type with statement used (no force)
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select myInt from myMapEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("myMapEvent").ToArray(), new[]{"stmtOne"});
    
            int numTypes = epService.EPAdministrator.Configuration.EventTypes.Count;
            Assert.AreEqual("myMapEvent", epService.EPAdministrator.Configuration.GetEventType("myMapEvent").Name);
    
            try {
                configOps.RemoveEventType("myMapEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("myMapEvent"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("myMapEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("myMapEvent"));
            Assert.IsTrue(configOps.RemoveEventType("myMapEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("myMapEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("myMapEvent"));
            Assert.AreEqual(numTypes - 1, epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual(null, epService.EPAdministrator.Configuration.GetEventType("myMapEvent"));
            try {
                epService.EPAdministrator.CreateEPL("select myInt from myMapEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // add back the type
            var properties = new Properties();
            properties.Put("p01", "string");
            configOps.AddEventType("myMapEvent", properties);
            Assert.IsTrue(configOps.IsEventTypeExists("myMapEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("myMapEvent").IsEmpty());
            Assert.AreEqual(numTypes, epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual("myMapEvent", epService.EPAdministrator.Configuration.GetEventType("myMapEvent").Name);
    
            // compile
            epService.EPAdministrator.CreateEPL("select p01 from myMapEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("myMapEvent").ToArray(), new[]{"stmtTwo"});
            try {
                epService.EPAdministrator.CreateEPL("select myInt from myMapEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("myMapEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("myMapEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("myMapEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("myMapEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("myMapEvent").IsEmpty());
    
            // add back the type
            properties = new Properties();
            properties.Put("newprop", "string");
            configOps.AddEventType("myMapEvent", properties);
            Assert.IsTrue(configOps.IsEventTypeExists("myMapEvent"));
    
            // compile
            epService.EPAdministrator.CreateEPL("select newprop from myMapEvent");
            try {
                epService.EPAdministrator.CreateEPL("select p01 from myMapEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedObjects(EPServiceProvider epService) {
            string statementText = "select beanA.simpleProperty as simple," +
                    "beanA.nested.nestedValue as nested," +
                    "beanA.indexed[1] as indexed," +
                    "beanA.nested.nestedNested.nestedNestedValue as nestednested " +
                    "from myMapEvent#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(_map, "myMapEvent");
            Assert.AreEqual("NestedValue", listener.LastNewData[0].Get("nested"));
            Assert.AreEqual(2, listener.LastNewData[0].Get("indexed"));
            Assert.AreEqual("NestedNestedValue", listener.LastNewData[0].Get("nestednested"));
            statement.Stop();
        }
    
        private void RunAssertionQueryFields(EPServiceProvider epService) {
            string statementText = "select myInt as intVal, myString as stringVal from myMapEvent#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // send IDictionary<string, Object> event
            epService.EPRuntime.SendEvent(_map, "myMapEvent");
            Assert.AreEqual(3, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("some string", listener.LastNewData[0].Get("stringVal"));
    
            // send Map base event
            var mapNoType = new Dictionary<string, object>();
            mapNoType.Put("myInt", 4);
            mapNoType.Put("myString", "string2");
            epService.EPRuntime.SendEvent(mapNoType, "myMapEvent");
            Assert.AreEqual(4, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("string2", listener.LastNewData[0].Get("stringVal"));
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalidStatement(EPServiceProvider epService) {
            TryInvalid(epService, "select XXX from myMapEvent#length(5)");
            TryInvalid(epService, "select myString * 2 from myMapEvent#length(5)");
            TryInvalid(epService, "select String.Trim(myInt) from myMapEvent#length(5)");
        }
    
        private void TryInvalid(EPServiceProvider epService, string statementText) {
            try {
                epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    }
} // end of namespace
