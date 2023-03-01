///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextCategory
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithWContextProps(execs);
            WithBooleanExprFilter(execs);
            WithContextPartitionSelection(execs);
            WithSingleCategorySODAPrior(execs);
            WithInvalid(execs);
            WithDeclaredExpr(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDeclaredExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategoryDeclaredExpr(true));
            execs.Add(new ContextCategoryDeclaredExpr(false));
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategoryInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleCategorySODAPrior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategorySingleCategorySODAPrior());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartitionSelection(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategoryContextPartitionSelection());
            return execs;
        }

        public static IList<RegressionExecution> WithBooleanExprFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategoryBooleanExprFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithWContextProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategoryWContextProps());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategorySceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextCategorySceneOne());
            return execs;
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string ctx,
            AtomicLong milestone)
        {
            var fields = new[] { "c0", "c1", "c2" };
            env.SendEventBean(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] { ctx, "cat1", null });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 20));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] { ctx, "cat1", 5 });

            Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));
            env.UndeployAll();
            Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
        }

        private static void SendAssertBooleanExprFilter(
            RegressionEnvironment env,
            string theString,
            string groupExpected,
            long countExpected)
        {
            var fields = new[] { "c0", "c1" };
            env.SendEventBean(new SupportBean(theString, 1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] { groupExpected, countExpected });
        }

        public class ContextCategorySceneOne : RegressionExecution
        {
            private static readonly string[] FIELDS = new[] { "c0", "c1" };

            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('context') create context CategoryContext\n" +
                          "group TheString = 'A' as cat1,\n" +
                          "group TheString = 'B' as cat2 \n" +
                          "from SupportBean;\n" +
                          "@Name('s0') context CategoryContext select count(*) as c0, context.label as c1 from SupportBean;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var deploymentIdContext = env.DeploymentId("context");
                var statementNames =
                    env.Runtime.ContextPartitionService.GetContextStatementNames(
                        deploymentIdContext,
                        "CategoryContext");
                EPAssertionUtil.AssertEqualsExactOrder(statementNames, new[] { "s0" });
                Assert.AreEqual(
                    1,
                    env.Runtime.ContextPartitionService.GetContextNestingLevel(deploymentIdContext, "CategoryContext"));
                var ids = env.Runtime.ContextPartitionService.GetContextPartitionIds(
                    deploymentIdContext,
                    "CategoryContext",
                    new ContextPartitionSelectorAll());
                EPAssertionUtil.AssertEqualsExactOrder(new[] { 0, 1 }, ids.ToArray());

                SendAssert(env, "A", 1, "cat1", 1L);
                SendAssert(env, "C", 2, null, null);

                env.Milestone(1);

                SendAssert(env, "B", 3, "cat2", 1L);
                SendAssert(env, "A", 4, "cat1", 2L);

                env.Milestone(2);

                SendAssert(env, "A", 6, "cat1", 3L);
                SendAssert(env, "B", 5, "cat2", 2L);
                SendAssert(env, "C", 7, null, null);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                int intPrimitive,
                string categoryName,
                long? expected)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                if (expected == null) {
                    Assert.IsFalse(env.Listener("s0").IsInvoked);
                }
                else {
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        FIELDS,
                        new object[] { expected, categoryName });
                }
            }
        }

        public class ContextCategorySceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "c1", "c2", "c3", "c4", "c5" };
                var epl = "@Name('CTX') create context CtxCategory " +
                          "group by IntPrimitive > 0 as cat1," +
                          "group by IntPrimitive < 0 as cat2 from SupportBean;\n" +
                          "@Name('s0') context CtxCategory " +
                          "select TheString as c1, sum(IntPrimitive) as c2, context.label as c3, context.name as c4, context.id as c5 " +
                          "from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");
                AssertPartitionInfo(env);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "G1", 1, "cat1", "CtxCategory", 0 });
                AssertPartitionInfo(env);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", -2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "G2", -2, "cat2", "CtxCategory", 1 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "G3", 4, "cat1", "CtxCategory", 0 });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G4", -4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "G4", -6, "cat2", "CtxCategory", 1 });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G5", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "G5", 9, "cat1", "CtxCategory", 0 });

                env.UndeployAll();
            }

            private void AssertPartitionInfo(RegressionEnvironment env)
            {
                var partitionAdmin = env.Runtime.ContextPartitionService;
                var depIdCtx = env.DeploymentId("CTX");

                var partitions = partitionAdmin.GetContextPartitions(
                    depIdCtx,
                    "CtxCategory",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(2, partitions.Identifiers.Count);
                var descs = partitions.Identifiers.Values.ToArray();
                var first = (ContextPartitionIdentifierCategory)descs[0];
                var second = (ContextPartitionIdentifierCategory)descs[1];
                EPAssertionUtil.AssertEqualsAnyOrder(
                    new[] { "cat1", "cat2" },
                    new object[] { first.Label, second.Label });

                var desc = partitionAdmin.GetIdentifier(depIdCtx, "CtxCategory", 0);
                Assert.AreEqual("cat1", ((ContextPartitionIdentifierCategory)desc).Label);

                SupportContextPropUtil.AssertContextProps(
                    env,
                    "CTX",
                    "CtxCategory",
                    new[] { 0, 1 },
                    "label",
                    new[] {
                        new object[] { "cat1" },
                        new object[] { "cat2" }
                    });
            }
        }

        internal class ContextCategoryBooleanExprFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCtx =
                    "@Name('ctx') create context Ctx600a group by TheString like 'A%' as agroup, group by TheString like 'B%' as bgroup, group by TheString like 'C%' as cgroup from SupportBean";
                env.CompileDeploy(eplCtx, path);
                var eplSum = "@Name('s0') context Ctx600a select context.label as c0, count(*) as c1 from SupportBean";
                env.CompileDeploy(eplSum, path).AddListener("s0");

                SendAssertBooleanExprFilter(env, "B1", "bgroup", 1);

                env.Milestone(0);

                SendAssertBooleanExprFilter(env, "A1", "agroup", 1);

                env.Milestone(1);

                SendAssertBooleanExprFilter(env, "B171771", "bgroup", 2);

                env.Milestone(2);

                SendAssertBooleanExprFilter(env, "A  x", "agroup", 2);

                env.UndeployAll();
            }
        }

        internal class ContextCategoryContextPartitionSelection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] { "c0", "c1", "c2", "c3" };
                var milestone = new AtomicLong();
                var path = new RegressionPath();

                env.CompileDeploy(
                    "@Name('ctx') create context MyCtx as group by IntPrimitive < -5 as grp1, group by IntPrimitive between -5 and +5 as grp2, group by IntPrimitive > 5 as grp3 from SupportBean",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx select context.id as c0, context.label as c1, TheString as c2, sum(IntPrimitive) as c3 from SupportBean#keepall group by TheString",
                    path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", -5));
                env.SendEventBean(new SupportBean("E1", 2));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", -100));
                env.SendEventBean(new SupportBean("E3", -8));
                env.SendEventBean(new SupportBean("E1", 60));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] { 0, "grp1", "E3", -108 },
                        new object[] { 1, "grp2", "E1", 3 },
                        new object[] { 1, "grp2", "E2", -5 },
                        new object[] { 2, "grp3", "E1", 60 }
                    });
                SupportContextPropUtil.AssertContextProps(
                    env,
                    "ctx",
                    "MyCtx",
                    new[] { 0, 1, 2 },
                    "label",
                    new[] {
                        new object[] { "grp1" },
                        new object[] { "grp2" },
                        new object[] { "grp3" }
                    });

                env.MilestoneInc(milestone);

                // test iterator targeted by context partition id
                var selectorById = new SupportSelectorById(Collections.SingletonList(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorById),
                    env.Statement("s0").GetSafeEnumerator(selectorById),
                    fields,
                    new[] {
                        new object[] { 1, "grp2", "E1", 3 },
                        new object[] { 1, "grp2", "E2", -5 }
                    });

                // test iterator targeted for a given category
                var selector = new SupportSelectorCategory(
                    new HashSet<string>(Arrays.AsList("grp1", "grp3")));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selector),
                    env.Statement("s0").GetSafeEnumerator(selector),
                    fields,
                    new[] {
                        new object[] { 0, "grp1", "E3", -108 },
                        new object[] { 2, "grp3", "E1", 60 }
                    });

                // test iterator targeted for a given filtered category
                var filtered = new MySelectorFilteredCategory("grp1");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(filtered),
                    env.Statement("s0").GetSafeEnumerator(filtered),
                    fields,
                    new[] {
                        new object[] { 0, "grp1", "E3", -108 }
                    });
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(
                            new SupportSelectorCategory(
                                (ISet<string>)null))
                        .MoveNext());
                Assert.IsFalse(
                    env.Statement("s0")
                        .GetEnumerator(
                            new SupportSelectorCategory(
                                new EmptySet<string>()))
                        .MoveNext());

                env.MilestoneInc(milestone);

                // test always-false filter - compare context partition info
                filtered = new MySelectorFilteredCategory(null);
                Assert.IsFalse(env.Statement("s0").GetEnumerator(filtered).MoveNext());
                EPAssertionUtil.AssertEqualsAnyOrder(new object[] { "grp1", "grp2", "grp3" }, filtered.Categories);

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
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorCategory] interfaces but received com."),
                        "message: " + ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextCategoryInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid filter spec
                epl = "create context ACtx group TheString is not null as cat1 from SupportBean(dummy = 1)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // not a boolean expression
                epl = "create context ACtx group IntPrimitive as grp1 from SupportBean";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Filter expression not returning a boolean value: 'IntPrimitive' [");

                // validate statement not applicable filters
                var path = new RegressionPath();
                env.CompileDeploy("create context ACtx group IntPrimitive < 10 as cat1 from SupportBean", path);
                epl = "context ACtx select * from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Category context 'ACtx' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement [");

                env.UndeployAll();
            }
        }

        internal class ContextCategoryWContextProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var ctx = "CategorizedContext";
                var fields = new[] { "c0", "c1", "c2" };

                var epl = "@Name('context') create context " +
                          ctx +
                          " " +
                          "group IntPrimitive < 10 as cat1, " +
                          "group IntPrimitive between 10 and 20 as cat2, " +
                          "group IntPrimitive > 20 as cat3 " +
                          "from SupportBean;\n";
                epl += "@Name('s0') context CategorizedContext " +
                       "select context.name as c0, context.label as c1, sum(IntPrimitive) as c2 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(3, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 3, null, null, null);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat1", 5 });
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] { ctx, "cat1", 5 },
                        new object[] { ctx, "cat2", null },
                        new object[] { ctx, "cat3", null }
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat1", 9 });

                env.SendEventBean(new SupportBean("E3", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat2", 11 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 25));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat3", 25 });

                env.SendEventBean(new SupportBean("E5", 25));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat3", 50 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { ctx, "cat1", 12 });

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    new[] {
                        new object[] { ctx, "cat1", 12 },
                        new object[] { ctx, "cat2", 11 },
                        new object[] { ctx, "cat3", 50 }
                    });

                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(3, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));

                env.UndeployModuleContaining("s0");

                Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
            }
        }

        internal class ContextCategorySingleCategorySODAPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var milestone = new AtomicLong();
                var ctx = "CategorizedContext";
                var eplCtx = "@Name('context') create context " +
                             ctx +
                             " as " +
                             "group IntPrimitive<10 as cat1 " +
                             "from SupportBean";
                env.CompileDeploy(eplCtx, path);

                var eplStmt =
                    "@Name('s0') context CategorizedContext select context.name as c0, context.label as c1, prior(1,IntPrimitive) as c2 from SupportBean";
                env.CompileDeploy(eplStmt, path).AddListener("s0");

                RunAssertion(env, ctx, milestone);

                // test SODA
                path.Clear();
                env.EplToModelCompileDeploy(eplCtx, path);
                env.EplToModelCompileDeploy(eplStmt, path);
                env.AddListener("s0");

                RunAssertion(env, ctx, milestone);
            }
        }

        public class ContextCategoryDeclaredExpr : RegressionExecution
        {
            private readonly bool isAlias;

            public ContextCategoryDeclaredExpr(bool isAlias)
            {
                this.isAlias = isAlias;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean",
                    path);
                env.CompileDeploy("@Name('expr-1') create expression getLabelOne { context.label }", path);
                env.CompileDeploy("@Name('expr-2') create expression getLabelTwo { 'x'||context.label||'x' }", path);

                env.Milestone(0);

                if (!isAlias) {
                    env.CompileDeploy(
                        "@Name('s0') expression getLabelThree { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean",
                        path);
                }
                else {
                    env.CompileDeploy(
                        "@Name('s0') expression getLabelThree alias for { context.label } " +
                        "context MyCtx " +
                        "select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean",
                        path);
                }

                env.AddListener("s0");

                env.Milestone(1);

                var fields = new[] { "c0", "c1", "c2" };
                env.SendEventBean(new SupportBean("E1", -2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "n", "xnx", "n" });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "p", "xpx", "p" });

                env.UndeployAll();
            }
        }

        internal class MySelectorFilteredCategory : ContextPartitionSelectorFiltered
        {
            private readonly IList<object> categories = new List<object>();
            private readonly LinkedHashSet<int> cpids = new LinkedHashSet<int>();
            private readonly string matchCategory;

            internal MySelectorFilteredCategory(string matchCategory)
            {
                this.matchCategory = matchCategory;
            }

            internal object[] Categories => categories.ToArray();

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierCategory)contextPartitionIdentifier;
                if (matchCategory == null && cpids.Contains(id.ContextPartitionId)) {
                    throw new EPException("Already exists context Id: " + id.ContextPartitionId);
                }

                cpids.Add(id.ContextPartitionId);
                categories.Add(id.Label);
                return matchCategory != null && matchCategory.Equals(id.Label);
            }
        }
    }
} // end of namespace