///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectCorrelatedAggregationPerformance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@Name('s0') select P00, " +
                           "(select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.P00) as sumP00 " +
                           "from SupportBean_S0 as s0";
            env.CompileDeploy(stmtText).AddListener("s0");

            var fields = "P00,sumP00".SplitCsv();

            // preload
            var max = 50000;
            for (var i = 0; i < max; i++) {
                env.SendEventBean(new SupportBean("T" + i, -i));
                env.SendEventBean(new SupportBean("T" + i, 10));
            }

            // exercise
            var start = PerformanceObserver.MilliTime;
            var random = new Random();
            for (var i = 0; i < 10000; i++) {
                var index = random.Next(max);
                env.SendEventBean(new SupportBean_S0(0, "T" + index));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"T" + index, -index + 10});
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;

            //System.out.println("delta=" + delta);
            Assert.IsTrue(delta < 500, "delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace