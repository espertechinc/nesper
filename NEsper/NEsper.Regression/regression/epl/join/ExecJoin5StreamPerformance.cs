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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoin5StreamPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string statement = "select * from " +
                    typeof(SupportBean_S0).FullName + "#length(100000) as s0," +
                    typeof(SupportBean_S1).FullName + "#length(100000) as s1," +
                    typeof(SupportBean_S2).FullName + "#length(100000) as s2," +
                    typeof(SupportBean_S3).FullName + "#length(100000) as s3," +
                    typeof(SupportBean_S4).FullName + "#length(100000) as s4" +
                    " where s0.p00 = s1.p10 " +
                    "and s1.p10 = s2.p20 " +
                    "and s2.p20 = s3.p30 " +
                    "and s3.p30 = s4.p40 ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statement);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            Log.Info(".testPerfAllProps Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                SendEvents(epService, new int[]{0, 0, 0, 0, 0}, new string[]{"s0" + i, "s1" + i, "s2" + i, "s3" + i, "s4" + i});
            }
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(".testPerfAllProps delta=" + (endTime - startTime));
            Assert.IsTrue((endTime - startTime) < 1500);
    
            // test if join returns data
            Assert.IsNull(updateListener.LastNewData);
            var propertyValues = new string[]{"x", "x", "x", "x", "x"};
            var ids = new int[]{1, 2, 3, 4, 5};
            SendEvents(epService, ids, propertyValues);
            AssertEventsReceived(updateListener, ids);
        }
    
        private void AssertEventsReceived(SupportUpdateListener updateListener, int[] expectedIds) {
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);
            EventBean theEvent = updateListener.LastNewData[0];
            Assert.AreEqual(expectedIds[0], ((SupportBean_S0) theEvent.Get("s0")).Id);
            Assert.AreEqual(expectedIds[1], ((SupportBean_S1) theEvent.Get("s1")).Id);
            Assert.AreEqual(expectedIds[2], ((SupportBean_S2) theEvent.Get("s2")).Id);
            Assert.AreEqual(expectedIds[3], ((SupportBean_S3) theEvent.Get("s3")).Id);
            Assert.AreEqual(expectedIds[4], ((SupportBean_S4) theEvent.Get("s4")).Id);
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvents(EPServiceProvider epService, int[] ids, string[] propertyValues) {
            SendEvent(epService, new SupportBean_S0(ids[0], propertyValues[0]));
            SendEvent(epService, new SupportBean_S1(ids[1], propertyValues[1]));
            SendEvent(epService, new SupportBean_S2(ids[2], propertyValues[2]));
            SendEvent(epService, new SupportBean_S3(ids[3], propertyValues[3]));
            SendEvent(epService, new SupportBean_S4(ids[4], propertyValues[4]));
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
