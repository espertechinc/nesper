///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraGetterDynamicIndexexPropertyPredefined : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, NullableObject<int?>> bean = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventBean(new LocalEvent());
				}
				else if (nullable.Value == null) {
					env.SendEventBean(new LocalEventSubA(null));
				}
				else {
					var array = new LocalInnerEvent[nullable.Value.Value];
					for (var i = 0; i < array.Length; i++) {
						array[i] = new LocalInnerEvent();
					}

					env.SendEventBean(new LocalEventSubA(array));
				}
			};
			var beanepl =
				"@public @buseventtype create schema LocalInnerEvent as " +
				typeof(LocalInnerEvent).FullName +
				";\n" +
				"@public @buseventtype create schema LocalEvent as " +
				typeof(LocalEvent).FullName +
				";\n" +
				"@public @buseventtype create schema LocalEventSubA as " +
				typeof(LocalEventSubA).FullName +
				";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, NullableObject<int?>> map = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventMap(EmptyDictionary<string, object>.Instance, "LocalEvent");
				}
				else if (nullable.Value == null) {
					env.SendEventMap(Collections.SingletonDataMap("array", null), "LocalEvent");
				}
				else {
					var array = new IDictionary<string, object>[nullable.Value.Value];
					for (var i = 0; i < array.Length; i++) {
						array[i] = new Dictionary<string, object>();
					}

					env.SendEventMap(Collections.SingletonDataMap("array", array), "LocalEvent");
				}
			};
			var mapepl =
				"@public @buseventtype create schema LocalInnerEvent();\n" +
				"@public @buseventtype create schema LocalEvent(array LocalInnerEvent[]);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			var oaepl = "@public @buseventtype create objectarray schema LocalEvent();\n" +
			            "@public @buseventtype create objectarray schema LocalEventSubA (array string[]) inherits LocalEvent;\n";
			RunAssertion(env, oaepl, null);

			// Json
			BiConsumer<EventType, NullableObject<int?>> json = (
				type,
				nullable) => {
				if (nullable == null) {
					env.SendEventJson("{}", "LocalEvent");
				}
				else if (nullable.Value == null) {
					env.SendEventJson(new JObject(new JProperty("array", null)).ToString(), "LocalEvent");
				}
				else {
					var @event = new JObject();
					var array = new JArray();
					@event.Add("array", array);
					for (var i = 0; i < nullable.Value; i++) {
						array.Add(new JObject());
					}

					env.SendEventJson(@event.ToString(), "LocalEvent");
				}
			};
			var epl = "@public @buseventtype create json schema LocalInnerEvent();\n" +
			          "@public @buseventtype create json schema LocalEvent(array LocalInnerEvent[]);\n";
			RunAssertion(env, epl, json);

			// Json-Class-Provided
			var eplJsonProvided = "@JsonSchema(className='" +
			                      typeof(MyLocalJsonProvided).FullName +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, eplJsonProvided, json);

			// Avro
			BiConsumer<EventType, NullableObject<int?>> avro = (
				type,
				nullable) => {
				var inner = SchemaBuilder.Record("name");
				var schema = SchemaBuilder.Record("name", TypeBuilder.Field("array", TypeBuilder.Array(inner)));
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
					ICollection<GenericRecord> inners = new List<GenericRecord>();
					for (var i = 0; i < nullable.Value; i++) {
						inners.Add(new GenericRecord(inner));
					}

					@event.Put("array", inners);
				}

				env.SendEventAvro(@event, "LocalEvent");
			};
			var avroepl = "@public @buseventtype create avro schema LocalInnerEvent();\n" +
			              "@public @buseventtype create avro schema LocalEvent(array LocalInnerEvent[]);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, NullableObject<int?>> sender)
		{

			var path = new RegressionPath();
			env.CompileDeploy(createSchemaEPL, path);

			env.CompileDeploy("@name('s0') select * from LocalEvent", path).AddListener("s0");
			var eventType = env.Statement("s0").EventType;
			var g0 = eventType.GetGetter("array[0]?");
			var g1 = eventType.GetGetter("array[1]?");

			if (sender == null) {
				Assert.IsNull(g0);
				Assert.IsNull(g1);
				env.UndeployAll();
				return;
			}
			else {
				var propepl = "@name('s1') select array[0]? as c0, array[1]? as c1," +
				              "exists(array[0]?) as c2, exists(array[1]?) as c3, " +
				              "typeof(array[0]?) as c4, typeof(array[1]?) as c5 from LocalEvent;\n";
				env.CompileDeploy(propepl, path).AddListener("s1");
			}

			sender.Invoke(eventType, new NullableObject<int?>(2));
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true);
			AssertGetter(@event, g1, true);
			AssertProps(env, true, true);

			sender.Invoke(eventType, new NullableObject<int?>(1));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true);
			AssertGetter(@event, g1, false);
			AssertProps(env, true, false);

			sender.Invoke(eventType, new NullableObject<int?>(0));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false);
			AssertGetter(@event, g1, false);
			AssertProps(env, false, false);

			sender.Invoke(eventType, new NullableObject<int?>(null));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false);
			AssertGetter(@event, g1, false);
			AssertProps(env, false, false);

			sender.Invoke(eventType, null);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false);
			AssertGetter(@event, g1, false);
			AssertProps(env, false, false);

			env.UndeployAll();
		}

		private void AssertGetter(
			EventBean @event,
			EventPropertyGetter getter,
			bool exists)
		{
			Assert.AreEqual(exists, getter.IsExistsProperty(@event));
			Assert.AreEqual(exists, getter.Get(@event) != null);
			var beanBacked = @event.EventType is BeanEventType || SupportJsonEventTypeUtil.IsBeanBackedJson(@event.EventType);
			Assert.AreEqual(beanBacked && exists, getter.GetFragment(@event) != null);
		}

		private void AssertProps(
			RegressionEnvironment env,
			bool hasA,
			bool hasB)
		{
			var @event = env.Listener("s1").AssertOneGetNewAndReset();
			Assert.AreEqual(hasA, @event.Get("c0") != null);
			Assert.AreEqual(hasB, @event.Get("c1") != null);
			Assert.AreEqual(hasA, @event.Get("c2"));
			Assert.AreEqual(hasB, @event.Get("c3"));
			Assert.AreEqual(hasA, @event.Get("c4") != null);
			Assert.AreEqual(hasB, @event.Get("c5") != null);
		}

		public class LocalInnerEvent
		{
		}

		public class LocalEvent
		{
		}

		public class LocalEventSubA : LocalEvent
		{
			public LocalEventSubA(LocalInnerEvent[] array)
			{
				this.Array = array;
			}

			public LocalInnerEvent[] Array { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonProvidedInnerEvent[] array;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
		}
	}
} // end of namespace
