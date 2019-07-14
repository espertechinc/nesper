///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternDeadPattern : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var pattern = "select * from pattern[(SupportBean_A => SupportBean_B) and not SupportBean_C]";
            // Adjust to 20000 to better test the limit
            var compiled = env.Compile(pattern);
            for (var i = 0; i < 1000; i++) {
                env.Deploy(compiled);
            }

            env.SendEventBean(new SupportBean_C("C1"));

            var startTime = PerformanceObserver.MilliTime;
            env.SendEventBean(new SupportBean_A("A1"));
            var delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 20, "performance: delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace