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

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicSimple : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Json";

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            // Bean
            var beanTests = new Pair<object, object>[] {
                new Pair<object, object>(new SupportMarkerImplA("e1"), Exists("e1")),
                new Pair<object, object>(new SupportMarkerImplB(1), Exists(1)),
                new Pair<object, object>(new SupportMarkerImplC(), NotExists())
            };
            RunAssertion(env, nameof(SupportMarkerInterface), FBEAN, null, beanTests, typeof(object), path);

            // Map
            var mapTests = new Pair<object, object>[] {
                new Pair<object, object>(Collections.SingletonMap("somekey", "10"), NotExists()),
                new Pair<object, object>(Collections.SingletonMap("id", "abc"), Exists("abc")),
                new Pair<object, object>(Collections.SingletonMap("id", 10), Exists(10)),
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

            // Object-Array
            var oaTests = new Pair<object, object>[] {
                new Pair<object, object>(new object[] { 1, null }, Exists(null)),
                new Pair<object, object>(new object[] { 2, "abc" }, Exists("abc")),
                new Pair<object, object>(new object[] { 3, 10 }, Exists(10)),
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

            // XML
            var xmlTests = new Pair<object, object>[] {
                new Pair<object, object>("", NotExists()),
                new Pair<object, object>("<id>10</id>", Exists("10")),
                new Pair<object, object>("<id>abc</id>", Exists("abc")),
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

            // Avro
            var avroSchema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
            var datumEmpty = new GenericRecord(avroSchema);
            var datumOne = new GenericRecord(avroSchema);
            datumOne.Put("id", 101);
            var datumTwo = new GenericRecord(avroSchema);
            datumTwo.Put("id", null);
            var avroTests = new Pair<object, object>[] {
                new Pair<object, object>(datumEmpty, Exists(null)),
                new Pair<object, object>(datumOne, Exists(101)),
                new Pair<object, object>(datumTwo, Exists(null))
            };
            RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path);

            // Json
            env.CompileDeploy(
                "@JsonSchema(dynamic=true) @public @buseventtype create json schema " + JSON_TYPENAME + "()",
                path);
            var jsonTests = new Pair<object, object>[] {
                new Pair<object, object>("{}", NotExists()),
                new Pair<object, object>("{\"id\": 10}", Exists(10)),
                new Pair<object, object>("{\"id\": \"abc\"}", Exists("abc"))
            };
            RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            SupportEventInfra.FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType,
            RegressionPath path)
        {
            var stmtText = "@name('s0') select id? as myid, exists(id?) as exists_myid from " + typename;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(expectedPropertyType, statement.EventType.GetPropertyType("myid"));
                    Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("exists_myid"));

                    foreach (var pair in tests) {
                        send.Invoke(env, pair.First, typename);
                        env.AssertEventNew(
                            "s0",
                            @event => SupportEventInfra.AssertValueMayConvert(
                                @event,
                                "myid",
                                (ValueWithExistsFlag)pair.Second,
                                optionalValueConversion));
                    }
                });

            env.UndeployAll();
        }

        private void AddMapEventType(RegressionEnvironment env)
        {
        }

        private void AddOAEventType(RegressionEnvironment env)
        {
        }

        private void AddAvroEventType(RegressionEnvironment env)
        {
        }
    }
} // end of namespace