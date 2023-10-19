///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogRepetition
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithRepeats(execs);
            WithPrev(execs);
            WithInvalid(execs);
            WithDocSamples(execs);
            WithEquivalent(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEquivalent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRepetitionEquivalent());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSamples(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRepetitionDocSamples());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRepetitionInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRepetitionPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithRepeats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new RowRecogRepetitionRepeats(false));
            execs.Add(new RowRecogRepetitionRepeats(true));
            return execs;
        }

        private class RowRecogRepetitionDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunDocSampleExactlyN(env, milestone);
                RunDocSampleNOrMore_and_BetweenNandM(env, "A{2,} B", milestone);
                RunDocSampleNOrMore_and_BetweenNandM(env, "A{2,3} B", milestone);
                RunDocSampleUpToN(env, milestone);
            }
        }

        private static void RunDocSampleUpToN(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "a0_id,a1_id,b_id".SplitCsv();
            var epl = "@name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].id as a0_id, A[1].id as a1_id, B.id as b_id\n" +
                      "  pattern (A{,2} B)\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100,\n" +
                      "\tB as B.temp >= 102)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] { "E1", 1, 99d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E2", 1, 100d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E3", 1, 100d }, "TemperatureSensorEvent");

            env.MilestoneInc(milestone);

            env.SendEventObjectArray(new object[] { "E4", 1, 101d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E5", 1, 102d }, "TemperatureSensorEvent");
            env.AssertPropsNew("s0", fields, new object[] { "E3", "E4", "E5" });

            env.UndeployAll();
        }

        private static void RunDocSampleNOrMore_and_BetweenNandM(
            RegressionEnvironment env,
            string pattern,
            AtomicLong milestone)
        {
            var fields = "a0_id,a1_id,a2_id,b_id".SplitCsv();
            var epl = "@name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].id as a0_id, A[1].id as a1_id, A[2].id as a2_id, B.id as b_id\n" +
                      "  pattern (" +
                      pattern +
                      ")\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100,\n" +
                      "\tB as B.temp >= 102)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] { "E1", 1, 99d }, "TemperatureSensorEvent");

            env.MilestoneInc(milestone);

            env.SendEventObjectArray(new object[] { "E2", 1, 100d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E3", 1, 100d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E4", 1, 101d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E5", 1, 102d }, "TemperatureSensorEvent");
            env.AssertPropsNew("s0", fields, new object[] { "E2", "E3", "E4", "E5" });

            env.UndeployAll();
        }

        private static void RunDocSampleExactlyN(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "a0_id,a1_id".SplitCsv();
            var epl = "@name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].id as a0_id, A[1].id as a1_id\n" +
                      "  pattern (A{2})\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] { "E1", 1, 99d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E2", 1, 100d }, "TemperatureSensorEvent");

            env.SendEventObjectArray(new object[] { "E3", 1, 100d }, "TemperatureSensorEvent");
            env.AssertPropsNew("s0", fields, new object[] { "E2", "E3" });

            env.MilestoneInc(milestone);

            env.SendEventObjectArray(new object[] { "E4", 1, 101d }, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] { "E5", 1, 102d }, "TemperatureSensorEvent");
            env.AssertPropsNew("s0", fields, new object[] { "E4", "E5" });

            env.UndeployAll();
        }

        private class RowRecogRepetitionInvalid : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }

            public void Run(RegressionEnvironment env)
            {
                var template = "select * from SupportBean " +
                               "match_recognize (" +
                               "  measures A as a" +
                               "  pattern (REPLACE) " +
                               ")";
                env.CompileDeploy("create variable int myvariable = 0");

                env.TryInvalidCompile(
                    template.RegexReplaceAll("REPLACE", "A{}"),
                    "Invalid match-recognize quantifier '{}', expecting an expression");
                env.TryInvalidCompile(
                    template.RegexReplaceAll("REPLACE", "A{null}"),
                    "Pattern quantifier 'null' must return an integer-type value");
                env.TryInvalidCompile(
                    template.RegexReplaceAll("REPLACE", "A{myvariable}"),
                    "Pattern quantifier 'myvariable' must return a constant value");
                env.TryInvalidCompile(
                    template.RegexReplaceAll("REPLACE", "A{prev(A)}"),
                    "Invalid match-recognize pattern expression 'prev(A)': Aggregation, sub-select, previous or prior functions are not supported in this context");

                var expected = "Invalid pattern quantifier value -1, expecting a minimum of 1";
                env.TryInvalidCompile(template.RegexReplaceAll("REPLACE", "A{-1}"), expected);
                env.TryInvalidCompile(template.RegexReplaceAll("REPLACE", "A{,-1}"), expected);
                env.TryInvalidCompile(template.RegexReplaceAll("REPLACE", "A{-1,10}"), expected);
                env.TryInvalidCompile(template.RegexReplaceAll("REPLACE", "A{-1,}"), expected);
                env.TryInvalidCompile(
                    template.RegexReplaceAll("REPLACE", "A{5,3}"),
                    "Invalid pattern quantifier value 5, expecting a minimum of 1 and maximum of 3");

                env.UndeployAll();
            }
        }

        private class RowRecogRepetitionPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select * from SupportBean " +
                           "match_recognize (" +
                           "  measures A as a" +
                           "  pattern (A{3}) " +
                           "  define " +
                           "    A as A.intPrimitive > prev(A.intPrimitive)" +
                           ")";

                env.CompileDeploy(text).AddListener("s0");

                SendEvent("A1", 1, env);
                SendEvent("A2", 4, env);
                SendEvent("A3", 2, env);

                env.Milestone(0);

                SendEvent("A4", 6, env);
                SendEvent("A5", 5, env);
                var b6 = SendEvent("A6", 6, env);
                var b7 = SendEvent("A7", 7, env);
                var b8 = SendEvent("A9", 8, env);
                env.AssertPropsNew("s0", "a".SplitCsv(), new object[] { new object[] { b6, b7, b8 } });

                env.UndeployAll();
            }
        }

        private class RowRecogRepetitionRepeats : RegressionExecution
        {
            private readonly bool soda;

            public RowRecogRepetitionRepeats(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                // Atom Assertions
                //
                //

                // single-bound assertions
                RunAssertionRepeatSingleBound(env, soda);

                // defined-range assertions
                RunAssertionsRepeatRange(env, soda);

                // lower-bounds assertions
                RunAssertionsUpTo(env, soda);

                // upper-bounds assertions
                RunAssertionsAtLeast(env, soda);

                // Nested Assertions
                //
                //

                // single-bound nested assertions
                RunAssertionNestedRepeatSingle(env, soda);

                // defined-range nested assertions
                RunAssertionNestedRepeatRange(env, soda);

                // lower-bounds nested assertions
                RunAssertionsNestedUpTo(env, soda);

                // upper-bounds nested assertions
                RunAssertionsNestedAtLeast(env, soda);
            }

            public string Name()
            {
                return "RowRecogRepetitionRepeats{" +
                       "soda=" +
                       soda +
                       '}';
            }
        }

        private class RowRecogRepetitionEquivalent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                //
                // Single-bounds Repeat.
                //
                RunEquivalent(env, "A{1}", "A");
                RunEquivalent(env, "A{2}", "A A");
                RunEquivalent(env, "A{3}", "A A A");
                RunEquivalent(env, "A{1} B{2}", "A B B");
                RunEquivalent(env, "A{1} B{2} C{3}", "A B B C C C");
                RunEquivalent(env, "(A{2})", "(A A)");
                RunEquivalent(env, "A?{2}", "A? A?");
                RunEquivalent(env, "A*{2}", "A* A*");
                RunEquivalent(env, "A+{2}", "A+ A+");
                RunEquivalent(env, "A??{2}", "A?? A??");
                RunEquivalent(env, "A*?{2}", "A*? A*?");
                RunEquivalent(env, "A+?{2}", "A+? A+?");
                RunEquivalent(env, "(A B){1}", "(A B)");
                RunEquivalent(env, "(A B){2}", "(A B) (A B)");
                RunEquivalent(env, "(A B)?{2}", "(A B)? (A B)?");
                RunEquivalent(env, "(A B)*{2}", "(A B)* (A B)*");
                RunEquivalent(env, "(A B)+{2}", "(A B)+ (A B)+");

                RunEquivalent(env, "A B{2} C", "A B B C");
                RunEquivalent(env, "A (B{2}) C", "A (B B) C");
                RunEquivalent(env, "(A{2}) C", "(A A) C");
                RunEquivalent(env, "A (B{2}|C{2})", "A (B B|C C)");
                RunEquivalent(env, "A{2} B{2} C{2}", "A A B B C C");
                RunEquivalent(env, "A{2} B C{2}", "A A B C C");
                RunEquivalent(env, "A B{2} C{2}", "A B B C C");

                // range bounds
                RunEquivalent(env, "A{1, 3}", "A A? A?");
                RunEquivalent(env, "A{2, 4}", "A A A? A?");
                RunEquivalent(env, "A?{1, 3}", "A? A? A?");
                RunEquivalent(env, "A*{1, 3}", "A* A* A*");
                RunEquivalent(env, "A+{1, 3}", "A+ A* A*");
                RunEquivalent(env, "A??{1, 3}", "A?? A?? A??");
                RunEquivalent(env, "A*?{1, 3}", "A*? A*? A*?");
                RunEquivalent(env, "A+?{1, 3}", "A+? A*? A*?");
                RunEquivalent(env, "(A B)?{1, 3}", "(A B)? (A B)? (A B)?");
                RunEquivalent(env, "(A B)*{1, 3}", "(A B)* (A B)* (A B)*");
                RunEquivalent(env, "(A B)+{1, 3}", "(A B)+ (A B)* (A B)*");

                // lower-only bounds
                RunEquivalent(env, "A{2,}", "A A A*");
                RunEquivalent(env, "A?{2,}", "A? A? A*");
                RunEquivalent(env, "A*{2,}", "A* A* A*");
                RunEquivalent(env, "A+{2,}", "A+ A+ A*");
                RunEquivalent(env, "A??{2,}", "A?? A?? A*?");
                RunEquivalent(env, "A*?{2,}", "A*? A*? A*?");
                RunEquivalent(env, "A+?{2,}", "A+? A+? A*?");
                RunEquivalent(env, "(A B)?{2,}", "(A B)? (A B)? (A B)*");
                RunEquivalent(env, "(A B)*{2,}", "(A B)* (A B)* (A B)*");
                RunEquivalent(env, "(A B)+{2,}", "(A B)+ (A B)+ (A B)*");

                // upper-only bounds
                RunEquivalent(env, "A{,2}", "A? A?");
                RunEquivalent(env, "A?{,2}", "A? A?");
                RunEquivalent(env, "A*{,2}", "A* A*");
                RunEquivalent(env, "A+{,2}", "A* A*");
                RunEquivalent(env, "A??{,2}", "A?? A??");
                RunEquivalent(env, "A*?{,2}", "A*? A*?");
                RunEquivalent(env, "A+?{,2}", "A*? A*?");
                RunEquivalent(env, "(A B){,2}", "(A B)? (A B)?");
                RunEquivalent(env, "(A B)?{,2}", "(A B)? (A B)?");
                RunEquivalent(env, "(A B)*{,2}", "(A B)* (A B)*");
                RunEquivalent(env, "(A B)+{,2}", "(A B)* (A B)*");

                //
                // Nested Repeat.
                //
                RunEquivalent(env, "(A B){2}", "(A B) (A B)");
                RunEquivalent(env, "(A){2}", "A A");
                RunEquivalent(env, "(A B C){3}", "(A B C) (A B C) (A B C)");
                RunEquivalent(env, "(A B){2} (C D){2}", "(A B) (A B) (C D) (C D)");
                RunEquivalent(env, "((A B){2} C){2}", "((A B) (A B) C) ((A B) (A B) C)");
                RunEquivalent(env, "((A|B){2} (C|D){2}){2}", "((A|B) (A|B) (C|D) (C|D)) ((A|B) (A|B) (C|D) (C|D))");
            }
        }

        private static void RunAssertionNestedRepeatSingle(
            RegressionEnvironment env,
            bool soda)
        {
            RunTwiceAB(env, soda, "(A B) (A B)");
            RunTwiceAB(env, soda, "(A B){2}");

            RunAThenTwiceBC(env, soda, "A (B C) (B C)");
            RunAThenTwiceBC(env, soda, "A (B C){2}");
        }

        private static void RunAssertionNestedRepeatRange(
            RegressionEnvironment env,
            bool soda)
        {
            RunOnceOrTwiceABThenC(env, soda, "(A B) (A B)? C");
            RunOnceOrTwiceABThenC(env, soda, "(A B){1,2} C");
        }

        private static void RunAssertionsAtLeast(
            RegressionEnvironment env,
            bool soda)
        {
            RunAtLeast2AThenB(env, soda, "A A A* B");
            RunAtLeast2AThenB(env, soda, "A{2,} B");
            RunAtLeast2AThenB(env, soda, "A{2,4} B");
        }

        private static void RunAssertionsUpTo(
            RegressionEnvironment env,
            bool soda)
        {
            RunUpTo2AThenB(env, soda, "A? A? B");
            RunUpTo2AThenB(env, soda, "A{,2} B");
        }

        private static void RunAssertionsRepeatRange(
            RegressionEnvironment env,
            bool soda)
        {
            Run2To3AThenB(env, soda, "A A A? B");
            Run2To3AThenB(env, soda, "A{2,3} B");
        }

        private static void RunAssertionsNestedUpTo(
            RegressionEnvironment env,
            bool soda)
        {
            RunUpTo2ABThenC(env, soda, "(A B)? (A B)? C");
            RunUpTo2ABThenC(env, soda, "(A B){,2} C");
        }

        private static void RunAssertionsNestedAtLeast(
            RegressionEnvironment env,
            bool soda)
        {
            RunAtLeast2ABThenC(env, soda, "(A B) (A B) (A B)* C");
            RunAtLeast2ABThenC(env, soda, "(A B){2,} C");
        }

        private static void RunAssertionRepeatSingleBound(
            RegressionEnvironment env,
            bool soda)
        {
            RunExactly2A(env, soda, "A A");
            RunExactly2A(env, soda, "A{2}");
            RunExactly2A(env, soda, "(A{2})");

            // concatenation
            RunAThen2BThenC(env, soda, "A B B C");
            RunAThen2BThenC(env, soda, "A B{2} C");

            // nested
            RunAThen2BThenC(env, false, "A (B B) C");
            RunAThen2BThenC(env, false, "A (B{2}) C");

            // alteration
            RunAThen2BOr2C(env, soda, "A (B B|C C)");
            RunAThen2BOr2C(env, soda, "A (B{2}|C{2})");

            // multiple
            Run2AThen2B(env, soda, "A A B B");
            Run2AThen2B(env, soda, "A{2} B{2}");
        }

        private static void RunAtLeast2ABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { true, true, false },
                new string[] {
                    "A1,B1,A2,B2,C1",
                    "A1,B1,A2,B2,A3,B3,C1"
                },
                new string[] { "A1,B1,C1", "A1,B1,A2,C1", "B1,A1,B2,C1" });
        }

        private static void RunOnceOrTwiceABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { true, true, false },
                new string[] {
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1"
                },
                new string[] { "C1", "A1,A2,C2", "B1,A1,C1" });
        }

        private static void RunAtLeast2AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new bool[] { true, false },
                new string[] {
                    "A1,A2,B1",
                    "A1,A2,A3,B1"
                },
                new string[] { "A1,B1", "B1" });
        }

        private static void RunUpTo2AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new bool[] { true, false },
                new string[] {
                    "B1",
                    "A1,B1",
                    "A1,A2,B1"
                },
                new string[] { "A1" });
        }

        private static void Run2AThen2B(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new bool[] { true, true },
                new string[] {
                    "A1,A2,B1,B2",
                },
                new string[] { "A1,A2,B1", "B1,B2,A1,A2", "A1,B1,A2,B2" });
        }

        private static void RunUpTo2ABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { true, true, false },
                new string[] {
                    "C1",
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1",
                },
                new string[] { "A1,B1,A2,B2", "A1,A2" });
        }

        private static void Run2To3AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new bool[] { true, false },
                new string[] {
                    "A1,A2,A3,B1",
                    "A1,A2,B1",
                },
                new string[] { "A1,B1", "A1,A2", "B1" });
        }

        private static void RunAThen2BOr2C(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { false, true, true },
                new string[] {
                    "A1,C1,C2",
                    "A2,B1,B2",
                },
                new string[] { "B1,B2", "C1,C2", "A1,B1,C1", "A1,C1,B1" });
        }

        private static void RunTwiceAB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new bool[] { true, true },
                new string[] {
                    "A1,B1,A2,B2",
                },
                new string[] { "A1,A2,B1", "A1,A2,B1,B2", "A1,B1,B2,A2" });
        }

        private static void RunAThenTwiceBC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { false, true, true },
                new string[] {
                    "A1,B1,C1,B2,C2",
                },
                new string[] { "A1,B1,C1,B2", "A1,B1,C1,C2", "A1,B1,B2,C1,C2" });
        }

        private static void RunAThen2BThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new bool[] { false, true, false },
                new string[] {
                    "A1,B1,B2,C1",
                },
                new string[] { "B1,B2,C1", "A1,B1,C1", "A1,B1,B2" });
        }

        private static void RunExactly2A(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a",
                new bool[] { true },
                new string[] {
                    "A1,A2",
                    "A3,A4",
                },
                new string[] { "A5" });
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            bool soda,
            string pattern,
            string propertyNames,
            bool[] arrayProp,
            string[] sequencesWithMatch,
            string[] sequencesNoMatch)
        {
            var props = propertyNames.SplitCsv();
            var measures = MakeMeasures(props);
            var defines = MakeDefines(props);

            var text = "@name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       " partition by intPrimitive" +
                       " measures " +
                       measures +
                       " pattern (" +
                       pattern +
                       ")" +
                       " define " +
                       defines +
                       ")";
            env.CompileDeploy(soda, text).AddListener("s0");

            var sequenceNum = 0;
            foreach (var aSequencesWithMatch in sequencesWithMatch) {
                RunAssertionSequence(env, true, props, arrayProp, sequenceNum, aSequencesWithMatch);
                sequenceNum++;
            }

            foreach (var aSequencesNoMatch in sequencesNoMatch) {
                RunAssertionSequence(env, false, props, arrayProp, sequenceNum, aSequencesNoMatch);
                sequenceNum++;
            }

            env.UndeployAll();
        }

        private static void RunAssertionSequence(
            RegressionEnvironment env,
            bool match,
            string[] propertyNames,
            bool[] arrayProp,
            int sequenceNum,
            string sequence)
        {
            // send events
            var events = sequence.SplitCsv();
            IDictionary<string, IList<SupportBean>> sent = new Dictionary<string, IList<SupportBean>>();
            foreach (var anEvent in events) {
                var type = new string(new char[] { anEvent[0] });
                var bean = SendEvent(anEvent, sequenceNum, env);
                var propName = type.ToLowerInvariant();
                if (!sent.ContainsKey(propName)) {
                    sent.Put(propName, new List<SupportBean>());
                }

                sent.Get(propName).Add(bean);
            }

            // prepare expected
            var expected = new object[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++) {
                var sentForType = sent.Get(propertyNames[i]);
                if (arrayProp[i]) {
                    expected[i] = sentForType?.ToArray();
                }
                else {
                    if (match) {
                        Assert.IsTrue(sentForType.Count == 1);
                        expected[i] = sentForType[0];
                    }
                }
            }

            if (match) {
                env.AssertPropsNew("s0", propertyNames, expected);
            }
            else {
                env.AssertListenerNotInvoked("s0");
            }
        }

        private static string MakeDefines(string[] props)
        {
            var delimiter = "";
            var buf = new StringWriter();
            foreach (var prop in props) {
                buf.Write(delimiter);
                delimiter = ", ";
                buf.Write(prop.ToUpperInvariant());
                buf.Write(" as ");
                buf.Write(prop.ToUpperInvariant());
                buf.Write(".theString like \"");
                buf.Write(prop.ToUpperInvariant());
                buf.Write("%\"");
            }

            return buf.ToString();
        }

        private static string MakeMeasures(string[] props)
        {
            var delimiter = "";
            var buf = new StringWriter();
            foreach (var prop in props) {
                buf.Write(delimiter);
                delimiter = ", ";
                buf.Write(prop.ToUpperInvariant());
                buf.Write(" as ");
                buf.Write(prop);
            }

            return buf.ToString();
        }

        private static SupportBean SendEvent(
            string theString,
            int intPrimitive,
            RegressionEnvironment env)
        {
            var sb = new SupportBean(theString, intPrimitive);
            env.SendEventBean(sb);
            return sb;
        }

        internal static void RunEquivalent(
            RegressionEnvironment env,
            string before,
            string after)
        {
            var hook = "@Hook(type=" +
                       typeof(HookType).FullName +
                       ".INTERNAL_COMPILE,hook='" +
                       SupportStatementCompileHook.ResetGetClassName() +
                       "')";
            var epl = hook +
                      "@name('s0') select * from SupportBean#keepall " +
                      "match_recognize (" +
                      " measures A as a" +
                      " pattern (" +
                      before +
                      ")" +
                      " define" +
                      " A as A.theString like \"A%\"" +
                      ")";

            var model = env.EplToModel(epl);
            env.CompileDeploy(model);
            env.UndeployAll();

            env.AssertThat(
                () => {
                    var spec = SupportStatementCompileHook.GetSpecs()[0];
                    RowRecogExprNode expanded = null;
                    try {
                        expanded = RowRecogPatternExpandUtil.Expand(spec.Raw.MatchRecognizeSpec.Pattern, null);
                    }
                    catch (ExprValidationException e) {
                        Assert.Fail(e.Message);
                    }

                    var writer = new StringWriter();
                    expanded.ToEPL(writer, RowRecogExprNodePrecedenceEnum.MINIMUM);
                    Assert.AreEqual(after, writer.ToString());
                });
        }
    }
} // end of namespace