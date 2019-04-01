///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestSuiteRowRecog
    {
        [Test]
        public void TestExecRowRecogInvalid() {
            RegressionRunner.Run(new ExecRowRecogInvalid());
        }
    
        [Test]
        public void TestExecRowRecogMaxStatesEngineWideNoPreventStart() {
            RegressionRunner.Run(new ExecRowRecogMaxStatesEngineWideNoPreventStart());
        }
    
        [Test]
        public void TestExecRowRecogMaxStatesEngineWide3Instance() {
            RegressionRunner.Run(new ExecRowRecogMaxStatesEngineWide3Instance());
        }
    
        [Test]
        public void TestExecRowRecogMaxStatesEngineWide4Instance() {
            RegressionRunner.Run(new ExecRowRecogMaxStatesEngineWide4Instance());
        }
    
        [Test]
        public void TestExecRowRecogAfter() {
            RegressionRunner.Run(new ExecRowRecogAfter());
        }
    
        [Test]
        public void TestExecRowRecogAggregation() {
            RegressionRunner.Run(new ExecRowRecogAggregation());
        }
    
        [Test]
        public void TestExecRowRecogArrayAccess() {
            RegressionRunner.Run(new ExecRowRecogArrayAccess());
        }
    
        [Test]
        public void TestExecRowRecogClausePresence() {
            RegressionRunner.Run(new ExecRowRecogClausePresence());
        }
    
        [Test]
        public void TestExecRowRecogDataSet() {
            RegressionRunner.Run(new ExecRowRecogDataSet());
        }
    
        [Test]
        public void TestExecRowRecogDataWin() {
            RegressionRunner.Run(new ExecRowRecogDataWin());
        }
    
        [Test]
        public void TestExecRowRecogDelete() {
            RegressionRunner.Run(new ExecRowRecogDelete());
        }
    
        [Test]
        public void TestExecRowRecogEmptyPartition() {
            RegressionRunner.Run(new ExecRowRecogEmptyPartition());
        }
    
        [Test]
        public void TestExecRowRecogEnumMethod() {
            RegressionRunner.Run(new ExecRowRecogEnumMethod());
        }
    
        [Test]
        public void TestExecRowRecogGreedyness() {
            RegressionRunner.Run(new ExecRowRecogGreedyness());
        }
    
        [Test]
        public void TestExecRowRecogInterval() {
            RegressionRunner.Run(new ExecRowRecogInterval());
        }
    
        [Test]
        public void TestExecRowRecogIntervalMicrosecondResolution() {
            RegressionRunner.Run(new ExecRowRecogIntervalMicrosecondResolution());
        }
    
        [Test]
        public void TestExecRowRecogIntervalOrTerminated() {
            RegressionRunner.Run(new ExecRowRecogIntervalOrTerminated());
        }
    
        [Test]
        public void TestExecRowRecogIterateOnly() {
            RegressionRunner.Run(new ExecRowRecogIterateOnly());
        }
    
        [Test]
        public void TestExecRowRecogOps() {
            RegressionRunner.Run(new ExecRowRecogOps());
        }
    
        [Test]
        public void TestExecRowRecogPerf() {
            RegressionRunner.Run(new ExecRowRecogPerf());
        }
    
        [Test]
        public void TestExecRowRecogPermute() {
            RegressionRunner.Run(new ExecRowRecogPermute());
        }
    
        [Test]
        public void TestExecRowRecogPrev() {
            RegressionRunner.Run(new ExecRowRecogPrev());
        }
    
        [Test]
        public void TestExecRowRecogRegex() {
            RegressionRunner.Run(new ExecRowRecogRegex());
        }
    
        [Test]
        public void TestExecRowRecogRepetition() {
            RegressionRunner.Run(new ExecRowRecogRepetition());
        }
    
        [Test]
        public void TestExecRowRecogVariantStream() {
            RegressionRunner.Run(new ExecRowRecogVariantStream());
        }
    }
} // end of namespace
