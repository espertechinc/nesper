///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinSelectClause : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('s0') select s0.DoubleBoxed, s1.IntPrimitive*s1.IntBoxed/2.0 as div from " +
                      "SupportBean(TheString='s0')#length(3) as s0," +
                      "SupportBean(TheString='s1')#length(3) as s1" +
                      " where s0.DoubleBoxed = s1.DoubleBoxed";
            env.CompileDeployAddListenerMileZero(epl, "s0");

            env.AssertStatement(
                "s0",
                statement => {
                    var result = statement.EventType;
                    Assert.AreEqual(typeof(double?), result.GetPropertyType("s0.DoubleBoxed"));
                    Assert.AreEqual(typeof(double?), result.GetPropertyType("div"));
                    Assert.AreEqual(2, statement.EventType.PropertyNames.Length);
                });
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "s0", 1, 4, 5);

            env.Milestone(1);

            SendEvent(env, "s1", 1, 3, 2);
            env.AssertListener(
                "s0",
                listener => {
                    var newEvents = listener.LastNewData;
                    Assert.AreEqual(1d, newEvents[0].Get("s0.DoubleBoxed"));
                    Assert.AreEqual(3d, newEvents[0].Get("div"));
                });

            env.Milestone(2);

            env.AssertIterator(
                "s0",
                iterator => {
                    var theEvent = iterator.Advance();
                    Assert.AreEqual(1d, theEvent.Get("s0.DoubleBoxed"));
                    Assert.AreEqual(3d, theEvent.Get("div"));
                });

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