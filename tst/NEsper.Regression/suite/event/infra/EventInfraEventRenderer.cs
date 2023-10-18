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

using com.espertech.esper.common.client.render;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraEventRenderer : RegressionExecution
	{
		public const string XML_TYPENAME = "EventInfraEventRendererXML";
		private static readonly Type BEAN_TYPE = typeof(MyEvent);
		public const string MAP_TYPENAME = "EventInfraEventRendererMap";
		public const string OA_TYPENAME = "EventInfraEventRendererOA";
		public const string AVRO_TYPENAME = "EventInfraEventRendererAvro";
		public const string JSON_TYPENAME = "EventInfraEventRendererJson";
		public const string JSONPROVIDED_TYPENAME = "EventInfraEventRendererJsonProvided";

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			// Bean
			RunAssertion(env, BEAN_TYPE.Name, FBEAN, new MyEvent(1, "abc", new MyInsideEvent(10)), path);

			// Map
			IDictionary<string, object> mapInner = new Dictionary<string, object>();
			mapInner.Put("myInsideInt", 10);
			IDictionary<string, object> topInner = new Dictionary<string, object>();
			topInner.Put("myInt", 1);
			topInner.Put("myString", "abc");
			topInner.Put("nested", mapInner);
			RunAssertion(env, MAP_TYPENAME, FMAP, topInner, path);

			// Object-array
			var oaInner = new object[] { 10 };
			var oaTop = new object[] { 1, "abc", oaInner };
			RunAssertion(env, OA_TYPENAME, FOA, oaTop, path);

			// XML
			var xml = "<myevent myInt=\"1\" myString=\"abc\"><nested myInsideInt=\"10\"/></myevent>";
			RunAssertion(env, XML_TYPENAME, FXML, xml, path);

			// Avro
			var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
			var innerSchema = schema.GetField("nested").Schema.AsRecordSchema();
			var avroInner = new GenericRecord(innerSchema);
			avroInner.Put("myInsideInt", 10);
			var avro = new GenericRecord(schema);
			avro.Put("myInt", 1);
			avro.Put("myString", "abc");
			avro.Put("nested", avroInner);
			RunAssertion(env, AVRO_TYPENAME, FAVRO, avro, path);

			// Json
			var schemasJson = "create json schema Nested(myInsideInt int);\n" +
			                  "@public @buseventtype @name('schema') create json schema " +
			                  JSON_TYPENAME +
			                  "(myInt int, myString string, nested Nested)";
			env.CompileDeploy(schemasJson, path);
			var json = "{\n" +
			           "  \"myInt\": 1,\n" +
			           "  \"myString\": \"abc\",\n" +
			           "  \"nested\": {\n" +
			           "    \"myInsideInt\": 10\n" +
			           "  }\n" +
			           "}";
			RunAssertion(env, JSON_TYPENAME, FJSON, json, path);

			// Json-Class-Provided
			var schemas = "@JsonSchema(className='" +
			              typeof(MyLocalJsonProvided).FullName +
			              "') " +
			              "@public @buseventtype @name('schema') create json schema " +
			              JSONPROVIDED_TYPENAME +
			              "()";
			env.CompileDeploy(schemas, path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, json, path);
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent send,
			object @event,
			RegressionPath path)
		{
			var epl = "@name('s0') select * from " + typename;
			env.CompileDeploy(epl, path).AddListener("s0");
			send.Invoke(env, @event, typename);

			env.AssertEventNew(
				"s0",
				eventBean => {
					var jsonEventRenderer = env.Runtime.RenderEventService.GetJSONRenderer(eventBean.EventType);
					var json = jsonEventRenderer.Render(eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
					Assert.AreEqual("{\"myInt\":1,\"myString\":\"abc\",\"nested\":{\"myInsideInt\":10}}", json);

					var xmlEventRenderer = env.Runtime.RenderEventService.GetXMLRenderer(eventBean.EventType);
					var xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
					Assert.AreEqual(
						"<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><myInt>1</myInt><myString>abc</myString><nested><myInsideInt>10</myInsideInt></nested></root>",
						xml);
				});

			env.UndeployAll();
		}

		[Serializable]
		public class MyEvent
		{
			public MyEvent(
				int myInt,
				string myString,
				MyInsideEvent nested)
			{
				MyInt = myInt;
				MyString = myString;
				Nested = nested;
			}

			public int MyInt { get; }

			public string MyString { get; }

			public MyInsideEvent Nested { get; }
		}

		[Serializable]
		public class MyInsideEvent
		{
			public MyInsideEvent(int myInsideInt)
			{
				MyInsideInt = myInsideInt;
			}

			public int MyInsideInt { get; }
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public int MyInt;
			public string MyString;
			public MyLocalJsonProvidedNested Nested;
		}

		[Serializable]
		public class MyLocalJsonProvidedNested
		{
			public int MyInsideInt;
		}
	}
} // end of namespace
