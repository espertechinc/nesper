///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprCurrentEvaluationContext : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendTimer(epService, 0);
    
            RunAssertionExecCtx(epService, false);
            RunAssertionExecCtx(epService, true);
        }
    
        private void RunAssertionExecCtx(EPServiceProvider epService, bool soda) {
            string epl = "select " +
                    "current_evaluation_context() as c0, " +
                    "current_evaluation_context(), " +
                    "current_evaluation_context().get_EngineURI() as c2 from SupportBean";
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, epl, "my_user_object");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            Assert.AreEqual(typeof(EPLExpressionEvaluationContext), stmt.EventType.GetPropertyType("current_evaluation_context()"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean @event = listener.AssertOneGetNewAndReset();
            EPLExpressionEvaluationContext ctx = (EPLExpressionEvaluationContext) @event.Get("c0");
            Assert.AreEqual(epService.URI, ctx.EngineURI);
            Assert.AreEqual(stmt.Name, ctx.StatementName);
            Assert.AreEqual(-1, ctx.ContextPartitionId);
            Assert.AreEqual("my_user_object", ctx.StatementUserObject);
            Assert.AreEqual(epService.URI, @event.Get("c2"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
