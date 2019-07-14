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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqUncorrel
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            // named window tests
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, false, false)); // testNoShare
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, true, false)); // testShare
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, true, true)); // testDisableShare

            // table tests
            execs.Add(new InfraNWTableSubqUncorrelAssertion(false, false, false));
            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(bean);
        }

        internal class InfraNWTableSubqUncorrelAssertion : RegressionExecution
        {
            private readonly bool disableIndexShareConsumer;
            private readonly bool enableIndexShareCreate;
            private readonly bool namedWindow;

            public InfraNWTableSubqUncorrelAssertion(
                bool namedWindow,
                bool enableIndexShareCreate,
                bool disableIndexShareConsumer)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
                this.disableIndexShareConsumer = disableIndexShareConsumer;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfra#keepall as select TheString as a, longPrimitive as b, longBoxed as c from SupportBean"
                    : "@Name('create') create table MyInfra(a string primary key, b long, c long)";
                if (enableIndexShareCreate) {
                    stmtTextCreate = "@Hint('enable_window_subquery_indexshare') " + stmtTextCreate;
                }

                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfra select TheString as a, longPrimitive as b, longBoxed as c from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var stmtTextSelectOne =
                    "@Name('select') select irstream (select a from MyInfra) as value, symbol from SupportMarketDataBean";
                if (disableIndexShareConsumer) {
                    stmtTextSelectOne = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectOne;
                }

                env.CompileDeploy(stmtTextSelectOne, path).AddListener("select");
                EPAssertionUtil.AssertEqualsAnyOrder(
                    env.Statement("select").EventType.PropertyNames,
                    new[] {"value", "symbol"});
                Assert.AreEqual(typeof(string), env.Statement("select").EventType.GetPropertyType("value"));
                Assert.AreEqual(typeof(string), env.Statement("select").EventType.GetPropertyType("symbol"));

                SendMarketBean(env, "M1");
                string[] fieldsStmt = {"value", "symbol"};
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M1"});

                SendSupportBean(env, "S1", 1L, 2L);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                string[] fieldsWin = {"a", "b", "c"};
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fieldsWin,
                        new object[] {"S1", 1L, 2L});
                }
                else {
                    Assert.IsFalse(env.Listener("create").IsInvoked);
                }

                // create consumer 2 -- note that this one should not start empty now
                var stmtTextSelectTwo =
                    "@Name('selectTwo') select irstream (select a from MyInfra) as value, symbol from SupportMarketDataBean";
                if (disableIndexShareConsumer) {
                    stmtTextSelectTwo = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectTwo;
                }

                env.CompileDeploy(stmtTextSelectTwo, path).AddListener("selectTwo");

                SendMarketBean(env, "M1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S1", "M1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S1", "M1"});

                SendSupportBean(env, "S2", 10L, 20L);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fieldsWin,
                        new object[] {"S2", 10L, 20L});
                }

                SendMarketBean(env, "M2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M2"});
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M2"});

                // create delete stmt
                var stmtTextDelete = "@Name('delete') on SupportBean_A delete from MyInfra where id = a";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // delete S1
                env.SendEventBean(new SupportBean_A("S1"));
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetOldAndReset(),
                        fieldsWin,
                        new object[] {"S1", 1L, 2L});
                }

                SendMarketBean(env, "M3");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S2", "M3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S2", "M3"});

                // delete S2
                env.SendEventBean(new SupportBean_A("S2"));
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetOldAndReset(),
                        fieldsWin,
                        new object[] {"S2", 10L, 20L});
                }

                SendMarketBean(env, "M4");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M4"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M4"});

                SendSupportBean(env, "S3", 100L, 200L);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fieldsWin,
                        new object[] {"S3", 100L, 200L});
                }

                SendMarketBean(env, "M5");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S3", "M5"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {"S3", "M5"});

                env.UndeployAll();
            }
        }
    }
} // end of namespace