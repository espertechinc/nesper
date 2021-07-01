///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapCore
    {
        private static readonly IDictionary<string, object> map;

        static EventMapCore()
        {
            map = new Dictionary<string, object>();
            map.Put("MyInt", 3);
            map.Put("MyString", "some string");
            map.Put("beanA", SupportBeanComplexProps.MakeDefaultBean());
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventMapCoreMapNestedEventType());
            execs.Add(new EventMapCoreMetadata());
            execs.Add(new EventMapCoreNestedObjects());
            execs.Add(new EventMapCoreQueryFields());
            execs.Add(new EventMapCoreInvalidStatement());
            return execs;
        }

        public static IDictionary<string, object> MakeMap(string nameValuePairs)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            var elements = nameValuePairs.SplitCsv();
            for (var i = 0; i < elements.Length; i++) {
                var pair = elements[i].Split('=');
                if (pair.Length == 2) {
                    result.Put(pair[0], pair[1]);
                }
            }

            return result;
        }

        public static IDictionary<string, object> MakeMap(object[][] entries)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            if (entries == null) {
                return result;
            }

            for (var i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }

            return result;
        }

        public static Properties MakeProperties(object[][] entries)
        {
            var result = new Properties();
            for (var i = 0; i < entries.Length; i++) {
                var clazz = (Type) entries[i][1];
                result.Put((string) entries[i][0], clazz.FullName);
            }

            return result;
        }

        public static object GetNestedKeyMap(
            IDictionary<string, object> root,
            string keyOne,
            string keyTwo)
        {
            var map = (IDictionary<string, object>) root.Get(keyOne);
            return map.Get(keyTwo);
        }

        public static object GetNestedKeyMap(
            IDictionary<string, object> root,
            string keyOne,
            string keyTwo,
            string keyThree)
        {
            var map = (IDictionary<string, object>) root.Get(keyOne);
            map = (IDictionary<string, object>) map.Get(keyTwo);
            return map.Get(keyThree);
        }

        internal class EventMapCoreMapNestedEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Assert.IsNotNull(env.Runtime.EventTypeService.GetEventTypePreconfigured("MyMap"));
                env.CompileDeploy("@Name('s0') select lev0name.lev1name.sb.TheString as val from MyMap")
                    .AddListener("s0");

                IDictionary<string, object> lev2data = new Dictionary<string, object>();
                lev2data.Put("sb", new SupportBean("E1", 0));
                IDictionary<string, object> lev1data = new Dictionary<string, object>();
                lev1data.Put("lev1name", lev2data);
                IDictionary<string, object> lev0data = new Dictionary<string, object>();
                lev0data.Put("lev0name", lev1data);

                env.SendEventMap(lev0data, "MyMap");
                Assert.AreEqual("E1", env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                try {
                    env.SendEventObjectArray(new object[0], "MyMap");
                    Assert.Fail();
                }
                catch (EPException ex) {
                    Assert.AreEqual(
                        "Event type named 'MyMap' has not been defined or is not a Object-array event type, the name 'MyMap' refers to a System.Collections.Generic.IDictionary<System.String, System.Object> event type",
                        ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class EventMapCoreMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("myMapEvent");
                Assert.AreEqual(EventTypeApplicationType.MAP, type.Metadata.ApplicationType);
                Assert.AreEqual("myMapEvent", type.Metadata.Name);

                EPAssertionUtil.AssertEqualsAnyOrder(
                    new EventPropertyDescriptor[] {
                        new EventPropertyDescriptor("MyInt", typeof(int?), null, false, false, false, false, false),
                        new EventPropertyDescriptor(
                            "MyString",
                            typeof(string),
                            typeof(char),
                            false,
                            false,
                            true,
                            false,
                            false),
                        new EventPropertyDescriptor(
                            "beanA",
                            typeof(SupportBeanComplexProps),
                            null,
                            false,
                            false,
                            false,
                            false,
                            true),
                        new EventPropertyDescriptor(
                            "MyStringArray",
                            typeof(string[]),
                            typeof(string),
                            false,
                            false,
                            true,
                            false,
                            false)
                    },
                    type.PropertyDescriptors);
            }
        }

        internal class EventMapCoreNestedObjects : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@Name('s0') select beanA.SimpleProperty as simple," +
                                    "beanA.Nested.NestedValue as nested," +
                                    "beanA.Indexed[1] as indexed," +
                                    "beanA.Nested.NestedNested.NestedNestedValue as nestednested " +
                                    "from myMapEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventMap(map, "myMapEvent");
                Assert.AreEqual("NestedValue", env.Listener("s0").LastNewData[0].Get("nested"));
                Assert.AreEqual(2, env.Listener("s0").LastNewData[0].Get("indexed"));
                Assert.AreEqual("NestedNestedValue", env.Listener("s0").LastNewData[0].Get("nestednested"));

                env.UndeployAll();
            }
        }

        internal class EventMapCoreQueryFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@Name('s0') select MyInt as intVal, MyString as stringVal from myMapEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                // send Map<String, Object> event
                env.SendEventMap(map, "myMapEvent");
                Assert.AreEqual(3, env.Listener("s0").LastNewData[0].Get("intVal"));
                Assert.AreEqual("some string", env.Listener("s0").LastNewData[0].Get("stringVal"));

                // send Map base event
                IDictionary<string, object> mapNoType = new Dictionary<string, object>();
                mapNoType.Put("MyInt", 4);
                mapNoType.Put("MyString", "string2");
                env.SendEventMap(mapNoType, "myMapEvent");
                Assert.AreEqual(4, env.Listener("s0").LastNewData[0].Get("intVal"));
                Assert.AreEqual("string2", env.Listener("s0").LastNewData[0].Get("stringVal"));

                env.UndeployAll();
            }
        }

        internal class EventMapCoreInvalidStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(env, "select XXX from myMapEvent#length(5)", "skip");
                TryInvalidCompile(env, "select myString * 2 from myMapEvent#length(5)", "skip");
                TryInvalidCompile(env, "select String.trim(myInt) from myMapEvent#length(5)", "skip");
            }
        }
    }
} // end of namespace