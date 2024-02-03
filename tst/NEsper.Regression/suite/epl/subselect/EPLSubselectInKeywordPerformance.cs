///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectInKeywordPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInKeywordAsPartOfSubquery(execs);
            WithWhereClauseCoercion(execs);
            WithWhereClause(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceWhereClauseCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithInKeywordAsPartOfSubquery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceInKeywordAsPartOfSubquery());
            return execs;
        }

        private class EPLSubselectPerformanceInKeywordAsPartOfSubquery : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var eplSingleIndex =
                    "@name('s0') select (select P00 from SupportBean_S0#keepall as s0 where s0.P01 in (s1.P10, s1.P11)) as c0 from SupportBean_S1 as s1";
                env.CompileDeployAddListenerMile(eplSingleIndex, "s0", milestone.GetAndIncrement());

                TryAssertionPerformanceInKeywordAsPartOfSubquery(env);
                env.UndeployAll();

                var eplMultiIdx =
                    "@name('s0') select (select P00 from SupportBean_S0#keepall as s0 where s1.P11 in (s0.P00, s0.P01)) as c0 from SupportBean_S1 as s1";
                env.CompileDeployAddListenerMile(eplMultiIdx, "s0", milestone.GetAndIncrement());

                TryAssertionPerformanceInKeywordAsPartOfSubquery(env);

                env.UndeployAll();
            }
        }

        private class EPLSubselectPerformanceWhereClauseCoercion : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select IntPrimitive from SupportBean(TheString='A') as s0 where IntPrimitive in (" +
                    "select LongBoxed from SupportBean(TheString='B')#length(10000) where s0.IntPrimitive = LongBoxed)";

                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    var bean = new SupportBean();
                    bean.TheString = "B";
                    bean.LongBoxed = i;
                    env.SendEventBean(bean);
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    var bean = new SupportBean();
                    bean.TheString = "A";
                    bean.IntPrimitive = index;
                    env.SendEventBean(bean);
                    env.AssertEqualsNew("s0", "IntPrimitive", index);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(2000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private class EPLSubselectPerformanceWhereClause : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@name('s0') select Id from SupportBean_S0 as s0 where P00 in (" +
                               "select P10 from SupportBean_S1#length(10000) where s0.P00 = P10)";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    env.AssertEqualsNew("s0", "Id", index);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        private static void TryAssertionPerformanceInKeywordAsPartOfSubquery(RegressionEnvironment env)
        {
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean_S0(i, "v" + i, "p00_" + i));
            }

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 2000; i++) {
                var index = 5000 + i % 1000;
                env.SendEventBean(new SupportBean_S1(index, "x", "p00_" + index));
                env.AssertEqualsNew("s0", "c0", "v" + index);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;

            Assert.That(delta, Is.LessThan(500), "Failed perf test, delta=" + delta);
        }
    }
} // end of namespace