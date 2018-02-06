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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NEsper.Scripting.ClearScript;

using NUnit.Framework;

namespace com.espertech.esper.regression.script
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    [ClearScript]
    public class TestScriptExpression
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestQuoteEscape()
        {
            String eplSLComment = "create expression f(params)[\n" +
                              "  // I'am...\n" +
                              "];";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplSLComment);

            String eplMLComment = "create expression g(params)[\n" +
                                  "  /* I'am... */" +
                                  "];";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplMLComment);
        }

        [Test]
        public void TestScriptReturningEvents()
        {
            _epService.EPAdministrator.CreateEPL("create schema ItemEvent(id string)");

            RunAssertionScriptReturningEvents(false);
            RunAssertionScriptReturningEvents(true);

            SupportMessageAssertUtil.TryInvalid(
                _epService, 
                "expression double @type(ItemEvent) fib(num) [] select fib(1) from SupportBean",
                "Error starting statement: Failed to validate select-clause expression 'fib(1)': The @type annotation is only allowed when the invocation target returns EventBean instances");
        }

        [Test]
        public void TestDocSamples()
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(ColorEvent));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(RFIDEvent));
            String epl;
    
            epl = "expression double fib(num) [" +
                    "function fib(n) { " +
                    "  if(n <= 1) " +
                    "    return n; " +
                    "  return fib(n-1) + fib(n-2); " +
                    "};" +
                    "return fib(num); " +
                    "]" +
                    "select fib(IntPrimitive) from SupportBean";
            _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            epl = "expression jscript:printColors(colorEvent) [" +
                    "debug.Render(colorEvent.Colors);" +
                    "]" +
                    "select printColors(colorEvent) from ColorEvent as colorEvent";
            _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new ColorEvent());
            _epService.EPAdministrator.DestroyAllStatements();
    
            epl = "expression boolean jscript:setFlag(name, value, returnValue) [\n" +
                    "if (returnValue) epl.SetScriptAttribute(name, value);\n" +
                    "return returnValue;\n" +
                    "]\n" +
                    "expression jscript:getFlag(name) [\n" +
                    "  return epl.GetScriptAttribute(name);\n" +
                    "]\n" +
                    "select getFlag('loc') as flag from RFIDEvent(zone = 'Z1' and \n" +
                    "  (setFlag('loc', true, loc = 'A') or setFlag('loc', false, loc = 'B')) )";
            _epService.EPAdministrator.CreateEPL(epl);
        }
    
        [Test]
        public void TestInvalidRegardlessDialect()
        {
            // parameter defined twice
            TryInvalidExact("expression jscript:abc(p1, p1) [/* text */] select * from SupportBean",
                    "Invalid script parameters for script 'abc', parameter 'p1' is defined more then once [expression jscript:abc(p1, p1) [/* text */] select * from SupportBean]");
    
            // invalid dialect
            TryInvalidExact("expression dummy:abc() [10] select * from SupportBean",
                    "Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression dummy:abc() [10] select * from SupportBean]");
    
            // not found
            TryInvalidExact("select abc() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'abc': Unknown single-row function, expression declaration, script or aggregation function named 'abc' could not be resolved [select abc() from SupportBean]");
    
            // test incorrect number of parameters
            TryInvalidExact("expression jscript:abc() [10] select abc(1) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'abc(1)': Invalid number of parameters for script 'abc', expected 0 parameters but received 1 parameters [expression jscript:abc() [10] select abc(1) from SupportBean]");
    
            // test expression name overlap
            TryInvalidExact("expression jscript:abc() [10] expression jscript:abc() [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression jscript:abc() [10] expression jscript:abc() [10] select abc() from SupportBean]");
    
            // test expression name overlap with parameters
            TryInvalidExact("expression jscript:abc(p1) [10] expression jscript:abc(p2) [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression jscript:abc(p1) [10] expression jscript:abc(p2) [10] select abc() from SupportBean]");
    
            // test script name overlap with expression declaration
            TryInvalidExact("expression jscript:abc() [10] expression abc {10} select abc() from SupportBean",
                    "Script name 'abc' overlaps with another expression of the same name [expression jscript:abc() [10] expression abc {10} select abc() from SupportBean]");
    
            // fails to resolve return type
            TryInvalidExact("expression dummy jscript:abc() [10] select abc() from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'abc()': Failed to resolve return type 'dummy' specified for script 'abc' [expression dummy jscript:abc() [10] select abc() from SupportBean]");
        }
    
        [Test]
        public void TestInvalidScriptJS()
        {
            TryInvalidContains("expression jscript:abc[dummy abc = 1;] select * from SupportBean",
                    "Expected ';'");
    
            TryInvalidContains("expression jscript:abc(aa) [return aa..Bb(1);] select abc(1) from SupportBean",
                    "Expected identifier");
    
            TryInvalidExact("expression jscript:abc[] select * from SupportBean",
                    "Incorrect syntax near ']' at line 1 column 23 near reserved keyword 'select' [expression jscript:abc[] select * from SupportBean]");
    
            // empty script
            _epService.EPAdministrator.CreateEPL("expression jscript:abc[\n] select * from SupportBean");
    
            // execution problem
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.CreateEPL("expression jscript:abc() [throw new Error(\"Some error\");] select * from SupportBean#keepall where abc() = 1");
            try {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }
    
            // execution problem
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.CreateEPL("expression jscript:abc[dummy;] select * from SupportBean#keepall where abc() = 1");
            try {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
            }

            // execution problem
#if false
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.CreateEPL("@Name('ABC') expression int[] jscript:callIt() [ var myarr = new Array(2, 8, 5, 9); return myarr; ] select callIt().countOf(v => v < 6) from SupportBean")
                .Events += _listener.Update;
            try {
                _epService.EPRuntime.SendEvent(new SupportBean());
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.IsTrue(ex.Message.Contains("Unexpected exception in statement 'ABC': Non-array value provided to collection"), "Message is: " + ex.Message);
            }
#endif
        }
    
        public void TryInvalidExact(String expression, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        public void TryInvalidContains(String expression, String part) {
            try {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.IsTrue(ex.Message.Contains(part), "Message not containing text '" + part + "' : " + ex.Message);
            }
        }
    
        [Test]
        public void TestScripts()
        {
            // test different return types
            TryReturnTypes("jscript");
    
            // test void return type
            TryVoidReturnType("jscript");
    
            // test enumeration method
            // Not supported: TryEnumeration("expression int[] jscript:callIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ]"); returns NativeArray which is a Rhino-specific array wrapper
    
            // test script props
            TrySetScriptProp("jscript");
    
            // test variable
            TryPassVariable("jscript");
    
            // test passing an event
            TryPassEvent("jscript");
    
            // test returning an object
            TryReturnObject("jscript");
    
            // test datetime method
            TryDatetime("jscript");
    
            // test unnamed expression
            TryUnnamedInSelectClause("jscript");
    
            // test import
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            TryImports(string.Join(
                "\n",
                "expression MyImportedClass jscript:callOne() [ ",
                "    var type = host.resolveType('" + typeof(MyImportedClass).FullName + "'); ",
                "    return host.newObj(type);",
                "]"));
    
            // test overloading script
            _epService.EPAdministrator.Configuration.AddImport(typeof(MyImportedClass));
            TryOverloaded("jscript");
    
            // test nested invocation
            TryNested("jscript");
    
            TryAggregation();

            TryDeployArrayInScript();

            TryCreateExpressionWArrayAllocate();
        }

        private void TryCreateExpressionWArrayAllocate()
        {
            String epl = string.Join(
                "\n",
                "@Name('first') create expression double jscript:test(bar) [",
                "  function test(bar) {",
                "    debug.Debug(bar);",
                "    var test = [];",
                "    return -1.0;",
                "  };",
                "  debug.Debug(bar);",
                "  return test(bar);",
                "]");
            _epService.EPAdministrator.CreateEPL(epl);

            _epService.EPAdministrator.CreateEPL("select test('a') as c0 from SupportBean_S0").AddListener(_listener);
            _listener.Reset();
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".SplitCsv(), new Object[] { -1d });
        }

        private void TryDeployArrayInScript()
        {
            String epl = "expression string jscript:myFunc(arg) [\n" +
                    "  function replace(text, values, replacement){\n" +
                    "    return text.replace(replacement, values[0]);\n" +
                    "  }\n" +
                    "  replace(\"A B C\", [\"X\"], \"B\")\n" +
                    "]\n" +
                    "select\n" +
                    "myFunc(*)\n" +
                    "from SupportBean;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
        }

        [Test]
        public void TestParserSelectNoArgConstant()
        {
            TryParseJS("\n\t  return 10.0;    \n\n\t\t", typeof(Object), 10.0);
            TryParseJS("return 10.0;", typeof(Object), 10.0);
            TryParseJS("return 5*5.0;", typeof(Object), 25.0);
            TryParseJS("return \"abc\";", typeof(Object), "abc");
            TryParseJS("return  \"abc\"     ", typeof(Object), "abc");
            TryParseJS("return 'def';", typeof(Object), "def");
            TryParseJS("return  'def' ;", typeof(Object), "def");
        }
    
        [Test]
        public void TestJavaScriptStatelessReturnPassArgs()
        {
            Object[][] testData;
            String expression;

            expression = string.Join(
                "\n",
                "function fib(n) {" +
                "  if(n <= 1) return n; " +
                "  return fib(n-1) + fib(n-2); " +
                "};" +
                "return fib(num);"
            );

            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 20), 6765.0},
            };
            TrySelect("expression double jscript:abc(num) [ " + expression + " ]", "abc(IntPrimitive)", typeof(double), testData);
    
            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 5), 50.0},
                    new Object[] {new SupportBean("E1", 6), 60.0}
            };
            TrySelect("expression jscript:abc(myint) [ return myint * 10 ]", "abc(IntPrimitive)", typeof(object), testData);
        }
    
        private void TryVoidReturnType(String dialect)
        {
            Object[][] testData;
            String expression;
    
            expression = "expression void " + dialect + ":mysetter() [ return epl.SetScriptAttribute('a', 1); ]";
            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 20), null},
                    new Object[] {new SupportBean("E1", 10), null},
            };
            TrySelect(expression, "mysetter()", typeof(Object), testData);
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TrySetScriptProp(String dialect) {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "expression " + dialect + ":getFlag() [" +
                    "  return epl.GetScriptAttribute('flag');" +
                    "]" +
                    "expression bool " + dialect + ":setFlag(flagValue) [" +
                    "  epl.SetScriptAttribute('flag', flagValue);" +
                    "  return flagValue;" +
                    "]" +
                    "select getFlag() as val from SupportBean(TheString = 'E1' or setFlag(IntPrimitive > 0))");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            Assert.AreEqual(true, _listener.AssertOneGetNewAndReset().Get("val"));
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryPassVariable(String dialect)
        {
            Object[][] testData;
            String expression;
    
            _epService.EPAdministrator.CreateEPL("create variable int THRESHOLD = 100"); // long doesnt work in jscript
    
            expression = "expression long " + dialect + ":thresholdAdder(numToAdd, th) [ return (th + numToAdd); ]";
            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 20), 120},
                    new Object[] {new SupportBean("E1", 10), 110},
            };
            TrySelect(expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long), testData);
    
            _epService.EPRuntime.SetVariableValue("THRESHOLD", 1);
            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 20), 21},
                    new Object[] {new SupportBean("E1", 10), 11},
            };
            TrySelect(expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long), testData);
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryPassEvent(String dialect)
        {
            Object[][] testData;
            String expression;
    
            expression = "expression int " + dialect + ":callIt(bean) [ return bean.IntPrimitive + 1; ]";
            testData = new Object[][] {
                    new Object[] {new SupportBean("E1", 20), 21},
                    new Object[] {new SupportBean("E1", 10), 11},
            };
            TrySelect(expression, "callIt(sb)", typeof(int), testData);
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryReturnObject(String dialect)
        {
            String beanType = typeof(SupportBean).FullName;
            String expression = string.Join(
                "\n",
                $"expression {beanType} {dialect} :callIt() [",
                $" var beanType = host.resolveType('{beanType}');",
                $" return host.newObj(beanType,'E1', 10);",
                $"]");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression + " select callIt() as val0, callIt().GetTheString() as val1 from SupportBean as sb");
            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof(SupportBean), stmt.EventType.GetPropertyType("val0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0.TheString,val0.IntPrimitive,val1".Split(','), new Object[]{"E1", 10, "E1"});
    
            stmt.Dispose();
        }
    
        private void TryDatetime(String dialect)
        {
            long msecDate = DateTimeParser.ParseDefaultMSec("2002-05-30 09:00:00.000");
            String expression = "expression long " + dialect + ":callIt() [ return " + msecDate + "]";
            String epl = expression + " select callIt().GetHourOfDay() as val0, callIt().GetDayOfWeek() as val1 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            Assert.That(stmt.EventType.GetPropertyType("val0"), Is.EqualTo(typeof (int)));
            Assert.That(stmt.EventType.GetPropertyType("val1"), Is.EqualTo(typeof (DayOfWeek)));
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[]{9, DayOfWeek.Thursday});
    
            stmt.Dispose();
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtTwo = _epService.EPAdministrator.Create(model);
            stmtTwo.Events += _listener.Update;
            Assert.AreEqual(epl, stmtTwo.Text);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0,val1".Split(','), new Object[]{9, DayOfWeek.Thursday});
    
            stmtTwo.Dispose();
        }
    
        private void TryNested(String dialect)
        {
            String epl = "expression int " + dialect + ":abc(p1, p2) [ return p1*p2*10 ]\n" +
                         "expression int " + dialect + ":abc(p1) [ return p1*10 ]\n" +
                         "select abc(abc(2), 5) as c0 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{1000});
    
            stmt.Dispose();
        }
    
        private void TryReturnTypes(String dialect)
        {
            String epl = "expression string " + dialect + ":one() [ return 'x' ]\n" +
                         "select one() as c0 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof(String), stmt.EventType.GetPropertyType("c0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{"x"});
    
            stmt.Dispose();
        }
    
        private void TryOverloaded(String dialect)
        {
            String epl = "expression int " + dialect + ":abc() [return 10]\n" +
                         "expression int " + dialect + ":abc(p1) [return p1*10]\n" +
                         "expression int " + dialect + ":abc(p1, p2) [return p1*p2*10]\n" +
                         "select abc() as c0, abc(2) as c1, abc(2,3) as c2 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1,c2".Split(','), new Object[]{10, 20, 60});
    
            stmt.Dispose();
        }
    
        private void TryUnnamedInSelectClause(String dialect)
        {
            String expressionOne = "expression int " + dialect + ":callOne() [return 1] ";
            String expressionTwo = "expression int " + dialect + ":callTwo(a) [return 1] ";
            String expressionThree = "expression int " + dialect + ":callThree(a,b) [return 1] ";
            String epl = expressionOne + expressionTwo + expressionThree + " select callOne(),callTwo(1),callThree(1, 2) from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EventBean outEvent = _listener.AssertOneGetNewAndReset();
            foreach (String col in Collections.List("callOne()","callTwo(1)","callThree(1,2)"))
            {
                Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType(col));
                Assert.AreEqual(1, outEvent.Get(col));
            }
    
            stmt.Dispose();
        }
    
        private void TryImports(String expression)
        {
            String epl = expression + " select callOne() as val0 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0.p00".Split(','), new Object[]{MyImportedClass.VALUE_P00});
    
            stmt.Dispose();
        }
    
        private void TryEnumeration(String expression)
        {
            String epl = expression + " select (callIt()).countOf(v => v < 6) as val0 from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof(int), stmt.EventType.GetPropertyType("val0"));
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[]{2});
    
            stmt.Dispose();
    
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
            EPStatement stmtTwo = _epService.EPAdministrator.Create(model);
            stmtTwo.Events += _listener.Update;
            Assert.AreEqual(epl, stmtTwo.Text);
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[]{2});
    
            stmtTwo.Dispose();
        }
    
        private void TrySelect(String scriptPart, String selectExpr, Type expectedType, Object[][] testdata)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(scriptPart +
                        " select " + selectExpr + " as val from SupportBean as sb");
            stmt.Events += _listener.Update;
            Assert.AreEqual(expectedType, stmt.EventType.GetPropertyType("val"));
    
            for (int row = 0; row < testdata.Length; row++) {
                Object theEvent = testdata[row][0];
                Object expected = testdata[row][1];
    
                _epService.EPRuntime.SendEvent(theEvent);
                EventBean outEvent = _listener.AssertOneGetNewAndReset();
                Assert.AreEqual(expected, outEvent.Get("val"));
            }
    
            stmt.Dispose();
        }
    
        private void TryParseJS(String js, Type type, Object value)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                        "expression jscript:getResultOne [" +
                        js +
                        "] " +
                        "select getResultOne() from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(type, stmt.EventType.GetPropertyType("getResultOne()"));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(value, theEvent.Get("getResultOne()"));
            stmt.Dispose();
        }
    
        private void TryAggregation()
        {
            _epService.EPAdministrator.CreateEPL("create expression change(open, close) [ return (open - close) / close ]");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select change(first(IntPrimitive), last(IntPrimitive)) as ch from SupportBean#time(1 day)");
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
        }

        private void RunAssertionScriptReturningEvents(bool soda)
        {
            String script = string.Join(
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

            EPStatement stmtScript = SupportModelHelper.CreateByCompileOrParse(_epService, soda, script);

            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL("select myScriptReturnsEvents().where(v => v.id in ('id1', 'id3')) as c0 from SupportBean");
            stmtSelect.AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean());
            var coll = _listener.AssertOneGetNewAndReset().Get("c0").Unwrap<Map>();
            EPAssertionUtil.AssertPropsPerRow(coll.ToArray(), "id".SplitCsv(), new Object[][] { new Object[] { "id1" }, new Object[] { "id3" } });

            stmtSelect.Dispose();
            stmtScript.Dispose();
        }

        public class ColorEvent
        {
            private String[] colors = {"Red", "Blue"};

            public string[] Colors
            {
                get { return colors; }
            }
        }
    
        public class RFIDEvent
        {
            public string Zone { get; private set; }
            public string Loc { get; private set; }
        }
    }
}
