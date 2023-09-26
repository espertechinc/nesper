///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextVariables
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSegmentedByKey(execs);
            WithOverlapping(execs);
            WithIterateAndListen(execs);
            WithGetSetAPI(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextVariablesInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithGetSetAPI(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextVariablesGetSetAPI());
            return execs;
        }

        public static IList<RegressionExecution> WithIterateAndListen(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextVariablesIterateAndListen());
            return execs;
        }

        public static IList<RegressionExecution> WithOverlapping(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextVariablesOverlapping());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedByKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextVariablesSegmentedByKey());
            return execs;
        }

        private class ContextVariablesSegmentedByKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "mycontextvar".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyCtx as " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0",
                    path);
                env.CompileDeploy("@public context MyCtx create variable int mycontextvar = 0", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(intPrimitive > 0) set mycontextvar = intPrimitive",
                    path);

                env.CompileDeploy("@name('s0') context MyCtx select mycontextvar from SupportBean_S0", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("P1", 0)); // allocate partition P1
                env.SendEventBean(new SupportBean("P1", 10)); // set variable
                env.SendEventBean(new SupportBean_S0(1, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { 10 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("P2", 11)); // allocate and set variable partition E2
                env.SendEventBean(new SupportBean_S0(2, "P2"));
                env.AssertPropsNew("s0", fields, new object[] { 11 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(3, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { 10 });
                env.SendEventBean(new SupportBean_S0(4, "P2"));
                env.AssertPropsNew("s0", fields, new object[] { 11 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(5, "P3"));
                env.AssertPropsNew("s0", fields, new object[] { 0 });

                env.SendEventBean(new SupportBean("P3", 12));
                env.SendEventBean(new SupportBean_S0(6, "P3"));
                env.AssertPropsNew("s0", fields, new object[] { 12 });

                env.UndeployAll();
            }
        }

        private class ContextVariablesOverlapping : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "mycontextvar".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyCtx as " +
                    "initiated by SupportBean_S0 s0 terminated by SupportBean_S1(p10 = s0.p00)",
                    path);
                env.CompileDeploy("@public context MyCtx create variable int mycontextvar = 5", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(theString = context.s0.p00) set mycontextvar = intPrimitive",
                    path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(intPrimitive < 0) set mycontextvar = intPrimitive",
                    path);

                env.CompileDeploy(
                    "@name('s0') context MyCtx select mycontextvar from SupportBean_S2(p20 = context.s0.p00)",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S2(1, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { 5 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(0, "P2")); // allocate partition P2

                env.Milestone(3);

                env.SendEventBean(new SupportBean("P2", 10));

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                env.AssertPropsNew("s0", fields, new object[] { 10 });

                // set all to -1
                env.SendEventBean(new SupportBean("P2", -1));

                env.Milestone(5);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                env.AssertPropsNew("s0", fields, new object[] { -1 });

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S2(2, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { -1 });

                env.Milestone(7);

                env.SendEventBean(new SupportBean("P2", 20));

                env.Milestone(8);

                env.SendEventBean(new SupportBean("P1", 21));

                env.Milestone(9);

                env.SendEventBean(new SupportBean_S2(2, "P2"));
                env.AssertPropsNew("s0", fields, new object[] { 20 });

                env.Milestone(10);

                env.SendEventBean(new SupportBean_S2(2, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { 21 });

                // terminate context partitions
                env.SendEventBean(new SupportBean_S1(0, "P1"));
                env.SendEventBean(new SupportBean_S1(0, "P2"));

                env.Milestone(11);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1
                env.SendEventBean(new SupportBean_S2(1, "P1"));
                env.AssertPropsNew("s0", fields, new object[] { 5 });

                env.UndeployAll();

                // test module deployment and undeployment
                var epl = "@name(\"context\")\n" +
                          "create context MyContext\n" +
                          "initiated by distinct(theString) SupportBean as input\n" +
                          "terminated by SupportBean(theString = input.theString);\n" +
                          "\n" +
                          "@name(\"ctx variable counter\")\n" +
                          "context MyContext create variable integer counter = 0;\n";
                env.CompileDeploy(epl).UndeployAll();
            }
        }

        private class ContextVariablesIterateAndListen : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyCtx as initiated by SupportBean_S0 s0 terminated after 24 hours",
                    path);

                var fields = "mycontextvar".SplitCsv();
                env.CompileDeploy("@name('var') @public context MyCtx create variable int mycontextvar = 5", path);

                env.Milestone(0);

                env.CompileDeploy(
                    "@name('upd') context MyCtx on SupportBean(theString = context.s0.p00) set mycontextvar = intPrimitive",
                    path);
                env.AddListener("var").AddListener("upd");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "P1")); // allocate partition P1

                env.Milestone(2);

                env.SendEventBean(new SupportBean("P1", 100)); // update
                env.AssertPropsNew("upd", fields, new object[] { 100 });
                env.AssertPropsPerRowIterator("upd", fields, new object[][] { new object[] { 100 } });
                env.AssertPropsIRPair("var", fields, new object[] { 100 }, new object[] { 5 });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(0, "P2")); // allocate partition P1

                env.Milestone(4);

                env.SendEventBean(new SupportBean("P2", 101)); // update
                env.AssertPropsNew("upd", fields, new object[] { 101 });
                env.AssertPropsPerRowIterator(
                    "upd",
                    fields,
                    new object[][] { new object[] { 100 }, new object[] { 101 } });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "var",
                    fields,
                    new object[][] { new object[] { 100 }, new object[] { 101 } });
                env.AssertPropsIRPair("var", fields, new object[] { 101 }, new object[] { 5 });

                env.UndeployAll();
            }
        }

        private class ContextVariablesGetSetAPI : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyCtx as initiated by SupportBean_S0 s0 terminated after 24 hours",
                    path);
                env.CompileDeploy("@name('var') @public context MyCtx create variable int mycontextvar = 5", path);
                env.CompileDeploy(
                    "context MyCtx on SupportBean(theString = context.s0.p00) set mycontextvar = intPrimitive",
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
                env.CompileDeploy("@name('globalvar') create variable int myglobarvar = 0");
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

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ContextVariablesInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create context MyCtxOne as partition by theString from SupportBean", path);
                env.CompileDeploy("@public create context MyCtxTwo as partition by p00 from SupportBean_S0", path);
                env.CompileDeploy("@public context MyCtxOne create variable int myctxone_int = 0", path);

                // undefined context
                env.TryInvalidCompile(
                    path,
                    "context MyCtx create variable int mycontext_invalid1 = 0",
                    "Context by name 'MyCtx' could not be found");

                // wrong context uses variable
                env.TryInvalidCompile(
                    path,
                    "context MyCtxTwo select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' is not available for use with context 'MyCtxTwo'");

                // variable use outside of context
                env.TryInvalidCompile(
                    path,
                    "select myctxone_int from SupportBean_S0",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0#expr(myctxone_int > 5)",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0#keepall limit myctxone_int",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0#keepall limit 10 offset myctxone_int",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0#keepall output every myctxone_int events",
                    "Failed to validate the output rate limiting clause: Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");
                env.TryInvalidCompile(
                    path,
                    "@Hint('reclaim_group_aged=myctxone_int') select longPrimitive, count(*) from SupportBean group by longPrimitive",
                    "Variable 'myctxone_int' defined for use with context 'MyCtxOne' can only be accessed within that context");

                env.UndeployAll();
            }
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
    }
} // end of namespace