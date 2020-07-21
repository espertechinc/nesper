///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
	public class ExprFilterOptimizableValueLimitedExpr
	{
		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprFilterOptValEqualsIsConstant());
			executions.Add(new ExprFilterOptValEqualsFromPatternSingle());
			executions.Add(new ExprFilterOptValEqualsFromPatternMulti());
			executions.Add(new ExprFilterOptValEqualsFromPatternConstant());
			executions.Add(new ExprFilterOptValEqualsFromPatternHalfConstant());
			executions.Add(new ExprFilterOptValEqualsFromPatternWithDotMethod());
			executions.Add(new ExprFilterOptValEqualsContextWithStart());
			executions.Add(new ExprFilterOptValEqualsSubstitutionParams());
			executions.Add(new ExprFilterOptValEqualsConstantVariable());
			executions.Add(new ExprFilterOptValEqualsCoercion());
			executions.Add(new ExprFilterOptValRelOpCoercion());
			executions.Add(new ExprFilterOptValDisqualify());
			executions.Add(new ExprFilterOptValInSetOfValueWPatternWCoercion());
			executions.Add(new ExprFilterOptValInRangeWCoercion());
			executions.Add(new ExprFilterOptValOrRewrite());
			return executions;
		}

		private class ExprFilterOptValOrRewrite : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
				             "@name('s0') context MyContext select * from SupportBean(theString = context.s0.p00 || context.s0.p01 or theString = context.s0.p01 || context.s0.p00);\n";
				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(1, "a", "b"));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcSingle(env.Statement("s0"), "theString", IN_LIST_OF_VALUES);
				}

				SendSBAssert(env, "ab", true);
				SendSBAssert(env, "ba", true);
				SendSBAssert(env, "aa", false);
				SendSBAssert(env, "aa", false);

				env.UndeployAll();
			}
		}

		private class ExprFilterOptValInRangeWCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern [" +
				             "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive in [a.id - 2 : b.id + 2])];\n";
				RunAssertionInRange(env, epl, false);

				epl = "@name('s0') select * from pattern [" +
				      "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive not in [a.id - 2 : b.id + 2])];\n";
				RunAssertionInRange(env, epl, true);
			}

			private void RunAssertionInRange(
				RegressionEnvironment env,
				string epl,
				bool not)
			{
				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(10));
				env.SendEventBean(new SupportBean_S1(200));

				env.Milestone(0);
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcSingle(env.Statement("s0"), "longPrimitive", not ? NOT_RANGE_CLOSED : RANGE_CLOSED);
				}

				SendSBAssert(env, 7, not);
				SendSBAssert(env, 8, !not);
				SendSBAssert(env, 100, !not);
				SendSBAssert(env, 202, !not);
				SendSBAssert(env, 203, not);

				env.UndeployAll();
			}
		}

		private class ExprFilterOptValInSetOfValueWPatternWCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern [" +
				             "a=SupportBean_S0 -> b=SupportBean_S1 -> c=SupportBean_S2 -> every SupportBean(longPrimitive in (a.id, b.id, c.id))];\n";
				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(10));
				env.SendEventBean(new SupportBean_S1(200));
				env.SendEventBean(new SupportBean_S2(3000));

				env.Milestone(0);

				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcSingle(env.Statement("s0"), "longPrimitive", IN_LIST_OF_VALUES);
				}

				SendSBAssert(env, 0, false);
				SendSBAssert(env, 10, true);
				SendSBAssert(env, 200, true);
				SendSBAssert(env, 3000, true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsFromPatternWithDotMethod : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern [a=SupportBean -> b=SupportBean(theString=a.getTheString())]";
				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean("E1", 1));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcSingle(env.Statement("s0"), "theString", EQUAL);
				}

				env.SendEventBean(new SupportBean("E1", 2));
				EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "a.intPrimitive,b.intPrimitive".SplitCsv(), new object[] {1, 2});
				env.UndeployAll();
			}
		}

		public class ExprFilterOptValRelOpCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from SupportBean(Integer.parseInt('10') > doublePrimitive)";
				RunAssertionRelOpCoercion(env, epl);

				epl = "@name('s0') select * from SupportBean(doublePrimitive < Integer.parseInt('10'))";
				RunAssertionRelOpCoercion(env, epl);
			}
		}

		public class ExprFilterOptValEqualsCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from SupportBean(doublePrimitive = Integer.parseInt('10') + Long.parseLong('20'))";
				env.CompileDeploy(epl).AddListener("s0");
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcSingle(env.Statement("s0"), "doublePrimitive", EQUAL);
				}

				SendSBAssert(env, 30d, true);
				SendSBAssert(env, 20d, false);
				SendSBAssert(env, 30d, true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsConstantVariable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string variable = "create constant variable string MYCONST = 'a';\n";
				TryDeployAndAssertionSB(env, variable + "@name('s0') select * from SupportBean(theString = MYCONST || 'x')", EQUAL);
				TryDeployAndAssertionSB(env, variable + "@name('s0') select * from SupportBean(MYCONST || 'x' = theString)", EQUAL);
			}
		}

		public class ExprFilterOptValEqualsSubstitutionParams : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from SupportBean(theString = ?::string)";
				EPCompiled compiled = env.Compile(epl);
				DeploymentOptions options = new DeploymentOptions();
				options.WithStatementSubstitutionParameter(opt => opt.SetObject(1, "ax"));
				env.Deploy(compiled, options).AddListener("s0");
				RunAssertionSB(env, epl, EQUAL);
			}
		}

		public class ExprFilterOptValEqualsIsConstant : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				TryDeployAndAssertionSB(env, "@name('s0') select * from SupportBean(theString = 'a' || 'x')", EQUAL);
				TryDeployAndAssertionSB(env, "@name('s0') select * from SupportBean('a' || 'x' is theString)", IS);
			}
		}

		public class ExprFilterOptValEqualsFromPatternSingle : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern[every a=SupportBean_S0 -> SupportBean(a.p00 || a.p01 = theString)]";

				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(0, "a", "x"));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("theString", EQUAL));
				}

				SendSBAssert(env, "a", false);
				SendSBAssert(env, "ax", true);

				env.Milestone(0);

				env.SendEventBean(new SupportBean_S0(0, "b", "y"));
				SendSBAssert(env, "ax", false);
				SendSBAssert(env, "by", true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsFromPatternConstant : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern[every SupportBean_S0 -> SupportBean_S1 -> SupportBean('a' || 'x' = theString)]";

				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(1));
				env.SendEventBean(new SupportBean_S1(2));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("theString", EQUAL));
				}

				SendSBAssert(env, "a", false);
				SendSBAssert(env, "ax", true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsFromPatternHalfConstant : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern[every s0=SupportBean_S0 -> s1=SupportBean_S1 -> SupportBean('a' || s1.p10 = theString)]";

				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(1));
				env.SendEventBean(new SupportBean_S1(2, "x"));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("theString", EQUAL));
				}

				SendSBAssert(env, "a", false);
				SendSBAssert(env, "ax", true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsFromPatternMulti : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "@name('s0') select * from pattern[every [2] a=SupportBean_S0 -> b=SupportBean_S1 -> SupportBean(theString = a[0].p00 || b.p10)]";

				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(1, "a"));
				env.SendEventBean(new SupportBean_S0(2, "b"));
				env.SendEventBean(new SupportBean_S1(2, "x"));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("theString", EQUAL));
				}

				SendSBAssert(env, "a", false);
				SendSBAssert(env, "ax", true);

				env.SendEventBean(new SupportBean_S0(1, "z"));
				env.SendEventBean(new SupportBean_S0(2, "-"));
				env.SendEventBean(new SupportBean_S1(2, "y"));

				env.Milestone(0);

				SendSBAssert(env, "ax", false);
				SendSBAssert(env, "zy", true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValEqualsContextWithStart : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
				             "@name('s0') context MyContext select * from SupportBean(theString = context.s0.p00 || context.s0.p01)";

				env.CompileDeploy(epl).AddListener("s0");
				env.SendEventBean(new SupportBean_S0(0, "a", "x"));
				if (HasFilterIndexPlanAdvanced(env)) {
					AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("theString", EQUAL));
				}

				SendSBAssert(env, "a", false);
				SendSBAssert(env, "ax", true);

				env.Milestone(0);

				SendSBAssert(env, "by", false);
				SendSBAssert(env, "ax", true);

				env.UndeployAll();
			}
		}

		public class ExprFilterOptValDisqualify : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				RegressionPath path = new RegressionPath();
				string objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
				                 "@public create table MyTable(tablecol string);\n" +
				                 "@public create window MyWindow#keepall as SupportBean;\n" +
				                 "@public create inlined_class \"\"\"\n" +
				                 "  public class Helper {\n" +
				                 "    public static String doit(Object param) {\n" +
				                 "      return null;\n" +
				                 "    }\n" +
				                 "  }\n" +
				                 "\"\"\";\n" +
				                 "@public create expression MyDeclaredExpr { (select theString from MyWindow) };\n" +
				                 "@public create expression MyHandThrough {v => v};\n" +
				                 "@public create expression string js:MyJavaScript(param) [\"a\"];\n";
				env.Compile(objects, path);

				AssertDisqualified(env, path, "SupportBean", "theString=Integer.toString(intPrimitive)");
				AssertDisqualified(env, path, "SupportBean", "theString=MYVARIABLE_NONCONSTANT");
				AssertDisqualified(env, path, "SupportBean", "theString=MyTable.tablecol");
				AssertDisqualified(env, path, "SupportBean", "theString=(select theString from MyWindow)");
				AssertDisqualified(env, path, "SupportBeanArrayCollMap", "id = setOfString.where(v => v=id).firstOf()");
				AssertDisqualified(env, path, "SupportBean", "theString=Helper.doit(*)");
				AssertDisqualified(env, path, "SupportBean", "theString=Helper.doit(me)");
				AssertDisqualified(env, path, "SupportBean", "boolPrimitive=event_identity_equals(me, me)");
				AssertDisqualified(env, path, "SupportBean", "theString=MyDeclaredExpr()");
				AssertDisqualified(env, path, "SupportBean", "intPrimitive=theString.length()");
				AssertDisqualified(env, path, "SupportBean", "intPrimitive = funcOne('hello')");
				AssertDisqualified(env, path, "SupportBean", "boolPrimitive = exists(theString)");
				AssertDisqualified(env, path, "SupportBean", "theString = MyJavaScript('a')");
				AssertDisqualified(env, path, "SupportBean", "theString = MyHandThrough('a')");
			}
		}

		private static void TryDeployAndAssertionSB(
			RegressionEnvironment env,
			string epl,
			FilterOperator op)
		{
			env.CompileDeploy(epl).AddListener("s0");
			RunAssertionSB(env, epl, op);
		}

		private static void RunAssertionSB(
			RegressionEnvironment env,
			string epl,
			FilterOperator op)
		{
			if (HasFilterIndexPlanAdvanced(env)) {
				AssertFilterSvcSingle(env.Statement("s0"), "theString", op);
			}

			SendSBAssert(env, "ax", true);
			SendSBAssert(env, "a", false);

			env.Milestone(0);

			SendSBAssert(env, "bx", false);
			SendSBAssert(env, "ax", true);

			env.UndeployAll();
		}

		protected static void AssertDisqualified(
			RegressionEnvironment env,
			RegressionPath path,
			string typeName,
			string filters)
		{
			string hook = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" + typeof(SupportFilterPlanHook).Name + "')";
			string epl = hook + "select * from " + typeName + "(" + filters + ") as me";
			SupportFilterPlanHook.Reset();
			env.Compile(epl, path);
			FilterSpecParamForge forge = SupportFilterPlanHook.AssertPlanSingleTripletAndReset(typeName);
			if (forge.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION && forge.FilterOperator != REBOOL) {
				Assert.Fail();
			}
		}

		private static void SendSBAssert(
			RegressionEnvironment env,
			string theString,
			bool received)
		{
			env.SendEventBean(new SupportBean(theString, 0));
			Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
		}

		private static void SendSBAssert(
			RegressionEnvironment env,
			double doublePrimitive,
			bool received)
		{
			SupportBean sb = new SupportBean("E", 0);
			sb.DoublePrimitive = doublePrimitive;
			env.SendEventBean(sb);
			Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
		}

		private static void SendSBAssert(
			RegressionEnvironment env,
			long longPrimitive,
			bool received)
		{
			SupportBean sb = new SupportBean("E", 0);
			sb.LongPrimitive = longPrimitive;
			env.SendEventBean(sb);
			Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
		}

		private static void RunAssertionRelOpCoercion(
			RegressionEnvironment env,
			string epl)
		{
			env.CompileDeploy(epl).AddListener("s0");
			if (HasFilterIndexPlanAdvanced(env)) {
				AssertFilterSvcSingle(env.Statement("s0"), "doublePrimitive", LESS);
			}

			SendSBAssert(env, 3d, true);
			SendSBAssert(env, 20d, false);
			SendSBAssert(env, 4d, true);

			env.UndeployAll();
		}
	}
} // end of namespace
