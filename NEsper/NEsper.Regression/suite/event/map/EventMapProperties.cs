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

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionlib.suite.@event.map
{
    public class EventMapProperties
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EventMapArrayProperty());
            execs.Add(new EventMapMappedProperty());
            execs.Add(new EventMapMapNamePropertyNested());
            execs.Add(new EventMapMapNameProperty());
            return execs;
        }

        internal class EventMapArrayProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select p0[0] as a, p0[1] as b, p1[0].IntPrimitive as c, p1[1] as d, p0 as e from MyArrayMap");
                env.AddListener("s0");

                int[] p0 = {1, 2, 3};
                SupportBean[] beans = {new SupportBean("e1", 5), new SupportBean("e2", 6)};
                var theEvent = EventMapCore.MakeMap(new[] {new object[] {"P0", p0}, new object[] {"P1", beans}});
                env.SendEventMap(theEvent, "MyArrayMap");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b,c,d,e".SplitCsv(),
                    new object[] {1, 2, 5, beans[1], p0});
                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("e"));
                env.UndeployAll();

                env.CompileDeploy(
                    "@Name('s0') select outer.p0[0] as a, outer.p0[1] as b, outer.p1[0].IntPrimitive as c, outer.p1[1] as d, outer.p0 as e from MyArrayMapOuter");
                env.AddListener("s0");

                var eventOuter = EventMapCore.MakeMap(new[] {new object[] {"outer", theEvent}});
                env.SendEventMap(eventOuter, "MyArrayMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b,c,d".SplitCsv(),
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

        internal class EventMapMappedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select p0('k1') as a from MyMappedPropertyMap");
                env.AddListener("s0");

                IDictionary<string, object> eventVal = new Dictionary<string, object>();
                eventVal.Put("k1", "v1");
                var theEvent = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"P0", eventVal}
                    });
                env.SendEventMap(theEvent, "MyMappedPropertyMap");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a".SplitCsv(),
                    new object[] {"v1"});
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.GetPropertyType("a"));
                env.UndeployAll();

                env.CompileDeploy("@Name('s0') select outer.p0('k1') as a from MyMappedPropertyMapOuter");
                env.AddListener("s0");

                var eventOuter = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"outer", theEvent}
                    });
                env.SendEventMap(eventOuter, "MyMappedPropertyMapOuter");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a".SplitCsv(),
                    new object[] {"v1"});
                Assert.AreEqual(typeof(object), env.Statement("s0").EventType.GetPropertyType("a"));
                env.UndeployModuleContaining("s0");

                // test map that contains a bean which has a map property
                env.CompileDeploy(
                    "@Name('s0') select outerTwo.mapProperty('xOne') as a from MyMappedPropertyMapOuterTwo");
                env.AddListener("s0");

                var eventOuterTwo = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"outerTwo", SupportBeanComplexProps.MakeDefaultBean()}
                    });
                env.SendEventMap(eventOuterTwo, "MyMappedPropertyMapOuterTwo");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a".SplitCsv(),
                    new object[] {"yOne"});
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("a"));

                env.UndeployAll();
            }
        }

        internal class EventMapMapNamePropertyNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select outer.p0.n0 as a, outer.p1[0].n0 as b, outer.p1[1].n0 as c, outer.p0 as d, outer.p1 as e from MyArrayMapTwo");
                env.AddListener("s0");

                var n0Bean1 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 1}
                    });
                var n0Bean21 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 2}
                    });
                var n0Bean22 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 3}
                    });
                IDictionary<string, object>[] n0Bean2 = {n0Bean21, n0Bean22};
                var theEvent = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"P0", n0Bean1}, new object[] {"P1", n0Bean2}
                    });
                var eventOuter = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"outer", theEvent}
                    });
                env.SendEventMap(eventOuter, "MyArrayMapTwo");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b,c,d,e".SplitCsv(),
                    new object[] {1, 2, 3, n0Bean1, n0Bean2});
                var eventType = env.Statement("s0").EventType;
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("d"));
                Assert.AreEqual(typeof(IDictionary<string, object>[]), eventType.GetPropertyType("e"));

                env.UndeployAll();
                env.CompileDeploy(
                    "@Name('s0') select outer.p0.n0? as a, outer.p1[0].n0? as b, outer.p1[1]?.n0 as c, outer.p0? as d, outer.p1? as e from MyArrayMapTwo");
                env.AddListener("s0");

                env.SendEventMap(eventOuter, "MyArrayMapTwo");

                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a,b,c,d,e".SplitCsv(),
                    new object[] {1, 2, 3, n0Bean1, n0Bean2});
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("a"));

                env.UndeployAll();
            }
        }

        internal class EventMapMapNameProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select p0.n0 as a, p1[0].n0 as b, p1[1].n0 as c, p0 as d, p1 as e from MyMapWithAMap");
                env.AddListener("s0");

                var n0Bean1 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 1}
                    });
                var n0Bean21 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 2}
                    });
                var n0Bean22 = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"n0", 3}
                    });
                IDictionary<string, object>[] n0Bean2 = {n0Bean21, n0Bean22};
                var theEvent = EventMapCore.MakeMap(
                    new[] {
                        new object[] {"P0", n0Bean1}, new object[] {"P1", n0Bean2}
                    });
                env.SendEventMap(theEvent, "MyMapWithAMap");

                var eventResult = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(
                    eventResult,
                    "a,b,c,d".SplitCsv(),
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
    }
} // end of namespace