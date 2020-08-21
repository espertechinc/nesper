///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumSumOf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumSumEvents());
			execs.Add(new ExprEnumSumEventsPlus());
			execs.Add(new ExprEnumSumScalar());
			execs.Add(new ExprEnumSumScalarStringValue());
			execs.Add(new ExprEnumInvalid());
			execs.Add(new ExprEnumSumArray());
			return execs;
		}

		internal class ExprEnumSumArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean");
				builder.WithExpression(fields[0], "{1d, 2d}.sumOf()");
				builder.WithExpression(fields[1], "{BigInteger.valueOf(1), BigInteger.valueOf(2)}.sumOf()");
				builder.WithExpression(fields[2], "{1L, 2L}.sumOf()");
				builder.WithExpression(fields[3], "{1L, 2L, null}.sumOf()");

				builder.WithAssertion(new SupportBean()).Expect(fields, 3d, new BigInteger(3), 3L, 3L);

				builder.Run(env);
			}
		}

		internal class ExprEnumSumEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_Container");
				builder.WithExpression(fields[0], "beans.sumOf(x => IntBoxed)");
				builder.WithExpression(fields[1], "beans.sumOf(x => DoubleBoxed)");
				builder.WithExpression(fields[2], "beans.sumOf(x => LongBoxed)");
				builder.WithExpression(fields[3], "beans.sumOf(x => DecimalBoxed)");
				builder.WithExpression(fields[4], "beans.sumOf(x => BigInteger)");

				builder.WithStatementConsumer(
					stmt => AssertTypes(
						stmt.EventType,
						fields,
						new[] {typeof(int?), typeof(double?), typeof(long?), typeof(decimal?), typeof(BigInteger)}));

				builder.WithAssertion(new SupportBean_Container(null)).Expect(fields, null, null, null, null, null);

				builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance)).Expect(fields, null, null, null, null, null);

				IList<SupportBean> listOne = new List<SupportBean>() {Make(2, 3d, 4L, 5, 6)};
				builder.WithAssertion(new SupportBean_Container(listOne)).Expect(fields, 2, 3d, 4L, 5m, new BigInteger(6));

				IList<SupportBean> listTwo = new List<SupportBean>() {Make(2, 3d, 4L, 5, 6), Make(4, 6d, 8L, 10, 12)};
				builder.WithAssertion(new SupportBean_Container(listTwo)).Expect(fields, 2 + 4, 3d + 6d, 4L + 8L, 5 + 10, new BigInteger(18));

				builder.Run(env);
			}
		}

		internal class ExprEnumSumEventsPlus : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_Container");
				builder.WithExpression(fields[0], "beans.sumOf(x => IntBoxed)");
				builder.WithExpression(fields[1], "beans.sumOf( (x, i) => IntBoxed + i*10)");
				builder.WithExpression(fields[2], "beans.sumOf( (x, i, s) => IntBoxed + i*10 + s*100)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

				builder.WithAssertion(new SupportBean_Container(null)).Expect(fields, null, null, null);

				builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance)).Expect(fields, null, null, null);

				IList<SupportBean> listOne = new List<SupportBean>() { MakeSB("E1", 10) };
				builder.WithAssertion(new SupportBean_Container(listOne)).Expect(fields, 10, 10, 110);

				IList<SupportBean> listTwo = new List<SupportBean>() {MakeSB("E1", 10), MakeSB("E2", 11)};
				builder.WithAssertion(new SupportBean_Container(listTwo)).Expect(fields, 21, 31, 431);

				builder.Run(env);
			}

			private SupportBean MakeSB(
				string theString,
				int intBoxed)
			{
				SupportBean bean = new SupportBean(theString, intBoxed);
				bean.IntBoxed = intBoxed;
				return bean;
			}
		}

		internal class ExprEnumSumScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "intvals.sumOf()");
				builder.WithExpression(fields[1], "bdvals.sumOf()");

				builder.WithStatementConsumer(stmt => AssertTypes(stmt.EventType, fields, new[] {typeof(int?), typeof(decimal?)}));

				builder.WithAssertion(SupportCollection.MakeNumeric("1,4,5")).Expect(fields, 1 + 4 + 5, 1 + 4 + 5);

				builder.WithAssertion(SupportCollection.MakeNumeric("3,4")).Expect(fields, 3 + 4, 3 + 4);

				builder.WithAssertion(SupportCollection.MakeNumeric("3")).Expect(fields, 3, 3m);

				builder.WithAssertion(SupportCollection.MakeNumeric("")).Expect(fields, null, null);

				builder.WithAssertion(SupportCollection.MakeNumeric(null)).Expect(fields, null, null);

				builder.Run(env);
			}
		}

		internal class ExprEnumSumScalarStringValue : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "strvals.sumOf(v => extractNum(v))");
				builder.WithExpression(fields[1], "strvals.sumOf(v => extractDecimal(v))");
				builder.WithExpression(fields[2], "strvals.sumOf( (v, i) => extractNum(v) + i*10)");
				builder.WithExpression(fields[3], "strvals.sumOf( (v, i, s) => extractNum(v) + i*10 + s*100)");

				builder.WithStatementConsumer(
					stmt => AssertTypes(env.Statement("s0").EventType, fields, new[] {typeof(int?), typeof(decimal?), typeof(int?), typeof(int?)}));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
					.Expect(fields, 2 + 1 + 5 + 4, 2 + 1 + 5 + 4, 2 + 11 + 25 + 34, 402 + 411 + 425 + 434);

				builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, 1, 1m, 1, 101);

				builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null);

				builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null, null, null);

				builder.Run(env);
			}
		}

		internal class ExprEnumInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "select beans.sumof() from SupportBean_Container";
				TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'beans.sumof()': Invalid input for built-in enumeration method 'sumof' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '");
			}
		}

		private static SupportBean Make(
			int? intBoxed,
			Double doubleBoxed,
			long? longBoxed,
			decimal? decimalBoxed,
			int bigInteger)
		{
			SupportBean bean = new SupportBean();
			bean.IntBoxed = intBoxed;
			bean.DoubleBoxed = doubleBoxed;
			bean.LongBoxed = longBoxed;
			bean.DecimalBoxed = decimalBoxed;
			bean.BigInteger = new BigInteger(bigInteger);
			return bean;
		}
	}
} // end of namespace
