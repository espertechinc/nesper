///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.plugin;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestAggregationMultiFunctionPlugIn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            var config = new ConfigurationPlugInAggregationMultiFunction(SupportAggMFFuncExtensions.GetFunctionNames(), typeof(SupportAggMFFactory).FullName);
            configuration.AddPlugInAggregationMultiFunction(config);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _listener = new SupportUpdateListener();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestDifferentReturnTypes()
        {
            // test scalar only
            var fieldsScalar = "c0,c1".Split(',');
            const string eplScalar = "select ss(TheString) as c0, ss(IntPrimitive) as c1 from SupportBean";
            var stmtScalar = _epService.EPAdministrator.CreateEPL(eplScalar);
            stmtScalar.Events += _listener.Update;
    
            var expectedScalar = new Object[][]{new Object[] {"c0", typeof(string), null, null}, new Object[] {"c1", typeof(int), null, null}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalar, stmtScalar.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalar, new Object[]{"E1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalar, new Object[]{"E2", 2});
            stmtScalar.Dispose();
    
            // test scalar-array only
            var fieldsScalarArray = "c0,c1,c2,c3".Split(',');
            const string eplScalarArray = "select " +
                                          "sa(TheString) as c0, " +
                                          "sa(IntPrimitive) as c1, " +
                                          "sa(TheString).AllOf(v => v = 'E1') as c2, " +
                                          "sa(IntPrimitive).AllOf(v => v = 1) as c3 " +
                                          "from SupportBean";
            var stmtScalarArray = _epService.EPAdministrator.CreateEPL(eplScalarArray);
            stmtScalarArray.Events += _listener.Update;
    
            var expectedScalarArray = new Object[][]{
                    new Object[] {"c0", typeof(string[]), null, null}, new Object[] {"c1", typeof(int[]), null, null},
                    new Object[] {"c2", typeof(bool), null, null}, new Object[] {"c3", typeof(bool), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalarArray, stmtScalarArray.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalarArray, new Object[]{
                    new String[] {"E1"}, new int[] {1}, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalarArray, new Object[]{
                    new String[] {"E1", "E2"}, new int[] {1, 2}, false, false});
            stmtScalarArray.Dispose();
    
            // test scalar-collection only
            var fieldsScalarColl = "c2,c3".Split(',');
            const string eplScalarColl = "select " +
                                         "sc(TheString) as c0, " +
                                         "sc(IntPrimitive) as c1, " +
                                         "sc(TheString).AllOf(v => v = 'E1') as c2, " +
                                         "sc(IntPrimitive).AllOf(v => v = 1) as c3 " +
                                         "from SupportBean";
            var stmtScalarColl = _epService.EPAdministrator.CreateEPL(eplScalarColl);
            stmtScalarColl.Events += _listener.Update;
    
            var expectedScalarColl = new Object[][]{
                    new Object[] {"c0", typeof(ICollection<string>), null, null}, new Object[] {"c1", typeof(ICollection<int>), null, null},
                    new Object[] {"c2", typeof(bool), null, null}, new Object[] {"c3", typeof(bool), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalarColl, stmtScalarColl.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1" }, (ICollection<object>)_listener.AssertOneGetNew().Get("c0"));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { 1 }, (ICollection<object>)_listener.AssertOneGetNew().Get("c1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalarColl, new Object[]{true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1", "E2" }, (ICollection<object>)_listener.AssertOneGetNew().Get("c0"));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { 1, 2 }, (ICollection<object>)_listener.AssertOneGetNew().Get("c1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsScalarColl, new Object[]{false, false});
            stmtScalarColl.Dispose();
    
            // test single-event return
            var fieldsSingleEvent = "c0,c1,c2,c3,c4".Split(',');
            const string eplSingleEvent = "select " +
                                          "se1() as c0, " +
                                          "se1().AllOf(v => v.TheString = 'E1') as c1, " +
                                          "se1().AllOf(v => v.IntPrimitive = 1) as c2, " +
                                          "se1().TheString as c3, " +
                                          "se1().IntPrimitive as c4 " +
                                          "from SupportBean";
            var stmtSingleEvent = _epService.EPAdministrator.CreateEPL(eplSingleEvent);
            stmtSingleEvent.Events += _listener.Update;
    
            var expectedSingleEvent = new Object[][]{
                    new Object[] {"c0", typeof(SupportBean), "SupportBean", false},
                    new Object[] {"c1", typeof(Boolean), null, null}, new Object[] {"c2", typeof(Boolean), null, null},
                    new Object[] {"c3", typeof(String), null, null}, new Object[] {"c4", typeof(int?), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedSingleEvent, stmtSingleEvent.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            var eventOne = new SupportBean("E1", 1);
            _epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSingleEvent, new Object[]{eventOne, true, true, "E1", 1});
    
            var eventTwo = new SupportBean("E2", 2);
            _epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsSingleEvent, new Object[]{eventTwo, false, false, "E2", 2});
            stmtSingleEvent.Dispose();
    
            // test single-event return
            var fieldsEnumEvent = "c0,c1,c2".Split(',');
            const string eplEnumEvent = "select " +
                                        "ee() as c0, " +
                                        "ee().AllOf(v => v.TheString = 'E1') as c1, " +
                                        "ee().AllOf(v => v.IntPrimitive = 1) as c2 " +
                                        "from SupportBean";
            var stmtEnumEvent = _epService.EPAdministrator.CreateEPL(eplEnumEvent);
            stmtEnumEvent.Events += _listener.Update;
    
            var expectedEnumEvent = new Object[][]{
                    new Object[] {"c0", typeof(SupportBean[]), "SupportBean", true},
                    new Object[] {"c1", typeof(Boolean), null, null}, new Object[] {"c2", typeof(Boolean), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedEnumEvent, stmtEnumEvent.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            var eventEnumOne = new SupportBean("E1", 1);
            _epService.EPRuntime.SendEvent(eventEnumOne);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsEnumEvent, new Object[]{new SupportBean[] {eventEnumOne}, true, true});
    
            var eventEnumTwo = new SupportBean("E2", 2);
            _epService.EPRuntime.SendEvent(eventEnumTwo);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsEnumEvent, new Object[]{new SupportBean[] {eventEnumOne, eventEnumTwo}, false, false});
        }
    
        [Test]
        public void TestSameProviderGroupedReturnSingleEvent()
        {
            const string epl = "select se1() as c0, se2() as c1 from SupportBean#keepall group by TheString";
    
            // test regular
            SupportAggMFFactory.Reset();
            SupportAggMFHandler.Reset();
            SupportAggMFFactorySingleEvent.Reset();
    
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            RunAssertion(stmt);
    
            // test SODA
            var model = _epService.EPAdministrator.CompileEPL(epl);
            SupportAggMFFactory.Reset();
            SupportAggMFHandler.Reset();
            SupportAggMFFactorySingleEvent.Reset();
            Assert.AreEqual(epl, model.ToEPL());
            var stmtModel = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmtModel.Text);
            RunAssertion(stmtModel);
        }
    
        private void RunAssertion(EPStatement stmt)
        {
            stmt.Events += _listener.Update;
            var fields = "c0,c1".Split(',');
            foreach (var prop in fields) {
                Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyDescriptor(prop).PropertyType);
                Assert.AreEqual(true, stmt.EventType.GetPropertyDescriptor(prop).IsFragment);
                Assert.AreEqual("SupportBean", stmt.EventType.GetFragmentType(prop).FragmentType.Name);
            }
    
            // there should be just 1 factory instance for all of the registered functions for this statement
            Assert.AreEqual(1, SupportAggMFFactory.Factories.Count);
            Assert.AreEqual(2, SupportAggMFFactory.FunctionDeclContexts.Count);
            for (var i = 0; i < 2; i++) {
                PlugInAggregationMultiFunctionDeclarationContext contextDecl = SupportAggMFFactory.FunctionDeclContexts[i];
                Assert.AreEqual(i == 0 ? "se1" : "se2", contextDecl.FunctionName);
                Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, contextDecl.EngineURI);
                Assert.IsFalse(contextDecl.IsDistinct);
                Assert.NotNull(contextDecl.Configuration);
    
                PlugInAggregationMultiFunctionValidationContext contextValid = SupportAggMFFactory.FunctionHandlerValidationContexts[i];
                Assert.AreEqual(i == 0 ? "se1" : "se2", contextValid.FunctionName);
                Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, contextValid.EngineURI);
                Assert.NotNull(contextValid.ParameterExpressions);
                Assert.NotNull(contextValid.AllParameterExpressions);
                Assert.NotNull(contextValid.Config);
                Assert.NotNull(contextValid.EventTypes);
                Assert.NotNull(contextValid.ValidationContext);
                Assert.NotNull(contextValid.StatementName);
            }
            Assert.AreEqual(2, SupportAggMFHandler.ProviderKeys.Count);
            Assert.AreEqual(2, SupportAggMFHandler.Accessors.Count);
            Assert.AreEqual(1, SupportAggMFHandler.ProviderFactories.Count);
            Assert.AreEqual(0, SupportAggMFFactorySingleEvent.StateContexts.Count);
    
            // group 1
            var eventOne = new SupportBean("E1", 1);
            _epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{eventOne, eventOne});
            Assert.AreEqual(1, SupportAggMFFactorySingleEvent.StateContexts.Count);
            PlugInAggregationMultiFunctionStateContext context = SupportAggMFFactorySingleEvent.StateContexts[0];
            Assert.AreEqual("E1", context.GroupKey);
    
            // group 2
            var eventTwo = new SupportBean("E2", 2);
            _epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{eventTwo, eventTwo});
            Assert.AreEqual(2, SupportAggMFFactorySingleEvent.StateContexts.Count);
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestInvalid()
        {
            // add overlapping config with regular agg function
            try {
                _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(SupportAggMFFunc.SCALAR.GetName(), "somefactory");
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'ss' is already defined", ex.Message);
            }
    
            // add overlapping config with regular agg function
            try {
                _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(SupportAggMFFunc.SCALAR.GetName(), "somefactory", "somename");
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'ss' is already defined", ex.Message);
            }
    
            // test over lapping with another multi-function
            var config = new ConfigurationPlugInAggregationMultiFunction("thefunction".Split(','), typeof(SupportAggMFFactory).FullName);
            _epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);
            try {
                var configTwo = new ConfigurationPlugInAggregationMultiFunction("xyz,gmbh,thefunction".Split(','), typeof(TestAggregationFunctionPlugIn).FullName);
                _epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(configTwo);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'thefunction' is already defined", ex.Message);
            }
    
            // test invalid class name
            try {
                var configTwo = new ConfigurationPlugInAggregationMultiFunction("thefunction2".Split(','), "x y z");
                _epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(configTwo);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                Assert.AreEqual("Invalid class name for aggregation multi-function factory 'x y z'", ex.Message);
            }
        }
    }
}
