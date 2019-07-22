///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateEventTypeColumn
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTypableSubquery());
            execs.Add(new EPLInsertIntoTypableNewOperatorDocSample());
            execs.Add(new EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice.MAP));
            execs.Add(new EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice.ARRAY));
            execs.Add(new EPLInsertIntoInvalid());
            execs.Add(new EPLInsertIntoEnumerationSubquery());
            return execs;
        }

        private static void TryAssertionFragmentSingeColNamedWindow(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeployWBusPublicType("create schema AEvent (Symbol string)", path);

            env.CompileDeploy("create window MyEventWindow#lastevent (e AEvent)", path);
            env.CompileDeploy(
                "insert into MyEventWindow select (select * from AEvent#lastevent) as e from SupportBean(TheString = 'A')",
                path);
            env.CompileDeploy("create schema BEvent (e AEvent)", path);
            env.CompileDeploy(
                    "@Name('s0') insert into BEvent select (select e from MyEventWindow) as e from SupportBean(TheString = 'B')",
                    path)
                .AddListener("s0");

            env.SendEventMap(Collections.SingletonDataMap("Symbol", "GE"), "AEvent");
            env.SendEventBean(new SupportBean("A", 1));
            env.SendEventBean(new SupportBean("B", 2));
            var result = env.Listener("s0").AssertOneGetNewAndReset();
            var fragment = (EventBean) result.Get("e");
            Assert.AreEqual("AEvent", fragment.EventType.Name);
            Assert.AreEqual("GE", fragment.Get("Symbol"));

            env.UndeployAll();
        }

        private static void TryAssertionTypableSubquerySingleMayFilter(
            RegressionEnvironment env,
            string typeType,
            bool filter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
            env.CompileDeploy("create " + typeType + " schema EventOne(ez EventZero)", path);

            var fields = "ez.e0_0,ez.e0_1".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0') insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#lastevent" +
                    (filter ? " where Id >= 100" : "") +
                    ") as ez " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var expected = filter ? new object[] {null, null} : new object[] {"x1", "y1"};
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.SendEventBean(new SupportBean_S0(100, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"x2", "y2"});

            env.SendEventBean(new SupportBean_S0(2, "x3", "y3"));
            env.SendEventBean(new SupportBean("E3", 3));
            expected = filter ? new object[] {null, null} : new object[] {"x3", "y3"};
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.UndeployAll();
        }

        private static void TryAssertionTypableSubqueryMulti(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
            env.CompileDeploy("create " + typeType + " schema EventOne(e1_0 string, ez EventZero[])", path);

            var fields = "e1_0,ez[0].e0_0,ez[0].e0_1,ez[1].e0_0,ez[1].e0_1".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0')" +
                    "expression thequery {" +
                    "  (select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall)" +
                    "} " +
                    "insert into EventOne select " +
                    "TheString as e1_0, " +
                    "thequery() as ez " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                fields,
                new object[] {"E1", "x1", "y1", null, null});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            env.SendEventBean(new SupportBean_S0(2, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "x1", "y1", "x2", "y2"});

            env.UndeployAll();
        }

        private static void TryAssertionTypableSubqueryMultiFilter(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
            env.CompileDeploy("create " + typeType + " schema EventOne(ez EventZero[])", path);

            var fields = "e0_0".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0') insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall where Id between 10 and 20) as ez " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(
                (EventBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("ez"),
                fields,
                null);

            env.SendEventBean(new SupportBean_S0(10, "x2"));
            env.SendEventBean(new SupportBean_S0(20, "x3"));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(
                (EventBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("ez"),
                fields,
                new[] {new object[] {"x2"}, new object[] {"x3"}});

            env.UndeployAll();
        }

        private static void TryAssertionEnumerationSubqueryMultiMayFilter(
            RegressionEnvironment env,
            string typeType,
            bool filter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventOne(sbarr SupportBean_S0[])", path);

            var fields = "p00".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0') insert into EventOne select " +
                    "(select * from SupportBean_S0#keepall " +
                    (filter ? "where 1=1" : "") +
                    ") as sbarr " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var inner = (EventBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(
                inner,
                fields,
                new[] {new object[] {"x1"}});

            env.SendEventBean(new SupportBean_S0(2, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            inner = (EventBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(
                inner,
                fields,
                new[] {new object[] {"x1"}, new object[] {"x2"}});

            env.UndeployAll();
        }

        private static void TryAssertionEnumerationSubquerySingleMayFilter(
            RegressionEnvironment env,
            string typeType,
            bool filter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventOne(sb SupportBean_S0)", path);

            var fields = "sb.p00".SplitCsv();
            env.CompileDeploy(
                    "@Name('s0') insert into EventOne select " +
                    "(select * from SupportBean_S0#length(2) " +
                    (filter ? "where Id >= 100" : "") +
                    ") as sb " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var expected = filter ? new object[] {null} : new object[] {"x1"};
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);

            env.SendEventBean(new SupportBean_S0(100, "x2"));
            env.SendEventBean(new SupportBean("E2", 2));
            var received = (string) env.Listener("s0").AssertOneGetNewAndReset().Get(fields[0]);
            if (filter) {
                Assert.AreEqual("x2", received);
            }
            else {
                if (!received.Equals("x1") && !received.Equals("x2")) {
                    Assert.Fail();
                }
            }

            env.UndeployAll();
        }

        private static void TryAssertionTypableNewOperatorDocSample(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema Item(name string, Price double)", path);
            env.CompileDeploy("create " + typeType + " schema PurchaseOrder(orderId string, items Item[])", path);
            env.CompileDeployWBusPublicType("create schema TriggerEvent()", path);
            env.CompileDeploy(
                    "@Name('s0') insert into PurchaseOrder select '001' as orderId, new {name= 'i1', Price=10} as items from TriggerEvent",
                    path)
                .AddListener("s0");

            env.SendEventMap(Collections.EmptyDataMap, "TriggerEvent");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                "orderId,items[0].name,items[0].Price".SplitCsv(),
                new object[] {"001", "i1", 10d});

            var underlying = (EventBean[]) @event.Get("items");
            Assert.AreEqual(1, underlying.Length);
            Assert.AreEqual("i1", underlying[0].Get("name"));
            Assert.AreEqual(10d, underlying[0].Get("Price"));

            env.UndeployAll();
        }

        internal class EPLInsertIntoTypableSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionTypableSubqueryMulti(env, "objectarray");
                TryAssertionTypableSubqueryMulti(env, "map");

                TryAssertionTypableSubquerySingleMayFilter(env, "objectarray", true);
                TryAssertionTypableSubquerySingleMayFilter(env, "map", true);

                TryAssertionTypableSubquerySingleMayFilter(env, "objectarray", false);
                TryAssertionTypableSubquerySingleMayFilter(env, "map", false);

                TryAssertionTypableSubqueryMultiFilter(env, "objectarray");
                TryAssertionTypableSubqueryMultiFilter(env, "map");
            }
        }

        internal class EPLInsertIntoEnumerationSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionEnumerationSubqueryMultiMayFilter(env, "objectarray", true);
                TryAssertionEnumerationSubqueryMultiMayFilter(env, "map", true);

                TryAssertionEnumerationSubqueryMultiMayFilter(env, "objectarray", false);
                TryAssertionEnumerationSubqueryMultiMayFilter(env, "map", false);

                TryAssertionEnumerationSubquerySingleMayFilter(env, "objectarray", true);
                TryAssertionEnumerationSubquerySingleMayFilter(env, "map", true);

                TryAssertionEnumerationSubquerySingleMayFilter(env, "objectarray", false);
                TryAssertionEnumerationSubquerySingleMayFilter(env, "map", false);

                TryAssertionFragmentSingeColNamedWindow(env);
            }
        }

        internal class EPLInsertIntoTypableNewOperatorDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionTypableNewOperatorDocSample(env, "objectarray");
                TryAssertionTypableNewOperatorDocSample(env, "map");
            }
        }

        internal class EPLInsertIntoTypableAndCaseNew : RegressionExecution
        {
            private readonly EventRepresentationChoice representation;

            public EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice representation)
            {
                this.representation = representation;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create " + representation.GetOutputTypeCreateSchemaName() + " schema Nested(p0 string, p1 int)",
                    path);
                env.CompileDeploy(
                    "create " + representation.GetOutputTypeCreateSchemaName() + " schema OuterType(n0 Nested)",
                    path);

                var fields = "n0.p0,n0.p1".SplitCsv();
                env.CompileDeploy(
                        "@Name('out') " +
                        "expression computeNested {\n" +
                        "  sb => case\n" +
                        "  when IntPrimitive = 1 \n" +
                        "    then new { p0 = 'a', p1 = 1}\n" +
                        "  else new { p0 = 'b', p1 = 2 }\n" +
                        "  end\n" +
                        "}\n" +
                        "insert into OuterType select computeNested(sb) as n0 from SupportBean as sb",
                        path)
                    .AddListener("out");

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("out").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"b", 2});

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("out").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"a", 1});

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema N1_1(p0 int)", path);
                env.CompileDeploy("create schema N1_2(p1 N1_1)", path);

                // enumeration type is incompatible
                env.CompileDeploy("create schema TypeOne(sbs SupportBean[])", path);
                TryInvalidCompile(
                    env,
                    path,
                    "insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Incompatible type detected attempting to insert into column 'sbs' type '" +
                    typeof(SupportBean).Name +
                    "' compared to selected type 'SupportBean_S0'");

                env.CompileDeploy("create schema TypeTwo(sbs SupportBean)", path);
                TryInvalidCompile(
                    env,
                    path,
                    "insert into TypeTwo select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Incompatible type detected attempting to insert into column 'sbs' type '" +
                    typeof(SupportBean).Name +
                    "' compared to selected type 'SupportBean_S0'");

                // typable - selected column type is incompatible
                TryInvalidCompile(
                    env,
                    path,
                    "insert into N1_2 select new {p0='a'} as p1 from SupportBean",
                    "InvalId assignment of column 'p0' of type 'System.String' to event property 'p0' typed as 'System.Integer', column and parameter types mismatch");

                // typable - selected column type is not matching anything
                TryInvalidCompile(
                    env,
                    path,
                    "insert into N1_2 select new {xxx='a'} as p1 from SupportBean",
                    "Failed to find property 'xxx' among properties for target event type 'N1_1'");

                env.UndeployAll();
            }
        }
    }
} // end of namespace