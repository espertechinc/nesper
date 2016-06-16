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
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerf2StreamRangeJoin
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();

            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestPerfKeyAndRangeOuterJoin()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            _epService.EPAdministrator.CreateEPL("create window SBR.win:keepall() as SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("create window SB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("G", i));
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", i - 1, i + 2));
            }
            Log.Info("Done preloading");

            // create
            String epl = "select * " +
                          "from SB sb " +
                          "full outer join " +
                          "SBR sbr " +
                          "on TheString = key " +
                          "where IntPrimitive between rangeStart and rangeEnd";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // Repeat
            Log.Info("Querying");
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("G", 9990));
                        Assert.AreEqual(4, _listener.GetAndResetLastNewData().Length);

                        _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "G", 4, 10));
                        Assert.AreEqual(7, _listener.GetAndResetLastNewData().Length);
                    }
                    Log.Info("Done Querying");
                });
            Log.Info("delta=" + delta);

            Assert.That(delta, Is.LessThan(500));
            stmt.Dispose();
        }

        [Test]
        public void TestPerfRelationalOp()
        {
            _epService.EPAdministrator.CreateEPL("create window SBR.win:keepall() as SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("create window SB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
                _epService.EPRuntime.SendEvent(new SupportBeanRange("E", i, -1));
            }
            Log.Info("Done preloading");

            // start query
            String epl = "select * from SBR a, SB b where a.rangeStart < b.IntPrimitive";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // Repeat
            Log.Info("Querying");
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean("B", 10));
                        Assert.AreEqual(10, _listener.GetAndResetLastNewData().Length);

                        _epService.EPRuntime.SendEvent(new SupportBeanRange("R", 9990, -1));
                        Assert.AreEqual(9, _listener.GetAndResetLastNewData().Length);
                    }
                    Log.Info("Done Querying");
                });
            Log.Info("delta=" + delta);

            Assert.That(delta, Is.LessThan(500));
            stmt.Dispose();
        }

        [Test]
        public void TestPerfKeyAndRange()
        {
            _epService.EPAdministrator.CreateEPL("create window SBR.win:keepall() as SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("@Name('I1') insert into SBR select * from SupportBeanRange");
            _epService.EPAdministrator.CreateEPL("create window SB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    _epService.EPRuntime.SendEvent(new SupportBean(Convert.ToString(i), j));
                    _epService.EPRuntime.SendEvent(new SupportBeanRange("R", Convert.ToString(i), j - 1, j + 1));
                }
            }
            Log.Info("Done preloading");

            // start query
            String epl = "select * from SBR sbr, SB sb where sbr.key = sb.TheString and sb.IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // repeat
            Log.Info("Querying");
            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("55", 10));
                Assert.AreEqual(3, _listener.GetAndResetLastNewData().Length);

                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "56", 12, 20));
                Assert.AreEqual(9, _listener.GetAndResetLastNewData().Length);
            }
            Log.Info("Done Querying");
            long endTime = PerformanceObserver.MilliTime;
            Log.Info("delta=" + (endTime - startTime));

            // test no event found
            _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "56", 2000, 3000));
            _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "X", 2000, 3000));
            Assert.IsFalse(_listener.IsInvoked);

            Assert.That((endTime - startTime), Is.LessThan(500));
            stmt.Dispose();

            // delete all events
            _epService.EPAdministrator.CreateEPL("on SupportBean delete from SBR");
            _epService.EPAdministrator.CreateEPL("on SupportBean delete from SB");
            _epService.EPRuntime.SendEvent(new SupportBean("D", -1));
        }

        [Test]
        public void TestPerfKeyAndRangeInverted()
        {

            _epService.EPAdministrator.CreateEPL("create window SB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('I2') insert into SB select * from SupportBean");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E", i));
            }
            Log.Info("Done preloading");

            // start query
            String epl = "select * from SupportBeanRange.std:lastevent() sbr, SB sb where sbr.key = sb.TheString and sb.IntPrimitive not in [sbr.rangeStart:sbr.rangeEnd]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // repeat
            Log.Info("Querying");
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "E", 5, 9995));
                        Assert.AreEqual(9, _listener.GetAndResetLastNewData().Length);
                    }
                    Log.Info("Done Querying");
                });
            Log.Info("delta=" + delta);

            Assert.That(delta, Is.LessThan(500));
            stmt.Dispose();
        }

        [Test]
        public void TestPerfUnidirectionalRelOp()
        {

            _epService.EPAdministrator.CreateEPL("create window SB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('I') insert into SB select * from SupportBean");

            // Preload
            Log.Info("Preloading events");
            for (int i = 0; i < 100000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
            }
            Log.Info("Done preloading");

            // Test range
            String rangeEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            String rangeEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                         "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            String rangeEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange.std:lastevent() r, SB a " +
                         "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            String rangeEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange.std:lastevent() r " +
                         "where a.IntPrimitive between r.rangeStart and r.rangeEnd";
            String rangeEplFive = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a\n" +
                         "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= r.rangeEnd";
            String rangeEplSix = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive <= r.rangeEnd and a.IntPrimitive >= r.rangeStart";

            AssertionCallback rangeCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 50000, iteration + 50100),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 50000 + iteration, 50100 + iteration }
            };

            RunAssertion(rangeEplOne, 100, rangeCallback);
            RunAssertion(rangeEplTwo, 100, rangeCallback);
            RunAssertion(rangeEplThree, 100, rangeCallback);
            RunAssertion(rangeEplFour, 100, rangeCallback);
            RunAssertion(rangeEplFive, 100, rangeCallback);
            RunAssertion(rangeEplSix, 100, rangeCallback);

            // Test Greater-Equals
            String geEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= 99200";
            String geEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                         "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive <= 99200";
            AssertionCallback geCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 99000, null),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 99000 + iteration, 99200 }
            };
            RunAssertion(geEplOne, 100, geCallback);
            RunAssertion(geEplTwo, 100, geCallback);

            // Test Greater-Then
            String gtEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            String gtEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            String gtEplThree = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange.std:lastevent() r, SB a " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            String gtEplFour = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange.std:lastevent() r " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= 99200";
            AssertionCallback gtCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 99000, null),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 99001 + iteration, 99200 }
            };
            RunAssertion(gtEplOne, 100, gtCallback);
            RunAssertion(gtEplTwo, 100, gtCallback);
            RunAssertion(gtEplThree, 100, gtCallback);
            RunAssertion(gtEplFour, 100, gtCallback);

            // Test Less-Then
            String ltEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive < r.rangeStart and a.IntPrimitive > 100";
            String ltEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                         "where a.IntPrimitive < r.rangeStart and a.IntPrimitive > 100";
            AssertionCallback ltCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 500, null),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 101, 499 + iteration }
            };
            RunAssertion(ltEplOne, 100, ltCallback);
            RunAssertion(ltEplTwo, 100, ltCallback);

            // Test Less-Equals
            String leEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive <= r.rangeStart and a.IntPrimitive > 100";
            String leEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SB a, SupportBeanRange r unidirectional " +
                         "where a.IntPrimitive <= r.rangeStart and a.IntPrimitive > 100";
            AssertionCallback leCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 500, null),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 101, 500 + iteration }
            };
            RunAssertion(leEplOne, 100, leCallback);
            RunAssertion(leEplTwo, 100, leCallback);

            // Test open range
            String openEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive < r.rangeEnd";
            String openEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive in (r.rangeStart:r.rangeEnd)";
            AssertionCallback openCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 3, iteration + 7),
                GetExpectedValueFunc = iteration =>
                    new Object[] { iteration + 4, iteration + 6 }
            };
            RunAssertion(openEplOne, 100, openCallback);
            RunAssertion(openEplTwo, 100, openCallback);

            // Test half-open range
            String hopenEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive >= r.rangeStart and a.IntPrimitive < r.rangeEnd";
            String hopenEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive in [r.rangeStart:r.rangeEnd)";
            AssertionCallback halfOpenCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 3, iteration + 7),
                GetExpectedValueFunc = iteration =>
                    new Object[] { iteration + 3, iteration + 6 }
            };
            RunAssertion(hopenEplOne, 100, halfOpenCallback);
            RunAssertion(hopenEplTwo, 100, halfOpenCallback);

            // Test half-closed range
            String hclosedEplOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive > r.rangeStart and a.IntPrimitive <= r.rangeEnd";
            String hclosedEplTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive in (r.rangeStart:r.rangeEnd]";
            AssertionCallback halfClosedCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", iteration + 3, iteration + 7),
                GetExpectedValueFunc = iteration =>
                    new Object[] { iteration + 4, iteration + 7 }
            };
            RunAssertion(hclosedEplOne, 100, halfClosedCallback);
            RunAssertion(hclosedEplTwo, 100, halfClosedCallback);

            // Test inverted closed range
            String invertedClosedEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive not in [r.rangeStart:r.rangeEnd]";
            String invertedClosedEPLTwo = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive not between r.rangeStart and r.rangeEnd";
            AssertionCallback invertedClosedCallback = new ProxyAssertionCallback
            {
                GetEventFunc = iteration =>
                    new SupportBeanRange("E", 20, 99990),
                GetExpectedValueFunc = iteration =>
                    new Object[] { 0, 99999 }
            };
            RunAssertion(invertedClosedEPLOne, 100, invertedClosedCallback);
            RunAssertion(invertedClosedEPLTwo, 100, invertedClosedCallback);

            // Test inverted open range
            String invertedOpenEPLOne = "select min(a.IntPrimitive) as mini, max(a.IntPrimitive) as maxi from SupportBeanRange r unidirectional, SB a " +
                         "where a.IntPrimitive not in (r.rangeStart:r.rangeEnd)";
            RunAssertion(invertedOpenEPLOne, 100, invertedClosedCallback);
        }

        public void RunAssertion(String epl, int numLoops, AssertionCallback assertionCallback)
        {
            String[] fields = "mini,maxi".Split(',');

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // Send range query events
            Log.Info("Querying");
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < numLoops; i++)
                    {
                        //if (i % 10 == 0) {
                        //    log.Info("At loop #" + i);
                        //}
                        _epService.EPRuntime.SendEvent(assertionCallback.GetEvent(i));
                        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                                    assertionCallback.GetExpectedValue(i));
                    }

                    Log.Info("Done Querying");
                });

            Log.Info("delta=" + delta);

            Assert.That(delta, Is.LessThan(1500));
            stmt.Dispose();
        }

        public interface AssertionCallback
        {
            Object GetEvent(int iteration);
            Object[] GetExpectedValue(int iteration);
        }

        public class ProxyAssertionCallback : AssertionCallback
        {
            public Func<int, object> GetEventFunc;
            public Func<int, object[]> GetExpectedValueFunc;

            public object GetEvent(int iteration)
            {
                return GetEventFunc(iteration);
            }

            public object[] GetExpectedValue(int iteration)
            {
                return GetExpectedValueFunc(iteration);
            }
        }
    }
}
