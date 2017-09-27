///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestOuterJoin7Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
        private static readonly string EVENT_S3 = typeof(SupportBean_S3).FullName;
        private static readonly string EVENT_S4 = typeof(SupportBean_S4).FullName;
        private static readonly string EVENT_S5 = typeof(SupportBean_S5).FullName;
        private static readonly string EVENT_S6 = typeof(SupportBean_S6).FullName;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _updateListener = null;
	    }

        [Test]
	    public void TestKeyPerStream() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p01 = s2.p20 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p11 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p11 = s4.p40 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p21 = s5.p50 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p21 = s6.p60 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAssertsKeysPerStream();
	    }

	    private void RunAssertsKeysPerStream() {
	        object[] s0Events, s1Events, s2Events, s3Events, s4Events, s5Events, s6Events;

	        // Test s0
	        //
	        s2Events = SupportBean_S2.MakeS2("A-s0-1", new string[] {"A-s2-1", "A-s2-2", "A-s2-3"});
	        SendEventsAndReset(s2Events);

	        object[] s6Events_1 = SupportBean_S6.MakeS6("A-s2-1", new string[] {"A-s6-1", "A-s6-2"});
	        SendEventsAndReset(s6Events_1);
	        object[] s6Events_2 = SupportBean_S6.MakeS6("A-s2-3", new string[] {"A-s6-1"});
	        SendEventsAndReset(s6Events_2);

	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, null, s6Events_1[0]},
	            new object[] {s0Events[0], null, s2Events[0], null, null, null, s6Events_1[1]},
	            new object[] {s0Events[0], null, s2Events[2], null, null, null, s6Events_2[0]}
	        }, GetAndResetNewEvents());

	        // Test s0
	        //
	        s1Events = SupportBean_S1.MakeS1("B", new string[] {"B-s1-1", "B-s1-2", "B-s1-3"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("B-s0-1", new string[] {"B-s2-1", "B-s2-2", "B-s2-3", "B-s2-4"});
	        SendEventsAndReset(s2Events);

	        object[] s5Events_1 = SupportBean_S5.MakeS5("B-s2-3", new string[] {"B-s6-1"});
	        SendEventsAndReset(s5Events_1);
	        object[] s5Events_2 = SupportBean_S5.MakeS5("B-s2-4", new string[] {"B-s5-1", "B-s5-2"});
	        SendEventsAndReset(s5Events_2);

	        s6Events = SupportBean_S6.MakeS6("B-s2-4", new string[] {"B-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[3], null, null, s5Events_2[1], s6Events[0]},
	            new object[] {s0Events[0], null, s2Events[3], null, null, s5Events_2[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1", "C-s1-2", "C-s1-3"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("C-s0-1", new string[] {"C-s2-1", "C-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("C-s1-2", new string[] {"C-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events_1 = SupportBean_S5.MakeS5("C-s2-1", new string[] {"C-s5-1"});
	        SendEventsAndReset(s5Events_1);
	        s5Events_2 = SupportBean_S5.MakeS5("C-s2-2", new string[] {"C-s5-1", "C-s5-2"});
	        SendEventsAndReset(s5Events_2);

	        s6Events_1 = SupportBean_S6.MakeS6("C-s2-1", new string[] {"C-s6-1"});
	        SendEventsAndReset(s6Events_1);
	        s6Events_2 = SupportBean_S6.MakeS6("C-s2-2", new string[] {"C-s6-2"});
	        SendEventsAndReset(s6Events_2);

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], null, s5Events_1[0], s6Events_1[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], null, s5Events_2[0], s6Events_2[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], null, s5Events_2[1], s6Events_2[0]},
	        }, GetAndResetNewEvents());

	        // Test s0
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-3", "D-s1-2", "D-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("D-s0-1", new string[] {"D-s2-2", "D-s2-1"});
	        SendEventsAndReset(s2Events);

	        object[] s3Events_1 = SupportBean_S3.MakeS3("D-s1-1", new string[] {"D-s3-1", "D-s3-2"});
	        SendEventsAndReset(s3Events_1);
	        object[] s3Events_2 = SupportBean_S3.MakeS3("D-s1-3", new string[] {"D-s3-3", "D-s3-4"});
	        SendEventsAndReset(s3Events_2);

	        s4Events = SupportBean_S4.MakeS4("D-s1-2", new string[] {"D-s4-1", "D-s4-2"});
	        SendEventsAndReset(s4Events);
	        s4Events = SupportBean_S4.MakeS4("D-s1-3", new string[] {"D-s4-3", "D-s4-4"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("D-s2-1", new string[] {"D-s5-1", "D-s5-2"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("D-s2-2", new string[] {"D-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events_2[0], s4Events[0], null, s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events_2[0], s4Events[1], null, s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events_2[1], s4Events[0], null, s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events_2[1], s4Events[1], null, s6Events[0]},
	            new object[] {s0Events[0], s1Events[2], s2Events[0], s3Events_1[0], null, null, s6Events[0]},
	            new object[] {s0Events[0], s1Events[2], s2Events[0], s3Events_1[1], null, null, s6Events[0]},
	        }, GetAndResetNewEvents());

	        // Test s1
	        //
	        s3Events = SupportBean_S3.MakeS3("E-s1-1", new string[] {"E-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("E-s1-1", new string[] {"E-s4-1"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1", "E-s0-2", "E-s0-3", "E-s0-4"});
	        SendEvent(s0Events);

	        object[] s2Events_1 = SupportBean_S2.MakeS2("E-s0-1", new string[] {"E-s2-1", "E-s2-2"});
	        SendEventsAndReset(s2Events_1);
	        object[] s2Events_2 = SupportBean_S2.MakeS2("E-s0-3", new string[] {"E-s2-3", "E-s2-4"});
	        SendEventsAndReset(s2Events_2);
	        object[] s2Events_3 = SupportBean_S2.MakeS2("E-s0-4", new string[] {"E-s2-5", "E-s2-6"});
	        SendEventsAndReset(s2Events_3);

	        s5Events_1 = SupportBean_S5.MakeS5("E-s2-2", new string[] {"E-s5-1", "E-s5-2"});
	        SendEventsAndReset(s5Events_1);
	        s5Events_2 = SupportBean_S5.MakeS5("E-s2-4", new string[] {"E-s5-3"});
	        SendEventsAndReset(s5Events_2);

	        s6Events_1 = SupportBean_S6.MakeS6("E-s2-2", new string[] {"E-s6-1"});
	        SendEventsAndReset(s6Events_1);
	        s6Events_2 = SupportBean_S6.MakeS6("E-s2-5", new string[] {"E-s6-2"});
	        SendEventsAndReset(s6Events_2);

	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events_1[1], s3Events[0], s4Events[0], s5Events_1[0], s6Events_1[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events_1[1], s3Events[0], s4Events[0], s5Events_1[1], s6Events_1[0]},
	            new object[] {s0Events[3], s1Events[0], s2Events_3[0], s3Events[0], s4Events[0], null, s6Events_2[0]},
	        }, GetAndResetNewEvents());

	        // Test s2
	        //
	        s5Events = SupportBean_S5.MakeS5("F-s2-1", new string[] {"F-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("F-s2-1", new string[] {"F-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s2-1", "F-s2-2"});
	        SendEventsAndReset(s0Events);

	        s3Events_1 = SupportBean_S3.MakeS3("F-s1-1", new string[] {"F-s3-1"});
	        SendEventsAndReset(s3Events_1);
	        s3Events_2 = SupportBean_S3.MakeS3("F-s1-3", new string[] {"F-s3-2"});
	        SendEventsAndReset(s3Events_2);

	        s4Events = SupportBean_S4.MakeS4("F-s1-1", new string[] {"F-s4-1"});
	        SendEventsAndReset(s4Events);

	        object[] s1Events_1 = SupportBean_S1.MakeS1("F", new string[] {"F-s1-1"});
	        SendEventsAndReset(s1Events_1);
	        object[] s1Events_2 = SupportBean_S1.MakeS1("F", new string[] {"F-s1-2"});
	        SendEventsAndReset(s1Events_2);
	        object[] s1Events_3 = SupportBean_S1.MakeS1("F", new string[] {"F-s1-3"});
	        SendEventsAndReset(s1Events_3);

	        s2Events = SupportBean_S2.MakeS2("F-s2-1", new string[] {"F-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events_1[0], s2Events[0], s3Events_1[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events_3[0], s2Events[0], s3Events_2[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-3", "G-s1-2", "G-s1-3"});
	        SendEventsAndReset(s1Events);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s2-1", "G-s2-2"});
	        SendEventsAndReset(s0Events);

	        s6Events = SupportBean_S6.MakeS6("G-s2-2", new string[] {"G-s6-1"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("G-s2-2", new string[] {"G-s2-2"});
	        SendEvent(s2Events);

	        s4Events = SupportBean_S4.MakeS4("G-s1-2", new string[] {"G-s4-1"});
	        SendEventsAndReset(s4Events);

	        s3Events = SupportBean_S3.MakeS3("G-s1-2", new string[] {"G-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-3", "H-s1-2", "H-s1-3"});
	        SendEventsAndReset(s1Events);

	        s4Events = SupportBean_S4.MakeS4("H-s1-2", new string[] {"H-s4-1"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s2-1", "H-s2-2", "H-s2-3"});
	        SendEventsAndReset(s0Events);

	        s2Events_1 = SupportBean_S2.MakeS2("H-s2-2", new string[] {"H-s2-20"});
	        SendEvent(s2Events_1);
	        s2Events_2 = SupportBean_S2.MakeS2("H-s2-3", new string[] {"H-s2-30", "H-s2-31"});
	        SendEvent(s2Events_2);

	        s6Events_1 = SupportBean_S6.MakeS6("H-s2-20", new string[] {"H-s6-1"});
	        SendEventsAndReset(s6Events_1);
	        s6Events_2 = SupportBean_S6.MakeS6("H-s2-31", new string[] {"H-s6-3", "H-s6-4"});
	        SendEventsAndReset(s6Events_2);

	        s3Events = SupportBean_S3.MakeS3("H-s1-2", new string[] {"H-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[1], s1Events[1], s2Events_1[0], s3Events[0], s4Events[0], null, s6Events_1[0]},
	            new object[] {s0Events[2], s1Events[1], s2Events_2[1], s3Events[0], s4Events[0], null, s6Events_2[0]},
	            new object[] {s0Events[2], s1Events[1], s2Events_2[1], s3Events[0], s4Events[0], null, s6Events_2[1]},
	        }, GetAndResetNewEvents());

	        // Test s4
	        //
	        s3Events = SupportBean_S3.MakeS3("I-s1-3", new string[] {"I-s3-1"});
	        SendEvent(s3Events);

	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1", "I-s1-2", "I-s1-3"});
	        SendEventsAndReset(s1Events);

	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s2-1", "I-s2-2", "I-s2-3"});
	        SendEventsAndReset(s0Events);

	        s2Events_1 = SupportBean_S2.MakeS2("I-s2-1", new string[] {"I-s2-20"});
	        SendEvent(s2Events_1);
	        s2Events_2 = SupportBean_S2.MakeS2("I-s2-2", new string[] {"I-s2-30", "I-s2-31"});
	        SendEvent(s2Events_2);

	        s5Events = SupportBean_S5.MakeS5("I-s2-30", new string[] {"I-s5-1", "I-s5-2"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("I-s2-30", new string[] {"I-s6-1", "I-s6-2"});
	        SendEventsAndReset(s6Events);

	        s4Events = SupportBean_S4.MakeS4("I-s1-3", new string[] {"I-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[1], s1Events[2], s2Events_2[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[1], s1Events[2], s2Events_2[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
	            new object[] {s0Events[1], s1Events[2], s2Events_2[0], s3Events[0], s4Events[0], s5Events[0], s6Events[1]},
	            new object[] {s0Events[1], s1Events[2], s2Events_2[0], s3Events[0], s4Events[0], s5Events[1], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("J-s2-30", new string[] {"J-s6-1", "J-s6-2"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("J-s2-1", new string[] {"J-s2-30", "J-s2-31"});
	        SendEvent(s2Events);

	        s5Events = SupportBean_S5.MakeS5("J-s2-30", new string[] {"J-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[0]},
	            new object[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("K-s2-31", new string[] {"K-s6-1", "K-s6-2"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("K", new string[] {"K-s2-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("K-s2-1", new string[] {"K-s2-30", "K-s2-31"});
	        SendEvent(s2Events);

	        s5Events = SupportBean_S5.MakeS5("K-s2-31", new string[] {"K-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("L-s2-31", new string[] {"L-s6-1", "L-s6-2"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("L-s2-1", new string[] {"L-s2-30", "L-s2-31"});
	        SendEvent(s2Events);

	        s0Events = SupportBean_S0.MakeS0("L", new string[] {"L-s2-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("L", new string[] {"L-s1-1"});
	        SendEventsAndReset(s1Events);

	        s5Events = SupportBean_S5.MakeS5("L-s2-31", new string[] {"L-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("M-s2-31", new string[] {"M-s6-1", "M-s6-2"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("M-s2-1", new string[] {"M-s2-30", "M-s2-31"});
	        SendEvent(s2Events);

	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s2-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEventsAndReset(s1Events);

	        s3Events = SupportBean_S3.MakeS3("M-s1-1", new string[] {"M-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("M-s2-31", new string[] {"M-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("N-s2-31", new string[] {"N-s6-1", "N-s6-2"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("N-s2-1", new string[] {"N-s2-30", "N-s2-31"});
	        SendEvent(s2Events);

	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s2-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("N", new string[] {"N-s1-1", "N-s1-2", "N-s1-3"});
	        SendEventsAndReset(s1Events);

	        s3Events = SupportBean_S3.MakeS3("N-s1-3", new string[] {"N-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("N-s2-31", new string[] {"N-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[2], s2Events[1], s3Events[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[2], s2Events[1], s3Events[0], null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5
	        //
	        s6Events = SupportBean_S6.MakeS6("O-s2-31", new string[] {"O-s6-1", "O-s6-2"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("O-s2-1", new string[] {"O-s2-30", "O-s2-31"});
	        SendEvent(s2Events);

	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s2-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1", "O-s1-2", "O-s1-3"});
	        SendEventsAndReset(s1Events);

	        s3Events_1 = SupportBean_S3.MakeS3("O-s1-2", new string[] {"O-s3-1", "O-s3-2"});
	        SendEventsAndReset(s3Events_1);
	        s3Events_2 = SupportBean_S3.MakeS3("O-s1-3", new string[] {"O-s3-3"});
	        SendEventsAndReset(s3Events_2);

	        s5Events = SupportBean_S5.MakeS5("O-s2-31", new string[] {"O-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[1], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[2], s2Events[1], s3Events_2[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[0], null, s5Events[0], s6Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[1], null, s5Events[0], s6Events[1]},
	            new object[] {s0Events[0], s1Events[2], s2Events[1], s3Events_2[0], null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s6
	        //
	        s5Events = SupportBean_S5.MakeS5("P-s2-31", new string[] {"P-s5-1"});
	        SendEvent(s5Events);

	        s2Events = SupportBean_S2.MakeS2("P-s2-1", new string[] {"P-s2-30", "P-s2-31"});
	        SendEvent(s2Events);

	        s0Events = SupportBean_S0.MakeS0("P", new string[] {"P-s2-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1", "P-s1-2", "P-s1-3"});
	        SendEventsAndReset(s1Events);

	        s3Events_1 = SupportBean_S3.MakeS3("P-s1-2", new string[] {"P-s3-1", "P-s3-2"});
	        SendEventsAndReset(s3Events_1);
	        s3Events_2 = SupportBean_S3.MakeS3("P-s1-3", new string[] {"P-s3-3"});
	        SendEventsAndReset(s3Events_2);

	        s6Events = SupportBean_S6.MakeS6("P-s2-31", new string[] {"P-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events_1[1], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[2], s2Events[1], s3Events_2[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());
	    }

        [Test]
	    public void TestRoot_s0() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s1() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s2() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s3() {
	        string joinStatement = "select * from " +
	                               EVENT_S3 + "#length(1000) as s3 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s3.p30 = s1.p10 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s4() {
	        string joinStatement = "select * from " +
	                               EVENT_S4 + "#length(1000) as s4 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s4.p40 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s5() {
	        string joinStatement = "select * from " +
	                               EVENT_S5 + "#length(1000) as s5 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S6 + "#length(1000) as s6 on s2.p20 = s6.p60 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s6() {
	        string joinStatement = "select * from " +
	                               EVENT_S6 + "#length(1000) as s6 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s6.p60 " +
	                               " left outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts() {
	        object[] s0Events, s1Events, s2Events, s3Events, s4Events, s5Events, s6Events;

	        // Test s0 and s1=0, s2=0, s3=0, s4=0, s5=0, s6=0
	        //
	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=0, s2=1, s3=0, s4=0, s5=0, s6=0
	        //
	        s2Events = SupportBean_S2.MakeS2("B", new string[] {"B-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=0, s2=1, s3=0, s4=0, s5=0, s6=1
	        //
	        s2Events = SupportBean_S2.MakeS2("C", new string[] {"C-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s6Events = SupportBean_S6.MakeS6("C", new string[] {"C-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=1, s4=0, s5=1, s6=1
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("D", new string[] {"D-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("D", new string[] {"D-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, s3Events[0], null, null, null}
	        }, GetAndResetNewEvents());

	        s5Events = SupportBean_S5.MakeS5("D", new string[] {"D-s5-1"});
	        SendEvent(s5Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s6Events = SupportBean_S6.MakeS6("D", new string[] {"D-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=1, s4=2, s5=1, s6=1
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("E", new string[] {"E-s2-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("E", new string[] {"E-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s5Events = SupportBean_S5.MakeS5("E", new string[] {"E-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("E", new string[] {"E-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=1, s4=2, s5=1, s6=1
	        //
	        s1Events = SupportBean_S1.MakeS1("F", new string[] {"F-s1-1", "F-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1", "F-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("F", new string[] {"F-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("F", new string[] {"F-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s5Events = SupportBean_S5.MakeS5("F", new string[] {"F-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("F", new string[] {"F-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=2, s4=2, s5=1, s6=2
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("G", new string[] {"G-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("G", new string[] {"G-s3-1", "G-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("G", new string[] {"G-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s5Events = SupportBean_S5.MakeS5("G", new string[] {"G-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("G", new string[] {"G-s6-1", "G-s6-2"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=1, s4=1, s5=2, s6=1
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1", "H-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1", "H-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("H", new string[] {"H-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("H", new string[] {"H-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s5Events = SupportBean_S5.MakeS5("H", new string[] {"H-s5-1", "H-s5-2"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("H", new string[] {"H-s6-1"});
	        SendEventsAndReset(s6Events);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1, s6=0
	        //
	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s0-1"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("I", new string[] {"I-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("I", new string[] {"I-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("I", new string[] {"I-s5-1"});
	        SendEventsAndReset(s5Events);

	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1", "I-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);    // no s6

	        // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("J", new string[] {"J-s0-1"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("J", new string[] {"J-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("J", new string[] {"J-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("J", new string[] {"J-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("J", new string[] {"J-s6-1"});
	        SendEventsAndReset(s6Events);

	        s1Events = SupportBean_S1.MakeS1("J", new string[] {"J-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=1, s2=1, s3=1, s4=1, s5=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("K", new string[] {"K-s0-1"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("K", new string[] {"K-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("K", new string[] {"K-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("K", new string[] {"K-s4-1"});
	        SendEventsAndReset(s4Events);

	        s6Events = SupportBean_S6.MakeS6("K", new string[] {"K-s6-1"});
	        SendEventsAndReset(s6Events);

	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=0, s3=0, s4=0, s5=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("L", new string[] {"L-s0-1"});
	        SendEventsAndReset(s0Events);

	        s6Events = SupportBean_S6.MakeS6("L", new string[] {"L-s6-1"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("L", new string[] {"L-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=1, s3=0, s4=0, s5=1, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEventsAndReset(s1Events);

	        s5Events = SupportBean_S5.MakeS5("M", new string[] {"M-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("M", new string[] {"M-s6-1"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[] {"M-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=1, s3=1, s4=0, s5=1, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("N", new string[] {"N-s1-1"});
	        SendEventsAndReset(s1Events);

	        s3Events = SupportBean_S3.MakeS3("N", new string[] {"N-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("N", new string[] {"N-s5-1"});
	        SendEventsAndReset(s5Events);

	        s6Events = SupportBean_S6.MakeS6("N", new string[] {"N-s6-1"});
	        SendEventsAndReset(s6Events);

	        s2Events = SupportBean_S2.MakeS2("N", new string[] {"N-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=0
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[] {"O-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("O", new string[] {"O-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, s3Events[0], null, null, null}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=0
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[] {"O-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("O", new string[] {"O-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, s3Events[0], null, null, null}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("P", new string[] {"P-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("P", new string[] {"P-s2-1"});
	        SendEventsAndReset(s2Events);

	        s6Events = SupportBean_S6.MakeS6("P", new string[] {"P-s6-1"});
	        SendEventsAndReset(s6Events);

	        s3Events = SupportBean_S3.MakeS3("P", new string[] {"P-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=1, s2=1, s4=2, s5=2, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("Q", new string[] {"Q-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s1Events = SupportBean_S1.MakeS1("Q", new string[] {"Q-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("Q", new string[] {"Q-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s4Events = SupportBean_S4.MakeS4("Q", new string[] {"Q-s4-1", "Q-s4-2"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s5Events = SupportBean_S5.MakeS5("Q", new string[] {"Q-s5-1", "Q-s5-2"});
	        SendEvent(s5Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s6Events = SupportBean_S6.MakeS6("Q", new string[] {"Q-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[1], s6Events[0]}
	        }, GetAndResetNewEvents());

	        s3Events = SupportBean_S3.MakeS3("Q", new string[] {"Q-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=1, s1=1, s2=0, s4=0, s5=0, s6=0
	        //
	        s0Events = SupportBean_S0.MakeS0("R", new string[] {"R-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1"});
	        SendEvent(s1Events);

	        s4Events = SupportBean_S4.MakeS4("R", new string[] {"R-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s4 and s0=2, s1=1, s2=1, s4=0, s5=0, s6=2
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[] {"S-s0-1", "S-s0-2"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[] {"S-s2-1"});
	        SendEventsAndReset(s2Events);

	        s6Events = SupportBean_S6.MakeS6("S", new string[] {"S-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]},
	            new object[] {s0Events[1], null, s2Events[0], null, null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        s4Events = SupportBean_S4.MakeS4("S", new string[] {"S-s4-1"});
	        SendEvent(s4Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s4 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("T", new string[] {"T-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("T", new string[] {"T-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("T", new string[] {"T-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("T", new string[] {"T-s3-1"});
	        SendEventsAndReset(s3Events);

	        s6Events = SupportBean_S6.MakeS6("T", new string[] {"T-s6-1"});
	        SendEvent(s6Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        s4Events = SupportBean_S4.MakeS4("T", new string[] {"T-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s5 and s0=1, s1=0, s2=1, s3=0, s4=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[] {"U-s0-1"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[] {"U-s2-1"});
	        SendEventsAndReset(s2Events);

	        s6Events = SupportBean_S6.MakeS6("U", new string[] {"U-s6-1"});
	        SendEventsAndReset(s6Events);

	        s5Events = SupportBean_S5.MakeS5("U", new string[] {"U-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s6 and s0=1, s1=2, s2=1, s3=0, s4=0, s6=2
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("V", new string[] {"V-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s2-1"});
	        SendEventsAndReset(s2Events);

	        s6Events = SupportBean_S6.MakeS6("V", new string[] {"V-s6-1", "V-s6-2"});
	        SendEventsAndReset(s6Events);

	        s5Events = SupportBean_S5.MakeS5("V", new string[] {"V-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5 and s0=1, s1=2, s2=1, s3=1, s4=0, s6=1
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[] {"W-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[] {"W-s1-1", "W-s1-2"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[] {"W-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("W", new string[] {"W-s3-1"});
	        SendEventsAndReset(s3Events);

	        s6Events = SupportBean_S6.MakeS6("W", new string[] {"W-s6-1"});
	        SendEventsAndReset(s6Events);

	        s5Events = SupportBean_S5.MakeS5("W", new string[] {"W-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
	        }, GetAndResetNewEvents());
	    }

	    private void SendEventsAndReset(object[] events) {
	        SendEvent(events);
	        _updateListener.Reset();
	    }

	    private void SendEvent(object[] events) {
	        for (int i = 0; i < events.Length; i++) {
	            _epService.EPRuntime.SendEvent(events[i]);
	        }
	    }

        private object[][] GetAndResetNewEvents()
        {
            EventBean[] newEvents = _updateListener.LastNewData;
            _updateListener.Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(
                newEvents, new string[]
                {
                    "s0",
                    "s1",
                    "s2",
                    "s3",
                    "s4",
                    "s5",
                    "s6"
                });
        }
    }
} // end of namespace
