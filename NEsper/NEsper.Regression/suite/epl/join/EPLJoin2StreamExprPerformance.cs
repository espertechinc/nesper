///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamExprPerformance : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run(RegressionEnvironment env)
        {
            string epl;
            var milestone = new AtomicLong();

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = 'E6750'";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6750);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = 'E6749'";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6749);

            epl = "create variable string myconst = 'E6751';\n" +
                  "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = myconst;\n";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6751);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = (Id || '6752')";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6752);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = (Id || '6753')";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6753);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean#keepall sb, SupportBean_ST0#lastevent s0 where sb.TheString = 'E6754' and sb.IntPrimitive=6754";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6754);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = (Id || '6755') and sb.IntPrimitive=6755";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6755);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between 6756 and 6756";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6756);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= 6757 and IntPrimitive <= 6757";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6757);

            epl =
                "@Name('s0') select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= (RangeStart + 1) and IntPrimitive <= (RangeEnd - 1)";
            TryAssertion(env, epl, milestone, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);

            epl =
                "@Name('s0') select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive >= 6001 and IntPrimitive <= (RangeEnd - 1)";
            TryAssertion(env, epl, milestone, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);

            epl =
                "@Name('s0') select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between (RangeStart + 1) and (RangeEnd - 1)";
            TryAssertion(env, epl, milestone, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);

            epl =
                "@Name('s0') select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive between (RangeStart + 1) and 6004";
            TryAssertion(env, epl, milestone, new SupportBeanRange("R1", 6000, 6005), 6001 + 6002 + 6003 + 6004);

            epl =
                "@Name('s0') select sum(IntPrimitive) as val from SupportBeanRange#lastevent s0, SupportBean#keepall sb where sb.IntPrimitive in (6001 : (RangeEnd - 1)]";
            TryAssertion(env, epl, milestone, new SupportBeanRange("R1", 6000, 6005), 6002 + 6003 + 6004);

            epl =
                "@Name('s0') select IntPrimitive as val from SupportBean_ST0#lastevent s0, SupportBean#keepall sb where sb.TheString = 'E6758' and sb.IntPrimitive >= 6758 and IntPrimitive <= 6758";
            TryAssertion(env, epl, milestone, new SupportBean_ST0("E", -1), 6758);
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string epl,
            AtomicLong milestone,
            object theEvent,
            object expected)
        {
            var fields = new [] { "val" };
            env.CompileDeploy(epl).AddListener("s0");

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean("E" + i, i));
            }

            env.MilestoneInc(milestone);

            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(theEvent);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {expected});
            }

            var delta = PerformanceObserver.MilliTime - startTime;
            Assert.That(delta, Is.LessThan(2000), "delta=" + delta);
            log.Info("delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace