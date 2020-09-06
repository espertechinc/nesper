///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextVariables
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextVariablesSegmentedByKey());
            execs.Add(new ContextVariablesOverlapping());
            execs.Add(new ContextVariablesIterateAndListen());
            execs.Add(new ContextVariablesGetSetAPI());
            execs.Add(new ContextVariablesInvalid());
            return execs;
        }

        private static void AssertVariableValues(
            RegressionEnvironment env,
            int agentInstanceId,
            int expected)
        {
            var namePairVariable = new DeploymentIdNamePair(env.DeploymentId("var"), "mycontextvar");
            var states = env.Runtime.VariableService.GetVariableValue(
                Collections.SingletonSet(namePairVariable),
                new SupportSelectorById(agentInstanceId));
            Assert.AreEqual(1, states.Count);
            var list = states.Get(namePairVariable);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(expected, list[0].State);
        }

        internal class ContextVariablesSegmentedByKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "mycontextvar" };
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyCtx as " +
                    "partition by TheString from SupportBean, P00 from SupportBean_S0",
                    path);
                env.CompileDeploy("context MyCtx create variable int mycontextvar = 0", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(IntPrimitive > 0) set mycontextvar = IntPrimitive",
                    path);

                env.CompileDeploy("@Name('s0') context MyCtx select mycontextvar from SupportBean_S0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("P1", 0)); // allocate partition P1
                env.SendEventBean(new SupportBean("P1", 10)); // set variable
                env.SendEventBean(new SupportBean_S0(1, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("P2", 11)); // allocate and set variable partition E2
                env.SendEventBean(new SupportBean_S0(2, "P2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});
                env.SendEventBean(new SupportBean_S0(4, "P2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(5, "P3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0});

                env.SendEventBean(new SupportBean("P3", 12));
                env.SendEventBean(new SupportBean_S0(6, "P3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12});

                env.UndeployAll();
            }
        }

        internal class ContextVariablesOverlapping : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "mycontextvar" };
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyCtx as " +
                    "initiated by SupportBean_S0 S0 terminated by SupportBean_S1(P10 = S0.P00)",
                    path);
                env.CompileDeploy("context MyCtx create variable int mycontextvar = 5", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(TheString = context.S0.P00) set mycontextvar = IntPrimitive",
                    path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(IntPrimitive < 0) set mycontextvar = IntPrimitive",
                    path);

                env.CompileDeploy(
                    "@Name('s0') context MyCtx select mycontextvar from SupportBean_S2(P20 = context.S0.P00)",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S2(1, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5});

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(0, "P2")); // allocate partition P2

                env.Milestone(3);

                env.SendEventBean(new SupportBean("P2", 10));

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10});

                // set all to -1
                env.SendEventBean(new SupportBean("P2", -1));

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {-1});

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S2(2, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {-1});

                env.Milestone(7);

                env.SendEventBean(new SupportBean("P2", 20));

                env.Milestone(8);

                env.SendEventBean(new SupportBean("P1", 21));

                env.Milestone(9);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20});

                env.Milestone(10);

                env.SendEventBean(new SupportBean_S2(2, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {21});

                // terminate context partitions
                env.SendEventBean(new SupportBean_S1(0, "P1"));
                env.SendEventBean(new SupportBean_S1(0, "P2"));

                env.Milestone(11);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1
                env.SendEventBean(new SupportBean_S2(1, "P1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5});

                env.UndeployAll();

                // test module deployment and undeployment
                var epl = "@Name(\"context\")\n" +
                          "create context MyContext\n" +
                          "initiated by distinct(TheString) SupportBean as input\n" +
                          "terminated by SupportBean(TheString = input.TheString);\n" +
                          "\n" +
                          "@Name(\"ctx variable counter\")\n" +
                          "context MyContext create variable integer counter = 0;\n";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        internal class ContextVariablesIterateAndListen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context MyCtx as initiated by SupportBean_S0 S0 terminated after 24 hours",
                    path);

                var fields = new [] { "mycontextvar" };
                env.CompileDeploy("@Name('var') context MyCtx create variable int mycontextvar = 5", path);

                env.Milestone(0);

                env.CompileDeploy(
                    "@Name('upd') context MyCtx on SupportBean(TheString = context.S0.P00) set mycontextvar = IntPrimitive",
                    path);
                env.AddListener("var").AddListener("upd");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1

                env.Milestone(2);

                env.SendEventBean(new SupportBean("P1", 100)); // update
                EPAssertionUtil.AssertProps(
                    env.Listener("upd").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100});
                EPAssertionUtil.AssertPropsPerRow(
                    EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("upd")),
                    fields,
                    new[] {new object[] {100}});
                EPAssertionUtil.AssertProps(
                    env.Listener("var").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {100},
                    new object[] {5});

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(0, "P2")); // allocate partition P1

                env.Milestone(4);

                env.SendEventBean(new SupportBean("P2", 101)); // update
                EPAssertionUtil.AssertProps(
                    env.Listener("upd").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {101});
                EPAssertionUtil.AssertPropsPerRow(
                    EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("upd")),
                    fields,
                    new[] {new object[] {100}, new object[] {101}});

                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("var"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    events,
                    fields,
                    new[] {new object[] {100}, new object[] {101}});
                EPAssertionUtil.AssertProps(
                    env.Listener("var").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {101},
                    new object[] {5});

                env.UndeployAll();
            }
        }

        internal class ContextVariablesGetSetAPI : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context MyCtx as initiated by SupportBean_S0 S0 terminated after 24 hours",
                    path);
                env.CompileDeploy("@Name('var') context MyCtx create variable int mycontextvar = 5", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(TheString = context.S0.P00) set mycontextvar = IntPrimitive",
                    path);
                var namePairVariable = new DeploymentIdNamePair(env.DeploymentId("var"), "mycontextvar");

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1
                AssertVariableValues(env, 0, 5);

                env.Runtime.VariableService.SetVariableValue(
                    Collections.SingletonMap<DeploymentIdNamePair, object>(namePairVariable, 10),
                    0);
                AssertVariableValues(env, 0, 10);

                env.SendEventBean(new SupportBean_S0(0, "P2")); // allocate partition P2
                AssertVariableValues(env, 1, 5);

                env.Runtime.VariableService.SetVariableValue(
                    Collections.SingletonMap<DeploymentIdNamePair, object>(namePairVariable, 11),
                    1);
                AssertVariableValues(env, 1, 11);

                // global variable - trying to set via context partition selection
                env.CompileDeploy("@Name('globalvar') create variable int myglobarvar = 0");
                var nameGlobalVar = new DeploymentIdNamePair(env.DeploymentId("globalvar"), "myglobarvar");
                try {
                    env.Runtime.VariableService.SetVariableValue(
                        Collections.SingletonMap<DeploymentIdNamePair, object>(nameGlobalVar, 11),
                        0);
                    Assert.Fail();
                }
                catch (VariableNotFoundException ex) {
                    Assert.AreEqual(
                        "Variable by name 'myglobarvar' is a global variable and not context-partitioned",
                        ex.Message);
                }

                // global variable - trying to get via context partition selection
                try {
                    env.Runtime.VariableService.GetVariableValue(
                        Collections.SingletonSet(nameGlobalVar),
                        new SupportSelectorById(1));
                    Assert.Fail();
                }
                catch (VariableNotFoundException ex) {
                    Assert.AreEqual(
                        "Variable by name 'myglobarvar' is a global variable and not context-partitioned",
                        ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextVariablesInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context MyCtxOne as partition by TheString from SupportBean", path);
                env.CompileDeploy("create context MyCtxTwo as partition by P00 from SupportBean_S0", path);
                env.CompileDeploy("context MyCtxOne create variable int myctxone_int = 0", path);

                // undefined context
                TryInvalidCompile(
                    env,
                    path,
                    "context MyCtx create variable int mycontext_invalid1 = 0",
                    "Context by name 'MyCtx' could not be found");

                // wrong context uses variable
                TryInvalidCompile(
                    env,
                    path,
                    "context MyCtxTwo select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' is not available for use with context 'MyCtxTwo'");

                // variable use outside of context
                TryInvalidCompile(
                    env,
                    path,
                    "select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0#expr(myctxone_int > 5)",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0#keepall limit myctxone_int",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0#keepall limit 10 offset myctxone_int",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0#keepall output every myctxone_int events",
                    "Failed to validate the output rate limiting clause: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                TryInvalidCompile(
                    env,
                    path,
                    "@Hint('reclaim_group_aged=myctxone_int') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");

                env.UndeployAll();
            }
        }
    }
} // end of namespace