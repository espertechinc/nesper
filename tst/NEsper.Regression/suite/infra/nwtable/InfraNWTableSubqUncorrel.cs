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

//using SupportBean_A = com.espertech.esper.common.@internal.support.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqUncorrel
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withn(execs); // testNoShare
            return execs;
        }

        public static IList<RegressionExecution> Withn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            // named window tests
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, false, false));
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, true, false));
            execs.Add(new InfraNWTableSubqUncorrelAssertion(true, true, true));
            // table tests
            execs.Add(new InfraNWTableSubqUncorrelAssertion(false, false, false));
            return execs;
        }

        private class InfraNWTableSubqUncorrelAssertion : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly bool enableIndexShareCreate;
            private readonly bool disableIndexShareConsumer;

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
                    ? "@name('create') @public create window MyInfra#keepall as select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean"
                    : "@name('create') @public create table MyInfra(a string primary key, b long, c long)";
                if (enableIndexShareCreate) {
                    stmtTextCreate = "@Hint('enable_window_subquery_indexshare') " + stmtTextCreate;
                }

                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfra select TheString as a, LongPrimitive as b, LongBoxed as c from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var stmtTextSelectOne =
                    "@name('select') select irstream (select a from MyInfra) as value, Symbol from SupportMarketDataBean";
                if (disableIndexShareConsumer) {
                    stmtTextSelectOne = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectOne;
                }

                env.CompileDeploy(stmtTextSelectOne, path).AddListener("select");
                env.AssertStatement(
                    "select",
                    statement => {
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            statement.EventType.PropertyNames,
                            new string[] { "value", "Symbol" });
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("value"));
                        Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("Symbol"));
                    });

                SendMarketBean(env, "M1");
                var fieldsStmt = new string[] { "value", "Symbol" };
                env.AssertPropsNew("select", fieldsStmt, new object[] { null, "M1" });

                SendSupportBean(env, "S1", 1L, 2L);
                env.AssertListenerNotInvoked("select");
                var fieldsWin = new string[] { "a", "b", "c" };
                if (namedWindow) {
                    env.AssertPropsNew("create", fieldsWin, new object[] { "S1", 1L, 2L });
                }
                else {
                    env.AssertListenerNotInvoked("create");
                }

                // create consumer 2 -- note that this one should not start empty now
                var stmtTextSelectTwo =
                    "@name('selectTwo') select irstream (select a from MyInfra) as value, Symbol from SupportMarketDataBean";
                if (disableIndexShareConsumer) {
                    stmtTextSelectTwo = "@Hint('disable_window_subquery_indexshare') " + stmtTextSelectTwo;
                }

                env.CompileDeploy(stmtTextSelectTwo, path).AddListener("selectTwo");

                SendMarketBean(env, "M1");
                env.AssertPropsNew("select", fieldsStmt, new object[] { "S1", "M1" });
                env.AssertPropsNew("selectTwo", fieldsStmt, new object[] { "S1", "M1" });

                SendSupportBean(env, "S2", 10L, 20L);
                env.AssertListenerNotInvoked("select");
                if (namedWindow) {
                    env.AssertPropsNew("create", fieldsWin, new object[] { "S2", 10L, 20L });
                }

                SendMarketBean(env, "M2");
                env.AssertPropsNew("select", fieldsStmt, new object[] { null, "M2" });
                env.AssertListenerNotInvoked("create");
                env.AssertPropsNew("selectTwo", fieldsStmt, new object[] { null, "M2" });

                // create delete stmt
                var stmtTextDelete = "@name('delete') on SupportBean_A delete from MyInfra where Id = a";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // delete S1
                env.SendEventBean(new SupportBean_A("S1"));
                if (namedWindow) {
                    env.AssertPropsOld("create", fieldsWin, new object[] { "S1", 1L, 2L });
                }

                SendMarketBean(env, "M3");
                env.AssertPropsNew("select", fieldsStmt, new object[] { "S2", "M3" });
                env.AssertPropsNew("selectTwo", fieldsStmt, new object[] { "S2", "M3" });

                // delete S2
                env.SendEventBean(new SupportBean_A("S2"));
                if (namedWindow) {
                    env.AssertPropsOld("create", fieldsWin, new object[] { "S2", 10L, 20L });
                }

                SendMarketBean(env, "M4");
                env.AssertPropsNew("select", fieldsStmt, new object[] { null, "M4" });
                env.AssertPropsNew("selectTwo", fieldsStmt, new object[] { null, "M4" });

                SendSupportBean(env, "S3", 100L, 200L);
                if (namedWindow) {
                    env.AssertPropsNew("create", fieldsWin, new object[] { "S3", 100L, 200L });
                }

                SendMarketBean(env, "M5");
                env.AssertPropsNew("select", fieldsStmt, new object[] { "S3", "M5" });
                env.AssertPropsNew("selectTwo", fieldsStmt, new object[] { "S3", "M5" });

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", enableIndexShareCreate=" +
                       enableIndexShareCreate +
                       ", disableIndexShareConsumer=" +
                       disableIndexShareConsumer +
                       '}';
            }
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
    }
} // end of namespace