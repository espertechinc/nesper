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
    public class EventInfraGetterDynamicNestedDeep : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<Nullable2Lvl> bean = val => {
                LocalEvent @event;
                if (val.IsNullAtRoot) {
                    @event = new LocalEvent();
                }
                else if (val.IsNullAtInner) {
                    @event = new LocalEventSubA(new LocalInnerEvent(null));
                }
                else {
                    @event = new LocalEventSubA(new LocalInnerEvent(new LocalLeafEvent(val.Id)));
                }

                env.SendEventBean(@event, "LocalEvent");
            };
            var beanepl = "@public @buseventtype create schema LocalEvent as " +
                          typeof(EventInfraGetterDynamicNested.LocalEvent).FullName +
                          ";\n" +
                          "@public @buseventtype create schema LocalEventSubA as " +
                          typeof(EventInfraGetterDynamicNested.LocalEventSubA).FullName +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<Nullable2Lvl> map = val => {
                IDictionary<string, object> @event = new LinkedHashMap<string, object>();
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    var inner = Collections.SingletonDataMap("Leaf", null);
                    @event.Put("Property", inner);
                }
                else {
                    var leaf = Collections.SingletonDataMap("Id", val.Id);
                    var inner = Collections.SingletonDataMap("Leaf", leaf);
                    @event.Put("Property", inner);
                }

                env.SendEventMap(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            RunAssertion(env, GetEpl("objectarray"), null);

            // Json
            Consumer<Nullable2Lvl> json = val => {
                var @event = new JObject();
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    @event.Add("Property", new JObject(new JProperty("Leaf")));
                }
                else {
                    var leaf = new JObject(new JProperty("Id", val.Id));
                    var inner = new JObject(new JProperty("Leaf", leaf));
                    @event.Add("Property", inner);
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
            var leafSchema = SchemaBuilder.Record(
                "Leaf",
                Field("Id", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
            var innerSchema = SchemaBuilder.Record("inner", Field("Leaf", leafSchema));
            var topSchema = SchemaBuilder.Record("top", Field("Property", innerSchema));
            Consumer<Nullable2Lvl> avro = val => {
                GenericRecord @event;
                if (val.IsNullAtRoot) {
                    @event = new GenericRecord(topSchema);
                }
                else if (val.IsNullAtInner) {
                    var inner = new GenericRecord(innerSchema);
                    @event = new GenericRecord(topSchema);
                    @event.Put("Property", inner);
                }
                else {
                    var leaf = new GenericRecord(leafSchema);
                    leaf.Put("Id", val.Id);
                    var inner = new GenericRecord(innerSchema);
                    inner.Put("Leaf", leaf);
                    @event = new GenericRecord(topSchema);
                    @event.Put("Property", inner);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            env.AssertThat(
                () => RunAssertion(
                    env,
                    GetEpl("avro"),
                    avro)); // Avro assertion localized for serialization of null values not according to schema
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype @JsonSchema(Dynamic=true) create " + underlying + " schema LocalEvent();\n";
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<Nullable2Lvl> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("Property?.Leaf.Id");
                        Assert.IsNull(g0);
                    });
                env.UndeployAll();
                return;
            }

            var propepl =
                "@name('s1') select Property?.Leaf.Id as c0, exists(Property?.Leaf.Id) as c1, typeof(Property?.Leaf.Id) as c2 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new Nullable2Lvl(false, false, "a"));
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, "a"));
            AssertProps(env, true, "a");

            sender.Invoke(new Nullable2Lvl(false, false, null));
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, null));
            AssertProps(env, true, null);

            sender.Invoke(new Nullable2Lvl(false, true, null));
            env.AssertEventNew("s0", @event => AssertGetter(@event, false, null));
            AssertProps(env, false, null);

            sender.Invoke(new Nullable2Lvl(true, false, null));
            env.AssertEventNew("s0", @event => AssertGetter(@event, false, null));
            AssertProps(env, false, null);

            env.UndeployAll();
        }

        private void AssertGetter(
            EventBean @event,
            bool exists,
            string value)
        {
            var getter = @event.EventType.GetGetter("Property?.Leaf.Id");
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
        public class LocalLeafEvent
        {
            public LocalLeafEvent(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        [Serializable]
        public class LocalInnerEvent
        {
            public LocalInnerEvent(LocalLeafEvent leaf)
            {
                Leaf = leaf;
            }

            public LocalLeafEvent Leaf { get; }
        }

        [Serializable]
        public class LocalEvent
        {
        }

        public class LocalEventSubA : LocalEvent
        {
            public LocalEventSubA(LocalInnerEvent property)
            {
                Property = property;
            }

            public LocalInnerEvent Property { get; }
        }

        [Serializable]
        private class Nullable2Lvl
        {
            public Nullable2Lvl(
                bool nullAtRoot,
                bool nullAtInner,
                string id)
            {
                IsNullAtRoot = nullAtRoot;
                IsNullAtInner = nullAtInner;
                Id = id;
            }

            public bool IsNullAtRoot { get; }

            public bool IsNullAtInner { get; }

            public string Id { get; }
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public EventInfraGetterNestedSimpleDeep.MyLocalJsonProvidedInnerEvent property;
        }

        [Serializable]
        public class MyLocalJsonProvidedInnerEvent
        {
            public EventInfraGetterNestedSimpleDeep.MyLocalJsonProvidedLeafEvent leaf;
        }

        [Serializable]
        public class MyLocalJsonProvidedLeafEvent
        {
            public string id;
        }
    }
} // end of namespace