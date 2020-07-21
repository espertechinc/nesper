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
    public class InfraNamedWindowRemoveStream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();

            string[] fields = {"TheString"};
            env.CompileDeploy("@name('c1') create window W1#length(2) as select * from SupportBean", path);
            env.CompileDeploy("@name('c2') create window W2#length(2) as select * from SupportBean", path);
            env.CompileDeploy("@name('c3') create window W3#length(2) as select * from SupportBean", path);

            env.CompileDeploy("insert into W1 select * from SupportBean", path);
            env.CompileDeploy("insert rstream into W2 select rstream * from W1", path);
            env.CompileDeploy("insert rstream into W3 select rstream * from W2", path);

            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c1"),
                fields,
                new[] {new object[] {"E1"}, new object[] {"E2"}});

            env.SendEventBean(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c1"),
                fields,
                new[] {new object[] {"E2"}, new object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c2"),
                fields,
                new[] {new object[] {"E1"}});

            env.SendEventBean(new SupportBean("E4", 1));
            env.SendEventBean(new SupportBean("E5", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c1"),
                fields,
                new[] {new object[] {"E4"}, new object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c2"),
                fields,
                new[] {new object[] {"E2"}, new object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("c3"),
                fields,
                new[] {new object[] {"E1"}});

            env.UndeployAll();
        }
    }
} // end of namespace