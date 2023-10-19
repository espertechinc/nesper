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

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableWNamedWindow : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@name('var') @public create table varagg (key string primary key, total sum(int))",
                path);
            env.CompileDeploy("@name('win') @public create window MyWindow#keepall as SupportBean", path);
            env.CompileDeploy("@name('insert') insert into MyWindow select * from SupportBean", path);
            env.CompileDeploy(
                "@name('populate') into table varagg select sum(IntPrimitive) as total from MyWindow group by TheString",
                path);
            env.CompileDeploy(
                    "@name('s0') on SupportBean_S0 select TheString, varagg[P00].total as c0 from MyWindow where TheString = P00",
                    path)
                .AddListener("s0");
            var fields = new[] { "TheString", "c0" };

            env.SendEventBean(new SupportBean("E1", 10));

            env.Milestone(0);

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] { "E1", 10 });

            env.UndeployAll();
        }
    }
} // end of namespace