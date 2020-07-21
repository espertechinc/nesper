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


using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumTakeAndTakeLast
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumTakeEvents());
			execs.Add(new ExprEnumTakeScalar());
			return execs;
		}

		internal class ExprEnumTakeEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "contained.take(2)");
				builder.WithExpression(fields[1], "contained.take(1)");
				builder.WithExpression(fields[2], "contained.take(0)");
				builder.WithExpression(fields[3], "contained.take(-1)");
				builder.WithExpression(fields[4], "contained.takeLast(2)");
				builder.WithExpression(fields[5], "contained.takeLast(1)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<object>)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"))
					.Verify("c0", val => AssertST0Id(val, "E1,E2"))
					.Verify("c1", val => AssertST0Id(val, "E1"))
					.Verify("c2", val => AssertST0Id(val, ""))
					.Verify("c3", val => AssertST0Id(val, ""))
					.Verify("c4", val => AssertST0Id(val, "E2,E3"))
					.Verify("c5", val => AssertST0Id(val, "E3"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"))
					.Verify("c0", val => AssertST0Id(val, "E1,E2"))
					.Verify("c1", val => AssertST0Id(val, "E1"))
					.Verify("c2", val => AssertST0Id(val, ""))
					.Verify("c3", val => AssertST0Id(val, ""))
					.Verify("c4", val => AssertST0Id(val, "E1,E2"))
					.Verify("c5", val => AssertST0Id(val, "E2"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1"))
					.Verify("c0", val => AssertST0Id(val, "E1"))
					.Verify("c1", val => AssertST0Id(val, "E1"))
					.Verify("c2", val => AssertST0Id(val, ""))
					.Verify("c3", val => AssertST0Id(val, ""))
					.Verify("c4", val => AssertST0Id(val, "E1"))
					.Verify("c5", val => AssertST0Id(val, "E1"));

				SupportEvalAssertionBuilder assertionEmpty = builder.WithAssertion(SupportBean_ST0_Container.Make2Value());
				foreach (string field in fields) {
					assertionEmpty.Verify(field, val => AssertST0Id(val, ""));
				}

				SupportEvalAssertionBuilder assertionNull = builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull());
				foreach (string field in fields) {
					assertionNull.Verify(field, val => AssertST0Id(val, null));
				}

				builder.Run(env);
			}
		}

		internal class ExprEnumTakeScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "strvals.take(2)");
				builder.WithExpression(fields[1], "strvals.take(1)");
				builder.WithExpression(fields[2], "strvals.takeLast(2)");
				builder.WithExpression(fields[3], "strvals.takeLast(1)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<object>)));

				builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E1"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E2", "E3"))
					.Verify("c3", val => AssertValuesArrayScalar(val, "E3"));

				builder.WithAssertion(SupportCollection.MakeString("E1,E2"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E1", "E2"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E1"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E1", "E2"))
					.Verify("c3", val => AssertValuesArrayScalar(val, "E2"));

				AssertSingleAndEmptySupportColl(builder, fields);

				builder.Run(env);
			}
		}
	}
} // end of namespace
