///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

using NEsper.Avro.Core;

using NEsper.Avro.Extensions;

using static com.espertech.esper.supportregression.events.SupportEventInfra;
using static com.espertech.esper.supportregression.events.ValueWithExistsFlag;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;

    public class ExecEventInfraPropertyNestedDynamicRootedSimple : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);
        private static readonly ValueWithExistsFlag[] NOT_EXISTS = MultipleNotExists(3);
    
        public override void Configure(Configuration configuration) {
            AddXMLEventType(configuration);
        }
    
        public override void Run(EPServiceProvider epService) {
            AddMapEventType(epService);
            AddOAEventType(epService);
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
            AddAvroEventType(epService);
    
            // Bean
            var beanTests = new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>[] {
                new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(SupportBeanComplexProps.MakeDefaultBean(), AllExist("Simple", "NestedValue", "NestedNestedValue")),
                new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(new SupportMarkerImplA("x"), NOT_EXISTS),
            };
            RunAssertion(epService, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));
    
            // Map
            Map mapNestedNestedOne = Collections.SingletonDataMap("nestedNestedValue", 101);
            Map mapNestedOne = TwoEntryMap("nestedNested", mapNestedNestedOne, "nestedValue", "abc");
            Map mapOne = TwoEntryMap("simpleProperty", 5, "nested", mapNestedOne);
            var mapTests = new Pair<Map, ValueWithExistsFlag[]>[]{
                    new Pair<Map, ValueWithExistsFlag[]>(Collections.SingletonDataMap("simpleProperty", "a"), new ValueWithExistsFlag[]{Exists("a"), NotExists(), NotExists()}),
                    new Pair<Map, ValueWithExistsFlag[]>(mapOne, AllExist(5, "abc", 101)),
            };
            RunAssertion(epService, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));
    
            // Object-Array
            var oaNestedNestedOne = new object[]{101};
            var oaNestedOne = new object[]{"abc", oaNestedNestedOne};
            var oaOne = new object[]{5, oaNestedOne};
            var oaTests = new Pair<object[], ValueWithExistsFlag[]>[]{
                    new Pair<object[], ValueWithExistsFlag[]>(new object[]{"a", null}, new ValueWithExistsFlag[]{Exists("a"), NotExists(), NotExists()}),
                    new Pair<object[], ValueWithExistsFlag[]>(oaOne, AllExist(5, "abc", 101)),
            };
            RunAssertion(epService, OA_TYPENAME, FOA, null, oaTests, typeof(object));
    
            // XML
            var xmlTests = new Pair<string, ValueWithExistsFlag[]>[]{
                    new Pair<string, ValueWithExistsFlag[]>(
                        "<simpleProperty>abc</simpleProperty>" +
                        "<nested nestedValue=\"100\">\n" +
                        "\t<nestedNested nestedNestedValue=\"101\">\n" +
                        "\t</nestedNested>\n" +
                        "</nested>\n", AllExist("abc", "100", "101")),
                    new Pair<string, ValueWithExistsFlag[]>("<nested/>", NOT_EXISTS),
            };
            RunAssertion(epService, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));
    
            // Avro
            var datumNull = new GenericRecord(GetAvroSchema());
            var schema = GetAvroSchema();
            var nestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(schema.GetField("nested").Schema);
            var nestedNestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(nestedSchema.GetField("nestedNested").Schema);
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema.AsRecordSchema());
            nestedNestedDatum.Put("nestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema.AsRecordSchema());
            nestedDatum.Put("nestedValue", 100);
            nestedDatum.Put("nestedNested", nestedNestedDatum);
            var datumOne = new GenericRecord(schema);
            datumOne.Put("simpleProperty", "abc");
            datumOne.Put("nested", nestedDatum);
            var avroTests = new Pair<GenericRecord, ValueWithExistsFlag[]>[]{
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME)), NOT_EXISTS),
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(datumNull, new ValueWithExistsFlag[]{Exists(null), NotExists(), NotExists()}),
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(datumOne, AllExist("abc", 100, 101)),
            };
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
        }

        private void RunAssertion<T>(
            EPServiceProvider epService,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T,ValueWithExistsFlag[]>[] tests,
            Type expectedPropertyType)
        {
            string stmtText = "select " +
                    "simpleProperty? as simple, " +
                    "exists(simpleProperty?) as exists_simple, " +
                    "nested?.nestedValue as nested, " +
                    "exists(nested?.nestedValue) as exists_nested, " +
                    "nested?.nestedNested.nestedNestedValue as nestedNested, " +
                    "exists(nested?.nestedNested.nestedNestedValue) as exists_nestedNested " +
                    "from " + typename;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] propertyNames = "simple,nested,nestedNested".Split(',');
            foreach (string propertyName in propertyNames) {
                Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
            }
    
            foreach (var pair in tests) {
                send.Invoke(epService, pair.First);
                SupportEventInfra.AssertValuesMayConvert(listener.AssertOneGetNewAndReset(), propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
            }
    
            stmt.Dispose();
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, Collections.EmptyDataMap);
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            string type_2 = OA_TYPENAME + "_2";
            string[] names_2 = {"nestedNestedValue"};
            object[] types_2 = {typeof(object)};
            epService.EPAdministrator.Configuration.AddEventType(type_2, names_2, types_2);
            string type_1 = OA_TYPENAME + "_1";
            string[] names_1 = {"nestedValue", "nestedNested"};
            object[] types_1 = {typeof(object), type_2};
            epService.EPAdministrator.Configuration.AddEventType(type_1, names_1, types_1);
            string[] names = {"simpleProperty", "nested"};
            object[] types = {typeof(object), type_1};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }
    
        private void AddXMLEventType(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            string schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "\t<xs:element name=\"myevent\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }
    
        private void AddAvroEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }
    
        private static RecordSchema GetAvroSchema() {
            RecordSchema s3 = SchemaBuilder.Record(AVRO_TYPENAME + "_3",
                TypeBuilder.OptionalInt("nestedNestedValue"));
            RecordSchema s2 = SchemaBuilder.Record(AVRO_TYPENAME + "_2",
                TypeBuilder.OptionalInt("nestedValue"),
                TypeBuilder.Field("nestedNested", TypeBuilder.Union(
                    TypeBuilder.IntType(), s3))
            );
            return SchemaBuilder.Record(AVRO_TYPENAME + "_1",
                TypeBuilder.Field("simpleProperty", TypeBuilder.Union(
                    TypeBuilder.IntType(), TypeBuilder.StringType())),
                TypeBuilder.Field("nested", TypeBuilder.Union(
                    TypeBuilder.IntType(), s2))
            );
        }
    }
} // end of namespace
