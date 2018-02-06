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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestOuterJoinUnidirectional
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.EngineDefaults.Logging.IsEnableQueryPlan = true;
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _listener = new SupportUpdateListener();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestUnidirectionalOuterJoin() {
	        foreach (Type clazz in new Type[] {typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_C)}) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }

	        // all: unidirectional and full-outer-join
	        RunAssertion2Stream();
	        RunAssertion3Stream();
	        RunAssertion3StreamMixed();
	        RunAssertion4StreamWhereClause();

	        // no-view-declared
	        SupportMessageAssertUtil.TryInvalid(_epService,
	                                            "select * from SupportBean_A unidirectional full outer join SupportBean_B#keepall unidirectional",
	                                            "Error starting statement: The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");

	        // not-all-unidirectional
	        SupportMessageAssertUtil.TryInvalid(_epService,
	                                            "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional full outer join SupportBean_C#keepall",
	                                            "Error starting statement: The unidirectional keyword must either apply to a single stream or all streams in a full outer join");

	        // no iterate
	        SupportMessageAssertUtil.TryInvalidIterate(_epService,
	                "select * from SupportBean_A unidirectional full outer join SupportBean_B unidirectional",
	                "Iteration over a unidirectional join is not supported");
	    }

	    private void RunAssertion2Stream() {
	        foreach (Type clazz in new Type[] {typeof(SupportBean_A), typeof(SupportBean_B), typeof(SupportBean_C), typeof(SupportBean_D)}) {
	            _epService.EPAdministrator.Configuration.AddEventType(clazz);
	        }

	        _epService.EPAdministrator.CreateEPL("select a.id as aid, b.id as bid from SupportBean_A as a unidirectional " +
	                "full outer join SupportBean_B as b unidirectional").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
	        AssertReceived2Stream("A1", null);

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
	        AssertReceived2Stream(null, "B1");

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
	        AssertReceived2Stream(null, "B2");

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
	        AssertReceived2Stream("A2", null);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertion3Stream() {
	        RunAssertion3StreamAllUnidirectional(false);
	        RunAssertion3StreamAllUnidirectional(true);
	    }

	    private void RunAssertion3StreamAllUnidirectional(bool soda) {

	        string epl = "select * from SupportBean_A as a unidirectional " +
	                     "full outer join SupportBean_B as b unidirectional " +
	                     "full outer join SupportBean_C as c unidirectional";
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, epl).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
	        AssertReceived3Stream("A1", null, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
	        AssertReceived3Stream(null, null, "C1");

	        _epService.EPRuntime.SendEvent(new SupportBean_C("C2"));
	        AssertReceived3Stream(null, null, "C2");

	        _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
	        AssertReceived3Stream("A2", null, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
	        AssertReceived3Stream(null, "B1", null);

	        _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
	        AssertReceived3Stream(null, "B2", null);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertion3StreamMixed() {
	        _epService.EPAdministrator.CreateEPL("create window MyCWindow#keepall as SupportBean_C");
	        _epService.EPAdministrator.CreateEPL("insert into MyCWindow select * from SupportBean_C");
	        string epl = "select a.id as aid, b.id as bid, MyCWindow.id as cid, SupportBean_D.id as did " +
	                     "from pattern[every a=SupportBean_A -> b=SupportBean_B] t1 unidirectional " +
	                     "full outer join " +
	                     "MyCWindow unidirectional " +
	                     "full outer join " +
	                     "SupportBean_D unidirectional";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_C("c1"));
	        AssertReceived3StreamMixed(null, null, "c1", null);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("a1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_B("b1"));
	        AssertReceived3StreamMixed("a1", "b1", null, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_A("a2"));
	        _epService.EPRuntime.SendEvent(new SupportBean_B("b2"));
	        AssertReceived3StreamMixed("a2", "b2", null, null);

	        _epService.EPRuntime.SendEvent(new SupportBean_D("d1"));
	        AssertReceived3StreamMixed(null, null, null, "d1");

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertion4StreamWhereClause() {
	        string epl = "select * from SupportBean_A as a unidirectional " +
	                     "full outer join SupportBean_B as b unidirectional " +
	                     "full outer join SupportBean_C as c unidirectional " +
	                     "full outer join SupportBean_D as d unidirectional " +
	                     "where coalesce(a.id,b.id,c.id,d.id) in ('YES')";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        SendAssert(new SupportBean_A("A1"), false);
	        SendAssert(new SupportBean_A("YES"), true);
	        SendAssert(new SupportBean_C("YES"), true);
	        SendAssert(new SupportBean_C("C1"), false);
	        SendAssert(new SupportBean_D("YES"), true);
	        SendAssert(new SupportBean_B("YES"), true);
	        SendAssert(new SupportBean_B("B1"), false);
	    }

	    private void SendAssert(SupportBeanBase @event, bool b) {
	        _epService.EPRuntime.SendEvent(@event);
	        Assert.AreEqual(b, _listener.GetAndClearIsInvoked());
	    }

	    private void AssertReceived2Stream(string a, string b) {
	        string[] fields = "aid,bid".SplitCsv();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {a, b});
	    }

	    private void AssertReceived3Stream(string a, string b, string c) {
	        string[] fields = "a.id,b.id,c.id".SplitCsv();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {a, b, c});
	    }

	    private void AssertReceived3StreamMixed(string a, string b, string c, string d) {
	        string[] fields = "aid,bid,cid,did".SplitCsv();
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {a, b, c, d});
	    }
	}
} // end of namespace
