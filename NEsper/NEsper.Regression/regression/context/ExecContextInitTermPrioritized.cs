///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.context
{
    public class ExecContextInitTermPrioritized : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsPrioritized = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionNonOverlappingSubqueryAndInvalid(epService);
            RunAssertionAtNowWithSelectedEventEnding(epService);
        }
    
        private void RunAssertionNonOverlappingSubqueryAndInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(ExecContextInitTerm.Event));
            SendTimeEvent(epService, "2002-05-1T10:00:00.000");
    
            string epl =
                    "\n @Name('ctx') create context RuleActivityTime as start (0, 9, *, *, *) end (0, 17, *, *, *);" +
                            "\n @Name('window') context RuleActivityTime create window EventsWindow#firstunique(productID) as Event;" +
                            "\n @Name('variable') create variable bool IsOutputTriggered_2 = false;" +
                            "\n @Name('A') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                            "\n @Name('B') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                            "\n @Name('C') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                            "\n @Name('D') context RuleActivityTime insert into EventsWindow select * from Event(not exists (select * from EventsWindow));" +
                            "\n @Name('out') context RuleActivityTime select * from EventsWindow";
    
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPAdministrator.GetStatement("out").Events += new SupportUpdateListener().Update;
    
            epService.EPRuntime.SendEvent(new ExecContextInitTerm.Event("A1"));
    
            // invalid - subquery not the same context
            TryInvalid(epService, "insert into EventsWindow select * from Event(not exists (select * from EventsWindow))",
                    "Failed to validate subquery number 1 querying EventsWindow: Named window by name 'EventsWindow' has been declared for context 'RuleActivityTime' and can only be used within the same context");
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionAtNowWithSelectedEventEnding(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string[] fields = "TheString".Split(',');
            epService.EPAdministrator.CreateEPL("@Priority(1) create context C1 start @now end SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Priority(0) context C1 select * from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2"});
        }
    
        private void SendTimeEvent(EPServiceProvider epService, string time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    }
} // end of namespace
