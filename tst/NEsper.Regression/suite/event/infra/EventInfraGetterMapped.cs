///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
    public class EventInfraGetterMapped : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // Bean
            Consumer<IDictionary<string, string>> bean = entries => {
                env.SendEventBean(new LocalEvent(entries));
            };
            var beanepl = $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n";
            RunAssertion(env, beanepl, bean);
            
			var properties = typeof(IDictionary<string, string>).CleanName();
			
            // Map
            Consumer<IDictionary<string, string>> map = entries => {
                env.SendEventMap(Collections.SingletonDataMap("Mapped", entries), "LocalEvent");
            };
			var mapepl = $"@public @buseventtype create schema LocalEvent(Mapped `{properties}`);\n";
            RunAssertion(env, mapepl, map);

            // Object-array
            Consumer<IDictionary<string, string>> oa = entries => {
                env.SendEventObjectArray(new object[] { entries }, "LocalEvent");
            };
			var oaepl = $"@public @buseventtype create objectarray schema LocalEvent(Mapped `{properties}`);\n";
            RunAssertion(env, oaepl, oa);

            // Json
            Consumer<IDictionary<string, string>> json = entries => {
                if (entries == null) {
                    env.SendEventJson(new JObject(new JProperty("Mapped", JValue.CreateNull())).ToString(), "LocalEvent");
                }
                else {
                    var @event = new JObject();
                    var mapped = new JObject();
                    @event.Add("Mapped", mapped);
                    foreach (var entry in entries) {
                        mapped.Add(entry.Key, entry.Value);
                    }

                    env.SendEventJson(@event.ToString(), "LocalEvent");
                }
            };
            RunAssertion(
                env,
                $"@public @buseventtype @JsonSchema(Dynamic=true) create json schema LocalEvent(Mapped `{properties}`);\n",
                json);

            // Json-Class-Provided
            RunAssertion(
                env,
                "@JsonSchema(ClassName='" +
                typeof(MyLocalJsonProvided).MaskTypeName() +
                "') @public @buseventtype create json schema LocalEvent();\n",
                json);

            // Avro
            Consumer<IDictionary<string, string>> avro = entries => {
                var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
                var @event = new GenericRecord(schema.AsRecordSchema());
                @event.Put("Mapped", entries ?? EmptyDictionary<string, string>.Instance);
                env.SendEventAvro(@event, "LocalEvent");
            };
            RunAssertion(
                env,
                $"@name('schema') @public @buseventtype create avro schema LocalEvent(Mapped `{properties}`);\n",
                avro);
        }

        public void RunAssertion(
            RegressionEnvironment env,
            string createSchemaEPL,
            Consumer<IDictionary<string, string>> sender)
        {
            var path = new RegressionPath();
            env.CompileDeploy(createSchemaEPL, path);

            env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

            var propepl =
                "@name('s1') select " +
                "Mapped('a') as c0, " +
                "Mapped('b') as c1," +
                "exists(Mapped('a')) as c2, " +
                "exists(Mapped('b')) as c3, " +
                "typeof(Mapped('a')) as c4, " +
                "typeof(Mapped('b')) as c5 " +
                "from LocalEvent;\n";
            env.CompileDeploy(propepl, path).AddListener("s1");

            IDictionary<string, string> values = new Dictionary<string, string>();
            values.Put("a", "x");
            values.Put("b", "y");
            sender.Invoke(values);
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "x", true, "y"));
            AssertProps(env, "x", "y");

            sender.Invoke(Collections.SingletonMap("a", "x"));
            env.AssertEventNew("s0", @event => AssertGetters(@event, true, "x", false, null));
            AssertProps(env, "x", null);

            sender.Invoke(EmptyDictionary<string, string>.Instance);
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
            var g0 = @event.EventType.GetGetter("Mapped('a')");
            var g1 = @event.EventType.GetGetter("Mapped('b')");
            AssertGetter(@event, g0, existsZero, valueZero);
            AssertGetter(@event, g1, existsOne, valueOne);
        }

        private void AssertGetter(
            EventBean @event,
            EventPropertyGetter getter,
            bool exists,
            string value)
        {
            ClassicAssert.AreEqual(exists, getter.IsExistsProperty(@event));
            ClassicAssert.AreEqual(value, getter.Get(@event));
            ClassicAssert.IsNull(getter.GetFragment(@event));
        }

        private void AssertProps(
            RegressionEnvironment env,
            string valueA,
            string valueB)
        {
            env.AssertEventNew(
                "s1",
                @event => {
                    ClassicAssert.AreEqual(valueA, @event.Get("c0"));
                    ClassicAssert.AreEqual(valueB, @event.Get("c1"));
                    ClassicAssert.AreEqual(valueA != null, @event.Get("c2"));
                    ClassicAssert.AreEqual(valueB != null, @event.Get("c3"));
                    ClassicAssert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
                    ClassicAssert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
                });
        }

        public class LocalEvent
        {
            [System.Text.Json.Serialization.JsonConstructor]
            public LocalEvent(IDictionary<string, string> mapped)
            {
                Mapped = mapped;
            }

            public IDictionary<string, string> Mapped { get; }
        }

        public class MyLocalJsonProvided
        {
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedMember.Global
			public IDictionary<string, string> Mapped;
        }
    }
} // end of namespace