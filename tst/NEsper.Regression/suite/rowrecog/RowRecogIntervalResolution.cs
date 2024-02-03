///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogIntervalResolution : RegressionExecution
    {
        private readonly long flipTime;

        public RowRecogIntervalResolution(long flipTime)
        {
            this.flipTime = flipTime;
        }

        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(0);

            var text = "@name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       " measures A as a" +
                       " pattern (A*)" +
                       " interval 10 seconds" +
                       ")";
            env.CompileDeploy(text).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 1));

            env.AdvanceTime(flipTime - 1);
            env.AssertListenerNotInvoked("s0");

            env.Milestone(0);

            env.AdvanceTime(flipTime);
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        public string Name()
        {
            return "RowRecogIntervalResolution{" +
                   "flipTime=" +
                   flipTime +
                   '}';
        }
    }
} // end of namespace