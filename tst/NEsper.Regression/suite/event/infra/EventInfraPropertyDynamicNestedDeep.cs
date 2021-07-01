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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNestedDeep : RegressionExecution
    {
        public static readonly string XML_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "XML";
        public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "Map";
        public static readonly string OA_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "OA";
        public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNestedDeep) + "Avro";

        private static readonly FunctionSendEvent FAVRO = (
            env,
            value,
            typename) => {
            var schema = AvroSchemaUtil.ResolveAvroSchema(
                env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
            var itemSchema = schema.GetField("Item").Schema;
            var itemDatum = new GenericRecord(itemSchema.AsRecordSchema());
            itemDatum.Put("Nested", value);
            var datum = new GenericRecord(schema.AsRecordSchema());
            datum.Put("Item", itemDatum);
            env.SendEventAvro(datum, typename);
        };

        public void Run(RegressionEnvironment env)
        {
            var notExists = MultipleNotExists(6);

            // Bean
            var beanOne = SupportBeanComplexProps.MakeDefaultBean();
            var n1v = beanOne.Nested.NestedValue;
            var n1nv = beanOne.Nested.NestedNested.NestedNestedValue;
            var beanTwo = SupportBeanComplexProps.MakeDefaultBean();
            beanTwo.Nested.NestedValue = "nested1";
            beanTwo.Nested.NestedNested.NestedNestedValue = "nested2";
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(new SupportBeanDynRoot(beanOne), AllExist(n1v, n1v, n1nv, n1nv, n1nv, n1nv)),
                new Pair<object, object>(
                    new SupportBeanDynRoot(beanTwo),
                    AllExist("nested1", "nested1", "nested2", "nested2", "nested2", "nested2")),
                new Pair<object, object>(new SupportBeanDynRoot("abc"), notExists)
            };
            RunAssertion(env, "SupportBeanDynRoot", FBEAN, null, beanTests, typeof(object));

            // Map
            IDictionary<string, object> mapOneL2 = new Dictionary<string, object>();
            mapOneL2.Put("NestedNestedValue", 101);
            IDictionary<string, object> mapOneL1 = new Dictionary<string, object>();
            mapOneL1.Put("NestedNested", mapOneL2);
            mapOneL1.Put("NestedValue", 100);
            IDictionary<string, object> mapOneL0 = new Dictionary<string, object>();
            mapOneL0.Put("Nested", mapOneL1);
            var mapOne = Collections.SingletonDataMap("Item", mapOneL0);
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(mapOne, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object, object>(Collections.EmptyDataMap, notExists)
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            object[] oaOneL2 = {101};
            object[] oaOneL1 = {oaOneL2, 100};
            object[] oaOneL0 = {oaOneL1};
            object[] oaOne = {oaOneL0};
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(oaOne, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object, object>(new object[] {null}, notExists)
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>(
                    "<Item>\n" +
                    "\t<Nested NestedValue=\"100\">\n" +
                    "\t\t<NestedNested NestedNestedValue=\"101\">\n" +
                    "\t\t</NestedNested>\n" +
                    "\t</Nested>\n" +
                    "</Item>\n",
                    AllExist("100", "100", "101", "101", "101", "101")),
                new Pair<object, object>("<item/>", notExists)
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var schema =
                AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
            var nestedSchema =
                AvroSchemaUtil.FindUnionRecordSchemaSingle(
                    schema
                        .GetField("Item")
                        .Schema.AsRecordSchema()
                        .GetField("Nested")
                        .Schema);
            var nestedNestedSchema =
                AvroSchemaUtil.FindUnionRecordSchemaSingle(
                    nestedSchema.GetField("NestedNested").Schema);
            var nestedNestedDatum = new GenericRecord(nestedNestedSchema.AsRecordSchema());
            nestedNestedDatum.Put("NestedNestedValue", 101);
            var nestedDatum = new GenericRecord(nestedSchema.AsRecordSchema());
            nestedDatum.Put("NestedValue", 100);
            nestedDatum.Put("NestedNested", nestedNestedDatum);
            var emptyDatum = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(nestedDatum, AllExist(100, 100, 101, 101, 101, 101)),
                new Pair<object, object>(emptyDatum, notExists),
                new Pair<object, object>(null, notExists)
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
            RunAssertionSelectNested(env, typename, send, optionalValueConversion, tests, expectedPropertyType);
            RunAssertionBeanNav(env, typename, send, tests[0].First);
        }

        private void RunAssertionBeanNav(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            object underlyingComplete)
        {
            var stmtText = "@Name('s0') select * from " + typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            send.Invoke(env, underlyingComplete, typename);
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            env.UndeployAll();
        }

        private void RunAssertionSelectNested(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "@Name('s0') select " +
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
            env.CompileDeploy(stmtText).AddListener("s0");

            var propertyNames = new [] { "n1","n2","n3","n4","n5","n6" };
            var eventType = env.Statement("s0").EventType;
            foreach (var propertyName in propertyNames) {
                Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType(propertyName));
                Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_" + propertyName));
            }

            foreach (var pair in tests) {
                send.Invoke(env, pair.First, typename);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertValuesMayConvert(
                    @event,
                    propertyNames,
                    (ValueWithExistsFlag[]) pair.Second,
                    optionalValueConversion);
            }

            env.UndeployAll();
        }
    }
} // end of namespace