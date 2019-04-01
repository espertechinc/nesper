///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestSuiteDataflow
    {
        [Test]
        public void TestExecDataflowAPIConfigAndInstance() {
            RegressionRunner.Run(new ExecDataflowAPIConfigAndInstance());
        }
    
        [Test]
        public void TestExecDataflowAPICreateStartStopDestroy() {
            RegressionRunner.Run(new ExecDataflowAPICreateStartStopDestroy());
        }
    
        [Test]
        public void TestExecDataflowAPIExceptions() {
            RegressionRunner.Run(new ExecDataflowAPIExceptions());
        }
    
        [Test]
        public void TestExecDataflowAPIInstantiationOptions() {
            RegressionRunner.Run(new ExecDataflowAPIInstantiationOptions());
        }
    
        [Test]
        public void TestExecDataflowAPIOpLifecycle() {
            RegressionRunner.Run(new ExecDataflowAPIOpLifecycle());
        }
    
        [Test]
        public void TestExecDataflowAPIRunStartCancelJoin() {
            RegressionRunner.Run(new ExecDataflowAPIRunStartCancelJoin());
        }
    
        [Test]
        public void TestExecDataflowAPIStartCaptive() {
            RegressionRunner.Run(new ExecDataflowAPIStartCaptive());
        }
    
        [Test]
        public void TestExecDataflowAPIStatistics() {
            RegressionRunner.Run(new ExecDataflowAPIStatistics());
        }
    
        [Test]
        public void TestExecDataflowCustomProperties() {
            RegressionRunner.Run(new ExecDataflowCustomProperties());
        }
    
        [Test]
        public void TestExecDataflowOpBeaconSource() {
            RegressionRunner.Run(new ExecDataflowOpBeaconSource());
        }
    
        [Test]
        public void TestExecDataflowOpEPStatementSource() {
            RegressionRunner.Run(new ExecDataflowOpEPStatementSource());
        }
    
        [Test]
        public void TestExecDataflowOpEventBusSink() {
            RegressionRunner.Run(new ExecDataflowOpEventBusSink());
        }
    
        [Test]
        public void TestExecDataflowOpEventBusSource() {
            RegressionRunner.Run(new ExecDataflowOpEventBusSource());
        }
    
        [Test]
        public void TestExecDataflowOpFilter() {
            RegressionRunner.Run(new ExecDataflowOpFilter());
        }
    
        [Test]
        public void TestExecDataflowOpLogSink() {
            RegressionRunner.Run(new ExecDataflowOpLogSink());
        }
    
        [Test]
        public void TestExecDataflowOpSelect() {
            RegressionRunner.Run(new ExecDataflowOpSelect());
        }
    
        [Test]
        public void TestExecDataflowDocSamples() {
            RegressionRunner.Run(new ExecDataflowDocSamples());
        }
    
        [Test]
        public void TestExecDataflowExampleRollingTopWords() {
            RegressionRunner.Run(new ExecDataflowExampleRollingTopWords());
        }
    
        [Test]
        public void TestExecDataflowExampleVwapFilterSelectJoin() {
            RegressionRunner.Run(new ExecDataflowExampleVwapFilterSelectJoin());
        }
    
        [Test]
        public void TestExecDataflowExampleWordCount() {
            RegressionRunner.Run(new ExecDataflowExampleWordCount());
        }
    
        [Test]
        public void TestExecDataflowInputOutputVariations() {
            RegressionRunner.Run(new ExecDataflowInputOutputVariations());
        }
    
        [Test]
        public void TestExecDataflowInvalidGraph() {
            RegressionRunner.Run(new ExecDataflowInvalidGraph());
        }
    
        [Test]
        public void TestExecDataflowTypes() {
            RegressionRunner.Run(new ExecDataflowTypes());
        }
    }
} // end of namespace
