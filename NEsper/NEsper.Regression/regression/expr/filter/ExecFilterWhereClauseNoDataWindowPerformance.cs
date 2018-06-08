///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.filter
{
    public class ExecFilterWhereClauseNoDataWindowPerformance : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MD", typeof(SupportMarketDataIDBean));
        }
    
        // Compares the performance of
        //     select * from MD(symbol = 'xyz')
        //  against
        //     select * from MD where symbol = 'xyz'
        public override void Run(EPServiceProvider epService) {
            for (int i = 0; i < 1000; i++) {
                string text = "select * from MD where symbol = '" + Convert.ToString(i) + "'";
                epService.EPAdministrator.CreateEPL(text);
            }
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 10000; i++) {
                var bean = new SupportMarketDataIDBean("NOMATCH", "", 1);
                epService.EPRuntime.SendEvent(bean);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Assert.IsTrue(delta < 500, "Delta=" + delta);
        }
    }
} // end of namespace
