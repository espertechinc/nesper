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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTPerfIntervalOps : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            var config = new ConfigurationEventTypeLegacy();
            config.StartTimestampPropertyName = "longdateStart";
            config.EndTimestampPropertyName = "longdateEnd";
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportTimeStartEndA).Name, config);
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportTimeStartEndB).Name, config);
    
            epService.EPAdministrator.CreateEPL("create window AWindow#keepall as A");
            epService.EPAdministrator.CreateEPL("insert into AWindow select * from A");
    
            // preload
            for (int i = 0; i < 10000; i++) {
                epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("A" + i, "2002-05-30T09:00:00.000", 100));
            }
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("AEarlier", "2002-05-30T08:00:00.000", 100));
            epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("ALater", "2002-05-30T10:00:00.000", 100));
    
            // assert BEFORE
            string eplBefore = "select a.key as c0 from AWindow as a, B b unidirectional where A.Before(b)";
            RunAssertion(epService, eplBefore, "2002-05-30T09:00:00.000", 0, "AEarlier");
    
            string eplBeforeMSec = "select a.key as c0 from AWindow as a, B b unidirectional where A.longdateEnd.Before(b.longdateStart)";
            RunAssertion(epService, eplBeforeMSec, "2002-05-30T09:00:00.000", 0, "AEarlier");
    
            string eplBeforeMSecMix1 = "select a.key as c0 from AWindow as a, B b unidirectional where A.longdateEnd.Before(b)";
            RunAssertion(epService, eplBeforeMSecMix1, "2002-05-30T09:00:00.000", 0, "AEarlier");
    
            string eplBeforeMSecMix2 = "select a.key as c0 from AWindow as a, B b unidirectional where A.Before(b.longdateStart)";
            RunAssertion(epService, eplBeforeMSecMix2, "2002-05-30T09:00:00.000", 0, "AEarlier");
    
            // assert AFTER
            string eplAfter = "select a.key as c0 from AWindow as a, B b unidirectional where A.After(b)";
            RunAssertion(epService, eplAfter, "2002-05-30T09:00:00.000", 0, "ALater");
    
            // assert COINCIDES
            string eplCoincides = "select a.key as c0 from AWindow as a, B b unidirectional where A.Coincides(b)";
            RunAssertion(epService, eplCoincides, "2002-05-30T08:00:00.000", 100, "AEarlier");
    
            // assert DURING
            string eplDuring = "select a.key as c0 from AWindow as a, B b unidirectional where A.During(b)";
            RunAssertion(epService, eplDuring, "2002-05-30T07:59:59.000", 2000, "AEarlier");
    
            // assert FINISHES
            string eplFinishes = "select a.key as c0 from AWindow as a, B b unidirectional where A.Finishes(b)";
            RunAssertion(epService, eplFinishes, "2002-05-30T07:59:59.950", 150, "AEarlier");
    
            // assert FINISHED-BY
            string eplFinishedBy = "select a.key as c0 from AWindow as a, B b unidirectional where A.FinishedBy(b)";
            RunAssertion(epService, eplFinishedBy, "2002-05-30T08:00:00.050", 50, "AEarlier");
    
            // assert INCLUDES
            string eplIncludes = "select a.key as c0 from AWindow as a, B b unidirectional where A.Includes(b)";
            RunAssertion(epService, eplIncludes, "2002-05-30T08:00:00.050", 20, "AEarlier");
    
            // assert MEETS
            string eplMeets = "select a.key as c0 from AWindow as a, B b unidirectional where A.Meets(b)";
            RunAssertion(epService, eplMeets, "2002-05-30T08:00:00.100", 0, "AEarlier");
    
            // assert METBY
            string eplMetBy = "select a.key as c0 from AWindow as a, B b unidirectional where A.MetBy(b)";
            RunAssertion(epService, eplMetBy, "2002-05-30T07:59:59.950", 50, "AEarlier");
    
            // assert OVERLAPS
            string eplOverlaps = "select a.key as c0 from AWindow as a, B b unidirectional where A.Overlaps(b)";
            RunAssertion(epService, eplOverlaps, "2002-05-30T08:00:00.050", 100, "AEarlier");
    
            // assert OVERLAPPEDY
            string eplOverlappedBy = "select a.key as c0 from AWindow as a, B b unidirectional where A.OverlappedBy(b)";
            RunAssertion(epService, eplOverlappedBy, "2002-05-30T09:59:59.950", 100, "ALater");
            RunAssertion(epService, eplOverlappedBy, "2002-05-30T07:59:59.950", 100, "AEarlier");
    
            // assert STARTS
            string eplStarts = "select a.key as c0 from AWindow as a, B b unidirectional where A.Starts(b)";
            RunAssertion(epService, eplStarts, "2002-05-30T08:00:00.000", 150, "AEarlier");
    
            // assert STARTEDBY
            string eplEnds = "select a.key as c0 from AWindow as a, B b unidirectional where A.StartedBy(b)";
            RunAssertion(epService, eplEnds, "2002-05-30T08:00:00.000", 50, "AEarlier");
        }
    
        private void RunAssertion(EPServiceProvider epService, string epl, string timestampB, long durationB, string expectedAKey) {
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // query
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(SupportTimeStartEndB.Make("B", timestampB, durationB));
                Assert.AreEqual(expectedAKey, listener.AssertOneGetNewAndReset().Get("c0"));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "Delta=" + delta / 1000d);
    
            stmt.Dispose();
        }
    }
} // end of namespace
