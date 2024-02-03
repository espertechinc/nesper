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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterSimpleFragment : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<bool> bean = hasValue => {
                env.SendEventBean(new LocalEvent(hasValue ? new LocalInnerEvent() : null));
            };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<bool> map = hasValue => {
                env.SendEventMap(
                    Collections.SingletonDataMap("Property", hasValue ? Collections.EmptyDataMap : null),
                    "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalInnerEvent();\n" +
                         "@public @buseventtype create schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<bool> oa = hasValue => {
                env.SendEventObjectArray(new object[] { hasValue ? Array.Empty<object>() : null }, "LocalEvent");
            };
            var oaepl = "@public @buseventtype create objectarray schema LocalInnerEvent();\n" +
                        "@public @buseventtype create objectarray schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<bool> json = hasValue => {
                var jsonObject = new JObject(
                    hasValue
                        ? new JProperty("Property", new JObject())
                        : new JProperty("Property", JValue.CreateNull()));
                env.SendEventJson(jsonObject.ToString(), "LocalEvent");
            };
            var jsonepl = "@public @buseventtype create json schema LocalInnerEvent();\n" +
                          "@public @buseventtype create json schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, jsonepl, json);

            // Json-Class-Provided
            var jsonprovidedepl = "@JsonSchema(ClassName='" +
                                  typeof(MyLocalJsonProvided).MaskTypeName() +
                                  "') @public @buseventtype create json schema LocalEvent();\n";
            RunAssertion(env, jsonprovidedepl, json);

            // Avro
            Consumer<bool> avro = hasValue => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var theEvent = new GenericRecord(schema.AsRecordSchema());
                theEvent.Put(
                    "Property",
                    hasValue ? new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema()) : null);
                env.SendEventAvro(theEvent, "LocalEvent");
            };
            var avroepl = "@public @buseventtype create avro schema LocalInnerEvent();\n" +
                          "@name('schema') @public @buseventtype create avro schema LocalEvent(Property LocalInnerEvent);\n";
            RunAssertion(env, avroepl, avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<bool> sender)
        {
            var epl = createSchemaEPL +
                      "@name('s0') select * from LocalEvent;\n" +
                      "@name('s1') select Property as c0, exists(Property) as c1, typeof(Property) as c2 from LocalEvent;\n";
            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

            sender.Invoke(true);
            env.AssertEventNew("s0", @event => AssertGetter(@event, true));
            AssertProps(env, true);

            sender.Invoke(false);
            env.AssertEventNew("s0", @event => AssertGetter(@event, false));
            AssertProps(env, false);

            env.UndeployAll();
        }

        private void AssertGetter(
            EventBean @event,
            bool hasValue)
        {
            var getter = @event.EventType.GetGetter("Property");
            ClassicAssert.IsTrue(getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(hasValue, getter.Get(@event) != null);
            ClassicAssert.AreEqual(hasValue, getter.GetFragment(@event) != null);
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool hasValue)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    ClassicAssert.IsTrue((bool)@event.Get("c1"));
                    ClassicAssert.AreEqual(hasValue, @event.Get("c0") != null);
                    ClassicAssert.AreEqual(hasValue, @event.Get("c2") != null);
                });
        }

        public class LocalInnerEvent
        {
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
        }
    }
} // end of namespace