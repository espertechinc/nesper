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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyMappedRuntimeKey : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			// Bean
			Consumer<IDictionary<string, string>> bean = entries => { env.SendEventBean(new LocalEvent(entries)); };
			var beanepl = "@Public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n";
			RunAssertion(env, beanepl, bean);

			// Map
			Consumer<IDictionary<string, string>> map = entries => {
				env.SendEventMap(Collections.SingletonDataMap("mapped", entries), "LocalEvent");
			};
			var mapepl = "@Public @buseventtype create schema LocalEvent(mapped java.util.Map);\n";
			RunAssertion(env, mapepl, map);

			// Object-array
			Consumer<IDictionary<string, string>> oa = entries => {
				env.SendEventObjectArray(new object[] { entries }, "LocalEvent");
			};
			var oaepl = "@public @buseventtype create objectarray schema LocalEvent(mapped java.util.Map);\n";
			RunAssertion(env, oaepl, oa);

			// Json
			Consumer<IDictionary<string, string>> json = entries => {
				var mapValues = new JObject();
				foreach (var entry in entries) {
					mapValues.Add(entry.Key, entry.Value);
				}

				var @event = new JObject(new JProperty("mapped", mapValues));
				env.SendEventJson(@event.ToString(), "LocalEvent");
			};
			var jsonepl = "@public @buseventtype create json schema LocalEvent(mapped java.util.Map);\n";
			RunAssertion(env, jsonepl, json);

			// Json-Class-Provided
			var jsonProvidedEpl = "@JsonSchema(className='" +
			                      typeof(MyLocalJsonProvided).FullName +
			                      "') @public @buseventtype create json schema LocalEvent();\n";
			RunAssertion(env, jsonProvidedEpl, json);

			// Avro
			Consumer<IDictionary<string, string>> avro = entries => {
				var schema = env.RuntimeAvroSchemaByDeployment("schema", "LocalEvent");
				var @event = new GenericRecord(schema.AsRecordSchema());
				@event.Put("mapped", entries);
				env.SendEventAvro(@event, "LocalEvent");
			};
			var avroepl =
				"@name('schema') @public @buseventtype create avro schema LocalEvent(mapped java.util.Map);\n";
			RunAssertion(env, avroepl, avro);
		}

		public void RunAssertion(
			RegressionEnvironment env,
			string createSchemaEPL,
			Consumer<IDictionary<string, string>> sender)
		{

			env.CompileDeploy(
					createSchemaEPL +
					"create constant variable string keyChar = 'a';" +
					"@name('s0') select mapped(keyChar||'1') as c0, mapped(keyChar||'2') as c1 from LocalEvent as e;\n"
				)
				.AddListener("s0");

			IDictionary<string, string> values = new Dictionary<string, string>();
			values.Put("a1", "x");
			values.Put("a2", "y");
			sender.Invoke(values);
			env.AssertPropsNew("s0", "c0,c1".SplitCsv(), new object[] { "x", "y" });

			env.UndeployAll();
		}

		[Serializable]
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
			public IDictionary<string, string> mapped;
		}
	}
} // end of namespace
