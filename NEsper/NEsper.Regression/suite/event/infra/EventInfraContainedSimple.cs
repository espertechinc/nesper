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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraContainedSimple : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, string> bean = (
				type,
				id) => {
				env.SendEventBean(new LocalEvent(new LocalInnerEvent(id)));
			};
			var beanepl =
				"@public @buseventtype create schema LocalInnerEvent as " + typeof(LocalInnerEvent).MaskTypeName() + ";\n" +
				"@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).MaskTypeName() + ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, string> map = (
				type,
				id) => {
				env.SendEventMap(
					Collections.SingletonDataMap(
						"property",
						Collections.SingletonDataMap("id", id)),
					"LocalEvent");
			};
			var mapepl =
				"@public @buseventtype create schema LocalInnerEvent(id string);\n" +
			             "@public @buseventtype create schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			BiConsumer<EventType, string> oa = (
				type,
				id) => {
				env.SendEventObjectArray(new object[] {new object[] {id}}, "LocalEvent");
			};
			var oaepl =
				"@public @buseventtype create objectarray schema LocalInnerEvent(id string);\n" +
			            "@public @buseventtype create objectarray schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			BiConsumer<EventType, string> json = (
				type,
				id) => {
				var @event = new JObject(new JProperty("property", new JObject(new JProperty("id", id))));
				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			var jsonepl =
				"@public @buseventtype create json schema LocalInnerEvent(id string);\n" +
			    "@public @buseventtype create json schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, jsonepl, json);

			// Json-Class-Provided
			var jsonProvidedEpl = $"@JsonSchema(className='{typeof(MyLocalJsonProvided).FullName}') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonProvidedEpl, json);

			// Avro
			BiConsumer<EventType, string> avro = (
				type,
				id) => {
				var schema = SchemaBuilder
					.Record("name", Field("id", StringType(
						Property(
						AvroConstant.PROP_STRING_KEY,
						AvroConstant.PROP_STRING_VALUE))));

				var inside = new GenericRecord(schema);
				inside.Put("id", id);
				var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				@event.Put("property", inside);
				env.SendEventAvro(@event, "LocalEvent");
			};
			var avroepl = "@public @buseventtype create avro schema LocalInnerEvent(id string);\n" +
			              "@public @buseventtype create avro schema LocalEvent(property LocalInnerEvent);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, string> sender)
		{
			env.CompileDeploy(createSchemaEPL + "@name('s0') select * from LocalEvent[property];\n").AddListener("s0");
			var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

			sender.Invoke(eventType, "a");
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			Assert.AreEqual("a", @event.Get("id"));

			env.UndeployAll();
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
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedMember.Global
			public MyLocalJsonProvidedInnerEvent property;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			// ReSharper disable once InconsistentNaming
			// ReSharper disable once UnusedMember.Global
			public string id;
		}
	}
} // end of namespace
