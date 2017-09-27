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
	public class TestOuterJoinChain4Stream  {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;
        private static readonly string EVENT_S3 = typeof(SupportBean_S3).FullName;

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
	    public void TestLeftOuterJoin_root_s0() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s2.p20 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s1() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s2.p20 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s2() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s2.p20 = s1.p10 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s1.p10 = s0.p00 " +
	                               " left outer join " + EVENT_S3 + "#length(1000) as s3 on s2.p20 = s3.p30 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s3() {
	        string joinStatement = "select * from " +
	                               EVENT_S3 + "#length(1000) as s3 " +
	                               " right outer join " + EVENT_S2 + "#length(1000) as s2 on s3.p30 = s2.p20 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s2.p20 = s1.p10 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s1.p10 = s0.p00 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts() {
	        object[] s0Events, s1Events, s2Events, s3Events;

	        // Test s0 and s1=1, s2=1, s3=1
	        //
	        s1Events = SupportBean_S1.MakeS1("A", new string[] {"A-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("A", new string[] {"A-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("A", new string[] {"A-s3-1"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(
	        new object[][] { new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}}, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=0, s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("B", new string[] {"B-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { s0Events[0], s1Events[0], null, null } }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("C", new string[] {"C-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(
            new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0], null } }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=0, s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1", "D-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("D", new string[] {"D-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=0
	        //
	        s1Events = SupportBean_S1.MakeS1("E", new string[] {"E-s1-1", "E-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1", "E-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], null},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], null},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], null}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=1
	        //
	        s1Events = SupportBean_S1.MakeS1("F", new string[] {"F-s1-1", "F-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1", "F-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("F", new string[] {"F-s3-1"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=2, s2=2, s3=2
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-1", "G-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("G", new string[] {"G-s2-1", "G-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("G", new string[] {"G-s3-1", "G-s3-2"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 and s1=1, s2=1, s3=3
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("H", new string[] {"H-s3-1", "H-s3-2", "H-s3-3"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=0, s1=0, s2=0
	        //
	        s3Events = SupportBean_S3.MakeS3("I", new string[] {"I-s3-1"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s3 and s0=0, s1=0, s2=1
	        //
	        s2Events = SupportBean_S2.MakeS2("J", new string[] {"J-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("J", new string[] {"J-s3-1"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s3 and s0=0, s1=1, s2=1
	        //
	        s2Events = SupportBean_S2.MakeS2("K", new string[] {"K-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s3Events = SupportBean_S3.MakeS3("K", new string[] {"K-s3-1"});
	        SendEvent(s3Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s3 and s0=1, s1=1, s2=1
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[] {"M-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("M", new string[] {"M-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=2, s2=1
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("N", new string[] {"N-s1-1", "N-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("N", new string[] {"N-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("N", new string[] {"N-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=1, s1=2, s2=3
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1", "O-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[] {"O-s2-1", "O-s2-2", "O-s2-3"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("O", new string[] {"O-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s3 and s0=2, s1=2, s2=3
	        //
	        s0Events = SupportBean_S0.MakeS0("P", new string[] {"P-s0-1", "P-s0-2"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1", "P-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("P", new string[] {"P-s2-1", "P-s2-2", "P-s2-3"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("P", new string[] {"P-s3-1"});
	        SendEvent(s3Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[2], s3Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[2], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=0, s2=1, s3=0
	        //
	        s2Events = SupportBean_S2.MakeS2("Q", new string[] {"Q-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("Q", new string[] {"Q-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s1 and s0=2, s2=1, s3=0
	        //
	        s0Events = SupportBean_S0.MakeS0("R", new string[] {"R-s0-1", "R-s0-2"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("R", new string[] {"R-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s1 and s0=2, s2=2, s3=2
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[] {"S-s0-1", "S-s0-2"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("S", new string[] {"S-s2-1"});
	        SendEventsAndReset(s2Events);

	        s3Events = SupportBean_S3.MakeS3("S", new string[] {"S-s3-1", "S-s3-1"});
	        SendEventsAndReset(s3Events);

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1"});
	        SendEvent(s1Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=0, s1=0, s3=1
	        //
	        s3Events = SupportBean_S3.MakeS3("T", new string[] {"T-s3-1"});
	        SendEventsAndReset(s3Events);

	        s2Events = SupportBean_S2.MakeS2("T", new string[] {"T-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s2 and s0=0, s1=1, s3=1
	        //
	        s3Events = SupportBean_S3.MakeS3("U", new string[] {"U-s3-1"});
	        SendEventsAndReset(s3Events);

	        s1Events = SupportBean_S1.MakeS1("U", new string[] {"U-s1-1"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("U", new string[] {"U-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s2 and s0=1, s1=1, s3=1
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("V", new string[] {"V-s1-1"});
	        SendEvent(s1Events);

	        s3Events = SupportBean_S3.MakeS3("V", new string[] {"V-s3-1"});
	        SendEventsAndReset(s3Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=2, s1=2, s3=0
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[] {"W-s0-1", "W-s0-2"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[] {"W-s1-1", "W-s1-2"});
	        SendEvent(s1Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[] {"W-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], null},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], null},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], null},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], null}
	        }, GetAndResetNewEvents());

	        // Test s2 and s0=2, s1=2, s3=2
	        //
	        s0Events = SupportBean_S0.MakeS0("X", new string[] {"X-s0-1", "X-s0-2"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("X", new string[] {"X-s1-1", "X-s1-2"});
	        SendEvent(s1Events);

	        s3Events = SupportBean_S3.MakeS3("X", new string[] {"X-s3-1", "X-s3-2"});
	        SendEventsAndReset(s3Events);

	        s2Events = SupportBean_S2.MakeS2("X", new string[] {"X-s2-1"});
	        SendEvent(s2Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
	            new object[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
	            new object[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
	            new object[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]},
	            new object[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1]}
	        }, GetAndResetNewEvents());
	    }

	    private void SendEvent(object theEvent) {
	        _epService.EPRuntime.SendEvent(theEvent);
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
                    "s3"
                });
        }
	}
} // end of namespace
