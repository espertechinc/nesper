///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientExtension
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_A),
                typeof(SupportBean_S0),
                typeof(SupportMarketDataBean),
                typeof(SupportSimpleBeanOne),
                typeof(SupportBean_ST0),
                typeof(SupportBeanRange)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            IDictionary<string, object> mapType = new Dictionary<string, object>();
            mapType.Put("col1", "string");
            mapType.Put("col2", "string");
            mapType.Put("col3", "int");
            configuration.Common.AddEventType("MapType", mapType);

            var configurationCompiler = configuration.Compiler;
            configurationCompiler.AddPlugInSingleRowFunction(
                "singlerow",
                typeof(SupportSingleRowFunctionTwo),
                "TestSingleRow");
            configurationCompiler.AddPlugInSingleRowFunction(
                "power3",
                typeof(SupportSingleRowFunction),
                "ComputePower3");
            configurationCompiler.AddPlugInSingleRowFunction(
                "chainTop",
                typeof(SupportSingleRowFunction),
                "GetChainTop");
            configurationCompiler.AddPlugInSingleRowFunction(
                "throwExceptionLogMe",
                typeof(SupportSingleRowFunction),
                "Throwexception",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED,
                false);
            configurationCompiler.AddPlugInSingleRowFunction(
                "throwExceptionRethrow",
                typeof(SupportSingleRowFunction),
                "Throwexception",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED,
                true);
            configurationCompiler.AddPlugInSingleRowFunction(
                "power3Rethrow",
                typeof(SupportSingleRowFunction),
                "ComputePower3",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED,
                true);
            configurationCompiler.AddPlugInSingleRowFunction(
                "power3Context",
                typeof(SupportSingleRowFunction),
                "ComputePower3WithContext",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED,
                true);
            foreach (var method in Collections.List(
                "Surroundx",
                "IsNullValue",
                "GetValueAsString",
                "EventsCheckStrings",
                "VarargsOnlyInt",
                "VarargsOnlyString",
                "VarargsOnlyObject",
                "VarargsOnlyNumber",
                "VarargsOnlyISupportBaseAB",
                "VarargsW1Param",
                "VarargsW2Param",
                "VarargsOnlyWCtx",
                "VarargsW1ParamWCtx",
                "VarargsW2ParamWCtx",
                "VarargsObjectsWCtx",
                "VarargsW1ParamObjectsWCtx",
                "VarargsOnlyBoxedFloat",
                "VarargsOnlyBoxedShort",
                "VarargsOnlyBoxedByte",
                "VarargOverload")
            ) {
                configurationCompiler.AddPlugInSingleRowFunction(method, typeof(SupportSingleRowFunction), method);
            }

            AddEventTypeUDF("myItemProducerEventBeanArray", "MyItem", "myItemProducerEventBeanArray", configuration);
            AddEventTypeUDF(
                "myItemProducerEventBeanCollection",
                "MyItem",
                "myItemProducerEventBeanCollection",
                configuration);
            AddEventTypeUDF("myItemProducerInvalidNoType", null, "myItemProducerEventBeanArray", configuration);
            AddEventTypeUDF("myItemProducerInvalidWrongType", "dummy", "myItemProducerEventBeanArray", configuration);

            configurationCompiler.AddPlugInAggregationFunctionForge(
                "concatstring",
                typeof(SupportConcatWManagedAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge(
                "myagg",
                typeof(SupportSupportBeanAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge(
                "countback",
                typeof(SupportCountBackAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge(
                "countboundary",
                typeof(SupportLowerUpperCompareAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge(
                "concatWCodegen",
                typeof(SupportConcatWCodegenAggregationFunctionForge));
            configurationCompiler.AddPlugInAggregationFunctionForge("invalidAggFuncForge", typeof(string));
            configurationCompiler.AddPlugInAggregationFunctionForge("nonExistAggFuncForge", "com.NoSuchClass");

            var configGeneral = new ConfigurationCompilerPlugInAggregationMultiFunction(
                "ss,sa,sc,se1,se2,ee".SplitCsv(),
                typeof(SupportAggMFMultiRTForge));
            configurationCompiler.AddPlugInAggregationMultiFunction(configGeneral);
            var codegenTestAccum = new ConfigurationCompilerPlugInAggregationMultiFunction(
                "collectEvents".SplitCsv(),
                typeof(SupportAggMFEventsAsListForge));
            configurationCompiler.AddPlugInAggregationMultiFunction(codegenTestAccum);

            configuration.Compiler.AddPlugInView("mynamespace", "flushedsimple", typeof(MyFlushedSimpleViewForge));
            configuration.Compiler.AddPlugInView("mynamespace", "invalid", typeof(string));
            configuration.Compiler.AddPlugInView("mynamespace", "trendspotter", typeof(MyTrendSpotterViewForge));

            configurationCompiler.AddPlugInVirtualDataWindow("test", "vdwnoparam", typeof(SupportVirtualDWForge));
            configurationCompiler.AddPlugInVirtualDataWindow(
                "test",
                "vdwwithparam",
                typeof(SupportVirtualDWForge),
                SupportVirtualDW.ITERATE); // configure with iteration
            configurationCompiler.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWForge));
            configurationCompiler.AddPlugInVirtualDataWindow("invalid", "invalid", typeof(SupportBean));
            configurationCompiler.AddPlugInVirtualDataWindow(
                "test",
                "testnoindex",
                typeof(SupportVirtualDWInvalidForge));
            configurationCompiler.AddPlugInVirtualDataWindow(
                "test",
                "exceptionvdw",
                typeof(SupportVirtualDWExceptionForge));

            configurationCompiler.AddPlugInPatternGuard(
                "myplugin",
                "count_to",
                typeof(MyCountToPatternGuardForge));
            configurationCompiler.AddPlugInPatternGuard("namespace", "name", typeof(string));

            configuration.Common.AddImportType(typeof(ClientExtendSingleRowFunction));

            configuration.Runtime.Threading.IsRuntimeFairlock = true;
            configuration.Common.Logging.IsEnableQueryPlan = true;
        }

        private static void AddEventTypeUDF(
            string name,
            string eventTypeName,
            string functionMethodName,
            Configuration configuration)
        {
            var entry = new ConfigurationCompilerPlugInSingleRowFunction();
            entry.Name = name;
            entry.FunctionClassName = typeof(ClientExtendUDFReturnTypeIsEvents).FullName;
            entry.FunctionMethodName = functionMethodName;
            entry.EventTypeName = eventTypeName;
            configuration.Compiler.AddPlugInSingleRowFunction(entry);
        }

        [Test]
        public void TestClientExtendAdapterLoaderLoad()
        {
            var session = RegressionRunner.Session();

            var props = new Properties();
            props.Put("name", "val");
            session.Configuration.Runtime.AddPluginLoader("MyLoader", typeof(SupportPluginLoader), props);

            props = new Properties();
            props.Put("name2", "val2");
            session.Configuration.Runtime.AddPluginLoader("MyLoader2", typeof(SupportPluginLoader), props);

            RegressionRunner.Run(session, new ClientExtendAdapterLoader());

            session.Destroy();
        }

        [Test]
        public void TestClientExtendAggregationFunction()
        {
            RegressionRunner.Run(session, new ClientExtendAggregationFunction());
        }

        [Test]
        public void TestClientExtendAggregationMultiFunction()
        {
            RegressionRunner.Run(session, new ClientExtendAggregationMultiFunction());
        }

        [Test]
        public void TestClientExtendPatternGuard()
        {
            RegressionRunner.Run(session, new ClientExtendPatternGuard());
        }

        [Test]
        public void TestClientExtendSingleRowFunction()
        {
            RegressionRunner.Run(session, ClientExtendSingleRowFunction.Executions());
        }

        [Test]
        public void TestClientExtendUDFReturnTypeIsEvents()
        {
            RegressionRunner.Run(session, new ClientExtendUDFReturnTypeIsEvents());
        }

        [Test]
        public void TestClientExtendUDFVarargs()
        {
            RegressionRunner.Run(session, new ClientExtendUDFVarargs());
        }

        [Test]
        public void TestClientExtendView()
        {
            RegressionRunner.Run(session, new ClientExtendView());
        }

        [Test]
        public void TestClientExtendVirtualDataWindow()
        {
            RegressionRunner.Run(session, new ClientExtendVirtualDataWindow());
        }
    }
} // end of namespace