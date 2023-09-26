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

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogArrayAccess
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSingleMultiMix(execs);
            WithMultiDepends(execs);
            WithMeasuresClausePresence(execs);
            WithLambda(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithLambda(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogLambda());
            return execs;
        }

        public static IList<RegressionExecution> WithMeasuresClausePresence(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogMeasuresClausePresence());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiDepends(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogMultiDepends());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleMultiMix(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogSingleMultiMix());
            return execs;
        }

        private class RowRecogSingleMultiMix : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,b0,c,d0,e".SplitCsv();
                var text = "@name('s0') select * from SupportBean " +
                           "match_recognize (" +
                           " measures A.theString as a, B[0].theString as b0, C.theString as c, D[0].theString as d0, E.theString as e" +
                           " pattern (A B+ C D+ E)" +
                           " define" +
                           " A as A.theString like 'A%', " +
                           " B as B.theString like 'B%'," +
                           " C as C.theString like 'C%' and C.intPrimitive = B[1].intPrimitive," +
                           " D as D.theString like 'D%'," +
                           " E as E.theString like 'E%' and E.intPrimitive = D[1].intPrimitive and E.intPrimitive = D[0].intPrimitive" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A1", 100 }, new object[] { "B1", 50 }, new object[] { "B2", 49 },
                        new object[] { "C1", 49 }, new object[] { "D1", 2 }, new object[] { "D2", 2 },
                        new object[] { "E1", 2 }
                    });
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C1", "D1", "E1" });

                env.Milestone(0);

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A1", 100 }, new object[] { "B1", 50 }, new object[] { "C1", 49 },
                        new object[] { "D1", 2 }, new object[] { "D2", 2 }, new object[] { "E1", 2 }
                    });
                env.AssertListenerNotInvoked("s0");

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A1", 100 }, new object[] { "B1", 50 }, new object[] { "B2", 49 },
                        new object[] { "C1", 49 }, new object[] { "D1", 2 }, new object[] { "D2", 3 },
                        new object[] { "E1", 2 }
                    });
                env.AssertListenerNotInvoked("s0");

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A1", 100 }, new object[] { "B1", 50 }, new object[] { "B2", 49 },
                        new object[] { "C1", 49 }, new object[] { "D1", 2 }, new object[] { "D2", 2 },
                        new object[] { "D3", 99 }, new object[] { "E1", 2 }
                    });
                env.AssertPropsNew("s0", fields, new object[] { "A1", "B1", "C1", "D1", "E1" });

                env.UndeployAll();
            }
        }

        private class RowRecogMultiDepends : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryMultiDepends(env, milestone, "A B A B C");
                TryMultiDepends(env, milestone, "(A B)* C");
            }
        }

        private class RowRecogMeasuresClausePresence : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryMeasuresClausePresence(env, milestone, "A as a_array, B as b");
                TryMeasuresClausePresence(env, milestone, "B as b");
                TryMeasuresClausePresence(env, milestone, "A as a_array");
                TryMeasuresClausePresence(env, milestone, "1 as one");
            }
        }

        private class RowRecogLambda : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsOne = "a0,a1,a2,b".SplitCsv();
                var epl = "@name('s0') select * from SupportBean " +
                          "match_recognize (" +
                          " measures A[0].theString as a0, A[1].theString as a1, A[2].theString as a2, B.theString as b" +
                          " pattern (A* B)" +
                          " define" +
                          " B as (coalesce(A.sumOf(v => v.intPrimitive), 0) + B.intPrimitive) > 100" +
                          ")";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvents(env, new object[][] { new object[] { "E1", 50 }, new object[] { "E2", 49 } });
                env.AssertListenerNotInvoked("s0");

                SendEvents(env, new object[][] { new object[] { "E3", 2 } });
                env.AssertPropsNew("s0", fieldsOne, new object[] { "E1", "E2", null, "E3" });

                env.Milestone(0);

                SendEvents(env, new object[][] { new object[] { "E4", 101 } });
                env.AssertPropsNew("s0", fieldsOne, new object[] { null, null, null, "E4" });

                SendEvents(env, new object[][] { new object[] { "E5", 50 }, new object[] { "E6", 51 } });
                env.AssertPropsNew("s0", fieldsOne, new object[] { "E5", null, null, "E6" });

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "E7", 10 }, new object[] { "E8", 10 }, new object[] { "E9", 79 },
                        new object[] { "E10", 1 }, new object[] { "E11", 1 }
                    });
                env.AssertPropsNew("s0", fieldsOne, new object[] { "E7", "E8", "E9", "E11" });
                env.UndeployAll();

                var fieldsTwo = "a[0].theString,a[1].theString,b.theString".SplitCsv();
                var eplTwo = "@name('s0') select * from SupportBean " +
                             "match_recognize (" +
                             " measures A as a, B as b " +
                             " pattern (A+ B)" +
                             " define" +
                             " A as theString like 'A%', " +
                             " B as theString like 'B%' and B.intPrimitive > A.sumOf(v => v.intPrimitive)" +
                             ")";

                env.CompileDeploy(eplTwo).AddListener("s0");

                SendEvents(
                    env,
                    new object[][] { new object[] { "A1", 1 }, new object[] { "A2", 2 }, new object[] { "B1", 3 } });
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "A2", null, "B1" });

                SendEvents(
                    env,
                    new object[][] { new object[] { "A3", 1 }, new object[] { "A4", 2 }, new object[] { "B2", 4 } });
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "A3", "A4", "B2" });

                env.Milestone(1);

                SendEvents(env, new object[][] { new object[] { "A5", -1 }, new object[] { "B3", 0 } });
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "A5", null, "B3" });

                SendEvents(
                    env,
                    new object[][] { new object[] { "A6", 10 }, new object[] { "B3", 9 }, new object[] { "B4", 11 } });
                SendEvents(
                    env,
                    new object[][] { new object[] { "A7", 10 }, new object[] { "A8", 9 }, new object[] { "A9", 8 } });
                env.AssertListenerNotInvoked("s0");

                SendEvents(env, new object[][] { new object[] { "B5", 18 } });
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "A8", "A9", "B5" });

                env.Milestone(2);

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A0", 10 }, new object[] { "A11", 9 }, new object[] { "A12", 8 },
                        new object[] { "B6", 8 }
                    });
                env.AssertListenerNotInvoked("s0");

                SendEvents(
                    env,
                    new object[][] {
                        new object[] { "A13", 1 }, new object[] { "A14", 1 }, new object[] { "A15", 1 },
                        new object[] { "A16", 1 }, new object[] { "B7", 5 }
                    });
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "A13", "A14", "B7" });

                SendEvents(
                    env,
                    new object[][] { new object[] { "A17", 1 }, new object[] { "A18", 1 }, new object[] { "B8", 1 } });
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void TryMeasuresClausePresence(
            RegressionEnvironment env,
            AtomicLong milestone,
            string measures)
        {
            var text = "@name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       " partition by theString " +
                       " measures " +
                       measures +
                       " pattern (A+ B)" +
                       " define" +
                       " B as B.intPrimitive = A[0].intPrimitive" +
                       ")";
            env.CompileDeploy(text).AddListener("s0");

            SendEvents(env, new object[][] { new object[] { "A", 1 }, new object[] { "A", 0 } });
            env.AssertListenerNotInvoked("s0");

            SendEvents(env, new object[][] { new object[] { "B", 1 }, new object[] { "B", 1 } });
            env.AssertListenerInvoked("s0");

            env.MilestoneInc(milestone);

            SendEvents(env, new object[][] { new object[] { "A", 2 }, new object[] { "A", 3 } });
            env.AssertListenerNotInvoked("s0");

            SendEvents(env, new object[][] { new object[] { "B", 2 }, new object[] { "B", 2 } });
            env.AssertListenerInvoked("s0");

            env.UndeployAll();
        }

        private static void TryMultiDepends(
            RegressionEnvironment env,
            AtomicLong milestone,
            string pattern)
        {
            var fields = "a0,a1,b0,b1,c".SplitCsv();
            var text = "@name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       " measures A[0].theString as a0, A[1].theString as a1, B[0].theString as b0, B[1].theString as b1, C.theString as c" +
                       " pattern (" +
                       pattern +
                       ")" +
                       " define" +
                       " A as theString like 'A%', " +
                       " B as theString like 'B%'," +
                       " C as theString like 'C%' and " +
                       "   C.intPrimitive = A[0].intPrimitive and " +
                       "   C.intPrimitive = B[0].intPrimitive and " +
                       "   C.intPrimitive = A[1].intPrimitive and " +
                       "   C.intPrimitive = B[1].intPrimitive" +
                       ")";
            env.CompileDeploy(text).AddListener("s0");

            SendEvents(
                env,
                new object[][] {
                    new object[] { "A1", 1 }, new object[] { "B1", 1 }, new object[] { "A2", 1 },
                    new object[] { "B2", 1 }
                });
            env.SendEventBean(new SupportBean("C1", 1));
            env.AssertPropsNew("s0", fields, new object[] { "A1", "A2", "B1", "B2", "C1" });

            SendEvents(
                env,
                new object[][] {
                    new object[] { "A10", 1 }, new object[] { "B10", 1 }, new object[] { "A11", 1 },
                    new object[] { "B11", 2 }, new object[] { "C2", 2 }
                });
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            SendEvents(
                env,
                new object[][] {
                    new object[] { "A20", 2 }, new object[] { "B20", 2 }, new object[] { "A21", 1 },
                    new object[] { "B21", 2 }, new object[] { "C3", 2 }
                });
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void SendEvents(
            RegressionEnvironment env,
            object[][] objects)
        {
            foreach (var @object in objects) {
                env.SendEventBean(new SupportBean((string)@object[0], @object[1].AsInt32()));
            }
        }
    }
} // end of namespace