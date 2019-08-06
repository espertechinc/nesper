///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreConcat : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl =
                "@Name('s0') select P00 || P01 as c1, P00 || P01 || p02 as c2, P00 || '|' || P01 as c3 from SupportBean_S0";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean_S0(1, "a", "b", "c"));
            AssertConcat(env, "ab", "abc", "a|b");

            env.SendEventBean(new SupportBean_S0(1, null, "b", "c"));
            AssertConcat(env, null, null, null);

            env.SendEventBean(new SupportBean_S0(1, "", "b", "c"));
            AssertConcat(env, "b", "bc", "|b");

            env.SendEventBean(new SupportBean_S0(1, "123", null, "c"));
            AssertConcat(env, null, null, null);

            env.SendEventBean(new SupportBean_S0(1, "123", "456", "c"));
            AssertConcat(env, "123456", "123456c", "123|456");

            env.SendEventBean(new SupportBean_S0(1, "123", "456", null));
            AssertConcat(env, "123456", null, "123|456");

            env.UndeployAll();
        }

        private void AssertConcat(
            RegressionEnvironment env,
            string c1,
            string c2,
            string c3)
        {
            var theEvent = env.Listener("s0").LastNewData[0];
            Assert.AreEqual(c1, theEvent.Get("c1"));
            Assert.AreEqual(c2, theEvent.Get("c2"));
            Assert.AreEqual(c3, theEvent.Get("c3"));
            env.Listener("s0").Reset();
        }
    }
} // end of namespace