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
using NUnit.Framework.Legacy;

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
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
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
                    @event.Add("Property", new JObject(new JProperty("Leaf", JValue.CreateNull())));
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
            var eplJsonProvided = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
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
                    var inner = new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema());
                    @event.Put("Property", inner);
                }
                else {
                    var leaf = new GenericRecord(
                        schema.GetField("Property")
                            .Schema
                            .GetField("Leaf")
                            .Schema.AsRecordSchema());

                    leaf.Put("Id", val.Id);
                    var inner = new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema());
                    inner.Put("Leaf", leaf);
                    @event.Put("Property", inner);
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
                      "@name('s1') select Property.Leaf.Id as c0, exists(Property.Leaf.Id) as c1, typeof(Property.Leaf.Id) as c2 from LocalEvent;\n";
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
            var getter = @event.EventType.GetGetter("Property.Leaf.Id");
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype create " +
                   underlying +
                   " schema LocalLeafEvent(Id string);\n" +
                   "@public @buseventtype create " +
                   underlying +
                   " schema LocalInnerEvent(Leaf LocalLeafEvent);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(Property LocalInnerEvent);\n";
        }

        public class LocalLeafEvent
        {
            public LocalLeafEvent(string id)
            {
                this.Id = id;
            }

            public string Id { get; }
        }

        public class LocalInnerEvent
        {
            public LocalInnerEvent(LocalLeafEvent leaf)
            {
                this.Leaf = leaf;
            }

            public LocalLeafEvent Leaf { get; }
        }

        public class LocalEvent
        {
            public LocalEvent(LocalInnerEvent property)
            {
                this.Property = property;
            }

            public LocalInnerEvent Property { get; }
        }

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

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent Property;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
            public MyLocalJsonProvidedLeafEvent Leaf;
        }

        public class MyLocalJsonProvidedLeafEvent
        {
            public string Id;
        }
    }
} // end of namespace