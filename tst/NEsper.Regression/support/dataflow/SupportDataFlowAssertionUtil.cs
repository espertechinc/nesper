///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class SupportDataFlowAssertionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void TryInvalidRun(
            RegressionEnvironment env,
            string epl,
            string name,
            string message)
        {
            env.CompileDeploy("@name('flow') " + epl);
            var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), name);

            try {
                df.Run();
                Assert.Fail();
            }
            catch (EPDataFlowExecutionException ex) {
                AssertException(message, ex.Message);
            }

            env.UndeployModuleContaining("flow");
        }

        public static void TryInvalidInstantiate(
            RegressionEnvironment env,
            string name,
            string epl,
            string message)
        {
            env.CompileDeploy("@name('flow') " + epl);

            try {
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), name);
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                Log.Info("Expected exception: " + ex.Message, ex);
                AssertException(message, ex.Message);
            }
            finally {
                env.UndeployModuleContaining("flow");
            }
        }

        private static void AssertException(
            string expected,
            string message)
        {
            string received;
            if (message.LastIndexOf("[") != -1) {
                received = message.Substring(0, message.LastIndexOf("[") + 1);
            }
            else {
                received = message;
            }

            if (message.StartsWith(expected)) {
                ClassicAssert.IsFalse(string.IsNullOrEmpty(expected.Trim()), "empty expected message, received:\n" + message);
                return;
            }

            Assert.Fail("Expected:\n" + expected + "\nbut received:\n" + received + "\n");
        }
    }
} // end of namespace