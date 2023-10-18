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
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
	public class ClientRuntimeItself {
	    public const string TEST_SERVICE_NAME = "TEST_SERVICE_NAME";
	    public const int TEST_SECRET_VALUE = 12345;

	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientRuntimeItselfTransientConfiguration());
	        execs.Add(new ClientRuntimeSPICompileReflective());
	        execs.Add(new ClientRuntimeSPIStatementSelection());
	        execs.Add(new ClientRuntimeSPIBeanAnonymousType());
	        execs.Add(new ClientRuntimeWrongCompileMethod());
	        return execs;
	    }

	    private class ClientRuntimeSPIBeanAnonymousType : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var beanEventType = new EPRuntimeBeanAnonymousTypeService(env.Container)
		            .MakeBeanEventTypeAnonymous(typeof(MyBeanAnonymousType));
	            Assert.AreEqual(typeof(int), beanEventType.GetPropertyType("prop"));
	        }
	    }

	    private class ClientRuntimeSPIStatementSelection : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy(
	                "@name('a') select * from SupportBean;\n" +
	                    "@name('b') select * from SupportBean(theString='xxx');\n");
	            var spi = (EPRuntimeSPI) env.Runtime;

	            var myTraverse = new MyStatementTraverse();
	            spi.TraverseStatements(myTraverse.Accept);
	            myTraverse.AssertAndReset(env.Statement("a"), env.Statement("b"));

	            var filter = spi.StatementSelectionSvc.CompileFilterExpression("name='b'");
	            spi.StatementSelectionSvc.TraverseStatementsFilterExpr(myTraverse.Accept, filter);
	            myTraverse.AssertAndReset(env.Statement("b"));
	            spi.StatementSelectionSvc.CompileFilterExpression("deploymentId like 'x'");

	            spi.StatementSelectionSvc.TraverseStatementsContains(myTraverse.Accept, "xxx");
	            myTraverse.AssertAndReset(env.Statement("b"));

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.RUNTIMEOPS);
	        }
	    }

	    private class ClientRuntimeWrongCompileMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create window SomeWindow#keepall as SupportBean", path);

	            var compiledFAF = env.CompileFAF("select * from SomeWindow", path);
	            var compiledModule = env.Compile("select * from SomeWindow", path);

	            var msgInvalidDeployFAF = "Cannot deploy EPL that was compiled as a fire-and-forget query, make sure to use the 'compile' method of the compiler";
	            try {
	                env.Runtime.DeploymentService.Deploy(compiledFAF);
	                Assert.Fail();
	            } catch (EPDeployException ex) {
	                Assert.AreEqual(msgInvalidDeployFAF, ex.Message);
	            }
	            try {
	                env.Runtime.DeploymentService.Rollout(Collections.SingletonList(new EPDeploymentRolloutCompiled(compiledFAF)));
	                Assert.Fail();
	            } catch (EPDeployException ex) {
	                Assert.AreEqual(msgInvalidDeployFAF, ex.Message);
	            }

	            try {
	                env.Runtime.FireAndForgetService.ExecuteQuery(compiledModule);
	                Assert.Fail();
	            } catch (EPException ex) {
	                Assert.AreEqual("Cannot execute a fire-and-forget query that was compiled as module EPL, make sure to use the 'compileQuery' method of the compiler", ex.Message);
	            }

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.RUNTIMEOPS);
	        }
	    }

	    private class ClientRuntimeSPICompileReflective : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy(
	                "@public create window MyWindow#keepall as SupportBean;\n" +
	                    "insert into MyWindow select * from SupportBean;\n");
	            env.SendEventBean(new SupportBean("E1", 10));

	            var spi = (EPRuntimeSPI) env.Runtime;
	            var svc = spi.ReflectiveCompileSvc;
	            Assert.IsTrue(svc.IsCompilerAvailable);

	            var compiledFAF = svc.ReflectiveCompileFireAndForget("select * from MyWindow");
	            var result = env.Runtime.FireAndForgetService.ExecuteQuery(compiledFAF);
	            EPAssertionUtil.AssertPropsPerRow(result.GetEnumerator(), new string[]{"theString"}, new object[][]{new object[] {"E1"}});

	            var compiledFromEPL = svc.ReflectiveCompile("@name('s0') select * from MyWindow");
	            env.Deploy(compiledFromEPL);
	            env.AssertPropsPerRowIterator("s0", new string[]{"theString"}, new object[][]{new object[] {"E1"}});

	            var module = new Module();
	            module.Items.Add(new ModuleItem("@name('s1') select * from MyWindow"));
	            var compiledFromModule = svc.ReflectiveCompile(module);
	            env.Deploy(compiledFromModule);
	            env.AssertPropsPerRowIterator("s1", new string[]{"theString"}, new object[][]{new object[] {"E1"}});

	            var node = svc.ReflectiveCompileExpression("1*1", null, null);
	            Assert.AreEqual(1, node.Forge.ExprEvaluator.Evaluate(null, true, null));

	            var model = spi.ReflectiveCompileSvc.ReflectiveEPLToModel("select * from MyWindow");
	            Assert.IsNotNull(model);

	            var moduleParsed = spi.ReflectiveCompileSvc.ReflectiveParseModule("select * from MyWindow");
	            Assert.AreEqual(1, moduleParsed.Items.Count);
	            Assert.AreEqual("select * from MyWindow", moduleParsed.Items[0].Expression);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.RUNTIMEOPS);
	        }
	    }

	    private class ClientRuntimeItselfTransientConfiguration : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.CompileDeploy("@name('s0') select * from SupportBean");
	            var listener = new MyListener();
	            env.Statement("s0").AddListener(listener);

	            env.SendEventBean(new SupportBean());
	            Assert.AreEqual(TEST_SECRET_VALUE, listener.SecretValue);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.OBSERVEROPS);
	        }
	    }

	    public class MyLocalService {
		    public MyLocalService(int secretValue) {
	            this.SecretValue = secretValue;
	        }

	        internal int SecretValue { get; }
	    }

	    public class MyListener : UpdateListener {
	        private int secretValue;

	        public void Update(
		        object sender,
		        UpdateEventArgs eventArgs)
	        {
		        var runtime = eventArgs.Runtime;
	            var svc = (MyLocalService) runtime.ConfigurationTransient.Get(TEST_SERVICE_NAME);
	            secretValue = svc.SecretValue;
	        }

	        internal int SecretValue => secretValue;
	    }

	    private class MyStatementTraverse {
	        IList<EPStatement> statements = new List<EPStatement>();

	        public void Accept(EPDeployment epDeployment, EPStatement epStatement) {
	            statements.Add(epStatement);
	        }

	        public IList<EPStatement> Statements => statements;

	        public void AssertAndReset(params EPStatement[] expected) {
	            EPAssertionUtil.AssertEqualsExactOrder(statements.ToArray(), expected);
	            statements.Clear();
	        }
	    }

	    public class MyBeanAnonymousType {
	        private int prop;

	        public int GetProp() {
	            return prop;
	        }
	    }
	}
} // end of namespace
