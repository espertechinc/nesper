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
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraOnMergePerf : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
    
            RunAssertionPerformance(epService, true, EventRepresentationChoice.ARRAY);
            RunAssertionPerformance(epService, true, EventRepresentationChoice.MAP);
            RunAssertionPerformance(epService, true, EventRepresentationChoice.DEFAULT);
            RunAssertionPerformance(epService, false, EventRepresentationChoice.ARRAY);
        }
    
        private void RunAssertionPerformance(EPServiceProvider epService, bool namedWindow, EventRepresentationChoice outputType) {
    
            string eplCreate = namedWindow ?
                    outputType.GetAnnotationText() + " create window MyWindow#keepall as (c1 string, c2 int)" :
                    "create table MyWindow(c1 string primary key, c2 int)";
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL(eplCreate);
            Assert.IsTrue(outputType.MatchesClass(stmtNamedWindow.EventType.UnderlyingType));
    
            // preload events
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into MyWindow select TheString as c1, IntPrimitive as c2 from SupportBean");
            int totalUpdated = 5000;
            for (int i = 0; i < totalUpdated; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, 0));
            }
            stmt.Dispose();
    
            string epl = "on SupportBean sb merge MyWindow nw where nw.c1 = sb.TheString " +
                    "when matched then update set nw.c2=sb.IntPrimitive";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
    
            // prime
            for (int i = 0; i < 100; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, 1));
            }
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < totalUpdated; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("E" + i, 1));
            }
            long endTime = DateTimeHelper.CurrentTimeMillis;
            long delta = endTime - startTime;
    
            // verify
            IEnumerator<EventBean> events = stmtNamedWindow.GetEnumerator();
            int count = 0;
            for (; events.MoveNext(); ) {
                EventBean next = events.Current;
                Assert.AreEqual(1, next.Get("c2"));
                count++;
            }
            Assert.AreEqual(totalUpdated, count);
            Assert.IsTrue(delta < 500, "Delta=" + delta);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindow", true);
        }
    }
} // end of namespace
