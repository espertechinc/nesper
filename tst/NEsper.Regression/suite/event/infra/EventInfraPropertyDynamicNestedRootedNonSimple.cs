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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using static com.espertech.esper.common.@internal.util.CollectionUtil; // twoEntryMap
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
	public class EventInfraPropertyDynamicNestedRootedNonSimple : RegressionExecution {
	    public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "XML";
	    public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "Map";
	    public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "OA";
	    public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "Avro";
	    private static readonly Type BEAN_TYPE = typeof(SupportBeanDynRoot);
	    public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "Json";
	    public const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedNonSimple) + "JsonProvided";

	    public ISet<RegressionFlag> Flags() {
	        return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
	    }

	    public void Run(RegressionEnvironment env) {
	        var notExists = ValueWithExistsFlag.MultipleNotExists(6);
	        var path = new RegressionPath();

	        // Bean
	        var inner = SupportBeanComplexProps.MakeDefaultBean();
	        var beanTests = new Pair<object, object>[]{
	            new Pair<object, object>(new SupportBeanDynRoot("xxx"), notExists),
	            new Pair<object, object>(new SupportBeanDynRoot(inner), AllExist(inner.GetIndexed(0), inner.GetIndexed(1), inner.ArrayProperty[1], inner.GetMapped("keyOne"), inner.GetMapped("keyTwo"), inner.MapProperty.Get("xOne"))),
	        };
	        RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object), path);

	        // Map
	        IDictionary<string, object> mapNestedOne = new Dictionary<string, object>();
	        mapNestedOne.Put("indexed", new int[]{1, 2});
	        mapNestedOne.Put("arrayProperty", null);
	        mapNestedOne.Put("mapped", TwoEntryMap("keyOne", 100, "keyTwo", 200));
	        mapNestedOne.Put("mapProperty", null);
	        var mapOne = Collections.SingletonDataMap("item", mapNestedOne);
	        var mapTests = new Pair<object, object>[]{
	            new Pair<object, object>(Collections.EmptyDataMap, notExists),
	            new Pair<object, object>(mapOne, new ValueWithExistsFlag[] {
		            Exists(1), Exists(2), NotExists(), Exists(100), Exists(200), NotExists()
	            }),
	        };
	        RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

	        // Object-Array
	        var oaNestedOne = new object[]{new int[]{1, 2}, TwoEntryMap("keyOne", 100, "keyTwo", 200), new int[]{1000, 2000}, Collections.SingletonMap("xOne", "abc")};
	        var oaOne = new object[]{null, oaNestedOne};
	        var oaTests = new Pair<object, object>[]{
	            new Pair<object, object>(new object[]{null, null}, notExists),
	            new Pair<object, object>(oaOne, AllExist(1, 2, 2000, 100, 200, "abc")),
	        };
	        RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

	        // XML
	        var xmlTests = new Pair<object, object>[]{
	            new Pair<object, object>("", notExists),
	            new Pair<object, object>("<item>" +
	                "<indexed>1</indexed><indexed>2</indexed><mapped id=\"keyOne\">3</mapped><mapped id=\"keyTwo\">4</mapped>" +
	                "</item>", new ValueWithExistsFlag[] {
		            Exists("1"), Exists("2"), NotExists(), Exists("3"), Exists("4"), NotExists()
	            })
	        };
	        RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

	        // Avro
	        var schema = env
		        .RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME)
		        .AsRecordSchema();
	        var itemSchema = AvroSchemaUtil
		        .FindUnionRecordSchemaSingle(schema.GetField("item").Schema)
		        .AsRecordSchema();
	        var datumOne = new GenericRecord(schema);
	        datumOne.Put("item", null);
	        var datumItemTwo = new GenericRecord(itemSchema);
	        datumItemTwo.Put("indexed", Arrays.AsList(1, 2));
	        datumItemTwo.Put("mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
	        var datumTwo = new GenericRecord(schema);
	        datumTwo.Put("item", datumItemTwo);
	        var avroTests = new Pair<object, object>[]{
	            new Pair<object, object>(new GenericRecord(schema), notExists),
	            new Pair<object, object>(datumOne, notExists),
	            new Pair<object, object>(datumTwo, new ValueWithExistsFlag[]{Exists(1), Exists(2), NotExists(), Exists(3), Exists(4), NotExists()}),
	        };
	        env.AssertThat(() => RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path));

	        // Json
	        var jsonTests = new Pair<object, object>[]{
	            new Pair<object, object>("{}", notExists),
	            new Pair<object, object>("{ \"item\" : {}}", notExists),
	            new Pair<object, object>("{\n" +
	                "  \"item\": {\n" +
	                "    \"indexed\": [1,2],\n" +
	                "    \"mapped\": {\n" +
	                "      \"keyOne\": 3,\n" +
	                "      \"keyTwo\": 4\n" +
	                "    }\n" +
	                "  }\n" +
	                "}", new ValueWithExistsFlag[] {
		            Exists(1), Exists(2), NotExists(), Exists(3), Exists(4), NotExists()
	            }),
	        };
	        var schemasJson = "@Public @buseventtype @name('schema') @JsonSchema(dynamic=true) create json schema " + JSON_TYPENAME + "()";
	        env.CompileDeploy(schemasJson, path);
	        RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);

	        // Json-Class-Provided
	        var jsonProvidedNulls = new ValueWithExistsFlag[] {
		        Exists(null), NotExists(), NotExists(), Exists(null), NotExists(), NotExists()
	        };
	        var jsonProvidedTests = new Pair<object, object>[]{
	            new Pair<object, object>("{}", notExists),
	            new Pair<object, object>("{ \"item\" : {}}", jsonProvidedNulls),
	            new Pair<object, object>("{\n" +
	                "  \"item\": {\n" +
	                "    \"indexed\": [1,2],\n" +
	                "    \"mapped\": {\n" +
	                "      \"keyOne\": 3,\n" +
	                "      \"keyTwo\": 4\n" +
	                "    }\n" +
	                "  }\n" +
	                "}", new ValueWithExistsFlag[] {
		            Exists(1), Exists(2), NotExists(), Exists(3), Exists(4), NotExists()
	            })
	        };
	        var schemasJsonProvided = "@JsonSchema(className='" + typeof(MyLocalJsonProvided).FullName + "') @public @buseventtype @name('schema') @JsonSchema(dynamic=true) create json schema " + JSONPROVIDED_TYPENAME + "()";
	        env.CompileDeploy(schemasJsonProvided, path);
	        RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, null, jsonProvidedTests, typeof(object), path);
	    }

	    private void RunAssertion(RegressionEnvironment env,
	                              string typename,
	                              SupportEventInfra.FunctionSendEvent send,
	                              Func<object, object> optionalValueConversion,
	                              Pair<object, object>[] tests,
	                              Type expectedPropertyType,
	                              RegressionPath path) {

	        var stmtText = "@name('s0') select " +
	                       "item?.indexed[0] as indexed1, " +
	                       "exists(item?.indexed[0]) as exists_indexed1, " +
	                       "item?.indexed[1]? as indexed2, " +
	                       "exists(item?.indexed[1]?) as exists_indexed2, " +
	                       "item?.arrayProperty[1]? as array, " +
	                       "exists(item?.arrayProperty[1]?) as exists_array, " +
	                       "item?.mapped('keyOne') as mapped1, " +
	                       "exists(item?.mapped('keyOne')) as exists_mapped1, " +
	                       "item?.mapped('keyTwo')? as mapped2,  " +
	                       "exists(item?.mapped('keyTwo')?) as exists_mapped2,  " +
	                       "item?.mapProperty('xOne')? as map, " +
	                       "exists(item?.mapProperty('xOne')?) as exists_map " +
	                       " from " + typename;
	        env.CompileDeploy(stmtText, path).AddListener("s0");

	        var propertyNames = "indexed1,indexed2,array,mapped1,mapped2,map".SplitCsv();
	        env.AssertStatement("s0", statement => {
	            var eventType = statement.EventType;
	            foreach (var propertyName in propertyNames) {
	                Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType(propertyName));
	                Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_" + propertyName));
	            }
	        });

	        foreach (var pair in tests) {
	            send.Invoke(env, pair.First, typename);
	            env.AssertEventNew("s0", @event => SupportEventInfra.AssertValuesMayConvert(@event, propertyNames, (ValueWithExistsFlag[]) pair.Second, optionalValueConversion));
	        }

	        env.UndeployAll();
	    }

	    [Serializable]
	    public class MyLocalJsonProvided {
	        public MyLocalJsonProvidedItem item;
	    }

	    [Serializable]
	    public class MyLocalJsonProvidedItem {
	        public object[] indexed;
	        public IDictionary<string, object> mapped;
	    }
	}
} // end of namespace
