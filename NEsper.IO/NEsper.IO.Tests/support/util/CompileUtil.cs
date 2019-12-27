///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esperio.support.util
{
    public class CompileUtil
    {
        public static EPDeployment CompileDeploy(
            EPRuntime epService,
            string epl)
        {
            try {
                var args = new CompilerArguments(epService.ConfigurationDeepCopy);
                args.Path.Add(epService.RuntimePath);
                var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
                return epService.DeploymentService.Deploy(compiled);
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex);
            }
        }

        public static void TryInvalidCompileGraph(
            EPRuntime epService,
            string graph,
            string expected)
        {
            try {
                var args = new CompilerArguments(epService.ConfigurationDeepCopy);
                args.Path.Add(epService.RuntimePath);
                EPCompilerProvider.Compiler.Compile(graph, args);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                if (!ex.Message.StartsWith(expected)) {
                    Assert.AreEqual(expected, ex.Message);
                }
            }
        }


        public static void UndeployAll(EPRuntime runtime)
        {
            try {
                runtime.DeploymentService.UndeployAll();
            }
            catch (EPUndeployException e) {
                throw new EPRuntimeException(e);
            }
        }
    }
}