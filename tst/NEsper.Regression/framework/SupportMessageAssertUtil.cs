///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.framework
{
    public class SupportMessageAssertUtil
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void TryInvalidFAFCompile(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string message)
        {
            try {
                CompileFAFInternal(env, path, epl);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex, message);
            }
        }

        public static void TryInvalidCompile(
            RegressionEnvironment env,
            string epl,
            string message)
        {
            try {
                env.CompileWCheckedEx(epl);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex, message);
            }
        }

        public static void TryInvalidCompile(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string message)
        {
            try {
                env.CompileWCheckedEx(epl, path);
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex, message);
            }
        }

        public static void AssertMessageContains(
            Exception ex,
            string message)
        {
            if (!ex.Message.Contains(message)) {
                Assert.Fail("Does not contain text: '" + message + "' in text \n text:" + ex.Message);
            }

            if (message.Trim().Length == 0) {
                Assert.Fail("empty expected message");
            }
        }

        public static void AssertMessage(
            Exception ex,
            string message)
        {
            if (message.Equals("skip")) {
                return; // skip message validation
            }

            var exceptionMessage = ex.Message;
            if (exceptionMessage.StartsWith("Error during compilation: ")) {
                message = "Error during compilation: " + message;
            }
            
            try {
                StringAssert.StartsWith(message, exceptionMessage);
            }
            catch {
                for (;ex != null; ex = ex.InnerException) {
                    Console.WriteLine();
                    Console.WriteLine("Exception: " + ex.GetType().FullName);
                    Console.WriteLine("Message: " + exceptionMessage);
                    Console.WriteLine("StackTrace:");
                    Console.WriteLine(ex.StackTrace);
                }

                throw;
            }

            //Assert.That(ex.Message, Does.StartWith(message));

#if DEPRECATED
            if (message.Length > 10) {
                if (!ex.Message.StartsWith(message)) {
                    log.Error("Expected:" + message + "\nReceived:" + ex.Message, ex);
                    Assert.Fail("\nExpected:" + message + "\nReceived:" + ex.Message);
                }
            }
            else {
                // Comment-in for logging: log.error("Exception: " + ex.getMessage(), ex);
                log.Error("No assertion provided, received: " + ex.Message, ex);
                Assert.Fail("No assertion provided, received: " + ex.Message);
            }
#endif
        }

        public static void AssertMessage(
            string exceptionMessage,
            string expected)
        {
            if (expected.Equals("skip")) {
                return; // skip message validation
            }

            Assert.That(exceptionMessage, Does.StartWith(exceptionMessage));

#if DEPRECATED
            if (expected.Length > 10) {
                // Comment-in for logging: log.error("Exception: " + ex.getMessage(), ex);
                if (!exceptionMessage.StartsWith(expected)) {
                    Console.Out.WriteLine(exceptionMessage);
                    Assert.Fail("\nExpected:" + expected + "\nReceived:" + exceptionMessage);
                }
            }
            else {
                // Comment-in for logging: log.error("Exception: " + ex.getMessage(), ex);
                Console.Out.WriteLine(exceptionMessage);
                Assert.Fail("No assertion provided, received: " + exceptionMessage);
            }
#endif
        }

        public static void TryInvalidIterate(
            RegressionEnvironment env,
            string epl,
            string message)
        {
            env.CompileDeploy(epl);
            try {
                env.Statement("s0").GetEnumerator();
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex) {
                AssertMessage(ex, message);
            }

            env.UndeployAll();
        }

        public static void TryInvalidDeploy(
            RegressionEnvironment env,
            EPCompiled unit,
            string expected)
        {
            try {
                env.Runtime.DeploymentService.Deploy(unit);
                Assert.Fail();
            }
            catch (EPDeployException ex) {
                AssertMessage(ex, expected);
            }
        }

        public static void TryInvalidDeploy(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string expected)
        {
            var compiled = env.Compile(epl, path);
            try {
                env.Runtime.DeploymentService.Deploy(compiled);
                Assert.Fail();
            }
            catch (EPDeployException ex) {
                AssertMessage(ex, expected);
            }

            path.Compileds.Remove(compiled);
        }

        public static void TryInvalidProperty(
            EventBean @event,
            string propertyName)
        {
            try {
                @event.Get(propertyName);
                Assert.Fail();
            }
            catch (PropertyAccessException ex) {
                // expected
                AssertMessage(ex, "Property named '" + propertyName + "' is not a valid property name for this type");
            }
        }

        public static void TryInvalidGetFragment(
            EventBean @event,
            string propertyName)
        {
            try {
                @event.GetFragment(propertyName);
                Assert.Fail();
            }
            catch (PropertyAccessException ex) {
                // expected
                AssertMessage(ex, "Property named '" + propertyName + "' is not a valid property name for this type");
            }
        }

        public static void TryInvalidConfigurationCompiler(
            Configuration config,
            Consumer<Configuration> configurer,
            string expected)
        {
            config.Common.AddEventType(typeof(SupportBean));
            configurer.Invoke(config);

            try {
                EPCompilerProvider.Compiler.Compile("select * from SupportBean", new CompilerArguments(config));
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex, expected);
            }
        }

        public static void TryInvalidConfigurationRuntime(
            EPRuntimeProvider runtimeProvider,
            Configuration config,
            Consumer<Configuration> configurer,
            string expected)
        {
            config.Common.AddEventType(typeof(SupportBean));
            configurer.Invoke(config);

            try {
                runtimeProvider.GetRuntime(Guid.NewGuid().ToString(), config);
                Assert.Fail();
            }
            catch (ConfigurationException ex) {
                AssertMessage(ex, expected);
            }
        }

        public static void TryInvalidConfigurationCompileAndRuntime(
            EPRuntimeProvider runtimeProvider,
            Configuration configuration,
            Consumer<Configuration> configurer,
            string expected)
        {
            TryInvalidConfigurationCompiler(
                configuration,
                configurer,
                "Failed compiler startup: " + expected);
            TryInvalidConfigurationRuntime(
                runtimeProvider,
                configuration,
                configurer,
                "Failed runtime startup: " + expected);
        }

        private static void CompileFAFInternal(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            var args = new CompilerArguments(env.Configuration);
            args.Path.AddAll(path.Compileds);
            env.Compiler.CompileQuery(epl, args);
        }
    }
} // end of namespace