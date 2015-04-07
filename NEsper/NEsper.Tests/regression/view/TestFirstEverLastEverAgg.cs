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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestFirstEverLastEverAgg  {

	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;

        [SetUp]
	    public void SetUp()
	    {
	        listener = new SupportUpdateListener();
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean", typeof(SupportBean));
	        config.AddEventType("SupportBean_A", typeof(SupportBean_A));
	        epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        listener = null;
	    }

        [Test]
	    public void TestFirstEverLastEver()
	    {
	        RunAssertionFirstLastEver(true);
	        RunAssertionFirstLastEver(false);

	        SupportMessageAssertUtil.TryInvalid(epService, "select countever(distinct IntPrimitive) from SupportBean",
	                "Error starting statement: Failed to validate select-clause expression 'countever(distinct IntPrimitive)': Aggregation function 'countever' does now allow distinct [");
	    }

        [Test]
	    public void TestOnDelete()
	    {
	        epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
	        epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow where TheString = id");

	        string[] fields = "firsteverstring,lasteverstring,counteverall".Split(',');
	        string epl = "select firstever(TheString) as firsteverstring, " +
	                "lastever(TheString) as lasteverstring," +
	                "countever(*) as counteverall from MyWindow";
	        EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1", 1L});

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E2", 2L});

	        epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});

	        epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});

	        epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});

	        epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E3", 3L});
	    }

	    private void RunAssertionFirstLastEver(bool soda) {
	        string[] fields = "firsteverstring,firsteverint,lasteverstring,lasteverint,counteverstar,counteverexpr,counteverexprfilter".Split(',');

	        string epl = "select " +
	                "firstever(TheString) as firsteverstring, " +
	                "lastever(TheString) as lasteverstring, " +
	                "firstever(IntPrimitive) as firsteverint, " +
	                "lastever(IntPrimitive) as lasteverint, " +
	                "countever(*) as counteverstar, " +
	                "countever(IntBoxed) as counteverexpr, " +
	                "countever(IntBoxed,BoolPrimitive) as counteverexprfilter " +
	                "from SupportBean.win:length(2)";
	        EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl);
	        stmt.AddListener(listener);

	        MakeSendBean("E1", 10, 100, true);
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E1", 10, 1L, 1L, 1L});

	        MakeSendBean("E2", 11, null, true);
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E2", 11, 2L, 1L, 1L});

	        MakeSendBean("E3", 12, 120, false);
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10, "E3", 12, 3L, 2L, 1L});

	        stmt.Dispose();
	    }

	    private void MakeSendBean(string theString, int intPrimitive, int? intBoxed, bool boolPrimitive) {
	        SupportBean sb = new SupportBean(theString, intPrimitive);
	        sb.IntBoxed = intBoxed;
	        sb.BoolPrimitive = boolPrimitive;
	        epService.EPRuntime.SendEvent(sb);
	    }
	}
} // end of namespace
