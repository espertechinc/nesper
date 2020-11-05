///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;
using static com.espertech.esper.regressionlib.support.bean.SupportBean_ST0_Container;
using static com.espertech.esper.regressionlib.support.bean.SupportCollection;
using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumArrayOf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfWSelectFromScalar());
			execs.Add(new ExprEnumArrayOfWSelectFromScalarWIndex());
			execs.Add(new ExprEnumArrayOfWSelectFromEvent());
			execs.Add(new ExprEnumArrayOfEvents());
			execs.Add(new ExprEnumArrayOfScalar());
			return execs;
		}

		private class ExprEnumArrayOfScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.arrayOf()");
				builder.WithExpression(fields[1], "Strvals.arrayOf(v => v)");
				builder.WithExpression(fields[2], "Strvals.arrayOf( (v, i) => v || '_' || Convert.ToString(i))");
				builder.WithExpression(fields[3], "Strvals.arrayOf( (v, i, s) => v || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string[])));

				builder.WithAssertion(SupportCollection.MakeString("A,B,C"))
					.Expect(fields, Csv("A,B,C"), Csv("A,B,C"), Csv("A_0,B_1,C_2"), Csv("A_0_3,B_1_3,C_2_3"));

				builder.WithAssertion(SupportCollection.MakeString(""))
					.Expect(fields, Csv(""), Csv(""), Csv(""), Csv(""));

				builder.WithAssertion(SupportCollection.MakeString("A"))
					.Expect(fields, Csv("A"), Csv("A"), Csv("A_0"), Csv("A_0_1"));

				builder.WithAssertion(SupportCollection.MakeString(null))
					.Expect(fields, null, null, null, null);

				builder.Run(env);
			}
		}

		private class ExprEnumArrayOfEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.arrayOf(x => x.P00)");
				builder.WithExpression(fields[1], "Contained.arrayOf((x, i) => x.P00 + i*10)");
				builder.WithExpression(fields[2], "Contained.arrayOf((x, i, s) => x.P00 + i*10 + s*100)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?[])));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E2,2"))
					.Expect(fields, IntArray(1, 9, 2), IntArray(1, 19, 22), IntArray(301, 319, 322));

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
					.Expect(fields, null, null, null);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
					.Expect(fields, IntArray(), IntArray(), IntArray());

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,9"))
					.Expect(fields, IntArray(9), IntArray(9), IntArray(109));

				builder.Run(env);
			}
		}

		private class ExprEnumArrayOfWSelectFromEvent : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.selectFrom(v => v.Id).arrayOf()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string[])));

				builder.WithAssertion(Make2Value("E1,12", "E2,11", "E3,2"))
					.Verify(fields[0], val => AssertArrayEquals(new[] {"E1", "E2", "E3"}, val));

				builder.WithAssertion(Make2Value("E4,14"))
					.Verify(fields[0], val => AssertArrayEquals(new[] {"E4"}, val));

				builder.WithAssertion(Make2Value())
					.Verify(fields[0], val => AssertArrayEquals(new string[0], val));

				builder.WithAssertion(Make2ValueNull())
					.Verify(fields[0], Assert.IsNull);

				builder.Run(env);
			}
		}

		private class ExprEnumArrayOfWSelectFromScalarWIndex : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.selectfrom((v, i) => v || '-' || Convert.ToString(i)).arrayOf()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string[])));

				builder.WithAssertion(MakeString("E1,E2,E3"))
					.Verify(fields[0], val => AssertArrayEquals(new[] {"E1-0", "E2-1", "E3-2"}, val));

				builder.WithAssertion(MakeString("E4"))
					.Verify(fields[0], val => AssertArrayEquals(new[] {"E4-0"}, val));

				builder.WithAssertion(MakeString(""))
					.Verify(fields[0], val => AssertArrayEquals(new string[0], val));

				builder.WithAssertion(MakeString(null))
					.Verify(fields[0], Assert.IsNull);

				builder.Run(env);
			}
		}

		private class ExprEnumArrayOfWSelectFromScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.selectfrom(v => Int32.Parse(v)).arrayOf()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?[])));

				builder.WithAssertion(MakeString("1,2,3"))
					.Verify(fields[0], val => AssertArrayEquals(new int?[] {1, 2, 3}, val));

				builder.WithAssertion(MakeString("1"))
					.Verify(fields[0], val => AssertArrayEquals(new int?[] {1}, val));

				builder.WithAssertion(MakeString(""))
					.Verify(fields[0], val => AssertArrayEquals(new int?[] { }, val));

				builder.WithAssertion(MakeString(null))
					.Verify(fields[0], Assert.IsNull);

				builder.Run(env);
			}
		}

		private static void AssertArrayEquals(
			string[] expected,
			object received)
		{
			AssertEqualsExactOrder(expected, (string[]) received);
		}

		private static void AssertArrayEquals(
			int?[] expected,
			object received)
		{
			AssertEqualsExactOrder(expected, (int?[]) received);
		}

		private static int[] IntArray(params int[] ints)
		{
			return ints;
		}

		private static string[] Csv(string csv)
		{
			if (string.IsNullOrWhiteSpace(csv)) {
				return new string[0];
			}

			return csv.SplitCsv();
		}
	}
} // end of namespace
