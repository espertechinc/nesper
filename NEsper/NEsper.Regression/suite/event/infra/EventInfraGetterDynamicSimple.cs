///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{

	public class EventInfraGetterDynamicSimple : RegressionExecution
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
					env.SendEventBean(new LocalEventSubA(nullable.Value));
				}
			};
			var beanepl = "@public @buseventtype create schema LocalEvent as " +
			              typeof(LocalEvent).FullName +
			              ";\n" +
			              "@public @buseventtype create schema LocalEventSubA as " +
			              typeof(LocalEventSubA).FullName +
			              ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, NullableObject<string>> map = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventMap(EmptyDictionary<string, object>.Instance, "LocalEvent");
				}
				else {
					env.SendEventMap(Collections.SingletonDataMap("property", nullable.Value), "LocalEvent");
				}
			};
			var mapepl = "@public @buseventtype create schema LocalEvent();\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			BiConsumer<EventType, NullableObject<string>> oa = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventObjectArray(new object[0], "LocalEvent");
				}
				else {
					env.SendEventObjectArray(new object[] {nullable.Value}, "LocalEventSubA");
				}
			};
			var oaepl = "@public @buseventtype create objectarray schema LocalEvent();\n" +
			            "@public @buseventtype create objectarray schema LocalEventSubA (property string) inherits LocalEvent;\n";
			RunAssertion(env, oaepl, oa);

			// Json
			BiConsumer<EventType, NullableObject<string>> json = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventJson("{}", "LocalEvent");
				}
				else if (nullable.Value == null) {
					env.SendEventJson(new JObject(new JProperty("property", null)).ToString(), "LocalEvent");
				}
				else {
					env.SendEventJson(new JObject(new JProperty("property", nullable.Value)).ToString(), "LocalEvent");
				}
			};
			RunAssertion(env, "@public @buseventtype @JsonSchema(dynamic=true) create json schema LocalEvent();\n", json);

			// Json-Class-Provided
			RunAssertion(
				env,
				"@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype create json schema LocalEvent();\n",
				json);

			// Avro
			BiConsumer<EventType, NullableObject<string>> avro = (
				type,
				nullable) => {
				
				var schema = SchemaBuilder.Record(
					"name",
					TypeBuilder.Field(
						"property",
						TypeBuilder.StringType(
							TypeBuilder.Property(
								AvroConstant.PROP_STRING_KEY,
								AvroConstant.PROP_STRING_VALUE))));

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
					@event.Put("property", nullable.Value);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, "@public @buseventtype create avro schema LocalEvent();\n", avro);
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
			var g0 = eventType.GetGetter("property?");

			if (sender == null) {
				Assert.IsNull(g0);
				env.UndeployAll();
				return;
			}
			else {
				var propepl = "@Name('s1') select property? as c0, exists(property?) as c1, typeof(property?) as c2 from LocalEvent;\n";
				env.CompileDeploy(propepl, path).AddListener("s1");
			}

			sender.Invoke(eventType, new NullableObject<string>("a"));
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "a");
			AssertProps(env, eventType, true, "a");

			sender.Invoke(eventType, new NullableObject<string>(null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, null);
			AssertProps(env, eventType, true, null);

			sender.Invoke(eventType, null);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertProps(env, eventType, false, null);

			env.UndeployAll();
		}

		private void AssertGetter(
			EventBean @event,
			EventPropertyGetter getter,
			bool exists,
			string value)
		{
			Assert.AreEqual(SupportJsonEventTypeUtil.IsBeanBackedJson(@event.EventType) || exists, getter.IsExistsProperty(@event));
			Assert.AreEqual(value, getter.Get(@event));
			Assert.IsNull(getter.GetFragment(@event));
		}

		private void AssertProps(
			RegressionEnvironment env,
			EventType eventType,
			bool exists,
			string value)
		{
			var @event = env.Listener("s1").AssertOneGetNewAndReset();
			Assert.AreEqual(value, @event.Get("c0"));
			Assert.AreEqual(SupportJsonEventTypeUtil.IsBeanBackedJson(eventType) || exists, @event.Get("c1"));
			Assert.AreEqual(value != null ? "String" : null, @event.Get("c2"));
		}

		public class LocalEvent
		{
		}

		public class LocalEventSubA : LocalEvent
		{
			public LocalEventSubA(string property)
			{
				this.Property = property;
			}

			public string Property { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public string property;
		}
	}
} // end of namespace
