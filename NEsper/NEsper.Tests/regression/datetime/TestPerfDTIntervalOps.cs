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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestPerfDTIntervalOps
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
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestPerf()
        {
            var config = new ConfigurationEventTypeLegacy();
            config.StartTimestampPropertyName = "MsecdateStart";
            config.EndTimestampPropertyName = "MsecdateEnd";
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).FullName, config);
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).FullName, config);

            _epService.EPAdministrator.CreateEPL("create window AWindow.win:keepall() as A");
            _epService.EPAdministrator.CreateEPL("insert into AWindow select * from A");

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A" + i, "2002-05-30 9:00:00.000", 100));
            }
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("AEarlier", "2002-05-30 8:00:00.000", 100));
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("ALater", "2002-05-30 10:00:00.000", 100));

            // assert BEFORE
            String eplBefore = "select a.Key as c0 from AWindow as a, B b unidirectional where a.before(b)";
            RunAssertion(eplBefore, "2002-05-30 9:00:00.000", 0, "AEarlier");

            String eplBeforeMSec = "select a.Key as c0 from AWindow as a, B b unidirectional where a.MsecdateEnd.before(b.MsecdateStart)";
            RunAssertion(eplBeforeMSec, "2002-05-30 9:00:00.000", 0, "AEarlier");

            String eplBeforeMSecMix1 = "select a.Key as c0 from AWindow as a, B b unidirectional where a.MsecdateEnd.before(b)";
            RunAssertion(eplBeforeMSecMix1, "2002-05-30 9:00:00.000", 0, "AEarlier");

            String eplBeforeMSecMix2 = "select a.Key as c0 from AWindow as a, B b unidirectional where a.before(b.MsecdateStart)";
            RunAssertion(eplBeforeMSecMix2, "2002-05-30 9:00:00.000", 0, "AEarlier");

            // assert AFTER
            String eplAfter = "select a.Key as c0 from AWindow as a, B b unidirectional where a.after(b)";
            RunAssertion(eplAfter, "2002-05-30 9:00:00.000", 0, "ALater");

            // assert COINCIDES
            String eplCoincides = "select a.Key as c0 from AWindow as a, B b unidirectional where a.coincides(b)";
            RunAssertion(eplCoincides, "2002-05-30 8:00:00.000", 100, "AEarlier");

            // assert DURING
            String eplDuring = "select a.Key as c0 from AWindow as a, B b unidirectional where a.during(b)";
            RunAssertion(eplDuring, "2002-05-30 7:59:59.000", 2000, "AEarlier");

            // assert FINISHES
            String eplFinishes = "select a.Key as c0 from AWindow as a, B b unidirectional where a.finishes(b)";
            RunAssertion(eplFinishes, "2002-05-30 7:59:59.950", 150, "AEarlier");

            // assert FINISHED-BY
            String eplFinishedBy = "select a.Key as c0 from AWindow as a, B b unidirectional where a.finishedBy(b)";
            RunAssertion(eplFinishedBy, "2002-05-30 8:00:00.050", 50, "AEarlier");

            // assert INCLUDES
            String eplIncludes = "select a.Key as c0 from AWindow as a, B b unidirectional where a.includes(b)";
            RunAssertion(eplIncludes, "2002-05-30 8:00:00.050", 20, "AEarlier");

            // assert MEETS
            String eplMeets = "select a.Key as c0 from AWindow as a, B b unidirectional where a.meets(b)";
            RunAssertion(eplMeets, "2002-05-30 8:00:00.100", 0, "AEarlier");

            // assert METBY
            String eplMetBy = "select a.Key as c0 from AWindow as a, B b unidirectional where a.metBy(b)";
            RunAssertion(eplMetBy, "2002-05-30 7:59:59.950", 50, "AEarlier");

            // assert OVERLAPS
            String eplOverlaps = "select a.Key as c0 from AWindow as a, B b unidirectional where a.overlaps(b)";
            RunAssertion(eplOverlaps, "2002-05-30 8:00:00.050", 100, "AEarlier");

            // assert OVERLAPPEDY
            String eplOverlappedBy = "select a.Key as c0 from AWindow as a, B b unidirectional where a.overlappedBy(b)";
            RunAssertion(eplOverlappedBy, "2002-05-30 9:59:59.950", 100, "ALater");
            RunAssertion(eplOverlappedBy, "2002-05-30 7:59:59.950", 100, "AEarlier");

            // assert STARTS
            String eplStarts = "select a.Key as c0 from AWindow as a, B b unidirectional where a.starts(b)";
            RunAssertion(eplStarts, "2002-05-30 8:00:00.000", 150, "AEarlier");

            // assert STARTEDBY
            String eplEnds = "select a.Key as c0 from AWindow as a, B b unidirectional where a.startedBy(b)";
            RunAssertion(eplEnds, "2002-05-30 8:00:00.000", 50, "AEarlier");
        }

        private void RunAssertion(String epl, String timestampB, long durationB, String expectedAKey)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            // query
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B", timestampB, durationB));
                        Assert.AreEqual(expectedAKey, _listener.AssertOneGetNewAndReset().Get("c0"));
                    }
                });

            Assert.That(delta, Is.LessThan(500));

            stmt.Dispose();
        }
    }
}
