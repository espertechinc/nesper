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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPatternJoin 
    {
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
        }
    
        [Test]
        public void TestPatternFilterJoin()
        {
            String stmtText = "select irstream es0a.Id as es0aId, " +
                                     "es0a.P00 as es0ap00, " +
                                     "es0b.Id as es0bId, " +
                                     "es0b.P00 as es0bp00, " +
                                     "s1.Id as s1Id, " +
                                     "s1.P10 as s1p10 " +
                    " from " +
                    " pattern [every (es0a=" + typeof(SupportBean_S0).FullName + "(P00='a') " +
                                 "or es0b=" + typeof(SupportBean_S0).FullName + "(P00='b'))]#length(5) as s0," +
                    typeof(SupportBean_S1).FullName + "#length(5) as s1" +
                    " where (es0a.Id = s1.Id) or (es0b.Id = s1.Id)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            SupportUpdateListener updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS1(1, "s1A");
            SendEventS0(2, "a");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(1, "b");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 1, "b", 1, "s1A");
    
            SendEventS1(2, "s2A");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");
    
            SendEventS1(20, "s20A");
            SendEventS1(30, "s30A");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(20, "a");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 20, "a", null, null, 20, "s20A");
    
            SendEventS0(20, "b");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 20, "b", 20, "s20A");
    
            SendEventS0(30, "c");   // filtered out
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(40, "a");   // not matching id in s1
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(50, "b");   // pushing an event S0(2, "a") out the window
            theEvent = updateListener.AssertOneGetOldAndReset();
            AssertEventData(theEvent, 2, "a", null, null, 2, "s2A");
    
            // stop statement
            statement.Stop();
    
            SendEventS1(60, "s20");
            SendEventS0(70, "a");
            SendEventS0(71, "b");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // start statement
            statement.Start();
    
            SendEventS1(70, "s1-70");
            SendEventS0(60, "a");
            SendEventS1(20, "s1");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(70, "b");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, null, null, 70, "b", 70, "s1-70");
        }
    
        [Test]
        public void Test2PatternJoinSelect()
        {
            String stmtText = "select irstream s0.es0.Id as s0es0Id," +
                                     "s0.es1.Id as s0es1Id, " +
                                     "s1.es2.Id as s1es2Id, " +
                                     "s1.es3.Id as s1es3Id, " +
                                     "es0.P00 as es0p00, " +
                                     "es1.P10 as es1p10, " +
                                     "es2.P20 as es2p20, " +
                                     "es3.P30 as es3p30" +
                    " from " +
                    " pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                                     " and es1=" + typeof(SupportBean_S1).FullName + ")]#length(3) as s0," +
                    " pattern [every (es2=" + typeof(SupportBean_S2).FullName +
                                     " and es3=" + typeof(SupportBean_S3).FullName + ")]#length(3) as s1" +
                    " where s0.es0.Id = s1.es2.Id";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            SupportUpdateListener updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SendEventS3(2, "d");
            SendEventS0(3, "a");
            SendEventS2(3, "c");
            SendEventS1(1, "b");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");
    
            SendEventS0(11, "a1");
            SendEventS2(13, "c1");
            SendEventS1(12, "b1");
            SendEventS3(15, "d1");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS3(25, "d2");
            SendEventS0(21, "a2");
            SendEventS2(21, "c2");
            SendEventS1(26, "b2");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 21, 26, 21, 25, "a2", "b2", "c2", "d2");
    
            SendEventS0(31, "a3");
            SendEventS1(32, "b3");
            theEvent = updateListener.AssertOneGetOldAndReset();   // event moving out of window
            AssertEventData(theEvent, 3, 1, 3, 2, "a", "b", "c", "d");
            SendEventS2(33, "c3");
            SendEventS3(35, "d3");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            SendEventS0(41, "a4");
            SendEventS2(43, "c4");
            SendEventS1(42, "b4");
            SendEventS3(45, "d4");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // stop statement
            statement.Stop();
    
            SendEventS3(52, "d5");
            SendEventS0(53, "a5");
            SendEventS2(53, "c5");
            SendEventS1(51, "b5");
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
    
            // start statement
            statement.Start();
    
            SendEventS3(55, "d6");
            SendEventS0(51, "a6");
            SendEventS2(51, "c6");
            SendEventS1(56, "b6");
            theEvent = updateListener.AssertOneGetNewAndReset();
            AssertEventData(theEvent, 51, 56, 51, 55, "a6", "b6", "c6", "d6");
        }
    
        [Test]
        public void Test2PatternJoinWildcard()
        {
            String stmtText = "select * " +
                    " from " +
                    " pattern [every (es0=" + typeof(SupportBean_S0).FullName +
                                     " and es1=" + typeof(SupportBean_S1).FullName + ")]#length(5) as s0," +
                    " pattern [every (es2=" + typeof(SupportBean_S2).FullName +
                                     " and es3=" + typeof(SupportBean_S3).FullName + ")]#length(5) as s1" +
                    " where s0.es0.Id = s1.es2.Id";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            SupportUpdateListener updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            SupportBean_S0 s0 = SendEventS0(100, "");
            SupportBean_S1 s1 = SendEventS1(1, "");
            SupportBean_S2 s2 = SendEventS2(100, "");
            SupportBean_S3 s3 = SendEventS3(2, "");
    
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();

            var result = (IDictionary<String, Object>)theEvent.Get("s0");
            Assert.AreSame(s0, ((EventBean) result.Get("es0")).Underlying);
            Assert.AreSame(s1, ((EventBean) result.Get("es1")).Underlying);

            result = (IDictionary<String, Object>)theEvent.Get("s1");
            Assert.AreSame(s2, ((EventBean) result.Get("es2")).Underlying);
            Assert.AreSame(s3, ((EventBean) result.Get("es3")).Underlying);
        }
    
        private SupportBean_S0 SendEventS0(int id, String P00)
        {
            SupportBean_S0 theEvent = new SupportBean_S0(id, P00);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S1 SendEventS1(int id, String p10)
        {
            SupportBean_S1 theEvent = new SupportBean_S1(id, p10);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S2 SendEventS2(int id, String p20)
        {
            SupportBean_S2 theEvent = new SupportBean_S2(id, p20);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBean_S3 SendEventS3(int id, String p30)
        {
            SupportBean_S3 theEvent = new SupportBean_S3(id, p30);
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void AssertEventData(EventBean theEvent, int s0es0Id, int s0es1Id, int s1es2Id, int s1es3Id,
                                     String P00, String p10, String p20, String p30)
        {
            Assert.AreEqual(s0es0Id, theEvent.Get("s0es0Id"));
            Assert.AreEqual(s0es1Id, theEvent.Get("s0es1Id"));
            Assert.AreEqual(s1es2Id, theEvent.Get("s1es2Id"));
            Assert.AreEqual(s1es3Id, theEvent.Get("s1es3Id"));
            Assert.AreEqual(P00, theEvent.Get("es0p00"));
            Assert.AreEqual(p10, theEvent.Get("es1p10"));
            Assert.AreEqual(p20, theEvent.Get("es2p20"));
            Assert.AreEqual(p30, theEvent.Get("es3p30"));
        }
    
        private void AssertEventData(EventBean theEvent,
                                     int? es0aId, String es0ap00,
                                     int? es0bId, String es0bp00,
                                     int s1Id, String s1p10
                                     )
        {
            Assert.AreEqual(es0aId, theEvent.Get("es0aId"));
            Assert.AreEqual(es0ap00, theEvent.Get("es0ap00"));
            Assert.AreEqual(es0bId, theEvent.Get("es0bId"));
            Assert.AreEqual(es0bp00, theEvent.Get("es0bp00"));
            Assert.AreEqual(s1Id, theEvent.Get("s1Id"));
            Assert.AreEqual(s1p10, theEvent.Get("s1p10"));
        }
    }
}
