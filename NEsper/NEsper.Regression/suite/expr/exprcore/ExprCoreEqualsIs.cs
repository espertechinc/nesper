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

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreEqualsIs
	{

		public static ICollection<RegressionExecution> Executions()
		{
			IList<RegressionExecution> executions = new List<RegressionExecution>();
			executions.Add(new ExprCoreEqualsIsCoercion());
			executions.Add(new ExprCoreEqualsIsCoercionSameType());
			executions.Add(new ExprCoreEqualsIsMultikeyWArray());
			executions.Add(new ExprCoreEqualsInvalid());
			return executions;
		}

		private class ExprCoreEqualsInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select intOne=booleanOne from SupportEventWithManyArray",
					"Failed to validate select-clause expression 'intOne=booleanOne': Implicit conversion from datatype 'boolean[]' to 'int[]' is not allowed");

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select objectOne=booleanOne from SupportEventWithManyArray",
					"skip");
			}
		}

		private class ExprCoreEqualsIsMultikeyWArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
				var builder = new SupportEvalBuilder("SupportEventWithManyArray")
					.WithExpression("c0", "intOne=intTwo")
					.WithExpression("c1", "intOne is intTwo")
					.WithExpression("c2", "IntBoxedOne=IntBoxedTwo")
					.WithExpression("c3", "IntBoxedOne is IntBoxedTwo")
					.WithExpression("c4", "int2DimOne=int2DimTwo")
					.WithExpression("c5", "int2DimOne is int2DimTwo")
					.WithExpression("c6", "objectOne=objectTwo")
					.WithExpression("c7", "objectOne is objectTwo");

				SupportEventWithManyArray array = new SupportEventWithManyArray("E1");
				array.WithIntOne(new[] {1, 2});
				array.WithIntTwo(new[] {1, 2});
				array.WithIntBoxedOne(new int?[] {1, 2});
				array.WithIntBoxedTwo(new int?[] {1, 2});
				array.WithObjectOne(new object[] {'a', new object[] {1}});
				array.WithObjectTwo(new object[] {'a', new object[] {1}});
				array.WithInt2DimOne(new[] {new[] {1, 2}, new[] {3, 4}});
				array.WithInt2DimTwo(new[] {new[] {1, 2}, new[] {3, 4}});
				builder.WithAssertion(array).Expect(fields, true, true, true, true, true, true, true, true);

				array = new SupportEventWithManyArray("E1");
				array.WithIntOne(new[] {1, 2});
				array.WithIntTwo(new[] {1});
				array.WithIntBoxedOne(new int?[] {1, 2});
				array.WithIntBoxedTwo(new int?[] {1});
				array.WithObjectOne(new object[] {'a', 2});
				array.WithObjectTwo(new object[] {'a'});
				array.WithInt2DimOne(new[] {new[] {1, 2}, new[] {3, 4}});
				array.WithInt2DimTwo(new[] { new[] {1, 2}, new[] {3}});
				builder.WithAssertion(array).Expect(fields, false, false, false, false, false, false, false, false);

				builder.Run(env);
				env.UndeployAll();
			}
		}

		private class ExprCoreEqualsIsCoercion : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean")
					.WithExpressions(fields, "IntPrimitive=longPrimitive", "IntPrimitive is LongPrimitive");
				builder.WithAssertion(MakeBean(1, 1L)).Expect(fields, true, true);
				builder.WithAssertion(MakeBean(1, 2L)).Expect(fields, false, false);
				builder.Run(env);
				env.UndeployAll();
			}

			private static SupportBean MakeBean(
				int intPrimitive,
				long longPrimitive)
			{
				var bean = new SupportBean();
				bean.IntPrimitive = intPrimitive;
				bean.LongPrimitive = longPrimitive;
				return bean;
			}
		}

		private class ExprCoreEqualsIsCoercionSameType : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2".SplitCsv();
				var builder = new SupportEvalBuilder("SupportBean_S0")
					.WithExpressions(fields, "p00 = p01", "id = id", "p02 is not null");
				builder.WithAssertion(new SupportBean_S0(1, "a", "a", "a")).Expect(fields, true, true, true);
				builder.WithAssertion(new SupportBean_S0(1, "a", "b", null)).Expect(fields, false, true, false);
				builder.Run(env);
				env.UndeployAll();
			}
		}
	}
} // end of namespace
