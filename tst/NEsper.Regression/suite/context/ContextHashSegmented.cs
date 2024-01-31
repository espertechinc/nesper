///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;

using NEsper.Avro.Extensions;

using Newtonsoft.Json.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextHashSegmented
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSegmentedBasic(execs);
            WithSegmentedFilter(execs);
            WithNoPreallocate(execs);
            WithSegmentedManyArg(execs);
            WithSegmentedMulti(execs);
            WithSegmentedBySingleRowFunc(execs);
            WithScoringUseCase(execs);
            WithPartitionSelection(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithPartitionSelection(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashPartitionSelection());
            return execs;
        }

        public static IList<RegressionExecution> WithScoringUseCase(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashScoringUseCase());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedBySingleRowFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedBySingleRowFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedManyArg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedManyArg());
            return execs;
        }

        public static IList<RegressionExecution> WithNoPreallocate(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashNoPreallocate());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedBasic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedBasic());
            return execs;
        }

        private class ContextHashScoringUseCase : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    TryAssertionScoringUseCase(env, rep, milestone);
                }
            }

            private static void TryAssertionScoringUseCase(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                AtomicLong milestone)
            {
                var fields = "UserId,Keyword,SumScore".SplitCsv();
                var epl =
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvidedScoreCycle)) +
                    "@buseventtype @public create schema ScoreCycle (UserId string, Keyword string, ProductId string, Score long);\n" +
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(
                        typeof(MyLocalJsonProvidedUserKeywordTotalStream)) +
                    "@buseventtype @public create schema UserKeywordTotalStream (UserId string, Keyword string, SumScore long);\n" +
                    "\n" +
                    eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvided)) +
                    " create context HashByUserCtx as " +
                    "coalesce by consistent_hash_crc32(UserId) from ScoreCycle, " +
                    "consistent_hash_crc32(UserId) from UserKeywordTotalStream " +
                    "granularity 1000000;\n" +
                    "\n" +
                    "context HashByUserCtx create window ScoreCycleWindow#unique(ProductId, Keyword) as ScoreCycle;\n" +
                    "\n" +
                    "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                    "\n" +
                    "@name('s0') context HashByUserCtx insert into UserKeywordTotalStream \n" +
                    "select UserId, Keyword, sum(Score) as SumScore from ScoreCycleWindow group by Keyword;\n" +
                    "\n" +
                    "@name('outTwo') context HashByUserCtx on UserKeywordTotalStream(SumScore > 10000) delete from ScoreCycleWindow;\n";
                env.CompileDeploy(epl, new RegressionPath());
                env.AddListener("s0");

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P1", 100);
                env.AssertPropsNew("s0", fields, new object[] { "Pete", "K1", 100L });

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P2", 15);
                env.AssertPropsNew("s0", fields, new object[] { "Pete", "K1", 115L });

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P2", 30);
                env.AssertPropsNew("s0", fields, new object[] { "Joe", "K1", 30L });

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K2", "P1", 40);
                env.AssertPropsNew("s0", fields, new object[] { "Joe", "K2", 40L });

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P1", 20);
                env.AssertPropsNew("s0", fields, new object[] { "Joe", "K1", 50L });

                env.UndeployAll();
            }
        }

        public class ContextHashPartitionSelection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyCtx as coalesce consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate",
                    path);
                env.CompileDeploy(
                    "@name('s0') context MyCtx select context.id as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 5, "E1", 1 } });

                env.SendEventBean(new SupportBean("E2", 10));
                env.SendEventBean(new SupportBean("E1", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 100));
                env.SendEventBean(new SupportBean("E3", 101));

                env.SendEventBean(new SupportBean("E1", 3));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { 5, "E1", 6 }, new object[] { 15, "E2", 10 }, new object[] { 9, "E3", 201 } });
                SupportContextPropUtil.AssertContextProps(env, "ctx", "MyCtx", new int[] { 5, 15, 9 }, null, null);

                env.Milestone(2);

                // test iterator targeted hash
                env.AssertStatement(
                    "s0",
                    statement => {
                        var selector = new SupportSelectorByHashCode(Collections.SingletonSet(15));
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            statement.GetEnumerator(selector),
                            statement.GetSafeEnumerator(selector),
                            fields,
                            new object[][] { new object[] { 15, "E2", 10 } });
                        selector = new SupportSelectorByHashCode(new HashSet<int>(Arrays.AsList(1, 9, 5)));
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            statement.GetEnumerator(selector),
                            statement.GetSafeEnumerator(selector),
                            fields,
                            new object[][] { new object[] { 5, "E1", 6 }, new object[] { 9, "E3", 201 } });
                        ClassicAssert.IsFalse(
                            statement.GetEnumerator(new SupportSelectorByHashCode(Collections.SingletonSet(99)))
                                .MoveNext());
                        ClassicAssert.IsFalse(
                            statement.GetEnumerator(new SupportSelectorByHashCode(EmptySet<int>.Instance)).MoveNext());
                        ClassicAssert.IsFalse(statement.GetEnumerator(new SupportSelectorByHashCode(null)).MoveNext());

                        // test iterator filtered
                        var filtered = new MySelectorFilteredHash(Collections.SingletonSet<int>(15));
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            statement.GetEnumerator(filtered),
                            statement.GetSafeEnumerator(filtered),
                            fields,
                            new object[][] { new object[] { 15, "E2", 10 } });
                        filtered = new MySelectorFilteredHash(new HashSet<int>(Arrays.AsList(1, 9, 5)));
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(
                            statement.GetEnumerator(filtered),
                            statement.GetSafeEnumerator(filtered),
                            fields,
                            new object[][] { new object[] { 5, "E1", 6 }, new object[] { 9, "E3", 201 } });

                        // test always-false filter - compare context partition info
                        filtered = new MySelectorFilteredHash(EmptySet<int>.Instance);
                        ClassicAssert.IsFalse(statement.GetEnumerator(filtered).MoveNext());
                        ClassicAssert.AreEqual(16, filtered.Contexts.Count);

                        try {
                            statement.GetEnumerator(new ProxyContextPartitionSelectorSegmented(() => null));
                            Assert.Fail();
                        }
                        catch (InvalidContextPartitionSelector ex) {
                            ClassicAssert.IsTrue(
                                ex.Message.StartsWith(
                                    "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorHash] interfaces but received com."),
                                "message: " + ex.Message);
                        }
                    });

                env.UndeployAll();

                env.Milestone(3);
            }
        }

        private class ContextHashInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid filter spec
                epl = "create context ACtx coalesce hash_code(IntPrimitive) from SupportBean(dummy = 1) granularity 10";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // invalid hash code function
                epl = "create context ACtx coalesce hash_code_xyz(IntPrimitive) from SupportBean granularity 10";
                env.TryInvalidCompile(
                    epl,
                    "For context 'ACtx' expected a hash function that is any of {consistent_hash_crc32, hash_code} or a plug-in single-row function or script but received 'hash_code_xyz' [");
                epl = "create context ACtx coalesce IntPrimitive from SupportBean granularity 10";
                env.TryInvalidCompile(
                    epl,
                    "For context 'ACtx' expected a hash function that is any of {consistent_hash_crc32, hash_code} or a plug-in single-row function or script but received 'IntPrimitive' [");

                // invalid no-param hash code function
                epl = "create context ACtx coalesce hash_code() from SupportBean granularity 10";
                env.TryInvalidCompile(
                    epl,
                    "For context 'ACtx' expected one or more parameters to the hash function, but found no parameter list [");

                // validate statement not applicable filters
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context ACtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10",
                    path);
                epl = "context ACtx select * from SupportBean_S0";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Segmented context 'ACtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

                // invalid attempt to partition a named window's streams
                env.CompileDeploy("@public create window MyWindow#keepall as SupportBean", path);
                epl = "@public create context SegmentedByWhat partition by TheString from MyWindow";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Partition criteria may not include named windows [@public create context SegmentedByWhat partition by TheString from MyWindow]");

                env.UndeployAll();
            }
        }

        private class ContextHashSegmentedFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var fields = "c0,c1".SplitCsv();

                var eplCtx = "@name('context') @public create context " +
                             ctx +
                             " as " +
                             "coalesce " +
                             " consistent_hash_crc32(TheString) from SupportBean(IntPrimitive > 10) " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt = "@name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, IntPrimitive as c1 from SupportBean#lastevent";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 12));
                env.AssertPropsNew("s0", fields, new object[] { ctx, 12 });
                AssertIterator(env, "s0", fields, new object[][] { new object[] { ctx, 12 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 10));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 1));
                AssertIterator(env, "s0", fields, new object[][] { new object[] { ctx, 12 } });
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E6", 15));
                env.AssertPropsNew("s0", fields, new object[] { ctx, 15 });

                env.UndeployAll();
            }
        }

        public class ContextHashNoPreallocate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext = "@name('CTX') @public create context CtxHash " +
                                 "coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 16";
                env.CompileDeploy(eplContext, path);

                var fields = "c0,c1,c2".SplitCsv();
                var eplGrouped = "@name('s0') context CtxHash " +
                                 "select context.id as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString";
                env.CompileDeploy(eplGrouped, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 0, "E1", 10 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                env.AssertPropsNew("s0", fields, new object[] { 1, "E2", 11 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 12));
                env.AssertPropsNew("s0", fields, new object[] { 1, "E2", 23 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E1", 14));
                env.AssertPropsNew("s0", fields, new object[] { 0, "E1", 24 });

                env.UndeployAll();

                env.Milestone(3);
            }
        }

        private class ContextHashSegmentedManyArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryHash(env, milestone, "consistent_hash_crc32(TheString, IntPrimitive)");
                TryHash(env, milestone, "hash_code(TheString, IntPrimitive)");
            }

            private static void TryHash(
                RegressionEnvironment env,
                AtomicLong milestone,
                string hashFunc)
            {
                var path = new RegressionPath();
                var eplCtxCRC32 = "@name('context') @public create context Ctx1 as coalesce " +
                                  hashFunc +
                                  " from SupportBean " +
                                  "granularity 1000000";
                env.CompileDeploy(eplCtxCRC32, path);

                var fields = "c1,c2,c3,c4,c5".SplitCsv();
                var eplStmt = "@name('s0') context Ctx1 select IntPrimitive as c1, " +
                              "sum(LongPrimitive) as c2, prev(1, LongPrimitive) as c3, prior(1, LongPrimitive) as c4," +
                              "(select P00 from SupportBean_S0#length(2)) as c5 " +
                              "from SupportBean#length(3)";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                env.SendEventBean(MakeBean("E1", 100, 20L));
                env.AssertPropsNew("s0", fields, new object[] { 100, 20L, null, null, null });

                env.SendEventBean(MakeBean("E1", 100, 21L));
                env.AssertPropsNew("s0", fields, new object[] { 100, 41L, 20L, 20L, null });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1000, "S0"));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean("E1", 100, 22L));
                env.AssertPropsNew("s0", fields, new object[] { 100, 63L, 21L, 21L, "S0" });

                env.UndeployAll();
            }
        }

        private class ContextHashSegmentedMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var eplCtx = "@name('context') @public create context " +
                             ctx +
                             " as " +
                             "coalesce " +
                             " consistent_hash_crc32(TheString) from SupportBean, " +
                             " consistent_hash_crc32(P00) from SupportBean_S0 " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);
                // comment-me-in: SupportHashCodeFuncGranularCRC32 codeFunc = new SupportHashCodeFuncGranularCRC32(4);

                var eplStmt = "@name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, IntPrimitive as c1, Id as c2 from SupportBean#keepall as t1, SupportBean_S0#keepall as t2 where t1.TheString = t2.P00";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                var fields = "c0,c1,c2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(1, "E2"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 11));
                env.SendEventBean(new SupportBean_S0(2, "E4"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3, "E1"));
                env.AssertPropsNew("s0", fields, new object[] { ctx, 10, 3 });
                AssertIterator(env, "s0", fields, new object[][] { new object[] { ctx, 10, 3 } });

                env.SendEventBean(new SupportBean_S0(4, "E4"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(5, "E5"));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E2", 12));
                env.AssertPropsNew("s0", fields, new object[] { ctx, 12, 1 });
                AssertIterator(
                    env,
                    "s0",
                    fields,
                    new object[][] { new object[] { ctx, 10, 3 }, new object[] { ctx, 12, 1 } });

                env.UndeployAll();
            }
        }

        private class ContextHashSegmentedBasic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Comment-in to see CRC32 code.
                for (var i = 0; i < 10; i++) {
                    var key = "E" + i;
                    var code = SupportHashCodeFuncGranularCRC32.ComputeCRC32(key) % 4;
                    var hashCode = i.GetHashCode() % 4;
                    //Console.WriteLine(key + " code " + code + " hashCode " + hashCode);
                }

                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var milestone = new AtomicLong();

                // test CRC32 Hash
                var eplCtx = "@name('context') @public create context " +
                             ctx +
                             " as " +
                             "coalesce consistent_hash_crc32(TheString) from SupportBean " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt = "@name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString";
                env.CompileDeploy(eplStmt, path).AddListener("s0");
                env.AssertThat(
                    () => {
                        ClassicAssert.AreEqual(4, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
                        AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 4, null, null, null);
                    });

                TryAssertionHash(env, milestone, "s0", ctx); // equivalent to: SupportHashCodeFuncGranularCRC32(4)

                env.AssertThat(() => ClassicAssert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                path.Clear();

                // test same with SODA
                env.EplToModelCompileDeploy(eplCtx, path);
                env.CompileDeploy(eplStmt, path).AddListener("s0");
                TryAssertionHash(env, milestone, "s0", ctx);
                path.Clear();

                // test with Java-hashCode String hash
                env.CompileDeploy(
                    "@name('context') @public create context " +
                    ctx +
                    " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 6 " +
                    "preallocate",
                    path);

                env.CompileDeploy(
                    "@name('s0') context " +
                    ctx +
                    " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.AddListener("s0");
                env.AssertThat(
                    () => {
                        ClassicAssert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
                        AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 6, null, null, null);
                    });

                TryAssertionHash(env, milestone, "s0", ctx);
                env.AssertThat(() => ClassicAssert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));
                path.Clear();

                // test no pre-allocate
                env.CompileDeploy(
                    "@name('context') @public create context " +
                    ctx +
                    " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 16",
                    path);

                env.CompileDeploy(
                    "@name('s0') context " +
                    ctx +
                    " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.AddListener("s0");
                env.AssertThat(
                    () => {
                        ClassicAssert.AreEqual(1, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
                        AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 0, null, null, null);
                    });

                TryAssertionHash(env, milestone, "s0", ctx);
                env.AssertThat(() => ClassicAssert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.UndeployAll();
            }

            private static void TryAssertionHash(
                RegressionEnvironment env,
                AtomicLong milestone,
                string stmtNameIterate,
                string stmtNameContext)
            {
                var fields = "c0,c1,c2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 5));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E1", 5 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] { new object[] { stmtNameContext, "E1", 5 } });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 6));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E2", 6 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][]
                        { new object[] { stmtNameContext, "E1", 5 }, new object[] { stmtNameContext, "E2", 6 } });

                env.SendEventBean(new SupportBean("E3", 7));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E3", 7 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] {
                        new object[] { stmtNameContext, "E1", 5 }, new object[] { stmtNameContext, "E3", 7 },
                        new object[] { stmtNameContext, "E2", 6 }
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 8));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E4", 8 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] {
                        new object[] { stmtNameContext, "E1", 5 }, new object[] { stmtNameContext, "E3", 7 },
                        new object[] { stmtNameContext, "E4", 8 }, new object[] { stmtNameContext, "E2", 6 }
                    });

                env.SendEventBean(new SupportBean("E5", 9));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E5", 9 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] {
                        new object[] { stmtNameContext, "E5", 9 }, new object[] { stmtNameContext, "E1", 5 },
                        new object[] { stmtNameContext, "E3", 7 }, new object[] { stmtNameContext, "E4", 8 },
                        new object[] { stmtNameContext, "E2", 6 }
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E1", 15 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] {
                        new object[] { stmtNameContext, "E5", 9 }, new object[] { stmtNameContext, "E1", 15 },
                        new object[] { stmtNameContext, "E3", 7 }, new object[] { stmtNameContext, "E4", 8 },
                        new object[] { stmtNameContext, "E2", 6 }
                    });

                env.SendEventBean(new SupportBean("E4", 11));
                env.AssertPropsNew("s0", fields, new object[] { stmtNameContext, "E4", 19 });
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new object[][] {
                        new object[] { stmtNameContext, "E5", 9 }, new object[] { stmtNameContext, "E1", 15 },
                        new object[] { stmtNameContext, "E3", 7 }, new object[] { stmtNameContext, "E4", 19 },
                        new object[] { stmtNameContext, "E2", 6 }
                    });

                env.AssertThat(() => ClassicAssert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env)));

                env.UndeployModuleContaining("s0");
                env.AssertThat(() => ClassicAssert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env)));

                env.UndeployAll();
                env.AssertThat(() => ClassicAssert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env)));
            }
        }

        private class ContextHashSegmentedBySingleRowFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCtx = "@name('context') @public create context HashSegmentedContext as " +
                             "coalesce myHash(*) from SupportBean " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt =
                    "@name('s0') context HashSegmentedContext select context.id as c1, myHash(*) as c2, mySecond(*, TheString) as c3, " +
                    nameof(ContextHashSegmented) +
                    ".MySecondFunc(*, TheString) as c4 from SupportBean";
                env.CompileDeploy(eplStmt, path);
                env.AddListener("s0");

                var fields = "c1,c2,c3,c4".SplitCsv();

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 3));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 3, 3, "E1", "E1" }); // context id matches the number returned by myHashFunc

                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsNew("s0", fields, new object[] { 0, 0, "E2", "E2" });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 7));
                env.AssertPropsNew("s0", fields, new object[] { 3, 7, "E3", "E3" });

                env.UndeployAll();
            }
        }

        public static int MyHashFunc(SupportBean sb)
        {
            return sb.IntPrimitive;
        }

        public static string MySecondFunc(
            SupportBean sb,
            string text)
        {
            return text;
        }

        private static void MakeSendScoreEvent(
            RegressionEnvironment env,
            string typeName,
            EventRepresentationChoice eventRepresentationEnum,
            string userId,
            string keyword,
            string productId,
            long score)
        {
            if (eventRepresentationEnum.IsMapEvent()) {
                IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("UserId", userId);
                theEvent.Put("Keyword", keyword);
                theEvent.Put("ProductId", productId);
                theEvent.Put("Score", score);
                env.SendEventMap(theEvent, typeName);
            }
            else if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] { userId, keyword, productId, score }, typeName);
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(env.RuntimeAvroSchemaPreconfigured(typeName).AsRecordSchema());
                record.Put("UserId", userId);
                record.Put("Keyword", keyword);
                record.Put("ProductId", productId);
                record.Put("Score", score);
                env.SendEventAvro(record, typeName);
            }
            else if (eventRepresentationEnum.IsJsonEvent() || eventRepresentationEnum.IsJsonProvidedClassEvent()) {
                var @object = new JObject();
                @object.Add("UserId", userId);
                @object.Add("Keyword", keyword);
                @object.Add("ProductId", productId);
                @object.Add("Score", score);
                env.SendEventJson(@object.ToString(), typeName);
            }
            else {
                Assert.Fail();
            }
        }

        private static void AssertIterator(
            RegressionEnvironment env,
            string statementName,
            string[] fields,
            object[][] expected)
        {
            env.AssertIterator(
                statementName,
                iterator => {
                    var rows = EPAssertionUtil.EnumeratorToArray(iterator);
                    AssertIterator(rows, fields, expected);
                });

            env.AssertSafeEnumerator(
                statementName,
                iterator => {
                    var rows = EPAssertionUtil.EnumeratorToArray(env.Statement(statementName).GetSafeEnumerator());
                    AssertIterator(rows, fields, expected);
                });
        }

        private static void AssertIterator(
            EventBean[] events,
            string[] fields,
            object[][] expected)
        {
            var result = EPAssertionUtil.EventsToObjectArr(events, fields);
            EPAssertionUtil.AssertEqualsAnyOrder(expected, result);
        }

        private static SupportBean MakeBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private class MySelectorFilteredHash : ContextPartitionSelectorFiltered
        {
            private ISet<int> match;
            private IList<int> contexts = new List<int>();
            private LinkedHashSet<int> cpids = new LinkedHashSet<int>();

            internal MySelectorFilteredHash(ISet<int> match)
            {
                this.match = match;
            }

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierHash)contextPartitionIdentifier;
                if (match == null && cpids.Contains(id.ContextPartitionId)) {
                    throw new EPRuntimeException("Already exists context Id: " + id.ContextPartitionId);
                }

                cpids.Add(id.ContextPartitionId);
                contexts.Add(id.Hash);
                return match.Contains(id.Hash);
            }

            public IList<int> Contexts => contexts;
        }

        public class MyLocalJsonProvided
        {
        }

        public class MyLocalJsonProvidedScoreCycle
        {
            public string UserId;
            public string Keyword;
            public string ProductId;
            public long Score;
        }

        public class MyLocalJsonProvidedUserKeywordTotalStream
        {
            public string UserId;
            public string Keyword;
            public long SumScore;
        }
    }
} // end of namespace