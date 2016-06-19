///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateWithRollupHavingAndOrderBy 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        var config = SupportConfigFactory.GetConfiguration();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestIteratorWindow() {
	        RunAssertionIteratorWindow(false);
	        RunAssertionIteratorWindow(true);
	    }

	    private void RunAssertionIteratorWindow(bool join) {

	        var fields = "c0,c1".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, sum(IntPrimitive) as c1 " +
	                "from SupportBean.win:length(3) " + (join ? ", SupportBean_S0.win:keepall()" : "") +
                    "group by rollup(TheString)");
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] {new object[] {"E1", 1}, new object[] {null, 1}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { null, 3 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { "E1", 4 }, new object[] { "E2", 2 }, new object[] { null, 6 } });

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new object[][] { new object[] { "E2", 6 }, new object[] { "E1", 3 }, new object[] { null, 9 } });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestHaving()
	    {
	        RunAssertionHaving(false);
	        RunAssertionHaving(true);
	    }

        [Test]
	    public void TestUnidirectional() {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
                    "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
	                "from SupportBean_S0 unidirectional, SupportBean.win:keepall() " +
                    "group by cube(TheString, IntPrimitive)").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 10, 100L},
	                        new object[] {"E2", 20, 600L},
	                        new object[] {"E1", 11, 300L},
	                        new object[] {"E1", null, 400L},
	                        new object[] {"E2", null, 600L},
	                        new object[] {null, 10, 100L},
	                        new object[] {null, 20, 600L},
	                        new object[] {null, 11, 300L},
	                        new object[] {null, null, 1000L}
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 1));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 10, 101L},
	                        new object[] {"E2", 20, 600L},
	                        new object[] {"E1", 11, 300L},
	                        new object[] {"E1", null, 401L},
	                        new object[] {"E2", null, 600L},
	                        new object[] {null, 10, 101L},
	                        new object[] {null, 20, 600L},
	                        new object[] {null, 11, 300L},
	                        new object[] {null, null, 1001L}
	                });
	    }

	    private void RunAssertionHaving(bool join) {

	        // test having on the aggregation alone
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
	                "from SupportBean.win:keepall() " + (join ? ", SupportBean_S0.std:lastevent() " : "") +
	                "group by rollup(TheString, IntPrimitive)" +
	                "having sum(LongPrimitive) > 1000").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{new object[] {null, null, 1500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 600));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{new object[] {"E2", 20, 1200L}, new object[] {"E2", null, 1200L}, new object[] {null, null, 2100L}});
	        _epService.EPAdministrator.DestroyAllStatements();

	        // test having on the aggregation alone
	        var fieldsC0C1 = "c0,c1".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, sum(IntPrimitive) as c1 " +
	                "from SupportBean.win:keepall() " + (join ? ", SupportBean_S0.std:lastevent() " : "") +
	                "group by rollup(TheString) " +
	                "having " +
	                "(TheString is null and sum(IntPrimitive) > 100) " +
	                "or " +
	                "(TheString is not null and sum(IntPrimitive) > 200)").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 50));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fieldsC0C1,
	                new object[][]{new object[] {null, 120}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E3", -300));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fieldsC0C1,
	                new object[][]{new object[] {"E1", 250}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 500));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fieldsC0C1,
	                new object[][]{new object[] {"E2", 570}, new object[] {null, 520}});
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestOrderByTwoCriteriaAsc() {
	        RunAssertionOrderByTwoCriteriaAsc(false);
	        RunAssertionOrderByTwoCriteriaAsc(true);
	    }

	    private void RunAssertionOrderByTwoCriteriaAsc(bool join)
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
	                "from SupportBean.win:time_batch(1 sec) " + (join ? ", SupportBean_S0.std:lastevent() " : "") +
	                "group by rollup(TheString, IntPrimitive) " +
	                "order by TheString, IntPrimitive").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 200));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 300));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 400));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 1000L},
	                        new object[] {"E1", null, 900L},
	                        new object[] {"E1", 10, 300L},
	                        new object[] {"E1", 11, 600L},
	                        new object[] {"E2", null, 100L},
	                        new object[] {"E2", 10, 100L}
	                },
	                new object[][]{new object[] {null, null, null},
	                        new object[] {"E1", null, null},
	                        new object[] {"E1", 10, null},
	                        new object[] {"E1", 11, null},
	                        new object[] {"E2", null, null},
	                        new object[] {"E2", 10, null}
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 12, 700));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{new object[] {null, null, 1800L},
	                        new object[] {"E1", null, 1800L},
	                        new object[] {"E1", 10, 600L},
	                        new object[] {"E1", 11, 500L},
	                        new object[] {"E1", 12, 700L},
	                        new object[] {"E2", null, null},
	                        new object[] {"E2", 10, null}
	                },
	                new object[][]{new object[] {null, null, 1000L},
	                        new object[] {"E1", null, 900L},
	                        new object[] {"E1", 10, 300L},
	                        new object[] {"E1", 11, 600L},
	                        new object[] {"E1", 12, null},
	                        new object[] {"E2", null, 100L},
	                        new object[] {"E2", 10, 100L}
	                });

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestOrderByOneCriteriaDesc()
	    {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean.win:time_batch(1 sec) " +
	                "group by rollup(TheString, IntPrimitive) " +
	                "order by TheString desc").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 200));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 300));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 400));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {"E2", 10, 100L},
	                        new object[] {"E2", null, 100L},
	                        new object[] {"E1", 11, 600L},
	                        new object[] {"E1", 10, 300L},
	                        new object[] {"E1", null, 900L},
	                        new object[] {null, null, 1000L}
	                },
	                new object[][]{
	                        new object[] {"E2", 10, null},
	                        new object[] {"E2", null, null},
	                        new object[] {"E1", 11, null},
	                        new object[] {"E1", 10, null},
	                        new object[] {"E1", null, null},
	                        new object[] {null, null, null}
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 12, 700));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {"E2", 10, null},
	                        new object[] {"E2", null, null},
	                        new object[] {"E1", 11, 500L},
	                        new object[] {"E1", 10, 600L},
	                        new object[] {"E1", 12, 700L},
	                        new object[] {"E1", null, 1800L},
	                        new object[] {null, null, 1800L}
	                },
	                new object[][]{
	                        new object[] {"E2", 10, 100L},
	                        new object[] {"E2", null, 100L},
	                        new object[] {"E1", 11, 600L},
	                        new object[] {"E1", 10, 300L},
	                        new object[] {"E1", 12, null},
	                        new object[] {"E1", null, 900L},
	                        new object[] {null, null, 1000L}
	                });
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        return sb;
	    }
	}
} // end of namespace
