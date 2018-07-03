///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    // see INFRA suite for additional Named Window tests
    [TestFixture]
    public class TestSuiteNamedWindow
    {
        [Test]
        public void TestExecNamedWindowConsumer() {
            RegressionRunner.Run(new ExecNamedWindowConsumer());
        }
    
        [Test]
        public void TestExecNamedWindowContainedEvent() {
            RegressionRunner.Run(new ExecNamedWindowContainedEvent());
        }
    
        [Test]
        public void TestExecNamedWindowIndex() {
            RegressionRunner.Run(new ExecNamedWindowIndex());
        }
    
        [Test]
        public void TestExecNamedWindowIndexAddedValType() {
            RegressionRunner.Run(new ExecNamedWindowIndexAddedValType());
        }
    
        [Test]
        public void TestExecNamedWindowInsertFrom() {
            RegressionRunner.Run(new ExecNamedWindowInsertFrom());
        }
    
        [Test]
        public void TestExecNamedWindowJoin() {
            RegressionRunner.Run(new ExecNamedWindowJoin());
        }
    
        [Test]
        public void TestExecNamedWindowOM() {
            RegressionRunner.Run(new ExecNamedWindowOM());
        }
    
        [Test]
        public void TestExecNamedWindowOnDelete() {
            RegressionRunner.Run(new ExecNamedWindowOnDelete());
        }
    
        [Test]
        public void TestExecNamedWindowOnMerge() {
            RegressionRunner.Run(new ExecNamedWindowOnMerge());
        }
    
        [Test]
        public void TestExecNamedWindowOnSelect() {
            RegressionRunner.Run(new ExecNamedWindowOnSelect());
        }
    
        [Test]
        public void TestExecNamedWindowOnUpdate() {
            RegressionRunner.Run(new ExecNamedWindowOnUpdate());
        }
    
        [Test]
        public void TestExecNamedWindowOnUpdateWMultiDispatch() {
            RegressionRunner.Run(new ExecNamedWindowOnUpdateWMultiDispatch(true, null, null));
            RegressionRunner.Run(new ExecNamedWindowOnUpdateWMultiDispatch(false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN));
            RegressionRunner.Run(new ExecNamedWindowOnUpdateWMultiDispatch(false, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND));
            RegressionRunner.Run(new ExecNamedWindowOnUpdateWMultiDispatch(false, false, null));
        }
    
        [Test]
        public void TestExecNamedWindowOutputrate() {
            RegressionRunner.Run(new ExecNamedWindowOutputrate());
        }
    
        [Test]
        public void TestExecNamedWindowProcessingOrder() {
            RegressionRunner.Run(new ExecNamedWindowProcessingOrder());
        }
    
        [Test]
        public void TestExecNamedWindowRemoveStream() {
            RegressionRunner.Run(new ExecNamedWindowRemoveStream());
        }
    
        [Test]
        public void TestExecNamedWindowStartStop() {
            RegressionRunner.Run(new ExecNamedWindowStartStop());
        }
    
        [Test]
        public void TestExecNamedWindowSubquery() {
            RegressionRunner.Run(new ExecNamedWindowSubquery());
        }
    
        [Test]
        public void TestExecNamedWindowTypes() {
            RegressionRunner.Run(new ExecNamedWindowTypes());
        }
    
        [Test]
        public void TestExecNamedWindowViews() {
            RegressionRunner.Run(new ExecNamedWindowViews());
        }
    
        [Test]
        public void TestExecNamedWindowPerformance() {
            RegressionRunner.Run(new ExecNamedWindowPerformance());
        }
    
        [Test]
        public void TestExecNamedWIndowFAFQueryJoinPerformance() {
            RegressionRunner.Run(new ExecNamedWIndowFAFQueryJoinPerformance());
        }
    
    }
} // end of namespace
