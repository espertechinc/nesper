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

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewExpressionBatch
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchNewestEventOldestEvent());
            execs.Add(new ViewExpressionBatchLengthBatch());
            execs.Add(new ViewExpressionBatchTimeBatch());
            execs.Add(new ViewExpressionBatchUDFBuiltin());
            execs.Add(new ViewExpressionBatchInvalid());
            execs.Add(new ViewExpressionBatchPrev());
            execs.Add(new ViewExpressionBatchEventPropBatch());
            execs.Add(new ViewExpressionBatchAggregationUngrouped());
            execs.Add(new ViewExpressionBatchAggregationWGroupwin());
            execs.Add(new ViewExpressionBatchAggregationOnDelete());
            execs.Add(new ViewExpressionBatchNamedWindowDelete());
            execs.Add(new ViewExpressionBatchDynamicTimeBatch());
            execs.Add(new ViewExpressionBatchVariableBatch());
            return execs;
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        internal class ViewExpressionBatchNewestEventOldestEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with include-trigger-event
                string[] fields = {"TheString"};
                var epl =
                    "@Name('s0') select irstream * from SupportBean#expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, false)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}},
                    null);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 3));
                env.SendEventBean(new SupportBean("E6", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E7", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}},
                    new[] {new object[] {"E3"}});
                env.UndeployAll();

                env.Milestone(5);

                // try with include-trigger-event
                epl =
                    "@Name('s0') select irstream * from SupportBean#expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, true)";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                env.Milestone(6);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(7);

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}},
                    null);

                env.Milestone(8);

                env.SendEventBean(new SupportBean("E4", 3));
                env.SendEventBean(new SupportBean("E5", 3));

                env.Milestone(9);

                env.SendEventBean(new SupportBean("E6", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(10);

                env.SendEventBean(new SupportBean("E7", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') select irstream * from SupportBean#expr_batch(current_count >= 3, true)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.SendEventBean(new SupportBean("E5", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E6", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.SendEventBean(new SupportBean("E7", 7));

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E8", 8));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E9", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E7"}, new object[] {"E8"}, new object[] {"E9"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                string[] fields = {"TheString"};
                var epl =
                    "@Name('s0') select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > 2000)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.AdvanceTime(3000);
                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(3100);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.SendEventBean(new SupportBean("E6", 6));
                env.AdvanceTime(5100);
                env.SendEventBean(new SupportBean("E7", 7));
                env.AdvanceTime(5101);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E8", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastOldData(),
                    fields,
                    new[] {
                        new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"},
                        new object[] {"E5"}
                    });

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchVariableBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string[] fields = {"TheString"};

                var epl = "create variable boolean POST = false;\n" +
                          "@Name('s0') select irstream * from SupportBean#expr_batch(POST);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "POST", true);
                env.AdvanceTime(1001);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}},
                    null);

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}},
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E3", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    new[] {new object[] {"E2"}});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "POST", false);
                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 2));
                env.AdvanceTime(2000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "POST", true);
                env.AdvanceTime(2001);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}},
                    new[] {new object[] {"E3"}});

                env.SendEventBean(new SupportBean("E6", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E6"}},
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchDynamicTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string[] fields = {"TheString"};

                var epl = "create variable long SIZE = 1000;\n" +
                          "@Name('s0') select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > SIZE);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 0));
                env.AdvanceTime(1900);
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "SIZE", 500);
                env.AdvanceTime(1901);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}},
                    null);

                env.SendEventBean(new SupportBean("E3", 0));
                env.AdvanceTime(2300);
                env.SendEventBean(new SupportBean("E4", 0));
                env.AdvanceTime(2500);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E5", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.AdvanceTime(3100);
                env.SendEventBean(new SupportBean("E6", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "SIZE", 999);
                env.AdvanceTime(3700);
                env.SendEventBean(new SupportBean("E7", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(4100);
                env.SendEventBean(new SupportBean("E8", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E6"}, new object[] {"E7"}, new object[] {"E8"}},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchUDFBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select * from SupportBean#expr_batch(udf(theString, view_reference, expired_count))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                ViewExpressionWindow.LocalUDF.Result = true;
                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual("E1", ViewExpressionWindow.LocalUDF.Key);
                Assert.AreEqual(0, (int) ViewExpressionWindow.LocalUDF.ExpiryCount);
                Assert.IsNotNull(ViewExpressionWindow.LocalUDF.Viewref);

                env.SendEventBean(new SupportBean("E2", 0));

                ViewExpressionWindow.LocalUDF.Result = false;
                env.SendEventBean(new SupportBean("E3", 0));
                Assert.AreEqual("E3", ViewExpressionWindow.LocalUDF.Key);
                Assert.AreEqual(0, (int) ViewExpressionWindow.LocalUDF.ExpiryCount);
                Assert.IsNotNull(ViewExpressionWindow.LocalUDF.Viewref);

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select * from SupportBean#expr_batch(1)",
                    "Failed to validate data window declaration: Invalid return value for expiry expression, expected a boolean return value but received int [select * from SupportBean#expr_batch(1)]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean#expr_batch((select * from SupportBean#lastevent))",
                    "Failed to validate data window declaration: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean#expr_batch((select * from SupportBean#lastevent))]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean#expr_batch(null < 0)",
                    "Failed to validate data window declaration: Invalid parameter expression 0 for Expression-batch view: Failed to validate view parameter expression 'null<0': Implicit conversion from datatype 'null' to numeric is not allowed");
            }
        }

        internal class ViewExpressionBatchNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') create window NW#expr_batch(current_count > 3) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E3"}});

                env.SendEventBean(new SupportBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}},
                    null);

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0"};
                var epl =
                    "@Name('s0') select prev(1, theString) as val0 from SupportBean#expr_batch(current_count > 2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {null}, new object[] {"E1"}, new object[] {"E2"}},
                    null);

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchEventPropBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0"};
                var epl = "@Name('s0') select irstream theString as val0 from SupportBean#expr_batch(intPrimitive > 0)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}},
                    null);

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}},
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E3", -1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E4", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}},
                    new[] {new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchAggregationUngrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl = "@Name('s0') select irstream theString from SupportBean#expr_batch(sum(IntPrimitive) > 100)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 90));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}},
                    null);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 101));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 1));
                env.SendEventBean(new SupportBean("E6", 99));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E7", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}},
                    new[] {new object[] {"E4"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchAggregationWGroupwin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl =
                    "@Name('s0') select irstream theString from SupportBean#groupwin(IntPrimitive)#expr_batch(sum(longPrimitive) > 100)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                SendEvent(env, "E2", 2, 10);
                SendEvent(env, "E3", 1, 90);
                SendEvent(env, "E4", 2, 80);
                SendEvent(env, "E5", 2, 10);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "E6", 2, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}},
                    null);

                SendEvent(env, "E7", 2, 50);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "E8", 1, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E3"}, new object[] {"E8"}},
                    null);

                SendEvent(env, "E9", 2, 50);
                SendEvent(env, "E10", 1, 101);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E10"}},
                    new[] {new object[] {"E1"}, new object[] {"E3"}, new object[] {"E8"}});

                SendEvent(env, "E11", 2, 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E7"}, new object[] {"E9"}, new object[] {"E11"}},
                    new[] {new object[] {"E2"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});

                SendEvent(env, "E12", 1, 102);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E12"}},
                    new[] {new object[] {"E10"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionBatchAggregationOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl = "@Name('s0') create window NW#expr_batch(sum(IntPrimitive) >= 10) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 8));
                env.SendEventBean(new SupportBean_A("E2"));

                env.SendEventBean(new SupportBean("E3", 8));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E4", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E3"}, new object[] {"E4"}},
                    null);

                env.UndeployAll();
            }
        }
    }
} // end of namespace