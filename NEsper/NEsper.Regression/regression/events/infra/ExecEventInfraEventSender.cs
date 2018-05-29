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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NEsper.Avro.Extensions;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    using Map = IDictionary<string, object>;

    public class ExecEventInfraEventSender : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            AddXMLEventType(configuration);
        }
    
        public override void Run(EPServiceProvider epService) {
            AddMapEventType(epService);
            AddOAEventType(epService);
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("Marker", typeof(SupportMarkerInterface));
            AddAvroEventType(epService);
    
            // Bean
            RunAssertionSuccess(epService, typeof(SupportBean).FullName, new SupportBean());
            RunAssertionInvalid(epService, typeof(SupportBean).FullName, new SupportBean_G("G1"),
                    "Event object of type " + typeof(SupportBean_G).FullName + " does not equal, extend or implement the type " + typeof(SupportBean).FullName + " of event type 'SupportBean'");
            RunAssertionSuccess(epService, "Marker", new SupportMarkerImplA("Q2"), new SupportBean_G("Q3"));
    
            // Map
            RunAssertionSuccess(epService, MAP_TYPENAME, new Dictionary<string, object>());
            RunAssertionInvalid(epService, MAP_TYPENAME, new SupportBean(),
                "Unexpected event object of type " + Name.Clean<SupportBean>() + ", expected " + Name.Clean<Map>());
    
            // Object-Array
            RunAssertionSuccess(epService, OA_TYPENAME, new object[]{});
            RunAssertionInvalid(epService, OA_TYPENAME, new SupportBean(),
                "Unexpected event object of type " + Name.Clean<SupportBean>() + ", expected " + Name.Clean<object[]>());
    
            // XML
            RunAssertionSuccess(epService, XML_TYPENAME, SupportXML.GetDocument("<myevent/>").DocumentElement);
            RunAssertionInvalid(epService, XML_TYPENAME, new SupportBean(),
                    "Unexpected event object type '" + typeof(SupportBean).FullName + "' encountered, please supply a XmlDocument or XmlElement node");
            RunAssertionInvalid(epService, XML_TYPENAME, SupportXML.GetDocument("<xxxx/>"),
                    "Unexpected root element name 'xxxx' encountered, expected a root element name of 'myevent'");
    
            // Avro
            RunAssertionSuccess(epService, AVRO_TYPENAME, new GenericRecord(GetAvroSchema()));
            RunAssertionInvalid(epService, AVRO_TYPENAME, new SupportBean(),
                    "Unexpected event object type '" + typeof(SupportBean).FullName + "' encountered, please supply a GenericRecord");
    
            // No such type
            try {
                epService.EPRuntime.GetEventSender("ABC");
                Assert.Fail();
            } catch (EventTypeException ex) {
                Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
            }
    
            // Internal implicit wrapper type
            epService.EPAdministrator.CreateEPL("insert into ABC select *, TheString as value from SupportBean");
            try {
                epService.EPRuntime.GetEventSender("ABC");
                Assert.Fail("Event type named 'ABC' could not be found");
            } catch (EventTypeException ex) {
                Assert.AreEqual("An event sender for event type named 'ABC' could not be created as the type is internal", ex.Message);
            }
        }
    
        private void RunAssertionSuccess(EPServiceProvider epService,
                                         string typename,
                                         params object[] correctUnderlyings) {
    
            string stmtText = "select * from " + typename;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            EventSender sender = epService.EPRuntime.GetEventSender(typename);
            foreach (Object underlying in correctUnderlyings) {
                sender.SendEvent(underlying);
                Assert.AreSame(underlying, listener.AssertOneGetNewAndReset().Underlying);
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService,
                                         string typename,
                                         Object incorrectUnderlying,
                                         string message) {
            EventSender sender = epService.EPRuntime.GetEventSender(typename);
    
            try {
                sender.SendEvent(incorrectUnderlying);
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }
    
        private void AddMapEventType(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(MAP_TYPENAME, Collections.EmptyDataMap);
        }
    
        private void AddOAEventType(EPServiceProvider epService) {
            string[] names = {};
            object[] types = {};
            epService.EPAdministrator.Configuration.AddEventType(OA_TYPENAME, names, types);
        }
    
        private void AddXMLEventType(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "myevent";
            string schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                    "\t<xs:element name=\"myevent\">\n" +
                    "\t\t<xs:complexType>\n" +
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
            return SchemaBuilder.Record(AVRO_TYPENAME);
        }
    }
} // end of namespace
