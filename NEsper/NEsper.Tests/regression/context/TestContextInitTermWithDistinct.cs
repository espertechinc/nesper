///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
	public class TestContextInitTermWithDistinct
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        configuration.AddEventType<SupportBean_S0>();
	        configuration.AddEventType<SupportBean_S1>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName); }

	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestInvalid() {
	        // require stream name assignment using 'as'
	        TryInvalid("create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds",
	                "Error starting statement: Distinct-expressions require that a stream name is assigned to the stream using 'as' [create context MyContext initiated by distinct(TheString) SupportBean terminated after 15 seconds]");

	        // require stream
	        TryInvalid("create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds",
	                "Error starting statement: Distinct-expressions require a stream as the initiated-by condition [create context MyContext initiated by distinct(a.TheString) pattern [a=SupportBean] terminated after 15 seconds]");

	        // invalid distinct-clause expression
	        TryInvalid("create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds",
	                   "Error starting statement: Invalid context distinct-clause expression 'subselect_0': Aggregation, sub-select, previous or prior functions are not supported in this context [create context MyContext initiated by distinct((select * from MyWindow)) SupportBean as sb terminated after 15 seconds]");

	        // empty list of expressions
	        TryInvalid("create context MyContext initiated by distinct() SupportBean terminated after 15 seconds",
	                "Error starting statement: Distinct-expressions have not been provided [create context MyContext initiated by distinct() SupportBean terminated after 15 seconds]");

	        // non-overlapping context not allowed with distinct
	        TryInvalid("create context MyContext start distinct(TheString) SupportBean end after 15 seconds",
	                "Incorrect syntax near 'distinct' (a reserved keyword) at line 1 column 31 [create context MyContext start distinct(TheString) SupportBean end after 15 seconds]");
	    }

        [Test]
	    public void TestDistinctOverlappingSingleKey() {
	        _epService.EPAdministrator.CreateEPL(
	                "create context MyContext " +
	                "  initiated by distinct(s0.TheString) SupportBean(IntPrimitive = 0) s0" +
	                "  terminated by SupportBean(TheString = s0.TheString and IntPrimitive = 1)");

	        string[] fields = "TheString,LongPrimitive,cnt".Split(',');
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(
	                "context MyContext " +
	                "select TheString, LongPrimitive, count(*) as cnt from SupportBean(TheString = context.s0.TheString)");
	        stmt.AddListener(_listener);

	        SendEvent(_epService, "A", -1, 10);
	        SendEvent(_epService, "A", 1, 11);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent(_epService, "A", 0, 12);   // allocate context
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 12L, 1L});

	        SendEvent(_epService, "A", 0, 13);   // counts towards the existing context, not having a new one
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 13L, 2L});

	        SendEvent(_epService, "A", -1, 14);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 14L, 3L});

	        SendEvent(_epService, "A", 1, 15);   // context termination
	        SendEvent(_epService, "A", -1, 16);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent(_epService, "A", 0, 17);   // allocate context
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 17L, 1L});

	        SendEvent(_epService, "A", -1, 18);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 18L, 2L});

	        SendEvent(_epService, "B", 0, 19);   // allocate context
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"B", 19L, 1L});

	        SendEvent(_epService, "B", -1, 20);
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"B", 20L, 2L});

	        SendEvent(_epService, "A", 1, 21);   // context termination
	        SendEvent(_epService, "B", 1, 22);   // context termination
	        SendEvent(_epService, "A", -1, 23);
	        SendEvent(_epService, "B", -1, 24);
	        Assert.IsFalse(_listener.IsInvoked);

	        SendEvent(_epService, "A", 0, 25);   // allocate context
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"A", 25L, 1L});

	        SendEvent(_epService, "B", 0, 26);   // allocate context
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"B", 26L, 1L});
	    }

        [Test]
	    public void TestDistinctOverlappingMultiKey() {
	        string epl = "create context MyContext as " +
	                "initiated by distinct(TheString, IntPrimitive) SupportBean as sb " +
	                "terminated SupportBean_S1";         // any S1 ends the contexts
	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        EPStatement stmtContext = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(stmtContext.Text, model.ToEPL());

	        string[] fields = "id,p00,p01,cnt".Split(',');
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(
	                "context MyContext " +
	                "select id, p00, p01, count(*) as cnt " +
	                        "from SupportBean_S0(id = context.sb.IntPrimitive and p00 = context.sb.TheString)");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E1"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "A", "E1", 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E2"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "A", "E2", 2L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // terminate all
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E3"));
	        _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 1));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A", "E4"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "A", "E4", 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E5"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2, "B", "E5", 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "B", "E6"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1, "B", "E6", 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E7"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2, "B", "E7", 2L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(-1)); // terminate all
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E8"));
	        _epService.EPRuntime.SendEvent(new SupportBean("B", 2));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E9"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E9", 1L});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E10"));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E10", 2L});

	        // destroy context partition, should forget about the distinct key
	        if (GetSpi(_epService).IsSupportsExtract) {
	            GetSpi(_epService).DestroyContextPartitions("MyContext", new ContextPartitionSelectorAll());
	            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E11"));
	            _epService.EPRuntime.SendEvent(new SupportBean("B", 2));
	            Assert.IsFalse(_listener.IsInvoked);

	            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B", "E12"));
	            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{2, "B", "E12", 1L});
	        }
	    }

	    private static void SendEvent(EPServiceProvider engine, string theString, int intPrimitive, long longPrimitive) {
	        SupportBean @event = new SupportBean(theString, intPrimitive);
	        @event.LongPrimitive = longPrimitive;
	        engine.EPRuntime.SendEvent(@event);
	    }

	    private void TryInvalid(string epl, string message) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(message, ex.Message);
	        }
	    }

	    private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
	        return ((EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin);
	    }
	}
} // end of namespace
