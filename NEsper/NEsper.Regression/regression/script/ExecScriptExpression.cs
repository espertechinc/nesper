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
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.script;
using com.espertech.esper.supportregression.util;

// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    public class ExecScriptExpression : RegressionExecution {
    
        private static readonly bool TEST_MVEL = false;
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create schema ItemEvent(id string)");
    
            RunAssertionQuoteEscape(epService);
            RunAssertionScriptReturningEvents(epService);
            RunAssertionDocSamples(epService);
            RunAssertionInvalidRegardlessDialect(epService);
            RunAssertionInvalidScriptJS(epService);
            RunAssertionInvalidScriptMVEL(epService);
            RunAssertionScripts(epService);
            RunAssertionParserMVELSelectNoArgConstant(epService);
            RunAssertionJavaScriptStatelessReturnPassArgs(epService);
            RunAssertionMVELStatelessReturnPassArgs(epService);
        }
    
        private void RunAssertionQuoteEscape(EPServiceProvider epService) {
            string eplSLComment = "create expression F(@params)[\n" +
                    "  // I'am...\n" +
                    "];";
            DeploymentResult resultOne = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplSLComment);
    
            string eplMLComment = "create expression G(@params)[\n" +
                    "  /* I'params am[] */" +
                    "];";
            DeploymentResult resultTwo = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplMLComment);
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultTwo.DeploymentId);
        }
    
        private void RunAssertionScriptReturningEvents(EPServiceProvider epService) {
            RunAssertionScriptReturningEvents(epService, false);
            RunAssertionScriptReturningEvents(epService, true);
    
            SupportMessageAssertUtil.TryInvalid(epService, "expression double @Type(ItemEvent) Fib(num) [] select Fib(1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'Fib(1)': The @type annotation is only allowed when the invocation target returns EventBean instances");
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(ColorEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(RFIDEvent));
            string epl;
    
            epl = "expression double Fib(num) [" +
                    "Fib(num); " +
                    "function Fib(n) { " +
                    "  If(n <= 1) " +
                    "    return n; " +
                    "  return Fib(n-1) + Fib(n-2); " +
                    "};" +
                    "]" +
                    "select Fib(intPrimitive) from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            if (TEST_MVEL) {
                epl = "expression mvel:PrintColors(colors) [" +
                        "string c = null;" +
                        "foreach (c in colors) {" +
                        "   Log.Info(c);" +
                        "}" +
                        "]" +
                        "select PrintColors(colors) from ColorEvent";
                EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
                stmt.AddListener(listener);
                epService.EPRuntime.SendEvent(new ColorEvent());
                stmt.Dispose();
            }
    
            if (SupportScriptUtil.JAVA_VERSION <= 1.7) {
                epl = "expression js:PrintColors(colorEvent) [" +
                        "importClass (java.lang.System);" +
                        "importClass (java.util.Arrays);" +
                        "Log.Info(Arrays.ToString(colorEvent.Colors));" +
                        "]" +
                        "select PrintColors(colorEvent) from ColorEvent as colorEvent";
            } else {
                epl = "expression js:PrintColors(colorEvent) [" +
                        "Print(java.util.Arrays.ToString(colorEvent.Colors));" +
                        "]" +
                        "select PrintColors(colorEvent) from ColorEvent as colorEvent";
            }
            epService.EPAdministrator.CreateEPL(epl).AddListener(listener);
            epService.EPRuntime.SendEvent(new ColorEvent());
            epService.EPAdministrator.DestroyAllStatements();
    
            epl = "expression bool js:SetFlag(name, value, returnValue) [\n" +
                    "  if (returnValue) epl.ScriptAttribute = name, value;\n" +
                    "  returnValue;\n" +
                    "]\n" +
                    "expression js:GetFlag(name) [\n" +
                    "  epl.GetScriptAttribute(name);\n" +
                    "]\n" +
                    "select GetFlag('loc') as flag from RFIDEvent(zone = 'Z1' and \n" +
                    "  (SetFlag('loc', true, loc = 'A') or SetFlag('loc', false, loc = 'B')) )";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidRegardlessDialect(EPServiceProvider epService) {
            // parameter defined twice
            TryInvalidExact(epService, "expression js:Abc(p1, p1) [/* text */] select * from SupportBean",
                    "Invalid script parameters for script 'abc', parameter 'p1' is defined more then once [expression js:Abc(p1, p1) [/* text */] select * from SupportBean]");
    
            // invalid dialect
            TryInvalidExact(epService, "expression dummy:Abc() [10] select * from SupportBean",
                    "Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression dummy:Abc() [10] select * from SupportBean]");
    
            // not found
            TryInvalidExact(epService, "select Abc() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'abc': Unknown single-row function, expression declaration, script or aggregation function named 'abc' could not be resolved [select Abc() from SupportBean]");
    
            // test incorrect number of parameters
            TryInvalidExact(epService, "expression js:Abc() [10] select Abc(1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'Abc(1)': Invalid number of parameters for script 'abc', expected 0 parameters but received 1 parameters [expression js:Abc() [10] select Abc(1) from SupportBean]");
    
            // test expression name overlap
            TryInvalidExact(epService, "expression js:Abc() [10] expression js:Abc() [10] select Abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:Abc() [10] expression js:Abc() [10] select Abc() from SupportBean]");
    
            // test expression name overlap with parameters
            TryInvalidExact(epService, "expression js:Abc(p1) [10] expression js:Abc(p2) [10] select Abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:Abc(p1) [10] expression js:Abc(p2) [10] select Abc() from SupportBean]");
    
            // test script name overlap with expression declaration
            TryInvalidExact(epService, "expression js:Abc() [10] expression abc {10} select Abc() from SupportBean",
                    "Script name 'abc' overlaps with another expression of the same name [expression js:Abc() [10] expression abc {10} select Abc() from SupportBean]");
    
            // fails to resolve return type
            TryInvalidExact(epService, "expression dummy js:Abc() [10] select Abc() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'Abc()': Failed to resolve return type 'dummy' specified for script 'abc' [expression dummy js:Abc() [10] select Abc() from SupportBean]");
        }
    
        private void RunAssertionInvalidScriptJS(EPServiceProvider epService) {
    
            if (SupportScriptUtil.JAVA_VERSION <= 1.7) {
                TryInvalidContains(epService, "expression js:abc[dummy abc = 1;] select * from SupportBean",
                        "missing ; before statement");
    
                TryInvalidContains(epService, "expression js:Abc(aa) [return Aa..Bb(1);] select Abc(1) from SupportBean",
                        "invalid return");
            } else {
                TryInvalidContains(epService, "expression js:abc[dummy abc = 1;] select * from SupportBean",
                        "Expected ; but found");
    
                TryInvalidContains(epService, "expression js:Abc(aa) [return Aa..Bb(1);] select Abc(1) from SupportBean",
                        "Invalid return statement");
            }
    
            TryInvalidExact(epService, "expression js:abc[] select * from SupportBean",
                    "Incorrect syntax near ']' at line 1 column 18 near reserved keyword 'select' [expression js:abc[] select * from SupportBean]");
    
            // empty script
            epService.EPAdministrator.CreateEPL("expression js:abc[\n] select * from SupportBean");
    
            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("expression js:Abc() [throw new Error(\"Some error\");] select * from SupportBean#keepall where Abc() = 1");
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }
    
            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("expression js:abc[dummy;] select * from SupportBean#keepall where Abc() = 1");
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }
    
            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('ABC') expression int[] js:CallIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ] select CallIt().CountOf(v => v < 6) from SupportBean").AddListener(listener);
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (Exception ex) {
                Assert.IsTrue("Message is: " + ex.Message, ex.Message.Contains("Unexpected exception in statement 'ABC': Non-array value provided to collection"));
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidScriptMVEL(EPServiceProvider epService) {
    
            if (!TEST_MVEL) {
                return;
            }
    
            // mvel return type check
            TryInvalidExact(epService, "expression java.lang.string mvel:abc[10] select * from SupportBean where Abc()",
                    "Return type and declared type not compatible for script 'abc', known return type is java.lang.int? versus declared return type java.lang.string [expression java.lang.string mvel:abc[10] select * from SupportBean where Abc()]");
    
            // undeclared variable
            TryInvalidExact(epService, "expression mvel:abc[dummy;] select * from SupportBean",
                    "For script 'abc' the variable 'dummy' has not been declared and is not a parameter [expression mvel:abc[dummy;] select * from SupportBean]");
    
            // invalid assignment
            TryInvalidContains(epService, "expression mvel:abc[dummy abc = 1;] select * from SupportBean",
                    "Line: 1, Column: 11");
    
            // syntax problem
            TryInvalidContains(epService, "expression mvel:Abc(aa) [return Aa..Bb(1);] select Abc(1) from SupportBean",
                    "unable to resolve method using strict-mode: java.lang.int?..bb");
    
            // empty brackets
            TryInvalidExact(epService, "expression mvel:abc[] select * from SupportBean",
                    "Incorrect syntax near ']' at line 1 column 20 near reserved keyword 'select' [expression mvel:abc[] select * from SupportBean]");
    
            // empty script
            epService.EPAdministrator.CreateEPL("expression mvel:abc[/* */] select * from SupportBean");
    
            // unused expression
            epService.EPAdministrator.CreateEPL("expression mvel:Abc(aa) [return Aa..Bb(1);] select * from SupportBean");
    
            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL("expression mvel:Abc() [int? a = null; a + 1;] select * from SupportBean#keepall where Abc() = 1");
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public void TryInvalidExact(EPServiceProvider epService, string expression, string message) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        public void TryInvalidContains(EPServiceProvider epService, string expression, string part) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.IsTrue("Message not containing text '" + part + "' : " + ex.Message, ex.Message.Contains(part));
            }
        }
    
        private void RunAssertionScripts(EPServiceProvider epService) {
    
            // test different return types
            TryReturnTypes(epService, "js");
            if (TEST_MVEL) {
                TryReturnTypes(epService, "mvel");
            }
    
            // test void return type
            TryVoidReturnType(epService, "js");
            if (TEST_MVEL) {
                TryVoidReturnType(epService, "js");
            }
    
            // test enumeration method
            // Not supported: TryEnumeration("expression int[] js:CallIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ]"); returns NativeArray which is a Rhino-specific array wrapper
            if (TEST_MVEL) {
                TryEnumeration(epService, "expression int[] mvel:CallIt() [ {2, 8, 5, 9} ]");
            }
    
            // test script props
            TrySetScriptProp(epService, "js");
            if (TEST_MVEL) {
                TrySetScriptProp(epService, "mvel");
            }
    
            // test variable
            TryPassVariable(epService, "js");
            if (TEST_MVEL) {
                TryPassVariable(epService, "mvel");
            }
    
            // test passing an event
            TryPassEvent(epService, "js");
            if (TEST_MVEL) {
                TryPassEvent(epService, "mvel");
            }
    
            // test returning an object
            TryReturnObject(epService, "js");
            if (TEST_MVEL) {
                TryReturnObject(epService, "mvel");
            }
    
            // test datetime method
            TryDatetime(epService, "js");
            if (TEST_MVEL) {
                TryDatetime(epService, "mvel");
            }
    
            // test unnamed expression
            TryUnnamedInSelectClause(epService, "js");
            if (TEST_MVEL) {
                TryUnnamedInSelectClause(epService, "mvel");
            }
    
            // test import
            epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            if (SupportScriptUtil.JAVA_VERSION <= 1.7) {
                TryImports(epService, "expression MyImportedClass js:CallOne() [ ImportClass(" + typeof(MyImportedClass).Name + "); new MyImportedClass() ] ");
            } else {
                TryImports(epService, "expression MyImportedClass js:CallOne() [ " +
                        "var MyJavaClass = Java.Type('" + typeof(MyImportedClass).Name + "');" +
                        "new MyJavaClass() ] ");
            }
            if (TEST_MVEL) {
                TryImports(epService, "expression MyImportedClass mvel:CallOne() [ import " + typeof(MyImportedClass).Name + "; new MyImportedClass() ] ");
            }
    
            // test overloading script
            epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            TryOverloaded(epService, "js");
            if (TEST_MVEL) {
                TryOverloaded(epService, "mvel");
            }
    
            // test nested invocation
            TryNested(epService, "js");
            if (TEST_MVEL) {
                TryNested(epService, "mvel");
            }
    
            TryAggregation(epService);
    
            TryDeployArrayInScript(epService);
    
            TryCreateExpressionWArrayAllocate(epService);
        }
    
        private void TryCreateExpressionWArrayAllocate(EPServiceProvider epService) {
            string epl = "@Name('first') create expression double js:Test(bar) [\n" +
                    "Test(bar);\n" +
                    "function Test(bar) {\n" +
                    "  var test=[];\n" +
                    "  return -1.0;\n" +
                    "}]\n";
            epService.EPAdministrator.CreateEPL(epl);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select Test('a') as c0 from SupportBean_S0").AddListener(listener);
            listener.Reset();
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{-1d});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryDeployArrayInScript(EPServiceProvider epService) {
            string epl = "expression string js:MyFunc(arg) [\n" +
                    "  function Replace(text, values, replacement){\n" +
                    "    return Text.Replace(replacement, values[0]);\n" +
                    "  }\n" +
                    "  Replace(\"A B C\", [\"X\"], \"B\")\n" +
                    "]\n" +
                    "select\n" +
                    "MyFunc(*)\n" +
                    "from SupportBean;";
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionParserMVELSelectNoArgConstant(EPServiceProvider epService) {
            if (TEST_MVEL) {
                TryParseMVEL(epService, "\n\t  10    \n\n\t\t", typeof(int?), 10);
                TryParseMVEL(epService, "10", typeof(int?), 10);
                TryParseMVEL(epService, "5*5", typeof(int?), 25);
                TryParseMVEL(epService, "\"abc\"", typeof(string), "abc");
                TryParseMVEL(epService, " \"abc\"     ", typeof(string), "abc");
                TryParseMVEL(epService, "'def'", typeof(string), "def");
                TryParseMVEL(epService, " 'def' ", typeof(string), "def");
                TryParseMVEL(epService, " new string[] {'a'}", typeof(string[]), new string[]{"a"});
            }
    
            TryParseJS(epService, "\n\t  10.0    \n\n\t\t", typeof(Object), 10.0);
            TryParseJS(epService, "10.0", typeof(Object), 10.0);
            TryParseJS(epService, "5*5.0", typeof(Object), 25.0);
            TryParseJS(epService, "\"abc\"", typeof(Object), "abc");
            TryParseJS(epService, " \"abc\"     ", typeof(Object), "abc");
            TryParseJS(epService, "'def'", typeof(Object), "def");
            TryParseJS(epService, " 'def' ", typeof(Object), "def");
        }
    
        private void RunAssertionJavaScriptStatelessReturnPassArgs(EPServiceProvider epService) {
            Object[][] testData;
            string expression;
    
            expression = "Fib(num);" +
                    "function Fib(n) {" +
                    "  If(n <= 1) return n; " +
                    "  return Fib(n-1) + Fib(n-2); " +
                    "};";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 20), 6765.0},
            };
            TrySelect(epService, "expression double js:Abc(num) [ " + expression + " ]", "Abc(intPrimitive)", typeof(double?), testData);
    
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 5), 50.0},
                    new object[] {new SupportBean("E1", 6), 60.0}
            };
            TrySelect(epService, "expression js:Abc(myint) [ myint * 10 ]", "Abc(intPrimitive)", typeof(Object), testData);
        }
    
        private void RunAssertionMVELStatelessReturnPassArgs(EPServiceProvider epService) {
            if (!TEST_MVEL) {
                return;
            }
    
            Object[][] testData;
            string expression;
    
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 5), 50},
                    new object[] {new SupportBean("E1", 6), 60}
            };
            TrySelect(epService, "expression mvel:Abc(myint) [ myint * 10 ]", "Abc(intPrimitive)", typeof(int), testData);
    
            expression = "if (theString.Equals('E1')) " +
                    "  return myint * 10;" +
                    "else " +
                    "  return myint * 5;";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 5), 50},
                    new object[] {new SupportBean("E1", 6), 60},
                    new object[] {new SupportBean("E2", 7), 35}
            };
            TrySelect(epService, "expression mvel:Abc(myint, theString) [" + expression + "]", "Abc(intPrimitive, theString)", typeof(Object), testData);
            TrySelect(epService, "expression int mvel:Abc(myint, theString) [" + expression + "]", "Abc(intPrimitive, theString)", typeof(int?), testData);
    
            expression = "a + Convert.ToString(b)";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 5), "E15"},
                    new object[] {new SupportBean("E1", 6), "E16"},
                    new object[] {new SupportBean("E2", 7), "E27"}
            };
            TrySelect(epService, "expression mvel:Abc(a, b) [" + expression + "]", "Abc(theString, intPrimitive)", typeof(string), testData);
        }
    
        private void TryVoidReturnType(EPServiceProvider epService, string dialect) {
            Object[][] testData;
            string expression;
    
            expression = "expression void " + dialect + ":Mysetter() [ Epl.ScriptAttribute = 'a', 1; ]";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 20), null},
                    new object[] {new SupportBean("E1", 10), null},
            };
            TrySelect(epService, expression, "Mysetter()", typeof(Object), testData);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TrySetScriptProp(EPServiceProvider epService, string dialect) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "expression " + dialect + ":GetFlag() [" +
                            "  epl.GetScriptAttribute('flag');" +
                            "]" +
                            "expression bool " + dialect + ":SetFlag(flagValue) [" +
                            "  epl.ScriptAttribute = 'flag', flagValue;" +
                            "  flagValue;" +
                            "]" +
                            "select GetFlag() as val from SupportBean(theString = 'E1' or SetFlag(intPrimitive > 0))");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("val"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryPassVariable(EPServiceProvider epService, string dialect) {
    
            Object[][] testData;
            string expression;
    
            epService.EPAdministrator.CreateEPL("create variable long THRESHOLD = 100");
    
            expression = "expression long " + dialect + ":ThresholdAdder(numToAdd, th) [ th + numToAdd; ]";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 20), 120L},
                    new object[] {new SupportBean("E1", 10), 110L},
            };
            TrySelect(epService, expression, "ThresholdAdder(intPrimitive, THRESHOLD)", typeof(long), testData);
    
            epService.EPRuntime.SetVariableValue("THRESHOLD", 1);
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 20), 21L},
                    new object[] {new SupportBean("E1", 10), 11L},
            };
            TrySelect(epService, expression, "ThresholdAdder(intPrimitive, THRESHOLD)", typeof(long), testData);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryPassEvent(EPServiceProvider epService, string dialect) {
    
            Object[][] testData;
            string expression;
    
            expression = "expression int " + dialect + ":CallIt(bean) [ Bean.IntPrimitive + 1; ]";
            testData = new Object[][]{
                    new object[] {new SupportBean("E1", 20), 21},
                    new object[] {new SupportBean("E1", 10), 11},
            };
            TrySelect(epService, expression, "CallIt(sb)", typeof(int?), testData);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryReturnObject(EPServiceProvider epService, string dialect) {
    
            string expression = "expression " + typeof(SupportBean).FullName + " " + dialect + ":CallIt() [ new " + typeof(SupportBean).FullName + "('E1', 10); ]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression + " select CallIt() as val0, CallIt().TheString as val1 from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0.theString,val0.intPrimitive,val1".Split(','), new Object[]{"E1", 10, "E1"});
    
            stmt.Dispose();
        }
    
        private void TryDatetime(EPServiceProvider epService, string dialect) {
    
            long msecDate = DateTimeParser.ParseDefaultMSec("2002-05-30T09:00:00.000");
            string expression = "expression long " + dialect + ":CallIt() [ " + msecDate + "]";
            string epl = expression + " select CallIt().HourOfDay as val0, CallIt().DayOfWeek as val1 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[]{9, 5});
    
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtTwo = epService.EPAdministrator.Create(model);
            stmtTwo.AddListener(listener);
            Assert.AreEqual(epl, stmtTwo.Text);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[]{9, 5});
    
            stmtTwo.Dispose();
        }
    
        private void TryNested(EPServiceProvider epService, string dialect) {
    
            string epl = "expression int " + dialect + ":Abc(p1, p2) [p1*p2*10]\n" +
                    "expression int " + dialect + ":Abc(p1) [p1*10]\n" +
                    "select Abc(Abc(2), 5) as c0 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{1000});
    
            stmt.Dispose();
        }
    
        private void TryReturnTypes(EPServiceProvider epService, string dialect) {
    
            string epl = "expression string " + dialect + ":One() ['x']\n" +
                    "select One() as c0 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("c0"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{"x"});
    
            stmt.Dispose();
        }
    
        private void TryOverloaded(EPServiceProvider epService, string dialect) {
    
            string epl = "expression int " + dialect + ":Abc() [10]\n" +
                    "expression int " + dialect + ":Abc(p1) [p1*10]\n" +
                    "expression int " + dialect + ":Abc(p1, p2) [p1*p2*10]\n" +
                    "select Abc() as c0, Abc(2) as c1, Abc(2,3) as c2 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1,c2".Split(','), new Object[]{10, 20, 60});
    
            stmt.Dispose();
        }
    
        private void TryUnnamedInSelectClause(EPServiceProvider epService, string dialect) {
    
            string expressionOne = "expression int " + dialect + ":CallOne() [1] ";
            string expressionTwo = "expression int " + dialect + ":CallTwo(a) [1] ";
            string expressionThree = "expression int " + dialect + ":CallThree(a,b) [1] ";
            string epl = expressionOne + expressionTwo + expressionThree + " select CallOne(),CallTwo(1),CallThree(1, 2) from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EventBean outEvent = listener.AssertOneGetNewAndReset();
            foreach (string col in Collections.List("CallOne()", "CallTwo(1)", "CallThree(1,2)")) {
                Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(col));
                Assert.AreEqual(1, outEvent.Get(col));
            }
    
            stmt.Dispose();
        }
    
        private void TryImports(EPServiceProvider epService, string expression) {
    
            string epl = expression + " select CallOne() as val0 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0.p00".Split(','), new Object[]{MyImportedClass.VALUE_P00});
    
            stmt.Dispose();
        }
    
        private void TryEnumeration(EPServiceProvider epService, string expression) {
    
            string epl = expression + " select (CallIt()).CountOf(v => v < 6) as val0 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("val0"));
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[]{2});
    
            stmt.Dispose();
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtTwo = epService.EPAdministrator.Create(model);
            stmtTwo.AddListener(listener);
            Assert.AreEqual(epl, stmtTwo.Text);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[]{2});
    
            stmtTwo.Dispose();
        }
    
        private void TrySelect(EPServiceProvider epService, string scriptPart, string selectExpr, Type expectedType, Object[][] testdata) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(scriptPart +
                    " select " + selectExpr + " as val from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            Assert.AreEqual(expectedType, stmt.EventType.GetPropertyType("val"));
    
            for (int row = 0; row < testdata.Length; row++) {
                Object theEvent = testdata[row][0];
                Object expected = testdata[row][1];
    
                epService.EPRuntime.SendEvent(theEvent);
                EventBean outEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(expected, outEvent.Get("val"));
            }
    
            stmt.Dispose();
        }
    
        private void TryParseJS(EPServiceProvider epService, string js, Type type, Object value) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "expression js:getResultOne [" +
                            js +
                            "] " +
                            "select GetResultOne() from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(type, stmt.EventType.GetPropertyType("GetResultOne()"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(value, theEvent.Get("GetResultOne()"));
            stmt.Dispose();
        }
    
        private void TryAggregation(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create expression Change(open, close) [ (open - close) / close ]");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select Change(First(intPrimitive), last(intPrimitive)) as ch from SupportBean#Time(1 day)");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryParseMVEL(EPServiceProvider epService, string mvelExpression, Type type, Object value) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "expression mvel:getResultOne [" +
                            mvelExpression +
                            "] " +
                            "select GetResultOne() from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "GetResultOne()".Split(','), new Object[]{value});
            stmt.Dispose();
    
            stmt = epService.EPAdministrator.CreateEPL(
                    "expression mvel:getResultOne [" +
                            mvelExpression +
                            "] " +
                            "expression mvel:getResultTwo [" +
                            mvelExpression +
                            "] " +
                            "select GetResultOne() as val0, GetResultTwo() as val1 from SupportBean");
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(type, stmt.EventType.GetPropertyType("val0"));
            Assert.AreEqual(type, stmt.EventType.GetPropertyType("val1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[]{value, value});
    
            stmt.Dispose();
        }
    
        private void RunAssertionScriptReturningEvents(EPServiceProvider epService, bool soda) {
            string script = "create expression EventBean[] @Type(ItemEvent) js:MyScriptReturnsEvents() [\n" +
                    "MyScriptReturnsEvents();" +
                    "function MyScriptReturnsEvents() {" +
                    "  var EventBeanArray = Java.Type(\"com.espertech.esper.client.EventBean[]\");\n" +
                    "  var events = new EventBeanArray(3);\n" +
                    "  events[0] = epl.EventBeanService.AdapterForMap(java.util.Collections.SingletonMap(\"id\", \"id1\"), \"ItemEvent\");\n" +
                    "  events[1] = epl.EventBeanService.AdapterForMap(java.util.Collections.SingletonMap(\"id\", \"id2\"), \"ItemEvent\");\n" +
                    "  events[2] = epl.EventBeanService.AdapterForMap(java.util.Collections.SingletonMap(\"id\", \"id3\"), \"ItemEvent\");\n" +
                    "  return events;\n" +
                    "}]";
            EPStatement stmtScript = SupportModelHelper.CreateByCompileOrParse(epService, soda, script);
    
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select MyScriptReturnsEvents().Where(v => v.id in ('id1', 'id3')) as c0 from SupportBean");
            var listener = new SupportUpdateListener();
            stmtSelect.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            ICollection<Map> coll = (ICollection<Map>) listener.AssertOneGetNewAndReset().Get("c0");
            EPAssertionUtil.AssertPropsPerRow(coll.ToArray(new Map[coll.Count]), "id".Split(','), new Object[][]{new object[] {"id1"}, new object[] {"id3"}});
    
            stmtSelect.Dispose();
            stmtScript.Dispose();
        }
    
        public class ColorEvent {
            private string[] colors = {"Red", "Blue"};
    
            public string[] GetColors() {
                return colors;
            }
        }
    
        public class RFIDEvent {
            private string zone;
            private string loc;
    
            public string GetZone() {
                return zone;
            }
    
            public string GetLoc() {
                return loc;
            }
        }
    }
} // end of namespace
