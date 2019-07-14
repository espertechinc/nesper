///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterWhereClauseNoDataWindowPerformance : RegressionExecution
    {
        // Compares the performance of
        //     select * from SupportBean(TheString = 'xyz')
        //  against
        //     select * from SupportBean where theString = 'xyz'

        public void Run(RegressionEnvironment env)
        {
            var module = new StringWriter();

            for (var i = 0; i < 100; i++) {
                var epl = string.Format(
                    "@Name('s{0}') select * from SupportBean where theString = '{1}';\n",
                    i,
                    Convert.ToString(i));
                module.Write(epl);
            }

            var compiled = env.Compile(module.ToString());
            env.Deploy(compiled);

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < 10000; i++) {
                var bean = new SupportBean("NOMATCH", 0);
                env.SendEventBean(bean);
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace