///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.resultset.querytype
{
    public class ResultSetQueryTypeRollupPlanningAndSODA : RegressionExecution
    {
        public static readonly string PLAN_CALLBACK_HOOK =
            "@Hook(type=" +
            typeof(HookType).FullName +
            ".INTERNAL_GROUPROLLUP_PLAN,hook='" +
            typeof(SupportGroupRollupPlanHook).FullName +
            "')";

        public void Run(RegressionEnvironment env)
        {
            // plain rollup
            Validate(env, "a", "rollup(a)", new[] {"a", ""});
            Validate(env, "a, b", "rollup(a, b)", new[] {"a,b", "a", ""});
            Validate(env, "a, b, c", "rollup(a, b, c)", new[] {"a,b,c", "a,b", "a", ""});
            Validate(env, "a, b, c, d", "rollup(a, b, c, d)", new[] {"a,b,c,d", "a,b,c", "a,b", "a", ""});

            // rollup with unenclosed
            Validate(env, "a, b", "a, rollup(b)", new[] {"a,b", "a"});
            Validate(env, "a, b, c", "a, b, rollup(c)", new[] {"a,b,c", "a,b"});
            Validate(env, "a, b, c", "a, rollup(b, c)", new[] {"a,b,c", "a,b", "a"});
            Validate(env, "a, b, c, d", "a, b, rollup(c, d)", new[] {"a,b,c,d", "a,b,c", "a,b"});
            Validate(env, "a, b, c, d, e", "a, b, rollup(c, d, e)", new[] {"a,b,c,d,e", "a,b,c,d", "a,b,c", "a,b"});

            // plain cube
            Validate(env, "a", "cube(a)", new[] {"a", ""});
            Validate(env, "a, b", "cube(a, b)", new[] {"a,b", "a", "b", ""});
            Validate(env, "a, b, c", "cube(a, b, c)", new[] {"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
            Validate(
                env,
                "a, b, c, d",
                "cube(a, b, c, d)",
                new[] {
                    "a,b,c,d", "a,b,c", "a,b,d",
                    "a,b", "a,c,d", "a,c", "a,d", "a",
                    "b,c,d", "b,c", "b,d", "b",
                    "c,d", "c", "d", ""
                });

            // cube with unenclosed
            Validate(env, "a, b", "a, cube(b)", new[] {"a,b", "a"});
            Validate(env, "a, b, c", "a, cube(b, c)", new[] {"a,b,c", "a,b", "a,c", "a"});
            Validate(
                env,
                "a, b, c, d",
                "a, cube(b, c, d)",
                new[] {"a,b,c,d", "a,b,c", "a,b,d", "a,b", "a,c,d", "a,c", "a,d", "a"});
            Validate(env, "a, b, c, d", "a, b, cube(c, d)", new[] {"a,b,c,d", "a,b,c", "a,b,d", "a,b"});

            // plain grouping set
            Validate(env, "a", "grouping sets(a)", new[] {"a"});
            Validate(env, "a", "grouping sets(a)", new[] {"a"});
            Validate(env, "a, b", "grouping sets(a, b)", new[] {"a", "b"});
            Validate(env, "a, b", "grouping sets(a, b, (a, b), ())", new[] {"a", "b", "a,b", ""});
            Validate(env, "a, b", "grouping sets(a, (a, b), (), b)", new[] {"a", "a,b", "", "b"});
            Validate(env, "a, b, c", "grouping sets((a, b), (a, c), (), (b, c))", new[] {"a,b", "a,c", "", "b,c"});
            Validate(env, "a, b", "grouping sets((a, b))", new[] {"a,b"});
            Validate(env, "a, b, c", "grouping sets((a, b, c), ())", new[] {"a,b,c", ""});
            Validate(env, "a, b, c", "grouping sets((), (a, b, c), (b, c))", new[] {"", "a,b,c", "b,c"});

            // grouping sets with unenclosed
            Validate(env, "a, b", "a, grouping sets(b)", new[] {"a,b"});
            Validate(env, "a, b, c", "a, grouping sets(b, c)", new[] {"a,b", "a,c"});
            Validate(env, "a, b, c", "a, grouping sets((b, c))", new[] {"a,b,c"});
            Validate(
                env,
                "a, b, c, d",
                "a, b, grouping sets((), c, d, (c, d))",
                new[] {"a,b", "a,b,c", "a,b,d", "a,b,c,d"});

            // multiple grouping sets
            Validate(env, "a, b", "grouping sets(a), grouping sets(b)", new[] {"a,b"});
            Validate(env, "a, b, c", "grouping sets(a), grouping sets(b, c)", new[] {"a,b", "a,c"});
            Validate(env, "a, b, c, d", "grouping sets(a, b), grouping sets(c, d)", new[] {"a,c", "a,d", "b,c", "b,d"});
            Validate(env, "a, b, c", "grouping sets((), a), grouping sets(b, c)", new[] {"b", "c", "a,b", "a,c"});
            Validate(env, "a, b, c, d", "grouping sets(a, b, c), grouping sets(d)", new[] {"a,d", "b,d", "c,d"});
            Validate(
                env,
                "a, b, c, d, e",
                "grouping sets(a, b, c), grouping sets(d, e)",
                new[] {"a,d", "a,e", "b,d", "b,e", "c,d", "c,e"});

            // multiple rollups
            Validate(env, "a, b, c", "rollup(a, b), rollup(c)", new[] {"a,b,c", "a,b", "a,c", "a", "c", ""});
            Validate(
                env,
                "a, b, c, d",
                "rollup(a, b), rollup(c, d)",
                new[] {"a,b,c,d", "a,b,c", "a,b", "a,c,d", "a,c", "a", "c,d", "c", ""});

            // grouping sets with rollup or cube inside
            Validate(env, "a, b, c", "grouping sets(a, rollup(b, c))", new[] {"a", "b,c", "b", ""});
            Validate(env, "a, b, c", "grouping sets(a, cube(b, c))", new[] {"a", "b,c", "b", "c", ""});
            Validate(env, "a, b", "grouping sets(rollup(a, b))", new[] {"a,b", "a", ""});
            Validate(env, "a, b", "grouping sets(cube(a, b))", new[] {"a,b", "a", "b", ""});
            Validate(env, "a, b, c, d", "grouping sets((a, b), rollup(c, d))", new[] {"a,b", "c,d", "c", ""});
            Validate(env, "a, b, c, d", "grouping sets(a, b, rollup(c, d))", new[] {"a", "b", "c,d", "c", ""});

            // cube and rollup with combined expression
            Validate(env, "a, b, c", "cube((a, b), c)", new[] {"a,b,c", "a,b", "c", ""});
            Validate(env, "a, b, c", "rollup((a, b), c)", new[] {"a,b,c", "a,b", ""});
            Validate(env, "a, b, c, d", "cube((a, b), (c, d))", new[] {"a,b,c,d", "a,b", "c,d", ""});
            Validate(env, "a, b, c, d", "rollup((a, b), (c, d))", new[] {"a,b,c,d", "a,b", ""});
            Validate(env, "a, b, c", "cube(a, (b, c))", new[] {"a,b,c", "a", "b,c", ""});
            Validate(env, "a, b, c", "rollup(a, (b, c))", new[] {"a,b,c", "a", ""});
            Validate(env, "a, b, c", "grouping sets(rollup((a, b), c))", new[] {"a,b,c", "a,b", ""});

            // multiple cubes and rollups
            Validate(
                env,
                "a, b, c, d",
                "rollup(a, b), rollup(c, d)",
                new[] {
                    "a,b,c,d", "a,b,c", "a,b",
                    "a,c,d", "a,c", "a", "c,d", "c", ""
                });
            Validate(env, "a, b", "cube(a), cube(b)", new[] {"a,b", "a", "b", ""});
            Validate(env, "a, b, c", "cube(a, b), cube(c)", new[] {"a,b,c", "a,b", "a,c", "a", "b,c", "b", "c", ""});
        }

        private static void Validate(
            RegressionEnvironment env,
            string selectClause,
            string groupByClause,
            string[] expectedCSV)
        {
            var epl = PLAN_CALLBACK_HOOK +
                      " select " +
                      selectClause +
                      ", count(*) from SupportEventABCProp group by " +
                      groupByClause;
            SupportGroupRollupPlanHook.Reset();

            env.Compile(epl);
            ComparePlan(expectedCSV);
            env.UndeployAll();

            var model = env.EplToModel(epl);
            Assert.AreEqual(epl, model.ToEPL());
            SupportGroupRollupPlanHook.Reset();

            model.Annotations.Add(AnnotationPart.NameAnnotation("s0"));
            env.CompileDeploy(model).AddListener("s0");
            ComparePlan(expectedCSV);

            env.UndeployAll();
        }

        private static void ComparePlan(string[] expectedCSV)
        {
            var plan = SupportGroupRollupPlanHook.GetPlan();
            var levels = plan.RollupDesc.Levels;
            var received = new string[levels.Length][];
            for (var i = 0; i < levels.Length; i++) {
                var level = levels[i];
                if (level.IsAggregationTop) {
                    received[i] = new string[0];
                }
                else {
                    received[i] = new string[level.RollupKeys.Length];
                    for (var j = 0; j < received[i].Length; j++) {
                        var key = level.RollupKeys[j];
                        received[i][j] =
                            ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(plan.Expressions[key]);
                    }
                }
            }

            Assert.AreEqual(expectedCSV.Length, received.Length, "Received: " + ToCSV(received));
            for (var i = 0; i < expectedCSV.Length; i++) {
                var receivedCSV = ToCSV(received[i]);
                Assert.AreEqual("Failed at row " + i, expectedCSV[i], receivedCSV);
            }
        }

        private static string ToCSV(string[][] received)
        {
            var builder = new StringBuilder();
            var delimiter = "";
            foreach (var item in received) {
                builder.Append(delimiter);
                builder.Append(ToCSV(item));
                delimiter = "  ";
            }

            return builder.ToString();
        }

        private static string ToCSV(string[] received)
        {
            var builder = new StringBuilder();
            var delimiter = "";
            foreach (var item in received) {
                builder.Append(delimiter);
                builder.Append(item);
                delimiter = ",";
            }

            return builder.ToString();
        }
    }
} // end of namespace