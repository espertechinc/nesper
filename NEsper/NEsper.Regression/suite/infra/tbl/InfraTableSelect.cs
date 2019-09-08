///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.subscriber;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableSelect
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraTableSelectStarPublicTypeVisibility());
            execs.Add(new InfraTableSelectEnum());
            return execs;
        }

        private static void RunAssertionSubqueryWindowAgg(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            env.CompileDeploy(
                    "@Name('s0') select " +
                    "(select window(mt.*) from MyTable as mt) as c0," +
                    "(select first(mt.*) from MyTable as mt) as c1" +
                    " from SupportBean_S2",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventUnd(((object[][]) @event.Get("c0"))[0], rowValues);
            AssertEventUnd(@event.Get("c1"), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionOnSelectWindowAgg(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            env.CompileDeploy(
                    "@Name('s0') on SupportBean_S2 select " +
                    "window(win.*) as c0," +
                    "last(win.*) as c1, " +
                    "first(win.*) as c2, " +
                    "first(p1) as c3," +
                    "window(p1) as c4," +
                    "sorted(p1) as c5," +
                    "minby(p1) as c6" +
                    " from MyTable as win",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            foreach (var col in new [] { "c1","c2","c6" }) {
                AssertEventUnd(@event.Get(col), rowValues);
            }

            foreach (var col in new [] { "c0","c5" }) {
                AssertEventUnd(((object[][]) @event.Get(col))[0], rowValues);
            }

            Assert.AreEqual("b", @event.Get("c3"));
            EPAssertionUtil.AssertEqualsExactOrder(new[] {"b"}, (string[]) @event.Get("c4"));

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionOutputSnapshot(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues,
            AtomicLong currentTime)
        {
            env.CompileDeploy("@Name('s0') select * from MyTable output snapshot every 1 second", path)
                .AddListener("s0");
            AssertEventType(env.Statement("s0").EventType, expectedType);

            currentTime.Set(currentTime.Get() + 1000L);
            env.AdvanceTime(currentTime.Get());
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionFireAndForgetInsertUpdateDelete(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType)
        {
            var result = env.CompileExecuteFAF("insert into MyTable(key) values ('dummy')", path);
            AssertEventType(result.EventType, expectedType);

            result = env.CompileExecuteFAF("delete from MyTable where key = 'dummy'", path);
            AssertEventType(result.EventType, expectedType);

            result = env.CompileExecuteFAF("update MyTable set key='dummy' where key='dummy'", path);
            AssertEventType(result.EventType, expectedType);
        }

        private static void RunAssertionIterateCreateTable(
            RegressionEnvironment env,
            object[][] expectedType,
            object[] rowValues,
            EPStatement stmtCreate)
        {
            AssertEventTypeAndEvent(
                stmtCreate.EventType,
                expectedType,
                stmtCreate.GetEnumerator().Advance().Underlying,
                rowValues);
        }

        private static void RunAssertionSingleRowFunc(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            // try join passing of params
            var eplJoin = "@Name('s0') select " +
                          typeof(InfraTableSelect).Name +
                          ".myServiceEventBean(mt) as c0, " +
                          typeof(InfraTableSelect).Name +
                          ".myServiceObjectArray(mt) as c1 " +
                          "from SupportBean_S2, MyTable as mt";
            env.CompileDeploy(eplJoin, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            var result = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            AssertEventUnd(result.Get("c1"), rowValues);
            env.UndeployModuleContaining("s0");

            // try subquery
            var eplSubquery = "@Name('s0') select (select pluginServiceEventBean(mt) from MyTable as mt) as c0 " +
                              "from SupportBean_S2";
            env.CompileDeploy(eplSubquery, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            result = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionInsertIntoBean(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            var epl = "@Name('s0') insert into SupportCtorSB2WithObjectArray select * from SupportBean_S2, MyTable";
            env.CompileDeploy(epl, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            AssertEventUnd(env.Listener("s0").AssertOneGetNewAndReset().Get("arr"), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionSubquerySelectWEnumMethod(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            var epl = "@Name('s0') select (select * from MyTable).where(v->v.Key = 'G1') as mt from SupportBean_S2";
            env.CompileDeploy(epl, path).AddListener("s0");

            Assert.AreEqual(typeof(ICollection<object>), env.Statement("s0").EventType.GetPropertyType("mt"));

            env.SendEventBean(new SupportBean_S2(0));
            var coll = (ICollection<object>) env.Listener("s0").AssertOneGetNewAndReset().Get("mt");
            AssertEventUnd(coll.First(), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionSubquerySelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            var eplFiltered = "@Name('s0') select (select * from MyTable where key = 'G1') as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(env, path, rowValues, eplFiltered);

            var eplUnfiltered = "@Name('s0') select (select * from MyTable) as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(env, path, rowValues, eplUnfiltered);

            // With @eventbean
            var eplEventBean = "@Name('s0') select (select * from MyTable) @eventbean as mt from SupportBean_S2";
            env.CompileDeploy(eplEventBean, path).AddListener("s0");
            Assert.AreEqual(typeof(object[][]), env.Statement("s0").EventType.GetPropertyType("mt"));
            Assert.AreSame(
                env.Statement("create").EventType,
                env.Statement("s0").EventType.GetFragmentType("mt").FragmentType);

            env.SendEventBean(new SupportBean_S2(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            var value = (object[][]) @event.Get("mt");
            AssertEventUnd(value[0], rowValues);
            Assert.AreSame(env.Statement("create").EventType, ((EventBean[]) @event.GetFragment("mt"))[0].EventType);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionSubquerySelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues,
            string epl)
        {
            env.CompileDeploy(epl, path).AddListener("s0");

            Assert.AreEqual(typeof(object[]), env.Statement("s0").EventType.GetPropertyType("mt"));

            env.SendEventBean(new SupportBean_S2(0));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventUnd(@event.Get("mt"), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionJoinSelectStreamStarUnnamed(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            var joinEpl = "@Name('s0') select mt.* from MyTable as mt, SupportBean_S2 where key = P20";
            env.CompileDeploy(joinEpl, path).AddListener("s0");
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            env.Statement("s0").SetSubscriber(subscriber);

            AssertEventType(env.Statement("s0").EventType, expectedType);

            // listener assertion
            env.SendEventBean(new SupportBean_S2(0, "G1"));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionJoinSelectStreamStarNamed(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            var joinEpl = "@Name('s0') select mt.* as mymt from MyTable as mt, SupportBean_S2 where key = P20";
            env.CompileDeploy(joinEpl, path).AddListener("s0");
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            env.Statement("s0").SetSubscriber(subscriber);

            AssertEventType(env.Statement("s0").EventType.GetFragmentType("mymt").FragmentType, expectedType);

            // listener assertion
            env.SendEventBean(new SupportBean_S2(0, "G1"));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("mymt").FragmentType,
                expectedType,
                @event.Get("mymt"),
                rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionJoinSelectStreamName(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            var joinEpl = "@Name('s0') select mt from MyTable as mt, SupportBean_S2 where key = P20";
            env.CompileDeploy(joinEpl, path).AddListener("s0");

            AssertEventType(env.Statement("s0").EventType.GetFragmentType("mt").FragmentType, expectedType);

            env.SendEventBean(new SupportBean_S2(0, "G1"));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("mt").FragmentType,
                expectedType,
                @event.Get("mt"),
                rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionJoinSelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            var joinEpl = "@Name('s0') select * from MyTable, SupportBean_S2 where key = P20";
            env.CompileDeploy(joinEpl, path).AddListener("s0");
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            env.Statement("s0").SetSubscriber(subscriber);

            AssertEventType(env.Statement("s0").EventType.GetFragmentType("stream_0").FragmentType, expectedType);

            // listener assertion
            env.SendEventBean(new SupportBean_S2(0, "G1"));
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("stream_0").FragmentType,
                expectedType,
                @event.Get("stream_0"),
                rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionFireAndForgetSelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            object[][] expectedType,
            object[] rowValues)
        {
            var result = env.CompileExecuteFAF("select * from MyTable where key = 'G1'", path);
            AssertEventTypeAndEvent(result.EventType, expectedType, result.Array[0].Underlying, rowValues);
        }

        private static void AssertEventTypeAndEvent(
            EventType eventType,
            object[][] expectedType,
            object underlying,
            object[] expectedValues)
        {
            AssertEventType(eventType, expectedType);
            AssertEventUnd(underlying, expectedValues);
        }

        private static void AssertEventUnd(
            object underlying,
            object[] expectedValues)
        {
            var und = (object[]) underlying;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, und);
        }

        private static void AssertEventType(
            EventType eventType,
            object[][] expectedType)
        {
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                eventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            int longPrimitive)
        {
            var supportBean = new SupportBean(theString, intPrimitive);
            supportBean.LongPrimitive = longPrimitive;
            return supportBean;
        }

        public static object[] MyServiceEventBean(EventBean @event)
        {
            return (object[]) @event.Underlying;
        }

        public static object[] MyServiceObjectArray(object[] data)
        {
            return data;
        }

        internal class InfraTableSelectEnum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table MyTable(p string);\n" +
                          "@Name('s0') select t.firstOf() as c0 from MyTable as t;\n";
                env.CompileDeploy(epl, path);
                env.CompileExecuteFAF("insert into MyTable select 'a' as p", path);

                var @event = env.GetEnumerator("s0").Advance();
                var row = (object[]) @event.Get("c0");
                Assert.AreEqual("a", row[0]);

                env.UndeployAll();
            }
        }

        internal class InfraTableSelectStarPublicTypeVisibility : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var currentTime = new AtomicLong(0);
                env.AdvanceTime(currentTime.Get());
                var path = new RegressionPath();

                env.CompileDeploy(
                    "@Name('create') create table MyTable as (\n" +
                    "key string primary key,\n" +
                    "totalInt sum(int),\n" +
                    "p0 string,\n" +
                    "winsb window(*) @type(SupportBean),\n" +
                    "totalLong sum(long),\n" +
                    "p1 string,\n" +
                    "winsb0 window(*) @type(SupportBean_S0)\n" +
                    ")",
                    path);
                object[][] expectedType = {
                    new object[] {"key", typeof(string)},
                    new object[] {"totalInt", typeof(int?)},
                    new object[] {"P0", typeof(string)},
                    new object[] {"winsb", typeof(SupportBean[])},
                    new object[] {"totalLong", typeof(long?)},
                    new object[] {"P1", typeof(string)},
                    new object[] {"winsb0", typeof(SupportBean_S0[])}
                };

                env.CompileDeploy(
                    "into table MyTable " +
                    "select sum(IntPrimitive) as totalInt, sum(LongPrimitive) as totalLong," +
                    "window(*) as winsb from SupportBean#keepall group by TheString",
                    path);
                env.CompileDeploy(
                    "into table MyTable " +
                    "select window(*) as winsb0 from SupportBean_S0#keepall group by P00",
                    path);
                env.CompileDeploy(
                    "on SupportBean_S1 " +
                    "merge MyTable where P10 = key when matched then " +
                    "update set P0 = P11, P1 = P12",
                    path);

                var e1Sb = MakeSupportBean("G1", 10, 100);
                env.SendEventBean(e1Sb); // update some aggs

                var e2Sb0 = new SupportBean_S0(5, "G1");
                env.SendEventBean(e2Sb0); // update more aggs

                env.SendEventBean(new SupportBean_S1(6, "G1", "a", "b")); // merge more values

                object[] rowValues = {"G1", 10, "a", new[] {e1Sb}, 100L, "b", new[] {e2Sb0}};
                RunAssertionSubqueryWindowAgg(env, path, rowValues);
                RunAssertionOnSelectWindowAgg(env, path, expectedType, rowValues);
                RunAssertionSubquerySelectStar(env, path, rowValues);
                RunAssertionSubquerySelectWEnumMethod(env, path, rowValues);
                RunAssertionIterateCreateTable(env, expectedType, rowValues, env.Statement("create"));
                RunAssertionJoinSelectStar(env, path, expectedType, rowValues);
                RunAssertionJoinSelectStreamName(env, path, expectedType, rowValues);
                RunAssertionJoinSelectStreamStarNamed(env, path, expectedType, rowValues);
                RunAssertionJoinSelectStreamStarUnnamed(env, path, expectedType, rowValues);
                RunAssertionInsertIntoBean(env, path, rowValues);
                RunAssertionSingleRowFunc(env, path, rowValues);
                RunAssertionOutputSnapshot(env, path, expectedType, rowValues, currentTime);
                RunAssertionFireAndForgetSelectStar(env, path, expectedType, rowValues);
                RunAssertionFireAndForgetInsertUpdateDelete(env, path, expectedType);

                env.UndeployAll();
            }
        }
    }
} // end of namespace