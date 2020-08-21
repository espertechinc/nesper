///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.suite.@event.map.EventMapCore;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.objectarray
{
    public class EventObjectArrayEventNested
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventObjectArrayArrayProperty());
            execs.Add(new EventObjectArrayMappedProperty());
            execs.Add(new EventObjectArrayMapNamePropertyNested());
            execs.Add(new EventObjectArrayMapNameProperty());
            execs.Add(new EventObjectArrayObjectArrayNested());
            return execs;
        }

        internal class EventObjectArrayArrayProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test map containing first-level property that is an array of primitive or Class
                env.CompileDeploy(
                    "@Name('s0') select P0[0] as a, P0[1] as b, P1[0].IntPrimitive as c, P1[1] as d, P0 as e from MyArrayOA");
                env.AddListener("s0");

                int[] p0 = {1, 2, 3};
                SupportBean[] beans = {new SupportBean("e1", 5), new SupportBean("e2", 6)};
                object[] eventData = {p0, beans};
                env.SendEventObjectArray(eventData, "MyArrayOA");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a","b","c","d","e" },
                    new object[] {1, 2, 5, beans[1], p0});
                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("e"));
                env.UndeployAll();

                // test map at the second level of a nested map that is an array of primitive or Class
                env.CompileDeploy(
                    "@Name('s0') select " +
                    "outer.P0[0] as a, " +
                    "outer.P0[1] as b, " +
                    "outer.P1[0].IntPrimitive as c, " +
                    "outer.P1[1] as d, " +
                    "outer.P0 as e " +
                    "from MyArrayOAMapOuter");
                env.AddListener("s0");

                env.SendEventObjectArray(new object[] {eventData}, "MyArrayOAMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a","b","c","d" },
                    new object[] {1, 2, 5, beans[1]});
                eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("e"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayMappedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test map containing first-level property that is an array of primitive or Class
                env.CompileDeploy("@Name('s0') select P0('k1') as a from MyMappedPropertyMap");
                env.AddListener("s0");

                IDictionary<string, object> eventVal = new Dictionary<string, object>();
                eventVal.Put("k1", "v1");
                var theEvent = MakeMap(
                    new[] {
                        new object[] {"P0", eventVal}
                    });
                env.SendEventMap(theEvent, "MyMappedPropertyMap");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a" },
                    new object[] {"v1"});
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.GetPropertyType("a"));
                env.UndeployAll();

                // test map at the second level of a nested map that is an array of primitive or Class
                env.CompileDeploy("@Name('s0') select outer.P0('k1') as a from MyMappedPropertyMapOuter");
                env.AddListener("s0");

                var eventOuter = MakeMap(
                    new[] {
                        new object[] {"outer", theEvent}
                    });
                env.SendEventMap(eventOuter, "MyMappedPropertyMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"a"},
                    new object[] {"v1"}
                );
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.GetPropertyType("a"));
                env.UndeployModuleContaining("s0");

                // test map that contains a bean which has a map property
                env.CompileDeploy(
                        "@Name('s0') select outerTwo.MapProperty('xOne') as a from MyMappedPropertyMapOuterTwo")
                    .AddListener("s0");

                var eventOuterTwo = MakeMap(
                    new[] {
                        new object[] {"outerTwo", SupportBeanComplexProps.MakeDefaultBean()}
                    });
                env.SendEventMap(eventOuterTwo, "MyMappedPropertyMapOuterTwo");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a" },
                    new object[] {"yOne"});
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.GetPropertyType("a"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayMapNamePropertyNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test named-map at the second level of a nested map
                env.CompileDeploy(
                    "@Name('s0') select " +
                    "outer.p0.n0 as a, " +
                    "outer.p1[0].n0 as b, " +
                    "outer.p1[1].n0 as c, " +
                    "outer.p0 as d, " +
                    "outer.p1 as e " +
                    "from MyObjectArrayMapOuter");
                env.AddListener("s0");

                var n0Bean1 = MakeMap(
                    new[] {
                        new object[] {"n0", 1}
                    });
                var n0Bean21 = MakeMap(
                    new[] {
                        new object[] {"n0", 2}
                    });
                var n0Bean22 = MakeMap(
                    new[] {
                        new object[] {"n0", 3}
                    });
                IDictionary<string, object>[] n0Bean2 = {n0Bean21, n0Bean22};
                var theEvent = MakeMap(
                    new[] {
                        new object[] {"p0", n0Bean1},
                        new object[] {"p1", n0Bean2}
                    });
                env.SendEventObjectArray(new object[] {theEvent}, "MyObjectArrayMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a","b","c","d","e" },
                    new object[] {1, 2, 3, n0Bean1, n0Bean2});
                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(IDictionary<string, object>[]), eventType.GetPropertyType("e"));

                env.UndeployAll();
                env.CompileDeploy(
                    "@Name('s0') select outer.p0.n0? as a, outer.p1[0].n0? as b, outer.p1[1]?.n0 as c, outer.p0? as d, outer.p1? as e from MyObjectArrayMapOuter");
                env.AddListener("s0");

                env.SendEventObjectArray(new object[] {theEvent}, "MyObjectArrayMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a","b","c","d","e" },
                    new object[] {1, 2, 3, n0Bean1, n0Bean2});
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("a"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayMapNameProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select p0.n0 as a, p1[0].n0 as b, p1[1].n0 as c, p0 as d, p1 as e from MyOAWithAMap");
                env.AddListener("s0");

                var n0Bean1 = MakeMap(
                    new[] {
                        new object[] {"n0", 1}
                    });
                var n0Bean21 = MakeMap(
                    new[] {
                        new object[] {"n0", 2}
                    });
                var n0Bean22 = MakeMap(
                    new[] {
                        new object[] {"n0", 3}
                    });
                IDictionary<string, object>[] n0Bean2 = {n0Bean21, n0Bean22};
                env.SendEventObjectArray(new object[] {n0Bean1, n0Bean2}, "MyOAWithAMap");

                var eventResult = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    eventResult,
                    new [] { "a","b","c","d" },
                    new object[] {1, 2, 3, n0Bean1});
                var valueE = (IDictionary<string, object>[]) eventResult.Get("e");
                Assert.AreEqual(valueE[0], n0Bean2[0]);
                Assert.AreEqual(valueE[1], n0Bean2[1]);

                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(IDictionary<string, object>[]), eventType.GetPropertyType("e"));

                env.UndeployAll();
            }
        }

        internal class EventObjectArrayObjectArrayNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from TypeRoot#lastevent");

                object[] dataLev1 = {1000};
                object[] dataLev0 = {100, dataLev1};
                env.SendEventObjectArray(new object[] {10, dataLev0}, "TypeRoot");
                var theEvent = env.GetEnumerator("s0").Advance();
                EPAssertionUtil.AssertProps(
                    theEvent,
                    new [] { "rootId","p0.p0id","p0.p1.p1id" },
                    new object[] {10, 100, 1000});

                env.UndeployAll();
            }
        }
    }
} // end of namespace