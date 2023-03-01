///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterOptimizableLookupableLimitedExpr
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var executions = new List<RegressionExecution>();
            executions.Add(new ExprFilterOptLkupEqualsOneStmt());
            executions.Add(new ExprFilterOptLkupEqualsOneStmtWPatternSharingIndex());
            executions.Add(new ExprFilterOptLkupEqualsMultiStmtSharingIndex());
            executions.Add(new ExprFilterOptLkupEqualsCoercion());
            executions.Add(new ExprFilterOptLkupInSetOfValue());
            executions.Add(new ExprFilterOptLkupInRangeWCoercion());
            executions.Add(new ExprFilterOptLkupDisqualify());
            executions.Add(new ExprFilterOptLkupCurrentTimestamp());
            return executions;
        }

        private class ExprFilterOptLkupCurrentTimestamp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern[a=SupportBean -> SupportBean(a.LongPrimitive = current_timestamp() + LongPrimitive)];\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.AdvanceTime(1000);
                env.SendEventBean(MakeSBLong(1123));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "current_timestamp()+LongPrimitive", EQUAL);
                }

                env.Milestone(0);

                env.SendEventBean(MakeSBLong(123));
                env.Listener("s0").AssertOneGetNewAndReset();

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupDisqualify : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
                              "@public create table MyTable(tablecol string);\n" +
                              "@public create window MyWindow#keepall as SupportBean;\n" +
                              "@public create inlined_class \"\"\"\n" +
                              "  public class Helper {\n" +
                              "    public static string Doit(object param) { return null;}\n" +
                              "    public static string Doit(object one, object two) { return null;}\n" +
                              "  }\n" +
                              "\"\"\";\n" +
                              "@public create expression MyDeclaredExpr { (select TheString from MyWindow) };\n" +
                              "@public create expression MyHandThrough {v => v};\n" +
                              "@public create expression string js:MyJavaScript(param) [\"a\"];\n";
                env.Compile(objects, path);

                var hook = "@Hook(HookType=HookType.INTERNAL_FILTERSPEC, Hook='" + nameof(SupportFilterPlanHook) + "')";

                AssertDisqualified(
                    env,
                    path,
                    "SupportBean",
                    hook + "select * from SupportBean(TheString||MYVARIABLE_NONCONSTANT='ax')");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBean",
                    hook + "select * from SupportBean(TheString||MyTable.tablecol='ax')");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBean",
                    hook + "select * from SupportBean(TheString||(select TheString from MyWindow)='ax')");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBeanArrayCollMap",
                    hook + "select * from SupportBeanArrayCollMap(Id || SetOfString.where(v => v=Id).firstOf() = 'ax')");
                AssertDisqualified(
                    env,
                    path,
                    "SupportBean",
                    hook + "select * from pattern[s0=SupportBean_S0 -> SupportBean(MyJavaScript(TheString)='x')]");

                // local inlined class
                var eplWithLocalHelper = hook +
                                         "inlined_class \"\"\"\n" +
                                         "  public class LocalHelper {\n" +
                                         "    public static string Doit(object param) {\n" +
                                         "      return null;\n" +
                                         "    }\n" +
                                         "  }\n" +
                                         "\"\"\"\n" +
                                         "select * from SupportBean(LocalHelper.Doit(TheString) = 'abc')";
                AssertDisqualified(env, path, "SupportBean", eplWithLocalHelper);
            }
        }

        private class ExprFilterOptLkupInRangeWCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [" +
                          "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(LongPrimitive+LongBoxed in [a.Id - 2 : b.Id + 2])];\n";
                RunAssertionInRange(env, epl, false);

                epl = "@Name('s0') select * from pattern [" +
                      "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(LongPrimitive+LongBoxed not in [a.Id - 2 : b.Id + 2])];\n";
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
                    AssertFilterSvcSingle(env.Statement("s0"), "LongPrimitive+LongBoxed", not ? NOT_RANGE_CLOSED : RANGE_CLOSED);
                }

                SendSBLongsAssert(env, 3, 4, not);
                SendSBLongsAssert(env, 5, 3, !not);
                SendSBLongsAssert(env, 1, 99, !not);
                SendSBLongsAssert(env, 101, 101, !not);
                SendSBLongsAssert(env, 200, 3, not);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupInSetOfValue : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern [" +
                          "a=SupportBean_S0 -> b=SupportBean_S1 -> c=SupportBean_S2 -> every SupportBean(LongPrimitive+LongBoxed in (a.Id, b.Id, c.Id))];\n";
                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S1(200));
                env.SendEventBean(new SupportBean_S2(3000));

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "LongPrimitive+LongBoxed", IN_LIST_OF_VALUES);
                }

                SendSBLongsAssert(env, 0, 9, false);
                SendSBLongsAssert(env, 9, 1, true);
                SendSBLongsAssert(env, 199, 1, true);
                SendSBLongsAssert(env, 2090, 910, true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupEqualsCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean(DoublePrimitive + DoubleBoxed = Int32.Parse('10'))";
                env.CompileDeploy(epl).AddListener("s0");
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "DoublePrimitive+DoubleBoxed", EQUAL);
                }

                env.Milestone(0);

                SendSBDoublesAssert(env, 5, 5, true);
                SendSBDoublesAssert(env, 10, 0, true);
                SendSBDoublesAssert(env, 0, 10, true);
                SendSBDoublesAssert(env, 0, 9, false);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupEqualsOneStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Audit @name('s0') select * from pattern[s0=SupportBean_S0 -> every SupportBean_S1(P10 || P11 = 'ax')];\n";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcSingle(env.Statement("s0"), "P10||P11", EQUAL);
                }

                env.Milestone(0);

                SendSB1Assert(env, "a", "x", true);
                SendSB1Assert(env, "a", "y", false);
                SendSB1Assert(env, "b", "x", false);
                SendSB1Assert(env, "a", "x", true);

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupEqualsOneStmtWPatternSharingIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from pattern[every s0=SupportBean_S0 -> every SupportBean_S1('ax' = P10 || P11)] order by s0.Id asc;\n";

                env.CompileDeploy(epl).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));

                env.Milestone(0);

                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcMultiSameIndexDepthOne(env.Statement("s0"), "SupportBean_S1", 2, "P10||P11", EQUAL);
                }

                env.SendEventBean(new SupportBean_S1(10, "a", "x"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "s0.Id".SplitCsv(),
                    new object[][] {
                        new object[] {1},
                        new object[] {2}
                    });

                env.UndeployAll();
            }
        }

        private class ExprFilterOptLkupEqualsMultiStmtSharingIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean_S0(P00 || P01 = 'ax');\n" +
                          "@Name('s1') select * from SupportBean_S0(P00 || P01 = 'ax');\n" +
                          "" +
                          "create constant variable string VAR = 'ax';\n" +
                          "@Name('s2') select * from SupportBean_S0(P00 || P01 = VAR);\n" +
                          "" +
                          "create context MyContextOne start SupportBean_S1 as s1;\n" +
                          "@Name('s3') context MyContextOne select * from SupportBean_S0(P00 || P01 = context.s1.P10);\n" +
                          "" +
                          "create context MyContextTwo start SupportBean_S1 as s1;\n" +
                          "@Name('s4') context MyContextTwo select * from pattern[a=SupportBean_S1 -> SupportBean_S0(a.P10 = P00     ||     P01)];\n";
                env.CompileDeploy(epl);
                var names = "s0,s1,s2,s3,s4".SplitCsv();
                foreach (var name in names) {
                    env.AddListener(name);
                }

                env.SendEventBean(new SupportBean_S1(0, "ax"));

                env.Milestone(0);

                var filters = GetFilterSvcAllStmtForType(env.Runtime, "SupportBean_S0");
                if (HasFilterIndexPlanAdvanced(env)) {
                    AssertFilterSvcMultiSameIndexDepthOne(filters, 5, "P00||P01", EQUAL);
                }

                env.SendEventBean(new SupportBean_S0(10, "a", "x"));
                foreach (var name in names) {
                    env.Listener(name).AssertOneGetNewAndReset();
                }

                env.UndeployAll();
            }
        }

        private static void SendSBDoublesAssert(
            RegressionEnvironment env,
            double doublePrimitive,
            double doubleBoxed,
            bool received)
        {
            var sb = new SupportBean();
            sb.DoublePrimitive = doublePrimitive;
            sb.DoubleBoxed = doubleBoxed;
            env.SendEventBean(sb);
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static void SendSBLongsAssert(
            RegressionEnvironment env,
            long longPrimitive,
            long longBoxed,
            bool received)
        {
            var sb = new SupportBean();
            sb.LongPrimitive = longPrimitive;
            sb.LongBoxed = longBoxed;
            env.SendEventBean(sb);
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        private static SupportBean MakeSBLong(long longPrimitive)
        {
            var sb = new SupportBean();
            sb.LongPrimitive = longPrimitive;
            return sb;
        }

        private static void SendSB1Assert(
            RegressionEnvironment env,
            string p10,
            string p11,
            bool received)
        {
            env.SendEventBean(new SupportBean_S1(0, p10, p11));
            Assert.AreEqual(received, env.Listener("s0").IsInvokedAndReset());
        }

        protected static void AssertDisqualified(
            RegressionEnvironment env,
            RegressionPath path,
            string typeName,
            string epl)
        {
            SupportFilterPlanHook.Reset();
            env.Compile(epl, path);
            var forge = SupportFilterPlanHook.AssertPlanSingleForTypeAndReset(typeName);
            Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, forge.FilterOperator);
        }
    }
} // end of namespace