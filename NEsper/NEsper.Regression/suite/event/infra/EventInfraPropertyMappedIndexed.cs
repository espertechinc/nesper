///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;
using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyMappedIndexed : RegressionExecution
    {
        public static readonly string XML_TYPENAME = typeof(EventInfraPropertyMappedIndexed).FullName + "XML";
        public static readonly string MAP_TYPENAME = typeof(EventInfraPropertyMappedIndexed).FullName + "Map";
        public static readonly string OA_TYPENAME = typeof(EventInfraPropertyMappedIndexed).FullName + "OA";
        public static readonly string AVRO_TYPENAME = typeof(EventInfraPropertyMappedIndexed).FullName + "Avro";
        private static readonly Type BEAN_TYPE = typeof(MyIMEvent);

        public void Run(RegressionEnvironment env)
        {
            RunAssertion(
                env,
                BEAN_TYPE.Name,
                FBEAN,
                new MyIMEvent(new[] {"v1", "v2"}, Collections.SingletonMap("k1", "v1")));

            RunAssertion(
                env,
                MAP_TYPENAME,
                FMAP,
                TwoEntryMap<string, object>(
                    "Indexed",
                    new[] {"v1", "v2"},
                    "Mapped",
                    Collections.SingletonDataMap("k1", "v1")));

            RunAssertion(
                env,
                OA_TYPENAME,
                FOA,
                new object[] {new[] {"v1", "v2"}, Collections.SingletonMap("k1", "v1")});

            // Avro
            var avroSchema =
                AvroSchemaUtil.ResolveAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(AVRO_TYPENAME));
            var datum = new GenericRecord(avroSchema.AsRecordSchema());
            datum.Put("Indexed", Arrays.AsList("v1", "v2"));
            datum.Put("Mapped", Collections.SingletonMap("k1", "v1"));
            RunAssertion(env, AVRO_TYPENAME, FAVRO, datum);
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent send,
            object underlying)
        {
            RunAssertionTypeValidProp(env, typename, underlying);
            RunAssertionTypeInvalidProp(env, typename);

            var stmtText = "@Name('s0') select * from " + typename;
            env.CompileDeploy(stmtText).AddListener("s0");

            send.Invoke(env, underlying, typename);
            var @event = env.Listener("s0").AssertOneGetNewAndReset();

            var mappedGetter = @event.EventType.GetGetterMapped("Mapped");
            Assert.AreEqual("v1", mappedGetter.Get(@event, "k1"));

            var indexedGetter = @event.EventType.GetGetterIndexed("Indexed");
            Assert.AreEqual("v2", indexedGetter.Get(@event, 1));

            RunAssertionEventInvalidProp(@event);
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            env.UndeployAll();
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("xxxx", "Mapped[1]", "Indexed('a')", "Mapped.x", "Indexed.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            object underlying)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            object[][] expectedType = {
                new object[] {
                    "Indexed", underlying is GenericRecord ? typeof(ICollection<object>) : typeof(string[]), null, null
                },
                new object[] {"Mapped", typeof(IDictionary<string, object>), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] {"Indexed", "Mapped"}, eventType.PropertyNames);

            Assert.IsNotNull(eventType.GetGetter("Mapped"));
            Assert.IsNotNull(eventType.GetGetter("Mapped('a')"));
            Assert.IsNotNull(eventType.GetGetter("Indexed"));
            Assert.IsNotNull(eventType.GetGetter("Indexed[0]"));
            Assert.IsTrue(eventType.IsProperty("Mapped"));
            Assert.IsTrue(eventType.IsProperty("Mapped('a')"));
            Assert.IsTrue(eventType.IsProperty("Indexed"));
            Assert.IsTrue(eventType.IsProperty("Indexed[0]"));
            Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("Mapped"));
            Assert.AreEqual(
                underlying is IDictionary<string, object> || underlying is object[] ? typeof(object) : typeof(string),
                eventType.GetPropertyType("Mapped('a')"));
            Assert.AreEqual(
                underlying is GenericRecord ? typeof(ICollection<object>) : typeof(string[]),
                eventType.GetPropertyType("Indexed"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("Indexed[0]"));

            Assert.AreEqual(
                new EventPropertyDescriptor(
                    "Indexed",
                    underlying is GenericRecord ? typeof(ICollection<object>) : typeof(string[]),
                    typeof(string),
                    false,
                    false,
                    true,
                    false,
                    false),
                eventType.GetPropertyDescriptor("Indexed"));
            Assert.AreEqual(
                new EventPropertyDescriptor(
                    "Mapped",
                    typeof(IDictionary<string, object>),
                    underlying is IDictionary<string, object> || underlying is object[]
                        ? typeof(object)
                        : typeof(string),
                    false,
                    false,
                    false,
                    true,
                    false),
                eventType.GetPropertyDescriptor("Mapped"));

            Assert.IsNull(eventType.GetFragmentType("Indexed"));
            Assert.IsNull(eventType.GetFragmentType("Mapped"));
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            foreach (var prop in Arrays.AsList(
                "xxxx",
                "myString[0]",
                "Indexed('a')",
                "Indexed.x",
                "Mapped[0]",
                "Mapped.x")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }

        public class MyIMEvent
        {
            private readonly IDictionary<string, string> mapped;

            public MyIMEvent(
                string[] indexed,
                IDictionary<string, string> mapped)
            {
                Indexed = indexed;
                this.mapped = mapped;
            }

            public string[] Indexed { get; }

            public IDictionary<string, string> GetMapped()
            {
                return mapped;
            }
        }
    }
} // end of namespace