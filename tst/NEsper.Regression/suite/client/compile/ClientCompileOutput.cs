///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.compiler.@internal.util.CompilerVersion;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileOutput
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
#if REGRESSION_EXECUTIONS
            With(ManifestSimple)(execs);
#endif
            return execs;
        }

        public static IList<RegressionExecution> WithManifestSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientCompileOutputManifestSimple());
            return execs;
        }

        private class ClientCompileOutputManifestSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("select * from SupportBean");

                var manifest = compiled.Manifest;
                ClassicAssert.AreEqual(COMPILER_VERSION, manifest.CompilerVersion);
                ClassicAssert.IsNotNull(manifest.ModuleProviderClassName);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.COMPILEROPS);
            }
        }
    }
} // end of namespace