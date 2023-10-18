///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

using static com.espertech.esper.common.@internal.filterspec.FilterOperator;
using static com.espertech.esper.regressionlib.support.filter.SupportFilterOptimizableHelper; // hasFilterIndexPlanAdvanced
using static com.espertech.esper.regressionlib.support.filter.SupportFilterServiceHelper;
using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
	public class ExprFilterOptimizableLookupableLimitedExpr {
	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> executions = new List<RegressionExecution>();
	        executions.Add(new ExprFilterOptLkupEqualsOneStmt());
	        executions.Add(new ExprFilterOptLkupEqualsOneStmtWPatternSharingIndex());
	        executions.Add(new ExprFilterOptLkupEqualsMultiStmtSharingIndex());
	        executions.Add(new ExprFilterOptLkupEqualsCoercion());
	        executions.Add(new ExprFilterOptLkupInSetOfValue());
	        executions.Add(new ExprFilterOptLkupInRangeWCoercion());
	        executions.Add(new ExprFilterOptLkupDisqualify());
	        executions.Add(new ExprFilterOptLkupCurrentTimestampWEquals());
	        executions.Add(new ExprFilterOptLkupCurrentTimestampCompare());
	        executions.Add(new ExprFilterOptLkupConstantEqualsNull());
	        return executions;
	    }

	    private class ExprFilterOptLkupConstantEqualsNull : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean(null = 'a');\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupCurrentTimestampCompare : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean(current_timestamp().getSecondOfMinute()%2=0);\n";
	            env.AdvanceTime(0);
	            env.CompileDeploy(epl).AddListener("s0");

	            SendSBAssert(env, true);
	            env.AdvanceTime(999);
	            SendSBAssert(env, true);

	            env.AdvanceTime(1000);
	            SendSBAssert(env, false);
	            env.AdvanceTime(1999);
	            SendSBAssert(env, false);

	            env.AdvanceTime(2000);
	            SendSBAssert(env, true);

	            env.AdvanceTime(3000);
	            SendSBAssert(env, false);

	            env.UndeployAll();
	        }

	        private void SendSBAssert(RegressionEnvironment env, bool received) {
	            env.SendEventBean(new SupportBean());
	            env.AssertListenerInvokedFlag("s0", received);
	        }
	    }

	    private class ExprFilterOptLkupCurrentTimestampWEquals : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from pattern[a=SupportBean -> SupportBean(a.longPrimitive = current_timestamp() + longPrimitive)];\n";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.AdvanceTime(1000);
	            env.SendEventBean(MakeSBLong(1123));
	            if (HasFilterIndexPlanAdvanced(env)) {
	                AssertFilterSvcSingle(env, "s0", "current_timestamp()+longPrimitive", EQUAL);
	            }

	            env.Milestone(0);

	            env.SendEventBean(MakeSBLong(123));
	            env.AssertEventNew("s0", @event => {
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupDisqualify : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var objects = "@public create variable string MYVARIABLE_NONCONSTANT = 'abc';\n" +
	                          "@public create table MyTable(tablecol string);\n" +
	                          "@public create window MyWindow#keepall as SupportBean;\n" +
	                          "@public create inlined_class \"\"\"\n" +
	                          "  public class Helper {\n" +
	                          "    public static String doit(Object param) { return null;}\n" +
	                          "    public static String doit(Object one, Object two) { return null;}\n" +
	                          "  }\n" +
	                          "\"\"\";\n" +
	                          "@public create expression MyDeclaredExpr { (select theString from MyWindow) };\n" +
	                          "@public create expression MyHandThrough {v => v};\n" +
	                          "@public create expression string js:MyJavaScript(param) [\"a\"];\n";
	            env.Compile(objects, path);

	            var hook = "@Hook(type=HookType.INTERNAL_FILTERSPEC, hook='" + typeof(SupportFilterPlanHook).FullName + "')";

	            AssertDisqualified(env, path, "SupportBean",
	                hook + "select * from SupportBean(theString||MYVARIABLE_NONCONSTANT='ax')");
	            AssertDisqualified(env, path, "SupportBean",
	                hook + "select * from SupportBean(theString||MyTable.tablecol='ax')");
	            AssertDisqualified(env, path, "SupportBean",
	                hook + "select * from SupportBean(theString||(select theString from MyWindow)='ax')");
	            AssertDisqualified(env, path, "SupportBeanArrayCollMap",
	                hook + "select * from SupportBeanArrayCollMap(id || setOfString.where(v => v=id).firstOf() = 'ax')");
	            AssertDisqualified(env, path, "SupportBean",
	                hook + "select * from pattern[s0=SupportBean_S0 -> SupportBean(MyJavaScript(theString)='x')]");
	            AssertDisqualified(env, path, "SupportBean",
	                hook + "select * from SupportBean(current_timestamp()=1)");

	            // local inlined class
	            var eplWithLocalHelper = hook + "inlined_class \"\"\"\n" +
	                                     "  public class LocalHelper {\n" +
	                                     "    public static String doit(Object param) {\n" +
	                                     "      return null;\n" +
	                                     "    }\n" +
	                                     "  }\n" +
	                                     "\"\"\"\n" +
	                                     "select * from SupportBean(LocalHelper.doit(theString) = 'abc')";
	            AssertDisqualified(env, path, "SupportBean", eplWithLocalHelper);
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.STATICHOOK);
	        }
	    }

	    private class ExprFilterOptLkupInRangeWCoercion : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            var epl = "@name('s0') select * from pattern [" +
	                      "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive+longBoxed in [a.id - 2 : b.id + 2])];\n";
	            RunAssertionInRange(env, epl, false, milestone);

	            epl = "@name('s0') select * from pattern [" +
	                "a=SupportBean_S0 -> b=SupportBean_S1 -> every SupportBean(longPrimitive+longBoxed not in [a.id - 2 : b.id + 2])];\n";
	            RunAssertionInRange(env, epl, true, milestone);
	        }

	        private void RunAssertionInRange(RegressionEnvironment env, string epl, bool not, AtomicLong milestone) {
	            env.CompileDeploy(epl).AddListener("s0");
	            env.SendEventBean(new SupportBean_S0(10));
	            env.SendEventBean(new SupportBean_S1(200));

	            env.MilestoneInc(milestone);
	            env.AssertThat(() => {
	                if (HasFilterIndexPlanAdvanced(env)) {
	                    AssertFilterSvcSingle(env, "s0", "longPrimitive+longBoxed", not ? NOT_RANGE_CLOSED : RANGE_CLOSED);
	                }
	            });

	            SendSBLongsAssert(env, 3, 4, not);
	            SendSBLongsAssert(env, 5, 3, !not);
	            SendSBLongsAssert(env, 1, 99, !not);
	            SendSBLongsAssert(env, 101, 101, !not);
	            SendSBLongsAssert(env, 200, 3, not);

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupInSetOfValue : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from pattern [" +
	                      "a=SupportBean_S0 -> b=SupportBean_S1 -> c=SupportBean_S2 -> every SupportBean(longPrimitive+longBoxed in (a.id, b.id, c.id))];\n";
	            env.CompileDeploy(epl).AddListener("s0");
	            env.SendEventBean(new SupportBean_S0(10));
	            env.SendEventBean(new SupportBean_S1(200));
	            env.SendEventBean(new SupportBean_S2(3000));

	            env.Milestone(0);

	            if (HasFilterIndexPlanAdvanced(env)) {
	                AssertFilterSvcSingle(env, "s0", "longPrimitive+longBoxed", IN_LIST_OF_VALUES);
	            }

	            SendSBLongsAssert(env, 0, 9, false);
	            SendSBLongsAssert(env, 9, 1, true);
	            SendSBLongsAssert(env, 199, 1, true);
	            SendSBLongsAssert(env, 2090, 910, true);

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupEqualsCoercion : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean(doublePrimitive + doubleBoxed = Integer.parseInt('10'))";
	            env.CompileDeploy(epl).AddListener("s0");
	            if (HasFilterIndexPlanAdvanced(env)) {
	                AssertFilterSvcSingle(env, "s0", "doublePrimitive+doubleBoxed", EQUAL);
	            }

	            env.Milestone(0);

	            SendSBDoublesAssert(env, 5, 5, true);
	            SendSBDoublesAssert(env, 10, 0, true);
	            SendSBDoublesAssert(env, 0, 10, true);
	            SendSBDoublesAssert(env, 0, 9, false);

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupEqualsOneStmt : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@Audit @name('s0') select * from pattern[s0=SupportBean_S0 -> every SupportBean_S1(p10 || p11 = 'ax')];\n";

	            env.CompileDeploy(epl).AddListener("s0");
	            env.SendEventBean(new SupportBean_S0(1));
	            if (HasFilterIndexPlanAdvanced(env)) {
	                AssertFilterSvcSingle(env, "s0", "p10||p11", EQUAL);
	            }

	            env.Milestone(0);

	            SendSB1Assert(env, "a", "x", true);
	            SendSB1Assert(env, "a", "y", false);
	            SendSB1Assert(env, "b", "x", false);
	            SendSB1Assert(env, "a", "x", true);

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupEqualsOneStmtWPatternSharingIndex : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from pattern[every s0=SupportBean_S0 -> every SupportBean_S1('ax' = p10 || p11)] order by s0.id asc;\n";

	            env.CompileDeploy(epl).AddListener("s0");
	            env.SendEventBean(new SupportBean_S0(1));
	            env.SendEventBean(new SupportBean_S0(2));

	            env.Milestone(0);

	            if (HasFilterIndexPlanAdvanced(env)) {
	                AssertFilterSvcMultiSameIndexDepthOne(env, "s0", "SupportBean_S1", 2, "p10||p11", EQUAL);
	            }

	            env.SendEventBean(new SupportBean_S1(10, "a", "x"));
	            env.AssertPropsPerRowLastNew("s0", "s0.id".SplitCsv(), new object[][]{new object[] {1}, new object[] {2}});

	            env.UndeployAll();
	        }
	    }

	    private class ExprFilterOptLkupEqualsMultiStmtSharingIndex : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean_S0(p00 || p01 = 'ax');\n" +
	                      "@name('s1') select * from SupportBean_S0(p00 || p01 = 'ax');\n" +
	                      "" +
	                      "create constant variable string VAR = 'ax';\n" +
	                      "@name('s2') select * from SupportBean_S0(p00 || p01 = VAR);\n" +
	                      "" +
	                      "create context MyContextOne start SupportBean_S1 as s1;\n" +
	                      "@name('s3') context MyContextOne select * from SupportBean_S0(p00 || p01 = context.s1.p10);\n" +
	                      "" +
	                      "create context MyContextTwo start SupportBean_S1 as s1;\n" +
	                      "@name('s4') context MyContextTwo select * from pattern[a=SupportBean_S1 -> SupportBean_S0(a.p10 = p00     ||     p01)];\n";
	            env.CompileDeploy(epl);
	            var names = "s0,s1,s2,s3,s4".SplitCsv();
	            foreach (var name in names) {
	                env.AddListener(name);
	            }
	            env.SendEventBean(new SupportBean_S1(0, "ax"));

	            env.Milestone(0);

	            env.AssertThat(() => {
	                var filters = GetFilterSvcAllStmtForType(env.Runtime, "SupportBean_S0");
	                if (HasFilterIndexPlanAdvanced(env)) {
	                    AssertFilterSvcMultiSameIndexDepthOne(filters, 5, "p00||p01", EQUAL);
	                }
	            });

	            env.SendEventBean(new SupportBean_S0(10, "a", "x"));
	            foreach (var name in names) {
	                env.AssertEventNew(name, @event => {
	                });
	            }

	            env.UndeployAll();
	        }
	    }

	    private static void SendSBDoublesAssert(RegressionEnvironment env, double doublePrimitive, double doubleBoxed, bool received) {
	        var sb = new SupportBean();
	        sb.DoublePrimitive = doublePrimitive;
	        sb.DoubleBoxed = doubleBoxed;
	        env.SendEventBean(sb);
	        env.AssertListenerInvokedFlag("s0", received);
	    }

	    private static void SendSBLongsAssert(RegressionEnvironment env, long longPrimitive, long longBoxed, bool received) {
	        var sb = new SupportBean();
	        sb.LongPrimitive = longPrimitive;
	        sb.LongBoxed = longBoxed;
	        env.SendEventBean(sb);
	        env.AssertListenerInvokedFlag("s0", received);
	    }

	    private static SupportBean MakeSBLong(long longPrimitive) {
	        var sb = new SupportBean();
	        sb.LongPrimitive = longPrimitive;
	        return sb;
	    }

	    private static void SendSB1Assert(RegressionEnvironment env, string p10, string p11, bool received) {
	        env.SendEventBean(new SupportBean_S1(0, p10, p11));
	        env.AssertListenerInvokedFlag("s0", received);
	    }

	    internal static void AssertDisqualified(RegressionEnvironment env, RegressionPath path, string typeName, string epl) {
	        SupportFilterPlanHook.Reset();
	        env.Compile(epl, path);
	        var forge = SupportFilterPlanHook.AssertPlanSingleForTypeAndReset(typeName);
	        Assert.AreEqual(FilterOperator.BOOLEAN_EXPRESSION, forge.FilterOperator);
	    }
	}
} // end of namespace
