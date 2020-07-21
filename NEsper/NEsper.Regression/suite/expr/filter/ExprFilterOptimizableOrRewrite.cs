///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
	public class ExprFilterOptimizableOrRewrite
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprFilterOrRewriteTwoOr());
			executions.Add(new ExprFilterOrRewriteOrRewriteThreeOr());
			executions.Add(new ExprFilterOrRewriteOrRewriteWithAnd());
			executions.Add(new ExprFilterOrRewriteOrRewriteThreeWithOverlap());
			executions.Add(new ExprFilterOrRewriteOrRewriteFourOr());
			executions.Add(new ExprFilterOrRewriteOrRewriteEightOr());
			executions.Add(new ExprFilterOrRewriteAndRewriteNotEqualsOr());
			executions.Add(new ExprFilterOrRewriteAndRewriteNotEqualsConsolidate());
			executions.Add(new ExprFilterOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond());
			executions.Add(new ExprFilterOrRewriteAndRewriteInnerOr());
			executions.Add(new ExprFilterOrRewriteOrRewriteAndOrMulti());
			executions.Add(new ExprFilterOrRewriteBooleanExprSimple());
			executions.Add(new ExprFilterOrRewriteBooleanExprAnd());
			executions.Add(new ExprFilterOrRewriteSubquery());
			executions.Add(new ExprFilterOrRewriteHint());
			executions.Add(new ExprFilterOrRewriteContextPartitionedSegmented());
			executions.Add(new ExprFilterOrRewriteContextPartitionedHash());
			executions.Add(new ExprFilterOrRewriteContextPartitionedCategory());
			executions.Add(new ExprFilterOrRewriteContextPartitionedInitiatedSameEvent());
			executions.Add(new ExprFilterOrRewriteContextPartitionedInitiated());
			return executions;
		}

		public class ExprFilterOrRewriteHint : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@Hint('MAX_FILTER_WIDTH=0') @name('s0') select * from SupportBean_IntAlphabetic((b=1 or c=1) and (d=1 or e=1))";
				env.CompileDeployAddListenerMile(epl, "s0", 0);
				SupportFilterServiceHelper.AssertFilterSvcSingle(env.Statement("s0"), ".boolean_expression", FilterOperator.BOOLEAN_EXPRESSION);
				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteSubquery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string epl = "@name('s0') select (select * from SupportBean_IntAlphabetic(a=1 or b=1)#keepall) as c0 from SupportBean";
				env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

				SupportBean_IntAlphabetic iaOne = IntEvent(1, 1);
				env.SendEventBean(iaOne);
				env.SendEventBean(new SupportBean());
				Assert.AreEqual(iaOne, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteContextPartitionedCategory : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('ctx') create context MyContext \n" +
				             "  group a=1 or b=1 as g1,\n" +
				             "  group c=1 as g1\n" +
				             "  from SupportBean_IntAlphabetic;" +
				             "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(d=1 or e=1)";
				env.CompileDeployAddListenerMile(epl, "s0", 0);

				SendAssertEvents(
					env,
					new object[] {IntEvent(1, 0, 0, 0, 1), IntEvent(0, 1, 0, 1, 0), IntEvent(0, 0, 1, 1, 1)},
					new object[] {IntEvent(0, 0, 0, 1, 0), IntEvent(1, 0, 0, 0, 0), IntEvent(0, 0, 1, 0, 0)}
				);

				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteContextPartitionedHash : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create context MyContext " +
				             "coalesce by consistent_hash_crc32(a) from SupportBean_IntAlphabetic(b=1) granularity 16 preallocate;" +
				             "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(c=1 or d=1)";
				env.CompileDeployAddListenerMile(epl, "s0", 0);

				SendAssertEvents(
					env,
					new object[] {IntEvent(100, 1, 0, 1), IntEvent(100, 1, 1, 0)},
					new object[] {IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
				);
				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteContextPartitionedSegmented : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create context MyContext partition by a from SupportBean_IntAlphabetic(b=1 or c=1);" +
				             "@name('s0') context MyContext select * from SupportBean_IntAlphabetic(d=1)";
				env.CompileDeployAddListenerMile(epl, "s0", 0);

				SendAssertEvents(
					env,
					new object[] {IntEvent(100, 1, 0, 1), IntEvent(100, 0, 1, 1)},
					new object[] {IntEvent(100, 0, 0, 1), IntEvent(100, 1, 0, 0)}
				);
				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteBooleanExprAnd : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filters = new[] {
					"(a='a' or a like 'A%') and (b='b' or b like 'B%')",
				};
				foreach (string filter in filters) {
					string epl = "@name('s0') select * from SupportBean_StringAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasic(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_StringAlphabetic",
							new[] {
								new[] {new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL)},
								new[] {new FilterItem("a", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem},
								new[] {new FilterItem("b", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem},
								new[] {FilterItem.BoolExprFilterItem},
							});
					}

					SendAssertEvents(
						env,
						new object[] {StringEvent("a", "b"), StringEvent("A1", "b"), StringEvent("a", "B1"), StringEvent("A1", "B1")},
						new object[] {StringEvent("x", "b"), StringEvent("a", "x"), StringEvent("A1", "C"), StringEvent("C", "B1")}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteBooleanExprSimple : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filters = new[] {
					"a like 'a%' and (b='b' or c='c')",
				};
				foreach (string filter in filters) {
					string epl = "@name('s0') select * from SupportBean_StringAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasic(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_StringAlphabetic",
							new[] {
								new[] {new FilterItem("b", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem},
								new[] {new FilterItem("c", FilterOperator.EQUAL), FilterItem.BoolExprFilterItem},
							});
					}

					SendAssertEvents(
						env,
						new object[] {StringEvent("a1", "b", null), StringEvent("a1", null, "c")},
						new object[] {StringEvent("x", "b", null), StringEvent("a1", null, null), StringEvent("a1", null, "x")}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteAndRewriteNotEqualsWithOrConsolidateSecond : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filters = new[] {
					"a!=1 and a!=2 and ((a!=3 and a!=4) or (a!=5 and a!=6))",
				};
				foreach (string filter in filters) {
					string epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_IntAlphabetic",
							new[] {
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), FilterItem.BoolExprFilterItem},
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), FilterItem.BoolExprFilterItem},
							});
					}

					SendAssertEvents(
						env,
						new object[] {IntEvent(3), IntEvent(4), IntEvent(0)},
						new object[] {IntEvent(2), IntEvent(1)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteAndRewriteNotEqualsConsolidate : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filters = new[] {
					"a!=1 and a!=2 and (a!=3 or a!=4)",
				};
				foreach (string filter in filters) {
					string epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_IntAlphabetic",
							new[] {
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("a", FilterOperator.NOT_EQUAL)},
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("a", FilterOperator.NOT_EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new object[] {IntEvent(3), IntEvent(4), IntEvent(0)},
						new object[] {IntEvent(2), IntEvent(1)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteAndRewriteNotEqualsOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filters = new[] {
					"a!=1 and a!=2 and (b=1 or c=1)",
				};
				foreach (string filter in filters) {
					string epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_IntAlphabetic",
							new[] {
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("b", FilterOperator.EQUAL)},
								new[] {new FilterItem("a", FilterOperator.NOT_IN_LIST_OF_VALUES), new FilterItem("c", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new object[] {IntEvent(3, 1, 0), IntEvent(3, 0, 1), IntEvent(0, 1, 0)},
						new object[] {IntEvent(2, 0, 0), IntEvent(1, 0, 0), IntEvent(3, 0, 0)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteAndRewriteInnerOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"theString='a' and (intPrimitive=1 or longPrimitive=10)",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("theString", FilterOperator.EQUAL), new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("theString", FilterOperator.EQUAL), new FilterItem("longPrimitive", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {MakeEvent("a", 1, 0), MakeEvent("a", 0, 10), MakeEvent("a", 1, 10)},
						new[] {MakeEvent("x", 0, 0), MakeEvent("a", 2, 20), MakeEvent("x", 1, 10)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteOrRewriteAndOrMulti : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"a=1 and (b=1 or c=1) and (d=1 or e=1)",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean_IntAlphabetic(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean_IntAlphabetic",
							new[] {
								new[] {
									new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL),
									new FilterItem("d", FilterOperator.EQUAL)
								},
								new[] {
									new FilterItem("a", FilterOperator.EQUAL), new FilterItem("c", FilterOperator.EQUAL),
									new FilterItem("d", FilterOperator.EQUAL)
								},
								new[] {
									new FilterItem("a", FilterOperator.EQUAL), new FilterItem("c", FilterOperator.EQUAL),
									new FilterItem("e", FilterOperator.EQUAL)
								},
								new[] {
									new FilterItem("a", FilterOperator.EQUAL), new FilterItem("b", FilterOperator.EQUAL),
									new FilterItem("e", FilterOperator.EQUAL)
								},
							});
					}

					SendAssertEvents(
						env,
						new object[] {IntEvent(1, 1, 0, 1, 0), IntEvent(1, 0, 1, 0, 1), IntEvent(1, 1, 0, 0, 1), IntEvent(1, 0, 1, 1, 0)},
						new object[] {IntEvent(1, 0, 0, 1, 0), IntEvent(1, 0, 0, 1, 0), IntEvent(1, 1, 1, 0, 0), IntEvent(0, 1, 1, 1, 1)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteOrRewriteEightOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"theString = 'a' or intPrimitive=1 or longPrimitive=10 or doublePrimitive=100 or boolPrimitive=true or " +
					"intBoxed=2 or longBoxed=20 or doubleBoxed=200",
					"longBoxed=20 or theString = 'a' or boolPrimitive=true or intBoxed=2 or longPrimitive=10 or doublePrimitive=100 or " +
					"intPrimitive=1 or doubleBoxed=200",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
								new[] {new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("longPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("doublePrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("boolPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("intBoxed", FilterOperator.EQUAL)},
								new[] {new FilterItem("longBoxed", FilterOperator.EQUAL)},
								new[] {new FilterItem("doubleBoxed", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {
							MakeEvent("a", 1, 10, 100, true, 2, 20, 200), MakeEvent("a", 0, 0, 0, true, 0, 0, 0),
							MakeEvent("a", 0, 0, 0, true, 0, 20, 0), MakeEvent("x", 0, 0, 100, false, 0, 0, 0),
							MakeEvent("x", 1, 0, 0, false, 0, 0, 200), MakeEvent("x", 0, 0, 0, false, 0, 0, 200),
						},
						new[] {MakeEvent("x", 0, 0, 0, false, 0, 0, 0)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteOrRewriteFourOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"theString = 'a' or intPrimitive=1 or longPrimitive=10 or doublePrimitive=100",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
								new[] {new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("longPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("doublePrimitive", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {MakeEvent("a", 1, 10, 100), MakeEvent("x", 0, 0, 100), MakeEvent("x", 0, 10, 100), MakeEvent("a", 0, 0, 0)},
						new[] {MakeEvent("x", 0, 0, 0)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteContextPartitionedInitiated : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('ctx') create context MyContext initiated by SupportBean(theString='A' or intPrimitive=1) terminated after 24 hours;\n" +
				             "@name('s0') context MyContext select * from SupportBean;\n";
				env.CompileDeployAddListenerMile(epl, "s0", 0);

				env.SendEventBean(new SupportBean("A", 1));
				env.Listener("s0").AssertOneGetNewAndReset();

				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteContextPartitionedInitiatedSameEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create context MyContext initiated by SupportBean terminated after 24 hours;" +
				             "@name('s0') context MyContext select * from SupportBean(theString='A' or intPrimitive=1)";
				env.CompileDeployAddListenerMile(epl, "s0", 0);

				env.SendEventBean(new SupportBean("A", 1));
				env.Listener("s0").AssertOneGetNewAndReset();

				env.UndeployAll();
			}
		}

		public class ExprFilterOrRewriteOrRewriteThreeOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"theString = 'a' or intPrimitive = 1 or longPrimitive = 2",
					"2 = longPrimitive or 1 = intPrimitive or theString = 'a'"
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
								new[] {new FilterItem("longPrimitive", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {MakeEvent("a", 0, 0), MakeEvent("b", 1, 0), MakeEvent("c", 0, 2), MakeEvent("c", 0, 2)},
						new[] {MakeEvent("v", 0, 0), MakeEvent("c", 2, 1)}
					);

					env.UndeployAll();
				}
			}
		}

		private static void SendAssertEvents(
			RegressionEnvironment env,
			object[] matches,
			object[] nonMatches)
		{
			env.Listener("s0").Reset();
			foreach (object match in matches) {
				env.SendEventBean(match);
				Assert.AreSame(match, env.Listener("s0").AssertOneGetNewAndReset().Underlying);
			}

			env.Listener("s0").Reset();
			foreach (object nonMatch in nonMatches) {
				env.SendEventBean(nonMatch);
				Assert.IsFalse(env.Listener("s0").IsInvoked);
			}
		}

		public class ExprFilterOrRewriteOrRewriteThreeWithOverlap : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"theString = 'a' or theString = 'b' or intPrimitive=1",
					"intPrimitive = 1 or theString = 'b' or theString = 'a'",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
								new[] {new FilterItem("intPrimitive", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {MakeEvent("a", 1), MakeEvent("b", 0), MakeEvent("x", 1)},
						new[] {MakeEvent("x", 0)}
					);
					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteTwoOr : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();

				// test 'or' rewrite
				string[] filtersAB = new[] {
					"select * from SupportBean(theString = 'a' or intPrimitive = 1)",
					"select * from SupportBean(theString = 'a' or 1 = intPrimitive)",
					"select * from SupportBean('a' = theString or 1 = intPrimitive)",
					"select * from SupportBean('a' = theString or intPrimitive = 1)",
				};

				foreach (string filter in filtersAB) {
					env.CompileDeployAddListenerMile("@name('s0')" + filter, "s0", milestone.GetAndIncrement());

					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("theString", FilterOperator.EQUAL)},
							});
					}

					env.SendEventBean(new SupportBean("a", 0));
					env.Listener("s0").AssertOneGetNewAndReset();
					env.SendEventBean(new SupportBean("b", 1));
					env.Listener("s0").AssertOneGetNewAndReset();
					env.SendEventBean(new SupportBean("c", 0));
					Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

					env.UndeployAll();
				}
			}
		}

		public class ExprFilterOrRewriteOrRewriteWithAnd : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				AtomicLong milestone = new AtomicLong();
				string[] filtersAB = new[] {
					"(theString = 'a' and intPrimitive = 1) or (theString = 'b' and intPrimitive = 2)",
					"(intPrimitive = 1 and theString = 'a') or (intPrimitive = 2 and theString = 'b')",
					"(theString = 'b' and intPrimitive = 2) or (theString = 'a' and intPrimitive = 1)",
				};
				foreach (string filter in filtersAB) {
					string epl = "@name('s0') select * from SupportBean(" + filter + ")";
					env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());
					if (SupportFilterOptimizableHelper.HasFilterIndexPlanBasicOrMore(env)) {
						SupportFilterServiceHelper.AssertFilterSvcByTypeMulti(
							env.Statement("s0"),
							"SupportBean",
							new[] {
								new[] {new FilterItem("theString", FilterOperator.EQUAL), new FilterItem("intPrimitive", FilterOperator.EQUAL)},
								new[] {new FilterItem("theString", FilterOperator.EQUAL), new FilterItem("intPrimitive", FilterOperator.EQUAL)},
							});
					}

					SendAssertEvents(
						env,
						new[] {MakeEvent("a", 1), MakeEvent("b", 2)},
						new[] {MakeEvent("x", 0), MakeEvent("a", 0), MakeEvent("a", 2), MakeEvent("b", 1)}
					);
					env.UndeployAll();
				}
			}
		}

		private static SupportBean MakeEvent(
			string theString,
			int intPrimitive)
		{
			return MakeEvent(theString, intPrimitive, 0L);
		}

		private static SupportBean MakeEvent(
			string theString,
			int intPrimitive,
			long longPrimitive)
		{
			return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
		}

		private static SupportBean_IntAlphabetic IntEvent(int a)
		{
			return new SupportBean_IntAlphabetic(a);
		}

		private static SupportBean_IntAlphabetic IntEvent(
			int a,
			int b)
		{
			return new SupportBean_IntAlphabetic(a, b);
		}

		private static SupportBean_IntAlphabetic IntEvent(
			int a,
			int b,
			int c,
			int d)
		{
			return new SupportBean_IntAlphabetic(a, b, c, d);
		}

		private static SupportBean_StringAlphabetic StringEvent(
			string a,
			string b)
		{
			return new SupportBean_StringAlphabetic(a, b);
		}

		private static SupportBean_StringAlphabetic StringEvent(
			string a,
			string b,
			string c)
		{
			return new SupportBean_StringAlphabetic(a, b, c);
		}

		private static SupportBean_IntAlphabetic IntEvent(
			int a,
			int b,
			int c)
		{
			return new SupportBean_IntAlphabetic(a, b, c);
		}

		private static SupportBean_IntAlphabetic IntEvent(
			int a,
			int b,
			int c,
			int d,
			int e)
		{
			return new SupportBean_IntAlphabetic(a, b, c, d, e);
		}

		private static SupportBean MakeEvent(
			string theString,
			int intPrimitive,
			long longPrimitive,
			double doublePrimitive)
		{
			SupportBean @event = new SupportBean(theString, intPrimitive);
			@event.LongPrimitive = longPrimitive;
			@event.DoublePrimitive = doublePrimitive;
			return @event;
		}

		private static SupportBean MakeEvent(
			string theString,
			int intPrimitive,
			long longPrimitive,
			double doublePrimitive,
			bool boolPrimitive,
			int intBoxed,
			long longBoxed,
			double doubleBoxed)
		{
			SupportBean @event = new SupportBean(theString, intPrimitive);
			@event.LongPrimitive = longPrimitive;
			@event.DoublePrimitive = doublePrimitive;
			@event.BoolPrimitive = boolPrimitive;
			@event.LongBoxed = longBoxed;
			@event.DoubleBoxed = doubleBoxed;
			@event.IntBoxed = intBoxed;
			return @event;
		}
	}
} // end of namespace
