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
using System.Threading;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    using Map = IDictionary<string, object>;

    public class ExecVariables : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
            configuration.AddVariable("MYCONST_THREE", "bool", true, true);
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
    
            configuration.AddVariable("papi_1", typeof(string), "begin");
            configuration.AddVariable("papi_2", typeof(bool), true);
            configuration.AddVariable("papi_3", typeof(string), "value");
    
            var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<esper-configuration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"../esper-configuration-6-0.xsd\">" +
                    "<variable name=\"p_1\" type=\"string\" />" +
                    "<variable name=\"p_2\" type=\"bool\" initialization-value=\"true\"/>" +
                    "<variable name=\"p_3\" type=\"long\" initialization-value=\"10\"/>" +
                    "<variable name=\"p_4\" type=\"double\" initialization-value=\"11.1d\"/>" +
                    "</esper-configuration>";

            var configDoc = new XmlDocument();
            configDoc.LoadXml(xml);

            configuration.Configure(configDoc);
        }

        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionDotVariableSeparateThread(epService);
            RunAssertionInvokeMethod(epService);
            RunAssertionConstantVariable(epService);
            RunAssertionVariableEPRuntime(epService);
            RunAssertionSetSubquery(epService);
            RunAssertionVariableInFilterBoolean(epService);
            RunAssertionVariableInFilter(epService);
            RunAssertionAssignmentOrderNoDup(epService);
            RunAssertionAssignmentOrderDup(epService);
            RunAssertionObjectModel(epService);
            RunAssertionCompile(epService);
            RunAssertionRuntimeConfig(epService);
            RunAssertionRuntimeOrderMultiple(epService);
            RunAssertionEngineConfigAPI(epService);
            RunAssertionEngineConfigXML(epService);
            RunAssertionCoercion(epService);
            RunAssertionInvalidSet(epService);
            RunAssertionInvalidInitialization(epService);
        }
    
        private void RunAssertionDotVariableSeparateThread(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddVariable("mySimpleVariableService", typeof(MySimpleVariableService), null);
            epService.EPRuntime.SetVariableValue("mySimpleVariableService", new MySimpleVariableService());
    
            var epStatement = epService.EPAdministrator.CreateEPL(
                "select mySimpleVariableService.DoSomething() as c0 from SupportBean");
    
            var latch = new CountDownLatch(1);
            var values = new List<string>();
            epStatement.Subscriber = new Action<IDictionary<string, object>>(
                @event =>
                {
                    var value = (string) @event.Get("c0");
                    values.Add(value);
                    latch.CountDown();
                });
    
            var executorService = Executors.NewSingleThreadExecutor();
            executorService.Submit(() => {
                epService.EPRuntime.SendEvent(new SupportBean());
            });

            latch.Await();
            executorService.Shutdown();
    
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("hello", values[0]);
    
            epStatement.Dispose();
        }
    
        private void RunAssertionInvokeMethod(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport(typeof(MySimpleVariableServiceFactory));
            epService.EPAdministrator.Configuration.AddImport(typeof(MySimpleVariableService));
    
            // declared via EPL
            epService.EPAdministrator.CreateEPL("create constant variable MySimpleVariableService myService = MySimpleVariableServiceFactory.MakeService()");
    
            // added via runtime config
            epService.EPAdministrator.Configuration.AddVariable("myRuntimeInitService", typeof(MySimpleVariableService), MySimpleVariableServiceFactory.MakeService());
    
            // exercise
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select " +
                    "myService.DoSomething() as c0, " +
                    "myRuntimeInitService.DoSomething() as c1 " +
                    "from SupportBean").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{"hello", "hello"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionConstantVariable(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create const variable int MYCONST = 10");
    
            TryOperator(epService, "MYCONST = IntBoxed", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
    
            TryOperator(epService, "MYCONST > IntBoxed", new[] {new object[] {11, false}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "MYCONST >= IntBoxed", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "MYCONST < IntBoxed", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "MYCONST <= IntBoxed", new[] {new object[] {11, true}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "IntBoxed < MYCONST", new[] {new object[] {11, false}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed <= MYCONST", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed > MYCONST", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "IntBoxed >= MYCONST", new[] {new object[] {11, true}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "IntBoxed in (MYCONST)", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
            TryOperator(epService, "IntBoxed between MYCONST and MYCONST", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, false}});
    
            TryOperator(epService, "MYCONST != IntBoxed", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
            TryOperator(epService, "IntBoxed != MYCONST", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, false}});
    
            TryOperator(epService, "IntBoxed not in (MYCONST)", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
            TryOperator(epService, "IntBoxed not between MYCONST and MYCONST", new[] {new object[] {11, true}, new object[] {10, false}, new object[] {9, true}, new object[] {8, true}});
    
            TryOperator(epService, "MYCONST is IntBoxed", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
            TryOperator(epService, "IntBoxed is MYCONST", new[] {new object[] {10, true}, new object[] {9, false}, new object[] {null, false}});
    
            TryOperator(epService, "MYCONST is not IntBoxed", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
            TryOperator(epService, "IntBoxed is not MYCONST", new[] {new object[] {10, false}, new object[] {9, true}, new object[] {null, true}});
    
            // try coercion
            TryOperator(epService, "MYCONST = ShortBoxed", new[] {new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}});
            TryOperator(epService, "ShortBoxed = MYCONST", new[] {new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {null, false}});
    
            TryOperator(epService, "MYCONST > ShortBoxed", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, false}, new object[] {(short) 9, true}, new object[] {(short) 8, true}});
            TryOperator(epService, "ShortBoxed < MYCONST", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, false}, new object[] {(short) 9, true}, new object[] {(short) 8, true}});
    
            TryOperator(epService, "ShortBoxed in (MYCONST)", new[] {new object[] {(short) 11, false}, new object[] {(short) 10, true}, new object[] {(short) 9, false}, new object[] {(short) 8, false}});
    
            // test SODA
            var epl = "create constant variable int MYCONST = 10";
            epService.EPAdministrator.DestroyAllStatements();
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            var stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl, stmt.Text);
    
            // test invalid
            TryInvalidSet(epService, "on SupportBean set MYCONST = 10",
                    "Error starting statement: Variable by name 'MYCONST' is declared constant and may not be set [on SupportBean set MYCONST = 10]");
            TryInvalidSet(epService, "select * from SupportBean output when true then set MYCONST=1",
                    "Error starting statement: Error in the output rate limiting clause: Variable by name 'MYCONST' is declared constant and may not be set [select * from SupportBean output when true then set MYCONST=1]");
    
            // assure no update via API
            TryInvalidSetConstant(epService, "MYCONST", 1);
    
            // add constant variable via runtime API
            epService.EPAdministrator.Configuration.AddVariable("MYCONST_TWO", "string", null, true);
            TryInvalidSetConstant(epService, "MYCONST_TWO", "dummy");
            TryInvalidSetConstant(epService, "MYCONST_THREE", false);
    
            // try ESPER-653
            var stmtDate = epService.EPAdministrator.CreateEPL(
                "create constant variable System.DateTime START_TIME = com.espertech.esper.compat.DateTimeHelper.GetCurrentTime()");
            var value = stmtDate.First().Get("START_TIME");
            Assert.IsNotNull(value);
    
            // test array constant
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("create constant variable string[] var_strings = {'E1', 'E2'}");
            var stmtArrayVar = epService.EPAdministrator.CreateEPL("select var_strings from SupportBean");
            Assert.AreEqual(typeof(string[]), stmtArrayVar.EventType.GetPropertyType("var_strings"));
            TryAssertionArrayVar(epService, "var_strings");
            epService.EPAdministrator.Configuration.AddVariable("varcoll", "string[]", new[]{"E1", "E2"}, true);
    
            TryOperator(epService, "IntBoxed in (10, 8)", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, true}});
    
            epService.EPAdministrator.CreateEPL("create constant variable int [ ] var_ints = {8, 10}");
            TryOperator(epService, "IntBoxed in (var_ints)", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, false}, new object[] {8, true}});
    
            epService.EPAdministrator.CreateEPL("create constant variable int[]  var_intstwo = {9}");
            TryOperator(epService, "IntBoxed in (var_ints, var_intstwo)", new[] {new object[] {11, false}, new object[] {10, true}, new object[] {9, true}, new object[] {8, true}});
    
            SupportMessageAssertUtil.TryInvalid(epService, "create constant variable SupportBean[] var_beans",
                    "Error starting statement: Cannot create variable: Cannot create variable 'var_beans', type 'SupportBean' cannot be declared as an array type [create constant variable SupportBean[] var_beans]");
    
            // test array of primitives
            var stmtArrayOne = epService.EPAdministrator.CreateEPL("create variable byte[] myBytesBoxed");
            var expectedType = new[] {new object[] {"myBytesBoxed", typeof(byte?[])}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtArrayOne.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
            var stmtArrayTwo = epService.EPAdministrator.CreateEPL("create variable byte[primitive] myBytesPrimitive");
            expectedType = new[] {new object[] {"myBytesPrimitive", typeof(byte[])}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtArrayTwo.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // test enum constant
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));
            epService.EPAdministrator.CreateEPL("create constant variable SupportEnum var_enumone = SupportEnum.ENUM_VALUE_2");
            TryOperator(epService, "var_enumone = enumValue", new[] {new object[] {SupportEnum.ENUM_VALUE_3, false}, new object[] {SupportEnum.ENUM_VALUE_2, true}, new object[] {SupportEnum.ENUM_VALUE_1, false}});
    
            epService.EPAdministrator.CreateEPL("create constant variable SupportEnum[] var_enumarr = {SupportEnum.ENUM_VALUE_2, SupportEnum.ENUM_VALUE_1}");
            TryOperator(epService, "enumValue in (var_enumarr, var_enumone)", new[] {new object[] {SupportEnum.ENUM_VALUE_3, false}, new object[] {SupportEnum.ENUM_VALUE_2, true}, new object[] {SupportEnum.ENUM_VALUE_1, true}});
    
            epService.EPAdministrator.CreateEPL("create variable SupportEnum var_enumtwo = SupportEnum.ENUM_VALUE_2");
            epService.EPAdministrator.CreateEPL("on SupportBean set var_enumtwo = enumValue");
    
            epService.EPAdministrator.Configuration.AddVariable("supportEnum", typeof(SupportEnum).Name, SupportEnum.ENUM_VALUE_1);
            epService.EPAdministrator.Configuration.AddVariable("enumWithOverride", typeof(MyEnumWithOverride).Name, MyEnumWithOverride.LONG);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionArrayVar(EPServiceProvider epService, string varName) {
            var stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean(TheString in (" + varName + "))");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendBeanAssert(epService, listener, "E1", true);
            SendBeanAssert(epService, listener, "E2", true);
            SendBeanAssert(epService, listener, "E3", false);
            stmt.Dispose();
        }
    
        private void SendBeanAssert(EPServiceProvider epService, SupportUpdateListener listener, string theString, bool expected) {
            epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
            Assert.AreEqual(expected, listener.GetAndClearIsInvoked());
        }
    
        private void RunAssertionVariableEPRuntime(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1", typeof(int), -1);
            epService.EPAdministrator.Configuration.AddVariable("var2", typeof(string), "abc");
            var runtimeSPI = (EPRuntimeSPI) epService.EPRuntime;
            var types = runtimeSPI.VariableTypeAll;
            Assert.AreEqual(typeof(int?), types.Get("var1"));
            Assert.AreEqual(typeof(string), types.Get("var2"));
            Assert.AreEqual(typeof(int?), runtimeSPI.GetVariableType("var1"));
            Assert.AreEqual(typeof(string), runtimeSPI.GetVariableType("var2"));
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1 = IntPrimitive, var2 = TheString";
            epService.EPAdministrator.CreateEPL(stmtTextSet);
    
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{-1, "abc"});
            SendSupportBean(epService, null, 99);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{99, null});
    
            epService.EPRuntime.SetVariableValue("var2", "def");
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{99, "def"});
    
            epService.EPRuntime.SetVariableValue("var1", 123);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{123, "def"});
    
            IDictionary<string, object> newValues = new Dictionary<string, object>();
            newValues.Put("var1", 20);
            epService.EPRuntime.SetVariableValue(newValues);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{20, "def"});
    
            newValues.Put("var1", (byte) 21);
            newValues.Put("var2", "test");
            epService.EPRuntime.SetVariableValue(newValues);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{21, "test"});
    
            newValues.Put("var1", null);
            newValues.Put("var2", null);
            epService.EPRuntime.SetVariableValue(newValues);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{null, null});
    
            // try variable not found
            try {
                epService.EPRuntime.SetVariableValue("dummy", null);
                Assert.Fail();
            } catch (VariableNotFoundException ex) {
                // expected
                Assert.AreEqual("Variable by name 'dummy' has not been declared", ex.Message);
            }
    
            // try variable not found
            try {
                newValues.Put("dummy2", 20);
                epService.EPRuntime.SetVariableValue(newValues);
                Assert.Fail();
            } catch (VariableNotFoundException ex) {
                // expected
                Assert.AreEqual("Variable by name 'dummy2' has not been declared", ex.Message);
            }
    
            // create new variable on the fly
            epService.EPAdministrator.CreateEPL("create variable int dummy = 20 + 20");
            Assert.AreEqual(40, epService.EPRuntime.GetVariableValue("dummy"));
    
            // try type coercion
            try {
                epService.EPRuntime.SetVariableValue("dummy", "abc");
                Assert.Fail();
            } catch (VariableValueException ex) {
                // expected
                Assert.AreEqual("Variable 'dummy' of declared type " + Name.Clean<int>() + " cannot be assigned a value of type System.String", ex.Message);
            }
            try {
                epService.EPRuntime.SetVariableValue("dummy", 100L);
                Assert.Fail();
            } catch (VariableValueException ex) {
                // expected
                Assert.AreEqual("Variable 'dummy' of declared type " + Name.Clean<int>() + " cannot be assigned a value of type " + Name.Clean<long>(false), ex.Message);
            }
            try {
                epService.EPRuntime.SetVariableValue("var2", 0);
                Assert.Fail();
            } catch (VariableValueException ex) {
                // expected
                Assert.AreEqual("Variable 'var2' of declared type System.String cannot be assigned a value of type " + Name.Clean<int>(false) + "", ex.Message);
            }
    
            // coercion
            epService.EPRuntime.SetVariableValue("var1", (short) -1);
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{-1, null});
    
            // rollback for coercion failed
            newValues = new LinkedHashMap<string, object>();    // preserve order
            newValues.Put("var2", "xyz");
            newValues.Put("var1", 4.4d);
            try {
                epService.EPRuntime.SetVariableValue(newValues);
                Assert.Fail();
            } catch (VariableValueException) {
                // expected
            }
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{-1, null});
    
            // rollback for variable not found
            newValues = new LinkedHashMap<string, object>();    // preserve order
            newValues.Put("var2", "xyz");
            newValues.Put("var1", 1);
            newValues.Put("notfoundvariable", null);
            try {
                epService.EPRuntime.SetVariableValue(newValues);
                Assert.Fail();
            } catch (VariableNotFoundException) {
                // expected
            }
            AssertVariableValues(epService, new[]{"var1", "var2"}, new object[]{-1, null});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSetSubquery(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("S1", typeof(SupportBean_S1));
            epService.EPAdministrator.Configuration.AddVariable("var1SS", typeof(string), "a");
            epService.EPAdministrator.Configuration.AddVariable("var2SS", typeof(string), "b");
    
            var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " as s0str set var1SS = (select p10 from S1#lastevent), var2SS = (select p11||s0str.p01 from S1#lastevent)";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1SS", "var2SS"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"a", "b"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, null}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "x", "y"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "1", "2"));
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"x", "y2"}});
    
            stmtSet.Dispose();
        }
    
        private void RunAssertionVariableInFilterBoolean(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1IFB", typeof(string), null);
            epService.EPAdministrator.Configuration.AddVariable("var2IFB", typeof(string), null);
    
            var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " set var1IFB = p00, var2IFB = p01";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1IFB", "var2IFB"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, null}});
    
            var stmtTextSelect = "select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "(TheString = var1IFB or TheString = var2IFB)";
            var fieldsSelect = new[]{"TheString", "IntPrimitive"};
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            SendSupportBean(epService, null, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeanS0NewThread(epService, 100, "a", "b");
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"a", "b"});
    
            SendSupportBean(epService, "a", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"a", 2});
    
            SendSupportBean(epService, null, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "b", 3);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"b", 3});
    
            SendSupportBean(epService, "c", 4);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeanS0NewThread(epService, 100, "e", "c");
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"e", "c"});
    
            SendSupportBean(epService, "c", 5);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"c", 5});
    
            SendSupportBean(epService, "e", 6);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"e", 6});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionVariableInFilter(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1IF", typeof(string), null);
    
            var stmtTextSet = "on " + typeof(SupportBean_S0).FullName + " set var1IF = p00";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1IF"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null}});
    
            var stmtTextSelect = "select TheString, IntPrimitive from " + typeof(SupportBean).FullName + "(TheString = var1IF)";
            var fieldsSelect = new[]{"TheString", "IntPrimitive"};
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            SendSupportBean(epService, null, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeanS0NewThread(epService, 100, "a", "b");
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"a"});
    
            SendSupportBean(epService, "a", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"a", 2});
    
            SendSupportBean(epService, null, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBeanS0NewThread(epService, 100, "e", "c");
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"e"});
    
            SendSupportBean(epService, "c", 5);
            Assert.IsFalse(listener.IsInvoked);
    
            SendSupportBean(epService, "e", 6);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{"e", 6});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAssignmentOrderNoDup(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1OND", typeof(int?), "12");
            epService.EPAdministrator.Configuration.AddVariable("var2OND", typeof(int?), "2");
            epService.EPAdministrator.Configuration.AddVariable("var3OND", typeof(int?), null);
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1OND = IntPrimitive, var2OND = var1OND + 1, var3OND = var1OND + var2OND";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1OND", "var2OND", "var3OND"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {12, 2, null}});
    
            SendSupportBean(epService, "S1", 3);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3, 4, 7});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {3, 4, 7}});
    
            SendSupportBean(epService, "S1", -1);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1, 0, -1});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {-1, 0, -1}});
    
            SendSupportBean(epService, "S1", 90);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{90, 91, 181});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {90, 91, 181}});
    
            stmtSet.Dispose();
        }
    
        private void RunAssertionAssignmentOrderDup(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1OD", typeof(int?), 0);
            epService.EPAdministrator.Configuration.AddVariable("var2OD", typeof(int?), 1);
            epService.EPAdministrator.Configuration.AddVariable("var3OD", typeof(int?), 2);
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1OD = IntPrimitive, var2OD = var2OD, var1OD = IntBoxed, var3OD = var3OD + 1";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1OD", "var2OD", "var3OD"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {0, 1, 2}});
    
            SendSupportBean(epService, "S1", -1, 10);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{10, 1, 3});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {10, 1, 3}});
    
            SendSupportBean(epService, "S2", -2, 20);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{20, 1, 4});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {20, 1, 4}});
    
            SendSupportBeanNewThread(epService, "S3", -3, 30);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{30, 1, 5});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {30, 1, 5}});
    
            SendSupportBeanNewThread(epService, "S4", -4, 40);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{40, 1, 6});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {40, 1, 6}});
    
            stmtSet.Dispose();
        }
    
        private void RunAssertionObjectModel(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1OM", typeof(double), 10d);
            epService.EPAdministrator.Configuration.AddVariable("var2OM", typeof(long), 11L);
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("var1OM", "var2OM", "id");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean_A).FullName));
    
            var stmtSelect = epService.EPAdministrator.Create(model);
            var stmtText = "select var1OM, var2OM, id from " + typeof(SupportBean_A).FullName;
            Assert.AreEqual(stmtText, model.ToEPL());
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var fieldsSelect = new[]{"var1OM", "var2OM", "id"};
            SendSupportBean_A(epService, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10d, 11L, "E1"});
    
            model = new EPStatementObjectModel();
            model.OnExpr = OnClause.CreateOnSet(Expressions.Eq(Expressions.Property("var1OM"), Expressions.Property("IntPrimitive"))).AddAssignment(Expressions.Eq(Expressions.Property("var2OM"), Expressions.Property("IntBoxed")));
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1OM=IntPrimitive, var2OM=IntBoxed";
            var stmtSet = epService.EPAdministrator.Create(model);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            Assert.AreEqual(stmtTextSet, model.ToEPL());
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1OM"));
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2OM"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            var fieldsVar = new[]{"var1OM", "var2OM"};
            EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);
    
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {10d, 11L}});
            SendSupportBean(epService, "S1", 3, 4);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3d, 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {3d, 4L}});
    
            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3d, 4L, "E2"});
    
            stmtSet.Dispose();
            stmtSelect.Dispose();
        }
    
        private void RunAssertionCompile(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1C", typeof(double), 10d);
            epService.EPAdministrator.Configuration.AddVariable("var2C", typeof(long), 11L);
    
            var stmtText = "select var1C, var2C, id from " + typeof(SupportBean_A).FullName;
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            var stmtSelect = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtText, model.ToEPL());
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var fieldsSelect = new[]{"var1C", "var2C", "id"};
            SendSupportBean_A(epService, "E1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10d, 11L, "E1"});
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1C=IntPrimitive, var2C=IntBoxed";
            model = epService.EPAdministrator.CompileEPL(stmtTextSet);
            var stmtSet = epService.EPAdministrator.Create(model);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            Assert.AreEqual(stmtTextSet, model.ToEPL());
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var1C"));
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var2C"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            var fieldsVar = new[]{"var1C", "var2C"};
            EPAssertionUtil.AssertEqualsAnyOrder(fieldsVar, typeSet.PropertyNames);
    
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {10d, 11L}});
            SendSupportBean(epService, "S1", 3, 4);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3d, 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {3d, 4L}});
    
            SendSupportBean_A(epService, "E2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3d, 4L, "E2"});
    
            stmtSet.Dispose();
            stmtSelect.Dispose();
    
            // test prepared statement
            epService.EPAdministrator.Configuration.AddVariable("var_a", typeof(A), new A());
            epService.EPAdministrator.Configuration.AddEventType(typeof(B));
            var prepared = epService.EPAdministrator.PrepareEPL("select var_a.value from B");
            var statement = epService.EPAdministrator.Create(prepared);
            statement.Subscriber = new Action<string>(value => { });
            epService.EPRuntime.SendEvent(new B());
    
            statement.Dispose();
        }
    
    
        private void RunAssertionRuntimeConfig(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1RTC", typeof(int?), 10);
    
            var stmtText = "select var1RTC, TheString from " + typeof(SupportBean).FullName + "(TheString like 'E%')";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var fieldsSelect = new[]{"var1RTC", "TheString"};
            SendSupportBean(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10, "E1"});
    
            SendSupportBean(epService, "E2", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{10, "E2"});
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%') set var1RTC = IntPrimitive";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1RTC"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, new[]{"var1RTC"}));
    
            var fieldsVar = new[]{"var1RTC"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {10}});
    
            SendSupportBean(epService, "S1", 3);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {3}});
    
            SendSupportBean(epService, "E3", 4);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{3, "E3"});
    
            SendSupportBean(epService, "S2", -1);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {-1}});
    
            SendSupportBean(epService, "E4", 5);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{-1, "E4"});
    
            try {
                epService.EPAdministrator.Configuration.AddVariable("var1RTC", typeof(int?), 10);
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Error creating variable: Variable by name 'var1RTC' has already been created", ex.Message);
            }
    
            stmtSet.Dispose();
            stmtSelect.Dispose();
        }
    
        private void RunAssertionRuntimeOrderMultiple(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1ROM", typeof(int?), null);
            epService.EPAdministrator.Configuration.AddVariable("var2ROM", typeof(int?), 1);
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%' or TheString like 'B%') set var1ROM = IntPrimitive, var2ROM = IntBoxed";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1ROM", "var2ROM"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, 1}});
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var1ROM"));
            Assert.AreEqual(typeof(int?), typeSet.GetPropertyType("var2ROM"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{"var1ROM", "var2ROM"}, typeSet.PropertyNames);
    
            SendSupportBean(epService, "S1", 3, null);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{3, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {3, null}});
    
            SendSupportBean(epService, "S1", -1, -2);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{-1, -2});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {-1, -2}});
    
            var stmtText = "select var1ROM, var2ROM, TheString from " + typeof(SupportBean).FullName + "(TheString like 'E%' or TheString like 'B%')";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            var fieldsSelect = new[]{"var1ROM", "var2ROM", "TheString"};
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, null);
    
            SendSupportBean(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{-1, -2, "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {-1, -2}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {-1, -2, "E1"}});
    
            SendSupportBean(epService, "S1", 11, 12);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{11, 12});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {11, 12}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {11, 12, "E1"}});
    
            SendSupportBean(epService, "E2", 2);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{11, 12, "E2"});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {11, 12, "E2"}});
    
            stmtSelect.Dispose();
            stmtSet.Dispose();
        }
    
        private void RunAssertionEngineConfigAPI(EPServiceProvider epService) {
            var stmtTextSet = "on " + typeof(SupportBean).FullName + "(TheString like 'S%') set papi_1 = 'end', papi_2 = false, papi_3 = null";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"papi_1", "papi_2", "papi_3"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"begin", true, "value"}});
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_1"));
            Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("papi_2"));
            Assert.AreEqual(typeof(string), typeSet.GetPropertyType("papi_3"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            typeSet.PropertyNames.SortInPlace();
            Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, fieldsVar));
    
            SendSupportBean(epService, "S1", 3);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"end", false, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"end", false, null}});
    
            SendSupportBean(epService, "S2", 4);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"end", false, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"end", false, null}});
    
            stmtSet.Dispose();
        }
    
        private void RunAssertionEngineConfigXML(EPServiceProvider epService) {
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set p_1 = TheString, p_2 = BoolBoxed, p_3 = IntBoxed, p_4 = IntBoxed";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"p_1", "p_2", "p_3", "p_4"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, true, 10L, 11.1d}});
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(string), typeSet.GetPropertyType("p_1"));
            Assert.AreEqual(typeof(bool?), typeSet.GetPropertyType("p_2"));
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("p_3"));
            Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("p_4"));
            typeSet.PropertyNames.SortInPlace();
            Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, fieldsVar));
    
            var bean = new SupportBean();
            bean.TheString = "text";
            bean.BoolBoxed = false;
            bean.IntBoxed = 200;
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{"text", false, 200L, 200d});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {"text", false, 200L, 200d}});
    
            bean = new SupportBean();   // leave all fields null
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{null, null, null, null});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, null, null, null}});
    
            stmtSet.Dispose();
        }
    
        private void RunAssertionCoercion(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1COE", typeof(float?), null);
            epService.EPAdministrator.Configuration.AddVariable("var2COE", typeof(double?), null);
            epService.EPAdministrator.Configuration.AddVariable("var3COE", typeof(long), null);
    
            var stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1COE = IntPrimitive, var2COE = IntPrimitive, var3COE=IntBoxed";
            var stmtSet = epService.EPAdministrator.CreateEPL(stmtTextSet);
            var listenerSet = new SupportUpdateListener();
            stmtSet.Events += listenerSet.Update;
            var fieldsVar = new[]{"var1COE", "var2COE", "var3COE"};
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {null, null, null}});
    
            var stmtText = "select irstream var1COE, var2COE, var3COE, id from " + typeof(SupportBean_A).FullName + "#length(2)";
            var stmtSelect = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
            var fieldsSelect = new[]{"var1COE", "var2COE", "var3COE", "id"};
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, null);
    
            var typeSet = stmtSet.EventType;
            Assert.AreEqual(typeof(float?), typeSet.GetPropertyType("var1COE"));
            Assert.AreEqual(typeof(double?), typeSet.GetPropertyType("var2COE"));
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var3COE"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            EPAssertionUtil.AssertEqualsAnyOrder(typeSet.PropertyNames, fieldsVar);
    
            SendSupportBean_A(epService, "A1");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{null, null, null, "A1"});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {null, null, null, "A1"}});
    
            SendSupportBean(epService, "S1", 1, 2);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{1f, 1d, 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {1f, 1d, 2L}});
    
            SendSupportBean_A(epService, "A2");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsSelect, new object[]{1f, 1d, 2L, "A2"});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {1f, 1d, 2L, "A1"}, new object[] {1f, 1d, 2L, "A2"}});
    
            SendSupportBean(epService, "S1", 10, 20);
            EPAssertionUtil.AssertProps(listenerSet.AssertOneGetNewAndReset(), fieldsVar, new object[]{10f, 10d, 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtSet.GetEnumerator(), fieldsVar, new[] {new object[] {10f, 10d, 20L}});
    
            SendSupportBean_A(epService, "A3");
            EPAssertionUtil.AssertProps(listener.LastNewData[0], fieldsSelect, new object[]{10f, 10d, 20L, "A3"});
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fieldsSelect, new object[]{10f, 10d, 20L, "A1"});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fieldsSelect, new[] {new object[] {10f, 10d, 20L, "A2"}, new object[] {10f, 10d, 20L, "A3"}});
    
            stmtSelect.Dispose();
            stmtSet.Dispose();
        }
    
        private void RunAssertionInvalidSet(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("var1IS", typeof(string), null);
            epService.EPAdministrator.Configuration.AddVariable("var2IS", typeof(bool), false);
            epService.EPAdministrator.Configuration.AddVariable("var3IS", typeof(int), 1);
    
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set dummy = 100",
                    "Error starting statement: Variable by name 'dummy' has not been created or configured");
    
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var1IS = 1",
                    string.Format("Error starting statement: Variable 'var1IS' of declared type {0} cannot be assigned a value of type {1}", 
                        typeof(string).GetCleanName(),
                        typeof(int).GetCleanName()));
    
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var3IS = 'abc'",
                    string.Format("Error starting statement: Variable 'var3IS' of declared type {0} cannot be assigned a value of type {1}", 
                        typeof(int?).GetCleanName(),
                        typeof(string).GetCleanName()));
    
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var3IS = DoublePrimitive",
                    string.Format("Error starting statement: Variable 'var3IS' of declared type {0} cannot be assigned a value of type {1}", 
                        typeof(int?).GetCleanName(),
                        typeof(double).GetCleanName()));

            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var2IS = 'false'", null);
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var3IS = 1.1", null);
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var3IS = 22222222222222", null);
            TryInvalidSet(epService, "on " + typeof(SupportBean).FullName + " set var3IS", "Error starting statement: Missing variable assignment expression in assignment number 0 [");
        }
    
        private void TryInvalidSet(EPServiceProvider epService, string stmtText, string message) {
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                if (message != null) {
                    SupportMessageAssertUtil.AssertMessage(ex, message);
                }
            }
        }
    
        private void RunAssertionInvalidInitialization(EPServiceProvider epService) {
            TryInvalid(epService, typeof(int?), "abcdef",
                    string.Format("Error creating variable: Variable 'invalidvar1' of declared type {0} cannot be initialized by value 'abcdef': System.FormatException: Input string was not in a correct format.", 
                        typeof(int?).GetCleanName()));
    
            TryInvalid(epService, typeof(int?), new double?(11.1),
                    string.Format("Error creating variable: Variable 'invalidvar1' of declared type {0} cannot be initialized by a value of type {1}", 
                        typeof(int?).GetCleanName(),
                        typeof(double).GetCleanName()));
    
            TryInvalid(epService, typeof(int), new double?(11.1), null);
            TryInvalid(epService, typeof(string), true, null);
        }
    
        private void TryInvalid(EPServiceProvider epService, Type type, object value, string message) {
            try {
                epService.EPAdministrator.Configuration.AddVariable("invalidvar1", type, value);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                if (message != null) {
                    Assert.AreEqual(message, ex.Message);
                }
            }
        }
    
        private SupportBean_A SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed) {
            var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendSupportBeanNewThread(EPServiceProvider epService, string theString, int intPrimitive, int? intBoxed)
        {
            var t = new Thread(
                () =>
                {
                    var bean = MakeSupportBean(theString, intPrimitive, intBoxed);
                    epService.EPRuntime.SendEvent(bean);
                });
            t.Start();
            t.Join();
        }
    
        private void SendSupportBeanS0NewThread(EPServiceProvider epService, int id, string p00, string p01)
        {
            var t = new Thread(() => epService.EPRuntime.SendEvent(new SupportBean_S0(id, p00, p01)));
            t.Start();
            t.Join();
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, int? intBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            return bean;
        }
    
        private void AssertVariableValues(EPServiceProvider epService, string[] names, object[] values) {
            Assert.AreEqual(names.Length, values.Length);
    
            // assert one-by-one
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], epService.EPRuntime.GetVariableValue(names[i]));
            }
    
            // get and assert all
            var all = epService.EPRuntime.VariableValueAll;
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], all.Get(names[i]));
            }
    
            // get by request
            var nameSet = new HashSet<string>();
            nameSet.AddAll(names);
            var valueSet = epService.EPRuntime.GetVariableValue(nameSet);
            for (var i = 0; i < names.Length; i++) {
                Assert.AreEqual(values[i], valueSet.Get(names[i]));
            }
        }
    
        [Serializable]
        public class A  {
            public string GetValue() {
                return "";
            }
        }
    
        public class B {
        }
    
        private void TryInvalidSetConstant(EPServiceProvider epService, string variableName, object newValue) {
            try {
                epService.EPRuntime.SetVariableValue(variableName, newValue);
                Assert.Fail();
            } catch (VariableConstantValueException ex) {
                Assert.AreEqual(ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
            }
            try {
                epService.EPRuntime.SetVariableValue(Collections.SingletonDataMap(variableName, newValue));
                Assert.Fail();
            } catch (VariableConstantValueException ex) {
                Assert.AreEqual(ex.Message, "Variable by name '" + variableName + "' is declared as constant and may not be assigned a new value");
            }
        }
    
        private void TryOperator(EPServiceProvider epService, string @operator, object[][] testdata) {
            var spi = (EPServiceProviderSPI) epService;
            var filterSpi = (FilterServiceSPI) spi.FilterService;
    
            var stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select TheString as c0,IntPrimitive as c1 from SupportBean(" + @operator + ")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // initiate
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "S01"));
    
            for (var i = 0; i < testdata.Length; i++) {
                var bean = new SupportBean();
                var testValue = testdata[i][0];
                if (testValue is int iValue) {
                    bean.IntBoxed = iValue;
                } else if (testValue is SupportEnum eValue) {
                    bean.EnumValue = eValue;
                } else {
                    bean.ShortBoxed = testValue.AsBoxedShort();
                }
                var expected = (bool) testdata[i][1];
    
                epService.EPRuntime.SendEvent(bean);
                Assert.AreEqual(expected, listener.GetAndClearIsInvoked(), "Failed at " + i);
            }
    
            // assert type of expression
            if (filterSpi.IsSupportsTakeApply) {
                var set = filterSpi.Take(Collections.SingletonList(stmt.StatementId));
                Assert.AreEqual(1, set.Filters.Count);
                var valueSet = set.Filters[0].FilterValueSet;
                Assert.AreEqual(1, valueSet.Parameters.Length);
                var para = valueSet.Parameters[0][0];
                Assert.IsTrue(para.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION);
            }
    
            stmt.Dispose();
        }
    
        public class MySimpleVariableServiceFactory {
            public static MySimpleVariableService MakeService() {
                return new MySimpleVariableService();
            }
        }
    
        public class MySimpleVariableService {
            public string DoSomething() {
                return "hello";
            }
        }

        public enum MyEnumWithOverride
        {
            LONG,
            SHORT
        }

        public static int GetValue(MyEnumWithOverride value)
        {
            switch (value)
            {
                case MyEnumWithOverride.LONG:
                    return 1;
                case MyEnumWithOverride.SHORT:
                    return -1;
                default:
                    throw new ArgumentException();
            }
        }
    }
} // end of namespace
