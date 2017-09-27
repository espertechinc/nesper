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
	public class TestOuterInnerJoin3Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;

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
	    public void TestFullJoinVariantThree() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#keepall as s1 inner join " +
	                               EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20 " +
	                               " full outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";

	        RunAssertionFull(joinStatement);
	    }

        [Test]
	    public void TestFullJoinVariantTwo() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " inner join " + EVENT_S1 + "#keepall s1 on s1.p10 = s2.p20" +
	                               " full outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";

	        RunAssertionFull(joinStatement);
	    }

        [Test]
	    public void TestFullJoinVariantOne() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " full outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10" +
	                               " inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";

	        RunAssertionFull(joinStatement);
	    }

        [Test]
	    public void TestLeftJoinVariantThree() {
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#keepall as s1 left outer join " +
	                               EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               "inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";

	        RunAssertionFull(joinStatement);
	    }

        [Test]
	    public void TestLeftJoinVariantTwo() {
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " inner join " + EVENT_S1 + "#keepall s1 on s1.p10 = s2.p20" +
	                               " left outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10";

	        RunAssertionFull(joinStatement);
	    }

        [Test]
	    public void TestRightJoinVariantOne() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " right outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10" +
	                               " inner join " + EVENT_S2 + "#length(1000) as s2 on s1.p10 = s2.p20";

	        RunAssertionFull(joinStatement);
	    }

	    public void RunAssertionFull(string expression) {
	        var fields = "s0.id, s0.p00, s1.id, s1.p10, s2.id, s2.p20".SplitCsv();

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(expression);
	        joinView.AddListener(_updateListener);

	        // s1, s2, s0
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(100, "A_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(200, "A_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {null, null, 100, "A_1", 200, "A_1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {0, "A_1", 100, "A_1", 200, "A_1"});

	        // s1, s0, s2
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(103, "D_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(203, "D_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {null, null, 103, "D_1", 203, "D_1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "D_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {3, "D_1", 103, "D_1", 203, "D_1"});

	        // s2, s1, s0
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(201, "B_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(101, "B_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {null, null, 101, "B_1", 201, "B_1"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {1, "B_1", 101, "B_1", 201, "B_1"});

	        // s2, s0, s1
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(202, "C_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(102, "C_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {2, "C_1", 102, "C_1", 202, "C_1"});

	        // s0, s1, s2
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(104, "E_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(204, "E_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {4, "E_1", 104, "E_1", 204, "E_1"});

	        // s0, s2, s1
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "F_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(205, "F_1"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(105, "F_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {5, "F_1", 105, "F_1", 205, "F_1"});
	    }
	}
} // end of namespace
