///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestStatementAwareEvents 
    {
        private EPServiceProvider epService;
        private SupportStmtAwareUpdateListener listener;
    
        [SetUp]
        public void SetUp() {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean));
            epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
    
            listener = new SupportStmtAwareUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            listener = null;
        }
    
        [Test]
        public void TestStmtAware() {
            String stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.IsInvoked);
            Assert.AreEqual(1, listener.StatementList.Count);
            Assert.AreEqual(statement, listener.StatementList[0]);
            Assert.AreEqual(1, listener.SvcProviderList.Count);
            Assert.AreEqual(epService, listener.SvcProviderList[0]);
        }
    
#if false
        [Test]
        public void TestInvalid() {
            String stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            StatementAwareUpdateListener listener = null;
            try {
                statement.Events += listener.Update;
                Assert.Fail();
            } catch (ArgumentException ex) {
                // expected
            }
        }
#endif

        [Test]
        public void TestBothListeners() {
            String stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            SupportStmtAwareUpdateListener[] awareListeners = new SupportStmtAwareUpdateListener[3];
            SupportUpdateListener[] updateListeners = new SupportUpdateListener[awareListeners.Length];
            for (int i = 0; i < awareListeners.Length; i++) {
                awareListeners[i] = new SupportStmtAwareUpdateListener();
                statement.Events += awareListeners[i].Update;
                updateListeners[i] = new SupportUpdateListener();
                statement.Events += updateListeners[i].Update;
            }
    
            Object theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            for (int i = 0; i < awareListeners.Length; i++) {
                Assert.AreSame(theEvent, updateListeners[i].AssertOneGetNewAndReset().Underlying);
                Assert.AreSame(theEvent, awareListeners[i].AssertOneGetNewAndReset().Underlying);
            }

            statement.Events -= awareListeners[1].Update;
            theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            for (int i = 0; i < awareListeners.Length; i++) {
                if (i == 1) {
                    Assert.AreSame(theEvent, updateListeners[i].AssertOneGetNewAndReset().Underlying);
                    Assert.IsFalse(awareListeners[i].IsInvoked);
                } else {
                    Assert.AreSame(theEvent, updateListeners[i].AssertOneGetNewAndReset().Underlying);
                    Assert.AreSame(theEvent, awareListeners[i].AssertOneGetNewAndReset().Underlying);
                }
            }

            statement.Events -= updateListeners[1].Update;
            theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            for (int i = 0; i < awareListeners.Length; i++) {
                if (i == 1) {
                    Assert.IsFalse(updateListeners[i].IsInvoked);
                    Assert.IsFalse(awareListeners[i].IsInvoked);
                } else {
                    Assert.AreSame(theEvent, updateListeners[i].AssertOneGetNewAndReset().Underlying);
                    Assert.AreSame(theEvent, awareListeners[i].AssertOneGetNewAndReset().Underlying);
                }
            }
    
            statement.Events += updateListeners[1].Update;
            statement.Events += awareListeners[1].Update;
            theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            for (int i = 0; i < awareListeners.Length; i++) {
                Assert.AreSame(theEvent, updateListeners[i].AssertOneGetNewAndReset().Underlying);
                Assert.AreSame(theEvent, awareListeners[i].AssertOneGetNewAndReset().Underlying);
            }
    
            statement.RemoveAllEventHandlers();
            for (int i = 0; i < awareListeners.Length; i++) {
                Assert.IsFalse(updateListeners[i].IsInvoked);
                Assert.IsFalse(awareListeners[i].IsInvoked);
            }
        }
    
        [Test]
        public void TestUseOnMultipleStmts() {
            EPStatement statementOne = epService.EPAdministrator.CreateEPL("select * from Bean(TheString='A' or TheString='C')");
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL("select * from Bean(TheString='B' or TheString='C')");
    
            SupportStmtAwareUpdateListener awareListener = new SupportStmtAwareUpdateListener();
            statementOne.Events += awareListener.Update;
            statementTwo.Events += awareListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            Assert.AreEqual("B", awareListener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            Assert.AreEqual("A", awareListener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("C", 1));
            Assert.AreEqual(2, awareListener.NewDataList.Count);
            Assert.AreEqual("C", awareListener.NewDataList[0][0].Get("TheString"));
            Assert.AreEqual("C", awareListener.NewDataList[1][0].Get("TheString"));
            EPStatement[] stmts = awareListener.StatementList.ToArray();
            EPAssertionUtil.AssertEqualsAnyOrder(stmts, new object[]{statementOne, statementTwo});
        }
    
        [Test]
        public void TestOrderOfInvocation()
        {
            const string stmtText = "select * from Bean";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();
            var invoked = new List<Guid>();

            statement.Events += delegate { invoked.Add(id1); };
            statement.Events += delegate { invoked.Add(id3); };
            statement.Events += delegate { invoked.Add(id2); };
            statement.Events += delegate { invoked.Add(id4); };

            epService.EPRuntime.SendEvent(new SupportBean());

            Assert.AreEqual(invoked.Count, 4);
            Assert.AreEqual(id1, invoked[0]);
            Assert.AreEqual(id3, invoked[1]);
            Assert.AreEqual(id2, invoked[2]);
            Assert.AreEqual(id4, invoked[3]);
        }
    
        public class MyUpdateListener
        {
            private readonly List<object> _invoked;
    
            public MyUpdateListener(List<object> invoked) {
                _invoked = invoked;
            }
    
            public void Update(Object sender, UpdateEventArgs e)
            {
                Update(e.NewEvents, e.OldEvents);
            }


            public void Update(EventBean[] newEvents, EventBean[] oldEvents) {
                _invoked.Add(this);
            }
        }
    
        public class MyStmtAwareUpdateListener
        {
            private readonly List<object> _invoked;
    
            public MyStmtAwareUpdateListener(List<object> invoked) {
                _invoked = invoked;
            }
    
            public void Update(Object sender, UpdateEventArgs e)
            {
                Update(e.NewEvents, e.OldEvents, e.Statement, e.ServiceProvider);
            }

            public void Update(EventBean[] newEvents, EventBean[] oldEvents, EPStatement statement, EPServiceProvider epServiceProvider) {
                _invoked.Add(this);
            }
        }
    }
}
