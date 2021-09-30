///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
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
			WithEnumArrayOfWSelectFromScalar(execs);
			WithEnumArrayOfWSelectFromScalarWIndex(execs);
			WithEnumArrayOfWSelectFromEvent(execs);
			WithEnumArrayOfEvents(execs);
			WithEnumArrayOfScalar(execs);
			WithArrayOfInvalid(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithArrayOfInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprArrayOfInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithEnumArrayOfScalar(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfScalar());
			return execs;
		}

		public static IList<RegressionExecution> WithEnumArrayOfEvents(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfEvents());
			return execs;
		}

		public static IList<RegressionExecution> WithEnumArrayOfWSelectFromEvent(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfWSelectFromEvent());
			return execs;
		}

		public static IList<RegressionExecution> WithEnumArrayOfWSelectFromScalarWIndex(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfWSelectFromScalarWIndex());
			return execs;
		}

		public static IList<RegressionExecution> WithEnumArrayOfWSelectFromScalar(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumArrayOfWSelectFromScalar());
			return execs;
		}

		private class ExprEnumArrayOfScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.arrayOf()");
				builder.WithExpression(fields[1], "Strvals.arrayOf(v => v)");
				builder.WithExpression(fields[2], "Strvals.arrayOf( (v, i) => v || '_' || Convert.ToString(i))");
				builder.WithExpression(fields[3], "Strvals.arrayOf( (v, i, s) => v || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");
				builder.WithExpression(fields[4], "Strvals.arrayOf( (v, i) => i)");

				builder.WithStatementConsumer(
					stmt => SupportEventPropUtil.AssertTypes(
						env.Statement("s0").EventType,
						fields,
						new Type[] {
							typeof(string[]),
							typeof(string[]),
							typeof(string[]),
							typeof(string[]),
							typeof(int[])
						}));

				builder.WithAssertion(SupportCollection.MakeString("A,B,C"))
					.Expect(fields, Csv("A,B,C"), Csv("A,B,C"), Csv("A_0,B_1,C_2"), Csv("A_0_3,B_1_3,C_2_3"), new int[] {0, 1, 2});

				builder.WithAssertion(SupportCollection.MakeString(""))
					.Expect(fields, Csv(""), Csv(""), Csv(""), Csv(""), new int[] { });

				builder.WithAssertion(SupportCollection.MakeString("A"))
					.Expect(fields, Csv("A"), Csv("A"), Csv("A_0"), Csv("A_0_1"), new int[] {0});

				builder.WithAssertion(SupportCollection.MakeString(null))
					.Expect(fields, null, null, null, null, null);

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

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(int?[])));

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

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(string[])));

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

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(string[])));

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

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(int?[])));

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

		private class ExprArrayOfInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "select strvals.arrayOf(v => null) from SupportCollection";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'strvals.arrayOf()': Null-type is not allowed");
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
