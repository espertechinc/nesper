///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.dataflow;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.dataflow;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLDataflow
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
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddEventType(
                "MyOAEventType",
                new[] {"p0", "p1"},
                new object[] {typeof(string), typeof(int)});

            var legacy = new ConfigurationCommonEventTypeBean();
            configuration.Common.AddEventType("MyLegacyEvent", typeof(EPLDataflowOpBeaconSource.MyLegacyEvent), legacy);
            configuration.Common.AddEventType(
                "MyEventNoDefaultCtor",
                typeof(EPLDataflowOpBeaconSource.MyEventNoDefaultCtor));

            configuration.Compiler.AddPlugInSingleRowFunction(
                "generateTagId",
                typeof(EPLDataflowOpBeaconSource),
                "GenerateTagId");

            DefaultSupportGraphEventUtil.AddTypeConfiguration(configuration);

            configuration.Common.AddImportType(typeof(Randomizer));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportSourceOp));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportCaptureOp));
            configuration.Common.AddImportType(typeof(EPLDataflowCustomProperties.MyOperatorOneForge));
            configuration.Common.AddImportType(typeof(EPLDataflowCustomProperties.MyOperatorTwoForge));
            configuration.Common.AddImportType(typeof(EPLDataflowAPIOpLifecycle.SupportGraphSourceForge));
            configuration.Common.AddImportType(typeof(EPLDataflowAPIOpLifecycle.SupportOperatorForge));
            configuration.Common.AddImportType(typeof(EPLDataflowAPIExceptions.MyExceptionOpForge));
            configuration.Common.AddImportType(typeof(EPLDataflowAPIOpLifecycle.MyCaptureOutputPortOpForge));
            configuration.Common.AddImportType(typeof(EPLDataflowExampleRollingTopWords));
            configuration.Common.AddImportType(typeof(EPLDataflowInputOutputVariations.MyFactorialOp));
            configuration.Common.AddImportType(typeof(EPLDataflowInputOutputVariations.MyCustomOp));
            configuration.Common.AddImportType(typeof(EPLDataflowInvalidGraph.MyInvalidOpForge));
            configuration.Common.AddImportType(typeof(EPLDataflowInvalidGraph.MyTestOp));
            configuration.Common.AddImportType(typeof(EPLDataflowInvalidGraph.MySBInputOp));
            configuration.Common.AddImportType(typeof(EPLDataflowTypes.MySupportBeanOutputOp));
            configuration.Common.AddImportType(typeof(EPLDataflowTypes.MyMapOutputOp));
            configuration.Common.AddImportType(typeof(MyLineFeedSource));
            configuration.Common.AddImportType(typeof(EPLDataflowAPIInstantiationOptions.MyOpForge));
            configuration.Common.AddImportNamespace(typeof(MyObjectArrayGraphSource));
            configuration.Common.AddImportNamespace(typeof(MyTokenizerCounter));
            configuration.Common.AddImportType(typeof(SupportBean));
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowAPIConfigAndInstance()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIConfigAndInstance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowAPIExceptions()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIExceptions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowAPIStartCaptive()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIStartCaptive());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowAPIStatistics()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIStatistics());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowExampleRollingTopWords()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleRollingTopWords());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowExampleVwapFilterSelectJoin()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleVwapFilterSelectJoin());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowExampleWordCount()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleWordCount());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDataflowOpLogSink()
        {
            RegressionRunner.Run(session, new EPLDataflowOpLogSink());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowAPICreateStartStopDestroy
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowAPICreateStartStopDestroy.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowAPICreateStartStopDestroy : AbstractTestBase
        {
            public TestEPLDataflowAPICreateStartStopDestroy() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDeploymentAdmin() => RegressionRunner.Run(_session, EPLDataflowAPICreateStartStopDestroy.WithDeploymentAdmin());

            [Test, RunInApplicationDomain]
            public void WithCreateStartStop() => RegressionRunner.Run(_session, EPLDataflowAPICreateStartStopDestroy.WithCreateStartStop());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowAPIInstantiationOptions
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowAPIInstantiationOptions.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowAPIInstantiationOptions : AbstractTestBase
        {
            public TestEPLDataflowAPIInstantiationOptions() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOperatorInjectionCallback() => RegressionRunner.Run(_session, EPLDataflowAPIInstantiationOptions.WithOperatorInjectionCallback());

            [Test, RunInApplicationDomain]
            public void WithParameterInjectionCallback() => RegressionRunner.Run(_session, EPLDataflowAPIInstantiationOptions.WithParameterInjectionCallback());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowAPIOpLifecycle
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowAPIOpLifecycle.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowAPIOpLifecycle : AbstractTestBase
        {
            public TestEPLDataflowAPIOpLifecycle() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFlowGraphOperator() => RegressionRunner.Run(_session, EPLDataflowAPIOpLifecycle.WithFlowGraphOperator());

            [Test, RunInApplicationDomain]
            public void WithFlowGraphSource() => RegressionRunner.Run(_session, EPLDataflowAPIOpLifecycle.WithFlowGraphSource());

            [Test, RunInApplicationDomain]
            public void WithTypeEvent() => RegressionRunner.Run(_session, EPLDataflowAPIOpLifecycle.WithTypeEvent());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowAPIRunStartCancelJoin
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowAPIRunStartCancelJoin : AbstractTestBase
        {
            public TestEPLDataflowAPIRunStartCancelJoin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBlockingRunJoin() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithBlockingRunJoin());

            [Test, RunInApplicationDomain]
            public void WithFastCompleteNonBlocking() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithFastCompleteNonBlocking());

            [Test, RunInApplicationDomain]
            public void WithRunBlocking() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithRunBlocking());

            [Test, RunInApplicationDomain]
            public void WithFastCompleteBlocking() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithFastCompleteBlocking());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingJoinSingleRunnable() => RegressionRunner.Run(
                _session,
                EPLDataflowAPIRunStartCancelJoin.WithNonBlockingJoinSingleRunnable());

            [Test, RunInApplicationDomain]
            public void WithBlockingMultipleRunnable() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithBlockingMultipleRunnable());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingJoinMultipleRunnable() => RegressionRunner.Run(
                _session,
                EPLDataflowAPIRunStartCancelJoin.WithNonBlockingJoinMultipleRunnable());

            [Test, RunInApplicationDomain]
            public void WithInvalidJoinRun() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithInvalidJoinRun());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingCancel() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithNonBlockingCancel());

            [Test, RunInApplicationDomain]
            public void WithBlockingCancel() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithBlockingCancel());

            [Test, RunInApplicationDomain]
            public void WithBlockingException() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithBlockingException());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingException() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithNonBlockingException());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingJoinException() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithNonBlockingJoinException());

            [Test, RunInApplicationDomain]
            public void WithNonBlockingJoinCancel() => RegressionRunner.Run(_session, EPLDataflowAPIRunStartCancelJoin.WithNonBlockingJoinCancel());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowCustomProperties
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowCustomProperties.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowCustomProperties : AbstractTestBase
        {
            public TestEPLDataflowCustomProperties() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCustomProps() => RegressionRunner.Run(_session, EPLDataflowCustomProperties.WithCustomProps());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLDataflowCustomProperties.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowDocSamples
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowDocSamples.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowDocSamples : AbstractTestBase
        {
            public TestEPLDataflowDocSamples() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSODA() => RegressionRunner.Run(_session, EPLDataflowDocSamples.WithSODA());

            [Test, RunInApplicationDomain]
            public void WithDocSamplesRun() => RegressionRunner.Run(_session, EPLDataflowDocSamples.WithDocSamplesRun());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowInputOutputVariations
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowInputOutputVariations.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowInputOutputVariations : AbstractTestBase
        {
            public TestEPLDataflowInputOutputVariations() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithFactorial() => RegressionRunner.Run(_session, EPLDataflowInputOutputVariations.WithFactorial());

            [Test, RunInApplicationDomain]
            public void WithFanInOut() => RegressionRunner.Run(_session, EPLDataflowInputOutputVariations.WithFanInOut());

            [Test, RunInApplicationDomain]
            public void WithLargeNumOpsDataFlow() => RegressionRunner.Run(_session, EPLDataflowInputOutputVariations.WithLargeNumOpsDataFlow());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowInvalidGraph
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowInvalidGraph.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowInvalidGraph : AbstractTestBase
        {
            public TestEPLDataflowInvalidGraph() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInstantiate() => RegressionRunner.Run(_session, EPLDataflowInvalidGraph.WithInstantiate());

            [Test, RunInApplicationDomain]
            public void WithCompile() => RegressionRunner.Run(_session, EPLDataflowInvalidGraph.WithCompile());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpBeaconSource
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpBeaconSource.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpBeaconSource : AbstractTestBase
        {
            public TestEPLDataflowOpBeaconSource() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNoType() => RegressionRunner.Run(_session, EPLDataflowOpBeaconSource.WithNoType());

            [Test, RunInApplicationDomain]
            public void WithFields() => RegressionRunner.Run(_session, EPLDataflowOpBeaconSource.WithFields());

            [Test, RunInApplicationDomain]
            public void WithVariable() => RegressionRunner.Run(_session, EPLDataflowOpBeaconSource.WithVariable());

            [Test, RunInApplicationDomain]
            public void WithWithBeans() => RegressionRunner.Run(_session, EPLDataflowOpBeaconSource.WithWithBeans());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpEPStatementSource
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpEPStatementSource.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpEPStatementSource : AbstractTestBase
        {
            public TestEPLDataflowOpEPStatementSource() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLDataflowOpEPStatementSource.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithStatementFilter() => RegressionRunner.Run(_session, EPLDataflowOpEPStatementSource.WithStatementFilter());

            [Test, RunInApplicationDomain]
            public void WithStmtNameDynamic() => RegressionRunner.Run(_session, EPLDataflowOpEPStatementSource.WithStmtNameDynamic());

            [Test, RunInApplicationDomain]
            public void WithAllTypes() => RegressionRunner.Run(_session, EPLDataflowOpEPStatementSource.WithAllTypes());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpEventBusSink
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpEventBusSink.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpEventBusSink : AbstractTestBase
        {
            public TestEPLDataflowOpEventBusSink() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSendEventDynamicType() => RegressionRunner.Run(_session, EPLDataflowOpEventBusSink.WithSendEventDynamicType());

            [Test, RunInApplicationDomain]
            public void WithBeacon() => RegressionRunner.Run(_session, EPLDataflowOpEventBusSink.WithBeacon());

            [Test, RunInApplicationDomain]
            public void WithAllTypes() => RegressionRunner.Run(_session, EPLDataflowOpEventBusSink.WithAllTypes());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpEventBusSource
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpEventBusSource.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpEventBusSource : AbstractTestBase
        {
            public TestEPLDataflowOpEventBusSource() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSchemaObjectArray() => RegressionRunner.Run(_session, EPLDataflowOpEventBusSource.WithSchemaObjectArray());

            [Test, RunInApplicationDomain]
            public void WithAllTypes() => RegressionRunner.Run(_session, EPLDataflowOpEventBusSource.WithAllTypes());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpFilter
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpFilter.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpFilter : AbstractTestBase
        {
            public TestEPLDataflowOpFilter() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAllTypes() => RegressionRunner.Run(_session, EPLDataflowOpFilter.WithAllTypes());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLDataflowOpFilter.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowOpSelect
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowOpSelect.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowOpSelect : AbstractTestBase
        {
            public TestEPLDataflowOpSelect() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOuterJoinMultirow() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithOuterJoinMultirow());

            [Test, RunInApplicationDomain]
            public void WithSelectPerformance() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithSelectPerformance());

            [Test, RunInApplicationDomain]
            public void WithFromClauseJoinOrder() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithFromClauseJoinOrder());

            [Test, RunInApplicationDomain]
            public void WithTimeWindowTriggered() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithTimeWindowTriggered());

            [Test, RunInApplicationDomain]
            public void WithOutputRateLimit() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithOutputRateLimit());

            [Test, RunInApplicationDomain]
            public void WithIterateFinalMarker() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithIterateFinalMarker());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithDocSamples() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithDocSamples());

            [Test, RunInApplicationDomain]
            public void WithAllTypes() => RegressionRunner.Run(_session, EPLDataflowOpSelect.WithAllTypes());
        }

        /// <summary>
        /// Auto-test(s): EPLDataflowTypes
        /// <code>
        /// RegressionRunner.Run(_session, EPLDataflowTypes.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDataflowTypes : AbstractTestBase
        {
            public TestEPLDataflowTypes() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMapType() => RegressionRunner.Run(_session, EPLDataflowTypes.WithMapType());

            [Test, RunInApplicationDomain]
            public void WithBeanType() => RegressionRunner.Run(_session, EPLDataflowTypes.WithBeanType());
        }
    }
} // end of namespace