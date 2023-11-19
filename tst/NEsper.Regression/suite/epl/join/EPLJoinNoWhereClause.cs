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

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinNoWhereClause
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithWInnerKeywordWOOnClause(execs);
            With(NoWhereClause)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithNoWhereClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinNoWhereClause());
            return execs;
        }

        public static IList<RegressionExecution> WithWInnerKeywordWOOnClause(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinJoinWInnerKeywordWOOnClause());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            SendEvent(env, new SupportBean(theString, intPrimitive));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        internal class EPLJoinJoinWInnerKeywordWOOnClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "a.TheString", "b.TheString" };
                var epl =
                    "@name('s0') select * from SupportBean(TheString like 'A%')#length(3) as a inner join SupportBean(TheString like 'B%')#length(3) as b " +
                    "where a.IntPrimitive = b.IntPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "A1", 1);
                SendEvent(env, "A2", 2);
                SendEvent(env, "A3", 3);
                SendEvent(env, "B2", 2);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { "A2", "B2" });

                env.UndeployAll();
            }
        }

        internal class EPLJoinJoinNoWhereClause : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = { "stream_0.Volume", "stream_1.LongBoxed" };
                var joinStatement = "@name('s0') select * from " +
                                    "SupportMarketDataBean#length(3)," +
                                    "SupportBean#length(3)";
                env.CompileDeploy(joinStatement).AddListener("s0");

                var setOne = new object[5];
                var setTwo = new object[5];
                for (var i = 0; i < setOne.Length; i++) {
                    setOne[i] = new SupportMarketDataBean("IBM", 0, i, "");

                    var theEvent = new SupportBean();
                    theEvent.LongBoxed = i;
                    setTwo[i] = theEvent;
                }

                // Send 2 events, should join on second one
                SendEvent(env, setOne[0]);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendEvent(env, setTwo[0]);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(1, listener.LastNewData.Length);
                        Assert.AreEqual(setOne[0], listener.LastNewData[0].Get("stream_0"));
                        Assert.AreEqual(setTwo[0], listener.LastNewData[0].Get("stream_1"));
                        listener.Reset();
                    });

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new[] { new object[] { 0L, 0L } });

                SendEvent(env, setOne[1]);
                SendEvent(env, setOne[2]);
                SendEvent(env, setTwo[1]);
                env.AssertListener("s0", listener => Assert.AreEqual(3, listener.LastNewData.Length));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new[] {
                        new object[] { 0L, 0L },
                        new object[] { 1L, 0L },
                        new object[] { 2L, 0L },
                        new object[] { 0L, 1L },
                        new object[] { 1L, 1L },
                        new object[] { 2L, 1L }
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace