///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.@event;

using NEsper.Avro.Extensions;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertPropEquals
using static com.espertech.esper.common.@internal.util.CollectionUtil; // twoEntryMap
using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.@event.SupportEventInfra;
using static com.espertech.esper.regressionlib.support.@event.ValueWithExistsFlag;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyMappedIndexed : RegressionExecution
    {
        private static readonly Type BEAN_TYPE = typeof(MyIMEvent);
        public const string XML_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Avro";
        public const string JSON_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "Json";
        public const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyMappedIndexed) + "JsonProvided";

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            RunAssertion(
                env,
                BEAN_TYPE.Name,
                FBEAN,
                new MyIMEvent(new string[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1")),
                path);

            RunAssertion(
                env,
                MAP_TYPENAME,
                FMAP,
                TwoEntryMap<string, object>(
                    "indexed",
                    new string[] { "v1", "v2" },
                    "mapped",
                    Collections.SingletonMap("k1", "v1")),
                path);

            RunAssertion(
                env,
                OA_TYPENAME,
                FOA,
                new object[] { new string[] { "v1", "v2" }, Collections.SingletonMap("k1", "v1") },
                path);

            // Avro
            var avroSchema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
            var datum = new GenericRecord(avroSchema);
            datum.Put("indexed", Arrays.AsList("v1", "v2"));
            datum.Put("mapped", Collections.SingletonMap("k1", "v1"));
            RunAssertion(env, AVRO_TYPENAME, FAVRO, datum, path);

            // Json
            env.CompileDeploy(
                "@public @buseventtype @name('schema') create json schema " +
                JSON_TYPENAME +
                "(indexed string[], mapped java.util.Map)",
                path);
            var json = "{\"mapped\":{\"k1\":\"v1\"},\"indexed\":[\"v1\",\"v2\"]}";
            RunAssertion(env, JSON_TYPENAME, FJSON, json, path);

            // Json+ProvidedClass
            env.CompileDeploy(
                "@public @buseventtype @name('schema') @JsonSchema(className='" +
                typeof(MyLocalJsonProvided).FullName +
                "') create json schema " +
                JSONPROVIDED_TYPENAME +
                "()",
                path);
            RunAssertion(env, JSONPROVIDED_TYPENAME, FJSON, json, path);
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            SupportEventInfra.FunctionSendEvent send,
            object underlying,
            RegressionPath path)
        {
            RunAssertionTypeValidProp(env, typename, underlying);
            RunAssertionTypeInvalidProp(env, typename);

            var stmtText = "@name('s0') select * from " + typename;
            env.CompileDeploy(stmtText, path).AddListener("s0");

            send.Invoke(env, underlying, typename);

            env.AssertEventNew(
                "s0",
                @event => {
                    var mappedGetter = @event.EventType.GetGetterMapped("mapped");
                    Assert.AreEqual("v1", mappedGetter.Get(@event, "k1"));

                    var indexedGetter = @event.EventType.GetGetterIndexed("indexed");
                    Assert.AreEqual("v2", indexedGetter.Get(@event, 1));

                    RunAssertionEventInvalidProp(@event);
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                });

            env.UndeployAll();
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("xxxx", "mapped[1]", "indexed('a')", "mapped.x", "indexed.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            object underlying)
        {
            env.AssertThat(
                () => {
                    var eventType = env.Runtime.EventTypeService.GetBusEventType(typeName);

                    var expectedType = new object[][] {
                        new object[] {
                            "indexed", underlying is GenericRecord ? typeof(ICollection<string>) : typeof(string[]),
                            null, null
                        },
                        new object[] { "mapped", typeof(IDictionary<string, object>), null, null }
                    };
                    SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                        expectedType,
                        eventType,
                        SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

                    EPAssertionUtil.AssertEqualsAnyOrder(new string[] { "indexed", "mapped" }, eventType.PropertyNames);

                    Assert.IsNotNull(eventType.GetGetter("mapped"));
                    Assert.IsNotNull(eventType.GetGetter("mapped('a')"));
                    Assert.IsNotNull(eventType.GetGetter("indexed"));
                    Assert.IsNotNull(eventType.GetGetter("indexed[0]"));
                    Assert.IsTrue(eventType.IsProperty("mapped"));
                    Assert.IsTrue(eventType.IsProperty("mapped('a')"));
                    Assert.IsTrue(eventType.IsProperty("indexed"));
                    Assert.IsTrue(eventType.IsProperty("indexed[0]"));
                    Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("mapped"));
                    var mappedReturnsObject = typeName.Equals(MAP_TYPENAME) ||
                                              typeName.Equals(OA_TYPENAME) ||
                                              typeName.Equals(JSON_TYPENAME) ||
                                              typeName.Equals(JSONPROVIDED_TYPENAME);
                    Assert.AreEqual(
                        mappedReturnsObject ? typeof(object) : typeof(string),
                        eventType.GetPropertyType("mapped('a')"));
                    Assert.AreEqual(
                        underlying is GenericRecord ? typeof(ICollection<string>) : typeof(string[]),
                        eventType.GetPropertyType("indexed"));
                    Assert.AreEqual(typeof(string), eventType.GetPropertyType("indexed[0]"));

                    Type indexedType;
                    if (typeName.Equals(AVRO_TYPENAME)) {
                        indexedType = typeof(ICollection<string>);
                    }
                    else {
                        indexedType = typeof(string[]);
                    }

                    AssertPropEquals(
                        new SupportEventPropDesc("indexed", indexedType)
                            .WithIndexed()
                            .WithComponentType(typeof(string)),
                        eventType.GetPropertyDescriptor("indexed"));

                    Type mappedType;
                    Type componentType;
                    if (typeName.Equals(MAP_TYPENAME) ||
                        typeName.Equals(OA_TYPENAME) ||
                        typeName.Equals(JSON_TYPENAME)) {
                        componentType = typeof(object);
                        mappedType = typeof(IDictionary<string, object>);
                    }
                    else {
                        componentType = typeof(string);
                        mappedType = typeof(IDictionary<string, string>);
                    }

                    AssertPropEquals(
                        new SupportEventPropDesc("mapped", mappedType)
                            .WithComponentType(componentType)
                            .WithMapped(),
                        eventType.GetPropertyDescriptor("mapped"));

                    Assert.IsNull(eventType.GetFragmentType("indexed"));
                    Assert.IsNull(eventType.GetFragmentType("mapped"));
                });
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName)
        {
            env.AssertThat(
                () => {
                    var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

                    foreach (var prop in Arrays.AsList(
                                 "xxxx",
                                 "myString[0]",
                                 "indexed('a')",
                                 "indexed.x",
                                 "mapped[0]",
                                 "mapped.x")) {
                        Assert.AreEqual(false, eventType.IsProperty(prop));
                        Assert.AreEqual(null, eventType.GetPropertyType(prop));
                        Assert.IsNull(eventType.GetPropertyDescriptor(prop));
                    }
                });
        }

        [Serializable]
        public class MyIMEvent
        {
            private readonly string[] indexed;
            private readonly IDictionary<string, string> mapped;

            public MyIMEvent(
                string[] indexed,
                IDictionary<string, string> mapped)
            {
                this.indexed = indexed;
                this.mapped = mapped;
            }

            public string[] Indexed => indexed;

            public IDictionary<string, string> Mapped => mapped;
        }

        [Serializable]
        public class MyLocalJsonProvided
        {
            public string[] indexed;
            public IDictionary<string, string> mapped;
        }
    }
} // end of namespace