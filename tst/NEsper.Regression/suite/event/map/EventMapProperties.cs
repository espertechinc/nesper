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
            WithArrayProperty(execs);
            WithMappedProperty(execs);
            WithMapNamePropertyNested(execs);
            WithMapNameProperty(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMapNameProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapMapNameProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithMapNamePropertyNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapMapNamePropertyNested());
            return execs;
        }

        public static IList<RegressionExecution> WithMappedProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapMappedProperty());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayProperty(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EventMapArrayProperty());
            return execs;
        }

        private class EventMapArrayProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select P0[0] as a, P0[1] as b, P1[0].IntPrimitive as c, P1[1] as d, P0 as e from MyArrayMap");
                env.AddListener("s0");

                var p0 = new int[] { 1, 2, 3 };
                var beans = new SupportBean[] { new SupportBean("e1", 5), new SupportBean("e2", 6) };
                var theEvent = EventMapCore.MakeMap(
                    new object[][] { new object[] { "P0", p0 }, new object[] { "P1", beans } });
                env.SendEventMap(theEvent, "MyArrayMap");

                env.AssertPropsNew("s0", "a,b,c,d,e".SplitCsv(), new object[] { 1, 2, 5, beans[1], p0 });
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                        Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("d"));
                        Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("e"));
                    });
                env.UndeployAll();

                env.CompileDeploy(
                    "@name('s0') select outer.P0[0] as a, outer.P0[1] as b, outer.P1[0].IntPrimitive as c, outer.P1[1] as d, outer.P0 as e from MyArrayMapOuter");
                env.AddListener("s0");

                var eventOuter = EventMapCore.MakeMap(new object[][] { new object[] { "outer", theEvent } });
                env.SendEventMap(eventOuter, "MyArrayMapOuter");

                env.AssertPropsNew("s0", "a,b,c,d".SplitCsv(), new object[] { 1, 2, 5, beans[1] });
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                        Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("d"));
                        Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("e"));
                    });

                env.UndeployAll();
            }
        }

        private class EventMapMappedProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select P0('k1') as a from MyMappedPropertyMap");
                env.AddListener("s0");

                IDictionary<string, object> eventVal = new Dictionary<string, object>();
                eventVal.Put("k1", "v1");
                var theEvent = EventMapCore.MakeMap(new object[][] { new object[] { "P0", eventVal } });
                env.SendEventMap(theEvent, "MyMappedPropertyMap");

                env.AssertPropsNew("s0", "a".SplitCsv(), new object[] { "v1" });
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(object), statement.EventType.GetPropertyType("a")));
                env.UndeployAll();

                env.CompileDeploy("@name('s0') select outer.P0('k1') as a from MyMappedPropertyMapOuter");
                env.AddListener("s0");

                var eventOuter = EventMapCore.MakeMap(new object[][] { new object[] { "outer", theEvent } });
                env.SendEventMap(eventOuter, "MyMappedPropertyMapOuter");

                env.AssertPropsNew("s0", "a".SplitCsv(), new object[] { "v1" });
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(object), statement.EventType.GetPropertyType("a")));
                env.UndeployModuleContaining("s0");

                // test map that contains a bean which has a map property
                env.CompileDeploy(
                    "@name('s0') select outerTwo.MapProperty('xOne') as a from MyMappedPropertyMapOuterTwo");
                env.AddListener("s0");

                var eventOuterTwo = EventMapCore.MakeMap(
                    new object[][] { new object[] { "outerTwo", SupportBeanComplexProps.MakeDefaultBean() } });
                env.SendEventMap(eventOuterTwo, "MyMappedPropertyMapOuterTwo");

                env.AssertPropsNew("s0", "a".SplitCsv(), new object[] { "yOne" });
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(object), statement.EventType.GetPropertyType("a")));

                env.UndeployAll();
            }
        }

        private class EventMapMapNamePropertyNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select outer.P0.n0 as a, outer.P1[0].n0 as b, outer.P1[1].n0 as c, outer.P0 as d, outer.P1 as e from MyArrayMapTwo");
                env.AddListener("s0");

                var n0Bean1 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 1 } });
                var n0Bean21 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 2 } });
                var n0Bean22 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 3 } });
                var n0Bean2 = new IDictionary<string, object>[] { n0Bean21, n0Bean22 };
                var theEvent = EventMapCore.MakeMap(
                    new object[][] { new object[] { "P0", n0Bean1 }, new object[] { "P1", n0Bean2 } });
                var eventOuter = EventMapCore.MakeMap(new object[][] { new object[] { "outer", theEvent } });
                env.SendEventMap(eventOuter, "MyArrayMapTwo");

                env.AssertPropsNew("s0", "a,b,c,d,e".SplitCsv(), new object[] { 1, 2, 3, n0Bean1, n0Bean2 });
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("d"));
                        Assert.AreEqual(typeof(IDictionary<string, object>[]), eventType.GetPropertyType("e"));
                    });

                env.UndeployAll();
                env.CompileDeploy(
                    "@name('s0') select outer.P0.n0? as a, outer.P1[0].n0? as b, outer.P1[1]?.n0 as c, outer.P0? as d, outer.P1? as e from MyArrayMapTwo");
                env.AddListener("s0");

                env.SendEventMap(eventOuter, "MyArrayMapTwo");

                env.AssertPropsNew("s0", "a,b,c,d,e".SplitCsv(), new object[] { 1, 2, 3, n0Bean1, n0Bean2 });
                env.AssertStatement(
                    "s0",
                    statement => { Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("a")); });

                env.UndeployAll();
            }
        }

        private class EventMapMapNameProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@name('s0') select P0.n0 as a, P1[0].n0 as b, P1[1].n0 as c, P0 as d, P1 as e from MyMapWithAMap");
                env.AddListener("s0");

                var n0Bean1 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 1 } });
                var n0Bean21 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 2 } });
                var n0Bean22 = EventMapCore.MakeMap(new object[][] { new object[] { "n0", 3 } });
                var n0Bean2 = new IDictionary<string, object>[] { n0Bean21, n0Bean22 };
                var theEvent = EventMapCore.MakeMap(
                    new object[][] { new object[] { "P0", n0Bean1 }, new object[] { "P1", n0Bean2 } });
                env.SendEventMap(theEvent, "MyMapWithAMap");

                env.AssertEventNew(
                    "s0",
                    eventResult => {
                        EPAssertionUtil.AssertProps(
                            eventResult,
                            "a,b,c,d".SplitCsv(),
                            new object[] { 1, 2, 3, n0Bean1 });
                        var valueE = (IDictionary<string, object>[])eventResult.Get("e");
                        Assert.AreEqual(valueE[0], n0Bean2[0]);
                        Assert.AreEqual(valueE[1], n0Bean2[1]);
                    });

                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("a"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("b"));
                        Assert.AreEqual(typeof(int?), eventType.GetPropertyType("c"));
                        Assert.AreEqual(typeof(IDictionary<string, object>), eventType.GetPropertyType("d"));
                        Assert.AreEqual(typeof(IDictionary<string, object>[]), eventType.GetPropertyType("e"));
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace