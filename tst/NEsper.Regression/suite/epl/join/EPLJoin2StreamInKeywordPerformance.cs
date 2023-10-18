///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamInKeywordPerformance
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSingleIndexLookup(execs);
            WithMultiIndexLookup(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultiIndexLookup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInKeywordMultiIndexLookup());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleIndexLookup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInKeywordSingleIndexLookup());
            return execs;
        }

        internal class EPLJoinInKeywordSingleIndexLookup : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select IntPrimitive as val from SupportBean#keepall sb, SupportBean_S0 S0 unidirectional " +
                    "where sb.TheString in (S0.P00, S0.P01)";
                var fields = new[] {"val"};
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean($"E{i}", i));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBean_S0(1, "E645", "E8975"));
                    env.AssertPropsPerRowLastNew(
                        "s0",
                        fields,
                        new[] {new object[] {645}, new object[] {8975}});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                log.Info($"delta={delta}");

                env.UndeployAll();
            }
        }

        internal class EPLJoinInKeywordMultiIndexLookup : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
            }
            
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select Id as val from SupportBean_S0#keepall S0, SupportBean sb unidirectional " +
                    "where sb.TheString in (S0.P00, S0.P01)";
                var fields = new[] {"val"};
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, $"P00_{i}", $"P01_{i}"));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBean("P01_645", 0));
                    env.AssertPropsNew(
                        "s0",
                        fields,
                        new object[] {645});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.That(delta, Is.LessThan(500), "delta=" + delta);
                log.Info($"delta={delta}");

                env.UndeployAll();
            }
        }
    }
} // end of namespace