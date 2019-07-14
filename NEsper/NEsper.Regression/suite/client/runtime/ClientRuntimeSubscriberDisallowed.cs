///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeSubscriberDisallowed : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy("@Name('s0') select * from SupportBean");

            var stmt = env.Statement("s0");
            TryInvalid(() => stmt.SetSubscriber(new SupportSubscriberMRD()));
            TryInvalid(() => stmt.SetSubscriber(new SupportSubscriberMRD(), "update"));

            env.UndeployAll();
        }

        private static void TryInvalid(Runnable r)
        {
            try {
                r.Invoke();
                Assert.Fail();
            }
            catch (EPSubscriberException ex) {
                AssertMessage(
                    ex,
                    "Setting a subscriber is not allowed for the statement, the statement has been compiled with allowSubscriber=false");
            }
        }
    }
} // end of namespace