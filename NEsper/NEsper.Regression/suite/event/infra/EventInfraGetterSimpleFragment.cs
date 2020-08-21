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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraGetterSimpleFragment : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, Boolean> bean = (
				type,
				hasValue) => {
				env.SendEventBean(new LocalEvent(hasValue ? new LocalInnerEvent() : null));
			};
			var beanepl = "@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, Boolean> map = (
				type,
				hasValue) => {
				var propertyValue = hasValue ? EmptyDictionary<string, object>.Instance : null;
				env.SendEventMap(Collections.SingletonDataMap("property", propertyValue), "LocalEvent");
			};
			var mapepl = "@public @buseventtype create schema LocalInnerEvent();\n" +
			             "@public @buseventtype create schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			BiConsumer<EventType, Boolean> oa = (
				type,
				hasValue) => {
				env.SendEventObjectArray(new object[] {hasValue ? new object[0] : null}, "LocalEvent");
			};
			var oaepl = "@public @buseventtype create objectarray schema LocalInnerEvent();\n" +
			            "@public @buseventtype create objectarray schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			BiConsumer<EventType, Boolean> json = (
				type,
				hasValue) => {
				var property = new JProperty("property", hasValue ? new JObject() : null);
				env.SendEventJson(new JObject(property).ToString(), "LocalEvent");
			};
			var jsonepl = "@public @buseventtype create json schema LocalInnerEvent();\n" +
			              "@public @buseventtype create json schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, jsonepl, json);

			// Json-Class-Provided
			var jsonprovidedepl = "@JsonSchema(className='" +
			                      typeof(MyLocalJsonProvided).FullName +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonprovidedepl, json);

			// Avro
			BiConsumer<EventType, Boolean> avro = (
				type,
				hasValue) => {
				var schema = SupportAvroUtil.GetAvroSchema(type).AsRecordSchema();
				var theEvent = new GenericRecord(schema);
				theEvent.Put("property", hasValue ? new GenericRecord(schema.GetField("property").Schema.AsRecordSchema()) : null);
				env.SendEventAvro(theEvent, type.Name);
			};
			var avroepl = "@public @buseventtype create avro schema LocalInnerEvent();\n" +
			              "@public @buseventtype create avro schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, Boolean> sender)
		{

			var epl = createSchemaEPL +
			          "@Name('s0') select * from LocalEvent;\n" +
			          "@Name('s1') select property as c0, exists(property) as c1, typeof(property) as c2 from LocalEvent;\n";
			env.CompileDeploy(epl).AddListener("s0").AddListener("s1");
			var eventType = env.Statement("s0").EventType;

			var g0 = eventType.GetGetter("property");

			sender.Invoke(eventType, true);
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, true);
			AssertProps(env, true);

			sender.Invoke(eventType, false);
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertGetter(@event, g0, false);
			AssertProps(env, false);

			env.UndeployAll();
		}

		private void AssertGetter(
			EventBean @event,
			EventPropertyGetter getter,
			bool hasValue)
		{
			Assert.IsTrue(getter.IsExistsProperty(@event));
			Assert.AreEqual(hasValue, getter.Get(@event) != null);
			Assert.AreEqual(hasValue, getter.GetFragment(@event) != null);
		}

		private void AssertProps(
			RegressionEnvironment env,
			bool hasValue)
		{
			var @event = env.Listener("s1").AssertOneGetNewAndReset();
			Assert.IsTrue((Boolean) @event.Get("c1"));
			Assert.AreEqual(hasValue, @event.Get("c0") != null);
			Assert.AreEqual(hasValue, @event.Get("c2") != null);
		}

		public class LocalInnerEvent
		{
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
			public MyLocalJsonProvidedInnerEvent property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
		}
	}
} // end of namespace
