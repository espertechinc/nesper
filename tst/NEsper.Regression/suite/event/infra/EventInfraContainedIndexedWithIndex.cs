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

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraContainedIndexedWithIndex : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			Consumer<string[]> bean = ids => {
				var inners = new LocalInnerEvent[ids.Length];
				for (var i = 0; i < ids.Length; i++) {
					inners[i] = new LocalInnerEvent(ids[i]);
				}

				env.SendEventBean(new LocalEvent(inners));
			};
			var beanepl = "@public @buseventtype create schema LocalInnerEvent as " +
			              typeof(LocalInnerEvent).FullName +
			              ";\n" +
			              "@public @buseventtype create schema LocalEvent as " +
			              typeof(LocalEvent).FullName +
			              ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			Consumer<string[]> map = ids => {
				var inners = new IDictionary<string, object>[ids.Length];
				for (var i = 0; i < ids.Length; i++) {
					inners[i] = Collections.SingletonDataMap("id", ids[i]);
				}

				env.SendEventMap(Collections.SingletonDataMap("indexed", inners), "LocalEvent");
			};
			var mapepl = "@public @buseventtype create schema LocalInnerEvent(id string);\n" +
			             "@public @buseventtype create schema LocalEvent(indexed LocalInnerEvent[]);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			Consumer<string[]> oa = ids => {
				var inners = new object[ids.Length][];
				for (var i = 0; i < ids.Length; i++) {
					inners[i] = new object[] { ids[i] };
				}

				env.SendEventObjectArray(new object[] { inners }, "LocalEvent");
			};
			var oaepl = "@public @buseventtype create objectarray schema LocalInnerEvent(id string);\n" +
			            "@public @buseventtype create objectarray schema LocalEvent(indexed LocalInnerEvent[]);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			Consumer<string[]> json = ids => {
				var array = new JArray();
				for (var i = 0; i < ids.Length; i++) {
					array.Add(new JObject(new JProperty("id", ids[i])));
				}

				var @event = new JObject(new JProperty("indexed", array));
				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			var jsonepl = "@public @buseventtype create json schema LocalInnerEvent(id string);\n" +
			              "@public @buseventtype create json schema LocalEvent(indexed LocalInnerEvent[]);\n";
			RunAssertion(env, jsonepl, json);

			// Json-Class-Provided
			var jsonProvidedEpl = "@JsonSchema(className='" +
			                      typeof(MyLocalJsonProvided).FullName +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonProvidedEpl, json);

			// Avro
			Consumer<string[]> avro = ids => {
				var schemaInner = env.RuntimeAvroSchemaByDeployment("schema", "LocalInnerEvent");
				ICollection<GenericRecord> inners = new List<GenericRecord>();
				for (var i = 0; i < ids.Length; i++) {
					var inner = new GenericRecord(schemaInner.AsRecordSchema());
					inner.Put("id", ids[i]);
					inners.Add(inner);
				}

				var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
				var @event = new GenericRecord(schema.AsRecordSchema());
				@event.Put("indexed", inners);
				env.SendEventAvro(@event, "LocalEvent");
			};
			var avroepl = "@name('schema') @public @buseventtype create avro schema LocalInnerEvent(id string);\n" +
			              "@public @buseventtype create avro schema LocalEvent(indexed LocalInnerEvent[]);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			Consumer<string[]> sender)
		{

			env.CompileDeploy(
					createSchemaEPL +
					"@name('s0') select * from LocalEvent[indexed[0]];\n" +
					"@name('s1') select * from LocalEvent[indexed[1]];\n"
				)
				.AddListener("s0")
				.AddListener("s1");

			sender.Invoke(new string[] { "a", "b" });
			env.AssertEqualsNew("s0", "id", "a");
			env.AssertEqualsNew("s1", "id", "b");

			env.UndeployAll();
		}

		[Serializable]
		public class LocalInnerEvent
		{
			private readonly string id;

			public LocalInnerEvent(string id)
			{
				this.id = id;
			}

			public string Id => id;
		}

		[Serializable]
		public class LocalEvent
		{
			public LocalEvent(LocalInnerEvent[] indexed)
			{
				this.Indexed = indexed;
			}

			public LocalInnerEvent[] Indexed { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonProvidedInnerEvent[] indexed;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerEvent
		{
			public string id;
		}
	}
} // end of namespace
