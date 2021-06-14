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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyNestedSimple : RegressionExecution
	{
		public static readonly string XML_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "XML";
		public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Map";
		public static readonly string OA_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "OA";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Avro";
		private static readonly string BEAN_TYPENAME = nameof(InfraNestedSimplePropTop);
		private static readonly string JSON_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Json";
		private static readonly string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "JsonProvided";

		public void Run(RegressionEnvironment env)
		{
			var path = new RegressionPath();

			RunAssertion(env, BEAN_TYPENAME, FBEAN, typeof(InfraNestedSimplePropLvl1), typeof(InfraNestedSimplePropLvl1).FullName, path);
			RunAssertion(env, MAP_TYPENAME, FMAP, typeof(IDictionary<string, object>), MAP_TYPENAME + "_1", path);
			RunAssertion(env, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1", path);
			RunAssertion(env, XML_TYPENAME, FXML, typeof(XmlNode), XML_TYPENAME + ".L1", path);
			RunAssertion(env, AVRO_TYPENAME, FAVRO, typeof(GenericRecord), AVRO_TYPENAME + "_1", path);

			var epl =
				"create json schema " + JSON_TYPENAME + "_4(Lvl4 int);\n" +
				"create json schema " + JSON_TYPENAME + "_3(Lvl3 int, L4 " + JSON_TYPENAME + "_4);\n" +
				"create json schema " + JSON_TYPENAME + "_2(Lvl2 int, L3 " + JSON_TYPENAME + "_3);\n" +
				"create json schema " + JSON_TYPENAME + "_1(Lvl1 int, L2 " + JSON_TYPENAME + "_2);\n" +
				"@Name('types') @public @buseventtype " +
				"create json schema " + JSON_TYPENAME + "(L1 " + JSON_TYPENAME + "_1);\n";

			env.CompileDeploy(epl, path);
			var nestedClass = SupportJsonEventTypeUtil.GetUnderlyingType(env, "types", JSON_TYPENAME + "_1");
			RunAssertion(env, JSON_TYPENAME, FJSON, nestedClass, JSON_TYPENAME + "_1", path);
			
			epl = $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedTop).MaskTypeName()}') @name('types') @public @buseventtype create json schema {JSONPROVIDED_TYPENAME}();\n";
			env.CompileDeploy(epl, path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, typeof(MyLocalJSONProvidedLvl1), typeof(MyLocalJSONProvidedLvl1).FullName, path);

			env.UndeployAll();
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent4Int send,
			Type nestedClass,
			string fragmentTypeName,
			RegressionPath path)
		{
			RunAssertionSelectNested(env, typename, send, path);
			RunAssertionBeanNav(env, typename, send, path);
			RunAssertionTypeValidProp(env, typename, send, nestedClass, fragmentTypeName);
			RunAssertionTypeInvalidProp(env, typename);
		}

		private void RunAssertionBeanNav(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent4Int send,
			RegressionPath path)
		{
			var epl = "@Name('s0') select * from " + typename;
			env.CompileDeploy(epl, path).AddListener("s0");

			send.Invoke(typename, env, 1, 2, 3, 4);
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			EPAssertionUtil.AssertProps(@event, "L1.Lvl1,L1.L2.Lvl2,L1.L2.L3.Lvl3,L1.L2.L3.L4.Lvl4".SplitCsv(), new object[] {1, 2, 3, 4});
			SupportEventTypeAssertionUtil.AssertConsistency(@event);
			var nativeFragment = typename.Equals(BEAN_TYPENAME) || typename.Equals(JSONPROVIDED_TYPENAME);
			SupportEventTypeAssertionUtil.AssertFragments(@event, nativeFragment, false, "L1.L2");
			SupportEventTypeAssertionUtil.AssertFragments(@event, nativeFragment, false, "L1,L1.L2,L1.L2.L3,L1.L2.L3.L4");
			RunAssertionEventInvalidProp(@event);

			env.UndeployModuleContaining("s0");
		}

		private void RunAssertionSelectNested(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent4Int send,
			RegressionPath path)
		{
			var epl =
				"@Name('s0') select " +
				"L1.Lvl1 as c0, " +
				"exists(L1.Lvl1) as exists_c0, " +
				"L1.L2.Lvl2 as c1, " +
				"exists(L1.L2.Lvl2) as exists_c1, " +
				"L1.L2.L3.Lvl3 as c2, " +
				"exists(L1.L2.L3.Lvl3) as exists_c2, " +
				"L1.L2.L3.L4.Lvl4 as c3, " +
				"exists(L1.L2.L3.L4.Lvl4) as exists_c3 " +
				"from " +
				typename;
			
			env.CompileDeploy(epl, path).AddListener("s0");
			var fields = "c0,exists_c0,c1,exists_c1,c2,exists_c2,c3,exists_c3".SplitCsv();

			var eventType = env.Statement("s0").EventType;
			foreach (var property in fields) {
				Assert.AreEqual(property.StartsWith("exists") ? typeof(bool?) : typeof(int?), eventType.GetPropertyType(property).GetBoxedType());
			}

			send.Invoke(typename, env, 1, 2, 3, 4);
			var @event = env.Listener("s0").AssertOneGetNewAndReset();
			EPAssertionUtil.AssertProps(@event, fields, new object[] {1, true, 2, true, 3, true, 4, true});
			SupportEventTypeAssertionUtil.AssertConsistency(@event);

			send.Invoke(typename, env, 10, 5, 50, 400);
			EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, new object[] {10, true, 5, true, 50, true, 400, true});

			env.UndeployModuleContaining("s0");
		}

		private void RunAssertionEventInvalidProp(EventBean @event)
		{
			foreach (var prop in Arrays.AsList("L2", "L1.L3", "L1.xxx", "L1.L2.x", "L1.L2.L3.x", "L1.Lvl1.x")) {
				SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
				SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
			}
		}

		private void RunAssertionTypeValidProp(
			RegressionEnvironment env,
			string typeName,
			FunctionSendEvent4Int send,
			Type nestedClass,
			string fragmentTypeName)
		{
			var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

			var expectedType = new object[][] {
				new object[] {"L1", nestedClass, fragmentTypeName, false}
			};
			SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

			EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"L1"}, eventType.PropertyNames);

			foreach (var prop in Arrays.AsList("L1", "L1.Lvl1", "L1.L2", "L1.L2.Lvl2")) {
				Assert.IsNotNull(eventType.GetGetter(prop));
				Assert.IsTrue(eventType.IsProperty(prop));
			}

			Assert.AreEqual(nestedClass, eventType.GetPropertyType("L1"));
			foreach (var prop in Arrays.AsList("L1.Lvl1", "L1.L2.Lvl2", "L1.L2.L3.Lvl3")) {
				Assert.AreEqual(typeof(int?), eventType.GetPropertyType(prop).GetBoxedType());
			}

			var lvl1Fragment = eventType.GetFragmentType("L1");
			Assert.IsFalse(lvl1Fragment.IsIndexed);
			var isNative = typeName.Equals(BEAN_TYPENAME) || typeName.Equals(JSONPROVIDED_TYPENAME);
			Assert.AreEqual(isNative, lvl1Fragment.IsNative);
			Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

			var lvl2Fragment = eventType.GetFragmentType("L1.L2");
			Assert.IsFalse(lvl2Fragment.IsIndexed);
			Assert.AreEqual(isNative, lvl2Fragment.IsNative);

			Assert.AreEqual(new EventPropertyDescriptor("L1", nestedClass, null, false, false, false, false, true), eventType.GetPropertyDescriptor("L1"));
		}

		private void RunAssertionTypeInvalidProp(
			RegressionEnvironment env,
			string typeName)
		{
			var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

			foreach (var prop in Arrays.AsList("L2", "L1.L3", "L1.Lvl1.Lvl1", "L1.L2.L4", "L1.L2.xx", "L1.L2.L3.Lvl5")) {
				Assert.AreEqual(false, eventType.IsProperty(prop));
				Assert.AreEqual(null, eventType.GetPropertyType(prop));
				Assert.IsNull(eventType.GetPropertyDescriptor(prop));
			}
		}

		private static readonly FunctionSendEvent4Int FMAP = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var l4 = Collections.SingletonDataMap("Lvl4", lvl4);
			var l3 = TwoEntryMap<string, object>("L4", l4, "Lvl3", lvl3);
			var l2 = TwoEntryMap<string, object>("L3", l3, "Lvl2", lvl2);
			var l1 = TwoEntryMap<string, object>("L2", l2, "Lvl1", lvl1);
			var top = Collections.SingletonDataMap("L1", l1);
			env.SendEventMap(top, eventTypeName);
		};

		private static readonly FunctionSendEvent4Int FOA = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var l4 = new object[] {lvl4};
			var l3 = new object[] {l4, lvl3};
			var l2 = new object[] {l3, lvl2};
			var l1 = new object[] {l2, lvl1};
			var top = new object[] {l1};
			env.SendEventObjectArray(top, eventTypeName);
		};

		private static readonly FunctionSendEvent4Int FBEAN = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var l4 = new InfraNestedSimplePropLvl4(lvl4);
			var l3 = new InfraNestedSimplePropLvl3(l4, lvl3);
			var l2 = new InfraNestedSimplePropLvl2(l3, lvl2);
			var l1 = new InfraNestedSimplePropLvl1(l2, lvl1);
			var top = new InfraNestedSimplePropTop(l1);
			env.SendEventBean(top);
		};

		private static readonly FunctionSendEvent4Int FXML = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
			          "<myevent>\n" +
			          "\t<L1 Lvl1=\"${lvl1}\">\n" +
			          "\t\t<L2 Lvl2=\"${lvl2}\">\n" +
			          "\t\t\t<L3 Lvl3=\"${lvl3}\">\n" +
			          "\t\t\t\t<L4 Lvl4=\"${lvl4}\">\n" +
			          "\t\t\t\t</L4>\n" +
			          "\t\t\t</L3>\n" +
			          "\t\t</L2>\n" +
			          "\t</L1>\n" +
			          "</myevent>";
			xml = xml.Replace("${lvl1}", Convert.ToString(lvl1));
			xml = xml.Replace("${lvl2}", Convert.ToString(lvl2));
			xml = xml.Replace("${lvl3}", Convert.ToString(lvl3));
			xml = xml.Replace("${lvl4}", Convert.ToString(lvl4));
			SupportXML.SendXMLEvent(env, xml, eventTypeName);
		};

		private static readonly FunctionSendEvent4Int FAVRO = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var schema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME)).AsRecordSchema();
			var lvl1Schema = schema.GetField("L1").Schema.AsRecordSchema();
			var lvl2Schema = lvl1Schema.GetField("L2").Schema.AsRecordSchema();
			var lvl3Schema = lvl2Schema.GetField("L3").Schema.AsRecordSchema();
			var lvl4Schema = lvl3Schema.GetField("L4").Schema.AsRecordSchema();
			var lvl4Rec = new GenericRecord(lvl4Schema);
			lvl4Rec.Put("Lvl4", lvl4);
			var lvl3Rec = new GenericRecord(lvl3Schema);
			lvl3Rec.Put("L4", lvl4Rec);
			lvl3Rec.Put("Lvl3", lvl3);
			var lvl2Rec = new GenericRecord(lvl2Schema);
			lvl2Rec.Put("L3", lvl3Rec);
			lvl2Rec.Put("Lvl2", lvl2);
			var lvl1Rec = new GenericRecord(lvl1Schema);
			lvl1Rec.Put("L2", lvl2Rec);
			lvl1Rec.Put("Lvl1", lvl1);
			var datum = new GenericRecord(schema);
			datum.Put("L1", lvl1Rec);
			env.SendEventAvro(datum, eventTypeName);
		};

		private static readonly FunctionSendEvent4Int FJSON = (
			eventTypeName,
			env,
			lvl1,
			lvl2,
			lvl3,
			lvl4) => {
			var json = "{\n" +
			           "  \"L1\": {\n" +
			           "    \"Lvl1\": ${lvl1},\n" +
			           "    \"L2\": {\n" +
			           "      \"Lvl2\": ${lvl2},\n" +
			           "      \"L3\": {\n" +
			           "        \"Lvl3\": ${lvl3},\n" +
			           "        \"L4\": {\n" +
			           "          \"Lvl4\": ${lvl4}\n" +
			           "        }\n" +
			           "      }\n" +
			           "    }\n" +
			           "  }\n" +
			           "}";
			json = json.Replace("${lvl1}", Convert.ToString(lvl1));
			json = json.Replace("${lvl2}", Convert.ToString(lvl2));
			json = json.Replace("${lvl3}", Convert.ToString(lvl3));
			json = json.Replace("${lvl4}", Convert.ToString(lvl4));
			env.SendEventJson(json, eventTypeName);
		};

		public delegate void FunctionSendEvent4Int(
			string eventTypeName,
			RegressionEnvironment env,
			int lvl1,
			int lvl2,
			int lvl3,
			int lvl4);

		public class InfraNestedSimplePropTop
		{
			public InfraNestedSimplePropTop(InfraNestedSimplePropLvl1 l1)
			{
				this.L1 = l1;
			}

			public InfraNestedSimplePropLvl1 L1 { get; }
		}

		public class InfraNestedSimplePropLvl1
		{
			public InfraNestedSimplePropLvl1(
				InfraNestedSimplePropLvl2 l2,
				int lvl1)
			{
				this.L2 = l2;
				this.Lvl1 = lvl1;
			}

			public InfraNestedSimplePropLvl2 L2 { get; }

			public int Lvl1 { get; }
		}

		public class InfraNestedSimplePropLvl2
		{
			public InfraNestedSimplePropLvl2(
				InfraNestedSimplePropLvl3 l3,
				int lvl2)
			{
				this.L3 = l3;
				this.Lvl2 = lvl2;
			}

			public InfraNestedSimplePropLvl3 L3 { get; }

			public int Lvl2 { get; }
		}

		public class InfraNestedSimplePropLvl3
		{
			public InfraNestedSimplePropLvl3(
				InfraNestedSimplePropLvl4 l4,
				int lvl3)
			{
				this.L4 = l4;
				this.Lvl3 = lvl3;
			}

			public InfraNestedSimplePropLvl4 L4 { get; }

			public int Lvl3 { get; }
		}

		public class InfraNestedSimplePropLvl4
		{
			public InfraNestedSimplePropLvl4(int lvl4)
			{
				this.Lvl4 = lvl4;
			}

			public int Lvl4 { get; }
		}

		[Serializable]
		public class MyLocalJSONProvidedTop
		{
			public MyLocalJSONProvidedLvl1 L1;
		}

		public class MyLocalJSONProvidedLvl1
		{
			public MyLocalJSONProvidedLvl2 L2;
			public int Lvl1;
		}

		public class MyLocalJSONProvidedLvl2
		{
			public MyLocalJSONProvidedLvl3 L3;
			public int Lvl2;
		}

		public class MyLocalJSONProvidedLvl3
		{
			public MyLocalJSONProvidedLvl4 L4;
			public int Lvl3;
		}

		public class MyLocalJSONProvidedLvl4
		{
			public int Lvl4;
		}
	}
} // end of namespace
