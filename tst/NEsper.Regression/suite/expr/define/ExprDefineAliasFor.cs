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

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineAliasFor
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithContextPartition(execs);
            WithDocSamples(execs);
            WithNestedAlias(execs);
            WithAliasAggregation(execs);
            WithGlobalAliasAndSODA(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithGlobalAliasAndSODA(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineGlobalAliasAndSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithAliasAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineAliasAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineNestedAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithDocSamples(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineDocSamples());
            return execs;
        }

        public static IList<RegressionExecution> WithContextPartition(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprDefineContextPartition());
            return execs;
        }

        private class ExprDefineContextPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create expression the_expr alias for {TheString='a' and IntPrimitive=1};\n" +
                          "create context the_context start @now end after 10 minutes;\n" +
                          "@name('s0') context the_context select * from SupportBean(the_expr)\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("a", 1));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean("b", 1));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class ExprDefineDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create schema SampleEvent()", path);
                env.CompileDeploy(
                    "expression twoPI alias for {Math.PI * 2}\n" +
                    "select twoPI from SampleEvent",
                    path);

                env.CompileDeploy("@public create schema EnterRoomEvent()", path);
                env.CompileDeploy(
                    "expression countPeople alias for {count(*)} \n" +
                    "select countPeople from EnterRoomEvent#time(10 seconds) having countPeople > 10",
                    path);

                env.UndeployAll();
            }
        }

        private class ExprDefineNestedAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy("@public create expression F1 alias for {10}", path);
                env.CompileDeploy("@public create expression F2 alias for {20}", path);
                env.CompileDeploy("@public create expression F3 alias for {F1+F2}", path);
                env.CompileDeploy("@name('s0') select F3 as c0 from SupportBean", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 30 });

                env.UndeployAll();
            }
        }

        private class ExprDefineAliasAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') @Audit expression Total alias for {sum(IntPrimitive)} "+
"select Total, Total+1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = "Total,Total+1".SplitCsv();
                env.AssertStatement(
                    "s0",
                    statement => {
                        foreach (var field in fields) {
                            Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType(field));
                        }
                    });

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 10, 11 });

                env.UndeployAll();
            }
        }

        private class ExprDefineGlobalAliasAndSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "@public create expression myaliastwo alias for {2}";
                env.CompileDeploy(eplDeclare, path);

                env.CompileDeploy("@public create expression myalias alias for {1}", path);
                env.CompileDeploy("@name('s0') select myaliastwo from SupportBean(IntPrimitive = myalias)", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEqualsNew("s0", "myaliastwo", 2);

                env.UndeployAll();
            }
        }

        private class ExprDefineInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
"expression Total alias for {sum(xxx)} select Total+1 from SupportBean",
"Failed to validate select-clause expression 'Total+1': Failed to validate expression alias 'Total': Failed to validate alias expression body expression 'sum(xxx)': Property named 'xxx' is not valid in any stream [expression Total alias for {sum(xxx)} select Total+1 from SupportBean]");
                env.TryInvalidCompile(
"expression Total xxx for {1} select Total+1 from SupportBean",
"For expression alias 'Total' expecting 'alias' keyword but received 'xxx' [expression Total xxx for {1} select Total+1 from SupportBean]");
                env.TryInvalidCompile(
"expression Total(a) alias for {1} select Total+1 from SupportBean",
"For expression alias 'Total' expecting no parameters but received 'a' [expression Total(a) alias for {1} select Total+1 from SupportBean]");
                env.TryInvalidCompile(
"expression Total alias for {a -> 1} select Total+1 from SupportBean",
"For expression alias 'Total' expecting an expression without parameters but received 'a ->' [expression Total alias for {a -> 1} select Total+1 from SupportBean]");
                env.TryInvalidCompile(
"expression Total alias for ['some text'] select Total+1 from SupportBean",
"For expression alias 'Total' expecting an expression but received a script [expression Total alias for ['some text'] select Total+1 from SupportBean]");
            }
        }
    }
} // end of namespace