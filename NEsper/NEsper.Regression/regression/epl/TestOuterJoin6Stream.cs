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
	public class TestOuterJoin6Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

	    private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
        private static readonly string EVENT_S3 = typeof(SupportBean_S3).FullName;
        private static readonly string EVENT_S4 = typeof(SupportBean_S4).FullName;
        private static readonly string EVENT_S5 = typeof(SupportBean_S5).FullName;

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
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " right outer join " + EVENT_S4 + "#length(1000) as s4 on s2.p20 = s4.p40 " +
	                               " right outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s1() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S4 + "#length(1000) as s4 on s2.p20 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s2() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 " +
	                               " right outer join " + EVENT_S4 + "#length(1000) as s4 on s2.p20 = s4.p40 " +
	                               " right outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s3() {
	        string joinStatement = "select * from " +
	                               EVENT_S3 + "#length(1000) as s3 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s1.p10 = s3.p30 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S4 + "#length(1000) as s4 on s2.p20 = s4.p40 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s4() {
	        string joinStatement = "select * from " +
	                               EVENT_S4 + "#length(1000) as s4 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s4.p40 " +
	                               " right outer join " + EVENT_S5 + "#length(1000) as s5 on s2.p20 = s5.p50 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRoot_s5() {
	        string joinStatement = "select * from " +
	                               EVENT_S5 + "#length(1000) as s5 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s5.p50 " +
	                               " right outer join " + EVENT_S4 + "#length(1000) as s4 on s2.p20 = s4.p40 " +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " right outer join " + EVENT_S3 + "#length(1000) as s3 on s1.p10 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts()
        {
	        object[] s0Events;
	        object[] s1Events;
	        object[] s2Events;
	        object[] s3Events;
	        object[] s4Events;
	        object[] s5Events;

	        // Test s0 and s1=0, s2=0, s3=0, s4=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=1, s2=0, s3=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("B", new string[] {"B-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=1, s2=1, s3=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("C", new string[] {"C-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=1, s2=1, s3=1, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("D", new string[] {"D-s2-1"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("D", new string[] {"D-s2-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=1, s2=1, s3=1, s4=1, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("E", new string[] {"E-s2-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        s4Events = SupportBean_S4.MakeS4("E", new string[] {"E-s2-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, null, s4Events[0], null}
	        }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s0 and s1=2, s2=1, s3=1, s4=1, s5=1
	        //
	        s1Events = SupportBean_S1.MakeS1("F", new string[] {"F-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("F", new string[] {"F-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        s4Events = SupportBean_S4.MakeS4("F", new string[] {"F-s2-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, null, s4Events[0], null}
	        }, GetAndResetNewEvents());

	        s5Events = SupportBean_S5.MakeS5("F", new string[] {"F-s2-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}
	        }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=1, s4=1, s5=2
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-1", "G-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("G", new string[] {"G-s2-1", "G-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("G", new string[] {"G-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("G", new string[] {"G-s2-1"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("G", new string[] {"G-s5-1", "G-s5-2"});
	        SendEventsAndReset(s5Events);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=2, s4=2, s5=2
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1", "H-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1", "H-s2-2"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("H", new string[] {"H-s3-1", "H-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("H", new string[] {"H-s4-1", "H-s4-2"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("H", new string[] {"H-s5-1", "H-s5-2"});
	        SendEventsAndReset(s5Events);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=1, s3=1, s4=3, s5=1
	        //
	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1", "I-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("I", new string[] {"I-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("I", new string[] {"I-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("I", new string[] {"I-s4-1", "I-s4-2", "I-s4-3"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("I", new string[] {"I-s5-1"});
	        SendEventsAndReset(s5Events);

	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[2], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("J", new string[] {"J-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s1 and s0=1, s2=0, s3=1, s4=1, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("K", new string[] {"K-s0-1"});
	        SendEvent(s0Events);

	        s3Events = SupportBean_S3.MakeS3("K", new string[] {"K-s3-1"});
	        SendEventsAndReset(s3Events);

	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("L", new string[] {"L-s0-1"});
	        SendEvent(s0Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("L", new string[] {"L-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("L", new string[] {"L-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("L", new string[] {"L-s5-1"});
	        SendEventsAndReset(s5Events);

	        s1Events = SupportBean_S1.MakeS1("L", new string[] {"L-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=1, s2=1, s3=1, s4=2, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s0-1"});
	        SendEvent(s0Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[] {"M-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("M", new string[] {"M-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("M", new string[] {"M-s4-1", "M-s4-2"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("M", new string[] {"M-s5-1"});
	        SendEventsAndReset(s5Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=0, s3=0, s4=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s0-1"});
	        SendEvent(s0Events);

	        s4Events = SupportBean_S4.MakeS4("N", new string[] {"N-s4-1"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("N", new string[] {"N-s5-1", "N-s5-2"});
	        SendEventsAndReset(s5Events);

	        s2Events = SupportBean_S2.MakeS2("N", new string[] {"N-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]},
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=1, s1=1, s3=3, s4=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1"});
	        SendEvent(s1Events);

	        s3Events = SupportBean_S3.MakeS3("O", new string[] {"O-s3-1", "O-s3-2", "O-s3-3"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("O", new string[] {"O-s4-1"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("O", new string[] {"O-s5-1", "O-s5-2"});
	        SendEventsAndReset(s5Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[] {"O-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2], s4Events[0], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=0, s1=0, s2=0, s4=0, s5=0
	        //
	        s3Events = SupportBean_S3.MakeS3("P", new string[] {"P-s1-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=0, s1=1, s2=0, s4=0, s5=0
	        //
	        s1Events = SupportBean_S1.MakeS1("Q", new string[] {"Q-s1-1"});
	        SendEvent(s1Events);

	        s3Events = SupportBean_S3.MakeS3("Q", new string[] {"Q-s1-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=2, s2=2, s4=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("R", new string[] {"R-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1", "R-s1-2"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("R", new string[] {"R-s2-1", "R-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("R", new string[] {"R-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, s1Events[0], null, s3Events[0], null, null},
	            new object[] {null, s1Events[1], null, s3Events[0], null, null}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=2, s1=2, s2=1, s4=2, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[] {"S-s0-1", "S-s0-2"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1", "S-s1-2"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[] {"S-s2-1", "S-s2-1"});
	        SendEventsAndReset(s2Events);

	        s4Events = SupportBean_S4.MakeS4("S", new string[] {"S-s4-1", "S-s4-2"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("S", new string[] {"S-s5-1", "S-s5-2"});
	        SendEventsAndReset(s5Events);

	        s3Events = SupportBean_S3.MakeS3("S", new string[] {"s-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[1], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=0
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[] {"U-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[] {"U-s1-1"});
	        SendEventsAndReset(s2Events);

	        s4Events = SupportBean_S4.MakeS4("U", new string[] {"U-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, null, null, s4Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=1, s1=0, s2=1, s3=0, s5=1
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s1-1"});
	        SendEventsAndReset(s2Events);

	        s5Events = SupportBean_S5.MakeS5("V", new string[] {"V-s5-1"});
	        SendEventsAndReset(s5Events);

	        s4Events = SupportBean_S4.MakeS4("V", new string[] {"V-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {null, null, s2Events[0], null, s4Events[0], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s4 and s0=1, s1=1, s2=1, s3=1, s5=2
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[] {"W-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[] {"W-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[] {"W-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("W", new string[] {"W-s3-1"});
	        SendEventsAndReset(s3Events);

	        s5Events = SupportBean_S5.MakeS5("W", new string[] {"W-s5-1", "W-s5-2"});
	        SendEventsAndReset(s5Events);

	        s4Events = SupportBean_S4.MakeS4("W", new string[] {"W-s4-1"});
	        SendEvent(s4Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s5 and s0=1, s1=2, s2=2, s3=1, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("X", new string[] {"X-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("X", new string[] {"X-s1-1", "X-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("X", new string[] {"X-s2-1", "X-s2-2"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("X", new string[] {"X-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("X", new string[] {"X-s4-1"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("X", new string[] {"X-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[]{s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0]},
	            new object[]{s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[]{s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s5 and s0=2, s1=1, s2=1, s3=1, s4=1
	        //
	        s0Events = SupportBean_S0.MakeS0("Y", new string[] {"Y-s0-1", "Y-s0-2"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("Y", new string[] {"Y-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("Y", new string[] {"Y-s2-1"});
	        SendEvent(s2Events);

	        s3Events = SupportBean_S3.MakeS3("Y", new string[] {"Y-s3-1"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("Y", new string[] {"Y-s4-1"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("Y", new string[] {"X-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[]{s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s5 and s0=1, s1=1, s2=1, s3=2, s4=2
	        //
	        s0Events = SupportBean_S0.MakeS0("Z", new string[] {"Z-s0-1"});
	        SendEvent(s0Events);

	        s1Events = SupportBean_S1.MakeS1("Z", new string[] {"Z-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("Z", new string[] {"Z-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("Z", new string[] {"Z-s3-1", "Z-s3-2"});
	        SendEventsAndReset(s3Events);

	        s4Events = SupportBean_S4.MakeS4("Z", new string[] {"Z-s4-1", "Z-s4-2"});
	        SendEventsAndReset(s4Events);

	        s5Events = SupportBean_S5.MakeS5("Z", new string[] {"Z-s5-1"});
	        SendEvent(s5Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0]},
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0]},
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0]},
	            new object[]{s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1], s5Events[0]}
	        }, GetAndResetNewEvents());
	    }

        private void SendEventsAndReset(object[] events)
        {
            SendEvent(events);
            _updateListener.Reset();
        }

        private void SendEvent(object[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
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
                    "s5"
                });
        }
    }
} // end of namespace
