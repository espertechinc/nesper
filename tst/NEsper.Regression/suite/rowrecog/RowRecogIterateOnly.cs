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
using com.espertech.esper.regressionlib.support.rowrecog;

using static com.espertech.esper.regressionlib.framework.RegressionFlag; // PERFORMANCE
using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogIterateOnly
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNoListenerMode(execs);
            WithPrev(execs);
            WithPrevPartitioned(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithPrevPartitioned(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPrevPartitioned());
            return execs;
        }

        public static IList<RegressionExecution> WithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithNoListenerMode(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogNoListenerMode());
            return execs;
        }

        private class RowRecogNoListenerMode : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a".SplitCsv();
                var text = "@name('s0') @Hint('iterate_only') select * from SupportRecogBean#length(1) " +
                           "match_recognize (" +
                           "  measures A.TheString as a" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as SupportStaticMethodLib.SleepReturnTrue(mySleepDuration)" +
                           ")";
                env.CompileDeploy(text).AddListener("s0");

                // this should not block
                var start = PerformanceObserver.MilliTime;
                for (var i = 0; i < 50; i++) {
                    env.SendEventBean(new SupportRecogBean("E1", 1));
                }

                var end = PerformanceObserver.MilliTime;
                Assert.IsTrue((end - start) <= 100);
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E2", 2));
                env.RuntimeSetVariable(null, "mySleepDuration", 0);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(PERFORMANCE);
            }
        }

        private class RowRecogPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a".SplitCsv();
                var text = "@Hint('iterate_only') @name('s0') select * from SupportRecogBean#lastevent " +
                           "match_recognize (" +
                           "  measures A.TheString as a" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as prev(A.Value, 2) = Value" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 3));
                env.SendEventBean(new SupportRecogBean("E4", 4));
                env.SendEventBean(new SupportRecogBean("E5", 2));
                env.AssertIterator("s0", it => Assert.IsFalse(it.MoveNext()));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E6", 4));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 2));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" } });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class RowRecogPrevPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,cat".SplitCsv();
                var text = "@name('s0') @Hint('iterate_only') select * from SupportRecogBean#lastevent " +
                           "match_recognize (" +
                           "  partition by Cat" +
                           "  measures A.TheString as a, A.Cat as cat" +
                           "  all matches " +
                           "  pattern (A) " +
                           "  define A as prev(A.Value, 2) = Value" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", "A", 1));
                env.SendEventBean(new SupportRecogBean("E2", "B", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", "B", 3));
                env.SendEventBean(new SupportRecogBean("E4", "A", 4));
                env.SendEventBean(new SupportRecogBean("E5", "B", 2));
                env.AssertIterator("s0", it => Assert.IsFalse(it.MoveNext()));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E6", "A", 1));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6", "A" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", "B", 3));
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7", "B" } });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }
    }
} // end of namespace