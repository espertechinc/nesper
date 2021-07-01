///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution : RegressionExecution
    {
        private readonly long flipTime;

        public ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(long flipTime)
        {
            this.flipTime = flipTime;
        }

        public void Run(RegressionEnvironment env)
        {
            env.AdvanceTime(0);

            var epl =
                "@Name('s0') @IterableUnbound @Hint('reclaim_group_aged=1,reclaim_group_freq=5') select TheString, count(*) from SupportBean group by TheString";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            AssertCount(env.Statement("s0"), 1);

            env.AdvanceTime(flipTime - 1);
            env.SendEventBean(new SupportBean("E2", 0));
            AssertCount(env.Statement("s0"), 2);

            env.AdvanceTime(flipTime);
            env.SendEventBean(new SupportBean("E3", 0));
            AssertCount(env.Statement("s0"), 2);

            env.UndeployAll();
        }

        private static void AssertCount(
            EPStatement stmt,
            long count)
        {
            Assert.AreEqual(count, EPAssertionUtil.EnumeratorCount(stmt.GetEnumerator()));
        }
    }
} // end of namespace