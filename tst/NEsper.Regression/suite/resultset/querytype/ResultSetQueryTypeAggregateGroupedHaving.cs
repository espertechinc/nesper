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

using NUnit.Framework;
namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeAggregateGroupedHaving
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithGroupByHaving(execs);
            WithSumOneView(execs);
            WithSumJoin(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSumJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSumOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSumOneView());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupByHaving(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeGroupByHaving(false));
            execs.Add(new ResultSetQueryTypeGroupByHaving(true));
            return execs;
        }

        private class ResultSetQueryTypeGroupByHaving : RegressionExecution
        {
            private readonly bool join;

            public ResultSetQueryTypeGroupByHaving(bool join)
            {
                this.join = join;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = !join
                    ? "@name('s0') select * from SupportBean#length_batch(3) group by TheString having count(*) > 1"
                    : "@name('s0') select TheString, IntPrimitive from SupportBean_S0#lastevent, SupportBean#length_batch(3) group by TheString having count(*) > 1";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 21));

                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    "TheString,IntPrimitive".SplitCsv(),
                    new object[][] { new object[] { "E2", 20 }, new object[] { "E2", 21 } });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "join=" +
                       join +
                       '}';
            }
        }

        private class ResultSetQueryTypeSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream Symbol, Volume, sum(Price) as mySum " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "having sum(Price) >= 50";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Every event generates a new row, this time we sum the price by symbol and output volume
                var epl = "@name('s0') select irstream Symbol, Volume, sum(Price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          "SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                          "  and one.TheString = two.Symbol " +
                          "group by Symbol " +
                          "having sum(Price) >= 50";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));

                TryAssertionSum(env);

                env.UndeployAll();
            }
        }

        private static void TryAssertionSum(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("Volume"));
                    Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("mySum"));
                });

            var fields = "Symbol,Volume,mySum".SplitCsv();
            SendEvent(env, SYMBOL_DELL, 10000, 49);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_DELL, 20000, 54);
            env.AssertPropsNew("s0", fields, new object[] { SYMBOL_DELL, 20000L, 103d });

            SendEvent(env, SYMBOL_IBM, 1000, 10);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, SYMBOL_IBM, 5000, 20);
            env.AssertPropsOld("s0", fields, new object[] { SYMBOL_DELL, 10000L, 54d });

            SendEvent(env, SYMBOL_IBM, 6000, 5);
            env.AssertListenerNotInvoked("s0");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            long volume,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, volume, null);
            env.SendEventBean(bean);
        }
    }
} // end of namespace