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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraGetterDynamicNestedDeep : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, Nullable2Lvl> bean = (
				type,
				val) => {
				LocalEvent @event;
				if (val.IsNullAtRoot) {
					@event = new LocalEvent();
				}
				else if (val.IsNullAtInner) {
					@event = new LocalEventSubA(new LocalInnerEvent(null));
				}
				else {
					@event = new LocalEventSubA(new LocalInnerEvent(new LocalLeafEvent(val.Id)));
				}

				env.SendEventBean(@event, "LocalEvent");
			};
			var beanepl = "@public @buseventtype create schema LocalEvent as " +
			              typeof(EventInfraGetterDynamicNested.LocalEvent).MaskTypeName() +
			              ";\n" +
			              "@public @buseventtype create schema LocalEventSubA as " +
			              typeof(EventInfraGetterDynamicNested.LocalEventSubA).MaskTypeName() +
			              ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, Nullable2Lvl> map = (
				type,
				val) => {
				IDictionary<string, object> @event = new LinkedHashMap<string, object>();
				if (val.IsNullAtRoot) {
					// no change
				}
				else if (val.IsNullAtInner) {
					var inner = Collections.SingletonDataMap("Leaf", null);
					@event.Put("Property", inner);
				}
				else {
					var leaf = Collections.SingletonDataMap("Id", val.Id);
					var inner = Collections.SingletonDataMap("Leaf", leaf);
					@event.Put("Property", inner);
				}

				env.SendEventMap(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("map"), map);

			// Object-array
			RunAssertion(env, GetEpl("objectarray"), null);

			// Json
			BiConsumer<EventType, Nullable2Lvl> json = (
				type,
				val) => {
				var @event = new JObject();
				if (val.IsNullAtRoot) {
					// no change
				}
				else if (val.IsNullAtInner) {
					@event.Add("Property", new JObject(new JProperty("Leaf", null)));
				}
				else {
					var leaf = new JObject(new JProperty("Id", val.Id));
					var inner = new JObject(new JProperty("Leaf", leaf));
					@event.Add("Property", inner);
				}

				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			RunAssertion(env, GetEpl("json"), json);

			// Json-Class-Provided
			var eplJsonProvided = "@JsonSchema(ClassName='" +
			                      typeof(MyLocalJsonProvided).MaskTypeName() +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, Nullable2Lvl> avro = (
				type,
				val) => {
				var emptySchema = SchemaBuilder.Record("empty");
				GenericRecord @event;
				if (val.IsNullAtRoot) {
					@event = new GenericRecord(emptySchema);
				}
				else if (val.IsNullAtInner) {
					var inner = new GenericRecord(emptySchema);
					var topSchema = SchemaBuilder.Record("name", TypeBuilder.Field("Property", emptySchema));
					@event = new GenericRecord(topSchema);
					@event.Put("Property", inner);
				}
				else {
					var leafSchema = SchemaBuilder.Record(
						"leaf",
						TypeBuilder.Field(
							"Id",
							TypeBuilder.StringType(
								TypeBuilder.Property(
									AvroConstant.PROP_STRING_KEY,
									AvroConstant.PROP_STRING_VALUE))));

					var innerSchema = SchemaBuilder.Record("inner", TypeBuilder.Field("Leaf", leafSchema));
					var topSchema = SchemaBuilder.Record("top", TypeBuilder.Field("Property", innerSchema));
					var leaf = new GenericRecord(leafSchema);
					leaf.Put("Id", val.Id);
					var inner = new GenericRecord(innerSchema);
					inner.Put("Leaf", leaf);
					@event = new GenericRecord(topSchema);
					@event.Put("Property", inner);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("avro"), avro);
		}

		private string GetEpl(string underlying)
		{
			return "@public @buseventtype @JsonSchema(Dynamic=true) create " + underlying + " schema LocalEvent();\n";
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, Nullable2Lvl> sender)
		{

			var path = new RegressionPath();
			env.CompileDeploy(createSchemaEPL, path);

			env.CompileDeploy("@Name('s0') select * from LocalEvent", path).AddListener("s0");
			var eventType = env.Statement("s0").EventType;
			var g0 = eventType.GetGetter("Property?.Leaf.Id");

			if (sender == null) {
				Assert.IsNull(g0);
				env.UndeployAll();
				return;
			}
			else {
				var propepl = "@Name('s1') select Property?.Leaf.Id as c0, exists(Property?.Leaf.Id) as c1, typeof(Property?.Leaf.Id) as c2 from LocalEvent;\n";
				env.CompileDeploy(propepl, path).AddListener("s1");
			}

			sender.Invoke(eventType, new Nullable2Lvl(false, false, "a"));
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertProps(env, true, "a");

			sender.Invoke(eventType, new Nullable2Lvl(false, false, null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, null);
			AssertProps(env, true, null);

			sender.Invoke(eventType, new Nullable2Lvl(false, true, null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertProps(env, false, null);

			sender.Invoke(eventType, new Nullable2Lvl(true, false, null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertProps(env, false, null);

			env.UndeployAll();
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
			bool exists,
			string value)
		{
			var @event = env.Listener("s1").AssertOneGetNewAndReset();
			Assert.AreEqual(value, @event.Get("c0"));
			Assert.AreEqual(exists, @event.Get("c1"));
			Assert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
		}

		public class LocalLeafEvent
		{
			public LocalLeafEvent(string id)
			{
				Id = id;
			}

			public string Id { get; }
		}

		public class LocalInnerEvent
		{
			public LocalInnerEvent(LocalLeafEvent leaf)
			{
				Leaf = leaf;
			}

			public LocalLeafEvent Leaf { get; }
		}

		public class LocalEvent
		{

		}

		public class LocalEventSubA : LocalEvent
		{
			public LocalEventSubA(LocalInnerEvent property)
			{
				Property = property;
			}

			public LocalInnerEvent Property { get; }
		}

		private class Nullable2Lvl
		{
			public Nullable2Lvl(
				bool nullAtRoot,
				bool nullAtInner,
				string id)
			{
				IsNullAtRoot = nullAtRoot;
				IsNullAtInner = nullAtInner;
				Id = id;
			}

			public bool IsNullAtRoot { get; }

			public bool IsNullAtInner { get; }

			public string Id { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public EventInfraGetterNestedSimpleDeep.MyLocalJsonProvidedInnerEvent Property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			public EventInfraGetterNestedSimpleDeep.MyLocalJsonProvidedLeafEvent Leaf;
		}

		[Serializable]
		public class MyLocalJsonProvidedLeafEvent
		{
			public string Id;
		}
	}
} // end of namespace
