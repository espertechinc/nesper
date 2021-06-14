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
    public class EPLOtherIStreamRStreamConfigSelectorRStream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var stmtText = "@Name('s0') select * from SupportBean#length(3)";
            env.CompileDeploy(stmtText).AddListener("s0");

            var theEvent = SendEvent(env, "a");
            SendEvent(env, "b");
            SendEvent(env, "c");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            SendEvent(env, "d");
            Assert.IsTrue(env.Listener("s0").IsInvoked);
            Assert.AreSame(theEvent, env.Listener("s0").LastNewData[0].Underlying); // receive 'a' as new data
            Assert.IsNull(env.Listener("s0").LastOldData); // receive no more old data

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