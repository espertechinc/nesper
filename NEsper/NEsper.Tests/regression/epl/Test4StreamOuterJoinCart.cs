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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;


namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test4StreamOuterJoinCart  {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
        private readonly static String EVENT_S3 = typeof(SupportBean_S3).FullName;
    
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
        public void TestRoot_s0() {
            /// <summary>Query:  -&gt; s1 s0      -&gt; s2 -&gt; s3 </summary>
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s0.P00 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s1() {
            /// <summary>Query:  -&gt; s1 s0      -&gt; s2 -&gt; s3 </summary>
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:length(1000) as s1 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s0.P00 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s2() {
            /// <summary>Query:  -&gt; s1 s0      -&gt; s2 -&gt; s3 </summary>
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s2.p20 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s0.P00 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s3() {
            /// <summary>Query:  -&gt; s1 s0      -&gt; s2 -&gt; s3 </summary>
            String joinStatement = "select * from " +
                    EVENT_S3 + ".win:length(1000) as s3 " +
                    " right outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s3.p30 " +
                    " left outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s0.P00 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        private void RunAsserts() {
            Object[] s0Events;
            Object[] s1Events;
            Object[] s2Events;
            Object[] s3Events;
    
            // Test s0 and s1=1, s2=1, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("A", new[]{"A-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("A", new[]{"A-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("A", new[]{"A-s3-1"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("A", new[]{"A-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}}, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new[]{"B-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("B", new[]{"B-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {s0Events[0], s1Events[0], null, null}}, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new[]{"C-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("C", new[]{"C-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("C", new[]{"C-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0], null}}, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new[]{"D-s1-1", "D-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("D", new[]{"D-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("D", new[]{"D-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], null}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("E", new[]{"E-s1-1", "E-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("E", new[]{"E-s2-1", "E-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("E", new[]{"E-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], null},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], null},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], null}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new[]{"F-s1-1", "F-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("F", new[]{"F-s2-1", "F-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("F", new[]{"F-s3-1"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("F", new[]{"F-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[]{"G-s1-1", "G-s1-2"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("G", new[]{"G-s2-1", "G-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("G", new[]{"G-s3-1", "G-s3-2"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("G", new[]{"G-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=3
            //
            s1Events = SupportBean_S1.MakeS1("H", new[]{"H-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s2Events = SupportBean_S2.MakeS2("H", new[]{"H-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("H", new[]{"H-s3-1", "H-s3-2", "H-s3-3"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s0Events = SupportBean_S0.MakeS0("H", new[]{"H-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2]}
            }, GetAndResetNewEvents());
    
            // Test s3 and s0=0, s1=0, s2=0
            //
            s3Events = SupportBean_S3.MakeS3("I", new[]{"I-s3-1"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s3 and s0=0, s1=0, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("J", new[]{"J-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("J", new[]{"J-s3-1"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s3 and s0=0, s1=1, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("K", new[]{"K-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s1Events = SupportBean_S1.MakeS1("K", new[]{"K-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            s3Events = SupportBean_S3.MakeS3("K", new[]{"K-s3-1"});
            SendEvent(s3Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s3 and s0=1, s1=1, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new[]{"M-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new[]{"M-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new[]{"M-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("M", new[]{"M-s3-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s3 and s0=1, s1=2, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("Count", new[]{"Count-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("Count", new[]{"Count-s1-1", "Count-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("Count", new[]{"Count-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("Count", new[]{"Count-s3-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s3 and s0=1, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("O", new[]{"O-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new[]{"O-s1-1", "O-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("O", new[]{"O-s2-1", "O-s2-2", "O-s2-3"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("O", new[]{"O-s3-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s3 and s0=2, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("P", new[]{"P-s0-1", "P-s0-2"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new[]{"P-s1-1", "P-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("P", new[]{"P-s2-1", "P-s2-2", "P-s2-3"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("P", new[]{"P-s3-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[2], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[2], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=1, s3=0
            //
            s2Events = SupportBean_S2.MakeS2("Q", new[]{"Q-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("Q", new[]{"Q-s1-1"});
            SendEvent(s1Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s1 and s0=2, s2=1, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("R", new[]{"R-s0-1", "R-s0-2"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("R", new[]{"R-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("R", new[]{"R-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], null}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=2, s2=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("S", new[]{"S-s0-1", "S-s0-2"});
            SendEventsAndReset(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("S", new[]{"S-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("S", new[]{"S-s3-1", "S-s3-1"});
            SendEventsAndReset(s3Events);
    
            s1Events = SupportBean_S1.MakeS1("S", new[]{"S-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=0, s1=0, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("T", new[]{"T-s3-1"});
            SendEventsAndReset(s3Events);
    
            s2Events = SupportBean_S2.MakeS2("T", new[]{"T-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s2 and s0=0, s1=1, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("U", new[]{"U-s3-1"});
            SendEventsAndReset(s3Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new[]{"U-s1-1"});
            SendEvent(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("U", new[]{"U-s2-1"});
            SendEvent(s2Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s2 and s0=1, s1=1, s3=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new[]{"V-s0-1"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new[]{"V-s1-1"});
            SendEvent(s1Events);
    
            s3Events = SupportBean_S3.MakeS3("V", new[]{"V-s3-1"});
            SendEventsAndReset(s3Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new[]{"V-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=2, s1=2, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("W", new[]{"W-s0-1", "W-s0-2"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new[]{"W-s1-1", "W-s1-2"});
            SendEvent(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new[]{"W-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], null},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], null},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], null}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=2, s1=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("X", new[]{"X-s0-1", "X-s0-2"});
            SendEventsAndReset(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("X", new[]{"X-s1-1", "X-s1-2"});
            SendEvent(s1Events);
    
            s3Events = SupportBean_S3.MakeS3("X", new[]{"X-s3-1", "X-s3-2"});
            SendEventsAndReset(s3Events);
    
            s2Events = SupportBean_S2.MakeS2("X", new[]{"X-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1]}
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
            updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[]{"s0", "s1", "s2", "s3"});
        }
    }
}
