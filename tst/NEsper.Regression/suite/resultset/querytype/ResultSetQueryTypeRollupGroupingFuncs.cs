///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.regressionlib.framework.RegressionFlag; // FIREANDFORGET
// INVALIDITY
using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRollupGroupingFuncs
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithDocSampleCarEventAndGroupingFunc(execs);
            WithInvalid(execs);
            WithFAFCarEventAndGroupingFunc(execs);
            WithGroupingFuncExpressionUse(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithGroupingFuncExpressionUse(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeGroupingFuncExpressionUse());
            return execs;
        }

        public static IList<RegressionExecution> WithFAFCarEventAndGroupingFunc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeFAFCarEventAndGroupingFunc());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSampleCarEventAndGroupingFunc(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetQueryTypeDocSampleCarEventAndGroupingFunc());
            return execs;
        }

        private class ResultSetQueryTypeFAFCarEventAndGroupingFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@public create window CarWindow#keepall as SupportCarEvent;\n" +
                          "insert into CarWindow select * from SupportCarEvent;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportCarEvent("skoda", "france", 10000));
                env.SendEventBean(new SupportCarEvent("skoda", "germany", 5000));
                env.SendEventBean(new SupportCarEvent("bmw", "france", 100));
                env.SendEventBean(new SupportCarEvent("bmw", "germany", 1000));
                env.SendEventBean(new SupportCarEvent("opel", "france", 7000));
                env.SendEventBean(new SupportCarEvent("opel", "germany", 7000));

                epl =
                    "@name('s0') select name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place) as gid " +
                    "from CarWindow group by grouping sets((name, place),name, place,())";
                var result = env.CompileExecuteFAF(epl, path);

                Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("grouping(name)"));
                Assert.AreEqual(typeof(int?), result.EventType.GetPropertyType("gid"));

                var fields = new string[] { "name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid" };
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new object[][] {
                        new object[] { "skoda", "france", 10000, 0, 0, 0 },
                        new object[] { "skoda", "germany", 5000, 0, 0, 0 },
                        new object[] { "bmw", "france", 100, 0, 0, 0 },
                        new object[] { "bmw", "germany", 1000, 0, 0, 0 },
                        new object[] { "opel", "france", 7000, 0, 0, 0 },
                        new object[] { "opel", "germany", 7000, 0, 0, 0 },
                        new object[] { "skoda", null, 15000, 0, 1, 1 },
                        new object[] { "bmw", null, 1100, 0, 1, 1 },
                        new object[] { "opel", null, 14000, 0, 1, 1 },
                        new object[] { null, "france", 17100, 1, 0, 2 },
                        new object[] { null, "germany", 13000, 1, 0, 2 },
                        new object[] { null, null, 30100, 1, 1, 3 }
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(FIREANDFORGET);
            }
        }

        private class ResultSetQueryTypeDocSampleCarEventAndGroupingFunc : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // try simple
                var epl =
                    "@name('s0') select name, place, sum(count), grouping(name), grouping(place), grouping_id(name,place) as gid " +
                    "from SupportCarEvent group by grouping sets((name, place), name, place, ())";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionDocSampleCarEvent(env, milestone);
                env.UndeployAll();

                // try audit
                env.CompileDeploy("@Audit " + epl).AddListener("s0");
                TryAssertionDocSampleCarEvent(env, milestone);
                env.UndeployAll();

                // try model
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertionDocSampleCarEvent(env, milestone);

                env.UndeployAll();
            }

            private static void TryAssertionDocSampleCarEvent(
                RegressionEnvironment env,
                AtomicLong milestone)
            {
                var fields = new string[] { "name", "place", "sum(count)", "grouping(name)", "grouping(place)", "gid" };
                env.SendEventBean(new SupportCarEvent("skoda", "france", 100));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "skoda", "france", 100, 0, 0, 0 },
                        new object[] { "skoda", null, 100, 0, 1, 1 },
                        new object[] { null, "france", 100, 1, 0, 2 },
                        new object[] { null, null, 100, 1, 1, 3 }
                    });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportCarEvent("skoda", "germany", 75));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "skoda", "germany", 75, 0, 0, 0 },
                        new object[] { "skoda", null, 175, 0, 1, 1 },
                        new object[] { null, "germany", 75, 1, 0, 2 },
                        new object[] { null, null, 175, 1, 1, 3 }
                    });
            }
        }

        private class ResultSetQueryTypeGroupingFuncExpressionUse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                GroupingSupportFunc.Parameters.Clear();

                // test uncorrelated subquery and expression-declaration and single-row func
                var epl = "create expression myExpr {x=> '|' || x.name || '|'};\n" +
                          "@name('s0') select myfunc(" +
                          "  name, place, sum(count), grouping(name), grouping(place), grouping_id(name, place)," +
                          "  (select refId from SupportCarInfoEvent#lastevent), " +
                          "  myExpr(ce)" +
                          "  )" +
                          "from SupportCarEvent ce group by grouping sets((name, place),name, place,())";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportCarInfoEvent("a", "b", "c01"));

                env.SendEventBean(new SupportCarEvent("skoda", "france", 10000));
                env.AssertThat(
                    () => {
                        EPAssertionUtil.AssertEqualsExactOrder(
                            new object[][] {
                                new object[] { "skoda", "france", 10000, 0, 0, 0, "c01", "|skoda|" },
                                new object[] { "skoda", null, 10000, 0, 1, 1, "c01", "|skoda|" },
                                new object[] { null, "france", 10000, 1, 0, 2, "c01", "|skoda|" },
                                new object[] { null, null, 10000, 1, 1, 3, "c01", "|skoda|" }
                            },
                            GroupingSupportFunc.AssertGetAndClear(4));
                    });
                env.UndeployAll();

                // test "prev" and "prior"
                var fields = "c0,c1,c2,c3".SplitCsv();
                var eplTwo =
                    "@name('s0') select prev(1, name) as c0, prior(1, name) as c1, name as c2, sum(count) as c3 " +
                    "from SupportCarEvent#keepall ce group by rollup(name)";
                env.CompileDeploy(eplTwo).AddListener("s0");

                env.SendEventBean(new SupportCarEvent("skoda", "france", 10));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { null, null, "skoda", 10 }, new object[] { null, null, null, 10 }
                    });

                env.SendEventBean(new SupportCarEvent("vw", "france", 15));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "skoda", "skoda", "vw", 15 }, new object[] { "skoda", "skoda", null, 25 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetQueryTypeInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid use of function
                var expected =
                    "Failed to validate select-clause expression 'grouping(theString)': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select grouping(theString) from SupportBean]";
                env.TryInvalidCompile("select grouping(theString) from SupportBean", expected);
                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive) from SupportBean(grouping(theString) = 1) group by rollup(theString)",
                    "Failed to validate filter expression 'grouping(theString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean(grouping(theString) = 1) group by rollup(theString)]");
                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive) from SupportBean where grouping(theString) = 1 group by rollup(theString)",
                    "Failed to validate filter expression 'grouping(theString)=1': The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean where grouping(theString) = 1 group by rollup(theString)]");
                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive) from SupportBean group by rollup(grouping(theString))",
                    "The grouping function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause [select theString, sum(intPrimitive) from SupportBean group by rollup(grouping(theString))]");

                // invalid parameters
                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive), grouping(longPrimitive) from SupportBean group by rollup(theString)",
                    "Failed to find expression 'longPrimitive' among group-by expressions");
                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive), grouping(theString||'x') from SupportBean group by rollup(theString)",
                    "Failed to find expression 'theString||\"x\"' among group-by expressions [select theString, sum(intPrimitive), grouping(theString||'x') from SupportBean group by rollup(theString)]");

                env.TryInvalidCompile(
                    "select theString, sum(intPrimitive), grouping_id(theString, theString) from SupportBean group by rollup(theString)",
                    "Duplicate expression 'theString' among grouping function parameters [select theString, sum(intPrimitive), grouping_id(theString, theString) from SupportBean group by rollup(theString)]");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(INVALIDITY);
            }
        }

        public class GroupingSupportFunc
        {
            private static IList<object[]> parameters = new List<object[]>();

            public static void Myfunc(
                string name,
                string place,
                int? cnt,
                int? grpName,
                int? grpPlace,
                int? grpId,
                string refId,
                string namePlusDelim)
            {
                parameters.Add(new object[] { name, place, cnt, grpName, grpPlace, grpId, refId, namePlusDelim });
            }

            public static IList<object[]> Parameters => parameters;

            public static object[][] AssertGetAndClear(int numRows)
            {
                Assert.AreEqual(numRows, parameters.Count);
                var result = parameters.ToArray();
                parameters.Clear();
                return result;
            }
        }
    }
} // end of namespace