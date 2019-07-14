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
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextHashSegmented
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextHashSegmentedBasic());
            execs.Add(new ContextHashSegmentedFilter());
            execs.Add(new ContextHashNoPreallocate());
            execs.Add(new ContextHashSegmentedManyArg());
            execs.Add(new ContextHashSegmentedMulti());
            execs.Add(new ContextHashSegmentedBySingleRowFunc());
            execs.Add(new ContextHashScoringUseCase());
            execs.Add(new ContextHashPartitionSelection());
            execs.Add(new ContextHashInvalid());
            return execs;
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
                theEvent.Put("userId", userId);
                theEvent.Put("keyword", keyword);
                theEvent.Put("productId", productId);
                theEvent.Put("score", score);
                env.SendEventMap(theEvent, typeName);
            }
            else if (eventRepresentationEnum.IsObjectArrayEvent()) {
                env.SendEventObjectArray(new object[] {userId, keyword, productId, score}, typeName);
            }
            else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(
                    SupportAvroUtil.GetAvroSchema(env.Runtime.EventTypeService.GetEventTypePreconfigured(typeName))
                        .AsRecordSchema());
                record.Put("userId", userId);
                record.Put("keyword", keyword);
                record.Put("productId", productId);
                record.Put("score", score);
                env.SendEventAvro(record, typeName);
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
            var rows = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator(statementName));
            AssertIterator(rows, fields, expected);

            rows = EPAssertionUtil.EnumeratorToArray(env.Statement(statementName).GetSafeEnumerator());
            AssertIterator(rows, fields, expected);
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

        internal class ContextHashScoringUseCase : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    TryAssertionScoringUseCase(env, rep, milestone);
                }
            }

            private static void TryAssertionScoringUseCase(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                AtomicLong milestone)
            {
                var fields = "userId,keyword,sumScore".SplitCsv();
                var epl =
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema ScoreCycle (userId string, keyword string, productId string, score long);\n" +
                    eventRepresentationEnum.GetAnnotationText() +
                    " create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                    "\n" +
                    eventRepresentationEnum.GetAnnotationText() +
                    " create context HashByUserCtx as " +
                    "coalesce by consistent_hash_crc32(userId) from ScoreCycle, " +
                    "consistent_hash_crc32(userId) from UserKeywordTotalStream " +
                    "granularity 1000000;\n" +
                    "\n" +
                    "context HashByUserCtx create window ScoreCycleWindow#unique(productId, keyword) as ScoreCycle;\n" +
                    "\n" +
                    "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                    "\n" +
                    "@Name('s0') context HashByUserCtx insert into UserKeywordTotalStream \n" +
                    "select userId, keyword, sum(score) as sumScore from ScoreCycleWindow group by keyword;\n" +
                    "\n" +
                    "@Name('outTwo') context HashByUserCtx on UserKeywordTotalStream(sumScore > 10000) delete from ScoreCycleWindow;\n";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());
                env.AddListener("s0");

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P1", 100);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"Pete", "K1", 100L});

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P2", 15);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"Pete", "K1", 115L});

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P2", 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"Joe", "K1", 30L});

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K2", "P1", 40);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"Joe", "K2", 40L});

                MakeSendScoreEvent(env, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P1", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"Joe", "K1", 50L});

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
                    "@Name('ctx') create context MyCtx as coalesce consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx select context.id as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] {5, "E1", 1}
                    });

                env.SendEventBean(new SupportBean("E2", 10));
                env.SendEventBean(new SupportBean("E1", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 100));
                env.SendEventBean(new SupportBean("E3", 101));

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] {5, "E1", 6},
                        new object[] {15, "E2", 10},
                        new object[] {9, "E3", 201}
                    });
                SupportContextPropUtil.AssertContextProps(env, "ctx", "MyCtx", new[] {5, 15, 9}, null, null);

                env.Milestone(2);

                // test iterator targeted hash
                var selector = new SupportSelectorByHashCode(Collections.SingletonSet(15));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    new[] {
                        new object[] {15, "E2", 10}
                    });
                selector = new SupportSelectorByHashCode(new HashSet<int>(Arrays.AsList(1, 9, 5)));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    new[] {
                        new object[] {5, "E1", 6},
                        new object[] {9, "E3", 201}
                    });
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(
                            new SupportSelectorByHashCode(Collections.SingletonSet(99)))
                        .MoveNext());
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(
                            new SupportSelectorByHashCode(Collections.GetEmptySet<int>()))
                        .MoveNext());
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(
                            new SupportSelectorByHashCode(null))
                        .MoveNext());

                // test iterator filtered
                var filtered = new MySelectorFilteredHash(
                    Collections.SingletonSet(15));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(filtered),
                    env.Statement("s0").GetSafeEnumerator(filtered),
                    fields,
                    new[] {
                        new object[] {15, "E2", 10}
                    });
                filtered = new MySelectorFilteredHash(new HashSet<int>(Arrays.AsList(1, 9, 5)));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(filtered),
                    env.Statement("s0").GetSafeEnumerator(filtered),
                    fields,
                    new[] {
                        new object[] {5, "E1", 6},
                        new object[] {9, "E3", 201}
                    });

                // test always-false filter - compare context partition info
                filtered = new MySelectorFilteredHash(Collections.GetEmptySet<int>());
                Assert.IsFalse(env.Statement("s0").GetEnumerator(filtered).MoveNext());
                Assert.AreEqual(16, filtered.Contexts.Count);

                try {
                    env.Statement("s0")
                        .GetEnumerator(
                            new ProxyContextPartitionSelectorSegmented {
                                ProcPartitionKeys = () => null
                            });
                    Assert.Fail();
                }
                catch (InvalidContextPartitionSelector ex) {
                    Assert.IsTrue(
                        ex.Message.StartsWith(
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorHash] interfaces but received com."),
                        "message: " + ex.Message);
                }

                env.UndeployAll();

                env.Milestone(3);
            }
        }

        internal class ContextHashInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid filter spec
                epl = "create context ACtx coalesce hash_code(IntPrimitive) from SupportBean(dummy = 1) granularity 10";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // invalid hash code function
                epl = "create context ACtx coalesce hash_code_xyz(IntPrimitive) from SupportBean granularity 10";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'ACtx' expected a hash function that is any of {consistent_hash_crc32, hash_code} or a plug-in single-row function or script but received 'hash_code_xyz' [");

                // invalid no-param hash code function
                epl = "create context ACtx coalesce hash_code() from SupportBean granularity 10";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "For context 'ACtx' expected one or more parameters to the hash function, but found no parameter list [");

                // validate statement not applicable filters
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context ACtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10",
                    path);
                epl = "context ACtx select * from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Segmented context 'ACtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

                // invalid attempt to partition a named window's streams
                env.CompileDeploy("create window MyWindow#keepall as SupportBean", path);
                epl = "create context SegmentedByWhat partition by theString from MyWindow";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Partition criteria may not include named windows [create context SegmentedByWhat partition by theString from MyWindow]");

                env.UndeployAll();
            }
        }

        internal class ContextHashSegmentedFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var fields = "c0,c1".SplitCsv();

                var eplCtx = "@Name('context') create context " +
                             ctx +
                             " as " +
                             "coalesce " +
                             " consistent_hash_crc32(TheString) from SupportBean(intPrimitive > 10) " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt = "@Name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, IntPrimitive as c1 from SupportBean#lastevent";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {ctx, 12});
                AssertIterator(
                    env,
                    "s0",
                    fields,
                    new[] {
                        new object[] {ctx, 12}
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 10));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 1));
                AssertIterator(
                    env,
                    "s0",
                    fields,
                    new[] {
                        new object[] {ctx, 12}
                    });
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E6", 15));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {ctx, 15});

                env.UndeployAll();
            }
        }

        public class ContextHashNoPreallocate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext = "@Name('CTX') create context CtxHash " +
                                 "coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 16";
                env.CompileDeploy(eplContext, path);

                var fields = "c0,c1,c2".SplitCsv();
                var eplGrouped = "@Name('s0') context CtxHash " +
                                 "select context.id as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString";
                env.CompileDeploy(eplGrouped, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, "E1", 10});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "E2", 11});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "E2", 23});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E1", 14));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, "E1", 24});

                env.UndeployAll();

                env.Milestone(3);
            }
        }

        internal class ContextHashSegmentedManyArg : RegressionExecution
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
                var eplCtxCRC32 = "@Name('context') create context Ctx1 as coalesce " +
                                  hashFunc +
                                  " from SupportBean " +
                                  "granularity 1000000";
                env.CompileDeploy(eplCtxCRC32, path);

                var fields = "c1,c2,c3,c4,c5".SplitCsv();
                var eplStmt = "@Name('s0') context Ctx1 select IntPrimitive as c1, " +
                              "sum(LongPrimitive) as c2, prev(1, longPrimitive) as c3, prior(1, longPrimitive) as c4," +
                              "(select p00 from SupportBean_S0#length(2)) as c5 " +
                              "from SupportBean#length(3)";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                env.SendEventBean(MakeBean("E1", 100, 20L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100, 20L, null, null, null});

                env.SendEventBean(MakeBean("E1", 100, 21L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100, 41L, 20L, 20L, null});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(1000, "S0"));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeBean("E1", 100, 22L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100, 63L, 21L, 21L, "S0"});

                env.UndeployAll();
            }
        }

        internal class ContextHashSegmentedMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var eplCtx = "@Name('context') create context " +
                             ctx +
                             " as " +
                             "coalesce " +
                             " consistent_hash_crc32(TheString) from SupportBean, " +
                             " consistent_hash_crc32(p00) from SupportBean_S0 " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);
                // comment-me-in: SupportHashCodeFuncGranularCRC32 codeFunc = new SupportHashCodeFuncGranularCRC32(4);

                var eplStmt = "@Name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, IntPrimitive as c1, id as c2 from SupportBean#keepall as t1, SupportBean_S0#keepall as t2 where t1.TheString = t2.p00";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                var fields = "c0,c1,c2".SplitCsv();

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean_S0(1, "E2"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 11));
                env.SendEventBean(new SupportBean_S0(2, "E4"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {ctx, 10, 3});
                AssertIterator(
                    env,
                    "s0",
                    fields,
                    new[] {
                        new object[] {ctx, 10, 3}
                    });

                env.SendEventBean(new SupportBean_S0(4, "E4"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(5, "E5"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E2", 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {ctx, 12, 1});
                AssertIterator(
                    env,
                    "s0",
                    fields,
                    new[] {
                        new object[] {ctx, 10, 3},
                        new object[] {ctx, 12, 1}
                    });

                env.UndeployAll();
            }
        }

        internal class ContextHashSegmentedBasic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Comment-in to see CRC32 code.
                for (var i = 0; i < 10; i++) {
                    var key = "E" + i;
                    var code = SupportHashCodeFuncGranularCRC32.ComputeCRC32(key) % 4;
                    var hashCode = i.GetHashCode() % 4;
                    //System.out.println(key + " code " + code + " hashCode " + hashCode);
                }

                var path = new RegressionPath();
                var ctx = "HashSegmentedContext";
                var milestone = new AtomicLong();

                // test CRC32 Hash
                var eplCtx = "@Name('context') create context " +
                             ctx +
                             " as " +
                             "coalesce consistent_hash_crc32(TheString) from SupportBean " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt = "@Name('s0') context " +
                              ctx +
                              " " +
                              "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString";
                env.CompileDeploy(eplStmt, path).AddListener("s0");
                Assert.AreEqual(4, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 4, null, null, null);

                TryAssertionHash(env, milestone, "s0", ctx); // equivalent to: SupportHashCodeFuncGranularCRC32(4)
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                path.Clear();

                // test same with SODA
                env.EplToModelCompileDeploy(eplCtx, path);
                env.CompileDeploy(eplStmt, path).AddListener("s0");
                TryAssertionHash(env, milestone, "s0", ctx);
                path.Clear();

                // test with Java-hashCode String hash
                env.CompileDeploy(
                    "@Name('context') create context " +
                    ctx +
                    " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 6 " +
                    "preallocate",
                    path);

                env.CompileDeploy(
                    "@Name('s0') context " +
                    ctx +
                    " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.AddListener("s0");
                Assert.AreEqual(6, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 6, null, null, null);

                TryAssertionHash(env, milestone, "s0", ctx);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                path.Clear();

                // test no pre-allocate
                env.CompileDeploy(
                    "@Name('context') create context " +
                    ctx +
                    " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 16",
                    path);

                env.CompileDeploy(
                    "@Name('s0') context " +
                    ctx +
                    " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString",
                    path);
                env.AddListener("s0");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 0, null, null, null);

                TryAssertionHash(env, milestone, "s0", ctx);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

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
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E1", 5});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E1", 5}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E2", 6});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E1", 5},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                env.SendEventBean(new SupportBean("E3", 7));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E3", 7});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E1", 5},
                        new object[] {stmtNameContext, "E3", 7},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 8));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E4", 8});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E1", 5},
                        new object[] {stmtNameContext, "E3", 7},
                        new object[] {stmtNameContext, "E4", 8},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                env.SendEventBean(new SupportBean("E5", 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E5", 9});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E5", 9},
                        new object[] {stmtNameContext, "E1", 5},
                        new object[] {stmtNameContext, "E3", 7},
                        new object[] {stmtNameContext, "E4", 8},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E1", 15});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E5", 9},
                        new object[] {stmtNameContext, "E1", 15},
                        new object[] {stmtNameContext, "E3", 7},
                        new object[] {stmtNameContext, "E4", 8},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                env.SendEventBean(new SupportBean("E4", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {stmtNameContext, "E4", 19});
                AssertIterator(
                    env,
                    stmtNameIterate,
                    fields,
                    new[] {
                        new object[] {stmtNameContext, "E5", 9},
                        new object[] {stmtNameContext, "E1", 15},
                        new object[] {stmtNameContext, "E3", 7},
                        new object[] {stmtNameContext, "E4", 19},
                        new object[] {stmtNameContext, "E2", 6}
                    });

                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));

                env.UndeployAll();
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
            }
        }

        internal class ContextHashSegmentedBySingleRowFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCtx = "@Name('context') create context HashSegmentedContext as " +
                             "coalesce myHash(*) from SupportBean " +
                             "granularity 4 " +
                             "preallocate";
                env.CompileDeploy(eplCtx, path);

                var eplStmt =
                    "@Name('s0') context HashSegmentedContext select context.id as c1, myHash(*) as c2, mySecond(*, theString) as c3, " +
                    typeof(ContextHashSegmented).Name +
                    ".mySecondFunc(*, theString) as c4 from SupportBean";
                env.CompileDeploy(eplStmt, path);
                env.AddListener("s0");

                var fields = "c1,c2,c3,c4".SplitCsv();

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 3, "E1", "E1"}); // context id matches the number returned by myHashFunc

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, 0, "E2", "E2"});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 7));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 7, "E3", "E3"});

                env.UndeployAll();
            }
        }

        internal class MySelectorFilteredHash : ContextPartitionSelectorFiltered
        {
            private readonly LinkedHashSet<int> cpids = new LinkedHashSet<int>();

            private readonly ISet<int> match;

            internal MySelectorFilteredHash(ISet<int> match)
            {
                this.match = match;
            }

            public IList<int> Contexts { get; } = new List<int>();

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierHash) contextPartitionIdentifier;
                if (match == null && cpids.Contains(id.ContextPartitionId)) {
                    throw new EPException("Already exists context id: " + id.ContextPartitionId);
                }

                cpids.Add(id.ContextPartitionId);
                Contexts.Add(id.Hash);
                return match.Contains(id.Hash);
            }
        }
    }
} // end of namespace