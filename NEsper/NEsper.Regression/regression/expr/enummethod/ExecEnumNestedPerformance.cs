///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lrreport;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.enummethod
{
    public class ExecEnumNestedPerformance : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddImport(typeof(LocationReportFactory));
            configuration.AddEventType("Bean", typeof(SupportBean_ST0_Container));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            var list = new List<SupportBean_ST0>();
            for (int i = 0; i < 10000; i++) {
                list.Add(new SupportBean_ST0("E1", 1000));
            }
            var minEvent = new SupportBean_ST0("E2", 5);
            list.Add(minEvent);
            var theEvent = new SupportBean_ST0_Container(list);
    
            // the "Contained.min" inner lambda only depends on values within "contained" (a stream's value)
            // and not on the particular "x".
            string eplFragment = "select Contained.where(x => x.p00 = Contained.min(y => y.p00)) as val from Bean";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.Events += listener.Update;
    
            long start = PerformanceObserver.MilliTime;
            epService.EPRuntime.SendEvent(theEvent);
            long delta = PerformanceObserver.MilliTime - start;
            Assert.IsTrue(delta < 100, "delta=" + delta);

            var result = listener.AssertOneGetNewAndReset().Get("val")
                .UnwrapIntoArray<SupportBean_ST0>();
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {minEvent}, result);
        }
    }
} // end of namespace
