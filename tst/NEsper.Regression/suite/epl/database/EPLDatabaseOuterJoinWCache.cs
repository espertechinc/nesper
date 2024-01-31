///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseOuterJoinWCache : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@name('s0') select * from SupportBean as sb " +
                           "left outer join " +
                           "sql:MyDBWithExpiryTime ['select myint from mytesttable'] as t " +
                           "on sb.IntPrimitive = t.myint " +
                           "where myint is null";
            env.CompileDeploy(stmtText).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", -1));
            env.AssertListenerInvoked("s0");

            env.SendEventBean(new SupportBean("E2", 10));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("E1", 1));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace