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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfHistoricalMethodJoin
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            config.AddEventType(typeof(SupportBeanInt));

            ConfigurationMethodRef configMethod = new ConfigurationMethodRef();
            configMethod.SetLRUCache(10);
            config.AddMethodRef(typeof(SupportJoinMethods).FullName, configMethod);

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void Test1Stream2HistInnerJoinPerformance()
        {
            const string expression =
                "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                "from SupportBeanInt#lastevent as s0, " +
                "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                "where h0.index = p00 and h1.index = p00";

            var stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            var fields = "id,valh0,valh1".Split(',');
            var random = new Random();

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 1; i < 5000; i++)
                    {
                        var num = random.Next(98) + 1;
                        SendBeanInt("E1", num);
                        var result = new Object[][] { new Object[] { "E1", "H0" + num, "H1" + num } };
                        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
                    }
                });

            stmt.Dispose();
            _listener.Reset();
            Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
        }

        [Test]
        public void Test1Stream2HistOuterJoinPerformance()
        {
            String expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                    "from SupportBeanInt#lastevent as s0 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0 " +
                    " on h0.index = p00 " +
                    " left outer join " +
                    "method:SupportJoinMethods.FetchVal('H1', 100) as h1 " +
                    " on h1.index = p00";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            String[] fields = "id,valh0,valh1".Split(',');
            Random random = new Random();

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 1; i < 5000; i++)
                    {
                        int num = random.Next(98) + 1;
                        SendBeanInt("E1", num);

                        Object[][] result = new Object[][] { new Object[] { "E1", "H0" + num, "H1" + num } };
                        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);
                    }
                });

            stmt.Dispose();
            Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
        }

        [Test]
        public void Test2Stream1HistTwoSidedEntryIdenticalIndex()
        {
            String expression;

            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0 " +
                    "from SupportBeanInt(id like 'E%')#lastevent as s0, " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(id like 'F%')#lastevent as s1 " +
                    "where h0.index = s0.P00 and h0.index = s1.P00";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            String[] fields = "s0id,s1id,valh0".Split(',');
            Random random = new Random();

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 1; i < 1000; i++)
                    {
                        int num = random.Next(98) + 1;
                        SendBeanInt("E1", num);
                        SendBeanInt("F1", num);

                        Object[][] result = new Object[][] { new Object[] { "E1", "F1", "H0" + num } };
                        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);

                        // send reset events to avoid duplicate matches
                        SendBeanInt("E1", 0);
                        SendBeanInt("F1", 0);
                        _listener.Reset();
                    }
                });

            stmt.Dispose();
            Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
        }

        [Test]
        public void Test2Stream1HistTwoSidedEntryMixedIndex()
        {
            String expression;

            expression = "select s0.id as s0id, s1.id as s1id, h0.val as valh0, h0.index as indexh0 from " +
                    "method:SupportJoinMethods.FetchVal('H0', 100) as h0, " +
                    "SupportBeanInt(id like 'H%')#lastevent as s1, " +
                    "SupportBeanInt(id like 'E%')#lastevent as s0 " +
                    "where h0.index = s0.P00 and h0.val = s1.id";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            String[] fields = "s0id,s1id,valh0,indexh0".Split(',');
            Random random = new Random();

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 1; i < 1000; i++)
                    {
                        int num = random.Next(98) + 1;
                        SendBeanInt("E1", num);
                        SendBeanInt("H0" + num, num);

                        Object[][] result = new Object[][] { new Object[] { "E1", "H0" + num, "H0" + num, num } };
                        EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, result);

                        // send reset events to avoid duplicate matches
                        SendBeanInt("E1", 0);
                        SendBeanInt("F1", 0);
                        _listener.Reset();
                    }
                });

            stmt.Dispose();
            Assert.That(delta, Is.LessThan(1000), "Delta to large, at " + delta + " msec");
        }

        private void SendBeanInt(String id, int p00, int p01, int p02, int p03)
        {
            _epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, -1, -1));
        }

        private void SendBeanInt(String id, int p00)
        {
            SendBeanInt(id, p00, -1, -1, -1);
        }
    }
}
