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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionSameWindowReuse(epService);
            RunAssertionStartStop(epService);
            RunAssertionAddRemoveListener(epService);
        }
    
        private void RunAssertionSameWindowReuse(EPServiceProvider epService) {
            string epl = "select * from " + typeof(SupportBean).FullName + "#length(3)";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmtOne.Events += listener.Update;
    
            // send a couple of events
            SendEvent(epService, 1);
            SendEvent(epService, 2);
            SendEvent(epService, 3);
            SendEvent(epService, 4);
    
            // create same statement again
            var testListenerTwo = new SupportUpdateListener();
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(epl);
            stmtTwo.Events += testListenerTwo.Update;
    
            // Send event, no old data should be received
            SendEvent(epService, 5);
            Assert.IsNull(testListenerTwo.LastOldData);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionStartStop(EPServiceProvider epService) {
            string epl = "select count(*) as size from " + typeof(SupportBean).FullName;
            EPStatement sizeStmt = epService.EPAdministrator.CreateEPL(epl);
    
            // View created is automatically started
            Assert.AreEqual(0L, sizeStmt.First().Get("size"));
            sizeStmt.Stop();
    
            // Send an event, view stopped
            SendEvent(epService);
            Assert.IsNotNull(sizeStmt.GetEnumerator());
            Assert.That(sizeStmt.GetEnumerator().MoveNext(), Is.False);
    
            // Start view
            sizeStmt.Start();
            Assert.AreEqual(0L, sizeStmt.First().Get("size"));
    
            // Send event
            SendEvent(epService);
            Assert.AreEqual(1L, sizeStmt.First().Get("size"));
    
            // Stop view
            sizeStmt.Stop();
            Assert.IsNotNull(sizeStmt.GetEnumerator());
            Assert.That(sizeStmt.GetEnumerator().MoveNext(), Is.False);

            // Start again, iterator is zero
            sizeStmt.Start();
            Assert.AreEqual(0L, sizeStmt.First().Get("size"));
    
            sizeStmt.Dispose();
        }
    
        private void RunAssertionAddRemoveListener(EPServiceProvider epService) {
            string epl = "select count(*) as size from " + typeof(SupportBean).FullName;
            EPStatement sizeStmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
    
            // View is started when created
    
            // Add listener send event
            sizeStmt.Events += listener.Update;
            Assert.IsNull(listener.LastNewData);
            Assert.AreEqual(0L, sizeStmt.First().Get("size"));
            SendEvent(epService);
            Assert.AreEqual(1L, listener.GetAndResetLastNewData()[0].Get("size"));
            Assert.AreEqual(1L, sizeStmt.First().Get("size"));
    
            // Stop view, send event, view
            sizeStmt.Stop();
            SendEvent(epService);
            Assert.That(sizeStmt.GetEnumerator(), Is.Not.Null);
            Assert.That(sizeStmt.GetEnumerator().MoveNext(), Is.False);
            Assert.IsNull(listener.LastNewData);
    
            // Start again
            sizeStmt.Events -= listener.Update;
            sizeStmt.Events += listener.Update;
            sizeStmt.Start();
    
            SendEvent(epService);
            Assert.AreEqual(1L, listener.GetAndResetLastNewData()[0].Get("size"));
            Assert.AreEqual(1L, sizeStmt.First().Get("size"));
    
            // Stop again, leave listeners
            sizeStmt.Stop();
            sizeStmt.Start();
            SendEvent(epService);
            Assert.AreEqual(1L, listener.GetAndResetLastNewData()[0].Get("size"));
    
            // Remove listener, send event
            sizeStmt.Events -= listener.Update;
            SendEvent(epService);
            Assert.IsNull(listener.LastNewData);
    
            // Add listener back, send event
            sizeStmt.Events += listener.Update;
            SendEvent(epService);
            Assert.AreEqual(3L, listener.GetAndResetLastNewData()[0].Get("size"));
    
            sizeStmt.Dispose();
        }
    
        private void SendEvent(EPServiceProvider epService) {
            SendEvent(epService, -1);
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
