///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestSuiteContext
    {
        [Test]
        public void TestExecContextAdminPartitionSPI() {
            RegressionRunner.Run(new ExecContextAdminPartitionSPI());
        }
    
        [Test]
        public void TestExecContextCategory() {
            RegressionRunner.Run(new ExecContextCategory());
        }
    
        [Test]
        public void TestExecContextDocExamples() {
            RegressionRunner.Run(new ExecContextDocExamples());
        }
    
        [Test]
        public void TestExecContextHashSegmented() {
            RegressionRunner.Run(new ExecContextHashSegmented());
        }
    
        [Test]
        public void TestExecContextInitTerm() {
            RegressionRunner.Run(new ExecContextInitTerm());
        }
    
        [Test]
        public void TestExecContextInitTermPrioritized() {
            RegressionRunner.Run(new ExecContextInitTermPrioritized());
        }
    
        [Test]
        public void TestExecContextInitTermTemporalFixed() {
            RegressionRunner.Run(new ExecContextInitTermTemporalFixed());
        }
    
        [Test]
        public void TestExecContextInitTermWithDistinct() {
            RegressionRunner.Run(new ExecContextInitTermWithDistinct());
        }
    
        [Test]
        public void TestExecContextInitTermWithNow() {
            RegressionRunner.Run(new ExecContextInitTermWithNow());
        }
    
        [Test]
        public void TestExecContextLifecycle() {
            RegressionRunner.Run(new ExecContextLifecycle());
        }
    
        [Test]
        public void TestExecContextNested() {
            RegressionRunner.Run(new ExecContextNested());
        }
    
        [Test]
        public void TestExecContextPartitioned() {
            RegressionRunner.Run(new ExecContextPartitioned());
        }
    
        [Test]
        public void TestExecContextPartitionedAggregate() {
            RegressionRunner.Run(new ExecContextPartitionedAggregate());
        }
    
        [Test]
        public void TestExecContextPartitionedInfra() {
            RegressionRunner.Run(new ExecContextPartitionedInfra());
        }
    
        [Test]
        public void TestExecContextPartitionedNamedWindow() {
            RegressionRunner.Run(new ExecContextPartitionedNamedWindow());
        }
    
        [Test]
        public void TestExecContextPartitionedPrioritized() {
            RegressionRunner.Run(new ExecContextPartitionedPrioritized());
        }
    
        [Test]
        public void TestExecContextSelectionAndFireAndForget() {
            RegressionRunner.Run(new ExecContextSelectionAndFireAndForget());
        }
    
        [Test]
        public void TestExecContextWDeclaredExpression() {
            RegressionRunner.Run(new ExecContextWDeclaredExpression());
        }
    
        [Test]
        public void TestExecContextVariables() {
            RegressionRunner.Run(new ExecContextVariables());
        }
    }
} // end of namespace
