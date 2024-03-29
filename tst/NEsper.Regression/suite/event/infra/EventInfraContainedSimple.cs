///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using static NEsper.Avro.Core.AvroConstant;
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraContainedSimple : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string> bean = id => { env.SendEventBean(new LocalEvent(new LocalInnerEvent(id))); };
            var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
                          typeof(LocalInnerEvent).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema LocalEvent as " +
                          typeof(LocalEvent).MaskTypeName() +
                          ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string> map = id => {
                env.SendEventMap(
                    Collections.SingletonDataMap("Property", Collections.SingletonDataMap("Id", id)),
                    "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalInnerEvent(Id string);\n" +
                         "@public @buseventtype create schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<string> oa = id => {
                env.SendEventObjectArray(new object[] { new object[] { id } }, "LocalEvent");
            };
            var oaepl = "@public @buseventtype create objectarray schema LocalInnerEvent(Id string);\n" +
                        "@public @buseventtype create objectarray schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<string> json = id => {
                var @event = new JObject(new JProperty("Property", new JObject(new JProperty("Id", id))));
                env.SendEventJson(@event.ToString(), "LocalEvent");
            };
            var jsonepl = "@public @buseventtype create json schema LocalInnerEvent(Id string);\n" +
                          "@public @buseventtype create json schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, jsonepl, json);

            // Json-Class-Provided
            var jsonProvidedEpl = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonProvidedEpl, json);

            // Avro
            Consumer<string> avro = id => {
                var schema = SchemaBuilder.Record(
                    "name",
                    Field("Id", StringType(Property(PROP_STRING_KEY, PROP_STRING_VALUE))));
                var inside = new GenericRecord(schema);
                inside.Put("Id", id);
                var schemaEvent = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var @event = new GenericRecord(schemaEvent.AsRecordSchema());
                @event.Put("Property", inside);
                env.SendEventAvro(@event, "LocalEvent");
            };
            var avroepl = "@name('schema') @public @buseventtype create avro schema LocalInnerEvent(Id string);\n" +
                          "@public @buseventtype create avro schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, avroepl, avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string> sender)
        {
            env.CompileDeploy(createSchemaEPL + "@name('s0') select * from LocalEvent[Property];\n").AddListener("s0");

            sender.Invoke("a");
            env.AssertEqualsNew("s0", "Id", "a");

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
            public LocalEvent(LocalInnerEvent property)
            {
                this.Property = property;
            }

            public LocalInnerEvent Property { get; }
        }

        public class MyLocalJsonProvided
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Global
            public MyLocalJsonProvidedInnerEvent Property;
        }

        public class MyLocalJsonProvidedInnerEvent
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once UnusedMember.Global
            public string Id;
        }
    }
} // end of namespace