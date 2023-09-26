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
    public class EPLJoin3StreamInKeywordPerformance : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select S0.Id as val from " +
                      "SupportBean_S0#keepall S0, " +
                      "SupportBean_S1#keepall S1, " +
                      "SupportBean_S2#keepall S2 " +
                      "where P00 in (P10, P20)";
            var fields = new[] { "val" };
            env.CompileDeployAddListenerMileZero(epl, "s0");

            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean_S0(i, $"P00_{i}"));
            }

            env.SendEventBean(new SupportBean_S1(0, "x"));

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(new SupportBean_S2(1, "P00_6541"));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new[] { new object[] { 6541 } });
            }

            var delta = PerformanceObserver.MilliTime - startTime;
            Assert.That(delta, Is.LessThan(500), "delta=" + delta);
            log.Info($"delta={delta}");

            env.UndeployAll();
        }
    }
} // end of namespace