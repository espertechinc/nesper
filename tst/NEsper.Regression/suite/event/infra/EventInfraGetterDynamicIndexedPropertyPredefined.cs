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
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterDynamicIndexedPropertyPredefined : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<NullableObject<int?>> bean = nullable => {
                if (nullable == null) {
                    env.SendEventBean(new LocalEvent());
                }
                else if (nullable.Value == null) {
                    env.SendEventBean(new LocalEventSubA(null));
                }
                else {
                    var array = new LocalInnerEvent[nullable.Value.Value];
                    for (var i = 0; i < array.Length; i++) {
                        array[i] = new LocalInnerEvent();
                    }

                    env.SendEventBean(new LocalEventSubA(array));
                }
            };

            var beanepl =
                $"@public @buseventtype create schema LocalInnerEvent as {typeof(LocalInnerEvent).MaskTypeName()};\n" +
                $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n" +
                "@public @buseventtype create schema LocalEventSubA as " +
                typeof(LocalEventSubA).MaskTypeName() +
                ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<int?>> map = nullable => {
                if (nullable == null) {
                    env.SendEventMap(Collections.EmptyDataMap, "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventMap(Collections.SingletonDataMap("Array", null), "LocalEvent");
                }
                else {
                    var array = new IDictionary<string, object>[nullable.Value.Value];
                    for (var i = 0; i < array.Length; i++) {
                        array[i] = new Dictionary<string, object>();
                    }

                    env.SendEventMap(Collections.SingletonDataMap("Array", array), "LocalEvent");
                }
            };
            var mapepl =
                "@public @buseventtype create schema LocalInnerEvent();\n" +
                "@public @buseventtype create schema LocalEvent(Array LocalInnerEvent[]);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            var oaepl =
                "@public @buseventtype create objectarray schema LocalEvent();\n" +
                "@public @buseventtype create objectarray schema LocalEventSubA (Array string[]) inherits LocalEvent;\n";
            RunAssertion(env, oaepl, null);

            // Json
            Consumer<NullableObject<int?>> json = nullable => {
                if (nullable == null) {
                    env.SendEventJson("{}", "LocalEvent");
                }
                else if (nullable.Value == null) {
                    env.SendEventJson(new JObject(new JProperty("Array")).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var array = new JArray();
                    @event.Add("Array", array);
                    for (var i = 0; i < nullable.Value; i++) {
                        array.Add(new JObject());
                    }

                    env.SendEventJson(@event.ToString(), "LocalEvent");
                }
            };
            var epl =
                "@public @buseventtype create json schema LocalInnerEvent();\n" +
                "@public @buseventtype create json schema LocalEvent(Array LocalInnerEvent[]);\n";
            RunAssertion(env, epl, json);

            // Json-Class-Provided
            var eplJsonProvided =
                $"@JsonSchema(ClassName='{typeof(MyLocalJsonProvided).MaskTypeName()}') " +
                "@public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonProvided, json);

            // Avro
            Consumer<NullableObject<int?>> avro = nullable => {
                var inner = SchemaBuilder.Record("inner");
                var schema = SchemaBuilder.Record("name", TypeBuilder.Field("Array", TypeBuilder.Array(inner)));
                GenericRecord @event;
                if (nullable == null) {
                    // no action
                    @event = new GenericRecord(schema);
                    @event.Put("Array", EmptyList<GenericRecord>.Instance);
                }
                else if (nullable.Value == null) {
                    @event = new GenericRecord(schema);
                    @event.Put("Array", EmptyList<GenericRecord>.Instance);
                }
                else {
                    @event = new GenericRecord(schema);
                    var inners = new List<GenericRecord>();
                    for (var i = 0; i < nullable.Value; i++) {
                        inners.Add(new GenericRecord(inner));
                    }

                    @event.Put("Array", inners);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            var avroepl =
                "@public @buseventtype create avro schema LocalInnerEvent();\n" +
                "@public @buseventtype create avro schema LocalEvent(Array LocalInnerEvent[]);\n";
            RunAssertion(env, avroepl, avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<NullableObject<int?>> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("Array[0]?");
                        var g1 = eventType.GetGetter("Array[1]?");
                        ClassicAssert.IsNull(g0);
                        ClassicAssert.IsNull(g1);
                    });
                env.UndeployAll();
                return;
            }

            var propepl = "@name('s1') select Array[0]? as c0, Array[1]? as c1," +
                          "exists(Array[0]?) as c2, exists(Array[1]?) as c3, " +
                          "typeof(Array[0]?) as c4, typeof(Array[1]?) as c5 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new NullableObject<int?>(2));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, true));
            AssertProps(env, true, true);

            sender.Invoke(new NullableObject<int?>(1));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, false));
            AssertProps(env, true, false);

            sender.Invoke(new NullableObject<int?>(0));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, false));
            AssertProps(env, false, false);

            sender.Invoke(new NullableObject<int?>(null));
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, false));
            AssertProps(env, false, false);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, false));
            AssertProps(env, false, false);

            env.UndeployAll();
        }

        private void AssertGetters(
            EventBean @event,
            bool existsZero,
            bool existsOne)
        {
            var g0 = @event.EventType.GetGetter("Array[0]?");
            var g1 = @event.EventType.GetGetter("Array[1]?");
            AssertGetter(@event, g0, existsZero);
            AssertGetter(@event, g1, existsOne);
        }

        private void AssertGetter(
            EventBean @event,
            EventPropertyGetter getter,
            bool exists)
        {
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(exists, getter.Get(@event) != null);
            var beanBacked = @event.EventType is BeanEventType ||
                             SupportJsonEventTypeUtil.IsBeanBackedJson(@event.EventType);
            ClassicAssert.AreEqual(beanBacked && exists, getter.GetFragment(@event) != null);
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool hasA,
            bool hasB)
        {
            var @event = env.Listener("s1").AssertOneGetNewAndReset();
            ClassicAssert.AreEqual(hasA, @event.Get("c0") != null);
            ClassicAssert.AreEqual(hasB, @event.Get("c1") != null);
            ClassicAssert.AreEqual(hasA, @event.Get("c2"));
            ClassicAssert.AreEqual(hasB, @event.Get("c3"));
            ClassicAssert.AreEqual(hasA, @event.Get("c4") != null);
            ClassicAssert.AreEqual(hasB, @event.Get("c5") != null);
        }

        public class LocalInnerEvent
        {
        }

        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            public LocalEventSubA(LocalInnerEvent[] array)
            {
                this.Array = array;
            }

            public LocalInnerEvent[] Array { get; }
        }

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent[] Array;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
        }
    }
} // end of namespace