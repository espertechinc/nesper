///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;
using System.Text;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreRelOp : RegressionExecution
    {
        private static readonly string[] FIELDS = new [] { "c0", "c1", "c2", "c3" };

        public void Run(RegressionEnvironment env)
        {
            RunAssertion(
                env,
                "TheString",
                "'B'",
                bean => bean.TheString = "A",
                bean => bean.TheString = "B",
                bean => bean.TheString = "C");
            RunAssertion(
                env,
                "IntPrimitive",
                "2",
                bean => bean.IntPrimitive = 1,
                bean => bean.IntPrimitive = 2,
                bean => bean.IntPrimitive = 3);
            RunAssertion(
                env,
                "LongBoxed",
                "2L",
                bean => bean.LongBoxed = 1L,
                bean => bean.LongBoxed = 2L,
                bean => bean.LongBoxed = 3L);
            RunAssertion(
                env,
                "FloatPrimitive",
                "2f",
                bean => bean.FloatPrimitive = 1,
                bean => bean.FloatPrimitive = 2,
                bean => bean.FloatPrimitive = 3);
            RunAssertion(
                env,
                "DoublePrimitive",
                "2d",
                bean => bean.DoublePrimitive = 1,
                bean => bean.DoublePrimitive = 2,
                bean => bean.DoublePrimitive = 3);
            RunAssertion(
                env,
                "DecimalBoxed",
                "2.0m",
                bean => bean.DecimalBoxed = 1.0m,
                bean => bean.DecimalBoxed = 2.0m,
                bean => bean.DecimalBoxed = 3.0m);
            RunAssertion(
                env,
                "IntPrimitive",
                "2.0m",
                bean => bean.IntPrimitive = 1,
                bean => bean.IntPrimitive = 2,
                bean => bean.IntPrimitive = 3);
            RunAssertion(
                env,
                "BigInteger",
                "BigInteger.ValueOf(2)",
                bean => bean.BigInteger = new BigInteger(1),
                bean => bean.BigInteger = new BigInteger(2),
                bean => bean.BigInteger = new BigInteger(3));
            RunAssertion(
                env,
                "IntPrimitive",
                "BigInteger.ValueOf(2)",
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
            var writer = new StringBuilder();
            writer.Append("@Name('s0') select ");
            writer.Append(lhs).Append(">=").Append(rhs).Append(" as c0,");
            writer.Append(lhs).Append(">").Append(rhs).Append(" as c1,");
            writer.Append(lhs).Append("<=").Append(rhs).Append(" as c2,");
            writer.Append(lhs).Append("<").Append(rhs).Append(" as c3");
            writer.Append(" from SupportBean");

            env.CompileDeploy(writer.ToString()).AddListener("s0");

            SendAssert(env, one, FIELDS, false, false, true, true);
            SendAssert(env, two, FIELDS, true, false, true, false);
            SendAssert(env, three, FIELDS, true, true, false, false);

            env.UndeployAll();
        }

        private static void SendAssert(
            RegressionEnvironment env,
            Consumer<SupportBean> consumer,
            string[] fields,
            params object[] expected)
        {
            var bean = new SupportBean();
            consumer.Invoke(bean);
            env.SendEventBean(bean);
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
        }
    }
} // end of namespace