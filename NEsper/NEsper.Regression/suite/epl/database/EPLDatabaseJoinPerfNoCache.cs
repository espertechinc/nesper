///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinPerfNoCache : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertion100EventsRetained(env, "MyDBWithRetain");
            RunAssertion100EventsPooled(env, "MyDBPooled");
            RunAssertionSelectRStream(env, "MyDBWithRetain");
            RunAssertionSelectIStream(env, "MyDBWithRetain");
            RunAssertionWhereClauseNoIndexNoCache(env, "MyDBWithRetain");
        }

        private static void RunAssertion100EventsRetained(
            RegressionEnvironment env,
            string dbname)
        {
            var startTime = PerformanceObserver.MilliTime;
            Try100Events(env, dbname);
            var endTime = PerformanceObserver.MilliTime;
            // log.info(".test100EventsRetained delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 5000);
        }

        private static void RunAssertion100EventsPooled(
            RegressionEnvironment env,
            string dbname)
        {
            var startTime = PerformanceObserver.MilliTime;
            Try100Events(env, dbname);
            var endTime = PerformanceObserver.MilliTime;
            // log.info(".test100EventsPooled delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 10000);
        }

        private static void RunAssertionSelectRStream(
            RegressionEnvironment env,
            string dbname)
        {
            var stmtText = "@Name('s0') select rstream myvarchar from " +
                           "SupportBean_S0#length(1000) as s0," +
                           " sql:" +
                           dbname +
                           "['select myvarchar from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
            env.CompileDeploy(stmtText).AddListener("s0");

            // 1000 events should enter the window fast, no joins
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                var beanX = new SupportBean_S0(10);
                env.SendEventBean(beanX);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.IsTrue(endTime - startTime < 1000, "delta=" + delta);

            // 1001st event should finally join and produce a result
            var bean = new SupportBean_S0(10);
            env.SendEventBean(bean);
            Assert.AreEqual("J", env.Listener("s0").AssertOneGetNewAndReset().Get("myvarchar"));

            env.UndeployAll();
        }

        private static void RunAssertionSelectIStream(
            RegressionEnvironment env,
            string dbname)
        {
            // set time to zero
            env.AdvanceTime(0);

            var stmtText = "@Name('s0') select istream myvarchar from " +
                           "SupportBean_S0#time(1 sec) as s0," +
                           " sql:" +
                           dbname +
                           " ['select myvarchar from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
            env.CompileDeploy(stmtText).AddListener("s0");

            // Send 100 events which all fireStatementStopped a join
            for (var i = 0; i < 100; i++) {
                var bean = new SupportBean_S0(5);
                env.SendEventBean(bean);
                Assert.AreEqual("E", env.Listener("s0").AssertOneGetNewAndReset().Get("myvarchar"));
            }

            // now advance the time, this should not produce events or join
            var startTime = PerformanceObserver.MilliTime;
            env.AdvanceTime(2000);
            var endTime = PerformanceObserver.MilliTime;

            // log.info(".testSelectIStream delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 200);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void RunAssertionWhereClauseNoIndexNoCache(
            RegressionEnvironment env,
            string dbname)
        {
            var stmtText = "@Name('s0') select Id, mycol3, mycol2 from " +
                           "SupportBean_S0#keepall as s0," +
                           " sql:" +
                           dbname +
                           "['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.Id = s1.mycol3";
            env.CompileDeploy(stmtText).AddListener("s0");

            for (var i = 0; i < 20; i++) {
                var num = i + 1;
                var col2 = Convert.ToString(Math.Round((float) num / 10));
                var bean = new SupportBean_S0(num);
                env.SendEventBean(bean);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"Id", "mycol3", "mycol2"},
                    new object[] {num, num, col2});
            }

            env.UndeployAll();
        }

        private static void Try100Events(
            RegressionEnvironment env,
            string dbname)
        {
            var stmtText = "@Name('s0') select myint from " +
                           "SupportBean_S0 as s0," +
                           " sql:" +
                           dbname +
                           " ['select myint from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
            env.CompileDeploy(stmtText).AddListener("s0");

            for (var i = 0; i < 100; i++) {
                var id = i % 10 + 1;

                var bean = new SupportBean_S0(id);
                env.SendEventBean(bean);

                var received = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(id * 10, received.Get("myint"));
            }

            env.UndeployAll();
        }
    }
} // end of namespace