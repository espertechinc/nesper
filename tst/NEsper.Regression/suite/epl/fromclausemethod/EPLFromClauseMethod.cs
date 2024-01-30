///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;
using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethod
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With2JoinHistoricalIndependentOuter(execs);
            With2JoinHistoricalSubordinateOuterMultiField(execs);
            With2JoinHistoricalSubordinateOuter(execs);
            With2JoinHistoricalOnlyDependent(execs);
            With2JoinHistoricalOnlyIndependent(execs);
            WithNoJoinIterateVariables(execs);
            WithOverloaded(execs);
            With2StreamMaxAggregation(execs);
            WithDifferentReturnTypes(execs);
            WithArrayNoArg(execs);
            WithArrayWithArg(execs);
            WithObjectNoArg(execs);
            WithObjectWithArg(execs);
            WithInvocationTargetEx(execs);
            WithStreamNameWContext(execs);
            WithWithMethodResultParam(execs);
            WithInvalid(execs);
            WithEventBeanArray(execs);
            WithUDFAndScriptReturningEvents(execs);
            With2JoinEventItselfProvidesMethod(execs);
            return execs;
        }

        public static IList<RegressionExecution> With2JoinEventItselfProvidesMethod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinEventItselfProvidesMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithUDFAndScriptReturningEvents(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodUDFAndScriptReturningEvents());
            return execs;
        }

        public static IList<RegressionExecution> WithEventBeanArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodEventBeanArray());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithWithMethodResultParam(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodWithMethodResultParam());
            return execs;
        }

        public static IList<RegressionExecution> WithStreamNameWContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodStreamNameWContext());
            return execs;
        }

        public static IList<RegressionExecution> WithInvocationTargetEx(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodInvocationTargetEx());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectWithArg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodObjectWithArg());
            return execs;
        }

        public static IList<RegressionExecution> WithObjectNoArg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodObjectNoArg());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayWithArg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodArrayWithArg());
            return execs;
        }

        public static IList<RegressionExecution> WithArrayNoArg(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodArrayNoArg());
            return execs;
        }

        public static IList<RegressionExecution> WithDifferentReturnTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodDifferentReturnTypes());
            return execs;
        }

        public static IList<RegressionExecution> With2StreamMaxAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2StreamMaxAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithOverloaded(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodOverloaded());
            return execs;
        }

        public static IList<RegressionExecution> WithNoJoinIterateVariables(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethodNoJoinIterateVariables());
            return execs;
        }

        public static IList<RegressionExecution> With2JoinHistoricalOnlyIndependent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyIndependent());
            return execs;
        }

        public static IList<RegressionExecution> With2JoinHistoricalOnlyDependent(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyDependent());
            return execs;
        }

        public static IList<RegressionExecution> With2JoinHistoricalSubordinateOuter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuter());
            return execs;
        }

        public static IList<RegressionExecution> With2JoinHistoricalSubordinateOuterMultiField(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField());
            return execs;
        }

        public static IList<RegressionExecution> With2JoinHistoricalIndependentOuter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalIndependentOuter());
            return execs;
        }

        private class EPLFromClauseMethod2JoinEventItselfProvidesMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "import " +
                          typeof(SupportEventWithStaticMethod).MaskTypeName() +
                          ";\n" +
                          "@public @buseventtype create schema SupportEventWithStaticMethod as SupportEventWithStaticMethod;\n" +
                          "@name('s0') select * from SupportEventWithStaticMethod as e, " +
                          "  method:SupportEventWithStaticMethod.ReturnLower() as lower,\n" +
                          "  method:SupportEventWithStaticMethod.ReturnUpper() as upper\n" +
                          "  where e.Value in [lower.Value:upper.Value]";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, 9, false);
                SendAssert(env, 10, true);
                SendAssert(env, 20, true);
                SendAssert(env, 21, false);

                env.UndeployAll();
            }

            private void SendAssert(
                RegressionEnvironment env,
                int value,
                bool expected)
            {
                env.SendEventBean(new SupportEventWithStaticMethod(value));
                env.AssertListenerInvokedFlag("s0", expected);
            }
        }

        private class EPLFromClauseMethodWithMethodResultParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean as e,\n" +
                          "method:" +
                          typeof(EPLFromClauseMethod).FullName +
                          ".GetWithMethodResultParam('somevalue', e, " +
                          typeof(EPLFromClauseMethod).FullName +
                          ".GetWithMethodResultParamCompute(true)) as s";
                env.CompileDeploy(epl).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", "s.P00,s.P01,s.P02".SplitCsv(), new object[] { "somevalue", "E1", "s0" });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodStreamNameWContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean as e,\n" +
                          "method:" +
                          typeof(EPLFromClauseMethod).FullName +
                          ".GetStreamNameWContext('somevalue', e) as s";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", "s.P00,s.P01,s.P02".SplitCsv(), new object[] { "somevalue", "E1", "s0" });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodUDFAndScriptReturningEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@buseventtype @public create schema ItemEvent(Id string)", path);

                var collections = typeof(Collections);
                var script =
                    "@name('script') @public create expression EventBean[] @type(ItemEvent) js:myItemProducerScript() [\n" +
                    "function myItemProducerScript() {" +
                    "  var EventBeanT = host.resolveType(\"com.espertech.esper.common.client.EventBean\");\n" +
                    "  var events = host.newArr(EventBeanT, 2);\n" +
                    "  events[0] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"Id\", \"id1\"), \"ItemEvent\");\n" +
                    "  events[1] = epl.EventBeanService.AdapterForMap(Collections.SingletonDataMap(\"Id\", \"id3\"), \"ItemEvent\");\n" +
                    "  return events;\n" +
                    "}\n" +
                    "return myItemProducerScript();" +
                    "]";
                env.CompileDeploy(script, path);

                TryAssertionUDFAndScriptReturningEvents(env, path, "MyItemProducerUDF");
                TryAssertionUDFAndScriptReturningEvents(env, path, "myItemProducerScript");

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodEventBeanArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@buseventtype @public create schema MyItemEvent(p0 string)", path);

                TryAssertionEventBeanArray(env, path, "EventBeanArrayForString", false);
                TryAssertionEventBeanArray(env, path, "EventBeanArrayForString", true);
                TryAssertionEventBeanArray(env, path, "EventBeanCollectionForString", false);
                TryAssertionEventBeanArray(env, path, "EventBeanIteratorForString", false);

                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchResult12(0) @type(ItemEvent)",
                    "The @type annotation is only allowed when the invocation target returns EventBean instances");

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodOverloaded : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionOverloaded(env, "", "A", "B");
                TryAssertionOverloaded(env, "10", "10", "B");
                TryAssertionOverloaded(env, "10, 20", "10", "20");
                TryAssertionOverloaded(env, "'x'", "x", "B");
                TryAssertionOverloaded(env, "'x', 50", "x", "50");
            }

            private static void TryAssertionOverloaded(
                RegressionEnvironment env,
                string @params,
                string expectedFirst,
                string expectedSecond)
            {
                var epl = "@name('s0') select col1, col2 from SupportBean, method:" +
                          typeof(SupportStaticMethodLib).FullName +
                          ".OverloadedMethodForJoin(" +
                          @params +
                          ")";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertPropsNew("s0", "col1,col2".SplitCsv(), new object[] { expectedFirst, expectedSecond });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2StreamMaxAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;
                var fields = "maxcol1".SplitCsv();

                // ESPER 556
                stmtText = "@name('s0') select max(col1) as maxcol1 from SupportBean#unique(TheString), method:" +
                           className +
                           ".FetchResult100() ";
                env.CompileDeploy(stmtText).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 9 } });

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 9 } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "@name('s0') select IntPrimitive,IntBoxed,col1,col2 from SupportBean#keepall " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult100() " +
                           "on IntPrimitive = col1 and IntBoxed = col2";

                var fields = "IntPrimitive,IntBoxed,col1,col2".SplitCsv();
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

                SendSupportBeanEvent(env, 2, 4);
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { 2, 4, 2, 4 } });
                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { 2, 4, 2, 4 } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2JoinHistoricalSubordinateOuter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.value) as s1 " +
                           "right outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.value = s1.value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.value) as s1 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.value = s1.value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.value) as s1 on s0.value = s1.value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);
            }
        }

        private class EPLFromClauseMethod2JoinHistoricalIndependentOuter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "valueOne,valueTwo".SplitCsv();
                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;

                stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 on s0.value = s1.value";
                env.CompileDeploy(stmtText).AddListener("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1, null }, new object[] { 2, 2 } });
                env.UndeployAll();

                stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 " +
                           "right outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.value = s1.value";
                env.CompileDeploy(stmtText);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1, null }, new object[] { 2, 2 } });
                env.UndeployAll();

                stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.value = s1.value";
                env.CompileDeploy(stmtText);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1, null }, new object[] { 2, 2 }, new object[] { null, 3 } });
                env.UndeployAll();

                stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 on s0.value = s1.value";
                env.CompileDeploy(stmtText);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { 1, null }, new object[] { 2, 2 }, new object[] { null, 3 } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethod2JoinHistoricalOnlyDependent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int lower", path);
                env.CompileDeploy("@public create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "select value,result from method:" +
                           className +
                           ".FetchBetween(lower, upper), " +
                           "method:" +
                           className +
                           ".FetchIdDelimited(value)";
                AssertJoinHistoricalOnlyDependent(env, path, stmtText);

                stmtText = "select value,result from " +
                           "method:" +
                           className +
                           ".FetchIdDelimited(value), " +
                           "method:" +
                           className +
                           ".FetchBetween(lower, upper)";
                AssertJoinHistoricalOnlyDependent(env, path, stmtText);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
        }

        private class EPLFromClauseMethod2JoinHistoricalOnlyIndependent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int lower", path);
                env.CompileDeploy("@public create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                var className = typeof(SupportStaticMethodLib).FullName;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" +
                           className +
                           ".FetchBetween(lower, upper) as s0, " +
                           "method:" +
                           className +
                           ".FetchBetweenString(lower, upper) as s1";
                AssertJoinHistoricalOnlyIndependent(env, path, stmtText);

                stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchBetweenString(lower, upper) as s1, " +
                           "method:" +
                           className +
                           ".FetchBetween(lower, upper) as s0 ";
                AssertJoinHistoricalOnlyIndependent(env, path, stmtText);

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodNoJoinIterateVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create variable int lower", path);
                env.CompileDeploy("@public create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                // Test int and singlerow
                var className = typeof(SupportStaticMethodLib).FullName;
                var stmtText = "@Name('s0') select value from method:" + className + ".FetchBetween(lower, upper)";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                env.AssertPropsPerRowIteratorAnyOrder("s0", new string[] { "value" }, null);

                SendSupportBeanEvent(env, 5, 10);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    new string[] { "value" },
                    new object[][] {
                        new object[] { 5 }, new object[] { 6 }, new object[] { 7 }, new object[] { 8 },
                        new object[] { 9 }, new object[] { 10 }
                    });

                env.Milestone(0);

                SendSupportBeanEvent(env, 10, 5);
                env.AssertPropsPerRowIteratorAnyOrder("s0", new string[] { "value" }, null);

                SendSupportBeanEvent(env, 4, 4);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    new string[] { "value" },
                    new object[][] { new object[] { 4 } });

                env.AssertListenerNotInvoked("s0");
                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodDifferentReturnTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionSingleRowFetch(env, "FetchMap(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "FetchMapEventBean(s1, 'TheString', 'IntPrimitive')");
                TryAssertionSingleRowFetch(env, "FetchObjectArrayEventBean(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "FetchPONOArray(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "FetchPONOCollection(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "FetchPONOIterator(TheString, IntPrimitive)");

                TryAssertionReturnTypeMultipleRow(env, "FetchMapArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchOAArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchPONOArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchMapCollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchOACollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchPONOCollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchMapIteratorMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchOAIteratorMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "FetchPONOIteratorMR(TheString, IntPrimitive)");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        private class EPLFromClauseMethodArrayNoArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = $"@Name('s0') select Id, TheString from SupportBean#length(3) as s1, method:{typeof(SupportStaticMethodLib).FullName}.FetchArrayNoArg";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                joinStatement = $"@Name('s0') select Id, TheString from SupportBean#length(3) as s1, method:{typeof(SupportStaticMethodLib).FullName}.FetchArrayNoArg()";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Id", "TheString");
                model.FromClause = FromClause.Create()
                    .Add(FilterStream.Create("SupportBean", "s1").AddView("length", Expressions.Constant(3)))
                    .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayNoArg"));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                Assert.AreEqual(joinStatement, model.ToEPL());

                TryArrayNoArg(env);
            }

            private static void TryArrayNoArg(RegressionEnvironment env)
            {
                var fields = new string[] { "Id", "TheString" };

                SendBeanEvent(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "1", "E1" });

                SendBeanEvent(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "1", "E2" });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodArrayWithArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement =
                    $"@Name('s0') select irstream Id, TheString " +
                    $"from SupportBean()#length(3) as s1, method:{typeof(SupportStaticMethodLib).FullName}.FetchArrayGen(IntPrimitive)";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                joinStatement =
                    $"@Name('s0') select irstream Id, TheString " +
                    $"from method:{typeof(SupportStaticMethodLib).FullName}.FetchArrayGen(IntPrimitive) as s0, SupportBean#length(3)";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Id", "TheString")
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);
                model.FromClause = FromClause.Create()
                    .Add(
                        MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayGen", "s0")
                            .AddParameter(Expressions.Property("IntPrimitive")))
                    .Add(FilterStream.Create("SupportBean").AddView("length", Expressions.Constant(3)));

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                Assert.AreEqual(joinStatement, model.ToEPL());

                TryArrayWithArg(env);
            }

            private static void TryArrayWithArg(RegressionEnvironment env)
            {
                var fields = new string[] { "Id", "TheString" };

                SendBeanEvent(env, "E1", -1);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E2", 0);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E3", 1);
                env.AssertPropsNew("s0", fields, new object[] { "A", "E3" });

                SendBeanEvent(env, "E4", 2);
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastNewData,
                            fields,
                            new object[][] { new object[] { "A", "E4" }, new object[] { "B", "E4" } });
                        Assert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });

                SendBeanEvent(env, "E5", 3);
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertPropsPerRow(
                            listener.LastNewData,
                            fields,
                            new object[][]
                                { new object[] { "A", "E5" }, new object[] { "B", "E5" }, new object[] { "C", "E5" } });
                        Assert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });

                SendBeanEvent(env, "E6", 1);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", "E6" } },
                    new object[][] { new object[] { "A", "E3" } });

                SendBeanEvent(env, "E7", 1);
                env.AssertPropsPerRowIRPair(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", "E7" } },
                    new object[][] { new object[] { "A", "E4" }, new object[] { "B", "E4" } });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodObjectNoArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = $"@name('s0') select Id, TheString from SupportBean()#length(3) as s1,  method:{typeof(SupportStaticMethodLib).FullName}.FetchObjectNoArg()";
                env.CompileDeploy(joinStatement).AddListener("s0");
                var fields = new string[] { "Id", "TheString" };

                SendBeanEvent(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "2", "E1" });

                env.Milestone(0);

                SendBeanEvent(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "2", "E2" });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodObjectWithArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@name('s0') select Id, TheString from " +
                                    "SupportBean()#length(3) as s1, " +
                                    " method:" +
                                    typeof(SupportStaticMethodLib).FullName +
                                    ".FetchObject(TheString)";
                env.CompileDeploy(joinStatement).AddListener("s0");

                var fields = new string[] { "Id", "TheString" };

                SendBeanEvent(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "|E1|", "E1" });

                SendBeanEvent(env, null);
                env.AssertListenerNotInvoked("s0");

                SendBeanEvent(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "|E2|", "E2" });

                env.UndeployAll();
            }
        }

        private class EPLFromClauseMethodInvocationTargetEx : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement =
                    $"select s1.TheString from SupportBean()#length(3) as s1, method:{typeof(SupportStaticMethodLib).FullName}.ThrowExceptionBeanReturn()";

                env.CompileDeploy(joinStatement);

                try {
                    SendBeanEvent(env, "E1");
                    Assert.Fail(); // default test configuration rethrows this exception
                }
                catch (EPException) {
                    // fine
                }

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        private class EPLFromClauseMethodInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".FetchArrayGen()",
                    "Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find static method named 'FetchArrayGen' in class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "' taking no parameters (nearest match found was 'FetchArrayGen' taking type(s) 'System.Int32') [");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:.abc where 1=2",
                    "Incorrect syntax near '.' at line 1 column 34, please check the method invocation join within the from clause [select * from SupportBean, method:.abc where 1=2]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchObjectAndSleep(1)",
                    "Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed 'System.Int32': Could not find static method named 'FetchObjectAndSleep' in class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "' with matching parameter number and expected parameter type(s) 'System.Int32' (nearest match found was 'FetchObjectAndSleep' taking type(s) 'System.String, System.Int32, System.Int64') [");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".Sleep(100) where 1=2",
                    "Invalid return type for static method 'Sleep' of class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "', expecting a type [select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".Sleep(100) where 1=2]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:AClass. where 1=2",
                    "Incorrect syntax near 'where' (a reserved keyword) expecting an identifier but found 'where' at line 1 column 42, please check the view specifications within the from clause [select * from SupportBean, method:AClass. where 1=2]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:Dummy.abc where 1=2",
                    "Could not load class by name 'Dummy', please check imports [select * from SupportBean, method:Dummy.abc where 1=2]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:Math where 1=2",
                    "A function named 'Math' is not defined");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2",
                    "Method data joins do not allow views onto the data, view 'length' is not valid in this context [select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".dummy where 1=2",
                    "Could not find public static method named 'dummy' in class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "' [");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".MinusOne(10) where 1=2",
                    "Invalid return type for static method 'MinusOne' of class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "', expecting a type [");

                env.TryInvalidCompile(
                    "select * from SupportBean, xyz:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchArrayNoArg() where 1=2",
                    "Expecting keyword 'method', found 'xyz' [select * from SupportBean, xyz:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchArrayNoArg() where 1=2]");

                env.TryInvalidCompile(
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchBetween(s1.value, s1.value) as s0, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchBetween(s0.value, s0.value) as s1",
                    "Circular dependency detected between historical streams [");

                env.TryInvalidCompile(
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchBetween(s0.value, s0.value) as s0, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchBetween(s0.value, s0.value) as s1",
                    "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

                env.TryInvalidCompile(
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".FetchBetween(s0.value, s0.value) as s0",
                    "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

                env.TryInvalidCompile(
                    "select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()",
                    "Could not find getter method for method invocation, expected a method by name 'ReadRowNoMetadataMetadata' accepting no parameters [select * from method:SupportMethodInvocationJoinInvalid.ReadRowNoMetadata()]");

                env.TryInvalidCompile(
                    "select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()",
                    "Getter method 'ReadRowWrongMetadataMetadata' does not return " + typeof(IDictionary<string, object>).CleanName() + " [select * from method:SupportMethodInvocationJoinInvalid.ReadRowWrongMetadata()]");

                env.TryInvalidCompile(
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).FullName +
                    ".InvalidOverloadForJoin(null)",
                    "Method by name 'InvalidOverloadForJoin' is overloaded in class '" +
                    typeof(SupportStaticMethodLib).FullName +
                    "' and overloaded methods do not return the same type");
            }
        }

        private static void TryAssertionUDFAndScriptReturningEvents(
            RegressionEnvironment env,
            RegressionPath path,
            string methodName)
        {
            env.CompileDeploy("@name('s0') select Id from SupportBean, method:" + methodName, path).AddListener("s0");

            env.SendEventBean(new SupportBean());
            env.AssertPropsPerRowLastNew(
                "s0",
                "Id".SplitCsv(),
                new object[][] { new object[] { "id1" }, new object[] { "id3" } });

            env.UndeployModuleContaining("s0");
        }

        private static void TryAssertionEventBeanArray(
            RegressionEnvironment env,
            RegressionPath path,
            string methodName,
            bool soda)
        {
            var epl = $"@name('s0') select p0 from SupportBean, method:{typeof(SupportStaticMethodLib).FullName}.{methodName}(TheString) @type(MyItemEvent)";
            env.CompileDeploy(soda, epl, path).AddListener("s0");

            env.SendEventBean(new SupportBean("a,b", 0));
            env.AssertPropsPerRowLastNew(
                "s0",
                "p0".SplitCsv(),
                new object[][] { new object[] { "a" }, new object[] { "b" } });

            env.UndeployModuleContaining("s0");
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
        }

        private static void SendBeanEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            int intPrimitive,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        public static EventBean[] MyItemProducerUDF(EPLMethodInvocationContext context)
        {
            var events = new EventBean[2];
            var count = 0;
            foreach (var id in "id1,id3".SplitCsv()) {
                events[count++] = context.EventBeanService.AdapterForMap(
                    Collections.SingletonDataMap("Id", id),
                    "ItemEvent");
            }

            return events;
        }

        private static void AssertJoinHistoricalSubordinateOuter(
            RegressionEnvironment env,
            string expression)
        {
            var fields = "valueOne,valueTwo".SplitCsv();
            env.CompileDeploy("@name('s0') " + expression).AddListener("s0");
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 1, null }, new object[] { 2, 2 } });
            env.UndeployAll();
        }

        private static void AssertJoinHistoricalOnlyDependent(
            RegressionEnvironment env,
            RegressionPath path,
            string expression)
        {
            env.CompileDeploy("@name('s0') " + expression, path).AddListener("s0");

            var fields = "value,result".SplitCsv();
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendSupportBeanEvent(env, 5, 5);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { 5, "|5|" } });

            SendSupportBeanEvent(env, 1, 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 1, "|1|" }, new object[] { 2, "|2|" } });

            SendSupportBeanEvent(env, 0, -1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendSupportBeanEvent(env, 4, 6);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] { new object[] { 4, "|4|" }, new object[] { 5, "|5|" }, new object[] { 6, "|6|" } });

            var listener = env.Listener("s0");
            env.UndeployModuleContaining("s0");

            SendSupportBeanEvent(env, 0, -1);
            Assert.IsFalse(listener.IsInvoked);
        }

        private static void AssertJoinHistoricalOnlyIndependent(
            RegressionEnvironment env,
            RegressionPath path,
            string expression)
        {
            env.CompileDeploy("@name('s0') " + expression, path).AddListener("s0");

            var fields = "valueOne,valueTwo".SplitCsv();
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            SendSupportBeanEvent(env, 5, 5);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { 5, "5" } });

            SendSupportBeanEvent(env, 1, 2);
            env.AssertPropsPerRowIteratorAnyOrder(
                "s0",
                fields,
                new object[][] {
                    new object[] { 1, "1" }, new object[] { 1, "2" }, new object[] { 2, "1" }, new object[] { 2, "2" }
                });

            SendSupportBeanEvent(env, 0, -1);
            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

            env.UndeployModuleContaining("s0");
        }

        private static void TryAssertionSingleRowFetch(
            RegressionEnvironment env,
            string method)
        {
            var epl = "@name('s0') select TheString, IntPrimitive, mapstring, mapint from " +
                      "SupportBean as s1, " +
                      "method:" +
                      typeof(SupportStaticMethodLib).FullName +
                      "." +
                      method;
            env.CompileDeploy(epl).AddListener("s0");

            var fields = new string[] { "TheString", "IntPrimitive", "mapstring", "mapint" };

            SendBeanEvent(env, "E1", 1);
            env.AssertPropsNew("s0", fields, new object[] { "E1", 1, "|E1|", 2 });

            SendBeanEvent(env, "E2", 3);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 3, "|E2|", 4 });

            SendBeanEvent(env, "E3", 0);
            env.AssertPropsNew("s0", fields, new object[] { "E3", 0, null, null });

            SendBeanEvent(env, "E4", -1);
            env.AssertListenerNotInvoked("s0");

            env.UndeployAll();
        }

        private static void TryAssertionReturnTypeMultipleRow(
            RegressionEnvironment env,
            string method)
        {
            var epl = "@name('s0') select TheString, IntPrimitive, mapstring, mapint from " +
                      "SupportBean#keepall as s1, " +
                      "method:" +
                      typeof(SupportStaticMethodLib).FullName +
                      "." +
                      method;
            var fields = "TheString,IntPrimitive,mapstring,mapint".SplitCsv();
            env.CompileDeploy(epl).AddListener("s0");

            env.AssertPropsPerRowIterator("s0", fields, null);

            SendBeanEvent(env, "E1", 0);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIterator("s0", fields, null);

            SendBeanEvent(env, "E2", -1);
            env.AssertListenerNotInvoked("s0");
            env.AssertPropsPerRowIterator("s0", fields, null);

            SendBeanEvent(env, "E3", 1);
            env.AssertPropsNew("s0", fields, new object[] { "E3", 1, "|E3_0|", 100 });
            env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E3", 1, "|E3_0|", 100 } });

            SendBeanEvent(env, "E4", 2);
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] { new object[] { "E4", 2, "|E4_0|", 100 }, new object[] { "E4", 2, "|E4_1|", 101 } });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E3", 1, "|E3_0|", 100 }, new object[] { "E4", 2, "|E4_0|", 100 },
                    new object[] { "E4", 2, "|E4_1|", 101 }
                });

            SendBeanEvent(env, "E5", 3);
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E5", 3, "|E5_0|", 100 }, new object[] { "E5", 3, "|E5_1|", 101 },
                    new object[] { "E5", 3, "|E5_2|", 102 }
                });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] {
                    new object[] { "E3", 1, "|E3_0|", 100 },
                    new object[] { "E4", 2, "|E4_0|", 100 }, new object[] { "E4", 2, "|E4_1|", 101 },
                    new object[] { "E5", 3, "|E5_0|", 100 }, new object[] { "E5", 3, "|E5_1|", 101 },
                    new object[] { "E5", 3, "|E5_2|", 102 }
                });

            env.UndeployAll();
        }

        public static SupportBean_S0 GetStreamNameWContext(
            string a,
            SupportBean bean,
            EPLMethodInvocationContext context)
        {
            return new SupportBean_S0(1, a, bean.TheString, context.StatementName);
        }

        public static SupportBean_S0 GetWithMethodResultParam(
            string a,
            SupportBean bean,
            string b)
        {
            return new SupportBean_S0(1, a, bean.TheString, b);
        }

        public static string GetWithMethodResultParamCompute(
            bool param,
            EPLMethodInvocationContext context)
        {
            return context.StatementName;
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        public class SupportEventWithStaticMethod
        {
            private readonly int value;

            public SupportEventWithStaticMethod(int value)
            {
                this.value = value;
            }

            public int GetValue()
            {
                return value;
            }

            public static SupportEventWithStaticMethodValue ReturnLower()
            {
                return new SupportEventWithStaticMethodValue(10);
            }

            public static SupportEventWithStaticMethodValue ReturnUpper()
            {
                return new SupportEventWithStaticMethodValue(20);
            }
        }

        /// <summary>
        /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
        /// </summary>
        public class SupportEventWithStaticMethodValue
        {
            public SupportEventWithStaticMethodValue(int value)
            {
                this.Value = value;
            }

            public int Value { get; }
        }
    }
} // end of namespace