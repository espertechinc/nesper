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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherForGroupDelivery
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalid());
            execs.Add(new EPLOtherSubscriberOnly());
            execs.Add(new EPLOtherDiscreteDelivery());
            execs.Add(new EPLOtherGroupDelivery());
            return execs;
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            double doubleBoxed,
            SupportEnum enumVal)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.DoubleBoxed = doubleBoxed;
            bean.EnumValue = enumVal;
            env.SendEventBean(bean);
        }

        internal class EPLOtherInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for ",
                    "Incorrect syntax near end-of-input ('for' is a reserved keyword) expecting an Identifier but found end-of-input at line 1 column 29");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for other_keyword",
                    "Expected any of the [grouped_delivery, discrete_delivery] for-clause keywords after reserved keyword 'for'");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for grouped_delivery",
                    "The for-clause with the grouped_delivery keyword requires one or more grouping expressions");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for grouped_delivery()",
                    "The for-clause with the grouped_delivery keyword requires one or more grouping expressions");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for grouped_delivery(dummy)",
                    "Failed to valIdate for-clause expression 'dummy': Property named 'dummy' is not valId in any stream");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for discrete_delivery(dummy)",
                    "The for-clause with the discrete_delivery keyword does not allow grouping expressions");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean for discrete_delivery for grouped_delivery(IntPrimitive)",
                    "Incorrect syntax near 'for' (a reserved keyword) at line 1 column 48 ");
            }
        }

        internal class EPLOtherSubscriberOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subscriber = new SupportSubscriberMRD();
                SendTimer(env, 0);
                env.CompileDeploy(
                    "@Name('s0') select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for discrete_delivery");
                env.Statement("s0").Subscriber = subscriber;

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                Assert.AreEqual(3, subscriber.InsertStreamList.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1", 1}, subscriber.InsertStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E2", 2}, subscriber.InsertStreamList[1][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E3", 1}, subscriber.InsertStreamList[2][0]);

                env.UndeployAll();
                subscriber.Reset();
                env.CompileDeploy(
                    "@Name('s0') select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for grouped_delivery(IntPrimitive)");
                env.Statement("s0").Subscriber = subscriber;

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 2000);
                Assert.AreEqual(2, subscriber.InsertStreamList.Count);
                Assert.AreEqual(2, subscriber.RemoveStreamList.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1", 1}, subscriber.InsertStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E3", 1}, subscriber.InsertStreamList[0][1]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E2", 2}, subscriber.InsertStreamList[1][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E1", 1}, subscriber.RemoveStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E3", 1}, subscriber.RemoveStreamList[0][1]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"E2", 2}, subscriber.RemoveStreamList[1][0]);

                env.UndeployAll();
            }
        }

        internal class EPLOtherDiscreteDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                env.CompileDeploy("@Name('s0') select * from SupportBean#time_batch(1) for discrete_delivery")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                Assert.AreEqual(3, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[0],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E1", 1}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[1],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E2", 2}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[2],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E3", 1}});
                env.UndeployAll();

                // test no-event delivery
                var epl = "@Name('s0') SELECT *  FROM ObjectEvent OUTPUT ALL EVERY 1 seconds for discrete_delivery";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new object(), "ObjectEvent");
                SendTimer(env, 2000);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                SendTimer(env, 3000);
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class EPLOtherGroupDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                env.CompileDeploy(
                        "@Name('s0') select * from SupportBean#time_batch(1) for grouped_delivery (IntPrimitive)")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                Assert.AreEqual(2, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(2, env.Listener("s0").NewDataList[0].Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[0],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 1}});
                Assert.AreEqual(1, env.Listener("s0").NewDataList[1].Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[1],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E2", 2}});

                // test sorted
                env.UndeployAll();
                env.CompileDeploy(
                    "@Name('s0') select * from SupportBean#time_batch(1) order by IntPrimitive desc for grouped_delivery (IntPrimitive)");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 2000);
                Assert.AreEqual(2, env.Listener("s0").NewDataList.Count);
                Assert.AreEqual(1, env.Listener("s0").NewDataList[0].Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[0],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E2", 2}});
                Assert.AreEqual(2, env.Listener("s0").NewDataList[1].Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[1],
                    "theString,IntPrimitive".SplitCsv(),
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 1}});

                // test multiple criteria
                env.UndeployAll();
                var stmtText =
                    "@Name('s0') select TheString, DoubleBoxed, enumValue from SupportBean#time_batch(1) order by TheString, DoubleBoxed, enumValue for grouped_delivery(DoubleBoxed, enumValue)";
                env.CompileDeploy(stmtText).AddListener("s0");

                SendEvent(env, "E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
                SendEvent(env, "E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
                SendEvent(env, "E3", 9d, SupportEnum.ENUM_VALUE_2); // C (3)
                SendEvent(env, "E4", 10d, SupportEnum.ENUM_VALUE_2); // A
                SendEvent(env, "E5", 10d, SupportEnum.ENUM_VALUE_1); // D (4)
                SendEvent(env, "E6", 10d, SupportEnum.ENUM_VALUE_1); // D
                SendEvent(env, "E7", 11d, SupportEnum.ENUM_VALUE_1); // B
                SendEvent(env, "E8", 10d, SupportEnum.ENUM_VALUE_1); // D
                SendTimer(env, 3000);
                Assert.AreEqual(4, env.Listener("s0").NewDataList.Count);
                var fields = "theString,DoubleBoxed,enumValue".SplitCsv();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[0],
                    fields,
                    new[] {
                        new object[] {"E1", 10d, SupportEnum.ENUM_VALUE_2},
                        new object[] {"E4", 10d, SupportEnum.ENUM_VALUE_2}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[1],
                    fields,
                    new[] {
                        new object[] {"E2", 11d, SupportEnum.ENUM_VALUE_1},
                        new object[] {"E7", 11d, SupportEnum.ENUM_VALUE_1}
                    });
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[2],
                    fields,
                    new[] {new object[] {"E3", 9d, SupportEnum.ENUM_VALUE_2}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[3],
                    fields,
                    new[] {
                        new object[] {"E5", 10d, SupportEnum.ENUM_VALUE_1},
                        new object[] {"E6", 10d, SupportEnum.ENUM_VALUE_1},
                        new object[] {"E8", 10d, SupportEnum.ENUM_VALUE_1}
                    });
                env.UndeployAll();

                // test SODA
                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());
                env.CompileDeploy(model).AddListener("s0");

                SendEvent(env, "E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
                SendEvent(env, "E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
                SendEvent(env, "E3", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
                SendTimer(env, 4000);
                Assert.AreEqual(2, env.Listener("s0").NewDataList.Count);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[0],
                    fields,
                    new[] {new object[] {"E1", 10d, SupportEnum.ENUM_VALUE_2}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataList[1],
                    fields,
                    new[] {
                        new object[] {"E2", 11d, SupportEnum.ENUM_VALUE_1},
                        new object[] {"E3", 11d, SupportEnum.ENUM_VALUE_1}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace