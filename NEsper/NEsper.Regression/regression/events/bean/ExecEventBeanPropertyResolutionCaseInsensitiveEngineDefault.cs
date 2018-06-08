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

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPropertyResolutionCaseInsensitiveEngineDefault : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("Bean", typeof(SupportBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            TryCaseInsensitive(epService, "select THESTRING, INTPRIMITIVE from Bean where THESTRING='A'", "THESTRING", "INTPRIMITIVE");
            TryCaseInsensitive(epService, "select ThEsTrInG, INTprimitIVE from Bean where THESTRing='A'", "ThEsTrInG", "INTprimitIVE");
        }
    
        internal static void TryCaseInsensitive(EPServiceProvider epService, string stmtText, string propOneName, string propTwoName) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", result.Get(propOneName));
            Assert.AreEqual(10, result.Get(propTwoName));
        }
    
    }
} // end of namespace
