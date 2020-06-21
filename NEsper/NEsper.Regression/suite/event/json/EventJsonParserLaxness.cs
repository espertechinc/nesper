///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonParserLaxness
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonParserMalformedJson());
			execs.Add(new EventJsonParserLaxnessStringType());
			execs.Add(new EventJsonParserLaxnessNumberType());
			execs.Add(new EventJsonParserLaxnessBooleanType());
			execs.Add(new EventJsonParserLaxnessObjectType());
			execs.Add(new EventJsonParserUndeclaredContent());
			return execs;
		}

		internal class EventJsonParserUndeclaredContent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema JsonEvent ();\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				var json = "{\n" +
				           "  \"users\": [\n" +
				           "    {\n" +
				           "      \"_id\": \"45166552176594981065\",\n" +
				           "      \"longitude\": 110.5363758848371,\n" +
				           "      \"tags\": [\n" +
				           "        \"ezNI8Gx5vq\"\n" +
				           "      ],\n" +
				           "      \"friends\": [\n" +
				           "        {\n" +
				           "          \"id\": \"4673\",\n" +
				           "          \"name\": \"EqVIiZyuhSCkWXvqSxgyQihZaiwSra\"\n" +
				           "        }\n" +
				           "      ],\n" +
				           "      \"greeting\": \"xfS8vUXYq4wzufBLP6CY\",\n" +
				           "      \"favoriteFruit\": \"KT0tVAxXRawtbeQIWAot\"\n" +
				           "    },\n" +
				           "    {\n" +
				           "      \"_id\": \"23504426278646846580\",\n" +
				           "      \"favoriteFruit\": \"9aUx0u6G840i0EeKFM4Z\"\n" +
				           "    }\n" +
				           "  ]\n" +
				           "}";

				env.SendEventJson(json, "JsonEvent");

				env.UndeployAll();
			}
		}

		internal class EventJsonParserLaxnessObjectType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema JsonEvent (carray Object[], cobject Map);\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssert(
					env,
					new JObject(new JProperty("carray", new JObject())).ToString(),
					@event => Assert.IsNull(@event.Get("carray")));
				SendAssert(
					env,
					new JObject(new JProperty("carray", "abc")).ToString(),
					@event => Assert.IsNull(@event.Get("carray")));
				SendAssert(
					env,
					new JObject(new JProperty("cobject", new JArray())).ToString(),
					@event => Assert.IsNull(@event.Get("cobject")));
				SendAssert(
					env,
					new JObject(new JProperty("cobject", "abc")).ToString(),
					@event => Assert.IsNull(@event.Get("cobject")));

				env.UndeployAll();
			}
		}

		internal class EventJsonParserLaxnessBooleanType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema JsonEvent (" +
				          "cbool boolean, cboola1 boolean[], cboola2 boolean[][]);\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				SendAssert(
					env,
					new JObject(new JProperty("cbool", "true")).ToString(),
					@event => Assert.IsTrue(@event.Get("cbool").AsBoolean()));
				SendAssert(
					env,
					new JObject(new JProperty("cbool", "false")).ToString(),
					@event => Assert.IsFalse(@event.Get("cbool").AsBoolean()));
				SendAssert(
					env,
					new JObject(new JProperty("cboola1", new JArray("true"))).ToString(),
					@event => Assert.IsTrue(((object[]) @event.Get("cboola1"))[0].AsBoolean()));
				SendAssert(
					env,
					new JObject(new JProperty("cboola2", new JArray(new JArray("true")))).ToString(),
					@event => Assert.IsTrue(((object[][]) @event.Get("cboola2"))[0][0].AsBoolean()));
				SendAssert(
					env,
					new JObject(new JProperty("cbool", new JObject())).ToString(),
					@event => Assert.IsNull(@event.Get("cbool")));
				SendAssert(
					env,
					new JObject(new JProperty("cbool", new JArray())).ToString(),
					@event => Assert.IsNull(@event.Get("cbool")));

				TryInvalid(
					env,
					new JObject(new JProperty("cbool", "x")).ToString(),
					"Failed to parse json member name 'cbool' as a boolean-type from value 'x'");
				TryInvalid(
					env,
					new JObject(new JProperty("cboola1", new JArray("x"))).ToString(),
					"Failed to parse json member name 'cboola1' as a boolean-type from value 'x'");
				TryInvalid(
					env,
					new JObject(new JProperty("cboola2", new JArray(new JArray(new JValue("x"))))).ToString(),
					"Failed to parse json member name 'cboola2' as a boolean-type from value 'x'");
				TryInvalid(
					env,
					new JObject(new JProperty("cbool", "null")).ToString(),
					"Failed to parse json member name 'cbool' as a boolean-type from value 'null'");

				env.UndeployAll();
			}
		}

		internal class EventJsonParserLaxnessNumberType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype create json schema JsonEvent (" +
				          "cbyte byte, cshort short, cint int, clong long, cdouble double, cfloat float, cBigint Biginteger, cDecimalOne DecimalOneimal," +
				          "cbytea1 byte[], cshorta1 short[], cinta1 int[], clonga1 long[], cdoublea1 double[], cfloata1 float[], cBiginta1 Biginteger[], cDecimalOnea1 DecimalOneimal[]," +
				          "cbytea2 byte[][], cshorta2 short[][], cinta2 int[][], clonga2 long[][], cdoublea2 double[][], cfloata2 float[][], cBiginta2 Biginteger[][], cDecimalOnea2 DecimalOneimal[][]);\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");
				var eventType = env.Runtime.EventTypeService.GetEventType(env.DeploymentId("s0"), "JsonEvent");

				// lax parsing is the default, allowing string values
				foreach (var propertyName in eventType.PropertyNames) {
					var @event = MakeSendJson(env, propertyName, "1");
					var value = @event.Get(propertyName);
					Assert.IsNotNull(value, "Null for property " + propertyName);
					if (propertyName.EndsWith("a2")) {
						AssertAsNumber(propertyName, 1, ((object[][]) value)[0][0]);
					}
					else if (propertyName.EndsWith("a1")) {
						AssertAsNumber(propertyName, 1, ((object[]) value)[0]);
					}
					else {
						AssertAsNumber(propertyName, 1, value);
					}
				}

				// invalid number
				foreach (var propertyName in eventType.PropertyNames) {
					try {
						MakeSendJson(env, propertyName, "x");
						Assert.Fail();
					}
					catch (EPException ex) {
						var typeName = propertyName.Substring(1).Replace("a1", "").Replace("a2", "");
						Type type;
						if (typeName.Equals("Bigint")) {
							type = typeof(BigInteger);
						}
						else {
							type = TypeHelper.GetPrimitiveTypeForName(typeName).GetBoxedType();
							Assert.IsNotNull(type, "Unrecognized type " + typeName);
						}

						var expected = "Failed to parse json member name '" +
						               propertyName +
						               "' as a " +
						               type.Name +
						               "-type from value 'x': NumberFormatException";
						Assert.IsTrue(ex.Message.StartsWith(expected));
					}
				}

				// unexpected object type
				SendAssert(env, new JObject(new JProperty("cint", new JObject())).ToString(), @event => Assert.IsNull(@event.Get("cint")));
				SendAssert(env, new JObject(new JProperty("cint", new JArray())).ToString(), @event => Assert.IsNull(@event.Get("cint")));

				env.UndeployAll();
			}

			private EventBean MakeSendJson(
				RegressionEnvironment env,
				string propertyName,
				string value)
			{
				var json = new JObject();
				if (propertyName.EndsWith("a2")) {
					json.Add(propertyName, new JArray(new JArray(value)));
				}
				else if (propertyName.EndsWith("a1")) {
					json.Add(propertyName, new JArray(value));
				}
				else {
					json.Add(propertyName, value);
				}

				env.SendEventJson(json.ToString(), "JsonEvent");
				return env.Listener("s0").AssertOneGetNewAndReset();
			}
		}

		internal class EventJsonParserLaxnessStringType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype @JsonSchema create json schema JsonEvent(p1 string);\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				// lax parsing is the default
				SendAssertP1(env, "{ \"p1\" : 1 }", "1");
				SendAssertP1(env, "{ \"p1\" : 1.1234 }", "1.1234");
				SendAssertP1(env, "{ \"p1\" : true }", "true");
				SendAssertP1(env, "{ \"p1\" : false }", "false");
				SendAssertP1(env, "{ \"p1\" : null }", null);
				SendAssertP1(env, "{ \"p1\" : [\"abc\"] }", null);
				SendAssertP1(env, "{ \"p1\" : {\"abc\": \"def\"} }", null);

				env.UndeployAll();
			}
		}

		internal class EventJsonParserMalformedJson : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype @JsonSchema create json schema JsonEvent(p1 string);\n" +
				          "@Name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				TryInvalid(env, "", "Failed to parse Json: Unexpected end of input at 1:1");
				TryInvalid(env, "{}{}", "Failed to parse Json: Unexpected character at 1:3");
				TryInvalid(env, "{{}", "Failed to parse Json: Expected name at 1:2");

				env.UndeployAll();
			}
		}

		private static void SendAssertP1(
			RegressionEnvironment env,
			string json,
			object expected)
		{
			SendAssert(env, json, @event => Assert.AreEqual(expected, @event.Get("p1")));
		}

		private static void SendAssert(
			RegressionEnvironment env,
			string json,
			Consumer<EventBean> assertion)
		{
			env.SendEventJson(json, "JsonEvent");
			assertion.Invoke(env.Listener("s0").AssertOneGetNewAndReset());
		}

		private static void TryInvalid(
			RegressionEnvironment env,
			string json,
			string message)
		{
			try {
				env.Runtime.EventService.SendEventJson(json, "JsonEvent");
				Assert.Fail();
			}
			catch (EPException ex) {
				SupportMessageAssertUtil.AssertMessage(message, ex.Message);
			}
		}

		private static void AssertAsNumber(
			string propertyName,
			object expected,
			object actualNumber)
		{
			var actual = actualNumber;
			if (propertyName.Contains("byte")) {
				Assert.AreEqual(expected.AsByte(), actual.AsByte());
			}
			else if (propertyName.Contains("short")) {
				Assert.AreEqual(expected.AsInt16(), actual.AsInt16());
			}
			else if (propertyName.Contains("int")) {
				Assert.AreEqual(expected.AsInt32(), actual.AsInt32());
			}
			else if (propertyName.Contains("long")) {
				Assert.AreEqual(expected.AsInt64(), actual.AsInt64());
			}
			else if (propertyName.Contains("double")) {
				Assert.AreEqual(expected.AsDouble(), actual.AsDouble(), 0.1);
			}
			else if (propertyName.Contains("float")) {
				Assert.AreEqual(expected.AsFloat(), actual.AsFloat(), 0.1);
			}
			else if (propertyName.Contains("decimal")) {
				Assert.AreEqual(expected.AsDecimal(), actual.AsDecimal());
			}
			else if (propertyName.Contains("Bigint")) {
				Assert.AreEqual(expected.ToString(), actual.ToString());
			}
			else {
				Assert.Fail("Not recognized '" + propertyName + "'");
			}
		}
	}
} // end of namespace
