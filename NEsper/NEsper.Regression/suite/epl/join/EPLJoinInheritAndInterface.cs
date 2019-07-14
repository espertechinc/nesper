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

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinInheritAndInterface : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select a, b from ISupportA#length(10), ISupportB#length(10) where a = b";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.SendEventBean(new ISupportAImpl("1", "ab1"));
            env.SendEventBean(new ISupportBImpl("2", "ab2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new ISupportBImpl("1", "ab3"));
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            var theEvent = env.Listener("s0").GetAndResetLastNewData()[0];
            Assert.AreEqual("1", theEvent.Get("a"));
            Assert.AreEqual("1", theEvent.Get("b"));

            env.UndeployAll();
        }
    }
} // end of namespace