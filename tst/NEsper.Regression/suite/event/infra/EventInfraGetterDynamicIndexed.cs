///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using Array = System.Array;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterDynamicIndexed : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<string[]>> bean = nullable => {
                if (nullable == null) {
                    env.SendEventBean(new LocalEvent());
                }
                else {
                    env.SendEventBean(new LocalEventSubA(nullable.Value));
                }
            };
            var beanepl = "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEventSubA as " +
                          typeof(LocalEventSubA).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<string[]>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else {
                    env.SendEventMap(Collections.SingletonDataMap("array", nullable.Value), "LocalEvent");
                }
            };
            var mapepl = "@public @buseventtype create schema LocalEvent();\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent();\n" +
                        "@public @buseventtype create objectarray schema LocalEventSubA (array string[]) inherits LocalEvent;\n";
            RunAssertion(env, oaepl, null);

            // Json
            Consumer<NullableObject<string[]>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventJson(new JObject(new JProperty("array")).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var array = new JArray();
                    @event.Add("array", array);
                    foreach (var @string in nullable.Value) {
                        array.Add(@string);
                    }

                    env.SendEventJson(@event.ToString(), "LocalEvent");
                }
            };
            RunAssertion(
                env,
                "@public @buseventtype @JsonSchema(dynamic=true) create json schema LocalEvent();\n",
                json);

            // Json-Class-Provided
            RunAssertion(
                env,
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype @JsonSchema() create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<NullableObject<string[]>> avro = nullable => {
                var schema = SchemaBuilder.Record("name", Field("array", SchemaBuilder.Array(StringType())));
                GenericRecord @event;
                if (nullable == null) {
                    // no action
                    @event = new GenericRecord(schema);
                    @event.Put("array", EmptyList<GenericRecord>.Instance);
                }
                else if (nullable.Value == null) {
                    @event = new GenericRecord(schema);
                    @event.Put("array", EmptyList<GenericRecord>.Instance);
                }
                else {
                    @event = new GenericRecord(schema);
                    @event.Put("array", Arrays.AsList(nullable.Value));
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, "@public @buseventtype create avro schema LocalEvent();\n", avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<string[]>> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);
            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("array[0]?");
                        var g1 = eventType.GetGetter("array[1]?");
                        Assert.IsNull(g0);
                        Assert.IsNull(g1);
                    });
                env.UndeployAll();
                return;
            }

            var propepl = "@name('s1') select array[0]? as c0, array[1]? as c1," +
                          "exists(array[0]?) as c2, exists(array[1]?) as c3, " +
                          "typeof(array[0]?) as c4, typeof(array[1]?) as c5 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new NullableObject<string[]>(new string[] { "a", "b" }));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", true, "b"));
            AssertProps(env, "a", "b");

            sender.Invoke(new NullableObject<string[]>(new string[] { "a" }));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", false, null));
            AssertProps(env, "a", null);

            sender.Invoke(new NullableObject<string[]>(Array.Empty<string>()));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(new NullableObject<string[]>(null));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));

            env.UndeployAll();
        }

        private void AssertGetters(
            EventBean @event,
            bool existsZero,
            string valueZero,
            bool existsOne,
            string valueOne)
        {
            var g0 = @event.EventType.GetGetter("array[0]?");
            var g1 = @event.EventType.GetGetter("array[1]?");
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

        private void AssertProps(
            RegressionEnvironment env,
            string valueA,
            string valueB)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    Assert.AreEqual(valueA, @event.Get("c0"));
                    Assert.AreEqual(valueB, @event.Get("c1"));
                    Assert.AreEqual(valueA != null, @event.Get("c2"));
                    Assert.AreEqual(valueB != null, @event.Get("c3"));
                    Assert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
                    Assert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
                });
        }

        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            private string[] array;

            public LocalEventSubA(string[] array)
            {
                this.array = array;
            }

            public string[] GetArray()
            {
                return array;
            }
        }

        public class MyLocalJsonProvided
        {
            public string[] array;
        }
    }
} // end of namespace