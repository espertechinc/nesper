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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.expr.filter
{
    public class ExecFilterLargeThreading : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportEvent", typeof(SupportTradeEvent));
            configuration.EngineDefaults.Execution.ThreadingProfile = ConfigurationEngineDefaults.ThreadingProfile.LARGE;
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtOneText = "every event1=SupportEvent(userId like '123%')";
            EPStatement statement = epService.EPAdministrator.CreatePattern(stmtOneText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(1, null, 1001));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportTradeEvent(2, "1234", 1001));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("event1.id"));
        }
    }
} // end of namespace
