///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreBigNumberSupportMathContext
    {
        public static IList<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprCoreMathContextBigDecConvDivide());
            executions.Add(new ExprCoreMathContextDivide());
            return executions;
        }

        internal class ExprCoreMathContextBigDecConvDivide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select 10/5.0m as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new [] { "c0" };
                Assert.AreEqual(typeof(decimal), env.Statement("s0").EventType.GetPropertyType("c0"));

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2m});

                env.UndeployAll();
            }
        }

        internal class ExprCoreMathContextDivide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // cast and divide
                env.CompileDeploy("@Name('s0')  Select cast(1.6, BigDecimal) / cast(9.2, BigDecimal) from SupportBean")
                    .AddListener("s0");
                env.Statement("s0").SetSubscriber(new MySubscriber());
                env.SendEventBean(new SupportBean());
                env.UndeployAll();
            }
        }

        internal class MySubscriber
        {
            public void Update(decimal? value)
            {
                Assert.AreEqual(0.1739130m, value);
            }
        }
    }
} // end of namespace