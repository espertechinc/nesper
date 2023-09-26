///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework; // assertEquals

// assertTrue

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumNamedWindowPerformance : RegressionExecution
    {
        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED, RegressionFlag.PERFORMANCE);
        }

        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create window Win#keepall as SupportBean", path);
            env.CompileDeploy("insert into Win select * from SupportBean", path);

            // preload
            for (var i = 0; i < 10000; i++) {
                env.SendEventBean(new SupportBean("K" + i % 100, i));
            }

            RunAssertionReuse(env, path);

            RunAssertionSubquery(env, path);

            env.UndeployAll();
        }

        private void RunAssertionSubquery(
            RegressionEnvironment env,
            RegressionPath path)
        {
            // test expression reuse
            var epl = "@name('s0') expression q {" +
                      "  x => (select * from Win where intPrimitive = x.p00)" +
                      "}" +
                      "select " +
                      "q(st0).where(x => theString = key0) as val0, " +
                      "q(st0).where(x => theString = key0) as val1, " +
                      "q(st0).where(x => theString = key0) as val2, " +
                      "q(st0).where(x => theString = key0) as val3, " +
                      "q(st0).where(x => theString = key0) as val4, " +
                      "q(st0).where(x => theString = key0) as val5, " +
                      "q(st0).where(x => theString = key0) as val6, " +
                      "q(st0).where(x => theString = key0) as val7, " +
                      "q(st0).where(x => theString = key0) as val8, " +
                      "q(st0).where(x => theString = key0) as val9 " +
                      "from SupportBean_ST0 st0";
            env.CompileDeploy(epl, path).AddListener("s0");

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < 5000; i++) {
                env.SendEventBean(new SupportBean_ST0("ID", "K50", 1050));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var j = 0; j < 10; j++) {
                            var coll = @event.Get("val" + j).Unwrap<object>();
                            Assert.AreEqual(1, coll.Count);
                            var bean = (SupportBean)coll.First();
                            Assert.AreEqual("K50", bean.TheString);
                            Assert.AreEqual(1050, bean.IntPrimitive);
                        }
                    });
            }

            var delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(1000), "Delta = " + delta);

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionReuse(
            RegressionEnvironment env,
            RegressionPath path)
        {
            // test expression reuse
            var epl = "@name('s0') expression q {" +
                      "  x => Win(theString = x.key0).where(y => intPrimitive = x.p00)" +
                      "}" +
                      "select " +
                      "q(st0) as val0, " +
                      "q(st0) as val1, " +
                      "q(st0) as val2, " +
                      "q(st0) as val3, " +
                      "q(st0) as val4, " +
                      "q(st0) as val5, " +
                      "q(st0) as val6, " +
                      "q(st0) as val7, " +
                      "q(st0) as val8, " +
                      "q(st0) as val9 " +
                      "from SupportBean_ST0 st0";
            env.CompileDeploy(epl, path).AddListener("s0");

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < 5000; i++) {
                env.SendEventBean(new SupportBean_ST0("ID", "K50", 1050));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        for (var j = 0; j < 10; j++) {
                            var coll = @event.Get("val" + j).Unwrap<object>();
                            Assert.AreEqual(1, coll.Count);
                            var bean = (SupportBean)coll.First();
                            Assert.AreEqual("K50", bean.TheString);
                            Assert.AreEqual(1050, bean.IntPrimitive);
                        }
                    });
            }

            var delta = PerformanceObserver.MilliTime - start;
            Assert.That(delta, Is.LessThan(1000), "Delta = " + delta);

            // This will create a single dispatch
            // env.sendEventBean(new SupportBean("E1", 1));
            env.UndeployModuleContaining("s0");
        }
    }
} // end of namespace