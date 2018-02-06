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
using com.espertech.esper.util.support;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyNestedDynamicDeep
    {
	    private static readonly string BEAN_TYPENAME = typeof(SupportBeanDynRoot).FullName;

	    private static readonly FunctionSendEvent FAVRO = (epService, value) => {
	        RecordSchema schema = GetAvroSchema().AsRecordSchema();
	        RecordSchema itemSchema = schema.GetField("item").Schema.AsRecordSchema();
	        var itemDatum = new GenericRecord(itemSchema);
	        itemDatum.Put("nested", value);
	        var datum = new GenericRecord(schema);
	        datum.Put("item", itemDatum);
            epService.EPRuntime.SendEventAvro(datum, SupportEventInfra.AVRO_TYPENAME);
	    };

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
	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPENAME, typeof(SupportBeanDynRoot));
	        AddAvroEventType();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestIt() {
	        var NOT_EXISTS = ValueWithExistsFlag.MultipleNotExists(6);

	        // Bean
	        SupportBeanComplexProps beanOne = SupportBeanComplexProps.MakeDefaultBean();
	        string n1_v = beanOne.Nested.NestedValue;
	        string n1_n_v = beanOne.Nested.NestedNested.NestedNestedValue;
	        SupportBeanComplexProps beanTwo = SupportBeanComplexProps.MakeDefaultBean();
	        beanTwo.Nested.NestedValue = "nested1";
            beanTwo.Nested.NestedNested.SetNestedNestedValue("nested2");
	        var beanTests = new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>[] {
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot(beanOne), ValueWithExistsFlag.AllExist(n1_v, n1_v, n1_n_v, n1_n_v, n1_n_v, n1_n_v)),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot(beanTwo), ValueWithExistsFlag.AllExist("nested1", "nested1", "nested2", "nested2", "nested2", "nested2")),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot("abc"), NOT_EXISTS)
	        };
            RunAssertion(BEAN_TYPENAME, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        IDictionary<string,object> mapOneL2 = new Dictionary<string, object>();
	        mapOneL2.Put("nestedNestedValue", 101);
	        IDictionary<string,object> mapOneL1 = new Dictionary<string, object>();
	        mapOneL1.Put("nestedNested", mapOneL2);
	        mapOneL1.Put("nestedValue", 100);
	        IDictionary<string,object> mapOneL0 = new Dictionary<string, object>();
	        mapOneL0.Put("nested", mapOneL1);
	        var mapOne = Collections.SingletonDataMap("item", mapOneL0);
            var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(mapOne, ValueWithExistsFlag.AllExist(100, 100, 101, 101, 101, 101)),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(Collections.EmptyDataMap, NOT_EXISTS),
	        };
            RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object-Array
	        var oaOneL2 = new object[] {101};
	        var oaOneL1 = new object[] {oaOneL2, 100};
	        var oaOneL0 = new object[] {oaOneL1};
	        var oaOne = new object[] {oaOneL0};
	        var oaTests = new Pair<object[], ValueWithExistsFlag[]>[] {
	            new Pair<object[], ValueWithExistsFlag[]>(oaOne, ValueWithExistsFlag.AllExist(100, 100, 101, 101, 101, 101)),
	            new Pair<object[], ValueWithExistsFlag[]>(new object[] {null}, NOT_EXISTS),
	        };
            RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

	        // XML
	        var xmlTests = new Pair<string, ValueWithExistsFlag[]>[] {
	            new Pair<string, ValueWithExistsFlag[]>("<item>\n" +
	                       "\t<nested nestedValue=\"100\">\n" +
	                       "\t\t<nestedNested nestedNestedValue=\"101\">\n" +
	                       "\t\t</nestedNested>\n" +
	                       "\t</nested>\n" +
	                       "</item>\n", ValueWithExistsFlag.AllExist("100", "100", "101", "101", "101", "101")),
	            new Pair<string, ValueWithExistsFlag[]>("<item/>", NOT_EXISTS),
	        };
            RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));

	        // Avro
	        var schema = GetAvroSchema();
	        var nestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
                schema.GetField("item").Schema.GetField("nested").Schema).AsRecordSchema();
	        var nestedNestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
                nestedSchema.GetField("nestedNested").Schema).AsRecordSchema();
	        var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
	        nestedNestedDatum.Put("nestedNestedValue", 101);
	        var nestedDatum = new GenericRecord(nestedSchema);
	        nestedDatum.Put("nestedValue", 100);
	        nestedDatum.Put("nestedNested", nestedNestedDatum);
            var emptyDatum = new GenericRecord(SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME));
	        var avroTests = new Pair<object, ValueWithExistsFlag[]>[] {
	            new Pair<object, ValueWithExistsFlag[]>(nestedDatum, ValueWithExistsFlag.AllExist(100, 100, 101, 101, 101, 101)),
	            new Pair<object, ValueWithExistsFlag[]>(emptyDatum, NOT_EXISTS),
	            new Pair<object, ValueWithExistsFlag[]>(null, NOT_EXISTS)
	        };
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
	    }

        private void RunAssertion<T>(
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<T, ValueWithExistsFlag[]>[] tests,
            Type expectedPropertyType)
        {
	        RunAssertionSelectNested(typename, send, optionalValueConversion, tests, expectedPropertyType);
	        RunAssertionBeanNav(typename, send, tests[0].First);
	    }

	    private void RunAssertionBeanNav(string typename, FunctionSendEvent send, object underlyingComplete)
        {
	        var stmtText = "select * from " + typename;

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        send.Invoke(_epService, underlyingComplete);
	        var @event = listener.AssertOneGetNewAndReset();
	        SupportEventTypeAssertionUtil.AssertConsistency(@event);

	        stmt.Dispose();
	    }

        private void RunAssertionSelectNested<T>(
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            IEnumerable<Pair<T, ValueWithExistsFlag[]>> tests,
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

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var propertyNames = "n1,n2,n3,n4,n5,n6".SplitCsv();
	        foreach (var propertyName in propertyNames) {
	            Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
	            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
	        }

	        foreach (var pair in tests) {
	            send.Invoke(_epService, pair.First);
	            var @event = listener.AssertOneGetNewAndReset();
	            SupportEventInfra.AssertValuesMayConvert(@event, propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
	        }

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
	        var top = Collections.SingletonDataMap("item", typeof(IDictionary<string, object>));
	        _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, top);
	    }

	    private void AddOAEventType() {
            var type_3 = SupportEventInfra.OA_TYPENAME + "_3";
	        string[] names_3 = {"nestedNestedValue"};
	        object[] types_3 = {typeof(object)};
	        _epService.EPAdministrator.Configuration.AddEventType(type_3, names_3, types_3);
            var type_2 = SupportEventInfra.OA_TYPENAME + "_2";
	        string[] names_2 = {"nestedNested", "nestedValue"};
	        object[] types_2 = {type_3, typeof(object)};
	        _epService.EPAdministrator.Configuration.AddEventType(type_2, names_2, types_2);
            var type_1 = SupportEventInfra.OA_TYPENAME + "_1";
	        string[] names_1 = {"nested"};
	        object[] types_1 = {type_2};
	        _epService.EPAdministrator.Configuration.AddEventType(type_1, names_1, types_1);
	        string[] names = {"item"};
	        object[] types = {type_1};
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
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
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType() {
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static Schema GetAvroSchema()
        {
            var s3 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_3", TypeBuilder.OptionalInt("nestedNestedValue"));
            var s2 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_2", TypeBuilder.OptionalInt("nestedValue"),
                TypeBuilder.Field("nestedNested", TypeBuilder.Union(TypeBuilder.Int(), s3)));
            var s1 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_1",
                TypeBuilder.Field("nested", TypeBuilder.Union(TypeBuilder.Int(), s2)));

            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME, TypeBuilder.Field("item", s1));
	    }
	}
} // end of namespace
