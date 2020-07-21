///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraContainedNested : RegressionExecution
	{
	    public void Run(RegressionEnvironment env) {
	        // Bean
	        BiConsumer<EventType, string> bean = (type, id) => {
	            env.SendEventBean(new LocalEvent(new LocalInnerEvent(new LocalLeafEvent(id))));
	        };
	        var beanepl = "@public @buseventtype create schema LocalLeafEvent as " + typeof(LocalLeafEvent).FullName + ";\n" +
	                      "@public @buseventtype create schema LocalInnerEvent as " + typeof(LocalInnerEvent).FullName + ";\n" +
	                      "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
	        RunAssertion(env, beanepl, bean);

	        // Map
	        BiConsumer<EventType, string> map = (type, id) => {
	            var leaf = Collections.SingletonDataMap("id", id);
	            var inner = Collections.SingletonDataMap("leaf", leaf);
	            env.SendEventMap(Collections.SingletonDataMap("property", inner), "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("map"), map);

	        // Object-array
	        BiConsumer<EventType, string> oa = (type, id) => {
	            var leaf = new object[]{id};
	            var inner = new object[]{leaf};
	            env.SendEventObjectArray(new object[]{inner}, "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("objectarray"), oa);

	        // Json
	        BiConsumer<EventType, string> json = (type, id) => {
	            var leaf = new JObject(new JProperty("id", id));
	            var inner = new JObject(new JProperty("leaf", leaf));
	            var @event = new JObject(new JProperty("property", inner));
	            env.SendEventJson(@event.ToString(), "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("json"), json);

	        // Json-Class-Provided
	        var eplJsonProvided = "@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype create json schema LocalEvent();\n";
	        RunAssertion(env, eplJsonProvided, json);

	        // Avro
	        BiConsumer<EventType, string> avro = (type, id) => {
	            var schema = SupportAvroUtil.GetAvroSchema(type).AsRecordSchema();
	            var leaf = new GenericRecord(schema.GetField("property")
		            .Schema.GetField("leaf").Schema.AsRecordSchema());
	            leaf.Put("id", id);
	            var inner = new GenericRecord(schema.GetField("property")
		            .Schema.AsRecordSchema());
	            inner.Put("leaf", leaf);
	            var @event = new GenericRecord(schema);
	            @event.Put("property", inner);
	            env.SendEventAvro(@event, "LocalEvent");
	        };
	        RunAssertion(env, GetEpl("avro"), avro);
	    }

	    private string GetEpl(string underlying) {
	        return "create " + underlying + " schema LocalLeafEvent(id string);\n" +
	            "create " + underlying + " schema LocalInnerEvent(leaf LocalLeafEvent);\n" +
	            "@public @buseventtype create " + underlying + " schema LocalEvent(property LocalInnerEvent);\n";
	    }

	    public void RunAssertion(RegressionEnvironment env,
	                             string createSchemaEPL,
	                             BiConsumer<EventType, string> sender) {

	        env.CompileDeploy(createSchemaEPL + "@name('s0') select * from LocalEvent[property.leaf];\n").AddListener("s0");
	        var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

	        sender.Invoke(eventType, "a");
	        var @event = env.Listener("s0").AssertOneGetNewAndReset();
	        Assert.AreEqual("a", @event.Get("id"));

	        env.UndeployAll();
	    }

	    public class LocalLeafEvent {
	        private readonly string id;

	        public LocalLeafEvent(string id) {
	            this.id = id;
	        }

	        public string GetId() {
	            return id;
	        }
	    }

	    public class LocalInnerEvent {
	        private readonly LocalLeafEvent leaf;

	        public LocalInnerEvent(LocalLeafEvent leaf) {
	            this.leaf = leaf;
	        }

	        public LocalLeafEvent GetLeaf() {
	            return leaf;
	        }
	    }

	    public class LocalEvent {
	        private LocalInnerEvent property;

	        public LocalEvent(LocalInnerEvent property) {
	            this.property = property;
	        }

	        public LocalInnerEvent GetProperty() {
	            return property;
	        }
	    }

	    [Serializable] public class MyLocalJsonProvided{
	        public MyLocalJsonProvidedInnerEvent property;
	    }

	    [Serializable] public class MyLocalJsonProvidedInnerEvent{
	        public MyLocalJsonProvidedLeafEvent leaf;
	    }

	    [Serializable] public class MyLocalJsonProvidedLeafEvent{
	        public string id;
	    }
	}
} // end of namespace
