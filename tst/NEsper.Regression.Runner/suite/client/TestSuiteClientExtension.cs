///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.client.extension;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionlib.support.extend.pattern;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.extend.view;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientExtension : AbstractTestBase
    {
        public TestSuiteClientExtension() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[]
                     {
                         typeof(SupportBean),
                         typeof(SupportBean_A),
                         typeof(SupportBean_S0),
                         typeof(SupportMarketDataBean),
                         typeof(SupportSimpleBeanOne),
                         typeof(SupportBean_ST0),
                         typeof(SupportBeanRange),
                         typeof(SupportDateTime),
                         typeof(SupportCollection),
                         typeof(SupportBean_ST0_Container)
                     }

                    )
            {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            IDictionary<string, object> mapType = new Dictionary<string, object>();
            mapType.Put("col1", "string");
            mapType.Put("col2", "string");
            mapType.Put("col3", "int");
            configuration.Common.AddEventType("MapType", mapType);
            var configurationCompiler = configuration.Compiler;
            configurationCompiler.AddPlugInSingleRowFunction("singlerow", typeof(SupportSingleRowFunctionTwo),
                "TestSingleRow");
            configurationCompiler.AddPlugInSingleRowFunction("power3", typeof(SupportSingleRowFunction),
                "ComputePower3");
            configurationCompiler.AddPlugInSingleRowFunction("chainTop", typeof(SupportSingleRowFunction),
                "GetChainTop");
            configurationCompiler.AddPlugInSingleRowFunction("throwExceptionLogMe", typeof(SupportSingleRowFunction),
                "Throwexception", ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, false);
            configurationCompiler.AddPlugInSingleRowFunction("throwExceptionRethrow", typeof(SupportSingleRowFunction),
                "Throwexception", ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, true);
            configurationCompiler.AddPlugInSingleRowFunction("power3Rethrow", typeof(SupportSingleRowFunction),
                "ComputePower3", ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, true);
            configurationCompiler.AddPlugInSingleRowFunction("power3Context", typeof(SupportSingleRowFunction),
                "ComputePower3WithContext", ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED, true);
            foreach (var method in Collections.List("Surroundx", "IsNullValue", "GetValueAsString",
                         "EventsCheckStrings", "VarargsOnlyInt", "VarargsOnlyString", "VarargsOnlyObject",
                         "VarargsOnlyNumber", "VarargsOnlyISupportBaseAB", "VarargsW1Param", "VarargsW2Param",
                         "VarargsOnlyWCtx", "VarargsW1ParamWCtx", "VarargsW2ParamWCtx", "VarargsObjectsWCtx",
                         "VarargsW1ParamObjectsWCtx", "VarargsOnlyBoxedFloat", "VarargsOnlyBoxedShort",
                         "VarargsOnlyBoxedByte", "VarargOverload"))
            {
                configurationCompiler.AddPlugInSingleRowFunction(method, typeof(SupportSingleRowFunction), method);
            }

            configurationCompiler.AddPlugInSingleRowFunction("extractNum", typeof(ClientExtendEnumMethod),
                "ExtractNum");
            AddEventTypeUDF("MyItemProducerEventBeanArray", "MyItem", "MyItemProducerEventBeanArray", configuration);
            AddEventTypeUDF("MyItemProducerEventBeanCollection", "MyItem", "MyItemProducerEventBeanCollection",
                configuration);
            AddEventTypeUDF("MyItemProducerInvalidNoType", null, "MyItemProducerEventBeanArray", configuration);
            AddEventTypeUDF("MyItemProducerInvalidWrongType", "dummy", "MyItemProducerEventBeanArray", configuration);
            configurationCompiler.AddPlugInAggregationFunctionForge("concatstring",
                typeof(SupportConcatWManagedAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("myagg",
                typeof(SupportSupportBeanAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("countback",
                typeof(SupportCountBackAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("countboundary",
                typeof(SupportLowerUpperCompareAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("concatWCodegen",
                typeof(SupportConcatWCodegenAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("invalidAggFuncForge", typeof(TimeSpan));
            configurationCompiler.AddPlugInAggregationFunctionForge("nonExistAggFuncForge", "com.NoSuchClass");
            var configGeneral =
                new ConfigurationCompilerPlugInAggregationMultiFunction(new[] { "ss", "sa", "sc", "se1", "se2", "ee" },
                    typeof(SupportAggMFMultiRTForge));
            configGeneral.AdditionalConfiguredProperties = Collections.SingletonDataMap("someinfokey", "someinfovalue");
            configurationCompiler.AddPlugInAggregationMultiFunction(configGeneral);
            var codegenTestAccum = new ConfigurationCompilerPlugInAggregationMultiFunction(new[] { "collectEvents" },
                typeof(SupportAggMFEventsAsListForge));
            configurationCompiler.AddPlugInAggregationMultiFunction(codegenTestAccum);
            // For use with the inlined-class example when disabled, comment-in when needed:
            // ConfigurationCompilerPlugInAggregationMultiFunction codegenTestTrie = new ConfigurationCompilerPlugInAggregationMultiFunction("".Split(","), ClientExtendAggregationMultiFunctionInlinedClass.TrieAggForge.class.getName());
            // configurationCompiler.addPlugInAggregationMultiFunction(codegenTestTrie);
            configuration.Compiler.AddPlugInView("mynamespace", "flushedsimple", typeof(MyFlushedSimpleViewForge));
            configuration.Compiler.AddPlugInView("mynamespace", "invalid", typeof(string));
            configuration.Compiler.AddPlugInView("mynamespace", "trendspotter", typeof(MyTrendSpotterViewForge));
            configurationCompiler.AddPlugInVirtualDataWindow("test", "vdwnoparam", typeof(SupportVirtualDWForge));
            configurationCompiler.AddPlugInVirtualDataWindow("test", "vdwwithparam", typeof(SupportVirtualDWForge),
                SupportVirtualDW.ITERATE); // configure with iteration
            configurationCompiler.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWForge));
            configurationCompiler.AddPlugInVirtualDataWindow("invalid", "invalid", typeof(SupportBean));
            configurationCompiler.AddPlugInVirtualDataWindow("test", "testnoindex",
                typeof(SupportVirtualDWInvalidForge));
            configurationCompiler.AddPlugInVirtualDataWindow("test", "exceptionvdw",
                typeof(SupportVirtualDWExceptionForge));
            configurationCompiler.AddPlugInPatternGuard("myplugin", "count_to", typeof(MyCountToPatternGuardForge));
            configurationCompiler.AddPlugInPatternGuard("namespace", "name", typeof(string));
            configurationCompiler.AddPlugInDateTimeMethod("roll",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryRoll));
            configurationCompiler.AddPlugInDateTimeMethod("asArrayOfString",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryArrayOfString));
            configurationCompiler.AddPlugInDateTimeMethod("dtmInvalidMethodNotExists",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryInvalidMethodNotExists));
            configurationCompiler.AddPlugInDateTimeMethod("dtmInvalidNotProvided",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryInvalidNotProvided));
            configurationCompiler.AddPlugInDateTimeMethod("someDTMInvalidReformat",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryInvalidReformat));
            configurationCompiler.AddPlugInDateTimeMethod("someDTMInvalidNoOp",
                typeof(ClientExtendDateTimeMethod.MyLocalDTMForgeFactoryInvalidNoOp));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInMedian",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeMedian));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInOne",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeOne));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInEarlyExit",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeEarlyExit));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInReturnEvents",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgePredicateReturnEvents));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInReturnSingleEvent",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgePredicateReturnSingleEvent));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInTwoLambda",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeTwoLambda));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInLambdaEventWPredicateAndIndex",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeThree));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInLambdaScalarWPredicateAndIndex",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeThree));
            configurationCompiler.AddPlugInEnumMethod("enumPlugInLambdaScalarWStateAndValue",
                typeof(ClientExtendEnumMethod.MyLocalEnumMethodForgeStateWValue));
            configuration.Common.AddImportType(typeof(ClientExtendSingleRowFunction));
            configuration.Common.AddImportType(typeof(BigInteger));
            configuration.Runtime.Threading.IsRuntimeFairlock = true;
            configuration.Common.Logging.IsEnableQueryPlan = true;
        }

        private static void AddEventTypeUDF(string name, string eventTypeName, string functionMethodName,
            Configuration configuration)
        {
            var entry = new ConfigurationCompilerPlugInSingleRowFunction();
            entry.Name = name;
            entry.FunctionClassName = typeof(ClientExtendUDFReturnTypeIsEvents).FullName;
            entry.FunctionMethodName = functionMethodName;
            entry.EventTypeName = eventTypeName;
            configuration.Compiler.AddPlugInSingleRowFunction(entry);
        }

        [Test, RunInApplicationDomain]
        public void TestClientExtendUDFReturnTypeIsEvents()
        {
            RegressionRunner.Run(_session, new ClientExtendUDFReturnTypeIsEvents());
        }

        [Test, RunInApplicationDomain]
        public void TestClientExtendView()
        {
            RegressionRunner.Run(_session, new ClientExtendView());
        }

        [Test, RunInApplicationDomain]
        public void TestClientExtendVirtualDataWindow()
        {
            RegressionRunner.Run(_session, new ClientExtendVirtualDataWindow());
        }

        [Test, RunInApplicationDomain]
        public void TestClientExtendPatternGuard()
        {
            RegressionRunner.Run(_session, new ClientExtendPatternGuard());
        }

        [Test, RunInApplicationDomain]
        public void TestClientExtendAdapterLoaderLoad()
        {
            using RegressionSession session = RegressionRunner.Session(Container);
            Properties props = new Properties();
            props.Put("name", "val");
            session.Configuration.Runtime.AddPluginLoader("MyLoader", typeof(SupportPluginLoader), props);
            props = new Properties();
            props.Put("name2", "val2");
            session.Configuration.Runtime.AddPluginLoader("MyLoader2", typeof(SupportPluginLoader), props);
            RegressionRunner.Run(session, new ClientExtendAdapterLoader());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendAggregationFunction
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendAggregationFunction.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendAggregationFunction : AbstractTestBase
        {
            public TestClientExtendAggregationFunction() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTable() => RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithTable());

            [Test, RunInApplicationDomain]
            public void WithInvalidCannotResolve() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithInvalidCannotResolve());

            [Test, RunInApplicationDomain]
            public void WithInvalidUse() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithInvalidUse());

            [Test, RunInApplicationDomain]
            public void WithFailedValidation() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithFailedValidation());

            [Test, RunInApplicationDomain]
            public void WithCodegeneratedCount() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithCodegeneratedCount());

            [Test, RunInApplicationDomain]
            public void WithMultiParamSingleArray() => RegressionRunner.Run(_session,
                ClientExtendAggregationFunction.WithMultiParamSingleArray());

            [Test, RunInApplicationDomain]
            public void WithMultiParamNoParam() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithMultiParamNoParam());

            [Test, RunInApplicationDomain]
            public void WithMultiParamMulti() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithMultiParamMulti());

            [Test, RunInApplicationDomain]
            public void WithManagedMappedPropertyLookAlike() => RegressionRunner.Run(_session,
                ClientExtendAggregationFunction.WithManagedMappedPropertyLookAlike());

            [Test, RunInApplicationDomain]
            public void WithManagedDotMethod() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithManagedDotMethod());

            [Test, RunInApplicationDomain]
            public void WithManagedDistinctAndStarParam() => RegressionRunner.Run(_session,
                ClientExtendAggregationFunction.WithManagedDistinctAndStarParam());

            [Test, RunInApplicationDomain]
            public void WithManagedGrouped() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithManagedGrouped());

            [Test, RunInApplicationDomain]
            public void WithManagedWindow() =>
                RegressionRunner.Run(_session, ClientExtendAggregationFunction.WithManagedWindow());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendEnumMethod
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendEnumMethod.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendEnumMethod : AbstractTestBase
        {
            public TestClientExtendEnumMethod() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLambdaScalarStateAndValue() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithLambdaScalarStateAndValue());

            [Test, RunInApplicationDomain]
            public void WithLambdaScalarInputValueAndIndex() => RegressionRunner.Run(_session,
                ClientExtendEnumMethod.WithLambdaScalarInputValueAndIndex());

            [Test, RunInApplicationDomain]
            public void WithLambdaEventInputValueAndIndex() => RegressionRunner.Run(_session,
                ClientExtendEnumMethod.WithLambdaEventInputValueAndIndex());

            [Test, RunInApplicationDomain]
            public void WithTwoLambdaParameters() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithTwoLambdaParameters());

            [Test, RunInApplicationDomain]
            public void WithPredicateReturnSingleEvent() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithPredicateReturnSingleEvent());

            [Test, RunInApplicationDomain]
            public void WithPredicateReturnEvents() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithPredicateReturnEvents());

            [Test, RunInApplicationDomain]
            public void WithScalarEarlyExit() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithScalarEarlyExit());

            [Test, RunInApplicationDomain]
            public void WithScalarNoLambdaWithParams() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithScalarNoLambdaWithParams());

            [Test, RunInApplicationDomain]
            public void WithScalarLambdaMedian() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithScalarLambdaMedian());

            [Test, RunInApplicationDomain]
            public void WithEventLambdaMedian() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithEventLambdaMedian());

            [Test, RunInApplicationDomain]
            public void WithScalarNoParamMedian() =>
                RegressionRunner.Run(_session, ClientExtendEnumMethod.WithScalarNoParamMedian());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendDateTimeMethod
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendDateTimeMethod.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendDateTimeMethod : AbstractTestBase
        {
            public TestClientExtendDateTimeMethod() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ClientExtendDateTimeMethod.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithReformat() => RegressionRunner.Run(_session, ClientExtendDateTimeMethod.WithReformat());

            [Test, RunInApplicationDomain]
            public void WithTransform() => RegressionRunner.Run(_session, ClientExtendDateTimeMethod.WithTransform());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendSingleRowFunction
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendSingleRowFunction.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendSingleRowFunction : AbstractTestBase
        {
            public TestClientExtendSingleRowFunction() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFailedValidation() =>
                RegressionRunner.Run(_session, ClientExtendSingleRowFunction.WithFailedValidation());

            [Test, RunInApplicationDomain]
            public void WithSingleMethod() =>
                RegressionRunner.Run(_session, ClientExtendSingleRowFunction.WithSingleMethod());

            [Test, RunInApplicationDomain]
            public void WithChainMethod() =>
                RegressionRunner.Run(_session, ClientExtendSingleRowFunction.WithChainMethod());

            [Test, RunInApplicationDomain]
            public void WithPropertyOrSingleRowMethod() => RegressionRunner.Run(_session,
                ClientExtendSingleRowFunction.WithPropertyOrSingleRowMethod());

            [Test, RunInApplicationDomain]
            public void WithEventBeanFootprint() =>
                RegressionRunner.Run(_session, ClientExtendSingleRowFunction.WithEventBeanFootprint());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendUDFInlinedClass
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendUDFInlinedClass : AbstractTestBase
        {
            public TestClientExtendUDFInlinedClass() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInlinedWOptions() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithInlinedWOptions());

            [Test, RunInApplicationDomain]
            public void WithOverloaded() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithOverloaded());

            [Test, RunInApplicationDomain]
            public void WithCreateInlinedSameModule() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithCreateInlinedSameModule());

            [Test, RunInApplicationDomain]
            public void WithInlinedFAF() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithInlinedFAF());

            [Test, RunInApplicationDomain]
            public void WithInlinedInvalid() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithInlinedInvalid());

            [Test, RunInApplicationDomain]
            public void WithInlinedLocalClass() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithInlinedLocalClass());

            [Test]
            [RunInApplicationDomain]
            [Ignore("Test is broken due to the need for namespace isolation within an assembly")]
            public void WithCreateInlinedOtherModule() =>
                RegressionRunner.Run(_session, ClientExtendUDFInlinedClass.WithCreateInlinedOtherModule());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendAggregationInlinedClass
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendAggregationInlinedClass : AbstractTestBase
        {
            public TestClientExtendAggregationInlinedClass() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMultiModuleUses() =>
                RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.WithMultiModuleUses());

            [Test, RunInApplicationDomain]
            public void WithInvalid() =>
                RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithSameModule() =>
                RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.WithSameModule());

            [Test, RunInApplicationDomain]
            public void WithFAF() => RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.WithFAF());

            [Test, RunInApplicationDomain]
            public void WithLocalClass() =>
                RegressionRunner.Run(_session, ClientExtendAggregationInlinedClass.WithLocalClass());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendAggregationMultiFunction
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendAggregationMultiFunction : AbstractTestBase
        {
            public TestClientExtendAggregationMultiFunction() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWithTable() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithWithTable());

            [Test, RunInApplicationDomain]
            public void WithSameProviderGroupedReturnSingleEvent() => RegressionRunner.Run(_session,
                ClientExtendAggregationMultiFunction.WithSameProviderGroupedReturnSingleEvent());

            [Test, RunInApplicationDomain]
            public void WithCollEvent() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithCollEvent());

            [Test, RunInApplicationDomain]
            public void WithSingleEvent() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithSingleEvent());

            [Test, RunInApplicationDomain]
            public void WithScalarColl() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithScalarColl());

            [Test, RunInApplicationDomain]
            public void WithScalarArray() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithScalarArray());

            [Test, RunInApplicationDomain]
            public void WithScalarOnly() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithScalarOnly());

            [Test, RunInApplicationDomain]
            public void WithSimpleState() =>
                RegressionRunner.Run(_session, ClientExtendAggregationMultiFunction.WithSimpleState());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendUDFVarargs
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendUDFVarargs.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendUDFVarargs : AbstractTestBase
        {
            public TestClientExtendUDFVarargs() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCombinations() =>
                RegressionRunner.Run(_session, ClientExtendUDFVarargs.WithCombinations());

            [Test, RunInApplicationDomain]
            public void WithCollOfEvent() =>
                RegressionRunner.Run(_session, ClientExtendUDFVarargs.WithCollOfEvent());
        }

        /// <summary>
        /// Auto-test(s): ClientExtendAggregationMultiFunctionInlinedClass
        /// <code>
        /// RegressionRunner.Run(_session, ClientExtendAggregationMultiFunctionInlinedClass.Executions());
        /// </code>
        /// </summary>
        public class TestClientExtendAggregationMultiFunctionInlinedClass : AbstractTestBase
        {
            public TestClientExtendAggregationMultiFunctionInlinedClass() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOneModule() => RegressionRunner.Run(_session,
                ClientExtendAggregationMultiFunctionInlinedClass.WithnOneModule());

            [Test, RunInApplicationDomain]
            public void WithOtherModule() => RegressionRunner.Run(_session,
                ClientExtendAggregationMultiFunctionInlinedClass.WithOtherModule());
        }
    }
} // end of namespace
