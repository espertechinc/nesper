///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestPriorityAndDropInstructions 
    {
        private EPServiceProvider epService;
        private SupportUpdateListener listener;
        private SupportUpdateListener listenerTwo;
        private SupportUpdateListener[] listeners;
    
        [SetUp]
        public void SetUp()
        {
            listener = new SupportUpdateListener();
            listenerTwo = new SupportUpdateListener();
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.EngineDefaults.ExecutionConfig.IsPrioritized = true;     // also sets share-views to false
    
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
    
            listeners = new SupportUpdateListener[10];
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i] = new SupportUpdateListener();
            }
        }
    
        [TearDown]
        public void TearDown() {
            listener = null;
            listenerTwo = null;
            listeners = null;
        }
    
        [Test]
        public void TestSchedulingPriority()
        {
            SendTimer(0,epService);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Priority(1) select 1 as prio from pattern [every timer:interval(10)]", "s1");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select 3 as prio from pattern [every timer:interval(10)]", "s3");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(4) select 4 as prio from pattern [every timer:interval(10)]", "s4");
            stmt.Events += listener.Update;
    
            SendTimer(10000, epService);
            AssertPrio(null, new int[] {4, 3, 2, 1});
    
            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select 0 as prio from pattern [every timer:interval(10)]", "s0");
            stmt.Events += listener.Update;
    
            SendTimer(20000, epService);
            AssertPrio(null, new int[] {4, 3, 1, 0});
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;
    
            SendTimer(30000, epService);
            AssertPrio(null, new int[] {4, 3, 2, 1, 0});
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select 3 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;
    
            SendTimer(40000, epService);
            AssertPrio(null, new int[] {4, 3, 3, 2, 1, 0});
        }
    
        [Test]
        public void TestSchedulingDrop()
        {
            SendTimer(0,epService);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Drop select 1 as prio from pattern [every timer:interval(10)]", "s1");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select 3 as prio from pattern [every timer:interval(10)]", "s3");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;
    
            SendTimer(10000, epService);
            AssertPrio(null, new int[] {3, 1});
        }
    
        [Test]
        public void TestNamedWindowPriority()
        {
            String stmtText;
            EPStatement stmt;
    
            stmtText = "create window MyWindow.std:lastevent() as select * from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "@Priority(1) on MyWindow e select e.TheString as TheString, 1 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s1");
            stmt.Events += listener.Update;

            stmtText = "@Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s3");
            stmt.Events += listener.Update;

            stmtText = "@Priority(2) on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;

            stmtText = "@Priority(4) on MyWindow e select e.TheString as TheString, 4 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s4");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPrio("E1", new int[] {4, 3, 2, 1});
    
            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL("on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow", "s0");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPrio("E2", new int[] {4, 3, 1, 0});

            stmtText = "@Priority(2) on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertPrio("E3", new int[] {4, 3, 2, 1, 0});

            stmtText = "@Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "sx");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertPrio("E4", new int[] {4, 3, 3, 2, 1, 0});
        }
    
        [Test]
        public void TestNamedWindowDrop()
        {
            String stmtText;
            EPStatement stmt;
    
            stmtText = "create window MyWindow.std:lastevent() as select * from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "@Drop on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;

            stmtText = "@Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s3");
            stmt.Events += listener.Update;

            stmtText = "on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPrio("E1", new int[] {3, 2});
        }
    
        [Test]
        public void TestPriority()
        {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Priority(1) select *, 1 as prio from SupportBean", "s1");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select *, 3 as prio from SupportBean", "s3");
            stmt.Events += listener.Update;
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select *, 2 as prio from SupportBean", "s2");
            stmt.Events += listener.Update;
            
            stmt = epService.EPAdministrator.CreateEPL("@Priority(4) select *, 4 as prio from SupportBean", "s4");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPrio("E1", new int[] {4, 3, 2, 1});
    
            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select *, 0 as prio from SupportBean", "s0");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPrio("E2", new int[] {4, 3, 1, 0});
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select *, 2 as prio from SupportBean", "s2");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertPrio("E3", new int[] {4, 3, 2, 1, 0});
    
            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select *, 3 as prio from SupportBean", "sx");
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertPrio("E4", new int[] {4, 3, 3, 2, 1, 0});
        }
    
        [Test]
        public void TestAddRemoveStmts()
        {
            String stmtSelectText = "insert into ABCStream select * from SupportBean";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtSelectText);
            stmtSelect.Events += listener.Update;
            
            String stmtOneText = "@Drop select * from SupportBean where IntPrimitive = 1";
            EPStatement statementOne = epService.EPAdministrator.CreateEPL(stmtOneText);
            statementOne.Events += listeners[0].Update;
    
            String stmtTwoText = "@Drop select * from SupportBean where IntPrimitive = 2";
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL(stmtTwoText);
            statementTwo.Events += listeners[1].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(0, "E1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(1, "E2");
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(0, "E3");
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone();
    
            String stmtThreeText = "@Drop select * from SupportBean where IntPrimitive = 3";
            EPStatement statementThree = epService.EPAdministrator.CreateEPL(stmtThreeText);
            statementThree.Events += listeners[2].Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(2, "E5");
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(0, "E6");
            
            statementOne.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean("E7", 1));
            Assert.AreEqual("E7", listener.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone();
    
            String stmtSelectTextTwo = "@Priority(50) select * from SupportBean";
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtSelectTextTwo);
            stmtSelectTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E8", 1));
            Assert.AreEqual("E8", listener.AssertOneGetNewAndReset().Get("TheString"));
            Assert.AreEqual("E8", listenerTwo.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone();
    
            epService.EPRuntime.SendEvent(new SupportBean("E9", 2));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(1, "E9");
        }
    
        private void AssertReceivedSingle(int index, String stringValue)
        {
            for (int i = 0; i < listeners.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                Assert.IsFalse(listeners[i].IsInvoked);
            }
            Assert.AreEqual(stringValue, listeners[index].AssertOneGetNewAndReset().Get("TheString"));
        }
    
        private void AssertPrio(String stringValue, int[] prioValues)
        {
            EventBean[] events = listener.GetNewDataListFlattened();
            Assert.AreEqual(prioValues.Length, events.Length);
            for (int i = 0; i < prioValues.Length; i++)
            {
                Assert.AreEqual(prioValues[i], events[i].Get("prio"));
                if (stringValue != null)
                {
                    Assert.AreEqual(stringValue, events[i].Get("TheString"));
                }
            }
            listener.Reset();
        }
    
        private void AssertReceivedNone()
        {
            foreach (SupportUpdateListener supportUpdateListener in listeners)
            {
                Assert.IsFalse(supportUpdateListener.IsInvoked);
            }
        }

        private static void SendTimer(long time, EPServiceProvider epService)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(time);
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
