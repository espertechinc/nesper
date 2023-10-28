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
    public class RowRecogAggregation
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withn(execs);
            WithnPartitioned(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithnPartitioned(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogMeasureAggregationPartitioned());
            return execs;
        }

        public static IList<RegressionExecution> Withn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogMeasureAggregation());
            return execs;
        }

        private class RowRecogMeasureAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, " +
                           "       C.TheString as c_string, " +
                           "       max(B.Value) as maxb, " +
                           "       min(B.Value) as minb, " +
                           "       2*min(B.Value) as minb2x, " +
                           "       last(B.Value) as lastb, " +
                           "       first(B.Value) as firstb," +
                           "       count(B.Value) as countb " +
                           "  all matches pattern (A B* C) " +
                           "  define " +
                           "   A as (A.Value = 0)," +
                           "   B as (B.Value != 1)," +
                           "   C as (C.Value = 1)" +
                           ") " +
                           "order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                var fields = "a_string,c_string,maxb,minb,minb2x,firstb,lastb,countb".SplitCsv();
                env.SendEventBean(new SupportRecogBean("E1", 0));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E2", null, null, null, null, null, 0L } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E2", null, null, null, null, null, 0L } });

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 0));
                env.SendEventBean(new SupportRecogBean("E4", 5));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E5", 3));
                env.SendEventBean(new SupportRecogBean("E6", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3", "E6", 5, 3, 6, 5, 3, 2L } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", "E2", null, null, null, null, null, 0L },
                        new object[] { "E3", "E6", 5, 3, 6, 5, 3, 2L }
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 0));
                env.SendEventBean(new SupportRecogBean("E8", 4));
                env.SendEventBean(new SupportRecogBean("E9", -1));

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E10", 7));
                env.SendEventBean(new SupportRecogBean("E11", 2));
                env.SendEventBean(new SupportRecogBean("E12", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7", "E12", 7, -1, -2, 4, 2, 4L } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E1", "E2", null, null, null, null, null, 0L },
                        new object[] { "E3", "E6", 5, 3, 6, 5, 3, 2L },
                        new object[] { "E7", "E12", 7, -1, -2, 4, 2, 4L },
                    });

                env.UndeployAll();
            }
        }

        private class RowRecogMeasureAggregationPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by Cat" +
                           "  measures A.Cat as Cat, A.TheString as a_string, " +
                           "       D.TheString as d_string, " +
                           "       sum(C.Value) as sumc, " +
                           "       sum(B.Value) as sumb, " +
                           "       sum(B.Value + A.Value) as sumaplusb, " +
                           "       sum(C.Value + A.Value) as sumaplusc " +
                           "  all matches pattern (A B B C C D) " +
                           "  define " +
                           "   A as (A.Value >= 10)," +
                           "   B as (B.Value > 1)," +
                           "   C as (C.Value < -1)," +
                           "   D as (D.Value = 999)" +
                           ") order by Cat";

                env.CompileDeploy(text).AddListener("s0");

                var fields = "a_string,d_string,sumb,sumc,sumaplusb,sumaplusc".SplitCsv();
                env.SendEventBean(new SupportRecogBean("E1", "x", 10));
                env.SendEventBean(new SupportRecogBean("E2", "y", 20));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", "x", 7)); // B
                env.SendEventBean(new SupportRecogBean("E4", "y", 5));
                env.SendEventBean(new SupportRecogBean("E5", "x", 8));
                env.SendEventBean(new SupportRecogBean("E6", "y", 2));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", "x", -2)); // C
                env.SendEventBean(new SupportRecogBean("E8", "y", -7));
                env.SendEventBean(new SupportRecogBean("E9", "x", -5));
                env.SendEventBean(new SupportRecogBean("E10", "y", -4));

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E11", "y", 999));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E11", 7, -11, 47, 29 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E11", 7, -11, 47, 29 } });

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E12", "x", 999));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E12", 15, -7, 35, 13 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E1", "E12", 15, -7, 35, 13 }, new object[] { "E2", "E11", 7, -11, 47, 29 } });

                env.UndeployAll();
            }
        }
    }
} // end of namespace