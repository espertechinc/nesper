///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
using NUnit.Framework.Legacy;
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
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEventSubA as " +
                          typeof(LocalEventSubA).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<IDictionary<string, string>>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else {
                    env.SendEventMap(Collections.SingletonDataMap("Mapped", nullable.Value), "LocalEvent");
                }
            };
            var mapepl = "@public @buseventtype create schema LocalEvent();\n";
            RunAssertion(env, mapepl, map);

            var mapType = "System.Collections.Generic.IDictionary<string, object>";
            
            // Object-array
            var oaepl =
                $"@public @buseventtype create objectarray schema LocalEvent();\n" + 
                $"@public @buseventtype create objectarray schema LocalEventSubA (Mapped `{mapType}`) inherits LocalEvent;\n";
            RunAssertion(env, oaepl, null);

            // Json
            Consumer<NullableObject<IDictionary<string, string>>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventJson(new JObject(new JProperty("Mapped", JValue.CreateNull())).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var mapped = new JObject();
                    @event.Add("Mapped", mapped);
                    foreach (var entry in nullable.Value) {
                        mapped.Add(entry.Key, entry.Value);
                    }

                    env.SendEventJson(@event.ToString(), "LocalEvent");
                }
            };
            RunAssertion(
                env,
                "@public @buseventtype @JsonSchema(Dynamic=true) create json schema LocalEvent();\n",
                json);

            // Json-Class-Provided
            RunAssertion(
                env,
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<NullableObject<IDictionary<string, string>>> avro = nullable => {
                var schema = SchemaBuilder.Record(
                    "name",
                    Field("Mapped", Map(StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)))));
                GenericRecord @event;
                if (nullable == null) {
                    // no action
                    @event = new GenericRecord(schema);
                    @event.Put("Mapped", Collections.EmptyDataMap);
                }
                else if (nullable.Value == null) {
                    @event = new GenericRecord(schema);
                    @event.Put("Mapped", Collections.EmptyDataMap);
                }
                else {
                    @event = new GenericRecord(schema);
                    @event.Put("Mapped", nullable.Value);
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
                        var g0 = eventType.GetGetter("Mapped('a')?");
                        var g1 = eventType.GetGetter("Mapped('b')?");
                        ClassicAssert.IsNull(g0);
                        ClassicAssert.IsNull(g1);
                    });
                env.UndeployAll();
                return;
            }
            
            var propepl =
                "@Name('s1') select " +
                "Mapped('a')? as c0, " + 
                "Mapped('b')? as c1," +
                "exists(Mapped('a')?) as c2, " +
                "exists(Mapped('b')?) as c3, " +
                "typeof(Mapped('a')?) as c4, " +
                "typeof(Mapped('b')?) as c5 from LocalEvent;\n";

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
            var g0 = @event.EventType.GetGetter("Mapped('a')?");
            var g1 = @event.EventType.GetGetter("Mapped('b')?");
            AssertGetter(@event, g0, existsZero, valueZero);
            AssertGetter(@event, g1, existsOne, valueOne);
        }

        private void AssertGetter(
            EventBean @event,
            EventPropertyGetter getter,
            bool exists,
            string value)
        {
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            string valueA,
            string valueB)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    ClassicAssert.AreEqual(valueA, @event.Get("c0"));
                    ClassicAssert.AreEqual(valueB, @event.Get("c1"));
                    ClassicAssert.AreEqual(valueA != null, @event.Get("c2"));
                    ClassicAssert.AreEqual(valueB != null, @event.Get("c3"));
                    ClassicAssert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
                    ClassicAssert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
                });
        }

        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            public LocalEventSubA(IDictionary<string, string> mapped)
            {
                this.Mapped = mapped;
            }

            public IDictionary<string, string> Mapped { get; }
        }

        public class MyLocalJsonProvided
        {
            public IDictionary<string, string> Mapped;
        }
    }
} // end of namespace