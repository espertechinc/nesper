///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanNativeAccessor : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@Name('s0') select IntPrimitive, explicitFInt, explicitMGetInt, explicitMReadInt " +
                                " from MyLegacyTwo#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var eventType = env.Statement("s0").EventType;

            var theEvent = new SupportLegacyBeanInt(10);
            env.SendEventBean(theEvent, "MyLegacyTwo");

            foreach (var name in new[] {"IntPrimitive", "explicitFInt", "explicitMGetInt", "explicitMReadInt"}) {
                Assert.AreEqual(typeof(int?), eventType.GetPropertyType(name));
                Assert.AreEqual(10, env.Listener("s0").LastNewData[0].Get(name));
            }

            env.UndeployAll();
        }
    }
} // end of namespace