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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using NEsper.Avro;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyDynamicNonSimple
    {
	    private readonly static Type BEAN_TYPE = typeof(SupportBeanComplexProps);

	    private EPServiceProvider _epService;

        [SetUp]
	    public void SetUp() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
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
	        ValueWithExistsFlag[] NOT_EXISTS = ValueWithExistsFlag.MultipleNotExists(4);

            // Bean
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
	        var beanTests = new Pair<SupportBeanComplexProps, ValueWithExistsFlag[]>[] {
	            new Pair<SupportBeanComplexProps, ValueWithExistsFlag[]>(bean, ValueWithExistsFlag.AllExist(bean.GetIndexed(0), bean.GetIndexed(1), bean.GetMapped("keyOne"), bean.GetMapped("keyTwo")))
	        };
	        RunAssertion(BEAN_TYPE.Name, SupportEventInfra.FBEAN, null, beanTests, typeof(object));

	        // Map
	        var mapTests = new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>[] {
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(Collections.SingletonDataMap("somekey", "10"), NOT_EXISTS),
	            new Pair<IDictionary<string, object>, ValueWithExistsFlag[]>(SupportEventInfra.TwoEntryMap("indexed", new int[]{1, 2}, "mapped", SupportEventInfra.TwoEntryMap("keyOne", 3, "keyTwo", 4)), ValueWithExistsFlag.AllExist(1, 2, 3, 4)),
	        };
	        RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, null, mapTests, typeof(object));

	        // Object-Array
	        var oaTests = new Pair<object[], ValueWithExistsFlag[]>[] {
	            new Pair<object[], ValueWithExistsFlag[]>(new object[] {null, null}, NOT_EXISTS),
	            new Pair<object[], ValueWithExistsFlag[]>(new object[] {new int[] {1, 2}, SupportEventInfra.TwoEntryMap("keyOne", 3, "keyTwo", 4)}, ValueWithExistsFlag.AllExist(1, 2, 3, 4)),
	        };
	        RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, null, oaTests, typeof(object));

            // XML
            var xmlTests = new Pair<string, ValueWithExistsFlag[]>[] {
	            new Pair<string, ValueWithExistsFlag[]>("", NOT_EXISTS),
	            new Pair<string, ValueWithExistsFlag[]>("<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>", ValueWithExistsFlag.AllExist("1", "2", "3", "4"))
	        };
	        RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, SupportEventInfra.XML_TO_VALUE, xmlTests, typeof(XmlNode));

            // Avro
            var datumOne = new GenericRecord(SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME));
            var datumTwo = new GenericRecord(GetAvroSchema());

            datumTwo.Put("indexed", Collections.List(1, 2));
	        datumTwo.Put("mapped", SupportEventInfra.TwoEntryMap("keyOne", 3, "keyTwo", 4));
	        var avroTests = new Pair<GenericRecord, ValueWithExistsFlag[]>[] {
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(datumOne, NOT_EXISTS),
	            new Pair<GenericRecord, ValueWithExistsFlag[]>(datumTwo, ValueWithExistsFlag.AllExist(1, 2, 3, 4)),
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

	        string stmtText = "select " +
	                          "indexed[0]? as indexed1, " +
	                          "exists(indexed[0]?) as exists_indexed1, " +
	                          "indexed[1]? as indexed2, " +
	                          "exists(indexed[1]?) as exists_indexed2, " +
	                          "mapped('keyOne')? as mapped1, " +
	                          "exists(mapped('keyOne')?) as exists_mapped1, " +
	                          "mapped('keyTwo')? as mapped2,  " +
	                          "exists(mapped('keyTwo')?) as exists_mapped2  " +
	                          "from " + typename;

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        string[] propertyNames = "indexed1,indexed2,mapped1,mapped2".SplitCsv();
	        foreach (string propertyName in propertyNames) {
	            Assert.AreEqual(expectedPropertyType, stmt.EventType.GetPropertyType(propertyName));
	            Assert.AreEqual(typeof(bool?), stmt.EventType.GetPropertyType("exists_" + propertyName));
	        }

	        foreach (var pair in tests) {
	            send.Invoke(_epService, pair.First);
	            EventBean @event = listener.AssertOneGetNewAndReset();
	            SupportEventInfra.AssertValuesMayConvert(@event, propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
	        }

	        stmt.Dispose();
	    }

	    private void AddMapEventType() {
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, Collections.EmptyDataMap);
	    }

	    private void AddOAEventType() {
	        string[] names = {"indexed", "mapped"};
	        object[] types = {typeof(int[]), typeof(IDictionary<string, object>)};
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
	        ConfigurationEventTypeXMLDOM eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "myevent";
	        string schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                        "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
	                        "\t<xs:element name=\"myevent\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "</xs:schema>\n";
	        eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType()
	    {
            _epService.EPAdministrator.Configuration.AddEventTypeAvro(
                SupportEventInfra.AVRO_TYPENAME,
                new ConfigurationEventTypeAvro(
                    SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME)));
	    }

        private static RecordSchema GetAvroSchema()
        {
            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.Field("indexed", TypeBuilder.Union(
                    TypeBuilder.Null(),
                    TypeBuilder.Int(),
                    TypeBuilder.Array(TypeBuilder.Int()))),
                TypeBuilder.Field("mapped", TypeBuilder.Union(
                    TypeBuilder.Null(),
                    TypeBuilder.Int(),
                    TypeBuilder.Map(TypeBuilder.Int()))));
        }
    }
} // end of namespace
