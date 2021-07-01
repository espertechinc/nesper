///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanExplicitOnly : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@Name('s0') select " +
                                "explicitFNested.fieldNestedClassValue as fnested, " +
                                "explicitMNested.readNestedClassValue as mnested" +
                                " from MyLegacyEvent#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            var eventType = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("fnested"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mnested"));

            var legacyBean = EventBeanPublicAccessors.MakeSampleEvent();
            env.SendEventBean(legacyBean, "MyLegacyEvent");

            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), env.Listener("s0").LastNewData[0].Get("fnested"));
            Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), env.Listener("s0").LastNewData[0].Get("mnested"));

            TryInvalidCompile(env, "select IntPrimitive from MySupportBean#length(5)", "skip");

            env.UndeployAll();
        }
    }
} // end of namespace