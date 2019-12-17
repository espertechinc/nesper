///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.@join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableJoin : IndexBackingTableInfo
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraFromClause());
            execs.Add(new InfraJoinIndexChoice());
            execs.Add(new InfraCoercion());
            execs.Add(new InfraUnkeyedTable());
            execs.Add(new InfraOuterJoin());
            return execs;
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            string eplDeclare,
            string eplPopulate,
            string eplQuery,
            string[] indexes,
            object[] preloadedEvents,
            AtomicLong milestone,
            IndexAssertion[] assertions)
        {
            AssertIndexChoice(
                env,
                eplDeclare,
                eplPopulate,
                eplQuery,
                indexes,
                preloadedEvents,
                assertions,
                milestone,
                false);
            AssertIndexChoice(
                env,
                eplDeclare,
                eplPopulate,
                eplQuery,
                indexes,
                preloadedEvents,
                assertions,
                milestone,
                true);
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            string eplDeclare,
            string eplPopulate,
            string eplQuery,
            string[] indexes,
            object[] preloadedEvents,
            IndexAssertion[] assertions,
            AtomicLong milestone,
            bool multistream)
        {
            var path = new RegressionPath();
            env.CompileDeploy(eplDeclare, path);
            env.CompileDeploy(eplPopulate, path);

            foreach (var index in indexes) {
                env.CompileDeploy(index, path);
            }

            foreach (var @event in preloadedEvents) {
                env.SendEventBean(@event);
            }

            env.MilestoneInc(milestone);

            var count = -1;
            foreach (var assertion in assertions) {
                count++;
                log.Info("======= Testing #" + count++);
                var epl = INDEX_CALLBACK_HOOK + (assertion.Hint == null ? "" : assertion.Hint) + eplQuery;
                epl += ", varagg as va";
                if (multistream) {
                    epl += ", SupportBeanSimple#lastevent";
                }

                epl += " where " + assertion.WhereClause;

                try {
                    var compiled = env.CompileWCheckedEx("@Name('s0')" + epl, path);
                    env.Deploy(compiled).AddListener("s0");
                }
                catch (EPCompileException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }

                    throw new EPException("Unexpected statement exception: " + ex.Message, ex);
                }

                // send multistream seed event
                env.SendEventBean(new SupportBeanSimple("", -1));

                // assert index and access
                assertion.EventSendAssertion.Invoke();
                var plan = SupportQueryPlanIndexHook.AssertJoinAndReset();

                TableLookupPlanForge tableLookupPlan;
                if (plan.ExecNodeSpecs[0] is TableLookupNodeForge) {
                    tableLookupPlan = ((TableLookupNodeForge) plan.ExecNodeSpecs[0]).TableLookupPlan;
                }
                else {
                    var lqp = (LookupInstructionQueryPlanNodeForge) plan.ExecNodeSpecs[0];
                    tableLookupPlan = lqp.LookupInstructions[0].LookupPlans[0];
                }

                Assert.AreEqual(assertion.ExpectedIndexName, tableLookupPlan.IndexNum[0].IndexName);
                Assert.AreEqual(assertion.ExpectedStrategy, tableLookupPlan.GetType());
                env.UndeployModuleContaining("s0");
            }

            env.UndeployAll();
        }

        private static void AssertValues(
            RegressionEnvironment env,
            string keys,
            int?[] values)
        {
            var keyarr = keys.SplitCsv();
            for (var i = 0; i < keyarr.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, keyarr[i]));
                if (values[i] == null) {
                    Assert.IsFalse(env.Listener("s0").IsInvoked);
                }
                else {
                    var @event = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
                }
            }
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        internal class InfraOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString"," p1" };
                var path = new RegressionPath();
                var epl = "create table MyTable as (p0 string primary key, p1 int);\n" +
                          "@Name('s0') select TheString, p1 from SupportBean unidirectional left outer join MyTable on TheString = p0;\n";
                env.CompileDeploy(epl, path).AddListener("s0");
                env.CompileExecuteFAF("insert into MyTable select 'a' as p0, 10 as p1", path);

                env.SendEventBean(new SupportBean("a", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"a", 10});

                env.SendEventBean(new SupportBean("b", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"b", null});

                env.UndeployAll();
            }
        }

        internal class InfraFromClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table varaggFC as (" +
                    "key string primary key, total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table varaggFC " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                        "@Name('s0') select total as value from SupportBean_S0 as S0, varaggFC as va " +
                        "where va.key = S0.P00",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 100));
                AssertValues(env, "G1,G2", new int?[] {100, null});

                env.SendEventBean(new SupportBean("G2", 200));
                AssertValues(env, "G1,G2", new int?[] {100, 200});

                env.UndeployAll();
            }
        }

        internal class InfraJoinIndexChoice : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare =
                    "create table varagg as (k0 string primary key, k1 int primary key, v1 string, total sum(long))";
                var eplPopulate =
                    "into table varagg select sum(LongPrimitive) as total from SupportBean group by TheString, IntPrimitive";
                var eplQuery = "select total as value from SupportBean_S0 as S0 unidirectional";

                string[] createIndexEmpty = { };
                object[] preloadedEventsTwo = {
                    MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)
                };
                var milestone = new AtomicLong();

                IndexAssertionEventSend eventSendAssertionRangeTwoExpected = () => {
                    env.SendEventBean(new SupportBean_S0(-1, null));
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.Listener("s0").NewDataListFlattened,
                        new [] { "value" },
                        new[] {new object[] {2000L}, new object[] {3000L}});
                    env.Listener("s0").Reset();
                };

                object[] preloadedEventsHash = {MakeEvent("G1", 10, 1000L)};
                IndexAssertionEventSend eventSendAssertionHash = () => {
                    env.SendEventBean(new SupportBean_S0(10, "G1"));
                    EPAssertionUtil.AssertPropsPerRow(
                        env.Listener("s0").NewDataListFlattened,
                        new [] { "value" },
                        new[] {new object[] {1000L}});
                    env.Listener("s0").Reset();
                };

                // no secondary indexes
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexEmpty,
                    preloadedEventsHash,
                    milestone,
                    new[] {
                        // primary index found
                        new IndexAssertion(
                            "k1 = Id and k0 = P00",
                            "varagg",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        new IndexAssertion(
                            "k0 = P00 and k1 = Id",
                            "varagg",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        new IndexAssertion(
                            "k0 = P00 and k1 = Id and v1 is null",
                            "varagg",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        // no index found
                        new IndexAssertion(
                            "k1 = Id",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertionHash)
                    }
                );

                // one secondary hash index on single field
                string[] createIndexHashSingleK1 = {"create index idx_k1 on varagg (k1)"};
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexHashSingleK1,
                    preloadedEventsHash,
                    milestone,
                    new[] {
                        // primary index found
                        new IndexAssertion(
                            "k1 = Id and k0 = P00",
                            "varagg",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        // secondary index found
                        new IndexAssertion(
                            "k1 = Id",
                            "idx_k1",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        new IndexAssertion(
                            "Id = k1",
                            "idx_k1",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        // no index found
                        new IndexAssertion(
                            "k0 = P00",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertionHash)
                    }
                );

                // two secondary hash indexes on one field each
                string[] createIndexHashTwoDiscrete =
                    {"create index idx_k1 on varagg (k1)", "create index idx_k0 on varagg (k0)"};
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexHashTwoDiscrete,
                    preloadedEventsHash,
                    milestone,
                    new[] {
                        // primary index found
                        new IndexAssertion(
                            "k1 = Id and k0 = P00",
                            "varagg",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        // secondary index found
                        new IndexAssertion(
                            "k0 = P00",
                            "idx_k0",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        new IndexAssertion(
                            "k1 = Id",
                            "idx_k1",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        new IndexAssertion(
                            "v1 is null and k1 = Id",
                            "idx_k1",
                            typeof(IndexedTableLookupPlanHashedOnlyForge),
                            eventSendAssertionHash),
                        // no index found
                        new IndexAssertion(
                            "1=1",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertionHash)
                    }
                );

                // one range secondary index
                // no secondary indexes
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexEmpty,
                    preloadedEventsTwo,
                    milestone,
                    new[] {
                        // no index found
                        new IndexAssertion(
                            "k1 between 20 and 30",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertionRangeTwoExpected)
                    }
                );

                // single range secondary index, expecting two events
                string[] createIndexRangeOne = {"create index b_k1 on varagg (k1 btree)"};
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexRangeOne,
                    preloadedEventsTwo,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "k1 between 20 and 30",
                            "b_k1",
                            typeof(SortedTableLookupPlanForge),
                            eventSendAssertionRangeTwoExpected),
                        new IndexAssertion(
                            "(k0 = 'G3' or k0 = 'G2') and k1 between 20 and 30",
                            "b_k1",
                            typeof(SortedTableLookupPlanForge),
                            eventSendAssertionRangeTwoExpected)
                    }
                );

                // single range secondary index, expecting single event
                IndexAssertionEventSend eventSendAssertionRangeOneExpected = () => {
                    env.SendEventBean(new SupportBean_S0(-1, null));
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.Listener("s0").NewDataListFlattened,
                        new [] { "value" },
                        new[] {new object[] {2000L}});
                    env.Listener("s0").Reset();
                };
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexRangeOne,
                    preloadedEventsTwo,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "k0 = 'G2' and k1 between 20 and 30",
                            "b_k1",
                            typeof(SortedTableLookupPlanForge),
                            eventSendAssertionRangeOneExpected),
                        new IndexAssertion(
                            "k1 between 20 and 30 and k0 = 'G2'",
                            "b_k1",
                            typeof(SortedTableLookupPlanForge),
                            eventSendAssertionRangeOneExpected)
                    }
                );

                // combined hash+range index
                string[] createIndexRangeCombined = {"create index h_k0_b_k1 on varagg (k0 hash, k1 btree)"};
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexRangeCombined,
                    preloadedEventsTwo,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "k0 = 'G2' and k1 between 20 and 30",
                            "h_k0_b_k1",
                            typeof(CompositeTableLookupPlanForge),
                            eventSendAssertionRangeOneExpected),
                        new IndexAssertion(
                            "k1 between 20 and 30 and k0 = 'G2'",
                            "h_k0_b_k1",
                            typeof(CompositeTableLookupPlanForge),
                            eventSendAssertionRangeOneExpected)
                    }
                );

                string[] createIndexHashSingleK0 = {"create index idx_k0 on varagg (k0)"};
                // in-keyword single-directional use
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexHashSingleK0,
                    preloadedEventsTwo,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "k0 in ('G2', 'G3')",
                            "idx_k0",
                            typeof(InKeywordTableLookupPlanSingleIdxForge),
                            eventSendAssertionRangeTwoExpected)
                    }
                );
                // in-keyword multi-directional use
                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexHashSingleK0,
                    preloadedEventsHash,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "'G1' in (k0)",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertionHash)
                    }
                );
            }
        }

        internal class InfraCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "create table varagg as (k0 int primary key, total sum(long))";
                var eplPopulate =
                    "into table varagg select sum(LongPrimitive) as total from SupportBean group by IntPrimitive";
                var eplQuery = "select total as value from SupportBeanRange unidirectional";

                string[] createIndexEmpty = { };
                object[] preloadedEvents = {
                    MakeEvent("G1", 10, 1000L), MakeEvent("G2", 20, 2000L),
                    MakeEvent("G3", 30, 3000L), MakeEvent("G4", 40, 4000L)
                };
                var milestone = new AtomicLong();

                IndexAssertionEventSend eventSendAssertion = () => {
                    env.SendEventBean(new SupportBeanRange(20L));
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.Listener("s0").NewDataListFlattened,
                        new [] { "value" },
                        new[] {new object[] {2000L}});
                    env.Listener("s0").Reset();
                };

                AssertIndexChoice(
                    env,
                    eplDeclare,
                    eplPopulate,
                    eplQuery,
                    createIndexEmpty,
                    preloadedEvents,
                    milestone,
                    new[] {
                        new IndexAssertion(
                            "k0 = KeyLong",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertion),
                        new IndexAssertion(
                            "k0 = KeyLong",
                            "varagg",
                            typeof(FullTableScanUniquePerKeyLookupPlanForge),
                            eventSendAssertion)
                    }
                );
            }
        }

        internal class InfraUnkeyedTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // Prepare
                env.CompileDeploy("create table MyTable (sumint sum(int))", path);
                env.CompileDeploy(
                    "@Name('into') into table MyTable select sum(IntPrimitive) as sumint from SupportBean",
                    path);
                env.SendEventBean(new SupportBean("E1", 100));
                env.SendEventBean(new SupportBean("E2", 101));
                env.UndeployModuleContaining("into");

                // join simple
                env.CompileDeploy("@Name('join') select sumint from MyTable, SupportBean", path).AddListener("join");
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(201, env.Listener("join").AssertOneGetNewAndReset().Get("sumint"));
                env.UndeployModuleContaining("join");

                // test regular columns inserted-into
                env.CompileDeploy("create table SecondTable (a string, b int)", path);
                env.CompileExecuteFAF("insert into SecondTable values ('a1', 10)", path);
                env.CompileDeploy("@Name('s0')select a, b from SecondTable, SupportBean", path).AddListener("s0");
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a","b" },
                    new object[] {"a1", 10});

                env.UndeployAll();
            }
        }
    }
} // end of namespace