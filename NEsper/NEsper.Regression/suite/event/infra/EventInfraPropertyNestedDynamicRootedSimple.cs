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
    public class EventInfraPropertyNestedDynamicRootedSimple : RegressionExecution
    {
        public static readonly string XML_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedSimple).FullName + "XML";

        public static readonly string MAP_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedSimple).FullName + "Map";

        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyNestedDynamicRootedSimple).FullName + "OA";

        public static readonly string AVRO_TYPENAME =
            typeof(EventInfraPropertyNestedDynamicRootedSimple).FullName + "Avro";

        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);
        private static readonly ValueWithExistsFlag[] NOT_EXISTS = MultipleNotExists(3);

        public void Run(RegressionEnvironment env)
        {
            // Bean
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(
                    SupportBeanComplexProps.MakeDefaultBean(),
                    AllExist("simple", "NestedValue", "nestedNestedValue")),
                new Pair<object, object>(new SupportMarkerImplA("x"), NOT_EXISTS)
            };
            RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));

            // Map
            var mapNestedNestedOne = Collections.SingletonDataMap("nestedNestedValue", 101);
            var mapNestedOne = TwoEntryMap<string, object>(
                "nestedNested",
                mapNestedNestedOne,
                "NestedValue",
                "abc");
            var mapOne = TwoEntryMap<string, object>("simpleProperty", 5, "nested", mapNestedOne);
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(
                    Collections.SingletonMap("simpleProperty", "a"),
                    new[] {Exists("a"), NotExists(), NotExists()}),
                new Pair<object, object>(mapOne, AllExist(5, "abc", 101))
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            object[] oaNestedNestedOne = {101};
            object[] oaNestedOne = {"abc", oaNestedNestedOne};
            object[] oaOne = {5, oaNestedOne};
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(new object[] {"a", null}, new[] {Exists("a"), NotExists(), NotExists()}),
                new Pair<object, object>(oaOne, AllExist(5, "abc", 101))
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>(
                    "<simpleProperty>abc</simpleProperty>" +
                    "<nested NestedValue=\"100\">\n" +
                    "\t<nestedNested nestedNestedValue=\"101\">\n" +
                    "\t</nestedNested>\n" +
                    "</nested>\n",
                    AllExist("abc", "100", "101")),
                new Pair<object, object>("<nested/>", NOT_EXISTS)
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var avroSchema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var datumNull = new GenericRecord(avroSchema);
            var schema = avroSchema;
            var nestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(schema.GetField("nested").Schema)
                .AsRecordSchema();
            var nestedNestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(nestedSchema.GetField("nestedNested").Schema)
                .AsRecordSchema();
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
            nestedNestedDatum.Put("nestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema);
            nestedDatum.Put("NestedValue", 100);
            nestedDatum.Put("nestedNested", nestedNestedDatum);
            var datumOne = new GenericRecord(schema);
            datumOne.Put("simpleProperty", "abc");
            datumOne.Put("nested", nestedDatum);
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(
                    new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME)),
                    NOT_EXISTS),
                new Pair<object, object>(datumNull, new[] {Exists(null), NotExists(), NotExists()}),
                new Pair<object, object>(datumOne, AllExist("abc", 100, 101))
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
                           "simpleProperty? as simple, " +
                           "exists(simpleProperty?) as exists_simple, " +
                           "nested?.NestedValue as nested, " +
                           "exists(nested?.NestedValue) as exists_nested, " +
                           "nested?.NestedNested.NestedNestedValue as nestedNested, " +
                           "exists(nested?.NestedNested.NestedNestedValue) as exists_nestedNested " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var propertyNames = new [] { "simple","nested","nestedNested" };
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