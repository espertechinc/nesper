///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraContainedNestedArray : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, string[]> bean = (
				type,
				ids) => {
				var property = new LocalInnerEvent[ids.Length];
				for (var i = 0; i < ids.Length; i++) {
					property[i] = new LocalInnerEvent(new LocalLeafEvent(ids[i]));
				}

				env.SendEventBean(new LocalEvent(property));
			};
			var beanepl = "@public @buseventtype create schema LocalLeafEvent as " +
			              typeof(LocalLeafEvent).FullName +
			              ";\n" +
			              "@public @buseventtype create schema LocalInnerEvent as " +
			              typeof(LocalInnerEvent).FullName +
			              ";\n" +
			              "@public @buseventtype create schema LocalEvent as " +
			              typeof(LocalEvent).FullName +
			              ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, string[]> map = (
				type,
				ids) => {
				var property = new IDictionary<string, object>[ids.Length];
				for (var i = 0; i < ids.Length; i++) {
					property[i] = Collections.SingletonDataMap("leaf", Collections.SingletonDataMap("id", ids[i]));
				}

				env.SendEventMap(Collections.SingletonDataMap("property", property), "LocalEvent");
			};
			RunAssertion(env, GetEpl("map"), map);

			// Object-array
			BiConsumer<EventType, string[]> oa = (
				type,
				ids) => {
				var property = new object[ids.Length][];
				for (var i = 0; i < ids.Length; i++) {
					property[i] = new object[] {new object[] {ids[i]}};
				}

				env.SendEventObjectArray(new object[] {property}, "LocalEvent");
			};
			RunAssertion(env, GetEpl("objectarray"), oa);

			// Json
			BiConsumer<EventType, string[]> json = (
				type,
				ids) => {
				JArray property = new JArray();
				for (var i = 0; i < ids.Length; i++) {
					var inner = new JObject(new JProperty("leaf", new JObject(new JProperty("id", ids[i]))));
					property.Add(inner);
				}

				env.SendEventJson(new JObject(new JProperty("property", property)).ToString(), "LocalEvent");
			};
			RunAssertion(env, GetEpl("json"), json);

			// Json-Class-Provided
			var eplJsonProvided = "@JsonSchema(className='" +
			                      typeof(MyLocalJsonProvided).FullName +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, string[]> avro = (
				type,
				ids) => {
				var schema = SupportAvroUtil.GetAvroSchema(type);
				var property = new List<GenericRecord>();
				for (var i = 0; i < ids.Length; i++) {
					var leaf = new GenericRecord(
						schema
							.GetField("property")
							.Schema
							.AsArraySchema()
							.ItemSchema
							.AsRecordSchema()
							.GetField("leaf")
							.Schema
							.AsRecordSchema()
					);
					leaf.Put("id", ids[i]);
					var inner = new GenericRecord(
						schema
							.GetField("property")
							.Schema
							.AsArraySchema()
							.ItemSchema
							.AsRecordSchema());
					inner.Put("leaf", leaf);
					property.Add(inner);
				}

				var @event = new GenericRecord(schema.AsRecordSchema());
				@event.Put("property", property);
				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("avro"), avro);
		}

		private string GetEpl(string underlying)
		{
			return "create " +
			       underlying +
			       " schema LocalLeafEvent(id string);\n" +
			       "create " +
			       underlying +
			       " schema LocalInnerEvent(leaf LocalLeafEvent);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalEvent(property LocalInnerEvent[]);\n";
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, string[]> sender)
		{

			env.CompileDeploy(
					createSchemaEPL +
					"@Name('s0') select * from LocalEvent[property[0].leaf];\n" +
					"@Name('s1') select * from LocalEvent[property[1].leaf];\n")
				.AddListener("s0")
				.AddListener("s1");
			var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

			sender.Invoke(eventType, "a,b".SplitCsv());
			Assert.AreEqual("a", env.Listener("s0").AssertOneGetNewAndReset().Get("id"));
			Assert.AreEqual("b", env.Listener("s1").AssertOneGetNewAndReset().Get("id"));

			env.UndeployAll();
		}

		public class LocalLeafEvent
		{
			public LocalLeafEvent(string id)
			{
				this.Id = id;
			}

			public string Id { get; }
		}

		public class LocalInnerEvent
		{
			public LocalInnerEvent(LocalLeafEvent leaf)
			{
				this.Leaf = leaf;
			}

			public LocalLeafEvent Leaf { get; }
		}

		public class LocalEvent
		{
			public LocalEvent(LocalInnerEvent[] property)
			{
				this.Property = property;
			}

			public LocalInnerEvent[] Property { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonProvidedInnerEvent[] property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			public MyLocalJsonProvidedLeafEvent leaf;
		}

		[Serializable]
		public class MyLocalJsonProvidedLeafEvent
		{
			public string id;
		}
	}
} // end of namespace
