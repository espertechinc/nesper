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
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
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

	public class EventInfraGetterNestedSimpleDeep : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, Nullable2Lvl> bean = (
				type,
				val) => {
				LocalEvent @event;
				if (val.IsNullAtRoot) {
					@event = new LocalEvent(null);
				}
				else if (val.IsNullAtInner) {
					@event = new LocalEvent(new LocalInnerEvent(null));
				}
				else {
					@event = new LocalEvent(new LocalInnerEvent(new LocalLeafEvent(val.Id)));
				}

				env.SendEventBean(@event);
			};
			var beanepl = $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n";
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
			BiConsumer<EventType, Nullable2Lvl> oa = (
				type,
				val) => {
				var @event = new object[1];
				if (val.IsNullAtRoot) {
					// no change
				}
				else if (val.IsNullAtInner) {
					var inner = new object[] {null};
					@event[0] = inner;
				}
				else {
					var leaf = new object[] {val.Id};
					var inner = new object[] {leaf};
					@event[0] = inner;
				}

				env.SendEventObjectArray(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("objectarray"), oa);

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
			var eplJsonProvided =
				$"@JsonSchema(ClassName='{typeof(MyLocalJsonProvided).MaskTypeName()}') " +
			    "@public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, Nullable2Lvl> avro = (
				type,
				val) => {
				var schema = SupportAvroUtil.GetAvroSchema(type);
				var @event = new GenericRecord(schema.AsRecordSchema());
				if (val.IsNullAtRoot) {
					// no change
				}
				else if (val.IsNullAtInner) {
					var inner = new GenericRecord(schema
						.GetField("Property")
						.Schema.AsRecordSchema());
					@event.Put("Property", inner);
				}
				else {
					var leaf = new GenericRecord(schema
						.GetField("Property")
						.Schema.AsRecordSchema()
						.GetField("Leaf")
						.Schema.AsRecordSchema());
					leaf.Put("Id", val.Id);
					var inner = new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema());
					inner.Put("Leaf", leaf);
					@event.Put("Property", inner);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("avro"), avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, Nullable2Lvl> sender)
		{

			var epl = createSchemaEPL +
			          "@Name('s0') select * from LocalEvent;\n" +
			          "@Name('s1') select Property.Leaf.Id as c0, exists(Property.Leaf.Id) as c1, typeof(Property.Leaf.Id) as c2 from LocalEvent;\n";
			env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
			var eventType = env.Statement("s0").EventType;

			var g0 = eventType.GetGetter("Property.Leaf.Id");

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

		private void AssertProps(
			RegressionEnvironment env,
			bool exists,
			string expected)
		{
			EPAssertionUtil.AssertProps(
				env.Listener("s1").AssertOneGetNewAndReset(),
				"c0,c1,c2".SplitCsv(),
				new object[] {expected, exists, expected != null ? nameof(String) : null});
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

		private string GetEpl(string underlying)
		{
			return "@public @buseventtype create " +
			       underlying +
			       " schema LocalLeafEvent(Id string);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalInnerEvent(Leaf LocalLeafEvent);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalEvent(Property LocalInnerEvent);\n";
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
			public LocalEvent(LocalInnerEvent property)
			{
				this.Property = property;
			}

			public LocalInnerEvent Property { get; }
		}

		public class Nullable2Lvl
		{
			public Nullable2Lvl(
				bool nullAtRoot,
				bool nullAtInner,
				string id)
			{
				this.IsNullAtRoot = nullAtRoot;
				this.IsNullAtInner = nullAtInner;
				this.Id = id;
			}

			public bool IsNullAtRoot { get; }

			public bool IsNullAtInner { get; }

			public string Id { get; }
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
