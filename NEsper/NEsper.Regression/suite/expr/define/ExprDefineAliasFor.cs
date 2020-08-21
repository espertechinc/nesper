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

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineAliasFor
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprDefineContextPartition());
            execs.Add(new ExprDefineDocSamples());
            execs.Add(new ExprDefineNestedAlias());
            execs.Add(new ExprDefineAliasAggregation());
            execs.Add(new ExprDefineGlobalAliasAndSODA());
            execs.Add(new ExprDefineInvalid());
            return execs;
        }

        internal class ExprDefineContextPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create expression the_expr alias for {TheString='a' and IntPrimitive=1};\n" +
                          "create context the_context start @now end after 10 minutes;\n" +
                          "@Name('s0') context the_context select * from SupportBean(the_expr)\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("a", 1));
                Assert.IsTrue(env.Listener("s0").IsInvokedAndReset());

                env.SendEventBean(new SupportBean("b", 1));
                Assert.IsFalse(env.Listener("s0").IsInvokedAndReset());

                env.UndeployAll();
            }
        }

        internal class ExprDefineDocSamples : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create schema SampleEvent()", path);
                env.CompileDeploy(
                    "expression twoPI alias for {Math.PI * 2}\n" +
                    "select twoPI from SampleEvent",
                    path);

                env.CompileDeploy("create schema EnterRoomEvent()", path);
                env.CompileDeploy(
                    "expression countPeople alias for {count(*)} \n" +
                    "select countPeople from EnterRoomEvent#time(10 seconds) having countPeople > 10",
                    path);

                env.UndeployAll();
            }
        }

        internal class ExprDefineNestedAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0"};

                var path = new RegressionPath();
                env.CompileDeploy("create expression F1 alias for {10}", path);
                env.CompileDeploy("create expression F2 alias for {20}", path);
                env.CompileDeploy("create expression F3 alias for {F1+F2}", path);
                env.CompileDeploy("@Name('s0') select F3 as c0 from SupportBean", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {30});

                env.UndeployAll();
            }
        }

        internal class ExprDefineAliasAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') @Audit expression total alias for {sum(IntPrimitive)} " +
                          "select total, total+1 from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                var fields = new[] {"total", "total+1"};
                foreach (var field in fields) {
                    Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(field));
                }

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 11});

                env.UndeployAll();
            }
        }

        internal class ExprDefineGlobalAliasAndSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplDeclare = "create expression myaliastwo alias for {2}";
                env.CompileDeploy(eplDeclare, path);

                env.CompileDeploy("create expression myalias alias for {1}", path);
                env.CompileDeploy("@Name('s0') select myaliastwo from SupportBean(IntPrimitive = myalias)", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(2, env.Listener("s0").AssertOneGetNewAndReset().Get("myaliastwo"));

                env.UndeployAll();
            }
        }

        internal class ExprDefineInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "expression total alias for {sum(xxx)} select total+1 from SupportBean",
                    "Failed to validate select-clause expression 'total+1': Failed to validate expression alias 'total': Failed to validate alias expression body expression 'sum(xxx)': Property named 'xxx' is not valid in any stream [expression total alias for {sum(xxx)} select total+1 from SupportBean]");
                TryInvalidCompile(
                    env,
                    "expression total xxx for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting 'alias' keyword but received 'xxx' [expression total xxx for {1} select total+1 from SupportBean]");
                TryInvalidCompile(
                    env,
                    "expression total(a) alias for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting no parameters but received 'a' [expression total(a) alias for {1} select total+1 from SupportBean]");
                TryInvalidCompile(
                    env,
                    "expression total alias for {a -> 1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression without parameters but received 'a ->' [expression total alias for {a -> 1} select total+1 from SupportBean]");
                TryInvalidCompile(
                    env,
                    "expression total alias for ['some text'] select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression but received a script [expression total alias for ['some text'] select total+1 from SupportBean]");
            }
        }
    }
} // end of namespace