///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.basic
{
    public class ClientBasicSelectClause : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select IntPrimitive from SupportBean";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.SendEventBean(new SupportBean("E1", 10));

            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "IntPrimitive".SplitCsv(),
                new object[] {10});

            env.UndeployAll();
        }
    }
} // end of namespace