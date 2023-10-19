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

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitFirstHaving
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if TEMPORARY
            WithNoAvgOutputFirstEvents(execs);
            WithNoAvgOutputFirstMinutes(execs);
            WithAvgOutputFirstEveryTwoMinutes(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithAvgOutputFirstEveryTwoMinutes(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHavingAvgOutputFirstEveryTwoMinutes());
            return execs;
        }

        public static IList<RegressionExecution> WithNoAvgOutputFirstMinutes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHavingNoAvgOutputFirstMinutes());
            return execs;
        }

        public static IList<RegressionExecution> WithNoAvgOutputFirstEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetHavingNoAvgOutputFirstEvents());
            return execs;
        }

        private class ResultSetHavingNoAvgOutputFirstEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var query =
                    "@name('s0') select doublePrimitive from SupportBean having doublePrimitive > 1 output first every 2 events";
                env.CompileDeploy(query).AddListener("s0");

                TryAssertion2Events(env);
                env.UndeployAll();

                // test joined
                query =
                    "@name('s0') select doublePrimitive from SupportBean#lastevent,SupportBean_ST0#lastevent st0 having doublePrimitive > 1 output first every 2 events";
                env.CompileDeploy(query).AddListener("s0");
                env.SendEventBean(new SupportBean_ST0("ID", 1));
                TryAssertion2Events(env);

                env.UndeployAll();
            }
        }

        private class ResultSetHavingNoAvgOutputFirstMinutes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = "val0".SplitCsv();
                var query =
                    "@name('s0') select sum(doublePrimitive) as val0 from SupportBean#length(5) having sum(doublePrimitive) > 100 output first every 2 seconds";
                env.CompileDeploy(query).AddListener("s0");

                SendBeanEvent(env, 10);
                SendBeanEvent(env, 80);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(1000);
                SendBeanEvent(env, 11);
                env.AssertPropsNew("s0", fields, new object[] { 101d });

                SendBeanEvent(env, 1);

                env.AdvanceTime(2999);
                SendBeanEvent(env, 1);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(3000);
                SendBeanEvent(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, 100);
                env.AssertPropsNew("s0", fields, new object[] { 114d });

                env.AdvanceTime(4999);
                SendBeanEvent(env, 0);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(5000);
                SendBeanEvent(env, 0);
                env.AssertPropsNew("s0", fields, new object[] { 102d });

                env.UndeployAll();
            }
        }

        private class ResultSetHavingAvgOutputFirstEveryTwoMinutes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var query =
                    "@name('s0') select doublePrimitive, avg(doublePrimitive) from SupportBean having doublePrimitive > 2*avg(doublePrimitive) output first every 2 minutes";
                env.CompileDeploy(query).AddListener("s0");

                SendBeanEvent(env, 1);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, 2);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, 9);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void TryAssertion2Events(RegressionEnvironment env)
        {
            SendBeanEvent(env, 1);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 2);
            env.AssertListenerInvoked("s0");

            SendBeanEvent(env, 9);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 1);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 1);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 2);
            env.AssertListenerInvoked("s0");

            SendBeanEvent(env, 1);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 2);
            env.AssertListenerInvoked("s0");

            SendBeanEvent(env, 2);
            env.AssertListenerNotInvoked("s0");

            SendBeanEvent(env, 2);
            env.AssertListenerInvoked("s0");
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            double doublePrimitive)
        {
            var b = new SupportBean();
            b.DoublePrimitive = doublePrimitive;
            env.SendEventBean(b);
        }
    }
} // end of namespace