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

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowOutputrate : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window MyWindowOne#keepall as (theString string, intv int)", path);
            env.CompileDeploy("insert into MyWindowOne select TheString, intPrimitive as intv from SupportBean", path);

            env.AdvanceTime(0);

            string[] fields = {"TheString", "c"};
            env.CompileDeploy(
                    "@Name('s0') select irstream theString, count(*) as c from MyWindowOne group by TheString output snapshot every 1 second",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean("A", 1));
            env.SendEventBean(new SupportBean("A", 2));
            env.SendEventBean(new SupportBean("B", 4));

            env.AdvanceTime(1000);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A", 2L}, new object[] {"B", 1L}});

            env.SendEventBean(new SupportBean("B", 5));
            env.AdvanceTime(2000);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A", 2L}, new object[] {"B", 2L}});

            env.AdvanceTime(3000);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A", 2L}, new object[] {"B", 2L}});

            env.SendEventBean(new SupportBean("A", 5));
            env.SendEventBean(new SupportBean("C", 1));
            env.AdvanceTime(4000);

            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                fields,
                new[] {new object[] {"A", 3L}, new object[] {"B", 2L}, new object[] {"C", 1L}});

            env.UndeployAll();
        }
    }
} // end of namespace