///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyUnderlyingSimple : RegressionExecution
    {
        public delegate object FunctionSendEventIntString(
            RegressionEnvironment env,
            int? intValue,
            string stringValue);

        public static readonly string XML_TYPENAME = typeof(EventInfraPropertyUnderlyingSimple).FullName + "XML";
        public static readonly string MAP_TYPENAME = typeof(EventInfraPropertyUnderlyingSimple).FullName + "Map";
        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyUnderlyingSimple).FullName + "OA";
        public static readonly string AVRO_TYPENAME = typeof(EventInfraPropertyUnderlyingSimple).FullName + "Avro";

        private static readonly string BEAN_TYPENAME = typeof(SupportBeanSimple).Name;

        private static readonly ILog log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly FunctionSendEventIntString FMAP = (
            env,
            a,
            b) => {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("MyInt", a);
            map.Put("MyString", b);
            env.SendEventMap(map, MAP_TYPENAME);
            return map;
        };

        private static readonly FunctionSendEventIntString FOA = (
            env,
            a,
            b) => {
            object[] oa = {a, b};
            env.SendEventObjectArray(oa, OA_TYPENAME);
            return oa;
        };

        private static readonly FunctionSendEventIntString FBEAN = (
            env,
            a,
            b) => {
            var bean = new SupportBeanSimple(b, a.Value);
            env.SendEventBean(bean);
            return bean;
        };

        private static readonly FunctionSendEventIntString FXML = (
            env,
            a,
            b) => {
            var xml = "<Myevent myInt=\"XXXXXX\" myString=\"YYYYYY\">\n" +
                      "</Myevent>\n";
            xml = xml.Replace("XXXXXX", a.ToString());
            xml = xml.Replace("YYYYYY", b);
            try {
                var d = SupportXML.SendXMLEvent(env, xml, XML_TYPENAME);
                return d.DocumentElement;
            }
            catch (Exception e) {
                throw new EPException(e);
            }
        };

        private static readonly FunctionSendEventIntString FAVRO = (
            env,
            a,
            b) => {
            var avroSchema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var datum = new GenericRecord(avroSchema);
            datum.Put("MyInt", a);
            datum.Put("MyString", b);
            env.SendEventAvro(datum, AVRO_TYPENAME);
            return datum;
        };

        public void Run(RegressionEnvironment env)
        {
            Pair<string, FunctionSendEventIntString>[] pairs = {
                new Pair<string, FunctionSendEventIntString>(MAP_TYPENAME, FMAP),
                new Pair<string, FunctionSendEventIntString>(OA_TYPENAME, FOA),
                new Pair<string, FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
                new Pair<string, FunctionSendEventIntString>(XML_TYPENAME, FXML),
                new Pair<string, FunctionSendEventIntString>(AVRO_TYPENAME, FAVRO)
            };

            foreach (var pair in pairs) {
                log.Info("Asserting type " + pair.First);
                RunAssertionPassUnderlying(env, pair.First, pair.Second);
                RunAssertionPropertiesWGetter(env, pair.First, pair.Second);
                RunAssertionTypeValidProp(
                    env,
                    pair.First,
                    pair.Second == FMAP || pair.Second == FXML || pair.Second == FOA || pair.Second == FAVRO);
                RunAssertionTypeInvalidProp(env, pair.First, pair.Second == FXML);
            }
        }

        private void RunAssertionPassUnderlying(
            RegressionEnvironment env,
            string typename,
            FunctionSendEventIntString send)
        {
            var epl = "@Name('s0') select * from " + typename;
            env.CompileDeploy(epl).AddListener("s0");

            var fields = new [] { "MyInt","MyString" };

            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("MyInt").GetBoxedType());
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("MyString"));

            var eventOne = send.Invoke(env, 3, "some string");

            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
            Assert.AreEqual(eventOne, @event.Underlying);
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {3, "some string"});

            var eventTwo = send.Invoke(env, 4, "other string");
            @event = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual(eventTwo, @event.Underlying);
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {4, "other string"});

            env.UndeployAll();
        }

        private void RunAssertionPropertiesWGetter(
            RegressionEnvironment env,
            string typename,
            FunctionSendEventIntString send)
        {
            var epl =
                "@Name('s0') select MyInt, exists(MyInt) as exists_MyInt, MyString, exists(MyString) as exists_MyString from " +
                typename;
            env.CompileDeploy(epl).AddListener("s0");

            var fields = new [] { "MyInt","exists_MyInt","MyString","exists_MyString" };

            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("MyInt").GetBoxedType());
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("MyString"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_MyInt"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_MyString"));

            send.Invoke(env, 3, "some string");

            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            RunAssertionEventInvalidProp(@event);
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {3, true, "some string", true});

            send.Invoke(env, 4, "other string");
            @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {4, true, "other string", true});

            env.UndeployAll();
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("xxxx", "MyString[1]", "MyString('a')", "x.y", "MyString.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            bool boxed)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            object[][] expectedType = {
                new object[] {"MyInt", boxed ? typeof(int?) : typeof(int), null, null},
                new object[] {"MyString", typeof(string), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] {"MyString", "MyInt"}, eventType.PropertyNames);

            Assert.IsNotNull(eventType.GetGetter("MyInt"));
            Assert.IsTrue(eventType.IsProperty("MyInt"));
            Assert.AreEqual(boxed ? typeof(int?) : typeof(int), eventType.GetPropertyType("MyInt"));
            Assert.AreEqual(
                new EventPropertyDescriptor("MyString", typeof(string), null, false, false, false, false, false),
                eventType.GetPropertyDescriptor("MyString"));
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName,
            bool xml)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            foreach (var prop in Arrays.AsList(
                "xxxx",
                "MyString[0]",
                "MyString('a')",
                "MyString.x",
                "MyString.x.y",
                "MyString.x")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Type expected = null;
                if (xml) {
                    if (prop.Equals("MyString[0]")) {
                        expected = typeof(string);
                    }

                    if (prop.Equals("MyString.x?")) {
                        expected = typeof(XmlNode);
                    }
                }

                Assert.AreEqual(expected, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
                Assert.IsNull(eventType.GetFragmentType(prop));
            }
        }
    }
} // end of namespace