///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Formatting = Newtonsoft.Json.Formatting;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyUnderlyingSimple : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Json";
        public const string JSONPROVIDEDBEAN_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "JsonWProvided";

        private static readonly ILog Log = LogManager.GetLogger(typeof(EventInfraPropertyUnderlyingSimple));

        private readonly EventRepresentationChoice _eventRepresentationChoice;
        
        /// <summary>
        /// Constructor for test
        /// </summary>
        /// <param name="eventRepresentationChoice"></param>
        /// <exception cref="NotImplementedException"></exception>
        public EventInfraPropertyUnderlyingSimple(EventRepresentationChoice eventRepresentationChoice)
        {
            _eventRepresentationChoice = eventRepresentationChoice;
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            var eplJson =
                $"@public @buseventtype @name('schema') create json schema {JSON_TYPENAME}(MyInt int, MyString string);\n" +
                $"@public @buseventtype @name('schema') @JsonSchema(ClassName='{typeof(MyLocalJsonProvided).CleanName()}') " +
                $" create json schema {JSONPROVIDEDBEAN_TYPENAME}();\n";
            env.CompileDeploy(eplJson, path);

            var pair = _eventRepresentationChoice switch
            {
                EventRepresentationChoice.DEFAULT => new Pair<string, FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
                EventRepresentationChoice.OBJECTARRAY => new Pair<string, FunctionSendEventIntString>(OA_TYPENAME, FOA),
                EventRepresentationChoice.MAP => new Pair<string, FunctionSendEventIntString>(MAP_TYPENAME, FMAP),
                EventRepresentationChoice.AVRO => new Pair<string, FunctionSendEventIntString>(AVRO_TYPENAME, FAVRO),
                EventRepresentationChoice.JSON => new Pair<string, FunctionSendEventIntString>(JSON_TYPENAME, FJSON),
                EventRepresentationChoice.JSONCLASSPROVIDED => new Pair<string, FunctionSendEventIntString>(JSONPROVIDEDBEAN_TYPENAME, FJSON),
                _ => throw new ArgumentOutOfRangeException()
            };

            // var pairs = new Pair<string, FunctionSendEventIntString>[] {
            //     new Pair<string, FunctionSendEventIntString>(MAP_TYPENAME, FMAP),
            //     new Pair<string, FunctionSendEventIntString>(OA_TYPENAME, FOA),
            //     new Pair<string, FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
            //     new Pair<string, FunctionSendEventIntString>(XML_TYPENAME, FXML),
            //     new Pair<string, FunctionSendEventIntString>(AVRO_TYPENAME, FAVRO),
            //     new Pair<string, FunctionSendEventIntString>(JSON_TYPENAME, FJSON),
            //     new Pair<string, FunctionSendEventIntString>(JSONPROVIDEDBEAN_TYPENAME, FJSON)
            // };

            // foreach (var pair in pairs) {
            RunAssertions(env, pair, path);
            // }

            env.UndeployAll();
        }

        private void RunAssertions(RegressionEnvironment env, Pair<string, FunctionSendEventIntString> pair, RegressionPath path)
        {
            Log.Info("Asserting type " + pair.First);
            RunAssertionPassUnderlying(env, pair.First, pair.Second, path);
            RunAssertionPropertiesWGetter(env, pair.First, pair.Second, path);
            RunAssertionTypeValidProp(env, pair.First, pair.Second != FBEAN && pair.Second != FAVRO);
            RunAssertionTypeInvalidProp(env, pair.First, pair.Second == FXML);
        }

        private void RunAssertionPassUnderlying(
            RegressionEnvironment env,
            string typename,
            FunctionSendEventIntString send,
            RegressionPath path)
        {
            var epl = "@name('s0') select * from " + typename;
            env.CompileDeploy(epl, path).AddListener("s0");

            var fields = "MyInt,MyString".SplitCsv();

            env.AssertStatement(
                "s0",
                statement => {
                    ClassicAssert.AreEqual(typeof(int?), Boxing.GetBoxedType(statement.EventType.GetPropertyType("MyInt")));
                    ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("MyString"));
                });

            var eventOne = send.Invoke(typename, env, 3, "some string");

            env.AssertEventNew(
                "s0",
                @event => {
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                    AssertUnderlying(typename, eventOne, @event.Underlying);
                    EPAssertionUtil.AssertProps(@event, fields, new object[] { 3, "some string" });
                });

            var eventTwo = send.Invoke(typename, env, 4, "other string");
            env.AssertEventNew(
                "s0",
                @event => {
                    AssertUnderlying(typename, eventTwo, @event.Underlying);
                    EPAssertionUtil.AssertProps(@event, fields, new object[] { 4, "other string" });
                });

            env.UndeployModuleContaining("s0");
        }

        private void AssertUnderlying(
            string typename,
            object expected,
            object received)
        {
            if (typename.Equals(JSONPROVIDEDBEAN_TYPENAME)) {
                ClassicAssert.IsTrue(received is MyLocalJsonProvided);
            }
            else if (typename.Equals(JSON_TYPENAME)) {
                ClassicAssert.AreEqual(expected, received.ToString());
            }
            else {
                ClassicAssert.AreEqual(expected, received);
            }
        }

        private void RunAssertionPropertiesWGetter(
            RegressionEnvironment env,
            string typename,
            FunctionSendEventIntString send,
            RegressionPath path)
        {
            var epl =
                "@name('s0') select MyInt, exists(MyInt) as exists_MyInt, MyString, exists(MyString) as exists_MyString from " +
                typename;
            env.CompileDeploy(epl, path).AddListener("s0");

            var fields = "MyInt,exists_MyInt,MyString,exists_MyString".SplitCsv();

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    ClassicAssert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType("MyInt")));
                    ClassicAssert.AreEqual(typeof(string), eventType.GetPropertyType("MyString"));
                    ClassicAssert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_MyInt"));
                    ClassicAssert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_MyString"));
                });

            send.Invoke(typename, env, 3, "some string");

            env.AssertEventNew(
                "s0",
                @event => {
                    RunAssertionEventInvalidProp(@event);
                    EPAssertionUtil.AssertProps(@event, fields, new object[] { 3, true, "some string", true });
                });

            send.Invoke(typename, env, 4, "other string");
            env.AssertEventNew(
                "s0",
                @event => EPAssertionUtil.AssertProps(@event, fields, new object[] { 4, true, "other string", true }));

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("xxxx", "MyString('a')", "x.y", "MyString.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            bool boxed)
        {
            env.AssertThat(
                () => {
                    var eventType = !typeName.Equals(JSON_TYPENAME)
                        ? env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName)
                        : env.Runtime.EventTypeService.GetEventType(env.DeploymentId("schema"), typeName);

                    var intType = boxed ? typeof(int?) : typeof(int);
                    var expectedType = new object[][] {
                        new object[] { "MyInt", intType, null, null },
                        new object[] { "MyString", typeof(string), null, null }
                    };
                    SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedType,
                        eventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

                    EPAssertionUtil.AssertEqualsAnyOrder(new string[] { "MyString", "MyInt" }, eventType.PropertyNames);

                    ClassicAssert.IsNotNull(eventType.GetGetter("MyInt"));
                    ClassicAssert.IsTrue(eventType.IsProperty("MyInt"));
                    ClassicAssert.AreEqual(intType, eventType.GetPropertyType("MyInt"));
                    SupportEventPropUtil.AssertPropEquals(
                        new SupportEventPropDesc("MyString", typeof(string)),
                        eventType.GetPropertyDescriptor("MyString"));
                });
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName,
            bool xml)
        {
            env.AssertThat(
                () => {
                    var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

                    foreach (var prop in Arrays.AsList(
                                 "xxxx",
                                 "MyString('a')",
                                 "MyString.x",
                                 "MyString.x.y",
                                 "MyString.x")) {
                        ClassicAssert.AreEqual(false, eventType.IsProperty(prop));
                        Type expected = null;
                        if (xml) {
                            if (prop.Equals("MyString.x?")) {
                                expected = typeof(XmlNode);
                            }
                        }

                        ClassicAssert.AreEqual(expected, eventType.GetPropertyType(prop));
                        ClassicAssert.IsNull(eventType.GetPropertyDescriptor(prop));
                        ClassicAssert.IsNull(eventType.GetFragmentType(prop));
                    }
                });
        }

        public delegate object FunctionSendEventIntString(
            string eventTypeName,
            RegressionEnvironment env,
            int intValue,
            string stringValue);

        private const string BEAN_TYPENAME = nameof(SupportBeanSimple);

        private static readonly FunctionSendEventIntString FMAP = (
            eventTypeName,
            env,
            a,
            b) => {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("MyInt", a);
            map.Put("MyString", b);
            env.SendEventMap(map, eventTypeName);
            return map;
        };

        private static readonly FunctionSendEventIntString FOA = (
            eventTypeName,
            env,
            a,
            b) => {
            var oa = new object[] { a, b };
            env.SendEventObjectArray(oa, eventTypeName);
            return oa;
        };

        private static readonly FunctionSendEventIntString FBEAN = (
            eventTypeName,
            env,
            a,
            b) => {
            var bean = new SupportBeanSimple(b, a);
            env.SendEventBean(bean);
            return bean;
        };

        private static readonly FunctionSendEventIntString FXML = (
            eventTypeName,
            env,
            a,
            b) => {
            var xml = "<myevent MyInt=\"XXXXXX\" MyString=\"YYYYYY\">\n" +
                      "</myevent>\n";
            xml = xml.Replace("XXXXXX", a.ToString());
            xml = xml.Replace("YYYYYY", b);
            try {
                var d = SupportXML.SendXMLEvent(env, xml, eventTypeName);
                return d.DocumentElement;
            }
            catch (Exception e) {
                throw new EPRuntimeException(e);
            }
        };

        private static readonly FunctionSendEventIntString FAVRO = (
            eventTypeName,
            env,
            a,
            b) => {
            var avroSchema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
            var datum = new GenericRecord(avroSchema);
            datum.Put("MyInt", a);
            datum.Put("MyString", b);
            env.SendEventAvro(datum, eventTypeName);
            return datum;
        };

        private static readonly FunctionSendEventIntString FJSON = (
            eventTypeName,
            env,
            a,
            b) => {
            var @object = new JObject
            {
                { "MyInt", a },
                { "MyString", b }
            };
            var json = @object.ToString(Formatting.None);
            env.SendEventJson(json, eventTypeName);
            return json;
        };

        public class MyLocalJsonProvided
        {
            public int? MyInt;
            public string MyString;
        }
    }
} // end of namespace