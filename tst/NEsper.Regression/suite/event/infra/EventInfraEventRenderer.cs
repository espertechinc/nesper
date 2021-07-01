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
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraEventRenderer : RegressionExecution
	{
		public const string XML_TYPENAME = "EventInfraEventRendererXML";
		private static readonly Type BEAN_TYPE = typeof(EventInfraEventRenderer.MyEvent);
		public const string MAP_TYPENAME = "EventInfraEventRendererMap";
		public const string OA_TYPENAME = "EventInfraEventRendererOA";
		public const string AVRO_TYPENAME = "EventInfraEventRendererAvro";
		public const string JSON_TYPENAME = "EventInfraEventRendererJson";
		public const string JSONPROVIDED_TYPENAME = "EventInfraEventRendererJsonProvided";

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			RunBeanType(env, path);
			RunMapType(env, path);
			RunObjectArrayType(env, path);
			RunXMLType(env, path);
			RunAvroType(env, path);
			RunJsonType(env, path);
			RunJsonTypeProvidedClass(env, path);
		}

		private void RunJsonTypeProvidedClass(
			RegressionEnvironment env,
			RegressionPath path)
		{
			var json = "{\n" +
			           "  \"MyInt\": 1,\n" +
			           "  \"MyString\": \"abc\",\n" +
			           "  \"Nested\": {\n" +
			           "    \"MyInsideInt\": 10\n" +
			           "  }\n" +
			           "}";

			// Json-Class-Provided
			var schemas = 
				$"@JsonSchema(ClassName='{typeof(MyLocalJsonProvided).FullName}') " +
			    $"@public @buseventtype @name('schema') " +
				$"create json schema {JSONPROVIDED_TYPENAME} ()";
			env.CompileDeploy(schemas, path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, json, path);
		}

		private void RunJsonType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			var json = "{\n" +
			           "  \"MyInt\": 1,\n" +
			           "  \"MyString\": \"abc\",\n" +
			           "  \"Nested\": {\n" +
			           "    \"MyInsideInt\": 10\n" +
			           "  }\n" +
			           "}";
			
			// Json
			var schemasJson = "create json schema Nested(MyInsideInt int);\n" +
			                  "@public @buseventtype @name('schema') create json schema " +
			                  JSON_TYPENAME +
			                  "(MyInt int, MyString string, Nested Nested)";
			env.CompileDeploy(schemasJson, path);
			RunAssertion(env, JSON_TYPENAME, FJSON, json, path);
		}

		private void RunAvroType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			// Avro
			var schema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
			var innerSchema = schema.GetField("Nested").Schema;
			var avroInner = new GenericRecord(innerSchema.AsRecordSchema());
			avroInner.Put("MyInsideInt", 10);
			var avro = new GenericRecord(schema.AsRecordSchema());
			avro.Put("MyInt", 1);
			avro.Put("MyString", "abc");
			avro.Put("Nested", avroInner);
			RunAssertion(env, AVRO_TYPENAME, FAVRO, avro, path);
		}

		private void RunXMLType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			// XML
			var xml = "<Myevent MyInt=\"1\" MyString=\"abc\"><Nested MyInsideInt=\"10\"/></Myevent>";
			RunAssertion(env, XML_TYPENAME, FXML, xml, path);
		}

		private void RunObjectArrayType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			// Object-array
			var oaInner = new object[] {10};
			var oaTop = new object[] {1, "abc", oaInner};
			RunAssertion(env, OA_TYPENAME, FOA, oaTop, path);
		}

		private void RunMapType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			// Map
			IDictionary<string, object> mapInner = new Dictionary<string, object>();
			mapInner.Put("MyInsideInt", 10);
			IDictionary<string, object> topInner = new Dictionary<string, object>();
			topInner.Put("MyInt", 1);
			topInner.Put("MyString", "abc");
			topInner.Put("Nested", mapInner);
			RunAssertion(env, MAP_TYPENAME, FMAP, topInner, path);
		}

		private void RunBeanType(
			RegressionEnvironment env,
			RegressionPath path)
		{
			// Bean
			RunAssertion(env, BEAN_TYPE.Name, FBEAN, new MyEvent(1, "abc", new MyInsideEvent(10)), path);
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent send,
			object @event,
			RegressionPath path)
		{
			var epl = "@Name('s0') select * from " + typename;
			env.CompileDeploy(epl, path).AddListener("s0");
			send.Invoke(env, @event, typename);

			var eventBean = env.Listener("s0").AssertOneGetNewAndReset();

			var jsonEventRenderer = env.Runtime.RenderEventService.GetJSONRenderer(env.Statement("s0").EventType);
			var json = jsonEventRenderer.Render(eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
			Assert.AreEqual("{\"MyInt\":1,\"MyString\":\"abc\",\"Nested\":{\"MyInsideInt\":10}}", json);

			var xmlEventRenderer = env.Runtime.RenderEventService.GetXMLRenderer(env.Statement("s0").EventType);
			var xml = xmlEventRenderer.Render("root", eventBean).RegexReplaceAll("(\\s|\\n|\\t)", "");
			Assert.AreEqual(
				"<?xmlversion=\"1.0\"encoding=\"UTF-8\"?><root><MyInt>1</MyInt><MyString>abc</MyString><Nested><MyInsideInt>10</MyInsideInt></Nested></root>",
				xml);

			env.UndeployAll();
		}

		public class MyEvent
		{
			public MyEvent(
				int myInt,
				string myString,
				MyInsideEvent nested)
			{
				this.MyInt = myInt;
				this.MyString = myString;
				this.Nested = nested;
			}

			public int MyInt { get; }

			public string MyString { get; }

			public MyInsideEvent Nested { get; }
		}

		public class MyInsideEvent
		{
			public MyInsideEvent(int myInsideInt)
			{
				this.MyInsideInt = myInsideInt;
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
