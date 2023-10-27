///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogGreedyness
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithZeroToOne(execs);
            WithZeroToMany(execs);
            WithOneToMany(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOneToMany(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogReluctantOneToMany());
            return execs;
        }

        public static IList<RegressionExecution> WithZeroToMany(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogReluctantZeroToMany());
            return execs;
        }

        public static IList<RegressionExecution> WithZeroToOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogReluctantZeroToOne());
            return execs;
        }

        private class RowRecogReluctantZeroToOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string " +
                           "  pattern (A?? B?) " +
                           "  define " +
                           "   A as A.value = 1," +
                           "   B as B.value = 1" +
                           ")";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "E1" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "E1" } });

                env.Milestone(0);

                env.UndeployAll();
            }
        }

        private class RowRecogReluctantZeroToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a0,a1,a2,b,c".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                           "  pattern (A*? B? C) " +
                           "  define " +
                           "   A as A.value = 1," +
                           "   B as B.value in (1, 2)," +
                           "   C as C.value = 3" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 1));
                env.SendEventBean(new SupportRecogBean("E4", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E2", null, "E3", "E4" } });

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E11", 1));
                env.SendEventBean(new SupportRecogBean("E12", 1));
                env.SendEventBean(new SupportRecogBean("E13", 1));
                env.SendEventBean(new SupportRecogBean("E14", 1));
                env.SendEventBean(new SupportRecogBean("E15", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E11", "E12", "E13", "E14", "E15" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E16", 1));
                env.SendEventBean(new SupportRecogBean("E17", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, null, null, "E16", "E17" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E18", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, null, null, null, "E18" } });

                env.UndeployAll();
            }
        }

        private class RowRecogReluctantOneToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a0,a1,a2,b,c".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, A[1].TheString as a1, A[2].TheString as a2, B.TheString as b, C.TheString as c" +
                           "  pattern (A+? B? C) " +
                           "  define " +
                           "   A as A.value = 1," +
                           "   B as B.value in (1, 2)," +
                           "   C as C.value = 3" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 1));
                env.SendEventBean(new SupportRecogBean("E4", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E2", null, "E3", "E4" } });

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E11", 1));
                env.SendEventBean(new SupportRecogBean("E12", 1));
                env.SendEventBean(new SupportRecogBean("E13", 1));
                env.SendEventBean(new SupportRecogBean("E14", 1));
                env.SendEventBean(new SupportRecogBean("E15", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E11", "E12", "E13", "E14", "E15" } });

                env.SendEventBean(new SupportRecogBean("E16", 1));
                env.SendEventBean(new SupportRecogBean("E17", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E16", null, null, null, "E17" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E18", 3));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }
    }
} // end of namespace