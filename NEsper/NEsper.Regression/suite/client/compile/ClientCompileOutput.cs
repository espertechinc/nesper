///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.compiler.@internal.util.CompilerVersion;

namespace com.espertech.esper.regressionlib.suite.client.compile
{
    public class ClientCompileOutput
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithManifestSimple(execs);
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
                Assert.AreEqual(COMPILER_VERSION, manifest.CompilerVersion);
                Assert.IsNotNull(manifest.ModuleProviderClassName);
            }
        }
    }
} // end of namespace