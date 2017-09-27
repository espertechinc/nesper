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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestAPIConfigAndInstance
    {
        private EPServiceProvider _epService;
        private EPDataFlowRuntime _dataFlowRuntime;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _dataFlowRuntime = _epService.EPRuntime.DataFlowRuntime;
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _dataFlowRuntime = null;
            _epService = null;
            _listener = null;
        }

        [Test]
        public void TestDataFlowConfigAndInstance()
        {
            Assert.AreEqual(0, _dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(_dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(_dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            try
            {
                _dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex)
            {
                Assert.AreEqual("Dataflow saved configuration 'MyFirstFlow' could not be found", ex.Message);
            }
            try
            {
                _dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowNotFoundException ex)
            {
                Assert.AreEqual("Failed to locate data flow 'MyDataflow'", ex.Message);
            }

            // finally create one
            _epService.EPAdministrator.CreateEPL("create objectarray schema MyEvent ()");
            _epService.EPAdministrator.CreateEPL("create dataflow MyDataflow " +
                    "BeaconSource -> outdata<MyEvent> {" +
                    "  iterations:1" +
                    "}" +
                    "EventBusSink(outdata) {}");

            // add it
            _dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
            Assert.AreEqual(1, _dataFlowRuntime.SavedConfigurations.Length);
            EPDataFlowSavedConfiguration savedConfiguration = _dataFlowRuntime.GetSavedConfiguration(_dataFlowRuntime.SavedConfigurations[0]);
            Assert.AreEqual("MyFirstFlow", savedConfiguration.SavedConfigurationName);
            Assert.AreEqual("MyDataflow", savedConfiguration.DataflowName);
            try
            {
                _dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex)
            {
                Assert.AreEqual("Data flow saved configuration by name 'MyFirstFlow' already exists", ex.Message);
            }

            // remove it
            Assert.IsTrue(_dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.IsFalse(_dataFlowRuntime.RemoveSavedConfiguration("MyFirstFlow"));
            Assert.AreEqual(0, _dataFlowRuntime.SavedConfigurations.Length);
            Assert.IsNull(_dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add once more to instantiate
            _dataFlowRuntime.SaveConfiguration("MyFirstFlow", "MyDataflow", null);
            EPDataFlowInstance instance = _dataFlowRuntime.InstantiateSavedConfiguration("MyFirstFlow");
            _epService.EPAdministrator.CreateEPL("select * from MyEvent").Events += _listener.Update;
            instance.Run();
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "MyFirstFlow" }, _dataFlowRuntime.SavedConfigurations);
            Assert.IsNotNull(_dataFlowRuntime.GetSavedConfiguration("MyFirstFlow"));

            // add/remove instance
            _dataFlowRuntime.SaveInstance("F1", instance);
            EPAssertionUtil.AssertEqualsExactOrder(new String[] { "F1" }, _dataFlowRuntime.SavedInstances);
            EPDataFlowInstance instanceFromSvc = _dataFlowRuntime.GetSavedInstance("F1");
            Assert.AreEqual("MyDataflow", instanceFromSvc.DataFlowName);
            try
            {
                _dataFlowRuntime.SaveInstance("F1", instance);
                Assert.Fail();
            }
            catch (EPDataFlowAlreadyExistsException ex)
            {
                // expected
                Assert.AreEqual("Data flow instance name 'F1' already saved", ex.Message);
            }
            Assert.IsTrue(_dataFlowRuntime.RemoveSavedInstance("F1"));
            Assert.IsFalse(_dataFlowRuntime.RemoveSavedInstance("F1"));
        }
    }
}
