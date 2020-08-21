///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternConsumingFilter
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new PatternFollowedBy());
            execs.Add(new PatternAnd());
            execs.Add(new PatternFilterAndSceneTwo());
            execs.Add(new PatternOr());
            execs.Add(new PatternInvalid());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string[] fields,
            string pattern,
            object expected)
        {
            env.CompileDeploy("@Name('s0') " + pattern).AddListener("s0");
            env.SendEventBean(new SupportBean("E1", 10));

            if (expected is object[][]) {
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    (object[][]) expected);
            }
            else {
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, (object[]) expected);
            }

            env.UndeployAll();
        }

        internal class PatternFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a","b" };
                var pattern =
                    "@Name('s0') select a.TheString as a, b.TheString as b from pattern[every a=SupportBean -> b=SupportBean@consume]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E2"});

                env.SendEventBean(new SupportBean("E3", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", "E4"});

                env.SendEventBean(new SupportBean("E5", 0));
                env.SendEventBean(new SupportBean("E6", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5", "E6"});

                env.UndeployAll();
            }
        }

        internal class PatternAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a","b" };
                var pattern =
                    "@Name('s0') select a.TheString as a, b.TheString as b from pattern[every (a=SupportBean and b=SupportBean)]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1"});
                env.UndeployAll();

                pattern =
                    "@Name('s0') select a.TheString as a, b.TheString as b from pattern [every (a=SupportBean and b=SupportBean(IntPrimitive=10)@consume(2))]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E1"});

                env.SendEventBean(new SupportBean("E3", 1));
                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", "E5"});
                env.UndeployAll();

                // test SODA
                env.EplToModelCompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E1"});

                env.UndeployAll();
            }
        }

        public class PatternFilterAndSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('A') select a.TheString as a, b.TheString as b from pattern[a=SupportBean and b=SupportBean(TheString='A')@consume]";
                env.CompileDeploy(epl).AddListener("A");

                string[] fields = {"a", "b"};
                env.SendEventBean(new SupportBean("A", 10));
                Assert.IsFalse(env.Listener("A").IsInvoked);

                env.SendEventBean(new SupportBean("X", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("A").AssertOneGetNew(),
                    fields,
                    new object[] {"X", "A"});

                env.UndeployAll();
            }
        }

        internal class PatternOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "a","b" };
                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean] order by a asc",
                    new[] {new object[] {null, "E1"}, new object[] {"E1", null}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(1)] order by a asc",
                    new[] {new object[] {null, "E1"}, new object[] {"E1", null}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(2) or b=SupportBean@consume(1)] order by a asc",
                    new object[] {"E1", null});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2)] order by a asc",
                    new object[] {null, "E1"});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean@consume(2)] order by a asc",
                    new object[] {null, "E1"});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean] order by a asc",
                    new object[] {"E1", null});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=11)@consume(1) or b=SupportBean] order by a asc",
                    new object[] {null, "E1"});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=10)@consume(1) or b=SupportBean] order by a asc",
                    new object[] {"E1", null});

                fields = new [] { "a","b","c" };
                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2) or c=SupportBean@consume(3)] order by a,b,c",
                    new[] {new object[] {null, null, "E1"}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new[] {new object[] {null, null, "E1"}, new object[] {null, "E1", null}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new[] {
                        new object[] {null, null, "E1"}, new object[] {null, "E1", null},
                        new object[] {"E1", null, null}
                    });

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(1)] order by a,b,c",
                    new[] {new object[] {null, "E1", null}, new object[] {"E1", null, null}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(1) or every c=SupportBean@consume(2)] order by a,b,c",
                    new[] {new object[] {null, null, "E1"}, new object[] {"E1", null, null}});

                TryAssertion(
                    env,
                    fields,
                    "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(0) or every b=SupportBean or every c=SupportBean] order by a,b,c",
                    new[] {
                        new object[] {null, null, "E1"}, new object[] {null, "E1", null},
                        new object[] {"E1", null, null}
                    });
            }
        }

        internal class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern[every a=SupportBean@consume()]",
                    "Incorrect syntax near ')' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a closing parenthesis ')' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume()]]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern[every a=SupportBean@consume(-1)]",
                    "Incorrect syntax near '-' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a minus '-' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume(-1)]]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from pattern[every a=SupportBean@xx]",
                    "Unexpected pattern filter @ annotation, expecting 'consume' but received 'xx' [select * from pattern[every a=SupportBean@xx]]");
            }
        }
    }
} // end of namespace