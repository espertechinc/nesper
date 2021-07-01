///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.expreval;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreRelOp : RegressionExecution
	{
		public void Run(RegressionEnvironment env)
		{
			var bigInteger = typeof(BigIntegerHelper).FullName;
			
			RunAssertion(env, "TheString", "'B'", bean => bean.TheString = "A", bean => bean.TheString = "B", bean => bean.TheString = "C");
			RunAssertion(env, "IntPrimitive", "2", bean => bean.IntPrimitive = 1, bean => bean.IntPrimitive = 2, bean => bean.IntPrimitive = 3);
			RunAssertion(env, "LongBoxed", "2L", bean => bean.LongBoxed = 1L, bean => bean.LongBoxed = 2L, bean => bean.LongBoxed = 3L);
			RunAssertion(env, "FloatPrimitive", "2f", bean => bean.FloatPrimitive = 1, bean => bean.FloatPrimitive = 2, bean => bean.FloatPrimitive = 3);
			RunAssertion(env, "DoublePrimitive", "2d", bean => bean.DoublePrimitive = 1, bean => bean.DoublePrimitive = 2, bean => bean.DoublePrimitive = 3);
			RunAssertion(env, "DecimalPrimitive", "2m", bean => bean.DecimalPrimitive = 1, bean => bean.DecimalPrimitive = 2, bean => bean.DecimalPrimitive = 3);
			RunAssertion(
				env,
				"IntPrimitive",
				"2m",
				bean => bean.IntPrimitive = 1,
				bean => bean.IntPrimitive = 2,
				bean => bean.IntPrimitive = 3);
			RunAssertion(
				env,
				"BigInteger",
				$"{bigInteger}.ValueOf(2)",
				bean => bean.BigInteger = new BigInteger(1),
				bean => bean.BigInteger = new BigInteger(2),
				bean => bean.BigInteger = new BigInteger(3));
			RunAssertion(
				env,
				"IntPrimitive",
				$"{bigInteger}.ValueOf(2)",
				bean => bean.IntPrimitive = 1,
				bean => bean.IntPrimitive = 2,
				bean => bean.IntPrimitive = 3);
		}

		private static void RunAssertion(
			RegressionEnvironment env,
			string lhs,
			string rhs,
			Consumer<SupportBean> one,
			Consumer<SupportBean> two,
			Consumer<SupportBean> three)
		{
			var builder = new SupportEvalBuilder("SupportBean");
			var fields = "c0,c1,c2,c3".SplitCsv();
			builder.WithExpression(fields[0], lhs + ">=" + rhs);
			builder.WithExpression(fields[1], lhs + ">" + rhs);
			builder.WithExpression(fields[2], lhs + "<=" + rhs);
			builder.WithExpression(fields[3], lhs + "<" + rhs);

			var beanOne = new SupportBean();
			one.Invoke(beanOne);
			builder.WithAssertion(beanOne).Expect(fields, false, false, true, true);

			var beanTwo = new SupportBean();
			two.Invoke(beanTwo);
			builder.WithAssertion(beanTwo).Expect(fields, true, false, true, false);

			var beanThree = new SupportBean();
			three.Invoke(beanThree);
			builder.WithAssertion(beanThree).Expect(fields, true, true, false, false);

			builder.Run(env);
			env.UndeployAll();
		}
	}
} // end of namespace
