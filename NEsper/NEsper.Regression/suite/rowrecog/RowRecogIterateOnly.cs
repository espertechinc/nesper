///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogIterateOnly
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new RowRecogNoListenerMode());
            execs.Add(new RowRecogPrev());
            execs.Add(new RowRecogPrevPartitioned());
            return execs;
        }

        internal class RowRecogNoListenerMode : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a".SplitCsv();
                var text = "@Name('s0') @Hint('iterate_only') select * from SupportRecogBean#length(1) " +
                           "match_recognize (" +
                           "  measures A.TheString as a" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as SupportStaticMethodLib.sleepReturnTrue(mySleepDuration)" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                // this should not block
                var start = PerformanceObserver.MilliTime;
                for (var i = 0; i < 50; i++) {
                    env.SendEventBean(new SupportRecogBean("E1", 1));
                }

                var end = PerformanceObserver.MilliTime;
                Assert.IsTrue(end - start <= 100);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportRecogBean("E2", 2));
                env.Runtime.VariableService.SetVariableValue(null, "mySleepDuration", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a".SplitCsv();
                var text = "@Hint('iterate_only') @name('s0') select * from SupportRecogBean#lastevent " +
                           "match_recognize (" +
                           "  measures A.TheString as a" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as prev(A.value, 2) = value" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 3));
                env.SendEventBean(new SupportRecogBean("E4", 4));
                env.SendEventBean(new SupportRecogBean("E5", 2));
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E6", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E6"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E7"}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class RowRecogPrevPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,cat".SplitCsv();
                var text = "@Name('s0') @Hint('iterate_only') select * from SupportRecogBean#lastevent " +
                           "match_recognize (" +
                           "  partition by cat" +
                           "  measures A.TheString as a, A.cat as cat" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as prev(A.value, 2) = value" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", "A", 1));
                env.SendEventBean(new SupportRecogBean("E2", "B", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", "B", 3));
                env.SendEventBean(new SupportRecogBean("E4", "A", 4));
                env.SendEventBean(new SupportRecogBean("E5", "B", 2));
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E6", "A", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E6", "A"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", "B", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E7", "B"}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace