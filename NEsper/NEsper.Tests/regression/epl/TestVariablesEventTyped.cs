///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestVariablesEventTyped 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerSet;
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _listenerSet = null;
        }
    
        [Test]
        public void TestInvalid()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("TypeS0", typeof(SupportBean_S0));
            config.AddVariable("vars0", "TypeS0", null);
            config.AddVariable("vars1", typeof(SupportBean_S1).FullName, null);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            try {
                _epService.EPRuntime.SetVariableValue("vars0", new SupportBean_S1(1));
                Assert.Fail();
            }
            catch (VariableValueException ex) {
                Assert.AreEqual("Variable 'vars0' of declared event type 'TypeS0' underlying type 'com.espertech.esper.support.bean.SupportBean_S0' cannot be assigned a value of type 'com.espertech.esper.support.bean.SupportBean_S1'", ex.Message);
            }
            
            TryInvalid(_epService, "on TypeS0 arrival set vars1 = arrival",
                       "Error starting statement: Error in variable assignment: Variable 'vars1' of declared event type 'SupportBean_S1' underlying type 'com.espertech.esper.support.bean.SupportBean_S1' cannot be assigned a value of type 'com.espertech.esper.support.bean.SupportBean_S0' [on TypeS0 arrival set vars1 = arrival]");
    
            TryInvalid(_epService, "on TypeS0 arrival set vars0 = 1",
                       "Error starting statement: Error in variable assignment: Variable 'vars0' of declared event type 'TypeS0' underlying type 'com.espertech.esper.support.bean.SupportBean_S0' cannot be assigned a value of type '" + Name.Of<int>() + "' [on TypeS0 arrival set vars0 = 1]");
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestConfig()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("TypeS0", typeof(SupportBean_S0));
            config.AddEventType("TypeS2", typeof(SupportBean_S2));
    
            config.AddVariable("vars0", "TypeS0", new SupportBean_S0(10));
            config.AddVariable("vars1", typeof(SupportBean_S1).FullName, new SupportBean_S1(20));
            config.AddVariable("varsobj1", typeof(Object).FullName, 123);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            Assert.AreEqual(10, ((SupportBean_S0) _epService.EPRuntime.GetVariableValue("vars0")).Id);
            Assert.AreEqual(20, ((SupportBean_S1) _epService.EPRuntime.GetVariableValue("vars1")).Id);
            Assert.AreEqual(123, _epService.EPRuntime.GetVariableValue("varsobj1"));
    
            _epService.EPAdministrator.Configuration.AddVariable("vars2", "TypeS2", new SupportBean_S2(30));
            _epService.EPAdministrator.Configuration.AddVariable("vars3", typeof(SupportBean_S3), new SupportBean_S3(40));
            _epService.EPAdministrator.Configuration.AddVariable("varsobj2", typeof(Object), "ABC");
    
            Assert.AreEqual(30, ((SupportBean_S2) _epService.EPRuntime.GetVariableValue("vars2")).Id);
            Assert.AreEqual(40, ((SupportBean_S3) _epService.EPRuntime.GetVariableValue("vars3")).Id);
            Assert.AreEqual("ABC", _epService.EPRuntime.GetVariableValue("varsobj2"));
            
            _epService.EPAdministrator.CreateEPL("create variable object varsobj3=222");
            Assert.AreEqual(222, _epService.EPRuntime.GetVariableValue("varsobj3"));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestEventTypedSetProp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listenerSet = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            _epService.EPAdministrator.CreateEPL("create variable SupportBean varbean");

            var fields = "varbean.TheString,varbean.IntPrimitive,varbean.GetTheString()".Split(',');
            var stmtSelect = _epService.EPAdministrator.CreateEPL("select varbean.TheString,varbean.IntPrimitive,varbean.GetTheString() from S0");
            stmtSelect.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null});
    
            var stmtSet = _epService.EPAdministrator.CreateEPL("on A set varbean.TheString = 'A', varbean.IntPrimitive = 1");
            stmtSet.Events += _listenerSet.Update;
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            _listenerSet.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null});
    
            var setBean = new SupportBean();
            _epService.EPRuntime.SetVariableValue("varbean", setBean);
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"A", 1, "A"});
            Assert.AreNotSame(setBean, _epService.EPRuntime.GetVariableValue("varbean"));
            Assert.AreEqual(1, ((SupportBean) _epService.EPRuntime.GetVariableValue("varbean")).IntPrimitive);
            EPAssertionUtil.AssertProps(_listenerSet.AssertOneGetNewAndReset(), "varbean.TheString,varbean.IntPrimitive".Split(','), new Object[]{"A", 1});
            EPAssertionUtil.AssertProps(stmtSet.First(), "varbean.TheString,varbean.IntPrimitive".Split(','), new Object[]{"A", 1});
    
            // test self evaluate
            stmtSet.Dispose();
            stmtSet = _epService.EPAdministrator.CreateEPL("on A set varbean.TheString = A.id, varbean.TheString = '>'||varbean.TheString||'<'");
            stmtSet.Events += _listenerSet.Update;
            _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            Assert.AreEqual(">E3<", ((SupportBean) _epService.EPRuntime.GetVariableValue("varbean")).TheString);
    
            // test widen
            stmtSet.Dispose();
            stmtSet = _epService.EPAdministrator.CreateEPL("on A set varbean.LongPrimitive = 1");
            stmtSet.Events += _listenerSet.Update;
            _epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
            Assert.AreEqual(1, ((SupportBean) _epService.EPRuntime.GetVariableValue("varbean")).LongPrimitive);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestEventTyped()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _listenerSet = new SupportUpdateListener();
    
            _epService.EPAdministrator.Configuration.AddEventType("S0Type", typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // assign to properties of a variable
            // assign: configuration runtime + config static
            // SODA
            _epService.EPAdministrator.CreateEPL("create variable Object varobject = null");
            _epService.EPAdministrator.CreateEPL("create variable " + typeof(SupportBean_A).FullName + " varbean = null");
            _epService.EPAdministrator.CreateEPL("create variable S0Type vartype = null");
    
            var fields = "varobject,varbean,varbean.id,vartype,vartype.id".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("select varobject, varbean, varbean.id, vartype, vartype.id from SupportBean");
            stmt.Events += _listener.Update;
    
            // test null
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null});
    
            // test objects
            var a1objectOne = new SupportBean_A("A1");
            var s0objectOne = new SupportBean_S0(1);
            _epService.EPRuntime.SetVariableValue("varobject", "abc");
            _epService.EPRuntime.SetVariableValue("varbean", a1objectOne);
            _epService.EPRuntime.SetVariableValue("vartype", s0objectOne);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"abc", a1objectOne, a1objectOne.Id, s0objectOne, s0objectOne.Id});
    
            // test on-set for Object and EventType
            var fieldsTop = "varobject,vartype,varbean".Split(',');
            var stmtSet = _epService.EPAdministrator.CreateEPL("on S0Type(p00='X') arrival set varobject=1, vartype=arrival, varbean=null");
            stmtSet.Events += _listener.Update;
    
            var s0objectTwo = new SupportBean_S0(2, "X");
            _epService.EPRuntime.SendEvent(s0objectTwo);
            Assert.AreEqual(1, _epService.EPRuntime.GetVariableValue("varobject"));
            Assert.AreEqual(s0objectTwo, _epService.EPRuntime.GetVariableValue("vartype"));
            Assert.AreEqual(s0objectTwo, _epService.EPRuntime.GetVariableValue(Collections.SingletonList("vartype")).Get("vartype"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTop, new Object[]{1, s0objectTwo, null});
            EPAssertionUtil.AssertProps(stmtSet.First(), fieldsTop, new Object[]{1, s0objectTwo, null});
    
            // set via API to null
            IDictionary<String,Object> newValues = new Dictionary<String, Object>();
            newValues.Put("varobject", null);
            newValues.Put("vartype", null);
            newValues.Put("varbean", null);
            _epService.EPRuntime.SetVariableValue(newValues);
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null, null, null, null, null});
    
            // set via API to values
            newValues.Put("varobject", 10L);
            newValues.Put("vartype", s0objectTwo);
            newValues.Put("varbean", a1objectOne);
            _epService.EPRuntime.SetVariableValue(newValues);
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{10L, a1objectOne, a1objectOne.Id, s0objectTwo, s0objectTwo.Id});
    
            // test on-set for Bean class
            stmtSet = _epService.EPAdministrator.CreateEPL("on " + typeof(SupportBean_A).FullName + "(id='Y') arrival set varobject=null, vartype=null, varbean=arrival");
            stmtSet.Events += _listener.Update;
            var a1objectTwo = new SupportBean_A("Y");
            _epService.EPRuntime.SendEvent(new SupportBean_A("Y"));
            Assert.AreEqual(null, _epService.EPRuntime.GetVariableValue("varobject"));
            Assert.AreEqual(null, _epService.EPRuntime.GetVariableValue("vartype"));
            Assert.AreEqual(a1objectTwo, _epService.EPRuntime.GetVariableValue(Collections.SingletonList("varbean")).Get("varbean"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsTop, new Object[]{null, null, a1objectTwo});
            EPAssertionUtil.AssertProps(stmtSet.First(), fieldsTop, new Object[]{null, null, a1objectTwo});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void TryInvalid(EPServiceProvider engine, String epl, String message)
        {
            try {
                engine.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
