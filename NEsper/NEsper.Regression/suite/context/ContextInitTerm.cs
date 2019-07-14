///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextInitTerm
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextInitTermNoTerminationCondition());
            execs.Add(new ContextStartEndNoTerminationCondition());
            execs.Add(new ContextStartEndAfterZeroInitiatedNow());
            execs.Add(new ContextStartEndEndSameEventAsAnalyzed());
            execs.Add(new ContextInitTermContextPartitionSelection());
            execs.Add(new ContextInitTermFilterInitiatedFilterAllTerminated());
            execs.Add(new ContextInitTermFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot());
            execs.Add(new ContextInitTermFilterAndAfter1Min());
            execs.Add(new ContextInitTermFilterAndPattern());
            execs.Add(new ContextInitTermPatternAndAfter1Min());
            execs.Add(new ContextInitTermScheduleFilterResources());
            execs.Add(new ContextInitTermPatternIntervalZeroInitiatedNow());
            execs.Add(new ContextInitTermPatternInclusion());
            execs.Add(new ContextInitTermPatternInitiatedStraightSelect());
            execs.Add(new ContextInitTermFilterInitiatedStraightEquals());
            execs.Add(new ContextInitTermFilterAllOperators());
            execs.Add(new ContextInitTermFilterBooleanOperator());
            execs.Add(new ContextInitTermTerminateTwoContextSameTime());
            execs.Add(new ContextInitTermOutputSnapshotWhenTerminated());
            execs.Add(new ContextInitTermOutputAllEvery2AndTerminated());
            execs.Add(new ContextInitTermOutputWhenExprWhenTerminatedCondition());
            execs.Add(new ContextInitTermOutputOnlyWhenTerminatedCondition());
            execs.Add(new ContextInitTermOutputOnlyWhenSetAndWhenTerminatedSet());
            execs.Add(new ContextInitTermOutputOnlyWhenTerminatedThenSet());
            execs.Add(new ContextInitTermCrontab());
            execs.Add(new ContextStartEndStartNowCalMonthScoped());
            execs.Add(new ContextInitTermAggregationGrouped());
            execs.Add(new ContextInitTermPrevPrior());
            return execs;
        }

        private static void TryAssertionNoTerminationConditionOverlapping(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool soda)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                soda,
                "@Name('ctx') create context SupportBeanInstanceCtx as initiated by SupportBean as sb",
                path);
            env.CompileDeploy(
                soda,
                "@Name('s0') context SupportBeanInstanceCtx " +
                "select id, context.sb.IntPrimitive as sbint, context.startTime as starttime, context.endTime as endtime from SupportBean_S0(p00=context.sb.TheString)",
                path);
            env.AddListener("s0");
            var fields = "id,sbint,starttime,endtime".SplitCsv();
            Assert.AreEqual(
                StatementType.CREATE_CONTEXT,
                env.Statement("ctx").GetProperty(StatementProperty.STATEMENTTYPE));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("P1", 100));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("P2", 200));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10, "P2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {10, 200, 5L, null});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(20, "P1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {20, 100, 5L, null});

            env.UndeployAll();
        }

        private static void TryAssertionNoTerminationConditionNonoverlapping(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool soda)
        {
            var path = new RegressionPath();
            env.CompileDeploy(soda, "create context SupportBeanInstanceCtx as start SupportBean as sb", path);
            env.CompileDeploy(
                soda,
                "@Name('s0') context SupportBeanInstanceCtx " +
                "select id, context.sb.IntPrimitive as sbint, context.startTime as starttime, context.endTime as endtime from SupportBean_S0(p00=context.sb.TheString)",
                path);
            env.AddListener("s0");
            var fields = "id,sbint,starttime,endtime".SplitCsv();

            env.SendEventBean(new SupportBean("P1", 100));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("P2", 200));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10, "P2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(20, "P1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {20, 100, 5L, null});

            env.UndeployAll();
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
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

        private static void AssertSODA(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            env.EplToModelCompileDeploy(epl, path).UndeployAll();
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        internal class ContextInitTermNoTerminationCondition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                env.AdvanceTime(5);
                TryAssertionNoTerminationConditionOverlapping(env, milestone, false);
                TryAssertionNoTerminationConditionOverlapping(env, milestone, true);
            }
        }

        internal class ContextStartEndNoTerminationCondition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                env.AdvanceTime(5);
                TryAssertionNoTerminationConditionNonoverlapping(env, milestone, false);
                TryAssertionNoTerminationConditionNonoverlapping(env, milestone, true);
            }
        }

        internal class ContextStartEndAfterZeroInitiatedNow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsOne = "c0,c1".SplitCsv();
                var path = new RegressionPath();
                env.AdvanceTime(0);

                // test start-after with immediate start
                var contextExpr = "create context CtxPerId start after 0 sec end after 60 sec";
                env.CompileDeploy(contextExpr, path);
                env.CompileDeploy(
                    "@Name('s0') context CtxPerId select TheString as c0, intPrimitive as c1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"E1", 1});

                env.Milestone(0);

                env.AdvanceTime(59999);
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"E2", 2});

                env.Milestone(1);

                env.AdvanceTime(60000);
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ContextInitTermPatternIntervalZeroInitiatedNow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsOne = "c0,c1".SplitCsv();

                // test initiated-by pattern with immediate start
                env.AdvanceTime(120000);
                var epl =
                    "create context CtxPerId initiated by pattern [timer:interval(0) or every timer:interval(1 min)] terminated after 60 sec;\n" +
                    "@Name('s0') context CtxPerId select TheString as c0, sum(IntPrimitive) as c1 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"E1", 10});

                env.Milestone(0);

                env.AdvanceTime(120000 + 59999);
                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {"E2", 30});

                env.Milestone(1);

                env.AdvanceTime(120000 + 60000);
                env.SendEventBean(new SupportBean("E3", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextInitTermPatternInclusion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,intPrimitive".SplitCsv();
                var path = new RegressionPath();
                env.AdvanceTime(0);

                var contextExpr =
                    "create context CtxPerId initiated by pattern [every-distinct (a.TheString, 10 sec) a=SupportBean]@Inclusive terminated after 10 sec ";
                env.CompileDeploy(contextExpr, path);
                var streamExpr =
                    "@Name('s0') context CtxPerId select * from SupportBean(theString = context.a.TheString) output last when terminated";
                env.CompileDeploy(streamExpr, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E2", 2));

                env.Milestone(1);

                env.AdvanceTime(8000);
                env.SendEventBean(new SupportBean("E1", 3));

                env.Milestone(2);

                env.AdvanceTime(9999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(10000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3});

                env.Milestone(3);

                env.AdvanceTime(10100);
                env.SendEventBean(new SupportBean("E2", 4));
                env.SendEventBean(new SupportBean("E1", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.AdvanceTime(11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 4});

                env.Milestone(5);

                env.AdvanceTime(16100);
                env.SendEventBean(new SupportBean("E2", 6));

                env.AdvanceTime(20099);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.AdvanceTime(20100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 5});

                env.Milestone(7);

                env.AdvanceTime(26100 - 1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(26100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 6});

                env.UndeployAll();
                path.Clear();

                // test multiple pattern with multiple events
                var contextExprMulti =
                    "create context CtxPerId initiated by pattern [every a=SupportBean_S0 => b=SupportBean_S1]@Inclusive terminated after 10 sec ";
                env.CompileDeploy(contextExprMulti, path);
                var streamExprMulti =
                    "@Name('s0') context CtxPerId select * from pattern [every a=SupportBean_S0 => b=SupportBean_S1]";
                env.CompileDeploy(streamExprMulti, path).AddListener("s0");

                env.Milestone(8);

                env.SendEventBean(new SupportBean_S0(10, "S0_1"));

                env.Milestone(9);

                env.SendEventBean(new SupportBean_S1(20, "S1_1"));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class ContextStartEndEndSameEventAsAnalyzed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                string[] fields;
                string epl;

                // same event terminates - not included
                fields = "c1,c2,c3,c4".SplitCsv();
                env.CompileDeploy(
                    "create context MyCtx as " +
                    "start SupportBean " +
                    "end SupportBean(intPrimitive=11)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx " +
                    "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
                    "output snapshot when terminated",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 10, 10, 10d});

                env.Milestone(2);

                env.UndeployAll();
                path.Clear();

                env.Milestone(3);

                // same event terminates - included
                fields = "c1,c2,c3,c4".SplitCsv();
                epl = "create schema MyCtxTerminate(theString string);\n" +
                      "create context MyCtx as start SupportBean end MyCtxTerminate;\n" +
                      "@Name('s0') context MyCtx " +
                      "select min(IntPrimitive) as c1, max(IntPrimitive) as c2, sum(IntPrimitive) as c3, avg(IntPrimitive) as c4 from SupportBean " +
                      "output snapshot when terminated;\n" +
                      "insert into MyCtxTerminate select TheString from SupportBean(intPrimitive=11);\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E1", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 11, 21, 10.5d});

                env.UndeployAll();

                // test with audit
                epl = "@Audit create context AdBreakCtx as initiated by SupportBean(intPrimitive > 0) as ad " +
                      " terminated by SupportBean(theString=ad.TheString, intPrimitive < 0) as endAd";
                env.CompileDeploy(epl, path);
                env.CompileDeploy("context AdBreakCtx select count(*) from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E1", -10));

                env.UndeployAll();
            }
        }

        internal class ContextInitTermContextPartitionSelection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                env.AdvanceTime(0);
                var path = new RegressionPath();
                var milestone = new AtomicLong();

                env.CompileDeploy(
                    "@Name('ctx') create context MyCtx as initiated by SupportBean_S0 s0 terminated by SupportBean_S1(id=s0.id)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx select context.id as c0, context.s0.p00 as c1, theString as c2, sum(IntPrimitive) as c3 from SupportBean#keepall group by TheString",
                    path);

                env.AdvanceTime(1000);
                var initOne = new SupportBean_S0(1, "S0_1");
                env.SendEventBean(initOne);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 10));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 2));

                env.AdvanceTime(2000);
                var initTwo = new SupportBean_S0(2, "S0_2");
                env.SendEventBean(initTwo);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 100));
                env.SendEventBean(new SupportBean("E3", 101));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] {0, "S0_1", "E1", 6},
                        new object[] {0, "S0_1", "E2", 10},
                        new object[] {0, "S0_1", "E3", 201},
                        new object[] {1, "S0_2", "E1", 3},
                        new object[] {1, "S0_2", "E3", 201}
                    });
                SupportContextPropUtil.AssertContextProps(
                    env,
                    "ctx",
                    "MyCtx",
                    new[] {0, 1},
                    "startTime,endTime,s0",
                    new[] {
                        new object[] {1000L, null, initOne},
                        new object[] {2000L, null, initTwo}
                    });

                env.MilestoneInc(milestone);

                // test iterator targeted by context partition id
                var selectorById = new SupportSelectorById(Collections.SingletonList(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorById),
                    env.Statement("s0").GetSafeEnumerator(selectorById),
                    fields,
                    new[] {
                        new object[] {1, "S0_2", "E1", 3},
                        new object[] {1, "S0_2", "E3", 201}
                    });

                // test iterator targeted by property on triggering event
                var filtered = new SupportSelectorFilteredInitTerm("S0_2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(filtered),
                    env.Statement("s0").GetSafeEnumerator(filtered),
                    fields,
                    new[] {
                        new object[] {1, "S0_2", "E1", 3},
                        new object[] {1, "S0_2", "E3", 201}
                    });

                // test always-false filter - compare context partition info
                filtered = new SupportSelectorFilteredInitTerm(null);
                Assert.IsFalse(env.Statement("s0").GetEnumerator(filtered).MoveNext());
                EPAssertionUtil.AssertEqualsAnyOrder(new object[] {1000L, 2000L}, filtered.ContextsStartTimes);
                EPAssertionUtil.AssertEqualsAnyOrder(new object[] {"S0_1", "S0_2"}, filtered.P00PropertyValues);

                try {
                    env.Statement("s0")
                        .GetEnumerator(
                            new ProxyContextPartitionSelectorSegmented {
                                ProcPartitionKeys = () => { return null; }
                            });
                    Assert.Fail();
                }
                catch (InvalidContextPartitionSelector ex) {
                    Assert.IsTrue(
                        ex.Message.StartsWith(
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById] interfaces but received com."),
                        "message: " + ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextInitTermFilterInitiatedFilterAllTerminated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c1".SplitCsv();
                var epl = "create context MyContext as " +
                          "initiated by SupportBean_S0 " +
                          "terminated by SupportBean_S1;\n" +
                          "@Name('s0') context MyContext select sum(IntPrimitive) as c1 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "S0_1")); // initiate one

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2});

                env.SendEventBean(new SupportBean_S0(11, "S0_2")); // initiate another

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {5},
                        new object[] {3}
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {9},
                        new object[] {7}
                    });

                env.SendEventBean(new SupportBean_S1(1, "S1_1")); // terminate all

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextInitTermFilterInitiatedFilterTerminatedCorrelatedOutputSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context EveryNowAndThen as " +
                    "initiated by SupportBean_S0 as s0 " +
                    "terminated by SupportBean_S1(p10 = s0.p00)",
                    path);

                var fields = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context EveryNowAndThen select context.s0.p00 as c1, sum(IntPrimitive) as c2 " +
                    "from SupportBean#keepall output snapshot when terminated",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_S0(100, "G1")); // starts it

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 3));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S1(200, "GX"));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(200, "G1")); // terminate
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 5});

                env.SendEventBean(new SupportBean_S0(101, "G2")); // starts new one
                env.SendEventBean(new SupportBean("E4", 4));

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(102, "G3")); // also starts new one

                env.SendEventBean(new SupportBean("E5", 5));

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E6", 6));

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S1(0, "G2")); // terminate G2
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 15});

                env.Milestone(7);

                env.SendEventBean(new SupportBean_S1(0, "G3")); // terminate G3
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 11});

                env.UndeployAll();
            }
        }

        public class ContextInitTermFilterAndAfter1Min : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T8:00:00.000");
                var eplContext = "@Name('CTX') create context CtxInitiated " +
                                 "initiated by SupportBean_S0 as sb0 " +
                                 "terminated after 1 minute;\n";
                var eplGrouped =
                    "@Name('S1') context CtxInitiated select TheString as c1, sum(IntPrimitive) as c2, context.sb0.p00 as c3 from SupportBean;\n";
                env.CompileDeploy(eplContext + eplGrouped).AddListener("S1");
                var fields = "c1,c2,c3".SplitCsv();

                env.SendEventBean(new SupportBean("G1", 1));
                Assert.IsFalse(env.Listener("S1").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(1, "SB01"));

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, "SB01"});

                env.SendEventBean(new SupportBean("G3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 5, "SB01"});

                env.SendEventBean(new SupportBean_S0(2, "SB02"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("S1").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"G4", 9, "SB01"},
                        new object[] {"G4", 4, "SB02"}
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("S1").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"G5", 14, "SB01"},
                        new object[] {"G5", 9, "SB02"}
                    });

                SendTimeEvent(env, "2002-05-1T8:01:00.000");

                env.SendEventBean(new SupportBean("G6", 6));
                Assert.IsFalse(env.Listener("S1").GetAndClearIsInvoked());

                // clean up
                env.UndeployAll();
            }
        }

        public class ContextInitTermFilterAndPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T8:00:00.000");
                var fields = "id".SplitCsv();

                var eplContext = "@Name('CTX') create context CtxInitiated " +
                                 "initiated by SupportBean sb " +
                                 "terminated by pattern [SupportBean_S0(p00=sb.TheString) => SupportBean_S1(p10=sb.TheString)];\n";
                var eplSelect = "@Name('S1') context CtxInitiated " +
                                "select id from SupportBean_S2(p20 = context.sb.TheString)";
                env.CompileDeploy(eplContext + eplSelect).AddListener("S1");

                // start context for G1
                env.SendEventBean(new SupportBean("G1", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S2(100, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100});

                // start context for G2
                env.SendEventBean(new SupportBean("G2", 0));

                env.SendEventBean(new SupportBean_S2(101, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {101});
                env.SendEventBean(new SupportBean_S2(102, "G2"));
                env.SendEventBean(new SupportBean_S2(103, "G3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {102});

                env.Milestone(1);

                // end context for G1
                env.SendEventBean(new SupportBean_S0(1, "G1"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S1(1, "G1"));

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S2(201, "G1"));
                env.SendEventBean(new SupportBean_S2(202, "G2"));
                env.SendEventBean(new SupportBean_S2(203, "G3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {202});

                // end context for G2
                env.SendEventBean(new SupportBean_S0(2, "G2"));

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(2, "G2"));

                env.SendEventBean(new SupportBean_S2(301, "G1"));
                env.SendEventBean(new SupportBean_S2(302, "G2"));
                env.SendEventBean(new SupportBean_S2(303, "G3"));
                Assert.IsFalse(env.Listener("S1").IsInvoked);

                env.UndeployAll();
            }
        }

        public class ContextInitTermPatternAndAfter1Min : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T8:00:00.000");
                var path = new RegressionPath();

                var eplContext = "@Name('CTX') create context CtxInitiated " +
                                 "initiated by pattern [every s0=SupportBean_S0 => s1=SupportBean_S1(id = s0.id)]" +
                                 "terminated after 1 minute";
                env.CompileDeploy(eplContext, path);

                var fields = "c1,c2,c3,c4".SplitCsv();
                var eplGrouped = "@Name('S1') context CtxInitiated " +
                                 "select TheString as c1, sum(IntPrimitive) as c2, context.s0.p00 as c3, context.s1.p10 as c4 from SupportBean";
                env.CompileDeploy(eplGrouped, path).AddListener("S1");

                env.SendEventBean(new SupportBean_S0(10, "S0_1"));
                env.SendEventBean(new SupportBean_S1(20, "S1_1"));
                Assert.IsFalse(env.Listener("S1").GetAndClearIsInvoked());

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(10, "S1_2"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1, "S0_1", "S1_2"});

                env.SendEventBean(new SupportBean_S0(11, "S0_2"));
                env.SendEventBean(new SupportBean_S1(11, "S1_2"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("S1").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 3, "S0_1", "S1_2"},
                        new object[] {"E2", 2, "S0_2", "S1_2"}
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("S1").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E3", 6, "S0_1", "S1_2"},
                        new object[] {"E3", 5, "S0_2", "S1_2"}
                    });

                env.UndeployModuleContaining("S1");
                env.UndeployModuleContaining("CTX");
            }
        }

        internal class ContextInitTermScheduleFilterResources : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test no-context statement
                env.CompileDeploy("@Name('s0') select * from SupportBean#time(30)");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                // test initiated
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();
                var eplCtx = "@Name('ctx') create context EverySupportBean as " +
                             "initiated by SupportBean as sb " +
                             "terminated after 1 minutes";
                env.CompileDeploy(eplCtx, path);

                env.CompileDeploy("context EverySupportBean select * from SupportBean_S0#time(2 min) sb0", path);
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "S0_1"));
                Assert.AreEqual(2, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(2);

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.UndeployAll();
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextInitTermPatternInitiatedStraightSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");

                var eplCtx = "@Name('ctx') create context EverySupportBean as " +
                             "initiated by pattern [every (a=SupportBean_S0 or b=SupportBean_S1)] " +
                             "terminated after 1 minutes";
                env.CompileDeploy(eplCtx, path);

                var fields = "c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context EverySupportBean " +
                    "select context.a.id as c1, context.b.id as c2, theString as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S1(2));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, 2, "E1"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {null, 2, "E2"},
                        new object[] {3, null, "E2"}
                    });

                env.UndeployAll();
                path.Clear();

                // test SODA
                AssertSODA(env, path, eplCtx);
            }
        }

        internal class ContextInitTermFilterInitiatedStraightEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var ctxEPL = "create context EverySupportBean as " +
                             "initiated by SupportBean(theString like \"I%\") as sb " +
                             "terminated after 1 minutes";
                env.CompileDeploy(ctxEPL, path);

                var fields = "c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context EverySupportBean select sum(longPrimitive) as c1 from SupportBean(IntPrimitive = context.sb.IntPrimitive)",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("E1", -1, -2L));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(MakeEvent("I1", 2, 4L)); // counts towards stuff
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4L});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 2, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {7L});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("I2", 3, 14L)); // counts towards stuff
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {14L});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E3", 2, 2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {9L});

                env.SendEventBean(MakeEvent("E4", 3, 15L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {29L});

                env.Milestone(3);

                SendTimeEvent(env, "2002-05-1T08:01:30.000");

                env.SendEventBean(MakeEvent("E", -1, -2L));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // test SODA
                env.UndeployAll();
                env.EplToModelCompileDeploy(ctxEPL).UndeployAll();
            }
        }

        internal class ContextInitTermFilterAllOperators : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var milestone = new AtomicLong();

                // test plain
                env.CompileDeploy(
                    "create context EverySupportBean as " +
                    "initiated by SupportBean_S0 as sb " +
                    "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds",
                    path);

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id = intBoxed",
                    new[] {
                        new object[] {10, true},
                        new object[] {9, false},
                        new object[] {null, false}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed = context.sb.id",
                    new[] {
                        new object[] {10, true},
                        new object[] {9, false},
                        new object[] {null, false}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id > intBoxed",
                    new[] {
                        new object[] {11, false},
                        new object[] {10, false},
                        new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id >= intBoxed",
                    new[] {
                        new object[] {11, false},
                        new object[] {10, true},
                        new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id < intBoxed",
                    new[] {
                        new object[] {11, true},
                        new object[] {10, false},
                        new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id <= intBoxed",
                    new[] {
                        new object[] {11, true},
                        new object[] {10, true},
                        new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed < context.sb.id",
                    new[] {
                        new object[] {11, false}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed <= context.sb.id",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed > context.sb.id",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed >= context.sb.id",
                    new[] {
                        new object[] {11, true}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed in (context.sb.id)",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed between context.sb.id and context.sb.id",
                    new[] {
                        new object[] {11, false}, new object[] {10, true}, new object[] {9, false},
                        new object[] {8, false}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id != intBoxed",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed != context.sb.id",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});

                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed not in (context.sb.id)",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed not between context.sb.id and context.sb.id",
                    new[] {
                        new object[] {11, true}, new object[] {10, false}, new object[] {9, true},
                        new object[] {8, true}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id is intBoxed",
                    new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed is context.sb.id",
                    new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id is not intBoxed",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
                TryOperator(
                    env,
                    path,
                    milestone,
                    "intBoxed is not context.sb.id",
                    new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});

                // try coercion
                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id = shortBoxed",
                    new[] {
                        new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "shortBoxed = context.sb.id",
                    new[] {
                        new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "context.sb.id > shortBoxed",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, false},
                        new object[] {(short) 9, true}, new object[] {(short) 8, true}
                    });
                TryOperator(
                    env,
                    path,
                    milestone,
                    "shortBoxed < context.sb.id",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, false},
                        new object[] {(short) 9, true}, new object[] {(short) 8, true}
                    });

                TryOperator(
                    env,
                    path,
                    milestone,
                    "shortBoxed in (context.sb.id)",
                    new[] {
                        new object[] {(short) 11, false}, new object[] {(short) 10, true},
                        new object[] {(short) 9, false}, new object[] {(short) 8, false}
                    });

                env.UndeployAll();
            }

            private static void TryOperator(
                RegressionEnvironment env,
                RegressionPath path,
                AtomicLong milestone,
                string @operator,
                object[][] testdata)
            {
                env.CompileDeploy(
                    "@Name('s0') context EverySupportBean " +
                    "select TheString as c0,intPrimitive as c1,context.sb.p00 as c2 " +
                    "from SupportBean(" +
                    @operator +
                    ")",
                    path);
                env.AddListener("s0");

                // initiate
                env.SendEventBean(new SupportBean_S0(10, "S01"));

                env.MilestoneInc(milestone);

                for (var i = 0; i < testdata.Length; i++) {
                    var bean = new SupportBean();
                    var testValue = testdata[i][0];
                    if (testValue is int?) {
                        bean.IntBoxed = (int?) testValue;
                    }
                    else {
                        bean.ShortBoxed = (short?) testValue;
                    }

                    var expected = (bool) testdata[i][1];

                    env.SendEventBean(bean);
                    Assert.AreEqual(expected, env.Listener("s0").GetAndClearIsInvoked(), "Failed at " + i);
                }

                env.UndeployModuleContaining("s0");
            }
        }

        internal class ContextInitTermFilterBooleanOperator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context EverySupportBean as " +
                    "initiated by SupportBean_S0 as sb " +
                    "terminated after 10 days 5 hours 2 minutes 1 sec 11 milliseconds",
                    path);

                env.Milestone(0);

                var fields = "c0,c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context EverySupportBean " +
                    "select TheString as c0,intPrimitive as c1,context.sb.p00 as c2 " +
                    "from SupportBean(intPrimitive + context.sb.id = 5)",
                    path);
                env.AddListener("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(3, "S01"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2, "S01"});

                env.SendEventBean(new SupportBean_S0(3, "S02"));

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E3", 2, "S01"}, new object[] {"E3", 2, "S02"}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(4, "S03"));

                env.SendEventBean(new SupportBean("E4", 2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", 2, "S01"}, new object[] {"E4", 2, "S02"}});

                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", 1, "S03"}});

                env.UndeployAll();
            }
        }

        internal class ContextInitTermTerminateTwoContextSameTime : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");

                var eplContext = "@Name('CTX') create context CtxInitiated " +
                                 "initiated by SupportBean_S0 as sb0 " +
                                 "terminated after 1 minute";
                env.CompileDeploy(eplContext, path);

                var fields = "c1,c2,c3".SplitCsv();
                var eplGrouped =
                    "@Name('s0') context CtxInitiated select TheString as c1, sum(IntPrimitive) as c2, context.sb0.p00 as c3 from SupportBean";
                env.CompileDeploy(eplGrouped, path).AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, "SB01"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, "SB01"});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 5, "SB01"});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(2, "SB02"));

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"G4", 9, "SB01"}, new object[] {"G4", 4, "SB02"}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("G5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"G5", 14, "SB01"}, new object[] {"G5", 9, "SB02"}});

                env.Milestone(6);

                SendTimeEvent(env, "2002-05-1T08:01:00.000");

                env.Milestone(7);

                env.SendEventBean(new SupportBean("G6", 6));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                // clean up
                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("CTX");
            }
        }

        internal class ContextInitTermOutputSnapshotWhenTerminated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c1".SplitCsv();
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");

                env.CompileDeploy(
                    "create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min",
                    path);

                // test when-terminated and snapshot
                var epl =
                    "@Name('s0') context EveryMinute select sum(IntPrimitive) as c1 from SupportBean output snapshot when terminated";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                SendTimeEvent(env, "2002-05-1T08:01:10.000");
                env.SendEventBean(new SupportBean("E2", 2));

                SendTimeEvent(env, "2002-05-1T08:01:59.999");
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(1);

                // terminate
                SendTimeEvent(env, "2002-05-1T08:02:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1 + 2 + 3});

                env.Milestone(2);

                SendTimeEvent(env, "2002-05-1T08:02:01.000");
                env.SendEventBean(new SupportBean("E4", 4));
                env.SendEventBean(new SupportBean("E5", 5));
                env.SendEventBean(new SupportBean("E6", 6));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(3);

                // terminate
                SendTimeEvent(env, "2002-05-1T08:03:00.000");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4 + 5 + 6});

                env.UndeployModuleContaining("s0");

                // test late-coming statement without "terminated"
                env.CompileDeploy(
                    "@Name('s0') context EveryMinute " +
                    "select context.id as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 2 events",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E10", 1));
                env.SendEventBean(new SupportBean("E11", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                SendTimeEvent(env, "2002-05-1T08:04:00.000");
                env.SendEventBean(new SupportBean("E12", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E13", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {7});

                env.Milestone(5);

                // terminate
                SendTimeEvent(env, "2002-05-1T08:05:00.000");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextInitTermOutputAllEvery2AndTerminated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min",
                    path);

                // test when-terminated and every 2 events output all with group by
                var fields = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context EveryMinute " +
                    "select TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString output all every 2 events and when terminated order by theString asc",
                    path);
                env.AddListener("s0");

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                SendTimeEvent(env, "2002-05-1T08:01:10.000");
                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 1 + 2}});

                env.Milestone(1);

                SendTimeEvent(env, "2002-05-1T08:01:59.999");
                env.SendEventBean(new SupportBean("E2", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(2);

                // terminate
                SendTimeEvent(env, "2002-05-1T08:02:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 1 + 2}, new object[] {"E2", 3}});

                SendTimeEvent(env, "2002-05-1T08:02:01.000");
                env.SendEventBean(new SupportBean("E4", 4));
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", 4}, new object[] {"E5", 5}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E6", 6));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E4", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", 14}, new object[] {"E5", 5}, new object[] {"E6", 6}});

                env.Milestone(5);

                // terminate
                SendTimeEvent(env, "2002-05-1T08:03:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", 14}, new object[] {"E5", 5}, new object[] {"E6", 6}});

                env.SendEventBean(new SupportBean("E1", -1));

                env.Milestone(6);

                env.SendEventBean(new SupportBean("E6", -2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", -1}, new object[] {"E6", -2}});

                env.UndeployAll();
            }
        }

        internal class ContextInitTermOutputWhenExprWhenTerminatedCondition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                env.CompileDeploy(
                    "create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min",
                    path);

                // test when-terminated and every 2 events output all with group by
                var fields = "c0".SplitCsv();
                var epl = "@Name('s0') context EveryMinute " +
                          "select TheString as c0 from SupportBean output when count_insert>1 and when terminated and count_insert>0";
                env.CompileDeploy(epl, path).AddListener("s0");

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(1);

                SendTimeEvent(env, "2002-05-1T08:01:59.999");
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(2);

                // terminate, new context partition
                SendTimeEvent(env, "2002-05-1T08:02:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E3"}});

                SendTimeEvent(env, "2002-05-1T08:02:10.000");
                env.SendEventBean(new SupportBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                SendTimeEvent(env, "2002-05-1T08:03:00.000");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.EplToModelCompileDeploy(epl, path).UndeployAll();
            }
        }

        internal class ContextInitTermOutputOnlyWhenTerminatedCondition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 1 min",
                    path);

                // test when-terminated and every 2 events output all with group by
                var fields = "c0".SplitCsv();
                var epl = "@Name('s0') context EveryMinute " +
                          "select TheString as c0 from SupportBean output when terminated and count_insert > 0";
                env.CompileDeploy(epl, path);
                env.AddListener("s0");

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                // terminate, new context partition
                SendTimeEvent(env, "2002-05-1T08:02:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(2);

                // terminate, new context partition
                SendTimeEvent(env, "2002-05-1T08:03:00.000");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextInitTermOutputOnlyWhenSetAndWhenTerminatedSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();
                var eplContext = "create context EveryMinute as " +
                                 "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                                 "terminated after 1 min";
                env.CompileDeploy(eplContext, path);

                // include then-set and both real-time and terminated output
                var eplVariable = "@Name('var') create variable int myvar = 0";
                env.CompileDeploy(eplVariable, path);
                var eplOne = "@Name('s0') context EveryMinute select TheString as c0 from SupportBean " +
                             "output when true " +
                             "then set myvar=1 " +
                             "and when terminated " +
                             "then set myvar=2";
                env.CompileDeploy(eplOne, path).AddListener("s0");

                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.AreEqual(1, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "myvar"));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                SendTimeEvent(env, "2002-05-1T08:02:00.000"); // terminate, new context partition
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(2, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "myvar"));

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("var");
                env.UndeployAll();
                path.Clear();

                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplVariable, path);
                AssertSODA(env, path, eplOne);
            }
        }

        internal class ContextInitTermOutputOnlyWhenTerminatedThenSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                env.CompileDeploy("@Name('var') create variable int myvar = 0", path);
                env.CompileDeploy(
                    "create context EverySupportBeanS0 as " +
                    "initiated by SupportBean_S0 as s0 " +
                    "terminated after 1 min",
                    path);

                // include only-terminated output with set
                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "myvar", 0);
                var eplTwo = "@Name('s0') context EverySupportBeanS0 select TheString as c0 from SupportBean " +
                             "output when terminated " +
                             "then set myvar=10";
                env.CompileDeploy(eplTwo, path).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "S0"));

                env.SendEventBean(new SupportBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // terminate, new context partition
                SendTimeEvent(env, "2002-05-1T08:01:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4"}});
                Assert.AreEqual(10, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("var"), "myvar"));

                AssertSODA(env, path, eplTwo);
            }
        }

        public class ContextInitTermCrontab : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create context EveryMinute as " +
                    "initiated by pattern[every timer:at(*, *, *, *, *)] " +
                    "terminated after 3 min",
                    path);

                var fields = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') @IterableUnbound context EveryMinute select TheString as c1, sum(IntPrimitive) as c2 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 0);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    null);

                env.Milestone(0);

                SendTimeEvent(env, "2002-05-1T08:01:00.000");

                env.Milestone(1);

                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 1);
                env.SendEventBean(new SupportBean("E2", 5));
                object[][] expected = {new object[] {"E2", 5}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:01:59.999");

                env.Milestone(2);

                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 1);
                env.SendEventBean(new SupportBean("E3", 6));
                expected = new[] {new object[] {"E3", 11}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:02:00.000");

                env.Milestone(4);

                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 2);
                env.SendEventBean(new SupportBean("E4", 7));
                expected = new[] {new object[] {"E4", 18}, new object[] {"E4", 7}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:02:59.999");

                env.Milestone(5);

                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 2);
                env.SendEventBean(new SupportBean("E5", 8));
                expected = new[] {new object[] {"E5", 26}, new object[] {"E5", 15}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:03:00.000");

                env.Milestone(6);

                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 3);
                env.SendEventBean(new SupportBean("E6", 9));
                expected = new[] {new object[] {"E6", 35}, new object[] {"E6", 24}, new object[] {"E6", 9}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:04:00.000");

                env.Milestone(7);

                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 3);
                env.SendEventBean(new SupportBean("E7", 10));
                expected = new[] {new object[] {"E7", 34}, new object[] {"E7", 19}, new object[] {"E7", 10}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                SendTimeEvent(env, "2002-05-1T08:05:00.000");

                env.Milestone(8);

                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 3);
                env.SendEventBean(new SupportBean("E8", 11));
                expected = new[] {new object[] {"E8", 30}, new object[] {"E8", 21}, new object[] {"E8", 11}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                // assert certain keywords are valid: last keyword, timezone
                env.CompileDeploy("create context CtxMonthly1 start (0, 0, 1, *, *, 0) end(59, 23, last, *, *, 59)");
                env.CompileDeploy("create context CtxMonthly2 start (0, 0, 1, *, *) end(59, 23, last, *, *)");
                env.CompileDeploy(
                    "create context CtxMonthly3 start (0, 0, 1, *, *, 0, 'GMT-5') end(59, 23, last, *, *, 59, 'GMT-8')");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context CtxMonthly4 start (0) end(*,*,*,*,*)",
                    "Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 1 [create context CtxMonthly4 start (0) end(*,*,*,*,*)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context CtxMonthly4 start (*,*,*,*,*) end(*,*,*,*,*,*,*,*)",
                    "Invalid schedule specification: Invalid number of crontab parameters, expecting between 5 and 7 parameters, received 8 [create context CtxMonthly4 start (*,*,*,*,*) end(*,*,*,*,*,*,*,*)]");

                // test invalid -after
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context CtxMonthly4 start after 1 second end after -1 seconds",
                    "Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after 1 second end after -1 seconds]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context CtxMonthly4 start after -1 second end after 1 seconds",
                    "Invalid negative time period expression '-1 seconds' [create context CtxMonthly4 start after -1 second end after 1 seconds]");

                env.UndeployAll();
            }
        }

        internal class ContextStartEndStartNowCalMonthScoped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                var path = new RegressionPath();
                env.CompileDeploy("create context MyCtx start SupportBean_S1 end after 1 month", path);
                env.CompileDeploy("@Name('s0') context MyCtx select * from SupportBean", path).AddListener("s0");

                env.SendEventBean(new SupportBean_S1(1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(1);

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(2);

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E3", 3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        public class ContextInitTermAggregationGrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "create schema SummedEvent(grp string, key string, value int);\n" +
                          "create schema InitEvent(grp string);\n" +
                          "create schema TermEvent(grp string);\n";

                epl += "@Name('Ctx1') create context MyContext " +
                       "initiated by InitEvent as i " +
                       "terminated by TermEvent(grp = i.grp);\n";

                epl += "@Name('s0') context MyContext " +
                       "select key as c0, sum(value) as c1 " +
                       "from SummedEvent(grp = context.i.grp) group by key;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());
                env.AddListener("s0");

                env.Milestone(0);

                SendInitEvent(env, "CP1");
                AssertPartitionInfo(env);

                env.Milestone(1);

                AssertPartitionInfo(env);
                SendInitEvent(env, "CP2");

                env.Milestone(2);

                SendSummedEvent(env, "CP2", "G1", 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 100});

                env.Milestone(3);

                SendSummedEvent(env, "CP1", "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(4);

                SendSummedEvent(env, "CP1", "G2", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 5});

                env.Milestone(5);

                SendSummedEvent(env, "CP2", "G1", 101);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 201});

                env.Milestone(6);

                SendSummedEvent(env, "CP1", "G1", 11);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 21});

                env.Milestone(7);

                SendTermEvent(env, "CP1");
                SendSummedEvent(env, "CP1", "G1", -1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(8);

                SendTermEvent(env, "CP2");
                SendSummedEvent(env, "CP1", "G1", -1);
                SendSummedEvent(env, "CP2", "G1", -1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(9);

                SendInitEvent(env, "CP1");
                SendSummedEvent(env, "CP1", "G1", 1000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1000});

                env.UndeployAll();
                env.Milestone(10);
            }

            private void AssertPartitionInfo(RegressionEnvironment env)
            {
                var partitionAdmin = env.Runtime.ContextPartitionService;
                var partitions = partitionAdmin.GetContextPartitions(
                    env.DeploymentId("Ctx1"),
                    "MyContext",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(1, partitions.Identifiers.Count);
                var ident = (ContextPartitionIdentifierInitiatedTerminated) partitions.Identifiers.Values
                    .First();
                Assert.AreEqual(null, ident.EndTime);
                Assert.IsNotNull(ident.Properties.Get("i"));
            }

            private void SendInitEvent(
                RegressionEnvironment env,
                string grp)
            {
                env.SendEventMap(Collections.SingletonDataMap("grp", grp), "InitEvent");
            }

            private void SendTermEvent(
                RegressionEnvironment env,
                string grp)
            {
                env.SendEventMap(Collections.SingletonDataMap("grp", grp), "TermEvent");
            }

            private void SendSummedEvent(
                RegressionEnvironment env,
                string grp,
                string key,
                int value)
            {
                env.SendEventMap(CollectionUtil.BuildMap("grp", grp, "key", key, "value", value), "SummedEvent");
            }
        }

        public class ContextInitTermPrevPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T8:00:00.000");
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)",
                    path);

                var fields = "col1,col2,col3,col4,col5".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NineToFive " +
                    "select prev(TheString) as col1, prevwindow(sb) as col2, prevtail(TheString) as col3, prior(1, theString) as col4, sum(IntPrimitive) as col5 " +
                    "from SupportBean#keepall() as sb",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                // now started
                SendTimeEvent(env, "2002-05-1T9:00:00.000");
                var event1 = new SupportBean("E1", 1);
                env.SendEventBean(event1);
                object[][] expected = {new object[] {null, new[] {event1}, "E1", null, 1}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                env.Milestone(1);

                var event2 = new SupportBean("E2", 2);
                env.SendEventBean(event2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", new[] {event2, event1}, "E1", "E1", 3});

                env.Milestone(2);

                // now gone
                SendTimeEvent(env, "2002-05-1T17:00:00.000");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    null);

                env.Milestone(3);

                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                // now started
                SendTimeEvent(env, "2002-05-2T9:00:00.000");

                env.Milestone(5);

                var event3 = new SupportBean("E3", 9);
                env.SendEventBean(event3);
                expected = new[] {new object[] {null, new[] {event3}, "E3", null, 9}};
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expected);

                env.UndeployAll();
            }
        }
    }
} // end of namespace