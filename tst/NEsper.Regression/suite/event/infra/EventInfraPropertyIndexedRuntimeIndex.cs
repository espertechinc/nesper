///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

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
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = values => {
                env.SendEventMap(Collections.SingletonDataMap("indexed", values), "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalEvent(indexed string[]);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<string[]> oa = values => { env.SendEventObjectArray(new object[] { values }, "LocalEvent"); };
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent(indexed string[]);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<string[]> json = values => {
                var array = new JArray();
                for (var i = 0; i < values.Length; i++) {
                    array.Add(values[i]);
                }

                var @event = new JObject(new JProperty("indexed", array));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            var jsonepl = "@public @buseventtype create json schema LocalEvent(indexed string[]);\n";
            RunAssertion(env, jsonepl, json);

            // Json-Class-Provided
            var jsonProvidedEpl = "@JsonSchema(className='" +
                                  typeof(MyLocalJsonProvided).FullName +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonProvidedEpl, json);

            // Avro
            Consumer<string[]> avro = values => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent").AsRecordSchema();
                var @event = new GenericRecord(schema);
                @event.Put("indexed", Arrays.AsList(values));
                env.SendEventAvro(@event, "LocalEvent");
            };
            var avroepl = "@name('schema') @public @buseventtype create avro schema LocalEvent(indexed string[]);\n";
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
                    "@name('s0') select indexed(offsetNum+0) as c0, indexed(offsetNum+1) as c1 from LocalEvent as e;\n"
                )
                .AddListener("s0");

            sender.Invoke(new string[] { "a", "b" });
            env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "a", "b" });

            env.UndeployAll();
        }

        [Serializable]
        public class LocalEvent
        {
            private string[] indexed;

            public LocalEvent(string[] indexed)
            {
                this.indexed = indexed;
            }

            public string[] GetIndexed()
            {
                return indexed;
            }
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public string[] indexed;
        }
    }
} // end of namespace