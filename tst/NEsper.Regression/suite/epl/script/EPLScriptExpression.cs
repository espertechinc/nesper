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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.script;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.script
{
    public class EPLScriptExpression
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithScripts(execs);
            WithQuoteEscape(execs);
            WithScriptReturningEvents(execs);
            WithDocSamples(execs);
            WithInvalidRegardlessDialect(execs);
            WithInvalidScriptJS(execs);
            WithParserMVELSelectNoArgConstant(execs);
            WithJavaScriptStatelessReturnPassArgs(execs);
            WithSubqueryParam(execs);
            WithReturnNullWhenNumeric(execs);
            WithGenericResultType(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGenericResultType(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptGenericResultType());
            return execs;
        }

        public static IList<RegressionExecution> WithReturnNullWhenNumeric(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptReturnNullWhenNumeric());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryParam(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptSubqueryParam());
            return execs;
        }

        public static IList<RegressionExecution> WithJavaScriptStatelessReturnPassArgs(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptJavaScriptStatelessReturnPassArgs());
            return execs;
        }

        public static IList<RegressionExecution> WithParserMVELSelectNoArgConstant(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptParserMVELSelectNoArgConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidScriptJS(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptInvalidScriptJS());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidRegardlessDialect(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptInvalidRegardlessDialect());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSamples(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptDocSamples());
            return execs;
        }

        public static IList<RegressionExecution> WithScriptReturningEvents(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptScriptReturningEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithQuoteEscape(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptQuoteEscape());
            return execs;
        }

        public static IList<RegressionExecution> WithScripts(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLScriptScripts());
            return execs;
        }

        private static void RunAssertionScriptReturningEvents(
            RegressionEnvironment env,
            bool soda)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@Name('type') create schema ItemEvent(Id string)", path);

            var script =
                "@Name('script') create expression EventBean[] @type(ItemEvent) js:myScriptReturnsEvents() [\n" +
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
            Assert.AreEqual(
                StatementType.CREATE_EXPRESSION,
                env.Statement("script").GetProperty(StatementProperty.STATEMENTTYPE));
            Assert.AreEqual(
                "myScriptReturnsEvents",
                env.Statement("script").GetProperty(StatementProperty.CREATEOBJECTNAME));

            env.CompileDeploy(
                "@Name('s0') select myScriptReturnsEvents().where(v => v.Id in ('id1', 'id3')) as c0 from SupportBean",
                path);
            env.AddListener("s0");

            env.SendEventBean(new SupportBean());
            var raw = env.Listener("s0").AssertOneGetNewAndReset();
            var coll = raw.Get("c0").Unwrap<IDictionary<string, object>>();
            EPAssertionUtil.AssertPropsPerRow(
                coll.ToArray(),
                new[] {"Id"},
                new[] {
                    new object[] {"id1"},
                    new object[] {"id3"}
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

            expression = "expression void " + dialect + ":mysetter() [ epl.SetScriptAttribute('a', 1); ]";
            testData = new[] {
                new object[] {new SupportBean("E1", 20), null},
                new object[] {new SupportBean("E1", 10), null}
            };
            TrySelect(env, path, expression, "mysetter()", typeof(object), testData);

            env.UndeployAll();
        }

        private static void TrySetScriptProp(
            RegressionEnvironment env,
            string dialect)
        {
            env.CompileDeploy(
                $"@Name('s0') " +
                $"expression {dialect}:getFlag() [ return epl.GetScriptAttribute('flag'); ] " +
                $"expression boolean {dialect}:setFlag(flagValue) [ epl.SetScriptAttribute('flag', flagValue); return flagValue; ]" +
                "select getFlag() as val from SupportBean(TheString = 'E1' or setFlag(IntPrimitive > 0))");
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("E2", 10));
            Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

            env.UndeployAll();
        }

        private static void TryPassVariable(
            RegressionEnvironment env,
            string dialect)
        {
            object[][] testData;
            string expression;

            var path = new RegressionPath();
            env.CompileDeploy("@Name('var') create variable long THRESHOLD = 100", path);

            expression = "expression long " + dialect + ":thresholdAdder(numToAdd, th) [ return th + numToAdd; ]";
            testData = new[] {
                new object[] {new SupportBean("E1", 20), 120L},
                new object[] {new SupportBean("E1", 10), 110L}
            };
            TrySelect(env, path, expression, "thresholdAdder(IntPrimitive, THRESHOLD)", typeof(long?), testData);

            env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "THRESHOLD", 1);
            testData = new[] {
                new object[] {new SupportBean("E1", 20), 21L},
                new object[] {new SupportBean("E1", 10), 11L}
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

            expression = "expression int " + dialect + ":callIt(bean) [ return bean.GetIntPrimitive() + 1; ]";
            testData = new[] {
                new object[] {new SupportBean("E1", 20), 21},
                new object[] {new SupportBean("E1", 10), 11}
            };
            TrySelect(env, path, expression, "callIt(sb)", typeof(int?), testData);

            env.UndeployAll();
        }

        private static void TryReturnObject(
            RegressionEnvironment env,
            string dialect)
        {
            var expression =
                $"@Name('s0') expression {typeof(SupportBean).FullName} {dialect}" +
                $":callIt() [ var beanType = host.resolveType('{typeof(SupportBean).FullName}'); return host.newObj(beanType, 'E1', 10); ]";
            env.CompileDeploy(
                    expression + " select callIt() as val0, callIt().GetTheString() as val1 from SupportBean as sb")
                .AddListener("s0");
            Assert.AreEqual(typeof(SupportBean), env.Statement("s0").EventType.GetPropertyType("val0"));

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0.TheString", "val0.IntPrimitive", "val1"},
                new object[] {"E1", 10, "E1"});

            env.UndeployAll();
        }

        private static void TryDatetime(
            RegressionEnvironment env,
            string dialect)
        {
            var dayOfWeek = (int) DayOfWeek.Thursday;
            var msecDate = DateTimeParsingFunctions.ParseDefaultMSec("2002-05-30T09:00:00.000");
            var expression = $"expression long {dialect}:callIt() [return {msecDate}]";
            var epl = $"@Name('s0') {expression} select callIt().getHourOfDay() as val0, callIt().getDayOfWeek() as val1 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("val0"));

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0", "val1"},
                new object[] {9, dayOfWeek});

            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0", "val1"},
                new object[] {9, dayOfWeek});

            env.UndeployAll();
        }

        private static void TryNested(
            RegressionEnvironment env,
            string dialect)
        {
            var epl = "@Name('s0') " +
                      $"expression int {dialect}:abc(p1, p2) [return p1*p2*10]\n" +
                      $"expression int {dialect}:abc(p1) [return p1*10]\n" +
                      "select abc(abc(2), 5) as c0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"c0"},
                new object[] {1000});

            env.UndeployAll();
        }

        private static void TryReturnTypes(
            RegressionEnvironment env,
            string dialect)
        {
            var expression = "return 'x'";
            var epl = 
                $"@Name('s0') expression string {dialect}:one() [{expression}]\n" +
                "select one() as c0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("c0"));

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"c0"},
                new object[] {"x"});

            env.UndeployAll();
        }

        private static void TryOverloaded(
            RegressionEnvironment env,
            string dialect)
        {
            var epl =
                "@Name('s0') " +
                $"expression int {dialect}:abc() [return 10;]\n" +
                $"expression int {dialect}:abc(p1) [return p1*10;]\n" +
                $"expression int {dialect}:abc(p1, p2) [return p1*p2*10]\n" +
                "select abc() as c0, abc(2) as c1, abc(2,3) as c2 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"c0", "c1", "c2"},
                new object[] {10, 20, 60});

            env.UndeployAll();
        }

        private static void TryUnnamedInSelectClause(
            RegressionEnvironment env,
            string dialect)
        {
            var expressionOne = "expression int " + dialect + ":callOne() [return 1] ";
            var expressionTwo = "expression int " + dialect + ":callTwo(a) [return 1] ";
            var expressionThree = "expression int " + dialect + ":callThree(a,b) [return 1] ";
            var epl = "@Name('s0') " +
                      expressionOne +
                      expressionTwo +
                      expressionThree +
                      " select callOne(),callTwo(1),callThree(1, 2) from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            var outEvent = env.Listener("s0").AssertOneGetNewAndReset();
            foreach (var col in Arrays.AsList("callOne()", "callTwo(1)", "callThree(1,2)")) {
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(col));
                Assert.AreEqual(1, outEvent.Get(col));
            }

            env.UndeployAll();
        }

        private static void TryImports(
            RegressionEnvironment env,
            string expression)
        {
            var epl = "@Name('s0') " + expression + " select callOne() as val0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0.P00"},
                new object[] {MyImportedClass.VALUE_P00});

            env.UndeployAll();
        }

        private static void TryEnumeration(
            RegressionEnvironment env,
            string expression)
        {
            var epl = "@Name('s0') " + expression + " select callIt().countOf(v => v<6) as val0 from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");
            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("val0"));

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0"},
                new object[] {2});

            env.UndeployAll();

            env.EplToModelCompileDeploy(epl).AddListener("s0");
            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0"},
                new object[] {2});

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
                    $"@Name('s0') {scriptPart} select {selectExpr} as val from SupportBean as sb",
                    path)
                .AddListener("s0");
            Assert.AreEqual(expectedType, env.Statement("s0").EventType.GetPropertyType("val"));

            for (var row = 0; row < testdata.Length; row++) {
                var theEvent = testdata[row][0];
                var expected = testdata[row][1];

                env.SendEventBean(theEvent);
                var outEvent = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(expected, outEvent.Get("val"));
            }

            env.UndeployModuleContaining("s0");
        }

        private static void TryParseJS(
            RegressionEnvironment env,
            string js,
            Type type,
            object value)
        {
            var epl = $"@Name('s0') expression js:getResultOne [return ({js})] select getResultOne() from SupportBean"; 
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean());
            Assert.AreEqual(type, env.Statement("s0").EventType.GetPropertyType("getResultOne()"));
            var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
            var theEventValue = theEvent.Get("getResultOne()");
            Assert.AreEqual(value, theEventValue);
            env.UndeployAll();
        }

        private static void TryAggregation(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "create expression change(open, close) [ return (open - close) / close; ]\n",
                path);
            env.CompileDeploy(
                    "@Name('s0') select change(first(IntPrimitive), last(IntPrimitive)) as ch from SupportBean#time(1 day)",
                    path)
                .AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 1500));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"ch"},
                new object[] {0});

            env.SendEventBean(new SupportBean("E2", 80)); // first(1500), last(80) = 1420 / 80 = 17.75
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"ch"},
                new object[] { 17.75f });

            env.UndeployAll();
        }

        private static void TryParseMVEL(
            RegressionEnvironment env,
            string mvelExpression,
            Type type,
            object value)
        {
            env.CompileDeploy(
                    "@Name('s0') expression mvel:getResultOne [" +
                    mvelExpression +
                    "] " +
                    "select getResultOne() from SupportBean")
                .AddListener("s0");

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"getResultOne()"},
                new[] {value});
            env.UndeployAll();

            env.CompileDeploy(
                    "@Name('s0') expression mvel:getResultOne [" +
                    mvelExpression +
                    "] " +
                    "expression mvel:getResultTwo [" +
                    mvelExpression +
                    "] " +
                    "select getResultOne() as val0, getResultTwo() as val1 from SupportBean")
                .AddListener("s0");

            env.SendEventBean(new SupportBean());
            Assert.AreEqual(type, env.Statement("s0").EventType.GetPropertyType("val0"));
            Assert.AreEqual(type, env.Statement("s0").EventType.GetPropertyType("val1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"val0", "val1"},
                new[] {value, value});

            env.UndeployAll();
        }

        private static void TryCreateExpressionWArrayAllocate(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            var epl =
                "@Name('first') create expression double js:test(bar) [\n" +
                "function test(bar) {\n" +
                "  var test=[];\n" +
                "  return -1.0;\n" +
                "}\n" +
                "return test(bar);\n" +
                "]\n";
            env.CompileDeploy(epl, path);

            env.CompileDeploy("@Name('s0') select test('a') as c0 from SupportBean_S0", path).AddListener("s0");
            env.Listener("s0").Reset();
            env.SendEventBean(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new[] {"c0"},
                new object[] {-1d});

            env.UndeployAll();
        }

        private static void TryDeployArrayInScript(RegressionEnvironment env)
        {
            var epl = "expression string js:myFunc(arg) [\n" +
                      "  function replace(text, values, replacement){\n" +
                      "    return text.replace(replacement, values[0]);\n" +
                      "  }\n" +
                      "  return replace(\"A B C\", [\"X\"], \"B\");\n" +
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
                Assert.That(ex.Message, Contains.Substring(part));
                //Assert.IsTrue(ex.Message.Contains(part), "Message not containing text '" + part + "' : " + ex.Message);
            }
        }

        internal class EPLScriptGenericResultType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@name('s0') expression System.Collections.Generic.IList<String> js:myJSFunc(stringvalue) [\n" +
                             "  function doSomething(stringvalue) {\n" +
                             "    return null;\n" +
                             "  }\n" +
                             "  return doSomething(stringvalue);\n" +
                             "]\n" +
                             "select myJSFunc('test') as c0 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(IList<String>), env.Statement("s0").EventType.GetPropertyType("c0"));

                env.UndeployAll();
            }
        }

        internal class EPLScriptReturnNullWhenNumeric : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create schema Event(host string); " +
                             "create window DnsTrafficProfile#time(5 minutes) (host string); " +
                             "expression double js:doSomething(p) [ " +
                             "function doSomething(p) { " +
                             "  Console.Out.WriteLine(p);" +
                             "  Console.Out.WriteLine(p.Length);" +
                             " } " +
                             "doSomething(p); " +
                             "] " +
                             "@Name('out') select doSomething((select window(z.*) from DnsTrafficProfile as z)) as score from DnsTrafficProfile;" +
                             "insert into DnsTrafficProfile select * from Event; ";
                env.CompileDeployWBusPublicType(epl, new RegressionPath());
                env.AddListener("out");

                var @event = new Dictionary<string, object>();
                @event.Put("host", "test.domain.com");
                env.SendEventMap(@event, "Event");

                env.UndeployAll();
            }
        }

        internal class EPLScriptSubqueryParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') expression double js:myJSFunc(stringvalue) [\n" +
                    "  function calcScore(stringvalue) {\n" +
                    "    return parseFloat(stringvalue);\n" +
                    "  }\n" +
                    "  return calcScore(stringvalue);\n" +
                    "]\n" +
                    "select myJSFunc((select TheString from SupportBean#lastevent)) as c0 from SupportBean_S0";
                env.CompileDeploy(epl).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("20", 0));
                env.SendEventBean(new SupportBean_S0(0));
                Assert.AreEqual(20d, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.SendEventBean(new SupportBean("30", 0));
                env.SendEventBean(new SupportBean_S0(1));
                Assert.AreEqual(30d, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }

        internal class EPLScriptQuoteEscape : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplSLComment = "create expression f(params)[\n" +
                                   "  // I'am...\n" +
                                   "];";
                env.CompileDeploy(eplSLComment);

                var eplMLComment = "create expression g(params)[\n" +
                                   "  /* I'params am[] */" +
                                   "];";
                env.CompileDeploy(eplMLComment);

                env.UndeployAll();
            }
        }

        internal class EPLScriptScriptReturningEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionScriptReturningEvents(env, false);
                RunAssertionScriptReturningEvents(env, true);

                var path = new RegressionPath();
                env.CompileDeploy("create schema ItemEvent(Id string)", path);
                TryInvalidCompile(
                    env,
                    path,
                    "expression double @type(ItemEvent) fib(num) [] select fib(1) from SupportBean",
                    "Failed to validate select-clause expression 'fib(1)': The @type annotation is only allowed when the invocation target returns EventBean instances");
                env.UndeployAll();
            }
        }

        internal class EPLScriptDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                /*
                epl = "@Name('s0') expression double fib(num) [" +
                      "function fib(n) { " +
                      "  if(n <= 1) " +
                      "    return n; " +
                      "  return fib(n-1) + fib(n-2); " +
                      "};" +
                      "return fib(num); " +
                      "]" +
                      "select fib(IntPrimitive) from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();
                */

                epl = "@Name('s0') expression js:printColors(colorEvent) [\n" +
                      "  debug.Print(debug.Render(colorEvent.Colors));\n" +
                      "]" +
                      "select printColors(colorEvent) from SupportColorEvent as colorEvent";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportColorEvent());
                env.UndeployAll();

                epl = "@Name('s0') expression boolean js:setFlag(name, value, returnValue) [\n" +
                      "  if (returnValue) epl.SetScriptAttribute(name, value);\n" +
                      "  return returnValue;\n" +
                      "]\n" +
                      "expression js:getFlag(name) [\n" +
                      "  return epl.GetScriptAttribute(name);\n" +
                      "]\n" +
                      "select getFlag('loc') as flag from SupportRFIDSimpleEvent(Zone = 'Z1' and \n" +
                      "  (setFlag('loc', true, Loc = 'A') or setFlag('loc', false, Loc = 'B')) )";
                env.CompileDeploy(epl);
                env.UndeployAll();
            }
        }

        internal class EPLScriptInvalidRegardlessDialect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // parameter defined twice
                TryInvalidCompile(
                    env,
                    "expression js:abc(p1, p1) [/* text */] select * from SupportBean",
                    "Invalid script parameters for script 'abc', parameter 'p1' is defined more then once [expression js:abc(p1, p1) [/* text */] select * from SupportBean]");

                // invalid dialect
                TryInvalidCompile(
                    env,
                    "expression dummy:abc() [10] select * from SupportBean",
                    "Failed to obtain script engine for dialect 'dummy' for script 'abc' [expression dummy:abc() [10] select * from SupportBean]");

                // not found
                TryInvalidCompile(
                    env,
                    "select abc() from SupportBean",
                    "Failed to validate select-clause expression 'abc()': Unknown single-row function, expression declaration, script or aggregation function named 'abc' could not be resolved [select abc() from SupportBean]");

                // test incorrect number of parameters
                TryInvalidCompile(
                    env,
                    "expression js:abc() [10] select abc(1) from SupportBean",
                    "Failed to validate select-clause expression 'abc(1)': Invalid number of parameters for script 'abc', expected 0 parameters but received 1 parameters [expression js:abc() [10] select abc(1) from SupportBean]");

                // test expression name overlap
                TryInvalidCompile(
                    env,
                    "expression js:abc() [10] expression js:abc() [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:abc() [10] expression js:abc() [10] select abc() from SupportBean]");

                // test expression name overlap with parameters
                TryInvalidCompile(
                    env,
                    "expression js:abc(p1) [10] expression js:abc(p2) [10] select abc() from SupportBean",
                    "Script name 'abc' has already been defined with the same number of parameters [expression js:abc(p1) [10] expression js:abc(p2) [10] select abc() from SupportBean]");

                // test script name overlap with expression declaration
                TryInvalidCompile(
                    env,
                    "expression js:abc() [10] expression abc {10} select abc() from SupportBean",
                    "Script name 'abc' overlaps with another expression of the same name [expression js:abc() [10] expression abc {10} select abc() from SupportBean]");

                // fails to resolve return type
                TryInvalidCompile(
                    env,
                    "expression dummy js:abc() [10] select abc() from SupportBean",
                    "Failed to validate select-clause expression 'abc()': Failed to resolve return type 'dummy' specified for script 'abc' [expression dummy js:abc() [10] select abc() from SupportBean]");
            }
        }

        internal class EPLScriptInvalidScriptJS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidContains(
                    env,
                    "expression js:abc[dummy abc = 1;] select * from SupportBean",
                    "Error during compilation: SyntaxError: Unexpected identifier");
                    //"Expected ';'");

                TryInvalidContains(
                    env,
                    "expression js:abc(aa) [return aa..bb(1);] select abc(1) from SupportBean",
                    "Error during compilation: SyntaxError: Unexpected token '.'");
                    //"Expected identifier");

                TryInvalidCompile(
                    env,
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
                    Assert.That(ex.Message, Contains.Substring("Unexpected exception executing script 'abc':"));
                }

                // execution problem
                env.UndeployAll();
                env.CompileDeploy("expression js:abc[dummy;] select * from SupportBean#keepall where abc() = 1");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.That(ex.Message, Contains.Substring("Unexpected exception executing script 'abc':"));
                }

                // execution problem
                env.UndeployAll();
                env.CompileDeploy(
                        "@Name('ABC') expression int[] js:callIt() [ var myarr = new Array(2, 8, 5, 9); return myarr; ]" +
                        " select callIt().countOf(v => v < 6) from SupportBean")
                    .AddListener("ABC");
                try {
                    env.SendEventBean(new SupportBean());
                    Assert.Fail();
                }
                catch (Exception ex) {
                    Assert.That(ex.Message, Contains.Substring("Unexpected exception in statement 'ABC': "));
                }

                env.UndeployAll();
            }
        }

        internal class EPLScriptScripts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // test different return types
                TryReturnTypes(env, "js");

                // test void return type
                TryVoidReturnType(env, "js");

                // test enumeration method
                // Not supported: tryEnumeration("expression int[] js:callIt() [ var myarr = new Array(2, 8, 5, 9); myarr; ]"); returns NativeArray which is a Rhino-specific array wrapper

                // test script props
                TrySetScriptProp(env, "js");

                // test variable
                TryPassVariable(env, "js");

                // test passing an event
                TryPassEvent(env, "js");

                // test returning an object
                TryReturnObject(env, "js");

                // test datetime method
                TryDatetime(env, "js");

                // test unnamed expression
                TryUnnamedInSelectClause(env, "js");

                // test import
                TryImports(
                    env,
                    "expression MyImportedClass js:callOne() [ " +
                    "var myClass = host.resolveType('" + typeof(MyImportedClass).FullName + "');\n" +
                    "return host.newObj(myClass);\n" +
                    "]");

                // test overloading script
                TryOverloaded(env, "js");

                // test nested invocation
                TryNested(env, "js");

                TryAggregation(env);

                TryDeployArrayInScript(env);

                TryCreateExpressionWArrayAllocate(env);
            }
        }

        internal class EPLScriptParserMVELSelectNoArgConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryParseJS(env, "\n\t  10.0    \n\n\t\t", typeof(object), 10);
                TryParseJS(env, "10.0", typeof(object), 10);
                TryParseJS(env, "5*5.0", typeof(object), 25);
                TryParseJS(env, "\"abc\"", typeof(object), "abc");
                TryParseJS(env, " \"abc\"     ", typeof(object), "abc");
                TryParseJS(env, "'def'", typeof(object), "def");
                TryParseJS(env, " 'def' ", typeof(object), "def");
            }
        }

        internal class EPLScriptJavaScriptStatelessReturnPassArgs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                object[][] testData;
                string expression;
                var path = new RegressionPath();

                expression = "function fib(n) {" +
                             "  if(n <= 1) return n; " +
                             "  return fib(n-1) + fib(n-2); " +
                             "};" +
                             "return fib(num);";
                testData = new[] {
                    new object[] {new SupportBean("E1", 20), 6765.0}
                };
                TrySelect(
                    env,
                    path,
                    "expression double js:abc(num) [ " + expression + " ]",
                    "abc(IntPrimitive)",
                    typeof(double?),
                    testData);
                path.Clear();

                testData = new[] {
                    new object[] {new SupportBean("E1", 5), 50.0},
                    new object[] {new SupportBean("E1", 6), 60.0}
                };
                TrySelect(
                    env,
                    path,
                    "expression js:abc(myint) [ return myint * 10; ]",
                    "abc(IntPrimitive)",
                    typeof(object),
                    testData);
                path.Clear();
            }
        }
    }
} // end of namespace