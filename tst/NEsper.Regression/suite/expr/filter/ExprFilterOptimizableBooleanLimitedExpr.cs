///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableBooleanLimitedExpr
    {
        public static ICollection<RegressionExecution> Executions()
        {
            List<RegressionExecution> execs = new List<RegressionExecution>();
            WithConstValueRegexpRHS(execs);
            WithMixedValueRegexpRHS(execs);
            WithConstValueRegexpRHSPerformance(execs);
            WithConstValueRegexpLHS(execs);
            WithNoValueExprRegexpSelf(execs);
            WithNoValueConcat(execs);
            WithContextValueDeep(execs);
            WithContextValueWithConst(execs);
            WithPatternValueWithConst(execs);
            WithWithEquals(execs);
            WithMultiple(execs);
            WithDisqualify(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDisqualify(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolDisqualify());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolMultiple());
            return execs;
        }

        public static IList<RegressionExecution> WithWithEquals(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolWithEquals());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternValueWithConst(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolPatternValueWithConst());
            return execs;
        }

        public static IList<RegressionExecution> WithContextValueWithConst(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolContextValueWithConst());
            return execs;
        }

        public static IList<RegressionExecution> WithContextValueDeep(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolContextValueDeep());
            return execs;
        }

        public static IList<RegressionExecution> WithNoValueConcat(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolNoValueConcat());
            return execs;
        }

        public static IList<RegressionExecution> WithNoValueExprRegexpSelf(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolNoValueExprRegexpSelf());
            return execs;
        }

        public static IList<RegressionExecution> WithConstValueRegexpLHS(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolConstValueRegexpLHS());
            return execs;
        }

        public static IList<RegressionExecution> WithConstValueRegexpRHSPerformance(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolConstValueRegexpRHSPerformance());
            return execs;
        }

        public static IList<RegressionExecution> WithMixedValueRegexpRHS(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolMixedValueRegexpRHS());
            return execs;
        }

        public static IList<RegressionExecution> WithConstValueRegexpRHS(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptReboolConstValueRegexpRHS());
            return execs;
        }

        private class ExprFilterOptReboolWithEquals : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from pattern[s0=SupportBean_S0 -> SupportBean(IntPrimitive+5=s0.Id and TheString='a')]";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));

                if (HasFilterIndexPlanAdvanced(env)) {
                    FilterItem[] @params = GetFilterSvcMultiAssertNonEmpty(env.Statement("s0"));
                    Assert.AreEqual(EQUAL, @params[0].Op);
                    Assert.AreEqual(EQUAL, @params[1].Op);
                }

                env.Milestone(0);

                SendSBAssert(env, "a", 10, false);
                SendSBAssert(env, "b", 5, false);
                SendSBAssert(env, "a", 5, true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolMultiple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBean_S0(P00 regexp '.*X' and P01 regexp '.*Y')").AddListener("s0");
                env.CompileDeploy("@Name('s1') select * from SupportBean_S0(P01 regexp '.*Y' and P00 regexp '.*X')").AddListener("s1");

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    IDictionary<string, FilterItem[]> filters = GetFilterSvcAllStmtForTypeMulti(env.Runtime, "SupportBean_S0");
                    FilterItem[] s0 = filters.Get("s0");
                    FilterItem[] s1 = filters.Get("s1");
                    Assert.AreEqual(REBOOL, s0[0].Op);
                    Assert.AreEqual(".P00 regexp ?", s0[0].Name);
                    Assert.AreEqual(REBOOL, s0[1].Op);
                    Assert.AreEqual(".P01 regexp ?", s0[1].Name);
                    Assert.AreEqual(s0[0], s1[0]);
                    Assert.AreEqual(s0[1], s1[1]);
                }

                SendS0Assert(env, "AX", "AZ", false);
                SendS0Assert(env, "AY", "AX", false);
                SendS0Assert(env, "AX", "BY", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolPatternValueWithConst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from pattern[s0=SupportBean_S0 -> SupportBean_S1(P10 || 'abc' regexp s0.P00)];\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "x.*abc"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), ".P10||\"abc\" regexp ?", REBOOL);
                }

                env.Milestone(0);

                SendS1Assert(env, "ydotabc", false);
                SendS1Assert(env, "xdotabc", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolContextValueWithConst : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                             "@Name('s0') context MyContext select * from SupportBean_S1(P10 || 'abc' regexp context.s0.P00);\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "x.*abc"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), ".P10||\"abc\" regexp ?", REBOOL);
                }

                env.Milestone(0);

                SendS1Assert(env, "ydotabc", false);
                SendS1Assert(env, "xdotabc", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolContextValueDeep : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                             "@Name('s0') context MyContext select * from SupportBean_S1(P10 regexp P11 || context.s0.P00);\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, ".*X"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), ".P10 regexp P11||?", REBOOL);
                }

                env.Milestone(0);

                SendS1Assert(env, "gardenX", "abc", false);
                SendS1Assert(env, "garden", "gard", false);
                SendS1Assert(env, "gardenX", "gard", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolNoValueConcat : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "select * from SupportBean_S0(P00 || P01 = P02 || P03)";
                RunTwoStmt(
                    env,
                    epl,
                    epl,
                    ".P00||P01=P02||P03",
                    "SupportBean_S0",
                    new SupportBean_S0(1, "a", "b", "a", "b"),
                    new SupportBean_S0(1, "a", "b", "a", "c"));
            }
        }

        private class ExprFilterOptReboolNoValueExprRegexpSelf : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunTwoStmt(
                    env,
                    "select * from SupportBean_S0(P00 regexp P01) as a",
                    "select * from SupportBean_S0(s0.P00 regexp s0.P01) as s0",
                    ".P00 regexp P01",
                    "SupportBean_S0",
                    new SupportBean_S0(1, "abc", ".*c"),
                    new SupportBean_S0(2, "abc", ".*d"));
            }
        }

        private class ExprFilterOptReboolConstValueRegexpLHS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunTwoStmt(
                    env,
                    "select * from SupportBean('abc' regexp a.TheString) as a",
                    "select * from SupportBean('abc' regexp TheString)",
                    ".? regexp TheString",
                    "SupportBean",
                    new SupportBean(".*bc", 0),
                    new SupportBean(".*d", 0));
            }
        }

        private class ExprFilterOptReboolMixedValueRegexpRHS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('var') create constant variable string MYVAR = '.*abc.*';\n" +
                             "@Name('s0') select * from SupportBean(TheString regexp MYVAR);\n" +
                             "" +
                             "@Name('ctx') create context MyContext start SupportBean_S0 as s0;\n" +
                             "@Name('s1') context MyContext select * from SupportBean(TheString regexp context.s0.P00);\n" +
                             "" +
                             "@Name('s2') select * from pattern[s0=SupportBean_S0 -> every SupportBean(TheString regexp s0.P00)];\n" +
                             "" +
                             "@Name('s3') select * from SupportBean(TheString regexp '.*' || 'abc' || '.*');\n";
                env.CompileDeploy(epl);
                EPDeployment deployment = env.Deployment.GetDeployment(env.DeploymentId("s0"));
                ISet<string> statementNames = new LinkedHashSet<string>();
                foreach (EPStatement stmt in deployment.Statements) {
                    if (stmt.Name.StartsWith("s")) {
                        stmt.AddListener(env.ListenerNew());
                        statementNames.Add(stmt.Name);
                    }
                }

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, ".*abc.*"));

                SendSBAssert(env, "xabsx", statementNames, false);
                SendSBAssert(env, "xabcx", statementNames, true);

                if (HasFilterIndexPlanAdvanced(env)) {
                    IDictionary<string, FilterItem> filters = SupportFilterServiceHelper.GetFilterSvcAllStmtForTypeSingleFilter(env.Runtime, "SupportBean");
                    FilterItem s0 = filters.Get("s0");
                    foreach (string name in statementNames) {
                        FilterItem sn = filters.Get(name);
                        Assert.AreEqual(FilterOperator.REBOOL, sn.Op);
                        Assert.IsNotNull(s0.OptionalValue);
                        Assert.IsNotNull(s0.Index);
                        Assert.AreSame(s0.Index, sn.Index);
                        Assert.AreSame(s0.OptionalValue, sn.OptionalValue);
                    }
                }

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolConstValueRegexpRHS : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportFilterPlanHook.Reset();
                string hook = "@Hook(HookType=HookType.INTERNAL_FILTERSPEC, Hook='" + typeof(SupportFilterPlanHook).FullName + "')";
                string epl = hook + "@Name('s0') select * from SupportBean(TheString regexp '.*a.*')";
                env.CompileDeploy(epl).AddListener("s0");
                if (HasFilterIndexPlanAdvanced(env)) {
                    FilterSpecParamForge forge = SupportFilterPlanHook.AssertPlanSingleTripletAndReset("SupportBean");
                    Assert.AreEqual(FilterOperator.REBOOL, forge.FilterOperator);
                    Assert.AreEqual(".TheString regexp ?", forge.Lookupable.Expression);
                    Assert.AreEqual(typeof(string), forge.Lookupable.ReturnType);
                    AssertFilterSvcSingle(env.Statement("s0"), ".TheString regexp ?", REBOOL);
                }

                epl = "@Name('s1') select * from SupportBean(TheString regexp '.*a.*')";
                env.CompileDeploy(epl).AddListener("s1");

                epl = "@Name('s2') select * from SupportBean(TheString regexp '.*b.*')";
                env.CompileDeploy(epl).AddListener("s2");

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    IDictionary<string, FilterItem> filters = SupportFilterServiceHelper.GetFilterSvcAllStmtForTypeSingleFilter(env.Runtime, "SupportBean");
                    FilterItem s0 = filters.Get("s0");
                    FilterItem s1 = filters.Get("s1");
                    FilterItem s2 = filters.Get("s2");
                    Assert.AreEqual(FilterOperator.REBOOL, s0.Op);
                    Assert.IsNotNull(s0.OptionalValue);
                    Assert.IsNotNull(s0.Index);
                    Assert.AreSame(s0.Index, s1.Index);
                    Assert.AreSame(s0.OptionalValue, s1.OptionalValue);
                    Assert.AreSame(s0.Index, s2.Index);
                    Assert.AreNotSame(s0.OptionalValue, s2.OptionalValue);
                }

                SendSBAssert(env, "garden", true, true, false);
                SendSBAssert(env, "house", false, false, false);
                SendSBAssert(env, "grub", false, false, true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptReboolConstValueRegexpRHSPerformance : RegressionExecution
        {
            public bool ExcludeWhenInstrumented()
            {
                return true;
            }

            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();

                string epl = "select * from SupportBean(TheString regexp '.*,.*,.*,.*,.*,13,.*,.*,.*,.*,.*,.*')";
                int count = 5;
                DeployMultiple(count, path, epl, env);

                env.Milestone(0);

                SupportListener[] listeners = new SupportListener[count];
                for (int i = 0; i < count; i++) {
                    listeners[i] = env.Listener("s" + i);
                }

                long delta = PerformanceObserver.TimeMillis(
                    () => {
                        int loops = 1000;
                        for (int i = 0; i < loops; i++) {
                            bool match = i % 100 == 0;
                            string value = match
                                ? "42,12,13,12,32,13,14,43,56,31,78,10"
                                : // match
                                "42,12,13,12,32,14,13,43,56,31,78,10"; // no-match

                            env.SendEventBean(new SupportBean(value, 0));
                            if (match) {
                                foreach (SupportListener listener in listeners) {
                                    listener.AssertOneGetNewAndReset();
                                }
                            }
                            else {
                                foreach (SupportListener listener in listeners) {
                                    Assert.IsFalse(listener.IsInvoked);
                                }
                            }
                        }
                    });

#if DEBUG
                Assert.That(delta, Is.LessThan(2000), "Delta is " + delta); // ~7 seconds without optimization
#else
				Assert.That(delta, Is.LessThan(1000), "Delta is " + delta); // ~7 seconds without optimization
#endif

                env.UndeployAll();
            }
        }

        private static void DeployMultiple(
            int count,
            RegressionPath path,
            string epl,
            RegressionEnvironment env)
        {
            for (int i = 0; i < count; i++) {
                EPCompiled compiled = env.Compile("@Name('s" + i + "')" + epl, path);
                EPDeploymentService admin = env.Runtime.DeploymentService;
                try {
                    admin.Deploy(compiled);
                }
                catch (EPDeployException) {
                    Assert.Fail();
                }
            }

            for (int i = 0; i < count; i++) {
                env.Statement("s" + i).AddListener(env.ListenerNew());
            }
        }

        public class ExprFilterOptReboolDisqualify : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
                                 "@public create table MyTable(tablecol string);\n" +
                                 "@public create window MyWindow#keepall as SupportBean;\n" +
                                 "@public create inlined_class \"\"\"\n" +
                                 "  using com.espertech.esper.common.client.hook.singlerowfunc;\n" +
                                 "  using com.espertech.esper.common.client.configuration.compiler;\n" +
                                 "  [ExtensionSingleRowFunction(Name=\"doit\", MethodName=\"Doit\", FilterOptimizable=ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED)]\n" +
                                 "  public class Helper {\n" +
                                 "    public static string Doit(object param) {\n" +
                                 "      return null;\n" +
                                 "    }\n" +
                                 "  }\n" +
                                 "\"\"\";\n" +
                                 "@public create expression MyDeclaredExpr { (select TheString from MyWindow) };\n" +
                                 "@public create expression MyHandThrough {v => v};" +
                                 "@public create expression string js:MyJavaScript() [\"a\"];\n";
                env.Compile(objects, path);
                string hook = "@Hook(HookType=HookType.INTERNAL_FILTERSPEC, Hook='" + nameof(SupportFilterPlanHook) + "')";

                // Core disqualifing: non-constant variables, tables, subselects, lambda, plug-in UDF with filter-opt-disabled, scripts
                AssertDisqualified(env, path, "SupportBean", hook + "select * from SupportBean(TheString regexp MYVARIABLE_NONCONSTANT)");
                AssertDisqualified(env, path, "SupportBean", hook + "select * from SupportBean(TheString=MyTable.tablecol)");
                AssertDisqualified(env, path, "SupportBean", hook + "select * from SupportBean(TheString=(select TheString from MyWindow))");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBeanArrayCollMap",
                    hook + "select * from SupportBeanArrayCollMap(Id = SetOfString.where(v => v=Id).firstOf())");
                AssertDisqualified(env, path, "SupportBean", hook + "select * from SupportBean(TheString regexp doit('abc'))");
                AssertDisqualified(env, path, "SupportBean", hook + "select * from SupportBean(TheString regexp MyJavaScript())");

                // multiple value expressions
                AssertDisqualified(
                    env,
                    path,
                    "SupportBean_S1",
                    hook + "select * from pattern[s0=SupportBean_S0 -> SupportBean_S1(s0.P00 || P10 = s0.P00 || P11)]");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBean_S1",
                    "create context MyContext start SupportBean_S0 as s0;\n" +
                    hook +
                    "context MyContext select * from SupportBean_S1(context.s0.P00 || P10 = context.s0.P01)");

                string eplWithLocalHelper = hook +
                                            "inlined_class \"\"\"\n" +
                                            "  public class LocalHelper {\n" +
                                            "    public static string Doit(object param) {\n" +
                                            "      return null;\n" +
                                            "    }\n" +
                                            "  }\n" +
                                            "\"\"\"\n" +
                                            "select * from SupportBean(TheString regexp LocalHelper.Doit('abc'))";
                AssertDisqualified(env, path, "SupportBean", eplWithLocalHelper);
            }
        }

        protected static void AssertDisqualified(
            RegressionEnvironment env,
            RegressionPath path,
            string typeName,
            string epl)
        {
            SupportFilterPlanHook.Reset();
            env.Compile(epl, path);
            FilterSpecParamForge forge = SupportFilterPlanHook.AssertPlanSingleForTypeAndReset(typeName);
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, forge.FilterOperator);
        }

        private static void SendS0Assert(
            RegressionEnvironment env,
            string p00,
            string p01,
            bool receivedS0)
        {
            env.SendEventBean(new SupportBean_S0(0, p00, p01));
            Assert.AreEqual(receivedS0, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            bool receivedS0,
            bool receivedS1,
            bool receivedS2)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            Assert.AreEqual(receivedS0, env.Listener("s0").IsInvokedAndReset());
            Assert.AreEqual(receivedS1, env.Listener("s1").IsInvokedAndReset());
            Assert.AreEqual(receivedS2, env.Listener("s2").IsInvokedAndReset());
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            bool receivedS0)
        {
            SendSBAssert(env, theString, -1, receivedS0);
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            bool receivedS0)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            Assert.AreEqual(receivedS0, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            ICollection<string> names,
            bool expected)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            AssertReceived(env, names, expected);
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            ICollection<string> names,
            bool expected)
        {
            foreach (string name in names) {
                Assert.AreEqual(expected, env.Listener(name).IsInvokedAndReset(), "failed for '" + name + "'");
            }
        }

        private static void AssertSameFilterEntry(
            RegressionEnvironment env,
            string eventTypeName)
        {
            IDictionary<string, FilterItem> filters = SupportFilterServiceHelper.GetFilterSvcAllStmtForTypeSingleFilter(env.Runtime, eventTypeName);
            FilterItem s0 = filters.Get("s0");
            FilterItem s1 = filters.Get("s1");
            Assert.AreEqual(FilterOperator.REBOOL, s0.Op);
            Assert.IsNotNull(s0.Index);
            Assert.AreSame(s0.Index, s1.Index);
            Assert.AreSame(s0.OptionalValue, s1.OptionalValue);
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            string p10,
            string p11,
            bool expected)
        {
            env.SendEventBean(new SupportBean_S1(1, p10, p11));
            Assert.AreEqual(expected, env.Listener("s0").GetAndClearIsInvoked());
        }

        private static void SendS1Assert(
            RegressionEnvironment env,
            string p10,
            bool expected)
        {
            SendS1Assert(env, p10, null, expected);
        }

        private static void RunTwoStmt(
            RegressionEnvironment env,
            string eplZero,
            string eplOne,
            string reboolExpressionText,
            string eventTypeName,
            object eventReceived,
            object eventNotReceived)
        {
            env.CompileDeploy("@Name('s0') " + eplZero).AddListener("s0");
            bool advanced = HasFilterIndexPlanAdvanced(env);
            if (advanced) {
                AssertFilterSvcSingle(env.Statement("s0"), reboolExpressionText, REBOOL);
            }

            env.CompileDeploy("@Name('s1') " + eplOne).AddListener("s1");
            if (advanced) {
                AssertFilterSvcSingle(env.Statement("s1"), reboolExpressionText, REBOOL);
            }

            IList<string> statementNames = Arrays.AsList("s0", "s1");

            env.Milestone(0);

            if (advanced) {
                AssertSameFilterEntry(env, eventTypeName);
            }

            env.SendEventBean(eventReceived);
            AssertReceived(env, statementNames, true);

            env.SendEventBean(eventNotReceived);
            AssertReceived(env, statementNames, false);

            env.UndeployAll();
        }

        public static string MyStaticMethod(object value)
        {
            SupportBean sb = (SupportBean) value;
            return sb.TheString.StartsWith("X") ? null : sb.TheString;
        }
    }
} // end of namespace