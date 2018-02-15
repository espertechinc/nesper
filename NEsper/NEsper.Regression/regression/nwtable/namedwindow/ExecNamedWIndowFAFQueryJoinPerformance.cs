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

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWIndowFAFQueryJoinPerformance : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = false;
            configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            configuration.AddEventType("SSB2", typeof(SupportSimpleBeanTwo));
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window W1#unique(s1) as SSB1");
            epService.EPAdministrator.CreateEPL("insert into W1 select * from SSB1");
    
            epService.EPAdministrator.CreateEPL("create window W2#unique(s2) as SSB2");
            epService.EPAdministrator.CreateEPL("insert into W2 select * from SSB2");
    
            for (int i = 0; i < 1000; i++) {
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("A" + i, 0, 0, 0));
                epService.EPRuntime.SendEvent(new SupportSimpleBeanTwo("A" + i, 0, 0, 0));
            }
    
            long start = PerformanceObserver.MilliTime;
            for (int i = 0; i < 100; i++) {
                EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select * from W1 as w1, W2 as w2 " +
                        "where w1.s1 = w2.s2");
                Assert.AreEqual(1000, result.Array.Length);
            }
            long end = PerformanceObserver.MilliTime;
            long delta = end - start;
            Log.Info("Delta=" + delta);
            Assert.IsTrue(delta < 1000, "Delta=" + delta);
        }
    }
} // end of namespace
