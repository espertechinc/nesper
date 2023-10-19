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
    public class EventInfraGetterDynamicMapped : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<IDictionary<string, string>>> bean = nullable => {
                if (nullable == null) {
                    env.SendEventBean(new LocalEvent());
                }
                else {
                    env.SendEventBean(new LocalEventSubA(nullable.Value));
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
            Consumer<NullableObject<IDictionary<string, string>>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else {
                    env.SendEventMap(Collections.SingletonDataMap("mapped", nullable.Value), "LocalEvent");
                }
            };
            var mapepl = "@public @buseventtype create schema LocalEvent();\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent();\n" +
                        "@public @buseventtype create objectarray schema LocalEventSubA (mapped java.util.Map) inherits LocalEvent;\n";
            RunAssertion(env, oaepl, null);

            // Json
            Consumer<NullableObject<IDictionary<string, string>>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventJson(new JObject(new JProperty("mapped")).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var mapped = new JObject();
                    @event.Add("mapped", mapped);
                    foreach (var entry in nullable.Value) {
                        mapped.Add(entry.Key, entry.Value);
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
                "@JsonSchema(className='" +
                typeof(MyLocalJsonProvided).FullName +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<NullableObject<IDictionary<string, string>>> avro = nullable => {
                var schema = SchemaBuilder.Record(
                    "name",
                    Field("mapped", Map(StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));
                GenericRecord @event;
                if (nullable == null) {
                    // no action
                    @event = new GenericRecord(schema);
                    @event.Put("mapped", Collections.EmptyDataMap);
                }
                else if (nullable.Value == null) {
                    @event = new GenericRecord(schema);
                    @event.Put("mapped", Collections.EmptyDataMap);
                }
                else {
                    @event = new GenericRecord(schema);
                    @event.Put("mapped", nullable.Value);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, "@public @buseventtype create avro schema LocalEvent();\n", avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<IDictionary<string, string>>> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("mapped('a')?");
                        var g1 = eventType.GetGetter("mapped('b')?");
                        Assert.IsNull(g0);
                        Assert.IsNull(g1);
                    });
                env.UndeployAll();
                return;
            }

            var propepl = "@name('s1') select mapped('a')? as c0, mapped('b')? as c1," +
                          "exists(mapped('a')?) as c2, exists(mapped('b')?) as c3, " +
                          "typeof(mapped('a')?) as c4, typeof(mapped('b')?) as c5 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            IDictionary<string, string> values = new Dictionary<string, string>();
            values.Put("a", "x");
            values.Put("b", "y");
            sender.Invoke(new NullableObject<IDictionary<string, string>>(values));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "x", true, "y"));
            AssertProps(env, "x", "y");

            sender.Invoke(new NullableObject<IDictionary<string, string>>(Collections.SingletonMap("a", "x")));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "x", false, null));
            AssertProps(env, "x", null);

            sender.Invoke(new NullableObject<IDictionary<string, string>>(EmptyDictionary<string, string>.Instance));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(new NullableObject<IDictionary<string, string>>(null));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            env.UndeployAll();
        }

        private void AssertGetters(
            EventBean @event,
            bool existsZero,
            string valueZero,
            bool existsOne,
            string valueOne)
        {
            var g0 = @event.EventType.GetGetter("mapped('a')?");
            var g1 = @event.EventType.GetGetter("mapped('b')?");
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

        [Serializable]
        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            private IDictionary<string, string> mapped;

            public LocalEventSubA(IDictionary<string, string> mapped)
            {
                this.mapped = mapped;
            }

            public IDictionary<string, string> Mapped => mapped;
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public IDictionary<string, string> mapped;
        }
    }
} // end of namespace