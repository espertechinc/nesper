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
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNonSimple : RegressionExecution
    {
        public static readonly string XML_TYPENAME = typeof(EventInfraPropertyDynamicNonSimple).FullName + "XML";
        public static readonly string MAP_TYPENAME = typeof(EventInfraPropertyDynamicNonSimple).FullName + "Map";
        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyDynamicNonSimple).FullName + "OA";
        public static readonly string AVRO_TYPENAME = typeof(EventInfraPropertyDynamicNonSimple).FullName + "Avro";

        public void Run(RegressionEnvironment env)
        {
            var notExists = MultipleNotExists(4);

            // Bean
            var bean = SupportBeanComplexProps.MakeDefaultBean();
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(
                    bean,
                    AllExist(
                        bean.GetIndexed(0),
                        bean.GetIndexed(1),
                        bean.GetMapped("keyOne"),
                        bean.GetMapped("keyTwo")))
            };
            RunAssertion(env, typeof(SupportBeanComplexProps).Name, FBEAN, null, beanTests, typeof(object));

            // Map
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(Collections.SingletonMap("somekey", "10"), notExists),
                new Pair<object, object>(
                    TwoEntryMap<string, object>(
                        "Indexed",
                        new[] {1, 2},
                        "Mapped",
                        TwoEntryMap<string, object>(
                            "keyOne",
                            3,
                            "keyTwo",
                            4)),
                    AllExist(1, 2, 3, 4))
            };
            RunAssertion(env, MAP_TYPENAME, FMAP, null, mapTests, typeof(object));

            // Object-Array
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(new object[] {null, null}, notExists),
                new Pair<object, object>(
                    new object[] {new[] {1, 2}, TwoEntryMap("keyOne", 3, "keyTwo", 4)},
                    AllExist(1, 2, 3, 4))
            };
            RunAssertion(env, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>("", notExists),
                new Pair<object, object>(
                    "<Indexed>1</Indexed><Indexed>2</Indexed><Mapped Id=\"keyOne\">3</Mapped><Mapped Id=\"keyTwo\">4</Mapped>",
                    AllExist("1", "2", "3", "4"))
            };
            RunAssertion(env, XML_TYPENAME, FXML, xmlToValue, xmlTests, typeof(XmlNode));

            // Avro
            var schema =
                AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
            var datumOne = new GenericRecord(SchemaBuilder.Record(AVRO_TYPENAME));
            var datumTwo = new GenericRecord(schema.AsRecordSchema());
            datumTwo.Put("Indexed", Arrays.AsList(1, 2));
            datumTwo.Put("Mapped", TwoEntryMap("keyOne", 3, "keyTwo", 4));
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(datumOne, notExists),
                new Pair<object, object>(datumTwo, AllExist(1, 2, 3, 4))
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
                           "Indexed[0]? as indexed1, " +
                           "exists(Indexed[0]?) as exists_indexed1, " +
                           "Indexed[1]? as indexed2, " +
                           "exists(Indexed[1]?) as exists_indexed2, " +
                           "Mapped('keyOne')? as mapped1, " +
                           "exists(Mapped('keyOne')?) as exists_mapped1, " +
                           "Mapped('keyTwo')? as mapped2,  " +
                           "exists(Mapped('keyTwo')?) as exists_mapped2  " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var propertyNames = new [] { "indexed1","indexed2","mapped1","mapped2" };
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