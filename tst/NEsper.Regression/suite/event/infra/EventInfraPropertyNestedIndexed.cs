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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyNestedIndexed : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "Avro";
        private const string BEAN_TYPENAME = nameof(InfraNestedIndexPropTop);
        private const string JSON_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "Json";
        private const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyNestedIndexed) + "JsonProvided";

        private readonly EventRepresentationChoice _eventRepresentationChoice;
        
        /// <summary>
        /// Constructor for test
        /// </summary>
        /// <param name="eventRepresentationChoice"></param>
        public EventInfraPropertyNestedIndexed(EventRepresentationChoice eventRepresentationChoice)
        {
            _eventRepresentationChoice = eventRepresentationChoice;
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            switch (_eventRepresentationChoice)
            {
                case EventRepresentationChoice.OBJECTARRAY:
                    RunAssertion(env, true, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1", path);
                    break;
                case EventRepresentationChoice.MAP:
                    RunAssertion(env, true, MAP_TYPENAME, FMAP, typeof(IDictionary<string, object>[]),
                        MAP_TYPENAME + "_1", path);
                    break;
                case EventRepresentationChoice.AVRO:
                    RunAssertion(env, true, AVRO_TYPENAME, FAVRO, typeof(GenericRecord[]), AVRO_TYPENAME + "_1", path);
                    break;
                case EventRepresentationChoice.JSON:
                    // Json
                    var eplJson =
                        $"create json schema {JSON_TYPENAME}_4(Lvl4 int);\n" +
                        $" create json schema {JSON_TYPENAME}_3(Lvl3 int, L4 {JSON_TYPENAME}_4[]);\n" +
                        $" create json schema {JSON_TYPENAME}_2(Lvl2 int, L3 {JSON_TYPENAME}_3[]);\n" +
                        $" create json schema {JSON_TYPENAME}_1(Lvl1 int, L2 {JSON_TYPENAME}_2[]);\n" +
                        " @name('types') @public @buseventtype" +
                        $" create json schema {JSON_TYPENAME}(L1 {JSON_TYPENAME}_1[]);\n";
                    env.CompileDeploy(eplJson, path);
                    RunAssertion(env, false, JSON_TYPENAME, FJSON, typeof(object[]), JSON_TYPENAME + "_1", path);
                    env.UndeployModuleContaining("types");
                    break;
                case EventRepresentationChoice.JSONCLASSPROVIDED:
                    // Json-Class-Provided
                    var eplJsonProvided =
                        $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedLvl4).FullName}')" +
                        $" create json schema {JSONPROVIDED_TYPENAME}_4();\n" +
                        $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedLvl3).FullName}')" +
                        $" create json schema {JSONPROVIDED_TYPENAME}_3(Lvl3 int, L4 {JSONPROVIDED_TYPENAME}_4[]);\n" +
                        $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedLvl2).FullName}')" +
                        $" create json schema {JSONPROVIDED_TYPENAME}_2(Lvl2 int, L3 {JSONPROVIDED_TYPENAME}_3[]);\n" +
                        $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedLvl1).FullName}')" +
                        $" create json schema {JSONPROVIDED_TYPENAME}_1(Lvl1 int, L2 {JSONPROVIDED_TYPENAME}_2[]);\n" +
                        $"@JsonSchema(ClassName='{typeof(MyLocalJSONProvidedTop).FullName}')" +
                        $" @name('types') @public @buseventtype" +
                        $" create json schema {JSONPROVIDED_TYPENAME}(L1 {JSONPROVIDED_TYPENAME}_1[]);\n";

                    env.CompileDeploy(eplJsonProvided, path);
                    RunAssertion(
                        env,
                        false,
                        JSONPROVIDED_TYPENAME,
                        FJSON,
                        typeof(MyLocalJSONProvidedLvl1[]),
                        "EventInfraPropertyNestedIndexedJsonProvided_1",
                        path);
                    break;
                case EventRepresentationChoice.DEFAULT:
                    RunAssertion(
                        env,
                        true,
                        BEAN_TYPENAME,
                        FBEAN,
                        typeof(InfraNestedIndexedPropLvl1[]),
                        typeof(InfraNestedIndexedPropLvl1).FullName,
                        path);

                    RunAssertion(env, true, XML_TYPENAME, FXML, typeof(XmlNode[]), XML_TYPENAME + ".L1", path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            env.UndeployAll();
        }

        private void RunAssertion(
            RegressionEnvironment env,
            bool preconfigured,
            string typename,
            FunctionSendEvent4IntWArrayNested send,
            Type nestedClass,
            string fragmentTypeName,
            RegressionPath path)
        {
            RunAssertionSelectNested(env, typename, send, path);
            RunAssertionBeanNav(env, typename, send, path);
            env.AssertThat(
                () => {
                    RunAssertionTypeValidProp(env, preconfigured, typename, send, nestedClass, fragmentTypeName);
                    RunAssertionTypeInvalidProp(env, typename);
                });
        }

        private void RunAssertionBeanNav(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4IntWArrayNested send,
            RegressionPath path)
        {
            var epl = "@name('s0') select * from " + typename;
            env.CompileDeploy(epl, path).AddListener("s0");

            send.Invoke(typename, env, 1, 2, 3, 4);
            env.AssertEventNew(
                "s0",
                @event => {
                    EPAssertionUtil.AssertProps(
                        @event,
                        "L1[0].Lvl1,L1[0].L2[0].Lvl2,L1[0].L2[0].L3[0].Lvl3,L1[0].L2[0].L3[0].L4[0].Lvl4".SplitCsv(),
                        new object[] { 1, 2, 3, 4 });
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                    var isNative = typename.Equals(BEAN_TYPENAME);
                    SupportEventTypeAssertionUtil.AssertFragments(
                        @event,
                        isNative,
                        true,
                        "L1,L1[0].L2,L1[0].L2[0].L3,L1[0].L2[0].L3[0].L4");
                    SupportEventTypeAssertionUtil.AssertFragments(
                        @event,
                        isNative,
                        false,
                        "L1[0],L1[0].L2[0],L1[0].L2[0].L3[0],L1[0].L2[0].L3[0].L4[0]");

                    RunAssertionEventInvalidProp(@event);
                });

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionSelectNested(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4IntWArrayNested send,
            RegressionPath path)
        {
            var epl = "@name('s0') select " +
                      "L1[0].Lvl1 as c0, " +
                      "exists(L1[0].Lvl1) as exists_c0, " +
                      "L1[0].L2[0].Lvl2 as c1, " +
                      "exists(L1[0].L2[0].Lvl2) as exists_c1, " +
                      "L1[0].L2[0].L3[0].Lvl3 as c2, " +
                      "exists(L1[0].L2[0].L3[0].Lvl3) as exists_c2, " +
                      "L1[0].L2[0].L3[0].L4[0].Lvl4 as c3, " +
                      "exists(L1[0].L2[0].L3[0].L4[0].Lvl4) as exists_c3 " +
                      "from " +
                      typename;
            env.CompileDeploy(epl, path).AddListener("s0");

            var fields = "c0,exists_c0,c1,exists_c1,c2,exists_c2,c3,exists_c3".SplitCsv();

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    foreach (var property in fields) {
                        Assert.AreEqual(
                            property.StartsWith("exists") ? typeof(bool?) : typeof(int?),
                            Boxing.GetBoxedType(eventType.GetPropertyType(property)));
                    }
                });

            send.Invoke(typename, env, 1, 2, 3, 4);
            env.AssertEventNew(
                "s0",
                @event => {
                    EPAssertionUtil.AssertProps(@event, fields, new object[] { 1, true, 2, true, 3, true, 4, true });
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                });

            send.Invoke(typename, env, 10, 5, 50, 400);
            env.AssertPropsNew("s0", fields, new object[] { 10, true, 5, true, 50, true, 400, true });

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionEventInvalidProp(EventBean @event)
        {
            foreach (var prop in Arrays.AsList("L2", "L2[0]", "L1[0].L3", "L1[0].L2[0].L3[0].x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            bool preconfigured,
            string typeName,
            FunctionSendEvent4IntWArrayNested send,
            Type nestedClass,
            string fragmentTypeName)
        {
            var eventType = preconfigured
                ? env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName)
                : env.Runtime.EventTypeService.GetEventType(env.DeploymentId("types"), typeName);

            var expectedType = new[] {
                new object[] { "L1", nestedClass, fragmentTypeName, true }
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new[] { "L1" }, eventType.PropertyNames);

            foreach (var prop in Arrays.AsList("L1[0]", "L1[0].Lvl1", "L1[0].L2", "L1[0].L2[0]", "L1[0].L2[0].Lvl2")) {
                Assert.IsNotNull(eventType.GetGetter(prop));
                Assert.IsTrue(eventType.IsProperty(prop));
            }

            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventType.GetPropertyType("L1"), nestedClass));
            foreach (var prop in Arrays.AsList("L1[0].Lvl1", "L1[0].L2[0].Lvl2", "L1[0].L2[0].L3[0].Lvl3")) {
                Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType(prop)));
            }

            var lvl1Fragment = eventType.GetFragmentType("L1");
            Assert.IsTrue(lvl1Fragment.IsIndexed);
            var isNative = typeName.Equals(BEAN_TYPENAME);
            Assert.AreEqual(isNative, lvl1Fragment.IsNative);
            Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

            var lvl2Fragment = eventType.GetFragmentType("L1[0].L2");
            Assert.IsTrue(lvl2Fragment.IsIndexed);
            Assert.AreEqual(isNative, lvl2Fragment.IsNative);

            var received = eventType.GetPropertyDescriptor("L1");

            var componentType = typeName.Equals(AVRO_TYPENAME) ? typeof(GenericRecord) : null;
            if (nestedClass.IsArray) {
                componentType = nestedClass.GetElementType();
            }

            if (typeName.Equals(JSON_TYPENAME)) {
                nestedClass = received.PropertyType;
                componentType = received.PropertyComponentType;
            }

            AssertPropEquals(
                new SupportEventPropDesc("L1", nestedClass)
                    .WithComponentType(componentType)
                    .WithIndexed()
                    .WithFragment(),
                received);
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            foreach (var prop in Arrays.AsList(
                         "L2[0]",
                         "L1[0].L3",
                         "L1[0].Lvl1.Lvl1",
                         "L1[0].L2.L4",
                         "L1[0].L2[0].xx",
                         "L1[0].L2[0].L3[0].lvl5")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }

        private static readonly FunctionSendEvent4IntWArrayNested FBEAN = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = new InfraNestedIndexedPropLvl4(lvl4);
            var l3 = new InfraNestedIndexedPropLvl3(new[] { l4 }, lvl3);
            var l2 = new InfraNestedIndexedPropLvl2(new[] { l3 }, lvl2);
            var l1 = new InfraNestedIndexedPropLvl1(new[] { l2 }, lvl1);
            var top = new InfraNestedIndexPropTop(new[] { l1 });
            env.SendEventBean(top);
        };

        private static readonly FunctionSendEvent4IntWArrayNested FMAP = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = Collections.SingletonDataMap("Lvl4", lvl4);
            var l3 = TwoEntryMap<string, object>("L4", new[] { l4 }, "Lvl3", lvl3);
            var l2 = TwoEntryMap<string, object>("L3", new[] { l3 }, "Lvl2", lvl2);
            var l1 = TwoEntryMap<string, object>("L2", new[] { l2 }, "Lvl1", lvl1);
            var top = Collections.SingletonDataMap("L1", new[] { l1 });
            env.SendEventMap(top, eventTypeName);
        };

        private static readonly FunctionSendEvent4IntWArrayNested FOA = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = new object[] { lvl4 };
            var l3 = new object[] { new object[] { l4 }, lvl3 };
            var l2 = new object[] { new object[] { l3 }, lvl2 };
            var l1 = new object[] { new object[] { l2 }, lvl1 };
            var top = new object[] { new object[] { l1 } };
            env.SendEventObjectArray(top, eventTypeName);
        };

        private static readonly FunctionSendEvent4IntWArrayNested FXML = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                      "<myevent>\n" +
                      "\t<L1 Lvl1=\"${lvl1}\">\n" +
                      "\t\t<L2 Lvl2=\"${lvl2}\">\n" +
                      "\t\t\t<L3 Lvl3=\"${lvl3}\">\n" +
                      "\t\t\t\t<L4 Lvl4=\"${lvl4}\">\n" +
                      "\t\t\t\t</L4>\n" +
                      "\t\t\t</L3>\n" +
                      "\t\t</L2>\n" +
                      "\t</L1>\n" +
                      "</myevent>";
            xml = xml.Replace("${lvl1}", Convert.ToString(lvl1));
            xml = xml.Replace("${lvl2}", Convert.ToString(lvl2));
            xml = xml.Replace("${lvl3}", Convert.ToString(lvl3));
            xml = xml.Replace("${lvl4}", Convert.ToString(lvl4));
            try {
                SupportXML.SendXMLEvent(env, xml, eventTypeName);
            }
            catch (Exception e) {
                throw new EPRuntimeException(e);
            }
        };

        private static readonly FunctionSendEvent4IntWArrayNested FAVRO = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
            var lvl1Schema = schema.GetField("L1").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl2Schema = lvl1Schema.GetField("L2").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl3Schema = lvl2Schema.GetField("L3").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl4Schema = lvl3Schema.GetField("L4").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl4Rec = new GenericRecord(lvl4Schema);
            lvl4Rec.Put("Lvl4", lvl4);
            var lvl3Rec = new GenericRecord(lvl3Schema);
            lvl3Rec.Put("L4", Collections.SingletonList(lvl4Rec));
            lvl3Rec.Put("Lvl3", lvl3);
            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("L3", Collections.SingletonList(lvl3Rec));
            lvl2Rec.Put("Lvl2", lvl2);
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("L2", Collections.SingletonList(lvl2Rec));
            lvl1Rec.Put("Lvl1", lvl1);
            var datum = new GenericRecord(schema);
            datum.Put("L1", Collections.SingletonList(lvl1Rec));
            env.SendEventAvro(datum, eventTypeName);
        };

        private static readonly FunctionSendEvent4IntWArrayNested FJSON = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var json = "{\n" +
                       "  \"L1\": [\n" +
                       "    {\n" +
                       "      \"Lvl1\": \"${lvl1}\",\n" +
                       "      \"L2\": [\n" +
                       "        {\n" +
                       "          \"Lvl2\": \"${lvl2}\",\n" +
                       "          \"L3\": [\n" +
                       "            {\n" +
                       "              \"Lvl3\": \"${lvl3}\",\n" +
                       "              \"L4\": [\n" +
                       "                {\n" +
                       "                  \"Lvl4\": \"${lvl4}\"\n" +
                       "                }\n" +
                       "              ]\n" +
                       "            }\n" +
                       "          ]\n" +
                       "        }\n" +
                       "      ]\n" +
                       "    }\n" +
                       "  ]\n" +
                       "}";
            json = json.Replace("${lvl1}", Convert.ToString(lvl1));
            json = json.Replace("${lvl2}", Convert.ToString(lvl2));
            json = json.Replace("${lvl3}", Convert.ToString(lvl3));
            json = json.Replace("${lvl4}", Convert.ToString(lvl4));
            env.SendEventJson(json, eventTypeName);
        };

        public delegate void FunctionSendEvent4IntWArrayNested(
            string eventTypeName,
            RegressionEnvironment env,
            int lvl1,
            int lvl2,
            int lvl3,
            int lvl4);

        public class InfraNestedIndexPropTop
        {
            public InfraNestedIndexPropTop(InfraNestedIndexedPropLvl1[] l1)
            {
                L1 = l1;
            }

            public InfraNestedIndexedPropLvl1[] L1 { get; }
        }

        public class InfraNestedIndexedPropLvl1
        {
            public InfraNestedIndexedPropLvl1(
                InfraNestedIndexedPropLvl2[] l2,
                int lvl1)
            {
                L2 = l2;
                Lvl1 = lvl1;
            }

            public InfraNestedIndexedPropLvl2[] L2 { get; }

            public int Lvl1 { get; }
        }

        public class InfraNestedIndexedPropLvl2
        {
            public InfraNestedIndexedPropLvl2(
                InfraNestedIndexedPropLvl3[] l3,
                int lvl2)
            {
                L3 = l3;
                Lvl2 = lvl2;
            }

            public InfraNestedIndexedPropLvl3[] L3 { get; }

            public int Lvl2 { get; }
        }

        public class InfraNestedIndexedPropLvl3
        {
            public InfraNestedIndexedPropLvl3(
                InfraNestedIndexedPropLvl4[] l4,
                int lvl3)
            {
                L4 = l4;
                Lvl3 = lvl3;
            }

            public InfraNestedIndexedPropLvl4[] L4 { get; }

            public int Lvl3 { get; }
        }

        public class InfraNestedIndexedPropLvl4
        {
            public InfraNestedIndexedPropLvl4(int lvl4)
            {
                Lvl4 = lvl4;
            }

            public int Lvl4 { get; }
        }

        public class MyLocalJSONProvidedTop
        {
            public MyLocalJSONProvidedLvl1[] L1;
        }

        public class MyLocalJSONProvidedLvl1
        {
            public MyLocalJSONProvidedLvl2[] L2;
            public int Lvl1;
        }

        public class MyLocalJSONProvidedLvl2
        {
            public MyLocalJSONProvidedLvl3[] L3;
            public int Lvl2;
        }

        public class MyLocalJSONProvidedLvl3
        {
            public MyLocalJSONProvidedLvl4[] L4;
            public int Lvl3;
        }

        public class MyLocalJSONProvidedLvl4
        {
            public int Lvl4;
        }
    }
} // end of namespace