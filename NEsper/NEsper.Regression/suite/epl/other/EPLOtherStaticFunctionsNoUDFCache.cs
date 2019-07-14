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

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherStaticFunctionsNoUDFCache : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text =
                "@Name('s0') select SupportStaticMethodLib.sleep(100) as val from SupportTemperatureBean as temp";
            env.CompileDeploy(text).AddListener("s0");

            var startTime = PerformanceObserver.MilliTime;
            env.SendEventBean(new SupportTemperatureBean("a"));
            env.SendEventBean(new SupportTemperatureBean("a"));
            env.SendEventBean(new SupportTemperatureBean("a"));
            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;

            Assert.IsTrue(delta > 120, "Failed perf test, delta=" + delta);
            env.UndeployAll();

            // test plug-in single-row function
            var textSingleRow = "@Name('s0') select sleepme(100) as val from SupportTemperatureBean as temp";
            env.CompileDeploy(textSingleRow).AddListener("s0");

            startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(new SupportTemperatureBean("a"));
            }

            delta = PerformanceObserver.MilliTime - startTime;

            Assert.IsTrue(delta < 1000, "Failed perf test, delta=" + delta);
            env.UndeployAll();
        }
    }
} // end of namespace