///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;


namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
	public class EPLSubselectIn
	{
		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLSubselectInSelect());
			execs.Add(new EPLSubselectInSelectOM());
			execs.Add(new EPLSubselectInSelectCompile());
			execs.Add(new EPLSubselectInSelectWhere());
			execs.Add(new EPLSubselectInSelectWhereExpressions());
			execs.Add(new EPLSubselectInFilterCriteria());
			execs.Add(new EPLSubselectInWildcard());
			execs.Add(new EPLSubselectInNullable());
			execs.Add(new EPLSubselectInNullableCoercion());
			execs.Add(new EPLSubselectInNullRow());
			execs.Add(new EPLSubselectInSingleIndex());
			execs.Add(new EPLSubselectInMultiIndex());
			execs.Add(new EPLSubselectNotInNullRow());
			execs.Add(new EPLSubselectNotInSelect());
			execs.Add(new EPLSubselectNotInNullableCoercion());
			execs.Add(new EPLSubselectInvalid());
			return execs;
		}

		private class EPLSubselectInSelect : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select id in (select id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
				env.CompileDeployAddListenerMileZero(stmtText, "s0");
				SupportAdminUtil.AssertStatelessStmt(env, "s0", false);

				RunTestInSelect(env);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInSelectOM : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var subquery = new EPStatementObjectModel();
				subquery.SelectClause = SelectClause.Create("id");
				subquery.FromClause = FromClause.Create(
					FilterStream.Create("SupportBean_S1").AddView(View.Create("length", Expressions.Constant(1000))));

				var model = new EPStatementObjectModel();
				model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_S0"));
				model.SelectClause = SelectClause.Create().Add(Expressions.SubqueryIn("id", subquery), "value");
				model = env.CopyMayFail(model);

				var stmtText = "select id in (select id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
				Assert.AreEqual(stmtText, model.ToEPL());

				model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
				env.CompileDeploy(model).AddListener("s0").Milestone(0);

				RunTestInSelect(env);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInSelectCompile : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select id in (select id from SupportBean_S1#length(1000)) as value from SupportBean_S0";
				env.EplToModelCompileDeploy(stmtText).AddListener("s0");

				RunTestInSelect(env);

				env.UndeployAll();
			}
		}

		public class EPLSubselectInFilterCriteria : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = new string[] { "id" };
				var text = "@name('s0') select id from SupportBean_S0(id in (select id from SupportBean_S1#length(2)))";
				env.CompileDeployAddListenerMileZero(text, "s0");

				env.SendEventBean(new SupportBean_S0(1));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean_S1(10));

				env.SendEventBean(new SupportBean_S0(10));
				env.AssertPropsNew("s0", fields, new object[] { 10 });
				env.SendEventBean(new SupportBean_S0(11));
				env.AssertListenerNotInvoked("s0");

				env.Milestone(1);

				env.SendEventBean(new SupportBean_S0(10));
				env.AssertPropsNew("s0", fields, new object[] { 10 });
				env.SendEventBean(new SupportBean_S0(11));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean_S1(11));
				env.SendEventBean(new SupportBean_S0(11));
				env.AssertPropsNew("s0", fields, new object[] { 11 });

				env.Milestone(2);

				env.SendEventBean(new SupportBean_S0(11));
				env.AssertPropsNew("s0", fields, new object[] { 11 });

				env.SendEventBean(new SupportBean_S1(12)); //pushing 10 out

				env.Milestone(3);

				env.SendEventBean(new SupportBean_S0(10));
				env.AssertListenerNotInvoked("s0");
				env.SendEventBean(new SupportBean_S0(11));
				env.AssertPropsNew("s0", fields, new object[] { 11 });
				env.SendEventBean(new SupportBean_S0(12));
				env.AssertPropsNew("s0", fields, new object[] { 12 });

				env.UndeployAll();
			}
		}

		private class EPLSubselectInSelectWhere : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select id in (select id from SupportBean_S1#length(1000) where id > 0) as value from SupportBean_S0";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S1(-1));
				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S0(-1));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S1(5));
				env.SendEventBean(new SupportBean_S0(4));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S0(5));
				env.AssertEqualsNew("s0", "value", true);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInSelectWhereExpressions : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select 3*id in (select 2*id from SupportBean_S1#length(1000)) as value from SupportBean_S0";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S1(-1));
				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S0(-1));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S1(6));
				env.SendEventBean(new SupportBean_S0(4));
				env.AssertEqualsNew("s0", "value", true);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInWildcard : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select s0.anyObject in (select * from SupportBean_S1#length(1000)) as value from SupportBeanArrayCollMap s0";
				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				var s1 = new SupportBean_S1(100);
				var arrayBean = new SupportBeanArrayCollMap(s1);
				env.SendEventBean(s1);
				env.SendEventBean(arrayBean);
				env.AssertEqualsNew("s0", "value", true);

				var s2 = new SupportBean_S2(100);
				arrayBean.AnyObject = s2;
				env.SendEventBean(s2);
				env.SendEventBean(arrayBean);
				env.AssertEqualsNew("s0", "value", false);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInNullable : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select id from SupportBean_S0 as s0 where p00 in (select p10 from SupportBean_S1#length(1000))";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				env.SendEventBean(new SupportBean_S0(1, "a"));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean_S0(2, null));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean_S1(-1, "A"));
				env.SendEventBean(new SupportBean_S0(3, null));
				env.AssertListenerNotInvoked("s0");

				env.SendEventBean(new SupportBean_S0(4, "A"));
				env.AssertEqualsNew("s0", "id", 4);

				env.SendEventBean(new SupportBean_S1(-2, null));
				env.SendEventBean(new SupportBean_S0(5, null));
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}
		}

		private class EPLSubselectInNullableCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select longBoxed from SupportBean(theString='A') as s0 " +
				               "where longBoxed in " +
				               "(select intBoxed from SupportBean(theString='B')#length(1000))";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				SendBean(env, "A", 0, 0L);
				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "B", null, null);

				SendBean(env, "A", 0, 0L);
				env.AssertListenerNotInvoked("s0");
				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "B", 99, null);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");
				SendBean(env, "A", null, 99L);
				env.AssertEqualsNew("s0", "longBoxed", 99L);

				SendBean(env, "B", 98, null);

				SendBean(env, "A", null, 98L);
				env.AssertEqualsNew("s0", "longBoxed", 98L);

				env.UndeployAll();
			}
		}

		private class EPLSubselectInNullRow : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select intBoxed from SupportBean(theString='A') as s0 " +
				               "where intBoxed in " +
				               "(select longBoxed from SupportBean(theString='B')#length(1000))";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				SendBean(env, "B", 1, 1L);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "A", 1, 1L);
				env.AssertEqualsNew("s0", "intBoxed", 1);

				SendBean(env, "B", null, null);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "A", 1, 1L);
				env.AssertEqualsNew("s0", "intBoxed", 1);

				env.UndeployAll();
			}
		}

		public class EPLSubselectInSingleIndex : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select (select p00 from SupportBean_S0#keepall() as s0 where s0.p01 in (s1.p10, s1.p11)) as c0 from SupportBean_S1 as s1";
				env.CompileDeploy(epl).AddListener("s0");

				for (var i = 0; i < 10; i++) {
					env.SendEventBean(new SupportBean_S0(i, "v" + i, "p00_" + i));
				}

				env.Milestone(0);

				for (var i = 0; i < 5; i++) {
					var index = i + 4;
					env.SendEventBean(new SupportBean_S1(index, "x", "p00_" + index));
					env.AssertEqualsNew("s0", "c0", "v" + index);
				}

				env.UndeployAll();
			}
		}

		public class EPLSubselectInMultiIndex : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select (select p00 from SupportBean_S0#keepall() as s0 where s1.p11 in (s0.p00, s0.p01)) as c0 from SupportBean_S1 as s1";
				env.CompileDeploy(epl).AddListener("s0");

				for (var i = 0; i < 10; i++) {
					env.SendEventBean(new SupportBean_S0(i, "v" + i, "p00_" + i));
				}

				env.Milestone(0);

				for (var i = 0; i < 5; i++) {
					var index = i + 4;
					env.SendEventBean(new SupportBean_S1(index, "x", "p00_" + index));
					env.AssertEqualsNew("s0", "c0", "v" + index);
				}

				env.UndeployAll();
			}
		}

		private class EPLSubselectNotInNullRow : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select intBoxed from SupportBean(theString='A') as s0 " +
				               "where intBoxed not in " +
				               "(select longBoxed from SupportBean(theString='B')#length(1000))";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				SendBean(env, "B", 1, 1L);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "A", 1, 1L);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "B", null, null);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "A", 1, 1L);
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}
		}

		private class EPLSubselectNotInSelect : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText =
					"@name('s0') select not id in (select id from SupportBean_S1#length(1000)) as value from SupportBean_S0";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", true);

				env.SendEventBean(new SupportBean_S1(-1));
				env.SendEventBean(new SupportBean_S0(2));
				env.AssertEqualsNew("s0", "value", true);

				env.SendEventBean(new SupportBean_S0(-1));
				env.AssertEqualsNew("s0", "value", false);

				env.SendEventBean(new SupportBean_S1(5));
				env.SendEventBean(new SupportBean_S0(4));
				env.AssertEqualsNew("s0", "value", true);

				env.SendEventBean(new SupportBean_S0(5));
				env.AssertEqualsNew("s0", "value", false);

				env.UndeployAll();
			}
		}

		private class EPLSubselectNotInNullableCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select longBoxed from SupportBean(theString='A') as s0 " +
				               "where longBoxed not in " +
				               "(select intBoxed from SupportBean(theString='B')#length(1000))";

				env.CompileDeployAddListenerMileZero(stmtText, "s0");

				SendBean(env, "A", 0, 0L);
				env.AssertEqualsNew("s0", "longBoxed", 0L);

				SendBean(env, "A", null, null);
				env.AssertEqualsNew("s0", "longBoxed", null);

				SendBean(env, "B", null, null);

				SendBean(env, "A", 1, 1L);
				env.AssertListenerNotInvoked("s0");
				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "B", 99, null);

				SendBean(env, "A", null, null);
				env.AssertListenerNotInvoked("s0");
				SendBean(env, "A", null, 99L);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "B", 98, null);

				SendBean(env, "A", null, 98L);
				env.AssertListenerNotInvoked("s0");

				SendBean(env, "A", null, 97L);
				env.AssertListenerNotInvoked("s0");

				env.UndeployAll();
			}
		}

		private static void RunTestInSelect(RegressionEnvironment env)
		{
			env.SendEventBean(new SupportBean_S0(2));
			env.AssertEqualsNew("s0", "value", false);

			env.SendEventBean(new SupportBean_S1(-1));
			env.SendEventBean(new SupportBean_S0(2));
			env.AssertEqualsNew("s0", "value", false);

			env.SendEventBean(new SupportBean_S0(-1));
			env.AssertEqualsNew("s0", "value", true);

			env.SendEventBean(new SupportBean_S1(5));
			env.SendEventBean(new SupportBean_S0(4));
			env.AssertEqualsNew("s0", "value", false);

			env.SendEventBean(new SupportBean_S0(5));
			env.AssertEqualsNew("s0", "value", true);
		}

		private class EPLSubselectInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				env.TryInvalidCompile(
					"@name('s0') select intArr in (select intPrimitive from SupportBean#keepall) as r1 from SupportBeanArrayCollMap",
					"Failed to validate select-clause expression subquery number 1 querying SupportBean: Collection or array comparison and null-type values are not allowed for the IN, ANY, SOME or ALL keywords");
			}
		}

		private static void SendBean(
			RegressionEnvironment env,
			string theString,
			int? intBoxed,
			long? longBoxed)
		{
			var bean = new SupportBean();
			bean.TheString = theString;
			bean.IntBoxed = intBoxed;
			bean.LongBoxed = longBoxed;
			env.SendEventBean(bean);
		}
	}
} // end of namespace
