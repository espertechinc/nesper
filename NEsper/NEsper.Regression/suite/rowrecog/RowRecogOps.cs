///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogOps
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new RowRecogConcatenation());
            execs.Add(new RowRecogZeroToMany());
            execs.Add(new RowRecogOneToMany());
            execs.Add(new RowRecogZeroToOne());
            execs.Add(new RowRecogPartitionBy());
            execs.Add(new RowRecogUnlimitedPartition());
            execs.Add(new RowRecogConcatWithinAlter());
            execs.Add(new RowRecogAlterWithinConcat());
            execs.Add(new RowRecogVariableMoreThenOnce());
            execs.Add(new RowRecogRegex());
            return execs;
        }

        internal class RowRecogConcatenation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});

                env.SendEventBean(new SupportRecogBean("E5", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", "E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E6", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked); // E5-E6 not a match since "skip past last row"
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});

                env.SendEventBean(new SupportRecogBean("E7", 9));
                env.SendEventBean(new SupportRecogBean("E8", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});

                env.UndeployModuleContaining("s0");
            }
        }

        internal class RowRecogZeroToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b0_string","b1_string","b2_string","c_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.SendEventBean(new SupportRecogBean("E3", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", null, null, null, "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, null, null, "E3"}});

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E4", 10));
                env.SendEventBean(new SupportRecogBean("E5", 12));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, null, null, "E3"}});

                env.SendEventBean(new SupportRecogBean("E6", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", "E5", null, null, "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, null, null, "E3"}, new object[] {"E4", "E5", null, null, "E6"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E10", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E7", "E8", "E9", "E10", "E11"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", null, null, null, "E3"}, new object[] {"E4", "E5", null, null, "E6"},
                        new object[] {"E7", "E8", "E9", "E10", "E11"}
                    });

                env.UndeployModuleContaining("s0");

                // Zero-to-many unfiltered
                var epl = "@Name('s0') select * from SupportRecogBean match_recognize (" +
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

        internal class RowRecogOneToMany : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b0_string","b1_string","b2_string","c_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.SendEventBean(new SupportRecogBean("E6", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", "E5", null, null, "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4", "E5", null, null, "E6"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E10", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E7", "E8", "E9", "E10", "E11"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4", "E5", null, null, "E6"}, new object[] {"E7", "E8", "E9", "E10", "E11"}});

                env.UndeployModuleContaining("s0");
            }
        }

        internal class RowRecogZeroToOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b_string","c_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E3", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", null, "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, "E3"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 10));
                env.SendEventBean(new SupportRecogBean("E5", 12));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, "E3"}});

                env.SendEventBean(new SupportRecogBean("E6", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", "E5", "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, "E3"}, new object[] {"E4", "E5", "E6"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 10));
                env.SendEventBean(new SupportRecogBean("E8", 12));
                env.SendEventBean(new SupportRecogBean("E9", 12));
                env.SendEventBean(new SupportRecogBean("E11", 9));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", null, "E3"}, new object[] {"E4", "E5", "E6"}});

                env.UndeployModuleContaining("s0");

                // test optional event not defined
                var epl = "@Name('s0') select * from SupportBean_A match_recognize (" +
                          "measures A.Id as Id, B.Id as b_Id " +
                          "pattern (A B?) " +
                          "define " +
                          " A as typeof(A) = 'SupportBean_A'" +
                          ")";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_A("A1"));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class RowRecogPartitionBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","a_value","b_value" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.SendEventBean(new SupportRecogBean("S1", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("S4", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S4", -1, 10}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});

                env.SendEventBean(new SupportRecogBean("S4", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked); // since skip past last row
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("S3", 3));
                env.SendEventBean(new SupportRecogBean("S4", -2));
                env.SendEventBean(new SupportRecogBean("S3", 2));
                env.SendEventBean(new SupportRecogBean("S1", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});

                env.SendEventBean(new SupportRecogBean("S1", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S1", 4, 7}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10}});

                env.SendEventBean(new SupportRecogBean("S4", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S4", -2, 12}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10},
                        new object[] {"S4", -2, 12}
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("S4", 12));
                env.SendEventBean(new SupportRecogBean("S1", 7));
                env.SendEventBean(new SupportRecogBean("S2", 4));
                env.SendEventBean(new SupportRecogBean("S1", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportRecogBean("S2", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S2", 4, 5}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S2", 4, 5},
                        new object[] {"S4", -1, 10}, new object[] {"S4", -2, 12}
                    });

                env.UndeployAll();
            }
        }

        internal class RowRecogUnlimitedPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by value" +
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
                    Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                }

                env.Milestone(0);

                for (var i = 0; i < 5 * RowRecogPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                    env.SendEventBean(new SupportRecogBean("A", i + 100000));
                }

                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());
                for (var i = 0; i < 5 * RowRecogPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                    env.SendEventBean(new SupportRecogBean("B", i + 100000));
                    Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                }

                env.UndeployAll();
            }
        }

        internal class RowRecogConcatWithinAlter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b_string","c_string","d_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E5", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, null, "E4", "E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, null, "E4", "E5"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", "E2", null, null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {null, null, "E4", "E5"}, new object[] {"E1", "E2", null, null}});

                env.UndeployModuleContaining("s0");
            }
        }

        internal class RowRecogAlterWithinConcat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a_string","b_string","c_string","d_string" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E6", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", null, "E6", null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", null, "E6", null}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E7", 2));
                env.SendEventBean(new SupportRecogBean("E8", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {null, "E7", "E8", null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", null, "E6", null}, new object[] {null, "E7", "E8", null}});

                env.UndeployAll();
            }
        }

        internal class RowRecogVariableMoreThenOnce : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a0","b","a1" };
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E7", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E8", 2));
                env.SendEventBean(new SupportRecogBean("E9", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}});

                env.SendEventBean(new SupportRecogBean("E10", 2));
                env.SendEventBean(new SupportRecogBean("E11", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E9", "E10", "E11"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}, new object[] {"E9", "E10", "E11"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogRegex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                Assert.IsTrue("aq".Matches("^aq|^Id"));
                Assert.IsTrue("Id".Matches("^aq|^Id"));
                Assert.IsTrue("ad".Matches("a(q|i)?d"));
                Assert.IsTrue("aqd".Matches("a(q|i)?d"));
                Assert.IsTrue("aId".Matches("a(q|i)?d"));
                Assert.IsFalse("aed".Matches("a(q|i)?d"));
                Assert.IsFalse("a".Matches("(a(b?)c)?"));
            }
        }
    }
} // end of namespace