///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.dataflow.util;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPICreateStartStopDestroy
    {
        private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestCreateStartStop()
        {
            String epl = "@Name('Create-A-Flow') create dataflow MyGraph Emitter -> outstream<?> {}";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            EPDataFlowRuntime dfruntime = _epService.EPRuntime.DataFlowRuntime;
            EPAssertionUtil.AssertEqualsAnyOrder(new String[] { "MyGraph" }, dfruntime.GetDataFlows());
            EPDataFlowDescriptor desc = dfruntime.GetDataFlow("MyGraph");
            Assert.AreEqual("MyGraph", desc.DataFlowName);
            Assert.AreEqual(EPStatementState.STARTED, desc.StatementState);
            Assert.AreEqual("Create-A-Flow", desc.StatementName);

            dfruntime.Instantiate("MyGraph");

            // test duplicate
            TryInvalidCompile(epl, "Error starting statement: Data flow by name 'MyGraph' has already been declared [");

            // stop - can no longer instantiate but still exists
            stmt.Stop();    // not removed
            Assert.AreEqual(EPStatementState.STOPPED, dfruntime.GetDataFlow("MyGraph").StatementState);
            TryInvalidCompile(epl, "Error starting statement: Data flow by name 'MyGraph' has already been declared [");
            TryInstantiate("MyGraph", "Data flow by name 'MyGraph' is currently in STOPPED statement state");
            TryInstantiate("DUMMY", "Data flow by name 'DUMMY' has not been defined");

            // destroy - should be gone
            stmt.Dispose(); // removed, create again
            Assert.AreEqual(null, dfruntime.GetDataFlow("MyGraph"));
            Assert.AreEqual(0, dfruntime.GetDataFlows().Length);
            TryInstantiate("MyGraph", "Data flow by name 'MyGraph' has not been defined");
            try
            {
                stmt.Start();
                Assert.Fail();
            }
            catch (IllegalStateException ex)
            {
                Assert.AreEqual("Cannot start statement, statement is in destroyed state", ex.Message);
            }

            // new one, try start-stop-start
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Stop();
            stmt.Start();
            dfruntime.Instantiate("MyGraph");

        }

        [Test]
        public void TestDeploymentAdmin()
        {
            _epService.EPAdministrator.Configuration.AddImport(typeof(DefaultSupportSourceOp).Namespace);
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

            String eplModule = "create dataflow TheGraph\n" +
                    "create schema ABC as " + typeof(SupportBean).FullName + "," +
                    "DefaultSupportSourceOp -> outstream<SupportBean> {}\n" +
                    "Select(outstream) -> selectedData {select: (select TheString, IntPrimitive from outstream) }\n" +
                    "DefaultSupportCaptureOp(selectedData) {};";
            Module module = _epService.EPAdministrator.DeploymentAdmin.Parse(eplModule);
            Assert.AreEqual(1, module.Items.Count);
            _epService.EPAdministrator.DeploymentAdmin.Deploy(module, null);

            _epService.EPRuntime.DataFlowRuntime.Instantiate("TheGraph");
        }

        private void TryInvalidCompile(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                AssertException(message, ex.Message);
            }
        }

        private void TryInstantiate(String graph, String message)
        {
            try
            {
                _epService.EPRuntime.DataFlowRuntime.Instantiate(graph);
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private static void AssertException(String expected, String message)
        {
            String received = message.Substring(0, message.IndexOf("[") + 1);
            Assert.AreEqual(expected, received);
        }
    }
}
