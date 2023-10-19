///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewExpressionBatch
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithNewestEventOldestEvent(execs);
            WithLengthBatch(execs);
            WithTimeBatch(execs);
            WithUDFBuiltin(execs);
            WithInvalid(execs);
            WithPrev(execs);
            WithEventPropBatch(execs);
            WithAggregationUngrouped(execs);
            WithAggregationWGroupwin(execs);
            WithAggregationOnDelete(execs);
            WithNamedWindowDelete(execs);
            WithDynamicTimeBatch(execs);
            WithVariableBatch(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithVariableBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchVariableBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchDynamicTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchNamedWindowDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationOnDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchAggregationOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationWGroupwin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchAggregationWGroupwin());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationUngrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchAggregationUngrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithEventPropBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchEventPropBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFBuiltin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchUDFBuiltin());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchLengthBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithNewestEventOldestEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionBatchNewestEventOldestEvent());
            return execs;
        }

        private class ViewExpressionBatchNewestEventOldestEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // try with include-trigger-event
                var milestone = new AtomicLong();
                var fields = new string[] { "theString" };
                var epl =
                    "@name('s0') select irstream * from SupportBean#expr_batch(newest_event.intPrimitive != oldest_event.intPrimitive, false)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } },
                    null);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E5", 3));
                env.SendEventBean(new SupportBean("E6", 3));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E7", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    new object[][] { new object[] { "E3" } });
                env.UndeployAll();

                env.MilestoneInc(milestone);

                // try with include-trigger-event
                epl =
                    "@name('s0') select irstream * from SupportBean#expr_batch(newest_event.intPrimitive != oldest_event.intPrimitive, true)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } },
                    null);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 3));
                env.SendEventBean(new SupportBean("E5", 3));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 3));
                env.AssertListenerNotInvoked("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E7", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "theString" };
                var epl = "@name('s0') select irstream * from SupportBean#expr_batch(current_count >= 3, true)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.SendEventBean(new SupportBean("E5", 5));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E6", 6));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.SendEventBean(new SupportBean("E7", 7));

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E8", 8));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E9", 9));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" }, new object[] { "E8" }, new object[] { "E9" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = new string[] { "theString" };
                var epl =
                    "@name('s0') select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > 2000)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.AdvanceTime(3000);
                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(3100);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E5", 5));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" },
                        new object[] { "E5" }
                    });

                env.SendEventBean(new SupportBean("E6", 6));
                env.AdvanceTime(5100);
                env.SendEventBean(new SupportBean("E7", 7));
                env.AdvanceTime(5101);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E8", 8));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" } },
                    new object[][] {
                        new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" },
                        new object[] { "E5" }
                    });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchVariableBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "theString" };

                var epl = "create variable boolean POST = false;\n" +
                          "@name('s0') select irstream * from SupportBean#expr_batch(POST);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable("s0", "POST", true);
                env.AdvanceTime(1001);
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E1" } }, null);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" } },
                    new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E3", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } },
                    new object[][] { new object[] { "E2" } });

                env.RuntimeSetVariable("s0", "POST", false);
                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 2));
                env.AdvanceTime(2000);
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable("s0", "POST", true);
                env.AdvanceTime(2001);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } },
                    new object[][] { new object[] { "E3" } });

                env.SendEventBean(new SupportBean("E6", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchDynamicTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "theString" };

                var epl = "create variable long SIZE = 1000;\n" +
                          "@name('s0') select irstream * from SupportBean#expr_batch(newest_timestamp - oldest_timestamp > SIZE);\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 0));
                env.AdvanceTime(1900);
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable("s0", "SIZE", 500);
                env.AdvanceTime(1901);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } },
                    null);

                env.SendEventBean(new SupportBean("E3", 0));
                env.AdvanceTime(2300);
                env.SendEventBean(new SupportBean("E4", 0));
                env.AdvanceTime(2500);
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E5", 0));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.AdvanceTime(3100);
                env.SendEventBean(new SupportBean("E6", 0));
                env.AssertListenerNotInvoked("s0");

                env.RuntimeSetVariable("s0", "SIZE", 999);
                env.AdvanceTime(3700);
                env.SendEventBean(new SupportBean("E7", 0));
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(4100);
                env.SendEventBean(new SupportBean("E8", 0));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" } },
                    new object[][] { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchUDFBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBean#expr_batch(udf(theString, view_reference, expired_count))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                ViewExpressionWindow.LocalUDF.SetResult(true);
                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertThat(
                    () => {
                        Assert.AreEqual("E1", ViewExpressionWindow.LocalUDF.GetKey());
                        Assert.AreEqual(0, (int)ViewExpressionWindow.LocalUDF.GetExpiryCount());
                        Assert.IsNotNull(ViewExpressionWindow.LocalUDF.GetViewref());
                    });

                env.SendEventBean(new SupportBean("E2", 0));

                ViewExpressionWindow.LocalUDF.SetResult(false);
                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertThat(
                    () => {
                        Assert.AreEqual("E3", ViewExpressionWindow.LocalUDF.GetKey());
                        Assert.AreEqual(0, (int)ViewExpressionWindow.LocalUDF.GetExpiryCount());
                        Assert.IsNotNull(ViewExpressionWindow.LocalUDF.GetViewref());
                    });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBean#expr_batch(1)",
                    "Failed to validate data window declaration: Invalid return value for expiry expression, expected a boolean return value but received int [select * from SupportBean#expr_batch(1)]");

                env.TryInvalidCompile(
                    "select * from SupportBean#expr_batch((select * from SupportBean#lastevent))",
                    "Failed to validate data window declaration: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean#expr_batch((select * from SupportBean#lastevent))]");

                env.TryInvalidCompile(
                    "select * from SupportBean#expr_batch(null < 0)",
                    "Failed to validate data window declaration: Invalid parameter expression 0 for Expression-batch view: Failed to validate view parameter expression 'null<0': Null-type value is not allow for relational operator");
            }
        }

        private class ViewExpressionBatchNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "theString" };
                var epl = "@name('s0') create window NW#expr_batch(current_count > 3) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E3" } });

                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E5", 5));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } },
                    null);

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "val0" };
                var epl =
                    "@name('s0') select prev(1, theString) as val0 from SupportBean#expr_batch(current_count > 2)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { null }, new object[] { "E1" }, new object[] { "E2" } },
                    null);

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchEventPropBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "val0" };
                var epl = "@name('s0') select irstream theString as val0 from SupportBean#expr_batch(intPrimitive > 0)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E1" } }, null);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" } },
                    new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E3", -1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E4", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } },
                    new object[][] { new object[] { "E2" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchAggregationUngrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "theString" };

                var epl = "@name('s0') select irstream theString from SupportBean#expr_batch(sum(intPrimitive) > 100)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 90));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 10));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } },
                    null);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 101));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 1));
                env.SendEventBean(new SupportBean("E6", 99));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E7", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } },
                    new object[][] { new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchAggregationWGroupwin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "theString" };

                var epl =
                    "@name('s0') select irstream theString from SupportBean#groupwin(intPrimitive)#expr_batch(sum(longPrimitive) > 100)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                SendEvent(env, "E2", 2, 10);
                SendEvent(env, "E3", 1, 90);
                SendEvent(env, "E4", 2, 80);
                SendEvent(env, "E5", 2, 10);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "E6", 2, 1);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } },
                    null);

                SendEvent(env, "E7", 2, 50);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "E8", 1, 2);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E3" }, new object[] { "E8" } },
                    null);

                SendEvent(env, "E9", 2, 50);
                SendEvent(env, "E10", 1, 101);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E10" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E3" }, new object[] { "E8" } });

                SendEvent(env, "E11", 2, 1);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" }, new object[] { "E9" }, new object[] { "E11" } },
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                SendEvent(env, "E12", 1, 102);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E12" } },
                    new object[][] { new object[] { "E10" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionBatchAggregationOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "theString" };

                var epl = "@name('s0') create window NW#expr_batch(sum(intPrimitive) >= 10) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where theString = id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 8));
                env.SendEventBean(new SupportBean_A("E2"));

                env.SendEventBean(new SupportBean("E3", 8));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E4", 1));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E3" }, new object[] { "E4" } },
                    null);

                env.UndeployAll();
            }
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
    }
} // end of namespace