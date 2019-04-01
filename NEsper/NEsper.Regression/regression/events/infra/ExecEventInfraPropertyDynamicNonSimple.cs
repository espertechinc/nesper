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
    using PairBean = Pair<SupportBeanComplexProps, ValueWithExistsFlag[]>;
    using PairMap = Pair<IDictionary<string, object>, ValueWithExistsFlag[]>;
    using PairArray = Pair<object[], ValueWithExistsFlag[]>;
    using PairAvro = Pair<GenericRecord, ValueWithExistsFlag[]>;
    using PairXML = Pair<string, ValueWithExistsFlag[]>;

    public class ExecEventInfraPropertyDynamicNonSimple : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(SupportBeanComplexProps);

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

            var notExists = MultipleNotExists(4);

            // Bean
            var bean = SupportBeanComplexProps.MakeDefaultBean();
            var beanTests = new[]
            {
                new PairBean(
                    bean,
                    AllExist(
                        bean.GetIndexed(0), bean.GetIndexed(1), bean.GetMapped("keyOne"), bean.GetMapped("keyTwo")))
            };
            RunAssertion(epService, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));

            // Map
            var mapTests = new[]
            {
                new PairMap(Collections.SingletonDataMap("somekey", "10"), notExists),
                new PairMap(
                    TwoEntryMap("indexed", new[] {1, 2}, "mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4)),
                    AllExist(1, 2, 3, 4))
            };
            RunAssertion(epService, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            var oaTests = new[]
            {
                new PairArray(new object[] {null, null}, notExists),
                new PairArray(new object[] {new[] {1, 2}, TwoEntryMap("keyOne", 3, "keyTwo", 4)}, AllExist(1, 2, 3, 4))
            };
            RunAssertion(epService, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            var xmlTests = new[]
            {
                new PairXML("", notExists),
                new PairXML(
                    "<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>",
                    AllExist("1", "2", "3", "4"))
            };
            RunAssertion(epService, XML_TYPENAME, FXML, XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            var datumOne = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            var datumTwo = new GenericRecord(GetAvroSchema());
            datumTwo.Put("indexed", Collections.List(1, 2));
            datumTwo.Put("mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
            var avroTests = new[]
            {
                new PairAvro(datumOne, notExists),
                new PairAvro(datumTwo, AllExist(1, 2, 3, 4))
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
                           "indexed[0]? as indexed1, " +
                           "exists(indexed[0]?) as exists_indexed1, " +
                           "indexed[1]? as indexed2, " +
                           "exists(indexed[1]?) as exists_indexed2, " +
                           "mapped('keyOne')? as mapped1, " +
                           "exists(mapped('keyOne')?) as exists_mapped1, " +
                           "mapped('keyTwo')? as mapped2,  " +
                           "exists(mapped('keyTwo')?) as exists_mapped2  " +
                           "from " + typename;

            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var propertyNames = "indexed1,indexed2,mapped1,mapped2".Split(',');
            foreach (var propertyName in propertyNames)
            {
                Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
            }

            foreach (var pair in tests)
            {
                send.Invoke(epService, pair.First);
                var @event = listener.AssertOneGetNewAndReset();
                AssertValuesMayConvert(@event, propertyNames, pair.Second, optionalValueConversion);
            }

            stmt.Dispose();
        }

        private void AddMapEventType(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, Collections.EmptyDataMap);
        }

        private void AddOAEventType(EPServiceProvider epService)
        {
            string[] names = {"indexed", "mapped"};
            object[] types = {typeof(int[]), typeof(Map)};
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
                AVRO_TYPENAME, new ConfigurationEventTypeAvro(
                    SchemaBuilder.Record(AVRO_TYPENAME)));
        }

        private static RecordSchema GetAvroSchema()
        {
            return SchemaBuilder.Record(
                AVRO_TYPENAME,
                TypeBuilder.Field(
                    "indexed", TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Array(TypeBuilder.IntType()))),
                TypeBuilder.Field(
                    "mapped", TypeBuilder.Union(
                        TypeBuilder.NullType(),
                        TypeBuilder.IntType(),
                        TypeBuilder.Map(TypeBuilder.IntType())))
            );
        }
    }
} // end of namespace