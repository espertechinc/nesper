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
    public class RowRecogPrev
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new RowRecogTimeWindowPartitionedSimple());
            execs.Add(new RowRecogPartitionBy2FieldsKeepall());
            execs.Add(new RowRecogUnpartitionedKeepAll());
            execs.Add(new RowRecogTimeWindowUnpartitioned());
            execs.Add(new RowRecogTimeWindowPartitioned());
            return execs;
        }

        private static void SendTimer(
            long time,
            RegressionEnvironment env)
        {
            env.AdvanceTime(time);
        }

        internal class RowRecogTimeWindowUnpartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string,b_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#time(5) " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string" +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4' and Math.abs(prev(A.Value, 0)) >= 0," +
                           "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                SendTimer(1000, env);
                env.SendEventBean(new SupportRecogBean("P2", 1));
                env.SendEventBean(new SupportRecogBean("P1", 2));
                env.SendEventBean(new SupportRecogBean("P3", 3));
                env.SendEventBean(new SupportRecogBean("P4", 4));

                env.Milestone(0);

                SendTimer(2000, env);
                env.SendEventBean(new SupportRecogBean("P2", 1));
                env.SendEventBean(new SupportRecogBean("E1", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                SendTimer(3000, env);
                env.SendEventBean(new SupportRecogBean("P4", 11));
                env.SendEventBean(new SupportRecogBean("P3", 12));
                env.SendEventBean(new SupportRecogBean("P2", 13));

                env.Milestone(2);

                SendTimer(4000, env);
                env.SendEventBean(new SupportRecogBean("xx", 4));
                env.SendEventBean(new SupportRecogBean("E2", -1));
                env.SendEventBean(new SupportRecogBean("E3", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}});

                env.Milestone(3);

                SendTimer(5000, env);
                env.SendEventBean(new SupportRecogBean("P4", 21));
                env.SendEventBean(new SupportRecogBean("P3", 22));

                env.Milestone(4);

                SendTimer(6000, env);
                env.SendEventBean(new SupportRecogBean("P2", 23));
                env.SendEventBean(new SupportRecogBean("xx", -2));
                env.SendEventBean(new SupportRecogBean("E5", -1));
                env.SendEventBean(new SupportRecogBean("E6", -2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5", "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E5", "E6"}});

                env.Milestone(5);

                SendTimer(8500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", "E3"}, new object[] {"E5", "E6"}});

                SendTimer(9500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6"}});

                env.Milestone(6);

                SendTimer(10500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", "E6"}});

                SendTimer(11500, env);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class RowRecogTimeWindowPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "cat,a_string,b_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#time(5) " +
                           "match_recognize (" +
                           "  partition by cat" +
                           "  measures A.cat as cat, A.TheString as a_string, B.TheString as b_string" +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4'," +
                           "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                           ") order by cat";

                env.CompileDeploy(text).AddListener("s0");

                SendTimer(1000, env);
                env.SendEventBean(new SupportRecogBean("P4", "c2", 1));
                env.SendEventBean(new SupportRecogBean("P3", "c1", 2));
                env.SendEventBean(new SupportRecogBean("P2", "c2", 3));
                env.SendEventBean(new SupportRecogBean("xx", "c1", 4));

                env.Milestone(0);

                SendTimer(2000, env);
                env.SendEventBean(new SupportRecogBean("P2", "c1", 1));
                env.SendEventBean(new SupportRecogBean("E1", "c1", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(1);

                SendTimer(3000, env);
                env.SendEventBean(new SupportRecogBean("P4", "c1", 11));
                env.SendEventBean(new SupportRecogBean("P3", "c1", 12));
                env.SendEventBean(new SupportRecogBean("P2", "c1", 13));

                env.Milestone(2);

                SendTimer(4000, env);
                env.SendEventBean(new SupportRecogBean("xx", "c1", 4));
                env.SendEventBean(new SupportRecogBean("E2", "c1", -1));
                env.SendEventBean(new SupportRecogBean("E3", "c1", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"c1", "E2", "E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"c1", "E2", "E3"}});

                env.Milestone(3);

                SendTimer(5000, env);
                env.SendEventBean(new SupportRecogBean("P4", "c2", 21));
                env.SendEventBean(new SupportRecogBean("P3", "c2", 22));

                env.Milestone(4);

                SendTimer(6000, env);
                env.SendEventBean(new SupportRecogBean("P2", "c2", 23));
                env.SendEventBean(new SupportRecogBean("xx", "c2", -2));
                env.SendEventBean(new SupportRecogBean("E5", "c2", -1));
                env.SendEventBean(new SupportRecogBean("E6", "c2", -2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"c2", "E5", "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"c1", "E2", "E3"}, new object[] {"c2", "E5", "E6"}});

                env.Milestone(5);

                SendTimer(8500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"c1", "E2", "E3"}, new object[] {"c2", "E5", "E6"}});

                SendTimer(9500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"c2", "E5", "E6"}});

                env.Milestone(6);

                SendTimer(10500, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"c2", "E5", "E6"}});

                SendTimer(11500, env);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class RowRecogTimeWindowPartitionedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#time(5 sec) " +
                           "match_recognize (" +
                           "  partition by cat " +
                           "  measures A.cat as cat, A.TheString as a_string" +
                           "  all matches pattern (A) " +
                           "  define " +
                           "    A as PREV(A.Value) = (A.Value - 1)" +
                           ") order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.Milestone(0);

                SendTimer(1000, env);
                env.SendEventBean(new SupportRecogBean("E1", "S1", 100));

                SendTimer(2000, env);
                env.SendEventBean(new SupportRecogBean("E2", "S3", 100));

                env.Milestone(1);

                SendTimer(2500, env);
                env.SendEventBean(new SupportRecogBean("E3", "S2", 102));

                env.Milestone(2);

                SendTimer(6200, env);
                env.SendEventBean(new SupportRecogBean("E4", "S1", 101));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E4"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}});

                env.Milestone(3);

                SendTimer(6500, env);
                env.SendEventBean(new SupportRecogBean("E5", "S3", 101));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}});

                env.Milestone(4);

                SendTimer(7000, env);
                env.SendEventBean(new SupportRecogBean("E6", "S1", 102));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});

                env.Milestone(5);

                SendTimer(10000, env);
                env.SendEventBean(new SupportRecogBean("E7", "S2", 103));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});

                env.SendEventBean(new SupportRecogBean("E8", "S2", 102));
                env.SendEventBean(new SupportRecogBean("E8", "S1", 101));
                env.SendEventBean(new SupportRecogBean("E8", "S2", 104));
                env.SendEventBean(new SupportRecogBean("E8", "S1", 105));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                SendTimer(11199, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});

                env.Milestone(7);

                SendTimer(11200, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});

                SendTimer(11600, env);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E6"}, new object[] {"E7"}});

                env.Milestone(8);

                SendTimer(16000, env);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.UndeployAll();
            }
        }

        internal class RowRecogPartitionBy2FieldsKeepall : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,a_cat,a_value,b_value".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by TheString, cat" +
                           "  measures A.TheString as a_string, A.cat as a_cat, A.Value as a_value, B.Value as b_value " +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as (A.Value > PREV(A.Value))," +
                           "    B as (B.Value > PREV(B.Value))" +
                           ") order by a_string, a_cat";

                env.CompileDeploy(text).AddListener("s0");
                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("S1", "T1", 5));
                env.SendEventBean(new SupportRecogBean("S2", "T1", 110));
                env.SendEventBean(new SupportRecogBean("S1", "T2", 21));
                env.SendEventBean(new SupportRecogBean("S1", "T1", 7));
                env.SendEventBean(new SupportRecogBean("S2", "T1", 111));
                env.SendEventBean(new SupportRecogBean("S1", "T2", 20));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("S2", "T1", 110));
                env.SendEventBean(new SupportRecogBean("S2", "T2", 1000));
                env.SendEventBean(new SupportRecogBean("S2", "T2", 1001));
                env.SendEventBean(new SupportRecogBean("S1", null, 9));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("S1", "T1", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S1", "T1", 7, 9}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", "T1", 7, 9}});

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("S2", "T2", 1001));
                env.SendEventBean(new SupportRecogBean("S2", "T1", 109));
                env.SendEventBean(new SupportRecogBean("S1", "T2", 25));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", "T1", 7, 9}});

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("S2", "T2", 1002));
                env.SendEventBean(new SupportRecogBean("S2", "T2", 1003));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S2", "T2", 1002, 1003}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"S1", "T1", 7, 9}, new object[] {"S2", "T2", 1002, 1003}});

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("S1", "T2", 28));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S1", "T2", 25, 28}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"S1", "T1", 7, 9}, new object[] {"S1", "T2", 25, 28},
                        new object[] {"S2", "T2", 1002, 1003}
                    });

                env.UndeployAll();
            }
        }

        internal class RowRecogUnpartitionedKeepAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string".SplitCsv();
                var text = "@Name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string" +
                           "  all matches pattern (A) " +
                           "  define A as (A.Value > PREV(A.Value))" +
                           ") " +
                           "order by a_string";
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
                    new[] {new object[] {"E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E5", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E5"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E5"}});

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E6", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E5"}, new object[] {"E6"}});

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("E7", 9));

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("E8", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E5"}, new object[] {"E6"}});

                env.UndeployModuleContaining("s0");

                text = "@Name('s0') select * from SupportRecogBean#keepall " +
                       "match_recognize (" +
                       "  measures A.TheString as a_string" +
                       "  all matches pattern (A) " +
                       "  define A as (PREV(A.Value, 2) = 5)" +
                       ") " +
                       "order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 5));
                env.SendEventBean(new SupportRecogBean("E2", 4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.IsFalse(env.Statement("s0").GetEnumerator().MoveNext());

                env.Milestone(6);

                env.SendEventBean(new SupportRecogBean("E3", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E3"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(7);

                env.SendEventBean(new SupportRecogBean("E4", 3));
                env.SendEventBean(new SupportRecogBean("E5", 3));
                env.SendEventBean(new SupportRecogBean("E5", 5));
                env.SendEventBean(new SupportRecogBean("E6", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}});

                env.Milestone(8);

                env.SendEventBean(new SupportRecogBean("E7", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E7"}});

                env.Milestone(9);

                env.SendEventBean(new SupportRecogBean("E8", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E8"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3"}, new object[] {"E7"}, new object[] {"E8"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace