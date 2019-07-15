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
            execs.Add(new EPLJoinInKeywordSingleIndexLookup());
            execs.Add(new EPLJoinInKeywordMultiIndexLookup());
            return execs;
        }

        internal class EPLJoinInKeywordSingleIndexLookup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select IntPrimitive as val from SupportBean#keepall sb, SupportBean_S0 s0 unIdirectional " +
                    "where sb.TheString in (s0.p00, s0.p01)";
                var fields = "val".SplitCsv();
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("E" + i, i));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBean_S0(1, "E645", "E8975"));
                    EPAssertionUtil.AssertPropsPerRow(
                        env.Listener("s0").GetAndResetLastNewData(),
                        fields,
                        new[] {new object[] {645}, new object[] {8975}});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                log.Info("delta=" + delta);

                env.UndeployAll();
            }
        }

        internal class EPLJoinInKeywordMultiIndexLookup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select Id as val from SupportBean_S0#keepall s0, SupportBean sb unIdirectional " +
                    "where sb.TheString in (s0.p00, s0.p01)";
                var fields = "val".SplitCsv();
                env.CompileDeployAddListenerMileZero(epl, "s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean_S0(i, "p00_" + i, "p01_" + i));
                }

                var startTime = PerformanceObserver.MilliTime;
                for (var i = 0; i < 1000; i++) {
                    env.SendEventBean(new SupportBean("p01_645", 0));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {645});
                }

                var delta = PerformanceObserver.MilliTime - startTime;
                Assert.IsTrue(delta < 500, "delta=" + delta);
                log.Info("delta=" + delta);

                env.UndeployAll();
            }
        }
    }
} // end of namespace