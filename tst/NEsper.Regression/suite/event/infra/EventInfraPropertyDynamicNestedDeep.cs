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

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyDynamicNestedDeep : RegressionExecution
	{
		public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "XML";
		public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "Map";
		public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "OA";
		public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "Avro";
		public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "Json";
		public const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "JsonProvided";

		public ISet<RegressionFlag> Flags()
		{
			return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
		}

		public void Run(RegressionEnvironment env)
		{

			var notExists = MultipleNotExists(6);
			var path = new RegressionPath();

			// Bean
			var beanOne = SupportBeanComplexProps.MakeDefaultBean();
			var n1v = beanOne.Nested.NestedValue;
			var n1nv = beanOne.Nested.NestedNested.NestedNestedValue;
			var beanTwo = SupportBeanComplexProps.MakeDefaultBean();
			beanTwo.Nested.NestedValue = "nested1";
			beanTwo.Nested.NestedNested.NestedNestedValue = "nested2";
			var beanTests = new Pair<object, object>[] {
				new Pair<object, object>(new SupportBeanDynRoot(beanOne), AllExist(n1v, n1v, n1nv, n1nv, n1nv, n1nv)),
				new Pair<object, object>(
					new SupportBeanDynRoot(beanTwo),
					AllExist("nested1", "nested1", "nested2", "nested2", "nested2", "nested2")),
				new Pair<object, object>(new SupportBeanDynRoot("abc"), notExists)
			};
			RunAssertion(env, "SupportBeanDynRoot", FBEAN, null, beanTests, typeof(object), path);

			// Map
			IDictionary<string, object> mapOneL2 = new Dictionary<string, object>();
			mapOneL2.Put("NestedNestedValue", 101);
			IDictionary<string, object> mapOneL1 = new Dictionary<string, object>();
			mapOneL1.Put("NestedNested", mapOneL2);
			mapOneL1.Put("NestedValue", 100);
			IDictionary<string, object> mapOneL0 = new Dictionary<string, object>();
			mapOneL0.Put("Nested", mapOneL1);
			var mapOne = Collections.SingletonDataMap("Item", mapOneL0);
			var mapTests = new Pair<object, object>[] {
				new Pair<object, object>(mapOne, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object, object>(Collections.EmptyDataMap, notExists)
			};
			RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

			// Object-Array
			var oaOneL2 = new object[] { 101 };
			var oaOneL1 = new object[] { oaOneL2, 100 };
			var oaOneL0 = new object[] { oaOneL1 };
			var oaOne = new object[] { oaOneL0 };
			var oaTests = new Pair<object, object>[] {
				new Pair<object, object>(oaOne, AllExist(100, 100, 101, 101, 101, 101)),
				new Pair<object, object>(new object[] { null }, notExists),
			};
			RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

			// XML
			var xmlTests = new Pair<object, object>[] {
				new Pair<object, object>(
                    "<Item>\n" +
                    "\t<Nested NestedValue=\"100\">\n" +
                    "\t\t<NestedNested NestedNestedValue=\"101\">\n" +
                    "\t\t</NestedNested>\n" +
                    "\t</Nested>\n" +
                    "</Item>\n",
					AllExist("100", "100", "101", "101", "101", "101")),
				new Pair<object, object>("<item/>", notExists),
			};
			RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

			// Avro
			var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME);
			var nestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
					schema
						.GetField("Item")
						.Schema
						.AsRecordSchema()
						.GetField("Nested")
						.Schema);
			var nestedNestedSchema = AvroSchemaUtil.FindUnionRecordSchemaSingle(
				nestedSchema.GetField("NestedNested").Schema);
			var nestedNestedDatum = new GenericRecord(nestedNestedSchema.AsRecordSchema());
			nestedNestedDatum.Put("NestedNestedValue", 101);
			var nestedDatum = new GenericRecord(nestedSchema.AsRecordSchema());
			nestedDatum.Put("NestedValue", 100);
			nestedDatum.Put("NestedNested", nestedNestedDatum);
			var avroTests = new Pair<object, object>[] {
				new Pair<object, object>(nestedDatum, AllExist(100, 100, 101, 101, 101, 101)),
				new Pair<object, object>(new GenericRecord(schema.AsRecordSchema()), notExists),
			};
			env.AssertThat(() => RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path));

			// Json
			var jsonTests = new Pair<object, object>[] {
				new Pair<object, object>(
					"{\n" +
					"  \"Item\": {\n" +
					"    \"Nested\": {\n" +
					"      \"NestedValue\": 100,\n" +
					"      \"NestedNested\": {\n" +
					"        \"NestedNestedValue\": 101\n" +
					"      }\n" +
					"    }\n" +
					"  }\n" +
					"}",
					AllExist(100, 100, 101, 101, 101, 101)),
				new Pair<object, object>(
					"{\n" +
					"  \"Item\": {\n" +
					"    \"Nested\": {\n" +
					"      }\n" +
					"    }\n" +
					"  }\n",
					notExists),
				new Pair<object, object>("{ \"Item\": {}}", notExists),
				new Pair<object, object>("{}", notExists)
			};
			var schemas =
				"@JsonSchema(dynamic=true) create json schema Item();\n" +
			    "@public @buseventtype @name('schema') " +
				"create json schema " + JSON_TYPENAME + "(Item Item)";
			env.CompileDeploy(schemas, path);
			RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);

			// Json-Provided
			var jsonProvidedTests = new Pair<object, object>[] {
				new Pair<object, object>(
					"{\n" +
					"  \"Item\": {\n" +
					"    \"Nested\": {\n" +
					"      \"NestedValue\": 100,\n" +
					"      \"NestedNested\": {\n" +
					"        \"NestedNestedValue\": 101\n" +
					"      }\n" +
					"    }\n" +
					"  }\n" +
					"}",
					AllExist(100, 100, 101, 101, 101, 101)),
				new Pair<object, object>(
					"{\n" +
					"  \"Item\": {\n" +
					"    \"Nested\": {\n" +
					"      }\n" +
					"    }\n" +
					"  }\n",
					new ValueWithExistsFlag[]
					{
						Exists(null),
						Exists(null),
						NotExists(),
						NotExists(),
						NotExists(),
						NotExists()
					}),
				new Pair<object, object>("{ \"Item\": {}}", notExists),
				new Pair<object, object>("{}", notExists)
			};
			var schemasJsonProvided =
				"@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype @name('schema') " +
				"create json schema " + JSONPROVIDED_TYPENAME + "()";
			env.CompileDeploy(schemasJsonProvided, path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, null, jsonProvidedTests, typeof(object), path);
		}

		private void RunAssertion(
			RegressionEnvironment env,
			string typename,
			SupportEventInfra.FunctionSendEvent send,
			Func<object, object> optionalValueConversion,
			Pair<object, object>[] tests,
			Type expectedPropertyType,
			RegressionPath path)
		{
			RunAssertionSelectNested(env, typename, send, optionalValueConversion, tests, expectedPropertyType, path);
			RunAssertionBeanNav(env, typename, send, tests[0].First, path);
			env.UndeployAll();
		}

		private void RunAssertionBeanNav(
			RegressionEnvironment env,
			string typename,
			SupportEventInfra.FunctionSendEvent send,
			object underlyingComplete,
			RegressionPath path)
		{
			var stmtText = "@name('s0') select * from " + typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			send.Invoke(env, underlyingComplete, typename);
			env.AssertEventNew("s0", SupportEventTypeAssertionUtil.AssertConsistency);

			env.UndeployModuleContaining("s0");
		}

		private void RunAssertionSelectNested(
			RegressionEnvironment env,
			string typename,
			FunctionSendEvent send,
			Func<object, object> optionalValueConversion,
			Pair<object, object>[] tests,
			Type expectedPropertyType,
			RegressionPath path)
		{

			var stmtText = "@name('s0') select " +
                           " Item.Nested?.NestedValue as n1, " +
                           " exists(Item.Nested?.NestedValue) as exists_n1, " +
                           " Item.Nested?.NestedValue? as n2, " +
                           " exists(Item.Nested?.NestedValue?) as exists_n2, " +
                           " Item.Nested?.NestedNested.NestedNestedValue as n3, " +
                           " exists(Item.Nested?.NestedNested.NestedNestedValue) as exists_n3, " +
                           " Item.Nested?.NestedNested?.NestedNestedValue as n4, " +
                           " exists(Item.Nested?.NestedNested?.NestedNestedValue) as exists_n4, " +
                           " Item.Nested?.NestedNested.NestedNestedValue? as n5, " +
                           " exists(Item.Nested?.NestedNested.NestedNestedValue?) as exists_n5, " +
                           " Item.Nested?.NestedNested?.NestedNestedValue? as n6, " +
                           " exists(Item.Nested?.NestedNested?.NestedNestedValue?) as exists_n6 " +
			               " from " +
			               typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			var propertyNames = "n1,n2,n3,n4,n5,n6".SplitCsv();
			env.AssertStatement(
				"s0",
				statement => {
					var eventType = statement.EventType;
					foreach (var propertyName in propertyNames) {
						Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType(propertyName));
						Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_" + propertyName));
					}
				});

			foreach (var pair in tests) {
				send.Invoke(env, pair.First, typename);
				env.AssertEventNew(
					"s0",
					@event => SupportEventInfra.AssertValuesMayConvert(
						@event,
						propertyNames,
						(ValueWithExistsFlag[])pair.Second,
						optionalValueConversion));
			}

			env.UndeployModuleContaining("s0");
		}

		private static readonly SupportEventInfra.FunctionSendEvent FAVRO = (
			env,
			value,
			typename) => {
			var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME);
			var valueRecord = (GenericRecord)value;
			GenericRecord @event;
			if (valueRecord.Schema.Equals(schema)) {
				@event = valueRecord;
			}
			else {
				var itemSchema = schema.GetField("item").Schema;
				var itemDatum = new GenericRecord(itemSchema.AsRecordSchema());
				itemDatum.Put("nested", value);
				@event = new GenericRecord(schema.AsRecordSchema());
				@event.Put("item", itemDatum);
			}

			env.SendEventAvro(@event, typename);
		};

		[Serializable]
		public class MyLocalJsonProvided
		{
			public MyLocalJsonItem item;
		}

		[Serializable]
		public class MyLocalJsonItem
		{
			public MyLocalJsonProvidedNested nested;
		}

		[Serializable]
		public class MyLocalJsonProvidedNested
		{
			public int? nestedValue;
			public MyLocalJsonProvidedNestedNested nestedNested;
		}

		[Serializable]
		public class MyLocalJsonProvidedNestedNested
		{
			public int? nestedNestedValue;
		}
	}
} // end of namespace
