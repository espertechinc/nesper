///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraEventRenderer
    {
	    private readonly static Type BEAN_TYPE = typeof(MyEvent);

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp() {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        AddXMLEventType(configuration);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        AddMapEventType(_epService);
	        AddOAEventType(_epService);
	        _epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
	        AddAvroEventType(_epService);

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);
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
	        // Bean
            RunAssertion(BEAN_TYPE.Name, SupportEventInfra.FBEAN, new MyEvent(1, "abc", new MyInsideEvent(10)));

	        // Map
	        IDictionary<string, object> mapInner = new Dictionary<string, object>();
	        mapInner.Put("myInsideInt", 10);
	        IDictionary<string, object> topInner = new Dictionary<string, object>();
	        topInner.Put("myInt", 1);
	        topInner.Put("myString", "abc");
	        topInner.Put("nested", mapInner);
            RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, topInner);

	        // Object-array
	        var oaInner = new object[] {10};
	        var oaTop = new object[] {1, "abc", oaInner};
            RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, oaTop);

	        // XML
	        var xml = "<myevent myInt=\"1\" myString=\"abc\"><nested myInsideInt=\"10\"/></myevent>";
            RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, xml);

	        // Avro
	        var schema = GetAvroSchema();
            var innerSchema = schema.GetField("nested").Schema.AsRecordSchema();
	        var avroInner = new GenericRecord(innerSchema);
	        avroInner.Put("myInsideInt", 10);
	        var avro = new GenericRecord(schema);
	        avro.Put("myInt", 1);
	        avro.Put("myString", "abc");
	        avro.Put("nested", avroInner);
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, avro);
	    }

	    private void RunAssertion(string typename, FunctionSendEvent send, object @event) {
	        var epl = "select * from " + typename;
	        var statement = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);

	        send.Invoke(_epService, @event);

	        var eventBean = listener.AssertOneGetNewAndReset();

	        var jsonEventRenderer = _epService.EPRuntime.EventRenderer.GetJSONRenderer(statement.EventType);
	        var json = jsonEventRenderer.Render(eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
	        var jsonExpected = string.Format("{{\"{0}\":1,\"{1}\":\"abc\",\"{2}\":{{\"{3}\":10}}}}",
	            "myInt", "myString", "nested", "myInsideInt");
            Assert.AreEqual(jsonExpected, json);

	        var xmlEventRenderer = _epService.EPRuntime.EventRenderer.GetXMLRenderer(statement.EventType);
	        var xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            var xmlExpected = string.Format("<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><{0}>1</{0}><{1}>abc</{1}><{2}><{3}>10</{3}></{2}></root>",
                "myInt", "myString", "nested", "myInsideInt");
            Assert.AreEqual(xmlExpected, xml);

	        statement.Dispose();
	    }

	    private void AddMapEventType(EPServiceProvider epService) {
	        IDictionary<string, object> inner = new LinkedHashMap<string, object>();
	        inner.Put("myInsideInt", "int");
	        IDictionary<string, object> top = new LinkedHashMap<string, object>();
	        top.Put("myInt", "int");
	        top.Put("myString", "string");
	        top.Put("nested", inner);
	        epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, top);
	    }

	    private void AddOAEventType(EPServiceProvider epService) {
	        var namesInner = new string[] {"myInsideInt"};
	        var typesInner = new object[] {typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME + "_1", namesInner, typesInner);

	        var names = new string[] {"myInt", "myString", "nested"};
            var types = new object[] { typeof(int), typeof(string), SupportEventInfra.OA_TYPENAME + "_1" };
            epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "myevent";
	        var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                        "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
	                        "\t<xs:element name=\"myevent\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t\t<xs:sequence minOccurs=\"0\" maxOccurs=\"unbounded\">\n" +
	                        "\t\t\t\t<xs:choice>\n" +
	                        "\t\t\t\t\t<xs:element ref=\"esper:nested\" minOccurs=\"1\" maxOccurs=\"1\"/>\n" +
	                        "\t\t\t\t</xs:choice>\n" +
	                        "\t\t\t</xs:sequence>\n" +
	                        "\t\t\t<xs:attribute name=\"myInt\" type=\"xs:int\" use=\"required\"/>\n" +
	                        "\t\t\t<xs:attribute name=\"myString\" type=\"xs:string\" use=\"required\"/>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "\t<xs:element name=\"nested\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t\t<xs:attribute name=\"myInsideInt\" type=\"xs:int\" use=\"required\"/>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "</xs:schema>\n";
	        eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static RecordSchema GetAvroSchema()
        {
            Schema inner = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME + "_inside",
                TypeBuilder.RequiredInt("myInsideInt"));

            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.RequiredInt("myInt"),
                TypeBuilder.Field("myString", TypeBuilder.Primitive("string", TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))),
                TypeBuilder.Field("nested", inner));

	    }

	    public class MyInsideEvent
        {
	        public MyInsideEvent(int myInsideInt) {
	            MyInsideInt = myInsideInt;
	        }

            [PropertyName("myInsideInt")]
	        public int MyInsideInt { get; private set; }
        }

	    public class MyEvent
        {
	        public MyEvent(int myInt, string myString, MyInsideEvent nested)
            {
	            MyInt = myInt;
	            MyString = myString;
	            Nested = nested;
	        }

            [PropertyName("myInt")]
	        public int MyInt { get; private set; }

            [PropertyName("myString")]
	        public string MyString { get; private set; }

            [PropertyName("nested")]
	        public MyInsideEvent Nested { get; private set; }
	    }

	}
} // end of namespace
