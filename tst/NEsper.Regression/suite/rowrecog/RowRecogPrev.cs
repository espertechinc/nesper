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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogPrev
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithTimeWindowPartitionedSimple(execs);
            WithPartitionBy2FieldsKeepall(execs);
            WithUnpartitionedKeepAll(execs);
            WithTimeWindowUnpartitioned(execs);
            WithTimeWindowPartitioned(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowPartitioned(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogTimeWindowPartitioned());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowUnpartitioned(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogTimeWindowUnpartitioned());
            return execs;
        }

        public static IList<RegressionExecution> WithUnpartitionedKeepAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogUnpartitionedKeepAll());
            return execs;
        }

        public static IList<RegressionExecution> WithPartitionBy2FieldsKeepall(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogPartitionBy2FieldsKeepall());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowPartitionedSimple(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogTimeWindowPartitionedSimple());
            return execs;
        }

        private class RowRecogTimeWindowUnpartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string,b_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time(5) " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string" +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4' and Math.Abs(prev(A.Value, 0)) >= 0," +
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
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

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
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });

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
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E5", "E6" } });

                env.Milestone(5);

                SendTimer(8500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E5", "E6" } });

                SendTimer(9500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6" } });

                env.Milestone(6);

                SendTimer(10500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6" } });

                SendTimer(11500, env);
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.UndeployAll();
            }
        }

        private class RowRecogTimeWindowPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "Cat,a_string,b_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time(5) " +
                           "match_recognize (" +
                           "  partition by Cat" +
                           "  measures A.Cat as Cat, A.TheString as a_string, B.TheString as b_string" +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4'," +
                           "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                           ") order by Cat";

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
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

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
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c1", "E2", "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c1", "E2", "E3" } });

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
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c2", "E5", "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c1", "E2", "E3" }, new object[] { "c2", "E5", "E6" } });

                env.Milestone(5);

                SendTimer(8500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c1", "E2", "E3" }, new object[] { "c2", "E5", "E6" } });

                SendTimer(9500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c2", "E5", "E6" } });

                env.Milestone(6);

                SendTimer(10500, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "c2", "E5", "E6" } });

                SendTimer(11500, env);
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.UndeployAll();
            }
        }

        private class RowRecogTimeWindowPartitionedSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time(5 sec) " +
                           "match_recognize (" +
                           "  partition by Cat " +
                           "  measures A.Cat as Cat, A.TheString as a_string" +
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
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" } });

                env.Milestone(3);

                SendTimer(6500, env);
                env.SendEventBean(new SupportRecogBean("E5", "S3", 101));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" } });

                env.Milestone(4);

                SendTimer(7000, env);
                env.SendEventBean(new SupportRecogBean("E6", "S1", 102));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" } });

                env.Milestone(5);

                SendTimer(10000, env);
                env.SendEventBean(new SupportRecogBean("E7", "S2", 103));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });

                env.SendEventBean(new SupportRecogBean("E8", "S2", 102));
                env.SendEventBean(new SupportRecogBean("E8", "S1", 101));
                env.SendEventBean(new SupportRecogBean("E8", "S2", 104));
                env.SendEventBean(new SupportRecogBean("E8", "S1", 105));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);

                SendTimer(11199, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "E4" }, new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });

                env.Milestone(7);

                SendTimer(11200, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" }, new object[] { "E6" }, new object[] { "E7" } });

                SendTimer(11600, env);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" }, new object[] { "E7" } });

                env.Milestone(8);

                SendTimer(16000, env);
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.UndeployAll();
            }
        }

        private class RowRecogPartitionBy2FieldsKeepall : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string,a_cat,a_value,b_value".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  partition by TheString, Cat" +
                           "  measures A.TheString as a_string, A.Cat as a_cat, A.Value as a_value, B.Value as b_value " +
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
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("S1", "T1", 9));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", "T1", 7, 9 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", "T1", 7, 9 } });

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("S2", "T2", 1001));
                env.SendEventBean(new SupportRecogBean("S2", "T1", 109));
                env.SendEventBean(new SupportRecogBean("S1", "T2", 25));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", "T1", 7, 9 } });

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("S2", "T2", 1002));
                env.SendEventBean(new SupportRecogBean("S2", "T2", 1003));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S2", "T2", 1002, 1003 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", "T1", 7, 9 }, new object[] { "S2", "T2", 1002, 1003 } });

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("S1", "T2", 28));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "S1", "T2", 25, 28 } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "S1", "T1", 7, 9 }, new object[] { "S1", "T2", 25, 28 },
                        new object[] { "S2", "T2", 1002, 1003 }
                    });

                env.UndeployAll();
            }
        }

        private class RowRecogUnpartitionedKeepAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#keepall " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string" +
                           "  all matches pattern (A) " +
                           "  define A as (A.Value > PREV(A.Value))" +
                           ") " +
                           "order by a_string";
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
                    new object[][] { new object[] { "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E5", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E5" } });

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E6", 10));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E5" }, new object[] { "E6" } });

                env.Milestone(4);

                env.SendEventBean(new SupportRecogBean("E7", 9));

                env.Milestone(5);

                env.SendEventBean(new SupportRecogBean("E8", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E5" }, new object[] { "E6" } });

                env.UndeployModuleContaining("s0");

                text = "@name('s0') select * from SupportRecogBean#keepall " +
                       "match_recognize (" +
                       "  measures A.TheString as a_string" +
                       "  all matches pattern (A) " +
                       "  define A as (PREV(A.Value, 2) = 5)" +
                       ") " +
                       "order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 5));
                env.SendEventBean(new SupportRecogBean("E2", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(6);

                env.SendEventBean(new SupportRecogBean("E3", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } });

                env.Milestone(7);

                env.SendEventBean(new SupportRecogBean("E4", 3));
                env.SendEventBean(new SupportRecogBean("E5", 3));
                env.SendEventBean(new SupportRecogBean("E5", 5));
                env.SendEventBean(new SupportRecogBean("E6", 5));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" } });

                env.Milestone(8);

                env.SendEventBean(new SupportRecogBean("E7", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E7" } });

                env.Milestone(9);

                env.SendEventBean(new SupportRecogBean("E8", 6));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E8" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E3" }, new object[] { "E7" }, new object[] { "E8" } });

                env.UndeployAll();
            }
        }

        private static void SendTimer(
            long time,
            RegressionEnvironment env)
        {
            env.AdvanceTime(time);
        }
    }
} // end of namespace