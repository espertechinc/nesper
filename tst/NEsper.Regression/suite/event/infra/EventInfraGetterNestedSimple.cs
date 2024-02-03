///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
                          typeof(LocalInnerEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<NullableObject<string>> map = nullable => {
                var property = nullable == null ? null : Collections.SingletonDataMap("Id", nullable.Value);
                env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
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
                        @event.Add("Property", new JObject(new JProperty("Id", nullable.Value)));
                    }
                    else {
                        @event.Add("Property", new JObject(new JProperty("Id", JValue.CreateNull())));
                    }
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
            Consumer<NullableObject<string>> avro = nullable => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                if (nullable != null) {
                    var inside = new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema());
                    inside.Put("Id", nullable.Value);
                    @event.Put("Property", inside);
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
                      "@name('s1') select Property.Id as c0, exists(Property.Id) as c1, typeof(Property.Id) as c2 from LocalEvent;\n";
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
            var getter = @event.EventType.GetGetter("Property.Id");
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private string GetEpl(string underlying)
        {
            return "@public @buseventtype create " +
                   underlying +
                   " schema LocalInnerEvent(Id string);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(Property LocalInnerEvent);\n";
        }

        public class LocalInnerEvent
        {
            public LocalInnerEvent(string id)
            {
                this.Id = id;
            }

            public string Id { get; }
        }

        public class LocalEvent
        {
            public LocalEvent(LocalInnerEvent property)
            {
                this.Property = property;
            }

            public LocalInnerEvent Property { get; }
        }

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent Property;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
            public string Id;
        }
    }
} // end of namespace