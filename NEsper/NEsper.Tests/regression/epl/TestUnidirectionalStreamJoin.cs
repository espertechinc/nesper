///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestUnidirectionalStreamJoin
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestPatternUnidirectionalOuterJoinNoOn()
        {
            // test 2-stream left outer join and SODA
            //
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

            String stmtTextLO = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from pattern [every timer:interval(1 seconds)] unidirectional " +
                    "left outer join " +
                    "SupportBean.win:keepall()";
            EPStatement stmtLO = _epService.EPAdministrator.CreateEPL(stmtTextLO);
            stmtLO.Events += _listener.Update;

            RunAssertionPatternUniOuterJoinNoOn(0);

            stmtLO.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtTextLO);
            Assert.AreEqual(stmtTextLO, model.ToEPL());
            stmtLO = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTextLO, stmtLO.Text);
            stmtLO.Events += _listener.Update;

            RunAssertionPatternUniOuterJoinNoOn(100000);

            stmtLO.Dispose();

            // test 2-stream inner join
            //
            String[] fieldsIJ = "c0,c1".Split(',');
            String stmtTextIJ = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from SupportBean_S0 unidirectional " +
                    "inner join " +
                    "SupportBean.win:keepall()";
            EPStatement stmtIJ = _epService.EPAdministrator.CreateEPL(stmtTextIJ);
            stmtIJ.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsIJ, new Object[] { 100, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 200));

            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "S0_3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsIJ, new Object[] { 300, 2L });
            stmtIJ.Dispose();

            // test 2-stream inner join with group-by
            RunAssertion2StreamInnerWGroupBy();

            // test 3-stream inner join
            //
            String[] fields3IJ = "c0,c1".Split(',');
            String stmtText3IJ = "select sum(IntPrimitive) as c0, count(*) as c1 " +
                    "from " +
                    "SupportBean_S0.win:keepall()" +
                    "inner join " +
                    "SupportBean_S1.win:keepall()" +
                    "inner join " +
                    "SupportBean.win:keepall()";

            EPStatement stmt3IJ = _epService.EPAdministrator.CreateEPL(stmtText3IJ);
            stmt3IJ.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "S1_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields3IJ, new Object[] { 50, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 51));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields3IJ, new Object[] { 101, 2L });

            stmt3IJ.Dispose();

            // test 3-stream full outer join
            //
            String[] fields3FOJ = "P00,P10,TheString".Split(',');
            String stmtText3FOJ = "select P00, P10, TheString " +
                    "from " +
                    "SupportBean_S0.win:keepall()" +
                    "full outer join " +
                    "SupportBean_S1.win:keepall()" +
                    "full outer join " +
                    "SupportBean.win:keepall()";

            EPStatement stmt3FOJ = _epService.EPAdministrator.CreateEPL(stmtText3FOJ);
            stmt3FOJ.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields3FOJ, new Object[] { "S0_1", null, null });

            _epService.EPRuntime.SendEvent(new SupportBean("E10", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields3FOJ, new Object[] { null, null, "E10" });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields3FOJ, new Object[][] { new Object[] { "S0_2", null, null } });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "S1_0"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields3FOJ, new Object[][] { new Object[] { "S0_1", "S1_0", "E10" }, new Object[] { "S0_2", "S1_0", "E10" } });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_3"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields3FOJ, new Object[][] { new Object[] { "S0_3", "S1_0", "E10" } });

            _epService.EPRuntime.SendEvent(new SupportBean("E11", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.GetAndResetLastNewData(), fields3FOJ, new Object[][] { new Object[] { "S0_1", "S1_0", "E11" }, new Object[] { "S0_2", "S1_0", "E11" }, new Object[] { "S0_3", "S1_0", "E11" } });
            Assert.AreEqual(6, EPAssertionUtil.EnumeratorCount(stmt3FOJ.GetEnumerator()));

            stmt3FOJ.Dispose();

            // test 3-stream full outer join with where-clause
            //
            String[] fields3FOJW = "P00,P10,TheString".Split(',');
            String stmtText3FOJW = "select P00, P10, TheString " +
                    "from " +
                    "SupportBean_S0.win:keepall() as s0 " +
                    "full outer join " +
                    "SupportBean_S1.win:keepall() as s1 " +
                    "full outer join " +
                    "SupportBean.win:keepall() as sb " +
                    "where s0.P00 = s1.P10";

            EPStatement stmt3FOJW = _epService.EPAdministrator.CreateEPL(stmtText3FOJW);
            stmt3FOJW.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "X1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "Y1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "Y1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields3FOJW, new Object[] { "Y1", "Y1", null });
        }

        private void RunAssertion2StreamInnerWGroupBy()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema E1 (id string, grp string, value int)");
            _epService.EPAdministrator.CreateEPL("create objectarray schema E2 (id string, value2 int)");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select count(*) as c0, sum(E1.value) as c1, E1.id as c2 " +
                    "from E1 unidirectional inner join E2.win:keepall() on E1.id = E2.id group by E1.grp");
            stmt.Events += _listener.Update;
            String[] fields = "c0,c1,c2".Split(',');

            _epService.EPRuntime.SendEvent(new Object[] { "A", 100 }, "E2");
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new Object[] { "A", "X", 10 }, "E1");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 1L, 10, "A" });

            _epService.EPRuntime.SendEvent(new Object[] { "A", "Y", 20 }, "E1");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 1L, 20, "A" });
        }

        private void RunAssertionPatternUniOuterJoinNoOn(long startTime)
        {
            String[] fields = "c0,c1".Split(',');
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 2000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { null, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 3000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 10, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 11));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 4000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 21, 2L });

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(startTime + 5000));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 33, 3L });
        }

        [Test]
        public void Test2TableJoinGrouped()
        {
            String stmtText = "select irstream Symbol, count(*) as cnt " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName + ".win:keepall() " +
                    "where TheString = Symbol group by TheString, Symbol";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            TryUnsupportedIterator(stmt);

            // send event, expect result
            SendEventMD("E1", 1L);
            String[] fields = "Symbol,cnt".Split(',');
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("E1", 10);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 2L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "E1", 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "E1", 0L });
            _listener.Reset();

            SendEvent("E1", 20);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 3L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "E1", 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "E1", 0L });
            _listener.Reset();

            try
            {
                stmt.GetSafeEnumerator();
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex)
            {
                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
            }
            // assure lock given up by sending more events

            SendEvent("E2", 40);
            SendEventMD("E2", 4L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "E2", 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "E2", 0L });
            _listener.Reset();
        }

        [Test]
        public void Test2TableJoinRowForAll()
        {
            String stmtText = "select irstream count(*) as cnt " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName + ".win:keepall() " +
                    "where TheString = Symbol";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            TryUnsupportedIterator(stmt);

            // send event, expect result
            SendEventMD("E1", 1L);
            String[] fields = "cnt".Split(',');
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("E1", 10);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 2L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            _listener.Reset();

            SendEvent("E1", 20);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 3L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            _listener.Reset();

            SendEvent("E2", 40);
            SendEventMD("E2", 4L);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            _listener.Reset();
        }

        [Test]
        public void Test3TableOuterJoinVar1()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    " full outer join " + typeof(SupportBean_S1).FullName + ".win:keepall() as s1" +
                    " on P00 = P10 " +
                    " full outer join " + typeof(SupportBean_S2).FullName + ".win:keepall() as s2" +
                    " on P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableOuterJoin(stmt);
        }

        [Test]
        public void Test3TableOuterJoinVar2()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    " left outer join " + typeof(SupportBean_S1).FullName + ".win:keepall() as s1 " +
                    " on P00 = P10 " +
                    " left outer join " + typeof(SupportBean_S2).FullName + ".win:keepall() as s2 " +
                    " on P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableOuterJoin(stmt);
        }

        [Test]
        public void TestPatternJoin()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

            // no iterator allowed
            String stmtText = "select count(*) as num " +
                    "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                    "SupportBean(IntPrimitive=1).std:unique(TheString) a,\n" +
                    "SupportBean(IntPrimitive=2).std:unique(TheString) b\n" +
                    "where a.TheString = b.TheString";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            SendEvent("A", 1);
            SendEvent("A", 2);
            SendEvent("B", 1);
            SendEvent("B", 2);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(70000));
            Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("num"));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(140000));
            Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("num"));
        }

        [Test]
        public void TestPatternJoinOutputRate()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

            // no iterator allowed
            String stmtText = "select count(*) as num " +
                    "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
                    "SupportBean(IntPrimitive=1).std:unique(TheString) a,\n" +
                    "SupportBean(IntPrimitive=2).std:unique(TheString) b\n" +
                    "where a.TheString = b.TheString output every 2 minutes";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            SendEvent("A", 1);
            SendEvent("A", 2);
            SendEvent("B", 1);
            SendEvent("B", 2);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(70000));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(140000));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(210000));
            Assert.AreEqual(2L, _listener.LastNewData[0].Get("num"));
            Assert.AreEqual(2L, _listener.LastNewData[1].Get("num"));
        }

        private void Try3TableOuterJoin(EPStatement statement)
        {
            statement.Events += _listener.Update;
            String[] fields = "s0.id,s1.id,s2.id".Split(',');

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 1, null, null });
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 10, 20, null });
            _epService.EPRuntime.SendEvent(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S2(300, "E3"));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 100, null, null });
            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S2(31, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E4"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 11, 21, 31 });

            _epService.EPRuntime.SendEvent(new SupportBean_S2(32, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void Test3TableJoinVar1()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S1).FullName + ".win:keepall() as s1, " +
                    typeof(SupportBean_S2).FullName + ".win:keepall() as s2 " +
                    "where P00 = P10 and P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(stmt);
        }

        [Test]
        public void Test3TableJoinVar2A()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S1).FullName + ".win:keepall() as s1, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S2).FullName + ".win:keepall() as s2 " +
                    "where P00 = P10 and P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(stmt);
        }

        [Test]
        public void Test3TableJoinVar2B()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S2).FullName + ".win:keepall() as s2, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional, " +
                    typeof(SupportBean_S1).FullName + ".win:keepall() as s1 " +
                    "where P00 = P10 and P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(stmt);
        }

        [Test]
        public void Test3TableJoinVar3()
        {
            String stmtText = "select s0.id, s1.id, s2.id " +
                    "from " +
                    typeof(SupportBean_S1).FullName + ".win:keepall() as s1, " +
                    typeof(SupportBean_S2).FullName + ".win:keepall() as s2, " +
                    typeof(SupportBean_S0).FullName + " as s0 unidirectional " +
                    "where P00 = P10 and P10 = P20";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Try3TableJoin(stmt);
        }

        private void Try3TableJoin(EPStatement statement)
        {
            statement.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(3, "E1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(30, "E2"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S2(300, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(200, "E3"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S2(31, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(21, "E4"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E4"));
            String[] fields = "s0.id,s1.id,s2.id".Split(',');
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 11, 21, 31 });

            _epService.EPRuntime.SendEvent(new SupportBean_S2(32, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(22, "E4"));
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void Test2TableFullOuterJoin()
        {
            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    ".win:keepall() on TheString = Symbol";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            TryFullOuterPassive2Stream(stmt);
        }

        [Test]
        public void Test2TableFullOuterJoinCompile()
        {
            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    ".win:keepall() on TheString = Symbol";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);

            TryFullOuterPassive2Stream(stmt);
        }

        [Test]
        public void Test2TableFullOuterJoinOM()
        {
            EPStatementObjectModel model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("Symbol", "Volume", "TheString", "IntPrimitive");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).Unidirectional(true));
            model.FromClause.Add(FilterStream.Create(typeof(SupportBean).FullName).AddView("win", "keepall"));
            model.FromClause.Add(OuterJoinQualifier.Create("TheString", OuterJoinType.FULL, "Symbol"));

            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportBean).FullName +
                    ".win:keepall() on TheString = Symbol";
            Assert.AreEqual(stmtText, model.ToEPL());

            EPStatement stmt = _epService.EPAdministrator.Create(model);

            TryFullOuterPassive2Stream(stmt);
        }

        [Test]
        public void Test2TableFullOuterJoinBackwards()
        {
            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportBean).FullName +
                    ".win:keepall() full outer join " +
                    typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "on TheString = Symbol";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            TryFullOuterPassive2Stream(stmt);
        }

        [Test]
        public void Test2TableJoin()
        {
            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportMarketDataBean).FullName + " unidirectional, " +
                    typeof(SupportBean).FullName +
                    ".win:keepall() where TheString = Symbol";

            TryJoinPassive2Stream(stmtText);
        }

        [Test]
        public void Test2TableBackwards()
        {
            String stmtText = "select Symbol, Volume, TheString, IntPrimitive " +
                    "from " + typeof(SupportBean).FullName + ".win:keepall(), " +
                    typeof(SupportMarketDataBean).FullName + " unidirectional " +
                    "where TheString = Symbol";

            TryJoinPassive2Stream(stmtText);
        }

        [Test]
        public void TestInvalid()
        {
            String text = "select * from " + typeof(SupportBean).FullName + " unidirectional " +
                    "full outer join " +
                    typeof(SupportMarketDataBean).FullName + ".win:keepall() unidirectional " +
                    "on TheString = Symbol";
            TryInvalid(text, "Error starting statement: The unidirectional keyword can only apply to one stream in a join [select * from com.espertech.esper.support.bean.SupportBean unidirectional full outer join com.espertech.esper.support.bean.SupportMarketDataBean.win:keepall() unidirectional on TheString = Symbol]");

            text = "select * from " + typeof(SupportBean).FullName + ".win:length(2) unidirectional " +
                    "full outer join " +
                    typeof(SupportMarketDataBean).FullName + ".win:keepall()" +
                    "on TheString = Symbol";
            TryInvalid(text, "Error starting statement: The unidirectional keyword requires that no views are declared onto the stream [select * from com.espertech.esper.support.bean.SupportBean.win:length(2) unidirectional full outer join com.espertech.esper.support.bean.SupportMarketDataBean.win:keepall()on TheString = Symbol]");
        }

        private void TryInvalid(String text, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void TryFullOuterPassive2Stream(EPStatement stmt)
        {
            stmt.Events += _listener.Update;
            TryUnsupportedIterator(stmt);

            // send event, expect result
            SendEventMD("E1", 1L);
            String[] fields = "Symbol,Volume,TheString,IntPrimitive".Split(',');
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 1L, null, null });

            SendEvent("E1", 10);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 2L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 2L, "E1", 10 });

            SendEvent("E1", 20);
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void TryJoinPassive2Stream(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            TryUnsupportedIterator(stmt);

            // send event, expect result
            SendEventMD("E1", 1L);
            String[] fields = "Symbol,Volume,TheString,IntPrimitive".Split(',');
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("E1", 10);
            Assert.IsFalse(_listener.IsInvoked);

            SendEventMD("E1", 2L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 2L, "E1", 10 });

            SendEvent("E1", 20);
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void SendEvent(String s, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEventMD(String symbol, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(bean);
        }

        private void TryUnsupportedIterator(EPStatement stmt)
        {
            try
            {
                stmt.GetEnumerator();
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex)
            {
                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
            }
        }
    }
}
