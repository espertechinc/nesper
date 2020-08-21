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
        private static readonly string[] FIELDS = {"S0.P00", "S0.P01", "S1.P10", "S1.P11", "S2.P20", "S2.P21"};

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinFullJoin2SidesMulticolumn());
            execs.Add(new EPLJoinFullJoin2Sides());
            return execs;
        }

        private static void TryAssertsFullJoin_2sides(RegressionEnvironment env)
        {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            var s1Events = SupportBean_S1.MakeS1("A", new[] {"A-s1-1", "A-s1-2"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {null, s1Events[1], null}}, GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {null, null, "A", "A-s1-1", null, null},
                    new object[] {null, null, "A", "A-s1-2", null, null}
                });

            var s2Events = SupportBean_S2.MakeS2("A", new[] {"A-s2-1", "A-s2-2"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {null, null, s2Events[1]}}, GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {null, null, "A", "A-s1-1", null, null},
                    new object[] {null, null, "A", "A-s1-2", null, null},
                    new object[] {null, null, null, null, "A", "A-s2-1"},
                    new object[] {null, null, null, null, "A", "A-s2-2"}
                });

            var s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            object[][] expected = {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]},
                new[] {s0Events[0], s1Events[1], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"}
                });

            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                    new object[] {"B", "B-s0-1", null, null, null, null}
                });

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-2"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                    new object[] {"B", "B-s0-1", null, null, null, null},
                    new object[] {"B", "B-s0-2", null, null, null, null}
                });

            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1"});
            SendEventsAndReset(env, s1Events);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                    new object[] {"B", "B-s0-1", null, null, null, null},
                    new object[] {"B", "B-s0-2", null, null, null, null},
                    new object[] {null, null, "C", "C-s1-1", null, null}
                });

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], null}},
                GetAndResetNewEvents(env));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                FIELDS,
                new[] {
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                    new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                    new object[] {"B", "B-s0-1", null, null, null, null},
                    new object[] {"B", "B-s0-2", null, null, null, null},
                    new object[] {"C", "C-s0-1", "C", "C-s1-1", null, null}
                });

            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1", "D-s1-2"});
            SendEventsAndReset(env, s1Events);

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], null},
                    new[] {s0Events[0], s1Events[1], null}
                },
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1", "F-s2-2"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0]},
                    new[] {s0Events[0], null, s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new[] {"G-s2-1", "G-s2-2"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1", "H-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("I", new[] {"I-s2-1"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new[] {"Q-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s2Events = SupportBean_S2.MakeS2("Q", new[] {"Q-s2-1", "Q-s2-2"});
            SendEvent(env, s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));
            SendEvent(env, s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[1]}},
                GetAndResetNewEvents(env));

            s1Events = SupportBean_S1.MakeS1("Q", new[] {"Q-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new[] {"R-s2-1", "R-s2-2"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {null, null, s2Events[1]}}, GetAndResetNewEvents(env));

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {null, s1Events[0], null}}, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], null}},
                GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new[] {"T-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s2Events = SupportBean_S2.MakeS2("T", new[] {"T-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("T", new[] {"T-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new[] {"U-s0-1", "U-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new[] {"U-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], null},
                new[] {s0Events[1], s1Events[0], null}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1", "V-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("V", new[] {"V-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new[] {"W-s0-1", "W-s0-2"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("W", new[] {"W-s2-1", "W-s2-2"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("W", new[] {"W-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]},
                new[] {s0Events[1], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new[] {"J-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("J", new[] {"J-s1-1", "J-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("J", new[] {"J-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1", "K-s1-2"});
            SendEventsAndReset(env, s2Events);

            s2Events = SupportBean_S2.MakeS2("K", new[] {"K-s2-1"});
            SendEventsAndReset(env, s2Events);

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("L", new[] {"L-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new[] {"M-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s0-1", "N-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("N", new[] {"N-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], null, s2Events[0]},
                new[] {s0Events[1], null, s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1", "O-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new[] {"P-s0-1", "P-s0-2"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1", "P-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new[] {"P-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]},
                new[] {s0Events[1], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));
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
            env.Listener("s0").Reset();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private static object[][] GetAndResetNewEvents(RegressionEnvironment env)
        {
            var newEvents = env.Listener("s0").LastNewData;
            env.Listener("s0").Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] {"S0", "S1", "S2"});
        }

        internal class EPLJoinFullJoin2SidesMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.OBJECTARRAY);
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.MAP);
                TryAssertionFullJoin_2sides_multicolumn(env, EventRepresentationChoice.DEFAULT);
            }

            private static void TryAssertionFullJoin_2sides_multicolumn(
                RegressionEnvironment env,
                EventRepresentationChoice eventRepresentationEnum)
            {
                var fields = new [] { "S0.Id"," S0.P00"," S0.P01"," S1.Id"," S1.P10"," S1.P11"," S2.Id"," S2.P20"," S2.P21" };

                var epl = eventRepresentationEnum.GetAnnotationTextWJsonProvided<MyLocalJsonProvided>() +
                          " @Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " full outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 and S0.P01 = S1.P11" +
                          " full outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 and S0.P01 = S2.P21";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S1(10, "A_1", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 10, "A_1", "B_1", null, null, null});

                env.SendEventBean(new SupportBean_S1(11, "A_2", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 11, "A_2", "B_1", null, null, null});

                env.SendEventBean(new SupportBean_S1(12, "A_1", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 12, "A_1", "B_2", null, null, null});

                env.SendEventBean(new SupportBean_S1(13, "A_2", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 13, "A_2", "B_2", null, null, null});

                env.SendEventBean(new SupportBean_S2(20, "A_1", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, 20, "A_1", "B_1"});

                env.SendEventBean(new SupportBean_S2(21, "A_2", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, 21, "A_2", "B_1"});

                env.SendEventBean(new SupportBean_S2(22, "A_1", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, 22, "A_1", "B_2"});

                env.SendEventBean(new SupportBean_S2(23, "A_2", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, 23, "A_2", "B_2"});

                env.SendEventBean(new SupportBean_S0(1, "A_3", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A_3", "B_3", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(2, "A_1", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "A_1", "B_3", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(3, "A_3", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, "A_3", "B_1", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(4, "A_2", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});

                env.SendEventBean(new SupportBean_S0(5, "A_2", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});

                env.SendEventBean(new SupportBean_S1(14, "A_4", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 14, "A_4", "B_3", null, null, null});

                env.SendEventBean(new SupportBean_S1(15, "A_1", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "A_1", "B_3", 15, "A_1", "B_3", null, null, null});

                env.SendEventBean(new SupportBean_S2(24, "A_1", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "A_1", "B_3", 15, "A_1", "B_3", 24, "A_1", "B_3"});

                env.SendEventBean(new SupportBean_S2(25, "A_2", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null, null, 25, "A_2", "B_3"});

                env.UndeployAll();
            }
        }

        internal class EPLJoinFullJoin2Sides : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var joinStatement = "@Name('s0') select * from " +
                                    "SupportBean_S0#length(1000) as S0 " +
                                    " full outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                                    " full outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 ";
                env.CompileDeployAddListenerMileZero(joinStatement, "s0");

                TryAssertsFullJoin_2sides(env);

                env.UndeployAll();
            }
        }

        internal class MyLocalJsonProvided
        {
            public SupportBean_S0 s0;
            public SupportBean_S1 s1;
            public SupportBean_S2 s2;
        }
    }
} // end of namespace