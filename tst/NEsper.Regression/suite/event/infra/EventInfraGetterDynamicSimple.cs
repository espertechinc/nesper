///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;
using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

using Array = System.Array;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterDynamicSimple : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<string>> bean = nullable => {
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
            RunAssertion(env, beanepl, bean, false);

            // Map
            Consumer<NullableObject<string>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else {
                    env.SendEventMap(Collections.SingletonDataMap("Property", nullable.Value), "LocalEvent");
                }
            };
            var mapepl = "@public @buseventtype create schema LocalEvent();\n";
            RunAssertion(env, mapepl, map, false);

            // Object-array
            Consumer<NullableObject<string>> oa = nullable => {
                if (nullable == null) {
                    env.SendEventObjectArray(Array.Empty<object>(), "LocalEvent");
                }
                else {
                    env.SendEventObjectArray(new object[] { nullable.Value }, "LocalEventSubA");
                }
            };
            var oaepl =
                "@public @buseventtype create objectarray schema LocalEvent();\n" +
                "@public @buseventtype create objectarray schema LocalEventSubA (Property string) inherits LocalEvent;\n";
            RunAssertion(env, oaepl, oa, false);

            // Json
            Consumer<NullableObject<string>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventJson(new JObject(new JProperty("Property", JValue.CreateNull())).ToString(), "LocalEvent");
                }
                else {
                    env.SendEventJson(new JObject(new JProperty("Property", nullable.Value)).ToString(), "LocalEvent");
                }
            };
            RunAssertion(
                env,
                "@public @buseventtype @JsonSchema(Dynamic=true) create json schema LocalEvent();\n",
                json,
                false);

            // Json-Class-Provided
            RunAssertion(
                env,
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json,
                true);

            // Avro
            Consumer<NullableObject<string>> avro = nullable => {
                var schema = SchemaBuilder.Record(
                    "name",
                    Field("Property", Union(StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE)), NullType())));
                GenericRecord @event;
                if (nullable == null) {
                    // no action
                    @event = new GenericRecord(schema);
                }
                else if (nullable.Value == null) {
                    @event = new GenericRecord(schema);
                }
                else {
                    @event = new GenericRecord(schema);
                    @event.Put("Property", nullable.Value);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, "@public @buseventtype create avro schema LocalEvent();\n", avro, true);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<string>> sender,
            bool beanBackedJsonOrAvro)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("Property?");
                        ClassicAssert.IsNull(g0);
                    });
                env.UndeployAll();
                return;
            }

            var propepl =
                "@name('s1') select Property? as c0, exists(Property?) as c1, typeof(Property?) as c2 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new NullableObject<string>("a"));
            env.AssertEventNew("s0", @event => 
                AssertGetter(@event, beanBackedJsonOrAvro, true, "a"));
            AssertProps(env, beanBackedJsonOrAvro, true, "a");

            sender.Invoke(new NullableObject<string>(null));
            env.AssertEventNew("s0", @event =>
                AssertGetter(@event, beanBackedJsonOrAvro, true, null));
            AssertProps(env, beanBackedJsonOrAvro, true, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event =>
                AssertGetter(@event, beanBackedJsonOrAvro, false, null));
            AssertProps(env, beanBackedJsonOrAvro, false, null);

            env.UndeployAll();
        }

        private void AssertGetter(
            EventBean @event,
            bool beanBackedJason,
            bool exists,
            string value)
        {
            var getter = @event.EventType.GetGetter("Property?");
            ClassicAssert.AreEqual(beanBackedJason || exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool beanBackedJason,
            bool exists,
            string value)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    ClassicAssert.AreEqual(value, @event.Get("c0"));
                    ClassicAssert.AreEqual(beanBackedJason || exists, @event.Get("c1"));
                    ClassicAssert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
                });
        }

        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            public LocalEventSubA(string property)
            {
                this.Property = property;
            }

            public string Property { get; }
        }

        public class MyLocalJsonProvided
        {
            public string Property;
        }
    }
} // end of namespace