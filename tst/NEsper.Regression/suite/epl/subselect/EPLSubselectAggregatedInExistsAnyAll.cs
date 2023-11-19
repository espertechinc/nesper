///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectAggregatedInExistsAnyAll
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInSimple(execs);
            WithExistsSimple(execs);
            WithUngroupedWOHavingWRelOpAllAnySome(execs);
            WithUngroupedWOHavingWEqualsAllAnySome(execs);
            WithUngroupedWOHavingWIn(execs);
            WithUngroupedWOHavingWExists(execs);
            WithUngroupedWHavingWExists(execs);
            WithGroupedWOHavingWRelOpAllAnySome(execs);
            WithGroupedWOHavingWEqualsAllAnySome(execs);
            WithGroupedWOHavingWIn(execs);
            WithGroupedWHavingWIn(execs);
            WithGroupedWHavingWEqualsAllAnySome(execs);
            WithGroupedWHavingWRelOpAllAnySome(execs);
            WithUngroupedWHavingWIn(execs);
            WithUngroupedWHavingWRelOpAllAnySome(execs);
            WithUngroupedWHavingWEqualsAllAnySome(execs);
            WithGroupedWOHavingWExists(execs);
            WithGroupedWHavingWExists(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWHavingWExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWHavingWExists());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWOHavingWExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWOHavingWExists());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWHavingWEqualsAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWHavingWEqualsAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWHavingWRelOpAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWHavingWRelOpAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWHavingWIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWHavingWIn());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWHavingWRelOpAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWHavingWRelOpAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWHavingWEqualsAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWHavingWEqualsAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWHavingWIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWHavingWIn());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWOHavingWIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWOHavingWIn());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWOHavingWEqualsAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWOHavingWEqualsAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedWOHavingWRelOpAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectGroupedWOHavingWRelOpAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWHavingWExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWHavingWExists());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWOHavingWExists(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWOHavingWExists());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWOHavingWIn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWOHavingWIn());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWOHavingWEqualsAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWOHavingWEqualsAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithUngroupedWOHavingWRelOpAllAnySome(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectUngroupedWOHavingWRelOpAllAnySome());
            return execs;
        }

        public static IList<RegressionExecution> WithExistsSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectExistsSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithInSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInSimple());
            return execs;
        }

        private class EPLSubselectUngroupedWHavingWIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select Value in (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c0," +
                    "Value not in (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c1 " +
                    "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { null, null });

                env.SendEventBean(new SupportBean("E2", 0));
                SendVEAndAssert(env, fields, 10, new object[] { true, false });

                env.SendEventBean(new SupportBean("E3", 1));
                SendVEAndAssert(env, fields, 10, new object[] { false, true });

                env.SendEventBean(new SupportBean("E4", -1));
                SendVEAndAssert(env, fields, 10, new object[] { true, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWHavingWIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select Value in (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) != 'E1') as c0," +
                    "Value not in (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) != 'E1') as c1 " +
                    "from SupportValueEvent";
                env.CompileDeploy(epl).AddListener("s0");

                SendVEAndAssert(env, fields, 10, new object[] { false, true });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { false, true });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWOHavingWIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select Value in (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0," +
                    "Value not in (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1 " +
                    "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { false, true });

                env.SendEventBean(new SupportBean("E1", 19));
                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { false, true });
                SendVEAndAssert(env, fields, 11, new object[] { true, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWOHavingWIn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "@name('s0') select Value in (select sum(IntPrimitive) from SupportBean#keepall) as c0," +
                          "Value not in (select sum(IntPrimitive) from SupportBean#keepall) as c1 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, false });

                env.SendEventBean(new SupportBean("E2", 1));
                SendVEAndAssert(env, fields, 10, new object[] { false, true });

                env.SendEventBean(new SupportBean("E3", -1));
                SendVEAndAssert(env, fields, 10, new object[] { true, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWOHavingWRelOpAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value < all (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0, " +
                          "Value < any (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1, " +
                          "Value < some (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E1", 19));
                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E3", 9));
                SendVEAndAssert(env, fields, 10, new object[] { false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWHavingWRelOpAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value < all (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c0, " +
                          "Value < any (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c1, " +
                          "Value < some (select sum(IntPrimitive) from SupportBean#keepall group by TheString having last(TheString) not in ('E1', 'E3')) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E1", 19));
                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E3", 9));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E4", 9));
                SendVEAndAssert(env, fields, 10, new object[] { false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWOHavingWEqualsAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value = all (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c0, " +
                          "Value = any (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c1, " +
                          "Value = some (select sum(IntPrimitive) from SupportBean#keepall group by TheString) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWOHavingWEqualsAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value = all (select sum(IntPrimitive) from SupportBean#keepall) as c0, " +
                          "Value = any (select sum(IntPrimitive) from SupportBean#keepall) as c1, " +
                          "Value = some (select sum(IntPrimitive) from SupportBean#keepall) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { false, false, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWHavingWEqualsAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value = all (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c0, " +
                          "Value = any (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c1, " +
                          "Value = some (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) != 'E1') as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E2", 0));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E3", 1));
                SendVEAndAssert(env, fields, 10, new object[] { false, false, false });

                env.SendEventBean(new SupportBean("E1", -1));
                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWHavingWEqualsAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value = all (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c0, " +
                          "Value = any (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c1, " +
                          "Value = some (select sum(IntPrimitive) from SupportBean#keepall group by TheString having first(TheString) != 'E1') as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E1", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, false, false });

                env.SendEventBean(new SupportBean("E2", 10));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E3", 11));
                SendVEAndAssert(env, fields, 10, new object[] { false, true, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWHavingWExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@name('s0') select exists (select sum(IntPrimitive) from SupportBean having sum(IntPrimitive) < 15) as c0," +
                    "not exists (select sum(IntPrimitive) from SupportBean  having sum(IntPrimitive) < 15) as c1 from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, new object[] { false, true });

                env.SendEventBean(new SupportBean("E1", 1));
                SendVEAndAssert(env, fields, new object[] { true, false });

                env.SendEventBean(new SupportBean("E1", 100));
                SendVEAndAssert(env, fields, new object[] { false, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWOHavingWExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl = "@name('s0') select exists (select sum(IntPrimitive) from SupportBean) as c0," +
                          "not exists (select sum(IntPrimitive) from SupportBean) as c1 from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, new object[] { true, false });

                env.SendEventBean(new SupportBean("E1", 1));
                SendVEAndAssert(env, fields, new object[] { true, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWOHavingWExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@Public create window MyWindow#keepall as (key string, anint int);\n" +
                          "insert into MyWindow(key, anint) select Id, Value from SupportIdAndValueEvent;\n" +
                          "@name('s0') select exists (select sum(anint) from MyWindow group by key) as c0," +
                          "not exists (select sum(anint) from MyWindow group by key) as c1 from SupportValueEvent;\n";
                env.CompileDeploy(epl, path).AddListener("s0");
                var fields = "c0,c1".SplitCsv();

                SendVEAndAssert(env, fields, new object[] { false, true });

                env.SendEventBean(new SupportIdAndValueEvent("E1", 19));
                SendVEAndAssert(env, fields, new object[] { true, false });

                env.CompileExecuteFAFNoResult("delete from MyWindow", path);

                SendVEAndAssert(env, fields, new object[] { false, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectGroupedWHavingWExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@Public create window MyWindow#keepall as (key string, anint int);\n" +
                          "insert into MyWindow(key, anint) select Id, Value from SupportIdAndValueEvent;\n" +
                          "@name('s0') select exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c0," +
                          "not exists (select sum(anint) from MyWindow group by key having sum(anint) < 15) as c1 from SupportValueEvent";
                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(epl, path).AddListener("s0");

                SendVEAndAssert(env, fields, new object[] { false, true });

                env.SendEventBean(new SupportIdAndValueEvent("E1", 19));
                SendVEAndAssert(env, fields, new object[] { false, true });

                env.SendEventBean(new SupportIdAndValueEvent("E2", 12));
                SendVEAndAssert(env, fields, new object[] { true, false });

                env.CompileExecuteFAFNoResult("delete from MyWindow", path);

                SendVEAndAssert(env, fields, new object[] { false, true });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWHavingWRelOpAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value < all (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c0, " +
                          "Value < any (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c1, " +
                          "Value < some (select sum(IntPrimitive) from SupportBean#keepall having last(TheString) not in ('E1', 'E3')) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E1", 19));
                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E2", 11));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E3", 9));
                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E4", -1000));
                SendVEAndAssert(env, fields, 10, new object[] { false, false, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectUngroupedWOHavingWRelOpAllAnySome : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var epl = "@name('s0') select " +
                          "Value < all (select sum(IntPrimitive) from SupportBean#keepall) as c0, " +
                          "Value < any (select sum(IntPrimitive) from SupportBean#keepall) as c1, " +
                          "Value < some (select sum(IntPrimitive) from SupportBean#keepall) as c2 " +
                          "from SupportValueEvent";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendVEAndAssert(env, fields, 10, new object[] { null, null, null });

                env.SendEventBean(new SupportBean("E1", 11));
                SendVEAndAssert(env, fields, 10, new object[] { true, true, true });

                env.SendEventBean(new SupportBean("E2", -1000));
                SendVEAndAssert(env, fields, 10, new object[] { false, false, false });

                env.UndeployAll();
            }
        }

        private class EPLSubselectExistsSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Id from SupportBean_S0 where exists (select max(Id) from SupportBean_S1#length(3))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0(env, 1);
                env.AssertEqualsNew("s0", "Id", 1);

                SendEventS1(env, 100);
                SendEventS0(env, 2);
                env.AssertEqualsNew("s0", "Id", 2);

                env.UndeployAll();
            }
        }

        private class EPLSubselectInSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Id from SupportBean_S0 where Id in (select max(Id) from SupportBean_S1#length(2))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventS0(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, 100);
                SendEventS0(env, 2);
                env.AssertListenerNotInvoked("s0");

                SendEventS0(env, 100);
                env.AssertEqualsNew("s0", "Id", 100);

                SendEventS0(env, 200);
                env.AssertListenerNotInvoked("s0");

                SendEventS1(env, -1);
                SendEventS1(env, -1);
                SendEventS0(env, -1);
                env.AssertEqualsNew("s0", "Id", -1);

                env.UndeployAll();
            }
        }

        private static void SendVEAndAssert(
            RegressionEnvironment env,
            string[] fields,
            int value,
            object[] expected)
        {
            env.SendEventBean(new SupportValueEvent(value));
            env.AssertPropsNew("s0", fields, expected);
        }

        private static void SendVEAndAssert(
            RegressionEnvironment env,
            string[] fields,
            object[] expected)
        {
            env.SendEventBean(new SupportValueEvent(-1));
            env.AssertPropsNew("s0", fields, expected);
        }

        private static void SendEventS0(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S0(id));
        }

        private static void SendEventS1(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportBean_S1(id));
        }
    }
} // end of namespace