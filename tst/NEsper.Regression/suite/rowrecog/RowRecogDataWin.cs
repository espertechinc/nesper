///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.rowrecog;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogDataWin
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithUnboundStreamNoIterator(execs);
            WithTimeWindow(execs);
            WithTimeBatchWindow(execs);
            WithDataWinNamedWindow(execs);
            WithDataWinTimeBatch(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDataWinTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogDataWinTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithDataWinNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogDataWinNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogTimeBatchWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithUnboundStreamNoIterator(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogUnboundStreamNoIterator());
            return execs;
        }

        private class RowRecogUnboundStreamNoIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "string,value".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean " +
                           "match_recognize (" +
                           "  measures A.TheString as string, A.Value as value" +
                           "  all matches pattern (A) " +
                           "  define " +
                           "    A as PREV(A.TheString, 1) = TheString" +
                           ")";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("s1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("s2", 2));
                env.SendEventBean(new SupportRecogBean("s1", 3));
                env.SendEventBean(new SupportRecogBean("s3", 4));

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("s2", 5));
                env.SendEventBean(new SupportRecogBean("s1", 6));
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportRecogBean("s1", 7));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "s1", 7 } });
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.UndeployAll();
                /*
                  Optionally send some more events.
    
                for (int i = 0; i < 100000; i++)
                new object[] {
                    env.sendEventBean(new SupportRecogBean("P2", 1));
                }
                env.sendEventBean(new SupportRecogBean("P2", 1));
                 */
            }
        }

        private class RowRecogTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string,b_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time(5 sec) " +
                           "match_recognize (" +
                           "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string" +
                           "  all matches pattern ( A B C ) " +
                           "  define " +
                           "    A as (A.Value = 1)," +
                           "    B as (B.Value = 2)," +
                           "    C as (C.Value = 3)" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                env.Milestone(0);

                SendTimer(50, env);
                env.SendEventBean(new SupportRecogBean("E1", 1));

                env.Milestone(1);

                SendTimer(1000, env);
                env.SendEventBean(new SupportRecogBean("E2", 2));
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                SendTimer(6000, env);
                env.SendEventBean(new SupportRecogBean("E3", 3));
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(3);

                SendTimer(7000, env);
                env.SendEventBean(new SupportRecogBean("E4", 1));

                env.Milestone(4);

                SendTimer(8000, env);
                env.SendEventBean(new SupportRecogBean("E5", 2));

                env.Milestone(5);

                SendTimer(11500, env);
                env.SendEventBean(new SupportRecogBean("E6", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E4", "E5", "E6" } });

                env.Milestone(6);

                SendTimer(11999, env);
                env.AssertIterator("s0", it => ClassicAssert.IsTrue(it.MoveNext()));

                env.Milestone(7);

                SendTimer(12000, env);
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class RowRecogTimeBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimer(0, env);
                var fields = "a_string,b_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time_batch(5 sec) " +
                           "match_recognize (" +
                           "  partition by Cat " +
                           "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string" +
                           "  all matches pattern ( (A | B) C ) " +
                           "  define " +
                           "    A as A.TheString like 'A%'," +
                           "    B as B.TheString like 'B%'," +
                           "    C as C.TheString like 'C%' and C.Value in (A.Value, B.Value)" +
                           ") order by a_string";

                env.CompileDeploy(text).AddListener("s0");

                env.Milestone(0);

                SendTimer(50, env);
                env.SendEventBean(new SupportRecogBean("A1", "001", 1));
                env.SendEventBean(new SupportRecogBean("B1", "002", 1));
                env.SendEventBean(new SupportRecogBean("B2", "002", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(1);

                SendTimer(4000, env);
                env.SendEventBean(new SupportRecogBean("C1", "002", 4));
                env.SendEventBean(new SupportRecogBean("C2", "002", 5));
                env.SendEventBean(new SupportRecogBean("B3", "003", -1));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B2", "C1" } });

                env.Milestone(2);

                SendTimer(5050, env);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B2", "C1" } });
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(3);

                SendTimer(6000, env);
                env.SendEventBean(new SupportRecogBean("C3", "003", -1));
                env.SendEventBean(new SupportRecogBean("C4", "001", 1));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(4);

                SendTimer(10050, env);
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(5);

                SendTimer(14000, env);
                env.SendEventBean(new SupportRecogBean("A2", "002", 0));
                env.SendEventBean(new SupportRecogBean("B4", "003", 10));
                env.SendEventBean(new SupportRecogBean("C5", "002", 0));
                env.SendEventBean(new SupportRecogBean("C6", "003", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B4", "C6" }, new object[] { "A2", null, "C5" } });

                env.Milestone(6);

                SendTimer(15050, env);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B4", "C6" }, new object[] { "A2", null, "C5" } });
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.UndeployAll();
            }
        }

        public class RowRecogDataWinNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('createwindow') @public create window MyWindow#keepall as select * from SupportBean",
                    path);
                env.CompileDeploy("@name('insertwindow') insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean("A", 1));
                env.SendEventBean(new SupportBean("A", 2));
                env.SendEventBean(new SupportBean("B", 1));
                env.SendEventBean(new SupportBean("C", 3));

                var text = "@name('S1') select * from MyWindow " +
                           "match_recognize (" +
                           "  partition by TheString " +
                           "  measures A.TheString as ast, A.IntPrimitive as ai, B.IntPrimitive as bi" +
                           "  all matches pattern ( A B ) " +
                           "  define " +
                           "    B as (B.IntPrimitive = A.IntPrimitive)" +
                           ")";

                var fields = "ast,ai,bi".SplitCsv();
                env.CompileDeploy(text, path).AddListener("S1");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("C", 3));
                env.AssertPropsNew("S1", fields, new object[] { "C", 3, 3 });

                env.SendEventBean(new SupportBean("E", 5));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E", 5));
                env.AssertPropsNew(
                    "S1",
                    fields,
                    new object[] { "E", 5, 5 });

                env.UndeployAll();
            }
        }

        public class RowRecogDataWinTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var fields = "a_string,b_string,c_string".SplitCsv();
                var text = "@name('s0') select * from SupportRecogBean#time_batch(5 sec) " +
                           "match_recognize (" +
                           "  partition by Cat " +
                           "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string" +
                           "  all matches pattern ( (A | B) C ) " +
                           "  define " +
                           "    A as A.TheString like 'A%'," +
                           "    B as B.TheString like 'B%'," +
                           "    C as C.TheString like 'C%' and C.Value in (A.Value, B.Value)" +
                           ") order by a_string";
                env.CompileDeploy(text).AddListener("s0");

                env.AdvanceTime(50);
                env.SendEventBean(new SupportRecogBean("A1", "001", 1));
                env.SendEventBean(new SupportRecogBean("B1", "002", 1));
                env.SendEventBean(new SupportRecogBean("B2", "002", 4));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.AdvanceTime(4000);
                env.SendEventBean(new SupportRecogBean("C1", "002", 4));
                env.SendEventBean(new SupportRecogBean("C2", "002", 5));
                env.SendEventBean(new SupportRecogBean("B3", "003", -1));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B2", "C1" } });

                env.Milestone(1);

                env.AdvanceTime(5050);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B2", "C1" } });
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(2);

                env.AdvanceTime(6000);
                env.SendEventBean(new SupportRecogBean("C3", "003", -1));
                env.SendEventBean(new SupportRecogBean("C4", "001", 1));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(3);

                env.AdvanceTime(10050);
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

                env.Milestone(4);

                env.AdvanceTime(14000);
                env.SendEventBean(new SupportRecogBean("A2", "002", 0));
                env.SendEventBean(new SupportRecogBean("B4", "003", 10));
                env.SendEventBean(new SupportRecogBean("C5", "002", 0));
                env.SendEventBean(new SupportRecogBean("C6", "003", 10));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B4", "C6" }, new object[] { "A2", null, "C5" } });

                env.Milestone(5);

                env.AdvanceTime(15050);
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { null, "B4", "C6" }, new object[] { "A2", null, "C5" } });
                env.AssertIterator("s0", it => ClassicAssert.IsFalse(it.MoveNext()));

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