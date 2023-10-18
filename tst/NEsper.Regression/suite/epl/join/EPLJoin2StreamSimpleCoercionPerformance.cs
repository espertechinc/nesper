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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamSimpleCoercionPerformance
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithForward(execs);
            WithBack(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithBack(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerformanceCoercionBack());
            return execs;
        }

        public static IList<RegressionExecution> WithForward(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinPerformanceCoercionForward());
            return execs;
        }

        private static object MakeSupportEvent(
            string theString,
            int intPrimitive,
            long longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.LongBoxed = longBoxed;
            return bean;
        }

        internal class EPLJoinPerformanceCoercionForward : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var stmt = "@name('s0') select A.LongBoxed as value from " +
                           "SupportBean(TheString='A')#length(1000000) as A," +
                           "SupportBean(TheString='B')#length(1000000) as B" +
                           " where A.LongBoxed=B.IntPrimitive";
                env.CompileDeployAddListenerMileZero(stmt, "s0");
                // preload
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(MakeSupportEvent("A", 0, i));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(MakeSupportEvent("B", index, 0));
                    env.AssertEqualsNew("s0", "value", index);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
            }
        }

        internal class EPLJoinPerformanceCoercionBack : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }
            
            public void Run(RegressionEnvironment env)
            {
                var stmt = "@name('s0') select A.IntPrimitive as value from " +
                           "SupportBean(TheString='A')#length(1000000) as A," +
                           "SupportBean(TheString='B')#length(1000000) as B" +
                           " where A.IntPrimitive=B.LongBoxed";
                env.CompileDeployAddListenerMileZero(stmt, "s0");
                // preload
                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(MakeSupportEvent("A", i, 0));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 5000; i++) {
                    var index = 5000 + i % 1000;
                    env.SendEventBean(MakeSupportEvent("B", 0, index));
                    env.AssertEqualsNew("s0", "value", index);
                }

                var endTime = PerformanceObserver.MilliTime;
                var delta = endTime - startTime;
                env.UndeployAll();
                Assert.That(delta, Is.LessThan(1500), "Failed perf test, delta=" + delta);
            }
        }
    }
} // end of namespace