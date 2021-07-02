///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesOutputRate
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
WithEventsAll(execs);
WithEventsAllOM(execs);
WithEventsAllCompile(execs);
WithTimeAll(execs);
            return execs;
        }
public static IList<RegressionExecution> WithTimeAll(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLVariableOutputRateTimeAll());
    return execs;
}public static IList<RegressionExecution> WithEventsAllCompile(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLVariableOutputRateEventsAllCompile());
    return execs;
}public static IList<RegressionExecution> WithEventsAllOM(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLVariableOutputRateEventsAllOM());
    return execs;
}public static IList<RegressionExecution> WithEventsAll(IList<RegressionExecution> execs = null)
{
    execs = execs ?? new List<RegressionExecution>();
    execs.Add(new EPLVariableOutputRateEventsAll());
    return execs;
}
        private static void TryAssertionOutputRateEventsAll(RegressionEnvironment env)
        {
            SendSupportBeans(env, "E1", "E2"); // varargs: sends 2 events
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBeans(env, "E3");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {3L});
            env.Listener("s0").Reset();

            // set output limit to 5
            var stmtTextSet = "on SupportMarketDataBean set var_output_limit = Volume";
            env.CompileDeploy(stmtTextSet);
            SendSetterBean(env, 5L);

            SendSupportBeans(env, "E4", "E5", "E6", "E7"); // send 4 events
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBeans(env, "E8");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {8L});
            env.Listener("s0").Reset();

            // set output limit to 2
            SendSetterBean(env, 2L);

            SendSupportBeans(env, "E9"); // send 1 events
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendSupportBeans(env, "E10");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {10L});
            env.Listener("s0").Reset();

            // set output limit to 1
            SendSetterBean(env, 1L);

            SendSupportBeans(env, "E11");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {11L});
            env.Listener("s0").Reset();

            SendSupportBeans(env, "E12");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {12L});
            env.Listener("s0").Reset();

            // set output limit to null -- this continues at the current rate
            SendSetterBean(env, null);

            SendSupportBeans(env, "E13");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                new[] {"cnt"},
                new object[] {13L});
            env.Listener("s0").Reset();
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendSupportBeans(
            RegressionEnvironment env,
            params string[] strings)
        {
            foreach (var theString in strings) {
                SendSupportBean(env, theString);
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
        }

        private static void SendSetterBean(
            RegressionEnvironment env,
            long? longValue)
        {
            var bean = new SupportMarketDataBean("", 0, longValue, "");
            env.SendEventBean(bean);
        }

        internal class EPLVariableOutputRateEventsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "var_output_limit", 3L);
                var stmtTextSelect =
                    "@Name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        internal class EPLVariableOutputRateEventsAllOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "var_output_limit", 3L);
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.CountStar(), "cnt");
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model.OutputLimitClause = OutputLimitClause.Create(OutputLimitSelector.LAST, "var_output_limit");
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                var stmtTextSelect =
                    "@Name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                Assert.AreEqual(stmtTextSelect, model.ToEPL());
                env.CompileDeploy(model, new RegressionPath()).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        internal class EPLVariableOutputRateEventsAllCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "var_output_limit", 3L);

                var stmtTextSelect =
                    "@Name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                env.EplToModelCompileDeploy(stmtTextSelect).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        internal class EPLVariableOutputRateTimeAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Runtime.VariableService.SetVariableValue(null, "var_output_limit", 3L);
                SendTimer(env, 0);

                var stmtTextSelect =
                    "@Name('s0') select count(*) as cnt from SupportBean output snapshot every var_output_limit seconds";
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                SendSupportBeans(env, "E1", "E2"); // varargs: sends 2 events
                SendTimer(env, 2999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendTimer(env, 3000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    new[] {"cnt"},
                    new object[] {2L});
                env.Listener("s0").Reset();

                // set output limit to 5
                var stmtTextSet = "on SupportMarketDataBean set var_output_limit = Volume";
                env.CompileDeploy(stmtTextSet);
                SendSetterBean(env, 5L);

                // set output limit to 1 second
                SendSetterBean(env, 1L);

                env.Milestone(1);

                SendTimer(env, 3200);
                SendSupportBeans(env, "E3", "E4");
                SendTimer(env, 3999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 4000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    new[] {"cnt"},
                    new object[] {4L});
                env.Listener("s0").Reset();

                // set output limit to 4 seconds (takes effect next time rescheduled, and is related to reference point which is 0)
                SendSetterBean(env, 4L);

                env.Milestone(2);

                SendTimer(env, 4999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 5000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    new[] {"cnt"},
                    new object[] {4L});
                env.Listener("s0").Reset();

                SendTimer(env, 7999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 8000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    new[] {"cnt"},
                    new object[] {4L});
                env.Listener("s0").Reset();

                SendSupportBeans(env, "E5", "E6"); // varargs: sends 2 events

                env.Milestone(3);

                SendTimer(env, 11999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 12000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    new[] {"cnt"},
                    new object[] {6L});
                env.Listener("s0").Reset();

                SendTimer(env, 13000);
                // set output limit to 2 seconds (takes effect next time event received, and is related to reference point which is 0)
                SendSetterBean(env, 2L);
                SendSupportBeans(env, "E7", "E8"); // varargs: sends 2 events
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 13999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                // set output limit to null : should stay at 2 seconds
                SendSetterBean(env, null);
                try {
                    SendTimer(env, 14000);
                    Assert.Fail();
                }
                catch (Exception) {
                    // expected
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace