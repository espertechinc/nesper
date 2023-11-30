///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraContainedNestedArray : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string[]> bean = ids => {
                var property = new LocalInnerEvent[ids.Length];
                for (var i = 0; i < ids.Length; i++) {
                    property[i] = new LocalInnerEvent(new LocalLeafEvent(ids[i]));
                }

                env.SendEventBean(new LocalEvent(property));
            };
            var beanepl =
                "@public @buseventtype create schema LocalLeafEvent as " + typeof(LocalLeafEvent).MaskTypeName() + ";\n" +
                "@public @buseventtype create schema LocalInnerEvent as " + typeof(LocalInnerEvent).MaskTypeName() + ";\n" +
                "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = ids => {
                var property = new IDictionary<string, object>[ids.Length];
                for (var i = 0; i < ids.Length; i++) {
                    property[i] = Collections.SingletonDataMap("Leaf", Collections.SingletonDataMap("Id", ids[i]));
                }

                env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<string[]> oa = ids => {
                var property = new object[ids.Length][];
                for (var i = 0; i < ids.Length; i++) {
                    property[i] = new object[] { new object[] { ids[i] } };
                }

                env.SendEventObjectArray(new object[] { property }, "LocalEvent");
            };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<string[]> json = ids => {
                var property = new JArray();
                for (var i = 0; i < ids.Length; i++) {
                    var inner = new JObject(new JProperty("Leaf", new JObject(new JProperty("Id", ids[i]))));
                    property.Add(inner);
                }

                env.SendEventJson(new JObject(new JProperty("Property", property)).ToString(), "LocalEvent");
            };
            RunAssertion(env, GetEpl("json"), json);

            // Json-Class-Provided
            var eplJsonProvided =
                "@JsonSchema(ClassName='" + typeof(MyLocalJsonProvided).MaskTypeName() + "') " + 
                "@public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonProvided, json);

            // Avro
            Consumer<string[]> avro = ids => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var property = new List<GenericRecord>();
                for (var i = 0; i < ids.Length; i++) {
                    var leaf = new GenericRecord(
                        schema.GetField("Property")
                            .Schema.AsArraySchema()
                            .ItemSchema.GetField("Leaf")
                            .Schema.AsRecordSchema());
                    leaf.Put("Id", ids[i]);
                    var inner = new GenericRecord(
                        schema.GetField("Property")
                            .Schema.AsArraySchema()
                            .ItemSchema.AsRecordSchema());
                    inner.Put("Leaf", leaf);
                    property.Add(inner);
                }

                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("Property", property);
                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("avro"), avro);
        }

        private string GetEpl(string underlying)
        {
            return "create " + underlying + " schema LocalLeafEvent(Id string);\n" +
                   "create " + underlying + " schema LocalInnerEvent(Leaf LocalLeafEvent);\n" +
                   "@name('schema') @public @buseventtype create " + underlying + " schema LocalEvent(Property LocalInnerEvent[]);\n";
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string[]> sender)
        {
            env.CompileDeploy(
                    createSchemaEPL +
                    "@name('s0') select * from LocalEvent[Property[0].Leaf];\n" +
                    "@name('s1') select * from LocalEvent[Property[1].Leaf];\n")
                .AddListener("s0")
                .AddListener("s1");

            sender.Invoke("a,b".SplitCsv());
            env.AssertEqualsNew("s0", "Id", "a");
            env.AssertEqualsNew("s1", "Id", "b");

            env.UndeployAll();
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
            public LocalEvent(LocalInnerEvent[] property)
            {
                this.Property = property;
            }

            public LocalInnerEvent[] Property { get; }
        }

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent[] Property;
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