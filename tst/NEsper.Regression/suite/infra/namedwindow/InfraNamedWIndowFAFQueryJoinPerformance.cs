///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWIndowFAFQueryJoinPerformance : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(
                RegressionFlag.EXCLUDEWHENINSTRUMENTED,
                RegressionFlag.PERFORMANCE,
                RegressionFlag.FIREANDFORGET);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window W1#unique(S1) as SupportSimpleBeanOne", path);
            env.CompileDeploy("insert into W1 select * from SupportSimpleBeanOne", path);

            env.CompileDeploy("@public create window W2#unique(S2) as SupportSimpleBeanTwo", path);
            env.CompileDeploy("insert into W2 select * from SupportSimpleBeanTwo", path);

            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(new SupportSimpleBeanOne("A" + i, 0, 0, 0));
                env.SendEventBean(new SupportSimpleBeanTwo("A" + i, 0, 0, 0));
            }

            var start = PerformanceObserver.MilliTime;
            var compiled = env.CompileFAF("select * from W1 as w1, W2 as w2 where w1.S1 = w2.S2", path);
            var prepared = env.Runtime.FireAndForgetService.PrepareQuery(compiled);
            for (var i = 0; i < 100; i++) {
                var result = prepared.Execute();
                ClassicAssert.AreEqual(1000, result.Array.Length);
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Console.Out.WriteLine("Delta=" + delta);
            Assert.That(delta, Is.LessThan(1000), "Delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace