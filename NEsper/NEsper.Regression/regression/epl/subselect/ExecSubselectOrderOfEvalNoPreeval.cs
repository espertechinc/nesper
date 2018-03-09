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

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectOrderOfEvalNoPreeval : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Expression.IsSelfSubselectPreeval = false;
        }
    
        public override void Run(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string epl = "select * from SupportBean(IntPrimitive<10) where IntPrimitive not in (select IntPrimitive from SupportBean#unique(IntPrimitive))";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(epl);
            stmtOne.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            stmtOne.Dispose();
    
            string eplTwo = "select * from SupportBean where IntPrimitive not in (select IntPrimitive from SupportBean(IntPrimitive<10)#unique(IntPrimitive))";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
        }
    }
} // end of namespace
