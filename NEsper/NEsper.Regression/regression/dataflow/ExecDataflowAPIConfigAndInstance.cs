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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowAPIConfigAndInstance : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            EPDataFlowRuntime dataFlowRuntime = epService.EPRuntime.DataFlowRuntime;
            Assert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            try {
                dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
                Assert.Fail();
            } catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual("Dataflow saved configuration 'MyFirstFlow' could not be found", ex.Message);
            }
            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
                Assert.Fail();
            } catch (EPDataFlowNotFoundException ex) {
                Assert.AreEqual("Failed to locate data flow 'MyDataflow'", ex.Message);
            }
    
            // finally create one
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent ()");
            epService.EPAdministrator.CreateEPL("create dataflow MyDataflow " +
                    "BeaconSource -> outdata<MyEvent> {" +
                    "  iterations:1" +
                    "}" +
                    "EventBusSink(outdata) {}");
    
            // add it
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
            Assert.AreEqual(1, dataFlowRuntime.SavedConfigurations.Length);
            EPDataFlowSavedConfiguration savedConfiguration = dataFlowRuntime.GetSavedConfiguration(dataFlowRuntime.SavedConfigurations[0]);
            Assert.AreEqual("MyFirstFlow", savedConfiguration.SavedConfigurationName);
            Assert.AreEqual("MyDataflow", savedConfiguration.DataflowName);
            try {
                dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
                Assert.Fail();
            } catch (EPDataFlowAlreadyExistsException ex) {
                Assert.AreEqual("Data flow saved configuration by name 'MyFirstFlow' already exists", ex.Message);
            }
    
            // remove it
            Assert.IsTrue(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.AreEqual(0, dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
    
            // add once more to instantiate
            dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
            EPDataFlowInstance instance = dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyEvent").Events += listener.Update;
            instance.Run();
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"MyFirstFlow"}, dataFlowRuntime.SavedConfigurations);
            Assert.IsNotNull(dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
    
            // add/remove instance
            dataFlowRuntime.SaveInstance("F1", instance);
            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"F1"}, dataFlowRuntime.SavedInstances);
            EPDataFlowInstance instanceFromSvc = dataFlowRuntime.GetSavedInstance("F1");
            Assert.AreEqual("MyDataflow", instanceFromSvc.DataFlowName);
            try {
                dataFlowRuntime.SaveInstance("F1", instance);
                Assert.Fail();
            } catch (EPDataFlowAlreadyExistsException ex) {
                // expected
                Assert.AreEqual("Data flow instance name 'F1' already saved", ex.Message);
            }
            Assert.IsTrue(dataFlowRuntime.RemoveSavedInstance("F1"));
            Assert.IsFalse(dataFlowRuntime.RemoveSavedInstance("F1"));
        }
    }
} // end of namespace
