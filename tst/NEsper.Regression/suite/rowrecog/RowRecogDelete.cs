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

using NUnit.Framework; // assertFalse

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogDelete
    {
        // This test container is for
        //  (a) on-delete of events from a named window
        //  (b) a sorted window which also posts a remove stream that is out-of-order
        // ... also termed Out-Of-Sequence Delete (OOSD).
        //
        // The test is for out-of-sequence (and in-sequence) deletes:
        //  (1) Make sure that partial pattern matches get removed
        //  (2) Make sure that PREV is handled by order-of-arrival, and is not affected (by default) by delete (versus normal ordered remove stream).
        //      Since it is impossible to make guarantees as the named window could be entirely deleted, and "prev" depth is therefore unknown.
        //
        // Prev
        //    has OOSD
        //      update          PREV operates on original order-of-arrival; OOSD impacts matching: resequence only when partial matches deleted
        //      iterate         PREV operates on original order-of-arrival; OOSD impacts matching: iterator may present unseen-before matches after delete
        //    no OOSD
        //      update          PREV operates on original order-of-arrival; no resequencing when in-order deleted
        //      iterate         PREV operates on original order-of-arrival
        // No-Prev
        //    has OOSD
        //      update
        //      iterate
        //    no OOSD
        //      update
        //      iterate

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithOnDeleteOutOfSeq(execs);
            WithOutOfSequenceDelete(execs);
            WithInSequenceDelete(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInSequenceDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogNamedWindowInSequenceDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithOutOfSequenceDelete(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogNamedWindowOutOfSequenceDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithOnDeleteOutOfSeq(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogNamedWindowOnDeleteOutOfSeq());
            return execs;
        }

        private class RowRecogNamedWindowOnDeleteOutOfSeq : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyNamedWindow#keepall as SupportRecogBean", path);
                env.CompileDeploy("insert into MyNamedWindow select * from SupportRecogBean", path);
                env.CompileDeploy(
                    "on SupportBean as d delete from MyNamedWindow w where d.intPrimitive = w.value",
                    path);

                var fields = "a_string,b_string".SplitCsv();
                var text = "@name('s0') select * from MyNamedWindow " +
                           "match_recognize (" +
                           "  measures A.theString as a_string, B.theString as b_string" +
                           "  all matches pattern (A B) " +
                           "  define " +
                           "    A as PREV(A.theString, 3) = 'P3' and PREV(A.theString, 2) = 'P2' and PREV(A.theString, 4) = 'P4'," +
                           "    B as B.value in (PREV(B.value, 4), PREV(B.value, 2))" +
                           ")";

                env.CompileDeploy(text, path).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("P2", 1));
                env.SendEventBean(new SupportRecogBean("P1", 2));
                env.SendEventBean(new SupportRecogBean("P3", 3));
                env.SendEventBean(new SupportRecogBean("P4", 4));
                env.SendEventBean(new SupportRecogBean("P2", 1));
                env.SendEventBean(new SupportRecogBean("E1", 3));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => Assert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("P4", 11));
                env.SendEventBean(new SupportRecogBean("P3", 12));
                env.SendEventBean(new SupportRecogBean("P2", 13));
                env.SendEventBean(new SupportRecogBean("xx", 4));
                env.SendEventBean(new SupportRecogBean("E2", -4));
                env.SendEventBean(new SupportRecogBean("E3", 12));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("P4", 21));
                env.SendEventBean(new SupportRecogBean("P3", 22));
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

                env.Milestone(2);

                // delete an PREV-referenced event: no effect as PREV is an order-of-arrival operator
                env.SendEventBean(new SupportBean("D1", 21)); // delete P4 of second batch
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" }, new object[] { "E5", "E6" } });

                env.Milestone(3);

                // delete an partial-match event
                env.SendEventBean(new SupportBean("D2", -1)); // delete E5 of second batch
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2", "E3" } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("D3", 12)); // delete P3 and E3 of first batch
                env.AssertIterator("s0", it => Assert.IsFalse(it.MoveNext()));

                env.UndeployAll();
            }
        }

        private class RowRecogNamedWindowOutOfSequenceDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#keepall as SupportRecogBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportRecogBean", path);
                env.CompileDeploy(
                    "on SupportBean as s delete from MyWindow as w where s.theString = w.theString",
                    path);

                var fields = "a0,a1,b0,b1,c".SplitCsv();
                var text = "@name('s0') select * from MyWindow " +
                           "match_recognize (" +
                           "  measures A[0].theString as a0, A[1].theString as a1, B[0].theString as b0, B[1].theString as b1, C.theString as c" +
                           "  pattern ( A+ B* C ) " +
                           "  define " +
                           "    A as (A.value = 1)," +
                           "    B as (B.value = 2)," +
                           "    C as (C.value = 3)" +
                           ")";
                env.CompileDeploy(text, path).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportBean("E2", 0)); // deletes E2
                env.SendEventBean(new SupportRecogBean("E3", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, null, null, "E3" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", null, null, null, "E3" } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0)); // deletes E1
                env.SendEventBean(new SupportBean("E4", 0)); // deletes E4

                env.Milestone(1);

                env.SendEventBean(new SupportRecogBean("E4", 1));
                env.SendEventBean(new SupportRecogBean("E5", 1));
                env.SendEventBean(new SupportBean("E4", 0)); // deletes E4
                env.SendEventBean(new SupportRecogBean("E6", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", null, null, null, "E6" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", null, null, null, "E6" } });

                env.Milestone(2);

                env.SendEventBean(new SupportRecogBean("E7", 1));
                env.SendEventBean(new SupportRecogBean("E8", 1));
                env.SendEventBean(new SupportRecogBean("E9", 2));
                env.SendEventBean(new SupportRecogBean("E10", 2));
                env.SendEventBean(new SupportRecogBean("E11", 2));
                env.SendEventBean(new SupportBean("E9", 0)); // deletes E9
                env.SendEventBean(new SupportRecogBean("E12", 3));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E7", "E8", "E10", "E11", "E12" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E5", null, null, null, "E6" }, new object[] { "E7", "E8", "E10", "E11", "E12" }
                    }); // note interranking among per-event result

                env.Milestone(3);

                env.SendEventBean(new SupportRecogBean("E13", 1));
                env.SendEventBean(new SupportRecogBean("E14", 1));
                env.SendEventBean(new SupportRecogBean("E15", 2));
                env.SendEventBean(new SupportRecogBean("E16", 2));
                env.SendEventBean(new SupportBean("E14", 0)); // deletes E14
                env.SendEventBean(new SupportBean("E15", 0)); // deletes E15
                env.SendEventBean(new SupportBean("E16", 0)); // deletes E16
                env.SendEventBean(new SupportBean("E13", 0)); // deletes E17
                env.SendEventBean(new SupportRecogBean("E18", 3));
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E5", null, null, null, "E6" }, new object[] { "E7", "E8", "E10", "E11", "E12" }
                    }); // note interranking among per-event result

                env.UndeployAll();
            }
        }

        private class RowRecogNamedWindowInSequenceDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindow#keepall as SupportRecogBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportRecogBean", path);
                env.CompileDeploy(
                    "on SupportBean as s delete from MyWindow as w where s.theString = w.theString",
                    path);

                var fields = "a0,a1,b".SplitCsv();
                var text = "@name('s0') select * from MyWindow " +
                           "match_recognize (" +
                           "  measures A[0].theString as a0, A[1].theString as a1, B.theString as b" +
                           "  pattern ( A* B ) " +
                           "  define " +
                           "    A as (A.value = 1)," +
                           "    B as (B.value = 2)" +
                           ")";

                env.CompileDeploy(text, path).AddListener("s0");

                env.SendEventBean(new SupportRecogBean("E1", 1));
                env.SendEventBean(new SupportRecogBean("E2", 1));
                env.SendEventBean(new SupportBean("E1", 0)); // deletes E1
                env.SendEventBean(new SupportBean("E2", 0)); // deletes E2
                env.SendEventBean(new SupportRecogBean("E3", 3));
                env.AssertListenerNotInvoked("s0");
                env.AssertIterator("s0", it => Assert.IsFalse(it.MoveNext()));

                env.Milestone(0);

                env.SendEventBean(new SupportRecogBean("E4", 1));
                env.SendEventBean(new SupportRecogBean("E5", 1));
                env.SendEventBean(new SupportBean("E4", 0)); // deletes E4
                env.SendEventBean(new SupportRecogBean("E6", 1));
                env.SendEventBean(new SupportRecogBean("E7", 2));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" } });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E5", "E6", "E7" } });

                env.UndeployAll();
            }
        }
    }
} // end of namespace