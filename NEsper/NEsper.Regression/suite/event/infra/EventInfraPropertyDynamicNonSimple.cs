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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyDynamicNonSimple : RegressionExecution
	{

		public static readonly string XML_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "XML";
		public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "Map";
		public static readonly string OA_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "OA";
		public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "Avro";
		public static readonly string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "Json";
		public static readonly string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyDynamicNonSimple) + "JsonProvided";

		public bool ExcludeWhenInstrumented()
		{
			return true;
		}

		public void Run(RegressionEnvironment env)
		{
			var notExists = MultipleNotExists(4);
			var path = new RegressionPath();

			// Bean
			var bean = SupportBeanComplexProps.MakeDefaultBean();
			var beanTests = new Pair<object, object>[] {
				new Pair<object, object>(
					bean,
					AllExist(bean.GetIndexed(0), bean.GetIndexed(1), bean.GetMapped("keyOne"), bean.GetMapped("keyTwo")))
			};
			RunAssertion(env, nameof(SupportBeanComplexProps), FBEAN, null, beanTests, typeof(object), path);

			// Map
			var mapTests = new Pair<object, object>[] {
				new Pair<object, object>(Collections.SingletonMap("somekey", "10"), notExists),
				new Pair<object, object>(
					TwoEntryMap<string, object>(
						"indexed",
						new int[] {1, 2},
						"mapped",
						TwoEntryMap<string, object>("keyOne", 3, "keyTwo", 4)),
					AllExist(1, 2, 3, 4)),
			};
			RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

			// Object-Array
			var oaTests = new Pair<object, object>[] {
				new Pair<object, object>(new object[] {null, null}, notExists),
				new Pair<object, object>(new object[] {new int[] {1, 2}, TwoEntryMap("keyOne", 3, "keyTwo", 4)}, AllExist(1, 2, 3, 4)),
			};
			RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

			// XML
			var xmlTests = new Pair<object, object>[] {
				new Pair<object, object>("", notExists),
				new Pair<object, object>(
					"<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>",
					AllExist("1", "2", "3", "4"))
			};
			RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

			// Avro
			var schema = AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME)).AsRecordSchema();
			var datumOne = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
			var datumTwo = new GenericRecord(schema);
			datumTwo.Put("indexed", Arrays.AsList(1, 2));
			datumTwo.Put("mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
			var avroTests = new Pair<object, object>[] {
				new Pair<object, object>(datumOne, notExists),
				new Pair<object, object>(datumTwo, AllExist(1, 2, 3, 4)),
			};
			RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path);

			// Json
			env.CompileDeploy("@JsonSchema(dynamic=true) @public @buseventtype create json schema " + JSON_TYPENAME + "()", path);
			var jsonTests = new Pair<object, object>[] {
				new Pair<object, object>("{}", notExists),
				new Pair<object, object>(
					"{\"mapped\":{\"keyOne\":\"3\",\"keyTwo\":\"4\"},\"indexed\":[\"1\",\"2\"]}",
					AllExist("1", "2", "3", "4"))
			};
			RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);

			// Json-Provided-Class
			var jsonProvidedTests = new Pair<object, object>[] {
				new Pair<object, object>("{}", notExists),
				new Pair<object, object>(
					"{\"mapped\":{\"keyOne\":\"3\",\"keyTwo\":\"4\"},\"indexed\":[\"1\",\"2\"]}",
					AllExist(1, 2, "3", "4"))
			};
			env.CompileDeploy(
				"@JsonSchema(className='" + nameof(MyLocalJsonProvided) + "') @public @buseventtype create json schema " + JSONPROVIDED_TYPENAME + "()",
				path);
			RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, null, jsonProvidedTests, typeof(object), path);
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

			var stmtText = "@name('s0') select " +
			               "indexed[0]? as indexed1, " +
			               "exists(indexed[0]?) as exists_indexed1, " +
			               "indexed[1]? as indexed2, " +
			               "exists(indexed[1]?) as exists_indexed2, " +
			               "mapped('keyOne')? as mapped1, " +
			               "exists(mapped('keyOne')?) as exists_mapped1, " +
			               "mapped('keyTwo')? as mapped2,  " +
			               "exists(mapped('keyTwo')?) as exists_mapped2  " +
			               "from " +
			               typename;
			env.CompileDeploy(stmtText, path).AddListener("s0");

			var propertyNames = "indexed1,indexed2,mapped1,mapped2".SplitCsv();
			var eventType = env.Statement("s0").EventType;
			foreach (var propertyName in propertyNames) {
				Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType(propertyName));
				Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_" + propertyName));
			}

			foreach (var pair in tests) {
				send.Invoke(env, pair.First, typename);
				var @event = env.Listener("s0").AssertOneGetNewAndReset();
				AssertValuesMayConvert(@event, propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion);
			}

			env.UndeployAll();
		}

		[Serializable]
		public class MyLocalJsonProvided
		{
			public int[] indexed;
			public IDictionary<string, object> mapped;
		}
	}
} // end of namespace
