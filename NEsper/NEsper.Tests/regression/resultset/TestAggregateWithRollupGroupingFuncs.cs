///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestAggregateWithRollupGroupingFuncs 
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
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName);}
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestFAFCarEventAndGroupingFunc() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));
	        _epService.EPAdministrator.CreateEPL("create window CarWindow.win:keepall() as CarEvent");
	        _epService.EPAdministrator.CreateEPL("insert into CarWindow select * from CarEvent");

	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10000));
	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "germany", 5000));
	        _epService.EPRuntime.SendEvent(new CarEvent("bmw", "france", 100));
	        _epService.EPRuntime.SendEvent(new CarEvent("bmw", "germany", 1000));
	        _epService.EPRuntime.SendEvent(new CarEvent("opel", "france", 7000));
	        _epService.EPRuntime.SendEvent(new CarEvent("opel", "germany", 7000));

	        var epl = "select name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place) as gid " +
	            "from CarWindow group by grouping sets((name, place),name, place,())";
	        var result = _epService.EPRuntime.ExecuteQuery(epl);

	        Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("grouping(name)"));
            Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("gid"));

	        var fields = new string[] {"name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid"};
	        EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]
            {
	            new object[] {"skoda",   "france",   10000, 0, 0, 0},
	            new object[] {"skoda",   "germany",      5000, 0, 0, 0},
	            new object[] {"bmw",     "france",   100, 0, 0, 0},
	            new object[] {"bmw",     "germany",      1000, 0, 0, 0},
	            new object[] {"opel",    "france",   7000, 0, 0, 0},
	            new object[] {"opel",    "germany",      7000, 0, 0, 0},
	            new object[] {"skoda",   null,           15000, 0, 1, 1},
	            new object[] {"bmw",     null,           1100, 0, 1, 1},
	            new object[] {"opel",    null,           14000, 0, 1, 1},
	            new object[] {null,      "france",   17100, 1, 0, 2},
	            new object[] {null,      "germany",      13000, 1, 0, 2},
	            new object[] {null,      null,           30100, 1, 1, 3}
            });
	    }

        [Test]
	    public void TestDocSampleCarEventAndGroupingFunc() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));

	        // try simple
	        var epl = "select name, place, sum(count), grouping(name), grouping(place), grouping_id(name,place) as gid " +
	                "from CarEvent group by grouping sets((name, place), name, place, ())";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);
	        RunAssertionDocSampleCarEvent();
	        _epService.EPAdministrator.DestroyAllStatements();

	        // try audit
	        _epService.EPAdministrator.CreateEPL("@Audit " + epl).AddListener(_listener);
	        RunAssertionDocSampleCarEvent();
	        _epService.EPAdministrator.DestroyAllStatements();

	        // try model
	        var model = _epService.EPAdministrator.CompileEPL(epl);
	        Assert.AreEqual(epl, model.ToEPL());
	        var stmt = _epService.EPAdministrator.Create(model);
	        Assert.AreEqual(epl, stmt.Text);
	        stmt.AddListener(_listener);
	        RunAssertionDocSampleCarEvent();
	    }

	    private void RunAssertionDocSampleCarEvent() {
	        var fields = new string[] {"name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid"};
	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 100));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"skoda",   "france",   100, 0, 0, 0},
	                new object[] {"skoda",   null,       100, 0, 1, 1},
	                new object[] {null,      "france",   100, 1, 0, 2},
	                new object[] {null,      null,       100, 1, 1, 3}});

	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "germany", 75));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"skoda",   "germany",   75, 0, 0, 0},
	                new object[] {"skoda",   null,       175, 0, 1, 1},
	                new object[] {null,      "germany",   75, 1, 0, 2},
	                new object[] {null,      null,       175, 1, 1, 3}});
	    }

        [Test]
	    public void TestGroupingFuncExpressionUse() {
	        GroupingSupportFunc.GetParameters().Clear();
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(CarEvent));

	        // test uncorrelated subquery and expression-declaration and single-row func
	        _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myfunc", typeof(GroupingSupportFunc).FullName, "Myfunc");
	        _epService.EPAdministrator.CreateEPL("create expression myExpr {x=> '|' || x.name || '|'}");
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(CarInfoEvent));
	        var epl = "select myfunc(" +
	                "  name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place)," +
	                "  (select refId from CarInfoEvent.std:lastevent()), " +
	                "  myExpr(ce)" +
	                "  )" +
	                "from CarEvent ce group by grouping sets((name, place),name, place,())";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);
	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new CarInfoEvent("a", "b", "c01"));

	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10000));
	        EPAssertionUtil.AssertEqualsExactOrder(new object[][] {
	                new object[] {"skoda", "france", 10000, 0, 0, 0, "c01", "|skoda|"},
	                new object[] {"skoda", null, 10000, 0, 1, 1, "c01", "|skoda|"},
	                new object[] {null, "france", 10000, 1, 0, 2, "c01", "|skoda|"},
	                new object[] {null, null, 10000, 1, 1, 3, "c01", "|skoda|"}}, GroupingSupportFunc.AssertGetAndClear(4));
	        _epService.EPAdministrator.DestroyAllStatements();

	        // test "prev" and "prior"
	        var fields = "c0,c1,c2,c3".Split(',');
	        var eplTwo = "select prev(1, name) as c0, prior(1, name) as c1, name as c2, sum(count) as c3 from CarEvent.win:keepall() ce group by rollup(name)";
	        _epService.EPAdministrator.CreateEPL(eplTwo).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new CarEvent("skoda", "france", 10));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {null, null, "skoda", 10}, new object[] {null, null, null, 10}
	        });

	        _epService.EPRuntime.SendEvent(new CarEvent("vw", "france", 15));
	        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new object[][] {
	                new object[] {"skoda", "skoda", "vw", 15}, new object[] {"skoda", "skoda", null, 25}
	        });
	    }

        [Test]
	    public void TestInvalid() {
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

	        // invalid use of function
            var expected = "Failed to validate select-clause expression 'grouping(TheString)': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select grouping(TheString) from SupportBean]";
            TryInvalid("select grouping(TheString) from SupportBean", "Error starting statement: " + expected);
            TryInvalid("select TheString, sum(IntPrimitive) from SupportBean(grouping(TheString) = 1) group by rollup(TheString)",
                    "Failed to validate filter expression 'grouping(TheString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean(grouping(TheString) = 1) group by rollup(TheString)]");
            TryInvalid("select TheString, sum(IntPrimitive) from SupportBean where grouping(TheString) = 1 group by rollup(TheString)",
                    "Failed to validate filter expression 'grouping(TheString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean where grouping(TheString) = 1 group by rollup(TheString)]");
            TryInvalid("select TheString, sum(IntPrimitive) from SupportBean group by rollup(grouping(TheString))",
                    "Error starting statement: The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select TheString, sum(IntPrimitive) from SupportBean group by rollup(grouping(TheString))]");

            // invalid parameters
            TryInvalid("select TheString, sum(IntPrimitive), grouping(LongPrimitive) from SupportBean group by rollup(TheString)",
                    "Error starting statement: Group-by with rollup requires a fully-aggregated query, the query is not full-aggregated because of property 'LongPrimitive' [select TheString, sum(IntPrimitive), grouping(LongPrimitive) from SupportBean group by rollup(TheString)]");
            TryInvalid("select TheString, sum(IntPrimitive), grouping(TheString||'x') from SupportBean group by rollup(TheString)",
                    "Error starting statement: Failed to find expression 'TheString||\"x\"' among group-by expressions [select TheString, sum(IntPrimitive), grouping(TheString||'x') from SupportBean group by rollup(TheString)]");

            TryInvalid("select TheString, sum(IntPrimitive), grouping_id(TheString, TheString) from SupportBean group by rollup(TheString)",
                    "Error starting statement: Duplicate expression 'TheString' among grouping function parameters [select TheString, sum(IntPrimitive), grouping_id(TheString, TheString) from SupportBean group by rollup(TheString)]");
        }

	    private void TryInvalid(string epl, string message)
        {
	        try
            {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex)
            {
                if (!ex.Message.StartsWith(message) || string.IsNullOrWhiteSpace(message))
                {
                    Assert.Fail("\nExpected: " + message + "\nReceived: " + ex.Message);
	            }
	        }
	    }

	    public static class GroupingSupportFunc {
	        private static IList<object[]> parameters = new List<object[]>();

	        public static void Myfunc(string name,
	                                  string place,
	                                  int? cnt,
	                                  int? grpName,
	                                  int? grpPlace,
	                                  int? grpId,
	                                  string refId,
	                                  string namePlusDelim) {
	            parameters.Add(new object[] {name, place, cnt, grpName, grpPlace, grpId, refId, namePlusDelim});
	        }

	        public static IList<object[]> GetParameters() {
	            return parameters;
	        }

	        public static object[][] AssertGetAndClear(int numRows) {
	            Assert.AreEqual(numRows, parameters.Count);
	            var result = parameters.ToArray();
	            parameters.Clear();
	            return result;
	        }
	    }

        public class CarInfoEvent
        {
            public CarInfoEvent(string name, string place, string refId)
            {
	            Name = name;
	            Place = place;
	            RefId = refId;
	        }

	        public string Name { get; private set; }

	        public string Place { get; private set; }

	        public string RefId { get; private set; }
        }

	    public class CarEvent
        {
	        public CarEvent(string name, string place, int count)
            {
	            Name = name;
	            Place = place;
	            Count = count;
	        }

	        public string Name { get; private set; }

	        public string Place { get; private set; }

	        public int Count { get; private set; }
	    }
	}
} // end of namespace
