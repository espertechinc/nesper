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
	public class ExprEnumMostLeastFrequent
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumMostLeastFreqEvents());
			execs.Add(new ExprEnumMostLeastFreqScalarNoParam());
			execs.Add(new ExprEnumMostLeastFreqScalar());
			return execs;
		}

		internal class ExprEnumMostLeastFreqEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.mostFrequent(x => P00)");
				builder.WithExpression(fields[1], "Contained.leastFrequent(x => P00)");
				builder.WithExpression(fields[2], "Contained.mostFrequent( (x, i) => P00 + i*2)");
				builder.WithExpression(fields[3], "Contained.leastFrequent( (x, i) => P00 + i*2)");
				builder.WithExpression(fields[4], "Contained.mostFrequent( (x, i, s) => P00 + i*2 + s*4)");
				builder.WithExpression(fields[5], "Contained.leastFrequent( (x, i, s) => P00 + i*2 + s*4)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

				SupportBean_ST0_Container bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12");
				builder.WithAssertion(bean).Expect(fields, 12, 11, 12, 12, 28, 28);

				bean = SupportBean_ST0_Container.Make2Value("E1,12");
				builder.WithAssertion(bean).Expect(fields, 12, 12, 12, 12, 16, 16);

				bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2", "E3,12", "E1,12", "E2,11", "E3,11");
				builder.WithAssertion(bean).Expect(fields, 12, 2, 12, 12, 40, 40);

				bean = SupportBean_ST0_Container.Make2Value("E2,11", "E1,12", "E2,15", "E3,12", "E1,12", "E2,11", "E3,11");
				builder.WithAssertion(bean).Expect(fields, 11, 15, 11, 11, 39, 39);

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull()).Expect(fields, null, null, null, null, null, null);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value()).Expect(fields, null, null, null, null, null, null);

				builder.Run(env);
			}
		}

		internal class ExprEnumMostLeastFreqScalarNoParam : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.mostFrequent()");
				builder.WithExpression(fields[1], "Strvals.leastFrequent()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(string)));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3")).Expect(fields, "E3", "E4");

				builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, "E1", "E1");

				builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null);

				builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null);

				builder.Run(env);
			}
		}

		internal class ExprEnumMostLeastFreqScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0,c1,c2,c3,c4,c5".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.mostFrequent(v => extractNum(v))");
				builder.WithExpression(fields[1], "Strvals.leastFrequent(v => extractNum(v))");
				builder.WithExpression(fields[2], "Strvals.mostFrequent( (v, i) => extractNum(v) + i*10)");
				builder.WithExpression(fields[3], "Strvals.leastFrequent( (v, i) => extractNum(v) + i*10)");
				builder.WithExpression(fields[4], "Strvals.mostFrequent( (v, i, s) => extractNum(v) + i*10 + s*100)");
				builder.WithExpression(fields[5], "Strvals.leastFrequent( (v, i, s) => extractNum(v) + i*10 + s*100)");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(int?)));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E2,E1,E3,E3,E4,E3")).Expect(fields, 3, 4, 2, 2, 802, 802);

				builder.WithAssertion(SupportCollection.MakeString("E1")).Expect(fields, 1, 1, 1, 1, 101, 101);

				builder.WithAssertion(SupportCollection.MakeString(null)).Expect(fields, null, null, null, null, null, null);

				builder.WithAssertion(SupportCollection.MakeString("")).Expect(fields, null, null, null, null, null, null);

				builder.Run(env);
			}
		}
	}
} // end of namespace
