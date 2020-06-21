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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.EventRepresentationChoice;

using static NEsper.Avro.Extensions.TypeBuilder;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreTypeOf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
WithFragment(execs);
WithNamedUnnamedPONO(execs);
WithInvalid(execs);
WithDynamicProps(execs);
WithVariantStream(execs);
			return execs;
		}
public static IList<RegressionExecution> WithVariantStream(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprCoreTypeOfVariantStream());
    return execs;
}public static IList<RegressionExecution> WithDynamicProps(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprCoreTypeOfDynamicProps());
    return execs;
}public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprCoreTypeOfInvalid());
    return execs;
}public static IList<RegressionExecution> WithNamedUnnamedPONO(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprCoreTypeOfNamedUnnamedPONO());
    return execs;
}public static IList<RegressionExecution> WithFragment(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new ExprCoreTypeOfFragment());
    return execs;
}
		private class ExprCoreTypeOfInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select typeof(xx) from SupportBean",
					"Failed to validate select-clause expression 'typeof(xx)': Property named 'xx' is not valid in any stream [select typeof(xx) from SupportBean]");
			}
		}

		private class ExprCoreTypeOfDynamicProps : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var schema = MAP.GetAnnotationText() + " create schema MyDynoPropSchema as (key string);\n";
				env.CompileDeployWBusPublicType(schema, path);

				var fields = "typeof(prop?),typeof(key)".SplitCsv();
				var builder = new SupportEvalBuilder("MyDynoPropSchema").WithPath(path)
					.WithExpressions(fields, "typeof(prop?)", "typeof(key)");

				builder.WithAssertion(MakeSchemaEvent(1, "E1")).Expect(fields, "Int32", "String");

				builder.WithAssertion(MakeSchemaEvent("test", "E2")).Expect(fields, "String", "String");

				builder.WithAssertion(MakeSchemaEvent(null, "E3")).Expect(fields, null, "String");

				builder.Run(env, true);
				builder.Run(env, false);
				env.UndeployAll();
			}
		}

		private class ExprCoreTypeOfVariantStream : RegressionExecution
		{
			public bool IsExcludeWhenInstrumented {
				get { return true; }
			}

			public void Run(RegressionEnvironment env)
			{
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionVariantStream(env, rep);
				}
			}
		}

		private class ExprCoreTypeOfNamedUnnamedPONO : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("ISupportA", "A")
					.WithExpressions(fields, "typeof(A)");

				builder.WithAssertion(new ISupportAImpl(null, null)).Expect(fields, typeof(ISupportAImpl).Name);

				builder.WithAssertion(new ISupportABCImpl(null, null, null, null)).Expect(fields, typeof(ISupportABCImpl).Name);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreTypeOfFragment : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryAssertionFragment(env, EventRepresentationChoice.JSON);
				foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
					TryAssertionFragment(env, rep);
				}
			}
		}

		private static void TryAssertionVariantStream(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{

			var eplSchemas =
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedWKey>() +
				" create schema EventOne as (key string);\n" +
				eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedWKey>() +
				" create schema EventTwo as (key string);\n" +
				" create schema S0 as " +
				typeof(SupportBean_S0).FullName +
				";\n" +
				" create variant schema VarSchema as *;\n";
			var path = new RegressionPath();
			env.CompileDeployWBusPublicType(eplSchemas, path);

			env.CompileDeploy("insert into VarSchema select * from EventOne", path);
			env.CompileDeploy("insert into VarSchema select * from EventTwo", path);
			env.CompileDeploy("insert into VarSchema select * from S0", path);
			env.CompileDeploy("insert into VarSchema select * from SupportBean", path);

			var stmtText = "@Name('s0') select typeof(A) as t0 from VarSchema as A";
			env.CompileDeploy(stmtText, path).AddListener("s0");

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {"value"}, "EventOne");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventOne");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var record = new GenericRecord(SchemaBuilder.Record("EventOne", RequiredString("key")));
				record.Put("key", "value");
				env.SendEventAvro(record, "EventOne");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				env.SendEventJson(new JObject(new JProperty("key", "value")).ToString(), "EventOne");
			}
			else {
				Assert.Fail();
			}

			Assert.AreEqual("EventOne", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {"value"}, "EventTwo");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventTwo");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var record = new GenericRecord(SchemaBuilder.Record("EventTwo", RequiredString("key")));
				record.Put("key", "value");
				env.SendEventAvro(record, "EventTwo");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				env.SendEventJson(new JObject(new JProperty("key", "value")).ToString(), "EventTwo");
			}
			else {
				Assert.Fail();
			}

			Assert.AreEqual("EventTwo", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

			env.SendEventBean(new SupportBean_S0(1), "S0");
			Assert.AreEqual("S0", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

			env.SendEventBean(new SupportBean());
			Assert.AreEqual("SupportBean", env.Listener("s0").AssertOneGetNewAndReset().Get("t0"));

			env.UndeployModuleContaining("s0");
			env.CompileDeploy(
					"@Name('s0') select * from VarSchema match_recognize(\n" +
					"  measures A as a, B as b\n" +
					"  pattern (A B)\n" +
					"  define A as typeof(A) = \"EventOne\",\n" +
					"         B as typeof(B) = \"EventTwo\"\n" +
					"  )",
					path)
				.AddListener("s0");

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {"value"}, "EventOne");
				env.SendEventObjectArray(new object[] {"value"}, "EventTwo");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventOne");
				env.SendEventMap(Collections.SingletonDataMap("key", "value"), "EventTwo");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var schema = SchemaBuilder.Record("EventTwo", RequiredString("key"));
				var eventOne = new GenericRecord(schema);
				eventOne.Put("key", "value");
				var eventTwo = new GenericRecord(schema);
				eventTwo.Put("key", "value");
				env.SendEventAvro(eventOne, "EventOne");
				env.SendEventAvro(eventTwo, "EventTwo");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				env.SendEventJson(new JObject(new JProperty("key", "value")).ToString(), "EventOne");
				env.SendEventJson(new JObject(new JProperty("key", "value")).ToString(), "EventTwo");
			}
			else {
				Assert.Fail();
			}

			Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

			env.UndeployAll();
		}

		private static void TryAssertionFragment(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{
			var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedInnerSchema>() +
			          " create schema InnerSchema as (key string);\n" +
			          eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMySchema>() +
			          " create schema MySchema as (inside InnerSchema, insidearr InnerSchema[]);\n" +
			          eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedOut>() +
			          " @name('s0') select typeof(s0.inside) as t0, typeof(s0.insidearr) as t1 from MySchema as s0;\n";
			var fields = new[] {"t0", "t1"};
			env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");
			var deploymentId = env.DeploymentId("s0");

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[2], "MySchema");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				env.SendEventMap(new Dictionary<string, object>(), "MySchema");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var schema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema")).AsRecordSchema();
				env.SendEventAvro(new GenericRecord(schema), "MySchema");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				env.SendEventJson("{}", "MySchema");
			}
			else {
				Assert.Fail();
			}

			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, null, null);

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {new object[2], null}, "MySchema");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> theEvent = new Dictionary<string, object>();
				theEvent.Put("inside", new Dictionary<string, object>());
				env.SendEventMap(theEvent, "MySchema");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var mySchema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema")).AsRecordSchema();
				var innerSchema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "InnerSchema")).AsRecordSchema();
				var @event = new GenericRecord(mySchema);
				@event.Put("inside", new GenericRecord(innerSchema));
				env.SendEventAvro(@event, "MySchema");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var theEvent = new JObject(new JProperty("inside", new JObject()));
				env.SendEventJson(theEvent.ToString(), "MySchema");
			}
			else {
				Assert.Fail();
			}

			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, "InnerSchema", null);

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {null, new object[2][]}, "MySchema");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				IDictionary<string, object> theEvent = new Dictionary<string, object>();
				theEvent.Put("insidearr", new IDictionary<string, object>[0]);
				env.SendEventMap(theEvent, "MySchema");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				var mySchema = SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventType(deploymentId, "MySchema")).AsRecordSchema();
				var @event = new GenericRecord(mySchema);
				@event.Put("insidearr", EmptyList<object>.Instance);
				env.SendEventAvro(@event, "MySchema");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				var theEvent = new JObject(new JProperty("insidearr", new JArray(new JObject())));
				env.SendEventJson(theEvent.ToString(), "MySchema");
			}
			else {
				Assert.Fail();
			}

			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, null, "InnerSchema[]");

			env.UndeployAll();
		}

		private static IDictionary<string, object> MakeSchemaEvent(
			object prop,
			string key)
		{
			IDictionary<string, object> theEvent = new Dictionary<string, object>();
			theEvent.Put("prop", prop);
			theEvent.Put("key", key);
			return theEvent;
		}

		[Serializable]
		public class MyLocalJsonProvidedInnerSchema
		{
			public string key;
		}

		[Serializable]
		public class MyLocalJsonProvidedMySchema
		{
			public MyLocalJsonProvidedInnerSchema inside;
			public MyLocalJsonProvidedInnerSchema[] insidearr;
		}

		[Serializable]
		public class MyLocalJsonProvidedOut
		{
			public string t0;
			public string t1;
		}

		[Serializable]
		public class MyLocalJsonProvidedWKey
		{
			public string key;
		}
	}
} // end of namespace
