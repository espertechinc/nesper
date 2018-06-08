///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.join
{
    public class ExecOuterFullJoin3Stream : RegressionExecution {
        private static readonly string[] FIELDS = new string[]{"s0.p00", "s0.p01", "s1.p10", "s1.p11", "s2.p20", "s2.p21"};
        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionFullJoin_2sides_multicolumn(epService);
            RunAssertionFullJoin_2sides(epService);
        }
    
        private void RunAssertionFullJoin_2sides_multicolumn(EPServiceProvider epService) {
            TryAssertionFullJoin_2sides_multicolumn(epService, EventRepresentationChoice.ARRAY);
            TryAssertionFullJoin_2sides_multicolumn(epService, EventRepresentationChoice.MAP);
            TryAssertionFullJoin_2sides_multicolumn(epService, EventRepresentationChoice.DEFAULT);
        }
    
        private void TryAssertionFullJoin_2sides_multicolumn(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            string[] fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".Split(',');
    
            string epl = eventRepresentationEnum.GetAnnotationText() + " select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " full outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
                    " full outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 and s0.p01 = s2.p21";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 10, "A_1", "B_1", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 11, "A_2", "B_1", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 12, "A_1", "B_2", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(13, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 13, "A_2", "B_2", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(20, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, 20, "A_1", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(21, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, 21, "A_2", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(22, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, 22, "A_1", "B_2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(23, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, 23, "A_2", "B_2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_3", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, "A_3", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "A_1", "B_3", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "A_3", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, "A_3", "B_1", null, null, null, null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(14, "A_4", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, 14, "A_4", "B_3", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(15, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "A_1", "B_3", 15, "A_1", "B_3", null, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(24, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2, "A_1", "B_3", 15, "A_1", "B_3", 24, "A_1", "B_3"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S2(25, "A_2", "B_3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null, null, 25, "A_2", "B_3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFullJoin_2sides(EPServiceProvider epService) {
            /// <summary>
            /// Query:
            /// s0
            /// s1 <->      <-> s2
            /// </summary>
            string joinStatement = "select * from " +
                    EVENT_S0 + "#length(1000) as s0 " +
                    " full outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
                    " full outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(joinStatement);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertsFullJoin_2sides(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertsFullJoin_2sides(EPServiceProvider epService, SupportUpdateListener listener, EPStatement joinView) {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            object[] s1Events = SupportBean_S1.MakeS1("A", new string[]{"A-s1-1", "A-s1-2"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {null, s1Events[1], null}}, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, "A", "A-s1-1", null, null},
                            new object[] {null, null, "A", "A-s1-2", null, null}});
    
            object[] s2Events = SupportBean_S2.MakeS2("A", new string[]{"A-s2-1", "A-s2-2"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {null, null, s2Events[1]}}, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {null, null, "A", "A-s1-1", null, null},
                            new object[] {null, null, "A", "A-s1-2", null, null},
                            new object[] {null, null, null, null, "A", "A-s2-1"},
                            new object[] {null, null, null, null, "A", "A-s2-2"}});
    
            object[] s0Events = SupportBean_S0.MakeS0("A", new string[]{"A-s0-1"});
            SendEvent(epService, s0Events);
            var expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new object[] {s0Events[0], s1Events[1], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"}});
    
            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new string[]{"B-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new object[] {"B", "B-s0-1", null, null, null, null}});
    
            s0Events = SupportBean_S0.MakeS0("B", new string[]{"B-s0-2"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new object[] {"B", "B-s0-1", null, null, null, null},
                            new object[] {"B", "B-s0-2", null, null, null, null}});
    
            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[]{"C-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new object[] {"B", "B-s0-1", null, null, null, null},
                            new object[] {"B", "B-s0-2", null, null, null, null},
                            new object[] {null, null, "C", "C-s1-1", null, null}});
    
            s0Events = SupportBean_S0.MakeS0("C", new string[]{"C-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents(listener));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), FIELDS,
                    new object[][]{new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new object[] {"B", "B-s0-1", null, null, null, null},
                            new object[] {"B", "B-s0-2", null, null, null, null},
                            new object[] {"C", "C-s0-1", "C", "C-s1-1", null, null}});
    
            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[]{"D-s1-1", "D-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s0Events = SupportBean_S0.MakeS0("D", new string[]{"D-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{
                    new object[] {s0Events[0], s1Events[0], null},
                    new object[] {s0Events[0], s1Events[1], null}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new string[]{"E-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s0Events = SupportBean_S0.MakeS0("E", new string[]{"E-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new string[]{"F-s2-1", "F-s2-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s0Events = SupportBean_S0.MakeS0("F", new string[]{"F-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{
                    new object[] {s0Events[0], null, s2Events[0]},
                    new object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[]{"G-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("G", new string[]{"G-s2-1", "G-s2-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s0Events = SupportBean_S0.MakeS0("G", new string[]{"G-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[]{"H-s1-1", "H-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("H", new string[]{"H-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s0Events = SupportBean_S0.MakeS0("H", new string[]{"H-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new string[]{"I-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("I", new string[]{"I-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s0Events = SupportBean_S0.MakeS0("I", new string[]{"I-s0-2"});
            SendEvent(epService, s0Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new string[]{"Q-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s2Events = SupportBean_S2.MakeS2("Q", new string[]{"Q-s2-1", "Q-s2-2"});
            SendEvent(epService, s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
            SendEvent(epService, s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents(listener));
    
            s1Events = SupportBean_S1.MakeS1("Q", new string[]{"Q-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new string[]{"R-s2-1", "R-s2-2"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {null, null, s2Events[1]}}, GetAndResetNewEvents(listener));
    
            s1Events = SupportBean_S1.MakeS1("R", new string[]{"R-s1-1"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {null, s1Events[0], null}}, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[]{"S-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s1Events = SupportBean_S1.MakeS1("S", new string[]{"S-s1-1"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new string[]{"T-s0-1"});
            SendEvent(epService, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, null}}, GetAndResetNewEvents(listener));
    
            s2Events = SupportBean_S2.MakeS2("T", new string[]{"T-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("T", new string[]{"T-s1-1"});
            SendEvent(epService, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[]{"U-s0-1", "U-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new string[]{"U-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], null},
                    new object[] {s0Events[1], s1Events[0], null},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[]{"V-s0-1", "V-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new string[]{"V-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new string[]{"V-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[]{"W-s0-1", "W-s0-2"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new string[]{"W-s2-1", "W-s2-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new string[]{"W-s1-1"});
            SendEvent(epService, s1Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new object[] {s0Events[1], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new string[]{"J-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("J", new string[]{"J-s1-1", "J-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new string[]{"J-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new string[]{"K-s1-1", "K-s1-2"});
            SendEventsAndReset(epService, listener, s2Events);
    
            s2Events = SupportBean_S2.MakeS2("K", new string[]{"K-s2-1"});
            SendEventsAndReset(epService, listener, s2Events);
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new string[]{"L-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("L", new string[]{"L-s2-1"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[]{"M-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new string[]{"M-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new string[]{"M-s2-1"});
            SendEvent(epService, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][]{new object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[]{"N-s0-1", "N-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s2Events = SupportBean_S2.MakeS2("N", new string[]{"N-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], null, s2Events[0]},
                    new object[] {s0Events[1], null, s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new string[]{"O-s0-1", "O-s0-1"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new string[]{"O-s1-1"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("O", new string[]{"O-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new string[]{"P-s0-1", "P-s0-2"});
            SendEventsAndReset(epService, listener, s0Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new string[]{"P-s1-1", "P-s1-2"});
            SendEventsAndReset(epService, listener, s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new string[]{"P-s2-1"});
            SendEvent(epService, s2Events);
            expected = new object[][]{
                    new object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new object[] {s0Events[1], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(listener));
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventsAndReset(EPServiceProvider epService, SupportUpdateListener listener, object[] events) {
            SendEvent(epService, events);
            listener.Reset();
        }
    
        private void SendEvent(EPServiceProvider epService, object[] events) {
            for (int i = 0; i < events.Length; i++) {
                epService.EPRuntime.SendEvent(events[i]);
            }
        }
    
        private object[][] GetAndResetNewEvents(SupportUpdateListener listener) {
            EventBean[] newEvents = listener.LastNewData;
            listener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new string[]{"s0", "s1", "s2"});
        }
    }
} // end of namespace
