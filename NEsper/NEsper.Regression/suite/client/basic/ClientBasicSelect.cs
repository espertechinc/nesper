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
    public class ClientBasicSelect : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select * from SupportBean";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.SendEventBean(new SupportBean())
                .Listener("s0")
                .AssertInvokedAndReset();
            env.Milestone(1);

            env.UndeployAll();
        }
    }
} // end of namespace