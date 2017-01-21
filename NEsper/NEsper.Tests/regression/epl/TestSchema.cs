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
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using com.espertech.esper.support.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    using DataMap = IDictionary<string, object>;

    [TestFixture]
    public class TestSchema 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestSchemaArrayPrimitiveType()
        {
            RunAssertionSchemaArrayPrimitiveType(true);
            RunAssertionSchemaArrayPrimitiveType(false);

            SupportMessageAssertUtil.TryInvalid(_epService, "create schema Invalid (x dummy[primitive])",
                    "Error starting statement: Type 'dummy' is not a primitive type [create schema Invalid (x dummy[primitive])]");
            SupportMessageAssertUtil.TryInvalid(_epService, "create schema Invalid (x int[dummy])",
                    "Column type keyword 'dummy' not recognized, expecting '[primitive]'");
        }

        private void RunAssertionSchemaArrayPrimitiveType(bool soda)
        {
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create schema MySchema as (c0 int[primitive], c1 int[])");
            Object[][] expectedType = new Object[][] { new Object[] { "c0", typeof(int[]) }, new Object[] { "c1", typeof(int[]) } };
            EventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, _epService.EPAdministrator.Configuration.GetEventType("MySchema"),
                EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
            _epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
        }
    
        [Test]
        public void TestSchemaWithEventType()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType("BeanSourceEvent", typeof(BeanSourceEvent));
            BeanSourceEvent theEvent = new BeanSourceEvent(new SupportBean("E1", 1), new SupportBean_S0[] {new SupportBean_S0(2)});
    
            // test schema
            EPStatement stmtSchema = _epService.EPAdministrator.CreateEPL("create schema MySchema (bean SupportBean, beanarray SupportBean_S0[])");
            Assert.AreEqual(new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true), stmtSchema.EventType.GetPropertyDescriptor("bean"));
            Assert.AreEqual(new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true), stmtSchema.EventType.GetPropertyDescriptor("beanarray"));
    
            EPStatement stmtSchemaInsert = _epService.EPAdministrator.CreateEPL("insert into MySchema select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            stmtSchemaInsert.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new Object[] {"E1", 2});
            stmtSchemaInsert.Dispose();
    
            // test named window
            EPStatement stmtWindow = _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as (bean SupportBean, beanarray SupportBean_S0[])");
            stmtWindow.Events += _listener.Update;
            Assert.AreEqual(new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true), stmtWindow.EventType.GetPropertyDescriptor("bean"));
            Assert.AreEqual(new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true), stmtWindow.EventType.GetPropertyDescriptor("beanarray"));
    
            EPStatement stmtWindowInsert = _epService.EPAdministrator.CreateEPL("insert into MyWindow select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new Object[] {"E1", 2});
            stmtWindowInsert.Dispose();
    
            // insert pattern to named window
            EPStatement stmtWindowPattern = _epService.EPAdministrator.CreateEPL("insert into MyWindow select sb as bean, s0Arr as beanarray from pattern [sb=SupportBean -> s0Arr=SupportBean_S0 until SupportBean_S0(id=0)]");
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "S0_3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id,beanarray[1].id".Split(','), new Object[] {"E2", 10, 20});
            stmtWindowPattern.Dispose();
    
            // test configured Map type
            IDictionary<String, Object> configDef = new Dictionary<String, Object>();
            configDef.Put("bean", "SupportBean");
            configDef.Put("beanarray", "SupportBean_S0[]");
            _epService.EPAdministrator.Configuration.AddEventType("MyConfiguredMap", configDef);
            
            EPStatement stmtMapInsert = _epService.EPAdministrator.CreateEPL("insert into MyConfiguredMap select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            stmtMapInsert.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new Object[] {"E1", 2});
            stmtMapInsert.Dispose();
        }
    
        [Test]
        public void TestSchemaCopyProperties() {
            RunAssertionSchemaCopyProperties(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionSchemaCopyProperties(EventRepresentationEnum.DEFAULT);
            RunAssertionSchemaCopyProperties(EventRepresentationEnum.MAP);
        }
    
        private void RunAssertionSchemaCopyProperties(EventRepresentationEnum eventRepresentationEnum) {
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema BaseOne (prop1 String, prop2 int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema BaseTwo (prop3 long)");
    
            // test define and send
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E1 () copyfrom BaseOne");
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("select * from E1");
            stmtOne.Events += _listener.Update;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtOne.EventType.UnderlyingType);
            Assert.AreEqual(typeof(string), stmtOne.EventType.GetPropertyType("prop1"));
            Assert.AreEqual(typeof(int), stmtOne.EventType.GetPropertyType("prop2"));
    
            IDictionary<String, Object> eventE1 = new LinkedHashMap<String, Object>();
            eventE1.Put("prop1", "v1");
            eventE1.Put("prop2", 2);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(eventE1.Values.ToArray(), "E1");
            }
            else {
                _epService.EPRuntime.SendEvent(eventE1, "E1");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "prop1,prop2".Split(','), new Object[]{"v1", 2});
    
            // test two copy-from types
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E2 () copyfrom BaseOne, BaseTwo");
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select * from E2");
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("prop1"));
            Assert.AreEqual(typeof(int), stmtTwo.EventType.GetPropertyType("prop2"));
            Assert.AreEqual(typeof(long), stmtTwo.EventType.GetPropertyType("prop3"));
    
            // test API-defined type
            IDictionary<String, Object> def = new Dictionary<String, Object>();
            def.Put("a", "string");
            def.Put("b", typeof(String));
            def.Put("c", "BaseOne");
            def.Put("d", "BaseTwo[]");
            _epService.EPAdministrator.Configuration.AddEventType("MyType", def);
    
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E3(e long, f BaseOne) copyfrom MyType");
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL("select * from E3");
            Assert.AreEqual(typeof(String), stmtThree.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(String), stmtThree.EventType.GetPropertyType("b"));
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                Assert.AreEqual(typeof(Object[]), stmtThree.EventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(Object[][]), stmtThree.EventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(Object[]), stmtThree.EventType.GetPropertyType("f"));
            }
            else {
                Assert.AreEqual(typeof(DataMap), stmtThree.EventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(DataMap[]), stmtThree.EventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(DataMap), stmtThree.EventType.GetPropertyType("f"));
            }
            Assert.AreEqual(typeof(long), stmtThree.EventType.GetPropertyType("e"));
    
            // invalid tests
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema E4(a long) copyFrom MyType",
                    "Error starting statement: Type by name 'MyType' contributes property 'a' defined as 'System.String' which overides the same property of type '" + typeof(long).FullName + "' [");
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom MyType",
                    "Error starting statement: Property by name 'c' is defined twice by adding type 'MyType' [");
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom XYZ",
                    "Error starting statement: Type by name 'XYZ' could not be located [");
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema E4 as " + typeof(SupportBean).FullName + " copyFrom XYZ",
                    "Error starting statement: Copy-from types are not allowed with class-provided types [");
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create variant schema E4(c BaseTwo) copyFrom XYZ",
                    "Error starting statement: Copy-from types are not allowed with variant types [");
    
            // test SODA
            String createEPL = eventRepresentationEnum.GetAnnotationText() + " create schema EX as () copyFrom BaseOne, BaseTwo";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(createEPL);
            Assert.AreEqual(createEPL.Trim(), model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(createEPL.Trim(), stmt.Text);
    
            _epService.Initialize();
        }
        
        [Test]
        public void TestConfiguredNotRemoved() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("MapType", new Dictionary<String, Object>());
            ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            _epService.EPAdministrator.Configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);
    
            _epService.EPAdministrator.CreateEPL("create schema ABCType(col1 int, col2 int)");
            AssertTypeExists(_epService, "ABCType", false);
            
            String moduleText = "select * from SupportBean;\n"+
                                "select * from MapType;\n" +
                                "select * from TestXMLNoSchemaType;\n";
            DeploymentResult result = _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleText, "uri", "arch", null);
    
            AssertTypeExists(_epService, "SupportBean", true);
            AssertTypeExists(_epService, "MapType", true);
            AssertTypeExists(_epService, "TestXMLNoSchemaType", true);
    
            _epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
    
            AssertTypeExists(_epService, "SupportBean", true);
            AssertTypeExists(_epService, "MapType", true);
            AssertTypeExists(_epService, "TestXMLNoSchemaType", true);
        }
    
        private void AssertTypeExists(EPServiceProvider epService, String typeName, bool isPreconfigured) {
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            EventTypeSPI type = (EventTypeSPI) spi.EventAdapterService.GetEventTypeByName(typeName);
            Assert.IsTrue(type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(isPreconfigured, type.Metadata.IsApplicationPreConfigured);
            Assert.IsFalse(type.Metadata.IsApplicationPreConfiguredStatic);
        }
    
        [Test]
        public void TestInvalid() {
            RunAssertionInvalid(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionInvalid(EventRepresentationEnum.MAP);
            RunAssertionInvalid(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionInvalid(EventRepresentationEnum eventRepresentationEnum) {
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 xxxx)",
                        "Error starting statement: Nestable type configuration encountered an unexpected property type name 'xxxx' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [");
    
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 int, col1 string)",
                        "Error starting statement: Duplicate column name 'col1' [");
    
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 string)");
            TryInvalid("create schema MyEventType as (col1 string, col2 string)",
                        "Error starting statement: Event type named 'MyEventType' has already been declared with differing column name or type information: Type by name 'MyEventType' expects 1 properties but receives 2 properties [");
    
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as () inherit ABC",
                        "Error in expression: Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered 'inherit' [");
    
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as () inherits ABC",
                        "Error starting statement: Supertype by name 'ABC' could not be found [");
    
            TryInvalid(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as () inherits",
                        "Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column ");
    
            _epService.EPAdministrator.Configuration.RemoveEventType("MyEventType", true);
        }
    
        [Test]
        public void TestDestroySameType() {
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("create schema MyEventType as (col1 string)");
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("create schema MyEventType as (col1 string)");
            
            stmtOne.Dispose();
            Assert.AreEqual(1, _epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyEventType").Count);
            Assert.IsTrue(_epService.EPAdministrator.Configuration.IsEventTypeExists("MyEventType"));
    
            stmtTwo.Dispose();
            Assert.AreEqual(0, _epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyEventType").Count);
            Assert.IsFalse(_epService.EPAdministrator.Configuration.IsEventTypeExists("MyEventType"));
        }
    
        [Test]
        public void TestColDefPlain()
        {
            RunAssertionColDefPlain(EventRepresentationEnum.DEFAULT);
            RunAssertionColDefPlain(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionColDefPlain(EventRepresentationEnum.MAP);

            // test property classname, either simple or fully-qualified.
            _epService.EPAdministrator.Configuration.AddImport<System.Data.DataRow>(); // "java.beans.EventHandler"
            _epService.EPAdministrator.Configuration.AddImport<System.DateTime>(); // "java.sql.*"
            _epService.EPAdministrator.CreateEPL("create schema MySchema (f1 DateTime, f2 System.Data.DataRow, f3 Action, f4 null)");

            EventType eventType = _epService.EPAdministrator.Configuration.GetEventType("MySchema");
            Assert.AreEqual(typeof(System.DateTime), eventType.GetPropertyType("f1"));
            Assert.AreEqual(typeof(System.Data.DataRow), eventType.GetPropertyType("f2"));
            Assert.AreEqual(typeof(System.Action), eventType.GetPropertyType("f3"));
            Assert.AreEqual(null, eventType.GetPropertyType("f4"));
        }
    
        private void RunAssertionColDefPlain(EventRepresentationEnum eventRepresentationEnum)
        {
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 string, col2 int, sbean " + typeof(SupportBean).FullName + ", col3.col4 int)");
            AssertTypeColDef(stmtCreate.EventType);
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType");
            AssertTypeColDef(stmtSelect.EventType);
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
    
            // destroy and create differently 
            stmtCreate = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col3 string, col4 int)");
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("col4"));
            Assert.AreEqual(2, stmtCreate.EventType.PropertyDescriptors.Count);
    
            stmtCreate.Stop();
    
            // destroy and create differently
            stmtCreate = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col5 string, col6 int)");
            Assert.AreEqual(stmtCreate.EventType.UnderlyingType, eventRepresentationEnum.GetOutputClass());
            Assert.AreEqual(typeof(int), stmtCreate.EventType.GetPropertyType("col6"));
            Assert.AreEqual(2, stmtCreate.EventType.PropertyDescriptors.Count);
            stmtSelect = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType");
            stmtSelect.Events += _listener.Update;
            Assert.AreEqual(stmtSelect.EventType.UnderlyingType, eventRepresentationEnum.GetOutputClass());
    
            // send event
            IDictionary<String, Object> data = new LinkedHashMap<String, Object>();
            data.Put("col5", "abc");
            data.Put("col6", 1);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(data.Values.ToArray(), "MyEventType");
            }
            else {
                _epService.EPRuntime.SendEvent(data, "MyEventType");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col5,col6".Split(','), new Object[]{"abc", 1});
            
            // assert type information
            EventTypeSPI typeSPI = (EventTypeSPI) stmtSelect.EventType;
            Assert.AreEqual(TypeClass.APPLICATION, typeSPI.Metadata.TypeClass);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PublicName);
            Assert.IsTrue(typeSPI.Metadata.IsApplicationConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfiguredStatic);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PrimaryName);
    
            // test non-enum create-schema
            String epl = "create" + eventRepresentationEnum.GetOutputTypeCreateSchemaName() + " schema MyEventTypeTwo as (col1 string, col2 int, sbean " + typeof(SupportBean).FullName + ", col3.col4 int)";
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL(epl);
            AssertTypeColDef(stmtCreateTwo.EventType);
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtCreateTwo.EventType.UnderlyingType);
            stmtCreateTwo.Dispose();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyEventTypeTwo", true);
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            stmtCreateTwo = _epService.EPAdministrator.Create(model);
            AssertTypeColDef(stmtCreateTwo.EventType);
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtCreateTwo.EventType.UnderlyingType);
    
            _epService.Initialize();
        }
    
        [Test]
        public void TestModelPOCO()
        {
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL("create schema SupportBeanOne as " + typeof(SupportBean).FullName);
            AssertTypeSupportBean(stmtCreateOne.EventType);
    
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL("create schema SupportBeanTwo as " + typeof(SupportBean).FullName);
            AssertTypeSupportBean(stmtCreateTwo.EventType);
    
            EPStatement stmtSelectOne = _epService.EPAdministrator.CreateEPL("select * from SupportBeanOne");
            AssertTypeSupportBean(stmtSelectOne.EventType);
            stmtSelectOne.Events += _listener.Update;
    
            EPStatement stmtSelectTwo = _epService.EPAdministrator.CreateEPL("select * from SupportBeanTwo");
            AssertTypeSupportBean(stmtSelectTwo.EventType);
            stmtSelectTwo.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetNewDataListFlattened(), "TheString,IntPrimitive".Split(','), new Object[][]{new Object[] {"E1", 2}, new Object[] {"E1", 2}});
    
            // assert type information
            EventTypeSPI typeSPI = (EventTypeSPI) stmtSelectOne.EventType;
            Assert.AreEqual(TypeClass.APPLICATION, typeSPI.Metadata.TypeClass);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PublicName);
            Assert.IsTrue(typeSPI.Metadata.IsApplicationConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfiguredStatic);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PrimaryName);
    
            // test keyword
            TryInvalid("create schema MySchema as com.mycompany.event.ABC",
                       "Error starting statement: Event type or class named 'com.mycompany.event.ABC' was not found [create schema MySchema as com.mycompany.event.ABC]");
            TryInvalid("create schema MySchema as com.mycompany.events.ABC",
                    "Error starting statement: Event type or class named 'com.mycompany.events.ABC' was not found [create schema MySchema as com.mycompany.events.ABC]");
        }
    
        [Test]
        public void TestNestableMapArray() {
            RunAssertionNestableMapArray(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionNestableMapArray(EventRepresentationEnum.MAP);
            RunAssertionNestableMapArray(EventRepresentationEnum.DEFAULT);
        }
    
        public void RunAssertionNestableMapArray(EventRepresentationEnum eventRepresentationEnum)
        {
            EPStatement stmtInner = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyInnerType as (col1 string[], col2 int[])");
            EventType inner = stmtInner.EventType;
            Assert.AreEqual(typeof(string[]), inner.GetPropertyType("col1"));
            Assert.IsTrue(inner.GetPropertyDescriptor("col1").IsIndexed);
            Assert.AreEqual(typeof(int[]), inner.GetPropertyType("col2"));
            Assert.IsTrue(inner.GetPropertyDescriptor("col2").IsIndexed);
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), inner.UnderlyingType);
    
            EPStatement stmtOuter = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyOuterType as (col1 MyInnerType, col2 MyInnerType[])");
            FragmentEventType type = stmtOuter.EventType.GetFragmentType("col1");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsFalse(type.IsIndexed);
            Assert.IsFalse(type.IsNative);
            type = stmtOuter.EventType.GetFragmentType("col2");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsTrue(type.IsIndexed);
            Assert.IsFalse(type.IsNative);
            
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL("select * from MyOuterType");
            stmtSelect.Events += _listener.Update;
            Assert.AreEqual(eventRepresentationEnum.GetOutputClass(), stmtSelect.EventType.UnderlyingType);
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                Object[] innerData = {"abc,def".Split(','), new int[] {1, 2}};
                Object[] outerData = {innerData, new Object[] {innerData, innerData}};
                _epService.EPRuntime.SendEvent(outerData, "MyOuterType");
            }
            else {
                IDictionary<String, Object> innerData = new Dictionary<String, Object>();
                innerData.Put("col1", "abc,def".Split(','));
                innerData.Put("col2", new int[] {1, 2});
                IDictionary<String, Object> outerData = new Dictionary<String, Object>();
                outerData.Put("col1", innerData);
                outerData.Put("col2", new DataMap[]{innerData, innerData});
                _epService.EPRuntime.SendEvent(outerData, "MyOuterType");
            }
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col1.col1[1],col2[1].col2[1]".Split(','), new Object[]{"def", 2});
    
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInnerType", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyOuterType", true);
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestInherit()
        {
            _epService.EPAdministrator.CreateEPL("create schema MyParentType as (col1 int, col2 string)");
            EPStatement stmtChild = _epService.EPAdministrator.CreateEPL("create schema MyChildTypeOne (col3 int) inherits MyParentType");
            Assert.AreEqual(typeof(int), stmtChild.EventType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(string), stmtChild.EventType.GetPropertyType("col2"));
            Assert.AreEqual(typeof(int), stmtChild.EventType.GetPropertyType("col3"));
    
            _epService.EPAdministrator.CreateEPL("create schema MyChildTypeTwo as (col4 boolean)");
            String createText = "create schema MyChildChildType as (col5 short, col6 long) inherits MyChildTypeOne, MyChildTypeTwo";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(createText);
            Assert.AreEqual(createText, model.ToEPL());
            EPStatement stmtChildChild = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(typeof(bool), stmtChildChild.EventType.GetPropertyType("col4"));
            Assert.AreEqual(typeof(int), stmtChildChild.EventType.GetPropertyType("col3"));
            Assert.AreEqual(typeof(short), stmtChildChild.EventType.GetPropertyType("col5"));
    
            EPStatement stmtChildChildTwo = _epService.EPAdministrator.CreateEPL("create schema MyChildChildTypeTwo () inherits MyChildTypeOne, MyChildTypeTwo");
            Assert.AreEqual(typeof(bool), stmtChildChildTwo.EventType.GetPropertyType("col4"));
            Assert.AreEqual(typeof(int), stmtChildChildTwo.EventType.GetPropertyType("col3"));
        }
    
        [Test]
        public void TestVariantType()
        {
            _epService.EPAdministrator.CreateEPL("create schema MyTypeZero as (col1 int, col2 string)");
            _epService.EPAdministrator.CreateEPL("create schema MyTypeOne as (col1 int, col3 string, col4 int)");
            _epService.EPAdministrator.CreateEPL("create schema MyTypeTwo as (col1 int, col4 boolean, col5 short)");
    
            EPStatement stmtChildPredef = _epService.EPAdministrator.CreateEPL("create variant schema MyVariantPredef as MyTypeZero, MyTypeOne");
            EventType variantTypePredef = stmtChildPredef.EventType;
            Assert.AreEqual(typeof(int?), variantTypePredef.GetPropertyType("col1"));
            Assert.AreEqual(1, variantTypePredef.PropertyDescriptors.Count);
    
            String createText = "create variant schema MyVariantAnyModel as MyTypeZero, MyTypeOne, *";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(createText);
            Assert.AreEqual(createText, model.ToEPL());
            EPStatement stmtChildAnyModel = _epService.EPAdministrator.Create(model);
            EventType predefAnyType = stmtChildAnyModel.EventType;
            Assert.AreEqual(4, predefAnyType.PropertyDescriptors.Count);
            Assert.AreEqual(typeof(Object), predefAnyType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(Object), predefAnyType.GetPropertyType("col2"));
            Assert.AreEqual(typeof(Object), predefAnyType.GetPropertyType("col3"));
            Assert.AreEqual(typeof(Object), predefAnyType.GetPropertyType("col4"));
    
            EPStatement stmtChildAny = _epService.EPAdministrator.CreateEPL("create variant schema MyVariantAny as *");
            EventType variantTypeAny = stmtChildAny.EventType;
            Assert.AreEqual(0, variantTypeAny.PropertyDescriptors.Count);
    
            _epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeZero");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeOne");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeTwo");
    
            _epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeZero");
            _epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeOne");
            try {
                _epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeTwo");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantPredef' [insert into MyVariantPredef select * from MyTypeTwo]", ex.Message);
            }
        }
    
        private void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.IsTrue(ex.Message.StartsWith(message), "Expected:\n" + message + "\nActual:\n" + ex.Message);
            }
        }
    
        private void AssertTypeSupportBean(EventType eventType) {
            Assert.AreEqual(typeof(SupportBean), eventType.UnderlyingType);
        }
    
        private void AssertTypeColDef(EventType eventType) {
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(int), eventType.GetPropertyType("col2"));
            Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("sbean"));
            Assert.AreEqual(typeof(int), eventType.GetPropertyType("col3.col4"));
            Assert.AreEqual(4, eventType.PropertyDescriptors.Count);
        }
    
        public class BeanSourceEvent
        {
            public BeanSourceEvent(SupportBean sb, SupportBean_S0[] s0Arr)
            {
                Sb = sb;
                S0Arr = s0Arr;
            }

            public SupportBean Sb { get; private set; }

            public SupportBean_S0[] S0Arr { get; private set; }
        }
    }
}
