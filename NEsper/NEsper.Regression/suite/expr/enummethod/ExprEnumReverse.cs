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
	public class ExprEnumReverse
	{

		public static ICollection<RegressionExecution> Executions()
		{
			List<RegressionExecution> execs = new List<RegressionExecution>();
			execs.Add(new ExprEnumReverseEvents());
			execs.Add(new ExprEnumReverseScalar());
			return execs;
		}

		internal class ExprEnumReverseEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "contained.reverse()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<object>)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"))
					.Verify("c0", val => AssertST0Id(val, "E3,E2,E1"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E2,9", "E1,1"))
					.Verify("c0", val => AssertST0Id(val, "E1,E2"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1"))
					.Verify("c0", val => AssertST0Id(val, "E1"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
					.Verify("c0", val => AssertST0Id(val, null));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
					.Verify("c0", val => AssertST0Id(val, ""));

				builder.Run(env);
			}
		}

		internal class ExprEnumReverseScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				string[] fields = "c0".SplitCsv();
				SupportEvalBuilder builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "strvals.reverse()");

				builder.WithStatementConsumer(stmt => AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<object>)));

				builder.WithAssertion(SupportCollection.MakeString("E2,E1,E5,E4"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E4", "E5", "E1", "E2"));

				LambdaAssertionUtil.AssertSingleAndEmptySupportColl(builder, fields);

				builder.Run(env);
			}
		}
	}
} // end of namespace
