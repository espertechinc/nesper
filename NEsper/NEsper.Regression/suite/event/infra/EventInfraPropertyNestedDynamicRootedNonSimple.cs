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

using com.espertech.esper.collection;
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
    public class EventInfraPropertyNestedDynamicRootedNonSimple : RegressionExecution
    {
        public static readonly string XML_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedNonSimple).FullName + "XML";

        public static readonly string MAP_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedNonSimple).FullName + "Map";

        public static readonly string OA_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedNonSimple).FullName + "OA";

        public static readonly string AVRO_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedNonSimple).FullName + "Avro";

        private static readonly Type BEAN_TYPE = typeof(SupportBeanDynRoot);

        public void Run(RegressionEnvironment env)
        {
            var notExists = MultipleNotExists(6);

            // Bean
            var inner = SupportBeanComplexProps.MakeDefaultBean();
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(new SupportBeanDynRoot("xxx"), notExists),
                new Pair<object, object>(
                    new SupportBeanDynRoot(inner),
                    AllExist(
                        inner.GetIndexed(0),
                        inner.GetIndexed(1),
                        inner.ArrayProperty[1],
                        inner.GetMapped("keyOne"),
                        inner.GetMapped("keyTwo"),
                        inner.MapProperty.Get("xOne")))
            };
            RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));

            // Map
            IDictionary<string, object> mapNestedOne = new Dictionary<string, object>();
            mapNestedOne.Put("Indexed", new[] {1, 2});
            mapNestedOne.Put("ArrayProperty", null);
            mapNestedOne.Put("Mapped", TwoEntryMap("keyOne", 100, "keyTwo", 200));
            mapNestedOne.Put("MapProperty", null);
            var mapOne = Collections.SingletonDataMap("item", mapNestedOne);
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(Collections.EmptyDataMap, notExists),
                new Pair<object, object>(
                    mapOne,
                    new[] {Exists(1), Exists(2), NotExists(), Exists(100), Exists(200), NotExists()})
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            object[] oaNestedOne = {
                new[] {1, 2}, TwoEntryMap("keyOne", 100, "keyTwo", 200), new[] {1000, 2000},
                Collections.SingletonMap("xOne", "abc")
            };
            object[] oaOne = {null, oaNestedOne};
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(new object[] {null, null}, notExists),
                new Pair<object, object>(oaOne, AllExist(1, 2, 2000, 100, 200, "abc"))
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>("", notExists),
                new Pair<object, object>(
                    "<item>" +
                    "<indexed>1</indexed><indexed>2</indexed><mapped Id=\"keyOne\">3</mapped><mapped Id=\"keyTwo\">4</mapped>" +
                    "</item>",
                    new[] {Exists("1"), Exists("2"), NotExists(), Exists("3"), Exists("4"), NotExists()})
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var schema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var itemSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(schema.GetField("item").Schema)
                .AsRecordSchema();
            var datumOne = new GenericRecord(schema);
            datumOne.Put("item", null);
            var datumItemTwo = new GenericRecord(itemSchema);
            datumItemTwo.Put("Indexed", Arrays.AsList(1, 2));
            datumItemTwo.Put("Mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
            var datumTwo = new GenericRecord(schema);
            datumTwo.Put("item", datumItemTwo);
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(new GenericRecord(schema), notExists),
                new Pair<object, object>(datumOne, notExists),
                new Pair<object, object>(
                    datumTwo,
                    new[] {Exists(1), Exists(2), NotExists(), Exists(3), Exists(4), NotExists()})
            };
            RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object));
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "@Name('s0') select " +
                           "item?.indexed[0] as indexed1, " +
                           "exists(item?.indexed[0]) as exists_indexed1, " +
                           "item?.indexed[1]? as indexed2, " +
                           "exists(item?.indexed[1]?) as exists_indexed2, " +
                           "item?.ArrayProperty[1]? as array, " +
                           "exists(item?.ArrayProperty[1]?) as exists_array, " +
                           "item?.mapped('keyOne') as mapped1, " +
                           "exists(item?.mapped('keyOne')) as exists_mapped1, " +
                           "item?.mapped('keyTwo')? as mapped2,  " +
                           "exists(item?.mapped('keyTwo')?) as exists_mapped2,  " +
                           "item?.mapProperty('xOne')? as map, " +
                           "exists(item?.mapProperty('xOne')?) as exists_map " +
                           " from " +
                           typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var propertyNames = "Indexed1,indexed2,array,mapped1,mapped2,map".SplitCsv();
            var eventType = env.Statement("s0").EventType;
            foreach (var propertyName in propertyNames) {
                Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_" + propertyName));
            }

            foreach (var pair in tests) {
                send.Invoke(env, pair.First, typename);
                AssertValuesMayConvert(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    propertyNames,
                    (ValueWithExistsFlag[]) pair.Second,
                    optionalValueConversion);
            }

            env.UndeployAll();
        }
    }
} // end of namespace