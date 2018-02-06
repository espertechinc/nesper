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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using com.espertech.esper.util.support;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.supportregression.events.SupportEventInfra;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regression.events.infra
{
    public class ExecEventInfraPropertyUnderlyingSimple : RegressionExecution
    {
        private static readonly string BEAN_TYPENAME = typeof(SupportBeanSimple).Name;

        private static readonly FunctionSendEventIntString FMAP = (epService, a, b) =>
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("myInt", a);
            map.Put("myString", b);
            epService.EPRuntime.SendEvent(map, MAP_TYPENAME);
            return map;
        };

        private static readonly FunctionSendEventIntString FOA = (epService, a, b) =>
        {
            var oa = new object[] {a, b};
            epService.EPRuntime.SendEvent(oa, OA_TYPENAME);
            return oa;
        };

        private static readonly FunctionSendEventIntString FBEAN = (epService, a, b) =>
        {
            var bean = new SupportBeanSimple(b, a.Value);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        };

        private static readonly FunctionSendEventIntString FXML = (epService, a, b) =>
        {
            var xml = "<myevent myInt=\"XXXXXX\" myString=\"YYYYYY\">\n" +
                      "</myevent>\n";
            xml = xml.Replace("XXXXXX", a.ToString());
            xml = xml.Replace("YYYYYY", b);
            try
            {
                var d = SupportXML.SendEvent(epService.EPRuntime, xml);
                return d.DocumentElement;
            }
            catch (Exception e)
            {
                throw new EPException(e);
            }
        };

        private static readonly FunctionSendEventIntString FAVRO = (epService, a, b) =>
        {
            var datum = new GenericRecord(GetAvroSchema());
            datum.Put("myInt", a);
            datum.Put("myString", b);
            epService.EPRuntime.SendEventAvro(datum, AVRO_TYPENAME);
            return datum;
        };

        public override void Configure(Configuration configuration)
        {
            AddMapEventType(configuration);
            AddOAEventType(configuration);
            configuration.AddEventType(BEAN_TYPENAME, typeof(SupportBeanSimple));
            AddXMLEventType(configuration);
            AddAvroEventType(configuration);
        }

        public override void Run(EPServiceProvider epService)
        {
            var pairs = new[]
            {
                new Pair<string, FunctionSendEventIntString>(MAP_TYPENAME, FMAP),
                new Pair<string, FunctionSendEventIntString>(OA_TYPENAME, FOA),
                new Pair<string, FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
                new Pair<string, FunctionSendEventIntString>(XML_TYPENAME, FXML),
                new Pair<string, FunctionSendEventIntString>(AVRO_TYPENAME, FAVRO)
            };

            foreach (var pair in pairs)
            {
                RunAssertionPassUnderlying(epService, pair.First, pair.Second);
                RunAssertionPropertiesWGetter(epService, pair.First, pair.Second);
                RunAssertionTypeValidProp(epService, pair.First, pair.Second == FMAP || pair.Second == FXML || pair.Second == FOA, (pair.Second == FBEAN));
                RunAssertionTypeInvalidProp(epService, pair.First, pair.Second == FXML);
            }
        }

        private void RunAssertionPassUnderlying(
            EPServiceProvider epService, string typename, FunctionSendEventIntString send)
        {
            var epl = "select * from " + typename;
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            var fields = "myInt,myString".Split(',');

            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("myInt").GetBoxedType());
            Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("myString"));

            var eventOne = send.Invoke(epService, 3, "some string");

            var @event = listener.AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
            Assert.AreEqual(eventOne, @event.Underlying);
            EPAssertionUtil.AssertProps(@event, fields, new object[] {3, "some string"});

            var eventTwo = send.Invoke(epService, 4, "other string");
            @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(eventTwo, @event.Underlying);
            EPAssertionUtil.AssertProps(@event, fields, new object[] {4, "other string"});

            statement.Dispose();
        }

        private void RunAssertionPropertiesWGetter(
            EPServiceProvider epService, string typename, FunctionSendEventIntString send)
        {
            var epl =
                "select myInt, Exists(myInt) as exists_myInt, myString, Exists(myString) as exists_myString from " +
                typename;
            var statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            var fields = "myInt,exists_myInt,myString,exists_myString".Split(',');

            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("myInt").GetBoxedType());
            Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("myString"));
            Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("exists_myInt"));
            Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("exists_myString"));

            send.Invoke(epService, 3, "some string");

            var @event = listener.AssertOneGetNewAndReset();
            RunAssertionEventInvalidProp(@event);
            EPAssertionUtil.AssertProps(@event, fields, new object[] {3, true, "some string", true});

            send.Invoke(epService, 4, "other string");
            @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, fields, new object[] {4, true, "other string", true});

            statement.Dispose();
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Collections.List("xxxx", "myString('a')", "x.y", "myString.x"))
            {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(EPServiceProvider epService, string typeName, bool boxed, bool isBeanStyle)
        {
            var eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);

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

            var expectedType = isBeanStyle
                ? new object[][] { valuesMyString, valuesMyInt } // why are we dependent on order?
                : new object[][] { valuesMyInt, valuesMyString };

            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] {nameMyString, nameMyInt}, eventType.PropertyNames);

            Assert.IsNotNull(eventType.GetGetter(nameMyInt));
            Assert.IsTrue(eventType.IsProperty(nameMyInt));
            Assert.AreEqual(boxed ? typeof(int?) : typeof(int), eventType.GetPropertyType(nameMyInt));
            Assert.AreEqual(
                new EventPropertyDescriptor(nameMyString, typeof(string), typeof(char), false, false, true, false, false), 
                eventType.GetPropertyDescriptor(nameMyString));
        }

        private void RunAssertionTypeInvalidProp(EPServiceProvider epService, string typeName, bool xml)
        {
            var eventType = epService.EPAdministrator.Configuration.GetEventType(typeName);

            foreach (var prop in Collections.List(
                "xxxx", "myString('a')", "myString.x", "myString.x.y", "myString.x"))
            {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Type expected = null;
                if (xml)
                {
                    if (prop.Equals("myString[0]"))
                    {
                        expected = typeof(string);
                    }

                    if (prop.Equals("myString.x?"))
                    {
                        expected = typeof(XmlNode);
                    }
                }

                Assert.AreEqual(expected, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
                Assert.IsNull(eventType.GetFragmentType(prop));
            }
        }

        private void AddMapEventType(Configuration configuration)
        {
            var properties = new LinkedHashMap<string, object>();
            properties.Put("myInt", typeof(int?));
            properties.Put("myString", "string");
            configuration.AddEventType(MAP_TYPENAME, properties);
        }

        private void AddOAEventType(Configuration configuration)
        {
            string[] names = {"myInt", "myString"};
            object[] types = {typeof(int?), typeof(string)};
            configuration.AddEventType(OA_TYPENAME, names, types);
        }

        private void AddXMLEventType(Configuration configuration)
        {
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
            configuration.AddEventType(XML_TYPENAME, eventTypeMeta);
        }

        private void AddAvroEventType(Configuration configuration)
        {
            configuration.AddEventTypeAvro(AVRO_TYPENAME, new ConfigurationEventTypeAvro(GetAvroSchema()));
        }

        private static RecordSchema GetAvroSchema() {
            return SchemaBuilder.Record(
                AVRO_TYPENAME,
                Field("myInt", IntType()),
                Field("myString", StringType(Property(AvroConstant.PROP_STRING_KEY, AvroConstant.PROP_STRING_VALUE))));
        }

        internal delegate object FunctionSendEventIntString(
            EPServiceProvider epService, int? intValue, string stringValue);
    }
} // end of namespace