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

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyDynamicNested : RegressionExecution
    {
        private static readonly string BEAN_TYPENAME = nameof(SupportBeanDynRoot);
        public static readonly string XML_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "XML";
        public static readonly string MAP_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "Map";
        public static readonly string OA_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "OA";
        public static readonly string AVRO_TYPENAME = nameof(EventInfraPropertyDynamicNested) + "Avro";

        private static readonly FunctionSendEvent FAVRO = (
            env,
            value,
            typeName) => {
            var schema = AvroSchemaUtil.ResolveAvroSchema(
                env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName));
            var itemSchema = schema.GetField("Item").Schema;
            var itemDatum = new GenericRecord(itemSchema.AsRecordSchema());
            itemDatum.Put("Id", value);
            var datum = new GenericRecord(schema.AsRecordSchema());
            datum.Put("Item", itemDatum);
            env.SendEventAvro(datum, typeName);
        };

        public void Run(RegressionEnvironment env)
        {
            RunAssertion(env, EventRepresentationChoice.OBJECTARRAY, "");
            RunAssertion(env, EventRepresentationChoice.MAP, "");
            RunAssertion(
                env,
                EventRepresentationChoice.AVRO,
                "@AvroSchemaField(Name='myId',Schema='[\"int\",{\"type\":\"string\",\"avro.string\":\"String\"},\"null\"]')");
            RunAssertion(env, EventRepresentationChoice.DEFAULT, "");
        }

        private void RunAssertion(
            RegressionEnvironment env,
            EventRepresentationChoice outputEventRep,
            string additionalAnnotations)
        {
            // Bean
            Pair<object, object>[] beanTests = {
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_S0(101)), Exists(101)),
                new Pair<object, object>(new SupportBeanDynRoot("abc"), NotExists()),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_A("e1")), Exists("e1")),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_B("e2")), Exists("e2")),
                new Pair<object, object>(new SupportBeanDynRoot(new SupportBean_S1(102)), Exists(102))
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                BEAN_TYPENAME,
                FBEAN,
                null,
                beanTests,
                typeof(object));

            // Map
            Pair<object, object>[] mapTests = {
                new Pair<object, object>(Collections.EmptyDataMap, NotExists()),
                new Pair<object, object>(
                    Collections.SingletonMap("Item", Collections.SingletonMap("Id", 101)),
                    Exists(101)),
                new Pair<object, object>(
                    Collections.SingletonMap("Item", Collections.EmptyDataMap),
                    NotExists())
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                MAP_TYPENAME,
                FMAP,
                null,
                mapTests,
                typeof(object));

            // Object array
            Pair<object, object>[] oaTests = {
                new Pair<object, object>(new object[] {null}, NotExists()),
                new Pair<object, object>(new object[] {new SupportBean_S0(101)}, Exists(101)),
                new Pair<object, object>(new object[] {"abc"}, NotExists())
            };
            RunAssertion(env, outputEventRep, additionalAnnotations, OA_TYPENAME, FOA, null, oaTests, typeof(object));

            // XML
            Pair<object, object>[] xmlTests = {
                new Pair<object, object>("<Item Id=\"101\"/>", Exists("101")),
                new Pair<object, object>("<Item/>", NotExists())
            };
            if (!outputEventRep.IsAvroEvent()) {
                RunAssertion(
                    env,
                    outputEventRep,
                    additionalAnnotations,
                    XML_TYPENAME,
                    FXML,
                    xmlToValue,
                    xmlTests,
                    typeof(XmlNode));
            }

            // Avro
            Pair<object, object>[] avroTests = {
                new Pair<object, object>(null, Exists(null)),
                new Pair<object, object>(101, Exists(101)),
                new Pair<object, object>("abc", Exists("abc"))
            };
            RunAssertion(
                env,
                outputEventRep,
                additionalAnnotations,
                AVRO_TYPENAME,
                FAVRO,
                null,
                avroTests,
                typeof(object));
        }

        private void RunAssertion(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum,
            string additionalAnnotations,
            string typename,
            FunctionSendEvent send,
            Func<object, object> optionalValueConversion,
            Pair<object, object>[] tests,
            Type expectedPropertyType)
        {
            var stmtText = "@Name('s0') " +
                           eventRepresentationEnum.GetAnnotationText() +
                           additionalAnnotations +
                           " select " +
                           "Item.Id? as myId, " +
                           "exists(Item.Id?) as exists_myId " +
                           "from " +
                           typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(expectedPropertyType, eventType.GetPropertyType("myId"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("exists_myId").GetBoxedType());
            Assert.IsTrue(eventRepresentationEnum.MatchesClass(eventType.UnderlyingType));

            foreach (var pair in tests) {
                send.Invoke(env, pair.First, typename);
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                AssertValueMayConvert(@event, "myId", (ValueWithExistsFlag) pair.Second, optionalValueConversion);
            }

            env.UndeployAll();
        }
    }
} // end of namespace