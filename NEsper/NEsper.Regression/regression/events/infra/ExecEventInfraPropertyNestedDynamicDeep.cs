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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.supportregression.events.SupportEventInfra;
using static com.espertech.esper.supportregression.events.ValueWithExistsFlag;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;

    public class ExecEventInfraPropertyNestedDynamicDeep : RegressionExecution
    {
        private static readonly string BEAN_TYPENAME = typeof(SupportBeanDynRoot).Name;

        private static readonly FunctionSendEvent FAVRO = (epService, value) =>
        {
            var schema = GetAvroSchema().AsRecordSchema();
            var itemSchema = schema.GetField("item").Schema.AsRecordSchema();
            var itemDatum = new GenericRecord(itemSchema);
            itemDatum.Put("nested", value);
            var datum = new GenericRecord(schema);
            datum.Put("item", itemDatum);
            epService.EPRuntime.SendEventAvro(datum, AVRO_TYPENAME);
        };

        public override void Configure(Configuration configuration)
        {
            AddXMLEventType(configuration);
        }

        public override void Run(EPServiceProvider epService)
        {
            AddMapEventType(epService);
            AddOAEventType(epService);
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPENAME, typeof(SupportBeanDynRoot));
            AddAvroEventType(epService);

            var notExists = MultipleNotExists(6);

            // Bean
            var beanOne = SupportBeanComplexProps.MakeDefaultBean();
            var n1_v = beanOne.Nested.NestedValue;
            var n1_n_v = beanOne.Nested.NestedNested.NestedNestedValue;
            var beanTwo = SupportBeanComplexProps.MakeDefaultBean();
            beanTwo.Nested.NestedValue = "nested1";
            beanTwo.Nested.NestedNested.NestedNestedValue = "nested2";
            var beanTests = new[]
            {
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(
                    new SupportBeanDynRoot(beanOne), AllExist(n1_v, n1_v, n1_n_v, n1_n_v, n1_n_v, n1_n_v)),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(
                    new SupportBeanDynRoot(beanTwo),
                    AllExist("nested1", "nested1", "nested2", "nested2", "nested2", "nested2")),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot("abc"), notExists)
            };
            RunAssertion(epService, BEAN_TYPENAME, FBEAN, null, beanTests, typeof(object));

            // Map
            var mapOneL2 = new Dictionary<string, object>();
            mapOneL2.Put("nestedNestedValue", 101);
            var mapOneL1 = new Dictionary<string, object>();
            mapOneL1.Put("nestedNested", mapOneL2);
            mapOneL1.Put("nestedValue", 100);
            var mapOneL0 = new Dictionary<string, object>();
            mapOneL0.Put("nested", mapOneL1);
            var mapOne = Collections.SingletonDataMap("item", mapOneL0);
            var mapTests = new[]
            {
                new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(
                    mapOne, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(Collections.EmptyDataMap, notExists),
            };
            RunAssertion(epService, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            var oaOneL2 = new object[] {101};
            var oaOneL1 = new object[] {oaOneL2, 100};
            var oaOneL0 = new object[] {oaOneL1};
            var oaOne = new object[] {oaOneL0};
            var oaTests = new[]
            {
                new Pair<object[], ValueWithExistsFlag[]>(oaOne, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object[], ValueWithExistsFlag[]>(new object[] {null}, notExists),
            };
            RunAssertion(epService, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            var xmlTests = new[]
            {
                new Pair<string, ValueWithExistsFlag[]>(
                    "<item>\n" +
                    "\t<nested nestedValue=\"100\">\n" +
                    "\t\t<nestedNested nestedNestedValue=\"101\">\n" +
                    "\t\t</nestedNested>\n" +
                    "\t</nested>\n" +
                    "</item>\n", AllExist("100", "100", "101", "101", "101", "101")),
                new Pair<string, ValueWithExistsFlag[]>("<item/>", notExists),
            };
            RunAssertion(epService, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            Schema schema = GetAvroSchema();
            var nestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
                schema.GetField("item").Schema.GetField("nested").Schema).AsRecordSchema();
            var nestedNestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
                nestedSchema.GetField("nestedNested").Schema).AsRecordSchema();
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
            nestedNestedDatum.Put("nestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema);
            nestedDatum.Put("nestedValue", 100);
            nestedDatum.Put("nestedNested", nestedNestedDatum);
            var emptyDatum = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            var avroTests = new[]
            {
                new Pair<object, ValueWithExistsFlag[]>(nestedDatum, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object, ValueWithExistsFlag[]>(emptyDatum, notExists),
                new Pair<object, ValueWithExistsFlag[]>(null, notExists)
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
            RunAssertionSelectNested(epService, typename, send, optionalValueConversion, tests, expectedPropertyType);
            RunAssertionBeanNav(epService, typename, send, tests[0].First);
        }

        private void RunAssertionBeanNav(
            EPServiceProvider epService,
            string typename,
            FunctionSendEvent send,
            object underlyingComplete)
        {
            var stmtText = "select * from " + typename;

            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            send.Invoke(epService, underlyingComplete);
            var @event = listener.AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            stmt.Dispose();
        }

        private void RunAssertionSelectNested<T>(
            EPServiceProvider epService, string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T, ValueWithExistsFlag[]>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "select " +
                           " item.nested?.nestedValue as n1, " +
                           " exists(item.nested?.nestedValue) as exists_n1, " +
                           " item.nested?.nestedValue? as n2, " +
                           " exists(item.nested?.nestedValue?) as exists_n2, " +
                           " item.nested?.nestedNested.nestedNestedValue as n3, " +
                           " exists(item.nested?.nestedNested.nestedNestedValue) as exists_n3, " +
                           " item.nested?.nestedNested?.nestedNestedValue as n4, " +
                           " exists(item.nested?.nestedNested?.nestedNestedValue) as exists_n4, " +
                           " item.nested?.nestedNested.nestedNestedValue? as n5, " +
                           " exists(item.nested?.nestedNested.nestedNestedValue?) as exists_n5, " +
                           " item.nested?.nestedNested?.nestedNestedValue? as n6, " +
                           " exists(item.nested?.nestedNested?.nestedNestedValue?) as exists_n6 " +
                           " from " + typename;

            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var propertyNames = "n1,n2,n3,n4,n5,n6".Split(',');
            foreach (var propertyName in propertyNames)
            {
                Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
            }

            foreach (var pair in tests)
            {
                send.Invoke(epService, pair.First);
                var @event = listener.AssertOneGetNewAndReset();
                AssertValuesMayConvert(
                    @event, propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
            }

            stmt.Dispose();
        }

        private void AddMapEventType(EPServiceProvider epService)
        {
            var top = Collections.SingletonDataMap("item", typeof(Map));
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, top);
        }

        private void AddOAEventType(EPServiceProvider epService)
        {
            var type_3 = OA_TYPENAME + "_3";
            string[] names_3 = {"nestedNestedValue"};
            object[] types_3 = {typeof(object)};
            epService.EPAdministrator.Configuration.AddEventType(type_3, names_3, types_3);
            var type_2 = OA_TYPENAME + "_2";
            string[] names_2 = {"nestedNested", "nestedValue"};
            object[] types_2 = {type_3, typeof(object)};
            epService.EPAdministrator.Configuration.AddEventType(type_2, names_2, types_2);
            var type_1 = OA_TYPENAME + "_1";
            string[] names_1 = {"nested"};
            object[] types_1 = {type_2};
            epService.EPAdministrator.Configuration.AddEventType(type_1, names_1, types_1);
            string[] names = {"item"};
            object[] types = {type_1};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }

        private void AddXMLEventType(Configuration configuration)
        {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                         "\t<xs:element name=\"myevent\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t\t<xs:sequence>\n" +
                         "\t\t\t\t<xs:element ref=\"esper:item\"/>\n" +
                         "\t\t\t</xs:sequence>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "\t<xs:element name=\"item\">\n" +
                         "\t\t<xs:complexType>\n" +
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }

        private void AddAvroEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(
                AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }

        private static RecordSchema GetAvroSchema()
        {
            var s3 = SchemaBuilder.Record(AVRO_TYPENAME + "_3", TypeBuilder.OptionalInt("nestedNestedValue"));
            var s2 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_2", TypeBuilder.OptionalInt("nestedValue"),
                TypeBuilder.Field("nestedNested", TypeBuilder.Union(TypeBuilder.IntType(), s3)));
            var s1 = SchemaBuilder.Record(
                AVRO_TYPENAME + "_1",
                TypeBuilder.Field("nested", TypeBuilder.Union(TypeBuilder.IntType(), s2)));

            return SchemaBuilder.Record(AVRO_TYPENAME, TypeBuilder.Field("item", s1));
        }
    }
} // end of namespace
