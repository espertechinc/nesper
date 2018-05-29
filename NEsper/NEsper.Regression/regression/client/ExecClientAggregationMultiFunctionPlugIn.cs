///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.plugin;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientAggregationMultiFunctionPlugIn : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var config = new ConfigurationPlugInAggregationMultiFunction(SupportAggMFFuncExtensions.GetFunctionNames(), typeof(SupportAggMFFactory));
            configuration.AddPlugInAggregationMultiFunction(config);
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDifferentReturnTypes(epService);
            RunAssertionSameProviderGroupedReturnSingleEvent(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionDifferentReturnTypes(EPServiceProvider epService) {
    
            // test scalar only
            string[] fieldsScalar = "c0,c1".Split(',');
            string eplScalar = "select ss(TheString) as c0, ss(IntPrimitive) as c1 from SupportBean";
            EPStatement stmtScalar = epService.EPAdministrator.CreateEPL(eplScalar);
            var listener = new SupportUpdateListener();
            stmtScalar.Events += listener.Update;
    
            var expectedScalar = new object[][]{new object[] {"c0", typeof(string), null, null}, new object[] {"c1", typeof(int), null, null}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalar, stmtScalar.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalar, new object[]{"E1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalar, new object[]{"E2", 2});
            stmtScalar.Dispose();
    
            // test scalar-array only
            string[] fieldsScalarArray = "c0,c1,c2,c3".Split(',');
            string eplScalarArray = "select " +
                    "sa(TheString) as c0, " +
                    "sa(IntPrimitive) as c1, " +
                    "sa(TheString).allOf(v => v = 'E1') as c2, " +
                    "sa(IntPrimitive).allOf(v => v = 1) as c3 " +
                    "from SupportBean";
            EPStatement stmtScalarArray = epService.EPAdministrator.CreateEPL(eplScalarArray);
            stmtScalarArray.Events += listener.Update;
    
            var expectedScalarArray = new []{
                new object[] {"c0", typeof(string[]), null, null},
                new object[] {"c1", typeof(int[]), null, null},
                new object[] {"c2", typeof(bool), null, null},
                new object[] {"c3", typeof(bool), null, null},
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalarArray, stmtScalarArray.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalarArray, new object[]{
                    new string[]{"E1"}, new int[]{1}, true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalarArray, new object[]{
                    new string[]{"E1", "E2"}, new int[]{1, 2}, false, false});
            stmtScalarArray.Dispose();
    
            // test scalar-collection only
            string[] fieldsScalarColl = "c2,c3".Split(',');
            string eplScalarColl = "select " +
                    "sc(TheString) as c0, " +
                    "sc(IntPrimitive) as c1, " +
                    "sc(TheString).allOf(v => v = 'E1') as c2, " +
                    "sc(IntPrimitive).allOf(v => v = 1) as c3 " +
                    "from SupportBean";
            EPStatement stmtScalarColl = epService.EPAdministrator.CreateEPL(eplScalarColl);
            stmtScalarColl.Events += listener.Update;
    
            var expectedScalarColl = new object[][]{
                new object[] {"c0", typeof(ICollection<string>), null, null},
                new object[] {"c1", typeof(ICollection<int>), null, null},
                new object[] {"c2", typeof(bool), null, null},
                new object[] {"c3", typeof(bool), null, null},
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedScalarColl, stmtScalarColl.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"E1"}, listener.AssertOneGetNew().Get("c0").Unwrap<string>());
            EPAssertionUtil.AssertEqualsExactOrder(new int[]{1}, listener.AssertOneGetNew().Get("c1").Unwrap<int>());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalarColl, new object[]{true, true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"E1", "E2"}, listener.AssertOneGetNew().Get("c0").Unwrap<string>());
            EPAssertionUtil.AssertEqualsExactOrder(new int[]{1, 2}, listener.AssertOneGetNew().Get("c1").Unwrap<int>());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsScalarColl, new object[]{false, false});
            stmtScalarColl.Dispose();
    
            // test single-event return
            string[] fieldsSingleEvent = "c0,c1,c2,c3,c4".Split(',');
            string eplSingleEvent = "select " +
                    "se1() as c0, " +
                    "se1().allOf(v => v.TheString = 'E1') as c1, " +
                    "se1().allOf(v => v.IntPrimitive = 1) as c2, " +
                    "se1().TheString as c3, " +
                    "se1().IntPrimitive as c4 " +
                    "from SupportBean";
            EPStatement stmtSingleEvent = epService.EPAdministrator.CreateEPL(eplSingleEvent);
            stmtSingleEvent.Events += listener.Update;
    
            var expectedSingleEvent = new object[][]{
                new object[] {"c0", typeof(SupportBean), "SupportBean", false},
                new object[] {"c1", typeof(bool), null, null},
                new object[] {"c2", typeof(bool), null, null},
                new object[] {"c3", typeof(string), null, null},
                new object[] {"c4", typeof(int?), null, null},
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedSingleEvent, stmtSingleEvent.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            var eventOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSingleEvent, new object[]{eventOne, true, true, "E1", 1});
    
            var eventTwo = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSingleEvent, new object[]{eventTwo, false, false, "E2", 2});
            stmtSingleEvent.Dispose();
    
            // test single-event return
            string[] fieldsEnumEvent = "c0,c1,c2".Split(',');
            string eplEnumEvent = "select " +
                    "ee() as c0, " +
                    "ee().allOf(v => v.TheString = 'E1') as c1, " +
                    "ee().allOf(v => v.IntPrimitive = 1) as c2 " +
                    "from SupportBean";
            EPStatement stmtEnumEvent = epService.EPAdministrator.CreateEPL(eplEnumEvent);
            stmtEnumEvent.Events += listener.Update;
    
            var expectedEnumEvent = new object[][]{
                new object[] {"c0", typeof(SupportBean[]), "SupportBean", true},
                new object[] {"c1", typeof(bool), null, null},
                new object[] {"c2", typeof(bool), null, null}
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedEnumEvent, stmtEnumEvent.EventType, SupportEventTypeAssertionEnumExtensions.GetSetWithFragment());
    
            var eventEnumOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(eventEnumOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsEnumEvent, new object[]{new SupportBean[]{eventEnumOne}, true, true});
    
            var eventEnumTwo = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(eventEnumTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsEnumEvent, new object[]{new SupportBean[]{eventEnumOne, eventEnumTwo}, false, false});
    
            stmtEnumEvent.Dispose();
        }
    
        private void RunAssertionSameProviderGroupedReturnSingleEvent(EPServiceProvider epService) {
            string epl = "select se1() as c0, se2() as c1 from SupportBean#keepall group by TheString";
    
            // test regular
            SupportAggMFFactory.Reset();
            SupportAggMFHandler.Reset();
            SupportAggMFFactorySingleEvent.Reset();
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            TryAssertion(epService, stmt);
    
            // test SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            SupportAggMFFactory.Reset();
            SupportAggMFHandler.Reset();
            SupportAggMFFactorySingleEvent.Reset();
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtModel = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmtModel.Text);
            TryAssertion(epService, stmtModel);
        }
    
        private void TryAssertion(EPServiceProvider epService, EPStatement stmt) {
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "c0,c1".Split(',');
            foreach (string prop in fields) {
                Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyDescriptor(prop).PropertyType);
                Assert.AreEqual(true, stmt.EventType.GetPropertyDescriptor(prop).IsFragment);
                Assert.AreEqual("SupportBean", stmt.EventType.GetFragmentType(prop).FragmentType.Name);
            }
    
            // there should be just 1 factory instance for all of the registered functions for this statement
            Assert.AreEqual(1, SupportAggMFFactory.Factories.Count);
            Assert.AreEqual(2, SupportAggMFFactory.FunctionDeclContexts.Count);
            for (int i = 0; i < 2; i++) {
                PlugInAggregationMultiFunctionDeclarationContext contextDecl = SupportAggMFFactory.FunctionDeclContexts[i];
                Assert.AreEqual(i == 0 ? "se1" : "se2", contextDecl.FunctionName);
                Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, contextDecl.EngineURI);
                Assert.IsFalse(contextDecl.IsDistinct);
                Assert.IsNotNull(contextDecl.Configuration);
    
                PlugInAggregationMultiFunctionValidationContext contextValid = SupportAggMFFactory.FunctionHandlerValidationContexts[i];
                Assert.AreEqual(i == 0 ? "se1" : "se2", contextValid.FunctionName);
                Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, contextValid.EngineURI);
                Assert.IsNotNull(contextValid.ParameterExpressions);
                Assert.IsNotNull(contextValid.AllParameterExpressions);
                Assert.IsNotNull(contextValid.Config);
                Assert.IsNotNull(contextValid.EventTypes);
                Assert.IsNotNull(contextValid.ValidationContext);
                Assert.IsNotNull(contextValid.StatementName);
            }
            Assert.AreEqual(2, SupportAggMFHandler.ProviderKeys.Count);
            Assert.AreEqual(2, SupportAggMFHandler.Accessors.Count);
            Assert.AreEqual(1, SupportAggMFHandler.ProviderFactories.Count);
            Assert.AreEqual(0, SupportAggMFFactorySingleEvent.StateContexts.Count);
    
            // group 1
            var eventOne = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(eventOne);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{eventOne, eventOne});
            Assert.AreEqual(1, SupportAggMFFactorySingleEvent.StateContexts.Count);
            PlugInAggregationMultiFunctionStateContext context = SupportAggMFFactorySingleEvent.StateContexts[0];
            Assert.AreEqual("E1", context.GroupKey);
    
            // group 2
            var eventTwo = new SupportBean("E2", 2);
            epService.EPRuntime.SendEvent(eventTwo);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{eventTwo, eventTwo});
            Assert.AreEqual(2, SupportAggMFFactorySingleEvent.StateContexts.Count);
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // add overlapping config with regular agg function
            try {
                epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory(SupportAggMFFunc.SCALAR.GetName(), "somefactory");
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'ss' is already defined", ex.Message);
            }
    
            // add overlapping config with regular agg function
            try {
                epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                    SupportAggMFFunc.SCALAR.GetName(), "somefactory", "somename");
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'ss' is already defined", ex.Message);
            }
    
            // test over lapping with another multi-function
            var config = new ConfigurationPlugInAggregationMultiFunction("thefunction".Split(','), typeof(SupportAggMFFactory).FullName);
            epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(config);
            try {
                var configTwo = new ConfigurationPlugInAggregationMultiFunction("xyz,gmbh,thefunction".Split(','), typeof(ExecClientAggregationFunctionPlugIn));
                epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(configTwo);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Aggregation multi-function by name 'thefunction' is already defined", ex.Message);
            }
    
            // test invalid class name
            try {
                var configTwo = new ConfigurationPlugInAggregationMultiFunction("thefunction2".Split(','), "x y z");
                epService.EPAdministrator.Configuration.AddPlugInAggregationMultiFunction(configTwo);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Invalid class name for aggregation multi-function factory 'x y z'", ex.Message);
            }
        }
    }
} // end of namespace
