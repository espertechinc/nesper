///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumAverage
	{
		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumAverageEvents());
			execs.Add(new ExprEnumAverageScalar());
			execs.Add(new ExprEnumAverageScalarMore());
			execs.Add(new ExprEnumAverageInvalid());
			return execs;
		}

		internal class ExprEnumAverageEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean_Container");
				builder.WithExpression(fields[0], "Beans.average(x => IntBoxed)");
				builder.WithExpression(fields[1], "Beans.average(x => DoubleBoxed)");
				builder.WithExpression(fields[2], "Beans.average(x => LongBoxed)");
				builder.WithExpression(fields[3], "Beans.average(x => DecimalBoxed)");
				builder.WithExpression(fields[4], "Beans.average( (x, i) => IntBoxed + i*10)");
				builder.WithExpression(fields[5], "Beans.average( (x, i) => DecimalBoxed + i*10)");
				builder.WithExpression(fields[6], "Beans.average( (x, i, s) => IntBoxed + i*10 + s*100)");
				builder.WithExpression(fields[7], "Beans.average( (x, i, s) => DecimalBoxed + i*10 + s*100)");
				builder.WithExpression(fields[8], "Beans.average( (x, i, s) => case when i = 1 then null else 2 end)");

				builder.WithStatementConsumer(
					stmt => SupportEventPropUtil.AssertTypes(
						stmt.EventType,
						fields,
						new[] {
							typeof(double?), typeof(double?), typeof(double?), typeof(decimal?), typeof(double?), typeof(decimal?), typeof(double?),
							typeof(decimal?), typeof(double?)
						}));

				builder.WithAssertion(new SupportBean_Container(null))
					.Expect(fields, null, null, null, null, null, null, null, null, null);

				builder.WithAssertion(new SupportBean_Container(EmptyList<SupportBean>.Instance))
					.Expect(fields, null, null, null, null, null, null, null, null, null);

				var listOne = new List<SupportBean>() {
					Make(2, 3d, 4L, 5)
				};
				builder.WithAssertion(new SupportBean_Container(listOne))
					.Expect(fields, 2d, 3d, 4d, 5.0m, 2d, 5.0m, 102d, 105.0m, 2.0d);

				var listTwo = new List<SupportBean>() {
					Make(2, 3d, 4L, 5),
					Make(4, 6d, 8L, 10)
				};
				builder.WithAssertion(new SupportBean_Container(listTwo))
					.Expect(
						fields,
						(2 + 4) / 2d,
						(3d + 6d) / 2d,
						(4L + 8L) / 2d,
						(5m + 10m) / 2m,
						(2 + 14) / 2d,
						(5m + 20m) / 2m,
						(202 + 214) / 2d,
						(205m + 220m) / 2m,
						2.0d);

				builder.Run(env);
			}
		}

		internal class ExprEnumAverageScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Intvals.average()");
				builder.WithExpression(fields[1], "Bdvals.average()");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypes(env.Statement("s0").EventType, fields, new[] {typeof(double?), typeof(decimal?)}));

				builder.WithAssertion(SupportCollection.MakeNumeric("1,2,3")).Expect(fields, 2d, 2m);

				builder.WithAssertion(SupportCollection.MakeNumeric("1,null,3")).Expect(fields, 2d, 2m);

				builder.WithAssertion(SupportCollection.MakeNumeric("4")).Expect(fields, 4d, 4m);

				builder.Run(env);
			}
		}

		internal class ExprEnumAverageScalarMore : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.average(v => extractNum(v))");
				builder.WithExpression(fields[1], "Strvals.average(v => extractDecimal(v))");
				builder.WithExpression(fields[2], "Strvals.average( (v, i) => extractNum(v) + i*10)");
				builder.WithExpression(fields[3], "Strvals.average( (v, i) => extractDecimal(v) + i*10)");
				builder.WithExpression(fields[4], "Strvals.average( (v, i, s) => extractNum(v) + i*10 + s*100)");
				builder.WithExpression(fields[5], "Strvals.average( (v, i, s) => extractDecimal(v) + i*10 + s*100)");
				builder.WithExpression(fields[6], "Strvals.average( (v, i, s) => case when i = 1 then null else 2 end)");

				builder.WithStatementConsumer(
					stmt => SupportEventPropUtil.AssertTypes(
						stmt.EventType,
						fields,
						new[] {typeof(double?), typeof(decimal?), typeof(double?), typeof(decimal?), typeof(double?), typeof(decimal?), typeof(double?)}));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
					.Expect(
						fields,
						(2 + 1 + 5 + 4) / 4d,
						(2 + 1 + 5 + 4) / 4m,
						(2 + 11 + 25 + 34) / 4d,
						(2 + 11 + 25 + 34) / 4m,
						(402 + 411 + 425 + 434) / 4d,
						(402 + 411 + 425 + 434) / 4m,
						2.0d);

				builder.WithAssertion(SupportCollection.MakeString("E1"))
					.Expect(fields, 1d, 1m, 1d, 1m, 101d, 101m, 2.0d);

				builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null, null, null, null);

				builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null, null, null, null, null, null);

				builder.Run(env);
			}
		}

		internal class ExprEnumAverageInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "select Strvals.average() from SupportCollection";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'Strvals.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of numeric values as input, received collection of System.String [select Strvals.average() from SupportCollection]");

				epl = "select Beans.average() from SupportBean_Container";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'Beans.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
					typeof(SupportBean).CleanName() +
					"'");
				
				epl = "select Strvals.average(v => null) from SupportCollection";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'Strvals.average()': Failed to validate enumeration method 'average', expected a non-null result for expression parameter 0 but received a null-typed expression");
			}
		}

		private static SupportBean Make(
			int? intBoxed,
			double? doubleBoxed,
			long? longBoxed,
			decimal? decimalBoxed)
		{
			var bean = new SupportBean();
			bean.IntBoxed = intBoxed;
			bean.DoubleBoxed = doubleBoxed;
			bean.LongBoxed = longBoxed;
			bean.DecimalBoxed = decimalBoxed;
			return bean;
		}
	}
} // end of namespace
