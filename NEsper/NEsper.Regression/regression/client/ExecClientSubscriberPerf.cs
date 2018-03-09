///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientSubscriberPerf : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventTypeAutoName(typeof(SupportBean).Namespace);
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionPerformanceSyntheticUndelivered(epService);
            RunAssertionPerformanceSynthetic(epService);
        }

        private void RunAssertionPerformanceSyntheticUndelivered(EPServiceProvider epService)
        {
            var numLoop = 100000;
            epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < numLoop; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1000 + i));
            }

            var end = PerformanceObserver.MilliTime;

            Assert.IsTrue(end - start < 1000, "delta=" + (end - start));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionPerformanceSynthetic(EPServiceProvider epService)
        {
            var numLoop = 100000;
            var stmt = epService.EPAdministrator.CreateEPL(
                "select TheString, IntPrimitive from SupportBean(IntPrimitive > 10)");
            var results = new List<object[]>();

            var listener = new UpdateEventHandler(
                (sender, args) =>
                {
                    var newEvents = args.NewEvents;
                    var theString = (string) newEvents[0].Get("TheString");
                    var val = newEvents[0].Get("IntPrimitive").AsInt();
                    results.Add(new object[] {theString, val});
                });
            stmt.Events += listener;

            var start = PerformanceObserver.MilliTime;
            for (var i = 0; i < numLoop; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 1000 + i));
            }

            var end = PerformanceObserver.MilliTime;

            Assert.AreEqual(numLoop, results.Count);
            for (var i = 0; i < numLoop; i++)
            {
                EPAssertionUtil.AssertEqualsAnyOrder(results[i], new object[] {"E1", 1000 + i});
            }

            Assert.IsTrue(end - start < 1000, "delta=" + (end - start));

            stmt.Dispose();
        }
    }
} // end of namespace