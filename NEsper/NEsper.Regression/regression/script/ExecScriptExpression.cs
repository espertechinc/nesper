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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.script;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    using Map = IDictionary<string, object>;

    public class ExecScriptExpression : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create schema ItemEvent(id string)");

            RunAssertionQuoteEscape(epService);
            RunAssertionScriptReturningEvents(epService);
            RunAssertionDocSamples(epService);
            RunAssertionInvalidRegardlessDialect(epService);
            RunAssertionInvalidScriptJS(epService);
            RunAssertionScripts(epService);
            RunAssertionParserSelectNoArgConstant(epService);
            RunAssertionJavaScriptStatelessReturnPassArgs(epService);
        }

        private void RunAssertionQuoteEscape(EPServiceProvider epService) {
            var eplSLComment = "create expression F(params)[\n" +
                                  "  // I'am...\n" +
                                  "];";
            var resultOne = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplSLComment);

            var eplMLComment = "create expression G(params)[\n" +
                                  "  /* I'params am[] */" +
                                  "];";
            var resultTwo = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplMLComment);

            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultOne.DeploymentId);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(resultTwo.DeploymentId);
        }

        private void RunAssertionScriptReturningEvents(EPServiceProvider epService) {
            RunAssertionScriptReturningEvents(epService, false);
            RunAssertionScriptReturningEvents(epService, true);

            SupportMessageAssertUtil.TryInvalid(
                epService, "expression double @Type(ItemEvent) fib(num) [] select fib(1) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'fib(1)': The @type annotation is only allowed when the invocation target returns EventBean instances");
        }

        private void RunAssertionDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType(typeof(ColorEvent));
            epService.EPAdministrator.Configuration.AddEventType(typeof(RFIDEvent));
            string epl;

            epl = "expression double fib(num) [" +
                  "function fib(n) { " +
                  "  if(n <= 1) " +
                  "    return n; " +
                  "  return fib(n-1) + fib(n-2); " +
                  "};" +
                  "return fib(num); " +
                  "]" +
                  "select fib(IntPrimitive) from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            epl = "expression jscript:printColors(colorEvent) [" +
                  "debug.Render(colorEvent.Colors);" +
                  "]" +
                  "select printColors(colorEvent) from ColorEvent as colorEvent";
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            epService.EPRuntime.SendEvent(new ColorEvent());
            epService.EPAdministrator.DestroyAllStatements();

            epl = "expression boolean jscript:setFlag(name, value, returnValue) [\n" +
                  "if (returnValue) epl.SetScriptAttribute(name, value);\n" +
                  "return returnValue;\n" +
                  "]\n" +
                  "expression jscript:getFlag(name) [\n" +
                  "  return epl.GetScriptAttribute(name);\n" +
                  "]\n" +
                  "select getFlag('loc') as flag from RFIDEvent(zone = 'Z1' and \n" +
                  "  (setFlag('loc', true, loc = 'A') or setFlag('loc', false, loc = 'B')) )";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionInvalidRegardlessDialect(EPServiceProvider epService) {
            // parameter defined twice
            TryInvalidExact(
                epService, "expression jscript:abc(p1, p1) [/* text */] select * from SupportBean",
                "Invalid script parameters for script 'abc', parameter 'p1' is defined more then once [expression jscript:abc(p1, p1) [/* text */] select * from SupportBean]");

            // invalid dialect
            TryInvalidExact(
                epService, "expression dummy:abc() [10] select * from SupportBean",
                "Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression dummy:abc() [10] select * from SupportBean]");

            // not found
            TryInvalidExact(
                epService, "select abc() from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'abc': Unknown single-row function, expression declaration, script or aggregation function named 'abc' could not be resolved [select abc() from SupportBean]");

            // test incorrect number of parameters
            TryInvalidExact(
                epService, "expression jscript:abc() [10] select abc(1) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'abc(1)': Invalid number of parameters for script 'abc', expected 0 parameters but received 1 parameters [expression jscript:abc() [10] select abc(1) from SupportBean]");

            // test expression name overlap
            TryInvalidExact(
                epService, "expression jscript:abc() [10] expression jscript:abc() [10] select abc() from SupportBean",
                "Script name 'abc' has already been defined with the same number of parameters [expression jscript:abc() [10] expression jscript:abc() [10] select abc() from SupportBean]");

            // test expression name overlap with parameters
            TryInvalidExact(
                epService, "expression jscript:abc(p1) [10] expression jscript:abc(p2) [10] select abc() from SupportBean",
                "Script name 'abc' has already been defined with the same number of parameters [expression jscript:abc(p1) [10] expression jscript:abc(p2) [10] select abc() from SupportBean]");

            // test script name overlap with expression declaration
            TryInvalidExact(
                epService, "expression jscript:abc() [10] expression abc {10} select abc() from SupportBean",
                "Script name 'abc' overlaps with another expression of the same name [expression jscript:abc() [10] expression abc {10} select abc() from SupportBean]");

            // fails to resolve return type
            TryInvalidExact(
                epService, "expression dummy jscript:abc() [10] select abc() from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'abc()': Failed to resolve return type 'dummy' specified for script 'abc' [expression dummy jscript:abc() [10] select abc() from SupportBean]");
        }

        private void RunAssertionInvalidScriptJS(EPServiceProvider epService) {

            TryInvalidContains(
                epService, "expression jscript:abc[dummy abc = 1;] select * from SupportBean",
                "Expected ';'");

            TryInvalidContains(
                epService, "expression jscript:abc(aa) [return aa..Bb(1);] select abc(1) from SupportBean",
                "Expected identifier");

            TryInvalidExact(
                epService, "expression jscript:abc[] select * from SupportBean",
                "Incorrect syntax near ']' at line 1 column 23 near reserved keyword 'select' [expression jscript:abc[] select * from SupportBean]");

            // empty script
            epService.EPAdministrator.CreateEPL("expression jscript:abc[\n] select * from SupportBean");

            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL(
                "expression jscript:abc() [throw new Error(\"Some error\");] select * from SupportBean#keepall where abc() = 1");
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }

            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.CreateEPL(
                "expression jscript:abc[dummy;] select * from SupportBean#keepall where abc() = 1");
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }

