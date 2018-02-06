///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionStartStop(epService);
            RunAssertionAddRemoveListener(epService);
            RunAssertionStartStopTwo(epService);
        }
    
        private void RunAssertionStartStopTwo(EPServiceProvider epService) {
            string stmtText = "select * from pattern [Every(a=" + typeof(SupportBean).FullName +
                    " or b=" + typeof(SupportBeanComplexProps).FullName + ")]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
    
            for (int i = 0; i < 100; i++) {
                SendAndAssert(epService, updateListener);
    
                statement.Stop();
    
                epService.EPRuntime.SendEvent(new SupportBean());
                epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
                Assert.IsFalse(updateListener.IsInvoked);
    
                statement.Start();
            }
    
            statement.Dispose();
        }
    
        private void RunAssertionStartStop(EPServiceProvider epService) {
            string epl = "@IterableUnbound every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = epService.EPAdministrator.CreatePattern(epl, "MyPattern");
            Assert.AreEqual(StatementType.PATTERN, ((EPStatementSPI) patternStmt).StatementMetadata.StatementType);
    
            // Pattern started when created
            Assert.IsFalse(patternStmt.HasFirst());
            var safe = patternStmt.GetSafeEnumerator();
            Assert.IsFalse(safe.MoveNext());
            safe.Dispose();
    
            // Stop pattern
            patternStmt.Stop();
            SendEvent(epService);
            //Assert.IsNull(patternStmt.GetEnumerator());
    
            // Start pattern
            patternStmt.Start();
            Assert.IsFalse(patternStmt.HasFirst());
    
            // Send event
            SupportBean theEvent = SendEvent(epService);
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            safe = patternStmt.GetSafeEnumerator();
            Assert.IsTrue(safe.MoveNext());
            Assert.AreSame(theEvent, safe.Current.Get("tag"));
            safe.Dispose();
    
            // Stop pattern
            patternStmt.Stop();
            //Assert.IsNull(patternStmt.GetEnumerator());
    
            // Start again, iterator is zero
            patternStmt.Start();
            Assert.IsFalse(patternStmt.HasFirst());
    
            // assert statement-eventtype reference info
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsTrue(stmtNames.Contains("MyPattern"));
    
            patternStmt.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            Assert.IsFalse(stmtNames.Contains("MyPattern"));
        }
    
        private void RunAssertionAddRemoveListener(EPServiceProvider epService) {
            string epl = "@IterableUnbound every tag=" + typeof(SupportBean).FullName;
            EPStatement patternStmt = epService.EPAdministrator.CreatePattern(epl, "MyPattern");
            Assert.AreEqual(StatementType.PATTERN, ((EPStatementSPI) patternStmt).StatementMetadata.StatementType);
            var listener = new SupportUpdateListener();
    
            // Pattern started when created
    
            // Add listener
            patternStmt.Events += listener.Update;
            Assert.IsNull(listener.LastNewData);
            Assert.IsFalse(patternStmt.HasFirst());
    
            // Send event
            SupportBean theEvent = SendEvent(epService);
            Assert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
    
            // Remove listener
            patternStmt.Events -= listener.Update;
            theEvent = SendEvent(epService);
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            Assert.IsNull(listener.LastNewData);
    
            // Add listener back
            patternStmt.Events += listener.Update;
            theEvent = SendEvent(epService);
            Assert.AreSame(theEvent, patternStmt.First().Get("tag"));
            Assert.AreEqual(theEvent, listener.GetAndResetLastNewData()[0].Get("tag"));
        }
    
        private void SendAndAssert(EPServiceProvider epService, SupportUpdateListener updateListener) {
            for (int i = 0; i < 1000; i++) {
                Object theEvent = null;
                if (i % 3 == 0) {
                    theEvent = new SupportBean();
                } else {
                    theEvent = SupportBeanComplexProps.MakeDefaultBean();
                }
    
                epService.EPRuntime.SendEvent(theEvent);
    
                EventBean eventBean = updateListener.AssertOneGetNewAndReset();
                if (theEvent is SupportBean) {
                    Assert.AreSame(theEvent, eventBean.Get("a"));
                    Assert.IsNull(eventBean.Get("b"));
                } else {
                    Assert.AreSame(theEvent, eventBean.Get("b"));
                    Assert.IsNull(eventBean.Get("a"));
                }
            }
        }
    
        private SupportBean SendEvent(EPServiceProvider epService) {
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
} // end of namespace
