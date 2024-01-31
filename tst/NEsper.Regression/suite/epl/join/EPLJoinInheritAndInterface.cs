///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinInheritAndInterface : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select A, B from ISupportA#length(10), ISupportB#length(10) where A = B";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.SendEventBean(new ISupportAImpl("1", "ab1"));
            env.SendEventBean(new ISupportBImpl("2", "ab2"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new ISupportBImpl("1", "ab3"));

            env.AssertListener(
                "s0",
                listener => {
                    ClassicAssert.IsTrue(listener.IsInvoked);
                    var theEvent = listener.GetAndResetLastNewData()[0];
                    ClassicAssert.AreEqual("1", theEvent.Get("A"));
                    ClassicAssert.AreEqual("1", theEvent.Get("B"));
                });

            env.UndeployAll();
        }
    }
} // end of namespace