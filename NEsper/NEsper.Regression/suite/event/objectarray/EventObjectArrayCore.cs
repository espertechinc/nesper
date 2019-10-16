///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayCore
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventObjectArrayMetadata());
            execs.Add(new EventObjectArrayNestedObjects());
            execs.Add(new EventObjectArrayQueryFields());
            execs.Add(new EventObjectArrayNestedEventBeanArray());
            execs.Add(new EventObjectArrayInvalid());
            return execs;
        }

        public static object GetNestedKeyOA(
            object[] array,
            int index,
            string keyTwo)
        {
            var map = (IDictionary<string, object>) array[index];
            return map.Get(keyTwo);
        }

        public static object GetNestedKeyOA(
            object[] array,
            int index,
            string keyTwo,
            string keyThree)
        {
            var map = (IDictionary<string, object>) array[index];
            map = (IDictionary<string, object>) map.Get(keyTwo);
            return map.Get(keyThree);
        }

        internal class EventObjectArrayNestedEventBeanArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var schemas = "create objectarray schema NBAL_1(val string);\n" +
                              "create objectarray schema NBAL_2 (lvl1s NBAL_1[]);\n";
                env.CompileDeployWBusPublicType(schemas, path);

                env.CompileDeploy("@Name('s0') select * from NBAL_1", path).AddListener("s0");

                env.SendEventObjectArray(new object[] {"somevalue"}, "NBAL_1");
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                env.UndeployModuleContaining("s0");

                // add containing-type
                env.CompileDeploy("@Name('s0') select lvl1s[0] as c0 from NBAL_2", path).AddListener("s0");

                env.SendEventObjectArray(new object[] {new[] {@event.Underlying}}, "NBAL_2");
                Assert.AreEqual("somevalue", ((object[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c0"))[0]);

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayMetadata : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var type = env.Runtime.EventTypeService.GetEventTypePreconfigured("MyObjectArrayEvent");
                Assert.AreEqual(EventTypeApplicationType.OBJECTARR, type.Metadata.ApplicationType);
                Assert.AreEqual("MyObjectArrayEvent", type.Metadata.Name);

                EPAssertionUtil.AssertEqualsAnyOrder(
                    new EventPropertyDescriptor[] {
                        new EventPropertyDescriptor("MyInt", typeof(int?), null, false, false, false, false, false),
                        new EventPropertyDescriptor(
                            "MyString",
                            typeof(string),
                            null,
                            false,
                            false,
                            false,
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
                            true)
                    },
                    type.PropertyDescriptors);
            }
        }

        internal class EventObjectArrayNestedObjects : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText = "@Name('s0') select beanA.SimpleProperty as simple," +
                                    "beanA.Nested.NestedValue as nested," +
                                    "beanA.Indexed[1] as indexed," +
                                    "beanA.Nested.NestedNested.NestedNestedValue as nestednested " +
                                    "from MyObjectArrayEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventObjectArray(
                    new object[] {3, "some string", SupportBeanComplexProps.MakeDefaultBean()},
                    "MyObjectArrayEvent");
                Assert.AreEqual("NestedValue", env.Listener("s0").LastNewData[0].Get("nested"));
                Assert.AreEqual(2, env.Listener("s0").LastNewData[0].Get("Indexed"));
                Assert.AreEqual("nestedNestedValue", env.Listener("s0").LastNewData[0].Get("nestednested"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayQueryFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var statementText =
                    "@Name('s0') select myInt + 2 as intVal, 'x' || myString || 'x' as stringVal from MyObjectArrayEvent#length(5)";
                env.CompileDeploy(statementText).AddListener("s0");

                // send Map<String, Object> event
                env.SendEventObjectArray(
                    new object[] {3, "some string", SupportBeanComplexProps.MakeDefaultBean()},
                    "MyObjectArrayEvent");
                Assert.AreEqual(5, env.Listener("s0").LastNewData[0].Get("intVal"));
                Assert.AreEqual("xsome stringx", env.Listener("s0").LastNewData[0].Get("stringVal"));

                // send Map base event
                env.SendEventObjectArray(new object[] {4, "string2", null}, "MyObjectArrayEvent");
                Assert.AreEqual(6, env.Listener("s0").LastNewData[0].Get("intVal"));
                Assert.AreEqual("xstring2x", env.Listener("s0").LastNewData[0].Get("stringVal"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(env, "select XXX from MyObjectArrayEvent#length(5)", "skip");
                TryInvalidCompile(env, "select myString * 2 from MyObjectArrayEvent#length(5)", "skip");
                TryInvalidCompile(env, "select String.trim(myInt) from MyObjectArrayEvent#length(5)", "skip");
            }
        }
    }
} // end of namespace