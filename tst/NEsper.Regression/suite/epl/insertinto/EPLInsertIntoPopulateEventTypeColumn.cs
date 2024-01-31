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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoPopulateEventTypeColumn
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithTypableSubquery(execs);
            WithTypableNewOperatorDocSample(execs);
            WithTypableAndCaseNew(execs);
            WithInvalid(execs);
            With(EnumerationSubquery)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithEnumerationSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoEnumerationSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTypableAndCaseNew(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice.MAP));
            execs.Add(new EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice.OBJECTARRAY));
            execs.Add(new EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice.JSON));
            return execs;
        }

        public static IList<RegressionExecution> WithTypableNewOperatorDocSample(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTypableNewOperatorDocSample());
            return execs;
        }

        public static IList<RegressionExecution> WithTypableSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLInsertIntoTypableSubquery());
            return execs;
        }

        private static void TryAssertionFragmentSingeColNamedWindow(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public @buseventtype create schema AEvent (Symbol string)", path);

            env.CompileDeploy("create window MyEventWindow#lastevent (e AEvent)", path);
            env.CompileDeploy(
                "insert into MyEventWindow select (select * from AEvent#lastevent) as e from SupportBean(TheString = 'A')",
                path);
            env.CompileDeploy("create schema BEvent (e AEvent)", path);
            env.CompileDeploy(
                    "@name('s0') insert into BEvent select (select e from MyEventWindow) as e from SupportBean(TheString = 'B')",
                    path)
                .AddListener("s0");

            env.SendEventMap(Collections.SingletonDataMap("Symbol", "GE"), "AEvent");
            env.SendEventBean(new SupportBean("A", 1));
            env.SendEventBean(new SupportBean("B", 2));
            var result = env.Listener("s0").AssertOneGetNewAndReset();
            var fragment = (EventBean)result.Get("e");
            ClassicAssert.AreEqual("AEvent", fragment.EventType.Name);
            ClassicAssert.AreEqual("GE", fragment.Get("Symbol"));

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

            var fields = new[] { "ez.e0_0", "ez.e0_1" };
            env.CompileDeploy(
                    "@name('s0') insert into EventOne select " +
                    "(select P00 as e0_0, P01 as e0_1 from SupportBean_S0#lastevent" +
                    (filter ? " where Id >= 100" : "") +
                    ") as ez " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var expected = filter ? new object[] { null, null } : new object[] { "x1", "y1" };
            env.AssertPropsNew("s0", fields, expected);

            env.SendEventBean(new SupportBean_S0(100, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { "x2", "y2" });

            env.SendEventBean(new SupportBean_S0(2, "x3", "y3"));
            env.SendEventBean(new SupportBean("E3", 3));
            expected = filter ? new object[] { null, null } : new object[] { "x3", "y3" };
            env.AssertPropsNew("s0", fields, expected);

            env.UndeployAll();
        }

        private static void TryAssertionTypableSubqueryMulti(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
            env.CompileDeploy("create " + typeType + " schema EventOne(e1_0 string, ez EventZero[])", path);

            var fields = new[] { "e1_0", "ez[0].e0_0", "ez[0].e0_1", "ez[1].e0_0", "ez[1].e0_1" };
            env.CompileDeploy(
                    "@name('s0')" +
                    "expression thequery {" +
                    "  (select P00 as e0_0, P01 as e0_1 from SupportBean_S0#keepall)" +
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
                new object[] { "E1", "x1", "y1", null, null });
            SupportEventTypeAssertionUtil.AssertConsistency(@event);

            env.SendEventBean(new SupportBean_S0(2, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { "E2", "x1", "y1", "x2", "y2" });

            env.UndeployAll();
        }

        private static void TryAssertionTypableSubqueryMultiFilter(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)", path);
            env.CompileDeploy("create " + typeType + " schema EventOne(ez EventZero[])", path);

            var fields = new[] { "e0_0" };
            env.CompileDeploy(
                    "@name('s0') insert into EventOne select " +
                    "(select P00 as e0_0, P01 as e0_1 from SupportBean_S0#keepall where Id between 10 and 20) as ez " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1", "y1"));
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(
                (EventBean[])env.Listener("s0").AssertOneGetNewAndReset().Get("ez"),
                fields,
                null);

            env.SendEventBean(new SupportBean_S0(10, "x2"));
            env.SendEventBean(new SupportBean_S0(20, "x3"));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(
                (EventBean[])env.Listener("s0").AssertOneGetNewAndReset().Get("ez"),
                fields,
                new[] { new object[] { "x2" }, new object[] { "x3" } });

            env.UndeployAll();
        }

        private static void TryAssertionEnumerationSubqueryMultiMayFilter(
            RegressionEnvironment env,
            string typeType,
            bool filter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventOne(sbarr SupportBean_S0[])", path);

            var fields = new[] { "P00" };
            env.CompileDeploy(
                    "@name('s0') insert into EventOne select " +
                    "(select * from SupportBean_S0#keepall " +
                    (filter ? "where 1=1" : "") +
                    ") as sbarr " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var inner = (EventBean[])env.Listener("s0").AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(
                inner,
                fields,
                new[] { new object[] { "x1" } });

            env.SendEventBean(new SupportBean_S0(2, "x2", "y2"));
            env.SendEventBean(new SupportBean("E2", 2));
            inner = (EventBean[])env.Listener("s0").AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(
                inner,
                fields,
                new[] { new object[] { "x1" }, new object[] { "x2" } });

            env.UndeployAll();
        }

        private static void TryAssertionEnumerationSubquerySingleMayFilter(
            RegressionEnvironment env,
            string typeType,
            bool filter)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema EventOne(sb SupportBean_S0)", path);

            var fields = new[] { "sb.P00" };
            env.CompileDeploy(
                    "@name('s0') insert into EventOne select " +
                    "(select * from SupportBean_S0#length(2) " +
                    (filter ? "where Id >= 100" : "") +
                    ") as sb " +
                    "from SupportBean",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "x1"));
            env.SendEventBean(new SupportBean("E1", 1));
            var expected = filter ? new object[] { null } : new object[] { "x1" };
            env.AssertPropsNew("s0", fields, expected);

            env.SendEventBean(new SupportBean_S0(100, "x2"));
            env.SendEventBean(new SupportBean("E2", 2));
            var received = (string)env.Listener("s0").AssertOneGetNewAndReset().Get(fields[0]);
            if (filter) {
                ClassicAssert.AreEqual("x2", received);
            }
            else {
                ClassicAssert.IsNull(
                    received); // this should not take the first event and according to SQL standard returns null
            }

            env.UndeployAll();
        }

        private static void TryAssertionTypableNewOperatorDocSample(
            RegressionEnvironment env,
            string typeType)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create " + typeType + " schema Item(name string, Price double)", path);
            env.CompileDeploy("create " + typeType + " schema PurchaseOrder(OrderId string, items Item[])", path);
            env.CompileDeploy("@public @buseventtype create schema TriggerEvent()", path);
            env.CompileDeploy(
                    "@name('s0') insert into PurchaseOrder select '001' as OrderId, new {name= 'i1', Price=10} as items from TriggerEvent",
                    path)
                .AddListener("s0");

            env.SendEventMap(Collections.EmptyDataMap, "TriggerEvent");
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(
                @event,
                new[] { "OrderId", "items[0].name", "items[0].Price" },
                new object[] { "001", "i1", 10d });

            var underlying = (EventBean[])@event.Get("items");
            ClassicAssert.AreEqual(1, underlying.Length);
            ClassicAssert.AreEqual("i1", underlying[0].Get("name"));
            ClassicAssert.AreEqual(10d, underlying[0].Get("Price"));

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
            private readonly EventRepresentationChoice _representation;

            public EPLInsertIntoTypableAndCaseNew(EventRepresentationChoice representation)
            {
                _representation = representation;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    _representation.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedNested>() +
                    "create schema Nested(P0 string, P1 int)",
                    path);
                env.CompileDeploy(
                    _representation.GetAnnotationTextWJsonProvided<MyLocalJsonProvidedOuterType>() +
                    "create schema OuterType(N0 Nested)",
                    path);

                var fields = new[] { "N0.P0", "N0.P1" };
                env.CompileDeploy(
                        "@name('out') " +
                        "expression computeNested {\n" +
                        "  sb -> case\n" +
                        "  when IntPrimitive = 1 \n" +
                        "    then new { P0 = 'a', P1 = 1}\n" +
                        "  else new { P0 = 'b', P1 = 2 }\n" +
                        "  end\n" +
                        "}\n" +
                        "insert into OuterType select computeNested(sb) as N0 from SupportBean as sb",
                        path)
                    .AddListener("out");

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertPropsNew(
                    "out",
                    fields,
                    new object[] { "b", 2 });

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew(
                    "out",
                    fields,
                    new object[] { "a", 1 });

                env.UndeployAll();
            }
        }

        internal class EPLInsertIntoInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema N1_1(P0 int)", path);
                env.CompileDeploy("create schema N1_2(P1 N1_1)", path);

                // enumeration type is incompatible
                env.CompileDeploy("create schema TypeOne(sbs SupportBean[])", path);
                env.TryInvalidCompile(
                    path,
                    "insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Incompatible type detected attempting to insert into column 'sbs' type '" +
                    typeof(SupportBean).FullName +
                    "' compared to selected type 'SupportBean_S0'");

                env.CompileDeploy("create schema TypeTwo(sbs SupportBean)", path);
                env.TryInvalidCompile(
                    path,
                    "insert into TypeTwo select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Incompatible type detected attempting to insert into column 'sbs' type '" +
                    typeof(SupportBean).FullName +
                    "' compared to selected type 'SupportBean_S0'");

                // typable - selected column type is incompatible
                env.TryInvalidCompile(
                    path,
                    "insert into N1_2 select new {P0='a'} as P1 from SupportBean",
                    "Invalid assignment of column 'P0' of type 'System.String' to event property 'P0' typed as 'System.Nullable<System.Int32>', column and parameter types mismatch");

                // typable - selected column type is not matching anything
                env.TryInvalidCompile(
                    path,
                    "insert into N1_2 select new {xxx='a'} as P1 from SupportBean",
                    "Failed to find property 'xxx' among properties for target event type 'N1_1'");

                env.UndeployAll();
            }
        }

        public class MyLocalJsonProvidedNested
        {
            public string P0;
            public int P1;
        }

        public class MyLocalJsonProvidedOuterType
        {
            public MyLocalJsonProvidedNested N0;
        }
    }
} // end of namespace