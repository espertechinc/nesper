///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowAPIConfigAndInstance : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var dataFlowRuntime = env.Runtime.DataFlowService;
            ClassicAssert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            ClassicAssert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
            ClassicAssert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            try {
                dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                ClassicAssert.AreEqual("Dataflow saved configuration 'MyFirstFlow' could not be found", ex.Message);
            }

            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", "x", "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowNotFoundException ex) {
                ClassicAssert.AreEqual("Failed to locate data flow 'MyDataflow'", ex.Message);
            }

            // finally create one
            var path = new RegressionPath();
            var epl =
                "@public create objectarray schema MyEvent ();\n" +
                "@name('df') create dataflow MyDataflow " +
                "BeaconSource -> outdata<MyEvent> {" +
                "  iterations:1" +
                "}" +
                "EventBusSink(outdata) {};\n";
            env.CompileDeploy(epl, path);

            // add it
            var deploymentId = env.DeploymentId("df");
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
            ClassicAssert.AreEqual(1, dataFlowRuntime.SavedConfigurations.Length);
            var savedConfiguration = dataFlowRuntime.GetSavedConfiguration(dataFlowRuntime.SavedConfigurations[0]);
            ClassicAssert.AreEqual("MyFirstFlow", savedConfiguration.SavedConfigurationName);
            ClassicAssert.AreEqual("MyDataflow", savedConfiguration.DataflowName);
            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex) {
                ClassicAssert.AreEqual("Data flow saved configuration by name 'MyFirstFlow' already exists", ex.Message);
            }

            // remove it
            ClassicAssert.IsTrue(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            ClassicAssert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            ClassicAssert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            ClassicAssert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add once more to instantiate
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", deploymentId, "MyDataflow", null);
            var instance = dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
            env.CompileDeploy("@name('s0') select * from MyEvent", path).AddListener("s0");
            instance.Run();
            env.AssertListenerInvoked("s0");
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "MyFirstFlow" }, dataFlowRuntime.SavedConfigurations);
            ClassicAssert.IsNotNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add/remove instance
            dataFlowRuntime.SaveInstance("F1", instance);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { "F1" }, dataFlowRuntime.SavedInstances);
            var instanceFromSvc = dataFlowRuntime.GetSavedInstance("F1");
            ClassicAssert.AreEqual("MyDataflow", instanceFromSvc.DataFlowName);
            try {
                dataFlowRuntime.SaveInstance("F1", instance);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex) {
                // expected
                ClassicAssert.AreEqual("Data flow instance name 'F1' already saved", ex.Message);
            }

            ClassicAssert.IsTrue(dataFlowRuntime.RemoveSavedInstance("F1"));
            ClassicAssert.IsFalse(dataFlowRuntime.RemoveSavedInstance("F1"));

            env.UndeployAll();
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.DATAFLOW);
        }
    }
} // end of namespace