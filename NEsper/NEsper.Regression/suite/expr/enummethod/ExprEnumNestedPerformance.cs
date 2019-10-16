///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumNestedPerformance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            IList<SupportBean_ST0> list = new List<SupportBean_ST0>();
            for (var i = 0; i < 10000; i++) {
                list.Add(new SupportBean_ST0("E1", 1000));
            }

            var minEvent = new SupportBean_ST0("E2", 5);
            list.Add(minEvent);
            var theEvent = new SupportBean_ST0_Container(list);

            // the "contained.min" inner lambda only depends on values within "contained" (a stream's value)
            // and not on the particular "x".
            var eplFragment =
                "@Name('s0') select Contained.where(x => x.P00 = Contained.min(y -> y.P00)) as val from SupportBean_ST0_Container";
            env.CompileDeploy(eplFragment).AddListener("s0");

            var start = PerformanceObserver.MilliTime;
            env.SendEventBean(theEvent);
            var delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(100), "delta=" + delta);

            var result = env.Listener("s0")
                .AssertOneGetNewAndReset()
                .Get("val")
                .UnwrapIntoArray<SupportBean_ST0>();
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {minEvent}, result);

            env.UndeployAll();
        }
    }
} // end of namespace