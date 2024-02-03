///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterLargeThreading : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionNoVariable(env);
        }

        private void RunAssertionNoVariable(RegressionEnvironment env)
        {
            var epl =
                "@name('s0') select * from pattern[a=SupportBean -> every event1=SupportTradeEvent(UserId like '123%')]";
            env.CompileDeploy(epl).AddListener("s0").Milestone(0);
            env.SendEventBean(new SupportBean());

            env.SendEventBean(new SupportTradeEvent(1, null, 1001));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportTradeEvent(2, "1234", 1001));
            env.AssertEqualsNew("s0", "event1.Id", 2);

            env.UndeployAll();
        }
    }
} // end of namespace