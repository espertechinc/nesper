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

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherIStreamRStreamConfigSelectorIRStream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@Name('s0') select * from SupportBean#length(3)";
            env.CompileDeploy(stmtText).AddListener("s0");

            var eventOld = SendEvent(env, "a");
            SendEvent(env, "b");
            SendEvent(env, "c");
            env.Listener("s0").Reset();

            var eventNew = SendEvent(env, "d");
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            Assert.AreSame(eventNew, env.Listener("s0").LastNewData[0].Underlying); // receive 'a' as new data
            Assert.AreSame(eventOld, env.Listener("s0").LastOldData[0].Underlying); // receive 'a' as new data

            env.UndeployAll();
        }

        private object SendEvent(
            RegressionEnvironment env,
            string stringValue)
        {
            var theEvent = new SupportBean(stringValue, 0);
            env.SendEventBean(theEvent);
            return theEvent;
        }
    }
} // end of namespace