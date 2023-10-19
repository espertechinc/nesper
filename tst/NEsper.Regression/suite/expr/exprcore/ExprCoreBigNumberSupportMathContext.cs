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
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithBigDecConvDivide(execs);
            WithDivide(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDivide(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathContextDivide());
            return execs;
        }

        public static IList<RegressionExecution> WithBigDecConvDivide(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreMathContextBigDecConvDivide());
            return execs;
        }

        private class ExprCoreMathContextBigDecConvDivide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select 10/5.0m as c0 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "c0".SplitCsv();
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(decimal?), statement.EventType.GetPropertyType("c0")));

                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", fields, new object[] { 2.0m });

                env.UndeployAll();
            }
        }

        private class ExprCoreMathContextDivide : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // cast and divide
                env.CompileDeploy("@name('s0')  Select 1.6m / 9.2m from SupportBean").AddListener("s0");
                env.AssertStatement("s0", statement => { statement.SetSubscriber(new MySubscriber()); });
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