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
            "@Hook(HookType=" +
            typeof(HookType).FullName +
            ".INTERNAL_GROUPROLLUP_PLAN,Hook='" +
            typeof(SupportGroupRollupPlanHook).FullName +
            "')";

        public void Run(RegressionEnvironment env)
        {
            // plain rollup
            Validate(env, "A", "rollup(A)", new[] {"A", ""});
            Validate(env, "A, B", "rollup(A, B)", new[] {"A,B", "A", ""});
            Validate(env, "A, B, C", "rollup(A, B, C)", new[] {"A,B,C", "A,B", "A", ""});
            Validate(env, "A, B, C, D", "rollup(A, B, C, D)", new[] {"A,B,C,D", "A,B,C", "A,B", "A", ""});

            // rollup with unenclosed
            Validate(env, "A, B", "A, rollup(B)", new[] {"A,B", "A"});
            Validate(env, "A, B, C", "A, B, rollup(C)", new[] {"A,B,C", "A,B"});
            Validate(env, "A, B, C", "A, rollup(B, C)", new[] {"A,B,C", "A,B", "A"});
            Validate(env, "A, B, C, D", "A, B, rollup(C, D)", new[] {"A,B,C,D", "A,B,C", "A,B"});
            Validate(env, "A, B, C, D, E", "A, B, rollup(C, D, E)", new[] {"A,B,C,D,E", "A,B,C,D", "A,B,C", "A,B"});

            // plain cube
            Validate(env, "A", "cube(A)", new[] {"A", ""});
            Validate(env, "A, B", "cube(A, B)", new[] {"A,B", "A", "B", ""});
            Validate(env, "A, B, C", "cube(A, B, C)", new[] {"A,B,C", "A,B", "A,C", "A", "B,C", "B", "C", ""});
            Validate(
                env,
                "A, B, C, D",
                "cube(A, B, C, D)",
                new[] {
                    "A,B,C,D", "A,B,C", "A,B,D",
                    "A,B", "A,C,D", "A,C", "A,D", "A",
                    "B,C,D", "B,C", "B,D", "B",
                    "C,D", "C", "D", ""
                });

            // cube with unenclosed
            Validate(env, "A, B", "A, cube(B)", new[] {"A,B", "A"});
            Validate(env, "A, B, C", "A, cube(B, C)", new[] {"A,B,C", "A,B", "A,C", "A"});
            Validate(
                env,
                "A, B, C, D",
                "A, cube(B, C, D)",
                new[] {"A,B,C,D", "A,B,C", "A,B,D", "A,B", "A,C,D", "A,C", "A,D", "A"});
            Validate(env, "A, B, C, D", "A, B, cube(C, D)", new[] {"A,B,C,D", "A,B,C", "A,B,D", "A,B"});

            // plain grouping set
            Validate(env, "A", "grouping sets(A)", new[] {"A"});
            Validate(env, "A", "grouping sets(A)", new[] {"A"});
            Validate(env, "A, B", "grouping sets(A, B)", new[] {"A", "B"});
            Validate(env, "A, B", "grouping sets(A, B, (A, B), ())", new[] {"A", "B", "A,B", ""});
            Validate(env, "A, B", "grouping sets(A, (A, B), (), B)", new[] {"A", "A,B", "", "B"});
            Validate(env, "A, B, C", "grouping sets((A, B), (A, C), (), (B, C))", new[] {"A,B", "A,C", "", "B,C"});
            Validate(env, "A, B", "grouping sets((A, B))", new[] {"A,B"});
            Validate(env, "A, B, C", "grouping sets((A, B, C), ())", new[] {"A,B,C", ""});
            Validate(env, "A, B, C", "grouping sets((), (A, B, C), (B, C))", new[] {"", "A,B,C", "B,C"});

            // grouping sets with unenclosed
            Validate(env, "A, B", "A, grouping sets(B)", new[] {"A,B"});
            Validate(env, "A, B, C", "A, grouping sets(B, C)", new[] {"A,B", "A,C"});
            Validate(env, "A, B, C", "A, grouping sets((B, C))", new[] {"A,B,C"});
            Validate(
                env,
                "A, B, C, D",
                "A, B, grouping sets((), C, D, (C, D))",
                new[] {"A,B", "A,B,C", "A,B,D", "A,B,C,D"});

            // multiple grouping sets
            Validate(env, "A, B", "grouping sets(A), grouping sets(B)", new[] {"A,B"});
            Validate(env, "A, B, C", "grouping sets(A), grouping sets(B, C)", new[] {"A,B", "A,C"});
            Validate(env, "A, B, C, D", "grouping sets(A, B), grouping sets(C, D)", new[] {"A,C", "A,D", "B,C", "B,D"});
            Validate(env, "A, B, C", "grouping sets((), A), grouping sets(B, C)", new[] {"B", "C", "A,B", "A,C"});
            Validate(env, "A, B, C, D", "grouping sets(A, B, C), grouping sets(D)", new[] {"A,D", "B,D", "C,D"});
            Validate(
                env,
                "A, B, C, D, E",
                "grouping sets(A, B, C), grouping sets(D, E)",
                new[] {"A,D", "A,E", "B,D", "B,E", "C,D", "C,E"});

            // multiple rollups
            Validate(env, "A, B, C", "rollup(A, B), rollup(C)", new[] {"A,B,C", "A,B", "A,C", "A", "C", ""});
            Validate(
                env,
                "A, B, C, D",
                "rollup(A, B), rollup(C, D)",
                new[] {"A,B,C,D", "A,B,C", "A,B", "A,C,D", "A,C", "A", "C,D", "C", ""});

            // grouping sets with rollup or cube inside
            Validate(env, "A, B, C", "grouping sets(A, rollup(B, C))", new[] {"A", "B,C", "B", ""});
            Validate(env, "A, B, C", "grouping sets(A, cube(B, C))", new[] {"A", "B,C", "B", "C", ""});
            Validate(env, "A, B", "grouping sets(rollup(A, B))", new[] {"A,B", "A", ""});
            Validate(env, "A, B", "grouping sets(cube(A, B))", new[] {"A,B", "A", "B", ""});
            Validate(env, "A, B, C, D", "grouping sets((A, B), rollup(C, D))", new[] {"A,B", "C,D", "C", ""});
            Validate(env, "A, B, C, D", "grouping sets(A, B, rollup(C, D))", new[] {"A", "B", "C,D", "C", ""});

            // cube and rollup with combined expression
            Validate(env, "A, B, C", "cube((A, B), C)", new[] {"A,B,C", "A,B", "C", ""});
            Validate(env, "A, B, C", "rollup((A, B), C)", new[] {"A,B,C", "A,B", ""});
            Validate(env, "A, B, C, D", "cube((A, B), (C, D))", new[] {"A,B,C,D", "A,B", "C,D", ""});
            Validate(env, "A, B, C, D", "rollup((A, B), (C, D))", new[] {"A,B,C,D", "A,B", ""});
            Validate(env, "A, B, C", "cube(A, (B, C))", new[] {"A,B,C", "A", "B,C", ""});
            Validate(env, "A, B, C", "rollup(A, (B, C))", new[] {"A,B,C", "A", ""});
            Validate(env, "A, B, C", "grouping sets(rollup((A, B), C))", new[] {"A,B,C", "A,B", ""});

            // multiple cubes and rollups
            Validate(
                env,
                "A, B, C, D",
                "rollup(A, B), rollup(C, D)",
                new[] {
                    "A,B,C,D", "A,B,C", "A,B",
                    "A,C,D", "A,C", "A", "C,D", "C", ""
                });
            Validate(env, "A, B", "cube(A), cube(B)", new[] {"A,B", "A", "B", ""});
            Validate(env, "A, B, C", "cube(A, B), cube(C)", new[] {"A,B,C", "A,B", "A,C", "A", "B,C", "B", "C", ""});
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
                Assert.AreEqual(expectedCSV[i], receivedCSV, "Failed at row " + i);
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