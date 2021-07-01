///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeItself
    {
        public const string TEST_SERVICE_NAME = "TEST_SERVICE_NAME";
        public const int TEST_SECRET_VALUE = 12345;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithItselfTransientConfiguration(execs);
            WithSPICompileReflective(execs);
            WithSPIStatementSelection(execs);
            WithSPIBeanAnonymousType(execs);
            WithWrongCompileMethod(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithWrongCompileMethod(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeWrongCompileMethod());
            return execs;
        }

        public static IList<RegressionExecution> WithSPIBeanAnonymousType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSPIBeanAnonymousType());
            return execs;
        }

        public static IList<RegressionExecution> WithSPIStatementSelection(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSPIStatementSelection());
            return execs;
        }

        public static IList<RegressionExecution> WithSPICompileReflective(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeSPICompileReflective());
            return execs;
        }

        public static IList<RegressionExecution> WithItselfTransientConfiguration(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeItselfTransientConfiguration());
            return execs;
        }

        private class ClientRuntimeSPIBeanAnonymousType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var beanEventTypeService = new EPRuntimeBeanAnonymousTypeService(env.Container);
                var beanEventType = beanEventTypeService.MakeBeanEventTypeAnonymous(typeof(MyBeanAnonymousType));
                Assert.AreEqual(typeof(int), beanEventType.GetPropertyType("Prop"));
            }
        }

        private class ClientRuntimeSPIStatementSelection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('a') select * from SupportBean;\n" +
                    "@Name('b') select * from SupportBean(TheString='xxx');\n");
                var spi = (EPRuntimeSPI) env.Runtime;

                var myTraverse = new MyStatementTraverse();
                spi.TraverseStatements(myTraverse.Accept);
                myTraverse.AssertAndReset(env.Statement("a"), env.Statement("b"));

                var filter = spi.StatementSelectionSvc.CompileFilterExpression("Name='b'");
                spi.StatementSelectionSvc.TraverseStatementsFilterExpr(myTraverse.Accept, filter);
                myTraverse.AssertAndReset(env.Statement("b"));
                spi.StatementSelectionSvc.CompileFilterExpression("DeploymentId like 'x'");

                spi.StatementSelectionSvc.TraverseStatementsContains(myTraverse.Accept, "xxx");
                myTraverse.AssertAndReset(env.Statement("b"));

                env.UndeployAll();
            }
        }

        private class ClientRuntimeWrongCompileMethod : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window SomeWindow#keepall as SupportBean", path);

                var compiledFAF = env.CompileFAF("select * from SomeWindow", path);
                var compiledModule = env.Compile("select * from SomeWindow", path);

                var msgInvalidDeployFAF =
                    "Cannot deploy EPL that was compiled as a fire-and-forget query, make sure to use the 'compile' method of the compiler";
                try {
                    env.Runtime.DeploymentService.Deploy(compiledFAF);
                    Assert.Fail();
                }
                catch (EPDeployException ex) {
                    Assert.AreEqual(msgInvalidDeployFAF, ex.Message);
                }

                try {
                    env.Runtime.DeploymentService.Rollout(Collections.SingletonList(new EPDeploymentRolloutCompiled(compiledFAF)));
                    Assert.Fail();
                }
                catch (EPDeployException ex) {
                    Assert.AreEqual(msgInvalidDeployFAF, ex.Message);
                }

                try {
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiledModule);
                    Assert.Fail();
                }
                catch (EPException ex) {
                    Assert.AreEqual(
                        "Cannot execute a fire-and-forget query that was compiled as module EPL, make sure to use the 'compileQuery' method of the compiler",
                        ex.Message);
                }

                env.UndeployAll();
            }
        }

        private class ClientRuntimeSPICompileReflective : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@public create window MyWindow#keepall as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n");
                env.SendEventBean(new SupportBean("E1", 10));

                var spi = (EPRuntimeSPI) env.Runtime;
                var svc = spi.ReflectiveCompileSvc;
                Assert.IsTrue(svc.IsCompilerAvailable);

                var compiledFAF = svc.ReflectiveCompileFireAndForget("select * from MyWindow");
                var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiledFAF);
                EPAssertionUtil.AssertPropsPerRow(
                    result.GetEnumerator(),
                    new string[] {"TheString"},
                    new object[][] {
                        new object[] {"E1"}
                    });

                var compiledFromEPL = svc.ReflectiveCompile("@Name('s0') select * from MyWindow");
                env.Deploy(compiledFromEPL);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new string[] {"TheString"},
                    new object[][] {
                        new object[] {"E1"}
                    });

                var module = new Module();
                module.Items.Add(new ModuleItem("@Name('s1') select * from MyWindow"));
                var compiledFromModule = svc.ReflectiveCompile(module);
                env.Deploy(compiledFromModule);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s1"),
                    new string[] {"TheString"},
                    new object[][] {
                        new object[] {"E1"}
                    });

                var node = svc.ReflectiveCompileExpression("1*1", null, null);
                Assert.AreEqual(1, node.Forge.ExprEvaluator.Evaluate(null, true, null));

                var model = spi.ReflectiveCompileSvc.ReflectiveEPLToModel("select * from MyWindow");
                Assert.IsNotNull(model);

                var moduleParsed = spi.ReflectiveCompileSvc.ReflectiveParseModule("select * from MyWindow");
                Assert.AreEqual(1, moduleParsed.Items.Count);
                Assert.AreEqual("select * from MyWindow", moduleParsed.Items[0].Expression);

                env.UndeployAll();
            }
        }

        private class ClientRuntimeItselfTransientConfiguration : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') select * from SupportBean");
                var listener = new MyListener();
                env.Statement("s0").AddListener(listener);

                env.SendEventBean(new SupportBean());
                Assert.AreEqual(TEST_SECRET_VALUE, listener.SecretValue);

                env.UndeployAll();
            }
        }

        public class MyLocalService
        {
            public int SecretValue { get; }

            public MyLocalService(int secretValue)
            {
                SecretValue = secretValue;
            }
        }

        public class MyListener : UpdateListener
        {
            public int SecretValue { get; private set; }

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                var svc = (MyLocalService) eventArgs.Runtime.ConfigurationTransient.Get(TEST_SERVICE_NAME);
                SecretValue = svc.SecretValue;
            }
        }

        private class MyStatementTraverse
        {
            public IList<EPStatement> Statements { get; } = new List<EPStatement>();

            public void Accept(
                EPDeployment epDeployment,
                EPStatement epStatement)
            {
                Statements.Add(epStatement);
            }

            public void AssertAndReset(params EPStatement[] expected)
            {
                EPAssertionUtil.AssertEqualsExactOrder(Statements.ToArray(), expected);
                Statements.Clear();
            }
        }

        public class MyBeanAnonymousType
        {
            public int Prop { get; }
        }
    }
} // end of namespace