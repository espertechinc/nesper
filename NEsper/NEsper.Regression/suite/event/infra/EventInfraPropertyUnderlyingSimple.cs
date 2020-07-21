///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyUnderlyingSimple : RegressionExecution
	{
		public static readonly string BEAN_TYPENAME = nameof(SupportBeanSimple);
		public static readonly string XML_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "XML";
		public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Map";
		public static readonly string OA_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "OA";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Avro";
		public static readonly string JSON_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "Json";
		public static readonly string JSONPROVIDEDBEAN_TYPENAME = nameof(EventInfraPropertyUnderlyingSimple) + "JsonWProvided";

		private static readonly ILog log = LogManager.GetLogger(typeof(EventInfraPropertyUnderlyingSimple));

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			var eplJson =
				"@public @buseventtype @name('schema') create json schema " + JSON_TYPENAME + "(myInt int, myString string);\n" +
				"@public @buseventtype @name('schema') @JsonSchema(className='" + nameof(MyLocalJsonProvided) + "') " +
				" create json schema " + JSONPROVIDEDBEAN_TYPENAME + "();\n";
			env.CompileDeploy(eplJson, path);

			var pairs = new Pair<string, FunctionSendEventIntString>[] {
				new Pair<string, FunctionSendEventIntString>(MAP_TYPENAME, FMAP),
				new Pair<string, FunctionSendEventIntString>(OA_TYPENAME, FOA),
				new Pair<string, FunctionSendEventIntString>(BEAN_TYPENAME, FBEAN),
				new Pair<string, FunctionSendEventIntString>(XML_TYPENAME, FXML),
				new Pair<string, FunctionSendEventIntString>(AVRO_TYPENAME, FAVRO),
				new Pair<string, FunctionSendEventIntString>(JSON_TYPENAME, FJSON),
				new Pair<string, FunctionSendEventIntString>(JSONPROVIDEDBEAN_TYPENAME, FJSON)
			};

			foreach (var pair in pairs) {
				log.Info("Asserting type " + pair.First);
				RunAssertionPassUnderlying(env, pair.First, pair.Second, path);
				RunAssertionPropertiesWGetter(env, pair.First, pair.Second, path);
				RunAssertionTypeValidProp(env, pair.First, pair.Second != FBEAN);
				RunAssertionTypeInvalidProp(env, pair.First, pair.Second == FXML);
			}

			env.UndeployAll();
		}

		private void RunAssertionPassUnderlying(
			RegressionEnvironment env,
			string typename,
			FunctionSendEventIntString send,
			RegressionPath path)
		{
			var epl = "@name('s0') select * from " + typename;
			env.CompileDeploy(epl, path).AddListener("s0");

			var fields = "myInt,myString".SplitCsv();

			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(env.Statement("s0").EventType.GetPropertyType("myInt")));
			Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("myString"));

			var eventOne = send.Invoke(typename, env, 3, "some string");

			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			SupportEventTypeAssertionUtil.AssertConsistency(@event);
			AssertUnderlying(typename, eventOne, @event.Underlying);
			EPAssertionUtil.AssertProps(@event, fields, new object[] {3, "some string"});

			var eventTwo = send.Invoke(typename, env, 4, "other string");
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			AssertUnderlying(typename, eventTwo, @event.Underlying);
			EPAssertionUtil.AssertProps(@event, fields, new object[] {4, "other string"});

			env.UndeployModuleContaining("s0");
		}

		private void AssertUnderlying(
			string typename,
			object expected,
			object received)
		{
			if (typename.Equals(JSONPROVIDEDBEAN_TYPENAME)) {
				Assert.IsTrue(received is MyLocalJsonProvided);
			}
			else if (typename.Equals(JSON_TYPENAME)) {
				Assert.AreEqual(expected, received.ToString());
			}
			else {
				Assert.AreEqual(expected, received);
			}
		}

		private void RunAssertionPropertiesWGetter(
			RegressionEnvironment env,
			string typename,
			FunctionSendEventIntString send,
			RegressionPath path)
		{
			var epl = "@name('s0') select myInt, exists(myInt) as exists_myInt, myString, exists(myString) as exists_myString from " + typename;
			env.CompileDeploy(epl, path).AddListener("s0");

			var fields = "myInt,exists_myInt,myString,exists_myString".SplitCsv();

			var eventType = env.Statement("s0").EventType;
			Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType("myInt")));
			Assert.AreEqual(typeof(string), eventType.GetPropertyType("myString"));
			Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_myInt"));
			Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_myString"));

			send.Invoke(typename, env, 3, "some string");

			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			RunAssertionEventInvalidProp(@event);
			EPAssertionUtil.AssertProps(@event, fields, new object[] {3, true, "some string", true});

			send.Invoke(typename, env, 4, "other string");
			@event = env.Listener("s0").AssertOneGetNewAndReset();
			EPAssertionUtil.AssertProps(@event, fields, new object[] {4, true, "other string", true});

			env.UndeployModuleContaining("s0");
		}

		private void RunAssertionEventInvalidProp(EventBean @event)
		{
			foreach (var prop in Arrays.AsList("xxxx", "myString[1]", "myString('a')", "x.y", "myString.x")) {
				SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
				SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
			}
		}

		private void RunAssertionTypeValidProp(
			RegressionEnvironment env,
			string typeName,
			bool boxed)
		{
			var eventType = !typeName.Equals(JSON_TYPENAME)
				? env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName)
				: env.Runtime.EventTypeService.GetEventType(env.DeploymentId("schema"), typeName);

			var expectedType = new object[][] {
				new object[] {"myInt", boxed ? typeof(int?) : typeof(int), null, null},
				new object[] {"myString", typeof(string), null, null}
			};
			SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

			EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"myString", "myInt"}, eventType.PropertyNames);

			Assert.IsNotNull(eventType.GetGetter("myInt"));
			Assert.IsTrue(eventType.IsProperty("myInt"));
			Assert.AreEqual(boxed ? typeof(int?) : typeof(int), eventType.GetPropertyType("myInt"));
			Assert.AreEqual(
				new EventPropertyDescriptor("myString", typeof(string), null, false, false, false, false, false),
				eventType.GetPropertyDescriptor("myString"));
		}

		private void RunAssertionTypeInvalidProp(
			RegressionEnvironment env,
			string typeName,
			bool xml)
		{
			var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

			foreach (var prop in Arrays.AsList("xxxx", "myString[0]", "myString('a')", "myString.x", "myString.x.y", "myString.x")) {
				Assert.AreEqual(false, eventType.IsProperty(prop));
				Type expected = null;
				if (xml) {
					if (prop.Equals("myString[0]")) {
						expected = typeof(string);
					}

					if (prop.Equals("myString.x?")) {
						expected = typeof(XmlNode);
					}
				}

				Assert.AreEqual(expected, eventType.GetPropertyType(prop));
				Assert.IsNull(eventType.GetPropertyDescriptor(prop));
				Assert.IsNull(eventType.GetFragmentType(prop));
			}
		}

		delegate object FunctionSendEventIntString(
			string eventTypeName,
			RegressionEnvironment env,
			int intValue,
			string stringValue);

		private static readonly FunctionSendEventIntString FMAP = (
			eventTypeName,
			env,
			a,
			b) => {
			IDictionary<string, object> map = new Dictionary<string, object>();
			map.Put("myInt", a);
			map.Put("myString", b);
			env.SendEventMap(map, eventTypeName);
			return map;
		};

		private static readonly FunctionSendEventIntString FOA = (
			eventTypeName,
			env,
			a,
			b) => {
			var oa = new object[] {a, b};
			env.SendEventObjectArray(oa, eventTypeName);
			return oa;
		};

		private static readonly FunctionSendEventIntString FBEAN = (
			eventTypeName,
			env,
			a,
			b) => {
			var bean = new SupportBeanSimple(b, a);
			env.SendEventBean(bean);
			return bean;
		};

		private static readonly FunctionSendEventIntString FXML = (
			eventTypeName,
			env,
			a,
			b) => {
			var xml =
				"<myevent myInt=\"XXXXXX\" myString=\"YYYYYY\">\n" +
				"</myevent>\n";
			xml = xml.Replace("XXXXXX", a.ToString());
			xml = xml.Replace("YYYYYY", b);
			var d = SupportXML.SendXMLEvent(env, xml, eventTypeName);
			return d.DocumentElement;
		};

		private static readonly FunctionSendEventIntString FAVRO = (
			eventTypeName,
			env,
			a,
			b) => {
			var avroSchema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME)).AsRecordSchema();
			var datum = new GenericRecord(avroSchema);
			datum.Put("myInt", a);
			datum.Put("myString", b);
			env.SendEventAvro(datum, eventTypeName);
			return datum;
		};

		private static readonly FunctionSendEventIntString FJSON = (
			eventTypeName,
			env,
			a,
			b) => {
			var @object = new JObject();
			@object.Add("myInt", a);
			@object.Add("myString", b);
			var json = @object.ToString();
			env.SendEventJson(json, eventTypeName);
			return json;
		};

		[Serializable]
		public class MyLocalJsonProvided
		{
			public int? myInt;
			public string myString;
		}
	}
} // end of namespace
