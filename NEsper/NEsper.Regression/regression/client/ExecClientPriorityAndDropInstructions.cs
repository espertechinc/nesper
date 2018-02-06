///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientPriorityAndDropInstructions : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType<SupportBean>();
            configuration.EngineDefaults.Execution.IsPrioritized = true; // also sets share-views to false
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionSchedulingPriority(epService);
            RunAssertionSchedulingDrop(epService);
            RunAssertionNamedWindowPriority(epService);
            RunAssertionNamedWindowDrop(epService);
            RunAssertionPriority(epService);
            RunAssertionAddRemoveStmts(epService);
        }

        [Priority(10)]
        [Drop]
        private void RunAssertionSchedulingPriority(EPServiceProvider epService)
        {
            SendTimer(0, epService);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(1) select 1 as prio from pattern [every timer:interval(10)]", "s1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(3) select 3 as prio from pattern [every timer:interval(10)]", "s3");
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(2) select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(4) select 4 as prio from pattern [every timer:interval(10)]", "s4");
            stmt.Events += listener.Update;

            SendTimer(10000, epService);
            AssertPrio(listener, null, new[] {4, 3, 2, 1});

            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL(
                "select 0 as prio from pattern [every timer:interval(10)]", "s0");
            stmt.Events += listener.Update;

            SendTimer(20000, epService);
            AssertPrio(listener, null, new[] {4, 3, 1, 0});

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(2) select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;

            SendTimer(30000, epService);
            AssertPrio(listener, null, new[] {4, 3, 2, 1, 0});

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(3) select 3 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;

            SendTimer(40000, epService);
            AssertPrio(listener, null, new[] {4, 3, 3, 2, 1, 0});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionSchedulingDrop(EPServiceProvider epService)
        {
            SendTimer(0, epService);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                "@Drop select 1 as prio from pattern [every timer:interval(10)]", "s1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(2) select 3 as prio from pattern [every timer:interval(10)]", "s3");
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL(
                "select 2 as prio from pattern [every timer:interval(10)]", "s2");
            stmt.Events += listener.Update;

            SendTimer(10000, epService);
            AssertPrio(listener, null, new[] {3, 1});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNamedWindowPriority(EPServiceProvider epService)
        {
            string stmtText;
            EPStatement stmt;

            stmtText = "create window MyWindow#lastevent as select * from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "@Priority(1) on MyWindow e select e.TheString as TheString, 1 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s1");
            var listener = new SupportUpdateListener();
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
            AssertPrio(listener, "E1", new[] {4, 3, 2, 1});

            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL(
                "on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow", "s0");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPrio(listener, "E2", new[] {4, 3, 1, 0});

            stmtText = "@Priority(2) on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertPrio(listener, "E3", new[] {4, 3, 2, 1, 0});

            stmtText = "@Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "sx");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertPrio(listener, "E4", new[] {4, 3, 3, 2, 1, 0});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionNamedWindowDrop(EPServiceProvider epService)
        {
            string stmtText;
            EPStatement stmt;

            stmtText = "create window MyWindow#lastevent as select * from SupportBean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "@Drop on MyWindow e select e.TheString as TheString, 2 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            stmtText = "@Priority(3) on MyWindow e select e.TheString as TheString, 3 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s3");
            stmt.Events += listener.Update;

            stmtText = "on MyWindow e select e.TheString as TheString, 0 as prio from MyWindow";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "s2");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPrio(listener, "E1", new[] {3, 2});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionPriority(EPServiceProvider epService)
        {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                "@Priority(1) select *, 1 as prio from SupportBean", "s1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select *, 3 as prio from SupportBean", "s3");
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select *, 2 as prio from SupportBean", "s2");
            stmt.Events += listener.Update;

            stmt = epService.EPAdministrator.CreateEPL("@Priority(4) select *, 4 as prio from SupportBean", "s4");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            AssertPrio(listener, "E1", new[] {4, 3, 2, 1});

            epService.EPAdministrator.GetStatement("s2").Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select *, 0 as prio from SupportBean", "s0");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            AssertPrio(listener, "E2", new[] {4, 3, 1, 0});

            stmt = epService.EPAdministrator.CreateEPL("@Priority(2) select *, 2 as prio from SupportBean", "s2");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            AssertPrio(listener, "E3", new[] {4, 3, 2, 1, 0});

            stmt = epService.EPAdministrator.CreateEPL("@Priority(3) select *, 3 as prio from SupportBean", "sx");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertPrio(listener, "E4", new[] {4, 3, 3, 2, 1, 0});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionAddRemoveStmts(EPServiceProvider epService)
        {
            string stmtSelectText = "insert into ABCStream select * from SupportBean";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtSelectText);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            string stmtOneText = "@Drop select * from SupportBean where IntPrimitive = 1";
            EPStatement statementOne = epService.EPAdministrator.CreateEPL(stmtOneText);
            SupportUpdateListener[] listeners = SupportUpdateListener.MakeListeners(10);
            statementOne.Events += listeners[0].Update;

            string stmtTwoText = "@Drop select * from SupportBean where IntPrimitive = 2";
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL(stmtTwoText);
            statementTwo.Events += listeners[1].Update;

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 0, "E1");

            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 1, "E2");

            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 0, "E3");

            epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone(listeners);

            string stmtThreeText = "@Drop select * from SupportBean where IntPrimitive = 3";
            EPStatement statementThree = epService.EPAdministrator.CreateEPL(stmtThreeText);
            statementThree.Events += listeners[2].Update;

            epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 2, "E5");

            epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 0, "E6");

            statementOne.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean("E7", 1));
            Assert.AreEqual("E7", listener.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone(listeners);

            string stmtSelectTextTwo = "@Priority(50) select * from SupportBean";
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtSelectTextTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerTwo.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E8", 1));
            Assert.AreEqual("E8", listener.AssertOneGetNewAndReset().Get("TheString"));
            Assert.AreEqual("E8", listenerTwo.AssertOneGetNewAndReset().Get("TheString"));
            AssertReceivedNone(listeners);

            epService.EPRuntime.SendEvent(new SupportBean("E9", 2));
            Assert.IsFalse(listener.IsInvoked);
            AssertReceivedSingle(listeners, 1, "E9");

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void AssertReceivedSingle(SupportUpdateListener[] listeners, int index, string stringValue)
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

        private void AssertPrio(SupportUpdateListener listener, string theString, int[] prioValues)
        {
            EventBean[] events = listener.GetNewDataListFlattened();
            Assert.AreEqual(prioValues.Length, events.Length);
            for (int i = 0; i < prioValues.Length; i++)
            {
                Assert.AreEqual(prioValues[i], events[i].Get("prio"));
                if (theString != null)
                {
                    Assert.AreEqual(theString, events[i].Get("TheString"));
                }
            }

            listener.Reset();
        }

        private void AssertReceivedNone(SupportUpdateListener[] listeners)
        {
            for (int i = 0; i < listeners.Length; i++)
            {
                Assert.IsFalse(listeners[i].IsInvoked);
            }
        }

        private void SendTimer(long time, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(time);
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
