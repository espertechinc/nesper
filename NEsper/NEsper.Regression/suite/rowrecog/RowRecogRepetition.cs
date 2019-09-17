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
using System.Text;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogRepetition : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionRepeat(env, false);
            RunAssertionRepeat(env, true);
            RunAssertionPrev(env);
            RunAssertionInvalid(env);
            RunAssertionDocSamples(env);
            RunAssertionEquivalent(env);
        }

        private void RunAssertionDocSamples(RegressionEnvironment env)
        {
            RunDocSampleExactlyN(env);
            RunDocSampleNOrMore_and_BetweenNandM(env, "A{2,} B");
            RunDocSampleNOrMore_and_BetweenNandM(env, "A{2,3} B");
            RunDocSampleUpToN(env);
        }

        private void RunDocSampleUpToN(RegressionEnvironment env)
        {
            var fields = new [] { "a0_Id","a1_Id","b_Id" };
            var epl = "@Name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].Id as a0_Id, A[1].Id as a1_Id, B.Id as b_Id\n" +
                      "  pattern (A{,2} B)\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100,\n" +
                      "\tB as B.temp >= 102)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] {"E1", 1, 99d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E2", 1, 100d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E3", 1, 100d}, "TemperatureSensorEvent");

            env.Milestone(0);

            env.SendEventObjectArray(new object[] {"E4", 1, 101d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E5", 1, 102d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", "E4", "E5"});

            env.UndeployAll();
        }

        private void RunDocSampleNOrMore_and_BetweenNandM(
            RegressionEnvironment env,
            string pattern)
        {
            var fields = new [] { "a0_Id","a1_Id","a2_Id","b_Id" };
            var epl = "@Name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].Id as a0_Id, A[1].Id as a1_Id, A[2].Id as a2_Id, B.Id as b_Id\n" +
                      "  pattern (" +
                      pattern +
                      ")\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100,\n" +
                      "\tB as B.temp >= 102)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] {"E1", 1, 99d}, "TemperatureSensorEvent");

            env.Milestone(0);

            env.SendEventObjectArray(new object[] {"E2", 1, 100d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E3", 1, 100d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E4", 1, 101d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E5", 1, 102d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "E3", "E4", "E5"});

            env.UndeployAll();
        }

        private void RunDocSampleExactlyN(RegressionEnvironment env)
        {
            var fields = new [] { "a0_Id","a1_Id" };
            var epl = "@Name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by device\n" +
                      "  measures A[0].Id as a0_Id, A[1].Id as a1_Id\n" +
                      "  pattern (A{2})\n" +
                      "  define \n" +
                      "\tA as A.temp >= 100)";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] {"E1", 1, 99d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E2", 1, 100d}, "TemperatureSensorEvent");

            env.SendEventObjectArray(new object[] {"E3", 1, 100d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", "E3"});

            env.Milestone(0);

            env.SendEventObjectArray(new object[] {"E4", 1, 101d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E5", 1, 102d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E4", "E5"});

            env.UndeployAll();
        }

        private void RunAssertionInvalid(RegressionEnvironment env)
        {
            var template = "select * from SupportBean " +
                           "match_recognize (" +
                           "  measures A as a" +
                           "  pattern (REPLACE) " +
                           ")";
            env.CompileDeploy("create variable int myvariable = 0");

            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                template.Replace("REPLACE", "A{}"),
                "Invalid match-recognize quantifier '{}', expecting an expression");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                template.Replace("REPLACE", "A{null}"),
                "Pattern quantifier 'null' must return an integer-type value");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                template.Replace("REPLACE", "A{myvariable}"),
                "Pattern quantifier 'myvariable' must return a constant value");
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                template.Replace("REPLACE", "A{prev(A)}"),
                "Invalid match-recognize pattern expression 'prev(A)': Aggregation, sub-select, previous or prior functions are not supported in this context");

            var expected = "Invalid pattern quantifier value -1, expecting a minimum of 1";
            SupportMessageAssertUtil.TryInvalidCompile(env, template.Replace("REPLACE", "A{-1}"), expected);
            SupportMessageAssertUtil.TryInvalidCompile(env, template.Replace("REPLACE", "A{,-1}"), expected);
            SupportMessageAssertUtil.TryInvalidCompile(env, template.Replace("REPLACE", "A{-1,10}"), expected);
            SupportMessageAssertUtil.TryInvalidCompile(env, template.Replace("REPLACE", "A{-1,}"), expected);
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                template.Replace("REPLACE", "A{5,3}"),
                "Invalid pattern quantifier value 5, expecting a minimum of 1 and maximum of 3");

            env.UndeployAll();
        }

        private void RunAssertionPrev(RegressionEnvironment env)
        {
            var text = "@Name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       "  measures A as a" +
                       "  pattern (A{3}) " +
                       "  define " +
                       "    A as A.IntPrimitive > prev(A.IntPrimitive)" +
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
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                new [] { "a" },
                new object[] {new object[] {b6, b7, b8}});

            env.UndeployAll();
        }

        private void RunAssertionRepeat(
            RegressionEnvironment env,
            bool soda)
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

        private static void RunAssertionEquivalent(RegressionEnvironment env)
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

        private void RunAssertionNestedRepeatSingle(
            RegressionEnvironment env,
            bool soda)
        {
            RunTwiceAB(env, soda, "(A B) (A B)");
            RunTwiceAB(env, soda, "(A B){2}");

            RunAThenTwiceBC(env, soda, "A (B C) (B C)");
            RunAThenTwiceBC(env, soda, "A (B C){2}");
        }

        private void RunAssertionNestedRepeatRange(
            RegressionEnvironment env,
            bool soda)
        {
            RunOnceOrTwiceABThenC(env, soda, "(A B) (A B)? C");
            RunOnceOrTwiceABThenC(env, soda, "(A B){1,2} C");
        }

        private void RunAssertionsAtLeast(
            RegressionEnvironment env,
            bool soda)
        {
            RunAtLeast2AThenB(env, soda, "A A A* B");
            RunAtLeast2AThenB(env, soda, "A{2,} B");
            RunAtLeast2AThenB(env, soda, "A{2,4} B");
        }

        private void RunAssertionsUpTo(
            RegressionEnvironment env,
            bool soda)
        {
            RunUpTo2AThenB(env, soda, "A? A? B");
            RunUpTo2AThenB(env, soda, "A{,2} B");
        }

        private void RunAssertionsRepeatRange(
            RegressionEnvironment env,
            bool soda)
        {
            Run2To3AThenB(env, soda, "A A A? B");
            Run2To3AThenB(env, soda, "A{2,3} B");
        }

        private void RunAssertionsNestedUpTo(
            RegressionEnvironment env,
            bool soda)
        {
            RunUpTo2ABThenC(env, soda, "(A B)? (A B)? C");
            RunUpTo2ABThenC(env, soda, "(A B){,2} C");
        }

        private void RunAssertionsNestedAtLeast(
            RegressionEnvironment env,
            bool soda)
        {
            RunAtLeast2ABThenC(env, soda, "(A B) (A B) (A B)* C");
            RunAtLeast2ABThenC(env, soda, "(A B){2,} C");
        }

        private void RunAssertionRepeatSingleBound(
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

        private void RunAtLeast2ABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {true, true, false},
                new[] {
                    "A1,B1,A2,B2,C1",
                    "A1,B1,A2,B2,A3,B3,C1"
                },
                new[] {"A1,B1,C1", "A1,B1,A2,C1", "B1,A1,B2,C1"});
        }

        private void RunOnceOrTwiceABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {true, true, false},
                new[] {
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1"
                },
                new[] {"C1", "A1,A2,C2", "B1,A1,C1"});
        }

        private void RunAtLeast2AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new[] {true, false},
                new[] {
                    "A1,A2,B1",
                    "A1,A2,A3,B1"
                },
                new[] {"A1,B1", "B1"});
        }

        private void RunUpTo2AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new[] {true, false},
                new[] {
                    "B1",
                    "A1,B1",
                    "A1,A2,B1"
                },
                new[] {"A1"});
        }

        private void Run2AThen2B(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new[] {true, true},
                new[] {
                    "A1,A2,B1,B2"
                },
                new[] {"A1,A2,B1", "B1,B2,A1,A2", "A1,B1,A2,B2"});
        }

        private void RunUpTo2ABThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {true, true, false},
                new[] {
                    "C1",
                    "A1,B1,C1",
                    "A1,B1,A2,B2,C1"
                },
                new[] {"A1,B1,A2,B2", "A1,A2"});
        }

        private void Run2To3AThenB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new[] {true, false},
                new[] {
                    "A1,A2,A3,B1",
                    "A1,A2,B1"
                },
                new[] {"A1,B1", "A1,A2", "B1"});
        }

        private void RunAThen2BOr2C(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {false, true, true},
                new[] {
                    "A1,C1,C2",
                    "A2,B1,B2"
                },
                new[] {"B1,B2", "C1,C2", "A1,B1,C1", "A1,C1,B1"});
        }

        private void RunTwiceAB(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b",
                new[] {true, true},
                new[] {
                    "A1,B1,A2,B2"
                },
                new[] {"A1,A2,B1", "A1,A2,B1,B2", "A1,B1,B2,A2"});
        }

        private void RunAThenTwiceBC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {false, true, true},
                new[] {
                    "A1,B1,C1,B2,C2"
                },
                new[] {"A1,B1,C1,B2", "A1,B1,C1,C2", "A1,B1,B2,C1,C2"});
        }

        private void RunAThen2BThenC(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a,b,c",
                new[] {false, true, false},
                new[] {
                    "A1,B1,B2,C1"
                },
                new[] {"B1,B2,C1", "A1,B1,C1", "A1,B1,B2"});
        }

        private void RunExactly2A(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            RunAssertion(
                env,
                soda,
                pattern,
                "a",
                new[] {true},
                new[] {
                    "A1,A2",
                    "A3,A4"
                },
                new[] {"A5"});
        }

        private void RunAssertion(
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

            var text = "@Name('s0') select * from SupportBean " +
                       "match_recognize (" +
                       " partition by IntPrimitive" +
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

        private void RunAssertionSequence(
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
                var type = new string(new[] {anEvent[0]});
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
                    expected[i] = sentForType == null ? null : sentForType.ToArray();
                }
                else {
                    if (match) {
                        Assert.IsTrue(sentForType.Count == 1);
                        expected[i] = sentForType[0];
                    }
                }
            }

            if (match) {
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                EPAssertionUtil.AssertProps(@event, propertyNames, expected);
            }
            else {
                Assert.IsFalse(env.Listener("s0").IsInvoked, "Failed at " + sequence);
            }
        }

        private string MakeDefines(string[] props)
        {
            var delimiter = "";
            var buf = new StringBuilder();
            foreach (var prop in props) {
                buf.Append(delimiter);
                delimiter = ", ";
                buf.Append(prop.ToUpperInvariant());
                buf.Append(" as ");
                buf.Append(prop.ToUpperInvariant());
                buf.Append(".TheString like \"");
                buf.Append(prop.ToUpperInvariant());
                buf.Append("%\"");
            }

            return buf.ToString();
        }

        private string MakeMeasures(string[] props)
        {
            var delimiter = "";
            var buf = new StringBuilder();
            foreach (var prop in props) {
                buf.Append(delimiter);
                delimiter = ", ";
                buf.Append(prop.ToUpperInvariant());
                buf.Append(" as ");
                buf.Append(prop);
            }

            return buf.ToString();
        }

        private SupportBean SendEvent(
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
            var hook = "@Hook(HookType=" +
                       typeof(HookType).FullName +
                       ".INTERNAL_COMPILE,Hook='" +
                       SupportStatementCompileHook.ResetGetClassName() +
                       "')";
            var epl = hook +
                      "@Name('s0') select * from SupportBean#keepall " +
                      "match_recognize (" +
                      " measures A as a" +
                      " pattern (" +
                      before +
                      ")" +
                      " define" +
                      " A as A.TheString like \"A%\"" +
                      ")";

            var model = env.EplToModel(epl);
            env.CompileDeploy(model);
            env.UndeployAll();

            var spec = SupportStatementCompileHook.GetSpecs()[0];
            RowRecogExprNode expanded = null;
            try {
                expanded = RowRecogPatternExpandUtil.Expand(
                    env.Container,
                    spec.Raw.MatchRecognizeSpec.Pattern);
            }
            catch (ExprValidationException e) {
                Assert.Fail(e.Message);
            }

            var writer = new StringWriter();
            expanded.ToEPL(writer, RowRecogExprNodePrecedenceEnum.MINIMUM);
            Assert.AreEqual(after, writer.ToString());
        }
    }
} // end of namespace