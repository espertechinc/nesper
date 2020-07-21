///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

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
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportTradeEvent(2, "1234", 1001));
            Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("event1.Id"));

            env.UndeployAll();
        }
    }
} // end of namespace