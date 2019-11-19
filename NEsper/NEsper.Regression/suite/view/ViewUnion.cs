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

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewUnion
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewUnionFirstUniqueAndFirstLength());
            execs.Add(new ViewUnionBatchWindow());
            execs.Add(new ViewUnionAndDerivedValue());
            execs.Add(new ViewUnionGroupBy());
            execs.Add(new ViewUnionThreeUnique());
            execs.Add(new ViewUnionPattern());
            execs.Add(new ViewUnionTwoUnique());
            execs.Add(new ViewUnionSorted());
            execs.Add(new ViewUnionTimeWin());
            execs.Add(new ViewUnionTimeWinSODA());
            execs.Add(new ViewUnionInvalid());
            execs.Add(new ViewUnionSubselect());
            execs.Add(new ViewUnionFirstUniqueAndLengthOnDelete());
            execs.Add(new ViewUnionTimeWinNamedWindow());
            execs.Add(new ViewUnionTimeWinNamedWindowDelete());
            return execs;
        }

        private static void TryAssertionTimeWinUnique(RegressionEnvironment env)
        {
            string[] fields = {"TheString"};

            env.AdvanceTime(1000);
            SendEvent(env, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1"});

            env.Milestone(1);

            env.AdvanceTime(2000);
            SendEvent(env, "E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2"});

            env.AdvanceTime(3000);
            SendEvent(env, "E3", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3"});

            env.Milestone(2);

            env.AdvanceTime(4000);
            SendEvent(env, "E4", 3);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E4"});
            SendEvent(env, "E5", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E5"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E1", "E2", "E3", "E4", "E5"));
            SendEvent(env, "E6", 3);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E6"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6"));

            env.Milestone(3);

            env.AdvanceTime(5000);
            SendEvent(env, "E7", 4);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E7"});
            SendEvent(env, "E8", 4);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E8"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8"));

            env.Milestone(4);

            env.AdvanceTime(6000);
            SendEvent(env, "E9", 4);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E9"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.AdvanceTime(10999);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(5);

            env.AdvanceTime(11000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E1"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.AdvanceTime(12999);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            env.AdvanceTime(13000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E2", "E4", "E5", "E6", "E7", "E8", "E9"));

            env.Milestone(6);

            env.AdvanceTime(14000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E4"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E2", "E5", "E6", "E7", "E8", "E9"));

            env.Milestone(7);

            env.AdvanceTime(15000);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[0],
                fields,
                new object[] {"E7"});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[1],
                fields,
                new object[] {"E8"});
            env.Listener("s0").Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E2", "E5", "E6", "E9"));

            env.AdvanceTime(1000000);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                ToArr("E2", "E5", "E6", "E9"));
        }

        private static void TryAssertionFirstUniqueAndFirstLength(RegressionEnvironment env)
        {
            string[] fields = {"TheString", "IntPrimitive"};

            SendEvent(env, "E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1});

            SendEvent(env, "E1", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 2});

            SendEvent(env, "E2", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 1});

            SendEvent(env, "E2", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}});
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            SendEvent(env, "E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 3});

            SendEvent(env, "E3", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 1}, new object[] {"E3", 3}});
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
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
                arr[i] = new[] {values[i]};
            }

            return arr;
        }

        internal class ViewUnionFirstUniqueAndLengthOnDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create window MyWindowOne#firstunique(TheString)#firstlength(3) retain-union as SupportBean;\n" +
                    "insert into MyWindowOne select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowOne where TheString = P00;\n" +
                    "@Name('s0') select irstream * from MyWindowOne;\n";
                env.CompileDeploy(epl).AddListener("s0");
                string[] fields = {"TheString", "IntPrimitive"};

                SendEvent(env, "E1", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(0);

                SendEvent(env, "E1", 99);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E1", 99}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 99});

                SendEvent(env, "E2", 2);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E1", 99}, new object[] {"E2", 2}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", 2}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    new [] { "TheString" },
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[1],
                    new [] { "TheString" },
                    new object[] {"E1"});
                env.Listener("s0").Reset();

                env.Milestone(1);

                SendEvent(env, "E1", 3);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 3}, new object[] {"E2", 2}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3});

                env.UndeployAll();
            }
        }

        internal class ViewUnionFirstUniqueAndFirstLength : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select irstream TheString, IntPrimitive from SupportBean#firstlength(3)#firstunique(TheString) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionFirstUniqueAndFirstLength(env);

                env.UndeployAll();

                epl =
                    "@Name('s0') select irstream TheString, IntPrimitive from SupportBean#firstunique(TheString)#firstlength(3) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionFirstUniqueAndFirstLength(env);

                env.UndeployAll();
            }
        }

        internal class ViewUnionBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#length_batch(3)#unique(IntPrimitive) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                SendEvent(env, "E2", 2);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                SendEvent(env, "E3", 3);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                SendEvent(env, "E4", 4);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                SendEvent(env, "E5", 4);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4", "E5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                SendEvent(env, "E6", 4); // remove stream is E1, E2, E3
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4", "E5", "E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6"});

                SendEvent(env, "E7", 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7"});

                SendEvent(env, "E8", 6);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E5", "E4", "E6", "E7", "E8"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E8"});

                SendEvent(env, "E9", 7); // remove stream is E4, E5, E6; E4 and E5 get removed as their
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E6", "E7", "E8", "E9"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E9"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewUnionAndDerivedValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"total"};

                var epl =
                    "@Name('s0') select * from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#uni(DoublePrimitive) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10, 100d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr(100d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100d});

                SendEvent(env, "E2", 2, 20, 50d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr(150d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {150d});

                SendEvent(env, "E3", 1, 20, 20d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr(170d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {170d});

                env.UndeployAll();
            }
        }

        internal class ViewUnionGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var text =
                    "@Name('s0') select irstream TheString from SupportBean#groupwin(IntPrimitive)#length(2)#unique(IntBoxed) retain-union";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                SendEvent(env, "E2", 2, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                SendEvent(env, "E3", 1, 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);

                SendEvent(env, "E4", 1, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                env.Milestone(4);

                SendEvent(env, "E5", 2, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3", "E4", "E5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.Milestone(5);

                SendEvent(env, "E6", 1, 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E4", "E5", "E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E6"});
                env.Listener("s0").Reset();

                SendEvent(env, "E7", 1, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E2", "E4", "E5", "E6", "E7"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E7"});
                env.Listener("s0").Reset();

                env.Milestone(6);

                SendEvent(env, "E8", 2, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E5", "E6", "E7", "E8"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E8"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewUnionSubselect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select * from SupportBean_S0 where P00 in (select TheString from SupportBean#length(2)#unique(IntPrimitive) retain-union)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                SendEvent(env, "E1", 1);
                SendEvent(env, "E2", 2);

                env.Milestone(1);

                SendEvent(env, "E3", 3);
                SendEvent(env, "E4", 2); // throws out E1
                SendEvent(env, "E5", 1); // retains E3

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "E2"));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(1, "E3"));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "E4"));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ViewUnionThreeUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed)#unique(DoublePrimitive) retain-union";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 10, 100d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(0);

                SendEvent(env, "E2", 2, 10, 200d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(1);

                SendEvent(env, "E3", 2, 20, 100d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(2);

                SendEvent(env, "E4", 1, 30, 300d);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E2", "E3", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E4"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewUnionPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"string"};

                var text =
                    "@Name('s0') select irstream a.P00||b.P10 as string from pattern [every a=SupportBean_S0 -> b=SupportBean_S1]#unique(a.Id)#unique(b.Id) retain-union";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean_S1(2, "E2"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1E2"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10, "E3"));
                env.SendEventBean(new SupportBean_S1(20, "E4"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1E2", "E3E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3E4"});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "E5"));
                env.SendEventBean(new SupportBean_S1(2, "E6"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E3E4", "E5E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E1E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E5E6"});

                env.UndeployAll();
            }
        }

        internal class ViewUnionTwoUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#unique(IntBoxed) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                SendEvent(env, "E2", 2, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                SendEvent(env, "E3", 1, 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E2", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E3"});
                env.Listener("s0").Reset();

                env.Milestone(3);

                SendEvent(env, "E4", 1, 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E2", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E4"});
                env.Listener("s0").Reset();

                env.Milestone(4);

                SendEvent(env, "E5", 2, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E2", "E4", "E5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});

                env.Milestone(5);

                SendEvent(env, "E6", 3, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E5", "E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E6"});
                env.Listener("s0").Reset();

                env.Milestone(6);

                SendEvent(env, "E7", 3, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E5", "E6", "E7"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7"});

                env.Milestone(7);

                SendEvent(env, "E8", 4, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E5", "E7", "E8"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E6"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E8"});
                env.Listener("s0").Reset();

                env.Milestone(8);

                SendEvent(env, "E9", 3, 50);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E5", "E7", "E8", "E9"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E9"});

                env.Milestone(9);

                SendEvent(env, "E10", 2, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E8", "E9", "E10"));
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E5"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[1],
                    fields,
                    new object[] {"E7"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E10"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewUnionSorted : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString"};

                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#sort(2, IntPrimitive)#sort(2, IntBoxed) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                SendEvent(env, "E2", 2, 9);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                SendEvent(env, "E3", 0, 0);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);

                SendEvent(env, "E4", -1, -1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E3", "E4"));
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                object[] result = {
                    env.Listener("s0").LastOldData[0].Get("TheString"),
                    env.Listener("s0").LastOldData[1].Get("TheString")
                };
                EPAssertionUtil.AssertEqualsAnyOrder(result, new[] {"E1", "E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E4"});
                env.Listener("s0").Reset();

                env.Milestone(4);

                SendEvent(env, "E5", 1, 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E3", "E4"));
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E5"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E5"});
                env.Listener("s0").Reset();

                env.Milestone(5);

                SendEvent(env, "E6", 0, 0);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E4", "E6"));
                Assert.AreEqual(1, env.Listener("s0").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOld(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {"E6"});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ViewUnionTimeWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#unique(IntPrimitive)#time(10 sec) retain-union";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewUnionTimeWinSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') select irstream TheString from SupportBean#time(10 seconds)#unique(IntPrimitive) retain-union";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewUnionTimeWinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') create window MyWindowTwo#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean;\n" +
                    "insert into MyWindowTwo select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowTwo where IntBoxed = Id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertionTimeWinUnique(env);

                env.UndeployAll();
            }
        }

        internal class ViewUnionTimeWinNamedWindowDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') create window MyWindowThree#time(10 sec)#unique(IntPrimitive) retain-union as select * from SupportBean;\n" +
                    "insert into MyWindowThree select * from SupportBean;\n" +
                    "on SupportBean_S0 delete from MyWindowThree where IntBoxed = Id;\n";
                env.CompileDeploy(epl).AddListener("s0");

                string[] fields = {"TheString"};

                env.AdvanceTime(1000);
                SendEvent(env, "E1", 1, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(0);

                env.AdvanceTime(2000);
                SendEvent(env, "E2", 2, 20);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, ToArr("E1"));

                env.Milestone(2);

                env.AdvanceTime(3000);
                SendEvent(env, "E3", 3, 30);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});
                SendEvent(env, "E4", 3, 40);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3", "E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});

                env.Milestone(3);

                env.AdvanceTime(4000);
                SendEvent(env, "E5", 4, 50);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3", "E4", "E5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5"});
                SendEvent(env, "E6", 4, 50);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3", "E4", "E5", "E6"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6"});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(20));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3", "E4", "E5", "E6"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S0(50));
                Assert.AreEqual(2, env.Listener("s0").LastOldData.Length);
                object[] result = {
                    env.Listener("s0").LastOldData[0].Get("TheString"),
                    env.Listener("s0").LastOldData[1].Get("TheString")
                };
                EPAssertionUtil.AssertEqualsAnyOrder(result, new[] {"E5", "E6"});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E3", "E4"));

                env.Milestone(6);

                env.AdvanceTime(12999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(7);

                env.AdvanceTime(13000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    ToArr("E1", "E4"));

                env.AdvanceTime(10000000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ViewUnionInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string text = null;

                text =
                    "select TheString from SupportBean#groupwin(TheString)#unique(TheString)#merge(IntPrimitive) retain-union";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    text,
                    "Failed to validate data window declaration: Mismatching parameters between 'group' and 'merge'");

                text =
                    "select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    text,
                    "Failed to validate data window declaration: Multiple groupwin-declarations are not supported [select TheString from SupportBean#groupwin(TheString)#groupwin(IntPrimitive)#unique(TheString)#unique(IntPrimitive) retain-union]");
            }
        }
    }
} // end of namespace