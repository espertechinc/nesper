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
	public class EventInfraGetterMapped : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
#if false
			// Bean
			BiConsumer<EventType, IDictionary<string, string>> bean = (
				type,
				entries) => {
				env.SendEventBean(new LocalEvent(entries));
			};
			var beanepl = $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()}";
			RunAssertion(env, beanepl, bean);
#endif
			var properties = typeof(IDictionary<string, string>).CleanName();
			
			// Map
			BiConsumer<EventType, IDictionary<string, string>> map = (
				type,
				entries) => {
				env.SendEventMap(Collections.SingletonDataMap("Mapped", entries), "LocalEvent");
			};
			var mapepl = $"@public @buseventtype create schema LocalEvent(Mapped `{properties}`);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			BiConsumer<EventType, IDictionary<string, string>> oa = (
				type,
				entries) => {
				env.SendEventObjectArray(new object[] {entries}, "LocalEvent");
			};
			var oaepl = $"@public @buseventtype create objectarray schema LocalEvent(Mapped `{properties}`);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			BiConsumer<EventType, IDictionary<string, string>> json = (
				type,
				entries) => {
				if (entries == null) {
					env.SendEventJson(new JObject(new JProperty("Mapped", null)).ToString(), "LocalEvent");
				}
				else {
					var @event = new JObject();
					var mapped = new JObject();
					@event.Add("Mapped", mapped);
					foreach (var entry in entries) {
						mapped.Add(entry.Key, entry.Value);
					}

					env.SendEventJson(@event.ToString(), "LocalEvent");
				}
			};
			RunAssertion(env, $"@public @buseventtype @JsonSchema(Dynamic=true) create json schema LocalEvent(Mapped `{properties}`);\n", json);

			// Json-Class-Provided
			RunAssertion(
				env,
				"@JsonSchema(ClassName='" + typeof(MyLocalJsonProvided).MaskTypeName() + "') @public @buseventtype create json schema LocalEvent();\n",
				json);

			// Avro
			BiConsumer<EventType, IDictionary<string, string>> avro = (
				type,
				entries) => {
				var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				@event.Put("Mapped", entries);
				env.SendEventAvro(@event, "LocalEvent");
			};
			RunAssertion(env, $"@public @buseventtype create avro schema LocalEvent(Mapped `{properties}`);\n", avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, IDictionary<string, string>> sender)
		{
			var path = new RegressionPath();
			env.CompileDeploy(createSchemaEPL, path);

			env.CompileDeploy("@Name('s0') select * from LocalEvent", path).AddListener("s0");
			var eventType = env.Statement("s0").EventType;
			var g0 = eventType.GetGetter("Mapped('a')");
			var g1 = eventType.GetGetter("Mapped('b')");

			var propepl =
				"@Name('s1') select " +
				"Mapped('a') as c0, " +
				"Mapped('b') as c1, " +
			    "exists(Mapped('a')) as c2, " +
				"exists(Mapped('b')) as c3, " +
			    "typeof(Mapped('a')) as c4, " +
				"typeof(Mapped('b')) as c5 from LocalEvent;\n";
			env.CompileDeploy(propepl, path).AddListener("s1");

			IDictionary<string, string> values = new Dictionary<string, string>();
			values.Put("a", "x");
			values.Put("b", "y");
			sender.Invoke(eventType, values);
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "x");
			AssertGetter(@event, g1, true, "y");
			AssertProps(env, "x", "y");

			sender.Invoke(eventType, Collections.SingletonMap("a", "x"));
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true, "x");
			AssertGetter(@event, g1, false, null);
			AssertProps(env, "x", null);

			sender.Invoke(eventType, EmptyDictionary<string, string>.Instance);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false, null);
			AssertGetter(@event, g1, false, null);
			AssertProps(env, null, null);

			sender.Invoke(eventType, null);
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
			public LocalEvent(IDictionary<string, string> mapped)
			{
				this.Mapped = mapped;
			}

			public IDictionary<string, string> Mapped { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedMember.Global
			public IDictionary<string, string> Mapped;
		}
	}
} // end of namespace
