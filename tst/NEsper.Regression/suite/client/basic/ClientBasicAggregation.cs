///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicAggregation : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select count(*) as cnt from SupportBean";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            SendAssert(env, 1);

            env.Milestone(1);

            SendAssert(env, 2);

            env.Milestone(2);

            SendAssert(env, 3);

            env.UndeployAll();
        }

        private void SendAssert(
            RegressionEnvironment env,
            long expected)
        {
            env.SendEventBean(new SupportBean("E1", 0));
            env.AssertPropsNew(
                "s0",
                new[] { "cnt" },
                new object[] { expected });
        }
    }
} // end of namespace