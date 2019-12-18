///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicPattern : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var compiled = env.Compile("@Name('s0') select * from pattern[timer:interval(10)]");

            env.AdvanceTime(0);

            env.Deploy(compiled).AddListener("s0").Milestone(0);

            env.AdvanceTime(9999);
            Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

            env.Milestone(1);

            env.AdvanceTime(10000);
            Assert.IsTrue(env.Listener("s0").IsInvokedAndReset());

            env.Milestone(2);

            env.AdvanceTime(9999999);
            Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

            env.UndeployAll();
        }
    }
} // end of namespace