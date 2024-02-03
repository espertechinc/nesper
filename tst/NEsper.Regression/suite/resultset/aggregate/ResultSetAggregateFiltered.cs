///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateFiltered
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithBlackWhitePercent(execs);
            WithCountVariations(execs);
            WithAllAggFunctions(execs);
            WithFirstLastEver(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstLastEver(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateFirstLastEver());
            return execs;
        }

        public static IList<RegressionExecution> WithAllAggFunctions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateAllAggFunctions());
            return execs;
        }

        public static IList<RegressionExecution> WithCountVariations(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateCountVariations());
            return execs;
        }

        public static IList<RegressionExecution> WithBlackWhitePercent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateBlackWhitePercent());
            return execs;
        }

        private class ResultSetAggregateBlackWhitePercent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "cb,cnb,c,pct".SplitCsv();
                var epl =
                    "@name('s0') select count(*,BoolPrimitive) as cb, count(*,not BoolPrimitive) as cnb, count(*) as c, count(*,BoolPrimitive)/count(*) as pct from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");
                SupportAdminUtil.AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(MakeSB(true));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 0L, 1L, 1d });

                env.Milestone(0);

                env.SendEventBean(MakeSB(false));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 1L, 2L, 0.5d });

                env.SendEventBean(MakeSB(false));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 2L, 3L, 1 / 3d });

                env.Milestone(1);

                env.SendEventBean(MakeSB(false));
                env.AssertPropsNew("s0", fields, new object[] { 0L, 3L, 3L, 0d });

                env.UndeployAll();

                env.EplToModelCompileDeploy(epl);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateCountVariations : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "count(IntBoxed, BoolPrimitive) as c1," +
                          "count(distinct IntBoxed, BoolPrimitive) as c2 " +
                          "from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeBean(100, true));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 1L });

                env.Milestone(0);

                env.SendEventBean(MakeBean(100, true));
                env.AssertPropsNew("s0", fields, new object[] { 2L, 1L });

                env.SendEventBean(MakeBean(101, false));
                env.AssertPropsNew("s0", fields, new object[] { 2L, 1L });

                env.SendEventBean(MakeBean(102, true));
                env.AssertPropsNew("s0", fields, new object[] { 2L, 2L });

                env.SendEventBean(MakeBean(103, false));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 1L });

                env.SendEventBean(MakeBean(104, false));
                env.AssertPropsNew("s0", fields, new object[] { 1L, 1L });

                env.SendEventBean(MakeBean(105, false));
                env.AssertPropsNew("s0", fields, new object[] { 0L, 0L });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateAllAggFunctions : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string[] fields;
                string epl;

                fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum,cfmaxever,cfminever".SplitCsv();
                epl = "@name('s0') select " +
                      "avedev(IntBoxed, BoolPrimitive) as cavedev," +
                      "avg(IntBoxed, BoolPrimitive) as cavg, " +
                      "fmax(IntBoxed, BoolPrimitive) as cmax, " +
                      "median(IntBoxed, BoolPrimitive) as cmedian, " +
                      "fmin(IntBoxed, BoolPrimitive) as cmin, " +
                      "stddev(IntBoxed, BoolPrimitive) as cstddev, " +
                      "sum(IntBoxed, BoolPrimitive) as csum," +
                      "fmaxever(IntBoxed, BoolPrimitive) as cfmaxever, " +
                      "fminever(IntBoxed, BoolPrimitive) as cfminever " +
                      "from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeBean(100, false));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, null, null, null });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(10, true));
                env.AssertPropsNew("s0", fields, new object[] { 0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10 });

                env.SendEventBean(MakeBean(11, false));
                env.AssertPropsNew("s0", fields, new object[] { 0.0d, 10.0, 10, 10.0, 10, null, 10, 10, 10 });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(20, true));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 5.0d, 15.0, 20, 15.0, 10, 7.0710678118654755, 30, 20, 10 });

                env.SendEventBean(MakeBean(30, true));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 5.0d, 25.0, 30, 25.0, 20, 7.0710678118654755, 50, 30, 10 });

                // Test all remaining types of "sum"
                env.UndeployAll();

                fields = "c1,c2,c3,c4".SplitCsv();
                epl = "@name('s0') select " +
                      "sum(FloatPrimitive, BoolPrimitive) as c1," +
                      "sum(DoublePrimitive, BoolPrimitive) as c2, " +
                      "sum(LongPrimitive, BoolPrimitive) as c3, " +
                      "sum(ShortPrimitive, BoolPrimitive) as c4 " +
                      "from SupportBean#length(2)";
                env.CompileDeploy(epl).AddListener("s0");
                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(2f, 3d, 4L, 5, false));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null });

                env.SendEventBean(MakeBean(3f, 4d, 5L, 6, true));
                env.AssertPropsNew("s0", fields, new object[] { 3f, 4d, 5L, 6 });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(4f, 5d, 6L, 7, true));
                env.AssertPropsNew("s0", fields, new object[] { 7f, 9d, 11L, 13 });

                env.SendEventBean(MakeBean(1f, 1d, 1L, 1, true));
                env.AssertPropsNew("s0", fields, new object[] { 5f, 6d, 7L, 8 });

                // Test min/max-ever
                env.UndeployAll();
                fields = "c1,c2".SplitCsv();
                epl = "@name('s0') select " +
                      "fmax(IntBoxed, BoolPrimitive) as c1," +
                      "fmin(IntBoxed, BoolPrimitive) as c2 " +
                      "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                SupportAdminUtil.AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(MakeBean(10, true));
                env.AssertPropsNew("s0", fields, new object[] { 10, 10 });

                env.SendEventBean(MakeBean(20, true));
                env.AssertPropsNew("s0", fields, new object[] { 20, 10 });

                env.SendEventBean(MakeBean(8, false));
                env.AssertPropsNew("s0", fields, new object[] { 20, 10 });

                env.SendEventBean(MakeBean(7, true));
                env.AssertPropsNew("s0", fields, new object[] { 20, 7 });

                env.SendEventBean(MakeBean(30, false));
                env.AssertPropsNew("s0", fields, new object[] { 20, 7 });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(40, true));
                env.AssertPropsNew("s0", fields, new object[] { 40, 7 });

                // test big decimal big integer
                env.UndeployAll();
                fields = "c1,c2,c3".SplitCsv();
                epl = "@name('s0') select " +
                      "avg(DecimalOne, Bigint < 100) as c1," +
                      "sum(DecimalOne, Bigint < 100) as c2, " +
                      "sum(Bigint, Bigint < 100) as c3 " +
                      "from SupportBeanNumeric#length(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(10), 20m));
                env.AssertPropsNew("s0", fields, new object[] { 20m, 20m, new BigInteger(10) });

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(101), 101m));
                env.AssertPropsNew("s0", fields, new object[] { 20m, 20m, new BigInteger(10) });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(20), 40m));
                env.AssertPropsNew("s0", fields, new object[] { 40m, 40m, new BigInteger(20) });

                env.SendEventBean(new SupportBeanNumeric(new BigInteger(30), 50m));
                env.AssertPropsNew("s0", fields, new object[] { 45m, 90m, new BigInteger(50) });

                env.UndeployAll();
                epl = "@name('s0') select " +
                      "avedev(distinct IntBoxed,BoolPrimitive) as cavedev, " +
                      "avg(distinct IntBoxed,BoolPrimitive) as cavg, " +
                      "fmax(distinct IntBoxed,BoolPrimitive) as cmax, " +
                      "median(distinct IntBoxed,BoolPrimitive) as cmedian, " +
                      "fmin(distinct IntBoxed,BoolPrimitive) as cmin, " +
                      "stddev(distinct IntBoxed,BoolPrimitive) as cstddev, " +
                      "sum(distinct IntBoxed,BoolPrimitive) as csum " +
                      "from SupportBean#length(3)";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDistinct(env, milestone);

                // test SODA
                env.UndeployAll();

                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionDistinct(env, milestone);

                env.UndeployAll();
            }

            private void TryAssertionDistinct(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var fields = "cavedev,cavg,cmax,cmedian,cmin,cstddev,csum".SplitCsv();
                env.SendEventBean(MakeBean(100, true));
                env.AssertPropsNew("s0", fields, new object[] { 0d, 100d, 100, 100d, 100, null, 100 });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(100, true));
                env.AssertPropsNew("s0", fields, new object[] { 0d, 100d, 100, 100d, 100, null, 100 });

                env.SendEventBean(MakeBean(200, true));
                env.AssertPropsNew("s0", fields, new object[] { 50d, 150d, 200, 150d, 100, 70.71067811865476, 300 });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(200, true));
                env.AssertPropsNew("s0", fields, new object[] { 50d, 150d, 200, 150d, 100, 70.71067811865476, 300 });

                env.SendEventBean(MakeBean(200, true));
                env.AssertPropsNew("s0", fields, new object[] { 0d, 200d, 200, 200d, 200, null, 200 });
            }
        }

        private class ResultSetAggregateFirstLastEver : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionFirstLastEver(env, true, milestone);
                TryAssertionFirstLastEver(env, false, milestone);
            }

            private void TryAssertionFirstLastEver(
                RegressionEnvironment env,
                bool soda,
                AtomicLong milestone)
            {
                var fields = "c1,c2,c3".SplitCsv();
                var epl = "@name('s0') select " +
                          "firstever(IntBoxed,BoolPrimitive) as c1, " +
                          "lastever(IntBoxed,BoolPrimitive) as c2, " +
                          "countever(*,BoolPrimitive) as c3 " +
                          "from SupportBean#length(3)";
                env.CompileDeploy(soda, epl).AddListener("s0");

                env.SendEventBean(MakeBean(100, false));
                env.AssertPropsNew("s0", fields, new object[] { null, null, 0L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(100, true));
                env.AssertPropsNew("s0", fields, new object[] { 100, 100, 1L });

                env.SendEventBean(MakeBean(200, true));
                env.AssertPropsNew("s0", fields, new object[] { 100, 200, 2L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean(201, false));
                env.AssertPropsNew("s0", fields, new object[] { 100, 200, 2L });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select count(*, IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'count(*,IntPrimitive)': Invalid filter expression parameter to the aggregation function 'count' is expected to return a boolean value but returns System.Nullable<System.Int32> [select count(*, IntPrimitive) from SupportBean]");

                env.TryInvalidCompile(
                    "select fmin(IntPrimitive) from SupportBean",
                    "Failed to validate select-clause expression 'min(IntPrimitive)': MIN-filtered aggregation function must have a filter expression as a second parameter [select fmin(IntPrimitive) from SupportBean]");
            }
        }

        private static SupportBean MakeBean(
            float floatPrimitive,
            double doublePrimitive,
            long longPrimitive,
            short shortPrimitive,
            bool boolPrimitive)
        {
            var sb = new SupportBean();
            sb.FloatPrimitive = floatPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            sb.LongPrimitive = longPrimitive;
            sb.ShortPrimitive = shortPrimitive;
            sb.BoolPrimitive = boolPrimitive;
            return sb;
        }

        private static SupportBean MakeBean(
            int? intBoxed,
            bool boolPrimitive)
        {
            var sb = new SupportBean();
            sb.IntBoxed = intBoxed;
            sb.BoolPrimitive = boolPrimitive;
            return sb;
        }

        private static SupportBean MakeSB(bool boolPrimitive)
        {
            var sb = new SupportBean("E", 0);
            sb.BoolPrimitive = boolPrimitive;
            return sb;
        }
    }
} // end of namespace