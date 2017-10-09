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
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.util;
using com.espertech.esper.util.support;

using NEsper.Avro;
using NEsper.Avro.Core;

using NUnit.Framework;
using NEsper.Avro.Extensions;
using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraPropertyUnderlyingAndSimple
    {
	    private static readonly string BEAN_TYPENAME = typeof(SupportBeanSimple).FullName;

	    private static readonly FunctionSendEventIntString FMAP = (epService, a, b) => {
            IDictionary<string, object> map = new Dictionary<string, object>();
	        map.Put("myInt", a);
	        map.Put("myString", b);
            epService.EPRuntime.SendEvent(map, SupportEventInfra.MAP_TYPENAME);
	        return map;
	    };

	    private static readonly FunctionSendEventIntString FOA = (epService, a, b) => {
	        var oa = new object[] {a, b};
            epService.EPRuntime.SendEvent(oa, SupportEventInfra.OA_TYPENAME);
	        return oa;
	    };

	    private static readonly FunctionSendEventIntString FBEAN = (epService, a, b) => {
	        var bean = new SupportBeanSimple(b, a.Value);
	        epService.EPRuntime.SendEvent(bean);
	        return bean;
	    };

	    private static readonly FunctionSendEventIntString FXML = (epService, a, b) => {
	        var xml = "<myevent myInt=\"XXXXXX\" myString=\"YYYYYY\">\n" +
	        "</myevent>\n";
	        xml = xml.Replace("XXXXXX", a.ToString());
	        xml = xml.Replace("YYYYYY", b);
	        try {
	            var d = SupportXML.SendEvent(epService.EPRuntime, xml);
	            return d.DocumentElement;
	        } catch (Exception e) {
	            throw new EPException(e);
	        }
	    };

	    private static readonly FunctionSendEventIntString FAVRO = (epService, a, b) => {
	        var datum = new GenericRecord(GetAvroSchema());
	        datum.Put("myInt", a);
	        datum.Put("myString", b);
            epService.EPRuntime.SendEventAvro(datum, SupportEventInfra.AVRO_TYPENAME);
	        return datum;
	    };

	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
	        AddMapEventType(configuration);
	        AddOAEventType(configuration);
	        configuration.AddEventType(BEAN_TYPENAME, typeof(SupportBeanSimple));
	        AddXMLEventType(configuration);
	        AddAvroEventType(configuration);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
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
	    public void TestPassUnderlyingGetViaPropertyExpression() {

	        var pairs = new Pair<string, FunctionSendEventIntString>[] {
	            new Pair<string,FunctionSendEventIntString>(SupportEventInfra.MAP_TYPENAME, FMAP),
	            new Pair<string,FunctionSendEventIntString>(SupportEventInfra.OA_TYPENAME, FOA),
	            new Pair<string,FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
	            new Pair<string,FunctionSendEventIntString>(SupportEventInfra.XML_TYPENAME, FXML),
	            new Pair<string,FunctionSendEventIntString>(SupportEventInfra.AVRO_TYPENAME, FAVRO)
	        };

	        foreach (var pair in pairs) {
	            RunAssertionPassUnderlying(pair.First, pair.Second);
	            RunAssertionPropertiesWGetter(pair.First, pair.Second);
	            RunAssertionTypeValidProp(pair.First, (pair.Second == FMAP || pair.Second == FXML || pair.Second == FOA), (pair.Second == FBEAN));
	            RunAssertionTypeInvalidProp(pair.First, pair.Second == FXML);
	        }
	    }

	    private void RunAssertionPassUnderlying(string typename, FunctionSendEventIntString send) {
	        var epl = "select * from " + typename;
	        var statement = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);
	        var fields = "myInt,myString".SplitCsv();

	        Assert.AreEqual(typeof(int?), TypeHelper.GetBoxedType(statement.EventType.GetPropertyType("myInt")));
	        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("myString"));

            var eventOne = send.Invoke(_epService, 3, "some string");

	        var @event = listener.AssertOneGetNewAndReset();
	        SupportEventTypeAssertionUtil.AssertConsistency(@event);
	        Assert.AreEqual(eventOne, @event.Underlying);
	        EPAssertionUtil.AssertProps(@event, fields, new object[] {3, "some string"});

	        var eventTwo = send.Invoke(_epService, 4, "other string");
	        @event = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual(eventTwo, @event.Underlying);
	        EPAssertionUtil.AssertProps(@event, fields, new object[] {4, "other string"});

	        statement.Dispose();
	    }

	    private void RunAssertionPropertiesWGetter(string typename, FunctionSendEventIntString send) {
	        var epl = "select myInt, exists(myInt) as exists_myInt, myString, exists(myString) as exists_myString from " + typename;
	        var statement = _epService.EPAdministrator.CreateEPL(epl);
	        var listener = new SupportUpdateListener();
	        statement.AddListener(listener);
	        var fields = "myInt,exists_myInt,myString,exists_myString".SplitCsv();

	        Assert.AreEqual(typeof(int?), TypeHelper.GetBoxedType(statement.EventType.GetPropertyType("myInt")));
	        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("myString"));
	        Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("exists_myInt"));
	        Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("exists_myString"));

            send.Invoke(_epService, 3, "some string");

	        var @event = listener.AssertOneGetNewAndReset();
	        RunAssertionEventInvalidProp(@event);
	        EPAssertionUtil.AssertProps(@event, fields, new object[] {3, true, "some string", true});

            send.Invoke(_epService, 4, "other string");
	        @event = listener.AssertOneGetNewAndReset();
	        EPAssertionUtil.AssertProps(@event, fields, new object[] {4, true, "other string", true});

	        statement.Dispose();
	    }

	    private void RunAssertionEventInvalidProp(EventBean @event) {
	        foreach (var prop in Collections.List("xxxx", "myString('a')", "x.y", "myString.x")) {
	            SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
	            SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
	        }
	    }

	    private void RunAssertionTypeValidProp(string typeName, bool boxed, bool isBeanStyle) {
	        var eventType = _epService.EPAdministrator.Configuration.GetEventType(typeName);

            string nameMyString;
            string nameMyInt;

            if (isBeanStyle)
            {
                nameMyString = "MyString";
                nameMyInt = "MyInt";
            }
            else
            {
                nameMyString = "myString";
                nameMyInt = "myInt";
            }

            var valuesMyString = new object[] { nameMyString, typeof(string), null, null };
            var valuesMyInt = new object[] { nameMyInt, boxed ? typeof(int?) : typeof(int), null, null };

            object[][] expectedType = isBeanStyle
                ? new object[][] { valuesMyString, valuesMyInt } // why are we dependent on order?
                : new object[][] { valuesMyInt, valuesMyString };

            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

	        EPAssertionUtil.AssertEqualsAnyOrder(new string[] {nameMyString, nameMyInt}, eventType.PropertyNames);

	        Assert.IsNotNull(eventType.GetGetter(nameMyInt));
	        Assert.IsTrue(eventType.IsProperty(nameMyInt));
	        Assert.AreEqual(boxed ? typeof(int?) : typeof(int), eventType.GetPropertyType(nameMyInt));
	        Assert.AreEqual(new EventPropertyDescriptor(nameMyString, typeof(string), typeof(char), false, false, true, false, false), eventType.GetPropertyDescriptor(nameMyString));
	    }

	    private void RunAssertionTypeInvalidProp(string typeName, bool xml) {
	        var eventType = _epService.EPAdministrator.Configuration.GetEventType(typeName);

	        foreach (var prop in Collections.List("xxxx", "myString('a')", "myString.x", "myString.x.y", "myString.x")) {
	            Assert.AreEqual(false, eventType.IsProperty(prop));
	            Type expected = null;
	            if (xml) {
	                if (prop.Equals("myString[0]")) {
	                    expected = typeof(string);
	                }
	                if (prop.Equals("myString.x?")) {
	                    expected = typeof(XmlNode);
	                }
	            }
	            Assert.AreEqual(expected, eventType.GetPropertyType(prop));
	            Assert.IsNull(eventType.GetPropertyDescriptor(prop));
	            Assert.IsNull(eventType.GetFragmentType(prop));
	        }
	    }

	    private void AddMapEventType(Configuration configuration) {
	        var properties = new LinkedHashMap<string, object>();
	        properties.Put("myInt", typeof(int?));
	        properties.Put("myString", "string");
	        configuration.AddEventType(SupportEventInfra.MAP_TYPENAME, properties);
	    }

	    private void AddOAEventType(Configuration configuration) {
	        string[] names = {"myInt", "myString"};
	        object[] types = {typeof(int?), typeof(string)};
            configuration.AddEventType(SupportEventInfra.OA_TYPENAME, names, types);
	    }

	    private void AddXMLEventType(Configuration configuration) {
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "myevent";
	        var schema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                        "<xs:schema targetNamespace=\"http://www.espertech.com/schema/esper\" elementFormDefault=\"qualified\" xmlns:esper=\"http://www.espertech.com/schema/esper\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
	                        "\t<xs:element name=\"myevent\">\n" +
	                        "\t\t<xs:complexType>\n" +
	                        "\t\t\t<xs:attribute name=\"myInt\" type=\"xs:int\" use=\"required\"/>\n" +
	                        "\t\t\t<xs:attribute name=\"myString\" type=\"xs:string\" use=\"required\"/>\n" +
	                        "\t\t</xs:complexType>\n" +
	                        "\t</xs:element>\n" +
	                        "</xs:schema>\n";
	        eventTypeMeta.SchemaText = schema;
            configuration.AddEventType(SupportEventInfra.XML_TYPENAME, eventTypeMeta);
	    }

	    private void AddAvroEventType(Configuration configuration) {
            configuration.AddEventTypeAvro(SupportEventInfra.AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
	    }

	    private static RecordSchema GetAvroSchema()
        {
            return SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.Field("myInt", TypeBuilder.Int()),
                TypeBuilder.Field("myString", TypeBuilder.String(
                    TypeBuilder.Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))));
	    }


	    internal delegate object FunctionSendEventIntString(EPServiceProvider epService, int? intValue, string stringValue);
	}
} // end of namespace
