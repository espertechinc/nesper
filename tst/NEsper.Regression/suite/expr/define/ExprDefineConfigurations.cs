///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineConfigurations : RegressionExecution
    {
        private readonly int expectedInvocationCount;

        public ExprDefineConfigurations(int expectedInvocationCount)
        {
            this.expectedInvocationCount = expectedInvocationCount;
        }

        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                "@Name('s0') expression myExpr {v -> alwaysTrue(null) } " +
                "select myExpr(st0) as c0, myExpr(st1) as c1, myExpr(st0) as c2, myExpr(st1) as c3 from SupportBean_ST0#lastevent as st0, SupportBean_ST1#lastevent as st1");
            env.AddListener("s0");

            // send event and assert
            SupportStaticMethodLib.Invocations.Clear();
            env.SendEventBean(new SupportBean_ST0("a", 0));
            env.SendEventBean(new SupportBean_ST1("a", 0));
            Assert.AreEqual(expectedInvocationCount, SupportStaticMethodLib.Invocations.Count);

            env.UndeployAll();
        }
    }
} // end of namespace