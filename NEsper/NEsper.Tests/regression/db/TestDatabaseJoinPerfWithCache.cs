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
using com.espertech.esper.compat;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.epl.@join.util;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseJoinPerfWithCache : IndexBackingTableInfo
    {
        private EPServiceProvider _epServiceRetained;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            ConfigurationDBRef configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);

            // Turn this cache setting off to turn off indexing since without cache there is no point in indexing.
            configDB.LRUCache = 100000;
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            configuration.AddDatabaseReference("MyDB", configDB);

            _epServiceRetained = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epServiceRetained.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _epServiceRetained.Dispose();
        }

        [Test]
        public void TestConstants()
        {
            _epServiceRetained.EPAdministrator.Configuration.AddEventType<SupportBean>();

            String epl;

            epl = "select * from SupportBean sbr, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 951";
            RunAssertion(epl, "s1.mycol1", "951");

            epl = "select * from SupportBean sbr, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 = 950 and mycol1 = '950'";
            RunAssertion(epl, "s1.mycol1", "950");

            epl = "select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between 950 and 953";
            RunAssertion(epl, "val", 950 + 951 + 952 + 953);

            epl = "select sum(s1.mycol3) as val from SupportBean sbr unidirectional, sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = '950' and mycol3 between 950 and 953";
            RunAssertion(epl, "val", 950);
        }

        private void RunAssertion(String epl, String field, Object expected)
        {
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(epl);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epServiceRetained.EPRuntime.SendEvent(new SupportBean("E", 0));
                Assert.AreEqual(expected, _listener.AssertOneGetNewAndReset().Get(field));
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);
        }

        [Test]
        public void TestRangeIndex()
        {
            _epServiceRetained.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            String stmtText = "select * from SupportBeanRange sbr, " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStart and rangeEnd";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epServiceRetained.EPRuntime.SendEvent(new SupportBeanRange("R", 10, 12));
                Assert.AreEqual(3, _listener.GetAndResetLastNewData().Length);
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            // test coercion
            statement.Dispose();
            stmtText = "select * from SupportBeanRange sbr, " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol3 between rangeStartLong and rangeEndLong";

            statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            _epServiceRetained.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "K", 10L, 12L));
            Assert.AreEqual(3, _listener.GetAndResetLastNewData().Length);
        }

        [Test]
        public void TestKeyAndRangeIndex()
        {
            _epServiceRetained.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            String stmtText = "select * from SupportBeanRange sbr, " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStart and rangeEnd";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epServiceRetained.EPRuntime.SendEvent(new SupportBeanRange("R", "11", 10, 12));
                Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "Delta=" + delta);

            // test coercion
            statement.Dispose();
            stmtText = "select * from SupportBeanRange sbr, " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 where mycol1 = key and mycol3 between rangeStartLong and rangeEndLong";

            statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            _epServiceRetained.EPRuntime.SendEvent(SupportBeanRange.MakeLong("R", "11", 10L, 12L));
            Assert.AreEqual(1, _listener.GetAndResetLastNewData().Length);
        }

        /// <summary>Test for selecting from a table a large result set and then joining the result outside of the cache. Verifies performance of indexes cached for resolving join criteria fast. </summary>
        [Test]
        public void TestSelectLargeResultSet()
        {
            String stmtText = "select id, mycol3, mycol2 from " +
                    typeof(SupportBean_S0).FullName + ".win:keepall() as s0," +
                    " sql:MyDB ['select mycol3, mycol2 from mytesttable_large'] as s1 where s0.id = s1.mycol3";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Send 100 events which all perform the join
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 200; i++)
            {
                var num = i + 1;
                var col2 = Convert.ToString(Math.Round((float)num / 10, MidpointRounding.AwayFromZero));
                var bean = new SupportBean_S0(num);
                _epServiceRetained.EPRuntime.SendEvent(bean);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new String[] { "id", "mycol3", "mycol2" }, new Object[] { num, num, col2 });
            }
            long endTime = PerformanceObserver.MilliTime;

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestSelectLargeResultSetCoercion()
        {
            String stmtText = "select TheString, mycol3, mycol4 from " +
                    " sql:MyDB ['select mycol3, mycol4 from mytesttable_large'] as s0, " +
                    typeof(SupportBean).FullName + ".win:keepall() as s1 where s1.DoubleBoxed = s0.mycol3 and s1.ByteBoxed = s0.mycol4";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Send events which all perform the join
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 200; i++)
            {
                SupportBean bean = new SupportBean { DoubleBoxed = 100d, ByteBoxed = (byte)10, TheString = "E" + i };
                _epServiceRetained.EPRuntime.SendEvent(bean);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new[] { "TheString", "mycol3", "mycol4" }, new Object[] { "E" + i, 100, 10 });
            }
            long endTime = PerformanceObserver.MilliTime;

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);
        }

        [Test]
        public void Test2StreamOuterJoin()
        {
            String stmtText = "select TheString, mycol3, mycol1 from " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on TheString = mycol1";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Send events which all perform the join
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 200; i++)
            {
                SupportBean supportBean = new SupportBean { TheString = "50" };
                _epServiceRetained.EPRuntime.SendEvent(supportBean);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new String[] { "TheString", "mycol3", "mycol1" }, new Object[] { "50", 50, "50" });
            }
            long endTime = PerformanceObserver.MilliTime;

            // no matching
            SupportBean bean = new SupportBean { TheString = "-1" };
            _epServiceRetained.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new String[] { "TheString", "mycol3", "mycol1" }, new Object[] { "-1", null, null });

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);
        }

        [Test]
        public void TestOuterJoinPlusWhere()
        {
            String stmtText = "select TheString, mycol3, mycol1 from " +
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on TheString = mycol1 where s1.mycol3 = s0.IntPrimitive";

            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Send events which all perform the join
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 200; i++)
            {
                SupportBean supportBean = new SupportBean();
                supportBean.TheString = "50";
                supportBean.IntPrimitive = 50;
                _epServiceRetained.EPRuntime.SendEvent(supportBean);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), new[] { "TheString", "mycol3", "mycol1" }, new Object[] { "50", 50, "50" });
            }
            long endTime = PerformanceObserver.MilliTime;

            // no matching on-clause
            SupportBean bean = new SupportBean();
            Assert.IsFalse(_listener.IsInvoked);

            // matching on-clause not matching where
            bean = new SupportBean();
            bean.TheString = "50";
            bean.IntPrimitive = 49;
            _epServiceRetained.EPRuntime.SendEvent(bean);
            Assert.IsFalse(_listener.IsInvoked);

            Log.Info("delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < 500);
        }

        [Test]
        public void TestInKeywordSingleIndex()
        {
            _epServiceRetained.EPAdministrator.Configuration.AddEventType<SupportBean_S0>("S0");

            String stmtText = INDEX_CALLBACK_HOOK + "select * from S0 s0, "+
                    " sql:MyDB ['select mycol1, mycol3 from mytesttable_large'] as s1 " +
                    " where mycol1 in (p00, p01, p02)";
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            QueryPlanIndexDescHistorical historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
            Assert.That(historical.IndexName, Is.EqualTo(typeof (PollResultIndexingStrategyIndexSingle).Name));
            Assert.That(historical.StrategyName, Is.EqualTo(typeof (HistoricalIndexLookupStrategyInKeywordSingle).Name));

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 2000; i++) {
                        _epServiceRetained.EPRuntime.SendEvent(new SupportBean_S0(i, "x", "y", "815"));
                        Assert.AreEqual(815, _listener.AssertOneGetNewAndReset().Get("s1.mycol3"));
                    }
                });

            Log.Info("delta={0}", delta);
            Assert.That(delta, Is.LessThan(500));
        }

        [Test]
        public void TestInKeywordMultiIndex()
        {
            _epServiceRetained.EPAdministrator.Configuration.AddEventType<SupportBean_S0>("S0");

            String stmtText = INDEX_CALLBACK_HOOK + "select * from S0 s0, "+
                    " sql:MyDB ['select mycol1, mycol2, mycol3 from mytesttable_large'] as s1 " +
                    " where p00 in (mycol2, mycol1)";
            EPStatement statement = _epServiceRetained.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            QueryPlanIndexDescHistorical historical = SupportQueryPlanIndexHook.AssertHistoricalAndReset();
            Assert.That(historical.IndexName, Is.EqualTo(typeof (PollResultIndexingStrategyIndexSingleArray).Name));
            Assert.That(historical.StrategyName, Is.EqualTo(typeof (HistoricalIndexLookupStrategyInKeywordMulti).Name));

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 2000; i++)
                    {
                        _epServiceRetained.EPRuntime.SendEvent(new SupportBean_S0(i, "815"));
                        Assert.AreEqual(815, _listener.AssertOneGetNewAndReset().Get("s1.mycol3"));
                    }
                });

            Log.Info("delta=" + delta);
            Assert.That(delta, Is.LessThan(500));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
