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

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherLiteralConstants : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statement = "@name('s0') select 0x23 as mybyte, " +
                            "'\u0041' as myunicode," +
                            "08 as zero8, " +
                            "09 as zero9, " +
                            "008 as zeroZero8 " +
                            "from SupportBean";
            env.CompileDeploy(statement).AddListener("s0");

            env.SendEventBean(new SupportBean("e1", 100));

            env.AssertPropsNew(
                "s0",
                new[] {"mybyte", "myunicode", "zero8", "zero9", "zeroZero8"},
                new object[] {(byte) 35, "A", 8, 9, 8});

            env.UndeployAll();
        }
    }
} // end of namespace