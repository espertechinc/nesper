///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogPerf : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text = "@Name('s0') select * from SupportRecogBean " +
                       "match_recognize (" +
                       "  partition by value " +
                       "  measures A.TheString as a_string, C.TheString as c_string " +
                       "  all matches " +
                       "  pattern (A B*? C) " +
                       "  define A as A.Cat = '1'," +
                       "         B as B.Cat = '2'," +
                       "         C as C.Cat = '3'" +
                       ")";
            // When testing aggregation:
            //"  measures A.string as a_string, count(B.string) as cntb, C.string as c_string " +

            env.CompileDeploy(text).AddListener("s0");

            var start = PerformanceObserver.MilliTime;

            for (var partition = 0; partition < 2; partition++) {
                env.SendEventBean(new SupportRecogBean("E1", "1", partition));
                for (var i = 0; i < 25000; i++) {
                    env.SendEventBean(new SupportRecogBean("E2_" + i, "2", partition));
                }

                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportRecogBean("E3", "3", partition));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            }

            var end = PerformanceObserver.MilliTime;
            var delta = end - start;
            Assert.IsTrue(delta < 2000, "delta=" + delta);

            env.UndeployAll();
        }
    }
} // end of namespace