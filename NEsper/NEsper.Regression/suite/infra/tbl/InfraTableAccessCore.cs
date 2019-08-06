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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableAccessCore
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            execs.Add(new InfraTableAccessCoreUnGroupedWindowAndSum());
            execs.Add(new InfraIntegerIndexedPropertyLookAlike());
            execs.Add(new InfraFilterBehavior());
            execs.Add(new InfraExprSelectClauseRenderingUnnamedCol());
            execs.Add(new InfraTopLevelReadGrouped2Keys());
            execs.Add(new InfraTopLevelReadUnGrouped());
            execs.Add(new InfraExpressionAliasAndDecl());
            execs.Add(new InfraGroupedTwoKeyNoContext());
            execs.Add(new InfraGroupedThreeKeyNoContext());
            execs.Add(new InfraGroupedSingleKeyNoContext());
            execs.Add(new InfraUngroupedWContext());
            execs.Add(new InfraOrderOfAggregationsAndPush());
            execs.Add(new InfraMultiStmtContributing());
            execs.Add(new InfraGroupedMixedMethodAndAccess());
            execs.Add(new InfraNamedWindowAndFireAndForget());
            execs.Add(new InfraSubquery());
            execs.Add(new InfraOnMergeExpressions());
            execs.Add(new InfraTableAccessCoreSplitStream());
            return execs;
        }

        private static void TryAssertionGroupedMixedMethodAndAccess(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varMyAgg (" +
                             "key string primary key, " +
                             "c0 count(*), " +
                             "c1 count(distinct int), " +
                             "c2 window(*) @type('SupportBean'), " +
                             "c3 sum(long)" +
                             ")";
            env.CompileDeploy(soda, eplDeclare, path);

            var eplBind = "into table varMyAgg select " +
                          "count(*) as c0, " +
                          "count(distinct IntPrimitive) as c1, " +
                          "window(*) as c2, " +
                          "sum(LongPrimitive) as c3 " +
                          "from SupportBean#length(3) group by TheString";
            env.CompileDeploy(soda, eplBind, path);

            var eplSelect = "@Name('s0') select " +
                            "varMyAgg[P00].c0 as c0, " +
                            "varMyAgg[P00].c1 as c1, " +
                            "varMyAgg[P00].c2 as c2, " +
                            "varMyAgg[P00].c3 as c3" +
                            " from SupportBean_S0";
            env.CompileDeploy(soda, eplSelect, path).AddListener("s0");
            var fields = "c0,c1,c2,c3".SplitCsv();

            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c0"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c1"));
            Assert.AreEqual(typeof(SupportBean[]), eventType.GetPropertyType("c2"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c3"));

            var b1 = MakeSendBean(env, "E1", 10, 100);
            var b2 = MakeSendBean(env, "E1", 11, 101);
            var b3 = MakeSendBean(env, "E1", 10, 102);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {3L, 2L, new[] {b1, b2, b3}, 303L});

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null, null});

            var b4 = MakeSendBean(env, "E2", 20, 200);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1L, 1L, new[] {b4}, 200L});

            env.UndeployAll();
        }

        private static void TryAssertionTopLevelSingle(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            SendEventsAndAssert(env, "A", 10, "A", 10);
            SendEventsAndAssert(env, "A", 11, "A", 21);
            SendEventsAndAssert(env, "B", 20, "A", 21);

            env.MilestoneInc(milestone);

            SendEventsAndAssert(env, "B", 21, "B", 41);
            SendEventsAndAssert(env, "C", 30, "A", 21);
            SendEventsAndAssert(env, "D", 40, "C", 30);

            var fields = "c0,c1".SplitCsv();
            int[] expected = {21, 41, 30, 40};
            var count = 0;
            foreach (var p00 in "A,B,C,D".SplitCsv()) {
                env.SendEventBean(new SupportBean_S0(0, p00));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {p00, expected[count]});
                count++;
            }

            env.SendEventBean(new SupportBean_S0(0, "A"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A", 21});
        }

        private static void SendEventsAndAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            string p00,
            int total)
        {
            var fields = "c0,c1".SplitCsv();
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            env.SendEventBean(new SupportBean_S0(0, p00));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {p00, total});
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            return MakeSendBean(env, theString, intPrimitive, longPrimitive, -1);
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            return MakeSendBean(env, theString, intPrimitive, longPrimitive, doublePrimitive, -1);
        }

        private static SupportBean MakeSendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive,
            float floatPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            bean.FloatPrimitive = floatPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        private static void AssertTopLevelTypeInfo(EPStatement stmt)
        {
            Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("val0"));
            var fragType = stmt.EventType.GetFragmentType("val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(object[][]), fragType.FragmentType.GetPropertyType("thewindow"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("thetotal"));
        }

        private static void AssertIntegerIndexed(
            EventBean @event,
            SupportBean[] events)
        {
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c0.myevents"));
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c1"));
            Assert.AreEqual(events[events.Length - 1], @event.Get("c2"));
            Assert.AreEqual(events[events.Length - 2], @event.Get("c3"));
        }

        private static SupportBean_S1 MakeSendS1(
            RegressionEnvironment env,
            int id,
            string p10)
        {
            var bean = new SupportBean_S1(id, p10);
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean_S0 MakeSendS0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            var bean = new SupportBean_S0(id, p00);
            env.SendEventBean(bean);
            return bean;
        }

        private static void TryAssertionOrderOfAggs(
            RegressionEnvironment env,
            bool ungrouped,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "create table varaggOOA (" +
                             (ungrouped ? "" : "key string primary key, ") +
                             "sumint sum(int), " +
                             "sumlong sum(long), " +
                             "mysort sorted(IntPrimitive) @type(SupportBean)," +
                             "mywindow window(*) @type(SupportBean)" +
                             ")";
            env.CompileDeploy(eplDeclare, path);

            var fieldsTable = "sumint,sumlong,mywindow,mysort".SplitCsv();
            var eplSelect = "@Name('into') into table varaggOOA select " +
                            "sum(LongPrimitive) as sumlong, " +
                            "sum(IntPrimitive) as sumint, " +
                            "window(*) as mywindow," +
                            "sorted() as mysort " +
                            "from SupportBean#length(2) " +
                            (ungrouped ? "" : "group by TheString ");
            env.CompileDeploy(eplSelect, path).AddListener("into");

            var fieldsSelect = "c0,c1,c2,c3".SplitCsv();
            var groupKey = ungrouped ? "" : "['E1']";
            env.CompileDeploy(
                    "@Name('s0') select " +
                    "varaggOOA" +
                    groupKey +
                    ".sumint as c0, " +
                    "varaggOOA" +
                    groupKey +
                    ".sumlong as c1," +
                    "varaggOOA" +
                    groupKey +
                    ".mywindow as c2," +
                    "varaggOOA" +
                    groupKey +
                    ".mysort as c3 from SupportBean_S0",
                    path)
                .AddListener("s0");

            var e1 = MakeSendBean(env, "E1", 10, 100);
            EPAssertionUtil.AssertProps(
                env.Listener("into").AssertOneGetNewAndReset(),
                fieldsTable,
                new object[] {
                    10, 100L,
                    new object[] {e1},
                    new object[] {e1}
                });

            env.MilestoneInc(milestone);

            var e2 = MakeSendBean(env, "E1", 5, 50);
            EPAssertionUtil.AssertProps(
                env.Listener("into").AssertOneGetNewAndReset(),
                fieldsTable,
                new object[] {
                    15, 150L,
                    new object[] {e1, e2},
                    new object[] {e2, e1}
                });

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsSelect,
                new object[] {
                    15, 150L,
                    new object[] {e1, e2},
                    new object[] {e2, e1}
                });

            env.MilestoneInc(milestone);

            var e3 = MakeSendBean(env, "E1", 12, 120);
            EPAssertionUtil.AssertProps(
                env.Listener("into").AssertOneGetNewAndReset(),
                fieldsTable,
                new object[] {
                    17, 170L,
                    new object[] {e2, e3},
                    new object[] {e2, e3}
                });

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsSelect,
                new object[] {
                    17, 170L,
                    new object[] {e2, e3},
                    new object[] {e2, e3}
                });

            env.UndeployAll();
        }

        internal class InfraIntegerIndexedPropertyLookAlike : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionIntegerIndexedPropertyLookAlike(env, false);
                TryAssertionIntegerIndexedPropertyLookAlike(env, true);
            }

            private static void TryAssertionIntegerIndexedPropertyLookAlike(
                RegressionEnvironment env,
                bool soda)
            {
                var path = new RegressionPath();
                var eplDeclare =
                    "@Name('infra') create table varaggIIP (key int primary key, myevents window(*) @type('SupportBean'))";
                env.CompileDeploy(soda, eplDeclare, path);
                Assert.AreEqual(
                    StatementType.CREATE_TABLE,
                    env.Statement("infra").GetProperty(StatementProperty.STATEMENTTYPE));

                var eplInto =
                    "into table varaggIIP select window(*) as myevents from SupportBean#length(3) group by IntPrimitive";
                env.CompileDeploy(soda, eplInto, path);

                var eplSelect =
                    "@Name('s0') select varaggIIP[1] as c0, varaggIIP[1].myevents as c1, varaggIIP[1].myevents.last(*) as c2, varaggIIP[1].myevents.last(*,1) as c3 from SupportBean_S0";
                env.CompileDeploy(soda, eplSelect, path).AddListener("s0");

                var e1 = MakeSendBean(env, "E1", 1, 10L);
                var e2 = MakeSendBean(env, "E2", 1, 20L);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0));
                AssertIntegerIndexed(env.Listener("s0").AssertOneGetNewAndReset(), new[] {e1, e2});

                env.UndeployAll();
            }
        }

        internal class InfraFilterBehavior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table varaggFB (total count(*))", path);
                env.CompileDeploy("into table varaggFB select count(*) as total from SupportBean_S0", path);
                env.CompileDeploy("@Name('s0') select * from SupportBean(varaggFB.total = IntPrimitive)", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class InfraExprSelectClauseRenderingUnnamedCol : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table varaggESC (" +
                    "key string primary key, theEvents window(*) @type(SupportBean))",
                    path);

                env.CompileDeploy(
                    "@Name('s0') select " +
                    "varaggESC.keys()," +
                    "varaggESC[P00].theEvents," +
                    "varaggESC[P00]," +
                    "varaggESC[P00].theEvents.last(*)," +
                    "varaggESC[P00].theEvents.window(*).take(1) from SupportBean_S0",
                    path);

                object[][] expectedAggType = {
                    new object[] {"varaggESC.keys()", typeof(object[])},
                    new object[] {"varaggESC[P00].theEvents", typeof(SupportBean[])},
                    new object[] {"varaggESC[P00]", typeof(IDictionary<string, object>)},
                    new object[] {"varaggESC[P00].theEvents.last(*)", typeof(SupportBean)},
                    new object[] {"varaggESC[P00].theEvents.window(*).take(1)", typeof(ICollection<object>)}
                };
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedAggType,
                    env.Statement("s0").EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);
                env.UndeployAll();
            }
        }

        internal class InfraTopLevelReadGrouped2Keys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionTopLevelReadGrouped2Keys(env, false);
                TryAssertionTopLevelReadGrouped2Keys(env, true);
            }

            private static void TryAssertionTopLevelReadGrouped2Keys(
                RegressionEnvironment env,
                bool soda)
            {
                var path = new RegressionPath();
                var typeCompiled = env.Compile(
                    "create objectarray schema MyEventOA as (c0 int, c1 string, c2 int)",
                    options => {
                        options.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
                        options.BusModifierEventType = ctx => EventTypeBusModifier.BUS;
                    });

                env.Deploy(typeCompiled);
                path.Add(typeCompiled);

                env.CompileDeploy(
                    soda,
                    "create table windowAndTotalTLP2K (" +
                    "keyi int primary key, keys string primary key, thewindow window(*) @type('MyEventOA'), thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    soda,
                    "into table windowAndTotalTLP2K " +
                    "select window(*) as thewindow, sum(c2) as thetotal from MyEventOA#length(2) group by c0, c1",
                    path);

                env.CompileDeploy(
                        soda,
                        "@Name('s0') select windowAndTotalTLP2K[Id,P00] as val0 from SupportBean_S0",
                        path)
                    .AddListener("s0");
                AssertTopLevelTypeInfo(env.Statement("s0"));

                object[] e1 = {10, "G1", 100};
                env.SendEventObjectArray(e1, "MyEventOA");

                var fieldsInner = "thewindow,thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(10, "G1"));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e1},
                    100);

                object[] e2 = {20, "G2", 200};
                env.SendEventObjectArray(e2, "MyEventOA");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e2},
                    200);

                object[] e3 = {20, "G2", 300};
                env.SendEventObjectArray(e3, "MyEventOA");

                env.SendEventBean(new SupportBean_S0(10, "G1"));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    null,
                    null);
                env.SendEventBean(new SupportBean_S0(20, "G2"));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e2, e3},
                    500);

                // test typable output
                env.UndeployModuleContaining("s0");
                env.CompileDeploy(
                    "@Name('i1') insert into OutStream select windowAndTotalTLP2K[20, 'G2'] as val0 from SupportBean_S0",
                    path);
                env.AddListener("i1");

                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertProps(
                    env.Listener("i1").AssertOneGetNewAndReset(),
                    "val0.thewindow,val0.thetotal".SplitCsv(),
                    new object[] {new[] {e2, e3}, 500});

                env.UndeployAll();
            }
        }

        internal class InfraTopLevelReadUnGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportUpdateListener();
                object[] e1 = {10};
                object[] e2 = {20};
                object[] e3 = {30};

                var path = new RegressionPath();
                var typeCompiled = env.Compile(
                    "create objectarray schema MyEventOATLRU(c0 int)",
                    options => {
                        options.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
                        options.BusModifierEventType = ctx => EventTypeBusModifier.BUS;
                    });
                env.Deploy(typeCompiled);
                path.Add(typeCompiled);

                env.CompileDeploy(
                    "create table windowAndTotalTLRUG (" +
                    "thewindow window(*) @type(MyEventOATLRU), thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    "into table windowAndTotalTLRUG " +
                    "select window(*) as thewindow, sum(c0) as thetotal from MyEventOATLRU#length(2)",
                    path);

                env.CompileDeploy("@Name('s0') select windowAndTotalTLRUG as val0 from SupportBean_S0", path);
                env.AddListener("s0");

                env.SendEventObjectArray(e1, "MyEventOATLRU");

                var fieldsInner = "thewindow,thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e1},
                    10);

                env.SendEventObjectArray(e2, "MyEventOATLRU");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e1, e2},
                    30);

                env.SendEventObjectArray(e3, "MyEventOATLRU");

                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e2, e3},
                    50);

                // test typable output
                env.UndeployModuleContaining("s0");

                env.CompileDeploy(
                        "create schema AggBean as " +
                        typeof(AggBean).Name +
                        ";\n" +
                        "@Name('s0') insert into AggBean select windowAndTotalTLRUG as val0 from SupportBean_S0;\n",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "val0.thewindow,val0.thetotal".SplitCsv(),
                    new object[] {new[] {e2, e3}, 50});

                env.UndeployAll();
            }
        }

        internal class InfraExpressionAliasAndDecl : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionIntoTableFromExpression(env, milestone);

                TryAssertionExpressionHasTableAccess(env, milestone);

                TryAssertionSubqueryWithExpressionHasTableAccess(env, milestone);
            }

            private static void TryAssertionSubqueryWithExpressionHasTableAccess(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTableTwo(TheString string primary key, IntPrimitive int)", path);
                env.CompileDeploy(
                    "create expression getMyValue{o => (select MyTableTwo[o.P00].IntPrimitive from SupportBean_S1#lastevent)}",
                    path);
                env.CompileDeploy("insert into MyTableTwo select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S1(1000));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0".SplitCsv(),
                    new object[] {2});

                env.UndeployAll();
            }

            private static void TryAssertionExpressionHasTableAccess(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table MyTableOne(TheString string primary key, IntPrimitive int)", path);
                env.CompileDeploy("create expression getMyValue{o => MyTableOne[o.P00].IntPrimitive}", path);
                env.CompileDeploy("insert into MyTableOne select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0".SplitCsv(),
                    new object[] {2});

                env.UndeployAll();
            }

            private static void TryAssertionIntoTableFromExpression(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create expression sumi {a => sum(IntPrimitive)}", path);
                env.CompileDeploy("create expression sumd alias for {sum(DoublePrimitive)}", path);
                env.CompileDeploy(
                    "create table varaggITFE (" +
                    "sumi sum(int), sumd sum(double), sumf sum(float), suml sum(long))",
                    path);
                env.CompileDeploy(
                    "expression suml alias for {sum(LongPrimitive)} " +
                    "into table varaggITFE " +
                    "select suml, sum(FloatPrimitive) as sumf, sumd, sumi(sb) from SupportBean as sb",
                    path);

                MakeSendBean(env, "E1", 10, 100L, 1000d, 10000f);

                var fields = "varaggITFE.sumi,varaggITFE.sumd,varaggITFE.sumf,varaggITFE.suml";
                var listener = new SupportUpdateListener();
                env.CompileDeploy("@Name('s0') select " + fields + " from SupportBean_S0", path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {10, 1000d, 10000f, 100L});

                env.MilestoneInc(milestone);

                MakeSendBean(env, "E1", 11, 101L, 1001d, 10001f);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields.SplitCsv(),
                    new object[] {21, 2001d, 20001f, 201L});

                env.UndeployAll();
            }
        }

        internal class InfraGroupedTwoKeyNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare =
                    "create table varTotalG2K (Key0 string primary key, key1 int primary key, total sum(long), cnt count(*))";
                env.CompileDeploy(eplDeclare, path);

                var eplBind = "into table varTotalG2K " +
                              "select sum(LongPrimitive) as total, count(*) as cnt " +
                              "from SupportBean group by TheString, IntPrimitive";
                env.CompileDeploy(eplBind, path);

                var eplUse =
                    "@Name('s0') select varTotalG2K[P00, Id].total as c0, varTotalG2K[P00, Id].cnt as c1 from SupportBean_S0";
                env.CompileDeploy(eplUse, path).AddListener("s0");

                MakeSendBean(env, "E1", 10, 100);

                var fields = "c0,c1".SplitCsv();
                env.SendEventBean(new SupportBean_S0(10, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100L, 1L});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});
                env.SendEventBean(new SupportBean_S0(10, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }

        internal class InfraGroupedThreeKeyNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "create table varTotalG3K (Key0 string primary key, key1 int primary key," +
                                 "key2 long primary key, total sum(double), cnt count(*))";
                env.CompileDeploy(eplDeclare, path);

                var eplBind = "into table varTotalG3K " +
                              "select sum(DoublePrimitive) as total, count(*) as cnt " +
                              "from SupportBean group by TheString, IntPrimitive, LongPrimitive";
                env.CompileDeploy(eplBind, path);

                var fields = "c0,c1".SplitCsv();
                var eplUse =
                    "@Name('s0') select varTotalG3K[P00, Id, 100L].total as c0, varTotalG3K[P00, Id, 100L].cnt as c1 from SupportBean_S0";
                env.CompileDeploy(eplUse, path).AddListener("s0");

                MakeSendBean(env, "E1", 10, 100, 1000);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1000.0, 1L});

                env.Milestone(1);

                MakeSendBean(env, "E1", 10, 100, 1001);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2001.0, 2L});

                env.UndeployAll();
            }
        }

        internal class InfraGroupedSingleKeyNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionGroupedSingleKeyNoContext(env, false, milestone);
                TryAssertionGroupedSingleKeyNoContext(env, true, milestone);
            }

            private static void TryAssertionGroupedSingleKeyNoContext(
                RegressionEnvironment env,
                bool soda,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                var eplDeclare = "create table varTotalG1K (key string primary key, total sum(int))";
                env.CompileDeploy(soda, eplDeclare, path);

                var eplBind = "into table varTotalG1K " +
                              "select TheString, sum(IntPrimitive) as total from SupportBean group by TheString";
                env.CompileDeploy(soda, eplBind, path);

                var eplUse = "@Name('s0') select P00 as c0, varTotalG1K[P00].total as c1 from SupportBean_S0";
                env.CompileDeploy(soda, eplUse, path).AddListener("s0");

                TryAssertionTopLevelSingle(env, milestone);

                env.UndeployAll();
            }
        }

        internal class InfraUngroupedWContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplPart =
                    "create context PartitionedByString partition by TheString from SupportBean, P00 from SupportBean_S0;\n" +
                    "context PartitionedByString create table varTotalUG (total sum(int));\n" +
                    "context PartitionedByString into table varTotalUG select sum(IntPrimitive) as total from SupportBean;\n" +
                    "@Name('s0') context PartitionedByString select P00 as c0, varTotalUG.total as c1 from SupportBean_S0;\n";
                env.CompileDeploy(eplPart);
                env.AddListener("s0");

                TryAssertionTopLevelSingle(env, new AtomicLong());

                env.UndeployAll();
            }
        }

        internal class InfraOrderOfAggregationsAndPush : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionOrderOfAggs(env, true, milestone);
                TryAssertionOrderOfAggs(env, false, milestone);
            }
        }

        internal class InfraMultiStmtContributing : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                TryAssertionMultiStmtContributingDifferentAggs(env, false, milestone);
                TryAssertionMultiStmtContributingDifferentAggs(env, true, milestone);

                // contribute to the same aggregation
                var path = new RegressionPath();
                env.CompileDeploy("create table sharedagg (total sum(int))", path);
                env.CompileDeploy(
                        "@Name('i1') into table sharedagg " +
                        "select P00 as c0, sum(Id) as total from SupportBean_S0",
                        path)
                    .AddListener("i1");
                env.CompileDeploy(
                        "@Name('i2') into table sharedagg " +
                        "select P10 as c0, sum(Id) as total from SupportBean_S1",
                        path)
                    .AddListener("i2");
                env.CompileDeploy("@Name('s0') select TheString as c0, sharedagg.total as total from SupportBean", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "A"));
                AssertMultiStmtContributingTotal(env, env.Listener("i1"), "A", 10);

                env.SendEventBean(new SupportBean_S1(-5, "B"));
                AssertMultiStmtContributingTotal(env, env.Listener("i2"), "B", 5);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(2, "C"));
                AssertMultiStmtContributingTotal(env, env.Listener("i1"), "C", 7);

                env.UndeployAll();
            }

            private static void AssertMultiStmtContributingTotal(
                RegressionEnvironment env,
                SupportListener listener,
                string c0,
                int total)
            {
                var fields = "c0,total".SplitCsv();
                EPAssertionUtil.AssertProps(
                    listener.AssertOneGetNewAndReset(),
                    fields,
                    new object[] {c0, total});

                env.SendEventBean(new SupportBean(c0, 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {c0, total});
            }

            private static void TryAssertionMultiStmtContributingDifferentAggs(
                RegressionEnvironment env,
                bool grouped,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                var eplDeclare = "create table varaggMSC (" +
                                 (grouped ? "key string primary key," : "") +
                                 "s0sum sum(int), s0cnt count(*), s0win window(*) @type(SupportBean_S0)," +
                                 "s1sum sum(int), s1cnt count(*), s1win window(*) @type(SupportBean_S1)" +
                                 ")";
                env.CompileDeploy(eplDeclare, path);

                var fieldsSelect = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var eplSelectUngrouped = "@Name('s0') select varaggMSC.s0sum as c0, varaggMSC.s0cnt as c1," +
                                         "varaggMSC.s0win as c2, varaggMSC.s1sum as c3, varaggMSC.s1cnt as c4," +
                                         "varaggMSC.s1win as c5 from SupportBean";
                var eplSelectGrouped =
                    "@Name('s0') select varaggMSC[TheString].s0sum as c0, varaggMSC[TheString].s0cnt as c1," +
                    "varaggMSC[TheString].s0win as c2, varaggMSC[TheString].s1sum as c3, varaggMSC[TheString].s1cnt as c4," +
                    "varaggMSC[TheString].s1win as c5 from SupportBean";
                env.CompileDeploy(grouped ? eplSelectGrouped : eplSelectUngrouped, path).AddListener("s0");

                var fieldsOne = "s0sum,s0cnt,s0win".SplitCsv();
                var eplBindOne =
                    "@Name('s1') into table varaggMSC select sum(Id) as s0sum, count(*) as s0cnt, window(*) as s0win from SupportBean_S0#length(2) " +
                    (grouped ? "group by P00" : "");
                env.CompileDeploy(eplBindOne, path).AddListener("s1");

                var fieldsTwo = "s1sum,s1cnt,s1win".SplitCsv();
                var eplBindTwo =
                    "@Name('s2') into table varaggMSC select sum(Id) as s1sum, count(*) as s1cnt, window(*) as s1win from SupportBean_S1#length(2) " +
                    (grouped ? "group by P10" : "");
                env.CompileDeploy(eplBindTwo, path).AddListener("s2");

                // contribute S1
                var s1Bean1 = MakeSendS1(env, 10, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        10, 1L,
                        new object[] {s1Bean1}
                    });

                env.SendEventBean(new SupportBean("G1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {
                        null, 0L, null, 10, 1L,
                        new object[] {s1Bean1}
                    });

                env.MilestoneInc(milestone);

                // contribute S0
                var s0Bean1 = MakeSendS0(env, 20, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        20, 1L,
                        new object[] {s0Bean1}
                    });

                env.SendEventBean(new SupportBean("G1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {
                        20, 1L,
                        new object[] {s0Bean1}, 10, 1L,
                        new object[] {s1Bean1}
                    });

                // contribute S1 and S0
                var s1Bean2 = MakeSendS1(env, 11, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        21, 2L,
                        new object[] {s1Bean1, s1Bean2}
                    });
                var s0Bean2 = MakeSendS0(env, 21, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        41, 2L,
                        new object[] {s0Bean1, s0Bean2}
                    });

                env.SendEventBean(new SupportBean("G1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {
                        41, 2L,
                        new object[] {s0Bean1, s0Bean2}, 21, 2L,
                        new object[] {s1Bean1, s1Bean2}
                    });

                env.MilestoneInc(milestone);

                // contribute S1 and S0 (leave)
                var s1Bean3 = MakeSendS1(env, 12, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        23, 2L,
                        new object[] {s1Bean2, s1Bean3}
                    });
                var s0Bean3 = MakeSendS0(env, 22, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        43, 2L,
                        new object[] {s0Bean2, s0Bean3}
                    });

                env.SendEventBean(new SupportBean("G1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsSelect,
                    new object[] {
                        43, 2L,
                        new object[] {s0Bean2, s0Bean3}, 23, 2L,
                        new object[] {s1Bean2, s1Bean3}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraGroupedMixedMethodAndAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionGroupedMixedMethodAndAccess(env, false, milestone);
                TryAssertionGroupedMixedMethodAndAccess(env, true, milestone);
            }
        }

        internal class InfraNamedWindowAndFireAndForget : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create window MyWindow#length(2) as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "create table varaggNWFAF (total sum(int));\n" +
                          "into table varaggNWFAF select sum(IntPrimitive) as total from MyWindow;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                var resultSelect = env.CompileExecuteFAF("select varaggNWFAF.total as c0 from MyWindow", path);
                Assert.AreEqual(10, resultSelect.Array[0].Get("c0"));

                var resultDelete = env.CompileExecuteFAF(
                    "delete from MyWindow where varaggNWFAF.total = IntPrimitive",
                    path);
                Assert.AreEqual(1, resultDelete.Array.Length);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                var resultUpdate = env.CompileExecuteFAF(
                    "update MyWindow set DoublePrimitive = 100 where varaggNWFAF.total = IntPrimitive",
                    path);
                Assert.AreEqual(100d, resultUpdate.Array[0].Get("DoublePrimitive"));

                var resultInsert = env.CompileExecuteFAF(
                    "insert into MyWindow (TheString, IntPrimitive) values ('A', varaggNWFAF.total)",
                    path);
                EPAssertionUtil.AssertProps(
                    resultInsert.Array[0],
                    "theString,IntPrimitive".SplitCsv(),
                    new object[] {"A", 20});

                env.UndeployAll();
            }
        }

        internal class InfraSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table subquery_var_agg (key string primary key, total count(*))", path);
                env.CompileDeploy(
                        "@Name('s0') select (select subquery_var_agg[P00].total from SupportBean_S0#lastevent) as c0 " +
                        "from SupportBean_S1",
                        path)
                    .AddListener("s0");
                env.CompileDeploy(
                    "into table subquery_var_agg select count(*) as total from SupportBean group by TheString",
                    path);

                env.SendEventBean(new SupportBean("E1", -1));
                env.SendEventBean(new SupportBean_S0(0, "E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(1));
                Assert.AreEqual(1L, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.SendEventBean(new SupportBean("E1", -1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(2));
                Assert.AreEqual(2L, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class InfraOnMergeExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create table the_table (key string primary key, total count(*), value int)", path);
                env.CompileDeploy(
                    "into table the_table select count(*) as total from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                    "on SupportBean_S0 as s0 " +
                    "merge the_table as tt " +
                    "where s0.P00 = tt.key " +
                    "when matched and the_table[s0.P00].total > 0" +
                    "  then update set value = 1",
                    path);
                env.CompileDeploy("@Name('s0') select the_table[P10].Value as c0 from SupportBean_S1", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", -1));
                env.SendEventBean(new SupportBean_S0(0, "E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0, "E1"));
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class InfraTableAccessCoreSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create table MyTable(k1 string primary key, c1 int);\n" +
                          "insert into MyTable select TheString as k1, IntPrimitive as c1 from SupportBean;\n";
                env.CompileDeploy(epl, path);

                epl = "on SupportBean_S0 " +
                      "  insert into AStream select MyTable['A'].c1 as c0 where Id=1" +
                      "  insert into AStream select MyTable['B'].c1 as c0 where Id=2;\n";
                env.CompileDeploy(epl, path);

                env.CompileDeploy("@Name('out') select * from AStream", path).AddListener("out");

                env.SendEventBean(new SupportBean("A", 10));
                env.SendEventBean(new SupportBean("B", 20));

                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(10, env.Listener("out").AssertOneGetNewAndReset().Get("c0"));

                env.SendEventBean(new SupportBean_S0(2));
                Assert.AreEqual(20, env.Listener("out").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        public class InfraTableAccessCoreUnGroupedWindowAndSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create objectarray schema MyEvent(c0 int)", path);

                env.CompileDeploy(
                    "create table windowAndTotal (" +
                    "thewindow window(*) @type(MyEvent), thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    "into table windowAndTotal " +
                    "select window(*) as thewindow, sum(c0) as thetotal from MyEvent#length(2)",
                    path);

                env.CompileDeploy("@Name('s0') select windowAndTotal as val0 from SupportBean_S0", path)
                    .AddListener("s0");

                object[] e1 = {10};
                env.SendEventObjectArray(e1, "MyEvent");

                env.Milestone(0);

                var fieldsInner = "thewindow,thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e1},
                    10);

                env.Milestone(1);

                object[] e2 = {20};
                env.SendEventObjectArray(e2, "MyEvent");

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e1, e2},
                    30);

                env.Milestone(3);

                object[] e3 = {30};
                env.SendEventObjectArray(e3, "MyEvent");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    new[] {e2, e3},
                    50);

                env.UndeployAll();
            }
        }

        public class AggSubBean
        {
            public int Thetotal { get; private set; }

            public object[][] Thewindow { get; private set; }

            public void SetThetotal(int thetotal)
            {
                Thetotal = thetotal;
            }

            public void SetThewindow(object[][] thewindow)
            {
                Thewindow = thewindow;
            }
        }

        public class AggBean
        {
            public AggSubBean Val0 { get; private set; }

            public void SetVal0(AggSubBean val0)
            {
                Val0 = val0;
            }
        }
    }
} // end of namespace