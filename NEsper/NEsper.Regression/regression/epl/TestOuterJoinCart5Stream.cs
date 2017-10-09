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
	public class TestOuterJoinCart5Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
        private static readonly string EVENT_S3 = typeof(SupportBean_S3).FullName;
        private static readonly string EVENT_S4 = typeof(SupportBean_S4).FullName;

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
	    public void TestRoot_s0() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s1() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s1_order_2() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s2() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s2_order_2() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s3() {
	        string joinStatement = "select * from " +
	                               EVENT_S3 + "#length(1000) as s3 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s3_order2() {
	        string joinStatement = "select * from " +
	                               EVENT_S3 + "#length(1000) as s3 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S4 + "#length(1000) as s4 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s4() {
	        string joinStatement = "select * from " +
	                               EVENT_S4 + "#length(1000) as s4 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s4_order2() {
	        string joinStatement = "select * from " +
	                               EVENT_S4 + "#length(1000) as s4 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts() {
	        object[] s0Events;
	        object[] s1Events;
	        object[] s2Events;
	        object[] s3Events;
	        object[] s4Events;

	        // Test s0 and s1=0, s2=0, s3=0, s4=0
	        //
	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=1, s2=0, s3=0, s4=0
	        //
	        s1Events = SupportBean_S1.MakeS1("B", new string[] {"B-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { null, s1Events[0], null, null, null } }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { s0Events[0], s1Events[0], null, null, null } }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=0, s4=0
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("C", new string[] {"C-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { null, s1Events[0], s2Events[0], null, null } }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0], null, null } }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=1, s4=0
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("D", new string[] {"D-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("D", new string[] {"D-s2-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { null, s1Events[0], s2Events[0], s3Events[0], null } }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=1, s4=1
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("E", new string[] {"E-s2-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("E", new string[] {"E-s2-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { null, s1Events[0], s2Events[0], s3Events[0], s4Events[0] } }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=1, s3=1, s4=1
	        //
	        s1Events = SupportBean_S1.MakeS1("F", new string[] {"F-s1-1", "F-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("F", new string[] {"F-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("F", new string[] {"F-s2-1"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=1, s4=1
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-1", "G-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("G", new string[] {"G-s2-1", "G-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("G", new string[] {"G-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("G", new string[] {"G-s2-1"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=2, s4=1
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1", "H-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1", "H-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("H", new string[] {"H-s3-1", "H-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("H", new string[] {"H-s2-1"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=2, s4=2
	        //
	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1", "I-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("I", new string[] {"I-s2-1", "I-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("I", new string[] {"I-s3-1", "I-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("I", new string[] {"I-s4-1", "I-s4-2"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=2, s4=3
	        //
	        s1Events = SupportBean_S1.MakeS1("J", new string[] {"J-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("J", new string[] {"J-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("J", new string[] {"J-s3-1", "J-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("J", new string[] {"J-s4-1", "J-s4-2", "J-s4-3"});
	        SendEventsAndReset(s4Events);

	        s0Events = SupportBean_S0.MakeS0("J", new string[] {"J-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[2]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=1, s3=1, s4=1
	        //
	        s2Events = SupportBean_S2.MakeS2("K", new string[] {"K-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("K", new string[] {"K-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("K", new string[] {"K-s4-1"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=1, s3=0, s4=1
	        //
	        s2Events = SupportBean_S2.MakeS2("L", new string[] {"L-s2-1"});
	        SendEventsAndReset(s2Events);

	        s4Events = SupportBean_S4.MakeS4("L", new string[] {"L-s4-1"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("L", new string[] {"L-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], s2Events[0], null, s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=2, s2=1, s3=0, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s0-1", "M-s0-2"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[] {"M-s2-1"});
	        SendEventsAndReset(s2Events);

	        s4Events = SupportBean_S4.MakeS4("M", new string[] {"M-s4-1"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], null, s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=1, s2=0, s3=0, s4=0
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("N", new string[] {"N-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], null, null, null}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=0, s3=1, s4=0
	        //
	        s3Events = SupportBean_S3.MakeS3("O", new string[] {"O-s3-1"});
	        SendEventsAndReset(s3Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=0, s3=0, s4=1
	        //
	        s4Events = SupportBean_S4.MakeS4("P", new string[] {"P-s4-1"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, null, s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=0, s3=0, s4=2
	        //
	        s4Events = SupportBean_S4.MakeS4("Q", new string[] {"Q-s4-1", "Q-s4-2"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("Q", new string[] {"Q-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, null, s4Events[0]},
	            new object[] {null, s1Events[0], null, null, s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=0, s3=2, s4=2
	        //
	        s3Events = SupportBean_S3.MakeS3("R", new string[] {"R-s3-1", "R-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("R", new string[] {"R-s4-1", "R-s4-2"});
	        SendEventsAndReset(s4Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], s4Events[0]},
	            new object[] {null, s1Events[0], null, s3Events[1], s4Events[0]},
	            new object[] {null, s1Events[0], null, s3Events[0], s4Events[1]},
	            new object[] {null, s1Events[0], null, s3Events[1], s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=2, s3=0, s4=2
	        //
	        s4Events = SupportBean_S4.MakeS4("S", new string[] {"S-s4-1", "S-s4-2"});
	        SendEventsAndReset(s4Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[] {"S-s2-1", "S-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], s2Events[0], null, s4Events[0]},
	            new object[] {null, s1Events[0], s2Events[0], null, s4Events[1]},
	            new object[] {null, s1Events[0], s2Events[1], null, s4Events[0]},
	            new object[] {null, s1Events[0], s2Events[1], null, s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=2, s3=0, s4=2
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[] {"U-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("U", new string[] {"U-s1-1"});
	        SendEventsAndReset(s1Events);

	        s4Events = SupportBean_S4.MakeS4("U", new string[] {"U-s4-1", "U-s4-2"});
	        SendEventsAndReset(s4Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[] {"U-s1-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null, s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=3, s1=1, s3=2, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1", "V-s0-2", "V-s0-3"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("V", new string[] {"V-s1-1"});
	        SendEventsAndReset(s1Events);

	        s3Events = SupportBean_S3.MakeS3("V", new string[] {"V-s3-1", "V-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("V", new string[] {"V-s4-1"});
	        SendEventsAndReset(s4Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s1-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[2], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[2], s1Events[0], s2Events[0], s3Events[1], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=2, s1=2, s3=2, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[] {"W-s0-1", "W-s0-2"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[] {"W-s1-1", "W-s1-2"});
	        SendEventsAndReset(s1Events);

	        s3Events = SupportBean_S3.MakeS3("W", new string[] {"W-s3-1", "W-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("W", new string[] {"W-s4-1", "W-s4-2"});
	        SendEventsAndReset(s4Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[] {"W-s1-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=2, s1=2, s2=2, s3=2
	        //
	        s0Events = SupportBean_S0.MakeS0("X", new string[] {"X-s0-1", "X-s0-2"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("X", new string[] {"X-s1-1", "X-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("X", new string[] {"X-s2-1", "X-s2-2"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("X", new string[] {"X-s3-1", "X-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("X", new string[] {"X-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[1], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=0, s1=1, s2=1, s3=1
	        //
	        s1Events = SupportBean_S1.MakeS1("Y", new string[] {"Y-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("Y", new string[] {"Y-s2-1"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("Y", new string[] {"Y-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("Y", new string[] {"Y-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=0, s1=2, s2=1, s4=1
	        //
	        s1Events = SupportBean_S1.MakeS1("Z", new string[] {"Z-s1-1", "Z-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("Z", new string[] {"Z-s2-1"});
	        SendEventsAndReset(s2Events);

	        s4Events = SupportBean_S4.MakeS4("Z", new string[] {"Z-s4-1"});
	        SendEventsAndReset(s4Events);

	        s3Events = SupportBean_S3.MakeS3("Z", new string[] {"Z-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], s2Events[0], s3Events[0], s4Events[0]},
	            new object[] {null, s1Events[1], s2Events[0], s3Events[0], s4Events[0]}
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
                    "s4"
                });
        }
    }
} // end of namespace
