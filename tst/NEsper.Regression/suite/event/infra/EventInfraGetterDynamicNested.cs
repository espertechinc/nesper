///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterDynamicNested : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<string>> bean = nullable => {
                if (nullable == null) {
                    env.SendEventBean(new LocalEvent());
                }
                else {
                    env.SendEventBean(new LocalEventSubA(new LocalInnerEvent(nullable.Value)));
                }
            };
            var beanepl = "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).FullName +
                          ";\n" +
                          "@public @buseventtype create schema LocalEventSubA as " +
                          typeof(LocalEventSubA).FullName +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<string>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else {
                    var inner = Collections.SingletonDataMap("Id", nullable.Value);
                    env.SendEventMap(Collections.SingletonDataMap("property", inner), "LocalEvent");
                }
            };
            RunAssertion(env, GetEPL("map"), map);

            // Object-array
            RunAssertion(env, GetEPL("objectarray"), null);

            // Json
            Consumer<NullableObject<string>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else {
                    var inner = new JObject(new JProperty("Id", nullable.Value));
                    env.SendEventJson(new JObject(new JProperty("property", inner)).ToString(), "LocalEvent");
                }
            };
            RunAssertion(env, GetEPL("json"), json);

            // Json-Class-Provided
            var jsonProvidedEPL = "@JsonSchema(className='" +
                                  typeof(MyLocalJsonProvided).FullName +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonProvidedEPL, json);

            // Avro
            Consumer<NullableObject<string>> avro = nullable => {
                GenericRecord @event;

                var innerSchema = SchemaBuilder.Record(
                    "Inner",
                    Field("Id", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
                var schema = SchemaBuilder.Record("name", Field("Property", innerSchema));
                if (nullable == null) {
                    @event = new GenericRecord(schema);
                }
                else {
                    var inner = new GenericRecord(innerSchema);
                    inner.Put("Id", nullable.Value);
                    @event = new GenericRecord(schema);
                    @event.Put("property", inner);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            env.AssertThat(
                () => RunAssertion(env, GetEPL("avro"), avro)); // Avro may not serialize well when incomplete
        }

        private string GetEPL(string underlying)
        {
            return "@public @buseventtype @JsonSchema(dynamic=true) create " + underlying + " schema LocalEvent();\n";
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<string>> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("property?.Id");
                        Assert.IsNull(g0);
                    });
                env.UndeployAll();
                return;
            }

            var propepl =
                "@name('s1') select property?.Id as c0, exists(property?.Id) as c1, typeof(property?.Id) as c2 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new NullableObject<string>("a"));
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, "a"));
            AssertProps(env, true, "a");

            sender.Invoke(new NullableObject<string>(null));
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, null));
            AssertProps(env, true, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetter(@event, false, null));
            AssertProps(env, false, null);

            env.UndeployAll();
        }

        private void AssertGetter(
            EventBean @event,
            bool exists,
            string value)
        {
            var getter = @event.EventType.GetGetter("property?.Id");
            Assert.AreEqual(exists, getter.IsExistsProperty(@event));
            Assert.AreEqual(value, getter.Get(@event));
            Assert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool exists,
            string value)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    Assert.AreEqual(value, @event.Get("c0"));
                    Assert.AreEqual(exists, @event.Get("c1"));
                    Assert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
                });
        }

        [Serializable]
        public class LocalEvent
        {
        }

        [Serializable]
        public class LocalInnerEvent
        {
            private readonly string id;

            public LocalInnerEvent(string id)
            {
                this.id = id;
            }

            public string Id => id;
        }

        public class LocalEventSubA : LocalEvent
        {
            private LocalInnerEvent property;

            public LocalEventSubA(LocalInnerEvent property)
            {
                this.property = property;
            }

            public LocalInnerEvent Property => property;
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent property;
        }

        [Serializable]
        public class MyLocalJsonProvidedInnerEvent
        {
            public string id;
        }
    }
} // end of namespace