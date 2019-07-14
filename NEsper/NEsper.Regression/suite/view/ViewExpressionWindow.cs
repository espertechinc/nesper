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

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewExpressionWindow
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowSceneOne());
            execs.Add(new ViewExpressionWindowNewestEventOldestEvent());
            execs.Add(new ViewExpressionWindowLengthWindow());
            execs.Add(new ViewExpressionWindowTimeWindow());
            execs.Add(new ViewExpressionWindowUDFBuiltin());
            execs.Add(new ViewExpressionWindowInvalid());
            execs.Add(new ViewExpressionWindowPrev());
            execs.Add(new ViewExpressionWindowAggregationUngrouped());
            execs.Add(new ViewExpressionWindowAggregationWGroupwin());
            execs.Add(new ViewExpressionWindowNamedWindowDelete());
            execs.Add(new ViewExpressionWindowAggregationWOnDelete());
            execs.Add(new ViewExpressionWindowVariable());
            execs.Add(new ViewExpressionWindowDynamicTimeWindow());
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

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        public class ViewExpressionWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                env.AdvanceTime(0);

                var epl =
                    "@Name('s0') select irstream TheString as c0 from SupportBean#expr(newest_timestamp - oldest_timestamp < 1000)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.AdvanceTime(1000);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                env.AdvanceTime(1500);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                env.AdvanceTime(2000);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E3"},
                    new object[] {"E1"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                env.AdvanceTime(2499);

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
                env.AdvanceTime(2500);

                env.Milestone(5);

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});
                SendSupportBean(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E5"},
                    new object[] {"E2"});
                env.AdvanceTime(10000);
                SendSupportBean(env, "E6");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetIRPair(),
                    fields,
                    new[] {new object[] {"E6"}},
                    new[] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowNewestEventOldestEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl =
                    "@Name('s0') select irstream * from SupportBean#expr(newest_event.IntPrimitive = oldest_event.IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E4", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}},
                    new[] {new object[] {"E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E5", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E6", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});

                env.Milestone(6);

                env.SendEventBean(new SupportBean("E7", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E7"}},
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E7"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') select * from SupportBean#expr(current_count <= 2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                string[] fields = {"TheString"};
                var epl =
                    "@Name('s0') select irstream * from SupportBean#expr(oldest_timestamp > newest_timestamp - 2000)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.AdvanceTime(2500);
                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                env.AdvanceTime(3000);
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"E1"}});
                env.Listener("s0").Reset();

                env.AdvanceTime(3499);
                env.SendEventBean(new SupportBean("E6", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"},
                        new object[] {"E6"}
                    });

                env.Milestone(1);

                env.AdvanceTime(3500);
                env.SendEventBean(new SupportBean("E7", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                env.Listener("s0").Reset();

                env.AdvanceTime(10000);
                env.SendEventBean(new SupportBean("E8", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string[] fields = {"TheString"};

                var epl = "create variable boolean KEEP = true;\n" +
                          "@Name('s0') select irstream * from SupportBean#expr(KEEP);\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "KEEP", false);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Listener("s0").Reset();
                env.AdvanceTime(1001);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E2"});
                env.Listener("s0").Reset();
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "KEEP", true);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowDynamicTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string[] fields = {"TheString"};

                var epl = "create variable long SIZE = 1000;\n" +
                          "@Name('s0') select irstream * from SupportBean#expr(newest_timestamp - oldest_timestamp < SIZE)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.Milestone(1);

                env.AdvanceTime(2000);
                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "SIZE", 10000);

                env.Milestone(2);

                env.AdvanceTime(5000);
                env.SendEventBean(new SupportBean("E3", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(3);

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("s0"), "SIZE", 2000);

                env.Milestone(4);

                env.AdvanceTime(6000);
                env.SendEventBean(new SupportBean("E4", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E4"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowUDFBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean#expr(udf(theString, view_reference, expired_count))";
                env.CompileDeploy(epl).AddListener("s0");

                LocalUDF.Result = true;
                env.SendEventBean(new SupportBean("E1", 0));
                Assert.AreEqual("E1", LocalUDF.Key);
                Assert.AreEqual(0, (int) LocalUDF.ExpiryCount);
                Assert.IsNotNull(LocalUDF.Viewref);

                env.SendEventBean(new SupportBean("E2", 0));

                LocalUDF.Result = false;
                env.SendEventBean(new SupportBean("E3", 0));
                Assert.AreEqual("E3", LocalUDF.Key);
                Assert.AreEqual(2, (int) LocalUDF.ExpiryCount);
                Assert.IsNotNull(LocalUDF.Viewref);

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#expr(1)",
                    "Failed to validate data window declaration: Invalid return value for expiry expression, expected a boolean return value but received int [select * from SupportBean#expr(1)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#expr((select * from SupportBean#lastevent))",
                    "Failed to validate data window declaration: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean#expr((select * from SupportBean#lastevent))]");
            }
        }

        internal class ViewExpressionWindowNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') create window NW#expr(true) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E3"}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0"};
                var epl = "@Name('s0') select prev(1, theString) as val0 from SupportBean#expr(true)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowAggregationUngrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') select irstream theString from SupportBean#expr(sum(IntPrimitive) < 10)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}},
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean("E3", 11));
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), fields, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E4", 12));
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), fields, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}},
                    new[] {new object[] {"E4"}});

                env.SendEventBean(new SupportBean("E5", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E5"}},
                    null);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E6", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E6"}},
                    null);

                env.SendEventBean(new SupportBean("E7", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E7"}},
                    null);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E8", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E7"}, new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E8"}},
                    new[] {new object[] {"E5"}, new object[] {"E6"}});

                env.SendEventBean(new SupportBean("E9", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E9"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E9"}},
                    new[] {new object[] {"E7"}, new object[] {"E8"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowAggregationWGroupwin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl =
                    "@Name('s0') select irstream theString from SupportBean#groupwin(IntPrimitive)#expr(sum(LongPrimitive) < 10)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 5);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}},
                    null);

                SendEvent(env, "E2", 2, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}},
                    null);

                SendEvent(env, "E3", 1, 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    null);

                env.Milestone(0);

                SendEvent(env, "E4", 2, 4);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}},
                    null);

                SendEvent(env, "E5", 2, 6);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E5"}},
                    new[] {new object[] {"E2"}, new object[] {"E4"}});

                SendEvent(env, "E6", 1, 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E6"}},
                    new[] {new object[] {"E1"}});

                env.UndeployAll();
            }
        }

        internal class ViewExpressionWindowAggregationWOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};
                var epl = "@Name('s0') create window NW#expr(sum(IntPrimitive) < 10) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E1"}},
                    null);

                env.SendEventBean(new SupportBean("E2", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E2"}},
                    null);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    null,
                    new[] {new object[] {"E2"}});

                env.SendEventBean(new SupportBean("E3", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E3"}},
                    null);

                env.SendEventBean(new SupportBean("E4", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetDataListsFlattened(),
                    fields,
                    new[] {new object[] {"E4"}},
                    new[] {new object[] {"E1"}});

                env.UndeployAll();
            }
        }

        public class LocalUDF
        {
            private static bool result;

            public static string Key { get; private set; }

            public static int? ExpiryCount { get; private set; }

            public static object Viewref { get; private set; }

            public static bool Result {
                set => result = value;
            }

            public static bool EvaluateExpiryUDF(
                string key,
                object viewref,
                int? expiryCount)
            {
                Key = key;
                Viewref = viewref;
                ExpiryCount = expiryCount;
                return result;
            }
        }
    }
} // end of namespace