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

    public class ExecEventInfraPropertyNestedDynamicRootedNonSimple : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(SupportBeanDynRoot);
    
        public override void Configure(Configuration configuration)
        {
            AddXMLEventType(configuration);
        }
    
        public override void Run(EPServiceProvider epService)
        {
            AddMapEventType(epService);
            AddOAEventType(epService);
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
            AddAvroEventType(epService);
    
            var notExists = MultipleNotExists(6);
    
            // Bean
            var inner = SupportBeanComplexProps.MakeDefaultBean();
            var beanTests = new[]{
                    new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(new SupportBeanDynRoot("xxx"), notExists),
                    new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(new SupportBeanDynRoot(inner), AllExist(
                        inner.GetIndexed(0), 
                        inner.GetIndexed(1), 
                        inner.ArrayProperty[1], 
                        inner.GetMapped("keyOne"), 
                        inner.GetMapped("keyTwo"), 
                        inner.MapProperty.Get("xOne")
                        )),
            };
            RunAssertion(epService, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));
    
            // Map
            var mapNestedOne = new Dictionary<string, object>();
            mapNestedOne.Put("indexed", new[]{1, 2});
            mapNestedOne.Put("arrayProperty", null);
            mapNestedOne.Put("mapped", TwoEntryMap("keyOne", 100, "keyTwo", 200));
            mapNestedOne.Put("mapProperty", null);
            var mapOne = Collections.SingletonDataMap("item", mapNestedOne);
            var mapTests = new[]{
                    new Pair<Map, ValueWithExistsFlag[]>(Collections.EmptyDataMap, notExists),
                    new Pair<Map, ValueWithExistsFlag[]>(mapOne, new[] {
                        Exists(1),
                        Exists(2),
                        NotExists(),
                        Exists(100),
                        Exists(200),
                        NotExists()
                    }),
            };
            RunAssertion(epService, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));
    
            // Object-Array
            var oaNestedOne = new object[]{new[]{1, 2}, TwoEntryMap("keyOne", 100, "keyTwo", 200), new[]{1000, 2000}, Collections.SingletonMap("xOne", "abc")};
            var oaOne = new object[]{null, oaNestedOne};
            var oaTests = new[]{
                    new Pair<object[], ValueWithExistsFlag[]>(new object[]{null, null}, notExists),
                    new Pair<object[], ValueWithExistsFlag[]>(oaOne, AllExist(1, 2, 2000, 100, 200, "abc")),
            };
            RunAssertion(epService, OA_TYPENAME, FOA, null, oaTests, typeof(object));
    
            // XML
            var xmlTests = new[]{
                    new Pair<string, ValueWithExistsFlag[]>("", notExists),
                    new Pair<string, ValueWithExistsFlag[]>("<item>" +
                            "<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>" +
                            "</item>", new[] {
                        Exists("1"),
                        Exists("2"),
                        NotExists(),
                        Exists("3"),
                        Exists("4"),
                        NotExists()
                    })
            };
            RunAssertion(epService, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));
    
            // Avro
            var schema = GetAvroSchema();
            var itemSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(schema.GetField("item").Schema);
            var datumOne = new GenericRecord(schema);
            datumOne.Put("item", null);
            var datumItemTwo = new GenericRecord(itemSchema.AsRecordSchema());
            datumItemTwo.Put("indexed", Collections.List(1, 2));
            datumItemTwo.Put("mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
            var datumTwo = new GenericRecord(schema);
            datumTwo.Put("item", datumItemTwo);
            var avroTests = new[]{
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(new GenericRecord(schema), notExists),
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(datumOne, notExists),
                    new Pair<GenericRecord, ValueWithExistsFlag[]>(datumTwo, new[] {
                        Exists(1),
                        Exists(2),
                        NotExists(),
                        Exists(3),
                        Exists(4),
                        NotExists()
                    }),
            };
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
        }

        private void RunAssertion<T>(
            EPServiceProvider epService,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T, ValueWithExistsFlag[]>[] tests,
            Type expectedPropertyType)
        {
    
            var stmtText = "select " +
                    "item?.indexed[0] as indexed1, " +
                    "exists(item?.indexed[0]) as exists_indexed1, " +
                    "item?.indexed[1]? as indexed2, " +
                    "exists(item?.indexed[1]?) as exists_indexed2, " +
                    "item?.arrayProperty[1]? as array, " +
                    "exists(item?.arrayProperty[1]?) as exists_array, " +
                    "item?.mapped('keyOne') as mapped1, " +
                    "exists(item?.mapped('keyOne')) as exists_mapped1, " +
                    "item?.mapped('keyTwo')? as mapped2,  " +
                    "exists(item?.mapped('keyTwo')?) as exists_mapped2,  " +
                    "item?.mapProperty('xOne')? as map, " +
                    "exists(item?.mapProperty('xOne')?) as exists_map " +
                    " from " + typename;
    
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var propertyNames = "indexed1,indexed2,array,mapped1,mapped2,map".Split(',');
            foreach (var propertyName in propertyNames) {
                Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
            }
    
            foreach (var pair in tests) {
                send.Invoke(epService, pair.First);
                AssertValuesMayConvert(listener.AssertOneGetNewAndReset(), propertyNames, pair.Second, optionalValueConversion);
            }
    
            stmt.Dispose();
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, Collections.EmptyDataMap);
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            var nestedName = OA_TYPENAME + "_1";
            string[] namesNested = {"indexed", "mapped", "arrayProperty", "mapProperty"};
            object[] typesNested = {typeof(int[]), typeof(Map), typeof(int[]), typeof(Map)};
            epService.EPAdministrator.Configuration.AddEventType(nestedName, namesNested, typesNested);
            string[] names = {"someprop", "item"};
            object[] types = {typeof(string), nestedName};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }
    
        private void AddXMLEventType(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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
    
        private static RecordSchema GetAvroSchema()
        {
            var s1 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_1",
                TypeBuilder.Field(
                    "indexed", TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(TypeBuilder.IntType())
                    )),
                TypeBuilder.Field(
                    "mapped", TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(TypeBuilder.IntType())
                    ))
            );
            return SchemaBuilder.Record(AVRO_TYPENAME,
                    TypeBuilder.Field("item", TypeBuilder.Union(TypeBuilder.IntType(), s1)));
        }
    }
} // end of namespace
