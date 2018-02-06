///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.type;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestOuterJoinVarA3Stream
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        private static readonly string EVENT_S0 = typeof(SupportBean_S0).FullName;
        private static readonly string EVENT_S1 = typeof(SupportBean_S1).FullName;
        private static readonly string EVENT_S2 = typeof(SupportBean_S2).FullName;

        [SetUp]
	    public void SetUp() {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("P1", typeof(SupportBean_S1));
	        config.AddEventType("P2", typeof(SupportBean_S2));
	        config.AddEventType("P3", typeof(SupportBean_S3));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
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
	    public void TestMapLeftJoinUnsortedProps() {
	        string stmtText = "select t1.col1, t1.col2, t2.col1, t2.col2, t3.col1, t3.col2 from type1#keepall as t1" +
	                          " left outer join type2#keepall as t2" +
	                          " on t1.col2 = t2.col2 and t1.col1 = t2.col1" +
	                          " left outer join type3#keepall as t3" +
	                          " on t1.col1 = t3.col1";

	        IDictionary<string, object> mapType = new Dictionary<string, object>();
	        mapType.Put("col1", typeof(string));
	        mapType.Put("col2", typeof(string));
	        _epService.EPAdministrator.Configuration.AddEventType("type1", mapType);
	        _epService.EPAdministrator.Configuration.AddEventType("type2", mapType);
	        _epService.EPAdministrator.Configuration.AddEventType("type3", mapType);

	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(_updateListener);

	        var fields = new string[] {"t1.col1", "t1.col2", "t2.col1", "t2.col2", "t3.col1", "t3.col2"};

	        SendMapEvent("type2", "a1", "b1");
	        Assert.IsFalse(_updateListener.IsInvoked);

	        SendMapEvent("type1", "b1", "a1");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"b1", "a1", null, null, null, null});

	        SendMapEvent("type1", "a1", "a1");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"a1", "a1", null, null, null, null});

	        SendMapEvent("type1", "b1", "b1");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"b1", "b1", null, null, null, null});

	        SendMapEvent("type1", "a1", "b1");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"a1", "b1", "a1", "b1", null, null});

	        SendMapEvent("type3", "c1", "b1");
	        Assert.IsFalse(_updateListener.IsInvoked);

	        SendMapEvent("type1", "d1", "b1");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"d1", "b1", null, null, null, null});

	        SendMapEvent("type3", "d1", "bx");
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {"d1", "b1", null, null, "d1", "bx"});

	        Assert.IsFalse(_updateListener.IsInvoked);
	    }

        [Test]
	    public void TestLeftJoin_2sides_multicolumn() {
	        var fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11, s2.id, s2.p20, s2.p21".SplitCsv();

	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 and s0.p01 = s2.p21";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "A_1", "B_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(11, "A_2", "B_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(12, "A_1", "B_2"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S1(13, "A_2", "B_2"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S2(20, "A_1", "B_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(21, "A_2", "B_1"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(22, "A_1", "B_2"));
	        _epService.EPRuntime.SendEvent(new SupportBean_S2(23, "A_2", "B_2"));
	        Assert.IsFalse(_updateListener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_3", "B_3"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {1, "A_3", "B_3", null, null, null, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "A_1", "B_3"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {2, "A_1", "B_3", null, null, null, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "A_3", "B_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {3, "A_3", "B_1", null, null, null, null, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "A_2", "B_2"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "A_2", "B_1"));
	        EPAssertionUtil.AssertProps(_updateListener.AssertOneGetNewAndReset(), fields, new object[] {5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s0_OM() {
	        EPStatementObjectModel model = new EPStatementObjectModel();
	        model.SelectClause = SelectClause.CreateWildcard();
	        FromClause fromClause = FromClause.Create(
	                                    FilterStream.Create(EVENT_S0, "s0").AddView("keepall"),
	                                    FilterStream.Create(EVENT_S1, "s1").AddView("keepall"),
	                                    FilterStream.Create(EVENT_S2, "s2").AddView("keepall"));
	        fromClause.Add(OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s1.p10"));
	        fromClause.Add(OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s2.p20"));
	        model.FromClause = fromClause;
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

	        Assert.AreEqual("select * from " + typeof(SupportBean_S0).FullName + "#keepall as s0 left outer join " + typeof(SupportBean_S1).FullName + "#keepall as s1 on s0.p00 = s1.p10 left outer join " + typeof(SupportBean_S2).FullName + "#keepall as s2 on s0.p00 = s2.p20", model.ToEPL());
	        EPStatement joinView = _epService.EPAdministrator.Create(model);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s0_Compiled() {
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               "left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               "left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20";

	        EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(joinStatement);
	        model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
	        EPStatement joinView = _epService.EPAdministrator.Create(model);
	        joinView.AddListener(_updateListener);

	        Assert.AreEqual(joinStatement, model.ToEPL());

	        RunAsserts();
	    }

        [Test]
	    public void TestLeftOuterJoin_root_s0() {
	        /// <summary>
	        /// Query:
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S0 + "#length(1000) as s0 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRightOuterJoin_S2_root_s2() {
	        /// <summary>
	        /// Query: right other join is eliminated/translated
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S2 + "#length(1000) as s2 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s2.p20 " +
	                               " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

        [Test]
	    public void TestRightOuterJoin_S1_root_s1() {
	        /// <summary>
	        /// Query: right other join is eliminated/translated
	        /// s0
	        /// </summary>
	        string joinStatement = "select * from " +
	                               EVENT_S1 + "#length(1000) as s1 " +
	                               " right outer join " + EVENT_S0 + "#length(1000) as s0 on s0.p00 = s1.p10 " +
	                               " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 ";

	        EPStatement joinView = _epService.EPAdministrator.CreateEPL(joinStatement);
	        joinView.AddListener(_updateListener);

	        RunAsserts();
	    }

	    private void RunAsserts() {
	        // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
	        //
	        object[] s1Events = SupportBean_S1.MakeS1("A", new string[] {"A-s1-1", "A-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        object[] s2Events = SupportBean_S2.MakeS2("A", new string[] {"A-s2-1", "A-s2-2"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        object[] s0Events = SupportBean_S0.MakeS0("A", new string[] {"A-s0-1"});
	        SendEvent(s0Events);
	        object[][] expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[1], s2Events[0] },
	            new object[] { s0Events[0], s1Events[0], s2Events[1] },
	            new object[] { s0Events[0], s1Events[1], s2Events[1] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, no results for each s1 and s2
	        //
	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, null } }, GetAndResetNewEvents());

	        s0Events = SupportBean_S0.MakeS0("B", new string[] {"B-s0-2"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, null } }, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
	        //
	        s1Events = SupportBean_S1.MakeS1("C", new string[] {"C-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("C", new string[] {"C-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], s1Events[0], null } }, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
	        //
	        s1Events = SupportBean_S1.MakeS1("D", new string[] {"D-s1-1", "D-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("D", new string[] {"D-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], s1Events[0], null},
	            new object[] {s0Events[0], s1Events[1], null}
	        }, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
	        //
	        s2Events = SupportBean_S2.MakeS2("E", new string[] {"E-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("E", new string[] {"E-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, s2Events[0] } }, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
	        //
	        s2Events = SupportBean_S2.MakeS2("F", new string[] {"F-s2-1", "F-s2-2"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("F", new string[] {"F-s0-1"});
	        SendEvent(s0Events);
	        EPAssertionUtil.AssertSameAnyOrder(new object[][] {
	            new object[] {s0Events[0], null, s2Events[0]},
	            new object[] {s0Events[0], null, s2Events[1]}
	        }, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
	        //
	        s1Events = SupportBean_S1.MakeS1("G", new string[] {"G-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("G", new string[] {"G-s2-1", "G-s2-2"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("G", new string[] {"G-s0-2"});
	        SendEvent(s0Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[0], s2Events[1] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
	        //
	        s1Events = SupportBean_S1.MakeS1("H", new string[] {"H-s1-1", "H-s1-2"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("H", new string[] {"H-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("H", new string[] {"H-s0-2"});
	        SendEvent(s0Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[1], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s0 outer join to s1 and s2, one row for each s1 and s2
	        //
	        s1Events = SupportBean_S1.MakeS1("I", new string[] {"I-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s2Events = SupportBean_S2.MakeS2("I", new string[] {"I-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        s0Events = SupportBean_S0.MakeS0("I", new string[] {"I-s0-2"});
	        SendEvent(s0Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("Q", new string[] {"Q-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, null } }, GetAndResetNewEvents());

	        s2Events = SupportBean_S2.MakeS2("Q", new string[] {"Q-s2-1", "Q-s2-2"});
	        SendEvent(s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, s2Events[0] } }, GetAndResetNewEvents());
	        SendEvent(s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, s2Events[1] } }, GetAndResetNewEvents());

	        s1Events = SupportBean_S1.MakeS1("Q", new string[] {"Q-s1-1"});
	        SendEvent(s1Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[0], s2Events[1] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
	        //
	        s2Events = SupportBean_S2.MakeS2("R", new string[] {"R-s2-1", "R-s2-2"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("R", new string[] {"R-s1-1"});
	        SendEvent(s1Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("S", new string[] {"S-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, null } }, GetAndResetNewEvents());

	        s1Events = SupportBean_S1.MakeS1("S", new string[] {"S-s1-1"});
	        SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], s1Events[0], null } }, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("T", new string[] {"T-s0-1"});
	        SendEvent(s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, null } }, GetAndResetNewEvents());

	        s2Events = SupportBean_S2.MakeS2("T", new string[] {"T-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("T", new string[] {"T-s1-1"});
	        SendEvent(s1Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } }, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("U", new string[] {"U-s0-1", "U-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("U", new string[] {"U-s1-1"});
	        SendEvent(s1Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], null },
	            new object[] { s0Events[1], s1Events[0], null },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("V", new string[] {"V-s0-1", "V-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("V", new string[] {"V-s2-1"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("V", new string[] {"V-s1-1"});
	        SendEvent(s1Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[1], s1Events[0], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("W", new string[] {"W-s0-1", "W-s0-2"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("W", new string[] {"W-s2-1", "W-s2-2"});
	        SendEventsAndReset(s2Events);

	        s1Events = SupportBean_S1.MakeS1("W", new string[] {"W-s1-1"});
	        SendEvent(s1Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[1], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[0], s2Events[1] },
	            new object[] { s0Events[1], s1Events[0], s2Events[1] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("J", new string[] {"J-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("J", new string[] {"J-s1-1", "J-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("J", new string[] {"J-s2-1"});
	        SendEvent(s2Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[1], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
	        //
	        s1Events = SupportBean_S1.MakeS1("K", new string[] {"K-s1-1", "K-s1-2"});
	        SendEventsAndReset(s2Events);

	        s2Events = SupportBean_S2.MakeS2("K", new string[] {"K-s2-1"});
	        SendEvent(s2Events);
	        Assert.IsFalse(_updateListener.IsInvoked);

	        // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("L", new string[] {"L-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("L", new string[] {"L-s2-1"});
	        SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], null, s2Events[0] } }, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("M", new string[] {"M-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("M", new string[] {"M-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("M", new string[] {"M-s2-1"});
	        SendEvent(s2Events);
            EPAssertionUtil.AssertSameAnyOrder(new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0] } }, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("N", new string[] {"N-s0-1", "N-s0-1"});
	        SendEventsAndReset(s0Events);

	        s2Events = SupportBean_S2.MakeS2("N", new string[] {"N-s2-1"});
	        SendEvent(s2Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], null, s2Events[0]},
	            new object[] { s0Events[1], null, s2Events[0]},
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("O", new string[] {"O-s0-1", "O-s0-1"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("O", new string[] {"O-s1-1"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("O", new string[] {"O-s2-1"});
	        SendEvent(s2Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[1], s1Events[0], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());

	        // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
	        //
	        s0Events = SupportBean_S0.MakeS0("P", new string[] {"P-s0-1", "P-s0-2"});
	        SendEventsAndReset(s0Events);

	        s1Events = SupportBean_S1.MakeS1("P", new string[] {"P-s1-1", "P-s1-2"});
	        SendEventsAndReset(s1Events);

	        s2Events = SupportBean_S2.MakeS2("P", new string[] {"P-s2-1"});
	        SendEvent(s2Events);
	        expected = new object[][] {
	            new object[] { s0Events[0], s1Events[0], s2Events[0] },
	            new object[] { s0Events[1], s1Events[0], s2Events[0] },
	            new object[] { s0Events[0], s1Events[1], s2Events[0] },
	            new object[] { s0Events[1], s1Events[1], s2Events[0] },
	        };
	        EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents());
	    }

        [Test]
	    public void TestInvalidMulticolumn() {
	        try {
	            string joinStatement = "select * from " +
	                                   EVENT_S0 + "#length(1000) as s0 " +
	                                   " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
	                                   " left outer join " + EVENT_S2 + "#length(1000) as s2 on s0.p00 = s2.p20 and s1.p11 = s2.p21";
	            _epService.EPAdministrator.CreateEPL(joinStatement);
                Assert.Fail();
	        } catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause");
	        }

	        try {
	            string joinStatement = "select * from " +
	                                   EVENT_S0 + "#length(1000) as s0 " +
	                                   " left outer join " + EVENT_S1 + "#length(1000) as s1 on s0.p00 = s1.p10 and s0.p01 = s1.p11" +
	                                   " left outer join " + EVENT_S2 + "#length(1000) as s2 on s2.p20 = s0.p00 and s2.p20 = s1.p11";
	            _epService.EPAdministrator.CreateEPL(joinStatement);
	            Assert.Fail();
	        } catch (EPStatementException ex) {
	            SupportMessageAssertUtil.AssertMessage(ex, "Error validating expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [");
	        }
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

	    private void SendMapEvent(string type, string col1, string col2) {
	        IDictionary<string, object> mapEvent = new Dictionary<string, object>();
	        mapEvent.Put("col1", col1);
	        mapEvent.Put("col2", col2);
	        _epService.EPRuntime.SendEvent(mapEvent, type);
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
                    "s2"
                });
        }
    }
} // end of namespace
