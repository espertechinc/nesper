///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinNoTableName : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var joinStatement = "@name('s0') select * from " +
                                "SupportMarketDataBean#length(3)," +
                                "SupportBean#length(3)" +
                                " where Symbol=TheString and Volume=LongBoxed";
            env.CompileDeploy(joinStatement).AddListener("s0");

            var setOne = new object[5];
            var setTwo = new object[5];

            for (var i = 0; i < setOne.Length; i++) {
                setOne[i] = new SupportMarketDataBean("IBM", 0, i, "");

                var theEvent = new SupportBean();
                theEvent.TheString = "IBM";
                theEvent.LongBoxed = i;
                setTwo[i] = theEvent;
            }

            SendEvent(env, setOne[0]);
            SendEvent(env, setTwo[0]);
            env.AssertListener("s0", listener => ClassicAssert.IsNotNull(listener.LastNewData));

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace