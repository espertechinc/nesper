///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraContainedNested : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string> bean = id => {
                env.SendEventBean(new LocalEvent(new LocalInnerEvent(new LocalLeafEvent(id))));
            };
            var beanepl = "@public @buseventtype create schema LocalLeafEvent as " +
                          typeof(LocalLeafEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalInnerEvent as " +
                          typeof(LocalInnerEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string> map = id => {
                var leaf = Collections.SingletonDataMap("Id", id);
                var inner = Collections.SingletonDataMap("leaf", leaf);
                env.SendEventMap(Collections.SingletonDataMap("property", inner), "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<string> oa = id => {
                var leaf = new object[] { id };
                var inner = new object[] { leaf };
                env.SendEventObjectArray(new object[] { inner }, "LocalEvent");
            };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<string> json = id => {
                var leaf = new JObject(new JProperty("Id", id));
                var inner = new JObject(new JProperty("leaf", leaf));
                var @event = new JObject(new JProperty("property", inner));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            RunAssertion(env, GetEpl("json"), json);

            // Json-Class-Provided
            var eplJsonProvided = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonProvided, json);

            // Avro
            Consumer<string> avro = id => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var leaf = new GenericRecord(
                    schema.GetField("property").Schema.GetField("leaf").Schema.AsRecordSchema());
                leaf.Put("Id", id);
                var inner = new GenericRecord(schema.GetField("property").Schema.AsRecordSchema());
                inner.Put("leaf", leaf);
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("property", inner);
                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("avro"), avro);
        }

        private string GetEpl(string underlying)
        {
            return "create " +
                   underlying +
                   " schema LocalLeafEvent(Id string);\n" +
                   "create " +
                   underlying +
                   " schema LocalInnerEvent(leaf LocalLeafEvent);\n" +
                   "@name('schema') @public @buseventtype create " +
                   underlying +
                   " schema LocalEvent(property LocalInnerEvent);\n";
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string> sender)
        {
            env.CompileDeploy(createSchemaEPL + "@name('s0') select * from LocalEvent[property.leaf];\n")
                .AddListener("s0");

            sender.Invoke("a");
            env.AssertEqualsNew("s0", "Id", "a");

            env.UndeployAll();
        }

        public class LocalLeafEvent
        {
            private readonly string id;

            public LocalLeafEvent(string id)
            {
                this.id = id;
            }

            public string GetId()
            {
                return id;
            }
        }

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

        public class LocalEvent
        {
            private LocalInnerEvent property;

            public LocalEvent(LocalInnerEvent property)
            {
                this.property = property;
            }

            public LocalInnerEvent GetProperty()
            {
                return property;
            }
        }

        public class MyLocalJsonProvided
        {
            public MyLocalJsonProvidedInnerEvent property;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
            public MyLocalJsonProvidedLeafEvent leaf;
        }

        public class MyLocalJsonProvidedLeafEvent
        {
            public string id;
        }
    }
} // end of namespace