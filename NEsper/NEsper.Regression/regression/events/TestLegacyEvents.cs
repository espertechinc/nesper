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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestLegacyEvents
    {
        private SupportLegacyBean _legacyBean;
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var mappedProperty = new Dictionary<String, String>();
            mappedProperty["key1"] = "value1";
            mappedProperty["key2"] = "value2";
            _legacyBean = new SupportLegacyBean("leg", new[] { "a", "b" }, mappedProperty, "nest");
        }

        [Test]
        public void TestAddTypeAssemblyQualified()
        {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = CodeGenerationEnum.DISABLED;

            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType("MyLegacyEvent", typeof (SupportLegacyBean).AssemblyQualifiedName, legacyDef);

            Assert.IsTrue(_epService.EPAdministrator.Configuration.IsEventTypeExists("MyLegacyEvent"));
            Assert.That(_epService.EPAdministrator.Configuration.GetEventType("MyLegacyEvent"), Is.Not.Null);
            Assert.That(_epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyLegacyEvent"), Is.Empty);

            using (_epService.EPAdministrator.CreateEPL("select * from MyLegacyEvent"))
            {
            }
        }

        [Test]
        public void TestAddTypeFullName()
        {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = CodeGenerationEnum.DISABLED;

            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean), legacyDef);

            Assert.IsTrue(_epService.EPAdministrator.Configuration.IsEventTypeExists("MyLegacyEvent"));
            Assert.That(_epService.EPAdministrator.Configuration.GetEventType("MyLegacyEvent"), Is.Not.Null);
            Assert.That(_epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyLegacyEvent"), Is.Empty);

            using (_epService.EPAdministrator.CreateEPL("select * from MyLegacyEvent"))
            {
            }
        }

        [Test]
        public void TestAddRemoveType()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var configOps = _epService.EPAdministrator.Configuration;

            // test remove type with statement used (no force)
            configOps.AddEventType("MyBeanEvent", typeof(SupportBean_A));
            var stmt = _epService.EPAdministrator.CreateEPL("select id from MyBeanEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyBeanEvent").ToArray(), new[] { "stmtOne" });

            try
            {
                configOps.RemoveEventType("MyBeanEvent", false);
            }
            catch (ConfigurationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("MyBeanEvent"));
            }

            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyBeanEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyBeanEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyBeanEvent"));
            try
            {
                _epService.EPAdministrator.CreateEPL("select id from MyBeanEvent");
                Assert.Fail();
            }
            catch (EPException)
            {
                // expected
            }

            // add back the type
            configOps.AddEventType("MyBeanEvent", typeof(SupportBean));
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());

            // compile
            _epService.EPAdministrator.CreateEPL("select BoolPrimitive from MyBeanEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyBeanEvent").ToArray(), new[] { "stmtTwo" });
            try
            {
                _epService.EPAdministrator.CreateEPL("select id from MyBeanEvent");
                Assert.Fail();
            }
            catch (EPException)
            {
                // expected
            }

            // remove with force
            try
            {
                configOps.RemoveEventType("MyBeanEvent", false);
            }
            catch (ConfigurationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("MyBeanEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyBeanEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());

            // add back the type
            configOps.AddEventType("MyBeanEvent", typeof(SupportMarketDataBean));
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));

            // compile
            _epService.EPAdministrator.CreateEPL("select feed from MyBeanEvent");
            try
            {
                _epService.EPAdministrator.CreateEPL("select BoolPrimitive from MyBeanEvent");
                Assert.Fail();
            }
            catch (EPException)
            {
                // expected
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestPublicAccessors()
        {
            TryPublicAccessors(CodeGenerationEnum.ENABLED);
        }

        [Test]
        public void TestPublicAccessorsNoCodeGen()
        {
            TryPublicAccessors(CodeGenerationEnum.DISABLED);
        }

        [Test]
        public void TestExplicitOnly()
        {
            TryExplicitOnlyAccessors(CodeGenerationEnum.ENABLED);
        }

        [Test]
        public void TestExplicitOnlyNoCodeGen()
        {
            TryExplicitOnlyAccessors(CodeGenerationEnum.DISABLED);
        }

        [Test]
        public void TestBeanAccessor()
        {
            TryNativeBeanAccessor(CodeGenerationEnum.ENABLED);
        }

        [Test]
        public void TestBeanAccessorNoCodeGen()
        {
            TryNativeBeanAccessor(CodeGenerationEnum.DISABLED);
        }

        [Test]
        public void TestFinalClass()
        {
            TryFinalClass(CodeGenerationEnum.ENABLED);
        }

        [Test]
        public void TestFinalClassNoCodeGen()
        {
            TryFinalClass(CodeGenerationEnum.DISABLED);
        }

        private void TryPublicAccessors(CodeGenerationEnum codeGeneration)
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("explicitFSimple", "fieldLegacyVal");
            legacyDef.AddFieldProperty("explicitFIndexed", "fieldStringArray");
            legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
            legacyDef.AddMethodProperty("explicitMSimple", "ReadLegacyBeanVal");
            legacyDef.AddMethodProperty("explicitMArray", "ReadStringArray");
            legacyDef.AddMethodProperty("explicitMIndexed", "ReadStringIndexed");
            legacyDef.AddMethodProperty("explicitMMapped", "ReadMapByKey");
            _epService.EPAdministrator.Configuration.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean), legacyDef);

            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.PUBLIC;
            legacyDef.CodeGeneration = CodeGenerationEnum.DISABLED;
            _epService.EPAdministrator.Configuration.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

            // assert type metadata
            var type = (EventTypeSPI)((EPServiceProviderSPI)_epService).EventAdapterService.GetEventTypeByName("MyLegacyEvent");
            Assert.AreEqual(ApplicationType.CLASS, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(1, type.Metadata.OptionalSecondaryNames.Count);
            Assert.AreEqual(typeof(SupportLegacyBean).Name, type.Metadata.OptionalSecondaryNames.First());
            Assert.AreEqual("MyLegacyEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("MyLegacyEvent", type.Metadata.PublicName);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, type.Metadata.IsApplicationPreConfiguredStatic);

            var statementText = "select " +
                    "fieldLegacyVal as fieldSimple," +
                    "fieldStringArray as fieldArr," +
                    "fieldStringArray[1] as fieldArrIndexed," +
                    "fieldMapped as fieldMap," +
                    "fieldNested as fieldNested," +
                    "fieldNested.ReadNestedValue as fieldNestedVal," +
                    "ReadLegacyBeanVal as simple," +
                    "ReadLegacyNested as nestedObject," +
                    "ReadLegacyNested.ReadNestedValue as nested," +
                    "ReadStringArray[0] as array," +
                    "ReadStringIndexed[1] as indexed," +
                    "ReadMapByKey('key1') as mapped," +
                    "ReadMap as mapItself," +
                    "explicitFSimple, " +
                    "explicitFIndexed[0], " +
                    "explicitFNested, " +
                    "explicitMSimple, " +
                    "explicitMArray[0], " +
                    "explicitMIndexed[1], " +
                    "explicitMMapped('key2')" +
                    " from MyLegacyEvent#length(5)";

            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            var eventType = statement.EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldSimple"));
            Assert.AreEqual(typeof(String[]), eventType.GetPropertyType("fieldArr"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldArrIndexed"));
            Assert.AreEqual(typeof(IDictionary<string,string>), eventType.GetPropertyType("fieldMap"));
            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("fieldNested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fieldNestedVal"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("simple"));
            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("nestedObject"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("nested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("array"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("indexed"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mapped"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFSimple"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitFIndexed[0]"));
            Assert.AreEqual(typeof(SupportLegacyBean.LegacyNested), eventType.GetPropertyType("explicitFNested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMSimple"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMArray[0]"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMIndexed[1]"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("explicitMMapped('key2')"));

            _epService.EPRuntime.SendEvent(_legacyBean);

            Assert.AreEqual(_legacyBean.fieldLegacyVal, listener.LastNewData[0].Get("fieldSimple"));
            Assert.AreEqual(_legacyBean.fieldStringArray, listener.LastNewData[0].Get("fieldArr"));
            Assert.AreEqual(_legacyBean.fieldStringArray[1], listener.LastNewData[0].Get("fieldArrIndexed"));
            Assert.AreEqual(_legacyBean.fieldMapped, listener.LastNewData[0].Get("fieldMap"));
            Assert.AreEqual(_legacyBean.fieldNested, listener.LastNewData[0].Get("fieldNested"));
            Assert.AreEqual(_legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("fieldNestedVal"));

            Assert.AreEqual(_legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("simple"));
            Assert.AreEqual(_legacyBean.ReadLegacyNested(), listener.LastNewData[0].Get("nestedObject"));
            Assert.AreEqual(_legacyBean.ReadLegacyNested().ReadNestedValue(), listener.LastNewData[0].Get("nested"));
            Assert.AreEqual(_legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("array"));
            Assert.AreEqual(_legacyBean.ReadStringIndexed(1), listener.LastNewData[0].Get("indexed"));
            Assert.AreEqual(_legacyBean.ReadMapByKey("key1"), listener.LastNewData[0].Get("mapped"));
            Assert.AreEqual(_legacyBean.ReadMap(), listener.LastNewData[0].Get("mapItself"));

            Assert.AreEqual(_legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("explicitFSimple"));
            Assert.AreEqual(_legacyBean.ReadLegacyBeanVal(), listener.LastNewData[0].Get("explicitMSimple"));
            Assert.AreEqual(_legacyBean.ReadLegacyNested(), listener.LastNewData[0].Get("explicitFNested"));
            Assert.AreEqual(_legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("explicitFIndexed[0]"));
            Assert.AreEqual(_legacyBean.ReadStringIndexed(0), listener.LastNewData[0].Get("explicitMArray[0]"));
            Assert.AreEqual(_legacyBean.ReadStringIndexed(1), listener.LastNewData[0].Get("explicitMIndexed[1]"));
            Assert.AreEqual(_legacyBean.ReadMapByKey("key2"), listener.LastNewData[0].Get("explicitMMapped('key2')"));

            var stmtType = (EventTypeSPI)statement.EventType;
            Assert.AreEqual(ApplicationType.MAP, stmtType.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, stmtType.Metadata.OptionalSecondaryNames);
            Assert.NotNull(stmtType.Metadata.PrimaryName);
            Assert.NotNull(stmtType.Metadata.PublicName);
            Assert.NotNull(stmtType.Name);
            Assert.AreEqual(TypeClass.ANONYMOUS, stmtType.Metadata.TypeClass);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationConfigured);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(false, stmtType.Metadata.IsApplicationPreConfiguredStatic);

            _epService.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void TryExplicitOnlyAccessors(CodeGenerationEnum codeGeneration)
        {
            var config = SupportConfigFactory.GetConfiguration();

            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("explicitFNested", "fieldNested");
            legacyDef.AddMethodProperty("explicitMNested", "ReadLegacyNested");
            config.AddEventType("MyLegacyEvent", typeof(SupportLegacyBean), legacyDef);

            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("fieldNestedClassValue", "fieldNestedValue");
            legacyDef.AddMethodProperty("ReadNestedClassValue", "ReadNestedValue");
            config.AddEventType("MyLegacyNestedEvent", typeof(SupportLegacyBean.LegacyNested), legacyDef);

            legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.EXPLICIT;
            legacyDef.CodeGeneration = codeGeneration;
            config.AddEventType("MySupportBean", typeof(SupportBean), legacyDef);

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var statementText = "select " +
                    "explicitFNested.fieldNestedClassValue as fnested, " +
                    "explicitMNested.ReadNestedClassValue as mnested" +
                    " from MyLegacyEvent#length(5)";

            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            var eventType = statement.EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fnested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mnested"));

            _epService.EPRuntime.SendEvent(_legacyBean);

            Assert.AreEqual(_legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("fnested"));
            Assert.AreEqual(_legacyBean.fieldNested.ReadNestedValue(), listener.LastNewData[0].Get("mnested"));

            try
            {
                // invalid statement, JavaBean-style getters not exposed
                statementText = "select IntPrimitive from MySupportBean#length(5)";
                _epService.EPAdministrator.CreateEPL(statementText);
            }
            catch (EPStatementException)
            {
                // expected
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService.Dispose();
        }

        public void TryNativeBeanAccessor(CodeGenerationEnum codeGeneration)
        {
            var config = SupportConfigFactory.GetConfiguration();
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.NATIVE;
            legacyDef.CodeGeneration = codeGeneration;
            legacyDef.AddFieldProperty("explicitFInt", "fieldIntPrimitive");
            legacyDef.AddMethodProperty("explicitMGetInt", "GetIntPrimitive");
            legacyDef.AddMethodProperty("explicitMReadInt", "ReadIntPrimitive");
            config.AddEventType("MyLegacyEvent", typeof(SupportLegacyBeanInt), legacyDef);

            _epService = EPServiceProviderManager.GetDefaultProvider(config); 
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var statementText = "select IntPrimitive, explicitFInt, explicitMGetInt, explicitMReadInt " +
                    " from MyLegacyEvent#length(5)";

            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            var eventType = statement.EventType;

            var theEvent = new SupportLegacyBeanInt(10);
            _epService.EPRuntime.SendEvent(theEvent);

            foreach (var name in new[] { "IntPrimitive", "explicitFInt", "explicitMGetInt", "explicitMReadInt" })
            {
                Assert.AreEqual(typeof(int), eventType.GetPropertyType(name));
                Assert.AreEqual(10, listener.LastNewData[0].Get(name));
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService.Dispose();
        }

        private void TryFinalClass(CodeGenerationEnum codeGeneration)
        {
            var config = SupportConfigFactory.GetConfiguration();
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.AccessorStyle = AccessorStyleEnum.NATIVE;
            legacyDef.CodeGeneration = codeGeneration;
            config.AddEventType("MyFinalEvent", typeof(SupportBeanFinal), legacyDef);

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var statementText = "select IntPrimitive " +
                    "from " + typeof(SupportBeanFinal).FullName + "#length(5)";

            var statement = _epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            var theEvent = new SupportBeanFinal(10);
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(10, listener.LastNewData[0].Get("IntPrimitive"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _epService.Dispose();
        }
    }
}
