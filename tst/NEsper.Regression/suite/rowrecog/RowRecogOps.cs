///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogOps
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithConcatenation(execs);
            WithZeroToMany(execs);
            WithOneToMany(execs);
            WithZeroToOne(execs);
            WithPartitionBy(execs);
            WithUnlimitedPartition(execs);
            WithConcatWithinAlter(execs);
            WithAlterWithinConcat(execs);
            WithVariableMoreThenOnce(execs);
            WithRegex(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRegex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRegex());
            return execs;
        }

        public static IList<RegressionExecution> WithVariableMoreThenOnce(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogVariableMoreThenOnce());
            return execs;
        }

        public static IList<RegressionExecution> WithAlterWithinConcat(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogAlterWithinConcat());
            return execs;
        }

        public static IList<RegressionExecution> WithConcatWithinAlter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogConcatWithinAlter());
            return execs;
        }

        public static IList<RegressionExecution> WithUnlimitedPartition(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogUnlimitedPartition());
            return execs;
        }

        public static IList<RegressionExecution> WithPartitionBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPartitionBy());
            return execs;
        }

        public static IList<RegressionExecution> WithZeroToOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogZeroToOne());
            return execs;
        }

        public static IList<RegressionExecution> WithOneToMany(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogOneToMany());
            return execs;
        }

        public static IList<RegressionExecution> WithZeroToMany(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogZeroToMany());
            return execs;
        }

        public static IList<RegressionExecution> WithConcatenation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogConcatenation());
            return execs;
        }

        private class RowRecogConcatenation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string " +
                           "  all matches " +
                           "  pattern (A B) " +
                           "  define B as B.Value > A.Value" +
                           ") " +
                           "order by a_string, b_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 5));
                env.SendEventBean(new SupportRecogBean("E2", 3));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });

                env.SendEventBean(new SupportRecogBean("E5", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E4", "E5" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E6", 10));
                env.AssertListenerNotInvoked("s0"); // E5-E6 not a match since "skip past last row"
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E4", "E5" } });

                env.SendEventBean(new SupportRecogBean("E7", 9));
                env.SendEventBean(new SupportRecogBean("E8", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E4", "E5" } });

                env.UndeployModuleContaining("s0");
            }
        }

        private class RowRecogZeroToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b0_string,b1_string,b2_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, " +
                           "    B[0].TheString as b0_string, " +
                           "    B[1].TheString as b1_string, " +
                           "    B[2].TheString as b2_string, " +
                           "    C.TheString as c_string" +
                           "  all matches " +
                           "  pattern (A B* C) " +
                           "  define \n" +
                           "    A as A.Value = 10,\n" +
                           "    B as B.Value > 10,\n" +
                           "    C as C.Value < 10\n" +
                           ") " +
                           "order by a_string, c_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 12));
                env.SendEventBean(new SupportRecogBean("E2", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.SendEventBean(new SupportRecogBean("E3", 8));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, null, null, "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, null, null, "E3" } });

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E4", 10));
                env.SendEventBean(new SupportRecogBean("E5", 12));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, null, null, "E3" } });

                env.SendEventBean(new SupportRecogBean("E6", 8));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", null, null, "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", null, null, null, "E3" }, new object[] { "E4", "E5", null, null, "E6" }
                    });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E10", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7", "E8", "E9", "E10", "E11" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E2", null, null, null, "E3" }, new object[] { "E4", "E5", null, null, "E6" },
                        new object[] { "E7", "E8", "E9", "E10", "E11" }
                    });

                env.UndeployModuleContaining("s0");

                // Zero-to-many unfiltered
                var epl = "@name('s0') select * from SupportRecogBean match_recognize (" +
                          "measures A as a, B as b, C as c " +
                          "pattern (A C*? B) " +
                          "define " +
                          "A as typeof(A) = 'SupportRecogBeanTypeA'," +
                          "B as typeof(B) = 'SupportRecogBeanTypeB'" +
                          ")";
                env.CompileDeploy(epl);
                env.UndeployAll();
            }
        }

        private class RowRecogOneToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b0_string,b1_string,b2_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, " +
                           "    B[0].TheString as b0_string, " +
                           "    B[1].TheString as b1_string, " +
                           "    B[2].TheString as b2_string, " +
                           "    C.TheString as c_string" +
                           "  all matches " +
                           "  pattern (A B+ C) " +
                           "  define \n" +
                           "    A as (A.Value = 10),\n" +
                           "    B as (B.Value > 10),\n" +
                           "    C as (C.Value < 10)\n" +
                           ") " +
                           "order by a_string, c_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 12));
                env.SendEventBean(new SupportRecogBean("E2", 10));
                env.SendEventBean(new SupportRecogBean("E3", 8));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E4", 10));
                env.SendEventBean(new SupportRecogBean("E5", 12));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.SendEventBean(new SupportRecogBean("E6", 8));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", null, null, "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", null, null, "E6" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E10", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7", "E8", "E9", "E10", "E11" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E4", "E5", null, null, "E6" }, new object[] { "E7", "E8", "E9", "E10", "E11" }
                    });

                env.UndeployModuleContaining("s0");
            }
        }

        private class RowRecogZeroToOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string, " +
                           "    C.TheString as c_string" +
                           "  all matches " +
                           "  pattern (A B? C) " +
                           "  define \n" +
                           "    A as (A.Value = 10),\n" +
                           "    B as (B.Value > 10),\n" +
                           "    C as (C.Value < 10)\n" +
                           ") " +
                           "order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 12));
                env.SendEventBean(new SupportRecogBean("E2", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 8));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, "E3" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 10));
                env.SendEventBean(new SupportRecogBean("E5", 12));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, "E3" } });

                env.SendEventBean(new SupportRecogBean("E6", 8));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, "E3" }, new object[] { "E4", "E5", "E6" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", null, "E3" }, new object[] { "E4", "E5", "E6" } });

                env.UndeployModuleContaining("s0");

                // test optional event not defined
                var epl = "@name('s0') select * from SupportBean_A match_recognize (" +
                          "measures A.Id as Id, B.Id as b_id " +
                          "pattern (A B?) " +
                          "define " +
                          " A as typeof(A) = 'SupportBean_A'" +
                          ")";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class RowRecogPartitionBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,a_value,b_value".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by TheString" +
                           "  measures A.TheString as a_string, A.Value as a_value, B.Value as b_value " +
                           "  all matches pattern (A B) " +
                           "  define B as (B.Value > A.Value)" +
                           ")" +
                           " order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("S1", 5));
                env.SendEventBean(new SupportRecogBean("S2", 6));
                env.SendEventBean(new SupportRecogBean("S3", 3));
                env.SendEventBean(new SupportRecogBean("S4", 4));
                env.SendEventBean(new SupportRecogBean("S1", 5));
                env.SendEventBean(new SupportRecogBean("S2", 5));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("S1", 4));
                env.SendEventBean(new SupportRecogBean("S4", -1));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.SendEventBean(new SupportRecogBean("S1", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 6 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 6 } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("S4", 10));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S4", -1, 10 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 6 }, new object[] { "S4", -1, 10 } });

                env.SendEventBean(new SupportRecogBean("S4", 11));
                env.AssertListenerNotInvoked("s0"); // since skip past last row
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 6 }, new object[] { "S4", -1, 10 } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("S3", 3));
                env.SendEventBean(new SupportRecogBean("S4", -2));
                env.SendEventBean(new SupportRecogBean("S3", 2));
                env.SendEventBean(new SupportRecogBean("S1", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 6 }, new object[] { "S4", -1, 10 } });

                env.SendEventBean(new SupportRecogBean("S1", 7));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", 4, 7 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "S1", 4, 6 }, new object[] { "S1", 4, 7 }, new object[] { "S4", -1, 10 } });

                env.SendEventBean(new SupportRecogBean("S4", 12));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S4", -2, 12 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "S1", 4, 6 }, new object[] { "S1", 4, 7 }, new object[] { "S4", -1, 10 },
                        new object[] { "S4", -2, 12 }
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("S4", 12));
                env.SendEventBean(new SupportRecogBean("S1", 7));
                env.SendEventBean(new SupportRecogBean("S2", 4));
                env.SendEventBean(new SupportRecogBean("S1", 5));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportRecogBean("S2", 5));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S2", 4, 5 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "S1", 4, 6 }, new object[] { "S1", 4, 7 }, new object[] { "S2", 4, 5 },
                        new object[] { "S4", -1, 10 }, new object[] { "S4", -2, 12 }
                    });

                env.UndeployAll();
            }
        }

        private class RowRecogUnlimitedPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by Value" +
                           "  measures A.TheString as a_string " +
                           "  pattern (A B) " +
                           "  define " +
                           "    A as (A.TheString = 'A')," +
                           "    B as (B.TheString = 'B')" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                for (var i = 0; i < 5 * RowRecogPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                    env.SendEventBean(new SupportRecogBean("A", i));
                    env.SendEventBean(new SupportRecogBean("B", i));
                    env.AssertListenerInvoked("s0");
                }

                env.Milestone(0);

                for (var i = 0; i < 5 * RowRecogPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                    env.SendEventBean(new SupportRecogBean("A", i + 100000));
                }

                env.AssertListenerNotInvoked("s0");
                for (var i = 0; i < 5 * RowRecogPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                    env.SendEventBean(new SupportRecogBean("B", i + 100000));
                    env.AssertListenerInvoked("s0");
                }

                env.UndeployAll();
            }
        }

        private class RowRecogConcatWithinAlter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string,c_string,d_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                           "  all matches pattern ( A B | C D ) " +
                           "  define " +
                           "    A as (A.Value = 1)," +
                           "    B as (B.Value = 2)," +
                           "    C as (C.Value = 3)," +
                           "    D as (D.Value = 4)" +
                           ")";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 3));
                env.SendEventBean(new SupportRecogBean("E2", 5));
                env.SendEventBean(new SupportRecogBean("E3", 4));
                env.SendEventBean(new SupportRecogBean("E4", 3));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E5", 4));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, null, "E4", "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, null, "E4", "E5" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 2));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", "E2", null, null } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { null, null, "E4", "E5" }, new object[] { "E1", "E2", null, null } });

                env.UndeployModuleContaining("s0");
            }
        }

        private class RowRecogAlterWithinConcat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string,c_string,d_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                           "  all matches pattern ( (A | B) (C | D) ) " +
                           "  define " +
                           "    A as (A.Value = 1)," +
                           "    B as (B.Value = 2)," +
                           "    C as (C.Value = 3)," +
                           "    D as (D.Value = 4)" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 3));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 2));
                env.SendEventBean(new SupportRecogBean("E4", 5));
                env.SendEventBean(new SupportRecogBean("E5", 1));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E6", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", null, "E6", null } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", null, "E6", null } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 2));
                env.SendEventBean(new SupportRecogBean("E8", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "E7", "E8", null } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E5", null, "E6", null }, new object[] { null, "E7", "E8", null } });

                env.UndeployAll();
            }
        }

        private class RowRecogVariableMoreThenOnce : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a0,b,a1".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, B.TheString as b, A[1].TheString as a1 " +
                           "  all matches pattern ( A B A ) " +
                           "  define " +
                           "    A as (A.Value = 1)," +
                           "    B as (B.Value = 2)" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 3));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportRecogBean("E3", 2));
                env.SendEventBean(new SupportRecogBean("E4", 5));
                env.SendEventBean(new SupportRecogBean("E5", 1));
                env.SendEventBean(new SupportRecogBean("E6", 2));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E7", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E8", 2));
                env.SendEventBean(new SupportRecogBean("E9", 1));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" } });

                env.SendEventBean(new SupportRecogBean("E10", 2));
                env.SendEventBean(new SupportRecogBean("E11", 1));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E9", "E10", "E11" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" }, new object[] { "E9", "E10", "E11" } });

                env.UndeployAll();
            }
        }

        private class RowRecogRegex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                ClassicAssert.IsTrue("aq".Matches("^aq|^Id"));
                ClassicAssert.IsTrue("Id".Matches("^aq|^Id"));
                ClassicAssert.IsTrue("ad".Matches("a(q|i)?d"));
                ClassicAssert.IsTrue("aqd".Matches("a(q|i)?d"));
                ClassicAssert.IsTrue("aid".Matches("a(q|i)?d"));
                ClassicAssert.IsFalse("aed".Matches("a(q|i)?d"));
                ClassicAssert.IsFalse("a".Matches("(a(b?)c)?"));
            }
        }
    }
} // end of namespace