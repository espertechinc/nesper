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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestMapEvent
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _properties = new Properties();
            _properties["MyInt"] = "int";
            _properties["MyString"] = "string";
            _properties["beanA"] = typeof (SupportBeanComplexProps).FullName;
            _properties["myStringArray"] = "string[]";

            _map = new Dictionary<String, Object>();
            _map["MyInt"] = 3;
            _map["MyString"] = "some string";
            _map["beanA"] = SupportBeanComplexProps.MakeDefaultBean();

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("MyMapEvent", _properties);

            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
        }

        #endregion

        private Properties _properties;
        private IDictionary<String, Object> _map;
        private EPServiceProvider _epService;

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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestAddRemoveType()
        {
            // test remove type with statement used (no force)
            ConfigurationOperations configOps = _epService.EPAdministrator.Configuration;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select MyInt from MyMapEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyMapEvent").ToArray(),
                                                   new String[] {"stmtOne"});

            Assert.AreEqual(1, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual("MyMapEvent", _epService.EPAdministrator.Configuration.GetEventType("MyMapEvent").Name);

            try
            {
                configOps.RemoveEventType("MyMapEvent", false);
            }
            catch (ConfigurationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("MyMapEvent"));
            }

            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyMapEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyMapEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyMapEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyMapEvent", false)); // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyMapEvent"));
            Assert.AreEqual(0, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual(null, _epService.EPAdministrator.Configuration.GetEventType("MyMapEvent"));
            try
            {
                _epService.EPAdministrator.CreateEPL("select MyInt from MyMapEvent");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }

            // add back the type
            var properties = new Properties();
            properties["p01"] = "string";
            configOps.AddEventType("MyMapEvent", properties);
            Assert.IsTrue(configOps.IsEventTypeExists("MyMapEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyMapEvent").IsEmpty());
            Assert.AreEqual(1, _epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual("MyMapEvent", _epService.EPAdministrator.Configuration.GetEventType("MyMapEvent").Name);

            // compile
            _epService.EPAdministrator.CreateEPL("select p01 from MyMapEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyMapEvent").ToArray(),
                                                   new String[] {"stmtTwo"});
            try
            {
                _epService.EPAdministrator.CreateEPL("select MyInt from MyMapEvent");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }

            // remove with force
            try
            {
                configOps.RemoveEventType("MyMapEvent", false);
            }
            catch (ConfigurationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("MyMapEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyMapEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyMapEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyMapEvent").IsEmpty());

            // add back the type
            properties = new Properties();
            properties["newprop"] = "string";
            configOps.AddEventType("MyMapEvent", properties);
            Assert.IsTrue(configOps.IsEventTypeExists("MyMapEvent"));

            // compile
            _epService.EPAdministrator.CreateEPL("select newprop from MyMapEvent");
            try
            {
                _epService.EPAdministrator.CreateEPL("select p01 from MyMapEvent");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
            }
        }

        [Test]
        public void TestInvalidStatement()
        {
            TryInvalid("select XXX from MyMapEvent.win:length(5)");
            TryInvalid("select MyString * 2 from MyMapEvent.win:length(5)");
            TryInvalid("select TheString.Trim(MyInt) from MyMapEvent.win:length(5)");
        }

        [Test]
        public void TestMapNestedEventType()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof (SupportBean));
            EventType supportBeanType = _epService.EPAdministrator.Configuration.GetEventType("SupportBean");

            IDictionary<String, Object> lev2def = new Dictionary<String, Object>();
            lev2def["sb"] = "SupportBean";
            IDictionary<String, Object> lev1def = new Dictionary<String, Object>();
            lev1def["lev1name"] = lev2def;
            IDictionary<String, Object> lev0def = new Dictionary<String, Object>();
            lev0def["lev0name"] = lev1def;

            _epService.EPAdministrator.Configuration.AddEventType("MyMap", lev0def);
            Assert.NotNull(_epService.EPAdministrator.Configuration.GetEventType("MyMap"));

            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select lev0name.lev1name.sb.TheString as val from MyMap").Events +=
                listener.Update;

            EventAdapterService eventAdapterService = ((EPServiceProviderSPI)_epService).EventAdapterService;
            IDictionary<String, Object> lev2data = new Dictionary<String, Object>();
            lev2data["sb"] = eventAdapterService.AdapterForTypedObject(new SupportBean("E1", 0), supportBeanType);
            IDictionary<String, Object> lev1data = new Dictionary<String, Object>();
            lev1data["lev1name"] = lev2data;
            IDictionary<String, Object> lev0data = new Dictionary<String, Object>();
            lev0data["lev0name"] = lev1data;

            _epService.EPRuntime.SendEvent(lev0data, "MyMap");
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("val"));

            try
            {
                _epService.EPRuntime.SendEvent(new Object[0], "MyMap");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(
                    "Event type named 'MyMap' has not been defined or is not a Object-array event type, the name 'MyMap' refers to a " + typeof(IDictionary<string,object>).FullName + " event type",
                    ex.Message);
            }
        }

        [Test]
        public void TestMetadata()
        {
            var type = (EventTypeSPI) ((EPServiceProviderSPI) _epService).EventAdapterService.GetEventTypeByName("MyMapEvent");
            Assert.AreEqual(ApplicationType.MAP, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyMapEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("MyMapEvent", type.Metadata.PublicName);
            Assert.AreEqual("MyMapEvent", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);

            ICollection<EventType> types = ((EPServiceProviderSPI) _epService).EventAdapterService.AllTypes;
            Assert.AreEqual(1, types.Count);

            EPAssertionUtil.AssertEqualsAnyOrder(new[]
            {
                new EventPropertyDescriptor("MyInt", typeof (int), null, false, false, false, false, false),
                new EventPropertyDescriptor("MyString", typeof (string), typeof (char), false, false, true, false, false),
                new EventPropertyDescriptor("beanA", typeof (SupportBeanComplexProps), null, false, false, false, false, true),
                new EventPropertyDescriptor("myStringArray", typeof(string[]), typeof(string), false, false, true, false, false),
            }, type.PropertyDescriptors);
        }

        [Test]
        public void TestNestedObjects()
        {
            String statementText = "select beanA.SimpleProperty as simple," +
                                   "beanA.Nested.NestedValue as Nested," +
                                   "beanA.Indexed[1] as Indexed," +
                                   "beanA.Nested.NestedNested.NestedNestedValue as NestedNested " +
                                   "from MyMapEvent.win:length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            _epService.EPRuntime.SendEvent(_map, "MyMapEvent");
            Assert.AreEqual("NestedValue", listener.LastNewData[0].Get("Nested"));
            Assert.AreEqual(2, listener.LastNewData[0].Get("Indexed"));
            Assert.AreEqual("NestedNestedValue", listener.LastNewData[0].Get("NestedNested"));
            statement.Stop();
        }

        [Test]
        public void TestPrimitivesTypes()
        {
            _properties = new Properties();
            _properties["MyInt"] = typeof (int).FullName;
            _properties["byteArr"] = typeof (byte[]).FullName;
            _properties["MyInt2"] = "int";
            _properties["double"] = "double";
            _properties["boolean"] = "boolean";
            _properties["long"] = "long";
            _properties["astring"] = "string";

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("MyPrimMapEvent", _properties);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _epService.Dispose();
        }

        [Test]
        public void TestQueryFields()
        {
            String statementText =
                "select MyInt + 2 as IntVal, 'x' || MyString || 'x' as StringVal from MyMapEvent.win:length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            // send IDictionary<String, Object> event
            _epService.EPRuntime.SendEvent(_map, "MyMapEvent");
            Assert.AreEqual(5, listener.LastNewData[0].Get("IntVal"));
            Assert.AreEqual("xsome stringx", listener.LastNewData[0].Get("StringVal"));

            // send Map base event
            Map mapNoType = new Dictionary<string, object>();
            mapNoType["MyInt"] = 4;
            mapNoType["MyString"] = "string2";
            _epService.EPRuntime.SendEvent(mapNoType, "MyMapEvent");
            Assert.AreEqual(6, listener.LastNewData[0].Get("IntVal"));
            Assert.AreEqual("xstring2x", listener.LastNewData[0].Get("StringVal"));

            statement.Stop();
        }

        [Test]
        public void TestSendMapNative()
        {
            String statementText = "select * from MyMapEvent.win:length(5)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            // send IDictionary<String, Object> event
            _epService.EPRuntime.SendEvent(_map, "MyMapEvent");

            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(_map, listener.LastNewData[0].Underlying);
            Assert.AreEqual(3, listener.LastNewData[0].Get("MyInt"));
            Assert.AreEqual("some string", listener.LastNewData[0].Get("MyString"));

            // send Map base event
            Map mapNoType = new Dictionary<string, object>();
            mapNoType["MyInt"] = 4;
            mapNoType["MyString"] = "string2";
            _epService.EPRuntime.SendEvent(mapNoType, "MyMapEvent");

            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(mapNoType, listener.LastNewData[0].Underlying);
            Assert.AreEqual(4, listener.LastNewData[0].Get("MyInt"));
            Assert.AreEqual("string2", listener.LastNewData[0].Get("MyString"));

            IDictionary<String, Object> mapStrings = new Dictionary<String, Object>();
            mapStrings["MyInt"] = 5;
            mapStrings["MyString"] = "string3";
            _epService.EPRuntime.SendEvent(mapStrings, "MyMapEvent");

            Assert.IsTrue(listener.GetAndClearIsInvoked());
            Assert.AreEqual(5, listener.LastNewData[0].Get("MyInt"));
            Assert.AreEqual("string3", listener.LastNewData[0].Get("MyString"));
        }
    }
}