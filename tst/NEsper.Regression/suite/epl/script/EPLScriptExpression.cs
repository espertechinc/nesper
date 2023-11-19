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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.script;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.script
{
    public class EPLScriptExpression
    {
        private const bool TEST_MVEL = false;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithScripts(execs);
            WithQuoteEscape(execs);
            WithScriptReturningEvents(execs);
            WithDocSamples(execs);
            WithInvalidRegardlessDialect(execs);
            WithInvalidScriptJS(execs);
            WithInvalidScriptMVEL(execs);
            WithParserMVELSelectNoArgConstant(execs);
            WithJavaScriptStatelessReturnPassArgs(execs);
            WithMVELStatelessReturnPassArgs(execs);
            WithSubqueryParam(execs);
            WithReturnNullWhenNumeric(execs);
            WithGenericResultType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGenericResultType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptGenericResultType());
            return execs;
        }

        public static IList<RegressionExecution> WithReturnNullWhenNumeric(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptReturnNullWhenNumeric());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryParam(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptSubqueryParam());
            return execs;
        }

        public static IList<RegressionExecution> WithMVELStatelessReturnPassArgs(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptMVELStatelessReturnPassArgs());
            return execs;
        }

        public static IList<RegressionExecution> WithJavaScriptStatelessReturnPassArgs(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptJavaScriptStatelessReturnPassArgs());
            return execs;
        }

        public static IList<RegressionExecution> WithParserMVELSelectNoArgConstant(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptParserMVELSelectNoArgConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidScriptMVEL(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptInvalidScriptMVEL());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidScriptJS(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptInvalidScriptJS());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidRegardlessDialect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptInvalidRegardlessDialect());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSamples(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptDocSamples());
            return execs;
        }

        public static IList<RegressionExecution> WithScriptReturningEvents(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptScriptReturningEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithQuoteEscape(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptQuoteEscape());
            return execs;
        }

        public static IList<RegressionExecution> WithScripts(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLScriptScripts());
            return execs;
        }

        private class EPLScriptReturnNullWhenNumeric : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema Event(host string); " +
                          "create window DnsTrafficProfile#time(5 minutes) (host string); " +
                          "expression double js:doSomething(p) [ " +
                          "doSomething(p); " +
                          "function doSomething(p) { " +
                          "  System.Console.WriteLine(p);" +
                          "  System.Console.WriteLine(p.length);" +
                          " } " +
                          "] " +
                          "@name('out') select doSomething((select window(z.*) from DnsTrafficProfile as z)) as score from DnsTrafficProfile;" +
                          "insert into DnsTrafficProfile select * from Event; ";
                env.CompileDeploy(epl, new RegressionPath());
                env.AddListener("out");

                var @event = new Dictionary<string, object>();
                @event.Put("host", "test.domain.com");
                env.SendEventMap(@event, "Event");

                env.UndeployAll();
            }
        }

        private class EPLScriptGenericResultType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') expression System.Collections.Generic.IList<String> js:myJSFunc(stringvalue) [\n" +
                    "  doSomething(stringvalue);\n" +
                    "  function doSomething(stringvalue) {\n" +
                    "    return null;\n" +
                    "  }\n" +
                    "]\n" +
                    "select myJSFunc('test') as c0 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(typeof(IList<string>), statement.EventType.GetPropertyType("c0")));

                env.UndeployAll();
            }
        }

        private class EPLScriptSubqueryParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') expression double js:myJSFunc(stringvalue) [\n" +
                          "  calcScore(stringvalue);\n" +
                          "  function calcScore(stringvalue) {\n" +
                          "    return parseFloat(stringvalue);\n" +
                          "  }\n" +
                          "]\n" +
                          "select myJSFunc((select TheString from SupportBean#lastevent)) as c0 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("20", 0));
                env.SendEventBean(new SupportBean_S0(0));
                env.AssertEqualsNew("s0", "c0", 20d);

                env.SendEventBean(new SupportBean("30", 0));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertEqualsNew("s0", "c0", 30d);

                env.UndeployAll();
            }
        }

        private class EPLScriptQuoteEscape : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplSLComment = "create expression f(params)[\n" +
                                   "  // I'am...\n" +
                                   "];";
                env.CompileDeploy(eplSLComment);

                var eplMLComment = "create expression g(params)[\n" +
                                   "  /* I'am... */" +
                                   "];";
                env.CompileDeploy(eplMLComment);

                env.UndeployAll();
            }
        }

        private class EPLScriptScriptReturningEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionScriptReturningEvents(env, false);
                RunAssertionScriptReturningEvents(env, true);

                var path = new RegressionPath();
                env.CompileDeploy("create schema ItemEvent(Id string)", path);
                env.TryInvalidCompile(
                    path,
                    "expression double @type(ItemEvent) fib(num) [] select fib(1) from SupportBean",
                    "Failed to validate select-clause expression 'fib(1)': The @type annotation is only allowed when the invocation target returns EventBean instances");
                env.UndeployAll();
            }
        }

        private class EPLScriptDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "@name('s0') expression double fib(num) [" +
                      "fib(num); " +
                      "function fib(n) { " +
                      "  if(n <= 1) " +
                      "    return n; " +
                      "  return fib(n-1) + fib(n-2); " +
                      "};" +
                      "]" +
                      "select fib(IntPrimitive) from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();

                epl = "@Name('s0') expression js:printColors(colorEvent) [\n" +
                      "  debug.Print(debug.Render(colorEvent.Colors));\n" +
                      "]" +
                      "select printColors(colorEvent) from SupportColorEvent as colorEvent";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportColorEvent());
                env.UndeployAll();

                epl = "@name('s0') expression boolean js:setFlag(name, value, returnValue) [\n" +
                      "  if (returnValue) epl.setScriptAttribute(name, value);\n" +
                      "  returnValue;\n" +
                      "]\n" +
                      "expression js:getFlag(name) [\n" +
                      "  epl.getScriptAttribute(name);\n" +
                      "]\n" +
                      "select getFlag('loc') as flag from SupportRFIDSimpleEvent(zone = 'Z1' and \n" +
                      "  (setFlag('loc', true, loc = 'A') or setFlag('loc', false, loc = 'B')) )";
                env.CompileDeploy(epl);
                env.UndeployAll();
            }
        }

        private class EPLScriptInvalidRegardlessDialect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // parameter defined twice
                env.TryInvalidCompile(
                    "expression js:abc(p1, p1) [/* text */] select * from SupportBean",
                    "Invalid script parameters for script 'abc', parameter 'p1' is defined more then once [expression js:abc(p1, p1) [/* text */] select * from SupportBean]");

                // invalid dialect
                env.TryInvalidCompile(
                    "expression dummy:abc() [10] select * from SupportBean",
                    "Failed to obtain script runtime for dialect 'dummy' for script 'abc' [expression dummy:abc() [10] select * from SupportBean]");

                // not found
                env.TryInvalidCompile(
                    "select abc() from SupportBean",
                    "Failed to validate select-clause expression 'abc()': Unknown single-row function, expression declaration, script or aggregation function named 'abc' could not be resolved [select abc() from SupportBean]");

                // test incorrect number of parameters
                env.TryInvalidCompile(
                    "expression js:abc() [10] select abc(1) from SupportBean",
                    "Failed to validate select-clause expression 'abc(1)': Invalid number of parameters for script 'abc', expected 0 parameters but received 1 parameters [expression js:abc() [10] select abc(1) from SupportBean]");

                // test expression name overlap
                env.TryInvalidCompile(
                    "expression js:abc() [10] expression js:abc() [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:abc() [10] expression js:abc() [10] select abc() from SupportBean]");

                // test expression name overlap with parameters
                env.TryInvalidCompile(
                    "expression js:abc(p1) [10] expression js:abc(p2) [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:abc(p1) [10] expression js:abc(p2) [10] select abc() from SupportBean]");

                // test script name overlap with expression declaration
                env.TryInvalidCompile(
                    "expression js:abc() [10] expression abc {10} select abc() from SupportBean",
                    "Script name 'abc' overlaps with another expression of the same name [expression js:abc() [10] expression abc {10} select abc() from SupportBean]");

                // fails to resolve return type
                env.TryInvalidCompile(
                    "expression dummy js:abc() [10] select abc() from SupportBean",
                    "Failed to validate select-clause expression 'abc()': Failed to resolve return type 'dummy' specified for script 'abc' [expression dummy js:abc() [10] select abc() from SupportBean]");
            }
        }

        private class EPLScriptInvalidScriptJS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidContains(
                    env,
                    "expression js:abc[dummy abc = 1;] select * from SupportBean",
                    "Expected ; but found");

                TryInvalidContains(
                    env,
                    "expression js:abc(aa) [return aa..bb(1);] select abc(1) from SupportBean",
                    "Invalid return statement");

                env.TryInvalidCompile(
                    "expression js:abc[] select * from SupportBean",
                    "Incorrect syntax near ']' at line 1 column 18 near reserved keyword 'select' [expression js:abc[] select * from SupportBean]");

                // empty script
                env.CompileDeploy("expression js:abc[\n] select * from SupportBean");

                // execution problem
                env.UndeployAll();
                env.CompileDeploy(
                    "expression js:abc() [throw new Error(\"Some error\");] select * from SupportBean#keepall where abc() = 1");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
                }

                // execution problem
                env.UndeployAll();
                env.CompileDeploy("expression js:abc[dummy;] select * from SupportBean#keepall where abc() = 1");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
                }

                // execution problem
                env.UndeployAll();
                env.CompileDeploy(
                        "@name('ABC') expression int[] js:callIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ] select callIt().countOf(v => v < 6) from SupportBean")
                    .AddListener("ABC");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(
                        ex.Message.Contains("Unexpected exception in statement 'ABC': "),
                        "Message is: " + ex.Message);
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLScriptInvalidScriptMVEL : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (!TEST_MVEL) {
                    return;
                }

                // mvel return type check
                env.TryInvalidCompile(
                    "expression System.String mvel:abc[10] select * from SupportBean where abc()",
                    "Failed to validate filter expression 'abc()': Return type and declared type not compatible for script 'abc', known return type is System.Int32 versus declared return type System.String [expression System.String mvel:abc[10] select * from SupportBean where abc()]");

                // undeclared variable
                env.TryInvalidCompile(
                    "expression mvel:abc[dummy;] select * from SupportBean",
                    "For script 'abc' the variable 'dummy' has not been declared and is not a parameter [expression mvel:abc[dummy;] select * from SupportBean]");

                // invalid assignment
                TryInvalidContains(
                    env,
                    "expression mvel:abc[dummy abc = 1;] select * from SupportBean",
                    "Exception compiling MVEL script 'abc'");

                // syntax problem
                TryInvalidContains(
                    env,
                    "expression mvel:abc(aa) [return aa..bb(1);] select abc(1) from SupportBean",
                    "unable to resolve method using strict-mode");

                // empty brackets
                env.TryInvalidCompile(
                    "expression mvel:abc[] select * from SupportBean",
                    "Incorrect syntax near ']' at line 1 column 20 near reserved keyword 'select' [expression mvel:abc[] select * from SupportBean]");

                // empty script
                env.CompileDeploy("expression mvel:abc[/* */] select * from SupportBean");

                // unused expression
                env.CompileDeploy("expression mvel:abc(aa) [return aa..bb(1);] select * from SupportBean");

                // execution problem
                env.UndeployAll();

                env.CompileDeploy(
                    "expression mvel:abc() [Integer a = null; a + 1;] select * from SupportBean#keepall where abc() = 1");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex.Message.Contains("Unexpected exception executing script 'abc' for statement '"));
                }

                env.UndeployAll();
            }
        }

        private class EPLScriptScripts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test different return types
                TryReturnTypes(env, "js");
                if (TEST_MVEL) {
                    TryReturnTypes(env, "mvel");
                }

                // test void return type
                TryVoidReturnType(env, "js");
                if (TEST_MVEL) {
                    TryVoidReturnType(env, "js");
                }

                // test enumeration method
                // Not supported: tryEnumeration("expression int[] js:callIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ]"); returns NativeArray which is a Rhino-specific array wrapper
                if (TEST_MVEL) {
                    TryEnumeration(
                        env,
                        "expression Integer[] mvel:callIt() [ Integer[] array = {2, 8, 5, 9}; return array; ]");
                }

                // test script props
                TrySetScriptProp(env, "js");
                if (TEST_MVEL) {
                    TrySetScriptProp(env, "mvel");
                }

                // test variable
                TryPassVariable(env, "js");
                if (TEST_MVEL) {
                    TryPassVariable(env, "mvel");
                }

                // test passing an event
                TryPassEvent(env, "js");
                if (TEST_MVEL) {
                    TryPassEvent(env, "mvel");
                }

                // test returning an object
                TryReturnObject(env, "js");
                if (TEST_MVEL) {
                    TryReturnObject(env, "mvel");
                }

                // test datetime method
                TryDatetime(env, "js");
                if (TEST_MVEL) {
                    TryDatetime(env, "mvel");
                }

                // test unnamed expression
                TryUnnamedInSelectClause(env, "js");
                if (TEST_MVEL) {
                    TryUnnamedInSelectClause(env, "mvel");
                }

                // test import
                TryImports(
                    env,
                    "expression MyImportedClass js:callOne() [ " +
                    "var MyJavaClass = Java.type('" +
                    typeof(MyImportedClass).FullName +
                    "');" +
                    "new MyJavaClass() ] ");

                if (TEST_MVEL) {
                    TryImports(
                        env,
                        "expression MyImportedClass mvel:callOne() [ import " +
                        typeof(MyImportedClass).FullName +
                        "; new MyImportedClass() ] ");
                }

                // test overloading script
                TryOverloaded(env, "js");
                if (TEST_MVEL) {
                    TryOverloaded(env, "mvel");
                }

                // test nested invocation
                TryNested(env, "js");
                if (TEST_MVEL) {
                    TryNested(env, "mvel");
                }

                TryAggregation(env);

                TryDeployArrayInScript(env);

                TryCreateExpressionWArrayAllocate(env);
            }
        }

        private class EPLScriptParserMVELSelectNoArgConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (TEST_MVEL) {
                    TryParseMVEL(env, "\n\t  10    \n\n\t\t", typeof(int?), 10);
                    TryParseMVEL(env, "10", typeof(int?), 10);
                    TryParseMVEL(env, "5*5", typeof(int?), 25);
                    TryParseMVEL(env, "\"abc\"", typeof(string), "abc");
                    TryParseMVEL(env, " \"abc\"     ", typeof(string), "abc");
                    TryParseMVEL(env, "'def'", typeof(string), "def");
                    TryParseMVEL(env, " 'def' ", typeof(string), "def");
                    TryParseMVEL(env, " new String[] {'a'}", typeof(string[]), new string[] { "a" });
                }

                TryParseJS(env, "\n\t  10.0    \n\n\t\t", typeof(object), 10.0);
                TryParseJS(env, "10.0", typeof(object), 10.0);
                TryParseJS(env, "5*5.0", typeof(object), 25.0);
                TryParseJS(env, "\"abc\"", typeof(object), "abc");
                TryParseJS(env, " \"abc\"     ", typeof(object), "abc");
                TryParseJS(env, "'def'", typeof(object), "def");
                TryParseJS(env, " 'def' ", typeof(object), "def");
            }
        }

        private class EPLScriptJavaScriptStatelessReturnPassArgs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[][] testData;
                string expression;
                var path = new RegressionPath();

                expression = "fib(num);" +
                             "function fib(n) {" +
                             "  if(n <= 1) return n; " +
                             "  return fib(n-1) + fib(n-2); " +
                             "};";
                testData = new object[][] {
                    new object[] { new SupportBean("E1", 20), 6765.0 },
                };
                TrySelect(
                    env,
                    path,
                    "expression double js:abc(num) [ " + expression + " ]",
                    "abc(IntPrimitive)",
                    typeof(double?),
                    testData);
                path.Clear();

                testData = new object[][] {
                    new object[] { new SupportBean("E1", 5), 50.0 },
                    new object[] { new SupportBean("E1", 6), 60.0 }
                };
                TrySelect(
                    env,
                    path,
                    "expression js:abc(myint) [ myint * 10 ]",
                    "abc(IntPrimitive)",
                    typeof(object),
                    testData);
                path.Clear();
            }
        }

        private class EPLScriptMVELStatelessReturnPassArgs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                if (!TEST_MVEL) {
                    return;
                }

                object[][] testData;
                string expression;
                var path = new RegressionPath();

                testData = new object[][] {
                    new object[] { new SupportBean("E1", 5), 50 },
                    new object[] { new SupportBean("E1", 6), 60 }
                };
                TrySelect(
                    env,
                    path,
                    "expression mvel:abc(myint) [ myint * 10 ]",
                    "abc(IntPrimitive)",
                    typeof(int?),
                    testData);
                path.Clear();

                expression = "if (TheString.equals('E1')) " +
                             "  return myint * 10;" +
                             "else " +
                             "  return myint * 5;";
                testData = new object[][] {
                    new object[] { new SupportBean("E1", 5), 50 },
                    new object[] { new SupportBean("E1", 6), 60 },
                    new object[] { new SupportBean("E2", 7), 35 }
                };
                TrySelect(
                    env,
                    path,
                    "expression mvel:abc(myint, TheString) [" + expression + "]",
                    "abc(IntPrimitive, TheString)",
                    typeof(object),
                    testData);
                path.Clear();

                TrySelect(
                    env,
                    path,
                    "expression int mvel:abc(myint, TheString) [" + expression + "]",
                    "abc(IntPrimitive, TheString)",
                    typeof(int?),
                    testData);
                path.Clear();

                expression = "a + Convert.ToString(b)";
                testData = new object[][] {
                    new object[] { new SupportBean("E1", 5), "E15" },
                    new object[] { new SupportBean("E1", 6), "E16" },
                    new object[] { new SupportBean("E2", 7), "E27" }
                };
                TrySelect(
                    env,
                    path,
                    "expression mvel:abc(a, b) [" + expression + "]",
                    "abc(TheString, IntPrimitive)",
                    typeof(string),
                    testData);
            }
        }

        private static void RunAssertionScriptReturningEvents(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('type') @public create schema ItemEvent(Id string)", path);

            var script =
                "@name('script') create expression EventBean[] @type(ItemEvent) js:myScriptReturnsEvents() [\n" +
                "function myScriptReturnsEvents() {" +
                "  var eventBean = host.resolveType(\"com.espertech.esper.common.client.EventBean\");\n" +
                "  var events = host.newArr(eventBean, 3);\n" +
                "  events[0] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"Id\", \"id1\"), \"ItemEvent\");\n" +
                "  events[1] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"Id\", \"id2\"), \"ItemEvent\");\n" +
                "  events[2] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"Id\", \"id3\"), \"ItemEvent\");\n" +
                "  return events;\n" +
                "};\n" +
                "return myScriptReturnsEvents();" +
                "]";
            env.CompileDeploy(soda, script, path);
            env.AssertStatement(
                "script",
                statement => {
                    Assert.AreEqual(
                        StatementType.CREATE_EXPRESSION,
                        statement.GetProperty(StatementProperty.STATEMENTTYPE));
                    Assert.AreEqual("myScriptReturnsEvents", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
                });

            env.CompileDeploy(
                "@name('s0') select myScriptReturnsEvents().where(v => v.Id in ('id1', 'id3')) as c0 from SupportBean",
                path);
            env.AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertEventNew(
                "s0",
                @event => {
                    var coll = (ICollection<IDictionary<string, object>>)@event.Get("c0");
                    EPAssertionUtil.AssertPropsPerRow(
                        coll.ToArray(),
                        "Id".SplitCsv(),
                        new object[][] { new object[] { "id1" }, new object[] { "id3" } });
                });

            env.UndeployModuleContaining("s0");
            env.UndeployModuleContaining("script");
            env.UndeployModuleContaining("type");
        }

        private static void TryVoidReturnType(
            RegressionEnvironment env,
            string dialect)
        {
            object[][] testData;
            string expression;
            var path = new RegressionPath();

            expression = "expression void " + dialect + ":mysetter() [ epl.setScriptAttribute('a', 1); ]";
            testData = new object[][] {
                new object[] { new SupportBean("E1", 20), null },
                new object[] { new SupportBean("E1", 10), null },
            };
            TrySelect(env, path, expression, "mysetter()", typeof(object), testData);

            env.UndeployAll();
        }

        private static void TrySetScriptProp(
            RegressionEnvironment env,
            string dialect)
        {
            env.CompileDeploy(
                "@name('s0') expression " +
                dialect +
                ":getFlag() [" +
                "  epl.getScriptAttribute('flag');" +
                "]" +
                "expression boolean " +
                dialect +
                ":setFlag(flagValue) [" +
                "  epl.setScriptAttribute('flag', flagValue);" +
                "  flagValue;" +
                "]" +
                "select getFlag() as val from SupportBean(TheString = 'E1' or setFlag(IntPrimitive > 0))");
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("E2", 10));
            env.AssertEqualsNew("s0", "val", true);

            env.UndeployAll();
        }

        private static void TryPassVariable(
            RegressionEnvironment env,
            string dialect)
        {
            object[][] testData;
            string expression;

            var path = new RegressionPath();
            env.CompileDeploy("@name('var') @public create variable long THRESHOLD = 100", path);

            expression = "expression long " + dialect + ":thresholdAdder(numToAdd, th) [ th + numToAdd; ]";
            testData = new object[][] {
                new object[] { new SupportBean("E1", 20), 120L },
                new object[] { new SupportBean("E1", 10), 110L },
            };
            TrySelect(env, path, expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long?), testData);

            env.RuntimeSetVariable("var", "THRESHOLD", 1);
            testData = new object[][] {
                new object[] { new SupportBean("E1", 20), 21L },
                new object[] { new SupportBean("E1", 10), 11L },
            };
            TrySelect(env, path, expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long?), testData);

            env.UndeployAll();
        }

        private static void TryPassEvent(
            RegressionEnvironment env,
            string dialect)
        {
            object[][] testData;
            string expression;
            var path = new RegressionPath();

            expression = "expression int " + dialect + ":callIt(bean) [ bean.getIntPrimitive() + 1; ]";
            testData = new object[][] {
                new object[] { new SupportBean("E1", 20), 21 },
                new object[] { new SupportBean("E1", 10), 11 },
            };
            TrySelect(env, path, expression, "callIt(sb)", typeof(int?), testData);

            env.UndeployAll();
        }

        private static void TryReturnObject(
            RegressionEnvironment env,
            string dialect)
        {
            var expression = "@name('s0') expression " +
                             typeof(SupportBean).FullName +
                             " " +
                             dialect +
                             ":callIt() [ new " +
                             typeof(SupportBean).FullName +
                             "('E1', 10); ]";
            env.CompileDeploy(
                    expression + " select callIt() as val0, callIt().TheString as val1 from SupportBean as sb")
                .AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(SupportBean), statement.EventType.GetPropertyType("val0")));

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew(
                "s0",
                "val0.TheString,val0.IntPrimitive,val1".SplitCsv(),
                new object[] { "E1", 10, "E1" });

            env.UndeployAll();
        }

        private static void TryDatetime(
            RegressionEnvironment env,
            string dialect)
        {
            var msecDate = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
            var expression = "expression long " + dialect + ":callIt() [ " + msecDate + "]";
            var epl = "@name('s0') " +
                      expression +
                      " select callIt().getHourOfDay() as val0, callIt().getDayOfWeek() as val1 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("val0")));

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "val0,val1".SplitCsv(), new object[] { 9, 5 });

            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "val0,val1".SplitCsv(), new object[] { 9, 5 });

            env.UndeployAll();
        }

        private static void TryNested(
            RegressionEnvironment env,
            string dialect)
        {
            var epl = "@name('s0') expression int " +
                      dialect +
                      ":abc(p1, p2) [p1*p2*10]\n" +
                      "expression int " +
                      dialect +
                      ":abc(p1) [p1*10]\n" +
                      "select abc(abc(2), 5) as c0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { 1000 });

            env.UndeployAll();
        }

        private static void TryReturnTypes(
            RegressionEnvironment env,
            string dialect)
        {
            var epl = "@name('s0') expression string " +
                      dialect +
                      ":one() ['x']\n" +
                      "select one() as c0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(string), statement.EventType.GetPropertyType("c0")));

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { "x" });

            env.UndeployAll();
        }

        private static void TryOverloaded(
            RegressionEnvironment env,
            string dialect)
        {
            var epl = "@name('s0') expression int " +
                      dialect +
                      ":abc() [10]\n" +
                      "expression int " +
                      dialect +
                      ":abc(p1) [p1*10]\n" +
                      "expression int " +
                      dialect +
                      ":abc(p1, p2) [p1*p2*10]\n" +
                      "select abc() as c0, abc(2) as c1, abc(2,3) as c2 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "c0,c1,c2".SplitCsv(), new object[] { 10, 20, 60 });

            env.UndeployAll();
        }

        private static void TryUnnamedInSelectClause(
            RegressionEnvironment env,
            string dialect)
        {
            var expressionOne = "expression int " + dialect + ":callOne() [1] ";
            var expressionTwo = "expression int " + dialect + ":callTwo(a) [1] ";
            var expressionThree = "expression int " + dialect + ":callThree(a,b) [1] ";
            var epl = "@name('s0') " +
                      expressionOne +
                      expressionTwo +
                      expressionThree +
                      " select callOne(),callTwo(1),callThree(1, 2) from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertEventNew(
                "s0",
                outEvent => {
                    foreach (var col in Arrays.AsList("callOne()", "callTwo(1)", "callThree(1,2)")) {
                        Assert.AreEqual(typeof(int?), outEvent.EventType.GetPropertyType(col));
                        Assert.AreEqual(1, outEvent.Get(col));
                    }
                });

            env.UndeployAll();
        }

        private static void TryImports(
            RegressionEnvironment env,
            string expression)
        {
            var epl = "@name('s0') " + expression + " select callOne() as val0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "val0.P00".SplitCsv(), new object[] { MyImportedClass.VALUE_P00 });

            env.UndeployAll();
        }

        private static void TryEnumeration(
            RegressionEnvironment env,
            string expression)
        {
            var epl = "@name('s0') " + expression + " select callIt().countOf(v => v<6) as val0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("val0")));

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { 2 });

            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "val0".SplitCsv(), new object[] { 2 });

            env.UndeployAll();
        }

        private static void TrySelect(
            RegressionEnvironment env,
            RegressionPath path,
            string scriptPart,
            string selectExpr,
            Type expectedType,
            object[][] testdata)
        {
            env.CompileDeploy(
                    "@name('s0') " +
                    scriptPart +
                    " select " +
                    selectExpr +
                    " as val from SupportBean as sb",
                    path)
                .AddListener("s0");
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(expectedType, statement.EventType.GetPropertyType("val")));

            for (var row = 0; row < testdata.Length; row++) {
                var theEvent = testdata[row][0];
                var expected = testdata[row][1];

                env.SendEventBean(theEvent);
                env.AssertEqualsNew("s0", "val", expected);
            }

            env.UndeployModuleContaining("s0");
        }

        private static void TryParseJS(
            RegressionEnvironment env,
            string js,
            Type type,
            object value)
        {
            env.CompileDeploy(
                    "@name('s0') expression js:getResultOne [" +
                    js +
                    "] " +
                    "select getResultOne() from SupportBean")
                .AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertStatement(
                "s0",
                statement => Assert.AreEqual(type, statement.EventType.GetPropertyType("getResultOne()")));
            env.AssertEqualsNew("s0", "getResultOne()", value);

            env.UndeployAll();
        }

        private static void TryAggregation(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create expression change(open, close) [ (open - close) / close ]", path);
            env.CompileDeploy(
                    "@name('s0') select change(first(IntPrimitive), last(IntPrimitive)) as ch from SupportBean#time(1 day)",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 1));
            env.AssertPropsNew("s0", "ch".SplitCsv(), new object[] { 0d });

            env.SendEventBean(new SupportBean("E2", 10));
            env.AssertPropsNew("s0", "ch".SplitCsv(), new object[] { -0.9d });

            env.UndeployAll();
        }

        private static void TryParseMVEL(
            RegressionEnvironment env,
            string mvelExpression,
            Type type,
            object value)
        {
            env.CompileDeploy(
                    "@name('s0') expression mvel:getResultOne [" +
                    mvelExpression +
                    "] " +
                    "select getResultOne() from SupportBean")
                .AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertPropsNew("s0", "getResultOne()".SplitCsv(), new object[] { value });
            env.UndeployAll();

            env.CompileDeploy(
                    "@name('s0') expression mvel:getResultOne [" +
                    mvelExpression +
                    "] " +
                    "expression mvel:getResultTwo [" +
                    mvelExpression +
                    "] " +
                    "select getResultOne() as val0, getResultTwo() as val1 from SupportBean")
                .AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(type, statement.EventType.GetPropertyType("val0"));
                    Assert.AreEqual(type, statement.EventType.GetPropertyType("val1"));
                });
            env.AssertPropsNew("s0", "val0,val1".SplitCsv(), new object[] { value, value });

            env.UndeployAll();
        }

        private static void TryCreateExpressionWArrayAllocate(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl = "@name('first') @public create expression double js:test(bar) [\n" +
                      "test(bar);\n" +
                      "function test(bar) {\n" +
                      "  var test=[];\n" +
                      "  return -1.0;\n" +
                      "}]\n";
            env.CompileDeploy(epl, path);

            env.CompileDeploy("@name('s0') select test('a') as c0 from SupportBean_S0", path).AddListener("s0");
            env.ListenerReset("s0");
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[] { -1d });

            env.UndeployAll();
        }

        private static void TryDeployArrayInScript(RegressionEnvironment env)
        {
            var epl = "expression string js:myFunc(arg) [\n" +
                      "  function replace(text, values, replacement){\n" +
                      "    return text.replace(replacement, values[0]);\n" +
                      "  }\n" +
                      "  replace(\"A B C\", [\"X\"], \"B\")\n" +
                      "]\n" +
                      "select\n" +
                      "myFunc(*)\n" +
                      "from SupportBean;";
            env.CompileDeploy(epl).UndeployAll();
        }

        private static void TryInvalidContains(
            RegressionEnvironment env,
            string expression,
            string part)
        {
            try {
                env.CompileWCheckedEx(expression);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                Assert.IsTrue(ex.Message.Contains(part), "Message not containing text '" + part + "' : " + ex.Message);
            }
        }
    }
} // end of namespace