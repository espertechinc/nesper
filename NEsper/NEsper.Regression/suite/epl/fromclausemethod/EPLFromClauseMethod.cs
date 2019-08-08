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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
    public class EPLFromClauseMethod
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLFromClauseMethod2JoinHistoricalIndependentOuter());
            execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField());
            execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuter());
            execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyDependent());
            execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyIndependent());
            execs.Add(new EPLFromClauseMethodNoJoinIterateVariables());
            execs.Add(new EPLFromClauseMethodOverloaded());
            execs.Add(new EPLFromClauseMethod2StreamMaxAggregation());
            execs.Add(new EPLFromClauseMethodDifferentReturnTypes());
            execs.Add(new EPLFromClauseMethodArrayNoArg());
            execs.Add(new EPLFromClauseMethodArrayWithArg());
            execs.Add(new EPLFromClauseMethodObjectNoArg());
            execs.Add(new EPLFromClauseMethodObjectWithArg());
            execs.Add(new EPLFromClauseMethodInvocationTargetEx());
            execs.Add(new EPLFromClauseMethodStreamNameWContext());
            execs.Add(new EPLFromClauseMethodWithMethodResultParam());
            execs.Add(new EPLFromClauseMethodInvalid());
            execs.Add(new EPLFromClauseMethodEventBeanArray());
            execs.Add(new EPLFromClauseMethodUDFAndScriptReturningEvents());
            return execs;
        }

        private static void TryAssertionUDFAndScriptReturningEvents(
            RegressionEnvironment env,
            RegressionPath path,
            string methodName)
        {
            env.CompileDeploy("@Name('s0') select Id from SupportBean, method:" + methodName, path).AddListener("s0");

            env.SendEventBean(new SupportBean());
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                "Id".SplitCsv(),
                new[] {new object[] {"id1"}, new object[] {"id3"}});

            env.UndeployModuleContaining("s0");
        }

        private static void TryAssertionEventBeanArray(
            RegressionEnvironment env,
            RegressionPath path,
            string methodName,
            bool soda)
        {
            var epl = "@Name('s0') select p0 from SupportBean, method:" +
                      typeof(SupportStaticMethodLib).Name +
                      "." +
                      methodName +
                      "(TheString) @type(MyItemEvent)";
            env.CompileDeploy(soda, epl, path).AddListener("s0");

            env.SendEventBean(new SupportBean("a,b", 0));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetLastNewData(),
                "p0".SplitCsv(),
                new[] {new object[] {"a"}, new object[] {"b"}});

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
            foreach (var id in "id1,Id3".SplitCsv()) {
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
            env.CompileDeploy("@Name('s0') " + expression).AddListener("s0");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {1, null}, new object[] {2, 2}});
            env.UndeployAll();
        }

        private static void AssertJoinHistoricalOnlyDependent(
            RegressionEnvironment env,
            RegressionPath path,
            string expression)
        {
            env.CompileDeploy("@Name('s0') " + expression, path).AddListener("s0");

            var fields = "value,result".SplitCsv();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            SendSupportBeanEvent(env, 5, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {5, "|5|"}});

            SendSupportBeanEvent(env, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {1, "|1|"}, new object[] {2, "|2|"}});

            SendSupportBeanEvent(env, 0, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            SendSupportBeanEvent(env, 4, 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {4, "|4|"}, new object[] {5, "|5|"}, new object[] {6, "|6|"}});

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
            env.CompileDeploy("@Name('s0') " + expression, path).AddListener("s0");

            var fields = "valueOne,valueTwo".SplitCsv();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            SendSupportBeanEvent(env, 5, 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {5, "5"}});

            SendSupportBeanEvent(env, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {1, "1"}, new object[] {1, "2"}, new object[] {2, "1"}, new object[] {2, "2"}});

            SendSupportBeanEvent(env, 0, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

            var listener = env.Listener("s0");
            env.UndeployModuleContaining("s0");

            SendSupportBeanEvent(env, 0, -1);
            Assert.IsFalse(listener.IsInvoked);
        }

        private static void TryAssertionSingleRowFetch(
            RegressionEnvironment env,
            string method)
        {
            var epl = "@Name('s0') select TheString, IntPrimitive, mapstring, mapint from " +
                      "SupportBean as s1, " +
                      "method:" +
                      typeof(SupportStaticMethodLib).Name +
                      "." +
                      method;
            env.CompileDeploy(epl).AddListener("s0");

            string[] fields = {"TheString", "IntPrimitive", "mapstring", "mapint"};

            SendBeanEvent(env, "E1", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1, "|E1|", 2});

            SendBeanEvent(env, "E2", 3);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 3, "|E2|", 4});

            SendBeanEvent(env, "E3", 0);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 0, null, null});

            SendBeanEvent(env, "E4", -1);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void TryAssertionReturnTypeMultipleRow(
            RegressionEnvironment env,
            string method)
        {
            var epl = "@Name('s0') select TheString, IntPrimitive, mapstring, mapint from " +
                      "SupportBean#keepall as s1, " +
                      "method:" +
                      typeof(SupportStaticMethodLib).Name +
                      "." +
                      method;
            var fields = "theString,IntPrimitive,mapstring,mapint".SplitCsv();
            env.CompileDeploy(epl).AddListener("s0");

            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

            SendBeanEvent(env, "E1", 0);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

            SendBeanEvent(env, "E2", -1);
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

            SendBeanEvent(env, "E3", 1);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 1, "|E3_0|", 100});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {"E3", 1, "|E3_0|", 100}});

            SendBeanEvent(env, "E4", 2);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101}});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {
                    new object[] {"E3", 1, "|E3_0|", 100}, new object[] {"E4", 2, "|E4_0|", 100},
                    new object[] {"E4", 2, "|E4_1|", 101}
                });

            SendBeanEvent(env, "E5", 3);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {
                    new object[] {"E5", 3, "|E5_0|", 100}, new object[] {"E5", 3, "|E5_1|", 101},
                    new object[] {"E5", 3, "|E5_2|", 102}
                });
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {
                    new object[] {"E3", 1, "|E3_0|", 100},
                    new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101},
                    new object[] {"E5", 3, "|E5_0|", 100}, new object[] {"E5", 3, "|E5_1|", 101},
                    new object[] {"E5", 3, "|E5_2|", 102}
                });

            env.Listener("s0").Reset();
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

        internal class EPLFromClauseMethodWithMethodResultParam : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean as e,\n" +
                          "method:" +
                          typeof(EPLFromClauseMethod).Name +
                          ".getWithMethodResultParam('somevalue', e, " +
                          typeof(EPLFromClauseMethod).Name +
                          ".getWithMethodResultParamCompute(true)) as s";
                env.CompileDeploy(epl).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "s.P00,s.P01,s.P02".SplitCsv(),
                    new object[] {"somevalue", "E1", "s0"});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodStreamNameWContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean as e,\n" +
                          "method:" +
                          typeof(EPLFromClauseMethod).Name +
                          ".getStreamNameWContext('somevalue', e) as s";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "s.P00,s.P01,s.P02".SplitCsv(),
                    new object[] {"somevalue", "E1", "s0"});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodUDFAndScriptReturningEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create schema ItemEvent(Id string)", path);

                var script =
                    "@Name('script') create expression EventBean[] @type(ItemEvent) js:myItemProducerScript() [\n" +
                    "myItemProducerScript();" +
                    "function myItemProducerScript() {" +
                    "  var EventBeanArray = Java.type(\"com.espertech.esper.common.client.EventBean[]\");\n" +
                    "  var events = new EventBeanArray(2);\n" +
                    "  events[0] = epl.getEventBeanService().adapterForMap(java.util.Collections.singletonMap(\"id\", \"id1\"), \"ItemEvent\");\n" +
                    "  events[1] = epl.getEventBeanService().adapterForMap(java.util.Collections.singletonMap(\"id\", \"id3\"), \"ItemEvent\");\n" +
                    "  return events;\n" +
                    "}]";
                env.CompileDeploy(script, path);

                TryAssertionUDFAndScriptReturningEvents(env, path, "myItemProducerUDF");
                TryAssertionUDFAndScriptReturningEvents(env, path, "myItemProducerScript");

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodEventBeanArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeployWBusPublicType("create schema MyItemEvent(p0 string)", path);

                TryAssertionEventBeanArray(env, path, "eventBeanArrayForString", false);
                TryAssertionEventBeanArray(env, path, "eventBeanArrayForString", true);
                TryAssertionEventBeanArray(env, path, "eventBeanCollectionForString", false);
                TryAssertionEventBeanArray(env, path, "eventBeanIteratorForString", false);

                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchResult12(0) @type(ItemEvent)",
                    "The @type annotation is only allowed when the invocation target returns EventBean instances");

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodOverloaded : RegressionExecution
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
                var epl = "@Name('s0') select col1, col2 from SupportBean, method:" +
                          typeof(SupportStaticMethodLib).Name +
                          ".overloadedMethodForJoin(" +
                          @params +
                          ")";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "col1,col2".SplitCsv(),
                    new object[] {expectedFirst, expectedSecond});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2StreamMaxAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).Name;
                string stmtText;
                var fields = "maxcol1".SplitCsv();

                // ESPER 556
                stmtText = "@Name('s0') select max(col1) as maxcol1 from SupportBean#unique(TheString), method:" +
                           className +
                           ".FetchResult100() ";
                env.CompileDeploy(stmtText).AddListener("s0");
                AssertStatelessStmt(env, "s0", false);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {9}});

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {9}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).Name;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "@Name('s0') select IntPrimitive,IntBoxed,col1,col2 from SupportBean#keepall " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult100() " +
                           "on IntPrimitive = col1 and IntBoxed = col2";

                var fields = "IntPrimitive,IntBoxed,col1,col2".SplitCsv();
                env.CompileDeploy(stmtText).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, null);

                SendSupportBeanEvent(env, 2, 4);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {2, 4, 2, 4}});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {2, 4, 2, 4}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2JoinHistoricalSubordinateOuter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var className = typeof(SupportStaticMethodLib).Name;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.Value) as s1 on s0.Value = s1.Value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.Value) as s1 " +
                           "right outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.Value = s1.Value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.Value) as s1 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.Value = s1.Value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);

                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(s0.Value) as s1 on s0.Value = s1.Value";
                AssertJoinHistoricalSubordinateOuter(env, stmtText);
            }
        }

        internal class EPLFromClauseMethod2JoinHistoricalIndependentOuter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "valueOne,valueTwo".SplitCsv();
                var className = typeof(SupportStaticMethodLib).Name;
                string stmtText;

                stmtText = "@Name('s0') select s0.Value as valueOne, s1.Value as valueTwo from method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "left outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 on s0.Value = s1.Value";
                env.CompileDeploy(stmtText).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, null}, new object[] {2, 2}});
                env.UndeployAll();

                stmtText = "@Name('s0') select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 " +
                           "right outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.Value = s1.Value";
                env.CompileDeploy(stmtText);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, null}, new object[] {2, 2}});
                env.UndeployAll();

                stmtText = "@Name('s0') select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 on s0.Value = s1.Value";
                env.CompileDeploy(stmtText);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});
                env.UndeployAll();

                stmtText = "@Name('s0') select s0.Value as valueOne, s1.Value as valueTwo from " +
                           "method:" +
                           className +
                           ".FetchResult12(0) as s0 " +
                           "full outer join " +
                           "method:" +
                           className +
                           ".FetchResult23(0) as s1 on s0.Value = s1.Value";
                env.CompileDeploy(stmtText);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethod2JoinHistoricalOnlyDependent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int lower", path);
                env.CompileDeploy("create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                var className = typeof(SupportStaticMethodLib).Name;
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
        }

        internal class EPLFromClauseMethod2JoinHistoricalOnlyIndependent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int lower", path);
                env.CompileDeploy("create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                var className = typeof(SupportStaticMethodLib).Name;
                string stmtText;

                // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from method:" +
                           className +
                           ".FetchBetween(lower, upper) as s0, " +
                           "method:" +
                           className +
                           ".FetchBetweenString(lower, upper) as s1";
                AssertJoinHistoricalOnlyIndependent(env, path, stmtText);

                stmtText = "select s0.Value as valueOne, s1.Value as valueTwo from " +
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

        internal class EPLFromClauseMethodNoJoinIterateVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create variable int lower", path);
                env.CompileDeploy("create variable int upper", path);
                env.CompileDeploy("on SupportBean set lower=IntPrimitive,upper=IntBoxed", path);

                // Test int and singlerow
                var className = typeof(SupportStaticMethodLib).Name;
                var stmtText = "@Name('s0') select value from method:" + className + ".FetchBetween(lower, upper)";
                env.CompileDeploy(stmtText, path).AddListener("s0");

                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), new[] {"value"}, null);

                SendSupportBeanEvent(env, 5, 10);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new[] {"value"},
                    new[] {
                        new object[] {5}, new object[] {6}, new object[] {7}, new object[] {8}, new object[] {9},
                        new object[] {10}
                    });

                env.Milestone(0);

                SendSupportBeanEvent(env, 10, 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), new[] {"value"}, null);

                SendSupportBeanEvent(env, 4, 4);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    new[] {"value"},
                    new[] {new object[] {4}});

                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodDifferentReturnTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionSingleRowFetch(env, "fetchMap(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "fetchMapEventBean(s1, 'TheString', 'IntPrimitive')");
                TryAssertionSingleRowFetch(env, "fetchObjectArrayEventBean(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "fetchPONOArray(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "fetchPONOCollection(TheString, IntPrimitive)");
                TryAssertionSingleRowFetch(env, "fetchPONOIterator(TheString, IntPrimitive)");

                TryAssertionReturnTypeMultipleRow(env, "fetchMapArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchOAArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchPONOArrayMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchMapCollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchOACollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchPONOCollectionMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchMapIteratorMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchOAIteratorMR(TheString, IntPrimitive)");
                TryAssertionReturnTypeMultipleRow(env, "fetchPONOIteratorMR(TheString, IntPrimitive)");
            }
        }

        internal class EPLFromClauseMethodArrayNoArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select Id, TheString from " +
                                    "SupportBean#length(3) as s1, " +
                                    "method:" +
                                    typeof(SupportStaticMethodLib).Name +
                                    ".FetchArrayNoArg";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                joinStatement = "@Name('s0') select Id, TheString from " +
                                "SupportBean#length(3) as s1, " +
                                "method:" +
                                typeof(SupportStaticMethodLib).Name +
                                ".FetchArrayNoArg()";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
                TryArrayNoArg(env);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Id", "TheString");
                model.FromClause = FromClause.Create()
                    .Add(FilterStream.Create("SupportBean", "s1").AddView("length", Expressions.Constant(3)))
                    .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).Name, "fetchArrayNoArg"));
                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                Assert.AreEqual(joinStatement, model.ToEPL());

                TryArrayNoArg(env);
            }

            private static void TryArrayNoArg(RegressionEnvironment env)
            {
                string[] fields = {"Id", "TheString"};

                SendBeanEvent(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"1", "E1"});

                SendBeanEvent(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"1", "E2"});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodArrayWithArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select irstream Id, TheString from " +
                                    "SupportBean()#length(3) as s1, " +
                                    " method:" +
                                    typeof(SupportStaticMethodLib).Name +
                                    ".FetchArrayGen(IntPrimitive)";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                joinStatement = "@Name('s0') select irstream Id, TheString from " +
                                "method:" +
                                typeof(SupportStaticMethodLib).Name +
                                ".FetchArrayGen(IntPrimitive) as s0, " +
                                "SupportBean#length(3)";
                env.CompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
                TryArrayWithArg(env);

                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("Id", "TheString")
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);
                model.FromClause = FromClause.Create()
                    .Add(
                        MethodInvocationStream
                            .Create(typeof(SupportStaticMethodLib).FullName, "FetchArrayGen", "s0")
                            .AddParameter(Expressions.Property("IntPrimitive")))
                    .Add(
                        FilterStream
                            .Create("SupportBean")
                            .AddView("length", Expressions.Constant(3)));

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");
                Assert.AreEqual(joinStatement, model.ToEPL());

                TryArrayWithArg(env);
            }

            private static void TryArrayWithArg(RegressionEnvironment env)
            {
                string[] fields = {"Id", "TheString"};

                SendBeanEvent(env, "E1", -1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E2", 0);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E3", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", "E3"});

                SendBeanEvent(env, "E4", 2);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"A", "E4"}, new object[] {"B", "E4"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendBeanEvent(env, "E5", 3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"A", "E5"}, new object[] {"B", "E5"}, new object[] {"C", "E5"}});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();

                SendBeanEvent(env, "E6", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"A", "E6"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"A", "E3"}});
                env.Listener("s0").Reset();

                SendBeanEvent(env, "E7", 1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"A", "E7"}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fields,
                    new[] {new object[] {"A", "E4"}, new object[] {"B", "E4"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodObjectNoArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select Id, TheString from " +
                                    "SupportBean()#length(3) as s1, " +
                                    " method:" +
                                    typeof(SupportStaticMethodLib).Name +
                                    ".FetchObjectNoArg()";
                env.CompileDeploy(joinStatement).AddListener("s0");
                string[] fields = {"Id", "TheString"};

                SendBeanEvent(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"2", "E1"});

                env.Milestone(0);

                SendBeanEvent(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"2", "E2"});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodObjectWithArg : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "@Name('s0') select Id, TheString from " +
                                    "SupportBean()#length(3) as s1, " +
                                    " method:" +
                                    typeof(SupportStaticMethodLib).Name +
                                    ".FetchObject(TheString)";
                env.CompileDeploy(joinStatement).AddListener("s0");

                string[] fields = {"Id", "TheString"};

                SendBeanEvent(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"|E1|", "E1"});

                SendBeanEvent(env, null);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendBeanEvent(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"|E2|", "E2"});

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodInvocationTargetEx : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var joinStatement = "select s1.TheString from " +
                                    "SupportBean()#length(3) as s1, " +
                                    " method:" +
                                    typeof(SupportStaticMethodLib).Name +
                                    ".throwExceptionBeanReturn()";

                env.CompileDeploy(joinStatement);

                try {
                    SendBeanEvent(env, "E1");
                    Assert.Fail(); // default test configuration rethrows this exception
                }
                catch (EPException ex) {
                    // fine
                }

                env.UndeployAll();
            }
        }

        internal class EPLFromClauseMethodInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).Name + ".FetchArrayGen()",
                    "Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find static method named 'fetchArrayGen' in class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "' taking no parameters (nearest match found was 'fetchArrayGen' taking type(s) 'int') [");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:.abc where 1=2",
                    "Incorrect syntax near '.' at line 1 column 34, please check the method invocation join within the from clause [select * from SupportBean, method:.abc where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchObjectAndSleep(1)",
                    "Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed 'int': Could not find static method named 'fetchObjectAndSleep' in class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "' with matching parameter number and expected parameter type(s) 'int' (nearest match found was 'fetchObjectAndSleep' taking type(s) 'String, int, long') [");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".Sleep(100) where 1=2",
                    "Invalid return type for static method 'sleep' of class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "', expecting a Java class [select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".Sleep(100) where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:AClass. where 1=2",
                    "Incorrect syntax near 'where' (a reserved keyword) expecting an Identifier but found 'where' at line 1 column 42, please check the view specifications within the from clause [select * from SupportBean, method:AClass. where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:Dummy.abc where 1=2",
                    "Could not load class by name 'Dummy', please check imports [select * from SupportBean, method:Dummy.abc where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:Math where 1=2",
                    "A function named 'Math' is not defined");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2",
                    "Method data joins do not allow views onto the data, view 'length' is not valid in this context [select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).Name + ".dummy where 1=2",
                    "Could not find public static method named 'dummy' in class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "' [");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".minusOne(10) where 1=2",
                    "Invalid return type for static method 'minusOne' of class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "', expecting a Java class [");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, xyz:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchArrayNoArg() where 1=2",
                    "Expecting keyword 'method', found 'xyz' [select * from SupportBean, xyz:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchArrayNoArg() where 1=2]");

                TryInvalidCompile(
                    env,
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchBetween(s1.Value, s1.Value) as s0, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchBetween(s0.Value, s0.Value) as s1",
                    "Circular dependency detected between historical streams [");

                TryInvalidCompile(
                    env,
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchBetween(s0.Value, s0.Value) as s0, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchBetween(s0.Value, s0.Value) as s1",
                    "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

                TryInvalidCompile(
                    env,
                    "select * from method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".FetchBetween(s0.Value, s0.Value) as s0",
                    "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

                TryInvalidCompile(
                    env,
                    "select * from method:SupportMethodInvocationJoinInvalid.readRowNoMetadata()",
                    "Could not find getter method for method invocation, expected a method by name 'readRowNoMetadataMetadata' accepting no parameters [select * from method:SupportMethodInvocationJoinInvalid.readRowNoMetadata()]");

                TryInvalidCompile(
                    env,
                    "select * from method:SupportMethodInvocationJoinInvalid.readRowWrongMetadata()",
                    "Getter method 'readRowWrongMetadataMetadata' does not return IDictionary [select * from method:SupportMethodInvocationJoinInvalid.readRowWrongMetadata()]");

                TryInvalidCompile(
                    env,
                    "select * from SupportBean, method:" +
                    typeof(SupportStaticMethodLib).Name +
                    ".invalidOverloadForJoin(null)",
                    "Method by name 'invalidOverloadForJoin' is overloaded in class '" +
                    typeof(SupportStaticMethodLib).Name +
                    "' and overloaded methods do not return the same type");
            }
        }
    }
} // end of namespace