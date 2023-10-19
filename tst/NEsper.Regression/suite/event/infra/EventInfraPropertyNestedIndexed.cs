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

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            RunAssertion(
                env,
                true,
                BEAN_TYPENAME,
                FBEAN,
                typeof(InfraNestedIndexedPropLvl1),
                typeof(InfraNestedIndexedPropLvl1).FullName,
                path);
            RunAssertion(env, true, MAP_TYPENAME, FMAP, typeof(IDictionary<string, object>), MAP_TYPENAME + "_1", path);
            RunAssertion(env, true, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1", path);
            RunAssertion(env, true, XML_TYPENAME, FXML, typeof(XmlNode), XML_TYPENAME + ".l1", path);
            RunAssertion(env, true, AVRO_TYPENAME, FAVRO, typeof(GenericRecord), AVRO_TYPENAME + "_1", path);

            // Json
            var eplJson =
                "create json schema " +
                JSON_TYPENAME +
                "_4(lvl4 int);\n" +
                " create json schema " +
                JSON_TYPENAME +
                "_3(lvl3 int, l4 " +
                JSON_TYPENAME +
                "_4[]);\n" +
                " create json schema " +
                JSON_TYPENAME +
                "_2(lvl2 int, l3 " +
                JSON_TYPENAME +
                "_3[]);\n" +
                " create json schema " +
                JSON_TYPENAME +
                "_1(lvl1 int, l2 " +
                JSON_TYPENAME +
                "_2[]);\n" +
                "@name('types') @public @buseventtype" +
                " create json schema " +
                JSON_TYPENAME +
                "(l1 " +
                JSON_TYPENAME +
                "_1[]);\n";
            env.CompileDeploy(eplJson, path);
            RunAssertion(env, false, JSON_TYPENAME, FJSON, typeof(object[]), JSON_TYPENAME + "_1", path);
            env.UndeployModuleContaining("types");

            // Json-Class-Provided
            var eplJsonProvided =
                "@JsonSchema(className='" +
                typeof(MyLocalJSONProvidedLvl4).FullName +
                "')" +
                " create json schema " +
                JSONPROVIDED_TYPENAME +
                "_4();\n" +
                "@JsonSchema(className='" +
                typeof(MyLocalJSONProvidedLvl3).FullName +
                "')" +
                " create json schema " +
                JSONPROVIDED_TYPENAME +
                "_3(lvl3 int, l4 " +
                JSONPROVIDED_TYPENAME +
                "_4[]);\n" +
                "@JsonSchema(className='" +
                typeof(MyLocalJSONProvidedLvl2).FullName +
                "')" +
                " create json schema " +
                JSONPROVIDED_TYPENAME +
                "_2(lvl2 int, l3 " +
                JSONPROVIDED_TYPENAME +
                "_3[]);\n" +
                "@JsonSchema(className='" +
                typeof(MyLocalJSONProvidedLvl1).FullName +
                "')" +
                " create json schema " +
                JSONPROVIDED_TYPENAME +
                "_1(lvl1 int, l2 " +
                JSONPROVIDED_TYPENAME +
                "_2[]);\n" +
                "@JsonSchema(className='" +
                typeof(MyLocalJSONProvidedTop).FullName +
                "') @name('types') @public @buseventtype" +
                " create json schema " +
                JSONPROVIDED_TYPENAME +
                "(l1 " +
                JSONPROVIDED_TYPENAME +
                "_1[]);\n";

            env.CompileDeploy(eplJsonProvided, path);
            RunAssertion(
                env,
                false,
                JSONPROVIDED_TYPENAME,
                FJSON,
                typeof(MyLocalJSONProvidedLvl1[]),
                "EventInfraPropertyNestedIndexedJsonProvided_1",
                path);

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
                        "l1[0].lvl1,l1[0].l2[0].lvl2,l1[0].l2[0].l3[0].lvl3,l1[0].l2[0].l3[0].l4[0].lvl4".SplitCsv(),
                        new object[] { 1, 2, 3, 4 });
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                    var isNative = typename.Equals(BEAN_TYPENAME);
                    SupportEventTypeAssertionUtil.AssertFragments(
                        @event,
                        isNative,
                        true,
                        "l1,l1[0].l2,l1[0].l2[0].l3,l1[0].l2[0].l3[0].l4");
                    SupportEventTypeAssertionUtil.AssertFragments(
                        @event,
                        isNative,
                        false,
                        "l1[0],l1[0].l2[0],l1[0].l2[0].l3[0],l1[0].l2[0].l3[0].l4[0]");

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
                      "l1[0].lvl1 as c0, " +
                      "exists(l1[0].lvl1) as exists_c0, " +
                      "l1[0].l2[0].lvl2 as c1, " +
                      "exists(l1[0].l2[0].lvl2) as exists_c1, " +
                      "l1[0].l2[0].l3[0].lvl3 as c2, " +
                      "exists(l1[0].l2[0].l3[0].lvl3) as exists_c2, " +
                      "l1[0].l2[0].l3[0].l4[0].lvl4 as c3, " +
                      "exists(l1[0].l2[0].l3[0].l4[0].lvl4) as exists_c3 " +
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
            foreach (var prop in Arrays.AsList("l2", "l2[0]", "l1[0].l3", "l1[0].l2[0].l3[0].x")) {
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

            var expectedType = new object[][] {
                new object[] { "l1", nestedClass, fragmentTypeName, true }
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new string[] { "l1" }, eventType.PropertyNames);

            foreach (var prop in Arrays.AsList("l1[0]", "l1[0].lvl1", "l1[0].l2", "l1[0].l2[0]", "l1[0].l2[0].lvl2")) {
                Assert.IsNotNull(eventType.GetGetter(prop));
                Assert.IsTrue(eventType.IsProperty(prop));
            }

            Assert.IsTrue(TypeHelper.IsSubclassOrImplementsInterface(eventType.GetPropertyType("l1"), nestedClass));
            foreach (var prop in Arrays.AsList("l1[0].lvl1", "l1[0].l2[0].lvl2", "l1[0].l2[0].l3[0].lvl3")) {
                Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType(prop)));
            }

            var lvl1Fragment = eventType.GetFragmentType("l1");
            Assert.IsTrue(lvl1Fragment.IsIndexed);
            var isNative = typeName.Equals(BEAN_TYPENAME);
            Assert.AreEqual(isNative, lvl1Fragment.IsNative);
            Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

            var lvl2Fragment = eventType.GetFragmentType("l1[0].l2");
            Assert.IsTrue(lvl2Fragment.IsIndexed);
            Assert.AreEqual(isNative, lvl2Fragment.IsNative);

            var received = eventType.GetPropertyDescriptor("l1");

            var componentType = typeName.Equals(AVRO_TYPENAME) ? typeof(GenericRecord) : null;
            if (nestedClass.IsArray) {
                componentType = nestedClass.GetElementType();
            }

            if (typeName.Equals(JSON_TYPENAME)) {
                nestedClass = received.PropertyType;
                componentType = received.PropertyComponentType;
            }

            AssertPropEquals(
                new SupportEventPropDesc("l1", nestedClass)
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
                         "l2[0]",
                         "l1[0].l3",
                         "l1[0].lvl1.lvl1",
                         "l1[0].l2.l4",
                         "l1[0].l2[0].xx",
                         "l1[0].l2[0].l3[0].lvl5")) {
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
            var l3 = new InfraNestedIndexedPropLvl3(new InfraNestedIndexedPropLvl4[] { l4 }, lvl3);
            var l2 = new InfraNestedIndexedPropLvl2(new InfraNestedIndexedPropLvl3[] { l3 }, lvl2);
            var l1 = new InfraNestedIndexedPropLvl1(new InfraNestedIndexedPropLvl2[] { l2 }, lvl1);
            var top = new InfraNestedIndexPropTop(new InfraNestedIndexedPropLvl1[] { l1 });
            env.SendEventBean(top);
        };

        private static readonly FunctionSendEvent4IntWArrayNested FMAP = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = Collections.SingletonDataMap("lvl4", lvl4);
            var l3 = TwoEntryMap<string, object>("l4", new IDictionary<string, object>[] { l4 }, "lvl3", lvl3);
            var l2 = TwoEntryMap<string, object>("l3", new IDictionary<string, object>[] { l3 }, "lvl2", lvl2);
            var l1 = TwoEntryMap<string, object>("l2", new IDictionary<string, object>[] { l2 }, "lvl1", lvl1);
            var top = Collections.SingletonDataMap("l1", new IDictionary<string, object>[] { l1 });
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
                      "\t<l1 lvl1=\"${lvl1}\">\n" +
                      "\t\t<l2 lvl2=\"${lvl2}\">\n" +
                      "\t\t\t<l3 lvl3=\"${lvl3}\">\n" +
                      "\t\t\t\t<l4 lvl4=\"${lvl4}\">\n" +
                      "\t\t\t\t</l4>\n" +
                      "\t\t\t</l3>\n" +
                      "\t\t</l2>\n" +
                      "\t</l1>\n" +
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
            var lvl1Schema = schema.GetField("l1").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl2Schema = lvl1Schema.GetField("l2").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl3Schema = lvl2Schema.GetField("l3").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl4Schema = lvl3Schema.GetField("l4").Schema.AsArraySchema().ItemSchema.AsRecordSchema();
            var lvl4Rec = new GenericRecord(lvl4Schema);
            lvl4Rec.Put("lvl4", lvl4);
            var lvl3Rec = new GenericRecord(lvl3Schema);
            lvl3Rec.Put("l4", Collections.SingletonList(lvl4Rec));
            lvl3Rec.Put("lvl3", lvl3);
            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("l3", Collections.SingletonList(lvl3Rec));
            lvl2Rec.Put("lvl2", lvl2);
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("l2", Collections.SingletonList(lvl2Rec));
            lvl1Rec.Put("lvl1", lvl1);
            var datum = new GenericRecord(schema);
            datum.Put("l1", Collections.SingletonList(lvl1Rec));
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
                       "  \"l1\": [\n" +
                       "    {\n" +
                       "      \"lvl1\": \"${lvl1}\",\n" +
                       "      \"l2\": [\n" +
                       "        {\n" +
                       "          \"lvl2\": \"${lvl2}\",\n" +
                       "          \"l3\": [\n" +
                       "            {\n" +
                       "              \"lvl3\": \"${lvl3}\",\n" +
                       "              \"l4\": [\n" +
                       "                {\n" +
                       "                  \"lvl4\": \"${lvl4}\"\n" +
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

        [Serializable]
        public class InfraNestedIndexPropTop
        {
            private InfraNestedIndexedPropLvl1[] l1;

            public InfraNestedIndexPropTop(InfraNestedIndexedPropLvl1[] l1)
            {
                this.l1 = l1;
            }

            public InfraNestedIndexedPropLvl1[] L1 => l1;
        }

        [Serializable]
        public class InfraNestedIndexedPropLvl1
        {
            private InfraNestedIndexedPropLvl2[] l2;
            private int lvl1;

            public InfraNestedIndexedPropLvl1(
                InfraNestedIndexedPropLvl2[] l2,
                int lvl1)
            {
                this.l2 = l2;
                this.lvl1 = lvl1;
            }

            public InfraNestedIndexedPropLvl2[] L2 => l2;

            public int Lvl1 => lvl1;
        }

        [Serializable]
        public class InfraNestedIndexedPropLvl2
        {
            private InfraNestedIndexedPropLvl3[] l3;
            private int lvl2;

            public InfraNestedIndexedPropLvl2(
                InfraNestedIndexedPropLvl3[] l3,
                int lvl2)
            {
                this.l3 = l3;
                this.lvl2 = lvl2;
            }

            public InfraNestedIndexedPropLvl3[] L3 => l3;

            public int Lvl2 => lvl2;
        }

        [Serializable]
        public class InfraNestedIndexedPropLvl3
        {
            private InfraNestedIndexedPropLvl4[] l4;
            private int lvl3;

            public InfraNestedIndexedPropLvl3(
                InfraNestedIndexedPropLvl4[] l4,
                int lvl3)
            {
                this.l4 = l4;
                this.lvl3 = lvl3;
            }

            public InfraNestedIndexedPropLvl4[] L4 => l4;

            public int Lvl3 => lvl3;
        }

        [Serializable]
        public class InfraNestedIndexedPropLvl4
        {
            private int lvl4;

            public InfraNestedIndexedPropLvl4(int lvl4)
            {
                this.lvl4 = lvl4;
            }

            public int Lvl4 => lvl4;
        }

        [Serializable]
        public class MyLocalJSONProvidedTop
        {
            public MyLocalJSONProvidedLvl1[] l1;
        }

        [Serializable]
        public class MyLocalJSONProvidedLvl1
        {
            public MyLocalJSONProvidedLvl2[] l2;
            public int lvl1;
        }

        [Serializable]
        public class MyLocalJSONProvidedLvl2
        {
            public MyLocalJSONProvidedLvl3[] l3;
            public int lvl2;
        }

        [Serializable]
        public class MyLocalJSONProvidedLvl3
        {
            public MyLocalJSONProvidedLvl4[] l4;
            public int lvl3;
        }

        [Serializable]
        public class MyLocalJSONProvidedLvl4
        {
            public int lvl4;
        }
    }
} // end of namespace