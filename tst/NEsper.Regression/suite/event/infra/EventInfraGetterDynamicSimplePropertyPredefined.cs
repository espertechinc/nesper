///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterDynamicSimplePropertyPredefined : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string> bean = property => { env.SendEventBean(new LocalEvent(property)); };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string> map = property => {
                env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
            };
            RunAssertion(env, GetEpl("map"), map);

            // Object-array
            Consumer<string> oa = property => { env.SendEventObjectArray(new object[] { property }, "LocalEvent"); };
            RunAssertion(env, GetEpl("objectarray"), oa);

            // Json
            Consumer<string> json = property => {
                if (property == null) {
                    env.SendEventJson(new JObject(new JProperty("Property", JValue.CreateNull())).ToString(), "LocalEvent");
                }
                else {
                    env.SendEventJson(new JObject(new JProperty("Property", property)).ToString(), "LocalEvent");
                }
            };
            RunAssertion(env, GetEpl("json"), json);

            // Json-Class-Predefined
            var eplJsonPredefined = "@JsonSchema(ClassName='" +
                                    typeof(MyLocalJsonProvided).MaskTypeName() +
                                    "') @buseventtype @public " +
                                    "create json schema LocalEvent();\n";
            RunAssertion(env, eplJsonPredefined, json);

            // Avro
            Consumer<string> avro = property => {
                var schema = SchemaBuilder.Record("name", OptionalString("Property"));
                var @event = new GenericRecord(schema);
                @event.Put("Property", property);
                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(env, GetEpl("avro"), avro);
        }

        private string GetEpl(string underlying)
        {
            return "@name('schema') @buseventtype @public create " +
                   underlying +
                   " schema LocalEvent(Property string);\n";
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            if (sender == null) {
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        var g0 = eventType.GetGetter("Property?");
                        ClassicAssert.IsNull(g0);
                    });
                env.UndeployAll();
                return;
            }

            var propepl =
                "@name('s1') select Property? as c0, exists(Property?) as c1, typeof(Property?) as c2 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke("a");
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, "a"));
            AssertProps(env, true, "a");

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetter(@event, true, null));
            AssertProps(env, true, null);

            env.UndeployAll();
        }

        private void AssertGetter(
            EventBean @event,
            bool exists,
            string value)
        {
            var getter = @event.EventType.GetGetter("Property?");
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            bool exists,
            string value)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    ClassicAssert.AreEqual(value, @event.Get("c0"));
                    ClassicAssert.AreEqual(exists, @event.Get("c1"));
                    ClassicAssert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
                });
        }

        public class LocalEvent
        {
            public LocalEvent(string property)
            {
                this.Property = property;
            }

            public string Property { get; }
        }

        public class MyLocalJsonProvided
        {
            public string Property;
        }
    }
} // end of namespace