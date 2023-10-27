///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithNewestEventOldestEvent(execs);
            WithLengthWindow(execs);
            WithTimeWindow(execs);
            WithUDFBuiltin(execs);
            WithInvalid(execs);
            WithPrev(execs);
            WithAggregationUngrouped(execs);
            WithAggregationWGroupwin(execs);
            WithNamedWindowDelete(execs);
            WithAggregationWOnDelete(execs);
            WithVariable(execs);
            WithDynamicTimeWindow(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDynamicTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowDynamicTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationWOnDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowAggregationWOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowNamedWindowDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationWGroupwin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowAggregationWGroupwin());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationUngrouped(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowAggregationUngrouped());
            return execs;
        }

        public static IList<RegressionExecution> WithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFBuiltin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowUDFBuiltin());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowLengthWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithNewestEventOldestEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowNewestEventOldestEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewExpressionWindowSceneOne());
            return execs;
        }

        public class ViewExpressionWindowSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();
                env.AdvanceTime(0);

                var epl =
                    "@name('s0') select irstream TheString as c0 from SupportBean#expr(newest_timestamp - oldest_timestamp < 1000)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.AdvanceTime(1000);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                env.AdvanceTime(1500);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1" } });
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                env.AdvanceTime(2000);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                SendSupportBean(env, "E3");
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E1" });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                env.AdvanceTime(2499);

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                SendSupportBean(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });
                env.AdvanceTime(2500);

                env.Milestone(5);

                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });
                SendSupportBean(env, "E5");
                env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E2" });
                env.AdvanceTime(10000);
                SendSupportBean(env, "E6");
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } },
                    new object[][] { new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowNewestEventOldestEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl =
                    "@name('s0') select irstream * from SupportBean#expr(newest_event.IntPrimitive = oldest_event.IntPrimitive)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } },
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E3" } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } },
                    new object[][] { new object[] { "E3" } });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E4" } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E5", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E6", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E6" });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.Milestone(6);

                env.SendEventBean(new SupportBean("E7", 2));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E7" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl = "@name('s0') select * from SupportBean#expr(current_count <= 2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = new string[] { "TheString" };
                var epl =
                    "@name('s0') select irstream * from SupportBean#expr(oldest_timestamp > newest_timestamp - 2000)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.AdvanceTime(1500);
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.AdvanceTime(2500);
                env.SendEventBean(new SupportBean("E4", 4));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" } });
                env.ListenerReset("s0");

                env.AdvanceTime(3000);
                env.SendEventBean(new SupportBean("E5", 5));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" } });
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" } },
                    new object[][] { new object[] { "E1" } });

                env.AdvanceTime(3499);
                env.SendEventBean(new SupportBean("E6", 6));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2" }, new object[] { "E3" }, new object[] { "E4" }, new object[] { "E5" },
                        new object[] { "E6" }
                    });

                env.Milestone(1);

                env.AdvanceTime(3500);
                env.SendEventBean(new SupportBean("E7", 7));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastNewData,
                            fields,
                            new object[][] { new object[] { "E7" } });
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastOldData,
                            fields,
                            new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                        listener.Reset();
                    });

                env.AdvanceTime(10000);
                env.SendEventBean(new SupportBean("E8", 8));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E8" } });
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E8" } },
                    new object[][]
                        { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "TheString" };

                var epl = "create variable boolean KEEP = true;\n" +
                          "@name('s0') select irstream * from SupportBean#expr(KEEP);\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.RuntimeSetVariable("s0", "KEEP", false);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });
                env.ListenerReset("s0");

                env.AdvanceTime(1001);
                env.AssertPropsOld("s0", fields, new object[] { "E1" });
                env.AssertPropsPerRowIterator("s0", fields, null);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsIRPair("s0", fields, new object[] { "E2" }, new object[] { "E2" });
                env.AssertPropsPerRowIterator("s0", fields, null);

                env.RuntimeSetVariable("s0", "KEEP", true);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E3" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowDynamicTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var fields = new string[] { "TheString" };

                var epl = "create variable long SIZE = 1000;\n" +
                          "@name('s0') select irstream * from SupportBean#expr(newest_timestamp - oldest_timestamp < SIZE)";
                env.CompileDeploy(epl).AddListener("s0").Milestone(0);

                env.AdvanceTime(1000);
                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.Milestone(1);

                env.AdvanceTime(2000);
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E2" } });

                env.RuntimeSetVariable("s0", "SIZE", 10000);

                env.Milestone(2);

                env.AdvanceTime(5000);
                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(3);

                env.RuntimeSetVariable("s0", "SIZE", 2000);

                env.Milestone(4);

                env.AdvanceTime(6000);
                env.SendEventBean(new SupportBean("E4", 0));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowUDFBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean#expr(udf(TheString, view_reference, expired_count))";
                env.CompileDeploy(epl).AddListener("s0");

                LocalUDF.SetResult(true);
                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertThat(
                    () => {
                        Assert.AreEqual("E1", LocalUDF.GetKey());
                        Assert.AreEqual(0, (int)LocalUDF.GetExpiryCount());
                        Assert.IsNotNull(LocalUDF.GetViewref());
                    });

                env.SendEventBean(new SupportBean("E2", 0));

                LocalUDF.SetResult(false);
                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertThat(
                    () => {
                        Assert.AreEqual("E3", LocalUDF.GetKey());
                        Assert.AreEqual(2, (int)LocalUDF.GetExpiryCount());
                        Assert.IsNotNull(LocalUDF.GetViewref());
                    });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBean#expr(1)",
                    "Failed to validate data window declaration: Invalid return value for expiry expression, expected a boolean return value but received int [select * from SupportBean#expr(1)]");

                env.TryInvalidCompile(
                    "select * from SupportBean#expr((select * from SupportBean#lastevent))",
                    "Failed to validate data window declaration: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean#expr((select * from SupportBean#lastevent))]");
            }
        }

        private class ViewExpressionWindowNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl = "@name('s0') create window NW#expr(true) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where TheString = Id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.ListenerReset("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1" }, new object[] { "E3" } });
                env.AssertPropsOld("s0", fields, new object[] { "E2" });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "val0" };
                var epl = "@name('s0') select prev(1, TheString) as val0 from SupportBean#expr(true)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { null });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowAggregationUngrouped : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl = "@name('s0') select irstream TheString from SupportBean#expr(sum(IntPrimitive) < 10)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 9));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E2" } });
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" } },
                    new object[][] { new object[] { "E1" } });

                env.SendEventBean(new SupportBean("E3", 11));
                env.AssertPropsPerRowIterator("s0", fields, null);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } },
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E4", 12));
                env.AssertPropsPerRowIterator("s0", fields, null);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } },
                    new object[][] { new object[] { "E4" } });

                env.SendEventBean(new SupportBean("E5", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E5" } });
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E5" } }, null);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E6", 2));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" }, new object[] { "E6" } });
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E6" } }, null);

                env.SendEventBean(new SupportBean("E7", 3));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E7" } }, null);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E8", 6));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" }, new object[] { "E8" } });
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E8" } },
                    new object[][] { new object[] { "E5" }, new object[] { "E6" } });

                env.SendEventBean(new SupportBean("E9", 9));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E9" } });
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E9" } },
                    new object[][] { new object[] { "E7" }, new object[] { "E8" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowAggregationWGroupwin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#groupwin(IntPrimitive)#expr(sum(LongPrimitive) < 10)";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 5);
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E1" } }, null);

                SendEvent(env, "E2", 2, 2);
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E2" } }, null);

                SendEvent(env, "E3", 1, 3);
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E3" } }, null);

                env.Milestone(0);

                SendEvent(env, "E4", 2, 4);
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E4" } }, null);

                SendEvent(env, "E5", 2, 6);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" } },
                    new object[][] { new object[] { "E2" }, new object[] { "E4" } });

                SendEvent(env, "E6", 1, 2);
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } },
                    new object[][] { new object[] { "E1" } });

                env.UndeployAll();
            }
        }

        private class ViewExpressionWindowAggregationWOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };
                var epl = "@name('s0') create window NW#expr(sum(IntPrimitive) < 10) as SupportBean;\n" +
                          "insert into NW select * from SupportBean;\n" +
                          "on SupportBean_A delete from NW where TheString = Id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E1" } }, null);

                env.SendEventBean(new SupportBean("E2", 8));
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E2" } }, null);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E2"));
                env.AssertPropsPerRowIRPairFlattened("s0", fields, null, new object[][] { new object[] { "E2" } });

                env.SendEventBean(new SupportBean("E3", 7));
                env.AssertPropsPerRowIRPairFlattened("s0", fields, new object[][] { new object[] { "E3" } }, null);

                env.SendEventBean(new SupportBean("E4", 2));
                env.AssertPropsPerRowIRPairFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } },
                    new object[][] { new object[] { "E1" } });

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

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            env.SendEventBean(new SupportBean(theString, 0));
        }

        public class LocalUDF
        {
            private static string key;
            private static int? expiryCount;
            private static object viewref;
            private static bool result;

            public static bool EvaluateExpiryUDF(
                string key,
                object viewref,
                int? expiryCount)
            {
                LocalUDF.key = key;
                LocalUDF.viewref = viewref;
                LocalUDF.expiryCount = expiryCount;
                return result;
            }

            public static string GetKey()
            {
                return key;
            }

            public static int? GetExpiryCount()
            {
                return expiryCount;
            }

            public static object GetViewref()
            {
                return viewref;
            }

            public static void SetResult(bool result)
            {
                LocalUDF.result = result;
            }
        }
    }
} // end of namespace