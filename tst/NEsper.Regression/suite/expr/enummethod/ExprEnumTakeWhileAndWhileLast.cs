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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.expreval;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumTakeWhileAndWhileLast
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumTakeWhileEvents());
			execs.Add(new ExprEnumTakeWhileScalar());
			execs.Add(new ExprEnumTakeWhileInvalid());
			return execs;
		}

		internal class ExprEnumTakeWhileEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.takeWhile(x => x.P00 > 0)");
				builder.WithExpression(fields[1], "Contained.takeWhileLast(x => x.P00 > 0)");
				builder.WithExpression(fields[2], "Contained.takeWhile( (x, i) => x.P00 > 0 and i<2)");
				builder.WithExpression(fields[3], "Contained.takeWhileLast( (x, i) => x.P00 > 0 and i<2)");
				builder.WithExpression(fields[4], "Contained.takeWhile( (x, i, s) => x.P00 > 0 and i<s-2)");
				builder.WithExpression(fields[5], "Contained.takeWhileLast( (x, i,s) => x.P00 > 0 and i<s-2)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<SupportBean_ST0>)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,3"))
					.Verify("c0", val => AssertST0Id(val, "E1,E2,E3"))
					.Verify("c1", val => AssertST0Id(val, "E1,E2,E3"))
					.Verify("c2", val => AssertST0Id(val, "E1,E2"))
					.Verify("c3", val => AssertST0Id(val, "E2,E3"))
					.Verify("c4", val => AssertST0Id(val, "E1"))
					.Verify("c5", val => AssertST0Id(val, "E3"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,0", "E2,2", "E3,3"))
					.Verify("c0", val => AssertST0Id(val, ""))
					.Verify("c1", val => AssertST0Id(val, "E2,E3"))
					.Verify("c2", val => AssertST0Id(val, ""))
					.Verify("c3", val => AssertST0Id(val, "E2,E3"))
					.Verify("c4", val => AssertST0Id(val, ""))
					.Verify("c5", val => AssertST0Id(val, "E3"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,0", "E3,3"))
					.Verify("c0", val => AssertST0Id(val, "E1"))
					.Verify("c1", val => AssertST0Id(val, "E3"))
					.Verify("c2", val => AssertST0Id(val, "E1"))
					.Verify("c3", val => AssertST0Id(val, "E3"))
					.Verify("c4", val => AssertST0Id(val, "E1"))
					.Verify("c5", val => AssertST0Id(val, "E3"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,0"))
					.Verify("c0", val => AssertST0Id(val, "E1,E2"))
					.Verify("c1", val => AssertST0Id(val, ""))
					.Verify("c2", val => AssertST0Id(val, "E1,E2"))
					.Verify("c3", val => AssertST0Id(val, ""))
					.Verify("c4", val => AssertST0Id(val, "E1"))
					.Verify("c5", val => AssertST0Id(val, ""));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1"))
					.Verify("c0", val => AssertST0Id(val, "E1"))
					.Verify("c1", val => AssertST0Id(val, "E1"))
					.Verify("c2", val => AssertST0Id(val, "E1"))
					.Verify("c3", val => AssertST0Id(val, "E1"))
					.Verify("c4", val => AssertST0Id(val, ""))
					.Verify("c5", val => AssertST0Id(val, ""));

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

		internal class ExprEnumTakeWhileScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.takeWhile(x => x != 'E1')");
				builder.WithExpression(fields[1], "Strvals.takeWhileLast(x => x != 'E1')");
				builder.WithExpression(fields[2], "Strvals.takeWhile( (x, i) => x != 'E1' and i<2)");
				builder.WithExpression(fields[3], "Strvals.takeWhileLast( (x, i) => x != 'E1' and i<2)");
				builder.WithExpression(fields[4], "Strvals.takeWhile( (x, i, s) => x != 'E1' and i<s-2)");
				builder.WithExpression(fields[5], "Strvals.takeWhileLast( (x, i, s) => x != 'E1' and i<s-2)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<string>)));

				builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3,E4"))
					.Verify("c0", val => AssertValuesArrayScalar(val))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2", "E3", "E4"))
					.Verify("c2", val => AssertValuesArrayScalar(val))
					.Verify("c3", val => AssertValuesArrayScalar(val, "E3", "E4"))
					.Verify("c4", val => AssertValuesArrayScalar(val))
					.Verify("c5", val => AssertValuesArrayScalar(val, "E3", "E4"));

				builder.WithAssertion(SupportCollection.MakeString("E2"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c3", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c4", val => AssertValuesArrayScalar(val))
					.Verify("c5", val => AssertValuesArrayScalar(val));

				builder.WithAssertion(SupportCollection.MakeString("E2,E3,E4,E5"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E2", "E3", "E4", "E5"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2", "E3", "E4", "E5"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E2", "E3"))
					.Verify("c3", val => AssertValuesArrayScalar(val, "E4", "E5"))
					.Verify("c4", val => AssertValuesArrayScalar(val, "E2", "E3"))
					.Verify("c5", val => AssertValuesArrayScalar(val, "E4", "E5"));

				builder.Run(env);
			}
		}

		internal class ExprEnumTakeWhileInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string epl;

				epl = "select Strvals.takeWhile(x => null) from SupportCollection";
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					epl,
					"Failed to validate select-clause expression 'strvals.takeWhile()': Failed to validate enumeration method 'takeWhile', expected a non-null result for expression parameter 0 but received a null-typed expression");
			}
		}
	}
} // end of namespace
