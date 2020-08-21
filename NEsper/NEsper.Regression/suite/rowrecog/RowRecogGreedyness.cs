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
    public class RowRecogGreedyness
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new RowRecogReluctantZeroToOne());
            execs.Add(new RowRecogReluctantZeroToMany());
            execs.Add(new RowRecogReluctantOneToMany());
            return execs;
        }

        internal class RowRecogReluctantZeroToOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string " +
                           "  pattern (A?? B?) " +
                           "  define " +
                           "   A as A.Value = 1," +
                           "   B as B.Value = 1" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, "E1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, "E1"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogReluctantZeroToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a0","a1","a2","b","c" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                           "  pattern (A*? B? C) " +
                           "  define " +
                           "   A as A.Value = 1," +
                           "   B as B.Value in (1, 2)," +
                           "   C as C.Value = 3" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 1));
                env.SendEventBean(new SupportRecogBean("E4", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "E2", null, "E3", "E4"}});

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E11", 1));
                env.SendEventBean(new SupportRecogBean("E12", 1));
                env.SendEventBean(new SupportRecogBean("E13", 1));
                env.SendEventBean(new SupportRecogBean("E14", 1));
                env.SendEventBean(new SupportRecogBean("E15", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E11", "E12", "E13", "E14", "E15"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E16", 1));
                env.SendEventBean(new SupportRecogBean("E17", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, null, null, "E16", "E17"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E18", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, null, null, null, "E18"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogReluctantOneToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a0","a1","a2","b","c" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                           "  pattern (A+? B? C) " +
                           "  define " +
                           "   A as A.Value = 1," +
                           "   B as B.Value in (1, 2)," +
                           "   C as C.Value = 3" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 1));
                env.SendEventBean(new SupportRecogBean("E4", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "E2", null, "E3", "E4"}});

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E11", 1));
                env.SendEventBean(new SupportRecogBean("E12", 1));
                env.SendEventBean(new SupportRecogBean("E13", 1));
                env.SendEventBean(new SupportRecogBean("E14", 1));
                env.SendEventBean(new SupportRecogBean("E15", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E11", "E12", "E13", "E14", "E15"}});

                env.SendEventBean(new SupportRecogBean("E16", 1));
                env.SendEventBean(new SupportRecogBean("E17", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E16", null, null, null, "E17"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E18", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace