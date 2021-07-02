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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{

	public class EventInfraGetterDynamicNested : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, NullableObject<string>> bean = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventBean(new LocalEvent());
				}
				else {
					env.SendEventBean(new LocalEventSubA(new LocalInnerEvent(nullable.Value)));
				}
			};
			var beanepl =
				$"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n" +
				$"@public @buseventtype create schema LocalEventSubA as {typeof(LocalEventSubA).MaskTypeName()};\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, NullableObject<string>> map = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventMap(EmptyDictionary<string, object>.Instance, "LocalEvent");
				}
				else {
					var inner = Collections.SingletonDataMap("Id", nullable.Value);
					env.SendEventMap(Collections.SingletonDataMap("Property", inner), "LocalEvent");
				}
			};
			RunAssertion(env, GetEPL("map"), map);

			// Object-array
			RunAssertion(env, GetEPL("objectarray"), null);

			// Json
			BiConsumer<EventType, NullableObject<string>> json = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventJson("{}", "LocalEvent");
				}
				else {
					var inner = new JObject(new JProperty("Id", nullable.Value));
					env.SendEventJson(new JObject(new JProperty("Property", inner)).ToString(), "LocalEvent");
				}
			};
			RunAssertion(env, GetEPL("json"), json);

			// Json-Class-Provided
			var jsonProvidedEPL = 
				$"@JsonSchema(ClassName='{typeof(MyLocalJsonProvided).CleanName()}') " +
				"@public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonProvidedEPL, json);

			// Avro
			BiConsumer<EventType, NullableObject<string>> avro = (
				type,
				nullable) => {
				GenericRecord @event;
				if (nullable == null) {
					@event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				}
				else {
					var innerSchema = SchemaBuilder.Record(
						"inner",
						TypeBuilder.Field(
							"Id",
							TypeBuilder.StringType(
								TypeBuilder.Property(
									AvroConstant.PROP_STRING_KEY,
									AvroConstant.PROP_STRING_VALUE))));
					var inner = new GenericRecord(innerSchema);
					inner.Put("Id", nullable.Value);
					var schema = SchemaBuilder.Record("name", TypeBuilder.Field("Property", innerSchema));
					@event = new GenericRecord(schema);
					@event.Put("Property", inner);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEPL("avro"), avro);
		}

		private string GetEPL(string underlying)
		{
			return "@public @buseventtype @JsonSchema(Dynamic=true) create " + underlying + " schema LocalEvent();\n";
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, NullableObject<string>> sender)
		{

			var path = new RegressionPath();
			env.CompileDeploy(createSchemaEPL, path);

			env.CompileDeploy("@Name('s0') select * from LocalEvent", path).AddListener("s0");
			var eventType = env.Statement("s0").EventType;
			var g0 = eventType.GetGetter("Property?.Id");

			if (sender == null) {
				Assert.IsNull(g0);
				env.UndeployAll();
				return;
			}
			else {
				var propepl = "@Name('s1') select Property?.Id as c0, exists(Property?.Id) as c1, typeof(Property?.Id) as c2 from LocalEvent;\n";
				env.CompileDeploy(propepl, path).AddListener("s1");
			}

			sender.Invoke(eventType, new NullableObject<string>("a"));
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertProps(env, true, "a");

			sender.Invoke(eventType, new NullableObject<string>(null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, null);
			AssertProps(env, true, null);

			sender.Invoke(eventType, null);
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

		public class LocalEvent
		{
		}

		public class LocalInnerEvent
		{
			public LocalInnerEvent(string id)
			{
				this.Id = id;
			}

			public string Id { get; }
		}

		public class LocalEventSubA : LocalEvent
		{
			public LocalEventSubA(LocalInnerEvent property)
			{
				this.Property = property;
			}

			public LocalInnerEvent Property { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonProvidedInnerEvent Property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			public string Id;
		}
	}
} // end of namespace
