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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableAccessCore
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithTableAccessCoreUnGroupedWindowAndSum(execs);
            WithIntegerIndexedPropertyLookAlike(execs);
            WithFilterBehavior(execs);
            WithExprSelectClauseRenderingUnnamedCol(execs);
            WithTopLevelReadGrouped2Keys(execs);
            WithTopLevelReadUnGrouped(execs);
            WithExpressionAliasAndDecl(execs);
            WithGroupedTwoKeyNoContext(execs);
            WithGroupedThreeKeyNoContext(execs);
            WithGroupedSingleKeyNoContext(execs);
            WithUngroupedWContext(execs);
            WithOrderOfAggregationsAndPush(execs);
            WithMultiStmtContributing(execs);
            WithGroupedMixedMethodAndAccess(execs);
            WithNamedWindowAndFireAndForget(execs);
            WithSubquery(execs);
            WithOnMergeExpressions(execs);
            WithTableAccessCoreSplitStream(execs);
            WithTableAccessMultikeyWArrayOneArrayKey(execs);
            WithTableAccessMultikeyWArrayTwoArrayKey(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessMultikeyWArrayTwoArrayKey(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessMultikeyWArrayTwoArrayKey());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessMultikeyWArrayOneArrayKey(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessMultikeyWArrayOneArrayKey());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessCoreSplitStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessCoreSplitStream());
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergeExpressions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergeExpressions());
            return execs;
        }

        public static IList<RegressionExecution> WithSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowAndFireAndForget(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowAndFireAndForget());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedMixedMethodAndAccess(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraGroupedMixedMethodAndAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiStmtContributing(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMultiStmtContributing());
            return execs;
        }

        public static IList<RegressionExecution> WithOrderOfAggregationsAndPush(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOrderOfAggregationsAndPush());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUngroupedWContext());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedSingleKeyNoContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraGroupedSingleKeyNoContext());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedThreeKeyNoContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraGroupedThreeKeyNoContext());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedTwoKeyNoContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraGroupedTwoKeyNoContext());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionAliasAndDecl(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExpressionAliasAndDecl());
            return execs;
        }

        public static IList<RegressionExecution> WithTopLevelReadUnGrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTopLevelReadUnGrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithTopLevelReadGrouped2Keys(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTopLevelReadGrouped2Keys());
            return execs;
        }

        public static IList<RegressionExecution> WithExprSelectClauseRenderingUnnamedCol(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExprSelectClauseRenderingUnnamedCol());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterBehavior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFilterBehavior());
            return execs;
        }

        public static IList<RegressionExecution> WithIntegerIndexedPropertyLookAlike(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraIntegerIndexedPropertyLookAlike());
            return execs;
        }

        public static IList<RegressionExecution> WithTableAccessCoreUnGroupedWindowAndSum(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableAccessCoreUnGroupedWindowAndSum());
            return execs;
        }

        internal class InfraTableAccessMultikeyWArrayTwoArrayKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTable(k1 int[primitive] primary key, k2 int[primitive] primary key, value int);\n" +
                    "insert into MyTable select IntOne as k1, IntTwo as k2, Value as value from SupportEventWithManyArray(Id = 'I');\n" +
                    "@name('s0') select MyTable[IntOne, IntTwo].value as c0 from SupportEventWithManyArray(Id = 'Q');\n" +
                    "@name('s1') select MyTable.keys() as keys from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

                SendManyArrayI(env, new int[] { 1, 2 }, new int[] { 1, 2 }, 10);
                SendManyArrayI(env, new int[] { 1, 3 }, new int[] { 1, 1 }, 20);
                SendManyArrayI(env, new int[] { 1, 2 }, new int[] { 1, 1 }, 30);

                env.Milestone(0);

                SendManyArrayQAssert(env, new int[] { 1, 2 }, new int[] { 1, 2 }, 10);
                SendManyArrayQAssert(env, new int[] { 1, 2 }, new int[] { 1, 1 }, 30);
                SendManyArrayQAssert(env, new int[] { 1, 3 }, new int[] { 1, 1 }, 20);
                SendManyArrayQAssert(env, new int[] { 1, 2 }, new int[] { 1, 2, 2 }, null);

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s1",
                    @event => {
                        var keys = (object[])@event.Get("keys");
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            keys,
                            new object[] {
                                new object[] { new int[] { 1, 2 }, new int[] { 1, 2 } },
                                new object[] { new int[] { 1, 3 }, new int[] { 1, 1 } },
                                new object[] { new int[] { 1, 2 }, new int[] { 1, 1 } },
                            });
                    });

                env.UndeployAll();
            }

            private void SendManyArrayQAssert(
                RegressionEnvironment env,
                int[] arrayOne,
                int[] arrayTwo,
                int? expected)
            {
                env.SendEventBean(new SupportEventWithManyArray("Q").WithIntOne(arrayOne).WithIntTwo(arrayTwo));
                env.AssertEqualsNew("s0", "c0", expected);
            }

            private void SendManyArrayI(
                RegressionEnvironment env,
                int[] arrayOne,
                int[] arrayTwo,
                int value)
            {
                env.SendEventBean(
                    new SupportEventWithManyArray("I").WithIntOne(arrayOne).WithIntTwo(arrayTwo).WithValue(value));
            }
        }

        internal class InfraTableAccessMultikeyWArrayOneArrayKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create table MyTable(k int[primitive] primary key, value int);\n" +
                          "insert into MyTable select IntOne as k, Value as value from SupportEventWithManyArray(Id = 'I');\n" +
                          "@name('s0') select MyTable[IntOne].value as c0 from SupportEventWithManyArray(Id = 'Q');\n" +
                          "@name('s1') select MyTable.keys() as keys from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

                SendManyArrayI(env, new int[] { 1, 2 }, 10);
                SendManyArrayI(env, new int[] { 2, 1 }, 20);
                SendManyArrayI(env, new int[] { 1, 2, 1 }, 30);

                env.Milestone(0);

                SendManyArrayQAssert(env, new int[] { 1, 2 }, 10);
                SendManyArrayQAssert(env, new int[] { 1, 2, 1 }, 30);
                SendManyArrayQAssert(env, new int[] { 2, 1 }, 20);
                SendManyArrayQAssert(env, new int[] { 1, 2, 2 }, null);

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s1",
                    @event => {
                        var keys = @event.Get("keys").UnwrapIntoArray<object>();
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            keys,
                            new object[] { new int[] { 2, 1 }, new int[] { 1, 2 }, new int[] { 1, 2, 1 } });
                    });

                env.UndeployAll();
            }

            private void SendManyArrayQAssert(
                RegressionEnvironment env,
                int[] arrayOne,
                int? expected)
            {
                env.SendEventBean(new SupportEventWithManyArray("Q").WithIntOne(arrayOne));
                env.AssertEqualsNew("s0", "c0", expected);
            }

            private void SendManyArrayI(
                RegressionEnvironment env,
                int[] arrayOne,
                int value)
            {
                env.SendEventBean(new SupportEventWithManyArray("I").WithIntOne(arrayOne).WithValue(value));
            }
        }

        internal class InfraIntegerIndexedPropertyLookAlike : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionIntegerIndexedPropertyLookAlike(env, false, milestone);
                TryAssertionIntegerIndexedPropertyLookAlike(env, true, milestone);
            }

            private static void TryAssertionIntegerIndexedPropertyLookAlike(
                RegressionEnvironment env,
                bool soda,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                var eplDeclare =
                    "@name('infra') @public create table varaggIIP (key int primary key, myevents window(*) @type('SupportBean'))";
                env.CompileDeploy(soda, eplDeclare, path);
                env.AssertStatement(
                    "infra",
                    statement => {
                        Assert.AreEqual(
                            StatementType.CREATE_TABLE,
                            statement.GetProperty(StatementProperty.STATEMENTTYPE));
                        Assert.AreEqual("varaggIIP", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
                    });

                var eplInto =
                    "into table varaggIIP select window(*) as myevents from SupportBean#length(3) group by IntPrimitive";
                env.CompileDeploy(soda, eplInto, path);

                var eplSelect =
                    "@name('s0') select varaggIIP[1] as c0, varaggIIP[1].myevents as c1, varaggIIP[1].myevents.last(*) as c2, varaggIIP[1].myevents.last(*,1) as c3 from SupportBean_S0";
                env.CompileDeploy(soda, eplSelect, path).AddListener("s0");

                var e1 = MakeSendBean(env, "E1", 1, 10L);
                var e2 = MakeSendBean(env, "E2", 1, 20L);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEventNew("s0", @event => AssertIntegerIndexed(@event, new SupportBean[] { e1, e2 }));

                env.UndeployAll();
            }
        }

        internal class InfraFilterBehavior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table varaggFB (total count(*))", path);
                env.CompileDeploy("into table varaggFB select count(*) as total from SupportBean_S0", path);
                env.CompileDeploy("@name('s0') select * from SupportBean(varaggFB.total = IntPrimitive)", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(0));

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean_S0(0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraExprSelectClauseRenderingUnnamedCol : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table varaggESC (" +
                    "key string primary key, theEvents window(*) @type(SupportBean))",
                    path);

                env.CompileDeploy(
                    "@name('s0') select " +
                    "varaggESC.Keys()," +
                    "varaggESC[P00].theEvents," +
                    "varaggESC[P00]," +
                    "varaggESC[P00].theEvents.last(*)," +
                    "varaggESC[P00].theEvents.window(*).take(1) from SupportBean_S0",
                    path);

                var expectedAggType = new object[][] {
                    new object[] { "varaggESC.Keys()", typeof(object[]) },
                    new object[] { "varaggESC[P00].theEvents", typeof(SupportBean[]) },
                    new object[] { "varaggESC[P00]", typeof(IDictionary<string, object>) },
                    new object[] { "varaggESC[P00].theEvents.last(*)", typeof(SupportBean) },
                    new object[] { "varaggESC[P00].theEvents.window(*).take(1)", typeof(ICollection<SupportBean>) },
                };
                env.AssertStatement(
                    "s0",
                    statement => {
                        var eventType = statement.EventType;
                        SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                            expectedAggType,
                            eventType,
                            SupportEventTypeAssertionEnum.NAME,
                            SupportEventTypeAssertionEnum.TYPE);
                    });
                env.UndeployAll();
            }
        }

        internal class InfraTopLevelReadGrouped2Keys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionTopLevelReadGrouped2Keys(env, false, milestone);
                TryAssertionTopLevelReadGrouped2Keys(env, true, milestone);
            }

            private static void TryAssertionTopLevelReadGrouped2Keys(
                RegressionEnvironment env,
                bool soda,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                var typeCompiled = env.Compile(
                    "@buseventtype @public create objectarray schema MyEventOA as (c0 int, c1 string, c2 int)");
                env.Deploy(typeCompiled);
                path.Add(typeCompiled);

                env.CompileDeploy(
                    soda,
                    "@public create table windowAndTotalTLP2K (" +
                    "keyi int primary key, keys string primary key, Thewindow window(*) @type('MyEventOA'), Thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    soda,
                    "into table windowAndTotalTLP2K " +
                    "select window(*) as Thewindow, sum(c2) as Thetotal from MyEventOA#length(2) group by c0, c1",
                    path);

                env.CompileDeploy(
                        soda,
                        "@name('s0') select windowAndTotalTLP2K[Id,P00] as val0 from SupportBean_S0",
                        path)
                    .AddListener("s0");
                env.AssertStatement("s0", statement => AssertTopLevelTypeInfo(statement));

                var e1 = new object[] { 10, "G1", 100 };
                env.SendEventObjectArray(e1, "MyEventOA");

                var fieldsInner = "Thewindow,Thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(10, "G1"));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e1 },
                        100));

                var e2 = new object[] { 20, "G2", 200 };
                env.SendEventObjectArray(e2, "MyEventOA");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e2 },
                        200));

                var e3 = new object[] { 20, "G2", 300 };
                env.SendEventObjectArray(e3, "MyEventOA");

                env.SendEventBean(new SupportBean_S0(10, "G1"));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        null,
                        null));
                env.SendEventBean(new SupportBean_S0(20, "G2"));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e2, e3 },
                        500));

                // test typable output
                env.UndeployModuleContaining("s0");
                env.CompileDeploy(
                    "@name('i1') insert into OutStream select windowAndTotalTLP2K[20, 'G2'] as val0 from SupportBean_S0",
                    path);
                env.AddListener("i1");

                env.SendEventBean(new SupportBean_S0(0));
                env.AssertPropsNew(
                    "i1",
                    "val0.Thewindow,val0.Thetotal".SplitCsv(),
                    new object[] { new object[][] { e2, e3 }, 500 });

                env.UndeployAll();
            }
        }

        internal class InfraTopLevelReadUnGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var e1 = new object[] { 10 };
                var e2 = new object[] { 20 };
                var e3 = new object[] { 30 };

                var path = new RegressionPath();
                var typeCompiled = env.Compile("@public @buseventtype create objectarray schema MyEventOATLRU(c0 int)");
                env.Deploy(typeCompiled);
                path.Add(typeCompiled);

                env.CompileDeploy(
                    "@public create table windowAndTotalTLRUG (" +
                    "Thewindow window(*) @type(MyEventOATLRU), Thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    "into table windowAndTotalTLRUG " +
                    "select window(*) as Thewindow, sum(c0) as Thetotal from MyEventOATLRU#length(2)",
                    path);

                env.CompileDeploy("@name('s0') select windowAndTotalTLRUG as val0 from SupportBean_S0", path);
                env.AddListener("s0");

                env.SendEventObjectArray(e1, "MyEventOATLRU");

                var fieldsInner = "Thewindow,Thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e1 },
                        10));

                env.SendEventObjectArray(e2, "MyEventOATLRU");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e1, e2 },
                        30));

                env.SendEventObjectArray(e3, "MyEventOATLRU");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e2, e3 },
                        50));

                // test typable output
                env.UndeployModuleContaining("s0");

                env.CompileDeploy(
                        string.Format(
                            "create schema AggBean as {0};\n" +
                            "@Name('s0') insert into AggBean select windowAndTotalTLRUG as Val0 from SupportBean_S0;\n",
                            TypeHelper.MaskTypeName<AggBean>()),
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertPropsNew(
                    "s0",
                    "Val0.Thewindow,Val0.Thetotal".SplitCsv(),
                    new object[] { new object[][] { e2, e3 }, 50 });

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
                env.CompileDeploy(
                    "@public create table MyTableTwo(TheString string primary key, IntPrimitive int)",
                    path);
                env.CompileDeploy(
                    "@public create expression getMyValue{o => (select MyTableTwo[o.P00].IntPrimitive from SupportBean_S1#lastevent)}",
                    path);
                env.CompileDeploy("insert into MyTableTwo select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S1(1000));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { 2 });

                env.UndeployAll();
            }

            private static void TryAssertionExpressionHasTableAccess(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table MyTableOne(TheString string primary key, IntPrimitive int)",
                    path);
                env.CompileDeploy("@public create expression getMyValue{o => MyTableOne[o.P00].IntPrimitive}", path);
                env.CompileDeploy("insert into MyTableOne select TheString, IntPrimitive from SupportBean", path);
                env.CompileDeploy("@name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { 2 });

                env.UndeployAll();
            }

            private static void TryAssertionIntoTableFromExpression(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create expression sumi {a -> sum(IntPrimitive)}", path);
                env.CompileDeploy("@public create expression sumd alias for {sum(DoublePrimitive)}", path);
                env.CompileDeploy(
                    "@public create table varaggITFE (" +
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
                env.CompileDeploy("@name('s0') select " + fields + " from SupportBean_S0", path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", fields.SplitCsv(), new object[] { 10, 1000d, 10000f, 100L });

                env.MilestoneInc(milestone);

                MakeSendBean(env, "E1", 11, 101L, 1001d, 10001f);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertPropsNew("s0", fields.SplitCsv(), new object[] { 21, 2001d, 20001f, 201L });

                env.UndeployAll();
            }
        }

        internal class InfraGroupedTwoKeyNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create table varTotalG2K (key0 string primary key, key1 int primary key, Total sum(long), cnt count(*))";
                env.CompileDeploy(eplDeclare, path);

                var eplBind = "into table varTotalG2K " +
                              "select sum(LongPrimitive) as Total, count(*) as cnt " +
                              "from SupportBean group by TheString, IntPrimitive";
                env.CompileDeploy(eplBind, path);

                var eplUse = "@name('s0') select varTotalG2K[P00, Id].Total as c0, varTotalG2K[P00, Id].cnt as c1 from SupportBean_S0";
                env.CompileDeploy(eplUse, path).AddListener("s0");

                MakeSendBean(env, "E1", 10, 100);

                var fields = "c0,c1".SplitCsv();
                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 100L, 1L });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null });
                env.SendEventBean(new SupportBean_S0(10, "E2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null });

                env.UndeployAll();
            }
        }

        internal class InfraGroupedThreeKeyNoContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create table varTotalG3K (key0 string primary key, key1 int primary key," +
                                 "key2 long primary key, Total sum(double), cnt count(*))";
                env.CompileDeploy(eplDeclare, path);

                var eplBind = "into table varTotalG3K " +
                              "select sum(DoublePrimitive) as Total, count(*) as cnt " +
                              "from SupportBean group by TheString, IntPrimitive, LongPrimitive";
                env.CompileDeploy(eplBind, path);

                var fields = "c0,c1".SplitCsv();
                var eplUse =
                    "@name('s0') select varTotalG3K[P00, Id, 100L].Total as c0, varTotalG3K[P00, Id, 100L].cnt as c1 from SupportBean_S0";
                env.CompileDeploy(eplUse, path).AddListener("s0");

                MakeSendBean(env, "E1", 10, 100, 1000);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 1000.0, 1L });

                env.Milestone(1);

                MakeSendBean(env, "E1", 10, 100, 1001);

                env.SendEventBean(new SupportBean_S0(10, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { 2001.0, 2L });

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
                var eplDeclare = "@public create table varTotalG1K (key string primary key, Total sum(int))";
                env.CompileDeploy(soda, eplDeclare, path);

                var eplBind = "into table varTotalG1K " +
                              "select TheString, sum(IntPrimitive) as Total from SupportBean group by TheString";
                env.CompileDeploy(soda, eplBind, path);

                var eplUse = "@name('s0') select P00 as c0, varTotalG1K[P00].Total as c1 from SupportBean_S0";
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
                    "context PartitionedByString create table varTotalUG (Total sum(int));\n" +
                    "context PartitionedByString into table varTotalUG select sum(IntPrimitive) as Total from SupportBean;\n" +
                    "@name('s0') context PartitionedByString select P00 as c0, varTotalUG.Total as c1 from SupportBean_S0;\n";
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
                env.CompileDeploy("@public create table sharedagg (Total sum(int))", path);
                env.CompileDeploy(
                        "@name('i1') into table sharedagg " +
                        "select P00 as c0, sum(Id) as Total from SupportBean_S0",
                        path)
                    .AddListener("i1");
                env.CompileDeploy(
                        "@name('i2') into table sharedagg " +
                        "select P10 as c0, sum(Id) as Total from SupportBean_S1",
                        path)
                    .AddListener("i2");
                env.CompileDeploy("@name('s0') select TheString as c0, sharedagg.Total as Total from SupportBean", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "A"));
                AssertMultiStmtContributingTotal(env, "i1", "A", 10);

                env.SendEventBean(new SupportBean_S1(-5, "B"));
                AssertMultiStmtContributingTotal(env, "i2", "B", 5);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(2, "C"));
                AssertMultiStmtContributingTotal(env, "i1", "C", 7);

                env.UndeployAll();
            }

            private static void AssertMultiStmtContributingTotal(
                RegressionEnvironment env,
                string stmtName,
                string c0,
                int total)
            {
                var fields = "c0,Total".SplitCsv();
                env.AssertEventNew(
                    stmtName,
                    @event => EPAssertionUtil.AssertProps(@event, fields, new object[] { c0, total }));

                env.SendEventBean(new SupportBean(c0, 0));
                env.AssertPropsNew("s0", fields, new object[] { c0, total });
            }

            private static void TryAssertionMultiStmtContributingDifferentAggs(
                RegressionEnvironment env,
                bool grouped,
                AtomicLong milestone)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create table varaggMSC (" +
                                 (grouped ? "key string primary key," : "") +
                                 "s0sum sum(int), s0cnt count(*), s0win window(*) @type(SupportBean_S0)," +
                                 "s1sum sum(int), s1cnt count(*), s1win window(*) @type(SupportBean_S1)" +
                                 ")";
                env.CompileDeploy(eplDeclare, path);

                var fieldsSelect = "c0,c1,c2,c3,c4,c5".SplitCsv();
                var eplSelectUngrouped = "@name('s0') select varaggMSC.s0sum as c0, varaggMSC.s0cnt as c1," +
                                         "varaggMSC.s0win as c2, varaggMSC.s1sum as c3, varaggMSC.s1cnt as c4," +
                                         "varaggMSC.s1win as c5 from SupportBean";
                var eplSelectGrouped =
                    "@name('s0') select varaggMSC[TheString].s0sum as c0, varaggMSC[TheString].s0cnt as c1," +
                    "varaggMSC[TheString].s0win as c2, varaggMSC[TheString].s1sum as c3, varaggMSC[TheString].s1cnt as c4," +
                    "varaggMSC[TheString].s1win as c5 from SupportBean";
                env.CompileDeploy(grouped ? eplSelectGrouped : eplSelectUngrouped, path).AddListener("s0");

                var fieldsOne = "s0sum,s0cnt,s0win".SplitCsv();
                var eplBindOne =
                    "@name('s1') into table varaggMSC select sum(Id) as s0sum, count(*) as s0cnt, window(*) as s0win from SupportBean_S0#length(2) " +
                    (grouped ? "group by P00" : "");
                env.CompileDeploy(eplBindOne, path).AddListener("s1");

                var fieldsTwo = "s1sum,s1cnt,s1win".SplitCsv();
                var eplBindTwo =
                    "@name('s2') into table varaggMSC select sum(Id) as s1sum, count(*) as s1cnt, window(*) as s1win from SupportBean_S1#length(2) " +
                    (grouped ? "group by P10" : "");
                env.CompileDeploy(eplBindTwo, path).AddListener("s2");

                // contribute S1
                var s1Bean1 = MakeSendS1(env, 10, "G1");
                env.AssertPropsNew("s2", fieldsTwo, new object[] { 10, 1L, new object[] { s1Bean1 } });

                env.SendEventBean(new SupportBean("G1", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsSelect,
                    new object[] { null, 0L, null, 10, 1L, new object[] { s1Bean1 } });

                env.MilestoneInc(milestone);

                // contribute S0
                var s0Bean1 = MakeSendS0(env, 20, "G1");
                env.AssertPropsNew("s1", fieldsOne, new object[] { 20, 1L, new object[] { s0Bean1 } });

                env.SendEventBean(new SupportBean("G1", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsSelect,
                    new object[] { 20, 1L, new object[] { s0Bean1 }, 10, 1L, new object[] { s1Bean1 } });

                // contribute S1 and S0
                var s1Bean2 = MakeSendS1(env, 11, "G1");
                env.AssertPropsNew("s2", fieldsTwo, new object[] { 21, 2L, new object[] { s1Bean1, s1Bean2 } });
                var s0Bean2 = MakeSendS0(env, 21, "G1");
                env.AssertPropsNew("s1", fieldsOne, new object[] { 41, 2L, new object[] { s0Bean1, s0Bean2 } });

                env.SendEventBean(new SupportBean("G1", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsSelect,
                    new object[]
                        { 41, 2L, new object[] { s0Bean1, s0Bean2 }, 21, 2L, new object[] { s1Bean1, s1Bean2 } });

                env.MilestoneInc(milestone);

                // contribute S1 and S0 (leave)
                var s1Bean3 = MakeSendS1(env, 12, "G1");
                env.AssertPropsNew("s2", fieldsTwo, new object[] { 23, 2L, new object[] { s1Bean2, s1Bean3 } });
                var s0Bean3 = MakeSendS0(env, 22, "G1");
                env.AssertPropsNew("s1", fieldsOne, new object[] { 43, 2L, new object[] { s0Bean2, s0Bean3 } });

                env.SendEventBean(new SupportBean("G1", 0));
                env.AssertPropsNew(
                    "s0",
                    fieldsSelect,
                    new object[]
                        { 43, 2L, new object[] { s0Bean2, s0Bean3 }, 23, 2L, new object[] { s1Bean2, s1Bean3 } });

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
                var epl = "@public create window MyWindow#length(2) as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@public create table varaggNWFAF (Total sum(int));\n" +
                          "into table varaggNWFAF select sum(IntPrimitive) as Total from MyWindow;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean("E1", 10));
                var resultSelect = env.CompileExecuteFAF("select varaggNWFAF.Total as c0 from MyWindow", path);
                Assert.AreEqual(10, resultSelect.Array[0].Get("c0"));

                var resultDelete = env.CompileExecuteFAF(
                    "delete from MyWindow where varaggNWFAF.Total = IntPrimitive",
                    path);
                Assert.AreEqual(1, resultDelete.Array.Length);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                var resultUpdate = env.CompileExecuteFAF(
                    "update MyWindow set DoublePrimitive = 100 where varaggNWFAF.Total = IntPrimitive",
                    path);
                Assert.AreEqual(100d, resultUpdate.Array[0].Get("DoublePrimitive"));

                var resultInsert = env.CompileExecuteFAF(
                    "insert into MyWindow (TheString, IntPrimitive) values ('A', varaggNWFAF.Total)",
                    path);
                EPAssertionUtil.AssertProps(
                    resultInsert.Array[0],
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[] { "A", 20 });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        internal class InfraSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table subquery_var_agg (key string primary key, Total count(*))",
                    path);
                env.CompileDeploy(
                        "@name('s0') select (select subquery_var_agg[P00].Total from SupportBean_S0#lastevent) as c0 " +
                        "from SupportBean_S1",
                        path)
                    .AddListener("s0");
                env.CompileDeploy(
                    "into table subquery_var_agg select count(*) as Total from SupportBean group by TheString",
                    path);

                env.SendEventBean(new SupportBean("E1", -1));
                env.SendEventBean(new SupportBean_S0(0, "E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(1));
                env.AssertEqualsNew("s0", "c0", 1L);

                env.SendEventBean(new SupportBean("E1", -1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(2));
                env.AssertEqualsNew("s0", "c0", 2L);

                env.UndeployAll();
            }
        }

        internal class InfraOnMergeExpressions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table the_table (key string primary key, Total count(*), value int)",
                    path);
                env.CompileDeploy(
                    "into table the_table select count(*) as Total from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                    "on SupportBean_S0 as s0 " +
                    "merge the_table as tt " +
                    "where s0.P00 = tt.key " +
                    "when matched and the_table[s0.P00].Total > 0" +
                    "  then update set value = 1",
                    path);
                env.CompileDeploy("@name('s0') select the_table[P10].value as c0 from SupportBean_S1", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", -1));
                env.SendEventBean(new SupportBean_S0(0, "E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0, "E1"));
                env.AssertEqualsNew("s0", "c0", 1);

                env.UndeployAll();
            }
        }

        internal class InfraTableAccessCoreSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create table MyTable(k1 string primary key, c1 int);\n" +
                          "insert into MyTable select TheString as k1, IntPrimitive as c1 from SupportBean;\n";
                env.CompileDeploy(epl, path);

                epl = "@public on SupportBean_S0 " +
                      "  insert into AStream select MyTable['A'].c1 as c0 where Id=1" +
                      "  insert into AStream select MyTable['B'].c1 as c0 where Id=2;\n";
                env.CompileDeploy(epl, path);

                env.CompileDeploy("@name('out') select * from AStream", path).AddListener("out");

                env.SendEventBean(new SupportBean("A", 10));
                env.SendEventBean(new SupportBean("B", 20));

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("out", "c0", 10);

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEqualsNew("out", "c0", 20);

                env.UndeployAll();
            }
        }

        internal class InfraTableAccessCoreUnGroupedWindowAndSum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public @buseventtype create objectarray schema MyEvent(c0 int)", path);

                env.CompileDeploy(
                    "@public create table windowAndTotal (" +
                    "Thewindow window(*) @type(MyEvent), Thetotal sum(int))",
                    path);
                env.CompileDeploy(
                    "into table windowAndTotal " +
                    "select window(*) as Thewindow, sum(c0) as Thetotal from MyEvent#length(2)",
                    path);

                env.CompileDeploy("@name('s0') select windowAndTotal as val0 from SupportBean_S0", path)
                    .AddListener("s0");

                var e1 = new object[] { 10 };
                env.SendEventObjectArray(e1, "MyEvent");

                env.Milestone(0);

                var fieldsInner = "Thewindow,Thetotal".SplitCsv();
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e1 },
                        10));

                env.Milestone(1);

                var e2 = new object[] { 20 };
                env.SendEventObjectArray(e2, "MyEvent");

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e1, e2 },
                        30));

                env.Milestone(3);

                var e3 = new object[] { 30 };
                env.SendEventObjectArray(e3, "MyEvent");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(2));
                env.AssertEventNew(
                    "s0",
                    @event => EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)@event.Get("val0"),
                        fieldsInner,
                        new object[][] { e2, e3 },
                        50));

                env.UndeployAll();
            }
        }

        private static void TryAssertionGroupedMixedMethodAndAccess(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "@public create table varMyAgg (" +
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

            var eplSelect = "@name('s0') select " +
                            "varMyAgg[P00].c0 as c0, " +
                            "varMyAgg[P00].c1 as c1, " +
                            "varMyAgg[P00].c2 as c2, " +
                            "varMyAgg[P00].c3 as c3" +
                            " from SupportBean_S0";
            env.CompileDeploy(soda, eplSelect, path).AddListener("s0");
            var fields = "c0,c1,c2,c3".SplitCsv();

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c0"));
                    Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c1"));
                    Assert.AreEqual(typeof(SupportBean[]), eventType.GetPropertyType("c2"));
                    Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c3"));
                });

            var b1 = MakeSendBean(env, "E1", 10, 100);
            var b2 = MakeSendBean(env, "E1", 11, 101);
            var b3 = MakeSendBean(env, "E1", 10, 102);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { 3L, 2L, new SupportBean[] { b1, b2, b3 }, 303L });

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { null, null, null, null });

            var b4 = MakeSendBean(env, "E2", 20, 200);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { 1L, 1L, new SupportBean[] { b4 }, 200L });

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
            var expected = new int[] { 21, 41, 30, 40 };
            var count = 0;
            foreach (var p00 in "A,B,C,D".SplitCsv()) {
                env.SendEventBean(new SupportBean_S0(0, p00));
                env.AssertPropsNew("s0", fields, new object[] { p00, expected[count] });
                count++;
            }

            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.AssertPropsNew("s0", fields, new object[] { "A", 21 });
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
            env.AssertPropsNew("s0", fields, new object[] { p00, total });
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
            Assert.AreEqual(typeof(object[][]), fragType.FragmentType.GetPropertyType("Thewindow"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("Thetotal"));
        }

        private static void AssertIntegerIndexed(
            EventBean @event,
            SupportBean[] events)
        {
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[])@event.Get("c0.myevents"));
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[])@event.Get("c1"));
            Assert.AreEqual(events[^1], @event.Get("c2"));
            Assert.AreEqual(events[^2], @event.Get("c3"));
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
            var eplDeclare = "@public create table varaggOOA (" +
                             (ungrouped ? "" : "key string primary key, ") +
                             "sumint sum(int), " +
                             "sumlong sum(long), " +
                             "mysort sorted(IntPrimitive) @type(SupportBean)," +
                             "mywindow window(*) @type(SupportBean)" +
                             ")";
            env.CompileDeploy(eplDeclare, path);

            var fieldsTable = "sumint,sumlong,mywindow,mysort".SplitCsv();
            var eplSelect = "@name('into') into table varaggOOA select " +
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
                    "@name('s0') select " +
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
            env.AssertPropsNew(
                "into",
                fieldsTable,
                new object[] { 10, 100L, new object[] { e1 }, new object[] { e1 } });

            env.MilestoneInc(milestone);

            var e2 = MakeSendBean(env, "E1", 5, 50);
            env.AssertPropsNew(
                "into",
                fieldsTable,
                new object[] { 15, 150L, new object[] { e1, e2 }, new object[] { e2, e1 } });

            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew(
                "s0",
                fieldsSelect,
                new object[] { 15, 150L, new object[] { e1, e2 }, new object[] { e2, e1 } });

            env.MilestoneInc(milestone);

            var e3 = MakeSendBean(env, "E1", 12, 120);
            env.AssertPropsNew(
                "into",
                fieldsTable,
                new object[] { 17, 170L, new object[] { e2, e3 }, new object[] { e2, e3 } });

            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew(
                "s0",
                fieldsSelect,
                new object[] { 17, 170L, new object[] { e2, e3 }, new object[] { e2, e3 } });

            env.UndeployAll();
        }

        public class AggSubBean
        {
            private int thetotal;
            private object[][] thewindow;

            public int Thetotal {
                get => thetotal;
                set => thetotal = value;
            }

            public object[][] Thewindow {
                get => thewindow;
                set => thewindow = value;
            }
        }

        public class AggBean
        {
            private AggSubBean val0;

            public AggSubBean Val0 {
                get => val0;
                set => val0 = value;
            }
        }
    }
} // end of namespace