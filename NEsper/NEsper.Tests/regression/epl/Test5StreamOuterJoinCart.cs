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
    public class Test5StreamOuterJoinCart  {
        private EPServiceProvider epService;
        private SupportUpdateListener updateListener;
    
        private readonly static String EVENT_S0 = typeof(SupportBean_S0).FullName;
        private readonly static String EVENT_S1 = typeof(SupportBean_S1).FullName;
        private readonly static String EVENT_S2 = typeof(SupportBean_S2).FullName;
        private readonly static String EVENT_S3 = typeof(SupportBean_S3).FullName;
        private readonly static String EVENT_S4 = typeof(SupportBean_S4).FullName;
    
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
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S0 + ".win:length(1000) as s0 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s1() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:length(1000) as s1 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s1_order_2() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S1 + ".win:length(1000) as s1 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s2() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s2_order_2() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S2 + ".win:length(1000) as s2 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s3() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S3 + ".win:length(1000) as s3 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s3_order2() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S3 + ".win:length(1000) as s3 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S4 + ".win:length(1000) as s4 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s4() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S4 + ".win:length(1000) as s4 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        [Test]
        public void TestRoot_s4_order2() {
            /// <summary>Query: s0 &lt;- s1 -&gt; s2 -&gt; s3 -&gt; s4 </summary>
            String joinStatement = "select * from " +
                    EVENT_S4 + ".win:length(1000) as s4 " +
                    " right outer join " + EVENT_S1 + ".win:length(1000) as s1 on s1.p10 = s4.p40 " +
                    " left outer join " + EVENT_S0 + ".win:length(1000) as s0 on s0.P00 = s1.p10 " +
                    " left outer join " + EVENT_S2 + ".win:length(1000) as s2 on s1.p10 = s2.p20 " +
                    " left outer join " + EVENT_S3 + ".win:length(1000) as s3 on s1.p10 = s3.p30 ";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(joinStatement);
            joinView.Events += updateListener.Update;
    
            RunAsserts();
        }
    
        private void RunAsserts() {
            Object[] s0Events;
            Object[] s1Events;
            Object[] s2Events;
            Object[] s3Events;
            Object[] s4Events;
    
            // Test s0 and s1=0, s2=0, s3=0, s4=0
            //
            s0Events = SupportBean_S0.MakeS0("A", new String[]{"A-s0-1"});
            SendEvent(s0Events);
            Assert.IsFalse(updateListener.IsInvoked);
    
            // Test s0 and s1=1, s2=0, s3=0, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new String[]{"B-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {null, s1Events[0], null, null, null}}, GetAndResetNewEvents());
    
            s0Events = SupportBean_S0.MakeS0("B", new String[]{"B-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {s0Events[0], s1Events[0], null, null, null}}, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=0, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new String[]{"C-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("C", new String[]{"C-s2-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {null, s1Events[0], s2Events[0], null, null}}, GetAndResetNewEvents());
    
            s0Events = SupportBean_S0.MakeS0("C", new String[]{"C-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {s0Events[0], s1Events[0], s2Events[0], null, null}}, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=1, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new String[]{"D-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("D", new String[]{"D-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("D", new String[]{"D-s2-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {null, s1Events[0], s2Events[0], s3Events[0], null}}, GetAndResetNewEvents());
    
            s0Events = SupportBean_S0.MakeS0("D", new String[]{"D-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("E", new String[]{"E-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("E", new String[]{"E-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("E", new String[]{"E-s2-1"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("E", new String[]{"E-s2-1"});
            SendEvent(s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                    new Object[][]{new Object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]}}, GetAndResetNewEvents());
    
            s0Events = SupportBean_S0.MakeS0("E", new String[]{"E-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=1, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new String[]{"F-s1-1", "F-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("F", new String[]{"F-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("F", new String[]{"F-s3-1"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("F", new String[]{"F-s2-1"});
            SendEventsAndReset(s4Events);
    
            s0Events = SupportBean_S0.MakeS0("F", new String[]{"F-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("G", new String[]{"G-s1-1", "G-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("G", new String[]{"G-s2-1", "G-s2-2"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("G", new String[]{"G-s3-1"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("G", new String[]{"G-s2-1"});
            SendEventsAndReset(s4Events);
    
            s0Events = SupportBean_S0.MakeS0("G", new String[]{"G-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=2, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("H", new String[]{"H-s1-1", "H-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("H", new String[]{"H-s2-1", "H-s2-2"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("H", new String[]{"H-s3-1", "H-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("H", new String[]{"H-s2-1"});
            SendEventsAndReset(s4Events);
    
            s0Events = SupportBean_S0.MakeS0("H", new String[]{"H-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=2, s2=2, s3=2, s4=2
            //
            s1Events = SupportBean_S1.MakeS1("I", new String[]{"I-s1-1", "I-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("I", new String[]{"I-s2-1", "I-s2-2"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("I", new String[]{"I-s3-1", "I-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("I", new String[]{"I-s4-1", "I-s4-2"});
            SendEventsAndReset(s4Events);
    
            s0Events = SupportBean_S0.MakeS0("I", new String[]{"I-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s0 and s1=1, s2=1, s3=2, s4=3
            //
            s1Events = SupportBean_S1.MakeS1("J", new String[]{"J-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("J", new String[]{"J-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("J", new String[]{"J-s3-1", "J-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("J", new String[]{"J-s4-1", "J-s4-2", "J-s4-3"});
            SendEventsAndReset(s4Events);
    
            s0Events = SupportBean_S0.MakeS0("J", new String[]{"J-s0-1"});
            SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[2]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=1, s3=1, s4=1
            //
            s2Events = SupportBean_S2.MakeS2("K", new String[]{"K-s2-1"});
            SendEventsAndReset(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("K", new String[]{"K-s3-1"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("K", new String[]{"K-s4-1"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("K", new String[]{"K-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=1, s3=0, s4=1
            //
            s2Events = SupportBean_S2.MakeS2("L", new String[]{"L-s2-1"});
            SendEventsAndReset(s2Events);
    
            s4Events = SupportBean_S4.MakeS4("L", new String[]{"L-s4-1"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("L", new String[]{"L-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], s2Events[0], null, s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=2, s2=1, s3=0, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new String[]{"M-s0-1", "M-s0-2"});
            SendEvent(s0Events);
    
            s2Events = SupportBean_S2.MakeS2("M", new String[]{"M-s2-1"});
            SendEventsAndReset(s2Events);
    
            s4Events = SupportBean_S4.MakeS4("M", new String[]{"M-s4-1"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("M", new String[]{"M-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], null, s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=1, s2=0, s3=0, s4=0
            //
            s0Events = SupportBean_S0.MakeS0("Count", new String[]{"Count-s0-1"});
            SendEvent(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("Count", new String[]{"Count-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], null, null, null}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=0, s3=1, s4=0
            //
            s3Events = SupportBean_S3.MakeS3("O", new String[]{"O-s3-1"});
            SendEventsAndReset(s3Events);
    
            s1Events = SupportBean_S1.MakeS1("O", new String[]{"O-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], null, s3Events[0], null}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=0, s3=0, s4=1
            //
            s4Events = SupportBean_S4.MakeS4("P", new String[]{"P-s4-1"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("P", new String[]{"P-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], null, null, s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=0, s3=0, s4=2
            //
            s4Events = SupportBean_S4.MakeS4("Q", new String[]{"Q-s4-1", "Q-s4-2"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("Q", new String[]{"Q-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], null, null, s4Events[0]},
                    new Object[] {null, s1Events[0], null, null, s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=0, s3=2, s4=2
            //
            s3Events = SupportBean_S3.MakeS3("R", new String[]{"R-s3-1", "R-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("R", new String[]{"R-s4-1", "R-s4-2"});
            SendEventsAndReset(s4Events);
    
            s1Events = SupportBean_S1.MakeS1("R", new String[]{"R-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], null, s3Events[0], s4Events[0]},
                    new Object[] {null, s1Events[0], null, s3Events[1], s4Events[0]},
                    new Object[] {null, s1Events[0], null, s3Events[0], s4Events[1]},
                    new Object[] {null, s1Events[0], null, s3Events[1], s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s1 and s0=0, s2=2, s3=0, s4=2
            //
            s4Events = SupportBean_S4.MakeS4("S", new String[]{"S-s4-1", "S-s4-2"});
            SendEventsAndReset(s4Events);
    
            s2Events = SupportBean_S2.MakeS2("S", new String[]{"S-s2-1", "S-s2-1"});
            SendEventsAndReset(s2Events);
    
            s1Events = SupportBean_S1.MakeS1("S", new String[]{"S-s1-1"});
            SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], s2Events[0], null, s4Events[0]},
                    new Object[] {null, s1Events[0], s2Events[0], null, s4Events[1]},
                    new Object[] {null, s1Events[0], s2Events[1], null, s4Events[0]},
                    new Object[] {null, s1Events[0], s2Events[1], null, s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=1, s1=2, s3=0, s4=2
            //
            s0Events = SupportBean_S0.MakeS0("U", new String[]{"U-s0-1"});
            SendEvent(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("U", new String[]{"U-s1-1"});
            SendEventsAndReset(s1Events);
    
            s4Events = SupportBean_S4.MakeS4("U", new String[]{"U-s4-1", "U-s4-2"});
            SendEventsAndReset(s4Events);
    
            s2Events = SupportBean_S2.MakeS2("U", new String[]{"U-s1-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=3, s1=1, s3=2, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new String[]{"V-s0-1", "V-s0-2", "V-s0-3"});
            SendEvent(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("V", new String[]{"V-s1-1"});
            SendEventsAndReset(s1Events);
    
            s3Events = SupportBean_S3.MakeS3("V", new String[]{"V-s3-1", "V-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("V", new String[]{"V-s4-1"});
            SendEventsAndReset(s4Events);
    
            s2Events = SupportBean_S2.MakeS2("V", new String[]{"V-s1-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[2], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[2], s1Events[0], s2Events[0], s3Events[1], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s2 and s0=2, s1=2, s3=2, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("W", new String[]{"W-s0-1", "W-s0-2"});
            SendEvent(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("W", new String[]{"W-s1-1", "W-s1-2"});
            SendEventsAndReset(s1Events);
    
            s3Events = SupportBean_S3.MakeS3("W", new String[]{"W-s3-1", "W-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("W", new String[]{"W-s4-1", "W-s4-2"});
            SendEventsAndReset(s4Events);
    
            s2Events = SupportBean_S2.MakeS2("W", new String[]{"W-s1-1"});
            SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[1]}
            }, GetAndResetNewEvents());
    
            // Test s4 and s0=2, s1=2, s2=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("X", new String[]{"X-s0-1", "X-s0-2"});
            SendEvent(s0Events);
    
            s1Events = SupportBean_S1.MakeS1("X", new String[]{"X-s1-1", "X-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("X", new String[]{"X-s2-1", "X-s2-2"});
            SendEvent(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("X", new String[]{"X-s3-1", "X-s3-2"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("X", new String[]{"X-s4-1"});
            SendEvent(s4Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]},
                    new Object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[1], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s4 and s0=0, s1=1, s2=1, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("Y", new String[]{"Y-s1-1"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("Y", new String[]{"Y-s2-1"});
            SendEvent(s2Events);
    
            s3Events = SupportBean_S3.MakeS3("Y", new String[]{"Y-s3-1"});
            SendEventsAndReset(s3Events);
    
            s4Events = SupportBean_S4.MakeS4("Y", new String[]{"Y-s4-1"});
            SendEvent(s4Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
    
            // Test s3 and s0=0, s1=2, s2=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("Z", new String[]{"Z-s1-1", "Z-s1-2"});
            SendEventsAndReset(s1Events);
    
            s2Events = SupportBean_S2.MakeS2("Z", new String[]{"Z-s2-1"});
            SendEventsAndReset(s2Events);
    
            s4Events = SupportBean_S4.MakeS4("Z", new String[]{"Z-s4-1"});
            SendEventsAndReset(s4Events);
    
            s3Events = SupportBean_S3.MakeS3("Z", new String[]{"Z-s3-1"});
            SendEvent(s3Events);
            EPAssertionUtil.AssertSameAnyOrder(new Object[][]{
                    new Object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
                    new Object[] {null, s1Events[1], s2Events[0], s3Events[0], s4Events[0]}
            }, GetAndResetNewEvents());
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
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new String[]{"s0", "s1", "s2", "s3", "s4"});
        }
    }
}
