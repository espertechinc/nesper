///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNested : RegressionExecution
    {
        private const string BEAN_TYPENAME = nameof(SupportBeanDynRoot);
        public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "Json";
        public const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "JsonProvided";

        public void Run(RegressionEnvironment env)
        {
            RunAssertion(env, EventRepresentationChoice.OBJECTARRAY, "");
            RunAssertion(env, EventRepresentationChoice.MAP, "");
            RunAssertion(
                env,
                EventRepresentationChoice.AVRO,
                "@AvroSchemaField(name='myid',schema='[\"int\",{\"type\":\"string\",\"avro.java.string\":\"String\"},\"null\"]')");
            RunAssertion(env, EventRepresentationChoice.DEFAULT, "");
            RunAssertion(env, EventRepresentationChoice.JSON, "");
        }

        private void RunAssertion(
            RegressionEnvironment env,
            EventRepresentationChoice outputEventRep,
            string additionalAnnotations)
        {
            var path = new RegressionPath();

            // Bean
            var beanTests = new Pair<object, object>[] {
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_S0(101)), Exists(101)),
                new Pair<object, object>(new SupportBeanDynRoot("abc"), NotExists()),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_A("e1")), Exists("e1")),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_B("e2")), Exists("e2")),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_S1(102)), Exists(102))
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                BEAN_TYPENAME,
                FBEAN,
                null,
                beanTests,
                typeof(object),
                path);

            // Map
            var mapTests = new Pair<object, object>[] {
                new Pair<object, object>(Collections.EmptyDataMap, NotExists()),
                new Pair<object, object>(
                    Collections.SingletonMap("item", Collections.SingletonMap("id", 101)),
                    Exists(101)),
                new Pair<object, object>(Collections.SingletonMap("item", Collections.EmptyDataMap), NotExists()),
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                MAP_TYPENAME,
                FMAP,
                null,
                mapTests,
                typeof(object),
                path);

            // Object array
            var oaTests = new Pair<object, object>[] {
                new Pair<object, object>(new object[] { null }, NotExists()),
                new Pair<object, object>(new object[] { new SupportBean_S0(101) }, Exists(101)),
                new Pair<object, object>(new object[] { "abc" }, NotExists()),
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                OA_TYPENAME,
                FOA,
                null,
                oaTests,
                typeof(object),
                path);

            // XML
            var xmlTests = new Pair<object, object>[] {
                new Pair<object, object>("<item id=\"101\"/>", Exists("101")),
                new Pair<object, object>("<item/>", NotExists()),
            };
            if (!outputEventRep.IsAvroOrJsonEvent()) {
                RunAssertion(
                    env,
                    outputEventRep,
                    additionalAnnotations,
                    XML_TYPENAME,
                    FXML,
                    xmlToValue,
                    xmlTests,
                    typeof(XmlNode),
                    path);
            }

            // Avro
            var avroTests = new Pair<object, object>[] {
                new Pair<object, object>(null, Exists(null)),
                new Pair<object, object>(101, Exists(101)),
                new Pair<object, object>("abc", Exists("abc")),
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                AVRO_TYPENAME,
                FAVRO,
                null,
                avroTests,
                typeof(object),
                path);

            // Json
            var jsonTests = new Pair<object, object>[] {
                new Pair<object, object>("{}", NotExists()),
                new Pair<object, object>("{\"item\": { \"id\": 101} }", Exists(101)),
                new Pair<object, object>("{\"item\": { \"id\": \"abc\"} }", Exists("abc")),
            };
            var schemasJson = "@JsonSchema(dynamic=true) create json schema Undefined();\n" +
                              "@public @buseventtype @name('schema') create json schema " +
                              JSON_TYPENAME +
                              "(item Undefined)";
            env.CompileDeploy(schemasJson, path);
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                JSON_TYPENAME,
                FJSON,
                null,
                jsonTests,
                typeof(object),
                path);

            // Json-Provided (class is provided)
            var schemasJsonProvided =
                "@JsonSchema(className='" +
                typeof(MyLocalJsonProvidedItem).FullName +
                "') @public @buseventtype @name('schema') create json schema Item();\n" +
                "@JsonSchema(className='" +
                typeof(MyLocalJsonProvided).FullName +
                "') @public @buseventtype @name('schema') create json schema " +
                JSONPROVIDED_TYPENAME +
                "(item Item)";
            env.CompileDeploy(schemasJsonProvided, path);
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                JSONPROVIDED_TYPENAME,
                FJSON,
                null,
                jsonTests,
                typeof(object),
                path);
        }

        private void RunAssertion(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string additionalAnnotations,
            string typename,
            SupportEventInfra.FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType,
            RegressionPath path)
        {
            var stmtText = "@name('s0') " +
                           eventRepresentationEnum.GetAnnotationText() +
                           additionalAnnotations +
                           " select " +
                           "item.id? as myid, " +
                           "exists(item.id?) as exists_myid " +
                           "from " +
                           typename +
                           ";\n" +
                           "@name('s1') select * from " +
                           typename +
                           ";\n";
            env.CompileDeploy(stmtText, path).AddListener("s0").AddListener("s1");

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType("myid"));
                    Assert.AreEqual(typeof(bool?), Boxing.GetBoxedType(eventType.GetPropertyType("exists_myid")));
                    Assert.IsTrue(eventRepresentationEnum.MatchesClass(eventType.UnderlyingType));
                });

            foreach (var pair in tests) {
                send.Invoke(env, pair.First, typename);
                var expected = (ValueWithExistsFlag)pair.Second;
                env.AssertEventNew(
                    "s0",
                    @event => SupportEventInfra.AssertValueMayConvert(
                        @event,
                        "myid",
                        expected,
                        optionalValueConversion));

                env.AssertEventNew(
                    "s1",
                    @out => {
                        var getter = @out.EventType.GetGetter("item.id?");

                        if (!typename.Equals(XML_TYPENAME)) {
                            Assert.AreEqual(expected.Value, getter.Get(@out));
                        }
                        else {
                            var item = (XmlNode)getter.Get(@out);
                            Assert.AreEqual(expected.Value, item?.InnerText);
                        }

                        Assert.AreEqual(expected.IsExists, getter.IsExistsProperty(@out));
                    });
            }

            env.UndeployAll();
        }

        private static readonly SupportEventInfra.FunctionSendEvent FAVRO = (
            env,
            value,
            typeName) => {
            var schema = env.RuntimeAvroSchemaPreconfigured(typeName).AsRecordSchema();
            var itemSchema = schema.GetField("item").Schema.AsRecordSchema();
            var itemDatum = new GenericRecord(itemSchema);
            itemDatum.Put("id", value);
            var datum = new GenericRecord(schema);
            datum.Put("item", itemDatum);
            env.SendEventAvro(datum, typeName);
        };

        [Serializable]
        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedItem item;
        }

        [Serializable]
        public class MyLocalJsonProvidedItem
        {
            public object id;
        }
    }
} // end of namespace