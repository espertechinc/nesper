///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIConfigAndInstance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var dataFlowRuntime = env.Runtime.DataFlowService;
            Assert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            try {
                dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual("Dataflow saved configuration 'MyFirstFlow' could not be found", ex.Message);
            }

            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", "x", "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowNotFoundException ex) {
                Assert.AreEqual("Failed to locate data flow 'MyDataflow'", ex.Message);
            }

            // finally create one
            var path = new RegressionPath();
            var epl = "create objectarray schema MyEvent ();\n" +
                      "@Name('df') create dataflow MyDataflow " +
                      "BeaconSource => outdata<MyEvent> {" +
                      "  iterations:1" +
                      "}" +
                      "EventBusSink(outdata) {};\n";
            env.CompileDeploy(epl, path);

            // add it
            var deploymentId = env.DeploymentId("df");
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
            Assert.AreEqual(1, dataFlowRuntime.SavedConfigurations.Length);
            var savedConfiguration = dataFlowRuntime.GetSavedConfiguration(dataFlowRuntime.SavedConfigurations[0]);
            Assert.AreEqual("MyFirstFlow", savedConfiguration.SavedConfigurationName);
            Assert.AreEqual("MyDataflow", savedConfiguration.DataflowName);
            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex) {
                Assert.AreEqual("Data flow saved configuration by name 'MyFirstFlow' already exists", ex.Message);
            }

            // remove it
            Assert.IsTrue(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add once more to instantiate
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
            var instance = dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
            env.CompileDeploy("@Name('s0') select * from MyEvent", path).AddListener("s0");
            instance.Run();
            Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
            EPAssertionUtil.AssertEqualsExactOrder(new[] {"MyFirstFlow"}, dataFlowRuntime.SavedConfigurations);
            Assert.IsNotNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add/remove instance
            dataFlowRuntime.SaveInstance("F1", instance);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {"F1"}, dataFlowRuntime.SavedInstances);
            var instanceFromSvc = dataFlowRuntime.GetSavedInstance("F1");
            Assert.AreEqual("MyDataflow", instanceFromSvc.DataFlowName);
            try {
                dataFlowRuntime.SaveInstance("F1", instance);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex) {
                // expected
                Assert.AreEqual("Data flow instance name 'F1' already saved", ex.Message);
            }

            Assert.IsTrue(dataFlowRuntime.RemoveSavedInstance("F1"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedInstance("F1"));

            env.UndeployAll();
        }
    }
} // end of namespace