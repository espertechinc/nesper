///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using com.espertech.esper.util.support;
using Newtonsoft.Json;
using NEsper.Avro.Extensions;
using NEsper.Avro.IO;
using NEsper.Avro.Util.Support;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
using static NEsper.Avro.Extensions.TypeBuilder;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    using Map = IDictionary<string, object>;
    using Collection = ICollection<object>;

    public class ExecEPLSchema : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider epService) {
            RunAssertionSchemaArrayPrimitiveType(epService);
            RunAssertionSchemaWithEventType(epService);
            RunAssertionSchemaCopyProperties(epService);
            RunAssertionConfiguredNotRemoved(epService);
            RunAssertionDestroySameType(epService);
            RunAssertionAvroSchemaWAnnotation(epService);
            RunAssertionColDefPlain(epService);
            RunAssertionModelPono(epService);
            RunAssertionNestableMapArray(epService);
            RunAssertionInherit(epService);
            RunAssertionVariantType(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionSchemaArrayPrimitiveType(EPServiceProvider epService) {
            TryAssertionSchemaArrayPrimitiveType(epService, true);
            TryAssertionSchemaArrayPrimitiveType(epService, false);
    
            TryInvalid(epService, "create schema Invalid (x dummy[primitive])",
                    "Error starting statement: Type 'dummy' is not a primitive type [create schema Invalid (x dummy[primitive])]");
            TryInvalid(epService, "create schema Invalid (x int[dummy])",
                    "Column type keyword 'dummy' not recognized, expecting '[primitive]'");
        }
    
        private void TryAssertionSchemaArrayPrimitiveType(EPServiceProvider epService, bool soda) {
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create schema MySchema as (c0 int[primitive], c1 int[])");
            var expectedType = new object[][]{new object[] {"c0", typeof(int[])}, new object[] {"c1", typeof(int[])}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, epService.EPAdministrator.Configuration.GetEventType("MySchema"), SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
            epService.EPAdministrator.Configuration.RemoveEventType("MySchema", true);
        }
    
        private void RunAssertionSchemaWithEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("BeanSourceEvent", typeof(BeanSourceEvent));
            var theEvent = new BeanSourceEvent(
                new SupportBean("E1", 1),
                new SupportBean_S0[]{new SupportBean_S0(2)});
    
            // test schema
            var stmtSchema = epService.EPAdministrator.CreateEPL("create schema MySchema (bean SupportBean, beanarray SupportBean_S0[])");
            Assert.AreEqual(new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true), stmtSchema.EventType.GetPropertyDescriptor("bean"));
            Assert.AreEqual(new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true), stmtSchema.EventType.GetPropertyDescriptor("beanarray"));
    
            var stmtSchemaInsert = epService.EPAdministrator.CreateEPL("insert into MySchema select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            var listener = new SupportUpdateListener();
            stmtSchemaInsert.Events += listener.Update;
            epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new object[]{"E1", 2});
            stmtSchemaInsert.Dispose();
    
            // test named window
            var stmtWindow = epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as (bean SupportBean, beanarray SupportBean_S0[])");
            stmtWindow.Events += listener.Update;
            Assert.AreEqual(new EventPropertyDescriptor("bean", typeof(SupportBean), null, false, false, false, false, true), stmtWindow.EventType.GetPropertyDescriptor("bean"));
            Assert.AreEqual(new EventPropertyDescriptor("beanarray", typeof(SupportBean_S0[]), typeof(SupportBean_S0), false, false, true, false, true), stmtWindow.EventType.GetPropertyDescriptor("beanarray"));
    
            var stmtWindowInsert = epService.EPAdministrator.CreateEPL("insert into MyWindow select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new object[]{"E1", 2});
            stmtWindowInsert.Dispose();
    
            // insert pattern to named window
            var stmtWindowPattern = epService.EPAdministrator.CreateEPL("insert into MyWindow select sb as bean, s0Arr as beanarray from pattern [sb=SupportBean -> s0Arr=SupportBean_S0 until SupportBean_S0(id=0)]");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "S0_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id,beanarray[1].id".Split(','), new object[]{"E2", 10, 20});
            stmtWindowPattern.Dispose();
    
            // test configured Map type
            var configDef = new Dictionary<string, object>();
            configDef.Put("bean", "SupportBean");
            configDef.Put("beanarray", "SupportBean_S0[]");
            epService.EPAdministrator.Configuration.AddEventType("MyConfiguredMap", configDef);
    
            var stmtMapInsert = epService.EPAdministrator.CreateEPL("insert into MyConfiguredMap select sb as bean, s0Arr as beanarray from BeanSourceEvent");
            stmtMapInsert.Events += listener.Update;
            epService.EPRuntime.SendEvent(theEvent);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "bean.TheString,beanarray[0].id".Split(','), new object[]{"E1", 2});
            stmtMapInsert.Dispose();
        }
    
        private void RunAssertionSchemaCopyProperties(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionSchemaCopyProperties(epService, rep);
            }
        }
    
        private void TryAssertionSchemaCopyProperties(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema BaseOne (prop1 string, prop2 int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema BaseTwo (prop3 long)");
    
            // test define and send
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E1 () copyfrom BaseOne");
            var stmtOne = epService.EPAdministrator.CreateEPL("select * from E1");
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtOne.EventType.UnderlyingType));
            Assert.AreEqual(typeof(string), stmtOne.EventType.GetPropertyType("prop1"));
            Assert.AreEqual(typeof(int?), stmtOne.EventType.GetPropertyType("prop2").GetBoxedType());
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"v1", 2}, "E1");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> @event = new LinkedHashMap<string, object>();
                @event.Put("prop1", "v1");
                @event.Put("prop2", 2);
                epService.EPRuntime.SendEvent(@event, "E1");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var @event = new GenericRecord(SchemaBuilder.Record("name", 
                    RequiredString("prop1"), RequiredInt("prop2")));
                @event.Put("prop1", "v1");
                @event.Put("prop2", 2);
                epService.EPRuntime.SendEventAvro(@event, "E1");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "prop1,prop2".Split(','), new object[]{"v1", 2});
    
            // test two copy-from types
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E2 () copyfrom BaseOne, BaseTwo");
            var stmtTwo = epService.EPAdministrator.CreateEPL("select * from E2");
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("prop1"));
            Assert.AreEqual(typeof(int?), stmtTwo.EventType.GetPropertyType("prop2").GetBoxedType());
            Assert.AreEqual(typeof(long?), stmtTwo.EventType.GetPropertyType("prop3").GetBoxedType());
    
            // test API-defined type
            if (eventRepresentationEnum.IsMapEvent() ||
                eventRepresentationEnum.IsObjectArrayEvent()) {
                var def = new Dictionary<string, object>();
                def.Put("a", "string");
                def.Put("b", typeof(string));
                def.Put("c", "BaseOne");
                def.Put("d", "BaseTwo[]");
                epService.EPAdministrator.Configuration.AddEventType("MyType", def);
            } else {
                epService.EPAdministrator.CreateEPL("create avro schema MyType(a string, b string, c BaseOne, d BaseTwo[])");
            }
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema E3(e long, f BaseOne) copyfrom MyType");
            var stmtThree = epService.EPAdministrator.CreateEPL("select * from E3");
            Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("a"));
            Assert.AreEqual(typeof(string), stmtThree.EventType.GetPropertyType("b"));
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                Assert.AreEqual(typeof(object[]), stmtThree.EventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(object[][]), stmtThree.EventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(object[]), stmtThree.EventType.GetPropertyType("f"));
            } else if (eventRepresentationEnum.IsMapEvent()) {
                Assert.AreEqual(typeof(Map), stmtThree.EventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(Map[]), stmtThree.EventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(Map), stmtThree.EventType.GetPropertyType("f"));
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                Assert.AreEqual(typeof(GenericRecord), stmtThree.EventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(GenericRecord[]), stmtThree.EventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(GenericRecord), stmtThree.EventType.GetPropertyType("f"));
            } else {
                Assert.Fail();
            }
            Assert.AreEqual(typeof(long?), stmtThree.EventType.GetPropertyType("e").GetBoxedType());
    
            // invalid tests
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema E4(a long) copyFrom MyType",
                    "Error starting statement: Type by name 'MyType' contributes property 'a' defined as 'System.String' which overides the same property of type '" + Name.Clean<long>(false) + "' [");
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom MyType",
                    "Error starting statement: Property by name 'c' is defined twice by adding type 'MyType' [");
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema E4(c BaseTwo) copyFrom XYZ",
                    "Error starting statement: Type by name 'XYZ' could not be located [");
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema E4 as " + typeof(SupportBean).FullName + " copyFrom XYZ",
                    "Error starting statement: Copy-from types are not allowed with class-provided types [");
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create variant schema E4(c BaseTwo) copyFrom XYZ",
                    "Error starting statement: Copy-from types are not allowed with variant types [");
    
            // test SODA
            var createEPL = eventRepresentationEnum.GetAnnotationText() + " create schema EX as () copyFrom BaseOne, BaseTwo";
            var model = epService.EPAdministrator.CompileEPL(createEPL);
            Assert.AreEqual(createEPL.Trim(), model.ToEPL());
            var stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(createEPL.Trim(), stmt.Text);
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "BaseOne,BaseTwo,E1,E2,E3,MyType".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionConfiguredNotRemoved(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("MapType", new Dictionary<string, object>());
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            epService.EPAdministrator.Configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);
    
            epService.EPAdministrator.CreateEPL("create schema ABCType(col1 int, col2 int)");
            AssertTypeExists(epService, "ABCType", false);
    
            var moduleText = "select * from SupportBean;\n" +
                    "select * from MapType;\n" +
                    "select * from TestXMLNoSchemaType;\n";
            var result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(moduleText, "uri", "arch", null);
    
            AssertTypeExists(epService, "SupportBean", true);
            AssertTypeExists(epService, "MapType", true);
            AssertTypeExists(epService, "TestXMLNoSchemaType", true);
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
    
            AssertTypeExists(epService, "SupportBean", true);
            AssertTypeExists(epService, "MapType", true);
            AssertTypeExists(epService, "TestXMLNoSchemaType", true);
        }
    
        private void AssertTypeExists(EPServiceProvider epService, string typeName, bool isPreconfigured) {
            var spi = (EPServiceProviderSPI) epService;
            var type = (EventTypeSPI) spi.EventAdapterService.GetEventTypeByName(typeName);
            Assert.IsTrue(type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(isPreconfigured, type.Metadata.IsApplicationPreConfigured);
            Assert.IsFalse(type.Metadata.IsApplicationPreConfiguredStatic);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionInvalid(epService, rep);
            }
        }
    
        private void TryAssertionInvalid(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            var expectedOne = !eventRepresentationEnum.IsAvroEvent() ?
                    "Error starting statement: Nestable type configuration encountered an unexpected property type name 'xxxx' for property 'col1', expected Type or DataMap or the name of a previously-declared Map or ObjectArray type [" :
                    "Error starting statement: Type definition encountered an unexpected property type name 'xxxx' for property 'col1', expected the name of a previously-declared Avro type";
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 xxxx)", expectedOne);
    
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 int, col1 string)",
                    "Error starting statement: Duplicate column name 'col1' [");
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 string)");
            var expectedTwo = !eventRepresentationEnum.IsAvroEvent() ?
                    "Error starting statement: Event type named 'MyEventType' has already been declared with differing column name or type information: Type by name 'MyEventType' expects 1 properties but receives 2 properties [" :
                    "Error starting statement: Event type named 'MyEventType' has already been declared with differing column name or type information: Type by name 'MyEventType' is not a compatible type (target type underlying is '" + AvroConstantsNoDep.GENERIC_RECORD_CLASSNAME + "')";
            TryInvalid(epService, "create schema MyEventType as (col1 string, col2 string)", expectedTwo);
    
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT1 as () inherit ABC",
                    "Error in expression: Expected 'inherits', 'starttimestamp', 'endtimestamp' or 'copyfrom' keyword after create-schema clause but encountered 'inherit' [");
    
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT2 as () inherits ABC",
                    "Error starting statement: Supertype by name 'ABC' could not be found [");
    
            TryInvalid(epService, eventRepresentationEnum.GetAnnotationText() + " create schema MyEventTypeT3 as () inherits",
                    "Incorrect syntax near end-of-input expecting an identifier but found end-of-input at line 1 column ");
    
            epService.EPAdministrator.Configuration.RemoveEventType("MyEventType", true);
        }
    
        private void RunAssertionDestroySameType(EPServiceProvider epService) {
            var stmtOne = epService.EPAdministrator.CreateEPL("create schema MyEventTypeDST as (col1 string)");
            var stmtTwo = epService.EPAdministrator.CreateEPL("create schema MyEventTypeDST as (col1 string)");
    
            stmtOne.Dispose();
            Assert.AreEqual(1, epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyEventTypeDST").Count);
            Assert.IsTrue(epService.EPAdministrator.Configuration.IsEventTypeExists("MyEventTypeDST"));
    
            stmtTwo.Dispose();
            Assert.AreEqual(0, epService.EPAdministrator.Configuration.GetEventTypeNameUsedBy("MyEventTypeDST").Count);
            Assert.IsFalse(epService.EPAdministrator.Configuration.IsEventTypeExists("MyEventTypeDST"));
        }
    
        private void RunAssertionAvroSchemaWAnnotation(EPServiceProvider epService) {
            var schema = SchemaBuilder.Union(TypeBuilder.IntType(), TypeBuilder.StringType());
            var schemaAsString = SchemaToJsonEncoder.Encode(schema).ToString(Formatting.None);
            var epl = "@AvroSchemaField(Name='carId',Schema='" + schemaAsString + "') create avro schema MyEvent(carId object)";
            epService.EPAdministrator.CreateEPL(epl);
            Log.Info(schema.ToString());
        }
    
        private void RunAssertionColDefPlain(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionColDefPlain(epService, rep);
            }

            // test property classname, either simple or fully-qualified.
            epService.EPAdministrator.Configuration.AddImport<System.Data.DataRow>();
            epService.EPAdministrator.Configuration.AddImport<System.DateTime>();
            epService.EPAdministrator.CreateEPL("create schema MySchema (f1 DateTime, f2 System.Data.DataRow, f3 Action, f4 null)");
    
            var eventType = epService.EPAdministrator.Configuration.GetEventType("MySchema");
            Assert.AreEqual(typeof(System.DateTime), eventType.GetPropertyType("f1"));
            Assert.AreEqual(typeof(System.Data.DataRow), eventType.GetPropertyType("f2"));
            Assert.AreEqual(typeof(System.Action), eventType.GetPropertyType("f3"));
            Assert.AreEqual(null, eventType.GetPropertyType("f4"));

            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionColDefPlain(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            var stmtCreate = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col1 string, col2 int, col3_col4 int)");
            AssertTypeColDef(stmtCreate.EventType);
            var stmtSelect = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType");
            AssertTypeColDef(stmtSelect.EventType);
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
    
            // destroy and create differently
            stmtCreate = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col3 string, col4 int)");
            Assert.AreEqual(typeof(int?), stmtCreate.EventType.GetPropertyType("col4").GetBoxedType());
            Assert.AreEqual(2, stmtCreate.EventType.PropertyDescriptors.Count);
    
            stmtCreate.Stop();
    
            // destroy and create differently
            stmtCreate = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEventType as (col5 string, col6 int)");
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreate.EventType.UnderlyingType));
            Assert.AreEqual(typeof(int?), stmtCreate.EventType.GetPropertyType("col6").GetBoxedType());
            Assert.AreEqual(2, stmtCreate.EventType.PropertyDescriptors.Count);
            stmtSelect = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " select * from MyEventType");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtSelect.EventType.UnderlyingType));
    
            // send event
            if (eventRepresentationEnum.IsMapEvent()) {
                var data = new LinkedHashMap<string, object>();
                data.Put("col5", "abc");
                data.Put("col6", 1);
                epService.EPRuntime.SendEvent(data, "MyEventType");
            } else if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"abc", 1}, "MyEventType");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var schema = (RecordSchema) ((AvroSchemaEventType) epService.EPAdministrator.Configuration.GetEventType("MyEventType")).Schema;
                var @event = new GenericRecord(schema);
                @event.Put("col5", "abc");
                @event.Put("col6", 1);
                epService.EPRuntime.SendEventAvro(@event, "MyEventType");
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col5,col6".Split(','), new object[]{"abc", 1});
    
            // assert type information
            var typeSPI = (EventTypeSPI) stmtSelect.EventType;
            Assert.AreEqual(TypeClass.APPLICATION, typeSPI.Metadata.TypeClass);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PublicName);
            Assert.IsTrue(typeSPI.Metadata.IsApplicationConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfiguredStatic);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PrimaryName);
    
            // test non-enum create-schema
            var epl = "create" + eventRepresentationEnum.GetOutputTypeCreateSchemaName() + " schema MyEventTypeTwo as (col1 string, col2 int, col3_col4 int)";
            var stmtCreateTwo = epService.EPAdministrator.CreateEPL(epl);
            AssertTypeColDef(stmtCreateTwo.EventType);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreateTwo.EventType.UnderlyingType));
            stmtCreateTwo.Dispose();
            epService.EPAdministrator.Configuration.RemoveEventType("MyEventTypeTwo", true);
    
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            stmtCreateTwo = epService.EPAdministrator.Create(model);
            AssertTypeColDef(stmtCreateTwo.EventType);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtCreateTwo.EventType.UnderlyingType));
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (var name in "MyEventType,MyEventTypeTwo".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionModelPono(EPServiceProvider epService) {
            var stmtCreateOne = epService.EPAdministrator.CreateEPL("create schema SupportBeanOne as " + typeof(SupportBean_ST0).FullName);
            Assert.AreEqual(typeof(SupportBean_ST0), stmtCreateOne.EventType.UnderlyingType);
    
            var stmtCreateTwo = epService.EPAdministrator.CreateEPL("create schema SupportBeanTwo as " + typeof(SupportBean_ST0).FullName);
            Assert.AreEqual(typeof(SupportBean_ST0), stmtCreateTwo.EventType.UnderlyingType);
    
            var stmtSelectOne = epService.EPAdministrator.CreateEPL("select * from SupportBeanOne");
            Assert.AreEqual(typeof(SupportBean_ST0), stmtSelectOne.EventType.UnderlyingType);
            var listener = new SupportUpdateListener();
            stmtSelectOne.Events += listener.Update;
    
            var stmtSelectTwo = epService.EPAdministrator.CreateEPL("select * from SupportBeanTwo");
            Assert.AreEqual(typeof(SupportBean_ST0), stmtSelectTwo.EventType.UnderlyingType);
            stmtSelectTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetNewDataListFlattened(), "id,p00".Split(','),
                new object[][] {
                    new object[] {"E1", 2},
                    new object[] {"E1", 2}
                });
    
            // assert type information
            var typeSPI = (EventTypeSPI) stmtSelectOne.EventType;
            Assert.AreEqual(TypeClass.APPLICATION, typeSPI.Metadata.TypeClass);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PublicName);
            Assert.IsTrue(typeSPI.Metadata.IsApplicationConfigured);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfiguredStatic);
            Assert.IsFalse(typeSPI.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(typeSPI.Name, typeSPI.Metadata.PrimaryName);
    
            // test keyword
            TryInvalid(epService, "create schema MySchemaInvalid as com.mycompany.event.ABC",
                    "Error starting statement: Event type or class named 'com.mycompany.event.ABC' was not found");
            TryInvalid(epService, "create schema MySchemaInvalid as com.mycompany.events.ABC",
                    "Error starting statement: Event type or class named 'com.mycompany.events.ABC' was not found");
        }
    
        private void RunAssertionNestableMapArray(EPServiceProvider epService) {
            foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionNestableMapArray(epService, rep);
            }
        }
    
        private void TryAssertionNestableMapArray(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            var stmtInner = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyInnerType as (inn1 string[], inn2 int[])");
            var inner = stmtInner.EventType;
            Assert.AreEqual(typeof(string[]), inner.GetPropertyType("inn1"));
            Assert.IsTrue(inner.GetPropertyDescriptor("inn1").IsIndexed);
            Assert.AreEqual(typeof(int[]), inner.GetPropertyType("inn2"));
            Assert.IsTrue(inner.GetPropertyDescriptor("inn2").IsIndexed);
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(inner.UnderlyingType));
    
            var stmtOuter = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyOuterType as (col1 MyInnerType, col2 MyInnerType[])");
            var type = stmtOuter.EventType.GetFragmentType("col1");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsFalse(type.IsIndexed);
            Assert.IsFalse(type.IsNative);
            type = stmtOuter.EventType.GetFragmentType("col2");
            Assert.AreEqual("MyInnerType", type.FragmentType.Name);
            Assert.IsTrue(type.IsIndexed);
            Assert.IsFalse(type.IsNative);
    
            var stmtSelect = epService.EPAdministrator.CreateEPL("select * from MyOuterType");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmtSelect.EventType.UnderlyingType));
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                var innerData = new object[]{"abc,def".Split(','), new int[]{1, 2}};
                var outerData = new object[]{innerData, new object[]{innerData, innerData}};
                epService.EPRuntime.SendEvent(outerData, "MyOuterType");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var innerData = new Dictionary<string, object>();
                innerData.Put("inn1", "abc,def".Split(','));
                innerData.Put("inn2", new int[]{1, 2});
                var outerData = new Dictionary<string, object>();
                outerData.Put("col1", innerData);
                outerData.Put("col2", new Map[]{innerData, innerData});
                epService.EPRuntime.SendEvent(outerData, "MyOuterType");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var innerData = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "MyInnerType").AsRecordSchema());
                innerData.Put("inn1", Collections.List("abc", "def"));
                innerData.Put("inn2", Collections.List(1, 2));
                var outerData = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "MyOuterType").AsRecordSchema());
                outerData.Put("col1", innerData);
                outerData.Put("col2", Collections.List(innerData, innerData));
                epService.EPRuntime.SendEventAvro(outerData, "MyOuterType");
            } else {
                Assert.Fail();
            }
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1.inn1[1],col2[1].inn2[1]".Split(','), new object[]{"def", 2});
    
            epService.EPAdministrator.Configuration.RemoveEventType("MyInnerType", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MyOuterType", true);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInherit(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema MyParentType as (col1 int, col2 string)");
            var stmtChild = epService.EPAdministrator.CreateEPL("create schema MyChildTypeOne (col3 int) inherits MyParentType");
            Assert.AreEqual(typeof(int), stmtChild.EventType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(string), stmtChild.EventType.GetPropertyType("col2"));
            Assert.AreEqual(typeof(int), stmtChild.EventType.GetPropertyType("col3"));
    
            epService.EPAdministrator.CreateEPL("create schema MyChildTypeTwo as (col4 bool)");
            var createText = "create schema MyChildChildType as (col5 short, col6 long) inherits MyChildTypeOne, MyChildTypeTwo";
            var model = epService.EPAdministrator.CompileEPL(createText);
            Assert.AreEqual(createText, model.ToEPL());
            var stmtChildChild = epService.EPAdministrator.Create(model);
            Assert.AreEqual(typeof(bool), stmtChildChild.EventType.GetPropertyType("col4"));
            Assert.AreEqual(typeof(int), stmtChildChild.EventType.GetPropertyType("col3"));
            Assert.AreEqual(typeof(short), stmtChildChild.EventType.GetPropertyType("col5"));
    
            var stmtChildChildTwo = epService.EPAdministrator.CreateEPL("create schema MyChildChildTypeTwo () inherits MyChildTypeOne, MyChildTypeTwo");
            Assert.AreEqual(typeof(bool), stmtChildChildTwo.EventType.GetPropertyType("col4"));
            Assert.AreEqual(typeof(int), stmtChildChildTwo.EventType.GetPropertyType("col3"));
        }
    
        private void RunAssertionVariantType(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema MyTypeZero as (col1 int, col2 string)");
            epService.EPAdministrator.CreateEPL("create schema MyTypeOne as (col1 int, col3 string, col4 int)");
            epService.EPAdministrator.CreateEPL("create schema MyTypeTwo as (col1 int, col4 bool, col5 short)");
    
            var stmtChildPredef = epService.EPAdministrator.CreateEPL("create variant schema MyVariantPredef as MyTypeZero, MyTypeOne");
            var variantTypePredef = stmtChildPredef.EventType;
            Assert.AreEqual(typeof(int?), variantTypePredef.GetPropertyType("col1"));
            Assert.AreEqual(1, variantTypePredef.PropertyDescriptors.Count);
    
            var createText = "create variant schema MyVariantAnyModel as MyTypeZero, MyTypeOne, *";
            var model = epService.EPAdministrator.CompileEPL(createText);
            Assert.AreEqual(createText, model.ToEPL());
            var stmtChildAnyModel = epService.EPAdministrator.Create(model);
            var predefAnyType = stmtChildAnyModel.EventType;
            Assert.AreEqual(4, predefAnyType.PropertyDescriptors.Count);
            Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col2"));
            Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col3"));
            Assert.AreEqual(typeof(object), predefAnyType.GetPropertyType("col4"));
    
            var stmtChildAny = epService.EPAdministrator.CreateEPL("create variant schema MyVariantAny as *");
            var variantTypeAny = stmtChildAny.EventType;
            Assert.AreEqual(0, variantTypeAny.PropertyDescriptors.Count);
    
            epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeZero");
            epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeOne");
            epService.EPAdministrator.CreateEPL("insert into MyVariantAny select * from MyTypeTwo");
    
            epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeZero");
            epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeOne");
            try {
                epService.EPAdministrator.CreateEPL("insert into MyVariantPredef select * from MyTypeTwo");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantPredef' [insert into MyVariantPredef select * from MyTypeTwo]", ex.Message);
            }
        }
    
        private void AssertTypeColDef(EventType eventType)
        {
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("col1"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("col2").GetBoxedType());
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("col3_col4").GetBoxedType());
            Assert.AreEqual(3, eventType.PropertyDescriptors.Count);
        }
    
        public class BeanSourceEvent
        {
            public BeanSourceEvent(SupportBean sb, SupportBean_S0[] s0Arr)
            {
                Sb = sb;
                S0Arr = s0Arr;
            }

            public SupportBean Sb { get; }
            public SupportBean_S0[] S0Arr { get; }
        }
    }
} // end of namespace
