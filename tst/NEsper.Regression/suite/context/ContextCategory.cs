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
using com.espertech.esper.common.client.util;
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
		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new ContextCategorySceneOne());
			execs.Add(new ContextCategorySceneTwo());
			execs.Add(new ContextCategoryWContextProps());
			execs.Add(new ContextCategoryBooleanExprFilter());
			execs.Add(new ContextCategoryContextPartitionSelection());
			execs.Add(new ContextCategorySingleCategorySODAPrior());
			execs.Add(new ContextCategoryInvalid());
			execs.Add(new ContextCategoryDeclaredExpr(true));
			execs.Add(new ContextCategoryDeclaredExpr(false));
			return execs;
		}

		public class ContextCategorySceneOne : RegressionExecution
		{
			private static readonly string[] FIELDS = "c0,c1".SplitCsv();

			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('context') create context CategoryContext\n" +
				          "group theString = 'A' as cat1,\n" +
				          "group theString = 'B' as cat2 \n" +
				          "from SupportBean;\n" +
				          "@name('s0') context CategoryContext select count(*) as c0, context.label as c1 from SupportBean;\n";
				env.CompileDeployAddListenerMileZero(epl, "s0");

				env.AssertThat(
					() => {
						var deploymentIdContext = env.DeploymentId("context");
						var statementNames = env.Runtime.ContextPartitionService.GetContextStatementNames(
							deploymentIdContext,
							"CategoryContext");
						EPAssertionUtil.AssertEqualsExactOrder(statementNames, "s0".SplitCsv());
						Assert.AreEqual(
							1,
							env.Runtime.ContextPartitionService.GetContextNestingLevel(
								deploymentIdContext,
								"CategoryContext"));
						var ids = env.Runtime.ContextPartitionService.GetContextPartitionIds(
							deploymentIdContext,
							"CategoryContext",
							new ContextPartitionSelectorAll());
						Assert.AreEqual(
							2,
							env.Runtime.ContextPartitionService.GetContextPartitionCount(
								deploymentIdContext,
								"CategoryContext"));
						EPAssertionUtil.AssertEqualsAnyOrder(new int[] { 0, 1 }, ids.ToArray());
					});

				env.AssertStatement(
					"context",
					statement => {
						Assert.IsNull(statement.GetProperty(StatementProperty.CONTEXTNAME));
						Assert.IsNull(statement.GetProperty(StatementProperty.CONTEXTDEPLOYMENTID));
					});
				env.AssertStatement(
					"s0",
					statement => {
						Assert.AreEqual("CategoryContext", statement.GetProperty(StatementProperty.CONTEXTNAME));
						Assert.AreEqual(
							env.DeploymentId("s0"),
							statement.GetProperty(StatementProperty.CONTEXTDEPLOYMENTID));
					});

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
					env.AssertListenerNotInvoked("s0");
				}
				else {
					env.AssertPropsNew("s0", FIELDS, new object[] { expected, categoryName });
				}
			}
		}

		public class ContextCategorySceneTwo : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c1,c2,c3,c4,c5".SplitCsv();
				var epl = "@name('CTX') create context CtxCategory " +
				          "group by intPrimitive > 0 as cat1," +
				          "group by intPrimitive < 0 as cat2 from SupportBean;\n" +
				          "@name('s0') context CtxCategory select theString as c1, sum(intPrimitive) as c2, context.label as c3, context.name as c4, context.id as c5 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");
				AssertPartitionInfo(env);

				env.Milestone(0);

				env.SendEventBean(new SupportBean("G1", 1));
				env.AssertPropsNew("s0", fields, new object[] { "G1", 1, "cat1", "CtxCategory", 0 });
				AssertPartitionInfo(env);

				env.Milestone(1);

				env.SendEventBean(new SupportBean("G2", -2));
				env.AssertPropsNew("s0", fields, new object[] { "G2", -2, "cat2", "CtxCategory", 1 });

				env.Milestone(2);

				env.SendEventBean(new SupportBean("G3", 3));
				env.AssertPropsNew("s0", fields, new object[] { "G3", 4, "cat1", "CtxCategory", 0 });

				env.Milestone(3);

				env.SendEventBean(new SupportBean("G4", -4));
				env.AssertPropsNew("s0", fields, new object[] { "G4", -6, "cat2", "CtxCategory", 1 });

				env.Milestone(4);

				env.SendEventBean(new SupportBean("G5", 5));
				env.AssertPropsNew("s0", fields, new object[] { "G5", 9, "cat1", "CtxCategory", 0 });

				env.UndeployAll();
			}

			private void AssertPartitionInfo(RegressionEnvironment env)
			{
				env.AssertThat(
					() => {
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
							"cat1,cat2".SplitCsv(),
							new object[] { first.Label, second.Label });

						var desc = partitionAdmin.GetIdentifier(depIdCtx, "CtxCategory", 0);
						Assert.AreEqual("cat1", ((ContextPartitionIdentifierCategory)desc).Label);

						SupportContextPropUtil.AssertContextProps(
							env,
							"CTX",
							"CtxCategory",
							new int[] { 0, 1 },
							"label",
							new object[][] { new object[] { "cat1" }, new object[] { "cat2" } });
					});
			}
		}

		public class ContextCategoryBooleanExprFilter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var eplCtx =
					"@name('ctx') @public create context Ctx600a group by theString like 'A%' as agroup, group by theString like 'B%' as bgroup, group by theString like 'C%' as cgroup from SupportBean";
				env.CompileDeploy(eplCtx, path);
				var eplSum = "@name('s0') context Ctx600a select context.label as c0, count(*) as c1 from SupportBean";
				env.CompileDeploy(eplSum, path).AddListener("s0");

				env.AssertStatement(
					"s0",
					statement => {
						Assert.AreEqual("Ctx600a", statement.GetProperty(StatementProperty.CONTEXTNAME));
						Assert.AreEqual(
							env.DeploymentId("ctx"),
							statement.GetProperty(StatementProperty.CONTEXTDEPLOYMENTID));
					});

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

		public class ContextCategoryContextPartitionSelection : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3".SplitCsv();
				var milestone = new AtomicLong();
				var path = new RegressionPath();

				env.CompileDeploy(
					"@name('ctx') @public create context MyCtx as group by intPrimitive < -5 as grp1, group by intPrimitive between -5 and +5 as grp2, group by intPrimitive > 5 as grp3 from SupportBean",
					path);
				env.CompileDeploy(
					"@name('s0') context MyCtx select context.id as c0, context.label as c1, theString as c2, sum(intPrimitive) as c3 from SupportBean#keepall group by theString",
					path);

				env.SendEventBean(new SupportBean("E1", 1));
				env.SendEventBean(new SupportBean("E2", -5));
				env.SendEventBean(new SupportBean("E1", 2));

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("E3", -100));
				env.SendEventBean(new SupportBean("E3", -8));
				env.SendEventBean(new SupportBean("E1", 60));
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					fields,
					new object[][] {
						new object[] { 0, "grp1", "E3", -108 }, new object[] { 1, "grp2", "E1", 3 },
						new object[] { 1, "grp2", "E2", -5 }, new object[] { 2, "grp3", "E1", 60 }
					});
				SupportContextPropUtil.AssertContextProps(
					env,
					"ctx",
					"MyCtx",
					new int[] { 0, 1, 2 },
					"label",
					new object[][] { new object[] { "grp1" }, new object[] { "grp2" }, new object[] { "grp3" } });

				env.MilestoneInc(milestone);

				// test iterator targeted by context partition id
				var selectorById = new SupportSelectorById(Collections.SingletonSet(1));
				env.AssertStatement(
					"s0",
					statement => EPAssertionUtil.AssertPropsPerRowAnyOrder(
						statement.GetEnumerator(selectorById),
						statement.GetSafeEnumerator(selectorById),
						fields,
						new object[][] { new object[] { 1, "grp2", "E1", 3 }, new object[] { 1, "grp2", "E2", -5 } }));

				// test iterator targeted for a given category
				var selector = new SupportSelectorCategory(new HashSet<string>(Arrays.AsList("grp1", "grp3")));
				env.AssertStatement(
					"s0",
					statement => EPAssertionUtil.AssertPropsPerRowAnyOrder(
						statement.GetEnumerator(selector),
						statement.GetSafeEnumerator(selector),
						fields,
						new object[][]
							{ new object[] { 0, "grp1", "E3", -108 }, new object[] { 2, "grp3", "E1", 60 } }));

				// test iterator targeted for a given filtered category
				env.AssertStatement(
					"s0",
					statement => {
						var filtered = new MySelectorFilteredCategory("grp1");
						EPAssertionUtil.AssertPropsPerRowAnyOrder(
							statement.GetEnumerator(filtered),
							statement.GetSafeEnumerator(filtered),
							fields,
							new object[][] { new object[] { 0, "grp1", "E3", -108 } });
						Assert.IsFalse(
							statement.GetEnumerator(new SupportSelectorCategory((ISet<string>)null)).MoveNext());
						Assert.IsFalse(
							statement.GetEnumerator(new SupportSelectorCategory(EmptySet<string>.Instance)).MoveNext());
					});

				env.MilestoneInc(milestone);

				// test always-false filter - compare context partition info
				env.AssertStatement(
					"s0",
					statement => {
						var filtered = new MySelectorFilteredCategory(null);
						Assert.IsFalse(statement.GetEnumerator(filtered).MoveNext());
						EPAssertionUtil.AssertEqualsAnyOrder(
							new object[] { "grp1", "grp2", "grp3" },
							filtered.Categories);

						try {
							statement.GetEnumerator(new ProxyContextPartitionSelectorSegmented(() => null));
							Assert.Fail();
						}
						catch (InvalidContextPartitionSelector ex) {
							Assert.IsTrue(
								ex.Message.StartsWith(
									"Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorCategory] interfaces but received com."),
								"message: " + ex.Message);
						}
					});

				env.UndeployAll();
			}
		}

		public class ContextCategoryInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				// invalid filter spec
				epl = "create context ACtx group theString is not null as cat1 from SupportBean(dummy = 1)";
				env.TryInvalidCompile(
					epl,
					"Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

				// not a boolean expression
				epl = "create context ACtx group intPrimitive as grp1 from SupportBean";
				env.TryInvalidCompile(epl, "Filter expression not returning a boolean value: 'intPrimitive' [");

				// validate statement not applicable filters
				var path = new RegressionPath();
				env.CompileDeploy("@public create context ACtx group intPrimitive < 10 as cat1 from SupportBean", path);
				epl = "context ACtx select * from SupportBean_S0";
				env.TryInvalidCompile(
					path,
					epl,
					"Category context 'ACtx' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement [");

				env.UndeployAll();
			}
		}

		public class ContextCategoryWContextProps : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var milestone = new AtomicLong();
				var ctx = "CategorizedContext";
				var fields = "c0,c1,c2".SplitCsv();

				var epl = "@name('context') create context " +
				          ctx +
				          " " +
				          "group intPrimitive < 10 as cat1, " +
				          "group intPrimitive between 10 and 20 as cat2, " +
				          "group intPrimitive > 20 as cat3 " +
				          "from SupportBean;\n";
				epl += "@name('s0') context CategorizedContext " +
				       "select context.name as c0, context.label as c1, sum(intPrimitive) as c2 from SupportBean;\n";
				env.CompileDeploy(epl).AddListener("s0");

				env.AssertThat(
					() => {
						Assert.AreEqual(3, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
						AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 3, null, null, null);
					});

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("E1", 5));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat1", 5 });
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					fields,
					new object[][] {
						new object[] { ctx, "cat1", 5 }, new object[] { ctx, "cat2", null },
						new object[] { ctx, "cat3", null }
					});

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("E2", 4));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat1", 9 });

				env.SendEventBean(new SupportBean("E3", 11));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat2", 11 });

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("E4", 25));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat3", 25 });

				env.SendEventBean(new SupportBean("E5", 25));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat3", 50 });

				env.MilestoneInc(milestone);

				env.SendEventBean(new SupportBean("E6", 3));
				env.AssertPropsNew("s0", fields, new object[] { ctx, "cat1", 12 });

				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					fields,
					new object[][] {
						new object[] { ctx, "cat1", 12 }, new object[] { ctx, "cat2", 11 },
						new object[] { ctx, "cat3", 50 }
					});

				env.AssertThat(
					() => {
						Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));
						Assert.AreEqual(3, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
					});

				env.UndeployModuleContaining("s0");

				env.AssertThat(
					() => {
						Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));
						Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
					});
			}
		}

		public class ContextCategorySingleCategorySODAPrior : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				var milestone = new AtomicLong();
				var ctx = "CategorizedContext";
				var eplCtx = "@name('context') @public create context " +
				             ctx +
				             " as " +
				             "group intPrimitive<10 as cat1 " +
				             "from SupportBean";
				env.CompileDeploy(eplCtx, path);

				var eplStmt =
					"@name('s0') context CategorizedContext select context.name as c0, context.label as c1, prior(1,intPrimitive) as c2 from SupportBean";
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

		private static void RunAssertion(
			RegressionEnvironment env,
			string ctx,
			AtomicLong milestone)
		{

			var fields = "c0,c1,c2".SplitCsv();
			env.SendEventBean(new SupportBean("E1", 5));
			env.AssertPropsNew("s0", fields, new object[] { ctx, "cat1", null });

			env.MilestoneInc(milestone);

			env.SendEventBean(new SupportBean("E2", 20));
			env.AssertListenerNotInvoked("s0");

			env.MilestoneInc(milestone);

			env.SendEventBean(new SupportBean("E1", 4));
			env.AssertPropsNew("s0", fields, new object[] { ctx, "cat1", 5 });

			env.AssertThat(() => Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env)));
			env.UndeployAll();
			env.AssertThat(() => Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env)));
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
					"@name('ctx') @public create context MyCtx as " +
					"group by intPrimitive < 0 as n, " +
					"group by intPrimitive > 0 as p " +
					"from SupportBean",
					path);
				env.CompileDeploy("@name('expr-1') @public create expression getLabelOne { context.label }", path);
				env.CompileDeploy(
					"@name('expr-2') @public create expression getLabelTwo { 'x'||context.label||'x' }",
					path);

				env.Milestone(0);

				if (!isAlias) {
					env.CompileDeploy(
						"@name('s0') expression getLabelThree { context.label } " +
						"context MyCtx " +
						"select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean",
						path);
				}
				else {
					env.CompileDeploy(
						"@name('s0') expression getLabelThree alias for { context.label } " +
						"context MyCtx " +
						"select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean",
						path);
				}

				env.AddListener("s0");

				env.Milestone(1);

				var fields = "c0,c1,c2".SplitCsv();
				env.SendEventBean(new SupportBean("E1", -2));
				env.AssertPropsNew("s0", fields, new object[] { "n", "xnx", "n" });

				env.Milestone(2);

				env.SendEventBean(new SupportBean("E2", 1));
				env.AssertPropsNew("s0", fields, new object[] { "p", "xpx", "p" });

				env.UndeployAll();
			}

			public string Name()
			{
				return this.GetType().Name +
				       "{" +
				       "isAlias=" +
				       isAlias +
				       '}';
			}
		}

		private static void SendAssertBooleanExprFilter(
			RegressionEnvironment env,
			string theString,
			string groupExpected,
			long countExpected)
		{
			var fields = "c0,c1".SplitCsv();
			env.SendEventBean(new SupportBean(theString, 1));
			env.AssertPropsNew("s0", fields, new object[] { groupExpected, countExpected });
		}

		internal class MySelectorFilteredCategory : ContextPartitionSelectorFiltered
		{

			private readonly string matchCategory;

			private IList<object> categories = new List<object>();
			private LinkedHashSet<int> cpids = new LinkedHashSet<int>();

			internal MySelectorFilteredCategory(string matchCategory)
			{
				this.matchCategory = matchCategory;
			}

			public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
			{
				var id = (ContextPartitionIdentifierCategory)contextPartitionIdentifier;
				if (matchCategory == null && cpids.Contains(id.ContextPartitionId)) {
					throw new EPRuntimeException("Already exists context id: " + id.ContextPartitionId);
				}

				cpids.Add(id.ContextPartitionId);
				categories.Add(id.Label);
				return matchCategory != null && matchCategory.Equals(id.Label);
			}

			public object[] Categories => categories.ToArray();
		}
	}
} // end of namespace
