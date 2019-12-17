///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.patternassert;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternExpressionText : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TryAssertion(env, "every a=SupportBean -> b=SupportBean@consume", null);
            TryAssertion(env, "every a=SupportBean -> b=SupportBean@consume", null);
            TryAssertion(env, "every a=SupportBean -> b=SupportBean@consume(2)", null);
            TryAssertion(env, "a=SupportBean_A -> b=SupportBean_B", null);
            TryAssertion(env, "b=SupportBean_B and every d=SupportBean_D", null);
            TryAssertion(env, "every b=SupportBean_B and d=SupportBean_B", null);
            TryAssertion(env, "b=SupportBean_B and d=SupportBean_D", null);
            TryAssertion(env, "every (b=SupportBean_B and d=SupportBean_D)", null);
            TryAssertion(env, "every (b=SupportBean_B and every d=SupportBean_D)", null);
            TryAssertion(env, "every b=SupportBean_B and every d=SupportBean_D", null);
            TryAssertion(env, "every (every b=SupportBean_B and d=SupportBean_D)", null);
            TryAssertion(env, "every a=SupportBean_A and d=SupportBean_D and b=SupportBean_B", null);
            TryAssertion(env, "every (every b=SupportBean_B and every d=SupportBean_D)", null);
            TryAssertion(env, "a=SupportBean_A and d=SupportBean_D and b=SupportBean_B", null);
            TryAssertion(env, "every a=SupportBean_A and every d=SupportBean_D and b=SupportBean_B", null);
            TryAssertion(env, "b=SupportBean_B and b=SupportBean_B", null);
            TryAssertion(env, "every a=SupportBean_A and every d=SupportBean_D and every b=SupportBean_B", null);
            TryAssertion(env, "every (a=SupportBean_A and every d=SupportBean_D and b=SupportBean_B)", null);
            TryAssertion(env, "every (b=SupportBean_B and b=SupportBean_B)", null);
            TryAssertion(env, "every b=SupportBean_B", null);
            TryAssertion(env, "b=SupportBean_B", null);
            TryAssertion(env, "every (every (every b=SupportBean_B))", "every every every b=SupportBean_B");
            TryAssertion(env, "every (every b=SupportBean_B())", "every every b=SupportBean_B");
            TryAssertion(env, "b=SupportBean_B -> d=SupportBean_D or not d=SupportBean_D", null);
            TryAssertion(
                env,
                "b=SupportBean_B -> (d=SupportBean_D or not d=SupportBean_D)",
                "b=SupportBean_B -> d=SupportBean_D or not d=SupportBean_D");
            TryAssertion(env, "b=SupportBean_B -[1000]> d=SupportBean_D or not d=SupportBean_D", null);
            TryAssertion(env, "b=SupportBean_B -> every d=SupportBean_D", null);
            TryAssertion(env, "b=SupportBean_B -> d=SupportBean_D", null);
            TryAssertion(env, "b=SupportBean_B -> not d=SupportBean_D", null);
            TryAssertion(env, "b=SupportBean_B -[1000]> not d=SupportBean_D", null);
            TryAssertion(env, "every b=SupportBean_B -> every d=SupportBean_D", null);
            TryAssertion(env, "every b=SupportBean_B -> d=SupportBean_D", null);
            TryAssertion(env, "every b=SupportBean_B -[10]> d=SupportBean_D", null);
            TryAssertion(env, "every (b=SupportBean_B -> every d=SupportBean_D)", null);
            TryAssertion(env, "every (a_1=SupportBean_A -> b=SupportBean_B -> a_2=SupportBean_A)", null);
            TryAssertion(env, "c=SupportBean_C -> d=SupportBean_D -> a=SupportBean_A", null);
            TryAssertion(env, "every (a_1=SupportBean_A -> b=SupportBean_B -> a_2=SupportBean_A)", null);
            TryAssertion(env, "every (a_1=SupportBean_A -[10]> b=SupportBean_B -[10]> a_2=SupportBean_A)", null);
            TryAssertion(env, "every (every a=SupportBean_A -> every b=SupportBean_B)", null);
            TryAssertion(env, "every (a=SupportBean_A -> every b=SupportBean_B)", null);
            TryAssertion(
                env,
                "a=SupportBean_A(Id='A2') until SupportBean_D",
                "a=SupportBean_A(Id=\"A2\") until SupportBean_D");
            TryAssertion(env, "b=SupportBean_B until a=SupportBean_A", null);
            TryAssertion(env, "b=SupportBean_B until SupportBean_D", null);
            TryAssertion(env, "(a=SupportBean_A or b=SupportBean_B) until d=SupportBean_D", null);
            TryAssertion(env, "(a=SupportBean_A or b=SupportBean_B) until (g=SupportBean_G or d=SupportBean_D)", null);
            TryAssertion(env, "a=SupportBean_A until SupportBean_G", null);
            TryAssertion(env, "[2] a=SupportBean_A", null);
            TryAssertion(env, "[1:1] a=SupportBean_A", null);
            TryAssertion(env, "[4] (a=SupportBean_A or b=SupportBean_B)", null);
            TryAssertion(env, "[2] b=SupportBean_B until a=SupportBean_A", null);
            TryAssertion(env, "[2:2] b=SupportBean_B until g=SupportBean_G", null);
            TryAssertion(env, "[:4] b=SupportBean_B until g=SupportBean_G", null);
            TryAssertion(env, "[1:] b=SupportBean_B until g=SupportBean_G", null);
            TryAssertion(env, "[1:2] b=SupportBean_B until a=SupportBean_A", null);
            TryAssertion(env, "c=SupportBean_C -> [2] b=SupportBean_B -> d=SupportBean_D", null);
            TryAssertion(
                env,
                "d=SupportBean_D until timer:interval(7 sec)",
                "d=SupportBean_D until timer:interval(7 seconds)");
            TryAssertion(env, "every (d=SupportBean_D until b=SupportBean_B)", null);
            TryAssertion(env, "every d=SupportBean_D until b=SupportBean_B", null);
            TryAssertion(
                env,
                "(every d=SupportBean_D) until b=SupportBean_B",
                "every d=SupportBean_D until b=SupportBean_B");
            TryAssertion(
                env,
                "a=SupportBean_A until (every (timer:interval(6 sec) and not SupportBean_A))",
                "a=SupportBean_A until every (timer:interval(6 seconds) and not SupportBean_A)");
            TryAssertion(env, "[2] (a=SupportBean_A or b=SupportBean_B)", null);
            TryAssertion(env, "every [2] a=SupportBean_A", "every ([2] a=SupportBean_A)");
            TryAssertion(
                env,
                "every [2] a=SupportBean_A until d=SupportBean_D",
                "every ([2] a=SupportBean_A) until d=SupportBean_D"); // every has precedence; ESPER-339
            TryAssertion(env, "[3] (a=SupportBean_A or b=SupportBean_B)", null);
            TryAssertion(env, "[4] (a=SupportBean_A or b=SupportBean_B)", null);
            TryAssertion(
                env,
                "(a=SupportBean_A until b=SupportBean_B) until c=SupportBean_C",
                "a=SupportBean_A until b=SupportBean_B until c=SupportBean_C");
            TryAssertion(env, "b=SupportBean_B and not d=SupportBean_D", null);
            TryAssertion(env, "every b=SupportBean_B and not g=SupportBean_G", null);
            TryAssertion(env, "every b=SupportBean_B and not g=SupportBean_G", null);
            TryAssertion(env, "b=SupportBean_B and not a=SupportBean_A(Id=\"A1\")", null);
            TryAssertion(env, "every (b=SupportBean_B and not b3=SupportBean_B(Id=\"B3\"))", null);
            TryAssertion(env, "every (b=SupportBean_B or not SupportBean_D)", null);
            TryAssertion(env, "every (every b=SupportBean_B and not SupportBean_B)", null);
            TryAssertion(env, "every (b=SupportBean_B and not SupportBean_B)", null);
            TryAssertion(env, "(b=SupportBean_B -> d=SupportBean_D) and SupportBean_G", null);
            TryAssertion(env, "(b=SupportBean_B -> d=SupportBean_D) and (a=SupportBean_A -> e=SupportBean_E)", null);
            TryAssertion(
                env,
                "b=SupportBean_B -> (d=SupportBean_D() or a=SupportBean_A)",
                "b=SupportBean_B -> d=SupportBean_D or a=SupportBean_A");
            TryAssertion(
                env,
                "b=SupportBean_B -> ((d=SupportBean_D -> a=SupportBean_A) or (a=SupportBean_A -> e=SupportBean_E))",
                "b=SupportBean_B -> (d=SupportBean_D -> a=SupportBean_A) or (a=SupportBean_A -> e=SupportBean_E)");
            TryAssertion(env, "(b=SupportBean_B -> d=SupportBean_D) or a=SupportBean_A", null);
            TryAssertion(
                env,
                "(b=SupportBean_B and d=SupportBean_D) or a=SupportBean_A",
                "b=SupportBean_B and d=SupportBean_D or a=SupportBean_A");
            TryAssertion(env, "a=SupportBean_A or a=SupportBean_A", null);
            TryAssertion(env, "a=SupportBean_A or b=SupportBean_B or c=SupportBean_C", null);
            TryAssertion(env, "every b=SupportBean_B or every d=SupportBean_D", null);
            TryAssertion(env, "a=SupportBean_A or b=SupportBean_B", null);
            TryAssertion(env, "a=SupportBean_A or every b=SupportBean_B", null);
            TryAssertion(env, "every a=SupportBean_A or d=SupportBean_D", null);
            TryAssertion(env, "every (every b=SupportBean_B or d=SupportBean_D)", null);
            TryAssertion(env, "every (b=SupportBean_B or every d=SupportBean_D)", null);
            TryAssertion(env, "every (every d=SupportBean_D or every b=SupportBean_B)", null);
            TryAssertion(env, "timer:at(10,8,*,*,*)", null);
            TryAssertion(env, "every timer:at(*/5,*,*,*,*,*)", null);
            TryAssertion(env, "timer:at(10,9,*,*,*,10) or timer:at(30,9,*,*,*,*)", null);
            TryAssertion(env, "b=SupportBean_B(Id=\"B3\") -> timer:at(20,9,*,*,*,*)", null);
            TryAssertion(env, "timer:at(59,8,*,*,*,59) -> d=SupportBean_D", null);
            TryAssertion(env, "timer:at(22,8,*,*,*) -> b=SupportBean_B -> timer:at(55,*,*,*,*)", null);
            TryAssertion(env, "timer:at(40,*,*,*,*,1) and b=SupportBean_B", null);
            TryAssertion(env, "timer:at(40,9,*,*,*,1) or d=SupportBean_D", null);
            TryAssertion(env, "timer:at(22,8,*,*,*) -> b=SupportBean_B -> timer:at(55,8,*,*,*)", null);
            TryAssertion(env, "timer:at(22,8,*,*,*,1) where timer:within(30 minutes)", null);
            TryAssertion(env, "timer:at(*,9,*,*,*) and timer:at(55,*,*,*,*)", null);
            TryAssertion(env, "timer:at(40,8,*,*,*,1) and b=SupportBean_B", null);
            TryAssertion(env, "timer:interval(2 seconds)", null);
            TryAssertion(env, "timer:interval(2.001d)", null);
            TryAssertion(env, "timer:interval(2999 milliseconds)", null);
            TryAssertion(env, "timer:interval(4 seconds) -> b=SupportBean_B", null);
            TryAssertion(env, "b=SupportBean_B -> timer:interval(0)", null);
            TryAssertion(env, "b=SupportBean_B -> timer:interval(6.0d) -> d=SupportBean_D", null);
            TryAssertion(env, "every (b=SupportBean_B -> timer:interval(2.0d) -> d=SupportBean_D)", null);
            TryAssertion(env, "b=SupportBean_B or timer:interval(2.001d)", null);
            TryAssertion(env, "b=SupportBean_B or timer:interval(8.5d)", null);
            TryAssertion(env, "timer:interval(8.5d) or timer:interval(7.5d)", null);
            TryAssertion(env, "timer:interval(999999 milliseconds) or g=SupportBean_G", null);
            TryAssertion(env, "b=SupportBean_B and timer:interval(4000 milliseconds)", null);
            TryAssertion(env, "b=SupportBean_B(Id=\"B1\") where timer:within(2 seconds)", null);
            TryAssertion(env, "(every b=SupportBean_B) where timer:within(2.001d)", null);
            TryAssertion(
                env,
                "every (b=SupportBean_B) where timer:within(6.001d)",
                "every b=SupportBean_B where timer:within(6.001d)");
            TryAssertion(env, "b=SupportBean_B -> d=SupportBean_D where timer:within(4001 milliseconds)", null);
            TryAssertion(env, "b=SupportBean_B -> d=SupportBean_D where timer:within(4 seconds)", null);
            TryAssertion(
                env,
                "every (b=SupportBean_B where timer:within(4.001d) and d=SupportBean_D where timer:within(6.001d))",
                null);
            TryAssertion(env, "every b=SupportBean_B -> d=SupportBean_D where timer:within(4000 seconds)", null);
            TryAssertion(env, "every b=SupportBean_B -> every d=SupportBean_D where timer:within(4000 seconds)", null);
            TryAssertion(env, "b=SupportBean_B -> d=SupportBean_D where timer:within(3999 seconds)", null);
            TryAssertion(env, "every b=SupportBean_B -> (every d=SupportBean_D) where timer:within(2001)", null);
            TryAssertion(env, "every (b=SupportBean_B -> d=SupportBean_D) where timer:within(6001)", null);
            TryAssertion(
                env,
                "b=SupportBean_B where timer:within(2000) or d=SupportBean_D where timer:within(6000)",
                null);
            TryAssertion(
                env,
                "(b=SupportBean_B where timer:within(2000) or d=SupportBean_D where timer:within(6000)) where timer:within(1999)",
                null);
            TryAssertion(
                env,
                "every (b=SupportBean_B where timer:within(2001) and d=SupportBean_D where timer:within(6001))",
                null);
            TryAssertion(
                env,
                "b=SupportBean_B where timer:within(2001) or d=SupportBean_D where timer:within(6001)",
                null);
            TryAssertion(
                env,
                "SupportBean_B where timer:within(2000) or d=SupportBean_D where timer:within(6001)",
                null);
            TryAssertion(
                env,
                "every b=SupportBean_B where timer:within(2001) and every d=SupportBean_D where timer:within(6001)",
                null);
            TryAssertion(
                env,
                "(every b=SupportBean_B) where timer:within(2000) and every d=SupportBean_D where timer:within(6001)",
                null);
            TryAssertion(env, "b=SupportBean_B(Id=\"B1\") where timer:withinmax(2 seconds,100)", null);
            TryAssertion(env, "(every b=SupportBean_B) where timer:withinmax(4.001d,2)", null);
            TryAssertion(env, "every b=SupportBean_B where timer:withinmax(2.001d,4)", null);
            TryAssertion(
                env,
                "every (b=SupportBean_B where timer:withinmax(2001,0))",
                "every b=SupportBean_B where timer:withinmax(2001,0)");
            TryAssertion(env, "(every b=SupportBean_B) where timer:withinmax(4.001d,2)", null);
            TryAssertion(
                env,
                "every b=SupportBean_B -> d=SupportBean_D where timer:withinmax(4000 milliseconds,1)",
                null);
            TryAssertion(env, "every b=SupportBean_B -> every d=SupportBean_D where timer:withinmax(4000,1)", null);
            TryAssertion(env, "every b=SupportBean_B -> (every d=SupportBean_D) where timer:withinmax(1 days,3)", null);
            TryAssertion(env, "a=SupportBean_A -> (every b=SupportBean_B) while (b.Id!=\"B3\")", null);
            TryAssertion(env, "(every b=SupportBean_B) while (b.Id!=\"B1\")", null);
            TryAssertion(env, "every-distinct(a.IntPrimitive,1) a=SupportBean(TheString like \"A%\")", null);
            TryAssertion(env, "every-distinct(a.IntPrimitive,1 seconds) a=SupportBean(TheString like \"A%\")", null);
            TryAssertion(env, "every-distinct(IntPrimitive) a=SupportBean", null);
            TryAssertion(env, "[2] every-distinct(a.IntPrimitive) a=SupportBean", null);
            TryAssertion(env, "every-distinct(a[0].IntPrimitive) ([2] a=SupportBean)", null);
            TryAssertion(env, "every-distinct(a[0].IntPrimitive,a[0].IntPrimitive,1 hours) ([2] a=SupportBean)", null);
            TryAssertion(env, "(every-distinct(a.IntPrimitive) a=SupportBean) where timer:within(10 seconds)", null);
            TryAssertion(env, "every-distinct(a.IntPrimitive) a=SupportBean where timer:within(10)", null);
            TryAssertion(env, "every-distinct(a.IntPrimitive,1 hours) a=SupportBean where timer:within(10)", null);
            TryAssertion(
                env,
                "every-distinct(a.IntPrimitive,b.IntPrimitive) (a=SupportBean(TheString like \"A%\") and b=SupportBean(TheString like \"B%\"))",
                null);
            TryAssertion(env, "every-distinct(a.IntPrimitive) (a=SupportBean and not SupportBean)", null);
            TryAssertion(env, "every-distinct(a.IntPrimitive,1 hours) (a=SupportBean and not SupportBean)", null);
            TryAssertion(
                env,
                "every-distinct(a.IntPrimitive+b.IntPrimitive,1 hours) (a=SupportBean -> b=SupportBean)",
                null);
            TryAssertion(
                env,
                "every-distinct(a.IntPrimitive) a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive)",
                null);
            TryAssertion(
                env,
                "every-distinct(a.IntPrimitive) a=SupportBean -> every-distinct(b.IntPrimitive) b=SupportBean(TheString like \"B%\")",
                null);

            SupportPatternCompileHook.Reset();
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string patternText,
            string expectedIfDifferent)
        {
            var epl = "@Name('A') select * from pattern [" + patternText + "]";
            TryAssertionEPL(env, epl, patternText, expectedIfDifferent);

            epl = "@Audit @Name('A') select * from pattern [" + patternText + "]";
            TryAssertionEPL(env, epl, patternText, expectedIfDifferent);
        }

        private static void TryAssertionEPL(
            RegressionEnvironment env,
            string epl,
            string patternText,
            string expectedIfDifferent)
        {
            var hook = "@Hook(HookType=INTERNAL_PATTERNCOMPILE,Hook='" + typeof(SupportPatternCompileHook).FullName + "')";
            epl = hook + epl;
            env.Compile(epl);

            var root = SupportPatternCompileHook.GetOneAndReset();

            var writer = new StringWriter();
            root.ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
            if (expectedIfDifferent == null) {
                Assert.AreEqual(patternText, writer.ToString());
            }
            else {
                Assert.AreEqual(expectedIfDifferent, writer.ToString());
            }
        }
    }
} // end of namespace