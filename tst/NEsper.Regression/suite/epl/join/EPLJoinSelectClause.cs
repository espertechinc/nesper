///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinSelectClause : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select S0.DoubleBoxed, S1.IntPrimitive*S1.IntBoxed/2.0 as div from " +
                      "SupportBean(TheString='s0')#length(3) as S0," +
                      "SupportBean(TheString='s1')#length(3) as S1" +
                      " where S0.DoubleBoxed = S1.DoubleBoxed";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            var result = env.Statement("s0").EventType;
            Assert.AreEqual(typeof(double?), result.GetPropertyType("S0.DoubleBoxed"));
            Assert.AreEqual(typeof(double?), result.GetPropertyType("div"));
            Assert.AreEqual(2, env.Statement("s0").EventType.PropertyNames.Length);

            Assert.IsNull(env.Listener("s0").LastNewData);

            SendEvent(env, "s0", 1, 4, 5);

            env.Milestone(1);

            SendEvent(env, "s1", 1, 3, 2);
            var newEvents = env.Listener("s0").LastNewData;
            Assert.AreEqual(1d, newEvents[0].Get("S0.DoubleBoxed"));
            Assert.AreEqual(3d, newEvents[0].Get("div"));

            env.Milestone(2);

            var iterator = env.Statement("s0").GetEnumerator();
            var theEvent = iterator.Advance();
            Assert.AreEqual(1d, theEvent.Get("S0.DoubleBoxed"));
            Assert.AreEqual(3d, theEvent.Get("div"));

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            double doubleBoxed,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.DoubleBoxed = doubleBoxed;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }
    }
} // end of namespace