///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraContainedIndexedWithIndex : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string[]> bean = ids => {
                var inners = new LocalInnerEvent[ids.Length];
                for (var i = 0; i < ids.Length; i++) {
                    inners[i] = new LocalInnerEvent(ids[i]);
                }

                env.SendEventBean(new LocalEvent(inners));
            };
            var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
                          typeof(LocalInnerEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = ids => {
                var inners = new IDictionary<string, object>[ids.Length];
                for (var i = 0; i < ids.Length; i++) {
                    inners[i] = Collections.SingletonDataMap("Id", ids[i]);
                }

                env.SendEventMap(Collections.SingletonDataMap("Indexed", inners), "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalInnerEvent(Id string);\n" +
                         "@public @buseventtype create schema LocalEvent(Indexed LocalInnerEvent[]);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<string[]> oa = ids => {
                var inners = new object[ids.Length][];
                for (var i = 0; i < ids.Length; i++) {
                    inners[i] = new object[] { ids[i] };
                }

                env.SendEventObjectArray(new object[] { inners }, "LocalEvent");
            };
            var oaepl = "@public @buseventtype create objectarray schema LocalInnerEvent(Id string);\n" +
                        "@public @buseventtype create objectarray schema LocalEvent(Indexed LocalInnerEvent[]);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<string[]> json = ids => {
                var array = new JArray();
                for (var i = 0; i < ids.Length; i++) {
                    array.Add(new JObject(new JProperty("Id", ids[i])));
                }

                var @event = new JObject(new JProperty("Indexed", array));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            var jsonepl = "@public @buseventtype create json schema LocalInnerEvent(Id string);\n" +
                          "@public @buseventtype create json schema LocalEvent(Indexed LocalInnerEvent[]);\n";
            RunAssertion(env, jsonepl, json);

            // Json-Class-Provided
            var jsonProvidedEpl = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonProvidedEpl, json);

            // Avro
            Consumer<string[]> avro = ids => {
                var schemaInner = env.RuntimeAvroSchemaByDeployment("schema", "LocalInnerEvent");
                ICollection<GenericRecord> inners = new List<GenericRecord>();
                for (var i = 0; i < ids.Length; i++) {
                    var inner = new GenericRecord(schemaInner.AsRecordSchema());
                    inner.Put("Id", ids[i]);
                    inners.Add(inner);
                }

                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("Indexed", inners);
                env.SendEventAvro(@event, "LocalEvent");
            };
            var avroepl = "@name('schema') @public @buseventtype create avro schema LocalInnerEvent(Id string);\n" +
                          "@public @buseventtype create avro schema LocalEvent(Indexed LocalInnerEvent[]);\n";
            RunAssertion(env, avroepl, avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string[]> sender)
        {
            env.CompileDeploy(
                    createSchemaEPL +
                    "@name('s0') select * from LocalEvent[Indexed[0]];\n" +
                    "@name('s1') select * from LocalEvent[Indexed[1]];\n"
                )
                .AddListener("s0")
                .AddListener("s1");

            sender.Invoke(new string[] { "a", "b" });
            env.AssertEqualsNew("s0", "Id", "a");
            env.AssertEqualsNew("s1", "Id", "b");

            env.UndeployAll();
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
            public LocalEvent(LocalInnerEvent[] indexed)
            {
                this.Indexed = indexed;
            }

            public LocalInnerEvent[] Indexed { get; }
        }

        public class MyLocalJsonProvided
        {
			public MyLocalJsonProvidedInnerEvent[] Indexed;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
			public string Id;
        }
    }
} // end of namespace