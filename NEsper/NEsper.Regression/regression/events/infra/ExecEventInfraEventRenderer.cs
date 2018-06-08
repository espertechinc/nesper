///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

using NEsper.Avro.Extensions;

using static NEsper.Avro.Core.AvroConstant;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    public class ExecEventInfraEventRenderer : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(ExecEventInfraEventRenderer.MyEvent);
    
        public override void Configure(Configuration configuration) {
            AddXMLEventType(configuration);
        }
    
        public override void Run(EPServiceProvider epService) {
            AddMapEventType(epService);
            AddOAEventType(epService);
            epService.EPAdministrator.Configuration.AddEventType(BEAN_TYPE);
            AddAvroEventType(epService);
    
            // Bean
            RunAssertion(epService, BEAN_TYPE.Name, FBEAN, new MyEvent(1, "abc", new MyInsideEvent(10)));
    
            // Map
            var mapInner = new Dictionary<string, object>();
            mapInner.Put("MyInsideInt", 10);
            var topInner = new Dictionary<string, object>();
            topInner.Put("MyInt", 1);
            topInner.Put("MyString", "abc");
            topInner.Put("Nested", mapInner);
            RunAssertion(epService, MAP_TYPENAME, FMAP, topInner);
    
            // Object-array
            var oaInner = new object[]{10};
            var oaTop = new object[]{1, "abc", oaInner};
            RunAssertion(epService, OA_TYPENAME, FOA, oaTop);
    
            // XML
            string xml = "<myevent MyInt=\"1\" MyString=\"abc\"><Nested MyInsideInt=\"10\"/></myevent>";
            RunAssertion(epService, XML_TYPENAME, FXML, xml);
    
            // Avro
            var schema = GetAvroSchema();
            var innerSchema = schema.GetField("Nested").Schema.AsRecordSchema();
            var avroInner = new GenericRecord(innerSchema);
            avroInner.Put("MyInsideInt", 10);
            var avro = new GenericRecord(schema);
            avro.Put("MyInt", 1);
            avro.Put("MyString", "abc");
            avro.Put("Nested", avroInner);
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, avro);
        }
    
        private void RunAssertion(
            EPServiceProvider epService, string typename, FunctionSendEvent send, Object @event)
        {
            string epl = "select * from " + typename;
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            send.Invoke(epService, @event);
    
            EventBean eventBean = listener.AssertOneGetNewAndReset();
    
            JSONEventRenderer jsonEventRenderer = epService.EPRuntime.EventRenderer.GetJSONRenderer(statement.EventType);
            string json = jsonEventRenderer.Render(eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            Assert.AreEqual("{\"MyInt\":1,\"MyString\":\"abc\",\"Nested\":{\"MyInsideInt\":10}}", json);
    
            XMLEventRenderer xmlEventRenderer = epService.EPRuntime.EventRenderer.GetXMLRenderer(statement.EventType);
            string xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            Assert.AreEqual("<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><MyInt>1</MyInt><MyString>abc</MyString><Nested><MyInsideInt>10</MyInsideInt></Nested></root>", xml);
    
            statement.Dispose();
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            var inner = new LinkedHashMap<string, object>();
            inner.Put("MyInsideInt", "int");
            var top = new LinkedHashMap<string, object>();
            top.Put("MyInt", "int");
            top.Put("MyString", "string");
            top.Put("Nested", inner);
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, top);
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            var namesInner = new string[]{"MyInsideInt"};
            var typesInner = new object[]{typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME + "_1", namesInner, typesInner);
    
            var names = new string[]{"MyInt", "MyString", "Nested"};
            var types = new object[]{typeof(int), typeof(string), OA_TYPENAME + "_1"};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }
    
        private void AddXMLEventType(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            string schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "\t<xs:element name=\"myevent\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence minOccurs=\"0\" maxOccurs=\"unbounded\">\n" +
                    "\t\t\t\t<xs:choice>\n" +
                    "\t\t\t\t\t<xs:element ref=\"esper:Nested\" minOccurs=\"1\" maxOccurs=\"1\"/>\n" +
                    "\t\t\t\t</xs:choice>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:attribute name=\"MyInt\" type=\"xs:int\" use=\"required\"/>\n" +
                    "\t\t\t<xs:attribute name=\"MyString\" type=\"xs:string\" use=\"required\"/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "\t<xs:element name=\"Nested\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:attribute name=\"MyInsideInt\" type=\"xs:int\" use=\"required\"/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>\n";
            eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }
    
        private void AddAvroEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }
    
        private static RecordSchema GetAvroSchema() {
            Schema inner = SchemaBuilder.Record(AVRO_TYPENAME + "_inside",
                    TypeBuilder.Field("MyInsideInt", TypeBuilder.IntType()));
    
            return SchemaBuilder.Record(AVRO_TYPENAME,
                    TypeBuilder.Field("MyInt", TypeBuilder.IntType()),
                    TypeBuilder.Field("MyString", TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                    TypeBuilder.Field("Nested", inner));
        }
    
        public sealed class MyInsideEvent {
            public MyInsideEvent(int myInsideInt) {
                this.MyInsideInt = myInsideInt;
            }

            public int MyInsideInt { get; }
        }
    
        public sealed class MyEvent {
            public MyEvent(int myInt, string myString, MyInsideEvent nested) {
                this.MyInt = myInt;
                this.MyString = myString;
                this.Nested = nested;
            }

            public int MyInt { get; }

            public string MyString { get; }

            public MyInsideEvent Nested { get; }
        }
    }
} // end of namespace
