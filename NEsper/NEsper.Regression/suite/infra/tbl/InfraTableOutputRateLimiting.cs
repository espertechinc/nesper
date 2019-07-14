///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableOutputRateLimiting : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var currentTime = new AtomicLong(0);
            env.AdvanceTime(currentTime.Get());
            var path = new RegressionPath();

            env.CompileDeploy(
                "@Name('create') create table MyTable as (\n" +
                "key string primary key, thesum sum(int))",
                path);
            env.CompileDeploy(
                "@Name('intotable') into table MyTable " +
                "select sum(IntPrimitive) as thesum from SupportBean group by TheString",
                path);

            env.SendEventBean(new SupportBean("E1", 10));
            env.SendEventBean(new SupportBean("E2", 20));

            env.Milestone(0);

            env.SendEventBean(new SupportBean("E1", 30));
            env.UndeployModuleContaining("intotable");

            env.CompileDeploy("@Name('s0') select key, thesum from MyTable output snapshot every 1 seconds", path)
                .AddListener("s0");

            currentTime.Set(currentTime.Get() + 1000L);
            env.AdvanceTime(currentTime.Get());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                "key,thesum".SplitCsv(),
                new[] {new object[] {"E1", 40}, new object[] {"E2", 20}});

            env.Milestone(1);

            currentTime.Set(currentTime.Get() + 1000L);
            env.AdvanceTime(currentTime.Get());
            Assert.IsTrue(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }
    }
} // end of namespace