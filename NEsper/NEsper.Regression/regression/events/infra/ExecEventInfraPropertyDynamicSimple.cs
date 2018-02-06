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

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.supportregression.events.SupportEventInfra;
using static com.espertech.esper.supportregression.events.ValueWithExistsFlag;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;
    using PairBean = Pair<SupportMarkerInterface, ValueWithExistsFlag>;
    using PairMap = Pair<IDictionary<string, object>, ValueWithExistsFlag>;
    using PairArray = Pair<object[], ValueWithExistsFlag>;
    using PairAvro = Pair<GenericRecord, ValueWithExistsFlag>;
    using PairXML = Pair<string, ValueWithExistsFlag>;

    public class ExecEventInfraPropertyDynamicSimple : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);

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

            // Bean
            var beanTests = new[]
            {
                new PairBean(new SupportMarkerImplA("e1"), Exists("e1")),
                new PairBean(new SupportMarkerImplB(1), Exists(1)),
                new PairBean(new SupportMarkerImplC(), NotExists())
            };
            RunAssertion(epService, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));

            // Map
            var mapTests = new[]
            {
                new PairMap(Collections.SingletonDataMap("somekey", "10"), NotExists()),
                new PairMap(Collections.SingletonDataMap("id", "abc"), Exists("abc")),
                new PairMap(Collections.SingletonDataMap("id", 10), Exists(10))
            };
            RunAssertion(epService, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            var oaTests = new[]
            {
                new PairArray(new object[] {1, null}, Exists(null)),
                new PairArray(new object[] {2, "abc"}, Exists("abc")),
                new PairArray(new object[] {3, 10}, Exists(10))
            };
            RunAssertion(epService, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            var xmlTests = new[]
            {
                new PairXML("", NotExists()),
                new PairXML("<id>10</id>", Exists("10")),
                new PairXML("<id>abc</id>", Exists("abc"))
            };
            RunAssertion(epService, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            var datumEmpty = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            var datumOne = new GenericRecord(GetAvroSchema());
            datumOne.Put("id", 101);
            var datumTwo = new GenericRecord(GetAvroSchema());
            datumTwo.Put("id", null);
            var avroTests = new[]
            {
                new PairAvro(datumEmpty, NotExists()),
                new PairAvro(datumOne, Exists(101)),
                new PairAvro(datumTwo, Exists(null))
            };
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
        }

        private void RunAssertion<T>(
            EPServiceProvider epService,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T, ValueWithExistsFlag>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "select id? as myid, Exists(id?) as exists_myid from " + typename;
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType("myid"));
            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_myid"));

            foreach (var pair in tests)
            {
                send.Invoke(epService, pair.First);
                var @event = listener.AssertOneGetNewAndReset();
                AssertValueMayConvert(@event, "myid", pair.Second, optionalValueConversion);
            }

            stmt.Dispose();
        }

        private void AddMapEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, Collections.EmptyDataMap);
        }

        private void AddOAEventType(EPServiceProvider epService)
        {
            string[] names = {"somefield", "id"};
            object[] types = {typeof(object), typeof(object)};
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
                         "\t\t</xs:complexType>\n" +
                         "\t</xs:element>\n" +
                         "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }

        private void AddAvroEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(
                AVRO_TYPENAME, new ConfigurationEventTypeAvro(SchemaBuilder.Record(AVRO_TYPENAME)));
        }

        private static RecordSchema GetAvroSchema()
        {
            return SchemaBuilder.Record(
                AVRO_TYPENAME,
                TypeBuilder.Field(
                    "id", TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.BooleanType())));
        }
    }
} // end of namespace