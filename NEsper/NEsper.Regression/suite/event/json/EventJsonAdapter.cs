///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;

using com.espertech.esper.common.client;

using com.espertech.esper.common.client.json.util;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.json
{
	public class EventJsonAdapter
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EventJsonAdapterInsertInto());
			execs.Add(new EventJsonAdapterCreateSchemaWStringTransform());
			execs.Add(new EventJsonAdapterInvalid());
			execs.Add(new EventJsonAdapterDocSample());
			return execs;
		}

		internal class EventJsonAdapterDocSample : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var sdf = new SimpleDateFormat("dd-M-yyyy");
				var date = sdf.Parse("22-09-2018");

				var epl = "@public @buseventtype @JsonSchemaField(name=myDate, adapter='" +
				          typeof(MyDateJSONParser).FullName +
				          "')\n" +
				          "create json schema JsonEvent(myDate Date);\n" +
				          "@name('s0') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.SendEventJson("{\"myDate\" : \"22-09-2018\"}", "JsonEvent");
				Assert.AreEqual(date, env.Listener("s0").AssertOneGetNew().Get("myDate"));

				var renderer = env.Runtime.RenderEventService.GetJSONRenderer(env.Runtime.EventTypeService.GetBusEventType("JsonEvent"));
				Assert.AreEqual("{\"hello\":{\"myDate\":\"22-09-2018\"}}", renderer.Render("hello", env.Listener("s0").AssertOneGetNewAndReset()));

				env.UndeployAll();
			}
		}

		internal class EventJsonAdapterInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"@JsonSchemaField(name=mydate, adapter=x) create json schema JsonEvent(mydate Date)",
					"Failed to resolve Json schema field adapter class: Could not load class by name 'x', please check imports");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"@JsonSchemaField(name=mydate, adapter='java.lang.String') create json schema JsonEvent(mydate Date)",
					"Json schema field adapter class does not implement interface 'JsonFieldAdapterString");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"@JsonSchemaField(name=mydate, adapter='" + typeof(InvalidAdapterJSONDate).FullName + "') create json schema JsonEvent(mydate Date)",
					"Json schema field adapter class 'InvalidAdapterJSONDate' does not have a default constructor");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"@JsonSchemaField(name=mydate, adapter='" + nameof(SupportJsonFieldAdapterStringDate) + "') create json schema JsonEvent(mydate String)",
					"Json schema field adapter class 'SupportJsonFieldAdapterStringDate' mismatches the return type of the parse method, expected 'String' but found 'Date'");
			}
		}

		internal class EventJsonAdapterInsertInto : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@public @buseventtype create schema LocalEvent as " + typeof(LocalEvent).FullName + ";\n" +
					"@JsonSchemaField(name=mydate, adapter=" + nameof(SupportJsonFieldAdapterStringDate) + ") " +
					"@JsonSchemaField(name=point, adapter=" + nameof(SupportJsonFieldAdapterStringPoint) + ") " +
					EventRepresentationChoice.JSON.GetAnnotationText() +
					" insert into JsonEvent select point, mydate from LocalEvent;\n" +
					"@name('s0') select point, mydate from JsonEvent;\n" +
					"@name('s1') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

				env.SendEventBean(
					new LocalEvent(
						new Point(7, 14),
						DateTimeParsingFunctions.ParseDefaultEx("2002-05-01T08:00:01.999")));

				var jsonFilled = "{\"point\":\"7,14\",\"mydate\":\"2002-05-01T08:00:01.999\"}";
				DoAssert(
					env,
					jsonFilled,
					new object[] {
						new Point(7, 14),
						DateTimeParsingFunctions.ParseDefaultEx("2002-05-1T08:00:01.999")
					});

				env.UndeployAll();
			}
		}

		internal class EventJsonAdapterCreateSchemaWStringTransform : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@public @buseventtype " +
				          "@JsonSchemaField(name=point, adapter=" +
				          nameof(SupportJsonFieldAdapterStringPoint) +
				          ") " +
				          "@JsonSchemaField(name=mydate, adapter=" +
				          nameof(SupportJsonFieldAdapterStringDate) +
				          ") " +
				          "create json schema JsonEvent(point java.awt.Point, mydate Date);\n" +
				          "@name('s0') select point, mydate from JsonEvent;\n" +
				          "@name('s1') select * from JsonEvent;\n";
				env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

				var jsonFilled = "{\"point\":\"7,14\",\"mydate\":\"2002-05-01T08:00:01.999\"}";
				SendAssert(
					env,
					jsonFilled,
					new object[] {
						new Point(7, 14), DateTimeParsingFunctions.ParseDefaultEx("2002-05-1T08:00:01.999")
					});

				var jsonNulled = "{\"point\":null,\"mydate\":null}";
				SendAssert(
					env,
					jsonNulled,
					new object[] {
						null, null
					});

				env.UndeployAll();
			}
		}

		private static void SendAssert(
			RegressionEnvironment env,
			string json,
			object[] expected)
		{
			env.SendEventJson(json, "JsonEvent");
			DoAssert(env, json, expected);
		}

		private static void DoAssert(
			RegressionEnvironment env,
			string json,
			object[] expected)
		{
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "point,mydate".SplitCsv(), expected);

			var @event = (JsonEventObject) env.Listener("s1").AssertOneGetNewAndReset().Underlying;
			Assert.AreEqual(json, @event.ToString());
		}

		public class LocalEvent
		{
			private readonly Point point;
			private readonly DateTimeEx mydate;

			public LocalEvent(
				Point point,
				DateTimeEx mydate)
			{
				this.point = point;
				this.mydate = mydate;
			}

			public Point Point => point;

			public DateTimeEx Mydate => mydate;
		}

		public class InvalidAdapterJSONDate : JsonFieldAdapterString<DateTimeEx>
		{
			public InvalidAdapterJSONDate(int a)
			{
			}

			public DateTimeEx Parse(string value)
			{
				throw new UnsupportedOperationException();
			}

			public void Write(
				DateTimeEx value,
				Utf8JsonWriter writer)
			{
				throw new UnsupportedOperationException();
			}
		}

		public class MyDateJSONParser : JsonFieldAdapterString<DateTimeEx>
		{
			public DateTimeEx Parse(string value)
			{
				try {
					return value == null ? null : new SimpleDateFormat("dd-MM-yyyy").Parse(value);
				}
				catch (ParseException e) {
					throw new EPException("Failed to parse: " + e.Message, e);
				}
			}

			public void Write(
				DateTimeEx value,
				Utf8JsonWriter writer)
			{
				if (value == null) {
					writer.WriteNullValue();
					return;
				}

				writer.WriteStringValue(new SimpleDateFormat("dd-MM-yyyy").Format(value));
			}
		}
	}
} // end of namespace
