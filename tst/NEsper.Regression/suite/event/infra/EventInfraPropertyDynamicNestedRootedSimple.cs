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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
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

using NUnit.Framework;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNestedRootedSimple : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "Json";
        public const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyDynamicNestedRootedSimple) + "JsonProvided";

        private static readonly Type BEAN_TYPE = typeof(SupportMarkerInterface);
        private static readonly ValueWithExistsFlag[] NOT_EXISTS = MultipleNotExists(3);

        private readonly EventRepresentationChoice _eventRepresentationChoice;
        
        /// <summary>
        /// Constructor for test
        /// </summary>
        /// <param name="eventRepresentationChoice"></param>
        public EventInfraPropertyDynamicNestedRootedSimple(EventRepresentationChoice eventRepresentationChoice)
        {
            _eventRepresentationChoice = eventRepresentationChoice;
        }
        
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            switch (_eventRepresentationChoice)
            {
                case EventRepresentationChoice.OBJECTARRAY:
                    // Object-Array
                    var oaNestedNestedOne = new object[] { 101 };
                    var oaNestedOne = new object[] { "abc", oaNestedNestedOne };
                    var oaOne = new object[] { 5, oaNestedOne };
                    var oaTests = new Pair<object, object>[] {
                        new Pair<object, object>(new object[] { "a", null }, new[] { Exists("a"), NotExists(), NotExists() }),
                        new Pair<object, object>(oaOne, AllExist(5, "abc", 101))
                    };
                    RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object), path);
                    break;
                
                case EventRepresentationChoice.MAP:
                    // Map
                    var mapNestedNestedOne = Collections.SingletonDataMap("NestedNestedValue", 101);
                    var mapNestedOne = TwoEntryMap<string, object>(
                        "NestedNested", mapNestedNestedOne,
                        "NestedValue", "abc");
                    var mapOne = TwoEntryMap<string, object>(
                        "SimpleProperty", 5,
                        "Nested", mapNestedOne);
                    var mapTests = new Pair<object, object>[] {
                        new Pair<object, object>(
                            Collections.SingletonDataMap("SimpleProperty", "a"),
                            new ValueWithExistsFlag[] { Exists("a"), NotExists(), NotExists() }),
                        new Pair<object, object>(mapOne, AllExist(5, "abc", 101)),
                    };
                    RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object), path);
                    break;
                
                case EventRepresentationChoice.AVRO:
                    // Avro
                    var avroSchema = env
                        .RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME)
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
                    var avroTests = new Pair<object, object>[] {
                        new Pair<object, object>(
                            new GenericRecord(avroSchema),
                            new ValueWithExistsFlag[] { Exists(null), NotExists(), NotExists() }),
                        new Pair<object, object>(datumNull, new[] { Exists(null), NotExists(), NotExists() }),
                        new Pair<object, object>(datumOne, AllExist("abc", 100, 101)),
                    };
                    env.AssertThat(() => RunAssertion(env, AVRO_TYPENAME, FAVRO, null, avroTests, typeof(object), path));
                    break;
                
                case EventRepresentationChoice.JSON:
                    // Json
                    var jsonTests = new Pair<object, object>[] {
                        new Pair<object, object>("{}", NOT_EXISTS),
                        new Pair<object, object>(
                            "{\"SimpleProperty\": 1}",
                            new ValueWithExistsFlag[] { Exists(1), NotExists(), NotExists() }),
                        new Pair<object, object>(
                            "{\"SimpleProperty\": \"abc\", \"Nested\": { \"NestedValue\": 100, \"NestedNested\": { \"NestedNestedValue\": 101 } } }",
                            AllExist("abc", 100, 101)),
                    };
                    var schemasJson = "@JsonSchema(Dynamic=true) @public @buseventtype @name('schema') create json schema " +
                                      JSON_TYPENAME +
                                      "()";
                    env.CompileDeploy(schemasJson, path);
                    RunAssertion(env, JSON_TYPENAME, FJSON, null, jsonTests, typeof(object), path);
                    break;

                case EventRepresentationChoice.JSONCLASSPROVIDED:
                    // Json-Provided
                    var jsonProvidedTests = new Pair<object, object>[] {
                        new Pair<object, object>("{}", new ValueWithExistsFlag[] { Exists(null), NotExists(), NotExists() }),
                        new Pair<object, object>(
                            "{\"SimpleProperty\": 1}",
                            new ValueWithExistsFlag[] { Exists(1), NotExists(), NotExists() }),
                        new Pair<object, object>(
                            "{\"SimpleProperty\": \"abc\", \"Nested\": { \"NestedValue\": 100, \"NestedNested\": { \"NestedNestedValue\": 101 } } }",
                            AllExist("abc", 100, 101)),
                    };
                    var schemasJsonProvided = "@JsonSchema(ClassName='" +
                                              typeof(MyLocalJsonProvided).MaskTypeName() +
                                              "') @public @buseventtype @name('schema') create json schema " +
                                              JSONPROVIDED_TYPENAME +
                                              "()";
                    env.CompileDeploy(schemasJsonProvided, path);
                    RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, null, jsonProvidedTests, typeof(object), path);
                    break;
                
                case EventRepresentationChoice.DEFAULT:
                    // Bean
                    var beanTests = new Pair<object, object>[] {
                        new Pair<object, object>(
                            SupportBeanComplexProps.MakeDefaultBean(),
                            AllExist("Simple", "NestedValue", "NestedNestedValue")),
                        new Pair<object, object>(new SupportMarkerImplA("x"), NOT_EXISTS),
                    };
                    RunAssertion(env, BEAN_TYPE.Name, FBEAN, null, beanTests, typeof(object), path);

                    // XML
                    var xmlTests = new Pair<object, object>[] {
                        new Pair<object, object>(
                            "<SimpleProperty>abc</SimpleProperty>" +
                            "<Nested NestedValue=\"100\">\n" +
                            "\t<NestedNested NestedNestedValue=\"101\">\n" +
                            "\t</NestedNested>\n" +
                            "</Nested>\n",
                            AllExist("abc", "100", "101")),
                        new Pair<object, object>("<Nested/>", NOT_EXISTS)
                    };
                    RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode), path);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            var stmtText = "@Name('s0') select " +
                           "SimpleProperty? as Simple, " +
                           "exists(SimpleProperty?) as exists_Simple, " +
                           "Nested?.NestedValue as Nested, " +
                           "exists(Nested?.NestedValue) as exists_Nested, " +
                           "Nested?.NestedNested.NestedNestedValue as NestedNested, " +
                           "exists(Nested?.NestedNested.NestedNestedValue) as exists_NestedNested " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            var propertyNames = new [] { "Simple","Nested","NestedNested" };
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
                    @event => AssertValuesMayConvert(
                        @event,
                        propertyNames,
                        (ValueWithExistsFlag[])pair.Second,
                        optionalValueConversion));
            }

            env.UndeployAll();
        }

        public class MyLocalJsonProvided
        {
            public object SimpleProperty;
            public MyLocalJsonProvidedNested Nested;
        }

        public class MyLocalJsonProvidedNested
        {
            public int NestedValue;
            public MyLocalJsonProvidedNestedNested NestedNested;
        }

        public class MyLocalJsonProvidedNestedNested
        {
            public int NestedNestedValue;
        }
    }
} // end of namespace