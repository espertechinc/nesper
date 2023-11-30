///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.infra
{
    public class EventInfraPropertyNestedNestedEscaped : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            var eplSchema =
                "@name('types') @public @buseventtype create schema BeanLvl0 as " +
                typeof(SupportLvl0).FullName +
                ";\n" +
                "\n" +
                "create schema MapLvl3(vlvl3 string);\n" +
                "create schema MapLvl2(vlvl2 string, lvl3 MapLvl3);\n" +
                "create schema MapLvl1(lvl2 MapLvl2);\n" +
                "@public @buseventtype create schema MapLvl0(lvl1 MapLvl1);\n" +
                "\n" +
                "create objectarray schema OALvl3(vlvl3 string);\n" +
                "create objectarray schema OALvl2(vlvl2 string, vlvl2dyn string, lvl3 OALvl3);\n" +
                "create objectarray schema OALvl1(lvl2 OALvl2);\n" +
                "@public @buseventtype create objectarray schema OALvl0(lvl1 OALvl1);\n" +
                "\n" +
                "@JsonSchema(Dynamic=true) create json schema JSONLvl3(vlvl3 string);\n" +
                "@JsonSchema(Dynamic=true) create json schema JSONLvl2(vlvl2 string, lvl3 JSONLvl3);\n" +
                "create json schema JSONLvl1(lvl2 JSONLvl2);\n" +
                "@public @buseventtype create json schema JSONLvl0(lvl1 JSONLvl1);\n" +
                "\n" +
                "create avro schema AvroLvl3(vlvl3 string);\n" +
                "create avro schema AvroLvl2(vlvl2 string, vlvl2dyn string, lvl3 AvroLvl3);\n" +
                "create avro schema AvroLvl1(lvl2 AvroLvl2);\n" +
                "@public @buseventtype create avro schema AvroLvl0(lvl1 AvroLvl1);\n";

            env.CompileDeploy(eplSchema, path);

            RunAssertion(env, path, "BeanLvl0", FBEAN);
            RunAssertion(env, path, "MapLvl0", FMAP);
            RunAssertion(env, path, "OALvl0", FOA);
            RunAssertion(env, path, "JSONLvl0", FJSON);
            RunAssertion(env, path, "AvroLvl0", FAVRO);

            env.UndeployAll();
        }

        private void RunAssertion(
            RegressionEnvironment env,
            RegressionPath path,
            string eventTypeName,
            FunctionSendEvent sendEvent)
        {
            RunAssertionGetter(env, path, eventTypeName, sendEvent);
            RunAssertionSelect(env, path, eventTypeName, sendEvent);
        }

        private void RunAssertionSelect(
            RegressionEnvironment env,
            RegressionPath path,
            string eventTypeName,
            FunctionSendEvent sendEvent)
        {
            env.CompileDeploy(
                    "@name('s0') select " +
                    "lvl1.lvl2.`vlvl2` as c0, " +
                    "lvl1.lvl2.lvl3.`vlvl3` as c1, " +
                    "`lvl1`.`lvl2`.`lvl3`.`vlvl3` as c2, " +
                    "lvl1.lvl2.`vlvl2dyn`? as c3 " +
                    " from " +
                    eventTypeName,
                    path)
                .AddListener("s0");

            sendEvent.Invoke(eventTypeName, env, "v2", "v2dyn", "v3");
            env.AssertPropsNew("s0", "c0,c1,c2,c3".Split(","), new object[] { "v2", "v3", "v3", "v2dyn" });

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionGetter(
            RegressionEnvironment env,
            RegressionPath path,
            string eventTypeName,
            FunctionSendEvent sendEvent)
        {
            env.CompileDeploy("@name('s0') select * from " + eventTypeName, path).AddListener("s0");
            sendEvent.Invoke(eventTypeName, env, "v2", "v2dyn", "v3");

            env.AssertEventNew(
                "s0",
                @event => {
                    AssertProperty(@event, "v2", "lvl1.lvl2.`vlvl2`");
                    AssertProperty(@event, "v3", "lvl1.lvl2.lvl3.`vlvl3`");
                    AssertProperty(@event, "v3", "`lvl1`.`lvl2`.`lvl3`.`vlvl3`");
                    AssertProperty(@event, "v2dyn", "lvl1.lvl2.`vlvl2dyn`?");

                    var fragmentName = "lvl1.lvl2.`lvl3`";
                    AssertPropertyFragment(@event, fragmentName);
                    Assert.IsNotNull(@event.EventType.GetFragmentType(fragmentName));
                });

            env.UndeployModuleContaining("s0");
        }

        private void AssertProperty(
            EventBean @event,
            string value,
            string name)
        {
            Assert.AreEqual(value, @event.Get(name));
            var eventType = @event.EventType;
            Assert.IsNotNull(eventType.GetPropertyType(name));
            Assert.IsTrue(eventType.IsProperty(name));
            Assert.IsNotNull(eventType.GetGetter(name));
        }

        private void AssertPropertyFragment(
            EventBean @event,
            string name)
        {
            Assert.IsNotNull(@event.Get(name));
            var eventType = @event.EventType;
            Assert.IsNotNull(eventType.GetPropertyType(name));
            Assert.IsTrue(eventType.IsProperty(name));
            Assert.IsNotNull(eventType.GetGetter(name));
        }

        public class SupportLvl3
        {
            private readonly string vlvl3;

            public SupportLvl3(string vlvl3)
            {
                this.vlvl3 = vlvl3;
            }

            public string Vlvl3 => vlvl3;
        }

        public class SupportLvl2
        {
            private readonly string vlvl2;
            private readonly string vlvl2dyn;
            private readonly SupportLvl3 lvl3;

            public SupportLvl2(
                string vlvl2,
                string vlvl2dyn,
                SupportLvl3 lvl3)
            {
                this.vlvl2 = vlvl2;
                this.vlvl2dyn = vlvl2dyn;
                this.lvl3 = lvl3;
            }

            public string Vlvl2 => vlvl2;

            public SupportLvl3 Lvl3 => lvl3;

            public string Vlvl2dyn => vlvl2dyn;
        }

        public class SupportLvl1
        {
            private readonly SupportLvl2 lvl2;

            public SupportLvl1(SupportLvl2 lvl2)
            {
                this.lvl2 = lvl2;
            }

            public SupportLvl2 Lvl2 => lvl2;
        }

        public class SupportLvl0
        {
            private readonly SupportLvl1 lvl1;

            public SupportLvl0(SupportLvl1 lvl1)
            {
                this.lvl1 = lvl1;
            }

            public SupportLvl1 Lvl1 => lvl1;
        }

        private static readonly FunctionSendEvent FBEAN = (
            eventTypeName,
            env,
            vlvl2,
            vlvl2dyn,
            vlvl3) => {
            var l3 = new SupportLvl3(vlvl3);
            var l2 = new SupportLvl2(vlvl2, vlvl2dyn, l3);
            var l1 = new SupportLvl1(l2);
            var l0 = new SupportLvl0(l1);
            env.SendEventBean(l0, eventTypeName);
        };

        private static readonly FunctionSendEvent FMAP = (
            eventTypeName,
            env,
            vlvl2,
            vlvl2dyn,
            vlvl3) => {
            var l3 = Collections.SingletonDataMap("vlvl3", vlvl3);
            var l2 = CollectionUtil.BuildMap("vlvl2", vlvl2, "lvl3", l3, "vlvl2dyn", vlvl2dyn);
            var l1 = Collections.SingletonDataMap("lvl2", l2);
            var l0 = Collections.SingletonDataMap("lvl1", l1);
            env.SendEventMap(l0, eventTypeName);
        };

        private static readonly FunctionSendEvent FOA = (
            eventTypeName,
            env,
            vlvl2,
            vlvl2dyn,
            vlvl3) => {
            var l3 = new object[] { vlvl3 };
            var l2 = new object[] { vlvl2, vlvl2dyn, l3 };
            var l1 = new object[] { l2 };
            var l0 = new object[] { l1 };
            env.SendEventObjectArray(l0, eventTypeName);
        };

        private static readonly FunctionSendEvent FJSON = (
            eventTypeName,
            env,
            vlvl2,
            vlvl2dyn,
            vlvl3) => {
            var lvl3 = new JObject {
                { "vlvl3", vlvl3 }
            };
            var lvl2 = new JObject {
                { "vlvl2", vlvl2 },
                { "vlvl2dyn", vlvl2dyn },
                { "lvl3", lvl3 }
            };
            var lvl1 = new JObject {
                { "lvl2", lvl2 }
            };
            var lvl0 = new JObject {
                { "lvl1", lvl1 }
            };
            env.SendEventJson(lvl0.ToString(), eventTypeName);
        };

        private static readonly FunctionSendEvent FAVRO = (
            eventTypeName,
            env,
            vlvl2,
            vlvl2dyn,
            vlvl3) => {
            var schema = env.RuntimeAvroSchemaByDeployment("types", eventTypeName).AsRecordSchema();
            var lvl1Schema = schema.GetField("lvl1").Schema.AsRecordSchema();
            var lvl2Schema = lvl1Schema.GetField("lvl2").Schema.AsRecordSchema();
            var lvl3Schema = lvl2Schema.GetField("lvl3").Schema.AsRecordSchema();
            var lvl3 = new GenericRecord(lvl3Schema);
            lvl3.Put("vlvl3", vlvl3);
            var lvl2 = new GenericRecord(lvl2Schema);
            lvl2.Put("lvl3", lvl3);
            lvl2.Put("vlvl2", vlvl2);
            lvl2.Put("vlvl2dyn", vlvl2dyn);
            var lvl1 = new GenericRecord(lvl1Schema);
            lvl1.Put("lvl2", lvl2);
            var datum = new GenericRecord(schema);
            datum.Put("lvl1", lvl1);
            env.SendEventAvro(datum, eventTypeName);
        };

        public delegate void FunctionSendEvent(
            string eventTypeName,
            RegressionEnvironment env,
            string vlvl2,
            string vlvl2dyn,
            string vlvl3);
    }
} // end of namespace