///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileStatementName
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(e)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> Withe(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileStatementNameResolve());
            return execs;
        }

        private class ClientCompileStatementNameResolve : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                MyStatementNameResolver.Contexts.Clear();
                var args = new CompilerArguments(env.Configuration);
                args.Options.StatementName = (new MyStatementNameResolver()).GetValue;
                var epl = "select * from SupportBean";
                var compiled = env.Compile(epl, args);

                var ctx = MyStatementNameResolver.Contexts[0];
                ClassicAssert.AreEqual(epl, ctx.EplSupplier.Invoke());
                ClassicAssert.AreEqual(null, ctx.StatementName);
                ClassicAssert.AreEqual(null, ctx.ModuleName);
                ClassicAssert.AreEqual(0, ctx.Annotations.Length);
                ClassicAssert.AreEqual(0, ctx.StatementNumber);

                env.Deploy(compiled);
                env.AssertStatement("hello", statement => ClassicAssert.AreEqual("hello", statement.Name));
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }

        private class MyStatementNameResolver
        {
            private static readonly IList<StatementNameContext> contexts = new List<StatementNameContext>();

            public static IList<StatementNameContext> Contexts => contexts;

            public string GetValue(StatementNameContext env)
            {
                contexts.Add(env);
                return "hello";
            }
        }
    }
} // end of namespace