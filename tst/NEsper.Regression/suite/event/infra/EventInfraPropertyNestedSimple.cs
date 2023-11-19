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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.json;
using com.espertech.esper.regressionlib.support.util;

using NEsper.Avro.Extensions;

using static com.espertech.esper.common.@internal.support.SupportEventPropUtil; // assertPropEquals
using static com.espertech.esper.common.@internal.util.CollectionUtil; // twoEntryMap
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyNestedSimple : RegressionExecution
    {
        public const string XML_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "XML";
        public const string MAP_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Map";
        public const string OA_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "OA";
        public const string AVRO_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Avro";
        private const string BEAN_TYPENAME = nameof(InfraNestedSimplePropTop);
        private const string JSON_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "Json";
        private const string JSONPROVIDED_TYPENAME = nameof(EventInfraPropertyNestedSimple) + "JsonProvided";

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            RunAssertion(
                env,
                BEAN_TYPENAME,
                FBEAN,
                typeof(InfraNestedSimplePropLvl1),
                typeof(InfraNestedSimplePropLvl1).FullName,
                path);
            RunAssertion(env, MAP_TYPENAME, FMAP, typeof(IDictionary<string, object>), MAP_TYPENAME + "_1", path);
            RunAssertion(env, OA_TYPENAME, FOA, typeof(object[]), OA_TYPENAME + "_1", path);
            RunAssertion(env, XML_TYPENAME, FXML, typeof(XmlNode), XML_TYPENAME + ".l1", path);
            RunAssertion(env, AVRO_TYPENAME, FAVRO, typeof(GenericRecord), AVRO_TYPENAME + "_1", path);

            var epl =
                "@public create json schema " +
                JSON_TYPENAME +
                "_4(lvl4 int);\n" +
                "@public create json schema " +
                JSON_TYPENAME +
                "_3(lvl3 int, l4 " +
                JSON_TYPENAME +
                "_4);\n" +
                "@public create json schema " +
                JSON_TYPENAME +
                "_2(lvl2 int, l3 " +
                JSON_TYPENAME +
                "_3);\n" +
                "@public create json schema " +
                JSON_TYPENAME +
                "_1(lvl1 int, l2 " +
                JSON_TYPENAME +
                "_2);\n" +
                "@name('types') @public @buseventtype create json schema " +
                JSON_TYPENAME +
                "(l1 " +
                JSON_TYPENAME +
                "_1);\n";
            env.CompileDeploy(epl, path);
            RunAssertion(env, JSON_TYPENAME, FJSON, null, JSON_TYPENAME + "_1", path);

            epl = "@JsonSchema(ClassName='" +
                  typeof(MyLocalJSONProvidedTop).FullName +
                  "') @name('types') @public @buseventtype create json schema " +
                  JSONPROVIDED_TYPENAME +
                  "();\n";
            env.CompileDeploy(epl, path);
            RunAssertion(
                env,
                JSONPROVIDED_TYPENAME,
                FJSON,
                typeof(MyLocalJSONProvidedLvl1),
                typeof(MyLocalJSONProvidedLvl1).FullName,
                path);

            env.UndeployAll();
        }

        private void RunAssertion(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send,
            Type nestedClass,
            string fragmentTypeName,
            RegressionPath path)
        {
            RunAssertionSelectNested(env, typename, send, path);
            RunAssertionBeanNav(env, typename, send, path);
            env.AssertThat(
                () => {
                    var nestedType = typename.Equals(JSON_TYPENAME)
                        ? SupportJsonEventTypeUtil.GetUnderlyingType(env, "types", JSON_TYPENAME + "_1")
                        : nestedClass;
                    RunAssertionTypeValidProp(env, typename, nestedType, fragmentTypeName);
                    RunAssertionTypeInvalidProp(env, typename);
                });
        }

        private void RunAssertionBeanNav(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send,
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
                        "l1.lvl1,l1.l2.lvl2,l1.l2.l3.lvl3,l1.l2.l3.l4.lvl4".SplitCsv(),
                        new object[] { 1, 2, 3, 4 });
                    SupportEventTypeAssertionUtil.AssertConsistency(@event);
                    var nativeFragment = typename.Equals(BEAN_TYPENAME) || typename.Equals(JSONPROVIDED_TYPENAME);
                    SupportEventTypeAssertionUtil.AssertFragments(@event, nativeFragment, false, "l1.l2");
                    SupportEventTypeAssertionUtil.AssertFragments(
                        @event,
                        nativeFragment,
                        false,
                        "l1,l1.l2,l1.l2.l3,l1.l2.l3.l4");
                    RunAssertionEventInvalidProp(@event);
                });

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionSelectNested(
            RegressionEnvironment env,
            string typename,
            FunctionSendEvent4Int send,
            RegressionPath path)
        {
            var epl = "@name('s0') select " +
                      "l1.lvl1 as c0, " +
                      "exists(l1.lvl1) as exists_c0, " +
                      "l1.l2.lvl2 as c1, " +
                      "exists(l1.l2.lvl2) as exists_c1, " +
                      "l1.l2.l3.lvl3 as c2, " +
                      "exists(l1.l2.l3.lvl3) as exists_c2, " +
                      "l1.l2.l3.l4.lvl4 as c3, " +
                      "exists(l1.l2.l3.l4.lvl4) as exists_c3 " +
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
            foreach (var prop in Arrays.AsList("l2", "l1.l3", "l1.xxx", "l1.l2.x", "l1.l2.l3.x", "l1.lvl1.x")) {
                SupportMessageAssertUtil.TryInvalidProperty(@event, prop);
                SupportMessageAssertUtil.TryInvalidGetFragment(@event, prop);
            }
        }

        private void RunAssertionTypeValidProp(
            RegressionEnvironment env,
            string typeName,
            Type nestedClass,
            string fragmentTypeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            var expectedType = new object[][] { new object[] { "l1", nestedClass, fragmentTypeName, false } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());

            EPAssertionUtil.AssertEqualsAnyOrder(new string[] { "l1" }, eventType.PropertyNames);

            foreach (var prop in Arrays.AsList("l1", "l1.lvl1", "l1.l2", "l1.l2.lvl2")) {
                Assert.IsNotNull(eventType.GetGetter(prop));
                Assert.IsTrue(eventType.IsProperty(prop));
            }

            Assert.AreEqual(nestedClass, eventType.GetPropertyType("l1"));
            foreach (var prop in Arrays.AsList("l1.lvl1", "l1.l2.lvl2", "l1.l2.l3.lvl3")) {
                Assert.AreEqual(typeof(int?), Boxing.GetBoxedType(eventType.GetPropertyType(prop)));
            }

            var lvl1Fragment = eventType.GetFragmentType("l1");
            Assert.IsFalse(lvl1Fragment.IsIndexed);
            var isNative = typeName.Equals(BEAN_TYPENAME) || typeName.Equals(JSONPROVIDED_TYPENAME);
            Assert.AreEqual(isNative, lvl1Fragment.IsNative);
            Assert.AreEqual(fragmentTypeName, lvl1Fragment.FragmentType.Name);

            var lvl2Fragment = eventType.GetFragmentType("l1.l2");
            Assert.IsFalse(lvl2Fragment.IsIndexed);
            Assert.AreEqual(isNative, lvl2Fragment.IsNative);

            Type componentType = null;
            if (typeName.Equals(MAP_TYPENAME) || typeName.Equals(OA_TYPENAME) || typeName.Equals(JSON_TYPENAME)) {
                componentType = typeof(object);
            }

            AssertPropEquals(
                new SupportEventPropDesc("l1", nestedClass)
                    .WithComponentType(componentType)
                    .WithFragment()
                    .WithIndexed(false)
                    .WithMapped(false),
                eventType.GetPropertyDescriptor("l1"));
        }

        private void RunAssertionTypeInvalidProp(
            RegressionEnvironment env,
            string typeName)
        {
            var eventType = env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName);

            foreach (var prop in Arrays.AsList(
                         "l2",
                         "l1.l3",
                         "l1.lvl1.lvl1",
                         "l1.l2.l4",
                         "l1.l2.xx",
                         "l1.l2.l3.lvl5")) {
                Assert.AreEqual(false, eventType.IsProperty(prop));
                Assert.AreEqual(null, eventType.GetPropertyType(prop));
                Assert.IsNull(eventType.GetPropertyDescriptor(prop));
            }
        }

        private static readonly FunctionSendEvent4Int FMAP = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = Collections.SingletonDataMap("lvl4", lvl4);
            var l3 = TwoEntryMap<string, object>("l4", l4, "lvl3", lvl3);
            var l2 = TwoEntryMap<string, object>("l3", l3, "lvl2", lvl2);
            var l1 = TwoEntryMap<string, object>("l2", l2, "lvl1", lvl1);
            var top = Collections.SingletonDataMap("l1", l1);
            env.SendEventMap(top, eventTypeName);
        };

        private static readonly FunctionSendEvent4Int FOA = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = new object[] { lvl4 };
            var l3 = new object[] { l4, lvl3 };
            var l2 = new object[] { l3, lvl2 };
            var l1 = new object[] { l2, lvl1 };
            var top = new object[] { l1 };
            env.SendEventObjectArray(top, eventTypeName);
        };

        private static readonly FunctionSendEvent4Int FBEAN = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var l4 = new InfraNestedSimplePropLvl4(lvl4);
            var l3 = new InfraNestedSimplePropLvl3(l4, lvl3);
            var l2 = new InfraNestedSimplePropLvl2(l3, lvl2);
            var l1 = new InfraNestedSimplePropLvl1(l2, lvl1);
            var top = new InfraNestedSimplePropTop(l1);
            env.SendEventBean(top);
        };

        private static readonly FunctionSendEvent4Int FXML = (
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

        private static readonly FunctionSendEvent4Int FAVRO = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var schema = env.RuntimeAvroSchemaPreconfigured(AVRO_TYPENAME).AsRecordSchema();
            var lvl1Schema = schema.GetField("l1").Schema.AsRecordSchema();
            var lvl2Schema = lvl1Schema.GetField("l2").Schema.AsRecordSchema();
            var lvl3Schema = lvl2Schema.GetField("l3").Schema.AsRecordSchema();
            var lvl4Schema = lvl3Schema.GetField("l4").Schema.AsRecordSchema();
            var lvl4Rec = new GenericRecord(lvl4Schema);
            lvl4Rec.Put("lvl4", lvl4);
            var lvl3Rec = new GenericRecord(lvl3Schema);
            lvl3Rec.Put("l4", lvl4Rec);
            lvl3Rec.Put("lvl3", lvl3);
            var lvl2Rec = new GenericRecord(lvl2Schema);
            lvl2Rec.Put("l3", lvl3Rec);
            lvl2Rec.Put("lvl2", lvl2);
            var lvl1Rec = new GenericRecord(lvl1Schema);
            lvl1Rec.Put("l2", lvl2Rec);
            lvl1Rec.Put("lvl1", lvl1);
            var datum = new GenericRecord(schema);
            datum.Put("l1", lvl1Rec);
            env.SendEventAvro(datum, eventTypeName);
        };

        private static readonly FunctionSendEvent4Int FJSON = (
            eventTypeName,
            env,
            lvl1,
            lvl2,
            lvl3,
            lvl4) => {
            var json = "{\n" +
                       "  \"l1\": {\n" +
                       "    \"lvl1\": ${lvl1},\n" +
                       "    \"l2\": {\n" +
                       "      \"lvl2\": ${lvl2},\n" +
                       "      \"l3\": {\n" +
                       "        \"lvl3\": ${lvl3},\n" +
                       "        \"l4\": {\n" +
                       "          \"lvl4\": ${lvl4}\n" +
                       "        }\n" +
                       "      }\n" +
                       "    }\n" +
                       "  }\n" +
                       "}";
            json = json.Replace("${lvl1}", Convert.ToString(lvl1));
            json = json.Replace("${lvl2}", Convert.ToString(lvl2));
            json = json.Replace("${lvl3}", Convert.ToString(lvl3));
            json = json.Replace("${lvl4}", Convert.ToString(lvl4));
            env.SendEventJson(json, eventTypeName);
        };

        public delegate void FunctionSendEvent4Int(
            string eventTypeName,
            RegressionEnvironment env,
            int lvl1,
            int lvl2,
            int lvl3,
            int lvl4);

        public class InfraNestedSimplePropTop
        {
            private InfraNestedSimplePropLvl1 l1;

            public InfraNestedSimplePropTop(InfraNestedSimplePropLvl1 l1)
            {
                this.l1 = l1;
            }

            public InfraNestedSimplePropLvl1 L1 => l1;
        }

        public class InfraNestedSimplePropLvl1
        {
            private InfraNestedSimplePropLvl2 l2;
            private int lvl1;

            public InfraNestedSimplePropLvl1(
                InfraNestedSimplePropLvl2 l2,
                int lvl1)
            {
                this.l2 = l2;
                this.lvl1 = lvl1;
            }

            public InfraNestedSimplePropLvl2 L2 => l2;

            public int Lvl1 => lvl1;
        }

        public class InfraNestedSimplePropLvl2
        {
            private InfraNestedSimplePropLvl3 l3;
            private int lvl2;

            public InfraNestedSimplePropLvl2(
                InfraNestedSimplePropLvl3 l3,
                int lvl2)
            {
                this.l3 = l3;
                this.lvl2 = lvl2;
            }

            public InfraNestedSimplePropLvl3 L3 => l3;

            public int Lvl2 => lvl2;
        }

        public class InfraNestedSimplePropLvl3
        {
            private InfraNestedSimplePropLvl4 l4;
            private int lvl3;

            public InfraNestedSimplePropLvl3(
                InfraNestedSimplePropLvl4 l4,
                int lvl3)
            {
                this.l4 = l4;
                this.lvl3 = lvl3;
            }

            public InfraNestedSimplePropLvl4 L4 => l4;

            public int Lvl3 => lvl3;
        }

        public class InfraNestedSimplePropLvl4
        {
            private int lvl4;

            public InfraNestedSimplePropLvl4(int lvl4)
            {
                this.lvl4 = lvl4;
            }

            public int Lvl4 => lvl4;
        }

        public class MyLocalJSONProvidedTop
        {
            public MyLocalJSONProvidedLvl1 l1;
        }

        public class MyLocalJSONProvidedLvl1
        {
            public MyLocalJSONProvidedLvl2 l2;
            public int lvl1;
        }

        public class MyLocalJSONProvidedLvl2
        {
            public MyLocalJSONProvidedLvl3 l3;
            public int lvl2;
        }

        public class MyLocalJSONProvidedLvl3
        {
            public MyLocalJSONProvidedLvl4 l4;
            public int lvl3;
        }

        public class MyLocalJSONProvidedLvl4
        {
            public int lvl4;
        }
    }
} // end of namespace