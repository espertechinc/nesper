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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test3StreamOuterJoinVarB  {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
    
        [SetUp]
        public void SetUp() {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
            updateListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            updateListener = null;
        }
    
        [Test]
        public void TestOuterInnerJoin_root_s0() {
            /// <summary>Query: s0 s1 &lt;-      &lt;- s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " right outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestOuterInnerJoin_root_s1() {
            /// <summary>Query: s0 s1 &lt;-      &lt;- s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:length(1000) as s1 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " right outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestOuterInnerJoin_root_s2() {
            /// <summary>Query: s0 s1 &lt;-      &lt;- s2 </summary>
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s2.p20 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        private void RunAsserts() {
            Object[] s0Events = null;
            Object[] s1Events = null;
            Object[] s2Events = null;
    
            // Test s0 ... s1 with 1 rows, s2 with 0 rows
            //
            s1Events = SupportBean_S1.MakeS1("A", new[]{"A-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("A", new[]{"A-s0-1"});
            SendEvent(s0Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s0 ... s1 with 0 rows, s2 with 1 rows
            //
            s2Events = SupportBean_S2.MakeS2("B", new[]{"B-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("B", new[]{"B-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], null, s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s0 ... s1 with 1 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("C", new[]{"C-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("C", new[]{"C-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("C", new[]{"C-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0]}}, GetAndResetNewEvents());
    
            // Test s0 ... s1 with 2 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("D", new[]{"D-s1-1", "D-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("D", new[]{"D-s2-1"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("D", new[]{"D-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 ... s1 with 2 rows, s2 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("E", new[]{"E-s1-1", "E-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("E", new[]{"E-s2-1", "E-s2-2"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("E", new[]{"E-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s0 ... s1 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("F", new[]{"F-s2-1", "F-s2-2"});
            SendEventsAndReset(s2Events);
    
            s0Events = SupportBean_S0.MakeS0("F", new[]{"F-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[0], null, s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s1 ... s0 with 0 rows, s2 with 1 rows
            //
            s2Events = SupportBean_S2.MakeS2("H", new[]{"H-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("H", new[]{"H-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s1 ... s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("I", new[]{"I-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("I", new[]{"I-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s1 ... s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new[]{"J-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new[]{"J-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("J", new[]{"J-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 ... s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("K", new[]{"K-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("K", new[]{"K-s2-1", "K-s2-2"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("K", new[]{"K-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s1 ... s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new[]{"L-s0-1", "L-s0-2"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("L", new[]{"L-s2-1", "L-s2-2"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("L", new[]{"L-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s0 with 0 rows, s1 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("P", new[]{"P-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new[]{"P-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, null, s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 0 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new[]{"Q-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("Q", new[]{"Q-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 1 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("R", new[]{"R-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("R", new[]{"R-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("R", new[]{"R-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 2 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new[]{"S-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("S", new[]{"S-s1-1", "S-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("S", new[]{"S-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 0 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new[]{"T-s0-1", "T-s0-1"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("T", new[]{"T-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], null, s2Events[0]},
                    new Object[] {s0Events[1], null, s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 1 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new[]{"U-s0-1", "U-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new[]{"U-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("U", new[]{"U-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 ... s1 with 2 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new[]{"V-s0-1", "V-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new[]{"V-s1-1", "V-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new[]{"V-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0]}
            }, GetAndResetNewEvents());
        }
    
        private void SendEvent(Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventsAndReset(Object[] events) {
            SendEvent(events);
            updateListener.Reset();
        }
    
        private void SendEvent(Object[] events) {
            for (int i = 0; i < events.Length; i++) {
                epService.EPRuntime.SendEvent(events[i]);
            }
        }
    
        private Object[][] GetAndResetNewEvents() {
            EventBean[] newEvents = updateListener.LastNewData;
            Assert.NotNull(newEvents, "no events received");
            updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[]{"s0", "s1", "s2"});
        }
    }
}
