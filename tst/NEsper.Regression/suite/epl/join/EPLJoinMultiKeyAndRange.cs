///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertNull

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoinMultiKeyAndRange
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRangeNullAndDupAndInvalid(execs);
            WithMultikeyWArrayHashJoinArray(execs);
            WithMultikeyWArrayHashJoin2Prop(execs);
            WithMultikeyWArrayCompositeArray(execs);
            WithMultikeyWArrayComposite2Prop(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayComposite2Prop(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultikeyWArrayComposite2Prop());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayCompositeArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultikeyWArrayCompositeArray());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayHashJoin2Prop(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultikeyWArrayHashJoin2Prop());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayHashJoinArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultikeyWArrayHashJoinArray());
            return execs;
        }

        public static IList<RegressionExecution> WithRangeNullAndDupAndInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRangeNullAndDupAndInvalid());
            return execs;
        }

        private class EPLJoinMultikeyWArrayComposite2Prop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@name('s0') select * " +
                             "from SupportBean_S0#keepall as s0, SupportBean_S1#keepall as s1 " +
                             "where s0.P00 = s1.P10 and s0.P01 = s1.P11 and s0.P02 > s1.P12";
                env.CompileDeploy(eplOne).AddListener("s0");

                SendS0(env, 10, "a0", "b0", "X");
                SendS1(env, 20, "a0", "b0", "F");
                AssertReceived(env, new object[][] { new object[] { 10, 20 } });

                env.Milestone(0);

                SendS0(env, 11, "a1", "b0", "X");
                SendS1(env, 22, "a0", "b1", "F");
                SendS0(env, 12, "a0", "b1", "A");
                env.AssertListenerNotInvoked("s0");

                SendS0(env, 13, "a0", "b1", "Z");
                AssertReceived(env, new object[][] { new object[] { 13, 22 } });

                SendS1(env, 23, "a1", "b0", "A");
                AssertReceived(env, new object[][] { new object[] { 11, 23 } });

                env.UndeployAll();
            }

            private void AssertReceived(
                RegressionEnvironment env,
                object[][] expected)
            {
                var fields = "s0.Id,s1.Id".SplitCsv();
                env.AssertPropsPerRowLastNew("s0", fields, expected);
            }

            private void SendS0(
                RegressionEnvironment env,
                int id,
                string p00,
                string p01,
                string p02)
            {
                env.SendEventBean(new SupportBean_S0(id, p00, p01, p02));
            }

            private void SendS1(
                RegressionEnvironment env,
                int id,
                string p10,
                string p11,
                string p12)
            {
                env.SendEventBean(new SupportBean_S1(id, p10, p11, p12));
            }
        }

        private class EPLJoinMultikeyWArrayCompositeArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@name('s0') select * " +
                             "from SupportEventWithIntArray#keepall as si, SupportEventWithManyArray#keepall as sm " +
                             "where si.array = sm.intOne and si.Value > sm.Value";
                env.CompileDeploy(eplOne).AddListener("s0");

                SendIntArray(env, "I1", new int[] { 1, 2 }, 10);
                SendManyArray(env, "M1", new int[] { 1, 2 }, 5);
                AssertReceived(env, new object[][] { new object[] { "I1", "M1" } });

                env.Milestone(0);

                SendIntArray(env, "I2", new int[] { 1, 2 }, 20);
                AssertReceived(env, new object[][] { new object[] { "I2", "M1" } });

                SendManyArray(env, "M2", new int[] { 1, 2 }, 1);
                AssertReceived(env, new object[][] { new object[] { "I1", "M2" }, new object[] { "I2", "M2" } });

                SendManyArray(env, "M3", new int[] { 1 }, 1);
                env.AssertListenerNotInvoked("s0");

                SendIntArray(env, "I3", new int[] { 2 }, 30);
                env.AssertListenerNotInvoked("s0");

                SendIntArray(env, "I4", new int[] { 1 }, 40);
                AssertReceived(env, new object[][] { new object[] { "I4", "M3" } });

                SendManyArray(env, "M4", new int[] { 2 }, 2);
                AssertReceived(env, new object[][] { new object[] { "I3", "M4" } });

                env.UndeployAll();
            }

            private void AssertReceived(
                RegressionEnvironment env,
                object[][] expected)
            {
                var fields = "si.Id,sm.Id".SplitCsv();
                env.AssertPropsPerRowLastNewAnyOrder("s0", fields, expected);
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] ints,
                int value)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
            }

            private void SendIntArray(
                RegressionEnvironment env,
                string id,
                int[] array,
                int value)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array, value));
            }
        }

        private class EPLJoinMultikeyWArrayHashJoinArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@name('s0') select * " +
                             "from SupportEventWithIntArray#keepall as si, SupportEventWithManyArray#keepall as sm " +
                             "where si.array = sm.intOne";
                env.CompileDeploy(eplOne).AddListener("s0");

                SendIntArray(env, "I1", new int[] { 1, 2 });
                SendManyArray(env, "M1", new int[] { 1, 2 });
                AssertReceived(env, new object[][] { new object[] { "I1", "M1" } });

                env.Milestone(0);

                SendIntArray(env, "I2", new int[] { 1, 2 });
                AssertReceived(env, new object[][] { new object[] { "I2", "M1" } });

                SendManyArray(env, "M2", new int[] { 1, 2 });
                AssertReceived(env, new object[][] { new object[] { "I1", "M2" }, new object[] { "I2", "M2" } });

                SendManyArray(env, "M3", new int[] { 1 });
                env.AssertListenerNotInvoked("s0");

                SendIntArray(env, "I3", new int[] { 2 });
                env.AssertListenerNotInvoked("s0");

                SendIntArray(env, "I4", new int[] { 1 });
                AssertReceived(env, new object[][] { new object[] { "I4", "M3" } });

                SendManyArray(env, "M4", new int[] { 2 });
                AssertReceived(env, new object[][] { new object[] { "I3", "M4" } });

                env.UndeployAll();
            }

            private void AssertReceived(
                RegressionEnvironment env,
                object[][] expected)
            {
                var fields = "si.Id,sm.Id".SplitCsv();
                env.AssertPropsPerRowLastNew("s0", fields, expected);
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] ints)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints));
            }

            private void SendIntArray(
                RegressionEnvironment env,
                string id,
                int[] array)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array));
            }
        }

        private class EPLJoinRangeNullAndDupAndInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne =
                    "@name('s0') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where IntBoxed between rangeStart and rangeEnd";
                env.CompileDeploy(eplOne).AddListener("s0");

                var eplTwo =
                    "@name('s1') select sb.* from SupportBean#keepall sb, SupportBeanRange#lastevent where TheString = key and IntBoxed in [rangeStart: rangeEnd]";
                env.CompileDeploy(eplTwo).AddListener("s1");

                // null join lookups
                SendEvent(env, new SupportBeanRange("R1", "G", (int?)null, null));
                SendEvent(env, new SupportBeanRange("R2", "G", null, 10));
                SendEvent(env, new SupportBeanRange("R3", "G", 10, null));
                SendSupportBean(env, "G", -1, null);

                // range invalid
                SendEvent(env, new SupportBeanRange("R4", "G", 10, 0));
                env.AssertListenerNotInvoked("s0");
                env.AssertListenerNotInvoked("s1");

                // duplicates
                object eventOne = SendSupportBean(env, "G", 100, 5);
                object eventTwo = SendSupportBean(env, "G", 101, 5);
                SendEvent(env, new SupportBeanRange("R4", "G", 0, 10));
                env.AssertListener(
                    "s0",
                    listener => {
                        var events = listener.GetAndResetLastNewData();
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            new object[] { eventOne, eventTwo },
                            EPAssertionUtil.GetUnderlying(events));
                    });
                env.AssertListener(
                    "s1",
                    listener => {
                        var events = listener.GetAndResetLastNewData();
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            new object[] { eventOne, eventTwo },
                            EPAssertionUtil.GetUnderlying(events));
                    });

                // test string compare
                var eplThree =
                    "@name('s2') select sb.* from SupportBeanRange#keepall sb, SupportBean#lastevent where TheString in [rangeStartStr:rangeEndStr]";
                env.CompileDeploy(eplThree).AddListener("s2");

                SendSupportBean(env, "P", 1, 1);
                SendEvent(env, new SupportBeanRange("R5", "R5", "O", "Q"));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLJoinMultikeyWArrayHashJoin2Prop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select * from " +
                                    "SupportBean(TheString like 'A%')#length(3) as streamA," +
                                    "SupportBean(TheString like 'B%')#length(3) as streamB" +
                                    " where streamA.IntPrimitive = streamB.IntPrimitive " +
                                    "and streamA.IntBoxed = streamB.IntBoxed";
                env.CompileDeploy(joinStatement).AddListener("s0");
                var fields = "streamA.TheString,streamB.TheString".SplitCsv();

                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(SupportBean), statement.EventType.GetPropertyType("streamA"));
                        Assert.AreEqual(typeof(SupportBean), statement.EventType.GetPropertyType("streamB"));
                        Assert.AreEqual(2, statement.EventType.PropertyNames.Length);
                    });

                int[][] eventData = new int[][] {
                    new int[] { 1, 100 },
                    new int[] { 2, 100 },
                    new int[] { 1, 200 },
                    new int[] { 2, 200 }
                };
                var eventsA = new SupportBean[eventData.Length];
                var eventsB = new SupportBean[eventData.Length];

                for (var i = 0; i < eventData.Length; i++) {
                    eventsA[i] = new SupportBean();
                    eventsA[i].TheString = $"A{i}";
                    eventsA[i].IntPrimitive = eventData[i][0];
                    eventsA[i].IntBoxed = eventData[i][1];

                    eventsB[i] = new SupportBean();
                    eventsB[i].TheString = $"B{i}";
                    eventsB[i].IntPrimitive = eventData[i][0];
                    eventsB[i].IntBoxed = eventData[i][1];
                }

                SendEvent(env, eventsA[0]);
                SendEvent(env, eventsB[1]);
                SendEvent(env, eventsB[2]);
                SendEvent(env, eventsB[3]);
                env.AssertListener("s0", listener => Assert.IsNull(listener.LastNewData)); // No events expected

                env.Milestone(0);

                SendSupportBean(env, "AX", 2, 100);
                env.AssertPropsNew("s0", fields, new object[] { "AX", "B1" });

                SendSupportBean(env, "BX", 1, 100);
                env.AssertPropsNew("s0", fields, new object[] { "A0", "BX" });

                env.UndeployAll();
            }
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
    }
} // end of namespace