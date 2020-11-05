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
	public class EventInfraGetterNestedArray : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, string[]> bean = (
				type,
				array) => {
				LocalInnerEvent[] property;
				if (array == null) {
					property = null;
				}
				else {
					property = new LocalInnerEvent[array.Length];
					for (var i = 0; i < array.Length; i++) {
						property[i] = new LocalInnerEvent(array[i]);
					}
				}

				env.SendEventBean(new LocalEvent(property));
			};
			var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
			              typeof(LocalInnerEvent).MaskTypeName() +
			              ";\n" +
			              "@public @buseventtype create schema LocalEvent as " +
			              typeof(LocalEvent).MaskTypeName() +
			              ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, string[]> map = (
				type,
				array) => {
				IDictionary<string, object>[] property;
				if (array == null) {
					property = null;
				}
				else {
					property = new IDictionary<string, object>[array.Length];
					for (var i = 0; i < array.Length; i++) {
						property[i] = Collections.SingletonDataMap("Id", array[i]);
					}
				}

				env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
			};
			RunAssertion(env, GetEpl("map"), map);

			// Object-array
			BiConsumer<EventType, string[]> oa = (
				type,
				array) => {
				object[][] property;
				if (array == null) {
					property = new object[][] {null};
				}
				else {
					property = new object[array.Length][];
					for (var i = 0; i < array.Length; i++) {
						property[i] = new object[] {array[i]};
					}
				}

				env.SendEventObjectArray(new object[] {property}, "LocalEvent");
			};
			RunAssertion(env, GetEpl("objectarray"), oa);

			// Json
			BiConsumer<EventType, string[]> json = (
				type,
				array) => {
				JToken property;
				if (array == null) {
					property = new JValue((object) null);
				}
				else {
					var arr = new JArray();
					for (var i = 0; i < array.Length; i++) {
						arr.Add(new JObject(new JProperty("Id", array[i])));
					}

					property = arr;
				}

				env.SendEventJson(new JObject(new JProperty("Property", property)).ToString(), "LocalEvent");
			};
			RunAssertion(env, GetEpl("json"), json);

			// Json-Class-Provided
			var eplJsonProvided = "@JsonSchema(ClassName='" +
			                      typeof(MyLocalJsonProvided).MaskTypeName() +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, string[]> avro = (
				type,
				array) => {
				var schema = SupportAvroUtil.GetAvroSchema(type);
				var @event = new GenericRecord(schema.AsRecordSchema());
				if (array == null) {
					@event.Put("Property", null);
				}
				else {
					ICollection<GenericRecord> arr = new List<GenericRecord>();
					for (var i = 0; i < array.Length; i++) {
						var inner = new GenericRecord(schema.GetField("Property")
							.Schema.AsArraySchema()
							.ItemSchema
							.AsRecordSchema());
						inner.Put("Id", array[i]);
						arr.Add(inner);
					}

					@event.Put("Property", arr);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("avro"), avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, string[]> sender)
		{

			var epl = createSchemaEPL +
			          "@Name('s0') select * from LocalEvent;\n" +
			          "@Name('s1') select Property[0].Id as c0, Property[1].Id as c1," +
			          " exists(Property[0].Id) as c2, exists(Property[1].Id) as c3," +
			          " typeof(Property[0].Id) as c4, typeof(Property[1].Id) as c5" +
			          " from LocalEvent;\n";
			env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
			var eventType = env.Statement("s0").EventType;

			var g0 = eventType.GetGetter("Property[0].Id");
			var g1 = eventType.GetGetter("Property[1].Id");

			sender.Invoke(eventType, new string[] {"a", "b"});
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertGetter(@event, g1, true, "b");
			AssertProps(env, true, "a", true, "b");

			sender.Invoke(eventType, new string[] {"a"});
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertGetter(@event, g1, false, null);
			AssertProps(env, true, "a", false, null);

			sender.Invoke(eventType, new string[0]);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, false, null, false, null);

			sender.Invoke(eventType, null);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, false, null, false, null);

			env.UndeployAll();
		}

		private void AssertProps(
			RegressionEnvironment env,
			bool existsA,
			string expectedA,
			bool existsB,
			string expectedB)
		{
			EPAssertionUtil.AssertProps(
				env.Listener("s1").AssertOneGetNewAndReset(),
				"c0,c1,c2,c3,c4,c5".SplitCsv(),
				new object[] {expectedA, expectedB, existsA, existsB, existsA ? nameof(String) : null, existsB ? nameof(String) : null});
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
			       " schema LocalInnerEvent(Id string);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalEvent(Property LocalInnerEvent[]);\n";
		}

		public class LocalInnerEvent
		{
			public LocalInnerEvent(string id)
			{
				this.Id = id;
			}

			public string Id { get; }
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
			public MyLocalJsonProvidedInner[] Property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInner
		{
			public string Id;
		}
	}
} // end of namespace
