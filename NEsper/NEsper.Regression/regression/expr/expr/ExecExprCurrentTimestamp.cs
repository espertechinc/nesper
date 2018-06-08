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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCurrentTimestamp : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionGetTimestamp(epService);
            RunAssertionGetTimestamp_OM(epService);
            RunAssertionGetTimestamp_Compile(epService);
        }
    
        private void RunAssertionGetTimestamp(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select current_timestamp(), " +
                    " current_timestamp as t0, " +
                    " current_timestamp() as t1, " +
                    " current_timestamp + 1 as t2 " +
                    " from " + typeof(SupportBean).FullName;
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("current_timestamp()"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t0"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t1"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t2"));
    
            SendTimer(epService, 100);
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{100L, 100L, 101L});
    
            SendTimer(epService, 999);
            epService.EPRuntime.SendEvent(new SupportBean());
            theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{999L, 999L, 1000L});
            Assert.AreEqual(theEvent.Get("current_timestamp()"), theEvent.Get("t0"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionGetTimestamp_OM(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select current_timestamp() as t0 from " + typeof(SupportBean).FullName;
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.CurrentTimestamp(), "t0");
            model.FromClause = FromClause.Create().Add(FilterStream.Create(typeof(SupportBean).FullName));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t0"));
    
            SendTimer(epService, 777);
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{777L});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGetTimestamp_Compile(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string stmtText = "select current_timestamp() as t0 from " + typeof(SupportBean).FullName;
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("t0"));
    
            SendTimer(epService, 777);
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertResults(theEvent, new object[]{777L});
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void AssertResults(EventBean theEvent, object[] result) {
            for (int i = 0; i < result.Length; i++) {
                Assert.AreEqual(result[i], theEvent.Get("t" + i), "failed for index " + i);
            }
        }
    }
} // end of namespace
