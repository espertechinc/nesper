///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using NEsper.Avro;
using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyNestedDynamicRootedSimple
    {
	    private readonly static Type BEAN_TYPE = typeof(SupportMarkerInterface);

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
	        AddXMLEventType(configuration);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        AddMapEventType();
	        AddOAEventType();
	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
	        AddAvroEventType();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestIt() {
	        var NOT_EXISTS = ValueWithExistsFlag.MultipleNotExists(3);

	        // Bean
	        var beanTests = new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>[] {
	            new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(SupportBeanComplexProps.MakeDefaultBean(), ValueWithExistsFlag.AllExist("Simple", "NestedValue", "NestedNestedValue")),
	            new Pair<SupportMarkerInterface, ValueWithExistsFlag[]>(new SupportMarkerImplA("x"), NOT_EXISTS),
	        };
            RunAssertion(BEAN_TYPE.Name, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        var mapNestedNestedOne = Collections.SingletonDataMap("nestedNestedValue", 101);
            IDictionary<string, object> mapNestedOne = SupportEventInfra.TwoEntryMap("nestedNested", mapNestedNestedOne, "nestedValue", "abc");
            IDictionary<string, object> mapOne = SupportEventInfra.TwoEntryMap("simpleProperty", 5, "nested", mapNestedOne);
	        var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(Collections.SingletonDataMap("simpleProperty", "a"), new ValueWithExistsFlag[] {ValueWithExistsFlag.Exists("a"), ValueWithExistsFlag.NotExists(), ValueWithExistsFlag.NotExists()}),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(mapOne, ValueWithExistsFlag.AllExist(5, "abc", 101)),
	        };
            RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object-Array
	        var oaNestedNestedOne = new object[] {101};
	        var oaNestedOne = new object[] {"abc", oaNestedNestedOne};
	        var oaOne = new object[] {5, oaNestedOne};
	        var oaTests = new Pair<object[], ValueWithExistsFlag[]>[] {
	            new Pair<object[], ValueWithExistsFlag[]>(new object[] {"a", null}, new ValueWithExistsFlag[] {ValueWithExistsFlag.Exists("a"), ValueWithExistsFlag.NotExists(), ValueWithExistsFlag.NotExists()}),
	            new Pair<object[], ValueWithExistsFlag[]>(oaOne, ValueWithExistsFlag.AllExist(5, "abc", 101)),
	        };
            RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

	        // XML
            var xmlTests = new Pair<string, ValueWithExistsFlag[]>[]
            {
                new Pair<string, ValueWithExistsFlag[]>(
                    "<simpleProperty>abc</simpleProperty>" +
                    "<nested nestedValue=\"100\">\n" +
                    "\t<nestedNested nestedNestedValue=\"101\">\n" +
                    "\t</nestedNested>\n" +
                    "</nested>\n", ValueWithExistsFlag.AllExist("abc", "100", "101")),
                new Pair<string, ValueWithExistsFlag[]>("<nested/>", NOT_EXISTS),
            };
            RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            var schema = GetAvroSchema();
	        var datumNull = new GenericRecord(schema);
            var nestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(schema.GetField("nested").Schema).AsRecordSchema();
            var nestedNestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(nestedSchema.GetField("nestedNested").Schema).AsRecordSchema();
	        var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
	        nestedNestedDatum.Put("nestedNestedValue", 101);
	        var nestedDatum = new GenericRecord(nestedSchema);
	        nestedDatum.Put("nestedValue", 100);
	        nestedDatum.Put("nestedNested", nestedNestedDatum);
	        var datumOne = new GenericRecord(schema);
	        datumOne.Put("simpleProperty", "abc");
	        datumOne.Put("nested", nestedDatum);

	        var avroTests = new Pair<GenericRecord, ValueWithExistsFlag[]>[] {
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(new GenericRecord(SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME)), NOT_EXISTS),
                new Pair<GenericRecord, ValueWithExistsFlag[]>(datumNull, new ValueWithExistsFlag[] {ValueWithExistsFlag.Exists(null), ValueWithExistsFlag.NotExists(), ValueWithExistsFlag.NotExists()}),
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(datumOne, ValueWithExistsFlag.AllExist("abc", 100, 101)),
	        };
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, null, avroTests, typeof(object));
	    }

        private void RunAssertion<T>(
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T, ValueWithExistsFlag[]>[] tests,
            Type expectedPropertyType)
        {
	        var stmtText = "select " +
	                          "simpleProperty? as simple, "+
	                          "exists(simpleProperty?) as exists_simple, "+
	                          "nested?.nestedValue as nested, " +
	                          "exists(nested?.nestedValue) as exists_nested, " +
	                          "nested?.nestedNested.nestedNestedValue as nestedNested, " +
	                          "exists(nested?.nestedNested.nestedNestedValue) as exists_nestedNested " +
	                          "from " + typename;
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var propertyNames = "simple,nested,nestedNested".SplitCsv();
	        foreach (var propertyName in propertyNames) {
	            Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
	            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
	        }

	        foreach (var pair in tests) {
	            send.Invoke(_epService, pair.First);
	            SupportEventInfra.AssertValuesMayConvert(listener.AssertOneGetNewAndReset(), propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
	        }

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, Collections.EmptyDataMap);
	    }

	    private void AddOAEventType() {
            var type_2 = SupportEventInfra.OA_TYPENAME + "_2";
	        string[] names_2 = {"nestedNestedValue"};
	        object[] types_2 = {typeof(object)};
	        _epService.EPAdministrator.Configuration.AddEventType(type_2, names_2, types_2);
            var type_1 = SupportEventInfra.OA_TYPENAME + "_1";
	        string[] names_1 = {"nestedValue", "nestedNested"};
	        object[] types_1 = {typeof(object), type_2};
	        _epService.EPAdministrator.Configuration.AddEventType(type_1, names_1, types_1);
	        string[] names = {"simpleProperty", "nested"};
	        object[] types = {typeof(object), type_1};
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
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
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType() {
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static RecordSchema GetAvroSchema()
        {
            var s3 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_3", TypeBuilder.OptionalInt("nestedNestedValue"));
            var s2 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_2", TypeBuilder.OptionalInt("nestedValue"),
                TypeBuilder.Field("nestedNested", TypeBuilder.Union(TypeBuilder.Int(), s3)));
            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_1",
                TypeBuilder.Field("simpleProperty", TypeBuilder.Union(TypeBuilder.Int(), TypeBuilder.String())),
                TypeBuilder.Field("nested", TypeBuilder.Union(TypeBuilder.Int(), s2)));
	    }
	}
} // end of namespace
