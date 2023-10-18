///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil; // assertStatelessStmt

namespace com.espertech.esper.regressionlib.suite.epl.database
{
	public class EPLDatabaseNoJoinIterate
	{

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLDatabaseExpressionPoll());
			execs.Add(new EPLDatabaseVariablesPoll());
			execs.Add(new EPLDatabaseNullSelect());
			execs.Add(new EPLDatabaseSubstitutionParameter());
			execs.Add(new EPLDatabaseSQLTextParamSubquery());
			return execs;
		}

		private class EPLDatabaseNullSelect : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select * from sql:MyDBPlain ['select null as a from mytesttable where myint = 1']";
				env.CompileDeploy(epl);

				env.AssertPropsPerRowIterator("s0", new string[] { "a" }, null);

				env.UndeployAll();
			}
		}

		private class EPLDatabaseExpressionPoll : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@public create variable boolean queryvar_bool", path);
				env.CompileDeploy("@public create variable int queryvar_int", path);
				env.CompileDeploy("@public create variable int lower", path);
				env.CompileDeploy("@public create variable int upper", path);
				env.CompileDeploy(
					"on SupportBean set queryvar_int=intPrimitive, queryvar_bool=boolPrimitive, lower=intPrimitive,upper=intBoxed",
					path);

				// Test int and singlerow
				var stmtText =
					"@name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where ${queryvar_int -2} = mytesttable.mybigint']";
				env.CompileDeploy(stmtText, path).AddListener("s0");
				AssertStatelessStmt(env, "s0", false);

				env.AssertPropsPerRowIteratorAnyOrder("s0", new string[] { "myint" }, null);

				SendSupportBeanEvent(env, 5);
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					new string[] { "myint" },
					new object[][] { new object[] { 30 } });

				env.AssertListenerNotInvoked("s0");
				env.UndeployModuleContaining("s0");

				// Test multi-parameter and multi-row
				stmtText =
					"@name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where mytesttable.mybigint between ${queryvar_int-2} and ${queryvar_int+2}'] order by myint";
				env.CompileDeploy(stmtText, path);
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					new string[] { "myint" },
					new object[][] {
						new object[] { 30 }, new object[] { 40 }, new object[] { 50 }, new object[] { 60 },
						new object[] { 70 }
					});
				env.UndeployAll();
			}
		}

		private class EPLDatabaseVariablesPoll : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy("@public create variable boolean queryvar_bool", path);
				env.CompileDeploy("@public create variable int queryvar_int", path);
				env.CompileDeploy("@public create variable int lower", path);
				env.CompileDeploy("@public create variable int upper", path);
				env.CompileDeploy(
					"on SupportBean set queryvar_int=intPrimitive, queryvar_bool=boolPrimitive, lower=intPrimitive,upper=intBoxed",
					path);

				// Test int and singlerow
				var stmtText =
					"@name('s0') select myint from sql:MyDBWithTxnIso1WithReadOnly ['select myint from mytesttable where ${queryvar_int} = mytesttable.mybigint']";
				env.CompileDeploy(stmtText, path).AddListener("s0");

				env.AssertPropsPerRowIteratorAnyOrder("s0", new string[] { "myint" }, null);

				SendSupportBeanEvent(env, 5);
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					new string[] { "myint" },
					new object[][] { new object[] { 50 } });

				env.AssertListenerNotInvoked("s0");
				env.UndeployModuleContaining("s0");

				// Test boolean and multirow
				stmtText =
					"@name('s0') select * from sql:MyDBWithTxnIso1WithReadOnly ['select mybigint, mybool from mytesttable where ${queryvar_bool} = mytesttable.mybool and myint between ${lower} and ${upper} order by mybigint']";
				env.CompileDeploy(stmtText, path).AddListener("s0");

				var fields = new string[] { "mybigint", "mybool" };
				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				SendSupportBeanEvent(env, true, 10, 40);
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					fields,
					new object[][] { new object[] { 1L, true }, new object[] { 4L, true } });

				SendSupportBeanEvent(env, false, 30, 80);
				env.AssertPropsPerRowIteratorAnyOrder(
					"s0",
					fields,
					new object[][]
						{ new object[] { 3L, false }, new object[] { 5L, false }, new object[] { 6L, false } });

				SendSupportBeanEvent(env, true, 20, 30);
				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				SendSupportBeanEvent(env, true, 20, 60);
				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { 4L, true } });

				env.UndeployAll();
			}
		}

		private class EPLDatabaseSubstitutionParameter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl =
					"@name('s0') select * from sql:MyDBPlain['select * from mytesttable where myint = ${?:myint:int}']";
				var model = env.EplToModel(epl);
				var compiledFromSODA = env.Compile(
					model,
					new CompilerArguments().SetConfiguration(env.Configuration));
				var compiled = env.Compile(epl);

				AssertDeploy(env, compiled, 10, "A");
				AssertDeploy(env, compiledFromSODA, 50, "E");
				AssertDeploy(env, compiled, 30, "C");
			}

			private void AssertDeploy(
				RegressionEnvironment env,
				EPCompiled compiled,
				int myint,
				string expected)
			{
				var values = new SupportPortableDeploySubstitutionParams().Add("myint", myint);
				var options =
					new DeploymentOptions().WithStatementSubstitutionParameter(values.SetStatementParameters);
				env.Deploy(compiled, options);

				env.AssertPropsPerRowIterator(
					"s0",
					new string[] { "myvarchar" },
					new object[][] { new object[] { expected } });

				env.UndeployAll();
			}
		}

		private class EPLDatabaseSQLTextParamSubquery : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var path = new RegressionPath();
				env.CompileDeploy(
					"@public create window MyWindow#lastevent as SupportBean;\n" +
					"on SupportBean merge MyWindow insert select *",
					path);

				var epl =
					"@name('s0') select * from sql:MyDBPlain['select * from mytesttable where myint = ${(select intPrimitive from MyWindow)}']";
				env.CompileDeploy(epl, path);

				env.AssertPropsPerRowIterator("s0", new string[] { "myvarchar" }, Array.Empty<object[]>());

				SendAssert(env, 30, "C");
				SendAssert(env, 10, "A");

				env.UndeployAll();
			}

			private void SendAssert(
				RegressionEnvironment env,
				int intPrimitive,
				string expected)
			{
				env.SendEventBean(new SupportBean("", intPrimitive));
				env.AssertPropsPerRowIterator(
					"s0",
					new string[] { "myvarchar" },
					new object[][] { new object[] { expected } });
			}
		}

		private static void SendSupportBeanEvent(
			RegressionEnvironment env,
			int intPrimitive)
		{
			var bean = new SupportBean();
			bean.IntPrimitive = intPrimitive;
			env.SendEventBean(bean);
		}

		private static void SendSupportBeanEvent(
			RegressionEnvironment env,
			bool boolPrimitive,
			int intPrimitive,
			int intBoxed)
		{
			var bean = new SupportBean();
			bean.BoolPrimitive = boolPrimitive;
			bean.IntPrimitive = intPrimitive;
			bean.IntBoxed = intBoxed;
			env.SendEventBean(bean);
		}
	}
} // end of namespace
