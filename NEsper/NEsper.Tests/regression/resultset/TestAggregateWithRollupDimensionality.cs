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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateWithRollupDimensionality 
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
	    public void TestOutputWhenTerminated() {
	        RunAssertionOutputWhenTerminated("last", false);
	        RunAssertionOutputWhenTerminated("last", true);
	        RunAssertionOutputWhenTerminated("all", false);
	        RunAssertionOutputWhenTerminated("snapshot", false);
	    }

	    private void RunAssertionOutputWhenTerminated(string outputLimit, bool hinted) {
	        var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            _epService.EPAdministrator.CreateEPL("@Name('s0') create context MyContext start SupportBean_S0(id=1) end SupportBean_S0(id=0)");
            _epService.EPAdministrator.CreateEPL(hint + "@Name('s1') context MyContext select TheString as c0, sum(IntPrimitive) as c1 " +
	                "from SupportBean group by rollup(TheString) output " + outputLimit + " when terminated");
	        _epService.EPAdministrator.GetStatement("s1").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "c0,c1".Split(','),
	                new object[][] { new object[] {"E1", 4}, new object[] {"E2", 2}, new object[] {null, 6}});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), "c0,c1".Split(','),
	                new object[][] { new object[] {"E2", 4}, new object[] {"E1", 11}, new object[] {null, 15}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestGroupByWithComputation() {
	        var stmt = _epService.EPAdministrator.CreateEPL("select LongPrimitive as c0, sum(IntPrimitive) as c1 " +
	                "from SupportBean group by rollup(case when LongPrimitive > 0 then 1 else 0 end)");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c0"));
	        var fields = "c0,c1".Split(',');

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 10L, 1 }, new object[] { null, 1 } });

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 11));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 11L, 3 }, new object[] { null, 3 } });

	        _epService.EPRuntime.SendEvent(MakeEvent("E3", 5, -10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { -10L, 5 }, new object[] { null, 8 } });

	        _epService.EPRuntime.SendEvent(MakeEvent("E4", 6, -11));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { -11L, 11 }, new object[] { null, 14 } });

	        _epService.EPRuntime.SendEvent(MakeEvent("E5", 3, 12));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 12L, 6 }, new object[] { null, 17 } });
	    }

        [Test]
	    public void TestContextPartitionAlsoRollup() {
	        _epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean");
	        _epService.EPAdministrator.CreateEPL("context SegmentedByString select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean group by rollup(TheString, IntPrimitive)").AddListener(_listener);
	        var fields = "c0,c1,c2".Split(',');

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 1, 10L}, new object[] {"E1", null, 10L}, new object[] {null, null, 10L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 20));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 2, 20L}, new object[] {"E1", null, 30L}, new object[] {null, null, 30L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 25));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 1, 25L}, new object[] {"E2", null, 25L}, new object[] {null, null, 25L}});
	    }

        [Test]
	    public void TestOnSelect() {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
	        var stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 as s0 select mw.TheString as c0, sum(mw.IntPrimitive) as c1, count(*) as c2 from MyWindow mw group by rollup(mw.TheString)");
	        stmt.AddListener(_listener);
	        var fields = "c0,c1,c2".Split(',');

	        // {E0, 0}, {E1, 1}, {E2, 2}, {E0, 3}, {E1, 4}, {E2, 5}, {E0, 6}, {E1, 7}, {E2, 8}, {E0, 9}
	        for (var i = 0; i < 10; i++) {
	            var TheString = "E" + i % 3;
	            _epService.EPRuntime.SendEvent(new SupportBean(TheString, i));
	        }

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	            new object[][] { new object[] {"E0", 18, 4L}, new object[] {"E1", 12, 3L}, new object[] {"E2", 15, 3L}, new object[] {null, 18+12+15, 10L}, });

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 6));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][] { new object[] {"E0", 18, 4L}, new object[] {"E1", 12+6, 4L}, new object[] {"E2", 15, 3L}, new object[] {null, 18+12+15+6, 11L}, });
	    }

        [Test]
	    public void TestUnboundRollupUnenclosed() {
	        RunAssertionUnboundRollupUnenclosed("TheString, rollup(IntPrimitive, LongPrimitive)");
	        RunAssertionUnboundRollupUnenclosed("grouping sets(" +
	                "(TheString, IntPrimitive, LongPrimitive)," +
	                "(TheString, IntPrimitive)," +
	                "TheString)");
	        RunAssertionUnboundRollupUnenclosed("TheString, grouping sets(" +
	                "(IntPrimitive, LongPrimitive)," +
	                "(IntPrimitive), ())");
	    }

        [Test]
	    public void TestUnboundCubeUnenclosed() {
	        RunAssertionUnboundCubeUnenclosed("TheString, cube(IntPrimitive, LongPrimitive)");
	        RunAssertionUnboundCubeUnenclosed("grouping sets(" +
	                "(TheString, IntPrimitive, LongPrimitive)," +
	                "(TheString, IntPrimitive)," +
	                "(TheString, LongPrimitive)," +
	                "TheString)");
	        RunAssertionUnboundCubeUnenclosed("TheString, grouping sets(" +
	                "(IntPrimitive, LongPrimitive)," +
	                "(IntPrimitive)," +
	                "(LongPrimitive)," +
	                "())");
	    }

	    private void RunAssertionUnboundCubeUnenclosed(string groupBy) {

	        var fields = "c0,c1,c2,c3".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
	                "group by " + groupBy).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, 100L, 1000d}, new object[] {"E1", null, null, 1000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d}, new object[] {"E1", null, 200L, 2000d}, new object[] {"E1", null, null, 3000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 20, 100L, 4000d}, new object[] {"E1", 20, null, 4000d}, new object[] {"E1", null, 100L, 5000d}, new object[] {"E1", null, null, 7000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 100, 5000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 10, 100L, 5000d}, new object[] {"E2", 10, null, 5000d}, new object[] {"E2", null, 100L, 5000d}, new object[] {"E2", null, null, 5000d}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

	    private void RunAssertionUnboundRollupUnenclosed(string groupBy) {

	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
	                "group by " + groupBy);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
	        Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L, 1000d}, new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, null, 1000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 200L, 2000d}, new object[] {"E1", 10, null, 3000d}, new object[] {"E1", null, null, 3000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 20, 100L, 3000d}, new object[] {"E1", 20, null, 3000d}, new object[] {"E1", null, null, 6000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L, 5000d}, new object[] {"E1", 10, null, 7000d}, new object[] {"E1", null, null, 10000d}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestUnboundGroupingSet2LevelUnenclosed() {
	        RunAssertionUnboundGroupingSet2LevelUnenclosed("TheString, grouping sets(IntPrimitive, LongPrimitive)");
	        RunAssertionUnboundGroupingSet2LevelUnenclosed("grouping sets((TheString, IntPrimitive), (TheString, LongPrimitive))");
	    }

	    private void RunAssertionUnboundGroupingSet2LevelUnenclosed(string groupBy) {

	        var fields = "c0,c1,c2,c3".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, sum(DoublePrimitive) as c3 from SupportBean " +
	                "group by " + groupBy);
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
	        Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100, 1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, null, 1000d}, new object[] {"E1", null, 100L, 1000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 200, 2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 20, null, 2000d}, new object[] {"E1", null, 200L, 2000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200, 3000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, null, 4000d}, new object[] {"E1", null, 200L, 5000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 100, 4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 20, null, 6000d}, new object[] {"E1", null, 100L, 5000d}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestBoundGroupingSet2LevelNoTopNoDetail() {
	        var fields = "c0,c1,c2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean.win:length(4) " +
	                "group by grouping sets(TheString, IntPrimitive)");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", null, 100L}, new object[] {null, 10, 100L}},
	                new object[][]{ new object[] {"E1", null, null}, new object[] {null, 10, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", null, 200L}, new object[] {null, 20, 200L}},
	                new object[][]{ new object[] {"E2", null, null}, new object[] {null, 20, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 20, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", null, 400L}, new object[] {null, 20, 500L}},
	                new object[][]{ new object[] {"E1", null, 100L}, new object[] {null, 20, 200L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 10, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", null, 600L}, new object[] {null, 10, 500L}},
	                new object[][]{ new object[] {"E2", null, 200L}, new object[] {null, 10, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 500));   // removes E1/10/100
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", null, 1100L}, new object[] {"E1", null, 300L}, new object[] {null, 20, 1000L}, new object[] {null, 10, 400L}},
	                new object[][]{ new object[] {"E2", null, 600L},   new object[] {"E1", null, 400L}, new object[] {null, 20, 500L}, new object[] {null, 10, 500L}});
	    }

        [Test]
	    public void TestBoundGroupingSet2LevelTopAndDetail() {
	        var fields = "c0,c1,c2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean.win:length(4) " +
	                "group by grouping sets((), (TheString, IntPrimitive))");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}},
	                new object[][]{ new object[] {null, null, null}, new object[] {"E1", 10, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {null, null, 300L}, new object[] {"E1", 10, 300L}},
	                new object[][]{ new object[] {null, null, 100L}, new object[] {"E1", 10, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {null, null, 600L}, new object[] {"E2", 20, 300L}},
	                new object[][]{ new object[] {null, null, 300L}, new object[] {"E2", 20, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {null, null, 1000L}, new object[] {"E1", 10, 700L}},
	                new object[][]{ new object[] {null, null, 600L}, new object[] {"E1", 10, 300L}});
	    }

        [Test]
	    public void TestUnboundCube4Dim() {
	        var fields = "c0,c1,c2,c3,c4".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, DoublePrimitive as c3, sum(IntBoxed) as c4 from SupportBean " +
	                "group by cube(TheString, IntPrimitive, LongPrimitive, DoublePrimitive)");
	        stmt.AddListener(_listener);
	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));
	        Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("c2"));
	        Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("c3"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100, 1000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 1,   10L,    100d,   1000},  // {0, 1, 2, 3}
	                        new object[] {"E1", 1,   10L,    null,   1000},  // {0, 1, 2}
	                        new object[] {"E1", 1,   null,   100d,   1000},  // {0, 1, 3}
	                        new object[] {"E1", 1,   null,   null,   1000},  // {0, 1}
	                        new object[] {"E1", null,10L,    100d,   1000},  // {0, 2, 3}
	                        new object[] {"E1", null,10L,    null,   1000},  // {0, 2}
	                        new object[] {"E1", null,null,   100d,   1000},  // {0, 3}
	                        new object[] {"E1", null,null,   null,   1000},  // {0}
	                        new object[] {null, 1,   10L,    100d,   1000},  // {1, 2, 3}
	                        new object[] {null, 1,   10L,    null,   1000},  // {1, 2}
	                        new object[] {null, 1,   null,   100d,   1000},  // {1, 3}
	                        new object[] {null, 1,   null,   null,   1000},  // {1}
	                        new object[] {null, null,10L,    100d,   1000},  // {2, 3}
	                        new object[] {null, null,10L,    null,   1000},  // {2}
	                        new object[] {null, null,null,   100d,   1000},  // {3}
	                        new object[] {null, null,null,   null,   1000}   // {}
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 20, 100, 2000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 1,   20L,    100d,   2000},  // {0, 1, 2, 3}
	                        new object[] {"E2", 1,   20L,    null,   2000},  // {0, 1, 2}
	                        new object[] {"E2", 1,   null,   100d,   2000},  // {0, 1, 3}
	                        new object[] {"E2", 1,   null,   null,   2000},  // {0, 1}
	                        new object[] {"E2", null,20L,    100d,   2000},  // {0, 2, 3}
	                        new object[] {"E2", null,20L,    null,   2000},  // {0, 2}
	                        new object[] {"E2", null,null,   100d,   2000},  // {0, 3}
	                        new object[] {"E2", null,null,   null,   2000},  // {0}
	                        new object[] {null, 1,   20L,    100d,   2000},  // {1, 2, 3}
	                        new object[] {null, 1,   20L,    null,   2000},  // {1, 2}
	                        new object[] {null, 1,   null,   100d,   3000},  // {1, 3}
	                        new object[] {null, 1,   null,   null,   3000},  // {1}
	                        new object[] {null, null,20L,    100d,   2000},  // {2, 3}
	                        new object[] {null, null,20L,    null,   2000},  // {2}
	                        new object[] {null, null,null,   100d,   3000},  // {3}
	                        new object[] {null, null,null,   null,   3000}   // {}
	                });

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 100, 4000));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 2,   10L,    100d,   4000},  // {0, 1, 2, 3}
	                        new object[] {"E1", 2,   10L,    null,   4000},  // {0, 1, 2}
	                        new object[] {"E1", 2,   null,   100d,   4000},  // {0, 1, 3}
	                        new object[] {"E1", 2,   null,   null,   4000},  // {0, 1}
	                        new object[] {"E1", null,10L,    100d,   5000},  // {0, 2, 3}
	                        new object[] {"E1", null,10L,    null,   5000},  // {0, 2}
	                        new object[] {"E1", null,null,   100d,   5000},  // {0, 3}
	                        new object[] {"E1", null,null,   null,   5000},  // {0}
	                        new object[] {null, 2,   10L,    100d,   4000},  // {1, 2, 3}
	                        new object[] {null, 2,   10L,    null,   4000},  // {1, 2}
	                        new object[] {null, 2,   null,   100d,   4000},  // {1, 3}
	                        new object[] {null, 2,   null,   null,   4000},  // {1}
	                        new object[] {null, null,10L,    100d,   5000},  // {2, 3}
	                        new object[] {null, null,10L,    null,   5000},  // {2}
	                        new object[] {null, null,null,   100d,   7000},  // {3}
	                        new object[] {null, null,null,   null,   7000}   // {}
	                });
	    }

        [Test]
	    public void TestBoundCube3Dim() {
	        RunAssertionBoundCube("cube(TheString, IntPrimitive, LongPrimitive)");
	        RunAssertionBoundCube("grouping sets(" +
	                "(TheString, IntPrimitive, LongPrimitive)," +
	                "(TheString, IntPrimitive)," +
	                "(TheString, LongPrimitive)," +
	                "(TheString)," +
	                "(IntPrimitive, LongPrimitive)," +
	                "(IntPrimitive)," +
	                "(LongPrimitive)," +
	                "()" +
	                ")");
	    }

	    private void RunAssertionBoundCube(string groupBy) {

	        var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	            "select TheString as c0, " +
	                "IntPrimitive as c1, " +
	                "LongPrimitive as c2, " +
	                "count(*) as c3, " +
	                "sum(DoublePrimitive) as c4," +
	                "grouping(TheString) as c5," +
	                "grouping(IntPrimitive) as c6," +
	                "grouping(LongPrimitive) as c7," +
	                "grouping_id(TheString, IntPrimitive, LongPrimitive) as c8 " +
	                "from SupportBean.win:length(4) " +
	            "group by " + groupBy).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 1,   10L,    1L, 100d, 0, 0, 0, 0},  // {0, 1, 2}
	                        new object[] {"E1", 1,   null,   1L, 100d, 0, 0, 1, 1},  // {0, 1}
	                        new object[] {"E1", null,10L,    1L, 100d, 0, 1, 0, 2},  // {0, 2}
	                        new object[] {"E1", null,null,   1L, 100d, 0, 1, 1, 3},  // {0}
	                        new object[] {null, 1,   10L,    1L, 100d, 1, 0, 0, 4},  // {1, 2}
	                        new object[] {null, 1,   null,   1L, 100d, 1, 0, 1, 5},  // {1}
	                        new object[] {null, null,10L,    1L, 100d, 1, 1, 0, 6},  // {2}
	                        new object[] {null, null, null,  1L, 100d, 1, 1, 1, 7}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 20, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 1,   20L,    1L, 200d, 0, 0, 0, 0},
	                        new object[] {"E2", 1,   null,   1L, 200d, 0, 0, 1, 1},
	                        new object[] {"E2", null,20L,    1L, 200d, 0, 1, 0, 2},
	                        new object[] {"E2", null,null,   1L, 200d, 0, 1, 1, 3},
	                        new object[] {null, 1,   20L,    1L, 200d, 1, 0, 0, 4},
	                        new object[] {null, 1,   null,   2L, 300d, 1, 0, 1, 5},
	                        new object[] {null, null,20L,    1L, 200d, 1, 1, 0, 6},
	                        new object[] {null, null, null,  2L, 300d, 1, 1, 1, 7}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 2,   10L,    1L, 300d, 0, 0, 0, 0},
	                        new object[] {"E1", 2,   null,   1L, 300d, 0, 0, 1, 1},
	                        new object[] {"E1", null,10L,    2L, 400d, 0, 1, 0, 2},
	                        new object[] {"E1", null,null,   2L, 400d, 0, 1, 1, 3},
	                        new object[] {null, 2,   10L,    1L, 300d, 1, 0, 0, 4},
	                        new object[] {null, 2,   null,   1L, 300d, 1, 0, 1, 5},
	                        new object[] {null, null,10L,    2L, 400d, 1, 1, 0, 6},
	                        new object[] {null, null, null,  3L, 600d, 1, 1, 1, 7}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 2, 20, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 2,   20L,    1L, 400d, 0, 0, 0, 0},
	                        new object[] {"E2", 2,   null,   1L, 400d, 0, 0, 1, 1},
	                        new object[] {"E2", null,20L,    2L, 600d, 0, 1, 0, 2},
	                        new object[] {"E2", null,null,   2L, 600d, 0, 1, 1, 3},
	                        new object[] {null, 2,   20L,    1L, 400d, 1, 0, 0, 4},
	                        new object[] {null, 2,   null,   2L, 700d, 1, 0, 1, 5},
	                        new object[] {null, null,20L,    2L, 600d, 1, 1, 0, 6},
	                        new object[] {null, null, null,  4L, 1000d, 1, 1, 1, 7}});

	        // expiring/removing ("E1", 1, 10, 100)
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 10, 500));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 1,   10L,    1L, 500d, 0, 0, 0, 0},
	                        new object[] {"E1", 1,   10L,    0L, null, 0, 0, 0, 0},
	                        new object[] {"E2", 1,   null,   2L, 700d, 0, 0, 1, 1},
	                        new object[] {"E1", 1,   null,   0L, null, 0, 0, 1, 1},
	                        new object[] {"E2", null,10L,    1L, 500d, 0, 1, 0, 2},
	                        new object[] {"E1", null,10L,    1L, 300d, 0, 1, 0, 2},
	                        new object[] {"E2", null,null,   3L, 1100d, 0, 1, 1, 3},
	                        new object[] {"E1", null,null,   1L, 300d, 0, 1, 1, 3},
	                        new object[] {null, 1,   10L,    1L, 500d, 1, 0, 0, 4},
	                        new object[] {null, 1,   null,   2L, 700d, 1, 0, 1, 5},
	                        new object[] {null, null,10L,    2L, 800d, 1, 1, 0, 6},
	                        new object[] {null, null, null,  4L, 1400d, 1, 1, 1, 7}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestNamedWindowCube2Dim() {
	        RunAssertionNamedWindowCube2Dim("cube(TheString, IntPrimitive)");
	        RunAssertionNamedWindowCube2Dim("grouping sets(" +
	                "(TheString, IntPrimitive)," +
	                "(TheString)," +
	                "(IntPrimitive)," +
	                "()" +
	                ")");
	    }

	    private void RunAssertionNamedWindowCube2Dim(string groupBy) {

	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(IntBoxed = 0)");
	        _epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 3) delete from MyWindow");

	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
	                "group by " + groupBy).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));    // insert event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, 10, 100L}, new object[] {null, null, 100L}},
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, 10, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 11, 200));    // insert event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 11, 200L}, new object[] {"E1", null, 300L}, new object[] {null, 11, 200L}, new object[] {null, null, 300L}},
	                new object[][]{ new object[] {"E1", 11, null}, new object[] {"E1", null, 100L}, new object[] {null, 11, null}, new object[] {null, null, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));    // insert event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 600L}, new object[] {null, 10, 400L}, new object[] {null, null, 600L}},
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 300L}, new object[] {null, 10, 100L}, new object[] {null, null, 300L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 11, 400));    // insert event
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", 11, 400L}, new object[] {"E2", null, 400L}, new object[] {null, 11, 600L}, new object[] {null, null, 1000L}},
	                new object[][]{ new object[] {"E2", 11, null}, new object[] {"E2", null, null}, new object[] {null, 11, 200L}, new object[] {null, null, 600L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(3, null, -1, -1));    // delete-all
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", 11, null}, new object[] {"E2", 11, null},
	                        new object[] {"E1", null, null}, new object[] {"E2", null, null}, new object[] {null, 10, null}, new object[] {null, 11, null}, new object[] {null, null, null}},
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", 11, 200L}, new object[] {"E2", 11, 400L},
	                        new object[] {"E1", null, 600L}, new object[] {"E2", null, 400L}, new object[] {null, 10, 400L}, new object[] {null, 11, 600L}, new object[] {null, null, 1000L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestNamedWindowDeleteAndRStream2Dim() {
	        RunAssertionNamedWindowDeleteAndRStream2Dim("rollup(TheString, IntPrimitive)");
	        RunAssertionNamedWindowDeleteAndRStream2Dim("grouping sets(" +
	                "(TheString, IntPrimitive)," +
	                "(TheString)," +
	                "())");
	    }

	    private void RunAssertionNamedWindowDeleteAndRStream2Dim(string groupBy) {
	        _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
	        _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean(IntBoxed = 0)");
	        _epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 1) as sb " +
	                "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive");
	        _epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 2) as sb " +
	                "delete from MyWindow mw where sb.TheString = mw.TheString and sb.IntPrimitive = mw.IntPrimitive and sb.LongPrimitive = mw.LongPrimitive");
	        _epService.EPAdministrator.CreateEPL("on SupportBean(IntBoxed = 3) delete from MyWindow");

	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from MyWindow " +
	                "group by " + groupBy).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));    // insert event IntBoxed=0
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}},
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 10, 100));   // delete (IntBoxed = 1)
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 200));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 200L}, new object[] {"E1", null, 200L}, new object[] {null, null, 200L}},
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 300));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", 20, 300L}, new object[] {"E2", null, 300L}, new object[] {null, null, 500L}},
	                new object[][]{ new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 200L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(3, null, 0, 0));   // delete all
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {"E1", 10, null}, new object[] {"E2", 20, null},
	                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
	                        new object[] {null, null, null}},
	                new object[][]{
	                        new object[] {"E1", 10, 200L}, new object[] {"E2", 20, 300L},
	                        new object[] {"E1", null, 200L}, new object[] {"E2", null, 300L},
	                        new object[] {null, null, 500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 400));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}},
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 500));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 20, 500L}, new object[] {"E1", null, 900L}, new object[] {null, null, 900L}},
	                new object[][]{ new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 600));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1500L}},
	                new object[][]{ new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 900L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 700));   // insert
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 1100L}, new object[] {"E1", null, 1600L}, new object[] {null, null, 2200L}},
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(3, null, 0, 0));   // delete all
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{
	                        new object[] {"E1", 10, null}, new object[] {"E1", 20, null}, new object[] {"E2", 20, null},
	                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
	                        new object[] {null, null, null}},
	                new object[][]{
	                        new object[] {"E1", 10, 1100L}, new object[] {"E1", 20, 500L}, new object[] {"E2", 20, 600L},
	                        new object[] {"E1", null, 1600L}, new object[] {"E2", null, 600L},
	                        new object[] {null, null, 2200L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 200));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 400));   // insert
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 20, -1));   // delete (IntBoxed = 1)
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 20, null}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}},
	                new object[][]{ new object[] {"E1", 20, 600L}, new object[] {"E1", null, 1000L}, new object[] {null, null, 1000L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(1, "E1", 10, -1));   // delete (IntBoxed = 1)
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 400L}, new object[] {null, null, 400L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 100));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 200));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 10, 300));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E1", 20, 400));   // insert
	        _epService.EPRuntime.SendEvent(MakeEvent(0, "E2", 20, 500));   // insert
	        _listener.Reset();

	        _epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 200));   // delete specific (IntBoxed = 2)
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}},
	                new object[][]{ new object[] {"E1", 10, 600L}, new object[] {"E1", null, 1000L}, new object[] {null, null, 1500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 300));   // delete specific
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}},
	                new object[][]{ new object[] {"E1", 10, 400L}, new object[] {"E1", null, 800L}, new object[] {null, null, 1300L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 20, 400));   // delete specific
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 20, null}, new object[] {"E1", null, 100L}, new object[] {null, null, 600L}},
	                new object[][]{ new object[] {"E1", 20, 400L}, new object[] {"E1", null, 500L}, new object[] {null, null, 1000L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(2, "E2", 20, 500));   // delete specific
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E2", 20, null}, new object[] {"E2", null, null}, new object[] {null, null, 100L}},
	                new object[][]{ new object[] {"E2", 20, 500L}, new object[] {"E2", null, 500L}, new object[] {null, null, 600L}});

	        _epService.EPRuntime.SendEvent(MakeEvent(2, "E1", 10, 100));   // delete specific
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E1", null, null}, new object[] {null, null, null}},
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestBoundRollup2Dim() {
	        RunAssertionBoundRollup2Dim(false);
	        RunAssertionBoundRollup2Dim(true);
	    }

	    private void RunAssertionBoundRollup2Dim(bool join) {

	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 " +
	                "from SupportBean.win:length(3) " + (join ? ", SupportBean_S0.std:lastevent()" : "") +
	                "group by rollup(TheString, IntPrimitive)").AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));   // expires {TheString="E1", IntPrimitive=10, LongPrimitive=100}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 20, 600L}, new object[] {"E1", 10, null},
	                        new object[] {"E2", null, 600L}, new object[] {"E1", null, 300L},
	                        new object[] {null, null, 900L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 500));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=200}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 20, 900L},
	                        new object[] {"E2", null, 900L},
	                        new object[] {null, null, 1200L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 600));   // expires {TheString="E1", IntPrimitive=11, LongPrimitive=300}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 21, 600L}, new object[] {"E1", 11, null},
	                        new object[] {"E2", null, 1500L}, new object[] {"E1", null, null},
	                        new object[] {null, null, 1500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 700));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=400}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 21, 1300L}, new object[] {"E2", 20, 500L},
	                        new object[] {"E2", null, 1800L},
	                        new object[] {null, null, 1800L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 21, 800));   // expires {TheString="E2", IntPrimitive=20, LongPrimitive=500}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 21, 2100L}, new object[] {"E2", 20, null},
	                        new object[] {"E2", null, 2100L},
	                        new object[] {null, null, 2100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 900));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=600}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 10, 900L}, new object[] {"E2", 21, 1500L},
	                        new object[] {"E1", null, 900L}, new object[] {"E2", null, 1500L},
	                        new object[] {null, null, 2400L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 1000));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=700}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E1", 11, 1000L}, new object[] {"E2", 21, 800L},
	                        new object[] {"E1", null, 1900L}, new object[] {"E2", null, 800L},
	                        new object[] {null, null, 2700L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 1100));   // expires {TheString="E2", IntPrimitive=21, LongPrimitive=800}
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{
	                        new object[] {"E2", 20, 1100L}, new object[] {"E2", 21, null},
	                        new object[] {"E2", null, 1100L},
	                        new object[] {null, null, 3000L}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestUnboundRollup2Dim()
	    {
	        var fields = "c0,c1,c2".Split(',');
	        var stmt = _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean " +
	                "group by rollup(TheString, IntPrimitive)");
	        stmt.AddListener(_listener);

	        Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("c1"));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E1", null, 100L}, new object[] {null, null, 100L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 20, 200L}, new object[] {"E2", null, 200L}, new object[] {null, null, 300L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 11, 300L}, new object[] {"E1", null, 400L}, new object[] {null, null, 600L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 20, 600L}, new object[] {"E2", null, 600L}, new object[] {null, null, 1000L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 11, 800L}, new object[] {"E1", null, 900L}, new object[] {null, null, 1500L}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 600));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10, 700L}, new object[] {"E1", null, 1500L}, new object[] {null, null, 2100L}});
	    }

        [Test]
	    public void TestUnboundRollup1Dim()
	    {
	        RunAssertionUnboundRollup1Dim("rollup(TheString)");
	        RunAssertionUnboundRollup1Dim("cube(TheString)");
	    }

        [Test]
	    public void TestUnboundRollup2DimBatchWindow()
	    {
	        var fields = "c0,c1,c2".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select irstream TheString as c0, IntPrimitive as c1, sum(LongPrimitive) as c2 from SupportBean.win:length_batch(4) " +
	                "group by rollup(TheString, IntPrimitive)").AddListener(_listener);

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 10, 100));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 200));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 300));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 11, 300L},
	                        new object[] {"E1", null, 400L}, new object[] {"E2", null, 600L},
	                        new object[] {null, null, 1000L}},
	                new object[][]{ new object[] {"E1", 10, null}, new object[] {"E2", 20, null}, new object[] {"E1", 11, null},
	                        new object[] {"E1", null, null}, new object[] {"E2", null, null},
	                        new object[] {null, null, null}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 500));
	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 600));
	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 11, 700));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 20, 800));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetDataListsFlattened(), fields,
	                new object[][]{ new object[] {"E1", 11, 1200L}, new object[] {"E2", 20, 1400L}, new object[] {"E1", 10, null},
	                        new object[] {"E1", null, 1200L}, new object[] {"E2", null, 1400L},
	                        new object[] {null, null, 2600L}},
	                new object[][]{ new object[] {"E1", 11, 300L}, new object[] {"E2", 20, 600L}, new object[] {"E1", 10, 100L},
	                        new object[] {"E1", null, 400L}, new object[] {"E2", null, 600L},
	                        new object[] {null, null, 1000L}});
	    }

	    private void RunAssertionUnboundRollup1Dim(string rollup) {

	        var fields = "c0,c1".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, sum(IntPrimitive) as c1 from SupportBean " +
	                "group by " + rollup).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 10}, new object[] {null, 10}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 20}, new object[] {null, 30}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 30));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 40}, new object[] {null, 60}});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 40));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 60}, new object[] {null, 100}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestUnboundRollup3Dim()
	    {
	        var rollupEpl = "rollup(TheString, IntPrimitive, LongPrimitive)";
	        RunAssertionUnboundRollup3Dim(rollupEpl, false);
	        RunAssertionUnboundRollup3Dim(rollupEpl, true);

	        var gsEpl = "grouping sets(" +
	                "(TheString, IntPrimitive, LongPrimitive)," +
	                "(TheString, IntPrimitive)," +
	                "(TheString)," +
	                "()" +
	                ")";
	        RunAssertionUnboundRollup3Dim(gsEpl, false);
	        RunAssertionUnboundRollup3Dim(gsEpl, true);
	    }

	    private void RunAssertionUnboundRollup3Dim(string groupByClause, bool isJoin) {

	        var fields = "c0,c1,c2,c3,c4".Split(',');
	        _epService.EPAdministrator.CreateEPL("@Name('s1')" +
	                "select TheString as c0, IntPrimitive as c1, LongPrimitive as c2, count(*) as c3, sum(DoublePrimitive) as c4 " +
	                "from SupportBean.win:keepall() " + (isJoin ? ", SupportBean_S0.std:lastevent() " : "") +
	                "group by " + groupByClause).AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 1, 10L, 1L, 100d}, new object[] {"E1", 1, null, 1L, 100d}, new object[] {"E1", null, null, 1L, 100d}, new object[] {null, null, null, 1L, 100d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 11, 200));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 1, 11L, 1L, 200d}, new object[] {"E1", 1, null, 2L, 300d}, new object[] {"E1", null, null, 2L, 300d}, new object[] {null, null, null, 2L, 300d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 2, 10, 300));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 2, 10L, 1L, 300d}, new object[] {"E1", 2, null, 1L, 300d}, new object[] {"E1", null, null, 3L, 600d}, new object[] {null, null, null, 3L, 600d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E2", 1, 10, 400));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E2", 1, 10L, 1L, 400d}, new object[] {"E2", 1, null, 1L, 400d}, new object[] {"E2", null, null, 1L, 400d}, new object[] {null, null, null, 4L, 1000d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 10, 500));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 1, 10L, 2L, 600d}, new object[] {"E1", 1, null, 3L, 800d}, new object[] {"E1", null, null, 4L, 1100d}, new object[] {null, null, null, 5L, 1500d}});

	        _epService.EPRuntime.SendEvent(MakeEvent("E1", 1, 11, 600));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
	                new object[][]{ new object[] {"E1", 1, 11L, 2L, 800d}, new object[] {"E1", 1, null, 4L, 1400d}, new object[] {"E1", null, null, 5L, 1700d}, new object[] {null, null, null, 6L, 2100d}});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestMixedAccessAggregation() {
	        var fields = "c0,c1,c2".Split(',');
	        var epl = "select sum(IntPrimitive) as c0, TheString as c1, window(*) as c2 " +
	                "from SupportBean.win:length(2) sb group by rollup(TheString) order by TheString";
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        object eventOne = new SupportBean("E1", 1);
	        _epService.EPRuntime.SendEvent(eventOne);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 1, null, new object[] { eventOne } }, new object[] { 1, "E1", new object[] { eventOne } } });

	        object eventTwo = new SupportBean("E1", 2);
	        _epService.EPRuntime.SendEvent(eventTwo);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 3, null, new object[] { eventOne, eventTwo } }, new object[] { 3, "E1", new object[] { eventOne, eventTwo } } });

	        object eventThree = new SupportBean("E2", 3);
	        _epService.EPRuntime.SendEvent(eventThree);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 5, null, new object[] { eventTwo, eventThree } }, new object[] { 2, "E1", new object[] { eventTwo } }, new object[] { 3, "E2", new object[] { eventThree } } });

	        object eventFour = new SupportBean("E1", 4);
	        _epService.EPRuntime.SendEvent(eventFour);
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                    new object[][] { new object[] { 7, null, new object[] { eventThree, eventFour } }, new object[] { 4, "E1", new object[] { eventFour } } });
	    }

        [Test]
	    public void TestNonBoxedTypeWithRollup() {
	        var stmtOne = _epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(shortPrimitive) " +
	                "from SupportBean group by IntPrimitive, rollup(DoublePrimitive, LongPrimitive)");
	        AssertTypesC0C1C2(stmtOne, typeof(int?), typeof(double?), typeof(long?));

	        var stmtTwo = _epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(shortPrimitive) " +
	                "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive))");
            AssertTypesC0C1C2(stmtTwo, typeof(int?), typeof(double?), typeof(long?));

	        var stmtThree = _epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(shortPrimitive) " +
	                "from SupportBean group by grouping sets ((IntPrimitive, DoublePrimitive, LongPrimitive), (IntPrimitive, DoublePrimitive))");
            AssertTypesC0C1C2(stmtThree, typeof(int?), typeof(double?), typeof(long?));

	        var stmtFour = _epService.EPAdministrator.CreateEPL("select IntPrimitive as c0, DoublePrimitive as c1, LongPrimitive as c2, sum(shortPrimitive) " +
	                "from SupportBean group by grouping sets ((DoublePrimitive, IntPrimitive), (LongPrimitive, IntPrimitive))");
            AssertTypesC0C1C2(stmtFour, typeof(int?), typeof(double?), typeof(long?));
	    }

        [Test]
	    public void TestInvalid() {
	        var prefix = "select TheString, sum(IntPrimitive) from SupportBean group by ";

	        // invalid rollup expressions
	        TryInvalid(prefix + "rollup()",
	                "Incorrect syntax near ')' at line 1 column 69, please check the group-by clause [select TheString, sum(IntPrimitive) from SupportBean group by rollup()]");
	        TryInvalid(prefix + "rollup(TheString, TheString)",
	                "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by rollup(TheString, TheString)]");
	        TryInvalid(prefix + "rollup(x)",
	                "Error starting statement: Failed to validate group-by-clause expression 'x': Property named 'x' is not valid in any stream [select TheString, sum(IntPrimitive) from SupportBean group by rollup(x)]");
	        TryInvalid(prefix + "rollup(LongPrimitive)",
	                "Error starting statement: Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'TheString' [select TheString, sum(IntPrimitive) from SupportBean group by rollup(LongPrimitive)]");
	        TryInvalid(prefix + "rollup((TheString, LongPrimitive), (TheString, LongPrimitive))",
	                "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by rollup((TheString, LongPrimitive), (TheString, LongPrimitive))]");
	        TryInvalid(prefix + "rollup((TheString, LongPrimitive), (LongPrimitive, TheString))",
	                "Failed to validate the group-by clause, found duplicate specification of expressions (TheString, LongPrimitive) [select TheString, sum(IntPrimitive) from SupportBean group by rollup((TheString, LongPrimitive), (LongPrimitive, TheString))]");
	        TryInvalid(prefix + "grouping sets((TheString, TheString))",
	                "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets((TheString, TheString))]");
	        TryInvalid(prefix + "grouping sets(TheString, TheString)",
	                "Failed to validate the group-by clause, found duplicate specification of expressions (TheString) [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets(TheString, TheString)]");
	        TryInvalid(prefix + "grouping sets((), ())",
	                "Failed to validate the group-by clause, found duplicate specification of the overall grouping '()' [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets((), ())]");
	        TryInvalid(prefix + "grouping sets(())",
	                "Failed to validate the group-by clause, the overall grouping '()' cannot be the only grouping [select TheString, sum(IntPrimitive) from SupportBean group by grouping sets(())]");

	        // invalid select clause for this type of query
	        TryInvalid("select * from SupportBean group by grouping sets(TheString)",
	                "Group-by with rollup requires that the select-clause does not use wildcard [select * from SupportBean group by grouping sets(TheString)]");
	        TryInvalid("select sb.* from SupportBean sb group by grouping sets(TheString)",
	                "Group-by with rollup requires that the select-clause does not use wildcard [select sb.* from SupportBean sb group by grouping sets(TheString)]");

	        TryInvalid("@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping sets(TheString)",
	                "Error starting statement: Reclaim hints are not available with rollup [@Hint('disable_reclaim_group') select TheString, count(*) from SupportBean sb group by grouping sets(TheString)]");
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

	    private SupportBean MakeEvent(int intBoxed, string theString, int intPrimitive, long longPrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        sb.IntBoxed = intBoxed;
	        return sb;
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        return sb;
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        sb.DoublePrimitive = doublePrimitive;
	        return sb;
	    }

	    private SupportBean MakeEvent(string theString, int intPrimitive, long longPrimitive, double doublePrimitive, int intBoxed) {
	        var sb = new SupportBean(theString, intPrimitive);
	        sb.LongPrimitive = longPrimitive;
	        sb.DoublePrimitive = doublePrimitive;
	        sb.IntBoxed = intBoxed;
	        return sb;
	    }

	    private void AssertTypesC0C1C2(EPStatement stmtOne, Type expectedC0, Type expectedC1, Type expectedC2) {
	        Assert.AreEqual(expectedC0, stmtOne.EventType.GetPropertyType("c0"));
	        Assert.AreEqual(expectedC1, stmtOne.EventType.GetPropertyType("c1"));
	        Assert.AreEqual(expectedC2, stmtOne.EventType.GetPropertyType("c2"));
	    }
	}
} // end of namespace
