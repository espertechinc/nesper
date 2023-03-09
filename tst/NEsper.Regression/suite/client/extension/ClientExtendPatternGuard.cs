///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendPatternGuard : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionGuard(env);
            RunAssertionGuardVariable(env);
            RunAssertionInvalid(env);
        }

        private void RunAssertionGuard(RegressionEnvironment env)
        {
            if (env.IsHA) {
                return;
            }

            var stmtText = "@Name('s0') select * from pattern [(every SupportBean) where myplugin:count_to(10)]";
            env.CompileDeploy(stmtText).AddListener("s0");

            for (var i = 0; i < 10; i++) {
                env.SendEventBean(new SupportBean());
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            }

            env.SendEventBean(new SupportBean());
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private void RunAssertionGuardVariable(RegressionEnvironment env)
        {
            if (env.IsHA) {
                return;
            }

            var path = new RegressionPath();
            env.CompileDeploy("create variable int COUNT_TO = 3", path);
            var stmtText = "@Name('s0') select * from pattern [(every SupportBean) where myplugin:count_to(COUNT_TO)]";
            env.CompileDeploy(stmtText, path).AddListener("s0");

            for (var i = 0; i < 3; i++) {
                env.SendEventBean(new SupportBean());
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            }

            env.SendEventBean(new SupportBean());
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private void RunAssertionInvalid(RegressionEnvironment env)
        {
            TryInvalidCompile(
                env,
                "select * from pattern [every SupportBean where namespace:name(10)]",
                "Failed to resolve pattern guard 'SupportBean where namespace:name(10)': Error casting guard forge instance to " +
                nameof(GuardForge) +
                " interface for guard 'name'");
        }
    }
} // end of namespace