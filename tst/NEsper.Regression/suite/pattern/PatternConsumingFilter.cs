///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.pattern
{
    public class PatternConsumingFilter
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithFollowedBy(execs);
            WithAnd(execs);
            WithFilterAndSceneTwo(execs);
            WithOr(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithOr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternOr());
            return execs;
        }

        public static IList<RegressionExecution> WithFilterAndSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternFilterAndSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithAnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternAnd());
            return execs;
        }

        public static IList<RegressionExecution> WithFollowedBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new PatternFollowedBy());
            return execs;
        }

        private class PatternFollowedBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,b".SplitCsv();
                var pattern =
                    "@name('s0') select a.theString as a, b.theString as b from pattern[every a=SupportBean -> b=SupportBean@consume]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E2" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 0));
                env.SendEventBean(new SupportBean("E4", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E3", "E4" });

                env.SendEventBean(new SupportBean("E5", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E6", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E5", "E6" });

                env.UndeployAll();
            }
        }

        private class PatternAnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,b".SplitCsv();
                var pattern =
                    "@name('s0') select a.theString as a, b.theString as b from pattern[every (a=SupportBean and b=SupportBean)]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1" });
                env.UndeployAll();

                pattern =
                    "@name('s0') select a.theString as a, b.theString as b from pattern [every (a=SupportBean and b=SupportBean(intPrimitive=10)@consume(2))]";
                env.CompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E1" });

                env.SendEventBean(new SupportBean("E3", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E4", 1));
                env.SendEventBean(new SupportBean("E5", 10));
                env.AssertPropsNew("s0", fields, new object[] { "E3", "E5" });
                env.UndeployAll();

                // test SODA
                env.EplToModelCompileDeploy(pattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E1" });

                env.UndeployAll();
            }
        }

        public class PatternFilterAndSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('A') select a.theString as a, b.theString as b from pattern[a=SupportBean and b=SupportBean(theString='A')@consume]";
                env.CompileDeploy(epl).AddListener("A");

                var fields = new string[] { "a", "b" };
                env.SendEventBean(new SupportBean("A", 10));
                env.AssertListenerNotInvoked("A");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("X", 10));
                env.AssertPropsNew("A", fields, new object[] { "X", "A" });

                env.UndeployAll();
            }
        }

        private class PatternOr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "a,b".SplitCsv();
                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean or b=SupportBean] order by a asc",
                    new object[][] { new object[] { null, "E1" }, new object[] { "E1", null } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(1)] order by a asc",
                    new object[][] { new object[] { null, "E1" }, new object[] { "E1", null } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean@consume(2) or b=SupportBean@consume(1)] order by a asc",
                    new object[] { "E1", null });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2)] order by a asc",
                    new object[] { null, "E1" });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean or b=SupportBean@consume(2)] order by a asc",
                    new object[] { null, "E1" });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean] order by a asc",
                    new object[] { "E1", null });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean(intPrimitive=11)@consume(1) or b=SupportBean] order by a asc",
                    new object[] { null, "E1" });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b from pattern[every a=SupportBean(intPrimitive=10)@consume(1) or b=SupportBean] order by a asc",
                    new object[] { "E1", null });

                fields = "a,b,c".SplitCsv();
                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2) or c=SupportBean@consume(3)] order by a,b,c",
                    new object[][] { new object[] { null, null, "E1" } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][] { new object[] { null, null, "E1" }, new object[] { null, "E1", null } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][] {
                        new object[] { null, null, "E1" }, new object[] { null, "E1", null },
                        new object[] { "E1", null, null }
                    });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(1)] order by a,b,c",
                    new object[][] { new object[] { null, "E1", null }, new object[] { "E1", null, null } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(1) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][] { new object[] { null, null, "E1" }, new object[] { "E1", null, null } });

                TryAssertion(
                    env,
                    fields,
                    "select a.theString as a, b.theString as b, c.theString as c from pattern[every a=SupportBean@consume(0) or every b=SupportBean or every c=SupportBean] order by a,b,c",
                    new object[][] {
                        new object[] { null, null, "E1" }, new object[] { null, "E1", null },
                        new object[] { "E1", null, null }
                    });
            }
        }

        private class PatternInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select * from pattern[every a=SupportBean@consume()]",
                    "Incorrect syntax near ')' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a closing parenthesis ')' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume()]]");
                env.TryInvalidCompile(
                    "select * from pattern[every a=SupportBean@consume(-1)]",
                    "Incorrect syntax near '-' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a minus '-' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume(-1)]]");
                env.TryInvalidCompile(
                    "select * from pattern[every a=SupportBean@xx]",
                    "Unexpected pattern filter @ annotation, expecting 'consume' but received 'xx' [select * from pattern[every a=SupportBean@xx]]");
            }
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            string[] fields,
            string pattern,
            object expected)
        {
            env.CompileDeploy("@name('s0') " + pattern).AddListener("s0");
            env.SendEventBean(new SupportBean("E1", 10));

            if (expected is object[][]) {
                env.AssertPropsPerRowLastNew("s0", fields, (object[][])expected);
            }
            else {
                env.AssertPropsNew("s0", fields, (object[])expected);
            }

            env.UndeployAll();
        }
    }
} // end of namespace