///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumNamedWindowPerformance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("create window Win#keepall as SupportBean", path);
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
            var epl = "@Name('s0') expression q {" +
                      "  x => (select * from Win where IntPrimitive = x.p00)" +
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
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                for (var j = 0; j < 10; j++) {
                    var coll = theEvent.Get("val" + j).Unwrap<SupportBean>();
                    Assert.AreEqual(1, coll.Count);
                    var bean = coll.First();
                    Assert.AreEqual("K50", bean.TheString);
                    Assert.AreEqual(1050, bean.IntPrimitive);
                }
            }

            var delta = PerformanceObserver.MilliTime - start;
            Assert.IsTrue(delta < 1000, "Delta = " + delta);

            env.UndeployModuleContaining("s0");
        }

        private void RunAssertionReuse(
            RegressionEnvironment env,
            RegressionPath path)
        {
            // test expression reuse
            var epl = "@Name('s0') expression q {" +
                      "  x => Win(TheString = x.key0).where(y => IntPrimitive = x.p00)" +
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
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                for (var j = 0; j < 10; j++) {
                    var coll = theEvent.Get("val" + j).Unwrap<SupportBean>();
                    Assert.AreEqual(1, coll.Count);
                    var bean = coll.First();
                    Assert.AreEqual("K50", bean.TheString);
                    Assert.AreEqual(1050, bean.IntPrimitive);
                }
            }

            var delta = PerformanceObserver.MilliTime - start;
            Assert.IsTrue(delta < 1000, "Delta = " + delta);

            // This will create a single dispatch
            // env.SendEventBean(new SupportBean("E1", 1));
            env.UndeployModuleContaining("s0");
        }
    }
} // end of namespace