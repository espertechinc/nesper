///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.supportregression.bean;

using NUnit.Framework;

using NEsper.Avro.Extensions;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraEventSender
    {
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
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        _epService.EPAdministrator.Configuration.AddEventType("Marker", typeof(SupportMarkerInterface));
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
	    public void TestIt()
        {
            // Bean
	        RunAssertionSuccess(typeof(SupportBean).FullName, new SupportBean());
	        RunAssertionInvalid(typeof(SupportBean).FullName, new SupportBean_G("G1"),
	                            "Event object of type " + typeof(SupportBean_G).FullName + " does not equal, extend or implement the type " + typeof(SupportBean).FullName + " of event type 'SupportBean'");
	        RunAssertionSuccess("Marker", new SupportMarkerImplA("Q2"), new SupportBean_G("Q3"));

	        // Map
            RunAssertionSuccess(SupportEventInfra.MAP_TYPENAME, new Dictionary<string, object>());
            RunAssertionInvalid(SupportEventInfra.MAP_TYPENAME, new SupportBean(),
	                            "Unexpected event object of type " + typeof(SupportBean).FullName + ", expected " + Name.Of<IDictionary<string, object>>());

	        // Object-Array
            RunAssertionSuccess(SupportEventInfra.OA_TYPENAME, new object[] { });
            RunAssertionInvalid(SupportEventInfra.OA_TYPENAME, new SupportBean(),
	                            "Unexpected event object of type " + typeof(SupportBean).FullName + ", expected Object[]");

	        // XML
            RunAssertionSuccess(SupportEventInfra.XML_TYPENAME, SupportXML.GetDocument("<myevent/>").DocumentElement);
            RunAssertionInvalid(SupportEventInfra.XML_TYPENAME, new SupportBean(),
	                            "Unexpected event object type '" + typeof(SupportBean).FullName + "' encountered, please supply a XmlDocument or XmlElement node");
            RunAssertionInvalid(SupportEventInfra.XML_TYPENAME, SupportXML.GetDocument("<xxxx/>"),
	                            "Unexpected root element name 'xxxx' encountered, expected a root element name of 'myevent'");

	        // Avro
            RunAssertionSuccess(SupportEventInfra.AVRO_TYPENAME, new GenericRecord(GetAvroSchema()));
            RunAssertionInvalid(SupportEventInfra.AVRO_TYPENAME, new SupportBean(),
	                            "Unexpected event object type '" + typeof(SupportBean).FullName + "' encountered, please supply a GenericRecord");

	        // No such type
	        try {
	            _epService.EPRuntime.GetEventSender("ABC");
	            Assert.Fail();
	        } catch (EventTypeException ex) {
	            Assert.AreEqual("Event type named 'ABC' could not be found", ex.Message);
	        }

	        // Internal implicit wrapper type
	        _epService.EPAdministrator.CreateEPL("insert into ABC select *, theString as value from SupportBean");
	        try {
	            _epService.EPRuntime.GetEventSender("ABC");
	            Assert.Fail("Event type named 'ABC' could not be found");
	        } catch (EventTypeException ex) {
	            Assert.AreEqual("An event sender for event type named 'ABC' could not be created as the type is internal", ex.Message);
	        }
	    }

	    private void RunAssertionSuccess(string typename, params object[] correctUnderlyings) {
	        var stmtText = "select * from " + typename;
	        var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        var sender = _epService.EPRuntime.GetEventSender(typename);
	        foreach (var underlying in correctUnderlyings) {
	            sender.SendEvent(underlying);
	            Assert.AreSame(underlying, listener.AssertOneGetNewAndReset().Underlying);
	        }

	        stmt.Dispose();
	    }

	    private void RunAssertionInvalid(string typename,
	                                     object incorrectUnderlying,
	                                     string message) {
	        var sender = _epService.EPRuntime.GetEventSender(typename);

	        try {
	            sender.SendEvent(incorrectUnderlying);
	            Assert.Fail();
	        } catch (EPException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, message);
	        }
	    }

	    private void AddMapEventType() {
            _epService.EPAdministrator.Configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, Collections.EmptyDataMap);
	    }

	    private void AddOAEventType() {
	        string[] names = {};
	        object[] types = {};
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
	        return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME);
	    }
	}
} // end of namespace
