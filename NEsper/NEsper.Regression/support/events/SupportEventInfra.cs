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

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.@event
{
    public class SupportEventInfra
    {
        public delegate void FunctionSendEvent(
            RegressionEnvironment env,
            object value,
            string name);

        public delegate void FunctionSendEventWType(
            RegressionEnvironment env,
            object value,
            string typeName);

        public static readonly FunctionSendEvent FMAP = (
            env,
            @event,
            name) => {
            env.SendEventMap(@event.UnwrapStringDictionary(), name);
        };

        public static readonly FunctionSendEvent FOA = (
            env,
            @event,
            name) => {
            env.SendEventObjectArray(@event.UnwrapIntoArray<object>(), name);
        };

        public static readonly FunctionSendEvent FBEAN = (
            env,
            @event,
            name) => {
            env.SendEventBean(@event, name);
        };

        public static readonly FunctionSendEvent FAVRO = (
            env,
            @event,
            name) => {
            var record = (GenericRecord) @event;
            //GenericData.Get().Validate(record.Schema, record);
            env.SendEventAvro(record, name);
        };

        public static readonly FunctionSendEventWType FMAPWTYPE = (
            env,
            @event,
            typeName) => {
            env.SendEventMap((IDictionary<string, object>) @event, typeName);
        };

        public static readonly FunctionSendEventWType FOAWTYPE = (
            env,
            @event,
            typeName) => {
            env.SendEventObjectArray((object[]) @event, typeName);
        };

        public static readonly FunctionSendEventWType FBEANWTYPE = (
            env,
            @event,
            typeName) => {
            env.SendEventBean(@event);
        };

        public static readonly FunctionSendEventWType FAVROWTYPE = (
            env,
            @event,
            typeName) => {
            var record = (GenericRecord) @event;
            //GenericData.Get().Validate(record.Schema, record);
            env.SendEventAvro(record, typeName);
        };

        public static readonly FunctionSendEvent FXML = (
            env,
            @event,
            name) => {
            string xml;
            if (@event.ToString().Contains("<Myevent")) {
                xml = @event.ToString();
            }
            else {
                xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      "<Myevent>\n" +
                      "  " +
                      @event +
                      "\n" +
                      "</Myevent>\n";
            }

            try {
                SupportXML.SendXMLEvent(env, xml, name);
            }
            catch (Exception e) {
                throw new EPException(e);
            }
        };

        public static Func<object, object> xmlToValue = @in => {
            if (@in == null) {
                return null;
            }

            if (@in is XmlAttribute xmlAttr) {
                return xmlAttr.Value;
            }

            if (@in is XmlNode xmlNode) {
                return xmlNode.InnerText;
            }

            return "unknown xml value";
        };

        public static void AssertValuesMayConvert(
            EventBean eventBean,
            string[] propertyNames,
            ValueWithExistsFlag[] expected,
            Func<object, object> optionalValueConversion)
        {
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean);
            var receivedValues = new object[propertyNames.Length];
            var expectedValues = new object[propertyNames.Length];
            for (var i = 0; i < receivedValues.Length; i++) {
                var value = eventBean.Get(propertyNames[i]);
                if (optionalValueConversion != null) {
                    value = optionalValueConversion.Invoke(value);
                }

                receivedValues[i] = value;
                expectedValues[i] = expected[i].Value;
            }

            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, receivedValues);

            for (var i = 0; i < receivedValues.Length; i++) {
                var exists = (bool) eventBean.Get("exists_" + propertyNames[i]);
                Assert.AreEqual(
                    expected[i].IsExists,
                    exists,
                    "Assertion failed for property 'exists_" + propertyNames[i] + "'");
            }
        }

        public static void AssertValueMayConvert(
            EventBean eventBean,
            string propertyName,
            ValueWithExistsFlag expected,
            Func<object, object> optionalValueConversion)
        {
            SupportEventTypeAssertionUtil.AssertConsistency(eventBean);
            var value = eventBean.Get(propertyName);
            if (optionalValueConversion != null) {
                value = optionalValueConversion.Invoke(value);
            }

            Assert.AreEqual(expected.Value, value);
            Assert.AreEqual(expected.IsExists, eventBean.Get("exists_" + propertyName));
        }
    }
} // end of namespace