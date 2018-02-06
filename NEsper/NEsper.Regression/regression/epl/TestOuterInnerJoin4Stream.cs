///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestOuterInnerJoin4Stream
    {
	    private static readonly string[] fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".SplitCsv();

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("S0", typeof(SupportBean_S0));
	        config.AddEventType("S1", typeof(SupportBean_S1));
	        config.AddEventType("S2", typeof(SupportBean_S2));
	        config.AddEventType("S3", typeof(SupportBean_S3));

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestFullMiddleJoinVariantTwo() {
	        string joinStatement =  "select * from S3#keepall s3 " +
	                                " inner join S2#keepall s2 on s3.p30 = s2.p20 " +
	                                " full outer join S1#keepall s1 on s2.p20 = s1.p10 " +
	                                " inner join S0#keepall s0 on s1.p10 = s0.p00";

	        RunAssertionMiddle(joinStatement);
	    }

        [Test]
	    public void TestFullMiddleJoinVariantOne() {
	        string joinStatement =  "select * from S0#keepall s0 " +
	                                " inner join S1#keepall s1 on s0.p00 = s1.p10 " +
	                                " full outer join S2#keepall s2 on s1.p10 = s2.p20 " +
	                                " inner join S3#keepall s3 on s2.p20 = s3.p30";

	        RunAssertionMiddle(joinStatement);
	    }

        [Test]
	    public void TestFullSidedJoinVariantTwo() {
	        string joinStatement =  "select * from S3#keepall s3 " +
	                                " full outer join S2#keepall s2 on s3.p30 = s2.p20 " +
	                                " full outer join S1#keepall s1 on s2.p20 = s1.p10 " +
	                                " inner join S0#keepall s0 on s1.p10 = s0.p00";

	        RunAssertionSided(joinStatement);
	    }

        [Test]
	    public void TestFullSidedJoinVariantOne() {
	        string joinStatement =  "select * from S0#keepall s0 " +
	                                " inner join S1#keepall s1 on s0.p00 = s1.p10 " +
	                                " full outer join S2#keepall s2 on s1.p10 = s2.p20 " +
	                                " full outer join S3#keepall s3 on s2.p20 = s3.p30";

	        RunAssertionSided(joinStatement);
	    }

        [Test]
	    public void TestStarJoinVariantTwo() {
	        string joinStatement =  "select * from S0#keepall s0 " +
	                                " left outer join S1#keepall s1 on s0.p00 = s1.p10 " +
	                                " full outer join S2#keepall s2 on s0.p00 = s2.p20 " +
	                                " inner join S3#keepall s3 on s0.p00 = s3.p30";

	        RunAssertionStar(joinStatement);
	    }

        [Test]
	    public void TestStarJoinVariantOne() {
	        string joinStatement =  "select * from S3#keepall s3 " +
	                                " inner join S0#keepall s0 on s0.p00 = s3.p30 " +
	                                " full outer join S2#keepall s2 on s0.p00 = s2.p20 " +
	                                " left outer join S1#keepall s1 on s1.p10 = s0.p00";

	        RunAssertionStar(joinStatement);
	    }

	    public void RunAssertionMiddle(string expression) {
	        var  fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20, s3.id, s3.p30".SplitCsv();

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(expression);
	        joinView.AddListener(_listener);

	        // s0, s1, s2, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

	        // s0, s2, s3, s1
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

	        // s2, s3, s1, s0
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

	        // s1, s2, s0, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3, "D", 103, "D", 203, "D", 303, "D"});
	    }

	    public void RunAssertionSided(string expression) {
	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(expression);
	        joinView.AddListener(_listener);

	        // s0, s1, s2, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0, "A", 100, "A", null, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0, "A", 100, "A", 200, "A", null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

	        // s0, s2, s3, s1
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

	        // s2, s3, s1, s0
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

	        // s1, s2, s0, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3, "D", 103, "D", 203, "D", null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3, "D", 103, "D", 203, "D", 303, "D"});
	    }

	    public void RunAssertionStar(string expression) {
	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(expression);
	        joinView.AddListener(_listener);

	        // s0, s1, s2, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(300, "A"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0, "A", 100, "A", 200, "A", 300, "A"});

	        // s0, s2, s3, s1
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(301, "B"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "B", null, null, 201, "B", 301, "B"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "B", 101, "B", 201, "B", 301, "B"});

	        // s2, s3, s1, s0
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(302, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2, "C", 102, "C", 202, "C", 302, "C"});

	        // s1, s2, s0, s3
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S3(303, "D"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3, "D", 103, "D", 203, "D", 303, "D"});

	        // s3, s0, s1, s2
	        _epService.EPRuntime.SendEvent(new SupportBean_S3(304, "E"));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {4, "E", null, null, null, null, 304, "E"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {4, "E", 104, "E", null, null, 304, "E"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {4, "E", 104, "E", 204, "E", 304, "E"});
	    }
	}
} // end of namespace
