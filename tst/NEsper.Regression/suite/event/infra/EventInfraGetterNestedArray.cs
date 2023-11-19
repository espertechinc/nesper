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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterNestedArray : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string[]> bean = array => {
                LocalInnerEvent[] property;
                if (array == null) {
                    property = null;
                }
                else {
                    property = new LocalInnerEvent[array.Length];
                    for (var i = 0; i < array.Length; i++) {
                        property[i] = new LocalInnerEvent(array[i]);
                    }
                }

                env.SendEventBean(new LocalEvent(property));
            };
            var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
                          typeof(LocalInnerEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = array => {
                IDictionary<string, object>[] property;
                if (array == null) {
                    property = null;
                }
                else {
                    property = new IDictionary<string, object>[array.Length];
                    for (var i = 0; i < array.Length; i++) {
                        property[i] = Collections.SingletonDataMap("Id", array[i]);
                    }
                }

                env.SendEventMap(Collections.SingletonDataMap("property", property), "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<string[]> oa = array => {
                object[][] property;
                if (array == null) {
                    property = new object[][] { null };
                }
                else {
                    property = new object[array.Length][];
                    for (var i = 0; i < array.Length; i++) {
                        property[i] = new object[] { array[i] };
                    }
                }

                env.SendEventObjectArray(new object[] { property }, "LocalEvent");
            };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<string[]> json = array => {
                JToken property;
                if (array == null) {
                    property = null;
                }
                else {
                    var arr = new JArray();
                    for (var i = 0; i < array.Length; i++) {
                        arr.Add(new JObject(new JProperty("Id", array[i])));
                    }

                    property = arr;
                }

                env.SendEventJson(new JObject(new JProperty("property", property)).ToString(), "LocalEvent");
            };
            RunAssertion(env, GetEpl("json"), json);

            // Json-Class-Provided
            var eplJsonProvided = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonProvided, json);

            // Avro
            Consumer<string[]> avro = array => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                if (array == null) {
                    @event.Put("property", EmptyList<GenericRecord>.Instance);
                }
                else {
                    ICollection<GenericRecord> arr = new List<GenericRecord>();
                    for (var i = 0; i < array.Length; i++) {
                        var inner = new GenericRecord(
                            schema.GetField("property").Schema.AsArraySchema().ItemSchema.AsRecordSchema());
                        inner.Put("Id", array[i]);
                        arr.Add(inner);
                    }

                    @event.Put("property", arr);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("avro"), avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string[]> sender)
        {
            var epl = createSchemaEPL +
                      "@name('s0') select * from LocalEvent;\n" +
                      "@name('s1') select property[0].Id as c0, property[1].Id as c1," +
                      " exists(property[0].Id) as c2, exists(property[1].Id) as c3," +
                      " typeof(property[0].Id) as c4, typeof(property[1].Id) as c5" +
                      " from LocalEvent;\n";
            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

            sender.Invoke(new string[] { "a", "b" });
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", true, "b"));
            AssertProps(env, true, "a", true, "b");

            sender.Invoke(new string[] { "a" });
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", false, null));
            AssertProps(env, true, "a", false, null);

            sender.Invoke(Array.Empty<string>());
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, false, null, false, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, false, null, false, null);

            env.UndeployAll();
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool existsA,
            string expectedA,
            bool existsB,
            string expectedB)
        {
            env.AssertPropsNew(
                "s1",
                "c0,c1,c2,c3,c4,c5".SplitCsv(),
                new object[] {
                    expectedA, expectedB, existsA, existsB, existsA ? nameof(String) : null,
                    existsB ? nameof(String) : null
                });
        }

        private void AssertGetters(
            EventBean @event,
            bool existsZero,
            string valueZero,
            bool existsOne,
            string valueOne)
        {
            var g0 = @event.EventType.GetGetter("property[0].Id");
            var g1 = @event.EventType.GetGetter("property[1].Id");
            AssertGetter(@event, g0, existsZero, valueZero);
            AssertGetter(@event, g1, existsOne, valueOne);
        }

        private void AssertGetter(
            EventBean @event,
            EventPropertyGetter getter,
            bool exists,
            string value)
        {
            Assert.AreEqual(exists, getter.IsExistsProperty(@event));
            Assert.AreEqual(value, getter.Get(@event));
            Assert.IsNull(getter.GetFragment(@event));
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype create " +
                   underlying +
                   " schema LocalInnerEvent(Id string);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(property LocalInnerEvent[]);\n";
        }

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

        public class LocalEvent
        {
            private LocalInnerEvent[] property;

            public LocalEvent(LocalInnerEvent[] property)
            {
                this.property = property;
            }

            public LocalInnerEvent[] GetProperty()
            {
                return property;
            }
        }

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInner[] property;
        }

        public class MyLocalJsonProvidedInner
        {
            public string id;
        }
    }
} // end of namespace