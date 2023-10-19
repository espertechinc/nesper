///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterNestedSimple : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<string>> bean = nullable => {
                var property = nullable == null ? null : new LocalInnerEvent(nullable.Value);
                env.SendEventBean(new LocalEvent(property));
            };
            var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
                          typeof(LocalInnerEvent).FullName +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).FullName +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<string>> map = nullable => {
                var property = nullable == null ? null : Collections.SingletonDataMap("id", nullable.Value);
                env.SendEventMap(Collections.SingletonDataMap("property", property), "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<NullableObject<string>> oa = nullable => {
                var property = nullable == null ? null : new object[] { nullable.Value };
                env.SendEventObjectArray(new object[] { property }, "LocalEvent");
            };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<NullableObject<string>> json = nullable => {
                var @event = new JObject();
                if (nullable != null) {
                    if (nullable.Value != null) {
                        @event.Add("property", new JObject(new JProperty("id", nullable.Value)));
                    }
                    else {
                        @event.Add("property", new JObject(new JProperty("id")));
                    }
                }

                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            RunAssertion(env, GetEpl("json"), json);

            // Json-Class-Provided
            var eplJsonProvided = "@JsonSchema(className='" +
                                  typeof(MyLocalJsonProvided).FullName +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonProvided, json);

            // Avro
            Consumer<NullableObject<string>> avro = nullable => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                if (nullable != null) {
                    var inside = new GenericRecord(schema.GetField("property").Schema.AsRecordSchema());
                    inside.Put("id", nullable.Value);
                    @event.Put("property", inside);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<string>> sender)
        {
            var epl = createSchemaEPL +
                      "@name('s0') select * from LocalEvent;\n" +
                      "@name('s1') select property.id as c0, exists(property.id) as c1, typeof(property.id) as c2 from LocalEvent;\n";
            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

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

        private void AssertProps(
            RegressionEnvironment env,
            bool exists,
            string expected)
        {
            env.AssertPropsNew(
                "s1",
                "c0,c1,c2".SplitCsv(),
                new object[] { expected, exists, expected != null ? nameof(String) : null });
        }

        private void AssertGetter(
            EventBean @event,
            bool exists,
            string value)
        {
            var getter = @event.EventType.GetGetter("property.id");
            Assert.AreEqual(exists, getter.IsExistsProperty(@event));
            Assert.AreEqual(value, getter.Get(@event));
            Assert.IsNull(getter.GetFragment(@event));
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype create " +
                   underlying +
                   " schema LocalInnerEvent(id string);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(property LocalInnerEvent);\n";
        }

        [Serializable]
        public class LocalInnerEvent
        {
            private readonly string id;

            public LocalInnerEvent(string id)
            {
                this.id = id;
            }

            public string GetId()
            {
                return id;
            }
        }

        [Serializable]
        public class LocalEvent
        {
            private LocalInnerEvent property;

            public LocalEvent(LocalInnerEvent property)
            {
                this.property = property;
            }

            public LocalInnerEvent GetProperty()
            {
                return property;
            }
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