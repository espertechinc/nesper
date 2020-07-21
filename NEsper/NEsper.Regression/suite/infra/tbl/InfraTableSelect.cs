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
using com.espertech.esper.regressionlib.support.bean;
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
            execs.Add(new InfraTableSelectMultikeyWArraySingleArray());
            execs.Add(new InfraTableSelectMultikeyWArrayTwoArray());
            execs.Add(new InfraTableSelectMultikeyWArrayComposite());
            return execs;
        }

        private static void RunAssertionSubqueryWindowAgg(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            env.CompileDeploy(
                    "@name('s0') select " +
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
                    "@name('s0') on SupportBean_S2 select " +
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
            foreach (var col in new[] {"c1", "c2", "c6"}) {
                AssertEventUnd(@event.Get(col), rowValues);
            }

            foreach (var col in new[] {"c0", "c5"}) {
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
            env.CompileDeploy("@name('s0') select * from MyTable output snapshot every 1 second", path)
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
            var eplJoin = "@name('s0') select " +
                          typeof(InfraTableSelect).FullName +
                          ".MyServiceEventBean(mt) as c0, " +
                          typeof(InfraTableSelect).FullName +
                          ".MyServiceObjectArray(mt) as c1 " +
                          "from SupportBean_S2, MyTable as mt";
            env.CompileDeploy(eplJoin, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            var result = env.Listener("s0").AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            AssertEventUnd(result.Get("c1"), rowValues);
            env.UndeployModuleContaining("s0");

            // try subquery
            var eplSubquery = "@name('s0') select (select pluginServiceEventBean(mt) from MyTable as mt) as c0 " +
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
            var epl = "@name('s0') insert into SupportCtorSB2WithObjectArray select * from SupportBean_S2, MyTable";
            env.CompileDeploy(epl, path).AddListener("s0");

            env.SendEventBean(new SupportBean_S2(0));
            AssertEventUnd(env.Listener("s0").AssertOneGetNewAndReset().Get("Arr"), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionSubquerySelectWEnumMethod(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            var epl = "@name('s0') select (select * from MyTable).where(v->v.key = 'G1') as mt from SupportBean_S2";
            env.CompileDeploy(epl, path).AddListener("s0");

            Assert.AreEqual(typeof(ICollection<object>), env.Statement("s0").EventType.GetPropertyType("mt"));

            env.SendEventBean(new SupportBean_S2(0));
            var coll = env.Listener("s0").AssertOneGetNewAndReset().Get("mt").Unwrap<object[]>();
            AssertEventUnd(coll.First(), rowValues);

            env.UndeployModuleContaining("s0");
        }

        private static void RunAssertionSubquerySelectStar(
            RegressionEnvironment env,
            RegressionPath path,
            object[] rowValues)
        {
            var eplFiltered = "@name('s0') select (select * from MyTable where key = 'G1') as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(env, path, rowValues, eplFiltered);

            var eplUnfiltered = "@name('s0') select (select * from MyTable) as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(env, path, rowValues, eplUnfiltered);

            // With @eventbean
            var eplEventBean = "@name('s0') select (select * from MyTable) @eventbean as mt from SupportBean_S2";
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
            var joinEpl = "@name('s0') select mt.* from MyTable as mt, SupportBean_S2 where key = P20";
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
            var joinEpl = "@name('s0') select mt.* as mymt from MyTable as mt, SupportBean_S2 where key = P20";
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
            var joinEpl = "@name('s0') select mt from MyTable as mt, SupportBean_S2 where key = P20";
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
            var joinEpl = "@name('s0') select * from MyTable, SupportBean_S2 where key = P20";
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

        internal class InfraTableSelectMultikeyWArrayComposite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string epl = "create table MyTable(k0 string primary key, k1 string primary key, k2 string primary key, v string);\n" +
                             "create index MyIndex on MyTable(k0, k1, v btree);\n" +
                             "insert into MyTable select p00 as k0, p01 as k1, p02 as k2, p03 as v from SupportBean_S0;\n" +
                             "@name('s0') select t.v as v from SupportBean_S1, MyTable as t where k0 = p10 and k1 = p11 and v > p12;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendS0(env, "A", "BB", "CCC", "X1");
                SendS0(env, "A", "BB", "DDDD", "X4");
                SendS0(env, "A", "CC", "CCC", "X3");
                SendS0(env, "C", "CC", "CCC", "X4");

                env.Milestone(0);

                SendS1Assert(env, "A", "CC", "", "X3");
                SendS1Assert(env, "C", "CC", "", "X4");
                SendS1Assert(env, "A", "BB", "X3", "X4");
                SendS1Assert(env, "A", "BB", "Z", null);

                env.UndeployAll();
            }

            private void SendS0(
                RegressionEnvironment env,
                string p00,
                string p01,
                string p02,
                string p03)
            {
                env.SendEventBean(new SupportBean_S0(0, p00, p01, p02, p03));
            }

            private void SendS1Assert(
                RegressionEnvironment env,
                string p10,
                string p11,
                string p12,
                string expected)
            {
                env.SendEventBean(new SupportBean_S1(0, p10, p11, p12));
                if (expected == null) {
                    Assert.IsFalse(env.Listener("s0").IsInvoked, expected);
                }
                else {
                    Assert.AreEqual(env.Listener("s0").AssertOneGetNewAndReset().Get("v"), expected);
                }
            }
        }

        internal class InfraTableSelectMultikeyWArrayTwoArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string epl = "create table MyTable(k1 int[primitive] primary key, k2 int[primitive] primary key, value int);\n" +
                             "insert into MyTable select intOne as k1, intTwo as k2, value from SupportEventWithManyArray(id = 'I');\n" +
                             "@name('s0') select t.value as c0 from SupportEventWithManyArray(id='Q'), MyTable as t where k1 = intOne and k2 = intTwo;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendManyArray(env, "I", new int[] {1, 2}, new int[] {3, 4}, 10);
                SendManyArray(env, "I", new int[] {1, 3}, new int[] {1}, 20);
                SendManyArray(env, "I", new int[] {2}, new int[] { }, 30);

                env.Milestone(0);

                SendManyArrayAssert(env, "Q", new int[] {2}, new int[0], 30);
                SendManyArrayAssert(env, "Q", new int[] {1, 2}, new int[] {3, 4}, 10);
                SendManyArrayAssert(env, "Q", new int[] {1, 3}, new int[] {1}, 20);

                env.UndeployAll();
            }

            private void SendManyArrayAssert(
                RegressionEnvironment env,
                string id,
                int[] intOne,
                int[] intTwo,
                int expected)
            {
                SendManyArray(env, id, intOne, intTwo, -1);
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] intOne,
                int[] intTwo,
                int value)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(intOne).WithIntTwo(intTwo).WithValue(value));
            }
        }

        internal class InfraTableSelectMultikeyWArraySingleArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string epl = "create table MyTable(k int[primitive] primary key, value int);\n" +
                             "insert into MyTable select array as k, value from SupportEventWithIntArray;\n" +
                             "@name('s0') select t.value as c0 from SupportEventWithManyArray, MyTable as t where k = intOne;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendIntArray(env, "E1", new int[] {1, 2}, 10);
                SendIntArray(env, "E2", new int[] {1, 3}, 20);
                SendIntArray(env, "E3", new int[] {2}, 30);

                env.Milestone(0);

                SendAssertManyArray(env, new int[] {2}, 30);
                SendAssertManyArray(env, new int[] {1, 3}, 20);
                SendAssertManyArray(env, new int[] {1, 2}, 10);

                env.UndeployAll();
            }

            private void SendAssertManyArray(
                RegressionEnvironment env,
                int[] ints,
                int expected)
            {
                env.SendEventBean(new SupportEventWithManyArray().WithIntOne(ints));
                Assert.AreEqual(expected, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            }

            private void SendIntArray(
                RegressionEnvironment env,
                string id,
                int[] ints,
                int value)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, ints, value));
            }
        }

        internal class InfraTableSelectEnum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table MyTable(p string);\n" +
                          "@name('s0') select t.firstOf() as c0 from MyTable as t;\n";
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
                    "@name('create') create table MyTable as (\n" +
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
                    new object[] {"p0", typeof(string)},
                    new object[] {"p1", typeof(string)},
                    new object[] {"totalInt", typeof(int?)},
                    new object[] {"totalLong", typeof(long?)},
                    new object[] {"winsb", typeof(SupportBean[])},
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
                    "update set p0 = P11, p1 = P12",
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