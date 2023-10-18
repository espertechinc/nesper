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
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.database
{
	public class EPLDatabase2StreamOuterJoin
	{
		private const string ALL_FIELDS =
			"mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";

		public static IList<RegressionExecution> Executions()
		{
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new EPLDatabaseOuterJoinLeftS0());
			execs.Add(new EPLDatabaseOuterJoinRightS1());
			execs.Add(new EPLDatabaseOuterJoinFullS0());
			execs.Add(new EPLDatabaseOuterJoinFullS1());
			execs.Add(new EPLDatabaseOuterJoinRightS0());
			execs.Add(new EPLDatabaseOuterJoinLeftS1());
			execs.Add(new EPLDatabaseLeftOuterJoinOnFilter());
			execs.Add(new EPLDatabaseRightOuterJoinOnFilter());
			execs.Add(new EPLDatabaseOuterJoinReversedOnFilter());
			return execs;
		}

		private class EPLDatabaseOuterJoinLeftS0 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               "SupportBean as s0 left outer join " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 on intPrimitive = mybigint";
				TryOuterJoinResult(env, stmtText);
			}
		}

		private class EPLDatabaseOuterJoinRightS1 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
				               "SupportBean as s0 on intPrimitive = mybigint";
				TryOuterJoinResult(env, stmtText);
			}
		}

		private class EPLDatabaseOuterJoinFullS0 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 full outer join " +
				               "SupportBean as s0 on intPrimitive = mybigint";
				TryOuterJoinResult(env, stmtText);
			}
		}

		private class EPLDatabaseOuterJoinFullS1 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               "SupportBean as s0 full outer join " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 on intPrimitive = mybigint";
				TryOuterJoinResult(env, stmtText);
			}
		}

		private class EPLDatabaseOuterJoinRightS0 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               "SupportBean as s0 right outer join " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 on intPrimitive = mybigint";
				TryOuterJoinNoResult(env, stmtText);
			}
		}

		private class EPLDatabaseOuterJoinLeftS1 : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 left outer join " +
				               "SupportBean as s0 on intPrimitive = mybigint";
				TryOuterJoinNoResult(env, stmtText);
			}
		}

		private class EPLDatabaseLeftOuterJoinOnFilter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "MyInt,myint".SplitCsv();
				var stmtText = "@name('s0') @IterableUnbound select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               "SupportBean as s0 " +
				               " left outer join " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 " +
				               "on theString = myvarchar";
				env.CompileDeploy(stmtText).AddListener("s0");

				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				// Result as the SQL query returns 1 row and therefore the on-clause filters it out, but because of left out still getting a row
				SendEvent(env, 1, "xxx");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(1, received.Get("MyInt"));
						AssertReceived(received, null, null, null, null, null, null, null, null, null);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 1, null } });

				// Result as the SQL query returns 0 rows
				SendEvent(env, -1, "xxx");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(-1, received.Get("MyInt"));
						AssertReceived(received, null, null, null, null, null, null, null, null, null);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { -1, null } });

				SendEvent(env, 2, "B");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(2, received.Get("MyInt"));
						AssertReceived(received, 2L, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 2, 20 } });

				env.UndeployAll();
			}
		}

		private class EPLDatabaseRightOuterJoinOnFilter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "MyInt,myint".SplitCsv();
				var stmtText = "@name('s0') @IterableUnbound select s0.intPrimitive as MyInt, " +
				               ALL_FIELDS +
				               " from " +
				               " sql:MyDBWithRetain ['select " +
				               ALL_FIELDS +
				               " from mytesttable where ${s0.intPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
				               "SupportBean as s0 on theString = myvarchar";
				env.CompileDeploy(stmtText).AddListener("s0");

				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				// No result as the SQL query returns 1 row and therefore the on-clause filters it out
				SendEvent(env, 1, "xxx");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(1, received.Get("MyInt"));
						AssertReceived(received, null, null, null, null, null, null, null, null, null);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 1, null } });

				// Result as the SQL query returns 0 rows
				SendEvent(env, -1, "xxx");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(-1, received.Get("MyInt"));
						AssertReceived(received, null, null, null, null, null, null, null, null, null);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { -1, null } });

				SendEvent(env, 2, "B");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(2, received.Get("MyInt"));
						AssertReceived(received, 2L, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 2, 20 } });

				env.UndeployAll();
			}
		}

		private class EPLDatabaseOuterJoinReversedOnFilter : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "MyInt,MyVarChar".SplitCsv();
				var stmtText = "@name('s0') select s0.intPrimitive as MyInt, MyVarChar from " +
				               "SupportBean#keepall as s0 " +
				               " right outer join " +
				               " sql:MyDBWithRetain ['select myvarchar MyVarChar from mytesttable'] as s1 " +
				               "on theString = MyVarChar";
				env.CompileDeploy(stmtText).AddListener("s0");

				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				// No result as the SQL query returns 1 row and therefore the on-clause filters it out
				SendEvent(env, 1, "xxx");
				env.AssertListenerNotInvoked("s0");
				env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

				SendEvent(env, -1, "A");
				env.AssertEventNew(
					"s0",
					received => {
						Assert.AreEqual(-1, received.Get("MyInt"));
						Assert.AreEqual("A", received.Get("MyVarChar"));
					});
				env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { -1, "A" } });

				env.UndeployAll();
			}
		}

		private static void TryOuterJoinNoResult(
			RegressionEnvironment env,
			string statementText)
		{
			env.CompileDeploy(statementText).AddListener("s0");

			SendEvent(env, 2);
			env.AssertEventNew(
				"s0",
				received => {
					Assert.AreEqual(2, received.Get("MyInt"));
					AssertReceived(received, 2L, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
				});

			SendEvent(env, 11);
			env.AssertListenerNotInvoked("s0");

			env.UndeployAll();
		}

		private static void TryOuterJoinResult(
			RegressionEnvironment env,
			string statementText)
		{
			env.CompileDeploy(statementText).AddListener("s0");

			SendEvent(env, 1);
			env.AssertEventNew(
				"s0",
				received => {
					Assert.AreEqual(1, received.Get("MyInt"));
					AssertReceived(received, 1L, 10, "A", "Z", true, 5000m, 100m, 1.2d, 1.3d);
				});

			SendEvent(env, 11);
			env.AssertEventNew(
				"s0",
				received => {
					Assert.AreEqual(11, received.Get("MyInt"));
					AssertReceived(received, null, null, null, null, null, null, null, null, null);
				});

			env.UndeployAll();
		}

		private static void AssertReceived(
			EventBean theEvent,
			long? mybigint,
			int? myint,
			string myvarchar,
			string mychar,
			bool? mybool,
			decimal? mynumeric,
			decimal? mydecimal,
			double? mydouble,
			double? myreal)
		{
			Assert.AreEqual(mybigint, theEvent.Get("mybigint"));
			Assert.AreEqual(myint, theEvent.Get("myint"));
			Assert.AreEqual(myvarchar, theEvent.Get("myvarchar"));
			Assert.AreEqual(mychar, theEvent.Get("mychar"));
			Assert.AreEqual(mybool, theEvent.Get("mybool"));
			Assert.AreEqual(mynumeric, theEvent.Get("mynumeric"));
			Assert.AreEqual(mydecimal, theEvent.Get("mydecimal"));
			Assert.AreEqual(mydouble, theEvent.Get("mydouble"));
			Assert.AreEqual(myreal, theEvent.Get("myreal"));
		}

		private static void SendEvent(
			RegressionEnvironment env,
			int intPrimitive)
		{
			var bean = new SupportBean();
			bean.IntPrimitive = intPrimitive;
			env.SendEventBean(bean);
		}

		private static void SendEvent(
			RegressionEnvironment env,
			int intPrimitive,
			string theString)
		{
			var bean = new SupportBean();
			bean.IntPrimitive = intPrimitive;
			bean.TheString = theString;
			env.SendEventBean(bean);
		}
	}
} // end of namespace
