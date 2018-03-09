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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectCorrelatedAggregationPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("S0", typeof(SupportBean_S0));
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtText = "select p00, " +
                    "(select sum(IntPrimitive) from SupportBean#keepall where TheString = s0.p00) as sump00 " +
                    "from S0 as s0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "p00,sump00".Split(',');
    
            // preload
            int max = 50000;
            for (int i = 0; i < max; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("T" + i, -i));
                epService.EPRuntime.SendEvent(new SupportBean("T" + i, 10));
            }
    
            // excercise
            long start = PerformanceObserver.MilliTime;
            var random = new Random();
            for (int i = 0; i < 10000; i++) {
                int index = random.Next(max);
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, "T" + index));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"T" + index, -index + 10});
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
    
            //Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 500, "delta=" + delta);
        }
    }
} // end of namespace
