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
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientStatementAwareEvents : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>("Bean");
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionStmtAware(epService);
            RunAssertionInvalid(epService);
            RunAssertionBothListeners(epService);
            RunAssertionUseOnMultipleStmts(epService);
            RunAssertionOrderOfInvocation(epService);
        }
    
        private void RunAssertionStmtAware(EPServiceProvider epService) {
            string stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportStmtAwareUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listener.IsInvoked);
            Assert.AreEqual(1, listener.StatementList.Count);
            Assert.AreEqual(statement, listener.StatementList[0]);
            Assert.AreEqual(1, listener.SvcProviderList.Count);
            Assert.AreEqual(epService, listener.SvcProviderList[0]);
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
#if NOT_VALID
            string stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            try {
                statement.Events += (StatementAwareUpdateListener) null;
                Assert.Fail();
            } catch (ArgumentException ex) {
                // expected
            }
    
            statement.Dispose();
#endif
        }

        private void RunAssertionBothListeners(EPServiceProvider epService) {
            string stmtText = "select * from Bean";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
    
            var awareListeners = new SupportStmtAwareUpdateListener[3];
            var updateListeners = new SupportUpdateListener[awareListeners.Length];
            for (int i = 0; i < awareListeners.Length; i++) {
                awareListeners[i] = new SupportStmtAwareUpdateListener();
                statement.Events += awareListeners[i].Update;
                updateListeners[i] = new SupportUpdateListener();
                statement.Events += updateListeners[i].Update;
            }
    
            var theEvent = new SupportBean();
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
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUseOnMultipleStmts(EPServiceProvider epService) {
            EPStatement statementOne = epService.EPAdministrator.CreateEPL("select * from Bean(TheString='A' or TheString='C')");
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL("select * from Bean(TheString='B' or TheString='C')");
    
            var awareListener = new SupportStmtAwareUpdateListener();
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
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionOrderOfInvocation(EPServiceProvider epService) {
            const string stmtText = "select * from Bean";

            var statement = epService.EPAdministrator.CreateEPL(stmtText);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();
            var invoked = new List<Guid>();

            statement.Events += (sender, args) => invoked.Add(id1);
            statement.Events += (sender, args) => invoked.Add(id3);
            statement.Events += (sender, args) => invoked.Add(id2);
            statement.Events += (sender, args) => invoked.Add(id4);

            epService.EPRuntime.SendEvent(new SupportBean());

            Assert.That(invoked.Count, Is.EqualTo(4));
            Assert.That(invoked[0], Is.EqualTo(id1));
            Assert.That(invoked[1], Is.EqualTo(id3));
            Assert.That(invoked[2], Is.EqualTo(id2));
            Assert.That(invoked[3], Is.EqualTo(id4));

            statement.RemoveAllEventHandlers();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class MyUpdateListener
        {
            private readonly IList<object> _invoked;
    
            public MyUpdateListener(IList<object> invoked) {
                this._invoked = invoked;
            }

            public void Update(object sender, UpdateEventArgs e)
            {
                _invoked.Add(this);
            }
        }
    
        public class MyStmtAwareUpdateListener
        {
            private readonly IList<object> _invoked;

            public MyStmtAwareUpdateListener(IList<object> invoked) {
                this._invoked = invoked;
            }

            public void Update(object sender, UpdateEventArgs e)
            {
                _invoked.Add(this);
            }
        }
    }
} // end of namespace
