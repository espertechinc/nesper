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
    public class EventInfraGetterNestedSimpleDeep : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<Nullable2Lvl> bean = val => {
                LocalEvent @event;
                if (val.IsNullAtRoot) {
                    @event = new LocalEvent(null);
                }
                else if (val.IsNullAtInner) {
                    @event = new LocalEvent(new LocalInnerEvent(null));
                }
                else {
                    @event = new LocalEvent(new LocalInnerEvent(new LocalLeafEvent(val.Id)));
                }

                env.SendEventBean(@event);
            };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<Nullable2Lvl> map = val => {
                IDictionary<string, object> @event = new LinkedHashMap<string, object>();
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    var inner = Collections.SingletonDataMap("leaf", null);
                    @event.Put("property", inner);
                }
                else {
                    var leaf = Collections.SingletonDataMap("id", val.Id);
                    var inner = Collections.SingletonDataMap("leaf", leaf);
                    @event.Put("property", inner);
                }

                env.SendEventMap(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<Nullable2Lvl> oa = val => {
                var @event = new object[1];
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    var inner = new object[] { null };
                    @event[0] = inner;
                }
                else {
                    var leaf = new object[] { val.Id };
                    var inner = new object[] { leaf };
                    @event[0] = inner;
                }

                env.SendEventObjectArray(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<Nullable2Lvl> json = val => {
                var @event = new JObject();
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    @event.Add("property", new JObject(new JProperty("leaf")));
                }
                else {
                    var leaf = new JObject(new JProperty("id", val.Id));
                    var inner = new JObject(new JProperty("leaf", leaf));
                    @event.Add("property", inner);
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
            Consumer<Nullable2Lvl> avro = val => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                if (val.IsNullAtRoot) {
                    // no change
                }
                else if (val.IsNullAtInner) {
                    var inner = new GenericRecord(schema.GetField("property").Schema.AsRecordSchema());
                    @event.Put("property", inner);
                }
                else {
                    var leaf = new GenericRecord(
                        schema.GetField("property")
                            .Schema
                            .GetField("leaf")
                            .Schema.AsRecordSchema());

                    leaf.Put("id", val.Id);
                    var inner = new GenericRecord(schema.GetField("property").Schema.AsRecordSchema());
                    inner.Put("leaf", leaf);
                    @event.Put("property", inner);
                }

                env.SendEventAvro(@event, "LocalEvent");
            };
            env.AssertThat(() => RunAssertion(env, GetEpl("avro"), avro));
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<Nullable2Lvl> sender)
        {
            var epl = createSchemaEPL +
                      "@name('s0') select * from LocalEvent;\n" +
                      "@name('s1') select property.leaf.id as c0, exists(property.leaf.id) as c1, typeof(property.leaf.id) as c2 from LocalEvent;\n";
            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

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
            var getter = @event.EventType.GetGetter("property.leaf.id");
            Assert.AreEqual(exists, getter.IsExistsProperty(@event));
            Assert.AreEqual(value, getter.Get(@event));
            Assert.IsNull(getter.GetFragment(@event));
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype create " +
                   underlying +
                   " schema LocalLeafEvent(id string);\n" +
                   "@public @buseventtype create " +
                   underlying +
                   " schema LocalInnerEvent(leaf LocalLeafEvent);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(property LocalInnerEvent);\n";
        }

        [Serializable]
        public class LocalLeafEvent
        {
            private readonly string id;

            public LocalLeafEvent(string id)
            {
                this.id = id;
            }

            public string Id => id;
        }

        [Serializable]
        public class LocalInnerEvent
        {
            private readonly LocalLeafEvent leaf;

            public LocalInnerEvent(LocalLeafEvent leaf)
            {
                this.leaf = leaf;
            }

            public LocalLeafEvent GetLeaf()
            {
                return leaf;
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

            public LocalInnerEvent Property => property;
        }

        [Serializable]
        public class Nullable2Lvl
        {
            private readonly bool nullAtRoot;
            private readonly bool nullAtInner;
            private readonly string id;

            public Nullable2Lvl(
                bool nullAtRoot,
                bool nullAtInner,
                string id)
            {
                this.nullAtRoot = nullAtRoot;
                this.nullAtInner = nullAtInner;
                this.id = id;
            }

            public bool IsNullAtRoot => nullAtRoot;

            public bool IsNullAtInner => nullAtInner;

            public string Id => id;
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent property;
        }

        [Serializable]
        public class MyLocalJsonProvidedInnerEvent
        {
            public MyLocalJsonProvidedLeafEvent leaf;
        }

        [Serializable]
        public class MyLocalJsonProvidedLeafEvent
        {
            public string id;
        }
    }
} // end of namespace