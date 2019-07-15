///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerGroupHaving
    {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeHavingCount());
            execs.Add(new ResultSetQueryTypeSumJoin());
            execs.Add(new ResultSetQueryTypeSumOneView());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            SendEvent(env, SYMBOL_DELL, 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, SYMBOL_DELL, 60);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(0);

            SendEvent(env, SYMBOL_DELL, 30);
            AssertNewEvent(env, SYMBOL_DELL, 100);

            SendEvent(env, SYMBOL_IBM, 30);
            AssertOldEvent(env, SYMBOL_DELL, 100);

            SendEvent(env, SYMBOL_IBM, 80);
            AssertNewEvent(env, SYMBOL_IBM, 110);
        }

        private static void AssertNewEvent(
            RegressionEnvironment env,
            string symbol,
            double newSum)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(symbol, newData[0].Get("Symbol"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void AssertOldEvent(
            RegressionEnvironment env,
            string symbol,
            double newSum)
        {
            var oldData = env.Listener("s0").LastOldData;
            var newData = env.Listener("s0").LastNewData;

            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);

            Assert.AreEqual(newSum, oldData[0].Get("mySum"));
            Assert.AreEqual(symbol, oldData[0].Get("Symbol"));

            env.Listener("s0").Reset();
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            env.SendEventBean(bean);
        }

        internal class ResultSetQueryTypeHavingCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select * from SupportBean(IntPrimitive = 3)#length(10) as e1 group by TheString having count(*) > 2";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean("A1", 3));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A1", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.SendEventBean(new SupportBean("A1", 3));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, sum(price) as mySum " +
                          "from SupportBeanString#length(100) as one, " +
                          " SupportMarketDataBean#length(3) as two " +
                          "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE')" +
                          "       and one.TheString = two.Symbol " +
                          "group by Symbol " +
                          "having sum(price) >= 100";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBeanString(SYMBOL_DELL));
                env.SendEventBean(new SupportBeanString(SYMBOL_IBM));
                env.SendEventBean(new SupportBeanString("AAA"));

                TryAssertion(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream Symbol, sum(price) as mySum " +
                          "from SupportMarketDataBean#length(3) " +
                          "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                          "group by Symbol " +
                          "having sum(price) >= 100";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env);

                env.UndeployAll();
            }
        }
    }
} // end of namespace