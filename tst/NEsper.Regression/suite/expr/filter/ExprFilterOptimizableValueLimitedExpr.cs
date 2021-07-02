///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableValueLimitedExpr
    {
        public static ICollection<RegressionExecution> Executions()
        {
            List<RegressionExecution> execs = new List<RegressionExecution>();
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
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValOrRewrite());
            return execs;
        }

        public static IList<RegressionExecution> WithInRangeWCoercion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValInRangeWCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithInSetOfValueWPatternWCoercion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValInSetOfValueWPatternWCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithDisqualify(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValDisqualify());
            return execs;
        }

        public static IList<RegressionExecution> WithRelOpCoercion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValRelOpCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsCoercion(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsConstantVariable(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsConstantVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsSubstitutionParams(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsSubstitutionParams());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsContextWithStart(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsContextWithStart());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternWithDotMethod(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternWithDotMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternHalfConstant(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternHalfConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternConstant(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternConstant());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternMulti(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternMulti());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsFromPatternSingle(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsFromPatternSingle());
            return execs;
        }

        public static IList<RegressionExecution> WithEqualsIsConstant(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprFilterOptValEqualsIsConstant());
            return execs;
        }

        private class ExprFilterOptValOrRewrite : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                             "@Name('s0') context MyContext select * from SupportBean(TheString = context.s0.P00 || context.s0.P01 or TheString = context.s0.P01 || context.s0.P00);\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "a", "b"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", IN_LIST_OF_VALUES);
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
                string epl = "@Name('s0') select * from pattern [" +
                             "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(LongPrimitive in [a.Id - 2 : b.Id + 2])];\n";
                RunAssertionInRange(env, epl, false);

                epl = "@Name('s0') select * from pattern [" +
                      "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(LongPrimitive not in [a.Id - 2 : b.Id + 2])];\n";
                RunAssertionInRange(env, epl, true);
            }

            private void RunAssertionInRange(
                RegressionEnvironment env,
                string epl,
                bool not)
            {
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S1(200));

                env.Milestone(0);
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "LongPrimitive", not ? NOT_RANGE_CLOSED : RANGE_CLOSED);
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
                string epl = "@Name('s0') select * from pattern [" +
                             "a=SupportBean_S0 -> b=SupportBean_S1 -> c=SupportBean_S2 -> every SupportBean(LongPrimitive in (a.Id, b.Id, c.Id))];\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S1(200));
                env.SendEventBean(new SupportBean_S2(3000));

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "LongPrimitive", IN_LIST_OF_VALUES);
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
                string epl = "@Name('s0') select * from pattern [a=SupportBean -> b=SupportBean(TheString=a.GetTheString())]";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "TheString", EQUAL);
                }

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), "a.IntPrimitive,b.IntPrimitive".SplitCsv(), new object[] {1, 2});
                env.UndeployAll();
            }
        }

        public class ExprFilterOptValRelOpCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from SupportBean(Int32.Parse('10') > DoublePrimitive)";
                RunAssertionRelOpCoercion(env, epl);

                epl = "@Name('s0') select * from SupportBean(DoublePrimitive < Int32.Parse('10'))";
                RunAssertionRelOpCoercion(env, epl);
            }
        }

        public class ExprFilterOptValEqualsCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from SupportBean(DoublePrimitive = Int32.Parse('10') + Int64.Parse('20'))";
                env.CompileDeploy(epl).AddListener("s0");
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "DoublePrimitive", EQUAL);
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
                string variable = "create constant variable string MYCONST = 'a';\n";
                TryDeployAndAssertionSB(env, variable + "@Name('s0') select * from SupportBean(TheString = MYCONST || 'x')", EQUAL);
                TryDeployAndAssertionSB(env, variable + "@Name('s0') select * from SupportBean(MYCONST || 'x' = TheString)", EQUAL);
            }
        }

        public class ExprFilterOptValEqualsSubstitutionParams : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from SupportBean(TheString = ?::string)";
                EPCompiled compiled = env.Compile(epl);
                DeploymentOptions options = new DeploymentOptions();
                options.WithStatementSubstitutionParameter(opt => opt.SetObject(1, "ax"));
                env.Deploy(compiled, options).AddListener("s0");
                RunAssertionSB(env, epl, EQUAL);
            }
        }

        public class ExprFilterOptValEqualsIsConstant : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryDeployAndAssertionSB(env, "@Name('s0') select * from SupportBean(TheString = 'a' || 'x')", EQUAL);
                TryDeployAndAssertionSB(env, "@Name('s0') select * from SupportBean('a' || 'x' is TheString)", IS);
            }
        }

        public class ExprFilterOptValEqualsFromPatternSingle : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "@Name('s0') select * from pattern[every a=SupportBean_S0 -> SupportBean(a.P00 || a.P01 = TheString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(0, "a", "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
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
                string epl = "@Name('s0') select * from pattern[every SupportBean_S0 -> SupportBean_S1 -> SupportBean('a' || 'x' = TheString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(2));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
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
                string epl = "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> s1=SupportBean_S1 -> SupportBean('a' || s1.P10 = TheString)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(2, "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
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
                string epl = "@Name('s0') select * from pattern[every [2] a=SupportBean_S0 -> b=SupportBean_S1 -> SupportBean(TheString = a[0].P00 || b.P10)]";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "a"));
                env.SendEventBean(new SupportBean_S0(2, "b"));
                env.SendEventBean(new SupportBean_S1(2, "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
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
                string epl = "create context MyContext start SupportBean_S0 as s0;\n" +
                             "@Name('s0') context MyContext select * from SupportBean(TheString = context.s0.P00 || context.s0.P01)";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(0, "a", "x"));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcByTypeSingle(env.Statement("s0"), "SupportBean", new FilterItem("TheString", EQUAL));
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
                RegressionPath path = new RegressionPath();
                string namespc = NamespaceGenerator.Create();
                string objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
                                 "@public create table MyTable(tablecol string);\n" +
                                 "@public create window MyWindow#keepall as SupportBean;\n" +
                                 "@public create inlined_class \"\"\"\n" +
                                 "namespace " +
                                 namespc +
                                 " {\n" +
                                 "  public class Helper {\n" +
                                 "    public static string Doit(object param) {\n" +
                                 "      return null;\n" +
                                 "    }\n" +
                                 "  }\n" +
                                 "}\n" +
                                 "\"\"\";\n" +
                                 "@public create expression MyDeclaredExpr { (select TheString from MyWindow) };\n" +
                                 "@public create expression MyHandThrough {v => v};\n" +
                                 "@public create expression string js:MyJavaScript(param) [\"a\"];\n";
                env.Compile(objects, path);

                AssertDisqualified(env, path, "SupportBean", "TheString=Convert.ToString(IntPrimitive)");
                AssertDisqualified(env, path, "SupportBean", "TheString=MYVARIABLE_NONCONSTANT");
                AssertDisqualified(env, path, "SupportBean", "TheString=MyTable.tablecol");
                AssertDisqualified(env, path, "SupportBean", "TheString=(select TheString from MyWindow)");
                AssertDisqualified(env, path, "SupportBeanArrayCollMap", "Id = SetOfString.where(v => v=Id).firstOf()");
                AssertDisqualified(env, path, "SupportBean", $"TheString={namespc}.Helper.Doit(*)");
                AssertDisqualified(env, path, "SupportBean", $"TheString={namespc}.Helper.Doit(me)");
                AssertDisqualified(env, path, "SupportBean", "BoolPrimitive=event_identity_equals(me, me)");
                AssertDisqualified(env, path, "SupportBean", "TheString=MyDeclaredExpr()");
                AssertDisqualified(env, path, "SupportBean", "IntPrimitive=me.TheString.Length");
                AssertDisqualified(env, path, "SupportBean", "IntPrimitive = funcOne('hello')");
                AssertDisqualified(env, path, "SupportBean", "BoolPrimitive = exists(TheString)");
                AssertDisqualified(env, path, "SupportBean", "TheString = MyJavaScript('a')");
                AssertDisqualified(env, path, "SupportBean", "TheString = MyHandThrough('a')");
            }
        }

        private static void TryDeployAndAssertionSB(
            RegressionEnvironment env,
            string epl,
            FilterOperator op)
        {
            env.CompileDeploy(epl).AddListener("s0");
            RunAssertionSB(env, epl, op);
        }

        private static void RunAssertionSB(
            RegressionEnvironment env,
            string epl,
            FilterOperator op)
        {
            if (HasFilterIndexPlanAdvanced(env)) {
                AssertFilterSvcSingle(env.Statement("s0"), "TheString", op);
            }

            SendSBAssert(env, "ax", true);
            SendSBAssert(env, "a", false);

            env.Milestone(0);

            SendSBAssert(env, "bx", false);
            SendSBAssert(env, "ax", true);

            env.UndeployAll();
        }

        protected static void AssertDisqualified(
            RegressionEnvironment env,
            RegressionPath path,
            string typeName,
            string filters)
        {
            string hook = "@Hook(HookType=HookType.INTERNAL_FILTERSPEC, Hook='" + nameof(SupportFilterPlanHook) + "')";
            string epl = hook + "select * from " + typeName + "(" + filters + ") as me";
            SupportFilterPlanHook.Reset();
            env.Compile(epl, path);
            FilterSpecParamForge forge = SupportFilterPlanHook.AssertPlanSingleTripletAndReset(typeName);
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
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            double doublePrimitive,
            bool received)
        {
            SupportBean sb = new SupportBean("E", 0);
            sb.DoublePrimitive = doublePrimitive;
            env.SendEventBean(sb);
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendSBAssert(
            RegressionEnvironment env,
            long longPrimitive,
            bool received)
        {
            SupportBean sb = new SupportBean("E", 0);
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static void RunAssertionRelOpCoercion(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            if (HasFilterIndexPlanAdvanced(env)) {
                AssertFilterSvcSingle(env.Statement("s0"), "DoublePrimitive", LESS);
            }

            SendSBAssert(env, 3d, true);
            SendSBAssert(env, 20d, false);
            SendSBAssert(env, 4d, true);

            env.UndeployAll();
        }
    }
} // end of namespace