#if INVALID
            // execution problem
            epService.EPAdministrator.DestroyAllStatements();
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') expression int[] jscript:callIt() [ var myarr = new Array(2, 8, 5, 9); return myarr; ] select callIt().countOf(v => v < 6) from SupportBean")
                .Events += listener.Update;
            try {
                epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(
                    ex.Message.Contains(
                        "Unexpected exception in statement 'ABC': Non-array value provided to collection"),
                    "Message is: " + ex.Message);
            }
#endif

            epService.EPAdministrator.DestroyAllStatements();
        }

        public void TryInvalidExact(EPServiceProvider epService, string expression, string message) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }

        public void TryInvalidContains(EPServiceProvider epService, string expression, string part) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.IsTrue(ex.Message.Contains(part), "Message not containing text '" + part + "' : " + ex.Message);
            }
        }

        private void RunAssertionScripts(EPServiceProvider epService) {
            // test different return types
            TryReturnTypes(epService, "jscript");

            // test void return type
            TryVoidReturnType(epService, "jscript");

            // test enumeration method
            // Not supported: TryEnumeration("expression int[] jscript:callIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ]"); returns NativeArray which is a Rhino-specific array wrapper

            // test script props
            TrySetScriptProp(epService, "jscript");

            // test variable
            TryPassVariable(epService, "jscript");

            // test passing an event
            TryPassEvent(epService, "jscript");

            // test returning an object
            TryReturnObject(epService, "jscript");

            // test datetime method
            TryDatetime(epService, "jscript");

            // test unnamed expression
            TryUnnamedInSelectClause(epService, "jscript");

            // test import
            epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            TryImports(
                epService, string.Join(
                    "\n",
                    "expression MyImportedClass jscript:callOne() [ ",
                    "    var type = host.resolveType('" + typeof(MyImportedClass).FullName + "'); ",
                    "    return host.newObj(type);",
                    "]"));

            // test overloading script
            epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            TryOverloaded(epService, "jscript");

            // test nested invocation
            TryNested(epService, "jscript");

            TryAggregation(epService);

            TryDeployArrayInScript(epService);

            TryCreateExpressionWArrayAllocate(epService);
        }

        private void TryCreateExpressionWArrayAllocate(EPServiceProvider epService) {
            var epl = "@Name('first') create expression double jscript:test(bar) [\n" +
                         "function test(bar) {\n" +
                         "  var test=[];\n" +
                         "  return -1.0;\n" +
                         "}\n" +
                         "return test(bar);\n" +
                         "]\n";
            epService.EPAdministrator.CreateEPL(epl);

            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select test('a') as c0 from SupportBean_S0").Events += listener.Update;
            listener.Reset();
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {-1d});

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryDeployArrayInScript(EPServiceProvider epService) {
            var epl = "expression string jscript:myFunc(arg) [\n" +
                         "  function replace(text, values, replacement){\n" +
                         "    return text.replace(replacement, values[0]);\n" +
                         "  }\n" +
                         "  return replace(\"A B C\", [\"X\"], \"B\");\n" +
                         "]\n" +
                         "select\n" +
                         "myFunc(*)\n" +
                         "from SupportBean;";
            var result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }

        private void RunAssertionParserSelectNoArgConstant(EPServiceProvider epService) {
            TryParseJS(epService, "\n\t  return 10.0;    \n\n\t\t", typeof(object), 10.0);
            TryParseJS(epService, "return 10.0;", typeof(object), 10.0);
            TryParseJS(epService, "return 5*5.0;", typeof(object), 25.0);
            TryParseJS(epService, "return \"abc\";", typeof(object), "abc");
            TryParseJS(epService, "return  \"abc\"     ;", typeof(object), "abc");
            TryParseJS(epService, "return 'def';", typeof(object), "def");
            TryParseJS(epService, "return  'def' ;", typeof(object), "def");
        }

        private void RunAssertionJavaScriptStatelessReturnPassArgs(EPServiceProvider epService) {
            object[][] testData;
            string expression;

            expression =
                "function fib(n) {" +
                "  if(n <= 1) return n; " +
                "  return fib(n-1) + fib(n-2); " +
                "};" +
                "return fib(num);";
            testData = new object[][] {
                new object[] {new SupportBean("E1", 20), 6765.0},
            };
            TrySelect(
                epService, "expression double jscript:abc(num) [ " + expression + " ]", "abc(IntPrimitive)", typeof(double),
                testData);

            testData = new object[][] {
                new object[] {new SupportBean("E1", 5), 50.0},
                new object[] {new SupportBean("E1", 6), 60.0}
            };
            TrySelect(
                epService, "expression jscript:abc(myint) [ return myint * 10; ]", "abc(IntPrimitive)", typeof(object), testData);
        }

        private void TryVoidReturnType(EPServiceProvider epService, string dialect) {
            object[][] testData;
            string expression;

            expression = "expression void " + dialect + ":mysetter() [ return epl.SetScriptAttribute('a', 1); ]";
            testData = new object[][] {
                new object[] {new SupportBean("E1", 20), null},
                new object[] {new SupportBean("E1", 10), null},
            };
            TrySelect(epService, expression, "mysetter()", typeof(object), testData);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TrySetScriptProp(EPServiceProvider epService, string dialect) {
            var stmt = epService.EPAdministrator.CreateEPL(
                "expression " + dialect + ":getFlag() [" +
                "  return epl.GetScriptAttribute('flag');" +
                "]" +
                "expression bool " + dialect + ":setFlag(flagValue) [" +
                "  epl.SetScriptAttribute('flag', flagValue);" +
                "  return flagValue;" +
                "]" +
                "select getFlag() as val from SupportBean(TheString = 'E1' or setFlag(IntPrimitive > 0))");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("val"));

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryPassVariable(EPServiceProvider epService, string dialect) {
            object[][] testData;
            string expression;

            epService.EPAdministrator.CreateEPL("create variable int THRESHOLD = 100");

            expression = "expression long " + dialect + ":thresholdAdder(numToAdd, th) [ return th + numToAdd; ]";
            testData = new object[][] {
                new object[] {new SupportBean("E1", 20), 120L},
                new object[] {new SupportBean("E1", 10), 110L},
            };
            TrySelect(epService, expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long), testData);

            epService.EPRuntime.SetVariableValue("THRESHOLD", 1);
            testData = new object[][] {
                new object[] {new SupportBean("E1", 20), 21L},
                new object[] {new SupportBean("E1", 10), 11L},
            };
            TrySelect(epService, expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long), testData);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryPassEvent(EPServiceProvider epService, string dialect) {

            object[][] testData;
            string expression;

            expression = "expression int " + dialect + ":callIt(bean) [ return bean.IntPrimitive + 1; ]";
            testData = new object[][] {
                new object[] {new SupportBean("E1", 20), 21},
                new object[] {new SupportBean("E1", 10), 11},
            };
            TrySelect(epService, expression, "callIt(sb)", typeof(int), testData);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void TryReturnObject(EPServiceProvider epService, string dialect) {
            var beanType = typeof(SupportBean).FullName;
            var expression = string.Join(
                "\n",
                $"expression {beanType} {dialect} :callIt() [",
                $" var beanType = host.resolveType('{beanType}');",
                $" return host.newObj(beanType,'E1', 10);",
                $"]");

            var stmt = epService.EPAdministrator.CreateEPL(
                expression + " select callIt() as val0, callIt().GetTheString() as val1 from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("val0"));

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0.TheString,val0.IntPrimitive,val1".Split(','),
                new object[] {"E1", 10, "E1"});

            stmt.Dispose();
        }

        private void TryDatetime(EPServiceProvider epService, string dialect) {

            var msecDate = DateTimeParser.ParseDefaultMSec("2002-05-30T09:00:00.000");
            var expression = "expression long " + dialect + ":callIt() [ return " + msecDate + "; ]";
            var epl = expression + " select callIt().GetHourOfDay() as val0, callIt().GetDayOfWeek() as val1 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("val0"));

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[] {9, DayOfWeek.Thursday});

            stmt.Dispose();

            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            var stmtTwo = epService.EPAdministrator.Create(model);
            stmtTwo.Events += listener.Update;
            Assert.AreEqual(epl, stmtTwo.Text);

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new object[] {9, DayOfWeek.Thursday});

            stmtTwo.Dispose();
        }

        private void TryNested(EPServiceProvider epService, string dialect) {

            var epl = "expression int " + dialect + ":abc(p1, p2) [ return p1*p2*10; ]\n" +
                         "expression int " + dialect + ":abc(p1) [ return p1*10; ]\n" +
                         "select abc(abc(2), 5) as c0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {1000});

            stmt.Dispose();
        }

        private void TryReturnTypes(EPServiceProvider epService, string dialect) {

            var epl = "expression string " + dialect + ":one() [ return 'x'; ]\n" +
                         "select one() as c0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("c0"));

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[] {"x"});

            stmt.Dispose();
        }

        private void TryOverloaded(EPServiceProvider epService, string dialect) {

            var epl = "expression int " + dialect + ":abc() [ return 10; ]\n" +
                         "expression int " + dialect + ":abc(p1) [ return p1*10; ]\n" +
                         "expression int " + dialect + ":abc(p1, p2) [ return p1*p2*10; ]\n" +
                         "select abc() as c0, abc(2) as c1, abc(2,3) as c2 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "c0,c1,c2".Split(','), new object[] {10, 20, 60});

            stmt.Dispose();
        }

        private void TryUnnamedInSelectClause(EPServiceProvider epService, string dialect) {

            var expressionOne = "expression int " + dialect + ":callOne() [return 1;] ";
            var expressionTwo = "expression int " + dialect + ":callTwo(a) [return 1;] ";
            var expressionThree = "expression int " + dialect + ":callThree(a,b) [return 1;] ";
            var epl = expressionOne + expressionTwo + expressionThree +
                         " select callOne(),callTwo(1),callThree(1, 2) from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            var outEvent = listener.AssertOneGetNewAndReset();
            foreach (var col in Collections.List("callOne()", "callTwo(1)", "callThree(1,2)")) {
                Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType(col));
                Assert.AreEqual(1, outEvent.Get(col));
            }

            stmt.Dispose();
        }

        private void TryImports(EPServiceProvider epService, string expression) {

            var epl = expression + " select callOne() as val0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "val0.p00".Split(','), new object[] {MyImportedClass.VALUE_P00});

            stmt.Dispose();
        }

        private void TryEnumeration(EPServiceProvider epService, string expression) {

            var epl = expression + " select (callIt()).countOf(v => v < 6) as val0 from SupportBean";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("val0"));

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[] {2});

            stmt.Dispose();

            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            var stmtTwo = epService.EPAdministrator.Create(model);
            stmtTwo.Events += listener.Update;
            Assert.AreEqual(epl, stmtTwo.Text);

            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[] {2});

            stmtTwo.Dispose();
        }

        private void TrySelect(
            EPServiceProvider epService, 
            string scriptPart, 
            string selectExpr, 
            Type expectedType, 
            object[][] testdata)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                scriptPart +
                " select " + selectExpr + " as val from SupportBean as sb");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(expectedType, stmt.EventType.GetPropertyType("val"));

            for (var row = 0; row < testdata.Length; row++) {
                var theEvent = testdata[row][0];
                var expected = testdata[row][1];

                epService.EPRuntime.SendEvent(theEvent);
                var outEvent = listener.AssertOneGetNewAndReset();
                Assert.AreEqual(expected, outEvent.Get("val"));
            }

            stmt.Dispose();
        }

        private void TryParseJS(EPServiceProvider epService, string js, Type type, object value) {
            var stmt = epService.EPAdministrator.CreateEPL(
                "expression jscript:getResultOne [" +
                js +
                "] " +
                "select getResultOne() from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(type, stmt.EventType.GetPropertyType("getResultOne()"));
            var theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(value, theEvent.Get("getResultOne()"));
            stmt.Dispose();
        }

        private void TryAggregation(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL(
                "create expression change(open, close) [ return (open - close) / close; ]");
            var stmt = epService.EPAdministrator.CreateEPL(
                "select change(first(IntPrimitive), last(IntPrimitive)) as ch from SupportBean#time(1 day)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionScriptReturningEvents(EPServiceProvider epService, bool soda) {
            var script = string.Join(
                "\n",
                "create expression EventBean[] @type(ItemEvent) jscript:myScriptReturnsEvents() [",
                "  function myScriptReturnsEvents() {",
                "    var events = host.newArr(EventBean, 3);",
                "    events[0] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"id\", \"id1\"), \"ItemEvent\");",
                "    events[1] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"id\", \"id2\"), \"ItemEvent\");",
                "    events[2] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"id\", \"id3\"), \"ItemEvent\");",
                "    return events;",
                "  }",
                "  return myScriptReturnsEvents();",
                "]"
            );

            var stmtScript = SupportModelHelper.CreateByCompileOrParse(epService, soda, script);

            var stmtSelect = epService.EPAdministrator.CreateEPL(
                "select myScriptReturnsEvents().where(v => v.id in ('id1', 'id3')) as c0 from SupportBean");
            var listener = new SupportUpdateListener();
            stmtSelect.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean());
            var coll = listener.AssertOneGetNewAndReset().Get("c0")
                .UnwrapEnumerable<object>()
                .Select(item => item.UnwrapStringDictionary())
                .ToArray();

            EPAssertionUtil.AssertPropsPerRow(
                coll, "id".Split(','), 
                new object[][] {
                    new object[] {"id1"},
                    new object[] {"id3"}
                });

            stmtSelect.Dispose();
            stmtScript.Dispose();
        }

        public class ColorEvent {
            public string[] Colors { get; } = {"Red", "Blue"};
        }

        public class RFIDEvent {
            public string Zone { get; }

            public string Loc { get; }
        }
    }
} // end of namespace
