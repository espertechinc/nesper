///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraGetterIndexed : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<string[]> bean = Array => { env.SendEventBean(new LocalEvent(Array)); };
            var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
            RunAssertion(env, beanepl, bean);

            // Map
            Consumer<string[]> map = Array => {
                env.SendEventMap(Collections.SingletonDataMap("Array", Array), "LocalEvent");
            };
            var mapepl = "@public @buseventtype create schema LocalEvent(Array string[]);\n";
            RunAssertion(env, mapepl, map);

            // Object-Array
            Consumer<string[]> oa = array => { env.SendEventObjectArray(new object[] { array }, "LocalEvent"); };
            var oaepl = "@public @buseventtype create objectarray schema LocalEvent(Array string[]);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<string[]> json = array => {
                if (array == null) {
                    env.SendEventJson(new JObject(new JProperty("Array")).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var jsonarray = new JArray();
                    @event.Add("Array", jsonarray);
                    foreach (var @string in array) {
                        jsonarray.Add(@string);
                    }

                    env.SendEventJson(@event.ToString(), "LocalEvent");
                }
            };
            RunAssertion(env, "@public @buseventtype create json schema LocalEvent(Array string[]);\n", json);

            // Json-Class-Provided
            RunAssertion(
                env,
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<string[]> avro = array => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("Array", array == null ? EmptyList<string>.Instance : Arrays.AsList(array));
                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(
                env,
                "@name('schema') @public @buseventtype create avro schema LocalEvent(Array string[]);\n",
                avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<string[]> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            var propepl = "@name('s1') select Array[0] as c0, Array[1] as c1," +
                          "exists(Array[0]) as c2, exists(Array[1]) as c3, " +
                          "typeof(Array[0]) as c4, typeof(Array[1]) as c5 from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            sender.Invoke(new string[] { "a", "b" });
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", true, "b"));
            AssertProps(env, "a", "b");

            sender.Invoke(new string[] { "a" });
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "a", false, null));
            AssertProps(env, "a", null);

            sender.Invoke(Array.Empty<string>());
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            sender.Invoke(null);
            env.AssertEventNew("s0", @event => AssertGetters(@event, false, null, false, null));
            AssertProps(env, null, null);

            env.UndeployAll();
        }

        private void AssertGetters(
            EventBean @event,
            bool existsZero,
            string valueZero,
            bool existsOne,
            string valueOne)
        {
            var g0 = @event.EventType.GetGetter("Array[0]");
            var g1 = @event.EventType.GetGetter("Array[1]");
            AssertGetter(@event, g0, existsZero, valueZero);
            AssertGetter(@event, g1, existsOne, valueOne);
        }

        private void AssertGetter(
            EventBean @event,
            EventPropertyGetter getter,
            bool exists,
            string value)
        {
            Assert.AreEqual(exists, getter.IsExistsProperty(@event));
            Assert.AreEqual(value, getter.Get(@event));
            Assert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            string valueA,
            string valueB)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    Assert.AreEqual(valueA, @event.Get("c0"));
                    Assert.AreEqual(valueB, @event.Get("c1"));
                    Assert.AreEqual(valueA != null, @event.Get("c2"));
                    Assert.AreEqual(valueB != null, @event.Get("c3"));
                    Assert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
                    Assert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
                });
        }

        public class LocalEvent
        {
            public LocalEvent(string[] array)
            {
                this.Array = array;
            }

            public string[] Array { get; }
        }

        public class MyLocalJsonProvided
        {
            public string[] Array;
        }
    }
} // end of namespace