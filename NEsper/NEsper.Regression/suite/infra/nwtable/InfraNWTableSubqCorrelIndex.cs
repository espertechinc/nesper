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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqCorrelIndex : IndexBackingTableInfo
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            // named window tests
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(true, false, false, false, false)); // testNoShare
            execs.Add(
                new InfraNWTableSubqCorrelIndexAssertion(true, false, false, false, true)); // testNoShareSetnoindex
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(true, false, false, true, false)); // testNoShareCreate
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(true, true, false, false, false)); // testShare

            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(true, true, false, true, false)); // testShareCreate
            execs.Add(
                new InfraNWTableSubqCorrelIndexAssertion(true, true, false, true, true)); // testShareCreateSetnoindex
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(true, true, true, false, false)); // testDisableShare
            execs.Add(
                new InfraNWTableSubqCorrelIndexAssertion(true, true, true, true, false)); // testDisableShareCreate

            // table tests
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(false, false, false, false, false)); // table no-index
            execs.Add(new InfraNWTableSubqCorrelIndexAssertion(false, false, false, true, false)); // table yes-index

            execs.Add(new InfraNWTableSubqCorrelIndexMultipleIndexHints(true));
            execs.Add(new InfraNWTableSubqCorrelIndexMultipleIndexHints(false));

            execs.Add(new InfraNWTableSubqCorrelIndexShareIndexChoice(true));
            execs.Add(new InfraNWTableSubqCorrelIndexShareIndexChoice(false));

            execs.Add(new InfraNWTableSubqCorrelIndexNoIndexShareIndexChoice(true));
            execs.Add(new InfraNWTableSubqCorrelIndexNoIndexShareIndexChoice(false));

            return execs;
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            bool namedWindow,
            bool indexShare,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions)
        {
            var path = new RegressionPath();
            var epl = namedWindow
                ? "create window MyInfra." + datawindow + " as select * from SupportSimpleBeanOne"
                : "create table MyInfra(s1 string primary key, i1 int, d1 double, l1 long)";
            if (indexShare) {
                epl = "@Hint('enable_window_subquery_indexshare') " + epl;
            }

            env.CompileDeploy(epl, path);
            env.CompileDeploy("insert into MyInfra select * from SupportSimpleBeanOne", path);
            foreach (var index in indexes) {
                env.CompileDeploy(index, path);
            }

            foreach (var @event in preloadedEvents) {
                env.SendEventBean(@event);
            }

            var count = 0;
            foreach (var assertion in assertions) {
                log.Info("======= Testing #" + count++);
                var consumeEpl = INDEX_CALLBACK_HOOK +
                                 "@Name('s0') " +
                                 (assertion.Hint == null ? "" : assertion.Hint) +
                                 "select *, " +
                                 "(select * from MyInfra where " +
                                 assertion.WhereClause +
                                 ") @eventbean as ssb1 from SupportSimpleBeanTwo as ssb2";

                EPCompiled compiled;
                try {
                    compiled = env.CompileWCheckedEx(consumeEpl, path);
                }
                catch (EPCompileException ex) {
                    if (assertion.EventSendAssertion == null) {
                        // no assertion, expected
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                        continue;
                    }

                    throw new EPException("Unexpected statement exception: " + ex.Message, ex);
                }

                env.Deploy(compiled);

                // assert index and access
                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(
                    0,
                    assertion.ExpectedIndexName,
                    assertion.IndexBackingClass);
                env.AddListener("s0");
                assertion.EventSendAssertion.Invoke();
                env.UndeployModuleContaining("s0");
            }

            env.UndeployAll();
        }

        internal class InfraNWTableSubqCorrelIndexNoIndexShareIndexChoice : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableSubqCorrelIndexNoIndexShareIndexChoice(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var backingUniqueS1 = "unique hash={s1(string)} btree={} advanced={}";

                object[] preloadedEventsOne =
                    {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
                IndexAssertionEventSend eventSendAssertion = () => {
                    var fields = "s2,ssb1[0].s1,ssb1[0].i1".SplitCsv();
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", "E2", 20});
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", "E1", 10});
                };
                IndexAssertionEventSend noAssertion = () => { };

                // unique-s1
                AssertIndexChoice(
                    env,
                    namedWindow,
                    false,
                    new string[0],
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2",
                            namedWindow ? null : "MyInfra",
                            namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            namedWindow ? null : "MyInfra",
                            namedWindow ? BACKING_SINGLE_UNIQUE : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "i1 between 1 and 10",
                            null,
                            namedWindow ? BACKING_SORTED : null,
                            noAssertion),
                        new IndexAssertion(
                            null,
                            "l1 = ssb2.l2",
                            null,
                            namedWindow ? BACKING_SINGLE_DUPS : null,
                            eventSendAssertion)
                    });

                // unique-s1+i1
                if (namedWindow) {
                    AssertIndexChoice(
                        env,
                        namedWindow,
                        false,
                        new string[0],
                        preloadedEventsOne,
                        "std:unique(s1, d1)",
                        new[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", null, BACKING_SINGLE_DUPS, eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "s1 = ssb2.s2 and l1 = ssb2.l2",
                                null,
                                BACKING_MULTI_DUPS,
                                eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "s1 = ssb2.s2 and d1 = ssb2.d2",
                                null,
                                BACKING_MULTI_UNIQUE,
                                eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "s1 = ssb2.s2 and l1 = ssb2.l2 and d1 = ssb2.d2",
                                null,
                                BACKING_MULTI_UNIQUE,
                                eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "d1 = ssb2.d2 and s1 = ssb2.s2 and l1 = ssb2.l2",
                                null,
                                BACKING_MULTI_UNIQUE,
                                eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "l1 = ssb2.l2 and s1 = ssb2.s2 and d1 = ssb2.d2",
                                null,
                                BACKING_MULTI_UNIQUE,
                                eventSendAssertion)
                        });
                }
            }
        }

        internal class InfraNWTableSubqCorrelIndexShareIndexChoice : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableSubqCorrelIndexShareIndexChoice(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] noindexes = { };
                var backingUniqueS1 = "unique hash={s1(string)} btree={} advanced={}";
                var backingUniqueS1L1 = "unique hash={s1(string),l1(long)} btree={} advanced={}";
                var backingUniqueS1D1 = "unique hash={s1(string),d1(double)} btree={} advanced={}";
                var backingNonUniqueS1 = "non-unique hash={s1(string)} btree={} advanced={}";
                var backingNonUniqueD1 = "non-unique hash={d1(double)} btree={} advanced={}";
                var backingBtreeI1 = "non-unique hash={} btree={i1(int)} advanced={}";
                var backingBtreeD1 = "non-unique hash={} btree={d1(double)} advanced={}";
                var primaryIndexTable = namedWindow ? "MyNWIndex" : "MyInfra";
                var primaryIndexEPL = "create unique index MyNWIndex on MyInfra(s1)";

                object[] preloadedEventsOne =
                    {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
                IndexAssertionEventSend eventSendAssertion = () => {
                    var fields = "s2,ssb1[0].s1,ssb1[0].i1".SplitCsv();
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", "E2", 20});
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", "E1", 10});
                };

                // no index one field (essentially duplicated since declared std:unique)
                var primaryIndex = AppendArrayConditional(noindexes, namedWindow, primaryIndexEPL);
                AssertIndexChoice(
                    env,
                    namedWindow,
                    true,
                    primaryIndex,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2",
                            primaryIndexTable,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            primaryIndexTable,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            primaryIndexTable,
                            backingUniqueS1,
                            eventSendAssertion)
                    });

                // single index one field (essentially duplicated since declared std:unique)
                if (namedWindow) {
                    string[] indexOneField = {"create unique index One on MyInfra (s1)"};
                    AssertIndexChoice(
                        env,
                        namedWindow,
                        true,
                        indexOneField,
                        preloadedEventsOne,
                        "std:unique(s1)",
                        new[] {
                            new IndexAssertion(null, "s1 = ssb2.s2", "One", backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "s1 = ssb2.s2 and l1 = ssb2.l2",
                                "One",
                                backingUniqueS1,
                                eventSendAssertion),
                            new IndexAssertion(
                                "@Hint('index(One)')",
                                "s1 = ssb2.s2 and l1 = ssb2.l2",
                                "One",
                                backingUniqueS1,
                                eventSendAssertion)
                        });
                }

                // single index two field
                var secondaryEPL = "create unique index One on MyInfra (s1, l1)";
                var indexTwoField = AppendArrayConditional(secondaryEPL, namedWindow, primaryIndexEPL);
                AssertIndexChoice(
                    env,
                    namedWindow,
                    true,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2",
                            primaryIndexTable,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            "One",
                            backingUniqueS1L1,
                            eventSendAssertion)
                    });

                // two index one unique with std:unique(s1)
                string[] indexSetTwo = {
                    "create index One on MyInfra (s1)",
                    "create unique index Two on MyInfra (s1, d1)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    true,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "d1 = ssb2.d2", null, null, eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2",
                            namedWindow ? "Two" : "MyInfra",
                            namedWindow ? backingUniqueS1D1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                    });

                // two index one unique with keep-all
                AssertIndexChoice(
                    env,
                    namedWindow,
                    true,
                    indexSetTwo,
                    preloadedEventsOne,
                    "win:keepall()",
                    new[] {
                        new IndexAssertion(null, "d1 = ssb2.d2", null, null, eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "s1 = ssb2.s2 and l1 = ssb2.l2"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "s1 = ssb2.s2 and l1 = ssb2.l2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "s1 = ssb2.s2 and d1 = ssb2.d2 and l1 = ssb2.l2",
                            namedWindow ? "Two" : "MyInfra",
                            namedWindow ? backingUniqueS1D1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = ssb2.d2 and l1 = ssb2.l2") // busted
                    });

                // range
                IndexAssertionEventSend noAssertion = () => { };
                string[] indexSetThree = {
                    "create index One on MyInfra (i1 btree)",
                    "create index Two on MyInfra (d1 btree)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    true,
                    indexSetThree,
                    preloadedEventsOne,
                    "std:unique(s1)",
                    new[] {
                        new IndexAssertion(null, "i1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                        new IndexAssertion(null, "d1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                        new IndexAssertion("@Hint('index(One, bust)')", "d1 between 1 and 10") // busted
                    });

                env.UndeployAll();
            }
        }

        internal class InfraNWTableSubqCorrelIndexMultipleIndexHints : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableSubqCorrelIndexMultipleIndexHints(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "@Hint('enable_window_subquery_indexshare') create window MyInfraMIH#keepall as select * from SupportSimpleBeanOne"
                    : "create table MyInfraMIH(s1 String primary key, i1 int  primary key, d1 double primary key, l1 long primary key)";
                env.CompileDeploy(eplCreate, path);
                env.CompileDeploy("create unique index I1 on MyInfraMIH (s1)", path);
                env.CompileDeploy("create unique index I2 on MyInfraMIH (i1)", path);

                env.CompileDeploy(
                    INDEX_CALLBACK_HOOK +
                    "@Hint('index(subquery(1), I1, bust)')\n" +
                    "@Hint('index(subquery(0), I2, bust)')\n" +
                    "select " +
                    "(select * from MyInfraMIH where s1 = ssb2.s2 and i1 = ssb2.i2) as sub1," +
                    "(select * from MyInfraMIH where i1 = ssb2.i2 and s1 = ssb2.s2) as sub2 " +
                    "from SupportSimpleBeanTwo ssb2",
                    path);
                var subqueries = SupportQueryPlanIndexHook.GetAndResetSubqueries();
                subqueries.SortInPlace(
                    (
                        o1,
                        o2) => {
                        return o1.Tables[0].IndexName.CompareTo(o2.Tables[0].IndexName);
                    });
                SupportQueryPlanIndexHook.AssertSubquery(
                    subqueries[0],
                    1,
                    "I1",
                    "unique hash={s1(string)} btree={} advanced={}");
                SupportQueryPlanIndexHook.AssertSubquery(
                    subqueries[1],
                    0,
                    "I2",
                    "unique hash={i1(int)} btree={} advanced={}");

                env.UndeployAll();
            }
        }

        internal class InfraNWTableSubqCorrelIndexAssertion : RegressionExecution
        {
            private readonly bool createExplicitIndex;
            private readonly bool disableIndexShareConsumer;
            private readonly bool enableIndexShareCreate;
            private readonly bool namedWindow;
            private readonly bool setNoindex;

            public InfraNWTableSubqCorrelIndexAssertion(
                bool namedWindow,
                bool enableIndexShareCreate,
                bool disableIndexShareConsumer,
                bool createExplicitIndex,
                bool setNoindex)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
                this.disableIndexShareConsumer = disableIndexShareConsumer;
                this.createExplicitIndex = createExplicitIndex;
                this.setNoindex = setNoindex;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var createEpl = namedWindow
                    ? "create window MyInfraNWT#unique(TheString) as (TheString string, IntPrimitive int)"
                    : "create table MyInfraNWT(TheString string primary key, IntPrimitive int)";
                if (enableIndexShareCreate) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyInfraNWT select TheString, IntPrimitive from SupportBean", path);

                if (createExplicitIndex) {
                    env.CompileDeploy("@Name('index') create index MyIndex on MyInfraNWT (TheString)", path);
                }

                var consumeEpl =
                    "@Name('s0') select status.*, (select * from MyInfraNWT where TheString = SupportBean_S0.P00) @eventbean as details from SupportBean_S0 as status";
                if (disableIndexShareConsumer) {
                    consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
                }

                if (setNoindex) {
                    consumeEpl = "@Hint('set_noindex') " + consumeEpl;
                }

                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                var fields = "id,details[0].TheString,details[0].IntPrimitive".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E3", 30));

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "E1", 10});

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "E2", 20});

                // test late start
                env.UndeployModuleContaining("s0");
                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "E1", 10});

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "E2", 20});

                env.UndeployModuleContaining("s0");
                if (env.Statement("index") != null) {
                    env.UndeployModuleContaining("index");
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace