///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    public class ExecVariablesEventTyped : RegressionExecution {
        private NonSerializable nonSerializable;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("TypeS0", typeof(SupportBean_S0));
            configuration.AddEventType("TypeS2", typeof(SupportBean_S2));
    
            configuration.AddVariable("vars0_A", "TypeS0", new SupportBean_S0(10));
            configuration.AddVariable("vars1_A", typeof(SupportBean_S1).FullName, new SupportBean_S1(20));
            configuration.AddVariable("varsobj1", typeof(object).Name, 123);
    
            nonSerializable = new NonSerializable("abc");
            configuration.AddVariable("myNonSerializable", typeof(NonSerializable), nonSerializable);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionConfig(epService);
            RunAssertionEventTypedSetProp(epService);
            RunAssertionEventTyped(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            try {
                epService.EPRuntime.SetVariableValue("vars0_A", new SupportBean_S1(1));
                Assert.Fail();
            } catch (VariableValueException ex) {
                Assert.AreEqual("Variable 'vars0_A' of declared event type 'TypeS0' underlying type '" + typeof(SupportBean_S0).GetCleanName() + "' cannot be assigned a value of type '" + typeof(SupportBean_S1).GetCleanName() + "'", ex.Message);
            }

            TryInvalid(epService, "on TypeS0 arrival set vars1_A = arrival",
                string.Format(
                    "Error starting statement: Error in variable assignment: Variable 'vars1_A' of declared event type '{0}' underlying type '{0}' cannot be assigned a value of type '{1}'",
                    typeof(SupportBean_S1).GetCleanName(),
                    typeof(SupportBean_S0).GetCleanName()));

            TryInvalid(epService, "on TypeS0 arrival set vars0_A = 1",
                string.Format(
                    "Error starting statement: Error in variable assignment: Variable 'vars0_A' of declared event type 'TypeS0' underlying type '{0}' cannot be assigned a value of type '{1}'",
                    typeof(SupportBean_S0).GetCleanName(),
                    typeof(int).GetCleanName()));
        }
    
        private void RunAssertionConfig(EPServiceProvider epService) {
            Assert.AreEqual(10, ((SupportBean_S0) epService.EPRuntime.GetVariableValue("vars0_A")).Id);
            Assert.AreEqual(20, ((SupportBean_S1) epService.EPRuntime.GetVariableValue("vars1_A")).Id);
            Assert.AreEqual(123, epService.EPRuntime.GetVariableValue("varsobj1"));
            Assert.AreSame(nonSerializable, epService.EPRuntime.GetVariableValue("myNonSerializable"));
    
            epService.EPAdministrator.Configuration.AddVariable("vars2", "TypeS2", new SupportBean_S2(30));
            epService.EPAdministrator.Configuration.AddVariable("vars3", typeof(SupportBean_S3), new SupportBean_S3(40));
            epService.EPAdministrator.Configuration.AddVariable("varsobj2", typeof(object), "ABC");
    
            Assert.AreEqual(30, ((SupportBean_S2) epService.EPRuntime.GetVariableValue("vars2")).Id);
            Assert.AreEqual(40, ((SupportBean_S3) epService.EPRuntime.GetVariableValue("vars3")).Id);
            Assert.AreEqual("ABC", epService.EPRuntime.GetVariableValue("varsobj2"));
    
            epService.EPAdministrator.CreateEPL("create variable object varsobj3=222");
            Assert.AreEqual(222, epService.EPRuntime.GetVariableValue("varsobj3"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventTypedSetProp(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            var listenerSet = new SupportUpdateListener();
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.CreateEPL("create variable SupportBean varbean");
    
            string[] fields = "varbean.TheString,varbean.IntPrimitive,varbean.get_TheString()".Split(',');
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select varbean.TheString,varbean.IntPrimitive,varbean.get_TheString() from S0");
            stmtSelect.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null});
    
            EPStatement stmtSet = epService.EPAdministrator.CreateEPL("on A set varbean.TheString = 'A', varbean.IntPrimitive = 1");
            stmtSet.Events += listenerSet.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            listenerSet.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null});
    
            var setBean = new SupportBean();
            epService.EPRuntime.SetVariableValue("varbean", setBean);
            epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A", 1, "A"});
            Assert.AreNotSame(setBean, epService.EPRuntime.GetVariableValue("varbean"));
            Assert.AreEqual(1, ((SupportBean) epService.EPRuntime.GetVariableValue("varbean")).IntPrimitive);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), "varbean.TheString,varbean.IntPrimitive".Split(','), new object[]{"A", 1});
            EPAssertionUtil.AssertProps(stmtSet.First(), "varbean.TheString,varbean.IntPrimitive".Split(','), new object[]{"A", 1});
    
            // test self evaluate
            stmtSet.Dispose();
            stmtSet = epService.EPAdministrator.CreateEPL("on A set varbean.TheString = A.id, varbean.TheString = '>'||varbean.TheString||'<'");
            stmtSet.Events += listenerSet.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            Assert.AreEqual(">E3<", ((SupportBean) epService.EPRuntime.GetVariableValue("varbean")).TheString);
    
            // test widen
            stmtSet.Dispose();
            stmtSet = epService.EPAdministrator.CreateEPL("on A set varbean.LongPrimitive = 1");
            stmtSet.Events += listenerSet.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("E4"));
            Assert.AreEqual(1, ((SupportBean) epService.EPRuntime.GetVariableValue("varbean")).LongPrimitive);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionEventTyped(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.Configuration.AddEventType("S0Type", typeof(SupportBean_S0));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            // assign to properties of a variable
            // assign: configuration runtime + config static
            // SODA
            epService.EPAdministrator.CreateEPL("create variable Object varobject = null");
            epService.EPAdministrator.CreateEPL("create variable " + typeof(SupportBean_A).FullName + " varbean = null");
            epService.EPAdministrator.CreateEPL("create variable S0Type vartype = null");
    
            string[] fields = "varobject,varbean,varbean.id,vartype,vartype.id".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select varobject, varbean, varbean.id, vartype, vartype.id from SupportBean");
            stmt.Events += listener.Update;
    
            // test null
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
    
            // test objects
            var a1objectOne = new SupportBean_A("A1");
            var s0objectOne = new SupportBean_S0(1);
            epService.EPRuntime.SetVariableValue("varobject", "abc");
            epService.EPRuntime.SetVariableValue("varbean", a1objectOne);
            epService.EPRuntime.SetVariableValue("vartype", s0objectOne);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"abc", a1objectOne, a1objectOne.Id, s0objectOne, s0objectOne.Id});
    
            // test on-set for Object and EventType
            string[] fieldsTop = "varobject,vartype,varbean".Split(',');
            EPStatement stmtSet = epService.EPAdministrator.CreateEPL("on S0Type(p00='X') arrival set varobject =1, vartype=arrival, varbean=null");
            stmtSet.Events += listener.Update;
    
            var s0objectTwo = new SupportBean_S0(2, "X");
            epService.EPRuntime.SendEvent(s0objectTwo);
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("varobject"));
            Assert.AreEqual(s0objectTwo, epService.EPRuntime.GetVariableValue("vartype"));
            Assert.AreEqual(s0objectTwo, epService.EPRuntime.GetVariableValue(Collections.SingletonSet("vartype")).Get("vartype"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTop, new object[]{1, s0objectTwo, null});
            EPAssertionUtil.AssertProps(stmtSet.First(), fieldsTop, new object[]{1, s0objectTwo, null});
    
            // set via API to null
            var newValues = new Dictionary<string, object>();
            newValues.Put("varobject", null);
            newValues.Put("vartype", null);
            newValues.Put("varbean", null);
            epService.EPRuntime.SetVariableValue(newValues);
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null, null, null});
    
            // set via API to values
            newValues.Put("varobject", 10L);
            newValues.Put("vartype", s0objectTwo);
            newValues.Put("varbean", a1objectOne);
            epService.EPRuntime.SetVariableValue(newValues);
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10L, a1objectOne, a1objectOne.Id, s0objectTwo, s0objectTwo.Id});
    
            // test on-set for Bean class
            stmtSet = epService.EPAdministrator.CreateEPL("on " + typeof(SupportBean_A).FullName + "(id='Y') arrival set varobject =null, vartype=null, varbean=arrival");
            stmtSet.Events += listener.Update;
            var a1objectTwo = new SupportBean_A("Y");
            epService.EPRuntime.SendEvent(new SupportBean_A("Y"));
            Assert.AreEqual(null, epService.EPRuntime.GetVariableValue("varobject"));
            Assert.AreEqual(null, epService.EPRuntime.GetVariableValue("vartype"));
            Assert.AreEqual(a1objectTwo, epService.EPRuntime.GetVariableValue(Collections.SingletonSet("varbean")).Get("varbean"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsTop, new object[]{null, null, a1objectTwo});
            EPAssertionUtil.AssertProps(stmtSet.First(), fieldsTop, new object[]{null, null, a1objectTwo});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class NonSerializable {
            private readonly string myString;
    
            public NonSerializable(string myString) {
                this.myString = myString;
            }
    
            public string GetMyString() {
                return myString;
            }
        }
    }
} // end of namespace
