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
    public class ResultSetQueryTypeRowForAllHaving
    {
        private const string JOIN_KEY = "KEY";

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeSumOneView());
            execs.Add(new ResultSetQueryTypeSumJoin());
            execs.Add(new ResultSetQueryTypeAvgGroupWindow());
            return execs;
        }

        private static void TryAssert(RegressionEnvironment env)
        {
            // assert select result type
            Assert.AreEqual(typeof(long?), env.Statement("s0").EventType.GetPropertyType("mySum"));

            SendTimerEvent(env, 0);
            SendEvent(env, 10);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(0);

            SendTimerEvent(env, 5000);
            SendEvent(env, 15);
            Assert.AreEqual(25L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));

            SendTimerEvent(env, 8000);
            SendEvent(env, -5);
            Assert.AreEqual(20L, env.Listener("s0").GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(env.Listener("s0").LastOldData);

            env.Milestone(1);

            SendTimerEvent(env, 10000);
            Assert.AreEqual(20L, env.Listener("s0").LastOldData[0].Get("mySum"));
            Assert.IsNull(env.Listener("s0").GetAndResetLastNewData());
        }

        private static object SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            object theEvent = new SupportMarketDataBean(symbol, price, null, null);
            env.SendEventBean(theEvent);
            return theEvent;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed,
            int intBoxed,
            short shortBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            long longBoxed)
        {
            SendEvent(env, longBoxed, 0, 0);
        }

        private static void SendTimerEvent(
            RegressionEnvironment env,
            long msec)
        {
            env.AdvanceTime(msec);
        }

        internal class ResultSetQueryTypeSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream sum(LongBoxed) as mySum " +
                          "from SupportBean#time(10 seconds) " +
                          "having sum(LongBoxed) > 10";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssert(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream sum(LongBoxed) as mySum " +
                          "from SupportBeanString#time(10 seconds) as one, " +
                          "SupportBean#time(10 seconds) as two " +
                          "where one.TheString = two.TheString " +
                          "having sum(LongBoxed) > 10";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBeanString(JOIN_KEY));

                TryAssert(env);

                env.UndeployAll();
            }
        }

        internal class ResultSetQueryTypeAvgGroupWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select istream avg(Price) as aPrice from SupportMarketDataBean#unique(Symbol) having avg(Price) <= 0";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", -1);
                Assert.AreEqual(-1.0d, env.Listener("s0").LastNewData[0].Get("aPrice"));
                env.Listener("s0").Reset();

                SendEvent(env, "A", 5);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "B", -6);
                Assert.AreEqual(-.5d, env.Listener("s0").LastNewData[0].Get("aPrice"));
                env.Listener("s0").Reset();

                env.Milestone(0);

                SendEvent(env, "C", 2);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "C", 3);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                SendEvent(env, "C", -2);
                Assert.AreEqual(-1d, env.Listener("s0").LastNewData[0].Get("aPrice"));
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace