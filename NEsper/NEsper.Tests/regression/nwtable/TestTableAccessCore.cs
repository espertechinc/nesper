///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableAccessCore
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestIntegerIndexedPropertyLookAlike()
        {
            RunAssertionIntegerIndexedPropertyLookAlike(false);
            RunAssertionIntegerIndexedPropertyLookAlike(true);
        }
    
        private void RunAssertionIntegerIndexedPropertyLookAlike(bool soda)
        {
            var eplDeclare = "create table varagg (key int primary key, myevents window(*) @type('SupportBean'))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var eplInto = "into table varagg select window(*) as myevents from SupportBean.win:length(3) group by IntPrimitive";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplInto);
    
            var eplSelect = "select varagg[1] as c0, varagg[1].myevents as c1, varagg[1].myevents.last(*) as c2 from SupportBean_S0";
            var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplSelect);
            stmt.AddListener(_listener);
    
            var e1 = MakeSendBean("E1", 1, 10L);
            var e2 = MakeSendBean("E2", 1, 20L);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            AssertIntegerIndexed(_listener.AssertOneGetNewAndReset(), new SupportBean[] {e1, e2});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertIntegerIndexed(EventBean @event, SupportBean[] events) {
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c0.myevents"));
            EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c1"));
            Assert.AreSame(events[events.Length - 1], @event.Get("c2"));
        }
    
        [Test]
        public void TestFilterBehavior()
        {
            _epService.EPAdministrator.CreateEPL("create table varagg (total count(*))");
            _epService.EPAdministrator.CreateEPL("into table varagg select count(*) as total from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean(varagg.total = IntPrimitive)").AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }
    
        [Test]
        public void TestExprSelectClauseRenderingUnnamedCol()
        {
            _epService.EPAdministrator.CreateEPL("create table varagg (" +
                    "key string primary key, theEvents window(*) @type(SupportBean))");
    
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select " +
                    "varagg.keys()," +
                    "varagg[p00].theEvents," +
                    "varagg[p00]," +
                    "varagg[p00].theEvents.last(*)," +
                    "varagg[p00].theEvents.window(*).take(1) from SupportBean_S0");
    
            var expectedAggType = new object[][]{
                    new object[]{"varagg.keys()", typeof(object[])},
                    new object[]{"varagg[p00].theEvents", typeof(SupportBean[])},
                    new object[]{"varagg[p00]", typeof(IDictionary<string,object>)},
                    new object[]{"varagg[p00].theEvents.last(*)", typeof(SupportBean)},
                    new object[]{"varagg[p00].theEvents.window(*).take(1)", typeof(ICollection<object>)},
            };
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtSelect.EventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
        }
    
        [Test]
        public void TestTopLevelReadGrouped2Keys()
        {
            RunAssertionTopLevelReadGrouped2Keys(false);
            RunAssertionTopLevelReadGrouped2Keys(true);
        }
    
        private void RunAssertionTopLevelReadGrouped2Keys(bool soda)
        {
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create objectarray schema MyEvent as (c0 int, c1 string, c2 int)");
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create table windowAndTotal (" +
                    "keyi int primary key, keys string primary key, TheWindow window(*) @type('MyEvent'), TheTotal sum(int))");
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "into table windowAndTotal " +
                    "select window(*) as TheWindow, sum(c2) as TheTotal from MyEvent.win:length(2) group by c0, c1");
    
            var stmtSelect = SupportModelHelper.CreateByCompileOrParse(_epService, soda, "select windowAndTotal[id,p00] as Val0 from SupportBean_S0");
            stmtSelect.AddListener(_listener);
            AssertTopLevelTypeInfo(stmtSelect);
    
            var e1 = new object[] {10, "G1", 100};
            _epService.EPRuntime.SendEvent(e1, "MyEvent");
    
            var fieldsInner = "TheWindow,TheTotal".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] { e1 }, 100);
    
            var e2 = new object[] {20, "G2", 200};
            _epService.EPRuntime.SendEvent(e2, "MyEvent");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "G2"));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] { e2 }, 200);
    
            var e3 = new object[] {20, "G2", 300};
            _epService.EPRuntime.SendEvent(e3, "MyEvent");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "G1"));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, null, null);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "G2"));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] { e2, e3 }, 500);
    
            // test typable output
            stmtSelect.Dispose();
            var stmtConvert = _epService.EPAdministrator.CreateEPL("insert into AggBean select windowAndTotal[20, 'G2'] as Val0 from SupportBean_S0");
            stmtConvert.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "Val0.TheWindow,Val0.TheTotal".Split(','), new object[]{new object[][]{e2, e3}, 500});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestTopLevelReadUnGrouped()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(AggBean));
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent(c0 int)");
            _epService.EPAdministrator.CreateEPL("create table windowAndTotal (" +
                    "TheWindow window(*) @type(MyEvent), TheTotal sum(int))");
            _epService.EPAdministrator.CreateEPL("into table windowAndTotal " +
                    "select window(*) as TheWindow, sum(c0) as TheTotal from MyEvent.win:length(2)");
    
            var stmt = _epService.EPAdministrator.CreateEPL("select windowAndTotal as Val0 from SupportBean_S0");
            stmt.AddListener(_listener);
    
            var e1 = new object[] {10};
            _epService.EPRuntime.SendEvent(e1, "MyEvent");
    
            var fieldsInner = "TheWindow,TheTotal".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] { e1 }, 10);
    
            var e2 = new object[] {20};
            _epService.EPRuntime.SendEvent(e2, "MyEvent");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsMap((IDictionary<string,object>) _listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] {e1, e2}, 30);
    
            var e3 = new object[] {30};
            _epService.EPRuntime.SendEvent(e3, "MyEvent");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>)_listener.AssertOneGetNewAndReset().Get("Val0"), fieldsInner, new object[][] { e2, e3 }, 50);
    
            // test typable output
            stmt.Dispose();
            var stmtConvert = _epService.EPAdministrator.CreateEPL("insert into AggBean select windowAndTotal as Val0 from SupportBean_S0");
            stmtConvert.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "Val0.TheWindow,Val0.TheTotal".Split(','), new object[]{new object[][]{e2, e3}, 50});
        }
    
        [Test]
        public void TestExpressionAliasAndDecl()
        {
            RunAssertionIntoTableFromExpression();
            RunAssertionExpressionHasTableAccess();
            RunAssertionSubqueryWithExpressionHasTableAccess();
        }

        private void RunAssertionSubqueryWithExpressionHasTableAccess()
        {
            _epService.EPAdministrator.CreateEPL("create table MyTableTwo(theString string primary key, intPrimitive int)");
            _epService.EPAdministrator.CreateEPL("create expression getMyValue{o => (select MyTableTwo[o.p00].intPrimitive from SupportBean_S1.std:lastevent())}");
            _epService.EPAdministrator.CreateEPL("insert into MyTableTwo select theString, intPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0");

            _epService.EPAdministrator.GetStatement("s0").AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".SplitCsv(), new Object[] { 2 });

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionExpressionHasTableAccess()
        {
            _epService.EPAdministrator.CreateEPL("create table MyTableOne(theString string primary key, intPrimitive int)");
            _epService.EPAdministrator.CreateEPL("create expression getMyValue{o => MyTableOne[o.p00].intPrimitive}");
            _epService.EPAdministrator.CreateEPL("insert into MyTableOne select theString, intPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0");

            _epService.EPAdministrator.GetStatement("s0").AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".SplitCsv(), new Object[] { 2 });

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionIntoTableFromExpression()
        {
            _epService.EPAdministrator.CreateEPL("create expression sumi {a -> sum(IntPrimitive)}");
            _epService.EPAdministrator.CreateEPL("create expression sumd alias for {sum(DoublePrimitive)}");
            _epService.EPAdministrator.CreateEPL("create table varagg (" +
                    "sumi sum(int), sumd sum(double), sumf sum(float), suml sum(long))");
            _epService.EPAdministrator.CreateEPL("expression suml alias for {sum(LongPrimitive)} " +
                    "into table varagg " +
                    "select suml, sum(FloatPrimitive) as sumf, sumd, sumi(sb) from SupportBean as sb");
    
            MakeSendBean("E1", 10, 100L, 1000d, 10000f);
    
            var fields = "varagg.sumi,varagg.sumd,varagg.sumf,varagg.suml";
            _epService.EPAdministrator.CreateEPL("select " + fields + " from SupportBean_S0").AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.Split(','), new object[] {10, 1000d, 10000f, 100L});
    
            MakeSendBean("E1", 11, 101L, 1001d, 10001f);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields.Split(','), new object[]{21, 2001d, 20001f, 201L});

            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestGroupedTwoKeyNoContext()
        {
            var eplDeclare = "create table varTotal (key0 string primary key, key1 int primary key, total sum(long), cnt count(*))";
            _epService.EPAdministrator.CreateEPL(eplDeclare);
    
            var eplBind =
                    "into table varTotal " +
                    "select sum(LongPrimitive) as total, count(*) as cnt " +
                    "from SupportBean group by TheString, IntPrimitive";
            _epService.EPAdministrator.CreateEPL(eplBind);
    
            var eplUse = "select varTotal[p00, id].total as c0, varTotal[p00, id].cnt as c1 from SupportBean_S0";
            _epService.EPAdministrator.CreateEPL(eplUse).AddListener(_listener);
    
            MakeSendBean("E1", 10, 100);
    
            var fields = "c0,c1".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {100L, 1L});
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});
        }
    
        [Test]
        public void TestGroupedThreeKeyNoContext()
        {
            var eplDeclare = "create table varTotal (key0 string primary key, key1 int primary key," +
                    "key2 long primary key, total sum(double), cnt count(*))";
            _epService.EPAdministrator.CreateEPL(eplDeclare);
    
            var eplBind = "into table varTotal " +
                             "select sum(DoublePrimitive) as total, count(*) as cnt " +
                             "from SupportBean group by TheString, IntPrimitive, LongPrimitive";
            _epService.EPAdministrator.CreateEPL(eplBind);
    
            var fields = "c0,c1".Split(',');
            var eplUse = "select varTotal[p00, id, 100L].total as c0, varTotal[p00, id, 100L].cnt as c1 from SupportBean_S0";
            _epService.EPAdministrator.CreateEPL(eplUse).AddListener(_listener);
    
            MakeSendBean("E1", 10, 100, 1000);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {1000.0, 1L});
    
            MakeSendBean("E1", 10, 100, 1001);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2001.0, 2L});
        }
    
        [Test]
        public void TestGroupedSingleKeyNoContext()
        {
            RunAssertionGroupedSingleKeyNoContext(false);
            RunAssertionGroupedSingleKeyNoContext(true);
        }
    
        private void RunAssertionGroupedSingleKeyNoContext(bool soda)
        {
            var eplDeclare = "create table varTotal (key string primary key, total sum(int))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var eplBind = "into table varTotal " +
                    "select TheString, sum(IntPrimitive) as total from SupportBean group by TheString";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplBind);
    
            var eplUse = "select p00 as c0, varTotal[p00].total as c1 from SupportBean_S0";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplUse).AddListener(_listener);
    
            RunAssertionTopLevelSingle();
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestUngroupedWContext()
        {
            var eplPart =
                    "create context PartitionedByString partition by TheString from SupportBean, p00 from SupportBean_S0;\n" +
                    "context PartitionedByString create table varTotal (total sum(int));\n" +
                    "context PartitionedByString into table varTotal select sum(IntPrimitive) as total from SupportBean;\n" +
                    "@Name('L') context PartitionedByString select p00 as c0, varTotal.total as c1 from SupportBean_S0;\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplPart);
            _epService.EPAdministrator.GetStatement("L").AddListener(_listener);
    
            RunAssertionTopLevelSingle();
        }
    
        [Test]
        public void TestOrderOfAggregationsAndPush()
        {
            RunAssertionOrderOfAggs(true);
            RunAssertionOrderOfAggs(false);
        }
    
        [Test]
        public void TestMultiStmtContributing()
        {
            RunAssertionMultiStmtContributingDifferentAggs(false);
            RunAssertionMultiStmtContributingDifferentAggs(true);
    
            // contribute to the same aggregation
            _epService.EPAdministrator.CreateEPL("create table sharedagg (total sum(int))");
            _epService.EPAdministrator.CreateEPL("into table sharedagg " +
                    "select p00 as c0, sum(id) as total from SupportBean_S0").AddListener(_listener);
            _epService.EPAdministrator.CreateEPL("into table sharedagg " +
                    "select p10 as c0, sum(id) as total from SupportBean_S1").AddListener(_listener);
            _epService.EPAdministrator.CreateEPL("select TheString as c0, sharedagg.total as total from SupportBean").AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            AssertMultiStmtContributingTotal("A", 10);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-5, "B"));
            AssertMultiStmtContributingTotal("B", 5);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "C"));
            AssertMultiStmtContributingTotal("C", 7);
        }
    
        private void AssertMultiStmtContributingTotal(string c0, int total)
        {
            var fields = "c0,total".Split(',');
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{c0, total});
    
            _epService.EPRuntime.SendEvent(new SupportBean(c0, 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{c0, total});
        }
    
        private void RunAssertionMultiStmtContributingDifferentAggs(bool grouped)
        {
            var eplDeclare = "create table varagg (" +
                    (grouped ? "key string primary key," : "") +
                    "s0sum sum(int), s0cnt count(*), s0win window(*) @type(SupportBean_S0)," +
                    "s1sum sum(int), s1cnt count(*), s1win window(*) @type(SupportBean_S1)" +
                    ")";
            _epService.EPAdministrator.CreateEPL(eplDeclare);
    
            var fieldsSelect = "c0,c1,c2,c3,c4,c5".Split(',');
            var eplSelectUngrouped = "select varagg.s0sum as c0, varagg.s0cnt as c1," +
                    "varagg.s0win as c2, varagg.s1sum as c3, varagg.s1cnt as c4," +
                    "varagg.s1win as c5 from SupportBean";
            var eplSelectGrouped = "select varagg[TheString].s0sum as c0, varagg[TheString].s0cnt as c1," +
                    "varagg[TheString].s0win as c2, varagg[TheString].s1sum as c3, varagg[TheString].s1cnt as c4," +
                    "varagg[TheString].s1win as c5 from SupportBean";
            _epService.EPAdministrator.CreateEPL(grouped ? eplSelectGrouped : eplSelectUngrouped).AddListener(_listener);
    
            var listenerOne = new SupportUpdateListener();
            var fieldsOne = "s0sum,s0cnt,s0win".Split(',');
            var eplBindOne = "into table varagg select sum(id) as s0sum, count(*) as s0cnt, window(*) as s0win from SupportBean_S0.win:length(2) " +
                    (grouped ? "group by p00" : "");
            _epService.EPAdministrator.CreateEPL(eplBindOne).AddListener(listenerOne);
    
            var listenerTwo = new SupportUpdateListener();
            var fieldsTwo = "s1sum,s1cnt,s1win".Split(',');
            var eplBindTwo = "into table varagg select sum(id) as s1sum, count(*) as s1cnt, window(*) as s1win from SupportBean_S1.win:length(2) " +
                    (grouped ? "group by p10" : "");
            _epService.EPAdministrator.CreateEPL(eplBindTwo).AddListener(listenerTwo);
    
            // contribute S1
            var s1_1 = MakeSendS1(10, "G1");
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {10,1L,new object[] {s1_1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                    new object[] {null,0L,null,10,1L,new object[] {s1_1}});
    
            // contribute S0
            var s0_1 = MakeSendS0(20, "G1");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {20,1L,new object[] {s0_1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                    new object[] {20,1L,new object[] {s0_1},10,1L,new object[] {s1_1}});
    
            // contribute S1 and S0
            var s1_2 = MakeSendS1(11, "G1");
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {21,2L,new object[] {s1_1, s1_2}});
            var s0_2 = MakeSendS0(21, "G1");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {41,2L,new object[] {s0_1, s0_2}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                    new object[] {41,2L,new object[] {s0_1, s0_2},21,2L,new object[] {s1_1, s1_2}});
    
            // contribute S1 and S0 (leave)
            var s1_3 = MakeSendS1(12, "G1");
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fieldsTwo, new object[] {23,2L,new object[] {s1_2, s1_3}});
            var s0_3 = MakeSendS0(22, "G1");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fieldsOne, new object[] {43,2L,new object[] {s0_2, s0_3}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                    new object[] {43,2L,new object[] {s0_2, s0_3},23,2L,new object[] {s1_2, s1_3}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private SupportBean_S1 MakeSendS1(int id, string p10)
        {
            var bean = new SupportBean_S1(id, p10);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean_S0 MakeSendS0(int id, string p00)
        {
            var bean = new SupportBean_S0(id, p00);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void RunAssertionOrderOfAggs(bool ungrouped)
        {
            var eplDeclare = "create table varagg (" + (ungrouped ? "" : "key string primary key, ") +
                    "sumint sum(int), " +
                    "sumlong sum(long), " +
                    "mysort sorted(IntPrimitive) @type(SupportBean)," +
                    "mywindow window(*) @type(SupportBean)" +
                    ")";
            _epService.EPAdministrator.CreateEPL(eplDeclare);
    
            var fieldsTable = "sumint,sumlong,mywindow,mysort".Split(',');
            var listenerIntoTable = new SupportUpdateListener();
            var eplSelect = "into table varagg select " +
                    "sum(LongPrimitive) as sumlong, " +
                    "sum(IntPrimitive) as sumint, " +
                    "window(*) as mywindow," +
                    "sorted() as mysort " +
                    "from SupportBean.win:length(2) " +
                    (ungrouped ? "" : "group by TheString ");
            _epService.EPAdministrator.CreateEPL(eplSelect).AddListener(listenerIntoTable);
    
            var fieldsSelect = "c0,c1,c2,c3".Split(',');
            var groupKey = ungrouped ? "" : "['E1']";
            _epService.EPAdministrator.CreateEPL("select " +
                    "varagg" + groupKey + ".sumint as c0, " +
                    "varagg" + groupKey + ".sumlong as c1," +
                    "varagg" + groupKey + ".mywindow as c2," +
                    "varagg"  + groupKey + ".mysort as c3 from SupportBean_S0").AddListener(_listener);
    
            var e1 = MakeSendBean("E1", 10, 100);
            EPAssertionUtil.AssertProps(listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                    new object[] {10, 100L, new object[] {e1}, new object[] {e1}});
    
            var e2 = MakeSendBean("E1", 5, 50);
            EPAssertionUtil.AssertProps(listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                    new object[] {15, 150L, new object[] {e1, e2}, new object[] {e2, e1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                new object[] {15, 150L, new object[] {e1, e2}, new object[] {e2, e1}});
    
            var e3 = MakeSendBean("E1", 12, 120);
            EPAssertionUtil.AssertProps(listenerIntoTable.AssertOneGetNewAndReset(), fieldsTable,
                    new object[] {17, 170L, new object[] {e2, e3}, new object[] {e2, e3}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSelect,
                    new object[] {17, 170L, new object[] {e2, e3}, new object[] {e2, e3}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        [Test]
        public void TestGroupedMixedMethodAndAccess()
        {
            RunAssertionGroupedMixedMethodAndAccess(false);
            RunAssertionGroupedMixedMethodAndAccess(true);
        }
    
        [Test]
        public void TestNamedWindowAndFireAndForget() 
        {
            var epl = "create window MyWindow.win:length(2) as SupportBean;\n" +
                         "insert into MyWindow select * from SupportBean;\n" +
                         "create table varagg (total sum(int));\n" +
                         "into table varagg select sum(IntPrimitive) as total from MyWindow;\n";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            var resultSelect = _epService.EPRuntime.ExecuteQuery("select varagg.total as c0 from MyWindow");
            Assert.AreEqual(10, resultSelect.Array[0].Get("c0"));
    
            var resultDelete = _epService.EPRuntime.ExecuteQuery("delete from MyWindow where varagg.total = IntPrimitive");
            Assert.AreEqual(1, resultDelete.Array.Length);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            var resultUpdate = _epService.EPRuntime.ExecuteQuery("update MyWindow set DoublePrimitive = 100 where varagg.total = IntPrimitive");
            Assert.AreEqual(100d, resultUpdate.Array[0].Get("DoublePrimitive"));
    
            var resultInsert = _epService.EPRuntime.ExecuteQuery("insert into MyWindow (TheString, IntPrimitive) values ('A', varagg.total)");
            EPAssertionUtil.AssertProps(resultInsert.Array[0], "TheString,IntPrimitive".Split(','), new object[] {"A", 20});
        }
    
        [Test]
        public void TestSubquery()
        {
            _epService.EPAdministrator.CreateEPL("create table subquery_var_agg (key string primary key, total count(*))");
            _epService.EPAdministrator.CreateEPL("select (select subquery_var_agg[p00].total from SupportBean_S0.std:lastevent()) as c0 " +
                    "from SupportBean_S1").AddListener(_listener);
            _epService.EPAdministrator.CreateEPL("into table subquery_var_agg select count(*) as total from SupportBean group by TheString");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("c0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("c0"));
        }

        [Test]
        public void TestOnMergeExpressions()
        {
            _epService.EPAdministrator.CreateEPL("create table the_table (key string primary key, total count(*), value int)");
            _epService.EPAdministrator.CreateEPL("into table the_table select count(*) as total from SupportBean group by theString");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 as s0 " +
                    "merge the_table as tt " +
                    "where s0.p00 = tt.key " +
                    "when matched and the_table[s0.p00].total > 0" +
                    "  then update set value = 1");
            _epService.EPAdministrator.CreateEPL("select the_table[p10].value as c0 from SupportBean_S1").AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));

            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "E1"));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("c0"));
        }

        private void RunAssertionGroupedMixedMethodAndAccess(bool soda)
        {
            var eplDeclare = "create table varMyAgg (" +
                    "key string primary key, " +
                    "c0 count(*), " +
                    "c1 count(distinct object), " +
                    "c2 window(*) @type('SupportBean'), " +
                    "c3 sum(long)" +
                    ")";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var eplBind = "into table varMyAgg select " +
                    "count(*) as c0, " +
                    "count(distinct IntPrimitive) as c1, " +
                    "window(*) as c2, " +
                    "sum(LongPrimitive) as c3 " +
                    "from SupportBean.win:length(3) group by TheString";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplBind);
    
            var eplSelect = "select " +
                    "varMyAgg[p00].c0 as c0, " +
                    "varMyAgg[p00].c1 as c1, " +
                    "varMyAgg[p00].c2 as c2, " +
                    "varMyAgg[p00].c3 as c3" +
                    " from SupportBean_S0";
            var stmtSelect = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplSelect);
            stmtSelect.AddListener(_listener);
            var fields = "c0,c1,c2,c3".Split(',');
    
            Assert.AreEqual(typeof(long?), stmtSelect.EventType.GetPropertyType("c0"));
            Assert.AreEqual(typeof(long?), stmtSelect.EventType.GetPropertyType("c1"));
            Assert.AreEqual(typeof(SupportBean[]), stmtSelect.EventType.GetPropertyType("c2"));
            Assert.AreEqual(typeof(long?), stmtSelect.EventType.GetPropertyType("c3"));
    
            var b1 = MakeSendBean("E1", 10, 100);
            var b2 = MakeSendBean("E1", 11, 101);
            var b3 = MakeSendBean("E1", 10, 102);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new object[] {3L, 2L, new SupportBean[] {b1, b2, b3}, 303L});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new object[] {null, null, null, null});
    
            var b4 = MakeSendBean("E2", 20, 200);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new object[] {1L, 1L, new SupportBean[] {b4}, 200L});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTopLevelSingle()
        {
            SendEventsAndAssert("A", 10, "A", 10);
            SendEventsAndAssert("A", 11, "A", 21);
            SendEventsAndAssert("B", 20, "A", 21);
            SendEventsAndAssert("B", 21, "B", 41);
            SendEventsAndAssert("C", 30, "A", 21);
            SendEventsAndAssert("D", 40, "C", 30);
    
            var fields = "c0,c1".Split(',');
            var expected = new int[]{21, 41, 30, 40};
            var count = 0;
            foreach (var p00 in "A,B,C,D".Split(',')) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {p00, expected[count]});
                count++;
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 21});
        }
    
        private void SendEventsAndAssert(string theString, int intPrimitive, string p00, int total)
        {
            var fields = "c0,c1".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean(theString, intPrimitive));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {p00, total});
        }
    
        private SupportBean MakeSendBean(string theString, int intPrimitive, long longPrimitive)
        {
            return MakeSendBean(theString, intPrimitive, longPrimitive, -1);
        }
    
        private SupportBean MakeSendBean(string theString, int intPrimitive, long longPrimitive, double doublePrimitive)
        {
            return MakeSendBean(theString, intPrimitive, longPrimitive, doublePrimitive, -1);
        }
    
        private SupportBean MakeSendBean(string theString, int intPrimitive, long longPrimitive, double doublePrimitive, float floatPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            bean.FloatPrimitive = floatPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void AssertTopLevelTypeInfo(EPStatement stmt)
        {
            Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("Val0"));
            var fragType = stmt.EventType.GetFragmentType("Val0");
            Assert.IsFalse(fragType.IsIndexed);
            Assert.IsFalse(fragType.IsNative);
            Assert.AreEqual(typeof(object[][]), fragType.FragmentType.GetPropertyType("TheWindow"));
            Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("TheTotal"));
        }
    
        internal class AggSubBean
        {
            public int TheTotal { get; set; }
            public object[][] TheWindow { get; set; }
        }
    
        internal class AggBean
        {
            public AggSubBean Val0 { get; set; }
        }
    }
}
