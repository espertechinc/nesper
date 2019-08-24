///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.sales;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumChained : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var eplFragment =
                "@Name('s0') select Sales.where(x => x.Cost > 1000).min(y -> y.Buyer.Age) as val from PersonSales";
            env.CompileDeploy(eplFragment).AddListener("s0");

            LambdaAssertionUtil.AssertTypes(env.Statement("s0").EventType, new [] { "val" }, new[] {typeof(int?)});

            var bean = PersonSales.Make();
            env.SendEventBean(bean);
            Assert.AreEqual(50, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

            env.UndeployAll();
        }
    }
} // end of namespace