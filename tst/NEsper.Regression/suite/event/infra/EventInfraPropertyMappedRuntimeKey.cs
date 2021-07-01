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

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyMappedRuntimeKey : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			BiConsumer<EventType, IDictionary<string, string>> bean = (
				type,
				entries) => {
				env.SendEventBean(new LocalEvent(entries));
			};
			var beanepl = $"@public @buseventtype create schema LocalEvent as {typeof(LocalEvent).MaskTypeName()};\n";
			RunAssertion(env, beanepl, bean);

			// Map
			BiConsumer<EventType, IDictionary<string, string>> map = (
				type,
				entries) => {
				env.SendEventMap(Collections.SingletonDataMap("Mapped", entries), "LocalEvent");
			};
			var mapType = typeof(IDictionary<string, object>).CleanName();
			var mapepl = $"@public @buseventtype create schema LocalEvent(Mapped `{mapType}`);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			BiConsumer<EventType, IDictionary<string, string>> oa = (
				type,
				entries) => {
				env.SendEventObjectArray(new object[] {entries}, "LocalEvent");
			};
			var oaepl = $"@public @buseventtype create objectarray schema LocalEvent(Mapped `{mapType}`);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			BiConsumer<EventType, IDictionary<string, string>> json = (
				type,
				entries) => {
				var mapValues = new JObject();
				foreach (var entry in entries) {
					mapValues.Add(entry.Key, entry.Value);
				}

				var @event = new JObject(new JProperty("Mapped", mapValues));
				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			var jsonepl = $"@public @buseventtype create json schema LocalEvent(Mapped `{mapType}`);\n";
			RunAssertion(env, jsonepl, json);

			// Json-Class-Provided
			var jsonProvidedType = typeof(MyLocalJsonProvided).MaskTypeName();
			var jsonProvidedEpl = $"@JsonSchema(ClassName='{jsonProvidedType}') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonProvidedEpl, json);

			// Avro
			BiConsumer<EventType, IDictionary<string, string>> avro = (
				type,
				entries) => {
				var @event = new GenericRecord(SupportAvroUtil.GetAvroSchema(type).AsRecordSchema());
				@event.Put("Mapped", entries);
				env.SendEventAvro(@event, "LocalEvent");
			};
			var avroepl = $"@public @buseventtype create avro schema LocalEvent(Mapped `{mapType}`);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			BiConsumer<EventType, IDictionary<string, string>> sender)
		{

			env.CompileDeploy(
					createSchemaEPL +
					"create constant variable string keyChar = 'a';" +
					"@Name('s0') select Mapped(keyChar||'1') as c0, Mapped(keyChar||'2') as c1 from LocalEvent as e;\n"
				)
				.AddListener("s0");
			var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "LocalEvent");

			IDictionary<string, string> values = new Dictionary<string, string>();
			values.Put("a1", "x");
			values.Put("a2", "y");
			sender.Invoke(eventType, values);
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "c0,c1".SplitCsv(), new object[] {"x", "y"});

			env.UndeployAll();
		}

		public class LocalEvent
		{
			private IDictionary<string, string> mapped;

			public LocalEvent(IDictionary<string, string> mapped)
			{
				this.mapped = mapped;
			}

			public IDictionary<string, string> Mapped => mapped;
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public IDictionary<string, string> Mapped;
		}
	}
} // end of namespace
