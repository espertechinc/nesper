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
using com.espertech.esper.regressionlib.support.util;


namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumAggregate
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumAggregateEvents());
			execs.Add(new ExprEnumAggregateScalar());
			return execs;
		}

		internal class ExprEnumAggregateEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.aggregate(0, (result, item) => result + item.P00)");
				builder.WithExpression(fields[1], "Contained.aggregate('', (result, item) => result || ', ' || item.Id)");
				builder.WithExpression(fields[2], "Contained.aggregate('', (result, item) => result || (case when result='' then '' else ',' end) || item.Id)");
				builder.WithExpression(fields[3], "Contained.aggregate(0, (result, item, i) => result + item.P00 + i*10)");
				builder.WithExpression(fields[4], "Contained.aggregate(0, (result, item, i, s) => result + item.P00 + i*10 + s*100)");

				builder.WithStatementConsumer(
					stmt => LambdaAssertionUtil.AssertTypes(
						stmt.EventType,
						fields,
						new[] {typeof(int?), typeof(string), typeof(string), typeof(int?), typeof(int?)}));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"))
					.Expect(fields, 25, ", E1, E2, E2", "E1,E2,E2", 12 + 21 + 22, 312 + 321 + 322);

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
					.Expect(fields, null, null, null, null, null);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value(new string[0]))
					.Expect(fields, 0, "", "", 0, 0);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,12"))
					.Expect(fields, 12, ", E1", "E1", 12, 112);

				builder.Run(env);
			}
		}

		internal class ExprEnumAggregateScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.aggregate('', (result, item) => result || '+' || item)");
				builder.WithExpression(fields[1], "Strvals.aggregate('', (result, item, i) => result || '+' || item || '_' || Convert.ToString(i))");
				builder.WithExpression(
					fields[2],
					"Strvals.aggregate('', (result, item, i, s) => result || '+' || item || '_' || Convert.ToString(i) || '_' || Convert.ToString(s))");

				builder.WithStatementConsumer(stmt => LambdaAssertionUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

				builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
					.Expect(fields, "+E1+E2+E3", "+E1_0+E2_1+E3_2", "+E1_0_3+E2_1_3+E3_2_3");

				builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, "+E1", "+E1_0", "+E1_0_1");

				builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, "", "", "");

				builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null);

				builder.Run(env);
			}
		}
	}
} // end of namespace
