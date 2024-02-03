///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewUnion
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFirstUniqueAndFirstLength(execs);
            WithBatchWindow(execs);
            WithAndDerivedValue(execs);
            WithGroupBy(execs);
            WithThreeUnique(execs);
            WithPattern(execs);
            WithTwoUnique(execs);
            WithSorted(execs);
            WithTimeWin(execs);
            WithTimeWinSODA(execs);
            WithInvalid(execs);
            WithSubselect(execs);
            WithFirstUniqueAndLengthOnDelete(execs);
            WithTimeWinNamedWindow(execs);
            WithTimeWinNamedWindowDelete(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinNamedWindowDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionTimeWinNamedWindowDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionTimeWinNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUniqueAndLengthOnDelete(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionFirstUniqueAndLengthOnDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionSubselect());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWinSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionTimeWinSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionTimeWin());
            return execs;
        }

        public static IList<RegressionExecution> WithSorted(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionSorted());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionTwoUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithThreeUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionThreeUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithAndDerivedValue(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionAndDerivedValue());
            return execs;
        }

        public static IList<RegressionExecution> WithBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUniqueAndFirstLength(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUnionFirstUniqueAndFirstLength());
            return execs;
        }

        private class ViewUnionFirstUniqueAndLengthOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create window MyWindowOne#firstunique(TheString)#firstlength(3) retain-union as SupportBean;\n" +
                    "insert into MyWindowOne select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowOne where TheString = P00;\n" +
                    "@name('s0') select irstream * from MyWindowOne;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new string[] { "TheString", "IntPrimitive" };

                SendEvent(env, "E1", 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(0);

                SendEvent(env, "E1", 99);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 99 } });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 99 });

                SendEvent(env, "E2", 2);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 99 }, new object[] { "E2", 2 } });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E2", 2 } });
                env.AssertPropsPerRowIRPair(
                    "s0",
                    "TheString".SplitCsv(),
                    null,
                    new object[][] { new object[] { "E1" }, new object[] { "E1" } });

                env.Milestone(1);

                SendEvent(env, "E1", 3);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 3 }, new object[] { "E2", 2 } });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 3 });

                env.UndeployAll();
            }
        }

        private class ViewUnionFirstUniqueAndFirstLength : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#firstunique(TheString) retain-union";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                TryAssertionFirstUniqueAndFirstLength(env);

                env.UndeployAll();

                epl =
                    "@name('s0') select irstream TheString, IntPrimitive from SupportBean#firstunique(TheString)#firstlength(3) retain-union";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                TryAssertionFirstUniqueAndFirstLength(env);

                env.UndeployAll();
            }
        }

        private class ViewUnionBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#length_batch(3)#unique(IntPrimitive) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                SendEvent(env, "E2", 2);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                SendEvent(env, "E3", 3);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                SendEvent(env, "E4", 4);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                SendEvent(env, "E5", 4);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                SendEvent(env, "E6", 4); // remove stream is E1, E2, E3
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6"));
                env.AssertPropsNew("s0", fields, new object[] { "E6" });

                SendEvent(env, "E7", 5);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7"));
                env.AssertPropsNew("s0", fields, new object[] { "E7" });

                SendEvent(env, "E8", 6);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    ToArr("E1", "E2", "E3", "E5", "E4", "E6", "E7", "E8"));
                env.AssertPropsNew("s0", fields, new object[] { "E8" });

                SendEvent(env, "E9", 7); // remove stream is E4, E5, E6; E4 and E5 get removed as their
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E6", "E7", "E8", "E9"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E9" } },
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.UndeployAll();
            }
        }

        private class ViewUnionAndDerivedValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "total" };

                var epl =
                    "@name('s0') select * from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#uni(DoublePrimitive) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(100d));
                env.AssertPropsNew("s0", fields, new object[] { 100d });

                SendEvent(env, "E2", 2, 20, 50d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(150d));
                env.AssertPropsNew("s0", fields, new object[] { 150d });

                SendEvent(env, "E3", 1, 20, 20d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr(170d));
                env.AssertPropsNew("s0", fields, new object[] { 170d });

                env.UndeployAll();
            }
        }

        private class ViewUnionGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var text =
                    "@name('s0') select irstream TheString from SupportBean#groupwin(IntPrimitive)#length(2)#unique(IntBoxed) retain-union";
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
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(4);

                SendEvent(env, "E5", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.Milestone(5);

                SendEvent(env, "E6", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E4", "E5", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E3" });

                SendEvent(env, "E7", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E4", "E5", "E6", "E7"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E7" }, new object[] { "E1" });

                env.Milestone(6);

                SendEvent(env, "E8", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E6", "E7", "E8"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E8" }, new object[] { "E2" });

                env.UndeployAll();
            }
        }

        private class ViewUnionSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select * from SupportBean_S0 where P00 in (select TheString from SupportBean#length(2)#unique(IntPrimitive) retain-union)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendEvent(env, "E1", 1);
                SendEvent(env, "E2", 2);

                env.Milestone(1);

                SendEvent(env, "E3", 3);
                SendEvent(env, "E4", 2); // throws out E1
                SendEvent(env, "E5", 1); // retains E3

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "E2"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(1, "E3"));
                env.AssertListenerInvoked("s0");

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "E4"));
                env.AssertListenerInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ViewUnionThreeUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#unique(DoublePrimitive) retain-union";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 10, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(0);

                SendEvent(env, "E2", 2, 10, 200d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(1);

                SendEvent(env, "E3", 2, 20, 100d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(2);

                SendEvent(env, "E4", 1, 30, 300d);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E1" });

                env.UndeployAll();
            }
        }

        private class ViewUnionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "string" };

                var text =
                    "@name('s0') select irstream a.P00||b.P10 as string from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]#unique(a.Id)#unique(b.Id) retain-union";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean_S1(2, "E2"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E1E2" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10, "E3"));
                env.SendEventBean(new SupportBean_S1(20, "E4"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1E2", "E3E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E3E4" });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                env.SendEventBean(new SupportBean_S1(2, "E6"));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3E4", "E5E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5E6" }, new object[] { "E1E2" });

                env.UndeployAll();
            }
        }

        private class ViewUnionTwoUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                SendEvent(env, "E2", 2, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                SendEvent(env, "E3", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E3" }, new object[] { "E1" });

                env.Milestone(3);

                SendEvent(env, "E4", 1, 20);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E4"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4" }, new object[] { "E3" });

                env.Milestone(4);

                SendEvent(env, "E5", 2, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });

                env.Milestone(5);

                SendEvent(env, "E6", 3, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E6"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6" }, new object[] { "E2" });

                env.Milestone(6);

                SendEvent(env, "E7", 3, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E6", "E7"));
                env.AssertPropsNew("s0", fields, new object[] { "E7" });

                env.Milestone(7);

                SendEvent(env, "E8", 4, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E7", "E8"));
                env.AssertPropsIRPair("s0", fields, new object[] { "E8" }, new object[] { "E6" });

                env.Milestone(8);

                SendEvent(env, "E9", 3, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E5", "E7", "E8", "E9"));
                env.AssertPropsNew("s0", fields, new object[] { "E9" });

                env.Milestone(9);

                SendEvent(env, "E10", 2, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E8", "E9", "E10"));
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E10" } },
                    new object[][] { new object[] { "E5" }, new object[] { "E7" } });

                env.UndeployAll();
            }
        }

        private class ViewUnionSorted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "TheString" };

                var epl =
                    "@name('s0') select irstream TheString from SupportBean#sort(2, IntPrimitive)#sort(2, IntBoxed) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                SendEvent(env, "E2", 2, 9);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2"));
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.Milestone(2);

                SendEvent(env, "E3", 0, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);

                SendEvent(env, "E4", -1, -1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4"));
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastOldData.Length);
                        object[] result =
                            { listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString") };
                        EPAssertionUtil.AssertEqualsAnyOrder(result, new string[] { "E1", "E2" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E4" });
                        listener.Reset();
                    });

                env.Milestone(4);

                SendEvent(env, "E5", 1, 1);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E3", "E4"));
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(1, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[] { "E5" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E5" });
                        listener.Reset();
                    });

                env.Milestone(5);

                SendEvent(env, "E6", 0, 0);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E4", "E6"));
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(1, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.AssertOneGetOld(), fields, new object[] { "E3" });
                        EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), fields, new object[] { "E6" });
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        private class ViewUnionTimeWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#time(10 sec) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        private class ViewUnionTimeWinSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') select irstream TheString from SupportBean#time(10 seconds)#unique(IntPrimitive) retain-union";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        private class ViewUnionTimeWinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') create window MyWindowTwo#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean;\n" +
                    "insert into MyWindowTwo select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowTwo where IntBoxed = Id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        private class ViewUnionTimeWinNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@name('s0') create window MyWindowThree#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean;\n" +
                    "insert into MyWindowThree select * from SupportBean;\n" +
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

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(20));
                env.AssertPropsOld("s0", fields, new object[] { "E2" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1"));

                env.Milestone(2);

                env.AdvanceTime(3000);
                SendEvent(env, "E3", 3, 30);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3"));
                env.AssertPropsNew("s0", fields, new object[] { "E3" });
                SendEvent(env, "E4", 3, 40);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3", "E4"));
                env.AssertPropsNew("s0", fields, new object[] { "E4" });

                env.Milestone(3);

                env.AdvanceTime(4000);
                SendEvent(env, "E5", 4, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3", "E4", "E5"));
                env.AssertPropsNew("s0", fields, new object[] { "E5" });
                SendEvent(env, "E6", 4, 50);
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3", "E4", "E5", "E6"));
                env.AssertPropsNew("s0", fields, new object[] { "E6" });

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(20));
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3", "E4", "E5", "E6"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S0(50));
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastOldData.Length);
                        object[] result =
                            { listener.LastOldData[0].Get("TheString"), listener.LastOldData[1].Get("TheString") };
                        EPAssertionUtil.AssertEqualsAnyOrder(result, new string[] { "E5", "E6" });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E3", "E4"));

                env.Milestone(6);

                env.AdvanceTime(12999);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(7);

                env.AdvanceTime(13000);
                env.AssertPropsOld("s0", fields, new object[] { "E3" });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E4"));

                env.AdvanceTime(10000000);
                env.AssertListenerNotInvoked("s0");

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

            env.AdvanceTime(3000);
            SendEvent(env, "E3", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3"));
            env.AssertPropsNew("s0", fields, new object[] { "E3" });

            env.Milestone(2);

            env.AdvanceTime(4000);
            SendEvent(env, "E4", 3);
            env.AssertPropsNew("s0", fields, new object[] { "E4" });
            SendEvent(env, "E5", 1);
            env.AssertPropsNew("s0", fields, new object[] { "E5" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5"));
            SendEvent(env, "E6", 3);
            env.AssertPropsNew("s0", fields, new object[] { "E6" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6"));

            env.Milestone(3);

            env.AdvanceTime(5000);
            SendEvent(env, "E7", 4);
            env.AssertPropsNew("s0", fields, new object[] { "E7" });
            SendEvent(env, "E8", 4);
            env.AssertPropsNew("s0", fields, new object[] { "E8" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8"));

            env.Milestone(4);

            env.AdvanceTime(6000);
            SendEvent(env, "E9", 4);
            env.AssertPropsNew("s0", fields, new object[] { "E9" });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.AdvanceTime(10999);
            env.AssertListenerNotInvoked("s0");

            env.Milestone(5);

            env.AdvanceTime(11000);
            env.AssertPropsOld("s0", fields, new object[] { "E1" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.AdvanceTime(12999);
            env.AssertListenerNotInvoked("s0");
            env.AdvanceTime(13000);
            env.AssertPropsOld("s0", fields, new object[] { "E3" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.Milestone(6);

            env.AdvanceTime(14000);
            env.AssertPropsOld("s0", fields, new object[] { "E4" });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E5", "E6", "E7", "E8", "E9"));

            env.Milestone(7);

            env.AdvanceTime(15000);
            env.AssertPropsPerRowIRPair(
                "s0",
                fields,
                null,
                new object[][] { new object[] { "E7" }, new object[] { "E8" } });
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E5", "E6", "E9"));

            env.AdvanceTime(1000000);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, ToArr("E2", "E5", "E6", "E9"));
        }

        private class ViewUnionInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text = null;

                text =
                    "select TheString from SupportBean#groupwin(TheString)#unique(TheString)#merge(IntPrimitive) retain-union";
                env.TryInvalidCompile(
                    text,
                    "Failed to validate data window declaration: Mismatching parameters between 'group' and 'merge'");

                text =
                    "select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union";
                env.TryInvalidCompile(
                    text,
                    "Failed to validate data window declaration: Multiple groupwin-declarations are not supported [select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union]");
            }
        }

        private static void TryAssertionFirstUniqueAndFirstLength(RegressionEnvironment env)
        {
            var fields = new string[] { "TheString", "IntPrimitive" };

            SendEvent(env, "E1", 1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
            env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

            SendEvent(env, "E1", 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 2 } });
            env.AssertPropsNew("s0", fields, new object[] { "E1", 2 });

            SendEvent(env, "E2", 1);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E2", 1 } });
            env.AssertPropsNew("s0", fields, new object[] { "E2", 1 });

            SendEvent(env, "E2", 3);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E2", 1 } });
            env.AssertListenerNotInvoked("s0");

            SendEvent(env, "E3", 3);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E2", 1 },
                    new object[] { "E3", 3 }
                });
            env.AssertPropsNew("s0", fields, new object[] { "E3", 3 });

            SendEvent(env, "E3", 4);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E1", 1 }, new object[] { "E1", 2 }, new object[] { "E2", 1 },
                    new object[] { "E3", 3 }
                });
            env.AssertListenerNotInvoked("s0");
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
    }
} // end of namespace