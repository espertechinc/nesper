///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static NEsper.Avro.Extensions.TypeBuilder;

using JObject = Newtonsoft.Json.Linq.JObject;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
	public class EPLInsertIntoPopulateCreateStream : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
				RunAssertionCreateStream(env, rep);
			}

			foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
				RunAssertionCreateStreamTwo(env, rep);
			}

			foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
				RunAssertPopulateFromNamedWindow(env, rep);
			}

			RunAssertionObjectArrPropertyReorder(env);
		}

		private static void RunAssertionObjectArrPropertyReorder(RegressionEnvironment env)
		{
			var epl = "create objectarray schema MyInner (p_inner string);\n" +
			          "create objectarray schema MyOATarget (unfilled string, p0 string, p1 string, i0 MyInner);\n" +
			          "create objectarray schema MyOASource (p0 string, p1 string, i0 MyInner);\n" +
			          "insert into MyOATarget select p0, p1, i0, null as unfilled from MyOASource;\n" +
			          "@name('s0') select * from MyOATarget;\n";
			env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

			env.SendEventObjectArray(new object[] {"p0value", "p1value", new object[] {"i"}}, "MyOASource");
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "p0,p1".SplitCsv(), new object[] {"p0value", "p1value"});

			env.UndeployAll();
		}

		private static void RunAssertPopulateFromNamedWindow(
			RegressionEnvironment env,
			EventRepresentationChoice type)
		{
			var path = new RegressionPath();
			var schemaEPL = type.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNode>() + "create schema Node(nid string)";
			env.CompileDeployWBusPublicType(schemaEPL, path);

			env.CompileDeploy("create window NodeWindow#unique(nid) as Node", path);
			env.CompileDeploy("insert into NodeWindow select * from Node", path);
			env.CompileDeploy(
				type.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNodePlus>() + "create schema NodePlus(npid string, node Node)",
				path);
			env.CompileDeploy("@name('s0') insert into NodePlus select 'E1' as npid, n1 as node from NodeWindow n1", path).AddListener("s0");

			if (type.IsObjectArrayEvent()) {
				env.SendEventObjectArray(new object[] {"n1"}, "Node");
			}
			else if (type.IsMapEvent()) {
				env.SendEventMap(Collections.SingletonDataMap("nid", "n1"), "Node");
			}
			else if (type.IsAvroEvent()) {
				var genericRecord = new GenericRecord(SchemaBuilder.Record("name", RequiredString("nid")));
				genericRecord.Put("nid", "n1");
				env.SendEventAvro(genericRecord, "Node");
			}
			else if (type.IsJsonEvent() || type.IsJsonProvidedClassEvent()) {
				var @object = new JObject();
				@object.Add("nid", "n1");
				env.SendEventJson(@object.ToString(), "Node");
			}
			else {
				Assert.Fail();
			}

			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			Assert.AreEqual("E1", @event.Get("npid"));
			Assert.AreEqual("n1", @event.Get("node.nid"));
			var fragment = (EventBean) @event.GetFragment("node");
			Assert.AreEqual("Node", fragment.EventType.Name);

			env.UndeployAll();
		}

		private static void RunAssertionCreateStream(
			RegressionEnvironment env,
			EventRepresentationChoice representation)
		{
			var epl = representation.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEvent>() +
			          " create schema MyEvent(myId int);\n" +
			          representation.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedCompositeEvent>() +
			          " create schema CompositeEvent(c1 MyEvent, c2 MyEvent, rule string);\n" +
			          "insert into MyStream select c, 'additionalValue' as value from MyEvent c;\n" +
			          "insert into CompositeEvent select e1.c as c1, e2.c as c2, '4' as rule " +
			          "  from pattern [e1=MyStream -> e2=MyStream];\n" +
			          representation.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedCompositeEvent>() +
			          " @Name('Target') select * from CompositeEvent;\n";
			env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("Target");

			if (representation.IsObjectArrayEvent()) {
				env.SendEventObjectArray(MakeEvent(10).Values.ToArray(), "MyEvent");
				env.SendEventObjectArray(MakeEvent(11).Values.ToArray(), "MyEvent");
			}
			else if (representation.IsMapEvent()) {
				env.SendEventMap(MakeEvent(10), "MyEvent");
				env.SendEventMap(MakeEvent(11), "MyEvent");
			}
			else if (representation.IsAvroEvent()) {
				env.SendEventAvro(MakeEventAvro(10), "MyEvent");
				env.SendEventAvro(MakeEventAvro(11), "MyEvent");
			}
			else if (representation.IsJsonEvent() || representation.IsJsonProvidedClassEvent()) {
				env.SendEventJson("{\"myId\": 10}", "MyEvent");
				env.SendEventJson("{\"myId\": 11}", "MyEvent");
			}
			else {
				Assert.Fail();
			}

			var theEvent = env.Listener("Target").AssertOneGetNewAndReset();
			Assert.AreEqual(10, theEvent.Get("c1.myId"));
			Assert.AreEqual(11, theEvent.Get("c2.myId"));
			Assert.AreEqual("4", theEvent.Get("rule"));

			env.UndeployAll();
		}

		private static void RunAssertionCreateStreamTwo(
			RegressionEnvironment env,
			EventRepresentationChoice eventRepresentationEnum)
		{
			var path = new RegressionPath();
			var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedMyEvent>() +
			          " create schema MyEvent(myId int)\n;" +
			          eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedAllMyEvent>() +
			          " create schema AllMyEvent as (myEvent MyEvent, clazz String, reverse boolean);\n" +
			          eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedSuspectMyEvent>() +
			          " create schema SuspectMyEvent as (myEvent MyEvent, clazz String);\n";
			env.CompileDeployWBusPublicType(epl, path);

			env.CompileDeploy(
					"@name('s0') insert into AllMyEvent " +
					"select c as myEvent, 'test' as clazz, false as reverse " +
					"from MyEvent(myId=1) c",
					path)
				.AddListener("s0");

			Assert.IsTrue(eventRepresentationEnum.MatchesClass(env.Statement("s0").EventType.UnderlyingType));

			env.CompileDeploy(
					"@name('s1') insert into SuspectMyEvent " +
					"select c.myEvent as myEvent, clazz " +
					"from AllMyEvent(not reverse) c",
					path)
				.AddListener("s1");

			if (eventRepresentationEnum.IsObjectArrayEvent()) {
				env.SendEventObjectArray(MakeEvent(1).Values.ToArray(), "MyEvent");
			}
			else if (eventRepresentationEnum.IsMapEvent()) {
				env.SendEventMap(MakeEvent(1), "MyEvent");
			}
			else if (eventRepresentationEnum.IsAvroEvent()) {
				env.SendEventAvro(MakeEventAvro(1), "MyEvent");
			}
			else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
				env.SendEventJson("{\"myId\": 1}", "MyEvent");
			}
			else {
				Assert.Fail();
			}

			AssertCreateStreamTwo(eventRepresentationEnum, env.Listener("s0").AssertOneGetNewAndReset(), env.Statement("s0"));
			AssertCreateStreamTwo(eventRepresentationEnum, env.Listener("s1").AssertOneGetNewAndReset(), env.Statement("s1"));

			env.UndeployAll();
		}

		private static void AssertCreateStreamTwo(
			EventRepresentationChoice eventRepresentationEnum,
			EventBean eventBean,
			EPStatement statement)
		{
			if (eventRepresentationEnum.IsAvroOrJsonEvent()) {
				Assert.AreEqual(1, eventBean.Get("myEvent.myId"));
			}
			else {
				Assert.IsTrue(eventBean.Get("myEvent") is EventBean);
				Assert.AreEqual(1, ((EventBean) eventBean.Get("myEvent")).Get("myId"));
			}

			Assert.IsNotNull(statement.EventType.GetFragmentType("myEvent"));
		}

		private static IDictionary<string, object> MakeEvent(int myId)
		{
			return Collections.SingletonDataMap("myId", myId);
		}

		private static GenericRecord MakeEventAvro(int myId)
		{
			var schema = SchemaBuilder.Record("schema", RequiredInt("myId"));
			var record = new GenericRecord(schema);
			record.Put("myId", myId);
			return record;
		}

		[Serializable]
		public class MyLocalJsonProvidedMyEvent
		{
			public int? myId;
		}

		[Serializable]
		public class MyLocalJsonProvidedCompositeEvent
		{
			public MyLocalJsonProvidedMyEvent c1;
			public MyLocalJsonProvidedMyEvent c2;
			public string rule;
		}

		[Serializable]
		public class MyLocalJsonProvidedAllMyEvent
		{
			public MyLocalJsonProvidedMyEvent myEvent;
			public string clazz;
			public bool reverse;
		}

		[Serializable]
		public class MyLocalJsonProvidedSuspectMyEvent
		{
			public MyLocalJsonProvidedMyEvent myEvent;
			public string clazz;
		}

		[Serializable]
		public class MyLocalJsonProvidedNode
		{
			public string nid;
		}

		[Serializable]
		public class MyLocalJsonProvidedNodePlus
		{
			public string npid;
			public MyLocalJsonProvidedNode node;
		}
	}
} // end of namespace
