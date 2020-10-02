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
			IList<RegressionExecution> execs = new List<RegressionExecution>();
			WithIsCoercion(execs);
			WithIsCoercionSameType(execs);
			WithIsMultikeyWArray(execs);
			WithInvalid(execs);
			return execs;
		}

		public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprCoreEqualsInvalid());
			return execs;
		}

		public static IList<RegressionExecution> WithIsMultikeyWArray(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprCoreEqualsIsMultikeyWArray());
			return execs;
		}

		public static IList<RegressionExecution> WithIsCoercionSameType(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprCoreEqualsIsCoercionSameType());
			return execs;
		}

		public static IList<RegressionExecution> WithIsCoercion(IList<RegressionExecution> execs = null)
		{
			execs = execs ?? new List<RegressionExecution>();
			execs.Add(new ExprCoreEqualsIsCoercion());
			return execs;
		}

		private class ExprCoreEqualsInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select IntOne=BooleanOne from SupportEventWithManyArray",
					"Failed to validate select-clause expression 'IntOne=BooleanOne': Cannot convert datatype 'System.Array' to a value that fits both type 'System.Int32[]' and type 'System.Boolean[]'");
					//"Failed to validate select-clause expression 'IntOne=BooleanOne': Implicit conversion from datatype 'System.Boolean[]' to 'System.Int32[]' is not allowed"

				SupportMessageAssertUtil.TryInvalidCompile(
					env,
					"select objectOne=BooleanOne from SupportEventWithManyArray",
					"skip");
			}
		}

		private class ExprCoreEqualsIsMultikeyWArray : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
				var builder = new SupportEvalBuilder("SupportEventWithManyArray")
					.WithExpression("c0", "IntOne=IntTwo")
					.WithExpression("c1", "IntOne is IntTwo")
					.WithExpression("c2", "IntBoxedOne=IntBoxedTwo")
					.WithExpression("c3", "IntBoxedOne is IntBoxedTwo")
					.WithExpression("c4", "Int2DimOne=Int2DimTwo")
					.WithExpression("c5", "Int2DimOne is Int2DimTwo")
					.WithExpression("c6", "ObjectOne=ObjectTwo")
					.WithExpression("c7", "ObjectOne is ObjectTwo");

				var array = new SupportEventWithManyArray("E1")
					.WithIntOne(new[] {1, 2})
					.WithIntTwo(new[] {1, 2})
					.WithIntBoxedOne(new int?[] {1, 2})
					.WithIntBoxedTwo(new int?[] {1, 2})
					.WithObjectOne(new object[] {'a', new object[] {1}})
					.WithObjectTwo(new object[] {'a', new object[] {1}})
					.WithInt2DimOne(new[] {new[] {1, 2}, new[] {3, 4}})
					.WithInt2DimTwo(new[] {new[] {1, 2}, new[] {3, 4}});
				builder.WithAssertion(array).Expect(fields, true, true, true, true, true, true, true, true);

				array = new SupportEventWithManyArray("E1")
					.WithIntOne(new[] {1, 2})
					.WithIntTwo(new[] {1})
					.WithIntBoxedOne(new int?[] {1, 2})
					.WithIntBoxedTwo(new int?[] {1})
					.WithObjectOne(new object[] {'a', 2})
					.WithObjectTwo(new object[] {'a'})
					.WithInt2DimOne(new[] {new[] {1, 2}, new[] {3, 4}})
					.WithInt2DimTwo(new[] {new[] {1, 2}, new[] {3}});
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
					.WithExpressions(fields, "IntPrimitive=LongPrimitive", "IntPrimitive is LongPrimitive");
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
					.WithExpressions(fields, "P00 = P01", "Id = Id", "P02 is not null");
				builder.WithAssertion(new SupportBean_S0(1, "a", "a", "a")).Expect(fields, true, true, true);
				builder.WithAssertion(new SupportBean_S0(1, "a", "b", null)).Expect(fields, false, true, false);
				builder.Run(env);
				env.UndeployAll();
			}
		}
	}
} // end of namespace
