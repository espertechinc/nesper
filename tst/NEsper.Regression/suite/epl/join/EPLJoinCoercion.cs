///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinCoercion
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithnRange(execs);
            Withn(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withn(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithnRange(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinCoercionRange());
            return execs;
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendMarketEvent(
            RegressionEnvironment env,
            long volume)
        {
            var bean = new SupportMarketDataBean("", 0, volume, null);
            env.SendEventBean(bean);
        }

        internal class EPLJoinJoinCoercionRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var fields = new[] {"sbs", "sbi", "sbri"};
                var epl =
                    "@Name('s0') select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.Id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where IntPrimitive between RangeStartLong and RangeEndLong";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
                env.SendEventBean(new SupportBean("E1", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, "R1"});

                env.SendEventBean(SupportBeanRange.MakeLong("R2", "G", 90L, 100L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, "R2"});

                env.SendEventBean(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, "R3"});

                env.SendEventBean(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
                env.SendEventBean(new SupportBean("E1", 1000));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                epl =
                    "@Name('s0') select sb.TheString as sbs, sb.IntPrimitive as sbi, sbr.Id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where sbr.Key = sb.TheString and IntPrimitive between RangeStartLong and RangeEndLong";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
                env.SendEventBean(new SupportBean("G", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G", 101));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G", 101, "R1"});

                env.SendEventBean(SupportBeanRange.MakeLong("R2", "G", 90L, 102L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G", 101, "R2"});

                env.SendEventBean(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G", 10, "R3"});

                env.SendEventBean(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
                env.SendEventBean(new SupportBean("G", 1000));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLJoinJoinCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select Volume from " +
                                    "SupportMarketDataBean#length(3) as S0," +
                                    "SupportBean#length(3) as S1 " +
                                    " where S0.Volume = S1.IntPrimitive";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");
                SendBeanEvent(env, 100);
                SendMarketEvent(env, 100);
                Assert.AreEqual(100L, env.Listener("s0").AssertOneGetNewAndReset().Get("Volume"));
                env.UndeployAll();
            }
        }
    }
} // end of namespace