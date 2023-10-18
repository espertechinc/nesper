///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework; // assertEquals

// fail

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendSingleRowFunction {

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new ClientExtendSRFEventBeanFootprint());
	        execs.Add(new ClientExtendSRFPropertyOrSingleRowMethod());
	        execs.Add(new ClientExtendSRFChainMethod());
	        execs.Add(new ClientExtendSRFSingleMethod());
	        execs.Add(new ClientExtendSRFFailedValidation());
	        return execs;
	    }

	    private class ClientExtendSRFEventBeanFootprint : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            // test select-clause
	            var fields = new string[]{"c0", "c1"};
	            var text = "@name('s0') select isNullValue(*, 'theString') as c0," +
	                       nameof(ClientExtendSingleRowFunction) + ".localIsNullValue(*, 'theString') as c1 from SupportBean";
	            env.CompileDeploy(text).AddListener("s0");

	            env.SendEventBean(new SupportBean("a", 1));
	            env.AssertPropsNew("s0", fields, new object[]{false, false});

	            env.SendEventBean(new SupportBean(null, 2));
	            env.AssertPropsNew("s0", fields, new object[]{true, true});
	            env.UndeployAll();

	            // test pattern
	            var textPattern = "@name('s0') select * from pattern [a=SupportBean -> b=SupportBean(theString=getValueAsString(a, 'theString'))]";
	            env.CompileDeploy(textPattern).AddListener("s0");
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertPropsNew("s0", "a.intPrimitive,b.intPrimitive".SplitCsv(), new object[]{1, 2});
	            env.UndeployAll();

	            // test filter
	            var textFilter = "@name('s0') select * from SupportBean('E1'=getValueAsString(*, 'theString'))";
	            env.CompileDeploy(textFilter).AddListener("s0");
	            env.SendEventBean(new SupportBean("E2", 1));
	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertListenerInvoked("s0");
	            env.UndeployAll();

	            // test "first"
	            var textAccessAgg = "@name('s0') select * from SupportBean#keepall having 'E2' = getValueAsString(last(*), 'theString')";
	            env.CompileDeploy(textAccessAgg).AddListener("s0");
	            env.SendEventBean(new SupportBean("E2", 1));
	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertListenerInvoked("s0");
	            env.UndeployAll();

	            // test "window"
	            var textWindowAgg = "@name('s0') select * from SupportBean#keepall having eventsCheckStrings(window(*), 'theString', 'E1')";
	            env.CompileDeploy(textWindowAgg).AddListener("s0");
	            env.SendEventBean(new SupportBean("E2", 1));
	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertListenerInvoked("s0");
	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendSRFPropertyOrSingleRowMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select surroundx('test') as val from SupportBean";
	            env.CompileDeploy(text).AddListener("s0");

	            var fields = new string[]{"val"};
	            env.SendEventBean(new SupportBean("a", 3));
	            env.AssertPropsNew("s0", fields, new object[]{"XtestX"});

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendSRFChainMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select chainTop().chainValue(12,intPrimitive) as val from SupportBean";

	            env.CompileDeploy(text).AddListener("s0");
	            TryAssertionChainMethod(env);

	            env.EplToModelCompileDeploy(text).AddListener("s0");
	            TryAssertionChainMethod(env);
	        }
	    }

	    private class ClientExtendSRFSingleMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "@name('s0') select power3(intPrimitive) as val from SupportBean";

	            env.CompileDeploy(text).AddListener("s0");
	            TryAssertionSingleMethod(env);

	            env.EplToModelCompileDeploy(text).AddListener("s0");
	            TryAssertionSingleMethod(env);

	            text = "@name('s0') select power3(2) as val from SupportBean";
	            env.CompileDeploy(text).AddListener("s0");
	            TryAssertionSingleMethod(env);

	            // test passing a context as well
	            text = "@name('s0') select power3Context(intPrimitive) as val from SupportBean";
	            var args = new CompilerArguments(env.Configuration);
	            args.Options.StatementUserObject = (env) => "my_user_object";
	            var compiled = env.Compile(text, args);
	            env.Deploy(compiled).AddListener("s0");

	            SupportSingleRowFunction.MethodInvocationContexts.Clear();
	            TryAssertionSingleMethod(env);
	            env.AssertThat(() => {
	                var context = SupportSingleRowFunction.MethodInvocationContexts[0];
	                Assert.AreEqual("s0", context.StatementName);
	                Assert.AreEqual(env.Runtime.URI, context.RuntimeURI);
	                Assert.AreEqual(-1, context.ContextPartitionId);
	                Assert.AreEqual("power3Context", context.FunctionName);
	                Assert.AreEqual("my_user_object", context.StatementUserObject);
	            });

	            env.UndeployAll();

	            // test exception behavior
	            // logged-only
	            env.CompileDeploy("@name('s0') select throwExceptionLogMe() from SupportBean").AddListener("s0");
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.UndeployAll();

	            // rethrow
	            env.CompileDeploy("@name('s0') select throwExceptionRethrow() from SupportBean").AddListener("s0");
	            env.AssertThat(() => {
	                try {
	                    env.SendEventBean(new SupportBean("E1", 1));
	                    Assert.Fail();
	                } catch (EPException ex) {
	                    Assert.AreEqual("java.lang.RuntimeException: Unexpected exception in statement 's0': Invocation exception when invoking method 'throwexception' of class '" + typeof(SupportSingleRowFunction).FullName + "' passing parameters [] for statement 's0': RuntimeException : This is a 'throwexception' generated exception", ex.Message);
	                    env.UndeployAll();
	                }
	            });
	            env.UndeployAll();

	            // NPE when boxed is null
	            env.CompileDeploy("@name('s0') select power3Rethrow(intBoxed) from SupportBean").AddListener("s0");
	            env.AssertThat(() => {
	                try {
	                    env.SendEventBean(new SupportBean("E1", 1));
	                    Assert.Fail();
	                } catch (EPException ex) {
	                    Assert.AreEqual("java.lang.RuntimeException: Unexpected exception in statement 's0': NullPointerException invoking method 'computePower3' of class '" + typeof(SupportSingleRowFunction).FullName + "' in parameter 0 passing parameters [null] for statement 's0': The method expects a primitive int value but received a null value", ex.Message);
	                }
	            });

	            env.UndeployAll();
	        }
	    }

	    private class ClientExtendSRFFailedValidation : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.TryInvalidCompile("select singlerow('a', 'b') from SupportBean",
	                "Failed to validate select-clause expression 'singlerow(\"a\",\"b\")': Could not find static method named 'testSingleRow' in class '" + typeof(SupportSingleRowFunctionTwo).FullName + "' with matching parameter number and expected parameter type(s) 'String, String' (nearest match found was 'testSingleRow' taking type(s) 'String, int')");
	        }
	    }

	    private static void TryAssertionChainMethod(RegressionEnvironment env) {
	        var fields = new string[]{"val"};
	        env.SendEventBean(new SupportBean("a", 3));
	        env.AssertPropsNew("s0", fields, new object[]{36});

	        env.UndeployAll();
	    }

	    private static void TryAssertionSingleMethod(RegressionEnvironment env) {
	        var fields = new string[]{"val"};
	        env.SendEventBean(new SupportBean("a", 2));
	        env.AssertPropsNew("s0", fields, new object[]{8});
	        env.UndeployAll();
	    }

	    public static bool LocalIsNullValue(EventBean @event, string propertyName) {
	        return @event.Get(propertyName) == null;
	    }
	}
} // end of namespace
