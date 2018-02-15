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
            mapInner.Put("myInsideInt", 10);
            var topInner = new Dictionary<string, object>();
            topInner.Put("myInt", 1);
            topInner.Put("myString", "abc");
            topInner.Put("nested", mapInner);
            RunAssertion(epService, MAP_TYPENAME, FMAP, topInner);
    
            // Object-array
            var oaInner = new object[]{10};
            var oaTop = new object[]{1, "abc", oaInner};
            RunAssertion(epService, OA_TYPENAME, FOA, oaTop);
    
            // XML
            string xml = "<myevent myInt=\"1\" myString=\"abc\"><nested myInsideInt=\"10\"/></myevent>";
            RunAssertion(epService, XML_TYPENAME, FXML, xml);
    
            // Avro
            var schema = GetAvroSchema();
            var innerSchema = schema.GetField("nested").Schema.AsRecordSchema();
            var avroInner = new GenericRecord(innerSchema);
            avroInner.Put("myInsideInt", 10);
            var avro = new GenericRecord(schema);
            avro.Put("myInt", 1);
            avro.Put("myString", "abc");
            avro.Put("nested", avroInner);
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
            Assert.AreEqual("{\"myInt\":1,\"myString\":\"abc\",\"nested\":{\"myInsideInt\":10}}", json);
    
            XMLEventRenderer xmlEventRenderer = epService.EPRuntime.EventRenderer.GetXMLRenderer(statement.EventType);
            string xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
            Assert.AreEqual("<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><myInt>1</myInt><myString>abc</myString><nested><myInsideInt>10</myInsideInt></nested></root>", xml);
    
            statement.Dispose();
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            var inner = new LinkedHashMap<string, object>();
            inner.Put("myInsideInt", "int");
            var top = new LinkedHashMap<string, object>();
            top.Put("myInt", "int");
            top.Put("myString", "string");
            top.Put("nested", inner);
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, top);
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            var namesInner = new string[]{"myInsideInt"};
            var typesInner = new object[]{typeof(int)};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME + "_1", namesInner, typesInner);
    
            var names = new string[]{"myInt", "myString", "nested"};
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
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }
    
        private void AddAvroEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventTypeAvro(AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }
    
        private static RecordSchema GetAvroSchema() {
            Schema inner = SchemaBuilder.Record(AVRO_TYPENAME + "_inside",
                    TypeBuilder.Field("myInsideInt", TypeBuilder.IntType()));
    
            return SchemaBuilder.Record(AVRO_TYPENAME,
                    TypeBuilder.Field("myInt", TypeBuilder.IntType()),
                    TypeBuilder.Field("myString", TypeBuilder.StringType(
                            TypeBuilder.Property(PROP_STRING_KEY, PROP_STRING_VALUE))),
                    TypeBuilder.Field("nested", inner));
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
