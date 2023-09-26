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
    public class EventInfraPropertyDynamicNestedRootedSimple : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Json";

        public const string JSONPROVIDED_TYPENAME =
            nameof(EventInfraPropertyDynamicNestedRootedSimple) + "JsonProvided";

        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);
        private static readonly ValueWithExistsFlag[] NOT_EXISTS = MultipleNotExists(3);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            // Bean
            var beanTests = new Pair<object, object>[] {
                new Pair<object, object>(
                    SupportBeanComplexProps.MakeDefaultBean(),
                    AllExist("simple", "nestedValue", "nestedNestedValue")),
                new Pair<object, object>(new SupportMarkerImplA("x"), NOT_EXISTS),
            };
            RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object), path);

            // Map
            var mapNestedNestedOne = Collections.SingletonDataMap("nestedNestedValue", 101);
            var mapNestedOne = TwoEntryMap<string, object>("nestedNested", mapNestedNestedOne, "nestedValue", "abc");
            var mapOne = TwoEntryMap<string, object>("simpleProperty", 5, "nested", mapNestedOne);
            var mapTests = new Pair<object, object>[] {
                new Pair<object, object>(
                    Collections.SingletonMap("simpleProperty", "a"),
                    new ValueWithExistsFlag[] { Exists("a"), NotExists(), NotExists() }),
                new Pair<object, object>(mapOne, AllExist(5, "abc", 101)),
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);

            // Object-Array
            var oaNestedNestedOne = new object[] { 101 };
            var oaNestedOne = new object[] { "abc", oaNestedNestedOne };
            var oaOne = new object[] { 5, oaNestedOne };
            var oaTests = new Pair<object, object>[] {
                new Pair<object, object>(
                    new object[] { "a", null },
                    new ValueWithExistsFlag[] { Exists("a"), NotExists(), NotExists() }),
                new Pair<object, object>(oaOne, AllExist(5, "abc", 101)),
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);

            // XML
            var xmlTests = new Pair<object, object>[] {
                new Pair<object, object>(
                    "<simpleProperty>abc</simpleProperty>" +
                    "<nested nestedValue=\"100\">\n" +
                    "\t<nestedNested nestedNestedValue=\"101\">\n" +
                    "\t</nestedNested>\n" +
                    "</nested>\n",
                    AllExist("abc", "100", "101")),
                new Pair<object, object>("<nested/>", NOT_EXISTS),
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

            // Avro
            var avroSchema = env
                .RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME)
                .AsRecordSchema();
            var datumNull = new GenericRecord(avroSchema);
            var schema = avroSchema;
            var nestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(schema.GetField("nested").Schema.AsRecordSchema())
                .AsRecordSchema();
            var nestedNestedSchema = AvroSchemaUtil
                .FindUnionRecordSchemaSingle(nestedSchema.GetField("nestedNested").Schema.AsRecordSchema())
                .AsRecordSchema();
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema);
            nestedNestedDatum.Put("nestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema);
            nestedDatum.Put("nestedValue", 100);
            nestedDatum.Put("nestedNested", nestedNestedDatum);
            var datumOne = new GenericRecord(schema);
            datumOne.Put("simpleProperty", "abc");
            datumOne.Put("nested", nestedDatum);
            var avroTests = new Pair<object, object>[] {
                new Pair<object, object>(
                    new GenericRecord(avroSchema),
                    new ValueWithExistsFlag[] { Exists(null), NotExists(), NotExists() }),
                new Pair<object, object>(
                    datumNull,
                    new ValueWithExistsFlag[] { Exists(null), NotExists(), NotExists() }),
                new Pair<object, object>(datumOne, AllExist("abc", 100, 101)),
            };
            env.AssertThat(() => RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path));

            // Json
            var jsonTests = new Pair<object, object>[] {
                new Pair<object, object>("{}", NOT_EXISTS),
                new Pair<object, object>(
                    "{\"simpleProperty\": 1}",
                    new ValueWithExistsFlag[] { Exists(1), NotExists(), NotExists() }),
                new Pair<object, object>(
                    "{\"simpleProperty\": \"abc\", \"nested\": { \"nestedValue\": 100, \"nestedNested\": { \"nestedNestedValue\": 101 } } }",
                    AllExist("abc", 100, 101)),
            };
            var schemasJson = "@JsonSchema(dynamic=true) @public @buseventtype @name('schema') create json schema " +
                              JSON_TYPENAME +
                              "()";
            env.CompileDeploy(schemasJson, path);
            RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);

            // Json-Provided
            var jsonProvidedTests = new Pair<object, object>[] {
                new Pair<object, object>("{}", new ValueWithExistsFlag[] { Exists(null), NotExists(), NotExists() }),
                new Pair<object, object>(
                    "{\"simpleProperty\": 1}",
                    new ValueWithExistsFlag[] { Exists(1), NotExists(), NotExists() }),
                new Pair<object, object>(
                    "{\"simpleProperty\": \"abc\", \"nested\": { \"nestedValue\": 100, \"nestedNested\": { \"nestedNestedValue\": 101 } } }",
                    AllExist("abc", 100, 101)),
            };
            var schemasJsonProvided = "@JsonSchema(className='" +
                                      typeof(MyLocalJsonProvided).FullName +
                                      "') @public @buseventtype @name('schema') create json schema " +
                                      JSONPROVIDED_TYPENAME +
                                      "()";
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
            var stmtText = "@name('s0') select " +
                           "simpleProperty? as simple, " +
                           "exists(simpleProperty?) as exists_simple, " +
                           "nested?.nestedValue as nested, " +
                           "exists(nested?.nestedValue) as exists_nested, " +
                           "nested?.nestedNested.nestedNestedValue as nestedNested, " +
                           "exists(nested?.nestedNested.nestedNestedValue) as exists_nestedNested " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            var propertyNames = "simple,nested,nestedNested".SplitCsv();
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

            env.UndeployAll();
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public object simpleProperty;
            public MyLocalJsonProvidedNested nested;
        }

        [Serializable]
        public class MyLocalJsonProvidedNested
        {
            public int nestedValue;
            public MyLocalJsonProvidedNestedNested nestedNested;
        }

        [Serializable]
        public class MyLocalJsonProvidedNestedNested
        {
            public int nestedNestedValue;
        }
    }
} // end of namespace