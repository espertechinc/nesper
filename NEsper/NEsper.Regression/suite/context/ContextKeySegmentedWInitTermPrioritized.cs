///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedWInitTermPrioritized
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedTermByFilter(false));
            execs.Add(new ContextKeySegmentedTermByFilter(true));
            execs.Add(new ContextKeySegmentedTermByFilterWSubtype());
            execs.Add(new ContextKeySegmentedTermByFilterWSecondType());
            execs.Add(new ContextKeySegmentedTermByAfter());
            execs.Add(new ContextKeySegmentedTermByCrontabOutputWhenTerminated());
            execs.Add(new ContextKeySegmentedTermByPatternTwoFilters());
            execs.Add(new ContextKeySegmentedTermByUnrelated());
            execs.Add(new ContextKeySegmentedTermByFilter2Keys());
            execs.Add(new ContextKeySegmentedFilterExprTermByFilterWExpr());
            execs.Add(new ContextKeySegmentedFilterExprTermByFilter());
            execs.Add(new ContextKeySegmentedTermByPattern3Partition());
            execs.Add(new ContextKeySegmentedInitTermNoPartitionFilter());
            execs.Add(new ContextKeySegmentedInitTermWithPartitionFilter());
            execs.Add(new ContextKeySegmentedInitTermWithTwoInit(true));
            execs.Add(new ContextKeySegmentedInitTermWithTwoInit(false));
            execs.Add(new ContextKeySegmentedInitNoTerm(true));
            execs.Add(new ContextKeySegmentedInitNoTerm(false));
            execs.Add(new ContextKeySegmentedInitWCorrelatedTermFilter());
            execs.Add(new ContextKeySegmentedInitWCorrelatedTermPattern());
            execs.Add(new ContextKeySegmentedWithCorrelatedTermFilter(true));
            execs.Add(new ContextKeySegmentedWithCorrelatedTermFilter(false));
            execs.Add(new ContextKeySegmentedInvalid());
            return execs;
        }

        private static void SendAssertSB(
            long expected,
            RegressionEnvironment env,
            string theString)
        {
            SendAssertSB(expected, env, theString, 0);
        }

        private static void SendAssertSB(
            long expected,
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "TheString","cnt" },
                new object[] {theString, expected});
        }

        private static void SendAssertNone(
            RegressionEnvironment env,
            object @event)
        {
            env.SendEventBean(@event);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static SupportBean SendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            bool boolPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.BoolPrimitive = boolPrimitive;
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
            return sb;
        }

        private static void SendS0(
            RegressionEnvironment env,
            string p00,
            string p01)
        {
            env.SendEventBean(new SupportBean_S0(0, p00, p01));
        }

        private static void SendS1(
            RegressionEnvironment env,
            string p10,
            string p11)
        {
            env.SendEventBean(new SupportBean_S1(0, p10, p11));
        }

        private static void SendS2(
            RegressionEnvironment env,
            string p20,
            string p21)
        {
            env.SendEventBean(new SupportBean_S2(0, p20, p21));
        }

        private static void SendS0AssertNone(
            RegressionEnvironment env,
            int id,
            string p00,
            string p01)
        {
            env.SendEventBean(new SupportBean_S0(id, p00, p01));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static SupportBean_S0 SendS0Assert(
            int expected,
            SupportBean_S0 s0Init,
            RegressionEnvironment env,
            int id,
            string p00,
            string p01)
        {
            var s0 = new SupportBean_S0(id, p00, p01);
            env.SendEventBean(s0);
            var fields = new [] { "P00","P01","S0","theSum" };
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {p00, p01, s0Init ?? s0, expected});
            return s0;
        }

        internal class ContextKeySegmentedWithCorrelatedTermFilter : RegressionExecution
        {
            private readonly bool soda;

            public ContextKeySegmentedWithCorrelatedTermFilter(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@name('ctx') create context CtxPartitionWCorrTerm as " +
                          "partition by TheString from SupportBean as sb " +
                          "terminated by SupportBean(IntPrimitive=sb.IntPrimitive)";
                env.CompileDeploy(soda, epl, path);
                env.CompileDeploy(
                    "@name('s0') context CtxPartitionWCorrTerm select TheString, sum(IntPrimitive) as theSum from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");

                var fields = new [] { "TheString","theSum" };

                env.SendEventBean(new SupportBean("A", 10));
                env.SendEventBean(new SupportBean("B", 99));
                env.SendEventBean(new SupportBean("C", -1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 2));
                env.SendEventBean(new SupportBean("B", 3));
                env.SendEventBean(new SupportBean("C", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 10 + 2});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("C", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C", -1 + 4});

                env.SendEventBean(new SupportBean("A", 11));
                env.SendEventBean(new SupportBean("A", 12));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("B", 99));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 99 + 3});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("A", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 11 + 12});

                AssertFilterSvcCount(env, 1, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInitWCorrelatedTermFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@name('ctx') create context CtxPartitionInitWCorrTerm " +
                          "partition by TheString from SupportBean " +
                          "initiated by SupportBean(BoolPrimitive=true) as sb " +
                          "terminated by SupportBean(BoolPrimitive=false, IntPrimitive=sb.IntPrimitive)";
                env.CompileDeploy(epl, path);

                env.CompileDeploy(
                    "@name('s0') context CtxPartitionInitWCorrTerm select TheString, sum(LongPrimitive) as theSum from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");
                var fields = new [] { "TheString","theSum" };

                var initA = SendBean(env, "A", 100, 1, true);

                env.Milestone(0);

                SendBean(env, "B", 99, 2, false);
                var initB = SendBean(env, "B", 200, 3, true);
                SendBean(env, "A", 0, 4, false);
                SendBean(env, "B", 0, 5, false);
                SendBean(env, "A", 0, 6, true);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                AssertPartitionsInitWCorrelatedTermFilter(env);
                SupportContextPropUtil.AssertContextProps(
                    env,
                    "ctx",
                    "CtxPartitionInitWCorrTerm",
                    new[] {0, 1},
                    "key1,sb",
                    new[] {new object[] {"A", initA}, new object[] {"B", initB}});

                env.Milestone(1);

                SendBean(env, "B", 200, 7, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 3 + 5L});

                env.Milestone(2);

                SendBean(env, "A", 100, 8, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 1 + 4 + 6L});

                AssertFilterSvcCount(env, 1, "ctx");
                env.UndeployAll();
            }

            private static void AssertPartitionsInitWCorrelatedTermFilter(RegressionEnvironment env)
            {
                var partitions = env.Runtime.ContextPartitionService.GetContextPartitions(
                    env.DeploymentId("ctx"),
                    "CtxPartitionInitWCorrTerm",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(2, partitions.Identifiers.Count);
                var first = (ContextPartitionIdentifierPartitioned) partitions.Identifiers[0];
                var second = (ContextPartitionIdentifierPartitioned) partitions.Identifiers[1];
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"A"}, first.Keys);
                EPAssertionUtil.AssertEqualsExactOrder(new object[] {"B"}, second.Keys);
            }
        }

        internal class ContextKeySegmentedInitWCorrelatedTermPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@name('ctx') create context CtxPartitionInitWCorrTerm " +
                          "partition by P20 from SupportBean_S2, P10 from SupportBean_S1, P00 from SupportBean_S0 " +
                          "initiated by SupportBean_S0 as S0, SupportBean_S1 as S1 " +
                          "terminated by pattern[SupportBean_S0(Id=S0.Id) or SupportBean_S1(Id=S1.Id)]";
                env.CompileDeploy(epl, path);

                env.CompileDeploy(
                    "@name('s0') context CtxPartitionInitWCorrTerm " +
                    "select context.S0 as ctx0, context.S1 as ctx1, context.S0.Id as ctx0Id, context.S1.Id as ctx1Id, P20, sum(Id) as theSum " +
                    "from SupportBean_S2 output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "ctx0Id","ctx1Id","P20","theSum" };

                Assert.AreEqual(typeof(SupportBean_S0), env.Statement("s0").EventType.GetPropertyType("ctx0"));
                Assert.AreEqual(typeof(SupportBean_S1), env.Statement("s0").EventType.GetPropertyType("ctx1"));

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean_S1(2, "B"));
                env.SendEventBean(new SupportBean_S2(10, "A"));
                env.SendEventBean(new SupportBean_S2(11, "A"));
                env.SendEventBean(new SupportBean_S1(1, "A"));
                env.SendEventBean(new SupportBean_S0(2, "A"));
                env.SendEventBean(new SupportBean_S2(12, "B"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(1, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, null, "A", 21});

                env.SendEventBean(new SupportBean_S1(2, "B"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, 2, "B", 12});

                AssertFilterSvcCount(env, 2, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInitNoTerm : RegressionExecution
        {
            private readonly bool soda;

            public ContextKeySegmentedInitNoTerm(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "create context CtxInitS0PositiveId as " +
                          "partition by P00 and P01 from SupportBean_S0 " +
                          "initiated by SupportBean_S0(Id>0) as S0";
                env.CompileDeploy(soda, epl, path);
                env.CompileDeploy(
                    "@name('s0') context CtxInitS0PositiveId select P00, P01, context.S0 as S0, sum(Id) as theSum from SupportBean_S0",
                    path);
                env.AddListener("s0");

                Assert.AreEqual(typeof(SupportBean_S0), env.Statement("s0").EventType.GetPropertyType("S0"));

                SendS0AssertNone(env, 0, "A", "G1");
                SendS0AssertNone(env, -1, "B", "G1");

                env.Milestone(0);

                var s0BG1 = SendS0Assert(10, null, env, 10, "B", "G1");
                SendS0Assert(9, s0BG1, env, -1, "B", "G1");

                env.Milestone(1);

                var s0AG1 = SendS0Assert(2, null, env, 2, "A", "G1");

                env.Milestone(2);

                var s0AG2 = SendS0Assert(3, null, env, 3, "A", "G2");

                env.Milestone(3);

                SendS0Assert(7, s0AG2, env, 4, "A", "G2");
                SendS0Assert(8, s0AG1, env, 6, "A", "G1");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInitTermWithTwoInit : RegressionExecution
        {
            private readonly bool soda;

            public ContextKeySegmentedInitTermWithTwoInit(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@name('ctx') create context CtxTwoInitTerm as " +
                          "partition by P01 from SupportBean_S0, P11 from SupportBean_S1, P21 from SupportBean_S2 " +
                          "initiated by SupportBean_S0(P00=\"a\"), SupportBean_S1(P10=\"b\") " +
                          "terminated by SupportBean_S2(P20=\"z\")";
                env.CompileDeploy(soda, epl, path);
                env.CompileDeploy(
                    "@name('s0') context CtxTwoInitTerm select P21, count(*) as cnt from SupportBean_S2 output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "P21","cnt" };

                SendS2(env, "b", "A");
                SendS2(env, "a", "A");
                SendS0(env, "b", "A");
                SendS1(env, "a", "A");
                SendS2(env, "z", "A");
                SendS1(env, "b", "B");
                SendS0(env, "a", "C");
                SendS2(env, "", "B");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendS2(env, "z", "B");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 1L});

                env.Milestone(1);

                SendS2(env, "z", "C");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C", 0L});

                AssertFilterSvcCount(env, 2, "ctx");
                env.UndeployModuleContaining("s0");

                env.Milestone(2);

                AssertFilterSvcCount(env, 0, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInitTermWithPartitionFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context CtxStringZeroTo1k as " +
                          "partition by TheString from SupportBean(IntPrimitive > 0) " +
                          "initiated by SupportBean(IntPrimitive=0)" +
                          "terminated by SupportBean(IntPrimitive=1000);\n" +
                          "@name('s0') context CtxStringZeroTo1k select TheString, sum(IntPrimitive) as theSum from SupportBean output last when terminated;\n";
                env.CompileDeploy(epl).AddListener("s0");
                var fields = new [] { "TheString","theSum" };

                env.SendEventBean(new SupportBean("A", 20));
                env.SendEventBean(new SupportBean("A", 1000));
                env.SendEventBean(new SupportBean("B", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B", 30));
                env.SendEventBean(new SupportBean("B", -100));
                env.SendEventBean(new SupportBean("A", 1000));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 1000));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 30});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("A", 0));
                env.SendEventBean(new SupportBean("A", 40));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 50));
                env.SendEventBean(new SupportBean("A", -20));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean("A", 1000));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 90});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid initiated-by type
                epl = "create context InvalidCtx partition by TheString from SupportBean initiated by SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Segmented context 'InvalidCtx' requires that all of the event types that are listed in the initialized-by also appear in the partition-by, type 'SupportBean_S0' is not one of the types listed in partition-by");

                // cannot assign name in different places
                epl =
                    "create context InvalidCtx partition by P00 from SupportBean_S0 as n1 initiated by SupportBean_S0 as n2";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Segmented context 'InvalidCtx' requires that either partition-by or initialized-by assign stream names, but not both");

                // name assigned is already used
                var message = "Name 'a' already used for type 'SupportBean_S0'";
                epl =
                    "create context InvalidCtx partition by P00 from SupportBean_S0, P10 from SupportBean_S1 initiated by SupportBean_S0 as a, SupportBean_S1 as a";
                SupportMessageAssertUtil.TryInvalidCompile(env, epl, message);
                epl =
                    "create context InvalidCtx partition by P00 from SupportBean_S0 as a, P10 from SupportBean_S1 as a";
                SupportMessageAssertUtil.TryInvalidCompile(env, epl, message);
            }
        }

        internal class ContextKeySegmentedInitTermNoPartitionFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context CtxStringZeroTo1k as " +
                    "partition by TheString from SupportBean " +
                    "initiated by SupportBean(IntPrimitive=0)" +
                    "terminated by SupportBean(IntPrimitive=1000)",
                    path);
                env.CompileDeploy(
                    "@name('s0') context CtxStringZeroTo1k select TheString, sum(IntPrimitive) as theSum from SupportBean output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "TheString","theSum" };

                env.SendEventBean(new SupportBean("A", 20));
                env.SendEventBean(new SupportBean("A", 1000));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 30));
                env.SendEventBean(new SupportBean("A", 1000));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("B", 1000));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 30});

                env.SendEventBean(new SupportBean("C", 1000));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("C", -1));
                env.SendEventBean(new SupportBean("C", 1000));
                env.SendEventBean(new SupportBean("C", 0));

                env.Milestone(4);

                env.SendEventBean(new SupportBean("A", 0));

                env.Milestone(5);

                env.SendEventBean(new SupportBean("A", 40));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.SendEventBean(new SupportBean("C", 1000));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C", 0});

                env.Milestone(7);

                env.SendEventBean(new SupportBean("A", 1000));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 40});

                AssertFilterSvcCount(env, 1, "ctx");
                env.UndeployModuleContaining("s0");
                AssertFilterSvcCount(env, 0, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByPattern3Partition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context Ctx3Typed as " +
                    "partition by P00 from SupportBean_S0, P10 from SupportBean_S1, P20 from SupportBean_S2 " +
                    "terminated by pattern[SupportBean_S1 -> SupportBean_S2]",
                    path);
                env.CompileDeploy(
                    "@name('s0') context Ctx3Typed select P00, count(*) as cnt from SupportBean_S0 output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "P00","cnt" };

                env.SendEventBean(new SupportBean_S0(0, "A"));
                env.SendEventBean(new SupportBean_S0(0, "B"));
                env.SendEventBean(new SupportBean_S1(0, "B"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S2(0, "A"));
                env.SendEventBean(new SupportBean_S0(0, "B"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S2(0, "B"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 2L});

                env.SendEventBean(new SupportBean_S1(0, "A"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(0, "A"));
                env.SendEventBean(new SupportBean_S0(0, "A"));

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S2(0, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 3L});

                Assert.AreEqual(3, GetFilterSvcCountApprox(env));
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByFilter2Keys : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context TwoKeyPartition " +
                    "partition by TheString, IntPrimitive from SupportBean terminated by SupportBean(BoolPrimitive = false)",
                    path);
                env.CompileDeploy(
                    "@name('s0') context TwoKeyPartition select TheString, IntPrimitive, sum(LongPrimitive) as thesum from SupportBean output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "TheString","IntPrimitive","thesum" };

                SendBean(env, "A", 1, 10, true);
                SendBean(env, "B", 1, 11, true);
                SendBean(env, "A", 2, 12, true);
                SendBean(env, "B", 2, 13, true);

                env.Milestone(0);

                SendBean(env, "B", 1, 20, true);
                SendBean(env, "A", 1, 30, true);
                SendBean(env, "A", 2, 40, true);
                SendBean(env, "B", 2, 50, true);

                SendBean(env, "A", 2, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 2, 52L});

                env.Milestone(1);

                SendBean(env, "B", 2, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 2, 63L});

                SendBean(env, "A", 1, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 1, 40L});

                env.Milestone(2);

                SendBean(env, "B", 1, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 1, 31L});

                AssertFilterSvcCount(env, 1, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedFilterExprTermByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Audit @Name('ctx') create context MyTermByUnrelated partition by TheString from SupportBean(IntPrimitive=0) terminated by SupportBean",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByUnrelated select TheString, count(*) as cnt from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");
                var fields = new [] { "TheString","cnt" };

                env.SendEventBean(new SupportBean("A", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("B", 1));
                env.SendEventBean(new SupportBean("B", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 0));
                env.SendEventBean(new SupportBean("A", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("B", 99));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B", 1L}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 0));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A", 1L}});

                env.UndeployModuleContaining("s0");
                AssertFilterSvcCount(env, 0, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedFilterExprTermByFilterWExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Audit @Name('ctx') create context MyTermByUnrelated partition by TheString from SupportBean(IntPrimitive=0) terminated by SupportBean(IntPrimitive=1)",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByUnrelated select TheString, count(*) as cnt from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");
                var fields = new [] { "TheString","cnt" };

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 2));
                env.SendEventBean(new SupportBean("B", 1));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("B", 0));
                env.SendEventBean(new SupportBean("B", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("B", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"B", 1L}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("A", 0));
                env.UndeployModuleContaining("s0");
                AssertFilterSvcCount(env, 0, "ctx");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByUnrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyTermByUnrelated partition by TheString from SupportBean terminated by SupportBean_S0",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByUnrelated select TheString, count(*) as cnt from SupportBean output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "TheString","cnt" };

                env.SendEventBean(new SupportBean("A", 0));
                env.SendEventBean(new SupportBean("B", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("A", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(-1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A", 2L}, new object[] {"B", 1L}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("C", 0));
                env.SendEventBean(new SupportBean("A", 0));

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(-1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A", 1L}, new object[] {"C", 1L}});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByPatternTwoFilters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyTermByTimeout partition by P00 from SupportBean_S0, P10 from SupportBean_S1 terminated by pattern [SupportBean_S0(Id<0) or SupportBean_S1(Id<0)]",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByTimeout select coalesce(S0.P00, S1.P10) as key, count(*) as cnt from pattern [every (S0=SupportBean_S0 or S1=SupportBean_S1)] output last when terminated",
                    path);

                env.AddListener("s0");
                var fields = new [] { "key","cnt" };

                env.SendEventBean(new SupportBean_S0(0, "A"));
                env.SendEventBean(new SupportBean_S1(0, "A"));
                env.SendEventBean(new SupportBean_S1(0, "B"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(-1, "B")); // stop B
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 1L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(0, "B"));
                env.SendEventBean(new SupportBean_S0(0, "A"));
                env.SendEventBean(new SupportBean_S0(0, "B"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S1(-1, "A")); // stop A
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 3L});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(-1, "A")); // stop A
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(-1, "B")); // stop B
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 2L});

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S1(-1, "B")); // stop B
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByCrontabOutputWhenTerminated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendCurrentTime(env, "2002-02-01T09:00:00.000");

                env.CompileDeploy(
                    "create context MyTermByTimeout partition by TheString from SupportBean terminated (*, *, *, *, *)",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByTimeout select TheString, count(*) as cnt from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("A", 0));

                env.Milestone(0);

                SendCurrentTime(env, "2002-02-01T09:00:05.000");

                env.SendEventBean(new SupportBean("B", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("A", 0));

                SendCurrentTime(env, "2002-02-01T09:00:59.999");

                env.Milestone(2);

                env.SendEventBean(new SupportBean("B", 0));
                env.SendEventBean(new SupportBean("A", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendCurrentTime(env, "2002-02-01T09:01:00.000");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new [] { "TheString","cnt" },
                    new[] {new object[] {"A", 3L}, new object[] {"B", 2L}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("C", 0));
                env.SendEventBean(new SupportBean("A", 0));
                SendCurrentTime(env, "2002-02-01T09:01:30.000");
                env.SendEventBean(new SupportBean("D", 0));

                env.Milestone(4);

                env.SendEventBean(new SupportBean("C", 0));

                SendCurrentTime(env, "2002-02-01T09:02:00.000");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new [] { "TheString","cnt" },
                    new[] {new object[] {"A", 1L}, new object[] {"C", 2L}, new object[] {"D", 1L}});

                SendCurrentTime(env, "2002-02-01T09:03:00.000");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByAfter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyTermByTimeout partition by TheString from SupportBean terminated after 10 seconds",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyTermByTimeout select TheString, count(*) as cnt from SupportBean",
                    path);
                env.AddListener("s0");

                SendAssertSB(1, env, "A");

                env.Milestone(0);

                env.AdvanceTime(1000);

                SendAssertSB(2, env, "A");

                env.Milestone(1);

                SendAssertSB(1, env, "B");

                env.AdvanceTime(9999);

                SendAssertSB(2, env, "B");
                SendAssertSB(3, env, "A");

                env.Milestone(2);

                env.AdvanceTime(10000);

                env.Milestone(3);

                SendAssertSB(3, env, "B");
                SendAssertSB(1, env, "A");

                env.AdvanceTime(10999);

                SendAssertSB(4, env, "B");
                SendAssertSB(2, env, "A");

                env.AdvanceTime(11000);

                SendAssertSB(1, env, "B");

                env.Milestone(4);

                SendAssertSB(3, env, "A");

                env.Milestone(5);

                env.AdvanceTime(99999);

                SendAssertSB(1, env, "B");
                SendAssertSB(1, env, "A");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedTermByFilterWSecondType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create objectarray schema TypeOne(poa string);\n" +
                          "create map schema TypeTwo(pmap string);\n" +
                          "create context MyContextOAMap partition by poa from TypeOne, pmap from TypeTwo terminated by TypeTwo;\n" +
                          "@name('s0') context MyContextOAMap select poa, count(*) as cnt from TypeOne;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());

                env.AddListener("s0");

                SendOAAssert(env, "A", 1L);

                env.Milestone(0);

                SendOAAssert(env, "B", 1L);
                SendOAAssert(env, "A", 2L);
                SendOAAssert(env, "B", 2L);

                env.Milestone(1);

                env.SendEventMap(CollectionUtil.PopulateNameValueMap("pmap", "B"), "TypeTwo");

                SendOAAssert(env, "A", 3L);

                env.Milestone(2);

                SendOAAssert(env, "B", 1L);

                env.SendEventMap(CollectionUtil.PopulateNameValueMap("pmap", "A"), "TypeTwo");

                env.Milestone(3);

                SendOAAssert(env, "A", 1L);
                SendOAAssert(env, "B", 2L);

                env.UndeployAll();
            }

            private static void SendOAAssert(
                RegressionEnvironment env,
                string poa,
                long count)
            {
                env.SendEventObjectArray(new object[] {poa}, "TypeOne");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "poa","cnt" },
                    new object[] {poa, count});
            }
        }

        internal class ContextKeySegmentedTermByFilterWSubtype : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context ByP0 partition by A from ISupportA, B from ISupportB terminated by ISupportA(A='x')",
                    path);
                env.CompileDeploy(
                    "@name('s0') context ByP0 select coalesce(a.A, b.B) as p0, count(*) as cnt from pattern[every (a=ISupportA or b=ISupportB)]",
                    path);

                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new ISupportABCImpl("a", "a", null, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "p0","cnt" },
                    new object[] {"a", 1L});

                env.Milestone(1);

                env.SendEventBean(new ISupportAImpl("a", null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "p0","cnt" },
                    new object[] {"a", 2L});

                env.Milestone(2);

                env.SendEventBean(new ISupportBImpl("a", null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "p0","cnt" },
                    new object[] {"a", 3L});

                env.UndeployModuleContaining("s0");

                env.Milestone(3);

                env.UndeployModuleContaining("ctx");
            }
        }

        public class ContextKeySegmentedTermByFilter : RegressionExecution
        {
            private readonly bool soda;

            public ContextKeySegmentedTermByFilter(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    soda,
                    "create context ByP0 as partition by TheString from SupportBean terminated by SupportBean(IntPrimitive<0)",
                    path);
                env.CompileDeploy(
                    soda,
                    "@name('s0') context ByP0 select TheString, count(*) as cnt from SupportBean",
                    path);
                env.AddListener("s0");

                SendAssertSB(1, env, "A", 0);

                env.Milestone(0);

                SendAssertSB(2, env, "A", 0);

                env.Milestone(1);

                SendAssertNone(env, new SupportBean("A", -1));

                env.Milestone(2);

                SendAssertSB(1, env, "A", 0);

                SendAssertSB(1, env, "B", 0);
                SendAssertNone(env, new SupportBean("B", -1));
                SendAssertSB(1, env, "B", 0);
                SendAssertSB(2, env, "B", 0);

                env.Milestone(3);

                SendAssertNone(env, new SupportBean("B", -1));
                SendAssertSB(1, env, "B", 0);

                SendAssertSB(1, env, "C", -1);

                env.Milestone(4);

                SendAssertNone(env, new SupportBean("C", -1));

                env.Milestone(5);

                SendAssertSB(1, env, "C", -1);
                SendAssertNone(env, new SupportBean("C", -1));

                env.UndeployAll();
            }
        }
    }
} // end of namespace