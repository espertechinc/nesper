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
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.epl.dataflow;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.dataflow;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.Runner;

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
            session.Destroy();
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
                new [] { "p0","p1" },
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

        [Test]
        public void TestEPLDataflowAPIConfigAndInstance()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIConfigAndInstance());
        }

        [Test]
        public void TestEPLDataflowAPICreateStartStopDestroy()
        {
            RegressionRunner.Run(session, EPLDataflowAPICreateStartStopDestroy.Executions());
        }

        [Test]
        public void TestEPLDataflowAPIExceptions()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIExceptions());
        }

        [Test]
        public void TestEPLDataflowAPIInstantiationOptions()
        {
            RegressionRunner.Run(session, EPLDataflowAPIInstantiationOptions.Executions());
        }

        [Test]
        public void TestEPLDataflowAPIOpLifecycle()
        {
            RegressionRunner.Run(session, EPLDataflowAPIOpLifecycle.Executions());
        }

        [Test]
        public void TestEPLDataflowAPIRunStartCancelJoin()
        {
            RegressionRunner.Run(session, EPLDataflowAPIRunStartCancelJoin.Executions());
        }

        [Test]
        public void TestEPLDataflowAPIStartCaptive()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIStartCaptive());
        }

        [Test]
        public void TestEPLDataflowAPIStatistics()
        {
            RegressionRunner.Run(session, new EPLDataflowAPIStatistics());
        }

        [Test]
        public void TestEPLDataflowCustomProperties()
        {
            RegressionRunner.Run(session, EPLDataflowCustomProperties.Executions());
        }

        [Test]
        public void TestEPLDataflowDocSamples()
        {
            RegressionRunner.Run(session, EPLDataflowDocSamples.Executions());
        }

        [Test]
        public void TestEPLDataflowExampleRollingTopWords()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleRollingTopWords());
        }

        [Test]
        public void TestEPLDataflowExampleVwapFilterSelectJoin()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleVwapFilterSelectJoin());
        }

        [Test]
        public void TestEPLDataflowExampleWordCount()
        {
            RegressionRunner.Run(session, new EPLDataflowExampleWordCount());
        }

        [Test]
        public void TestEPLDataflowInputOutputVariations()
        {
            RegressionRunner.Run(session, EPLDataflowInputOutputVariations.Executions());
        }

        [Test]
        public void TestEPLDataflowInvalidGraph()
        {
            RegressionRunner.Run(session, EPLDataflowInvalidGraph.Executions());
        }

        [Test]
        public void TestEPLDataflowOpBeaconSource()
        {
            RegressionRunner.Run(session, EPLDataflowOpBeaconSource.Executions());
        }

        [Test]
        public void TestEPLDataflowOpEPStatementSource()
        {
            RegressionRunner.Run(session, EPLDataflowOpEPStatementSource.Executions());
        }

        [Test]
        public void TestEPLDataflowOpEventBusSink()
        {
            RegressionRunner.Run(session, EPLDataflowOpEventBusSink.Executions());
        }

        [Test]
        public void TestEPLDataflowOpEventBusSource()
        {
            RegressionRunner.Run(session, EPLDataflowOpEventBusSource.Executions());
        }

        [Test]
        public void TestEPLDataflowOpFilter()
        {
            RegressionRunner.Run(session, EPLDataflowOpFilter.Executions());
        }

        [Test]
        public void TestEPLDataflowOpLogSink()
        {
            RegressionRunner.Run(session, new EPLDataflowOpLogSink());
        }

        [Test]
        public void TestEPLDataflowOpSelect()
        {
            RegressionRunner.Run(session, EPLDataflowOpSelect.Executions());
        }

        [Test]
        public void TestEPLDataflowTypes()
        {
            RegressionRunner.Run(session, EPLDataflowTypes.Executions());
        }
    }
} // end of namespace