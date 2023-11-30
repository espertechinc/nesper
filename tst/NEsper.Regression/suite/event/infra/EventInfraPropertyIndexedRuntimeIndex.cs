///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class EventInfraPropertyIndexedRuntimeIndex : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string[]> bean = values => { env.SendEventBean(new LocalEvent(values)); };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = values => {
                env.SendEventMap(Collections.SingletonDataMap("Indexed", values), "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalEvent(Indexed string[]);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<string[]> oa = values => { env.SendEventObjectArray(new object[] { values }, "LocalEvent"); };
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent(Indexed string[]);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<string[]> json = values => {
                var array = new JArray();
                for (var i = 0; i < values.Length; i++) {
                    array.Add(values[i]);
                }

                var @event = new JObject(new JProperty("Indexed", array));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            var jsonepl = "@public @buseventtype create json schema LocalEvent(Indexed string[]);\n";
            RunAssertion(env, jsonepl, json);

            // Json-Class-Provided
            var jsonProvidedEpl = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonProvidedEpl, json);

            // Avro
            Consumer<string[]> avro = values => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("Indexed", Arrays.AsList(values));
                env.SendEventAvro(@event, "LocalEvent");
            };
            var avroepl = "@name('schema') @public @buseventtype create avro schema LocalEvent(Indexed string[]);\n";
            RunAssertion(env, avroepl, avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string[]> sender)
        {
            env.CompileDeploy(
                    createSchemaEPL +
                    "create constant variable int offsetNum = 0;" +
                    "@name('s0') select Indexed(offsetNum+0) as c0, Indexed(offsetNum+1) as c1 from LocalEvent as e;\n"
                )
                .AddListener("s0");

            sender.Invoke(new string[] { "a", "b" });
            env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "a", "b" });

            env.UndeployAll();
        }

        public class LocalEvent
        {
            public LocalEvent(string[] indexed)
            {
                this.Indexed = indexed;
            }

            public string[] Indexed { get; }
        }

        public class MyLocalJsonProvided
        {
            public string[] Indexed;
        }
    }
} // end of namespace