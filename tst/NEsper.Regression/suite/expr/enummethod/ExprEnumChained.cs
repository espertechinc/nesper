///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.sales;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumChained : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var eplFragment =
                "@name('s0') select Sales.where(x -> x.Cost > 1000).min(y -> y.Buyer.Age) as val from PersonSales";
            env.CompileDeploy(eplFragment).AddListener("s0");
            env.AssertStmtTypes("s0", new[] { "val" }, new[] { typeof(int?) });

            var bean = PersonSales.Make();
            env.SendEventBean(bean);
            env.AssertEqualsNew("s0", "val", 50);

            env.UndeployAll();
        }
    }
} // end of namespace