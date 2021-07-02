///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendSingleRowFunction
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithEventBeanFootprint(execs);
            WithPropertyOrSingleRowMethod(execs);
            WithChainMethod(execs);
            WithSingleMethod(execs);
            WithFailedValidation(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFailedValidation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendSRFFailedValidation());
            return execs;
        }

        public static IList<RegressionExecution> WithSingleMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendSRFSingleMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithChainMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendSRFChainMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithPropertyOrSingleRowMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendSRFPropertyOrSingleRowMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithEventBeanFootprint(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientExtendSRFEventBeanFootprint());
            return execs;
        }

        private static void TryAssertionChainMethod(RegressionEnvironment env)
        {
            string[] fields = {"val"};
            env.SendEventBean(new SupportBean("a", 3));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {36});

            env.UndeployAll();
        }

        private static void TryAssertionSingleMethod(RegressionEnvironment env)
        {
            string[] fields = {"val"};
            env.SendEventBean(new SupportBean("a", 2));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {8});
            env.UndeployAll();
        }

        public static bool LocalIsNullValue(
            EventBean @event,
            string propertyName)
        {
            return @event.Get(propertyName) == null;
        }

        internal class ClientExtendSRFEventBeanFootprint : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
#if false
                // test select-clause
                string[] fields = {"c0", "c1"};
                var text = "@Name('s0') select IsNullValue(*, 'TheString') as c0," +
                           typeof(ClientExtendSingleRowFunction).Name +
                           ".LocalIsNullValue(*, 'TheString') as c1 from SupportBean";

                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(new SupportBean("a", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false, false});

                env.SendEventBean(new SupportBean(null, 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true, true});
                env.UndeployAll();

                // test pattern
                var textPattern =
                    "@Name('s0') select * from pattern [a=SupportBean -> b=SupportBean(TheString=getValueAsString(a, 'TheString'))]";
                env.CompileDeploy(textPattern).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new [] { "a.IntPrimitive","b.IntPrimitive" },
                    new object[] {1, 2});
                env.UndeployAll();

                // test filter
                var textFilter = "@Name('s0') select * from SupportBean('E1'=getValueAsString(*, 'TheString'))";
                env.CompileDeploy(textFilter).AddListener("s0");
                env.SendEventBean(new SupportBean("E2", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                Assert.AreEqual(1, env.Listener("s0").GetAndResetLastNewData().Length);
                env.UndeployAll();

                // test "first"
                var textAccessAgg =
                    "@Name('s0') select * from SupportBean#keepall having 'E2' = getValueAsString(last(*), 'TheString')";
                env.CompileDeploy(textAccessAgg).AddListener("s0");
                env.SendEventBean(new SupportBean("E2", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                Assert.AreEqual(1, env.Listener("s0").GetAndResetLastNewData().Length);
                env.UndeployAll();

#endif

                // test "window"
                var textWindowAgg =
                    "@Name('s0') select * from SupportBean#keepall having eventsCheckStrings(window(*), 'TheString', 'E1')";
                env.CompileDeploy(textWindowAgg).AddListener("s0");
                env.SendEventBean(new SupportBean("E2", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                Assert.AreEqual(1, env.Listener("s0").GetAndResetLastNewData().Length);
                env.UndeployAll();
            }
        }

        internal class ClientExtendSRFPropertyOrSingleRowMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select surroundx('test') as val from SupportBean";
                env.CompileDeploy(text).AddListener("s0");

                string[] fields = {"val"};
                env.SendEventBean(new SupportBean("a", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"XtestX"});

                env.UndeployAll();
            }
        }

        internal class ClientExtendSRFChainMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select chainTop().ChainValue(12,IntPrimitive) as val from SupportBean";

                env.CompileDeploy(text).AddListener("s0");
                TryAssertionChainMethod(env);

                env.EplToModelCompileDeploy(text).AddListener("s0");
                TryAssertionChainMethod(env);
            }
        }

        internal class ClientExtendSRFSingleMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select power3(IntPrimitive) as val from SupportBean";

                env.CompileDeploy(text).AddListener("s0");
                TryAssertionSingleMethod(env);

                env.EplToModelCompileDeploy(text).AddListener("s0");
                TryAssertionSingleMethod(env);

                text = "@Name('s0') select power3(2) as val from SupportBean";
                env.CompileDeploy(text).AddListener("s0");
                TryAssertionSingleMethod(env);

                // test passing a context as well
                text = "@Name('s0') select power3Context(IntPrimitive) as val from SupportBean";
                var args = new CompilerArguments(env.Configuration);
                args.Options.StatementUserObject = _ => "my_user_object";
                var compiled = env.Compile(text, args);
                env.Deploy(compiled).AddListener("s0");

                SupportSingleRowFunction.MethodInvocationContexts.Clear();
                TryAssertionSingleMethod(env);
                var context = SupportSingleRowFunction.MethodInvocationContexts[0];
                Assert.AreEqual("s0", context.StatementName);
                Assert.AreEqual(env.Runtime.URI, context.RuntimeURI);
                Assert.AreEqual(-1, context.ContextPartitionId);
                Assert.AreEqual("power3Context", context.FunctionName);
                Assert.AreEqual("my_user_object", context.StatementUserObject);

                env.UndeployAll();

                // test exception behavior
                // logged-only
                env.CompileDeploy("@Name('s0') select throwExceptionLogMe() from SupportBean").AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();

                // rethrow
                env.CompileDeploy("@Name('s0') select throwExceptionRethrow() from SupportBean").AddListener("s0");
                try {
                    env.SendEventBean(new SupportBean("E1", 1));
                    Assert.Fail();
                }
                catch (EPException ex) {
                    Assert.AreEqual(
                        "Unexpected exception in statement 's0': Invocation exception when invoking method 'Throwexception' of class '" +
                        nameof(SupportSingleRowFunction) +
                        "' passing parameters [] for statement 's0': com.espertech.esper.common.client.EPException : This is a 'throwexception' generated exception",
                        ex.Message);
                    env.UndeployAll();
                }

                // NPE when boxed is null
                env.CompileDeploy("@Name('s0') select power3Rethrow(IntBoxed) from SupportBean").AddListener("s0");
                try {
                    env.SendEventBean(new SupportBean("E1", 1));
                    Assert.Fail();
                }
                catch (EPException ex) {
                    Assert.AreEqual(
                        "Unexpected exception in statement 's0': NullPointerException invoking method 'ComputePower3' of class '" +
                        nameof(SupportSingleRowFunction) +
                        "' in parameter 0 passing parameters [null] for statement 's0': The method expects a primitive Int32 value but received a null value",
                        ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ClientExtendSRFFailedValidation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select singlerow('a', 'b') from SupportBean",
                    "Failed to validate select-clause expression 'singlerow(\"a\",\"b\")': Could not find static method named 'TestSingleRow' in class '" +
                    typeof(SupportSingleRowFunctionTwo).FullName +
                    "' with matching parameter number and expected parameter type(s) 'System.String, System.String' (nearest match found was 'TestSingleRow' taking type(s) 'System.String, System.Int32')");
            }
        }
    }
} // end of namespace