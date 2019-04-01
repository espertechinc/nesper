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

namespace com.espertech.esper.regression.epl.join
{
    public class ExecJoinPatterns : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionPatternFilterJoin(epService);
            RunAssertion2PatternJoinSelect(epService);
            RunAssertion2PatternJoinWildcard(epService);
        }
    
        private void RunAssertionPatternFilterJoin(EPServiceProvider epService) {
            string stmtText = "select irstream es0a.id as es0aId, " +
                    "es0a.p00 as es0ap00, " +
                    "es0b.id as es0bId, " +
                    "es0b.p00 as es0bp00, " +
                    "s1.id as s1Id, " +
                    "s1.p10 as s1p10 " +
                    " from " +
                    " pattern [every (es0a=" + typeof(SupportBean_S0).FullName + "(p00='a') " +
                    "or es0b=" + typeof(SupportBean_S0).FullName + "(p00='b'))]#length(5) as s0," +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " where (es0a.id = s1.id) or (es0b.id = s1.id)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS1(epService, 1, "s1A");
            SendEventS0(epService, 2, "a");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 1, "b");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 1, "b", 1, "s1A");
    
            SendEventS1(epService, 2, "s2A");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");
    
            SendEventS1(epService, 20, "s20A");
            SendEventS1(epService, 30, "s30A");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 20, "a");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 20, "a", null, null, 20, "s20A");
    
            SendEventS0(epService, 20, "b");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 20, "b", 20, "s20A");
    
            SendEventS0(epService, 30, "c");   // filtered out
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 40, "a");   // not matching id in s1
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 50, "b");   // pushing an event S0(2, "a") out the window
            theEvent = updateListener.AssertOneGetOldAndReset();
            AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");
    
            // stop statement
            statement.Stop();
    
            SendEventS1(epService, 60, "s20");
            SendEventS0(epService, 70, "a");
            SendEventS0(epService, 71, "b");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // start statement
            statement.Start();
    
            SendEventS1(epService, 70, "s1-70");
            SendEventS0(epService, 60, "a");
            SendEventS1(epService, 20, "s1");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 70, "b");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 70, "b", 70, "s1-70");
    
            statement.Dispose();
        }
    
        private void RunAssertion2PatternJoinSelect(EPServiceProvider epService) {
            string stmtText = "select irstream s0.es0.id as s0es0Id," +
                    "s0.es1.id as s0es1Id, " +
                    "s1.es2.id as s1es2Id, " +
                    "s1.es3.id as s1es3Id, " +
                    "es0.p00 as es0p00, " +
                    "es1.p10 as es1p10, " +
                    "es2.p20 as es2p20, " +
                    "es3.p30 as es3p30" +
                    " from " +
                    " pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                    " and es1=" + typeof(SupportBean_S1).FullName + ")]#length(3) as s0," +
                    " pattern [every (es2=" + typeof(SupportBean_S2).FullName +
                    " and es3=" + typeof(SupportBean_S3).FullName + ")]#length(3) as s1" +
                    " where s0.es0.id = s1.es2.id";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS3(epService, 2, "d");
            SendEventS0(epService, 3, "a");
            SendEventS2(epService, 3, "c");
            SendEventS1(epService, 1, "b");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");
    
            SendEventS0(epService, 11, "a1");
            SendEventS2(epService, 13, "c1");
            SendEventS1(epService, 12, "b1");
            SendEventS3(epService, 15, "d1");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS3(epService, 25, "d2");
            SendEventS0(epService, 21, "a2");
            SendEventS2(epService, 21, "c2");
            SendEventS1(epService, 26, "b2");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 21, 26, 21, 25, "a2", "b2", "c2", "d2");
    
            SendEventS0(epService, 31, "a3");
            SendEventS1(epService, 32, "b3");
            theEvent = updateListener.AssertOneGetOldAndReset();   // event moving out of window
            AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");
            SendEventS2(epService, 33, "c3");
            SendEventS3(epService, 35, "d3");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(epService, 41, "a4");
            SendEventS2(epService, 43, "c4");
            SendEventS1(epService, 42, "b4");
            SendEventS3(epService, 45, "d4");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // stop statement
            statement.Stop();
    
            SendEventS3(epService, 52, "d5");
            SendEventS0(epService, 53, "a5");
            SendEventS2(epService, 53, "c5");
            SendEventS1(epService, 51, "b5");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // start statement
            statement.Start();
    
            SendEventS3(epService, 55, "d6");
            SendEventS0(epService, 51, "a6");
            SendEventS2(epService, 51, "c6");
            SendEventS1(epService, 56, "b6");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 51, 56, 51, 55, "a6", "b6", "c6", "d6");
    
            statement.Dispose();
        }
    
        private void RunAssertion2PatternJoinWildcard(EPServiceProvider epService) {
            string stmtText = "select * " +
                    " from " +
                    " pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                    " and es1=" + typeof(SupportBean_S1).FullName + ")]#length(5) as s0," +
                    " pattern [every (es2=" + typeof(SupportBean_S2).FullName +
                    " and es3=" + typeof(SupportBean_S3).FullName + ")]#length(5) as s1" +
                    " where s0.es0.id = s1.es2.id";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SupportBean_S0 s0 = SendEventS0(epService, 100, "");
            SupportBean_S1 s1 = SendEventS1(epService, 1, "");
            SupportBean_S2 s2 = SendEventS2(epService, 100, "");
            SupportBean_S3 s3 = SendEventS3(epService, 2, "");
    
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
    
            IDictionary<string, EventBean> result = theEvent.Get("s0")
                .UnwrapStringDictionary()
                .Transform(k => k, v => (EventBean) v, k => k, v => v);
            Assert.AreSame(s0, result.Get("es0").Underlying);
            Assert.AreSame(s1, result.Get("es1").Underlying);

            result = theEvent.Get("s1")
                .UnwrapStringDictionary()
                .Transform(k => k, v => (EventBean) v, k => k, v => v);
            Assert.AreSame(s2, result.Get("es2").Underlying);
            Assert.AreSame(s3, result.Get("es3").Underlying);
    
            statement.Dispose();
        }
    
        private SupportBean_S0 SendEventS0(EPServiceProvider epService, int id, string p00) {
            var theEvent = new SupportBean_S0(id, p00);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S1 SendEventS1(EPServiceProvider epService, int id, string p10) {
            var theEvent = new SupportBean_S1(id, p10);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S2 SendEventS2(EPServiceProvider epService, int id, string p20) {
            var theEvent = new SupportBean_S2(id, p20);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S3 SendEventS3(EPServiceProvider epService, int id, string p30) {
            var theEvent = new SupportBean_S3(id, p30);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void AssertEventData(EventBean theEvent, int s0es0Id, int s0es1Id, int s1es2Id, int s1es3Id,
                                     string p00, string p10, string p20, string p30) {
            Assert.AreEqual(s0es0Id, theEvent.Get("s0es0Id"));
            Assert.AreEqual(s0es1Id, theEvent.Get("s0es1Id"));
            Assert.AreEqual(s1es2Id, theEvent.Get("s1es2Id"));
            Assert.AreEqual(s1es3Id, theEvent.Get("s1es3Id"));
            Assert.AreEqual(p00, theEvent.Get("es0p00"));
            Assert.AreEqual(p10, theEvent.Get("es1p10"));
            Assert.AreEqual(p20, theEvent.Get("es2p20"));
            Assert.AreEqual(p30, theEvent.Get("es3p30"));
        }
    
        private void AssertEventData(EventBean theEvent,
                                     int? es0aId, string es0ap00,
                                     int? es0bId, string es0bp00,
                                     int s1Id, string s1p10
        ) {
            Assert.AreEqual(es0aId, theEvent.Get("es0aId"));
            Assert.AreEqual(es0ap00, theEvent.Get("es0ap00"));
            Assert.AreEqual(es0bId, theEvent.Get("es0bId"));
            Assert.AreEqual(es0bp00, theEvent.Get("es0bp00"));
            Assert.AreEqual(s1Id, theEvent.Get("s1Id"));
            Assert.AreEqual(s1p10, theEvent.Get("s1p10"));
        }
    }
} // end of namespace
