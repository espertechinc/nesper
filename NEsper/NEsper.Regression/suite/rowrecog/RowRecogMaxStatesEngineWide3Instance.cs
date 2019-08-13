///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogMaxStatesEngineWide3Instance : RegressionExecution
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;

        public void Run(RegressionEnvironment env)
        {
            handler = SupportConditionHandlerFactory.LastHandler;

            RunAssertionTwoStatementNoDelete(env);
            RunAssertionContextPartitionAndOverflow(env);
            RunAssertionNamedWindowInSequenceRemoveEvent(env);
            RunAssertionNamedWindowOutOfSequenceRemoveEvent(env);
        }

        private void RunAssertionTwoStatementNoDelete(RegressionEnvironment env)
        {
            var fields = new [] { "c0" };
            var eplOne = "@Name('S1') select * from SupportBean(TheString='A') " +
                         "match_recognize (" +
                         "  measures P1.LongPrimitive as c0" +
                         "  pattern (P1 P2 P3) " +
                         "  define " +
                         "    P1 as P1.IntPrimitive = 1," +
                         "    P2 as P2.IntPrimitive = 1," +
                         "    P3 as P3.IntPrimitive = 2 and P3.LongPrimitive = P1.LongPrimitive" +
                         ")";
            env.CompileDeploy(eplOne).AddListener("S1");

            var eplTwo = "@Name('S2') select * from SupportBean(TheString='B') " +
                         "match_recognize (" +
                         "  measures P1.LongPrimitive as c0" +
                         "  pattern (P1 P2 P3) " +
                         "  define " +
                         "    P1 as P1.IntPrimitive = 1," +
                         "    P2 as P2.IntPrimitive = 1," +
                         "    P3 as P3.IntPrimitive = 2 and P3.LongPrimitive = P1.LongPrimitive" +
                         ")";
            env.CompileDeploy(eplTwo).AddListener("S2");

            env.SendEventBean(MakeBean("A", 1, 10)); // A(10):P1=>P2
            env.SendEventBean(MakeBean("B", 1, 11)); // A(10):P1=>P2, B(11):P1=>P2
            env.SendEventBean(MakeBean("A", 1, 12)); // A(10):P2=>P3, A(12):P1=>P2, B(11):P1=>P2
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("B", 1, 13)); // would be: A(10):P2=>P3, A(12):P1=>P2, B(11):P2=>P3, B(13):P1=>P2
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 2, "S2", 1));

            // terminate B
            env.SendEventBean(MakeBean("B", 2, 11)); // we have no more B-state
            EPAssertionUtil.AssertProps(
                env.Listener("S2").AssertOneGetNewAndReset(),
                fields,
                new object[] {11L});

            // should not overflow
            env.SendEventBean(MakeBean("B", 1, 15));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("B", 1, 16));
            AssertContextEnginePool(
                env,
                env.Statement("S2"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 2, "S2", 1));

            // terminate A
            env.SendEventBean(MakeBean("A", 2, 10)); // we have no more A-state
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {10L});

            // should not overflow
            env.SendEventBean(MakeBean("B", 1, 17));
            env.SendEventBean(MakeBean("B", 1, 18));
            env.SendEventBean(MakeBean("A", 1, 19));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("A", 1, 20));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 1, "S2", 2));

            // terminate B
            env.SendEventBean(MakeBean("B", 2, 17));
            EPAssertionUtil.AssertProps(
                env.Listener("S2").AssertOneGetNewAndReset(),
                fields,
                new object[] {17L});

            // terminate A
            env.SendEventBean(MakeBean("A", 2, 19));
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {19L});

            env.UndeployAll();
        }

        private void RunAssertionContextPartitionAndOverflow(RegressionEnvironment env)
        {
            var fields = new [] { "c0" };
            var path = new RegressionPath();
            var eplCtx =
                "create context MyCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(P10 = s0.P00)";
            env.CompileDeploy(eplCtx, path);

            var epl = "@Name('S1') context MyCtx select * from SupportBean(TheString = context.s0.P00) " +
                      "match_recognize (" +
                      "  measures P2.TheString as c0" +
                      "  pattern (P1 P2) " +
                      "  define " +
                      "    P1 as P1.IntPrimitive = 1," +
                      "    P2 as P2.IntPrimitive = 2" +
                      ")";
            env.CompileDeploy(epl, path).AddListener("S1");

            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.SendEventBean(new SupportBean("A", 1));
            env.SendEventBean(new SupportBean_S0(0, "B"));
            env.SendEventBean(new SupportBean("B", 1));
            env.SendEventBean(new SupportBean_S0(0, "C"));
            env.SendEventBean(new SupportBean("C", 1));
            env.SendEventBean(new SupportBean_S0(0, "D"));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("D", 1));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            // terminate a context partition
            env.SendEventBean(new SupportBean_S1(0, "D"));
            env.SendEventBean(new SupportBean("D", 1));
            env.SendEventBean(new SupportBean_S0(0, "E"));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            env.SendEventBean(new SupportBean("E", 1));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            env.SendEventBean(new SupportBean("A", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {"A"});

            env.UndeployAll();
        }

        private void RunAssertionNamedWindowInSequenceRemoveEvent(RegressionEnvironment env)
        {
            var fields = new [] { "c0", "c1" };
            var path = new RegressionPath();

            var namedWindow = "create window MyWindow#keepall as SupportBean";
            env.CompileDeploy(namedWindow, path);
            var insert = "insert into MyWindow select * from SupportBean";
            env.CompileDeploy(insert, path);
            var delete = "on SupportBean_S0 delete from MyWindow where TheString = P00";
            env.CompileDeploy(delete, path);

            var epl = "@Name('S1') select * from MyWindow " +
                      "match_recognize (" +
                      "  partition by TheString " +
                      "  measures P1.LongPrimitive as c0, P2.LongPrimitive as c1" +
                      "  pattern (P1 P2) " +
                      "  define " +
                      "    P1 as P1.IntPrimitive = 0," +
                      "    P2 as P2.IntPrimitive = 1" +
                      ")";

            env.CompileDeploy(epl, path).AddListener("S1");

            env.SendEventBean(MakeBean("A", 0, 1));
            env.SendEventBean(MakeBean("B", 0, 2));
            env.SendEventBean(MakeBean("C", 0, 3));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("D", 0, 4));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            // delete A (in-sequence remove)
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(MakeBean("D", 0, 5)); // now 3 states: B, C, D
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // test matching
            env.SendEventBean(MakeBean("B", 1, 6)); // now 2 states: C, D
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {2L, 6L});

            // no overflows
            env.SendEventBean(MakeBean("E", 0, 7));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("F", 0, 9));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            // no match expected
            env.SendEventBean(MakeBean("F", 1, 10));
            Assert.IsFalse(env.Listener("S1").IsInvoked);

            env.UndeployAll();
        }

        private void RunAssertionNamedWindowOutOfSequenceRemoveEvent(RegressionEnvironment env)
        {
            var fields = new [] { "c0", "c1", "c2" };
            var path = new RegressionPath();

            var namedWindow = "create window MyWindow#keepall as SupportBean";
            env.CompileDeploy(namedWindow, path);
            var insert = "insert into MyWindow select * from SupportBean";
            env.CompileDeploy(insert, path);
            var delete = "on SupportBean_S0 delete from MyWindow where TheString = P00 and IntPrimitive = Id";
            env.CompileDeploy(delete, path);

            var epl = "@Name('S1') select * from MyWindow " +
                      "match_recognize (" +
                      "  partition by TheString " +
                      "  measures P1.LongPrimitive as c0, P2.LongPrimitive as c1, P3.LongPrimitive as c2" +
                      "  pattern (P1 P2 P3) " +
                      "  define " +
                      "    P1 as P1.IntPrimitive = 0," +
                      "    P2 as P2.IntPrimitive = 1," +
                      "    P3 as P3.IntPrimitive = 2" +
                      ")";
            env.CompileDeploy(epl, path).AddListener("S1");

            env.SendEventBean(MakeBean("A", 0, 1));
            env.SendEventBean(MakeBean("A", 1, 2));
            env.SendEventBean(MakeBean("B", 0, 3));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // delete A-1 (out-of-sequence remove)
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S0(0, "A"));
            env.SendEventBean(MakeBean("A", 2, 4));
            Assert.IsFalse(env.Listener("S1").IsInvoked);
            Assert.IsTrue(handler.Contexts.IsEmpty()); // states: B

            // test overflow
            env.SendEventBean(MakeBean("C", 0, 5));
            env.SendEventBean(MakeBean("D", 0, 6));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("E", 0, 7));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            // assert nothing matches for overflowed and deleted
            env.SendEventBean(MakeBean("E", 1, 8));
            env.SendEventBean(MakeBean("E", 2, 9));
            env.SendEventBean(new SupportBean_S0(0, "C")); // delete c
            env.SendEventBean(MakeBean("C", 1, 10));
            env.SendEventBean(MakeBean("C", 2, 11));
            Assert.IsFalse(env.Listener("S1").IsInvoked);

            // assert match found for B
            env.SendEventBean(MakeBean("B", 1, 12));
            env.SendEventBean(MakeBean("B", 2, 13));
            EPAssertionUtil.AssertProps(
                env.Listener("S1").AssertOneGetNewAndReset(),
                fields,
                new object[] {3L, 12L, 13L});

            // no overflow
            env.SendEventBean(MakeBean("F", 0, 14));
            env.SendEventBean(MakeBean("G", 0, 15));
            Assert.IsTrue(handler.Contexts.IsEmpty());

            // overflow
            env.SendEventBean(MakeBean("H", 0, 16));
            AssertContextEnginePool(
                env,
                env.Statement("S1"),
                handler.GetAndResetContexts(),
                3,
                GetExpectedCountMap(env, "S1", 3));

            env.UndeployAll();
        }

        internal static void AssertContextEnginePool(
            RegressionEnvironment env,
            EPStatement stmt,
            IList<ConditionHandlerContext> contexts,
            int max,
            IDictionary<DeploymentIdNamePair, long> counts)
        {
            Assert.AreEqual(1, contexts.Count);
            var context = contexts[0];
            Assert.AreEqual(env.RuntimeURI, context.RuntimeURI);
            Assert.AreEqual(stmt.DeploymentId, context.DeploymentId);
            Assert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionMatchRecognizeStatesMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            Assert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
            }

            contexts.Clear();
        }

        internal static IDictionary<DeploymentIdNamePair, long> GetExpectedCountMap(
            RegressionEnvironment env,
            string stmtOne,
            long countOne,
            string stmtTwo,
            long countTwo)
        {
            IDictionary<DeploymentIdNamePair, long> result = new Dictionary<DeploymentIdNamePair, long>();
            result.Put(new DeploymentIdNamePair(env.DeploymentId(stmtOne), stmtOne), countOne);
            result.Put(new DeploymentIdNamePair(env.DeploymentId(stmtTwo), stmtTwo), countTwo);
            return result;
        }

        internal static IDictionary<DeploymentIdNamePair, long> GetExpectedCountMap(
            RegressionEnvironment env,
            string stmtOne,
            long countOne)
        {
            IDictionary<DeploymentIdNamePair, long> result = new Dictionary<DeploymentIdNamePair, long>();
            result.Put(new DeploymentIdNamePair(env.DeploymentId(stmtOne), stmtOne), countOne);
            return result;
        }

        internal static SupportBean MakeBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var supportBean = new SupportBean(theString, intPrimitive);
            supportBean.LongPrimitive = longPrimitive;
            return supportBean;
        }
    }
} // end of namespace