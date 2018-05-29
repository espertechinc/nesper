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
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

// using static com.espertech.esper.avro.core.AvroConstant.PROP_JAVA_STRING_KEY;
// using static com.espertech.esper.avro.core.AvroConstant.PROP_JAVA_STRING_VALUE;

using static com.espertech.esper.supportregression.events.SupportEventInfra;
using static com.espertech.esper.supportregression.events.ValueWithExistsFlag;

using static NEsper.Avro.Extensions.TypeBuilder;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;

    public class ExecEventInfraPropertyNestedDynamic : RegressionExecution
    {
        private static readonly string BEAN_TYPENAME = typeof(SupportBeanDynRoot).Name;

        private static readonly FunctionSendEvent FAVRO = (epService, value) =>
        {
            var schema = GetAvroSchema();
            var itemSchema = schema.GetField("item").Schema.AsRecordSchema();
            var itemDatum = new GenericRecord(itemSchema);
            itemDatum.Put("id", value);
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

            RunAssertion(epService, EventRepresentationChoice.ARRAY, "");
            RunAssertion(epService, EventRepresentationChoice.MAP, "");
            RunAssertion(
                epService, EventRepresentationChoice.AVRO,
                "@AvroSchemaField(Name='myid',Schema='[\"int\",{\"type\":\"string\",\"avro.string\":\"string\"},\"null\"]')");
            RunAssertion(epService, EventRepresentationChoice.DEFAULT, "");
        }

        private void RunAssertion(
            EPServiceProvider epService, EventRepresentationChoice outputEventRep, string additionalAnnotations)
        {

            // Bean
            var beanTests = new Pair<SupportBeanDynRoot, ValueWithExistsFlag>[]
            {
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(
                    new SupportBeanDynRoot(new SupportBean_S0(101)), Exists(101)),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(new SupportBeanDynRoot("abc"), NotExists()),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(
                    new SupportBeanDynRoot(new SupportBean_A("e1")), Exists("e1")),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(
                    new SupportBeanDynRoot(new SupportBean_B("e2")), Exists("e2")),
                new Pair<SupportBeanDynRoot, ValueWithExistsFlag>(
                    new SupportBeanDynRoot(new SupportBean_S1(102)), Exists(102))
            };
            RunAssertion(
                epService, outputEventRep, additionalAnnotations, BEAN_TYPENAME, FBEAN, null, beanTests,
                typeof(object));

            // Map
            var mapTests = new Pair<Map, ValueWithExistsFlag>[]
            {
                new Pair<Map, ValueWithExistsFlag>(Collections.EmptyDataMap, NotExists()),
                new Pair<Map, ValueWithExistsFlag>(
                    Collections.SingletonDataMap("item", Collections.SingletonDataMap("id", 101)), Exists(101)),
                new Pair<Map, ValueWithExistsFlag>(Collections.SingletonDataMap("item", Collections.EmptyDataMap), NotExists()),
            };
            RunAssertion(
                epService, outputEventRep, additionalAnnotations, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object array
            var oaTests = new Pair<object[], ValueWithExistsFlag>[]
            {
                new Pair<object[], ValueWithExistsFlag>(new object[] {null}, NotExists()),
                new Pair<object[], ValueWithExistsFlag>(new object[] {new SupportBean_S0(101)}, Exists(101)),
                new Pair<object[], ValueWithExistsFlag>(new object[] {"abc"}, NotExists()),
            };
            RunAssertion(
                epService, outputEventRep, additionalAnnotations, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            var xmlTests = new Pair<string, ValueWithExistsFlag>[]
            {
                new Pair<string, ValueWithExistsFlag>("<item id=\"101\"/>", Exists("101")),
                new Pair<string, ValueWithExistsFlag>("<item/>", NotExists()),
            };
            if (!outputEventRep.IsAvroEvent())
            {
                RunAssertion(epService, outputEventRep, additionalAnnotations, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));
            }

            // Avro
            var avroTests = new Pair<object, ValueWithExistsFlag>[]
            {
                new Pair<object, ValueWithExistsFlag>(null, Exists(null)),
                new Pair<object, ValueWithExistsFlag>(101, Exists(101)),
                new Pair<object, ValueWithExistsFlag>("abc", Exists("abc")),
            };
            RunAssertion(
                epService, outputEventRep, additionalAnnotations, AVRO_TYPENAME, FAVRO, null, avroTests,
                typeof(object));
        }

        private void RunAssertion<T>(
            EPServiceProvider epService,
            EventRepresentationChoice eventRepresentationEnum,
            string additionalAnnotations,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            IEnumerable<Pair<T, ValueWithExistsFlag>> tests,
            Type expectedPropertyType)
        {
            var stmtText = eventRepresentationEnum.GetAnnotationText() + additionalAnnotations + " select " +
                           "item.id? as myid, " +
                           "exists(item.id?) as exists_myid " +
                           "from " + typename;
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType("myid"));
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_myid").GetBoxedType());
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(stmt.EventType.UnderlyingType));

            foreach (var pair in tests)
            {
                send.Invoke(epService, pair.First);
                var @event = listener.AssertOneGetNewAndReset();
                AssertValueMayConvert(
                    @event, "myid", (ValueWithExistsFlag) pair.Second, optionalValueConversion);
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
            string[] names = {"item"};
            object[] types = {typeof(object)};
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

        private static RecordSchema GetAvroSchema() {
            var s1 = Record(
                AVRO_TYPENAME + "_1",
                Field(
                    "id",
                    Union(
                        IntType(),
                        StringType(Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE)),
                        NullType())));

            return SchemaBuilder.Record(
                AVRO_TYPENAME,
                Field("item", s1));
        }
    }
} // end of namespace
