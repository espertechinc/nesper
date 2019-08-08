///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinMultiKeyAndRange
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinRangeNullAndDupAndInvalid());
            execs.Add(new EPLJoinMultiKeyed());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int? intBoxed)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
            return bean;
        }

        internal class EPLJoinRangeNullAndDupAndInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@Name('s0') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where IntBoxed between RangeStart and RangeEnd";
                env.CompileDeploy(eplOne).AddListener("s0");

                var eplTwo =
                    "@Name('s1') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where TheString = key and IntBoxed in [RangeStart: RangeEnd]";
                env.CompileDeploy(eplTwo).AddListener("s1");

                // null join lookups
                SendEvent(env, new SupportBeanRange("R1", "G", (int?) null, null));
                SendEvent(env, new SupportBeanRange("R2", "G", null, 10));
                SendEvent(env, new SupportBeanRange("R3", "G", 10, null));
                SendSupportBean(env, "G", -1, null);

                // range invalid
                SendEvent(env, new SupportBeanRange("R4", "G", 10, 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Listener("s1").IsInvoked);

                // duplicates
                object eventOne = SendSupportBean(env, "G", 100, 5);
                object eventTwo = SendSupportBean(env, "G", 101, 5);
                SendEvent(env, new SupportBeanRange("R4", "G", 0, 10));
                var events = env.Listener("s0").GetAndResetLastNewData();
                EPAssertionUtil.AssertEqualsAnyOrder(new[] {eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));
                events = env.Listener("s1").GetAndResetLastNewData();
                EPAssertionUtil.AssertEqualsAnyOrder(new[] {eventOne, eventTwo}, EPAssertionUtil.GetUnderlying(events));

                // test string compare
                var eplThree =
                    "@Name('s2') select sb.* from SupportBeanRange#keepall sb, SupportBean#lastevent where TheString in [RangeStartStr:RangeEndStr]";
                env.CompileDeploy(eplThree).AddListener("s2");

                SendSupportBean(env, "P", 1, 1);
                SendEvent(env, new SupportBeanRange("R5", "R5", "O", "Q"));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLJoinMultiKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean(TheString='A')#length(3) as streamA," +
                                    "SupportBean(TheString='B')#length(3) as streamB" +
                                    " where streamA.IntPrimitive = streamB.IntPrimitive " +
                                    "and streamA.IntBoxed = streamB.IntBoxed";
                env.CompileDeploy(joinStatement).AddListener("s0");

                Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("streamA"));
                Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("streamB"));
                Assert.AreEqual(2, env.Statement("s0").EventType.PropertyNames.Length);

                int[][] eventData = {
                    new[] {1, 100},
                    new[] {2, 100},
                    new[] {1, 200},
                    new[] {2, 200}
                };
                var eventsA = new SupportBean[eventData.Length];
                var eventsB = new SupportBean[eventData.Length];

                for (var i = 0; i < eventData.Length; i++) {
                    eventsA[i] = new SupportBean();
                    eventsA[i].TheString = "A";
                    eventsA[i].IntPrimitive = eventData[i][0];
                    eventsA[i].IntBoxed = eventData[i][1];

                    eventsB[i] = new SupportBean();
                    eventsB[i].TheString = "B";
                    eventsB[i].IntPrimitive = eventData[i][0];
                    eventsB[i].IntBoxed = eventData[i][1];
                }

                SendEvent(env, eventsA[0]);
                SendEvent(env, eventsB[1]);
                SendEvent(env, eventsB[2]);
                SendEvent(env, eventsB[3]);
                Assert.IsNull(env.Listener("s0").LastNewData); // No events expected

                env.UndeployAll();
            }
        }
    }
} // end of namespace