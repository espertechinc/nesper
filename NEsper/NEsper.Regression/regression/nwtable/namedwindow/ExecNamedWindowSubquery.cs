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

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowSubquery : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSubqueryTwoConsumerWindow(epService);
            RunAssertionSubqueryLateConsumerAggregation(epService);
        }
    
        private void RunAssertionSubqueryTwoConsumerWindow(EPServiceProvider epService) {
            string epl =
                    "\n create window MyWindowTwo#length(1) as (mycount long);" +
                            "\n @Name('insert-count') insert into MyWindowTwo select 1L as mycount from SupportBean;" +
                            "\n create variable long myvar = 0;" +
                            "\n @Name('assign') on MyWindowTwo set myvar = (select mycount from MyWindowTwo);";
            EPServiceProvider engine = EPServiceProviderManager.GetDefaultProvider();
            engine.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            engine.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            engine.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1L, engine.EPRuntime.GetVariableValue("myvar"));   // if the subquery-consumer executes first, this will be null
        }
    
        private void RunAssertionSubqueryLateConsumerAggregation(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyWindow where (select count(*) from MyWindow) > 0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsTrue(listener.IsInvoked);
        }
    }
} // end of namespace
