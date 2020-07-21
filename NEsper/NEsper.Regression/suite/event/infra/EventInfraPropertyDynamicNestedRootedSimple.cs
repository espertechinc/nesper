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

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNestedRootedSimple : RegressionExecution
    {
        public static readonly string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "XML";
        public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Map";
        public static readonly string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "OA";
        public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Avro";

        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);
        private static readonly ValueWithExistsFlag[] NOT_EXISTS = MultipleNotExists(3);

        public void Run(RegressionEnvironment env)
        {
            // Bean
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(
                    SupportBeanComplexProps.MakeDefaultBean(),
                    AllExist("Simple", "NestedValue", "NestedNestedValue")),
                new Pair<object, object>(new SupportMarkerImplA("x"), NOT_EXISTS)
            };
            RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object));

            // Map
            var mapNestedNestedOne = Collections.SingletonDataMap("NestedNestedValue", 101);
            var mapNestedOne = TwoEntryMap<string, object>(
                "NestedNested", mapNestedNestedOne,
                "NestedValue", "abc");
            var mapOne = TwoEntryMap<string, object>(
                "SimpleProperty", 5, 
                "Nested", mapNestedOne);
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(
                    Collections.SingletonDataMap("SimpleProperty", "a"),
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
                    "<SimpleProperty>abc</SimpleProperty>" +
                    "<Nested NestedValue=\"100\">\n" +
                    "\t<NestedNested NestedNestedValue=\"101\">\n" +
                    "\t</NestedNested>\n" +
                    "</Nested>\n",
                    AllExist("abc", "100", "101")),
                new Pair<object, object>("<Nested/>", NOT_EXISTS)
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var avroSchema = AvroSchemaUtil
                .ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME))
                .AsRecordSchema();
            var datumNull = new GenericRecord(avroSchema);
            var schema = avroSchema;
            var nestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(schema.GetField("Nested").Schema)
                .AsRecordSchema();
            var nestedNestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(nestedSchema.GetField("NestedNested").Schema)
                .AsRecordSchema();
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
            nestedNestedDatum.Put("NestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema);
            nestedDatum.Put("NestedValue", 100);
            nestedDatum.Put("NestedNested", nestedNestedDatum);
            var datumOne = new GenericRecord(schema);
            datumOne.Put("SimpleProperty", "abc");
            datumOne.Put("Nested", nestedDatum);
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
            var stmtText = "@name('s0') select " +
                           "SimpleProperty? as Simple, " +
                           "exists(SimpleProperty?) as exists_Simple, " +
                           "Nested?.NestedValue as Nested, " +
                           "exists(Nested?.NestedValue) as exists_Nested, " +
                           "Nested?.NestedNested.NestedNestedValue as NestedNested, " +
                           "exists(Nested?.NestedNested.NestedNestedValue) as exists_NestedNested " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var propertyNames = new [] { "Simple","Nested","NestedNested" };
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