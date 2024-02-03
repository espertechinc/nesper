///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherUnaryMinus : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            env.CompileDeploy(
                    "create variable double v = 1.0;\n;" +
                    "@name('s0') select -IntPrimitive as c0, -v as c1 from SupportBean;\n")
                .AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));

            env.AssertThat(
                () => ClassicAssert.AreEqual(1d, env.Runtime.VariableService.GetVariableValue(env.DeploymentId("s0"), "v")));
            env.AssertPropsNew(
                "s0",
                new[] { "c0", "c1" },
                new object[] { -10, -1d });

            env.UndeployAll();
        }
    }
} // end of namespace