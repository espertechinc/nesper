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

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.LambdaAssertionUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
	public class ExprEnumWhere
	{
		public static IList<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			WithEvents(execs);
			WithScalar(execs);
			WithScalarBoolean(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithScalarBoolean(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumWhereScalarBoolean());
			return execs;
		}

		public static IList<RegressionExecution> WithScalar(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumWhereScalar());
			return execs;
		}

		public static IList<RegressionExecution> WithEvents(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprEnumWhereEvents());
			return execs;
		}

		internal class ExprEnumWhereEvents : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean_ST0_Container");
				builder.WithExpression(fields[0], "Contained.where(x => P00 = 9)");
				builder.WithExpression(fields[1], "Contained.where((x, i) => x.P00 = 9 and i >= 1)");
				builder.WithExpression(fields[2], "Contained.where((x, i, s) => x.P00 = 9 and i >= 1 and s > 2)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<SupportBean_ST0>)));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1"))
					.Verify("c0", val => AssertST0Id(val, "E2"))
					.Verify("c1", val => AssertST0Id(val, "E2"))
					.Verify("c2", val => AssertST0Id(val, "E2"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,9", "E2,1", "E3,1"))
					.Verify("c0", val => AssertST0Id(val, "E1"))
					.Verify("c1", val => AssertST0Id(val, ""))
					.Verify("c2", val => AssertST0Id(val, ""));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,1", "E3,9"))
					.Verify("c0", val => AssertST0Id(val, "E3"))
					.Verify("c1", val => AssertST0Id(val, "E3"))
					.Verify("c2", val => AssertST0Id(val, "E3"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,9", "E3,9"))
					.Verify("c0", val => AssertST0Id(val, "E1,E3"))
					.Verify("c1", val => AssertST0Id(val, "E3"))
					.Verify("c2", val => AssertST0Id(val, ""));

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value("E1,1", "E2,9", "E3,1", "E4,9"))
					.Verify("c0", val => AssertST0Id(val, "E2,E4"))
					.Verify("c1", val => AssertST0Id(val, "E2,E4"))
					.Verify("c2", val => AssertST0Id(val, "E2,E4"));

				builder.WithAssertion(SupportBean_ST0_Container.Make2ValueNull())
					.Verify("c0", Assert.IsNull)
					.Verify("c1", Assert.IsNull)
					.Verify("c2", Assert.IsNull);

				builder.WithAssertion(SupportBean_ST0_Container.Make2Value())
					.Verify("c0", val => AssertST0Id(val, ""))
					.Verify("c1", val => AssertST0Id(val, ""))
					.Verify("c2", val => AssertST0Id(val, ""));

				builder.Run(env);
			}
		}

		internal class ExprEnumWhereScalar : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Strvals.where(x => x not like '%1%')");
				builder.WithExpression(fields[1], "Strvals.where((x, i) => x not like '%1%' and i >= 1)");
				builder.WithExpression(fields[2], "Strvals.where((x, i, s) => x not like '%1%' and i >= 1 and s >= 3)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<string>)));

				builder.WithAssertion(SupportCollection.MakeString("E1,E2,E3"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E2", "E3"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2", "E3"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E2", "E3"));

				builder.WithAssertion(SupportCollection.MakeString("E4,E2,E1"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E4", "E2"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c2", val => AssertValuesArrayScalar(val, "E2"));

				builder.WithAssertion(SupportCollection.MakeString(""))
					.Verify("c0", val => AssertValuesArrayScalar(val))
					.Verify("c1", val => AssertValuesArrayScalar(val))
					.Verify("c2", val => AssertValuesArrayScalar(val));

				builder.WithAssertion(SupportCollection.MakeString("E4,E2"))
					.Verify("c0", val => AssertValuesArrayScalar(val, "E4", "E2"))
					.Verify("c1", val => AssertValuesArrayScalar(val, "E2"))
					.Verify("c2", val => AssertValuesArrayScalar(val));

				builder.Run(env);
			}
		}

		internal class ExprEnumWhereScalarBoolean : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0".SplitCsv();
				var builder = new SupportEvalBuilder("SupportCollection");
				builder.WithExpression(fields[0], "Boolvals.where(x => x)");

				builder.WithStatementConsumer(stmt => SupportEventPropUtil.AssertTypesAllSame(stmt.EventType, fields, typeof(ICollection<bool>)));

				builder.WithAssertion(SupportCollection.MakeBoolean("true,true,false"))
					.Verify("c0", val => AssertValuesArrayScalar(val, true, true));

				builder.Run(env);
			}
		}
	}
} // end of namespace