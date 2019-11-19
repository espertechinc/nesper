///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.context;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.context
{
    [TestFixture]
    public class TestSuiteContext
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
                typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1),
                typeof(SupportBean_S2), typeof(ISupportBaseAB), typeof(ISupportA), typeof(SupportWebEvent),
                typeof(ISupportAImpl), typeof(SupportGroupSubgroupEvent)
            }) {
                configuration.Common.AddEventType(clazz.Name, clazz);
            }

            configuration.Common.AddEventType(typeof(ContextDocExamples.BankTxn));
            configuration.Common.AddEventType(typeof(ContextDocExamples.LoginEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.LogoutEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.SecurityEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.SensorEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrafficEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrainEnterEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.TrainLeaveEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.CumulativePrice));
            configuration.Common.AddEventType(typeof(ContextDocExamples.PassengerScanEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyInitEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTermEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyEvent));
            configuration.Common.AddEventType("StartEventOne", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("StartEventTwo", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("MyOtherEvent", typeof(ContextDocExamples.MyStartEvent));
            configuration.Common.AddEventType("EndEventOne", typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType("EndEventTwo", typeof(ContextDocExamples.MyEndEvent));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTwoKeyInit));
            configuration.Common.AddEventType(typeof(ContextDocExamples.MyTwoKeyTerm));

            configuration.Compiler.AddPlugInSingleRowFunction("myHash", typeof(ContextHashSegmented), "MyHashFunc");
            configuration.Compiler.AddPlugInSingleRowFunction("mySecond", typeof(ContextHashSegmented), "MySecondFunc");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "makeBean",
                typeof(ContextInitTermTemporalFixed),
                "SingleRowPluginMakeBean");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "toArray",
                typeof(ContextKeySegmentedAggregate),
                "ToArray");

            configuration.Compiler.AddPlugInSingleRowFunction(
                "customEnabled",
                typeof(ContextNested),
                "CustomMatch",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "customDisabled",
                typeof(ContextNested),
                "CustomMatch",
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.DISABLED);
            configuration.Compiler.AddPlugInSingleRowFunction(
                "stringContainsX",
                typeof(ContextKeySegmented),
                "StringContainsX");

            configuration.Common.AddImportType(typeof(ContextHashSegmented));

            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configuration.Common.AddDatabaseReference("MyDB", configDB);

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInVirtualDataWindow(
                "test",
                "vdw",
                typeof(SupportVirtualDWForge),
                SupportVirtualDW.ITERATE); // configure with iteration
        }

        [Test, RunInApplicationDomain]
        public void TestContextAdminListen()
        {
            RegressionRunner.Run(session, ContextAdminListen.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextCategory()
        {
            RegressionRunner.Run(session, ContextCategory.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextDocExamples()
        {
            RegressionRunner.Run(session, new ContextDocExamples());
        }

        [Test, RunInApplicationDomain]
        public void TestContextHashSegmented()
        {
            RegressionRunner.Run(session, ContextHashSegmented.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTerm()
        {
            RegressionRunner.Run(session, ContextInitTerm.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermTemporalFixed()
        {
            RegressionRunner.Run(session, ContextInitTermTemporalFixed.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermWithDistinct()
        {
            RegressionRunner.Run(session, ContextInitTermWithDistinct.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextInitTermWithNow()
        {
            RegressionRunner.Run(session, ContextInitTermWithNow.Executions());
        }

        [Test]
        public void TestContextKeySegmented()
        {
            RegressionRunner.Run(session, ContextKeySegmented.Executions());
        }

        [Test]
        public void TestContextKeySegmentedAggregate()
        {
            RegressionRunner.Run(session, ContextKeySegmentedAggregate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedInfra()
        {
            RegressionRunner.Run(session, ContextKeySegmentedInfra.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextKeySegmentedNamedWindow()
        {
            RegressionRunner.Run(session, ContextKeySegmentedNamedWindow.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextLifecycle()
        {
            RegressionRunner.Run(session, ContextLifecycle.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextNested()
        {
            RegressionRunner.Run(session, ContextNested.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextSelectionAndFireAndForget()
        {
            RegressionRunner.Run(session, ContextSelectionAndFireAndForget.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextVariables()
        {
            RegressionRunner.Run(session, ContextVariables.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestContextWDeclaredExpression()
        {
            RegressionRunner.Run(session, ContextWDeclaredExpression.Executions());
        }
    }
} // end of namespace