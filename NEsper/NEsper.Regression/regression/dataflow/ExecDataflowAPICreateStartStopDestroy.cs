///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPICreateStartStopDestroy : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCreateStartStop(epService);
            RunAssertionDeploymentAdmin(epService);
        }
    
        private void RunAssertionCreateStartStop(EPServiceProvider epService) {
            string epl = "@Name('Create-A-Flow') create dataflow MyGraph Emitter -> outstream<?> {}";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
    
            EPDataFlowRuntime dfruntime = epService.EPRuntime.DataFlowRuntime;
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"MyGraph"}, dfruntime.GetDataFlows());
            EPDataFlowDescriptor desc = dfruntime.GetDataFlow("MyGraph");
            Assert.AreEqual("MyGraph", desc.DataFlowName);
            Assert.AreEqual(EPStatementState.STARTED, desc.StatementState);
            Assert.AreEqual("Create-A-Flow", desc.StatementName);
    
            dfruntime.Instantiate("MyGraph");
    
            // test duplicate
            TryInvalidCompile(epService, epl, "Error starting statement: Data flow by name 'MyGraph' has already been declared [");
    
            // stop - can no longer instantiate but still Exists
            stmt.Stop();    // not removed
            Assert.AreEqual(EPStatementState.STOPPED, dfruntime.GetDataFlow("MyGraph").StatementState);
            TryInvalidCompile(epService, epl, "Error starting statement: Data flow by name 'MyGraph' has already been declared [");
            TryInstantiate(epService, "MyGraph", "Data flow by name 'MyGraph' is currently in STOPPED statement state");
            TryInstantiate(epService, "DUMMY", "Data flow by name 'DUMMY' has not been defined");
    
            // destroy - should be gone
            stmt.Dispose(); // removed, create again
            Assert.AreEqual(null, dfruntime.GetDataFlow("MyGraph"));
            Assert.AreEqual(0, dfruntime.GetDataFlows().Length);
            TryInstantiate(epService, "MyGraph", "Data flow by name 'MyGraph' has not been defined");
            try {
                stmt.Start();
                Assert.Fail();
            } catch (IllegalStateException ex) {
                Assert.AreEqual("Cannot start statement, statement is in destroyed state", ex.Message);
            }
    
            // new one, try start-stop-start
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Stop();
            stmt.Start();
            dfruntime.Instantiate("MyGraph");
        }
    
        private void RunAssertionDeploymentAdmin(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).Namespace);
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            string eplModule = "create dataflow TheGraph\n" +
                    "create schema ABC as " + typeof(SupportBean).FullName + "," +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {}\n" +
                    "Select(outstream) -> selectedData {select: (select TheString, IntPrimitive from outstream) }\n" +
                    "DefaultSupportCaptureOp(selectedData) {};";
            Module module = epService.EPAdministrator.DeploymentAdmin.Parse(eplModule);
            Assert.AreEqual(1, module.Items.Count);
            epService.EPAdministrator.DeploymentAdmin.Deploy(module, null);
    
            epService.EPRuntime.DataFlowRuntime.Instantiate("TheGraph");
        }
    
        private void TryInvalidCompile(EPServiceProvider epService, string epl, string message) {
            try {
                epService.EPAdministrator.CreateEPL(epl, UuidGenerator.Generate());
                Assert.Fail();
            } catch (EPStatementException ex) {
                AssertException(message, ex.Message);
            }
        }
    
        private void TryInstantiate(EPServiceProvider epService, string graph, string message) {
            try {
                epService.EPRuntime.DataFlowRuntime.Instantiate(graph);
                Assert.Fail();
            } catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertException(string expected, string message) {
            string received = message.Substring(0, message.IndexOf("[") + 1);
            Assert.AreEqual(expected, received);
        }
    }
} // end of namespace
