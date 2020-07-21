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


using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumAllOfAnyOf
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumAllOfAnyOfEvents());
			execs.Add(new ExprEnumAllOfAnyOfScalar());
			return execs;
		}

		internal class ExprEnumAllOfAnyOfEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "contained.allof(v => p00 = 7)");
				builder.WithExpression(fields[1], "contained.anyof(v => p00 = 7)");
				builder.WithExpression(fields[2], "contained.allof((v, i) => p00 = (7 + i*10))");
				builder.WithExpression(fields[3], "contained.anyof((v, i) => p00 = (7 + i*10))");
				builder.WithExpression(fields[4], "contained.allof((v, i, s) => p00 = (7 + i*10 + s*100))");
				builder.WithExpression(fields[5], "contained.anyof((v, i, s) => p00 = (7 + i*10 + s*100))");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,7", "E3,2"))
					.Expect(fields, false, true, false, false, false, false);

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
					.Expect(fields, null, null, null, null, null, null);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,7", "E2,7", "E3,7"))
					.Expect(fields, true, true, false, true, false, false);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,0", "E2,0", "E3,0"))
					.Expect(fields, false, false, false, false, false, false);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
					.Expect(fields, true, false, true, false, true, false);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2", "E3,327"))
					.Expect(fields, false, false, false, false, false, true);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,307", "E2,317", "E3,327"))
					.Expect(fields, false, false, false, false, true, true);

				builder.Run(env);
			}
		}

		internal class ExprEnumAllOfAnyOfScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "strvals.allof(v => v='A')");
				builder.WithExpression(fields[1], "strvals.anyof(v => v='A')");
				builder.WithExpression(fields[2], "strvals.allof((v, i) => (v='A' and i < 2) or (v='C' and i >= 2))");
				builder.WithExpression(fields[3], "strvals.anyof((v, i) => (v='A' and i < 2) or (v='C' and i >= 2))");
				builder.WithExpression(fields[4], "strvals.allof((v, i, s) => (v='A' and i < s - 2) or (v='C' and i >= s - 2))");
				builder.WithExpression(fields[5], "strvals.anyof((v, i, s) => (v='A' and i < s - 2) or (v='C' and i >= s - 2))");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(bool?)));

				builder.WithAssertion(SupportCollection.MakeString("B,A,C"))
					.Expect(fields, false, true, false, true, false, true);

				builder.WithAssertion(SupportCollection.MakeString(null))
					.Expect(fields, null, null, null, null, null, null);

				builder.WithAssertion(SupportCollection.MakeString("A,A"))
					.Expect(fields, true, true, true, true, false, false);

				builder.WithAssertion(SupportCollection.MakeString("B"))
					.Expect(fields, false, false, false, false, false, false);

				builder.WithAssertion(SupportCollection.MakeString(""))
					.Expect(fields, true, false, true, false, true, false);

				builder.WithAssertion(SupportCollection.MakeString("B,B,B"))
					.Expect(fields, false, false, false, false, false, false);

				builder.WithAssertion(SupportCollection.MakeString("A,A,C,C"))
					.Expect(fields, false, true, true, true, true, true);

				builder.Run(env);
			}
		}
	}
} // end of namespace
