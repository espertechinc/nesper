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
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithnRange(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinCoercionRange());
            return execs;
        }

        private class EPLJoinJoinCoercionRange : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var fields = "sbs,sbi,sbri".SplitCsv();
                string epl;

                epl =
                    "@name('s0') select sb.theString as sbs, sb.intPrimitive as sbi, sbr.id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where intPrimitive between rangeStartLong and rangeEndLong";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E2", 100));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 100, "R1" });

                env.SendEventBean(SupportBeanRange.MakeLong("R2", "G", 90L, 100L));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 100, "R2" });

                env.SendEventBean(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10, "R3" });

                env.SendEventBean(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
                env.SendEventBean(new SupportBean("E1", 1000));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();

                epl =
                    "@name('s0') select sb.theString as sbs, sb.intPrimitive as sbi, sbr.id as sbri from SupportBean#length(10) sb, SupportBeanRange#length(10) sbr " +
                    "where sbr.key = sb.theString and intPrimitive between rangeStartLong and rangeEndLong";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(SupportBeanRange.MakeLong("R1", "G", 100L, 200L));
                env.SendEventBean(new SupportBean("G", 10));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G", 101));
                env.AssertPropsNew("s0", fields, new object[] { "G", 101, "R1" });

                env.SendEventBean(SupportBeanRange.MakeLong("R2", "G", 90L, 102L));
                env.AssertPropsNew("s0", fields, new object[] { "G", 101, "R2" });

                env.SendEventBean(SupportBeanRange.MakeLong("R3", "G", 1L, 99L));
                env.AssertPropsNew("s0", fields, new object[] { "G", 10, "R3" });

                env.SendEventBean(SupportBeanRange.MakeLong("R4", "G", 2000L, 3000L));
                env.SendEventBean(new SupportBean("G", 1000));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLJoinJoinCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select volume from " +
                                    "SupportMarketDataBean#length(3) as s0," +
                                    "SupportBean#length(3) as s1 " +
                                    " where s0.volume = s1.intPrimitive";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");
                SendBeanEvent(env, 100);
                SendMarketEvent(env, 100);
                env.AssertEqualsNew("s0", "volume", 100L);
                env.UndeployAll();
            }
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
    }
} // end of namespace