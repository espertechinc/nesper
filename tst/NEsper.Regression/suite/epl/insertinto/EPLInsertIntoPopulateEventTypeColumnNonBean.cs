///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateEventTypeColumnNonBean
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFromSubquerySingle(execs);
            WithFromSubqueryMulti(execs);
            WithFromSubqueryMultiFilter(execs);
            WithNewOperatorDocSample(execs);
            WithCaseNew(execs);
            WithSingleColNamedWindow(execs);
            WithSingleToMulti(execs);
            WithBeanInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithBeanInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanBeanInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleToMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanSingleToMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleColNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanSingleColNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithCaseNew(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanCaseNew(EventRepresentationChoice.MAP));
            execs.Add(new EPLInsertIntoColNonBeanCaseNew(EventRepresentationChoice.OBJECTARRAY));
            execs.Add(new EPLInsertIntoColNonBeanCaseNew(EventRepresentationChoice.JSON));
            return execs;
        }

        public static IList<RegressionExecution> WithNewOperatorDocSample(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanNewOperatorDocSample("objectarray"));
            execs.Add(new EPLInsertIntoColNonBeanNewOperatorDocSample("map"));
            return execs;
        }

        public static IList<RegressionExecution> WithFromSubqueryMultiFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanFromSubqueryMultiFilter("objectarray"));
            execs.Add(new EPLInsertIntoColNonBeanFromSubqueryMultiFilter("map"));
            return execs;
        }

        public static IList<RegressionExecution> WithFromSubqueryMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanFromSubqueryMulti("objectarray"));
            execs.Add(new EPLInsertIntoColNonBeanFromSubqueryMulti("map"));
            return execs;
        }

        public static IList<RegressionExecution> WithFromSubquerySingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoColNonBeanFromSubquerySingle("objectarray", false));
            execs.Add(new EPLInsertIntoColNonBeanFromSubquerySingle("objectarray", true));
            execs.Add(new EPLInsertIntoColNonBeanFromSubquerySingle("map", false));
            execs.Add(new EPLInsertIntoColNonBeanFromSubquerySingle("map", true));
            return execs;
        }

        private class EPLInsertIntoColNonBeanSingleToMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema EventA(id string);\n" +
                          "select * from EventA#keepall;\n" +
                          "@public create schema EventB(aArray EventA[]);\n" +
                          "insert into EventB select maxby(id) as aArray from EventA;\n" +
                          "@name('s0') select * from EventB#keepall;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var aOne = Collections.SingletonDataMap("id", "x1");
                env.SendEventMap(aOne, "EventA");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var events = (EventBean[])@event.Get("aArray");
                        Assert.AreEqual(1, events.Length);
                        Assert.AreSame(aOne, events[0].Underlying);
                    });

                env.Milestone(0);

                env.AssertPropsPerRowIterator(
                    "s0",
                    "aArray[0].id".Split(","),
                    new object[][] { new object[] { "x1" } });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoColNonBeanFromSubqueryMultiFilter : RegressionExecution
        {
            private readonly string typeType;

            public EPLInsertIntoColNonBeanFromSubqueryMultiFilter(string typeType)
            {
                this.typeType = typeType;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
                env.CompileDeploy("@public create " + typeType + " schema EventOne(ez EventZero[])", path);

                var fields = "e0_0".Split(",");
                var epl = "@name('s0') insert into EventOne select " +
                          "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall where id between 10 and 20) as ez " +
                          "from SupportBean;\n" +
                          "@name('s1') select * from EventOne#keepall";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsPerRow((EventBean[])@event.Get("ez"), fields, null));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "x2"));
                env.SendEventBean(new SupportBean_S0(20, "x3"));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsPerRow(
                        (EventBean[])@event.Get("ez"),
                        fields,
                        new object[][] { new object[] { "x2" }, new object[] { "x3" } }));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "typeType='" +
                       typeType +
                       '\'' +
                       '}';
            }
        }

        private class EPLInsertIntoColNonBeanNewOperatorDocSample : RegressionExecution
        {
            private readonly string typeType;

            public EPLInsertIntoColNonBeanNewOperatorDocSample(string typeType)
            {
                this.typeType = typeType;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create " + typeType + " schema Item(name string, price double)", path);
                env.CompileDeploy(
                    "@public create " + typeType + " schema PurchaseOrder(orderId string, items Item[])",
                    path);
                env.CompileDeploy("@public @buseventtype create schema TriggerEvent()", path);
                env.CompileDeploy(
                        "@name('s0') insert into PurchaseOrder select '001' as orderId, new {name= 'i1', price=10} as items from TriggerEvent",
                        path)
                    .AddListener("s0");
                env.CompileDeploy("@name('s1') select * from PurchaseOrder#keepall", path);

                env.SendEventMap(Collections.EmptyDataMap, "TriggerEvent");
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertProps(
                            @event,
                            "orderId,items[0].name,items[0].price".Split(","),
                            new object[] { "001", "i1", 10d });

                        var underlying = (EventBean[])@event.Get("items");
                        Assert.AreEqual(1, underlying.Length);
                        Assert.AreEqual("i1", underlying[0].Get("name"));
                        Assert.AreEqual(10d, underlying[0].Get("price"));
                    });

                env.Milestone(0);

                env.SendEventMap(Collections.EmptyDataMap, "TriggerEvent");

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "typeType='" +
                       typeType +
                       '\'' +
                       '}';
            }
        }

        private class EPLInsertIntoColNonBeanCaseNew : RegressionExecution
        {
            private readonly EventRepresentationChoice representation;

            public EPLInsertIntoColNonBeanCaseNew(EventRepresentationChoice representation)
            {
                this.representation = representation;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedNested)) +
                    "@public create schema Nested(p0 string, p1 int)",
                    path);
                env.CompileDeploy(
                    representation.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedOuterType)) +
                    "@public create schema OuterType(n0 Nested)",
                    path);

                var fields = "n0.p0,n0.p1".Split(",");
                var epl = "@name('out') " +
                          "expression computeNested {\n" +
                          "  sb => case\n" +
                          "  when intPrimitive = 1 \n" +
                          "    then new { p0 = 'a', p1 = 1}\n" +
                          "  else new { p0 = 'b', p1 = 2 }\n" +
                          "  end\n" +
                          "}\n" +
                          "insert into OuterType select computeNested(sb) as n0 from SupportBean as sb;\n" +
                          "@name('s1') select * from OuterType#keepall;\n";
                env.CompileDeploy(epl, path).AddListener("out");

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertPropsNew("out", fields, new object[] { "b", 2 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("out", fields, new object[] { "a", 1 });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "representation=" +
                       representation +
                       '}';
            }
        }

        private class EPLInsertIntoColNonBeanBeanInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema N1_1(p0 int)", path);
                env.CompileDeploy("@public create schema N1_2(p1 N1_1)", path);

                // typable - selected column type is incompatible
                env.TryInvalidCompile(
                    path,
                    "insert into N1_2 select new {p0='a'} as p1 from SupportBean",
                    "Invalid assignment of column 'p0' of type 'String' to event property 'p0' typed as 'Integer', column and parameter types mismatch");

                // typable - selected column type is not matching anything
                env.TryInvalidCompile(
                    path,
                    "insert into N1_2 select new {xxx='a'} as p1 from SupportBean",
                    "Failed to find property 'xxx' among properties for target event type 'N1_1'");

                env.UndeployAll();
            }
        }

        public class EPLInsertIntoColNonBeanSingleColNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public @buseventtype create schema AEvent (symbol string)", path);

                env.CompileDeploy("@public create window MyEventWindow#lastevent (e AEvent)", path);
                env.CompileDeploy(
                    "insert into MyEventWindow select (select * from AEvent#lastevent) as e from SupportBean(theString = 'A')",
                    path);
                env.CompileDeploy("@public create schema BEvent (e AEvent)", path);
                env.CompileDeploy(
                        "@name('s0') insert into BEvent select (select e from MyEventWindow) as e from SupportBean(theString = 'B')",
                        path)
                    .AddListener("s0");
                env.CompileDeploy("@name('s1') select * from BEvent#keepall", path);

                env.SendEventMap(Collections.SingletonDataMap("symbol", "GE"), "AEvent");
                env.SendEventBean(new SupportBean("A", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B", 2));
                env.AssertEventNew(
                    "s0",
                    result => {
                        var fragment = (EventBean)result.Get("e");
                        Assert.AreEqual("AEvent", fragment.EventType.Name);
                        Assert.AreEqual("GE", fragment.Get("symbol"));
                    });

                env.UndeployAll();
            }
        }

        private class EPLInsertIntoColNonBeanFromSubquerySingle : RegressionExecution
        {
            private readonly string typeType;
            private readonly bool filter;

            public EPLInsertIntoColNonBeanFromSubquerySingle(
                string typeType,
                bool filter)
            {
                this.typeType = typeType;
                this.filter = filter;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
                env.CompileDeploy("@public create " + typeType + " schema EventOne(ez EventZero)", path);

                var fields = "ez.e0_0,ez.e0_1".Split(",");
                var epl = "@name('s0') insert into EventOne select " +
                          "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#lastevent" +
                          (filter ? " where id >= 100" : "") +
                          ") as ez " +
                          "from SupportBean;\n" +
                          "@name('s1') select * from EventOne#keepall;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
                env.SendEventBean(new SupportBean("E1", 1));
                var expected = filter ? new object[] { null, null } : new object[] { "x1", "y1" };
                env.AssertPropsNew("s0", fields, expected);

                env.SendEventBean(new SupportBean_S0(100, "x2", "y2"));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "x2", "y2" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2, "x3", "y3"));
                env.SendEventBean(new SupportBean("E3", 3));
                expected = filter ? new object[] { null, null } : new object[] { "x3", "y3" };
                env.AssertPropsNew("s0", fields, expected);
                if (!filter) {
                    env.AssertPropsPerRowIterator(
                        "s1",
                        "ez.e0_0".Split(","),
                        new object[][] { new object[] { "x1" }, new object[] { "x2" }, new object[] { "x3" } });
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "typeType='" +
                       typeType +
                       '\'' +
                       ", filter=" +
                       filter +
                       '}';
            }
        }

        private class EPLInsertIntoColNonBeanFromSubqueryMulti : RegressionExecution
        {
            private readonly string typeType;

            public EPLInsertIntoColNonBeanFromSubqueryMulti(string typeType)
            {
                this.typeType = typeType;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
                env.CompileDeploy("@public create " + typeType + " schema EventOne(e1_0 string, ez EventZero[])", path);

                var fields = "e1_0,ez[0].e0_0,ez[0].e0_1,ez[1].e0_0,ez[1].e0_1".Split(",");
                var epl = "@name('s0')" +
                          "expression thequery {" +
                          "  (select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall)" +
                          "} " +
                          "insert into EventOne select theString as e1_0, thequery() as ez from SupportBean;\n" +
                          "@name('s1') select * from EventOne#keepall;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        EPAssertionUtil.AssertProps(@event, fields, new object[] { "E1", "x1", "y1", null, null });
                        SupportEventTypeAssertionUtil.AssertConsistency(@event);
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2, "x2", "y2"));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "x1", "y1", "x2", "y2" });
                env.AssertPropsPerRowIterator(
                    "s1",
                    "ez[0].e0_0,ez[1].e0_0".Split(","),
                    new object[][] { new object[] { "x1", null }, new object[] { "x1", "x2" } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name + "{type=" + typeType + "}";
            }
        }

        [Serializable]
        public class MyLocalJsonProvidedNested
        {
            public string p0;
            public int p1;
        }

        [Serializable]
        public class MyLocalJsonProvidedOuterType
        {
            public MyLocalJsonProvidedNested n0;
        }
    }
} // end of namespace