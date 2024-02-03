///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreCurrentEvaluationContext
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithEvalCtx(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithEvalCtx(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ExprCoreCurrentEvalCtx(false));
            execs.Add(new ExprCoreCurrentEvalCtx(true));
            return execs;
        }

        private class ExprCoreCurrentEvalCtx : RegressionExecution
        {
            private readonly bool soda;

            public ExprCoreCurrentEvalCtx(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                SendTimer(env, 0);

                var epl = "@name('s0') select " +
                          "current_evaluation_context() as c0, " +
                          "current_evaluation_context(), " +
                          "current_evaluation_context().GetRuntimeURI() as c2 from SupportBean";
                var arguments = new CompilerArguments(new Configuration());
                arguments.Options.SetStatementUserObject(
                    new SupportPortableCompileOptionStmtUserObject("my_user_object").GetValue);
                var compiled = env.Compile(soda, epl, arguments);
                env.Deploy(compiled).AddListener("s0").Milestone(0);
                env.AssertStmtType("s0", "current_evaluation_context()", typeof(EPLExpressionEvaluationContext));

                env.SendEventBean(new SupportBean());
                env.AssertEventNew(
                    "s0",
                    @event => {
                        var ctx = (EPLExpressionEvaluationContext)@event.Get("c0");
                        ClassicAssert.AreEqual(env.RuntimeURI, ctx.RuntimeURI);
                        ClassicAssert.AreEqual(env.Statement("s0").Name, ctx.StatementName);
                        ClassicAssert.AreEqual(-1, ctx.ContextPartitionId);
                        ClassicAssert.AreEqual("my_user_object", ctx.StatementUserObject);
                        ClassicAssert.AreEqual(env.RuntimeURI, @event.Get("c2"));
                    });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{soda={soda}}}";
            }
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }
    }
} // end of namespace