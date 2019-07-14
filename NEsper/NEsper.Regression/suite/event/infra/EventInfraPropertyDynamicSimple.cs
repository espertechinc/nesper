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

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicSimple : RegressionExecution
    {
        public static readonly string XML_TYPENAME = typeof(EventInfraPropertyDynamicSimple).FullName + "XML";
        public static readonly string MAP_TYPENAME = typeof(EventInfraPropertyDynamicSimple).FullName + "Map";
        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyDynamicSimple).FullName + "OA";
        public static readonly string AVRO_TYPENAME = typeof(EventInfraPropertyDynamicSimple).FullName + "Avro";

        public void Run(RegressionEnvironment env)
        {
            // Bean
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(new SupportMarkerImplA("e1"), Exists("e1")),
                new Pair<object, object>(new SupportMarkerImplB(1), Exists(1)),
                new Pair<object, object>(new SupportMarkerImplC(), NotExists())
            };
            RunAssertion(env, typeof(SupportMarkerInterface).Name, FBEAN, null, beanTests, typeof(object));

            // Map
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(Collections.SingletonMap("somekey", "10"), NotExists()),
                new Pair<object, object>(Collections.SingletonMap("id", "abc"), Exists("abc")),
                new Pair<object, object>(Collections.SingletonMap("id", 10), Exists(10))
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(new object[] {1, null}, Exists(null)),
                new Pair<object, object>(new object[] {2, "abc"}, Exists("abc")),
                new Pair<object, object>(new object[] {3, 10}, Exists(10))
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>("", NotExists()),
                new Pair<object, object>("<id>10</id>", Exists("10")),
                new Pair<object, object>("<id>abc</id>", Exists("abc"))
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var avroSchema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
            var datumEmpty = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            var datumOne = new GenericRecord(avroSchema.AsRecordSchema());
            datumOne.Put("id", 101);
            var datumTwo = new GenericRecord(avroSchema.AsRecordSchema());
            datumTwo.Put("id", null);
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(datumEmpty, NotExists()),
                new Pair<object, object>(datumOne, Exists(101)),
                new Pair<object, object>(datumTwo, Exists(null))
            };
            RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "@Name('s0') select id? as myid, exists(id?) as exists_myid from " + typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            Assert.AreEqual(expectedPropertyType, env.Statement("s0").EventType.GetPropertyType("myid"));
            Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("exists_myid"));

            foreach (var pair in tests) {
                send.Invoke(env, pair.First, typename);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertValueMayConvert(
                    @event,
                    "myid",
                    (ValueWithExistsFlag) pair.Second,
                    optionalValueConversion);
            }

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