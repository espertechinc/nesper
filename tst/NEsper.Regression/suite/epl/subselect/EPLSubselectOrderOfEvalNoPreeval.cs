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

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectOrderOfEvalNoPreeval : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl =
                "@name('s0') @Name('s0')select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean#unique(IntPrimitive))";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.SendEventBean(new SupportBean("E1", 5));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();

            var eplTwo =
                "@name('s0') select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10)#unique(IntPrimitive))";
            env.CompileDeployAddListenerMile(eplTwo, "s0", 1);

            env.SendEventBean(new SupportBean("E1", 5));
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }
    }
} // end of namespace