///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.outputlimit
{
    public class ResultSetOutputLimitMicrosecondResolution : RegressionExecution
    {
        private readonly long flipTime;
        private readonly long repeatTime;
        private readonly string size;

        private readonly long startTime;

        public ResultSetOutputLimitMicrosecondResolution(
            long startTime,
            string size,
            long flipTime,
            long repeatTime)
        {
            this.startTime = startTime;
            this.size = size;
            this.flipTime = flipTime;
            this.repeatTime = repeatTime;
        }

        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(startTime);
            var epl = "@name('s0') select * from SupportBean output every " + size + " seconds";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));
            env.AdvanceTime(flipTime - 1);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(flipTime);
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("E2", 10));
            env.AdvanceTime(repeatTime + flipTime - 1);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(repeatTime + flipTime);
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace