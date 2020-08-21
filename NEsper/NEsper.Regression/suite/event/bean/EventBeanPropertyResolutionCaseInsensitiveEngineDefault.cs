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

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyResolutionCaseInsensitiveEngineDefault : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TryCaseInsensitive(
                env,
                "BeanWCIED",
                "@Name('s0') select THESTRING, INTPRIMITIVE from BeanWCIED where THESTRING='A'",
                "THESTRING",
                "INTPRIMITIVE");
            TryCaseInsensitive(
                env,
                "BeanWCIED",
                "@Name('s0') select ThEsTrInG, INTprimitIVE from BeanWCIED where THESTRing='A'",
                "ThEsTrInG",
                "INTprimitIVE");
        }

        public static void TryCaseInsensitive(
            RegressionEnvironment env,
            string eventTypeName,
            string stmtText,
            string propOneName,
            string propTwoName)
        {
            env.CompileDeploy(stmtText).AddListener("s0");

            env.SendEventBean(new SupportBean("A", 10), eventTypeName);
            var result = env.Listener("s0").AssertOneGetNewAndReset();
            Assert.AreEqual("A", result.Get(propOneName));
            Assert.AreEqual(10, result.Get(propTwoName));

            env.UndeployAll();
        }
    }
} // end of namespace