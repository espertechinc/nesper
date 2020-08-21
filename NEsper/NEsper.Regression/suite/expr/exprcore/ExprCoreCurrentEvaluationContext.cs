///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
	public class ExprCoreCurrentEvaluationContext {
	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new ExprCoreCurrentEvalCtx(false));
	        execs.Add(new ExprCoreCurrentEvalCtx(true));
	        return execs;
	    }

	    private class ExprCoreCurrentEvalCtx : RegressionExecution {
	        private readonly bool soda;

	        public ExprCoreCurrentEvalCtx(bool soda) {
	            this.soda = soda;
	        }

	        public void Run(RegressionEnvironment env) {
	            SendTimer(env, 0);

	            var epl = "@Name('s0') select " +
	                      "current_evaluation_context() as c0, " +
	                      "current_evaluation_context(), " +
	                      "current_evaluation_context().getRuntimeURI() as c2 from SupportBean";
	            var resolver = new StatementUserObjectOption(_ => "my_user_object");
	            var arguments = new CompilerArguments(new Configuration());
	            arguments.Options.StatementUserObject = resolver;
	            var compiled = env.Compile(soda, epl, arguments);
	            env.Deploy(compiled).AddListener("s0").Milestone(0);
	            Assert.AreEqual(typeof(EPLExpressionEvaluationContext), env.Statement("s0").EventType.GetPropertyType("current_evaluation_context()"));

	            env.SendEventBean(new SupportBean());
	            var @event = env.Listener("s0").AssertOneGetNewAndReset();
	            var ctx = (EPLExpressionEvaluationContext) @event.Get("c0");
	            Assert.AreEqual(env.RuntimeURI, ctx.RuntimeURI);
	            Assert.AreEqual(env.Statement("s0").Name, ctx.StatementName);
	            Assert.AreEqual(-1, ctx.ContextPartitionId);
	            Assert.AreEqual("my_user_object", ctx.StatementUserObject);
	            Assert.AreEqual(env.RuntimeURI, @event.Get("c2"));

	            env.UndeployAll();
	        }
	    }

	    private static void SendTimer(RegressionEnvironment env, long timeInMSec) {
	        env.AdvanceTime(timeInMSec);
	    }
	}
} // end of namespace
