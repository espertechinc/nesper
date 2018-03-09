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
    public class ExecJoin3StreamAndPropertyPerformance : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPerfAllProps(epService);
            RunAssertionPerfPartialProps(epService);
            RunAssertionPerfPartialStreams(epService);
        }
    
        private void RunAssertionPerfAllProps(EPServiceProvider epService) {
            // Statement where all streams are reachable from each other via properties
            string stmt = "select * from " +
                    typeof(SupportBean_A).FullName + "()#length(1000000) s1," +
                    typeof(SupportBean_B).FullName + "()#length(1000000) s2," +
                    typeof(SupportBean_C).FullName + "()#length(1000000) s3" +
                    " where s1.id=s2.id and s2.id=s3.id and s1.id=s3.id";
            TryJoinPerf3Streams(epService, stmt);
        }
    
        private void RunAssertionPerfPartialProps(EPServiceProvider epService) {
            // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
            string stmt = "select * from " +
                    typeof(SupportBean_A).FullName + "#length(1000000) s1," +
                    typeof(SupportBean_B).FullName + "#length(1000000) s2," +
                    typeof(SupportBean_C).FullName + "#length(1000000) s3" +
                    " where s1.id=s2.id and s2.id=s3.id";   // ==> therefore s1.id = s3.id
            TryJoinPerf3Streams(epService, stmt);
        }
    
        private void RunAssertionPerfPartialStreams(EPServiceProvider epService) {
            string methodName = ".testPerfPartialStreams";
    
            // Statement where the s1 stream is not reachable by joining s2 to s3 and s3 to s1
            string epl = "select * from " +
                    typeof(SupportBean_A).FullName + "()#length(1000000) s1," +
                    typeof(SupportBean_B).FullName + "()#length(1000000) s2," +
                    typeof(SupportBean_C).FullName + "()#length(1000000) s3" +
                    " where s1.id=s2.id";   // ==> stream s3 no properties supplied, full s3 scan
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            // preload s3 with just 1 event
            SendEvent(epService, new SupportBean_C("GE_0"));
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 1000; i++) {
                SendEvent(epService, new SupportBean_A("CSCO_" + i));
                SendEvent(epService, new SupportBean_B("IBM_" + i));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay below 500, no index would be 4 sec plus
            Assert.IsTrue((endTime - startTime) < 500);
            stmt.Dispose();
        }
    
        private void TryJoinPerf3Streams(EPServiceProvider epService, string epl) {
            string methodName = ".tryJoinPerf3Streams";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            // Send events for each stream
            Log.Info(methodName + " Preloading events");
            long startTime = DateTimeHelper.CurrentTimeMillis;
            for (int i = 0; i < 100; i++) {
                SendEvent(epService, new SupportBean_A("CSCO_" + i));
                SendEvent(epService, new SupportBean_B("IBM_" + i));
                SendEvent(epService, new SupportBean_C("GE_" + i));
            }
            Log.Info(methodName + " Done preloading");
    
            long endTime = DateTimeHelper.CurrentTimeMillis;
            Log.Info(methodName + " delta=" + (endTime - startTime));
    
            // Stay below 500, no index would be 4 sec plus
            Assert.IsTrue((endTime - startTime) < 500);
    
            stmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService, Object theEvent) {
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
