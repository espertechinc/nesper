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
    public class RowRecogAfter
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new RowRecogAfterCurrentRow());
            execs.Add(new RowRecogAfterNextRow());
            execs.Add(new RowRecogSkipToNextRow());
            execs.Add(new RowRecogVariableMoreThenOnce());
            execs.Add(new RowRecogSkipToNextRowPartitioned());
            execs.Add(new RowRecogAfterSkipPastLast());
            return execs;
        }

        internal class RowRecogAfterCurrentRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           " measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1" +
                           " after match skip to current row" +
                           " pattern (A B*)" +
                           " define" +
                           " A as A.TheString like \"A%\"," +
                           " B as B.TheString like \"B%\"" +
                           ")";
                env.CompileDeploy(text).AddListener("s0");
                TryAssertionAfterCurrentRow(env, milestone);
                env.UndeployAll();

                env.EplToModelCompileDeploy(text).AddListener("s0");
                TryAssertionAfterCurrentRow(env, milestone);
                env.UndeployAll();
            }

            private void TryAssertionAfterCurrentRow(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var fields = "a,b0,b1".SplitCsv();

                env.SendEventBean(new SupportRecogBean("A1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A1", null, null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"A1", null, null}});

                env.MilestoneInc(milestone);

                // since the first match skipped past A, we do not match again
                env.SendEventBean(new SupportRecogBean("B1", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A1", "B1", null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"A1", "B1", null}});
            }
        }

        internal class RowRecogAfterNextRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,b0,b1".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a, B[0].TheString as b0, B[1].TheString as b1" +
                           "  AFTER MATCH SKIP TO NEXT ROW " +
                           "  pattern (A B*) " +
                           "  define " +
                           "    A as A.TheString like 'A%'," +
                           "    B as B.TheString like 'B%'" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("A1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"A1", null, null}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"A1", null, null}});

                env.Milestone(0);

                // since the first match skipped past A, we do not match again
                env.SendEventBean(new SupportRecogBean("B1", 2));
                Assert.IsFalse(env.Listener("s0").IsInvoked); // incremental skips to next
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"A1", "B1", null}});

                env.UndeployAll();
            }
        }

        internal class RowRecogSkipToNextRow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string " +
                           "  all matches " +
                           "  after match skip to next row " +
                           "  pattern (A B) " +
                           "  define B as B.value > A.value" +
                           ") " +
                           "order by a_string, b_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 5));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E2", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E3", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E5", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4", "E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("E6", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}, new object[] {"E5", "E6"}});

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("E7", 9));

                env.Milestone(6);

                env.SendEventBean(new SupportRecogBean("E8", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E4", "E5"}, new object[] {"E5", "E6"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogVariableMoreThenOnce : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a0,b,a1".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A[0].TheString as a0, B.TheString as b, A[1].TheString as a1 " +
                           "  all matches " +
                           "  after match skip to next row " +
                           "  pattern ( A B A ) " +
                           "  define " +
                           "    A as (A.value = 1)," +
                           "    B as (B.value = 2)" +
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

                env.SendEventBean(new SupportRecogBean("E7", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}});

                env.SendEventBean(new SupportRecogBean("E8", 2));
                env.SendEventBean(new SupportRecogBean("E9", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E7", "E8", "E9"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6", "E7"}, new object[] {"E7", "E8", "E9"}});

                env.UndeployAll();
            }
        }

        internal class RowRecogSkipToNextRowPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,a_value,b_value".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by theString" +
                           "  measures A.TheString as a_string, A.value as a_value, B.value as b_value " +
                           "  all matches " +
                           "  after match skip to next row " +
                           "  pattern (A B) " +
                           "  define B as (B.value > A.value)" +
                           ")" +
                           " order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("S1", 5));
                env.SendEventBean(new SupportRecogBean("S2", 6));
                env.SendEventBean(new SupportRecogBean("S3", 3));
                env.SendEventBean(new SupportRecogBean("S4", 4));
                env.SendEventBean(new SupportRecogBean("S1", 5));
                env.SendEventBean(new SupportRecogBean("S2", 5));
                env.SendEventBean(new SupportRecogBean("S1", 4));
                env.SendEventBean(new SupportRecogBean("S4", -1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(0);

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

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("S4", 11));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S4", 10, 11}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}, new object[] {"S4", 10, 11}});

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("S3", 3));
                env.SendEventBean(new SupportRecogBean("S4", -1));
                env.SendEventBean(new SupportRecogBean("S3", 2));
                env.SendEventBean(new SupportRecogBean("S1", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}, new object[] {"S4", 10, 11}});

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("S1", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S1", 4, 7}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10},
                        new object[] {"S4", 10, 11}
                    });

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("S4", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S4", -1, 12}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10},
                        new object[] {"S4", 10, 11}, new object[] {"S4", -1, 12}
                    });

                env.Milestone(6);

                env.SendEventBean(new SupportRecogBean("S4", 12));
                env.SendEventBean(new SupportRecogBean("S1", 7));
                env.SendEventBean(new SupportRecogBean("S2", 4));
                env.SendEventBean(new SupportRecogBean("S1", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(7);

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
                        new object[] {"S4", -1, 10}, new object[] {"S4", 10, 11}, new object[] {"S4", -1, 12}
                    });

                env.UndeployAll();
            }
        }

        internal class RowRecogAfterSkipPastLast : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,b_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string " +
                           "  all matches " +
                           "  after match skip past last row" +
                           "  pattern (A B) " +
                           "  define B as B.value > A.value" +
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
    }
} // end of namespace