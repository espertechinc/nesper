///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewKeepAll
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSimple(execs);
            WithIterator(execs);
            WithWindowStats(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWindowStats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewKeepAllWindowStats());
            return execs;
        }

        public static IList<RegressionExecution> WithIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewKeepAllIterator());
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewKeepAllSimple());
            return execs;
        }

        public class ViewKeepAllSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl = "@name('s0') select irstream TheString as c0 from SupportBean#keepall()";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
                SendSupportBean(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        private class ViewKeepAllIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "Symbol,Price".SplitCsv();
                var epl = "@name('s0') select Symbol, Price from SupportMarketDataBean#keepall";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "ABC", 20);
                SendEvent(env, "DEF", 100);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "ABC", 20d }, new object[] { "DEF", 100d } });

                SendEvent(env, "EFG", 50);

                env.Milestone(1);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "ABC", 20d }, new object[] { "DEF", 100d }, new object[] { "EFG", 50d } });

                env.UndeployAll();
            }
        }

        private class ViewKeepAllWindowStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select irstream Symbol, count(*) as cnt, sum(Price) as mysum from SupportMarketDataBean#keepall group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "S1", 100);
                var fields = new string[] { "Symbol", "cnt", "mysum" };
                env.AssertPropsIRPair("s0", fields, new object[] { "S1", 1L, 100d }, new object[] { "S1", 0L, null });

                SendEvent(env, "S2", 50);
                env.AssertPropsIRPair("s0", fields, new object[] { "S2", 1L, 50d }, new object[] { "S2", 0L, null });

                env.Milestone(1);

                SendEvent(env, "S1", 5);
                env.AssertPropsIRPair("s0", fields, new object[] { "S1", 2L, 105d }, new object[] { "S1", 1L, 100d });

                SendEvent(env, "S2", -1);
                env.AssertPropsIRPair("s0", fields, new object[] { "S2", 2L, 49d }, new object[] { "S2", 1L, 50d });

                env.UndeployAll();
            }
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string)
        {
            env.SendEventBean(new SupportBean(@string, 0));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace