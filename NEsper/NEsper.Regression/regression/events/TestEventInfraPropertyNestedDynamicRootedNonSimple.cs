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

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyNestedDynamicRootedNonSimple
    {
	    private readonly static Type BEAN_TYPE = typeof(SupportBeanDynRoot);

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
	        var NOT_EXISTS = ValueWithExistsFlag.MultipleNotExists(6);

	        // Bean
	        SupportBeanComplexProps inner = SupportBeanComplexProps.MakeDefaultBean();
            var beanTests = new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>[] {
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot("xxx"), NOT_EXISTS),
	            new Pair<SupportBeanDynRoot, ValueWithExistsFlag[]>(new SupportBeanDynRoot(inner), ValueWithExistsFlag.AllExist(
                    inner.GetIndexed(0), 
                    inner.GetIndexed(1),
                    inner.ArrayProperty[1],
                    inner.GetMapped("keyOne"),
                    inner.GetMapped("keyTwo"), 
                    inner.MapProperty.Get("xOne"))),
	        };
	        RunAssertion(BEAN_TYPE.Name, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        IDictionary<string, object> mapNestedOne = new Dictionary<string, object>();
	        mapNestedOne.Put("indexed", new int[] {1, 2});
	        mapNestedOne.Put("arrayProperty", null);
	        mapNestedOne.Put("mapped", SupportEventInfra.TwoEntryMap("keyOne", 100, "keyTwo", 200));
	        mapNestedOne.Put("mapProperty", null);
	        var mapOne = Collections.SingletonDataMap("item", mapNestedOne);
	        var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(Collections.EmptyDataMap, NOT_EXISTS),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(mapOne, new ValueWithExistsFlag[]
	            {
	                ValueWithExistsFlag.Exists(1), 
                    ValueWithExistsFlag.Exists(2), 
                    ValueWithExistsFlag.NotExists(), 
                    ValueWithExistsFlag.Exists(100), 
                    ValueWithExistsFlag.Exists(200), 
                    ValueWithExistsFlag.NotExists()
	            }),
	        };
	        RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object-Array
	        var oaNestedOne = new object[] {new int[] {1, 2}, SupportEventInfra.TwoEntryMap("keyOne", 100, "keyTwo", 200), new int[] {1000, 2000}, Collections.SingletonMap("xOne", "abc")};
	        var oaOne = new object[] {null, oaNestedOne};
	        var oaTests = new Pair<object[], ValueWithExistsFlag[]>[] {
	            new Pair<object[], ValueWithExistsFlag[]>(new object[] {null, null}, NOT_EXISTS),
	            new Pair<object[], ValueWithExistsFlag[]>(oaOne, ValueWithExistsFlag.AllExist(1, 2, 2000, 100, 200, "abc")),
	        };
	        RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

	        // XML
	        var xmlTests = new Pair<string, ValueWithExistsFlag[]>[] {
	            new Pair<string, ValueWithExistsFlag[]>("", NOT_EXISTS),
	            new Pair<string, ValueWithExistsFlag[]>(
                    "<item>" +
	                "<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>" +
	                "</item>", new ValueWithExistsFlag[]
	                {
	                    ValueWithExistsFlag.Exists("1"), 
                        ValueWithExistsFlag.Exists("2"), 
                        ValueWithExistsFlag.NotExists(), 
                        ValueWithExistsFlag.Exists("3"), 
                        ValueWithExistsFlag.Exists("4"), 
                        ValueWithExistsFlag.NotExists()
	                })
	        };
	        RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));

	        // Avro
            var schema = GetAvroSchema();
	        var itemSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(schema.GetField("item").Schema).AsRecordSchema();
	        var datumOne = new GenericRecord(schema);
	        datumOne.Put("item", null);
	        var datumItemTwo = new GenericRecord(itemSchema);
	        datumItemTwo.Put("indexed", Collections.List(1, 2));
	        datumItemTwo.Put("mapped", SupportEventInfra.TwoEntryMap("keyOne", 3, "keyTwo", 4));
	        var datumTwo = new GenericRecord(schema);
	        datumTwo.Put("item", datumItemTwo);
	        var avroTests = new Pair<GenericRecord, ValueWithExistsFlag[]>[] {
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(new GenericRecord(schema), NOT_EXISTS),
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(datumOne, NOT_EXISTS),
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(datumTwo, new ValueWithExistsFlag[]
	            {
	                ValueWithExistsFlag.Exists(1), 
                    ValueWithExistsFlag.Exists(2), 
                    ValueWithExistsFlag.NotExists(), 
                    ValueWithExistsFlag.Exists(3), 
                    ValueWithExistsFlag.Exists(4), 
                    ValueWithExistsFlag.NotExists()
	            }),
	        };
	        RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, null, avroTests, typeof(object));
	    }

        private void RunAssertion<T>(
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            IEnumerable<Pair<T, ValueWithExistsFlag[]>> tests,
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

	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var propertyNames = "indexed1,indexed2,array,mapped1,mapped2,map".SplitCsv();
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
	        var nestedName = SupportEventInfra.OA_TYPENAME+"_1";
	        string[] namesNested = {"indexed", "mapped", "arrayProperty", "mapProperty"};
	        object[] typesNested = {typeof(int[]), typeof(IDictionary<string, object>), typeof(int[]), typeof(IDictionary<string, object>)};
	        _epService.EPAdministrator.Configuration.AddEventType(nestedName, namesNested, typesNested);
	        string[] names = {"someprop", "item"};
	        object[] types = {typeof(string), nestedName};
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

	    private static RecordSchema GetAvroSchema() {
            Schema s1 = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_1",
                TypeBuilder.Field("indexed", TypeBuilder.Union(
                    TypeBuilder.Null(), TypeBuilder.Int(), TypeBuilder.Array(TypeBuilder.Int()))),
                TypeBuilder.Field("mapped", TypeBuilder.Union(
                    TypeBuilder.Null(), TypeBuilder.Int(), TypeBuilder.Map(TypeBuilder.Int()))));

            var temp = TypeBuilder.Record(SupportEventInfra.AVRO_TYPENAME, TypeBuilder.Field("item", TypeBuilder.Union(TypeBuilder.Int(), s1)));

            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.Field("item", TypeBuilder.Union(
                    TypeBuilder.Int(), s1)));
	    }
	}
} // end of namespace
