///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterFullJoin3Stream
    {
        private static readonly string[] FIELDS = new string[]
            { "s0.p00", "s0.p01", "s1.p10", "s1.p11", "s2.p20", "s2.p21" };

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithsMulticolumn(execs);
            Withs(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoin2Sides());
            return execs;
        }

        public static IList<RegressionExecution> WithsMulticolumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoin2SidesMulticolumn());
            return execs;
        }

        private class EPLJoinFullJoin2SidesMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.OBJECTARRAY, milestone);
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.MAP, milestone);
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.DEFAULT, milestone);
            }

            private static void TryAssertionFullJoin_2sides_multicolumn(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum,
                AtomicLong milestone)
            {
                var fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".SplitCsv();

                var epl =
                    $"{eventRepresentationEnum.GetAnnotationTextWJsonProvided(typeof(MyLocalJsonProvided))} @name('s0') select * from SupportBean_S0#length(1000) as s0  full outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11 full outer join SupportBean_S2#length(1000) as s2 on s0.p00 = s2.p20 and s0.p01 = s2.p21";
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean_S1(10, "A_1", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 10, "A_1", "B_1", null, null, null });

                env.SendEventBean(new SupportBean_S1(11, "A_2", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 11, "A_2", "B_1", null, null, null });

                env.SendEventBean(new SupportBean_S1(12, "A_1", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 12, "A_1", "B_2", null, null, null });

                env.SendEventBean(new SupportBean_S1(13, "A_2", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 13, "A_2", "B_2", null, null, null });

                env.SendEventBean(new SupportBean_S2(20, "A_1", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, 20, "A_1", "B_1" });

                env.SendEventBean(new SupportBean_S2(21, "A_2", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, 21, "A_2", "B_1" });

                env.SendEventBean(new SupportBean_S2(22, "A_1", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, 22, "A_1", "B_2" });

                env.SendEventBean(new SupportBean_S2(23, "A_2", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, 23, "A_2", "B_2" });

                env.SendEventBean(new SupportBean_S0(1, "A_3", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 1, "A_3", "B_3", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(2, "A_1", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 2, "A_1", "B_3", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(3, "A_3", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { 3, "A_3", "B_1", null, null, null, null, null, null });

                env.SendEventBean(new SupportBean_S0(4, "A_2", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { 4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2" });

                env.SendEventBean(new SupportBean_S0(5, "A_2", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { 5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1" });

                env.SendEventBean(new SupportBean_S1(14, "A_4", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 14, "A_4", "B_3", null, null, null });

                env.SendEventBean(new SupportBean_S1(15, "A_1", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 2, "A_1", "B_3", 15, "A_1", "B_3", null, null, null });

                env.SendEventBean(new SupportBean_S2(24, "A_1", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { 2, "A_1", "B_3", 15, "A_1", "B_3", 24, "A_1", "B_3" });

                env.SendEventBean(new SupportBean_S2(25, "A_2", "B_3"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, null, null, null, 25, "A_2", "B_3" });

                env.UndeployAll();
            }
        }

        private class EPLJoinFullJoin2Sides : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Query:
                // s0
                var joinStatement = "@name('s0') select * from " +
                                    "SupportBean_S0#length(1000) as s0 " +
                                    " full outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
                                    " full outer join SupportBean_S2#length(1000) as s2 on s0.p00 = s2.p20 ";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");

                TryAssertsFullJoin_2sides(env);

                env.UndeployAll();
            }
        }

        private static void TryAssertsFullJoin_2sides(RegressionEnvironment env)
        {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            var s1Events = SupportBean_S1.MakeS1("A", new string[] { "A-s1-1", "A-s1-2" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { null, s1Events[1], null } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { null, null, "A", "A-s1-1", null, null },
                    new object[] { null, null, "A", "A-s1-2", null, null }
                });

            var s2Events = SupportBean_S2.MakeS2("A", new string[] { "A-s2-1", "A-s2-2" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { null, null, s2Events[1] } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { null, null, "A", "A-s1-1", null, null },
                    new object[] { null, null, "A", "A-s1-2", null, null },
                    new object[] { null, null, null, null, "A", "A-s2-1" },
                    new object[] { null, null, null, null, "A", "A-s2-2" }
                });

            var s0Events = SupportBean_S0.MakeS0("A", new string[] { "A-s0-1" });
            SendEvent(env, s0Events);
            var expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
                new object[] { s0Events[0], s1Events[1], s2Events[1] },
            };
            AssertListenerUnd(env, expected);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2" }
                });

            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2" },
                    new object[] { "B", "B-s0-1", null, null, null, null }
                });

            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-2" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2" },
                    new object[] { "B", "B-s0-1", null, null, null, null },
                    new object[] { "B", "B-s0-2", null, null, null, null }
                });

            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[] { "C-s1-1" });
            SendEventsAndReset(env, s1Events);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2" },
                    new object[] { "B", "B-s0-1", null, null, null, null },
                    new object[] { "B", "B-s0-2", null, null, null, null },
                    new object[] { null, null, "C", "C-s1-1", null, null }
                });

            s0Events = SupportBean_S0.MakeS0("C", new string[] { "C-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], null } });
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                FIELDS,
                new object[][] {
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2" },
                    new object[] { "A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2" },
                    new object[] { "B", "B-s0-1", null, null, null, null },
                    new object[] { "B", "B-s0-2", null, null, null, null },
                    new object[] { "C", "C-s0-1", "C", "C-s1-1", null, null }
                });

            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[] { "D-s1-1", "D-s1-2" });
            SendEventsAndReset(env, s1Events);

            s0Events = SupportBean_S0.MakeS0("D", new string[] { "D-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], null },
                    new object[] { s0Events[0], s1Events[1], null }
                });

            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new string[] { "E-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("E", new string[] { "E-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });

            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new string[] { "F-s2-1", "F-s2-2" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("F", new string[] { "F-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], null, s2Events[0] },
                    new object[] { s0Events[0], null, s2Events[1] }
                });

            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[] { "G-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new string[] { "G-s2-1", "G-s2-2" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("G", new string[] { "G-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[] { "H-s1-1", "H-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("H", new string[] { "H-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("H", new string[] { "H-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new string[] { "I-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("I", new string[] { "I-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("I", new string[] { "I-s0-2" });
            SendEvent(env, s0Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new string[] { "Q-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s2Events = SupportBean_S2.MakeS2("Q", new string[] { "Q-s2-1", "Q-s2-2" });
            SendEvent(env, s2Events[0]);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });
            SendEvent(env, s2Events[1]);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[1] } });

            s1Events = SupportBean_S1.MakeS1("Q", new string[] { "Q-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new string[] { "R-s2-1", "R-s2-2" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { null, null, s2Events[1] } });

            s1Events = SupportBean_S1.MakeS1("R", new string[] { "R-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { null, s1Events[0], null } });

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[] { "S-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s1Events = SupportBean_S1.MakeS1("S", new string[] { "S-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], null } });

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new string[] { "T-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, null } });

            s2Events = SupportBean_S2.MakeS2("T", new string[] { "T-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("T", new string[] { "T-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } });

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[] { "U-s0-1", "U-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new string[] { "U-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], null },
                new object[] { s0Events[1], s1Events[0], null },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[] { "V-s0-1", "V-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("V", new string[] { "V-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("V", new string[] { "V-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[] { "W-s0-1", "W-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("W", new string[] { "W-s2-1", "W-s2-2" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("W", new string[] { "W-s1-1" });
            SendEvent(env, s1Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[0], s2Events[1] },
                new object[] { s0Events[1], s1Events[0], s2Events[1] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new string[] { "J-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("J", new string[] { "J-s1-1", "J-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("J", new string[] { "J-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new string[] { "K-s1-1", "K-s1-2" });
            SendEventsAndReset(env, s2Events);

            s2Events = SupportBean_S2.MakeS2("K", new string[] { "K-s2-1" });
            SendEventsAndReset(env, s2Events);

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new string[] { "L-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("L", new string[] { "L-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], null, s2Events[0] } });

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[] { "M-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new string[] { "M-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new string[] { "M-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(env, new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } });

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[] { "N-s0-1", "N-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("N", new string[] { "N-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], null, s2Events[0] },
                new object[] { s0Events[1], null, s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new string[] { "O-s0-1", "O-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new string[] { "O-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new string[] { "O-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
            };
            AssertListenerUnd(env, expected);

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new string[] { "P-s0-1", "P-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new string[] { "P-s1-1", "P-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new string[] { "P-s2-1" });
            SendEvent(env, s2Events);
            expected = new object[][] {
                new object[] { s0Events[0], s1Events[0], s2Events[0] },
                new object[] { s0Events[1], s1Events[0], s2Events[0] },
                new object[] { s0Events[0], s1Events[1], s2Events[0] },
                new object[] { s0Events[1], s1Events[1], s2Events[0] },
            };
            AssertListenerUnd(env, expected);
        }

        private static void AssertListenerUnd(
            RegressionEnvironment env,
            object[][] expected)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var und = ArrayHandlingUtil.GetUnderlyingEvents(
                        listener.GetAndResetLastNewData(),
                        new string[] { "s0", "s1", "s2" });
                    EPAssertionUtil.AssertSameAnyOrder(expected, und);
                });
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static void SendEventsAndReset(
            RegressionEnvironment env,
            object[] events)
        {
            SendEvent(env, events);
            env.ListenerReset("s0");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private class MyLocalJsonProvided
        {
            public SupportBean_S0 s0;
            public SupportBean_S1 s1;
            public SupportBean_S2 s2;
        }
    }
} // end of namespace