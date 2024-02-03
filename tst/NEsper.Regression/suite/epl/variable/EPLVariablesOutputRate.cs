///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
        }

        public static IList<RegressionExecution> WithEventsAllCompile(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOutputRateEventsAllCompile());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsAllOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOutputRateEventsAllOM());
            return execs;
        }

        public static IList<RegressionExecution> WithEventsAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLVariableOutputRateEventsAll());
            return execs;
        }

        private class EPLVariableOutputRateEventsAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.RuntimeSetVariable(null, "var_output_limit", 3L);
                var stmtTextSelect =
                    "@name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        private class EPLVariableOutputRateEventsAllOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.RuntimeSetVariable(null, "var_output_limit", 3L);
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create().Add(Expressions.CountStar(), "cnt");
                model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportBean)));
                model.OutputLimitClause = OutputLimitClause.Create(OutputLimitSelector.LAST, "var_output_limit");
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));

                var stmtTextSelect =
                    "@name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                ClassicAssert.AreEqual(stmtTextSelect, model.ToEPL());
                env.CompileDeploy(model, new RegressionPath()).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        private class EPLVariableOutputRateEventsAllCompile : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.RuntimeSetVariable(null, "var_output_limit", 3L);

                var stmtTextSelect =
                    "@name('s0') select count(*) as cnt from SupportBean output last every var_output_limit events";
                env.EplToModelCompileDeploy(stmtTextSelect).AddListener("s0");

                TryAssertionOutputRateEventsAll(env);

                env.UndeployAll();
            }
        }

        private class EPLVariableOutputRateTimeAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "cnt" };
                env.RuntimeSetVariable(null, "var_output_limit", 3L);
                SendTimer(env, 0);

                var stmtTextSelect =
                    "@name('s0') select count(*) as cnt from SupportBean output snapshot every var_output_limit seconds";
                env.CompileDeploy(stmtTextSelect).AddListener("s0");

                SendSupportBeans(env, "E1", "E2"); // varargs: sends 2 events
                SendTimer(env, 2999);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                SendTimer(env, 3000);
                env.AssertPropsNew("s0", fields, new object[] { 2L });

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
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 4000);
                env.AssertPropsNew("s0", fields, new object[] { 4L });

                // set output limit to 4 seconds (takes effect next time rescheduled, and is related to reference point which is 0)
                SendSetterBean(env, 4L);

                env.Milestone(2);

                SendTimer(env, 4999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 5000);
                env.AssertPropsNew("s0", fields, new object[] { 4L });

                SendTimer(env, 7999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 8000);
                env.AssertPropsNew("s0", fields, new object[] { 4L });

                SendSupportBeans(env, "E5", "E6"); // varargs: sends 2 events

                env.Milestone(3);

                SendTimer(env, 11999);
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 12000);
                env.AssertPropsNew("s0", fields, new object[] { 6L });

                SendTimer(env, 13000);
                // set output limit to 2 seconds (takes effect next time event received, and is related to reference point which is 0)
                SendSetterBean(env, 2L);
                SendSupportBeans(env, "E7", "E8"); // varargs: sends 2 events
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 13999);
                env.AssertListenerNotInvoked("s0");
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

        private static void TryAssertionOutputRateEventsAll(RegressionEnvironment env)
        {
            var fields = new string[] { "cnt" };
            SendSupportBeans(env, "E1", "E2"); // varargs: sends 2 events
            env.AssertListenerNotInvoked("s0");

            SendSupportBeans(env, "E3");
            env.AssertPropsNew("s0", fields, new object[] { 3L });

            // set output limit to 5
            var stmtTextSet = "on SupportMarketDataBean set var_output_limit = Volume";
            env.CompileDeploy(stmtTextSet);
            SendSetterBean(env, 5L);

            SendSupportBeans(env, "E4", "E5", "E6", "E7"); // send 4 events
            env.AssertListenerNotInvoked("s0");

            SendSupportBeans(env, "E8");
            env.AssertPropsNew("s0", fields, new object[] { 8L });

            // set output limit to 2
            SendSetterBean(env, 2L);

            SendSupportBeans(env, "E9"); // send 1 events
            env.AssertListenerNotInvoked("s0");

            SendSupportBeans(env, "E10");
            env.AssertPropsNew("s0", fields, new object[] { 10L });

            // set output limit to 1
            SendSetterBean(env, 1L);

            SendSupportBeans(env, "E11");
            env.AssertPropsNew("s0", fields, new object[] { 11L });

            SendSupportBeans(env, "E12");
            env.AssertPropsNew("s0", fields, new object[] { 12L });

            // set output limit to null -- this continues at the current rate
            SendSetterBean(env, null);

            SendSupportBeans(env, "E13");
            env.AssertPropsNew("s0", fields, new object[] { 13L });
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
    }
} // end of namespace