///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
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
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseCoercion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceWhereClauseCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithInKeywordAsPartOfSubquery(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLSubselectPerformanceInKeywordAsPartOfSubquery());
            return execs;
        }

        private static void TryAssertionPerformanceInKeywordAsPartOfSubquery(RegressionEnvironment env)
        {
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean_S0(i, "v" + i, "P00_" + i));
            }

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 2000; i++) {
                var index = 5000 + i % 1000;
                env.SendEventBean(new SupportBean_S1(index, "x", "P00_" + index));
                Assert.AreEqual("v" + index, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;

            Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
        }

        internal class EPLSubselectPerformanceInKeywordAsPartOfSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var eplSingleIndex =
                    "@Name('s0') select (select P00 from SupportBean_S0#keepall as S0 where S0.P01 in (S1.P10, S1.P11)) as c0 from SupportBean_S1 as S1";
                env.CompileDeployAddListenerMile(eplSingleIndex, "s0", milestone.GetAndIncrement());

                TryAssertionPerformanceInKeywordAsPartOfSubquery(env);
                env.UndeployAll();

                var eplMultiIdx =
                    "@Name('s0') select (select P00 from SupportBean_S0#keepall as S0 where S1.P11 in (S0.P00, S0.P01)) as c0 from SupportBean_S1 as S1";
                env.CompileDeployAddListenerMile(eplMultiIdx, "s0", milestone.GetAndIncrement());

                TryAssertionPerformanceInKeywordAsPartOfSubquery(env);

                env.UndeployAll();
            }
        }

        internal class EPLSubselectPerformanceWhereClauseCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select IntPrimitive from SupportBean(TheString='A') as S0 where IntPrimitive in (" +
                    "select LongBoxed from SupportBean(TheString='B')#length(10000) where S0.IntPrimitive = LongBoxed)";

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
                    Assert.AreEqual(index, env.Listener("s0").AssertOneGetNewAndReset().Get("IntPrimitive"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(4000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectPerformanceWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText = "@Name('s0') select Id from SupportBean_S0 as S0 where P00 in (" +
                               "select P10 from SupportBean_S1#length(10000) where S0.P00 = P10)";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                // preload with 10k events
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S1(i, Convert.ToString(i)));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 10000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(new SupportBean_S0(index, Convert.ToString(index)));
                    Assert.AreEqual(index, env.Listener("s0").AssertOneGetNewAndReset().Get("Id"));
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;

                Assert.That(delta, Is.LessThan(1000), "Failed perf test, delta=" + delta);
                env.UndeployAll();
            }
        }
    }
} // end of namespace