///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.variable
{
    using Map = IDictionary<string, object>;

    public class ExecVariablesCreate : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionOM(epService);
            RunAssertionCompileStartStop(epService);
            RunAssertionSubscribeAndIterate(epService);
            RunAssertionDeclarationAndSelect(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.CreateVariable = CreateVariableClause.Create("long", "var1OM", null);
            epService.EPAdministrator.Create(model);
            Assert.AreEqual("create variable long var1OM", model.ToEPL());
    
            model = new EPStatementObjectModel();
            model.CreateVariable = CreateVariableClause.Create("string", "var2OM", Expressions.Constant("abc"));
            epService.EPAdministrator.Create(model);
            Assert.AreEqual("create variable string var2OM = \"abc\"", model.ToEPL());
    
            string stmtTextSelect = "select var1OM, var2OM from " + typeof(SupportBean).FullName;
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var fieldsVar = new string[]{"var1OM", "var2OM"};
            SendSupportBean(epService, "E1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsVar, new object[]{null, "abc"});
    
            SupportModelHelper.CompileCreate(epService, "create variable double[] arrdouble = {1.0d,2.0d}");
    
            stmtSelect.Dispose();
        }
    
        private void RunAssertionCompileStartStop(EPServiceProvider epService) {
            string text = "create variable long var1CSS";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(text);
            epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
    
            text = "create variable string var2CSS = \"abc\"";
            model = epService.EPAdministrator.CompileEPL(text);
            epService.EPAdministrator.Create(model);
            Assert.AreEqual(text, model.ToEPL());
    
            string stmtTextSelect = "select var1CSS, var2CSS from " + typeof(SupportBean).FullName;
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;
    
            var fieldsVar = new string[]{"var1CSS", "var2CSS"};
            SendSupportBean(epService, "E1", 10);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fieldsVar, new object[]{null, "abc"});
    
            // ESPER-545
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string createText = "create variable int FOO = 0";
            epService.EPAdministrator.CreateEPL(createText);
            epService.EPAdministrator.CreateEPL("on pattern [every SupportBean] set FOO = FOO + 1");
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(1, epService.EPRuntime.GetVariableValue("FOO"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL(createText);
            Assert.AreEqual(0, epService.EPRuntime.GetVariableValue("FOO"));
    
            // cleanup of variable when statement exception occurs
            epService.EPAdministrator.CreateEPL("create variable int x = 123");
            try {
                epService.EPAdministrator.CreateEPL("select MissingScript(x) from SupportBean");
            } catch (Exception) {
                foreach (string statementName in epService.EPAdministrator.StatementNames) {
                    epService.EPAdministrator.GetStatement(statementName).Dispose();
                }
            }
            epService.EPAdministrator.CreateEPL("create variable int x = 123");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubscribeAndIterate(EPServiceProvider epService) {
            string stmtCreateTextOne = "create variable long var1SAI = null";
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(stmtCreateTextOne);
            Assert.AreEqual(StatementType.CREATE_VARIABLE, ((EPStatementSPI) stmtCreateOne).StatementMetadata.StatementType);
            var listenerCreateOne = new SupportUpdateListener();
            stmtCreateOne.Events += listenerCreateOne.Update;
            var fieldsVar1 = new string[]{"var1SAI"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new object[][]{new object[] {null}});
            Assert.IsFalse(listenerCreateOne.IsInvoked);
    
            EventType typeSet = stmtCreateOne.EventType;
            Assert.AreEqual(typeof(long?), typeSet.GetPropertyType("var1SAI"));
            Assert.AreEqual(typeof(Map), typeSet.UnderlyingType);
            Assert.IsTrue(Collections.AreEqual(typeSet.PropertyNames, new string[]{"var1SAI"}));
    
            string stmtCreateTextTwo = "create variable long var2SAI = 20";
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(stmtCreateTextTwo);
            var listenerCreateTwo = new SupportUpdateListener();
            stmtCreateTwo.Events += listenerCreateTwo.Update;
            var fieldsVar2 = new string[]{"var2SAI"};
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new object[][]{new object[] {20L}});
            Assert.IsFalse(listenerCreateTwo.IsInvoked);
    
            string stmtTextSet = "on " + typeof(SupportBean).FullName + " set var1SAI = IntPrimitive * 2, var2SAI = var1SAI + 1";
            epService.EPAdministrator.CreateEPL(stmtTextSet);
    
            SendSupportBean(epService, "E1", 100);
            EPAssertionUtil.AssertProps(listenerCreateOne.LastNewData[0], fieldsVar1, new object[]{200L});
            EPAssertionUtil.AssertProps(listenerCreateOne.LastOldData[0], fieldsVar1, new object[]{null});
            listenerCreateOne.Reset();
            EPAssertionUtil.AssertProps(listenerCreateTwo.LastNewData[0], fieldsVar2, new object[]{201L});
            EPAssertionUtil.AssertProps(listenerCreateTwo.LastOldData[0], fieldsVar2, new object[]{20L});
            listenerCreateOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new object[][]{new object[] {200L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new object[][]{new object[] {201L}});
    
            SendSupportBean(epService, "E2", 200);
            EPAssertionUtil.AssertProps(listenerCreateOne.LastNewData[0], fieldsVar1, new object[]{400L});
            EPAssertionUtil.AssertProps(listenerCreateOne.LastOldData[0], fieldsVar1, new object[]{200L});
            listenerCreateOne.Reset();
            EPAssertionUtil.AssertProps(listenerCreateTwo.LastNewData[0], fieldsVar2, new object[]{401L});
            EPAssertionUtil.AssertProps(listenerCreateTwo.LastOldData[0], fieldsVar2, new object[]{201L});
            listenerCreateOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new object[][]{new object[] {400L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new object[][]{new object[] {401L}});
    
            stmtCreateTwo.Stop();
            stmtCreateTwo.Start();
    
            EPAssertionUtil.AssertPropsPerRow(stmtCreateOne.GetEnumerator(), fieldsVar1, new object[][]{new object[] {400L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateTwo.GetEnumerator(), fieldsVar2, new object[][]{new object[] {20L}});
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDeclarationAndSelect(EPServiceProvider epService) {
            var variables = new object[][]{
                    new object[] {"var1", "int", "1", 1},
                    new object[] {"var2", "int", "'2'", 2},
                    new object[] {"var3", "INTEGER", " 3+2 ", 5},
                    new object[] {"var4", "bool", " true|false ", true},
                    new object[] {"var5", "bool", " var1=1 ", true},
                    new object[] {"var6", "double", " 1.11 ", 1.11d},
                    new object[] {"var7", "double", " 1.20d ", 1.20d},
                    new object[] {"var8", "DOUBLE", " ' 1.12 ' ", 1.12d},
                    new object[] {"var9", "float", " 1.13f*2f ", 2.26f},
                    new object[] {"var10", "FLOAT", " -1.14f ", -1.14f},
                    new object[] {"var11", "string", " ' XXXX ' ", " XXXX "},
                    new object[] {"var12", "string", " \"a\" ", "a"},
                    new object[] {"var13", "character", "'a'", 'a'},
                    new object[] {"var14", "char", "'x'", 'x'},
                    new object[] {"var15", "short", " 20 ", (short) 20},
                    new object[] {"var16", "SHORT", " ' 9 ' ", (short) 9},
                    new object[] {"var17", "long", " 20*2 ", (long) 40},
                    new object[] {"var18", "LONG", " ' 9 ' ", (long) 9},
                    new object[] {"var19", "byte", " 20*2 ", (byte) 40},
                    new object[] {"var20", "BYTE", "9+1", (byte) 10},
                    new object[] {"var21", "int", null, null},
                    new object[] {"var22", "bool", null, null},
                    new object[] {"var23", "double", null, null},
                    new object[] {"var24", "float", null, null},
                    new object[] {"var25", "string", null, null},
                    new object[] {"var26", "char", null, null},
                    new object[] {"var27", "short", null, null},
                    new object[] {"var28", "long", null, null},
                    new object[] {"var29", "BYTE", null, null},
            };
    
            for (int i = 0; i < variables.Length; i++) {
                string text = "create variable " + variables[i][1] + " " + variables[i][0];
                if (variables[i][2] != null) {
                    text += " = " + variables[i][2];
                }
    
                epService.EPAdministrator.CreateEPL(text);
            }
    
            // select all variables
            var buf = new StringBuilder();
            string delimiter = "";
            buf.Append("select ");
            for (int i = 0; i < variables.Length; i++) {
                buf.Append(delimiter);
                buf.Append(variables[i][0]);
                delimiter = ",";
            }
            buf.Append(" from ");
            buf.Append(typeof(SupportBean).FullName);
            EPStatement stmt = epService.EPAdministrator.CreateEPL(buf.ToString());
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // assert initialization values
            SendSupportBean(epService, "E1", 1);
            EventBean received = listener.AssertOneGetNewAndReset();
            for (int i = 0; i < variables.Length; i++) {
                Assert.AreEqual(variables[i][3], received.Get((string) variables[i][0]));
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string stmt = "create variable somedummy myvar = 10";
            SupportMessageAssertUtil.TryInvalid(epService, stmt, "Error starting statement: Cannot create variable: Cannot create variable 'myvar', type 'somedummy' is not a recognized type [create variable somedummy myvar = 10]");
    
            stmt = "create variable string myvar = 5";
            SupportMessageAssertUtil.TryInvalid(epService, stmt, "Error starting statement: Cannot create variable: Variable 'myvar' of declared type System.String cannot be initialized by a value of type " + Name.Clean<int>(false) + " [create variable string myvar = 5]");
    
            stmt = "create variable string myvar = 'a'";
            epService.EPAdministrator.CreateEPL("create variable string myvar = 'a'");
            SupportMessageAssertUtil.TryInvalid(epService, stmt, "Error starting statement: Cannot create variable: Variable by name 'myvar' has already been created [create variable string myvar = 'a']");
    
            SupportMessageAssertUtil.TryInvalid(epService, "select * from " + typeof(SupportBean).FullName + " output every somevar events",
                    "Error starting statement: Error in the output rate limiting clause: Variable named 'somevar' has not been declared [");
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
