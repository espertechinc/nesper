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

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.resultset.orderby
{
    public class ResultSetOrderByRowForAll
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetNoOutputRateJoin());
            execs.Add(new ResultSetOutputDefault(false));
            execs.Add(new ResultSetOutputDefault(true));
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        private static void SendJoinEvents(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBeanString("CAT"));
            env.SendEventBean(new SupportBeanString("IBM"));
            env.SendEventBean(new SupportBeanString("CMU"));
            env.SendEventBean(new SupportBeanString("KGB"));
            env.SendEventBean(new SupportBeanString("DOG"));
        }

        internal class ResultSetOutputDefault : RegressionExecution
        {
            private readonly bool join;

            public ResultSetOutputDefault(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select irstream sum(IntPrimitive) as c0, last(TheString) as c1 from SupportBean#length(2) " +
                    (join ? ",SupportBean_A#keepall " : "") +
                    "output every 3 events order by sum(IntPrimitive) desc";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 12));

                var fields = "c0,c1".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {23, "E3"}, new object[] {21, "E2"}, new object[] {10, "E1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {21, "E2"}, new object[] {10, "E1"}, new object[] {null, null}});

                env.UndeployAll();
            }
        }

        internal class ResultSetNoOutputRateJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"sumPrice"};
                var epl = "@Name('s0')select sum(price) as sumPrice from " +
                          "SupportMarketDataBean#length(10) as one, " +
                          "SupportBeanString#length(100) as two " +
                          "where one.Symbol = two.TheString " +
                          "order by price";
                env.CompileDeploy(epl).AddListener("s0");

                SendJoinEvents(env);
                SendEvent(env, "CAT", 50);
                SendEvent(env, "IBM", 49);
                SendEvent(env, "CAT", 15);
                SendEvent(env, "IBM", 100);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {214d}});

                SendEvent(env, "KGB", 75);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {289d}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace