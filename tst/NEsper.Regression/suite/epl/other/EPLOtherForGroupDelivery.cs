///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherForGroupDelivery
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            WithInvalid(execs);
            WithSubscriberOnly(execs);
            WithDiscreteDelivery(execs);
            WithGroupDelivery(execs);
            WithGroupDeliveryMultikeyWArraySingleArray(execs);
            With(GroupDeliveryMultikeyWArrayTwoField)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithGroupDeliveryMultikeyWArrayTwoField(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherGroupDeliveryMultikeyWArrayTwoField());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupDeliveryMultikeyWArraySingleArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherGroupDeliveryMultikeyWArraySingleArray());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupDelivery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherGroupDelivery());
            return execs;
        }

        public static IList<RegressionExecution> WithDiscreteDelivery(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherDiscreteDelivery());
            return execs;
        }

        public static IList<RegressionExecution> WithSubscriberOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherSubscriberOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLOtherInvalid());
            return execs;
        }

        private class EPLOtherGroupDeliveryMultikeyWArrayTwoField : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "TheString", "IntPrimitive", "LongPrimitive" };
                var epl = "create context MyContext start @now end after 1 second;\n" +
                          "@name('s0') context MyContext select * from SupportBean#keepall output snapshot when terminated for grouped_delivery (IntPrimitive, LongPrimitive)";
                env.CompileDeploy(epl).AddListener("s0");

                SendSB(env, "E1", 1, 10);
                SendSB(env, "E2", 2, 10);
                SendSB(env, "E3", 1, 11);
                SendSB(env, "E4", 2, 10);
                SendSB(env, "E5", 1, 10);

                env.AdvanceTime(1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var received = listener.NewDataList;
                        Assert.AreEqual(3, received.Count);
                        EPAssertionUtil.AssertPropsPerRow(
                            received[0],
                            fields,
                            new object[][] { new object[] { "E1", 1, 10L }, new object[] { "E5", 1, 10L } });
                        EPAssertionUtil.AssertPropsPerRow(
                            received[1],
                            fields,
                            new object[][] { new object[] { "E2", 2, 10L }, new object[] { "E4", 2, 10L } });
                        EPAssertionUtil.AssertPropsPerRow(
                            received[2],
                            fields,
                            new object[][] { new object[] { "E3", 1, 11L } });
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherGroupDeliveryMultikeyWArraySingleArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "Id", "intOne" };
                var epl = "create context MyContext start @now end after 1 second;\n" +
                          "@name('s0') context MyContext select * from SupportEventWithManyArray#keepall output snapshot when terminated for grouped_delivery (intOne)";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, "E1", new int[] { 1, 2 });
                SendManyArray(env, "E2", new int[] { 1, 3 });
                SendManyArray(env, "E3", new int[] { 1, 2 });
                SendManyArray(env, "E4", new int[] { 1, 4 });
                SendManyArray(env, "E5", new int[] { 1, 4 });

                env.AdvanceTime(1000);

                env.AssertListener(
                    "s0",
                    listener => {
                        var received = listener.NewDataList;
                        Assert.AreEqual(3, received.Count);
                        EPAssertionUtil.AssertPropsPerRow(
                            received[0],
                            fields,
                            new object[][] {
                                new object[] { "E1", new int[] { 1, 2 } }, new object[] { "E3", new int[] { 1, 2 } }
                            });
                        EPAssertionUtil.AssertPropsPerRow(
                            received[1],
                            fields,
                            new object[][] { new object[] { "E2", new int[] { 1, 3 } } });
                        EPAssertionUtil.AssertPropsPerRow(
                            received[2],
                            fields,
                            new object[][] {
                                new object[] { "E4", new int[] { 1, 4 } }, new object[] { "E5", new int[] { 1, 4 } }
                            });
                    });

                env.UndeployAll();
            }
        }

        private class EPLOtherInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBean for ",
                    "Incorrect syntax near end-of-input ('for' is a reserved keyword) expecting an identifier but found end-of-input at line 1 column 29");

                env.TryInvalidCompile(
                    "select * from SupportBean for other_keyword",
                    "Expected any of the [grouped_delivery, discrete_delivery] for-clause keywords after reserved keyword 'for'");

                env.TryInvalidCompile(
                    "select * from SupportBean for grouped_delivery",
                    "The for-clause with the grouped_delivery keyword requires one or more grouping expressions");

                env.TryInvalidCompile(
                    "select * from SupportBean for grouped_delivery()",
                    "The for-clause with the grouped_delivery keyword requires one or more grouping expressions");

                env.TryInvalidCompile(
                    "select * from SupportBean for grouped_delivery(dummy)",
                    "Failed to validate for-clause expression 'dummy': Property named 'dummy' is not valid in any stream");

                env.TryInvalidCompile(
                    "select * from SupportBean for discrete_delivery(dummy)",
                    "The for-clause with the discrete_delivery keyword does not allow grouping expressions");

                env.TryInvalidCompile(
                    "select * from SupportBean for discrete_delivery for grouped_delivery(IntPrimitive)",
                    "Incorrect syntax near 'for' (a reserved keyword) at line 1 column 48 ");
            }
        }

        private class EPLOtherSubscriberOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var subscriber = new SupportSubscriberMRD();
                SendTimer(env, 0);
                env.CompileDeploy(
                    "@name('s0') select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for discrete_delivery");
                env.Statement("s0").SetSubscriber(subscriber);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                Assert.AreEqual(3, subscriber.InsertStreamList.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", 1 }, subscriber.InsertStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E2", 2 }, subscriber.InsertStreamList[1][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E3", 1 }, subscriber.InsertStreamList[2][0]);

                env.UndeployAll();
                subscriber.Reset();
                env.CompileDeploy(
                    "@name('s0') select irstream TheString,IntPrimitive from SupportBean#time_batch(1) for grouped_delivery(IntPrimitive)");
                env.Statement("s0").SetSubscriber(subscriber);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 2000);
                Assert.AreEqual(2, subscriber.InsertStreamList.Count);
                Assert.AreEqual(2, subscriber.RemoveStreamList.Count);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", 1 }, subscriber.InsertStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E3", 1 }, subscriber.InsertStreamList[0][1]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E2", 2 }, subscriber.InsertStreamList[1][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E1", 1 }, subscriber.RemoveStreamList[0][0]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E3", 1 }, subscriber.RemoveStreamList[0][1]);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] { "E2", 2 }, subscriber.RemoveStreamList[1][0]);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class EPLOtherDiscreteDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                env.CompileDeploy("@name('s0') select * from SupportBean#time_batch(1) for discrete_delivery")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(3, listener.NewDataList.Count);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[0],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E1", 1 } });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[1],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E2", 2 } });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[2],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E3", 1 } });
                    });
                env.UndeployAll();

                // test no-event delivery
                var epl = "@name('s0') SELECT *  FROM ObjectEvent OUTPUT ALL EVERY 1 seconds for discrete_delivery";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new object(), "ObjectEvent");
                SendTimer(env, 2000);
                env.AssertListenerInvoked("s0");
                SendTimer(env, 3000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class EPLOtherGroupDelivery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);
                env.CompileDeploy(
                        "@name('s0') select * from SupportBean#time_batch(1) for grouped_delivery (IntPrimitive)")
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 1000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(2, listener.NewDataList.Count);
                        Assert.AreEqual(2, listener.NewDataList[0].Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[0],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 1 } });
                        Assert.AreEqual(1, listener.NewDataList[1].Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[1],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E2", 2 } });
                    });

                // test sorted
                env.UndeployAll();
                env.CompileDeploy(
"@name('s0') select * from SupportBean#time_batch(1) Order by IntPrimitive desc for grouped_delivery (IntPrimitive)");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 1));
                SendTimer(env, 2000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(2, listener.NewDataList.Count);
                        Assert.AreEqual(1, listener.NewDataList[0].Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[0],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E2", 2 } });
                        Assert.AreEqual(2, listener.NewDataList[1].Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[1],
                            "TheString,IntPrimitive".SplitCsv(),
                            new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 1 } });
                    });

                // test multiple criteria
                env.UndeployAll();
                var stmtText =
"@name('s0') select TheString, DoubleBoxed, enumValue from SupportBean#time_batch(1) Order by TheString, DoubleBoxed, enumValue for grouped_delivery(DoubleBoxed, enumValue)";
                env.CompileDeploy(stmtText).AddListener("s0");
                var fields = "TheString,DoubleBoxed,enumValue".SplitCsv();

                SendEvent(env, "E1", 10d, SupportEnum.ENUM_VALUE_2); // A (1)
                SendEvent(env, "E2", 11d, SupportEnum.ENUM_VALUE_1); // B (2)
                SendEvent(env, "E3", 9d, SupportEnum.ENUM_VALUE_2); // C (3)
                SendEvent(env, "E4", 10d, SupportEnum.ENUM_VALUE_2); // A
                SendEvent(env, "E5", 10d, SupportEnum.ENUM_VALUE_1); // D (4)
                SendEvent(env, "E6", 10d, SupportEnum.ENUM_VALUE_1); // D
                SendEvent(env, "E7", 11d, SupportEnum.ENUM_VALUE_1); // B
                SendEvent(env, "E8", 10d, SupportEnum.ENUM_VALUE_1); // D
                SendTimer(env, 3000);
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(4, listener.NewDataList.Count);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[0],
                            fields,
                            new object[][] {
                                new object[] { "E1", 10d, SupportEnum.ENUM_VALUE_2 },
                                new object[] { "E4", 10d, SupportEnum.ENUM_VALUE_2 }
                            });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[1],
                            fields,
                            new object[][] {
                                new object[] { "E2", 11d, SupportEnum.ENUM_VALUE_1 },
                                new object[] { "E7", 11d, SupportEnum.ENUM_VALUE_1 }
                            });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[2],
                            fields,
                            new object[][] { new object[] { "E3", 9d, SupportEnum.ENUM_VALUE_2 } });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[3],
                            fields,
                            new object[][] {
                                new object[] { "E5", 10d, SupportEnum.ENUM_VALUE_1 },
                                new object[] { "E6", 10d, SupportEnum.ENUM_VALUE_1 },
                                new object[] { "E8", 10d, SupportEnum.ENUM_VALUE_1 }
                            });
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
                env.AssertListener(
                    "s0",
                    listener => {
                        Assert.AreEqual(2, listener.NewDataList.Count);
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[0],
                            fields,
                            new object[][] { new object[] { "E1", 10d, SupportEnum.ENUM_VALUE_2 } });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.NewDataList[1],
                            fields,
                            new object[][] {
                                new object[] { "E2", 11d, SupportEnum.ENUM_VALUE_1 },
                                new object[] { "E3", 11d, SupportEnum.ENUM_VALUE_1 }
                            });
                    });

                env.UndeployAll();
            }
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
            double? doubleBoxed,
            SupportEnum enumVal)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.DoubleBoxed = doubleBoxed;
            bean.EnumValue = enumVal;
            env.SendEventBean(bean);
        }

        private static void SendSB(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
        }

        private static void SendManyArray(
            RegressionEnvironment env,
            string id,
            int[] intOne)
        {
            env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(intOne));
        }
    }
} // end of namespace