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
	public class EventInfraGetterNestedSimple : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, NullableObject<string>> bean = (
				type,
				nullable) => {
				var property = nullable == null ? null : new LocalInnerEvent(nullable.Value);
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
			BiConsumer<EventType, NullableObject<string>> map = (
				type,
				nullable) => {
				IDictionary<string, object> property = nullable == null ? null : Collections.SingletonDataMap("Id", nullable.Value);
				env.SendEventMap(Collections.SingletonDataMap("Property", property), "LocalEvent");
			};
			RunAssertion(env, GetEpl("map"), map);

			// Object-array
			BiConsumer<EventType, NullableObject<string>> oa = (
				type,
				nullable) => {
				var property = nullable == null ? null : new object[] {nullable.Value};
				env.SendEventObjectArray(new object[] {property}, "LocalEvent");
			};
			RunAssertion(env, GetEpl("objectarray"), oa);

			// Json
			BiConsumer<EventType, NullableObject<string>> json = (
				type,
				nullable) => {
				var @event = new JObject();
				if (nullable != null) {
					if (nullable.Value != null) {
						@event.Add("Property", new JObject(new JProperty("Id", nullable.Value)));
					}
					else {
						@event.Add("Property", new JObject(new JProperty("Id", null)));
					}
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
			BiConsumer<EventType, NullableObject<string>> avro = (
				type,
				nullable) => {
				var schema = SupportAvroUtil.GetAvroSchema(type).AsRecordSchema();
				var @event = new GenericRecord(schema);
				if (nullable != null) {
					var inside = new GenericRecord(schema.GetField("Property").Schema.AsRecordSchema());
					inside.Put("Id", nullable.Value);
					@event.Put("Property", inside);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, GetEpl("avro"), avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, NullableObject<string>> sender)
		{

			var epl = createSchemaEPL +
			          "@Name('s0') select * from LocalEvent;\n" +
			          "@Name('s1') select Property.Id as c0, exists(Property.Id) as c1, typeof(Property.Id) as c2 from LocalEvent;\n";
			env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
			var eventType = env.Statement("s0").EventType;

			var g0 = eventType.GetGetter("Property.Id");

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
			       " schema LocalInnerEvent(Id string);\n" +
			       "@public @buseventtype create " +
			       underlying +
			       " schema LocalEvent(Property LocalInnerEvent);\n";
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
			public LocalEvent(LocalInnerEvent property)
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
