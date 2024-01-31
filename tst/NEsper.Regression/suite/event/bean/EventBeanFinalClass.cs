///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanFinalClass : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@name('s0') select IntPrimitive from MyFinalEvent#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var theEvent = new SupportBeanFinal(10);
            env.SendEventBean(theEvent, "MyFinalEvent");
            env.AssertEqualsNew("s0", "IntPrimitive", 10);

            env.UndeployAll();
        }
    }
} // end of namespace