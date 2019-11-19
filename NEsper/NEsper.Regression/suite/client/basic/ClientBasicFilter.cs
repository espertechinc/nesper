///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicFilter : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select * from SupportBean(IntPrimitive = 1)";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            SendAssert(env, 1, true);
            SendAssert(env, 0, false);

            env.Milestone(1);

            SendAssert(env, 1, true);
            SendAssert(env, 0, false);

            env.UndeployAll();
        }

        private void SendAssert(
            RegressionEnvironment env,
            int intPrimitive,
            bool expected)
        {
            env.SendEventBean(new SupportBean("E", intPrimitive))
                .Listener("s0")
                .AssertInvokedFlagAndReset(expected);
        }
    }
} // end of namespace