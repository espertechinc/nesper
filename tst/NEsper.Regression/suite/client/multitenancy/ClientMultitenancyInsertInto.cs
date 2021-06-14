///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.multitenancy
{
    public class ClientMultitenancyInsertInto
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyInsertIntoSingleModuleTwoStatements());
            execs.Add(new ClientMultitenancyInsertIntoTwoModule());
            return execs;
        }

        public class ClientMultitenancyInsertIntoSingleModuleTwoStatements : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') insert into SomeStream select TheString, IntPrimitive from SupportBean;" +
                    "@Name('s1') select TheString, IntPrimitive from SomeStream(IntPrimitive = 0)";
                var compiled = env.Compile(epl);

                env.Deploy(compiled).AddListener("s1").Milestone(0);

                SendAssert(env, "E1", 0, true);
                SendAssert(env, "E2", 1, false);

                env.Milestone(1);

                SendAssert(env, "E3", 1, false);
                SendAssert(env, "E4", 0, true);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                int intPrimitive,
                bool received)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                if (received) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("s1").AssertOneGetNewAndReset(),
                        new [] { "TheString","IntPrimitive" },
                        new object[] {theString, intPrimitive});
                }
                else {
                    Assert.IsFalse(env.Listener("s1").IsInvoked);
                }
            }
        }

        public class ClientMultitenancyInsertIntoTwoModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var args = new CompilerArguments(env.Configuration);
                args.Options.AccessModifierEventType = ctx => NameAccessModifier.PUBLIC;
                var first = env.Compile(
                    "@Name('s0') insert into SomeStream select TheString as a, IntPrimitive as b from SupportBean",
                    args);
                var second = env.Compile(
                    "@Name('s1') select a, b from SomeStream",
                    new CompilerArguments(env.Configuration).SetPath(new CompilerPath().Add(first)));

                env.Deploy(first).Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));

                env.Deploy(second).AddListener("s1").Milestone(1);

                SendAssert(env, "E2", 2);
                SendAssert(env, "E3", 3);

                env.Milestone(2);

                SendAssert(env, "E4", 4);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                int intPrimitive)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    new [] { "a","b" },
                    new object[] {theString, intPrimitive});
            }
        }
    }
} // end of namespace