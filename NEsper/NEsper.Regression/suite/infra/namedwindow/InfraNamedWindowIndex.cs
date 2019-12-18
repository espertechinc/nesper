///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowIndex : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('window') create window MyWindowOne#unique(TheString) as SupportBean;\n" +
                      "insert into MyWindowOne select * from SupportBean;\n" +
                      "@Name('idx') create unique index I1 on MyWindowOne(TheString);\n";
            env.CompileDeploy(epl);
            Assert.AreEqual(
                StatementType.CREATE_INDEX,
                env.Statement("idx").GetProperty(StatementProperty.STATEMENTTYPE));

            env.SendEventBean(new SupportBean("E0", 1));
            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportBean("E2", 3));
            env.SendEventBean(new SupportBean("E1", 4));
            env.SendEventBean(new SupportBean("E0", 5));

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("window"),
                new [] { "TheString","IntPrimitive" },
                new[] {new object[] {"E0", 5}, new object[] {"E1", 4}, new object[] {"E2", 3}});

            env.UndeployAll();
        }
    }
} // end of namespace