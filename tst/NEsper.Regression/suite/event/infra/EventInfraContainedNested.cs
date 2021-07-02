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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
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
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, string> bean = (
				type,
				id) => {
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
			BiConsumer<EventType, string> map = (
				type,
				id) => {
				var leaf = Collections.SingletonDataMap("Id", id);
				var inner = Collections.SingletonDataMap("Leaf", leaf);
				env.SendEventMap(Collections.SingletonDataMap("Property", inner), "LocalEvent");
			};
			RunAssertion(env, GetEpl("map"), map);

			// Object-array
			BiConsumer<EventType, string> oa = (
				type,
				id) => {
				var leaf = new object[] {id};
				var inner = new object[] {leaf};
				env.SendEventObjectArray(new object[] {inner}, "LocalEvent");
			};
			RunAssertion(env, GetEpl("objectarray"), oa);

			// Json
			BiConsumer<EventType, string> json = (
				type,
				id) => {
				var leaf = new JObject(new JProperty("Id", id));
				var inner = new JObject(new JProperty("Leaf", leaf));
				var @event = new JObject(new JProperty("Property", inner));
				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			RunAssertion(env, GetEpl("json"), json);

			// Json-Class-Provided
			var eplJsonProvided = "@JsonSchema(ClassName='" +
			                      typeof(MyLocalJsonProvided).MaskTypeName() +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, string> avro = (
				type,
				id) => {
				var schema = SupportAvroUtil.GetAvroSchema(type).AsRecordSchema();
				var leaf = new GenericRecord(
					schema.GetField("Property")
						.Schema.GetField("Leaf")
						.Schema.AsRecordSchema());
				leaf.Put("Id", id);
				var inner = new GenericRecord(
					schema.GetField("Property")
						.Schema.AsRecordSchema());
				inner.Put("Leaf", leaf);
				var @event = new GenericRecord(schema);
				@event.Put("Property", inner);
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
			       " schema LocalInnerEvent(Leaf LocalLeafEvent);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalEvent(Property LocalInnerEvent);\n";
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, string> sender)
		{

			env.CompileDeploy(createSchemaEPL + "@Name('s0') select * from LocalEvent[Property.Leaf];\n").AddListener("s0");
			var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

			sender.Invoke(eventType, "a");
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			Assert.AreEqual("a", @event.Get("Id"));

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

			public LocalInnerEvent Property => property;
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonProvidedInnerEvent Property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			public MyLocalJsonProvidedLeafEvent Leaf;
		}

		[Serializable]
		public class MyLocalJsonProvidedLeafEvent
		{
			public string Id;
		}
	}
} // end of namespace
