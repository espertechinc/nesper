///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientCompileWConfig
    {
        private void TryInvalidCompileConfigureAggFunc(
            string funcName,
            string className,
            string message)
        {
            Consumer<Configuration> configurer = config => {
                config.Compiler.AddPlugInAggregationFunctionForge(funcName, className);
            };
            TryInvalidConfigurationCompiler(SupportConfigFactory.GetConfiguration(), configurer, message);
        }

        private void TryInvalidPlugInSingleRow(
            string funcName,
            string className,
            string methodName,
            string expected)
        {
            Consumer<Configuration> configurer = config => {
                config.Compiler.AddPlugInSingleRowFunction(funcName, className, methodName);
            };
            TryInvalidConfigurationCompiler(SupportConfigFactory.GetConfiguration(), configurer, expected);
        }

        private class MyClassForNameProvider : ClassForNameProvider
        {
            public Type ClassForName(string className)
            {
                return ClassForNameProviderDefault.INSTANCE.ClassForName(className);
            }
        }

        private void TryInvalidCompileWConfig(
            Configuration config,
            string epl,
            string expected)
        {
            try {
                EPCompilerProvider.Compiler.Compile(epl, new CompilerArguments(config));
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                AssertMessage(ex.Message, expected);
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileClassForNameProvider()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.Common.AddEventType(typeof(SupportBean));
            config.Common.TransientConfiguration.Put(ClassForNameProviderConstants.NAME, new MyClassForNameProvider());

            var epl = "select System.Environment.Exit(-1) from SupportBean";
            TryInvalidCompileWConfig(
                config,
                epl,
                "Failed to validate select-clause expression 'System.Environment.Exit(-1)': Failed to resolve 'System.Environment.Exit' to");

            config.Common.TransientConfiguration.Put(
                ClassForNameProviderConstants.NAME,
                ClassForNameProviderDefault.INSTANCE);
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileInvalidConfigAggFuncs()
        {
            TryInvalidCompileConfigureAggFunc(
                "a b",
                "MyClass",
                "Failed compiler startup: Error configuring compiler: Invalid aggregation function name 'a b'");
            TryInvalidCompileConfigureAggFunc(
                "abc",
                "My Class",
                "Failed compiler startup: Error configuring compiler: Invalid class name for aggregation function forge 'My Class'");

            Consumer<Configuration> configurer = config => {
                config.Compiler.AddPlugInAggregationFunctionForge(
                    "abc",
                    typeof(SupportConcatWCodegenAggregationFunctionForge));
                config.Compiler.AddPlugInAggregationFunctionForge(
                    "abc",
                    typeof(SupportConcatWCodegenAggregationFunctionForge));
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Aggregation function by name 'abc' is already defined");
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileInvalidConfigAggMultiFunc()
        {
            Consumer<Configuration> configurer;

            configurer = config => {
                config.Compiler.AddPlugInAggregationFunctionForge(
                    "abc",
                    typeof(SupportConcatWCodegenAggregationFunctionForge));
                var func = new ConfigurationCompilerPlugInAggregationMultiFunction(
                    new [] { "abc" },
                    typeof(SupportAggMFMultiRTForge));
                config.Compiler.AddPlugInAggregationMultiFunction(func);
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Aggregation function by name 'abc' is already defined");

            configurer = config => {
                var funcOne = new ConfigurationCompilerPlugInAggregationMultiFunction(
                    new [] { "abc","def" },
                    typeof(SupportAggMFMultiRTForge));
                config.Compiler.AddPlugInAggregationMultiFunction(funcOne);
                var funcTwo = new ConfigurationCompilerPlugInAggregationMultiFunction(
                    new [] { "def","xyz" },
                    typeof(SupportAggMFMultiRTForge));
                config.Compiler.AddPlugInAggregationMultiFunction(funcTwo);
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Aggregation multi-function by name 'def' is already defined");

            configurer = config => {
                var configTwo =
                    new ConfigurationCompilerPlugInAggregationMultiFunction(new [] { "thefunction2" }, "x y z");
                config.Compiler.AddPlugInAggregationMultiFunction(configTwo);
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Invalid class name for aggregation multi-function factory 'x y z'");
        }

        [Test, RunInApplicationDomain]
        public void TestClientCompileInvalidSingleRowFunc()
        {
            TryInvalidPlugInSingleRow(
                "a b",
                "MyClass",
                "some",
                "Failed compiler startup: Error configuring compiler: Invalid single-row name 'a b'");
            TryInvalidPlugInSingleRow(
                "abc",
                "My Class",
                "other s",
                "Failed compiler startup: Error configuring compiler: Invalid class name for aggregation 'My Class'");

            Consumer<Configuration> configurer = config => {
                config.Compiler.AddPlugInSingleRowFunction(
                    "concatstring",
                    typeof(SupportSingleRowFunction),
                    "xyz");
                config.Compiler.AddPlugInAggregationFunctionForge(
                    "concatstring",
                    typeof(SupportConcatWCodegenAggregationFunctionForge));
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Aggregation function by name 'concatstring' is already defined");

            configurer = config => {
                config.Compiler.AddPlugInAggregationFunctionForge(
                    "teststring",
                    typeof(SupportConcatWCodegenAggregationFunctionForge));
                config.Compiler.AddPlugInSingleRowFunction("teststring", typeof(SupportSingleRowFunction), "xyz");
            };
            TryInvalidConfigurationCompiler(
                SupportConfigFactory.GetConfiguration(),
                configurer,
                "Failed compiler startup: Error configuring compiler: Aggregation function by name 'teststring' is already defined");
        }
    }
} // end of namespace