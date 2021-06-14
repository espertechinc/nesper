///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

// ReSharper disable RedundantExplicitArrayCreation

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRollupDimensionality
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithBoundRollup2Dim(execs);
            WithUnboundRollup2Dim(execs);
            WithUnboundRollup1Dim(execs);
            WithUnboundRollup2DimBatchWindow(execs);
            WithUnboundRollup3Dim(execs);
            WithMixedAccessAggregation(execs);
            WithNonBoxedTypeWithRollup(execs);
            WithGroupByWithComputation(execs);
            WithUnboundRollupUnenclosed(execs);
            WithUnboundCubeUnenclosed(execs);
            WithUnboundGroupingSet2LevelUnenclosed(execs);
            WithBoundCube3Dim(execs);
            WithBoundGroupingSet2LevelNoTopNoDetail(execs);
            WithBoundGroupingSet2LevelTopAndDetail(execs);
            WithUnboundCube4Dim(execs);
            WithInvalid(execs);
            WithOutputWhenTerminated(execs);
            WithContextPartitionAlsoRollup(execs);
            WithOnSelect(execs);
            WithNamedWindowCube2Dim(execs);
            WithNamedWindowDeleteAndRStream2Dim(execs);
            WithRollupMultikeyWArray(execs);
            WithRollupMultikeyWArrayGroupingSet(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRollupMultikeyWArrayGroupingSet(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRollupMultikeyWArrayGroupingSet());
            return execs;
        }

        public static IList<RegressionExecution> WithRollupMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRollupMultikeyWArray(false, true));
            execs.Add(new ResultSetQueryTypeRollupMultikeyWArray(false, false));
            execs.Add(new ResultSetQueryTypeRollupMultikeyWArray(true, false));
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowDeleteAndRStream2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeNamedWindowDeleteAndRStream2Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowCube2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeNamedWindowCube2Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithOnSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeOnSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionAlsoRollup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeContextPartitionAlsoRollup());
            return execs;
        }

        public static IList<RegressionExecution> WithOutputWhenTerminated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeOutputWhenTerminated());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundCube4Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundCube4Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundGroupingSet2LevelTopAndDetail(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeBoundGroupingSet2LevelTopAndDetail());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundGroupingSet2LevelNoTopNoDetail(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeBoundGroupingSet2LevelNoTopNoDetail());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundCube3Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeBoundCube3Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundGroupingSet2LevelUnenclosed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundGroupingSet2LevelUnenclosed());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundCubeUnenclosed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundCubeUnenclosed());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundRollupUnenclosed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundRollupUnenclosed());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupByWithComputation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeGroupByWithComputation());
            return execs;
        }

        public static IList<RegressionExecution> WithNonBoxedTypeWithRollup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeNonBoxedTypeWithRollup());
            return execs;
        }

        public static IList<RegressionExecution> WithMixedAccessAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeMixedAccessAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundRollup3Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundRollup3Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundRollup2DimBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundRollup2DimBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundRollup1Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundRollup1Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundRollup2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeUnboundRollup2Dim());
            return execs;
        }

        public static IList<RegressionExecution> WithBoundRollup2Dim(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeBoundRollup2Dim());
            return execs;
        }

        private static void TryAssertionOutputWhenTerminated(
            RegressionEnvironment env,
            string outputLimit,
            SupportOutputLimitOpt opt,
            AtomicLong milestone)
        {
            var epl = "@Name('ctx') create context MyContext start SupportBean_S0(Id=1) end SupportBean_S0(Id=0);\n" +
                      "@Name('s0') context MyContext select TheString as c0, sum(IntPrimitive) as c1 " +
                      "from SupportBean group by rollup(TheString) output " +
                      outputLimit +
                      " when terminated";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1));
            env.SendEventBean(new SupportBean("E1", 1));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportBean("E1", 3));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                new[] {"c0", "c1"},
                new[] {new object[] {"E1", 4}, new object[] {"E2", 2}, new object[] {null, 6}});

            env.SendEventBean(new SupportBean_S0(1));
            env.SendEventBean(new SupportBean("E2", 4));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 5));
            env.SendEventBean(new SupportBean("E1", 6));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                new[] {"c0", "c1"},
                new[] {new object[] {"E2", 4}, new object[] {"E1", 11}, new object[] {null, 15}});

            env.UndeployAll();
        }

        private static void TryAssertionBoundCube(
            RegressionEnvironment env,
            string groupBy,
            AtomicLong milestone)
        {
            var fields = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8"};

            var epl = "@Name('s0')" +
                      "select TheString as c0, " +
                      "IntPrimitive as c1, " +
                      "LongPrimitive as c2, " +
                      "count(*) as c3, " +
                      "sum(DoublePrimitive) as c4," +
                      "grouping(TheString) as c5," +
                      "grouping(IntPrimitive) as c6," +
                      "grouping(LongPrimitive) as c7," +
                      "grouping_id(TheString, IntPrimitive, LongPrimitive) as c8 " +
                      "from SupportBean#length(4) " +
                      "group by " +
                      groupBy;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(MakeEvent("E1", 1, 10, 100));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 1, 10L, 1L, 100d, 0, 0, 0, 0}, // {0, 1, 2}
                    new object[] {"E1", 1, null, 1L, 100d, 0, 0, 1, 1}, // {0, 1}
                    new object[] {"E1", null, 10L, 1L, 100d, 0, 1, 0, 2}, // {0, 2}
                    new object[] {"E1", null, null, 1L, 100d, 0, 1, 1, 3}, // {0}
                    new object[] {null, 1, 10L, 1L, 100d, 1, 0, 0, 4}, // {1, 2}
                    new object[] {null, 1, null, 1L, 100d, 1, 0, 1, 5}, // {1}
                    new object[] {null, null, 10L, 1L, 100d, 1, 1, 0, 6}, // {2}
                    new object[] {null, null, null, 1L, 100d, 1, 1, 1, 7}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E2", 1, 20, 200));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E2", 1, 20L, 1L, 200d, 0, 0, 0, 0},
                    new object[] {"E2", 1, null, 1L, 200d, 0, 0, 1, 1},
                    new object[] {"E2", null, 20L, 1L, 200d, 0, 1, 0, 2},
                    new object[] {"E2", null, null, 1L, 200d, 0, 1, 1, 3},
                    new object[] {null, 1, 20L, 1L, 200d, 1, 0, 0, 4},
                    new object[] {null, 1, null, 2L, 300d, 1, 0, 1, 5},
                    new object[] {null, null, 20L, 1L, 200d, 1, 1, 0, 6},
                    new object[] {null, null, null, 2L, 300d, 1, 1, 1, 7}
                });

            env.SendEventBean(MakeEvent("E1", 2, 10, 300));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 2, 10L, 1L, 300d, 0, 0, 0, 0},
                    new object[] {"E1", 2, null, 1L, 300d, 0, 0, 1, 1},
                    new object[] {"E1", null, 10L, 2L, 400d, 0, 1, 0, 2},
                    new object[] {"E1", null, null, 2L, 400d, 0, 1, 1, 3},
                    new object[] {null, 2, 10L, 1L, 300d, 1, 0, 0, 4},
                    new object[] {null, 2, null, 1L, 300d, 1, 0, 1, 5},
                    new object[] {null, null, 10L, 2L, 400d, 1, 1, 0, 6},
                    new object[] {null, null, null, 3L, 600d, 1, 1, 1, 7}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E2", 2, 20, 400));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E2", 2, 20L, 1L, 400d, 0, 0, 0, 0},
                    new object[] {"E2", 2, null, 1L, 400d, 0, 0, 1, 1},
                    new object[] {"E2", null, 20L, 2L, 600d, 0, 1, 0, 2},
                    new object[] {"E2", null, null, 2L, 600d, 0, 1, 1, 3},
                    new object[] {null, 2, 20L, 1L, 400d, 1, 0, 0, 4},
                    new object[] {null, 2, null, 2L, 700d, 1, 0, 1, 5},
                    new object[] {null, null, 20L, 2L, 600d, 1, 1, 0, 6},
                    new object[] {null, null, null, 4L, 1000d, 1, 1, 1, 7}
                });

            env.MilestoneInc(milestone);

            // expiring/removing ("E1", 1, 10, 100)
            env.SendEventBean(MakeEvent("E2", 1, 10, 500));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E2", 1, 10L, 1L, 500d, 0, 0, 0, 0},
                    new object[] {"E1", 1, 10L, 0L, null, 0, 0, 0, 0},
                    new object[] {"E2", 1, null, 2L, 700d, 0, 0, 1, 1},
                    new object[] {"E1", 1, null, 0L, null, 0, 0, 1, 1},
                    new object[] {"E2", null, 10L, 1L, 500d, 0, 1, 0, 2},
                    new object[] {"E1", null, 10L, 1L, 300d, 0, 1, 0, 2},
                    new object[] {"E2", null, null, 3L, 1100d, 0, 1, 1, 3},
                    new object[] {"E1", null, null, 1L, 300d, 0, 1, 1, 3},
                    new object[] {null, 1, 10L, 1L, 500d, 1, 0, 0, 4},
                    new object[] {null, 1, null, 2L, 700d, 1, 0, 1, 5},
                    new object[] {null, null, 10L, 2L, 800d, 1, 1, 0, 6},
                    new object[] {null, null, null, 4L, 1400d, 1, 1, 1, 7}
                });

            env.UndeployAll();
        }

        private static void TryAssertionUnboundRollupUnenclosed(
            RegressionEnvironment env,
            string groupBy,
            AtomicLong milestone)
        {
            var fields = new[] {"c0", "c1", "c2", "c3"};
            var epl = "@Name('s0')" +
                      "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                      "group by " +
                      groupBy;
            env.CompileDeploy(epl).AddListener("s0");

            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("c2"));

            env.SendEventBean(MakeEvent("E1", 10, 100, 1000));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d},
                    new object[] {"E1", null, null, 1000d}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 10, 200, 2000));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d},
                    new object[] {"E1", null, null, 3000d}
                });

            env.SendEventBean(MakeEvent("E1", 20, 100, 3000));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 20, 100L, 3000d}, new object[] {"E1", 20, null, 3000d},
                    new object[] {"E1", null, null, 6000d}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 10, 100, 4000));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 10, 100L, 5000d}, new object[] {"E1", 10, null, 7000d},
                    new object[] {"E1", null, null, 10000d}
                });

            env.UndeployAll();
        }

        private static void TryAssertionUnboundRollup1Dim(
            RegressionEnvironment env,
            string rollup,
            AtomicLong milestone)
        {
            var fields = new[] {"c0", "c1"};

            var epl = "@Name('s0')" +
                      "select TheString as c0, sum(IntPrimitive) as c1 from SupportBean " +
                      "group by " +
                      rollup;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"E1", 10}, new object[] {null, 10}});

            env.SendEventBean(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"E2", 20}, new object[] {null, 30}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 30));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"E1", 40}, new object[] {null, 60}});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 40));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"E2", 60}, new object[] {null, 100}});

            env.UndeployAll();
        }

        private static void TryAssertionUnboundRollup3Dim(
            RegressionEnvironment env,
            string groupByClause,
            bool isJoin,
            AtomicLong milestone)
        {
            var fields = new[] {"c0", "c1", "c2", "c3", "c4"};

            var epl = "@Name('s0')" +
                      "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, count(*) as c3, sum(DoublePrimitive) as c4 " +
                      "from SupportBean#keepall " +
                      (isJoin ? ", SupportBean_S0#lastevent " : "") +
                      "group by " +
                      groupByClause;
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1));

            env.SendEventBean(MakeEvent("E1", 1, 10, 100));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 1, 10L, 1L, 100d}, new object[] {"E1", 1, null, 1L, 100d},
                    new object[] {"E1", null, null, 1L, 100d}, new object[] {null, null, null, 1L, 100d}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 1, 11, 200));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 1, 11L, 1L, 200d}, new object[] {"E1", 1, null, 2L, 300d},
                    new object[] {"E1", null, null, 2L, 300d}, new object[] {null, null, null, 2L, 300d}
                });

            env.SendEventBean(MakeEvent("E1", 2, 10, 300));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 2, 10L, 1L, 300d}, new object[] {"E1", 2, null, 1L, 300d},
                    new object[] {"E1", null, null, 3L, 600d}, new object[] {null, null, null, 3L, 600d}
                });

            env.SendEventBean(MakeEvent("E2", 1, 10, 400));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E2", 1, 10L, 1L, 400d}, new object[] {"E2", 1, null, 1L, 400d},
                    new object[] {"E2", null, null, 1L, 400d}, new object[] {null, null, null, 4L, 1000d}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 1, 10, 500));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 1, 10L, 2L, 600d}, new object[] {"E1", 1, null, 3L, 800d},
                    new object[] {"E1", null, null, 4L, 1100d}, new object[] {null, null, null, 5L, 1500d}
                });

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 1, 11, 600));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {
                    new object[] {"E1", 1, 11L, 2L, 800d}, new object[] {"E1", 1, null, 4L, 1400d},
                    new object[] {"E1", null, null, 5L, 1700d}, new object[] {null, null, null, 6L, 2100d}
                });

            env.UndeployAll();
        }

        private static SupportBean MakeEvent(
            int intBoxed,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.IntBoxed = intBoxed;
            return sb;
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            return sb;
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive,
            int intBoxed)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            sb.IntBoxed = intBoxed;
            return sb;
        }

        private static void AssertTypesC0C1C2(
            EPStatement stmtOne,
            Type expectedC0,
            Type expectedC1,
            Type expectedC2)
        {
            Assert.AreEqual(expectedC0, stmtOne.EventType.GetPropertyType("c0"));
            Assert.AreEqual(expectedC1, stmtOne.EventType.GetPropertyType("c1"));
            Assert.AreEqual(expectedC2, stmtOne.EventType.GetPropertyType("c2"));
        }

        public class ResultSetQueryTypeRollupMultikeyWArrayGroupingSet : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@Name('s0') select sum(Value) as thesum from SupportThreeArrayEvent group by grouping sets((IntArray), (LongArray), (DoubleArray))";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "E1", 1, new int[] {1, 2}, new long[] {10, 20}, new double[] {300, 400}, 1, 1, 1);

                env.Milestone(0);

                SendAssert(env, "E2", 2, new int[] {1, 2}, new long[] {10, 20}, new double[] {300, 400}, 3, 3, 3);
                SendAssert(env, "E3", 3, new int[] {1, 2}, new long[] {11, 21}, new double[] {300, 400}, 6, 3, 6);
                SendAssert(env, "E4", 4, new int[] {1, 3}, new long[] {10}, new double[] {300, 400}, 4, 4, 10);
                SendAssert(env, "E5", 5, new int[] {1, 2}, new long[] {10, 21}, new double[] {301, 400}, 11, 5, 5);

                env.Milestone(1);

                SendAssert(env, "E6", 6, new int[] {1, 2}, new long[] {10, 20}, new double[] {300, 400}, 17, 9, 16);
                SendAssert(env, "E7", 7, new int[] {1, 3}, new long[] {11, 21}, new double[] {300, 400}, 11, 10, 23);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string id,
                int value,
                int[] ints,
                long[] longs,
                double[] doubles,
                int expectedIntArray,
                int expectedLongArray,
                int expectedDoubleArray)
            {
                env.SendEventBean(new SupportThreeArrayEvent(id, value, ints, longs, doubles));
                EventBean[] events = env.Listener("s0").GetAndResetLastNewData();
                Assert.AreEqual(expectedIntArray, events[0].Get("thesum"));
                Assert.AreEqual(expectedLongArray, events[1].Get("thesum"));
                Assert.AreEqual(expectedDoubleArray, events[2].Get("thesum"));
            }
        }

        public class ResultSetQueryTypeRollupMultikeyWArray : RegressionExecution
        {
            private readonly bool join;
            private readonly bool unbound;

            public ResultSetQueryTypeRollupMultikeyWArray(
                bool join,
                bool unbound)
            {
                this.join = join;
                this.unbound = unbound;
            }

            public void Run(RegressionEnvironment env)
            {
                string epl = join
                    ? "@Name('s0') select Array, Value, count(*) as cnt from SupportEventWithIntArray#keepall, SupportBean#keepall group by rollup(Array, Value)"
                    : (unbound
                        ? "@Name('s0') select Array, Value, count(*) as cnt from SupportEventWithIntArray group by rollup(Array, Value)"
                        : "@Name('s0') select Array, Value, count(*) as cnt from SupportEventWithIntArray#keepall group by rollup(Array, Value)"
                    );

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean());

                SendAssertIntArray(env, "E1", new int[] {1, 2}, 5, 1, 1, 1);

                env.Milestone(0);

                SendAssertIntArray(env, "E2", new int[] {1, 2}, 5, 2, 2, 2);
                SendAssertIntArray(env, "E3", new int[] {4, 5}, 5, 3, 1, 1);
                SendAssertIntArray(env, "E4", new int[] {1, 2}, 6, 4, 3, 1);

                env.Milestone(1);

                SendAssertIntArray(env, "E5", new int[] {1, 2}, 5, 5, 4, 3);
                SendAssertIntArray(env, "E6", new int[] {4, 5}, 5, 6, 2, 2);
                SendAssertIntArray(env, "E7", new int[] {1, 2}, 6, 7, 5, 2);
                SendAssertIntArray(env, "E8", new int[] {1}, 5, 8, 1, 1);

                env.UndeployAll();
            }

            private void SendAssertIntArray(
                RegressionEnvironment env,
                string id,
                int[] array,
                int value,
                long expectedTotal,
                long expectedByArray,
                long expectedByArrayAndValue)
            {
                string[] fields = new string[] {"Array", "Value", "cnt"};
                env.SendEventBean(new SupportEventWithIntArray(id, array, value));
                EventBean[] @out = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertProps(@out[0], fields, new object[] {array, value, expectedByArrayAndValue});
                EPAssertionUtil.AssertProps(@out[1], fields, new object[] {array, null, expectedByArray});
                EPAssertionUtil.AssertProps(@out[2], fields, new object[] {null, null, expectedTotal});
            }
        }

        internal class ResultSetQueryTypeOutputWhenTerminated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                foreach (var outputLimitOpt in EnumHelper.GetValues<SupportOutputLimitOpt>()) {
                    TryAssertionOutputWhenTerminated(env, "last", outputLimitOpt, milestone);
                }

                TryAssertionOutputWhenTerminated(env, "all", SupportOutputLimitOpt.DEFAULT, milestone);
                TryAssertionOutputWhenTerminated(env, "snapshot", SupportOutputLimitOpt.DEFAULT, milestone);
            }
        }

        internal class ResultSetQueryTypeGroupByWithComputation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select LongPrimitive as c0, sum(IntPrimitive) as c1 " +
                          "from SupportBean group by rollup(case when LongPrimitive > 0 then 1 else 0 end)";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("c0"));
                var fields = new[] {"c0", "c1"};

                env.SendEventBean(MakeEvent("E1", 1, 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {10L, 1}, new object[] {null, 1}});

                env.SendEventBean(MakeEvent("E2", 2, 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {11L, 3}, new object[] {null, 3}});

                env.SendEventBean(MakeEvent("E3", 5, -10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {-10L, 5}, new object[] {null, 8}});

                env.SendEventBean(MakeEvent("E4", 6, -11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {-11L, 11}, new object[] {null, 14}});

                env.SendEventBean(MakeEvent("E5", 3, 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {12L, 6}, new object[] {null, 17}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeContextPartitionAlsoRollup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context SegmentedByString partition by TheString from SupportBean;\n" +
                          "@Name('s0') context SegmentedByString select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean group by rollup(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new[] {"c0", "c1", "c2"};
                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 1, 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L}
                    });

                env.SendEventBean(MakeEvent("E1", 2, 20));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 2, 20L}, new object[] {"E1", null, 30L}, new object[] {null, null, 30L}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E2", 1, 25));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 1, 25L}, new object[] {"E2", null, 25L}, new object[] {null, null, 25L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean;\n" +
                          "@Name('s0') on SupportBean_S0 as S0 select mw.TheString as c0, sum(mw.IntPrimitive) as c1, count(*) as c2 from MyWindow mw group by rollup(mw.TheString);\n";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new[] {"c0", "c1", "c2"};

                // {E0, 0}, new object[] {E1, 1}, new object[] {E2, 2}, new object[] {E0, 3}, new object[] {E1, 4}, new object[] {E2, 5}, new object[] {E0, 6}, new object[] {E1, 7}, new object[] {E2, 8}, new object[] {E0, 9}
                for (var i = 0; i < 10; i++) {
                    var theString = "E" + i % 3;
                    env.SendEventBean(new SupportBean(theString, i));
                }

                env.SendEventBean(new SupportBean_S0(1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E0", 18, 4L}, new object[] {"E1", 12, 3L}, new object[] {"E2", 15, 3L},
                        new object[] {null, 18 + 12 + 15, 10L}
                    });

                env.SendEventBean(new SupportBean("E1", 6));
                env.SendEventBean(new SupportBean_S0(2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E0", 18, 4L}, new object[] {"E1", 12 + 6, 4L}, new object[] {"E2", 15, 3L},
                        new object[] {null, 18 + 12 + 15 + 6, 11L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundRollupUnenclosed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionUnboundRollupUnenclosed(env, "TheString, rollup(IntPrimitive, LongPrimitive)", milestone);
                TryAssertionUnboundRollupUnenclosed(
                    env,
                    "grouping sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "TheString)",
                    milestone);
                TryAssertionUnboundRollupUnenclosed(
                    env,
                    "TheString, grouping sets(" +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive), ())",
                    milestone);
            }
        }

        internal class ResultSetQueryTypeUnboundCubeUnenclosed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionUnboundCubeUnenclosed(env, "TheString, cube(IntPrimitive, LongPrimitive)", milestone);
                TryAssertionUnboundCubeUnenclosed(
                    env,
                    "grouping sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "(TheString, LongPrimitive)," +
                    "TheString)",
                    milestone);
                TryAssertionUnboundCubeUnenclosed(
                    env,
                    "TheString, grouping sets(" +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive)," +
                    "(LongPrimitive)," +
                    "())",
                    milestone);
            }

            private static void TryAssertionUnboundCubeUnenclosed(
                RegressionEnvironment env,
                string groupBy,
                AtomicLong milestone)
            {
                var fields = new[] {"c0", "c1", "c2", "c3"};
                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                          "group by " +
                          groupBy;
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 10, 100, 1000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d},
                        new object[] {"E1", null, 100L, 1000d}, new object[] {"E1", null, null, 1000d}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E1", 10, 200, 2000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d},
                        new object[] {"E1", null, 200L, 2000d}, new object[] {"E1", null, null, 3000d}
                    });

                env.SendEventBean(MakeEvent("E1", 20, 100, 4000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 20, 100L, 4000d}, new object[] {"E1", 20, null, 4000d},
                        new object[] {"E1", null, 100L, 5000d}, new object[] {"E1", null, null, 7000d}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E2", 10, 100, 5000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 10, 100L, 5000d}, new object[] {"E2", 10, null, 5000d},
                        new object[] {"E2", null, 100L, 5000d}, new object[] {"E2", null, null, 5000d}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundGroupingSet2LevelUnenclosed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionUnboundGroupingSet2LevelUnenclosed(
                    env,
                    "TheString, grouping sets(IntPrimitive, LongPrimitive)");
                TryAssertionUnboundGroupingSet2LevelUnenclosed(
                    env,
                    "grouping sets((TheString, IntPrimitive), (TheString, LongPrimitive))");
            }

            private static void TryAssertionUnboundGroupingSet2LevelUnenclosed(
                RegressionEnvironment env,
                string groupBy)
            {
                var fields = new[] {"c0", "c1", "c2", "c3"};
                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
                          "group by " +
                          groupBy;
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("c2"));

                env.SendEventBean(MakeEvent("E1", 10, 100, 1000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, 100L, 1000d}});

                env.SendEventBean(MakeEvent("E1", 20, 200, 2000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 20, null, 2000d}, new object[] {"E1", null, 200L, 2000d}});

                env.SendEventBean(MakeEvent("E1", 10, 200, 3000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 10, null, 4000d}, new object[] {"E1", null, 200L, 5000d}});

                env.SendEventBean(MakeEvent("E1", 20, 100, 4000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 20, null, 6000d}, new object[] {"E1", null, 100L, 5000d}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeBoundGroupingSet2LevelNoTopNoDetail : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length(4) " +
                          "group by grouping sets(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1", null, 100L}, new object[] {null, 10, 100L}},
                    new[] {new object[] {"E1", null, null}, new object[] {null, 10, null}});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 20, 200));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2", null, 200L}, new object[] {null, 20, 200L}},
                    new[] {new object[] {"E2", null, null}, new object[] {null, 20, null}});

                env.SendEventBean(MakeEvent("E1", 20, 300));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1", null, 400L}, new object[] {null, 20, 500L}},
                    new[] {new object[] {"E1", null, 100L}, new object[] {null, 20, 200L}});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E2", 10, 400));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2", null, 600L}, new object[] {null, 10, 500L}},
                    new[] {new object[] {"E2", null, 200L}, new object[] {null, 10, 100L}});

                env.SendEventBean(MakeEvent("E2", 20, 500)); // removes E1/10/100
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", null, 1100L}, new object[] {"E1", null, 300L},
                        new object[] {null, 20, 1000L}, new object[] {null, 10, 400L}
                    },
                    new[] {
                        new object[] {"E2", null, 600L}, new object[] {"E1", null, 400L}, new object[] {null, 20, 500L},
                        new object[] {null, 10, 500L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeBoundGroupingSet2LevelTopAndDetail : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length(4) " +
                          "group by grouping sets((), (TheString, IntPrimitive))";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}},
                    new[] {new object[] {null, null, null}, new object[] {"E1", 10, null}});

                env.SendEventBean(MakeEvent("E1", 10, 200));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {null, null, 300L}, new object[] {"E1", 10, 300L}},
                    new[] {new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}});

                env.SendEventBean(MakeEvent("E2", 20, 300));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {null, null, 600L}, new object[] {"E2", 20, 300L}},
                    new[] {new object[] {null, null, 300L}, new object[] {"E2", 20, null}});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E1", 10, 400));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {null, null, 1000L}, new object[] {"E1", 10, 700L}},
                    new[] {new object[] {null, null, 600L}, new object[] {"E1", 10, 300L}});

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundCube4Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2", "c3", "c4"};
                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, DoublePrimitive as c3, sum(IntBoxed) as c4 from SupportBean " +
                          "group by cube(TheString, IntPrimitive, LongPrimitive, DoublePrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));
                Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("c2"));
                Assert.AreEqual(typeof(double?), env.Statement("s0").EventType.GetPropertyType("c3"));

                env.SendEventBean(MakeEvent("E1", 1, 10, 100, 1000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 1, 10L, 100d, 1000}, // {0, 1, 2, 3}
                        new object[] {"E1", 1, 10L, null, 1000}, // {0, 1, 2}
                        new object[] {"E1", 1, null, 100d, 1000}, // {0, 1, 3}
                        new object[] {"E1", 1, null, null, 1000}, // {0, 1}
                        new object[] {"E1", null, 10L, 100d, 1000}, // {0, 2, 3}
                        new object[] {"E1", null, 10L, null, 1000}, // {0, 2}
                        new object[] {"E1", null, null, 100d, 1000}, // {0, 3}
                        new object[] {"E1", null, null, null, 1000}, // {0}
                        new object[] {null, 1, 10L, 100d, 1000}, // {1, 2, 3}
                        new object[] {null, 1, 10L, null, 1000}, // {1, 2}
                        new object[] {null, 1, null, 100d, 1000}, // {1, 3}
                        new object[] {null, 1, null, null, 1000}, // {1}
                        new object[] {null, null, 10L, 100d, 1000}, // {2, 3}
                        new object[] {null, null, 10L, null, 1000}, // {2}
                        new object[] {null, null, null, 100d, 1000}, // {3}
                        new object[] {null, null, null, null, 1000} // {}
                    });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 1, 20, 100, 2000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 1, 20L, 100d, 2000}, // {0, 1, 2, 3}
                        new object[] {"E2", 1, 20L, null, 2000}, // {0, 1, 2}
                        new object[] {"E2", 1, null, 100d, 2000}, // {0, 1, 3}
                        new object[] {"E2", 1, null, null, 2000}, // {0, 1}
                        new object[] {"E2", null, 20L, 100d, 2000}, // {0, 2, 3}
                        new object[] {"E2", null, 20L, null, 2000}, // {0, 2}
                        new object[] {"E2", null, null, 100d, 2000}, // {0, 3}
                        new object[] {"E2", null, null, null, 2000}, // {0}
                        new object[] {null, 1, 20L, 100d, 2000}, // {1, 2, 3}
                        new object[] {null, 1, 20L, null, 2000}, // {1, 2}
                        new object[] {null, 1, null, 100d, 3000}, // {1, 3}
                        new object[] {null, 1, null, null, 3000}, // {1}
                        new object[] {null, null, 20L, 100d, 2000}, // {2, 3}
                        new object[] {null, null, 20L, null, 2000}, // {2}
                        new object[] {null, null, null, 100d, 3000}, // {3}
                        new object[] {null, null, null, null, 3000} // {}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E1", 2, 10, 100, 4000));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 2, 10L, 100d, 4000}, // {0, 1, 2, 3}
                        new object[] {"E1", 2, 10L, null, 4000}, // {0, 1, 2}
                        new object[] {"E1", 2, null, 100d, 4000}, // {0, 1, 3}
                        new object[] {"E1", 2, null, null, 4000}, // {0, 1}
                        new object[] {"E1", null, 10L, 100d, 5000}, // {0, 2, 3}
                        new object[] {"E1", null, 10L, null, 5000}, // {0, 2}
                        new object[] {"E1", null, null, 100d, 5000}, // {0, 3}
                        new object[] {"E1", null, null, null, 5000}, // {0}
                        new object[] {null, 2, 10L, 100d, 4000}, // {1, 2, 3}
                        new object[] {null, 2, 10L, null, 4000}, // {1, 2}
                        new object[] {null, 2, null, 100d, 4000}, // {1, 3}
                        new object[] {null, 2, null, null, 4000}, // {1}
                        new object[] {null, null, 10L, 100d, 5000}, // {2, 3}
                        new object[] {null, null, 10L, null, 5000}, // {2}
                        new object[] {null, null, null, 100d, 7000}, // {3}
                        new object[] {null, null, null, null, 7000} // {}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeBoundCube3Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionBoundCube(env, "cube(TheString, IntPrimitive, LongPrimitive)", milestone);
                TryAssertionBoundCube(
                    env,
                    "grouping sets(" +
                    "(TheString, IntPrimitive, LongPrimitive)," +
                    "(TheString, IntPrimitive)," +
                    "(TheString, LongPrimitive)," +
                    "(TheString)," +
                    "(IntPrimitive, LongPrimitive)," +
                    "(IntPrimitive)," +
                    "(LongPrimitive)," +
                    "()" +
                    ")",
                    milestone);
            }
        }

        internal class ResultSetQueryTypeNamedWindowCube2Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionNamedWindowCube2Dim(env, "cube(TheString, IntPrimitive)");
                TryAssertionNamedWindowCube2Dim(
                    env,
                    "grouping sets(" +
                    "(TheString, IntPrimitive)," +
                    "(TheString)," +
                    "(IntPrimitive)," +
                    "()" +
                    ")");
            }

            private static void TryAssertionNamedWindowCube2Dim(
                RegressionEnvironment env,
                string groupBy)
            {
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean(IntBoxed = 0);\n" +
                          "on SupportBean(IntBoxed = 3) delete from MyWindow;\n" +
                          "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
                          "group by " +
                          groupBy;
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new[] {"c0", "c1", "c2"};

                env.SendEventBean(MakeEvent(0, "E1", 10, 100)); // insert event
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, 10, 100L},
                        new object[] {null, null, 100L}
                    },
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, 10, null},
                        new object[] {null, null, null}
                    });

                env.Milestone(0);

                env.SendEventBean(MakeEvent(0, "E1", 11, 200)); // insert event
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 11, 200L}, new object[] {"E1", null, 300L}, new object[] {null, 11, 200L},
                        new object[] {null, null, 300L}
                    },
                    new[] {
                        new object[] {"E1", 11, null}, new object[] {"E1", null, 100L}, new object[] {null, 11, null},
                        new object[] {null, null, 100L}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 10, 300)); // insert event
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 600L}, new object[] {null, 10, 400L},
                        new object[] {null, null, 600L}
                    },
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 300L}, new object[] {null, 10, 100L},
                        new object[] {null, null, 300L}
                    });

                env.SendEventBean(MakeEvent(0, "E2", 11, 400)); // insert event
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 11, 400L}, new object[] {"E2", null, 400L}, new object[] {null, 11, 600L},
                        new object[] {null, null, 1000L}
                    },
                    new[] {
                        new object[] {"E2", 11, null}, new object[] {"E2", null, null}, new object[] {null, 11, 200L},
                        new object[] {null, null, 600L}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent(3, null, -1, -1)); // delete-all
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", 11, null}, new object[] {"E2", 11, null},
                        new object[] {"E1", null, null}, new object[] {"E2", null, null}, new object[] {null, 10, null},
                        new object[] {null, 11, null}, new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", 11, 200L}, new object[] {"E2", 11, 400L},
                        new object[] {"E1", null, 600L}, new object[] {"E2", null, 400L}, new object[] {null, 10, 400L},
                        new object[] {null, 11, 600L}, new object[] {null, null, 1000L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeNamedWindowDeleteAndRStream2Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionNamedWindowDeleteAndRStream2Dim(env, "rollup(TheString, IntPrimitive)", milestone);
                TryAssertionNamedWindowDeleteAndRStream2Dim(
                    env,
                    "grouping sets(" +
                    "(TheString, IntPrimitive)," +
                    "(TheString)," +
                    "())",
                    milestone);
            }

            private static void TryAssertionNamedWindowDeleteAndRStream2Dim(
                RegressionEnvironment env,
                string groupBy,
                AtomicLong milestone)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "create window MyWindow#keepall as SupportBean;\n" +
                          "insert into MyWindow select * from SupportBean(IntBoxed = 0);\n" +
                          "on SupportBean(IntBoxed = 1) as sb " +
                          "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive;\n" +
                          "on SupportBean(IntBoxed = 2) as sb " +
                          "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive and sb.LongPrimitive = mw.LongPrimitive;\n" +
                          "on SupportBean(IntBoxed = 3) delete from MyWindow;\n" +
                          "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
                          "group by " +
                          groupBy;
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent(0, "E1", 10, 100)); // insert event IntBoxed=0
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}
                    },
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(1, "E1", 10, 100)); // delete (IntBoxed = 1)
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 10, 200)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 200L}, new object[] {"E1", null, 200L}, new object[] {null, null, 200L}
                    },
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(0, "E2", 20, 300)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 300L}, new object[] {"E2", null, 300L}, new object[] {null, null, 500L}
                    },
                    new[] {
                        new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 200L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(3, null, 0, 0)); // delete all
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E2", 20, null},
                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
                        new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 200L}, new object[] {"E2", 20, 300L},
                        new object[] {"E1", null, 200L}, new object[] {"E2", null, 300L},
                        new object[] {null, null, 500L}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 10, 400)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}
                    },
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 20, 500)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 20, 500L}, new object[] {"E1", null, 900L}, new object[] {null, null, 900L}
                    },
                    new[] {
                        new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}
                    });

                env.SendEventBean(MakeEvent(0, "E2", 20, 600)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1500L}
                    },
                    new[] {
                        new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 900L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(0, "E1", 10, 700)); // insert
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 1100L}, new object[] {"E1", null, 1600L},
                        new object[] {null, null, 2200L}
                    },
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}
                    });

                env.SendEventBean(MakeEvent(3, null, 0, 0)); // delete all
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", 20, null}, new object[] {"E2", 20, null},
                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
                        new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 1100L}, new object[] {"E1", 20, 500L}, new object[] {"E2", 20, 600L},
                        new object[] {"E1", null, 1600L}, new object[] {"E2", null, 600L},
                        new object[] {null, null, 2200L}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 10, 100)); // insert
                env.SendEventBean(MakeEvent(0, "E1", 20, 200)); // insert
                env.SendEventBean(MakeEvent(0, "E1", 10, 300)); // insert
                env.SendEventBean(MakeEvent(0, "E1", 20, 400)); // insert
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEvent(1, "E1", 20, -1)); // delete (IntBoxed = 1)
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}
                    },
                    new[] {
                        new object[] {"E1", 20, 600L}, new object[] {"E1", null, 1000L},
                        new object[] {null, null, 1000L}
                    });

                env.SendEventBean(MakeEvent(1, "E1", 10, -1)); // delete (IntBoxed = 1)
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}
                    });

                env.SendEventBean(MakeEvent(0, "E1", 10, 100)); // insert
                env.SendEventBean(MakeEvent(0, "E1", 10, 200)); // insert

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(0, "E1", 10, 300)); // insert
                env.SendEventBean(MakeEvent(0, "E1", 20, 400)); // insert
                env.SendEventBean(MakeEvent(0, "E2", 20, 500)); // insert
                env.Listener("s0").Reset();

                env.SendEventBean(MakeEvent(2, "E1", 10, 200)); // delete specific (IntBoxed = 2)
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}
                    },
                    new[] {
                        new object[] {"E1", 10, 600L}, new object[] {"E1", null, 1000L},
                        new object[] {null, null, 1500L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(2, "E1", 10, 300)); // delete specific
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}
                    },
                    new[] {
                        new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}
                    });

                env.SendEventBean(MakeEvent(2, "E1", 20, 400)); // delete specific
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 20, null}, new object[] {"E1", null, 100L}, new object[] {null, null, 600L}
                    },
                    new[] {
                        new object[] {"E1", 20, 400L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent(2, "E2", 20, 500)); // delete specific
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 100L}
                    },
                    new[] {
                        new object[] {"E2", 20, 500L}, new object[] {"E2", null, 500L}, new object[] {null, null, 600L}
                    });

                env.SendEventBean(MakeEvent(2, "E1", 10, 100)); // delete specific
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}
                    },
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeBoundRollup2Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionBoundRollup2Dim(env, false, milestone);
                TryAssertionBoundRollup2Dim(env, true, milestone);
            }

            private static void TryAssertionBoundRollup2Dim(
                RegressionEnvironment env,
                bool join,
                AtomicLong milestone)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
                          "from SupportBean#length(3) " +
                          (join ? ", SupportBean_S0#lastevent " : "") +
                          "group by rollup(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E2", 20, 200));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 300));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(
                    MakeEvent("E2", 20, 400)); // expires {TheString="E1", IntPrimitive=10, LongPrimitive=100}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 600L}, new object[] {"E1", 10, null},
                        new object[] {"E2", null, 600L}, new object[] {"E1", null, 300L},
                        new object[] {null, null, 900L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(
                    MakeEvent("E2", 20, 500)); // expires {TheString="E2", IntPrimitive=20, LongPrimitive=200}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 900L},
                        new object[] {"E2", null, 900L},
                        new object[] {null, null, 1200L}
                    });

                env.SendEventBean(
                    MakeEvent("E2", 21, 600)); // expires {TheString="E1", IntPrimitive=11, LongPrimitive=300}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 21, 600L}, new object[] {"E1", 11, null},
                        new object[] {"E2", null, 1500L}, new object[] {"E1", null, null},
                        new object[] {null, null, 1500L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(
                    MakeEvent("E2", 21, 700)); // expires {TheString="E2", IntPrimitive=20, LongPrimitive=400}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 21, 1300L}, new object[] {"E2", 20, 500L},
                        new object[] {"E2", null, 1800L},
                        new object[] {null, null, 1800L}
                    });

                env.SendEventBean(
                    MakeEvent("E2", 21, 800)); // expires {TheString="E2", IntPrimitive=20, LongPrimitive=500}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 21, 2100L}, new object[] {"E2", 20, null},
                        new object[] {"E2", null, 2100L},
                        new object[] {null, null, 2100L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(
                    MakeEvent("E1", 10, 900)); // expires {TheString="E2", IntPrimitive=21, LongPrimitive=600}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 900L}, new object[] {"E2", 21, 1500L},
                        new object[] {"E1", null, 900L}, new object[] {"E2", null, 1500L},
                        new object[] {null, null, 2400L}
                    });

                env.SendEventBean(
                    MakeEvent("E1", 11, 1000)); // expires {TheString="E2", IntPrimitive=21, LongPrimitive=700}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 11, 1000L}, new object[] {"E2", 21, 800L},
                        new object[] {"E1", null, 1900L}, new object[] {"E2", null, 800L},
                        new object[] {null, null, 2700L}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(
                    MakeEvent("E2", 20, 1100)); // expires {TheString="E2", IntPrimitive=21, LongPrimitive=800}
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 1100L}, new object[] {"E2", 21, null},
                        new object[] {"E2", null, 1100L},
                        new object[] {null, null, 3000L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundRollup2Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};
                var epl = "@Name('s0')" +
                          "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean " +
                          "group by rollup(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("c1"));

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}
                    });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 20, 200));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 300));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E2", 20, 400));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1000L}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 500));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 11, 800L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}
                    });

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E1", 10, 600));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 700L}, new object[] {"E1", null, 1500L},
                        new object[] {null, null, 2100L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundRollup1Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionUnboundRollup1Dim(env, "rollup(TheString)", milestone);
                TryAssertionUnboundRollup1Dim(env, "cube(TheString)", milestone);
            }
        }

        internal class ResultSetQueryTypeUnboundRollup2DimBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};

                var epl = "@Name('s0')" +
                          "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean#length_batch(4) " +
                          "group by rollup(TheString, IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 10, 100));
                env.SendEventBean(MakeEvent("E2", 20, 200));
                env.SendEventBean(MakeEvent("E1", 11, 300));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 20, 400));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 11, 300L},
                        new object[] {"E1", null, 400L}, new object[] {"E2", null, 600L},
                        new object[] {null, null, 1000L}
                    },
                    new[] {
                        new object[] {"E1", 10, null}, new object[] {"E2", 20, null}, new object[] {"E1", 11, null},
                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
                        new object[] {null, null, null}
                    });

                env.SendEventBean(MakeEvent("E1", 11, 500));
                env.SendEventBean(MakeEvent("E2", 20, 600));
                env.SendEventBean(MakeEvent("E1", 11, 700));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E2", 20, 800));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").DataListsFlattened,
                    fields,
                    new[] {
                        new object[] {"E1", 11, 1200L}, new object[] {"E2", 20, 1400L}, new object[] {"E1", 10, null},
                        new object[] {"E1", null, 1200L}, new object[] {"E2", null, 1400L},
                        new object[] {null, null, 2600L}
                    },
                    new[] {
                        new object[] {"E1", 11, 300L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 10, 100L},
                        new object[] {"E1", null, 400L}, new object[] {"E2", null, 600L},
                        new object[] {null, null, 1000L}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeUnboundRollup3Dim : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var rollupEpl = "rollup(TheString, IntPrimitive, LongPrimitive)";
                TryAssertionUnboundRollup3Dim(env, rollupEpl, false, milestone);
                TryAssertionUnboundRollup3Dim(env, rollupEpl, true, milestone);

                var gsEpl = "grouping sets(" +
                            "(TheString, IntPrimitive, LongPrimitive)," +
                            "(TheString, IntPrimitive)," +
                            "(TheString)," +
                            "()" +
                            ")";
                TryAssertionUnboundRollup3Dim(env, gsEpl, false, milestone);
                TryAssertionUnboundRollup3Dim(env, gsEpl, true, milestone);
            }
        }

        internal class ResultSetQueryTypeMixedAccessAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2"};

                var epl = "@Name('s0') select sum(IntPrimitive) as c0, TheString as c1, window(*) as c2 " +
                          "from SupportBean#length(2) sb group by rollup(TheString) order by TheString";
                env.CompileDeploy(epl).AddListener("s0");

                object eventOne = new SupportBean("E1", 1);
                env.SendEventBean(eventOne);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {
                            1, null,
                            new[] {eventOne}
                        },
                        new object[] {
                            1, "E1",
                            new[] {eventOne}
                        }
                    });

                object eventTwo = new SupportBean("E1", 2);
                env.SendEventBean(eventTwo);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {
                            3, null,
                            new[] {eventOne, eventTwo}
                        },
                        new object[] {
                            3, "E1",
                            new[] {eventOne, eventTwo}
                        }
                    });

                env.Milestone(0);

                object eventThree = new SupportBean("E2", 3);
                env.SendEventBean(eventThree);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {
                            5, null,
                            new[] {eventTwo, eventThree}
                        },
                        new object[] {
                            2, "E1",
                            new[] {eventTwo}
                        },
                        new object[] {
                            3, "E2",
                            new[] {eventThree}
                        }
                    });

                object eventFour = new SupportBean("E1", 4);
                env.SendEventBean(eventFour);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {
                            7, null,
                            new[] {eventThree, eventFour}
                        },
                        new object[] {
                            4, "E1",
                            new[] {eventFour}
                        }
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeNonBoxedTypeWithRollup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtOne = env.CompileDeploy(
                        "@Name('s0') select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                        "from SupportBean group by IntPrimitive, rollup(DoublePrimitive, LongPrimitive)")
                    .Statement("s0");
                AssertTypesC0C1C2(stmtOne, typeof(int?), typeof(double?), typeof(long?));

                var stmtTwo = env.CompileDeploy(
                        "@Name('s1') select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                        "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive))")
                    .Statement("s1");
                AssertTypesC0C1C2(stmtTwo, typeof(int?), typeof(double?), typeof(long?));

                var stmtThree = env.CompileDeploy(
                        "@Name('s2') select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                        "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive), (IntPrimitive, DoublePrimitive))")
                    .Statement("s2");
                AssertTypesC0C1C2(stmtThree, typeof(int?), typeof(double?), typeof(long?));

                var stmtFour = env.CompileDeploy(
                        "@Name('s3') select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(ShortPrimitive) " +
                        "from SupportBean group by grouping sets ((DoublePrimitive, IntPrimitive), (LongPrimitive, IntPrimitive))")
                    .Statement("s3");
                AssertTypesC0C1C2(stmtFour, typeof(int?), typeof(double?), typeof(long?));

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var prefix = "select TheString, sum(IntPrimitive) from SupportBean group by ";

                // invalid rollup expressions
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup()",
                    "Incorrect syntax near ')' at line 1 column 69, please check the group-by clause [select TheString, sum(IntPrimitive) from SupportBean group by rollup()]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup(TheString, TheString)",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by rollup(TheString, TheString)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup(x)",
                    "Failed to validate group-by-clause expression 'x': Property named 'x' is not valid in any stream [select TheString, sum(IntPrimitive) from SupportBean group by rollup(x)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup(LongPrimitive)",
                    "Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'TheString' [select TheString, sum(IntPrimitive) from SupportBean group by rollup(LongPrimitive)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup((TheString, LongPrimitive), (TheString, LongPrimitive))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by rollup((TheString, LongPrimitive), (TheString, LongPrimitive))]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "rollup((TheString, LongPrimitive), (LongPrimitive, TheString))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by rollup((TheString, LongPrimitive), (LongPrimitive, TheString))]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "grouping sets((TheString, TheString))",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets((TheString, TheString))]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "grouping sets(TheString, TheString)",
                    "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets(TheString, TheString)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "grouping sets((), ())",
                    "Failed to validate the group-by clause, found duplicate specification of the overall grouping '()' [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets((), ())]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    prefix + "grouping sets(())",
                    "Failed to validate the group-by clause, the overall grouping '()' cannot be the only grouping [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets(())]");

                // invalid select clause for this type of query
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean group by grouping sets(TheString)",
                    "Group-by with rollup requires that the select-clause does not use wildcard [select * from SupportBean group by grouping sets(TheString)]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select sb.* from SupportBean sb group by grouping sets(TheString)",
                    "Group-by with rollup requires that the select-clause does not use wildcard [select sb.* from SupportBean sb group by grouping sets(TheString)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping sets(TheString)",
                    "Reclaim hints are not available with rollup [@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping sets(TheString)]");
            }
        }
    }
} // end of namespace