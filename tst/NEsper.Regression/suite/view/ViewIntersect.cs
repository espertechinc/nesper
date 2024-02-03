///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewIntersect
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithUniqueAndFirstLength(execs);
            WithFirstUniqueAndFirstLength(execs);
            WithBatchWindow(execs);
            WithAndDerivedValue(execs);
            WithGroupBy(execs);
            WithThreeUnique(execs);
            WithPattern(execs);
            WithTwoUnique(execs);
            WithSorted(execs);
            WithTimeWin(execs);
            WithTimeWinReversed(execs);
            WithTimeWinSODA(execs);
            WithLengthOneUnique(execs);
            WithTimeUniqueMultikey(execs);
            WithGroupTimeUnique(execs);
            WithSubselect(execs);
            WithFirstUniqueAndLengthOnDelete(execs);
            WithTimeWinNamedWindow(execs);
            WithTimeWinNamedWindowDelete(execs);
            WithGroupTimeLength(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupTimeLength(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectGroupTimeLength());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinNamedWindowDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeWinNamedWindowDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeWinNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUniqueAndLengthOnDelete(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectFirstUniqueAndLengthOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupTimeUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectGroupTimeUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeUniqueMultikey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeUniqueMultikey());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthOneUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectLengthOneUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeWinSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinReversed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeWinReversed());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTimeWin());
            return execs;
        }

        public static IList<RegressionExecution> WithSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectSorted());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectTwoUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithThreeUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectThreeUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithAndDerivedValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectAndDerivedValue());
            return execs;
        }

        public static IList<RegressionExecution> WithBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUniqueAndFirstLength(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectFirstUniqueAndFirstLength());
            return execs;
        }

        public static IList<RegressionExecution> WithUniqueAndFirstLength(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewIntersectUniqueAndFirstLength());
            return execs;
        }

        internal class ViewIntersectGroupTimeLength : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select sum(IntPrimitive) as c0 from SupportBean#groupwin(TheString)#time(1 second)#length(2)";
                env.AdvanceTime(0);
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "G1", 10, 10);

                env.AdvanceTime(250);
                SendAssert(env, "G2", 100, 110);

                env.Milestone(0);

                env.AdvanceTime(500);
                SendAssert(env, "G1", 11, 10 + 100 + 11);

                env.AdvanceTime(750);
                SendAssert(env, "G2", 101, 10 + 100 + 11 + 101);

                env.Milestone(1);

                env.AdvanceTime(800);
                SendAssert(env, "G3", 1000, 10 + 100 + 11 + 101 + 1000);

                env.AdvanceTime(1000); // expires: {"G1", 10}
                AssertReceived(env, 100 + 11 + 101 + 1000);

                env.Milestone(2);

                SendAssert(env, "G2", 102, 11 + 101 + 1000 + 102); // expires: {"G2", 100}

                env.AdvanceTime(1499); // expires: {"G1", 10}
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.AdvanceTime(1500); // expires: {"G1", 11}
                AssertReceived(env, 101 + 1000 + 102);

                env.AdvanceTime(1750); // expires: {"G2", 101}
                AssertReceived(env, 1000 + 102);

                env.Milestone(4);

                env.AdvanceTime(1800); // expires: {"G3", 1000}
                AssertReceived(env, 102);

                env.AdvanceTime(2000); // expires: {"G2", 102}
                AssertReceived(env, null);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                int intPrimitive,
                object expected)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                AssertReceived(env, expected);
            }

            private void AssertReceived(
                RegressionEnvironment env,
                object expected)
            {
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        internal class ViewIntersectUniqueAndFirstLength : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong(1);

                var epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#unique(TheString)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionUniqueAndFirstLength(env, milestone);

                env.UndeployAll();

                epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#unique(TheString)#firstlength(3)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                TryAssertionUniqueAndFirstLength(env, milestone);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectFirstUniqueAndFirstLength : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#firstunique(TheString)#firstlength(3)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.IncrementAndGet());

                TryAssertionFirstUniqueAndLength(env);

                env.UndeployAll();

                epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#firstunique(TheString)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.IncrementAndGet());

                TryAssertionFirstUniqueAndLength(env);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectFirstUniqueAndLengthOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowOne#firstunique(TheString)#firstlength(3) as SupportBean;\n" +
                          "insert into MyWindowOne select * from SupportBean;\n" +
                          "on SupportBean_S0 delete from MyWindowOne where TheString = P00;\n" +
                          "@name('s0') select irstream * from MyWindowOne";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var fields = new string[] { "TheString", "IntPrimitive" };

                SendEvent(env, "E1", 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                SendEvent(env, "E1", 99);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "E2", 2);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E2", 2 } });
                env.AssertPropsOld("s0", fields, new object[] { "E1", 1 });

                SendEvent(env, "E1", 3);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 } });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 3 });

                SendEvent(env, "E1", 99);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 } });
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "E3", 3);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3 });

                SendEvent(env, "E3", 98);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ViewIntersectBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string epl;

                // test window
                epl =
                    "@name('s0') select irstream TheString from SupportBean#length_batch(3)#unique(IntPrimitive) order by TheString asc";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionUniqueAndBatch(env, milestone);
                env.UndeployAll();

                epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#length_batch(3) order by TheString asc";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionUniqueAndBatch(env, milestone);
                env.UndeployAll();

                // test aggregation with window
                epl =
                    "@name('s0') select count(*) as c0, sum(IntPrimitive) as c1 from SupportBean#unique(TheString)#length_batch(3)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionUniqueBatchAggreation(env, milestone);
                env.UndeployAll();

                epl =
                    "@name('s0') select count(*) as c0, sum(IntPrimitive) as c1 from SupportBean#length_batch(3)#unique(TheString)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionUniqueBatchAggreation(env, milestone);
                env.UndeployAll();

                // test first-unique
                epl = "@name('s0') select irstream * from SupportBean#firstunique(TheString)#length_batch(3)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionLengthBatchAndFirstUnique(env, milestone);
                env.UndeployAll();

                epl = "@name('s0') select irstream * from SupportBean#length_batch(3)#firstunique(TheString)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionLengthBatchAndFirstUnique(env, milestone);
                env.UndeployAll();

                // test time-based expiry
                env.AdvanceTime(0);
                epl = "@name('s0') select * from SupportBean#unique(TheString)#time_batch(1)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionTimeBatchAndUnique(env, 0, milestone);
                env.UndeployAll();

                env.AdvanceTime(0);
                epl = "@name('s0') select * from SupportBean#time_batch(1)#unique(TheString)";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
                TryAssertionTimeBatchAndUnique(env, 100000, milestone);
                env.UndeployAll();

                env.TryInvalidCompile(
                    "select * from SupportBean#time_batch(1)#length_batch(10)",
                    "Failed to validate data window declaration: Cannot combined multiple batch data windows into an intersection [");
            }
        }

        internal class ViewIntersectAndDerivedValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "total" };

                var epl =
                    "@name('s0') select * from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#uni(DoublePrimitive)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(100d));
                env.AssertPropsNew("s0", fields, new object[] { 100d });

                SendEvent(env, "E2", 2, 20, 50d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(150d));
                env.AssertPropsNew("s0", fields, new object[] { 150d });

                SendEvent(env, "E3", 1, 20, 20d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(20d));
                env.AssertPropsNew("s0", fields, new object[] { 20d });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var text =
                    "@name('s0') select irstream TheString from SupportBean#groupwin(IntPrimitive)#length(2)#unique(IntBoxed) retain-intersection";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                SendEvent(env, "E2", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                SendEvent(env, "E3", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);

                SendEvent(env, "E4", 1, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E1" });

                env.Milestone(4);

                SendEvent(env, "E5", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4", "E5"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E2" });

                env.Milestone(5);

                SendEvent(env, "E6", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E3" });

                env.Milestone(6);

                SendEvent(env, "E7", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E5", "E6", "E7"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E7" }, new object[] { "E4" });

                env.Milestone(7);

                SendEvent(env, "E8", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E6", "E7", "E8"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E8" }, new object[] { "E5" });

                env.UndeployAll();

                // another combination
                env.CompileDeployAddListenerMile(
                    "@name('s0') select * from SupportBean#groupwin(TheString)#time(.0083 sec)#firstevent",
                    "s0",
                    8);
                env.UndeployAll();
            }
        }

        internal class ViewIntersectSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select * from SupportBean_S0 where P00 in (select TheString from SupportBean#length(2)#unique(IntPrimitive) retain-intersection)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendEvent(env, "E1", 1);
                SendEvent(env, "E2", 2);

                env.Milestone(1);

                SendEvent(env, "E3", 3); // throws out E1
                SendEvent(env, "E4", 2); // throws out E2
                SendEvent(env, "E5", 1); // throws out E3

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "E2"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "E3"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(1, "E4"));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ViewIntersectThreeUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#unique(DoublePrimitive) retain-intersection";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 10, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(0);

                SendEvent(env, "E2", 2, 10, 200d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E2" }, new object[] { "E1" });

                env.Milestone(1);

                SendEvent(env, "E3", 2, 20, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E2" });

                env.Milestone(2);

                SendEvent(env, "E4", 1, 30, 300d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(3);

                SendEvent(env, "E5", 3, 40, 400d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.Milestone(4);

                SendEvent(env, "E6", 3, 40, 300d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E6"));
                env.AssertListener(
                    "s0",
                    listener => {
                        object[] result =
                            { listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString") };
                        EPAssertionUtil.AssertEqualsAnyOrder(result, new string[] { "E4", "E5" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E6" });
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var text =
                    "@name('s0') select irstream a.P00||b.P10 as TheString from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]#unique(a.Id)#unique(b.Id) retain-intersection";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean_S1(2, "E2"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E1E2" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "E3"));
                env.SendEventBean(new SupportBean_S1(20, "E4"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1E2", "E3E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E3E4" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                env.SendEventBean(new SupportBean_S1(2, "E6"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3E4", "E5E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5E6" }, new object[] { "E1E2" });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTwoUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed) retain-intersection";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                SendEvent(env, "E2", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E2" }, new object[] { "E1" });

                env.Milestone(1);

                SendEvent(env, "E3", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                SendEvent(env, "E4", 3, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E3" });

                env.Milestone(2);

                SendEvent(env, "E5", 2, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E2" });

                env.Milestone(3);

                SendEvent(env, "E6", 3, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E5", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E4" });

                env.Milestone(4);

                SendEvent(env, "E7", 3, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E7"));
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastOldData.Length);
                        object[] result =
                            { listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString") };
                        EPAssertionUtil.AssertEqualsAnyOrder(result, new string[] { "E5", "E6" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E7" });
                        listener.Reset();
                    });

                env.Milestone(5);

                SendEvent(env, "E8", 4, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E7", "E8"));
                env.AssertPropsNew("s0", fields, new object[] { "E8" });

                SendEvent(env, "E9", 3, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E8", "E9"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E9" }, new object[] { "E7" });

                env.Milestone(6);

                SendEvent(env, "E10", 2, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E8", "E10"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E10" }, new object[] { "E9" });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectSorted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#sort(2, IntPrimitive)#sort(2, IntBoxed) retain-intersection";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(0);

                SendEvent(env, "E2", 2, 9);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(1);

                SendEvent(env, "E3", 0, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3"));
                env.AssertListener(
                    "s0",
                    listener => {
                        object[] result =
                            { listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString") };
                        EPAssertionUtil.AssertEqualsAnyOrder(result, new string[] { "E1", "E2" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E3" });
                        listener.Reset();
                    });

                env.Milestone(2);

                SendEvent(env, "E4", -1, -1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(3);

                SendEvent(env, "E5", 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E5" });

                env.Milestone(4);

                SendEvent(env, "E6", 0, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E3" });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#time(10 sec) retain-intersection";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeWinReversed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#time(10 sec)#unique(IntPrimitive) retain-intersection";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeWinSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#time(10 seconds)#unique(IntPrimitive) retain-intersection";
                env.EplToModelCompileDeploy(epl).AddListener("s0").Milestone(0);

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeWinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') create window MyWindowTwo#time(10 sec)#unique(IntPrimitive) retain-intersection as select * from SupportBean;\n" +
                    "insert into MyWindowTwo select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowTwo where IntBoxed = Id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeWinNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') create window MyWindowThree#time(10 sec)#unique(IntPrimitive) retain-intersection as select * from SupportBean;\n" +
                    "insert into MyWindowThree select * from SupportBean\n;" +
                    "on SupportBean_S0 delete from MyWindowThree where IntBoxed = Id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new string[] { "TheString" };

                env.AdvanceTime(1000);
                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(0);

                env.AdvanceTime(2000);
                SendEvent(env, "E2", 2, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.SendEventBean(new SupportBean_S0(20));
                env.AssertPropsOld("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));

                env.Milestone(1);

                env.AdvanceTime(3000);
                SendEvent(env, "E3", 3, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });
                SendEvent(env, "E4", 3, 40);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E3" });

                env.Milestone(2);

                env.AdvanceTime(4000);
                SendEvent(env, "E5", 4, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });
                SendEvent(env, "E6", 4, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E5" });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(20));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4", "E6"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(50));
                env.AssertPropsOld("s0", fields, new object[] { "E6" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4"));

                env.Milestone(4);

                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(11000);
                env.AssertPropsOld("s0", fields, new object[] { "E1" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4"));

                env.Milestone(5);

                env.AdvanceTime(12999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(13000);
                env.AssertPropsOld("s0", fields, new object[] { "E4" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr());

                env.Milestone(6);

                env.AdvanceTime(10000000);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ViewIntersectLengthOneUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select irstream Symbol, Price from SupportMarketDataBean#length(1)#unique(Symbol)";
                env.CompileDeployAddListenerMileZero(text, "s0");
                env.SendEventBean(MakeMarketDataEvent("S1", 100));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 100.0 } },
                    null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S1", 5));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 5.0 } },
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 100.0 } });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectUniqueSort : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var symbolCsco = "CSCO.O";
                var symbolIbm = "IBM.N";
                var symbolMsft = "MSFT.O";
                var symbolC = "C.N";

                var epl = "@name('s0') select * from SupportMarketDataBean#unique(Symbol)#sort(3, Price desc)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var beans = new object[10];

                beans[0] = MakeEvent(symbolCsco, 50);
                env.SendEventBean(beans[0]);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var result = ToObjectArray(iterator);
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { beans[0] }, result);
                    });
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.IsTrue(listener.IsInvoked);
                        EPAssertionUtil.AssertEqualsExactOrder((object[])null, listener.LastOldData);
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[] { beans[0] },
                            new object[] { listener.LastNewData[0].Underlying });
                        listener.Reset();
                    });

                beans[1] = MakeEvent(symbolCsco, 20);
                beans[2] = MakeEvent(symbolIbm, 50);
                beans[3] = MakeEvent(symbolMsft, 40);
                beans[4] = MakeEvent(symbolC, 100);
                beans[5] = MakeEvent(symbolIbm, 10);

                env.SendEventBean(beans[1]);
                env.SendEventBean(beans[2]);
                env.SendEventBean(beans[3]);
                env.SendEventBean(beans[4]);
                env.SendEventBean(beans[5]);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var result = ToObjectArray(iterator);
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { beans[3], beans[4] }, result);
                    });

                beans[6] = MakeEvent(symbolCsco, 110);
                beans[7] = MakeEvent(symbolC, 30);
                beans[8] = MakeEvent(symbolCsco, 30);

                env.SendEventBean(beans[6]);
                env.SendEventBean(beans[7]);
                env.SendEventBean(beans[8]);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var result = ToObjectArray(iterator);
                        EPAssertionUtil.AssertEqualsExactOrder(new object[] { beans[3], beans[8] }, result);
                    });

                env.UndeployAll();
            }
        }

        internal class ViewIntersectGroupTimeUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') SELECT irstream * FROM SupportSensorEvent#groupwin(Type)#time(1 hour)#unique(Device)#sort(1, Measurement desc) as high order by Measurement asc";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var eventOne = new SupportSensorEvent(1, "Temperature", "Device1", 5.0, 96.5);
                env.SendEventBean(eventOne);
                AssertUnderlying(env, new object[] { eventOne }, null);

                var eventTwo = new SupportSensorEvent(2, "Temperature", "Device2", 7.0, 98.5);
                env.SendEventBean(eventTwo);
                AssertUnderlying(env, new object[] { eventTwo }, new object[] { eventOne });

                var eventThree = new SupportSensorEvent(3, "Temperature", "Device2", 4.0, 99.5);
                env.SendEventBean(eventThree);
                AssertUnderlying(env, new object[] { eventThree }, new object[] { eventThree, eventTwo });

                env.AssertIterator("s0", iterator => ClassicAssert.IsFalse(iterator.MoveNext()));

                env.UndeployAll();
            }
        }

        internal class ViewIntersectTimeUniqueMultikey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var epl = "@name('s0') select irstream * from SupportMarketDataBean#time(3.0)#unique(Symbol, Price)";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new string[] { "Symbol", "Price", "Volume" };

                SendMDEvent(env, "IBM", 10, 1L);
                env.AssertPropsNew("s0", fields, new object[] { "IBM", 10.0, 1L });

                SendMDEvent(env, "IBM", 11, 2L);
                env.AssertPropsNew("s0", fields, new object[] { "IBM", 11.0, 2L });

                SendMDEvent(env, "IBM", 10, 3L);
                env.AssertPropsIRPair("s0", fields, new object[] { "IBM", 10.0, 3L }, new object[] { "IBM", 10.0, 1L });

                SendMDEvent(env, "IBM", 11, 4L);
                env.AssertPropsIRPair("s0", fields, new object[] { "IBM", 11.0, 4L }, new object[] { "IBM", 11.0, 2L });

                env.AdvanceTime(2000);
                SendMDEvent(env, null, 11, 5L);
                env.AssertPropsNew("s0", fields, new object[] { null, 11.0, 5L });

                env.AdvanceTime(3000);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    null,
                    new object[][] { new object[] { "IBM", 10.0, 3L }, new object[] { "IBM", 11.0, 4L } });

                SendMDEvent(env, null, 11, 6L);
                env.AssertPropsIRPair("s0", fields, new object[] { null, 11.0, 6L }, new object[] { null, 11.0, 5L });

                env.AdvanceTime(6000);
                env.AssertPropsOld("s0", fields, new object[] { null, 11.0, 6L });

                env.UndeployAll();
            }
        }

        private static void TryAssertionTimeWinUnique(RegressionEnvironment env)
        {
            var fields = new string[] { "TheString" };

            env.AdvanceTime(1000);
            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
            env.AssertPropsNew("s0", fields, new object[] { "E1" });

            env.Milestone(1);

            env.AdvanceTime(2000);
            SendEvent(env, "E2", 2);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
            env.AssertPropsNew("s0", fields, new object[] { "E2" });

            env.Milestone(2);

            env.AdvanceTime(3000);
            SendEvent(env, "E3", 1);
            env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E1" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3"));

            env.Milestone(3);

            env.AdvanceTime(4000);
            SendEvent(env, "E4", 3);
            env.AssertPropsNew("s0", fields, new object[] { "E4" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3", "E4"));
            SendEvent(env, "E5", 3);
            env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E4" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3", "E5"));

            env.Milestone(4);

            env.AdvanceTime(11999);
            env.AssertListenerNotInvoked("s0");
            env.AdvanceTime(12000);
            env.AssertPropsOld("s0", fields, new object[] { "E2" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E5"));

            env.Milestone(5);

            env.AdvanceTime(12999);
            env.AssertListenerNotInvoked("s0");
            env.AdvanceTime(13000);
            env.AssertPropsOld("s0", fields, new object[] { "E3" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E5"));

            env.Milestone(6);

            env.AdvanceTime(13999);
            env.AssertListenerNotInvoked("s0");
            env.AdvanceTime(14000);
            env.AssertPropsOld("s0", fields, new object[] { "E5" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr());
        }

        private static void TryAssertionUniqueBatchAggreation(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "c0,c1".SplitCsv();

            env.SendEventBean(new SupportBean("A1", 10));
            env.SendEventBean(new SupportBean("A2", 11));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("A3", 12));
            env.AssertPropsNew("s0", fields, new object[] { 3L, 10 + 11 + 12 });

            env.SendEventBean(new SupportBean("A1", 13));
            env.SendEventBean(new SupportBean("A2", 14));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("A3", 15));
            env.AssertPropsNew("s0", fields, new object[] { 3L, 13 + 14 + 15 });

            env.SendEventBean(new SupportBean("A1", 16));
            env.SendEventBean(new SupportBean("A2", 17));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("A3", 18));
            env.AssertPropsNew("s0", fields, new object[] { 3L, 16 + 17 + 18 });

            env.SendEventBean(new SupportBean("A1", 19));
            env.SendEventBean(new SupportBean("A1", 20));
            env.SendEventBean(new SupportBean("A2", 21));
            env.SendEventBean(new SupportBean("A2", 22));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean("A3", 23));
            env.AssertPropsNew("s0", fields, new object[] { 3L, 20 + 22 + 23 });
        }

        private static void TryAssertionUniqueAndBatch(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new string[] { "TheString" };

            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E2", 2);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendEvent(env, "E3", 3);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } },
                null);

            SendEvent(env, "E4", 4);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4"));
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E5", 4); // throws out E5
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E5"));
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E6", 4); // throws out E6
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E6"));
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E7", 5);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E6", "E7"));
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendEvent(env, "E8", 6);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" } },
                new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });

            SendEvent(env, "E8", 7);
            SendEvent(env, "E9", 9);
            SendEvent(env, "E9", 9);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E8", "E9"));
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendEvent(env, "E10", 11);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E10" }, new object[] { "E8" }, new object[] { "E9" } },
                new object[][] { new object[] { "E6" }, new object[] { "E7" }, new object[] { "E8" } });
        }

        private static void TryAssertionUniqueAndFirstLength(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new string[] { "TheString", "IntPrimitive" };

            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
            env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

            SendEvent(env, "E2", 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

            env.MilestoneInc(milestone);

            SendEvent(env, "E1", 3);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 } });
            env.AssertPropsIRPair("s0", fields, new object[] { "E1", 3 }, new object[] { "E1", 1 });

            SendEvent(env, "E3", 30);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 }, new object[] { "E3", 30 } });
            env.AssertPropsNew("s0", fields, new object[] { "E3", 30 });

            env.MilestoneInc(milestone);

            SendEvent(env, "E4", 40);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 }, new object[] { "E3", 30 } });
            env.AssertListenerNotInvoked("s0");
        }

        private static void TryAssertionFirstUniqueAndLength(RegressionEnvironment env)
        {
            var fields = new string[] { "TheString", "IntPrimitive" };

            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
            env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

            SendEvent(env, "E2", 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

            SendEvent(env, "E2", 10);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E3", 3);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            env.AssertPropsNew("s0", fields, new object[] { "E3", 3 });

            SendEvent(env, "E4", 4);
            SendEvent(env, "E4", 5);
            SendEvent(env, "E5", 5);
            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            env.AssertListenerNotInvoked("s0");
        }

        private static void TryAssertionTimeBatchAndUnique(
            RegressionEnvironment env,
            long startTime,
            AtomicLong milestone)
        {
            var fields = "TheString,IntPrimitive".SplitCsv();

            SendEvent(env, "E1", 1);
            SendEvent(env, "E2", 2);
            SendEvent(env, "E1", 3);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(startTime + 1000);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E2", 2 }, new object[] { "E1", 3 } },
                null);

            SendEvent(env, "E3", 3);
            SendEvent(env, "E3", 4);
            SendEvent(env, "E3", 5);
            SendEvent(env, "E4", 6);
            SendEvent(env, "E3", 7);
            env.AssertListenerNotInvoked("s0");

            env.AdvanceTime(startTime + 2000);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E4", 6 }, new object[] { "E3", 7 } },
                null);
        }

        private static void TryAssertionLengthBatchAndFirstUnique(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "TheString,IntPrimitive".SplitCsv();

            SendEvent(env, "E1", 1);
            SendEvent(env, "E2", 2);
            SendEvent(env, "E1", 3);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E3", 4);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 4 } },
                null);

            SendEvent(env, "E1", 5);
            SendEvent(env, "E4", 7);
            SendEvent(env, "E1", 6);
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E5", 9);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 5 }, new object[] { "E4", 7 }, new object[] { "E5", 9 } },
                new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 4 } });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int intBoxed,
            double doublePrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static object[][] ToArr(params object[] values)
        {
            var arr = new object[values.Length][];
            for (var i = 0; i < values.Length; i++) {
                arr[i] = new object[] { values[i] };
            }

            return arr;
        }

        private static void SendMDEvent(
            RegressionEnvironment env,
            string symbol,
            double price,
            long? volume)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, volume, "");
            env.SendEventBean(theEvent);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, 0L, "");
        }

        private static object MakeEvent(
            string symbol,
            double price)
        {
            return new SupportMarketDataBean(symbol, price, 0L, "");
        }

        private static object[] ToObjectArray(IEnumerator<EventBean> it)
        {
            var result = new List<object>();
            for (; it.MoveNext();) {
                var theEvent = it.Current;
                result.Add(theEvent.Underlying);
            }

            return result.ToArray();
        }

        private static void AssertUnderlying(
            RegressionEnvironment env,
            object[] newUnd,
            object[] oldUnd)
        {
            env.AssertListener(
                "s0",
                listener => {
                    EPAssertionUtil.AssertUnderlyingPerRow(listener.AssertInvokedAndReset(), newUnd, oldUnd);
                });
        }
    }
} // end of namespace