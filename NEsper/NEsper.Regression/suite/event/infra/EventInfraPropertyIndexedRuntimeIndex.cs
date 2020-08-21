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
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{

	public class EventInfraPropertyIndexedRuntimeIndex : RegressionExecution {
	    public void Run(RegressionEnvironment env) {
	        // Bean
	        BiConsumer<EventType, string[]> bean = (type, values) => {
	            env.SendEventBean(new LocalEvent(values));
	        };
	        var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
	        RunAssertion(env, beanepl, bean);

	        // Map
	        BiConsumer<EventType, string[]> map = (type, values) => {
	            env.SendEventMap(Collections.SingletonDataMap("indexed", values), "LocalEvent");
	        };
	        var mapepl = "@public @buseventtype create schema LocalEvent(indexed string[]);\n";
	        RunAssertion(env, mapepl, map);

	        // Object-array
	        BiConsumer<EventType, string[]> oa = (type, values) => {
	            env.SendEventObjectArray(new object[]{values}, "LocalEvent");
	        };
	        var oaepl = "@public @buseventtype create objectarray schema LocalEvent(indexed string[]);\n";
	        RunAssertion(env, oaepl, oa);

	        // Json
	        BiConsumer<EventType, string[]> json = (type, values) => {
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
	        var jsonProvidedEpl = "@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype create json schema LocalEvent();\n";
	        RunAssertion(env, jsonProvidedEpl, json);

	        // Avro
	        BiConsumer<EventType, string[]> avro = (type, ids) => {
	            var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
	            @event.Put("indexed", Arrays.AsList(ids));
	            env.SendEventAvro(@event, "LocalEvent");
	        };
	        var avroepl = "@public @buseventtype create avro schema LocalEvent(indexed string[]);\n";
	        RunAssertion(env, avroepl, avro);
	    }

	    public void RunAssertion(RegressionEnvironment env,
	                             string createSchemaEPL,
	                             BiConsumer<EventType, string[]> sender) {

	        env.CompileDeploy(createSchemaEPL +
	            "create constant variable int offsetNum = 0;" +
	            "@Name('s0') select indexed(offsetNum+0) as c0, indexed(offsetNum+1) as c1 from LocalEvent as e;\n"
	        ).AddListener("s0");
	        var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

	        sender.Invoke(eventType, new string[]{"a", "b"});
	        EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[]{"a", "b"});

	        env.UndeployAll();
	    }

	    public class LocalEvent {
		    public LocalEvent(string[] indexed) {
	            this.Indexed = indexed;
	        }

	        public string[] Indexed { get; }
	    }

	    [Serializable] public class MyLocalJsonProvided{
	        public string[] indexed;
	    }
	}
} // end of namespace
