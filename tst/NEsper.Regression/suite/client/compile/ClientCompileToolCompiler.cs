///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileToolCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ClientCompileToolCompiler));

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            Withc(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withc(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileToolCompilerBasic());
            return execs;
        }

        private class ClientCompileToolCompilerBasic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
#if BROKEN
				var compiler = ToolProvider.SystemJavaCompiler;
				if (compiler == null) {
					Log.Info("Tools compiler is not in classpath");
					return;
				}

				var epl = "select * from SupportBean";
				var args = new CompilerArguments(env.Configuration);
				args.Options.CompilerHook = ctx => new CompilerAbstractionToolProvider(compiler);
				try {
					env.Compiler.Compile(epl, args);
				}
				catch (EPCompileException e) {
					throw new EPRuntimeException(e);
				}
#endif
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }
    }
} // end of namespace