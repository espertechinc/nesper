///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraGetterMapped : RegressionExecution {
	    public void Run(RegressionEnvironment env) {
	        // Bean
	        Consumer<IDictionary<string, string>> bean = entries => {
	            env.SendEventBean(new LocalEvent(entries));
	        };
	        var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
	        RunAssertion(env, beanepl, bean);

	        // Map
	        Consumer<IDictionary<string, string>> map = entries => {
	            env.SendEventMap(Collections.SingletonDataMap("mapped", entries), "LocalEvent");
	        };
	        var mapepl = "@public @buseventtype create schema LocalEvent(mapped java.util.Map);\n";
	        RunAssertion(env, mapepl, map);

	        // Object-array
	        Consumer<IDictionary<string, string>> oa = entries => {
	            env.SendEventObjectArray(new object[]{entries}, "LocalEvent");
	        };
	        var oaepl = "@public @buseventtype create objectarray schema LocalEvent(mapped java.util.Map);\n";
	        RunAssertion(env, oaepl, oa);

	        // Json
	        Consumer<IDictionary<string, string>> json = entries => {
	            if (entries == null) {
	                env.SendEventJson(new JObject(new JProperty("mapped")).ToString(), "LocalEvent");
	            } else {
	                var @event = new JObject();
	                var mapped = new JObject();
	                @event.Add("mapped", mapped);
	                foreach (var entry in entries) {
	                    mapped.Add(entry.Key, entry.Value);
	                }
	                env.SendEventJson(@event.ToString(), "LocalEvent");
	            }
	        };
	        RunAssertion(env, "@public @buseventtype @JsonSchema(dynamic=true) create json schema LocalEvent(mapped java.util.Map);\n", json);

	        // Json-Class-Provided
	        RunAssertion(env, "@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype create json schema LocalEvent();\n", json);

	        // Avro
	        Consumer<IDictionary<string, string>> avro = entries => {
	            var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
	            var @event = new GenericRecord(schema.AsRecordSchema());
	            @event.Put("mapped", entries == null ? EmptyDictionary<string, string>.Instance : entries);
	            env.SendEventAvro(@event, "LocalEvent");
	        };
	        RunAssertion(env, "@name('schema') @public @buseventtype create avro schema LocalEvent(mapped java.util.Map);\n", avro);
	    }

	    public void RunAssertion(RegressionEnvironment env,
	                             string createSchemaEPL,
	                             Consumer<IDictionary<string, string>> sender) {

	        var path = new RegressionPath();
	        env.CompileDeploy(createSchemaEPL, path);

	        env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");

	        var propepl = "@name('s1') select mapped('a') as c0, mapped('b') as c1," +
	                      "exists(mapped('a')) as c2, exists(mapped('b')) as c3, " +
	                      "typeof(mapped('a')) as c4, typeof(mapped('b')) as c5 from LocalEvent;\n";
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

	    private void AssertGetters(EventBean @event, bool existsZero, string valueZero, bool existsOne, string valueOne) {
	        var g0 = @event.EventType.GetGetter("mapped('a')");
	        var g1 = @event.EventType.GetGetter("mapped('b')");
	        AssertGetter(@event, g0, existsZero, valueZero);
	        AssertGetter(@event, g1, existsOne, valueOne);
	    }

	    private void AssertGetter(EventBean @event, EventPropertyGetter getter, bool exists, string value) {
	        Assert.AreEqual(exists, getter.IsExistsProperty(@event));
	        Assert.AreEqual(value, getter.Get(@event));
	        Assert.IsNull(getter.GetFragment(@event));
	    }

	    private void AssertProps(RegressionEnvironment env, string valueA, string valueB) {
	        env.AssertEventNew("s1", @event => {
	            Assert.AreEqual(valueA, @event.Get("c0"));
	            Assert.AreEqual(valueB, @event.Get("c1"));
	            Assert.AreEqual(valueA != null, @event.Get("c2"));
	            Assert.AreEqual(valueB != null, @event.Get("c3"));
	            Assert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
	            Assert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
	        });
	    }

	    [Serializable]
	    public class LocalEvent {
	        private IDictionary<string, string> mapped;

	        public LocalEvent(IDictionary<string, string> mapped) {
	            this.mapped = mapped;
	        }

	        public IDictionary<string, string> Mapped => mapped;
	    }

	    [Serializable]
	    public class MyLocalJsonProvided {
	        public IDictionary<string, string> mapped;
	    }
	}
} // end of namespace
