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

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowForAllHaving
    {
        private const string JOIN_KEY = "KEY";

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRowForAllWHavingSumOneView(execs);
            WithRowForAllWHavingSumJoin(execs);
            WithAvgRowForAllWHavingGroupWindow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAvgRowForAllWHavingGroupWindow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeAvgRowForAllWHavingGroupWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAllWHavingSumJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllWHavingSumJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAllWHavingSumOneView(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeRowForAllWHavingSumOneView());
            return execs;
        }

        private class ResultSetQueryTypeRowForAllWHavingSumOneView : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream sum(LongBoxed) as mySum " +
                          "from SupportBean#time(10 seconds) " +
                          "having sum(LongBoxed) > 10";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssert(env);

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeRowForAllWHavingSumJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream sum(LongBoxed) as mySum " +
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

        private class ResultSetQueryTypeAvgRowForAllWHavingGroupWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
"@name('s0') select istream avg(Price) as aprice from SupportMarketDataBean#unique(Symbol) having avg(Price) <= 0";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "A", -1);
                env.AssertEqualsNew("s0", "aprice", -1.0);

                SendEvent(env, "A", 5);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "B", -6);
                env.AssertEqualsNew("s0", "aprice", -.5d);

                env.Milestone(0);

                SendEvent(env, "C", 2);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "C", 3);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                SendEvent(env, "C", -2);
                env.AssertEqualsNew("s0", "aprice", -1d);

                env.UndeployAll();
            }
        }

        private static void TryAssert(RegressionEnvironment env)
        {
            // assert select result type
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("mySum")));

            SendTimerEvent(env, 0);
            SendEvent(env, 10);
            env.AssertListenerNotInvoked("s0");

            env.Milestone(0);

            SendTimerEvent(env, 5000);
            SendEvent(env, 15);
            env.AssertEqualsNew("s0", "mySum", 25L);

            SendTimerEvent(env, 8000);
            SendEvent(env, -5);
            env.AssertListener(
                "s0",
                listener => Assert.AreEqual(20L, listener.GetAndResetLastNewData()[0].Get("mySum")));

            env.Milestone(1);

            SendTimerEvent(env, 10000);
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(20L, listener.LastOldData[0].Get("mySum"));
                    Assert.IsNull(listener.GetAndResetLastNewData());
                });
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
    }
} // end of namespace