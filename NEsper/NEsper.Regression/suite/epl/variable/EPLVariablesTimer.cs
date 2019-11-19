///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.variable
{
    public class EPLVariablesTimer : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var startTime = PerformanceObserver.MilliTime;
            var stmtTextSet =
                "@Name('s0') on pattern [every timer:interval(100 milliseconds)] set var1 = current_timestamp, var2 = var1 + 1, var3 = var1 + var2";
            env.CompileDeploy(stmtTextSet).AddListener("s0");

            try {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException e) {
                Assert.Fail(e.Message);
            }

            var received = env.Listener("s0").NewDataListFlattened;
            Assert.IsTrue(received.Length >= 5, "received : " + received.Length);

            for (var i = 0; i < received.Length; i++) {
                var var1 = received[i].Get("var1").AsLong();
                var var2 = received[i].Get("var2").AsLong();
                var var3 = received[i].Get("var3").AsLong();
                Assert.IsTrue(var1 >= startTime);
                Assert.AreEqual(var1, var2 - 1);
                Assert.AreEqual(var3, var2 + var1);
            }

            env.UndeployAll();
        }
    }
} // end of namespace