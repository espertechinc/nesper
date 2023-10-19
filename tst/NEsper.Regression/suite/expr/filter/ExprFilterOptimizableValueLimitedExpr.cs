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
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static
    com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper; // hasFilterIndexPlanAdvanced
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper; // assertFilterSvcByTypeSingle
// assertFilterSvcSingle
using NUnit.Framework; // fail

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableValueLimitedExpr
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEqualsIsConstant(execs);
            WithEqualsFromPatternSingle(execs);
            WithEqualsFromPatternMulti(execs);
            WithEqualsFromPatternConstant(execs);
            WithEqualsFromPatternHalfConstant(execs);
            WithEqualsFromPatternWithDotMethod(execs);
            WithEqualsContextWithStart(execs);
            WithEqualsSubstitutionParams(execs);
            WithEqualsConstantVariable(execs);
            WithEqualsCoercion(execs);
            WithRelOpCoercion(execs);
            WithDisqualify(execs);
            WithInSetOfValueWPatternWCoercion(execs);
            WithInRangeWCoercion(execs);
            WithOrRewrite(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithOrRewrite(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValOrRewrite());
            return execs;
        }

        public static IList<RegressionExecution> WithInRangeWCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValInRangeWCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithInSetOfValueWPatternWCoercion(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValInSetOfValueWPatternWCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithDisqualify(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValDisqualify());
            return execs;
        }

        public static IList<RegressionExecution> WithRelOpCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValRelOpCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsConstantVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsConstantVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsSubstitutionParams(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsSubstitutionParams());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsContextWithStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsContextWithStart());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternWithDotMethod(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternWithDotMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternHalfConstant(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternHalfConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternMulti(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternSingle(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsIsConstant(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsIsConstant());
            return execs;
        }

        private class ExprFilterOptValOrRewrite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                          "@name('s0') context MyContext select * from SupportBean(theString = context.s0.p00 || context.s0.p01 or theString = context.s0.p01 || context.s0.p00);\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "a", "b"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env, "s0", "theString", IN_LIST_OF_VALUES);
                }

                SendSBAssert(env, "ab", true);
                SendSBAssert(env, "ba", true);
                SendSBAssert(env, "aa", false);
                SendSBAssert(env, "aa", false);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptValInRangeWCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from pattern [" +
                          "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive in [a.id - 2 : b.id + 2])];\n";
                RunAssertionInRange(env, epl, false, milestone);

                epl = "@name('s0') select * from pattern [" +
                      "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive not in [a.id - 2 : b.id + 2])];\n";
                RunAssertionInRange(env, epl, true, milestone);
            }

            private void RunAssertionInRange(
                RegressionEnvironment env,
                string epl,
                bool not,
                AtomicLong milestone)
            {
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S1(200));

                env.MilestoneInc(milestone);
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env, "s0", "longPrimitive", not ? NOT_RANGE_CLOSED : RANGE_CLOSED);
                }

                SendSBAssert(env, 7, not);
                SendSBAssert(env, 8, !not);
                SendSBAssert(env, 100, !not);
                SendSBAssert(env, 202, !not);
                SendSBAssert(env, 203, not);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptValInSetOfValueWPatternWCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from pattern [" +
                          "a=SupportBean_S0 -> b=SupportBean_S1 -> c=SupportBean_S2 -> every SupportBean(longPrimitive in (a.id, b.id, c.id))];\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S1(200));
                env.SendEventBean(new SupportBean_S2(3000));

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env, "s0", "longPrimitive", IN_LIST_OF_VALUES);
                }

                SendSBAssert(env, 0, false);
                SendSBAssert(env, 10, true);
                SendSBAssert(env, 200, true);
                SendSBAssert(env, 3000, true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsFromPatternWithDotMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern [a=SupportBean -> b=SupportBean(theString=a.getTheString())]";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env, "s0", "theString", EQUAL);
                }

                env.SendEventBean(new SupportBean("E1", 2));
                env.AssertPropsNew("s0", "a.intPrimitive,b.intPrimitive".SplitCsv(), new object[] { 1, 2 });
                env.UndeployAll();
            }
        }

        public class ExprFilterOptValRelOpCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean(Integer.parseInt('10') > doublePrimitive)";
                RunAssertionRelOpCoercion(env, epl);

                epl = "@name('s0') select * from SupportBean(doublePrimitive < Integer.parseInt('10'))";
                RunAssertionRelOpCoercion(env, epl);
            }
        }

        public class ExprFilterOptValEqualsCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from SupportBean(doublePrimitive = Integer.parseInt('10') + Long.parseLong('20'))";
                env.CompileDeploy(epl).AddListener("s0");
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env, "s0", "doublePrimitive", EQUAL);
                }

                SendSBAssert(env, 30d, true);
                SendSBAssert(env, 20d, false);
                SendSBAssert(env, 30d, true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsConstantVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var variable = "create constant variable string MYCONST = 'a';\n";
                var milestone = new AtomicLong();
                TryDeployAndAssertionSB(
                    env,
                    variable + "@name('s0') select * from SupportBean(theString = MYCONST || 'x')",
                    EQUAL,
                    milestone);
                TryDeployAndAssertionSB(
                    env,
                    variable + "@name('s0') select * from SupportBean(MYCONST || 'x' = theString)",
                    EQUAL,
                    milestone);
            }
        }

        public class ExprFilterOptValEqualsSubstitutionParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean(theString = ?::string)";
                var compiled = env.Compile(epl);
                var options = new DeploymentOptions();
                options.WithStatementSubstitutionParameter(
                    new SupportPortableDeploySubstitutionParams(1, "ax").SetStatementParameters);
                env.Deploy(compiled, options).AddListener("s0");
                RunAssertionSB(env, epl, EQUAL, new AtomicLong());
            }
        }

        public class ExprFilterOptValEqualsIsConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryDeployAndAssertionSB(
                    env,
                    "@name('s0') select * from SupportBean(theString = 'a' || 'x')",
                    EQUAL,
                    milestone);
                TryDeployAndAssertionSB(
                    env,
                    "@name('s0') select * from SupportBean('a' || 'x' is theString)",
                    IS,
                    milestone);
            }
        }

        public class ExprFilterOptValEqualsFromPatternSingle : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[every a=SupportBean_S0 -> SupportBean(a.p00 || a.p01 = theString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(0, "a", "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
                }

                SendSBAssert(env, "a", false);
                SendSBAssert(env, "ax", true);

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "b", "y"));
                SendSBAssert(env, "ax", false);
                SendSBAssert(env, "by", true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsFromPatternConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[every SupportBean_S0 -> SupportBean_S1 -> SupportBean('a' || 'x' = theString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(2));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
                }

                SendSBAssert(env, "a", false);
                SendSBAssert(env, "ax", true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsFromPatternHalfConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[every s0=SupportBean_S0 -> s1=SupportBean_S1 -> SupportBean('a' || s1.p10 = theString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(2, "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
                }

                SendSBAssert(env, "a", false);
                SendSBAssert(env, "ax", true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsFromPatternMulti : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('s0') select * from pattern[every [2] a=SupportBean_S0 -> b=SupportBean_S1 -> SupportBean(theString = a[0].p00 || b.p10)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "a"));
                env.SendEventBean(new SupportBean_S0(2, "b"));
                env.SendEventBean(new SupportBean_S1(2, "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
                }

                SendSBAssert(env, "a", false);
                SendSBAssert(env, "ax", true);

                env.SendEventBean(new SupportBean_S0(1, "z"));
                env.SendEventBean(new SupportBean_S0(2, "-"));
                env.SendEventBean(new SupportBean_S1(2, "y"));

                env.Milestone(0);

                SendSBAssert(env, "ax", false);
                SendSBAssert(env, "zy", true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValEqualsContextWithStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                          "@name('s0') context MyContext select * from SupportBean(theString = context.s0.p00 || context.s0.p01)";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(0, "a", "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env, "s0", "SupportBean", new FilterItem("theString", EQUAL));
                }

                SendSBAssert(env, "a", false);
                SendSBAssert(env, "ax", true);

                env.Milestone(0);

                SendSBAssert(env, "by", false);
                SendSBAssert(env, "ax", true);

                env.UndeployAll();
            }
        }

        public class ExprFilterOptValDisqualify : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
                              "@public create table MyTable(tablecol string);\n" +
                              "@public create window MyWindow#keepall as SupportBean;\n" +
                              "@public create inlined_class \"\"\"\n" +
                              "  public class Helper {\n" +
                              "    public static String doit(Object param) {\n" +
                              "      return null;\n" +
                              "    }\n" +
                              "  }\n" +
                              "\"\"\";\n" +
                              "@public create expression MyDeclaredExpr { (select theString from MyWindow) };\n" +
                              "@public create expression MyHandThrough {v => v};\n" +
                              "@public create expression string js:MyJavaScript(param) [\"a\"];\n";
                env.Compile(objects, path);

                AssertDisqualified(env, path, "SupportBean", "theString=Integer.toString(intPrimitive)");
                AssertDisqualified(env, path, "SupportBean", "theString=MYVARIABLE_NONCONSTANT");
                AssertDisqualified(env, path, "SupportBean", "theString=MyTable.tablecol");
                AssertDisqualified(env, path, "SupportBean", "theString=(select theString from MyWindow)");
                AssertDisqualified(env, path, "SupportBeanArrayCollMap", "id = setOfString.where(v => v=id).firstOf()");
                AssertDisqualified(env, path, "SupportBean", "theString=Helper.doit(*)");
                AssertDisqualified(env, path, "SupportBean", "theString=Helper.doit(me)");
                AssertDisqualified(env, path, "SupportBean", "boolPrimitive=event_identity_equals(me, me)");
                AssertDisqualified(env, path, "SupportBean", "theString=MyDeclaredExpr()");
                AssertDisqualified(env, path, "SupportBean", "intPrimitive=theString.length()");
                AssertDisqualified(env, path, "SupportBean", "intPrimitive = funcOne('hello')");
                AssertDisqualified(env, path, "SupportBean", "boolPrimitive = exists(theString)");
                AssertDisqualified(env, path, "SupportBean", "theString = MyJavaScript('a')");
                AssertDisqualified(env, path, "SupportBean", "theString = MyHandThrough('a')");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private static void TryDeployAndAssertionSB(
            RegressionEnvironment env,
            string epl,
            FilterOperator op,
            AtomicLong milestone)
        {
            env.CompileDeploy(epl).AddListener("s0");
            RunAssertionSB(env, epl, op, milestone);
        }

        private static void RunAssertionSB(
            RegressionEnvironment env,
            string epl,
            FilterOperator op,
            AtomicLong milestone)
        {
            if (HasFilterIndexPlanAdvanced(env)) {
                AssertFilterSvcSingle(env, "s0", "theString", op);
            }

            SendSBAssert(env, "ax", true);
            SendSBAssert(env, "a", false);

            env.MilestoneInc(milestone);

            SendSBAssert(env, "bx", false);
            SendSBAssert(env, "ax", true);

            env.UndeployAll();
        }

        internal static void AssertDisqualified(
            RegressionEnvironment env,
            RegressionPath path,
            string typeName,
            string filters)
        {
            var hook = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" +
                       typeof(SupportFilterPlanHook).FullName +
                       "')";
            var epl = hook + "select * from " + typeName + "(" + filters + ") as me";
            SupportFilterPlanHook.Reset();
            env.Compile(epl, path);
            var forge = SupportFilterPlanHook.AssertPlanSingleTripletAndReset(typeName);
            if (forge.FilterOperator != FilterOperator.BOOLEAN_EXPRESSION && forge.FilterOperator != REBOOL) {
                Assert.Fail();
            }
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            string theString,
            bool received)
        {
            env.SendEventBean(new SupportBean(theString, 0));
            env.AssertListenerInvokedFlag("s0", received);
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            double doublePrimitive,
            bool received)
        {
            var sb = new SupportBean("E", 0);
            sb.DoublePrimitive = doublePrimitive;
            env.SendEventBean(sb);
            env.AssertListenerInvokedFlag("s0", received);
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            long longPrimitive,
            bool received)
        {
            var sb = new SupportBean("E", 0);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
            env.AssertListenerInvokedFlag("s0", received);
        }

        private static void RunAssertionRelOpCoercion(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            if (HasFilterIndexPlanAdvanced(env)) {
                AssertFilterSvcSingle(env, "s0", "doublePrimitive", LESS);
            }

            SendSBAssert(env, 3d, true);
            SendSBAssert(env, 20d, false);
            SendSBAssert(env, 4d, true);

            env.UndeployAll();
        }
    }
} // end of namespace