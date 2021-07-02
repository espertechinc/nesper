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
	public class EventInfraGetterDynamicIndexed : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, NullableObject<string[]>> bean = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventBean(new LocalEvent());
				}
				else {
					env.SendEventBean(new LocalEventSubA(nullable.Value));
				}
			};
			var beanepl = 
				$"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n" +
				$"@public @buseventtype create schema LocalEventSubA as {typeof(LocalEventSubA).MaskTypeName()};\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, NullableObject<string[]>> map = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventMap(EmptyDictionary<string, object>.Instance, "LocalEvent");
				}
				else {
					env.SendEventMap(Collections.SingletonDataMap("Array", nullable.Value), "LocalEvent");
				}
			};
			var mapepl = "@public @buseventtype create schema LocalEvent();\n";
			RunAssertion(env, mapepl, map);

			// Object-Array
			var oaepl = 
				"@public @buseventtype create objectarray schema LocalEvent();\n" +
			    "@public @buseventtype create objectarray schema LocalEventSubA (Array string[]) inherits LocalEvent;\n";
			RunAssertion(env, oaepl, null);
			// Json
			BiConsumer<EventType, NullableObject<string[]>> json = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventJson("{}", "LocalEvent");
				}
				else if (nullable.Value == null) {
					env.SendEventJson(new JObject(new JProperty("Array", null)).ToString(), "LocalEvent");
				}
				else {
					var @event = new JObject();
					var array = new JArray();
					@event.Add("Array", array);
					foreach (var value in nullable.Value) {
						array.Add(value);
					}
					env.SendEventJson(@event.ToString(), "LocalEvent");
				}
			};
			RunAssertion(
				env,
				"@public @buseventtype @JsonSchema(Dynamic=true) create json schema LocalEvent();\n",
				json);

			// Json-Class-Provided
			RunAssertion(
				env,
				"@JsonSchema(ClassName='" + typeof(MyLocalJsonProvided).MaskTypeName() + "') @public @buseventtype @JsonSchema() create json schema LocalEvent();\n",
				json);

			// Avro
			BiConsumer<EventType, NullableObject<string[]>> avro = (
				type,
				nullable) => {
				var schema = SchemaBuilder.Record("name", TypeBuilder.Field("Array", TypeBuilder.Array(TypeBuilder.StringType())));
				GenericRecord @event;
				if (nullable == null) {
					// no action
					@event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				}
				else if (nullable.Value == null) {
					@event = new GenericRecord(schema);
				}
				else {
					@event = new GenericRecord(schema);
					@event.Put("Array", Arrays.AsList(nullable.Value));
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, "@public @buseventtype create avro schema LocalEvent();\n", avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, NullableObject<string[]>> sender)
		{
			var path = new RegressionPath();
			env.CompileDeploy(createSchemaEPL, path);

			env.CompileDeploy("@Name('s0') select * from LocalEvent", path).AddListener("s0");
			var eventType = env.Statement("s0").EventType;
			var g0 = eventType.GetGetter("Array[0]?");
			var g1 = eventType.GetGetter("Array[1]?");

			if (sender == null) {
				Assert.IsNull(g0);
				Assert.IsNull(g1);
				env.UndeployAll();
				return;
			}
			else {
				var propepl =
					"@Name('s1') select " +
					"Array[0]? as c0, " +
					"Array[1]? as c1," +
					"exists(Array[0]?) as c2, " +
					"exists(Array[1]?) as c3, " +
					"typeof(Array[0]?) as c4, " +
					"typeof(Array[1]?) as c5 " +
					"from LocalEvent;\n";
				env.CompileDeploy(propepl, path).AddListener("s1");
			}

			sender.Invoke(eventType, new NullableObject<string[]>(new string[] {"a", "b"}));
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertGetter(@event, g1, true, "b");
			AssertProps(env, "a", "b");

			sender.Invoke(eventType, new NullableObject<string[]>(new string[] {"a"}));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertGetter(@event, g1, false, null);
			AssertProps(env, "a", null);

			sender.Invoke(eventType, new NullableObject<string[]>(new string[0]));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, null, null);

			sender.Invoke(eventType, new NullableObject<string[]>(null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, null, null);

			sender.Invoke(eventType, null);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, null, null);

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
			string valueA,
			string valueB)
		{
			var @event = env.Listener("s1").AssertOneGetNewAndReset();
			Assert.AreEqual(valueA, @event.Get("c0"));
			Assert.AreEqual(valueB, @event.Get("c1"));
			Assert.AreEqual(valueA != null, @event.Get("c2"));
			Assert.AreEqual(valueB != null, @event.Get("c3"));
			Assert.AreEqual(valueA == null ? null : "String", @event.Get("c4"));
			Assert.AreEqual(valueB == null ? null : "String", @event.Get("c5"));
		}

		public class LocalEvent
		{
		}

		public class LocalEventSubA : LocalEvent
		{
			public LocalEventSubA(string[] Array)
			{
				this.Array = Array;
			}

			public string[] Array { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public string[] Array;
		}
	}
} // end of namespace
