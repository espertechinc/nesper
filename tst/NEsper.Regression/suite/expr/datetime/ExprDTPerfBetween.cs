///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.datetime
{
    public class ExprDTPerfBetween : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window AWindow#keepall as A", path);
            env.CompileDeploy("insert into AWindow select * from A", path);

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(SupportTimeStartEndA.Make("A" + i, "2002-05-30T09:00:00.000", 100), "A");
            }

            env.SendEventBean(SupportTimeStartEndA.Make("AEarlier", "2002-05-30T08:00:00.000", 100), "A");
            env.SendEventBean(SupportTimeStartEndA.Make("ALater", "2002-05-30T10:00:00.000", 100), "A");

            var epl =
                "@name('s0') select a.Key as c0 from SupportDateTime unidirectional, AWindow as a where LongDate.between(LongdateStart, LongdateEnd, false, true)";
            env.CompileDeploy(epl, path).AddListener("s0");

            // query
            var startTime = PerformanceObserver.MilliTime;
            for (var i = 0; i < 1000; i++) {
                env.SendEventBean(SupportDateTime.Make("2002-05-30T08:00:00.050"));
                env.AssertEqualsNew("s0", "c0", "AEarlier");
            }

            var endTime = PerformanceObserver.MilliTime;
            var delta = endTime - startTime;
            Assert.That(delta, Is.LessThan(500), "Delta=" + delta / 1000d);

            env.SendEventBean(SupportDateTime.Make("2002-05-30T10:00:00.050"));
            env.AssertEqualsNew("s0", "c0", "ALater");

            env.UndeployAll();
        }
    }
} // end of namespace