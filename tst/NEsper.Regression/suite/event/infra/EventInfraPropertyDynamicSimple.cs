///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyDynamicSimple : RegressionExecution
	{
		public static readonly string XML_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "XML";
		public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Map";
		public static readonly string OA_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "OA";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Avro";
		public static readonly string JSON_TYPENAME = nameof(EventInfraPropertyDynamicSimple) + "Json";

		public bool ExcludeWhenInstrumented()
		{
			return true;
		}

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			// Bean
			var beanTests = new Pair<object, object>[] {
				new Pair<object, object>(new SupportMarkerImplA("e1"), Exists("e1")),
				new Pair<object, object>(new SupportMarkerImplB(1), Exists(1)),
				new Pair<object, object>(new SupportMarkerImplC(), NotExists())
			};
			RunAssertion(env, nameof(SupportMarkerInterface), FBEAN, null, beanTests, typeof(object), path);

			// Map
			var mapTests = new Pair<object, object>[] {
				new Pair<object, object>(Collections.SingletonMap("somekey", "10"), NotExists()),
				new Pair<object, object>(Collections.SingletonMap("Id", "abc"), Exists("abc")),
				new Pair<object, object>(Collections.SingletonMap("Id", 10), Exists(10)),
			};
			RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

			// Object-Array
			var oaTests = new Pair<object, object>[] {
				new Pair<object, object>(new object[] {1, null}, Exists(null)),
				new Pair<object, object>(new object[] {2, "abc"}, Exists("abc")),
				new Pair<object, object>(new object[] {3, 10}, Exists(10)),
			};
			RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

			// XML
			var xmlTests = new Pair<object, object>[] {
				new Pair<object, object>("", NotExists()),
				new Pair<object, object>("<Id>10</Id>", Exists("10")),
				new Pair<object, object>("<Id>abc</Id>", Exists("abc")),
			};
			RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

			// Avro
			var avroSchema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME)).AsRecordSchema();
			var datumEmpty = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
			var datumOne = new GenericRecord(avroSchema);
			datumOne.Put("Id", 101);
			var datumTwo = new GenericRecord(avroSchema);
			datumTwo.Put("Id", null);
			var avroTests = new Pair<object, object>[] {
				new Pair<object, object>(datumEmpty, NotExists()),
				new Pair<object, object>(datumOne, Exists(101)),
				new Pair<object, object>(datumTwo, Exists(null))
			};
			RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path);

			// Json
			env.CompileDeploy("@JsonSchema(Dynamic=true) @public @buseventtype create json schema " + JSON_TYPENAME + "()", path);
			var jsonTests = new Pair<object, object>[] {
				new Pair<object, object>("{}", NotExists()),
				new Pair<object, object>("{\"Id\": 10}", Exists(10)),
				new Pair<object, object>("{\"Id\": \"abc\"}", Exists("abc"))
			};
			RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent send,
			Func<object, object> optionalValueConversion,
			Pair<object, object>[] tests,
			Type expectedPropertyType,
			RegressionPath path)
		{

			var stmtText = "@Name('s0') select Id? as myid, exists(Id?) as exists_myid from " + typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			Assert.AreEqual(expectedPropertyType, env.Statement("s0").EventType.GetPropertyType("myid"));
			Assert.AreEqual(typeof(bool?), env.Statement("s0").EventType.GetPropertyType("exists_myid"));

			foreach (var pair in tests) {
				send.Invoke(env, pair.First, typename);
				var @event = env.Listener("s0").AssertOneGetNewAndReset();
				SupportEventInfra.AssertValueMayConvert(@event, "myid", (ValueWithExistsFlag) pair.Second, optionalValueConversion);
			}

			env.UndeployAll();
		}

		private void AddMapEventType(RegressionEnvironment env)
		{
		}

		private void AddOAEventType(RegressionEnvironment env)
		{
		}

		private void AddAvroEventType(RegressionEnvironment env)
		{
		}
	}
} // end of namespace
