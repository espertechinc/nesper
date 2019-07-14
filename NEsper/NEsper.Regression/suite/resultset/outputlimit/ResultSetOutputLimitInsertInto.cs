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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitInsertInto
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ResultSetOutputLimitInsertFirst());
            execs.Add(new ResultSetOutputLimitInsertSnapshot());
            return execs;
        }

        private static void AssertReceivedS0AndS1(
            RegressionEnvironment env,
            object[][] props)
        {
            string[] fields = {"TheString"};
            EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, props);
            EPAssertionUtil.AssertPropsPerRow(env.Listener("s1").GetAndResetDataListsFlattened().First, fields, props);
        }

        internal class ResultSetOutputLimitInsertSnapshot : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                env.CompileDeploy(
                        "@Name('s0') insert into MyStream select * from SupportBean#keepall output snapshot every 1 second;\n" +
                        "@Name('s1') select * from MyStream")
                    .AddListener("s0")
                    .AddListener("s1");

                env.SendEventBean(new SupportBean("E1", 0));

                env.AdvanceTime(1000);
                AssertReceivedS0AndS1(
                    env,
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Listener("s1").IsInvoked);

                env.AdvanceTime(2000);
                AssertReceivedS0AndS1(
                    env,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class ResultSetOutputLimitInsertFirst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                env.CompileDeploy(
                        "@Name('s0') insert into MyStream select * from SupportBean output first every 1 second;\n" +
                        "@Name('s1') select * from MyStream")
                    .AddListener("s0")
                    .AddListener("s1");

                env.SendEventBean(new SupportBean("E1", 0));
                AssertReceivedS0AndS1(
                    env,
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Listener("s1").IsInvoked);

                env.AdvanceTime(1000);

                env.SendEventBean(new SupportBean("E2", 0));
                AssertReceivedS0AndS1(
                    env,
                    new[] {new object[] {"E2"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace