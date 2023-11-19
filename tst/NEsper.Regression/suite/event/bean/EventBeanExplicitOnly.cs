///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanExplicitOnly : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var statementText = "@name('s0') select " +
                                "explicitFNested.fieldNestedClassValue as fnested, " +
                                "explicitMNested.readNestedClassValue as mnested" +
                                " from MyLegacyEvent#length(5)";
            env.CompileDeploy(statementText).AddListener("s0");

            env.AssertStatement(
                "s0",
                statement => {
                    var eventType = statement.EventType;
                    Assert.AreEqual(typeof(string), eventType.GetPropertyType("fnested"));
                    Assert.AreEqual(typeof(string), eventType.GetPropertyType("mnested"));
                });

            var legacyBean = EventBeanPublicAccessors.MakeSampleEvent();
            env.SendEventBean(legacyBean, "MyLegacyEvent");


            env.AssertEventNew(
                "s0",
                @event => {
                    Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), @event.Get("fnested"));
                    Assert.AreEqual(legacyBean.fieldNested.ReadNestedValue(), @event.Get("mnested"));
                });

            env.TryInvalidCompile("select IntPrimitive from MySupportBean#length(5)", "skip");

            env.UndeployAll();
        }
    }
} // end of namespace