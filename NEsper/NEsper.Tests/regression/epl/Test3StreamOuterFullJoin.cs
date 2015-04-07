///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;
using com.espertech.esper.util;
using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test3StreamOuterFullJoin  {
        private readonly static String[] Fields = new String[]{"s0.P00", "s0.p01", "s1.p10", "s1.p11", "s2.p20", "s2.p21",};
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _updateListener;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _updateListener = null;
        }

        [Test]
        public void TestFullJoin_2sides_multicolumn()
        {
            RunAssertionFullJoin_2sides_multicolumn(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionFullJoin_2sides_multicolumn(EventRepresentationEnum.MAP);
            RunAssertionFullJoin_2sides_multicolumn(EventRepresentationEnum.DEFAULT);
        }

        private void RunAssertionFullJoin_2sides_multicolumn(EventRepresentationEnum eventRepresentationEnum)
        {
            String[] fields = "s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".Split(',');
    
            String joinStatement = eventRepresentationEnum.GetAnnotationText() + " select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " full outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 and s0.p01 = s1.p11" +
                    " full outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 and s0.p01 = s2.p21";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, 10, "A_1", "B_1", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, 11, "A_2", "B_1", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, 12, "A_1", "B_2", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, 13, "A_2", "B_2", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(20, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null, null, 20, "A_1", "B_1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(21, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null, null, 21, "A_2", "B_1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(22, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null, null, 22, "A_1", "B_2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(23, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null, null, 23, "A_2", "B_2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_3", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{1, "A_3", "B_3", null, null, null, null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{2, "A_1", "B_3", null, null, null, null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "A_3", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{3, "A_3", "B_1", null, null, null, null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A_2", "B_2"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(14, "A_4", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, 14, "A_4", "B_3", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(15, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{2, "A_1", "B_3", 15, "A_1", "B_3", null, null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(24, "A_1", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{2, "A_1", "B_3", 15, "A_1", "B_3", 24, "A_1", "B_3"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(25, "A_2", "B_3"));
            EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null, null, 25, "A_2", "B_3"});

            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestFullJoin_2sides() {
            /// <summary>Query: s0 s1 &lt;-&gt;      &lt;-&gt; s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " full outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " full outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += _updateListener.Update;
    
            RunAssertsFullJoin_2sides(joinView);
        }
    
        private void RunAssertsFullJoin_2sides(EPStatement joinView) {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            Object[] s1Events = SupportBean_S1.MakeS1("A", new String[]{"A-s1-1", "A-s1-2"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {null, s1Events[1], null}}, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {null, null, "A", "A-s1-1", null, null},
                            new Object[] {null, null, "A", "A-s1-2", null, null}
                    });
    
            Object[] s2Events = SupportBean_S2.MakeS2("A", new String[]{"A-s2-1", "A-s2-2"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {null, null, s2Events[1]}}, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {null, null, "A", "A-s1-1", null, null},
                            new Object[] {null, null, "A", "A-s1-2", null, null},
                            new Object[] {null, null, null, null, "A", "A-s2-1"},
                            new Object[] {null, null, null, null, "A", "A-s2-2"}
                    });
    
            Object[] s0Events = SupportBean_S0.MakeS0("A", new String[]{"A-s0-1"});
            SendEvent(s0Events);
            Object[][] expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"}
                    });
    
            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new String[]{"B-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new Object[] {"B", "B-s0-1", null, null, null, null}
                    });
    
            s0Events = SupportBean_S0.MakeS0("B", new String[]{"B-s0-2"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new Object[] {"B", "B-s0-1", null, null, null, null},
                            new Object[] {"B", "B-s0-2", null, null, null, null}
                    });
    
            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new String[]{"C-s1-1"});
            SendEventsAndReset(s1Events);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new Object[] {"B", "B-s0-1", null, null, null, null},
                            new Object[] {"B", "B-s0-2", null, null, null, null},
                            new Object[] {null, null, "C", "C-s1-1", null, null}
                    });
    
            s0Events = SupportBean_S0.MakeS0("C", new String[]{"C-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents());
            EPAssertionUtil.AssertPropsPerRowAnyOrder(joinView.GetEnumerator(), Fields,
                    new Object[][]{new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-1"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-1", "A", "A-s2-2"},
                            new Object[] {"A", "A-s0-1", "A", "A-s1-2", "A", "A-s2-2"},
                            new Object[] {"B", "B-s0-1", null, null, null, null},
                            new Object[] {"B", "B-s0-2", null, null, null, null},
                            new Object[] {"C", "C-s0-1", "C", "C-s1-1", null, null}
                    });
    
            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new String[]{"D-s1-1", "D-s1-2"});
            SendEventsAndReset(s1Events);
    
            s0Events = SupportBean_S0.MakeS0("D", new String[]{"D-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], null},
                    new Object[] {s0Events[0], s1Events[1], null}
            }, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new String[]{"E-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("E", new String[]{"E-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new String[]{"F-s2-1", "F-s2-2"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("F", new String[]{"F-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[0], null, s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new String[]{"G-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("G", new String[]{"G-s2-1", "G-s2-2"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("G", new String[]{"G-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new String[]{"H-s1-1", "H-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("H", new String[]{"H-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("H", new String[]{"H-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new String[]{"I-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("I", new String[]{"I-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("I", new String[]{"I-s0-2"});
            SendEvent(s0Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new String[]{"Q-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s2Events = SupportBean_S2.MakeS2("Q", new String[]{"Q-s2-1", "Q-s2-2"});
            SendEvent(s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
            SendEvent(s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[1]}}, GetAndResetNewEvents());
    
            s1Events = SupportBean_S1.MakeS1("Q", new String[]{"Q-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new String[]{"R-s2-1", "R-s2-2"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {null, null, s2Events[1]}}, GetAndResetNewEvents());
    
            s1Events = SupportBean_S1.MakeS1("R", new String[]{"R-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {null, s1Events[0], null}}, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new String[]{"S-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s1Events = SupportBean_S1.MakeS1("S", new String[]{"S-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], null}}, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new String[]{"T-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, null}}, GetAndResetNewEvents());
    
            s2Events = SupportBean_S2.MakeS2("T", new String[]{"T-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("T", new String[]{"T-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new String[]{"U-s0-1", "U-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new String[]{"U-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], null},
                    new Object[] {s0Events[1], s1Events[0], null},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new String[]{"V-s0-1", "V-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new String[]{"V-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new String[]{"V-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new String[]{"W-s0-1", "W-s0-2"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new String[]{"W-s2-1", "W-s2-2"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new String[]{"W-s1-1"});
            SendEvent(s1Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new String[]{"J-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("J", new String[]{"J-s1-1", "J-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new String[]{"J-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new String[]{"K-s1-1", "K-s1-2"});
            SendEventsAndReset(s2Events);
    
            s2Events = SupportBean_S2.MakeS2("K", new String[]{"K-s2-1"});
            SendEventsAndReset(s2Events);
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new String[]{"L-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("L", new String[]{"L-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new String[]{"M-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new String[]{"M-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new String[]{"M-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new String[]{"N-s0-1", "N-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("N", new String[]{"N-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[1], null, s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new String[]{"O-s0-1", "O-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new String[]{"O-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("O", new String[]{"O-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
    
            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new String[]{"P-s0-1", "P-s0-2"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new String[]{"P-s1-1", "P-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new String[]{"P-s2-1"});
            SendEvent(s2Events);
            expected = new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0]},
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
        }
    
        private void SendEvent(Object theEvent) {
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventsAndReset(Object[] events) {
            SendEvent(events);
            _updateListener.Reset();
        }
    
        private void SendEvent(Object[] events) {
            for (int i = 0; i < events.Length; i++) {
                _epService.EPRuntime.SendEvent(events[i]);
            }
        }
    
        private Object[][] GetAndResetNewEvents() {
            EventBean[] newEvents = _updateListener.LastNewData;
            _updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new String[]{"s0", "s1", "s2"});
        }
    }
}
