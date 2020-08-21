///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogPermute : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunPermute(env, false);
            RunPermute(env, true);

            RunDocSamples(env);

            RunEquivalent(
                env,
                "mAtCh_Recognize_Permute(A)",
                "(A)");
            RunEquivalent(
                env,
                "match_recognize_permute(A,B)",
                "(A B|B A)");
            RunEquivalent(
                env,
                "match_recognize_permute(A,B,C)",
                "(A B C|A C B|B A C|B C A|C A B|C B A)");
            RunEquivalent(
                env,
                "match_recognize_permute(A,B,C,D)",
                "(A B C D|A B D C|A C B D|A C D B|A D B C|A D C B|B A C D|B A D C|B C A D|B C D A|B D A C|B D C A|C A B D|C A D B|C B A D|C B D A|C D A B|C D B A|D A B C|D A C B|D B A C|D B C A|D C A B|D C B A)");

            RunEquivalent(
                env,
                "match_recognize_permute((A B), C)",
                "((A B) C|C (A B))");
            RunEquivalent(
                env,
                "match_recognize_permute((A|B), (C D), E)",
                "((A|B) (C D) E|(A|B) E (C D)|(C D) (A|B) E|(C D) E (A|B)|E (A|B) (C D)|E (C D) (A|B))");

            RunEquivalent(
                env,
                "A match_recognize_permute(B,C) D",
                "A (B C|C B) D");

            RunEquivalent(
                env,
                "match_recognize_permute(A, match_recognize_permute(B, C))",
                "(A (B C|C B)|(B C|C B) A)");
        }

        private void RunDocSamples(RegressionEnvironment env)
        {
            RunDocSampleUpToN(env);
        }

        private void RunDocSampleUpToN(RegressionEnvironment env)
        {
            var fields = new [] { "a_Id","b_Id" };
            var epl = "@Name('s0') select * from TemperatureSensorEvent\n" +
                      "match_recognize (\n" +
                      "  partition by Device\n" +
                      "  measures A.Id as a_Id, B.Id as b_Id\n" +
                      "  pattern (match_recognize_permute(A, B))\n" +
                      "  define \n" +
                      "\tA as A.Temp < 100, \n" +
                      "\tB as B.Temp >= 100)";

            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventObjectArray(new object[] {"E1", 1, 99d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E2", 1, 100d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", "E2"});

            env.Milestone(0);

            env.SendEventObjectArray(new object[] {"E3", 1, 100d}, "TemperatureSensorEvent");
            env.SendEventObjectArray(new object[] {"E4", 1, 99d}, "TemperatureSensorEvent");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E4", "E3"});

            env.SendEventObjectArray(new object[] {"E5", 1, 98d}, "TemperatureSensorEvent");
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private static void RunPermute(
            RegressionEnvironment env,
            bool soda)
        {
            TryPermute(env, soda, "(A B C)|(A C B)|(B A C)|(B C A)|(C A B)|(C B A)");
            TryPermute(env, soda, "(match_recognize_permute(A,B,C))");
        }

        public static void TryPermute(
            RegressionEnvironment env,
            bool soda,
            string pattern)
        {
            var epl = "@Name('s0') select * from SupportBean " +
                      "match_recognize (" +
                      " partition by IntPrimitive" +
                      " measures A as a, B as b, C as c" +
                      " pattern (" +
                      pattern +
                      ")" +
                      " define" +
                      " A as A.TheString like \"A%\"," +
                      " B as B.TheString like \"B%\"," +
                      " C as C.TheString like \"C%\"" +
                      ")";
            env.CompileDeploy(soda, epl).AddListener("s0");

            var prefixes = new [] { "A","B","C" };
            var fields = new [] { "a","b","c" };
            var count = 0;

            foreach (var indexes in PermutationEnumerator.Create(3)) {
                var expected = new object[3];
                for (var i = 0; i < 3; i++) {
                    expected[indexes[i]] = SendEvent(env, prefixes[indexes[i]] + Convert.ToString(count), count);
                }

                count++;

                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
            }

            env.UndeployAll();
        }

        private static void RunEquivalent(
            RegressionEnvironment env,
            string before,
            string after)
        {
            RowRecogRepetition.RunEquivalent(env, before, after);
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            env.SendEventBean(sb);
            return sb;
        }
    }
} // end of namespace