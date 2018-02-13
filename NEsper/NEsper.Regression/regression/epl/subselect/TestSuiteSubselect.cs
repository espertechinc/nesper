///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    [TestFixture]
    public class TestSuiteSubselect
    {
        [Test]
        public void TestExecSubselectAggregatedInExistsAnyAll() {
            RegressionRunner.Run(new ExecSubselectAggregatedInExistsAnyAll());
        }
    
        [Test]
        public void TestExecSubselectAggregatedMultirowAndColumn() {
            RegressionRunner.Run(new ExecSubselectAggregatedMultirowAndColumn());
        }
    
        [Test]
        public void TestExecSubselectAggregatedSingleValue() {
            RegressionRunner.Run(new ExecSubselectAggregatedSingleValue());
        }
    
        [Test]
        public void TestExecSubselectAllAnySomeExpr() {
            RegressionRunner.Run(new ExecSubselectAllAnySomeExpr());
        }
    
        [Test]
        public void TestExecSubselectExists() {
            RegressionRunner.Run(new ExecSubselectExists());
        }
    
        [Test]
        public void TestExecSubselectFiltered() {
            RegressionRunner.Run(new ExecSubselectFiltered());
        }
    
        [Test]
        public void TestExecSubselectIn() {
            RegressionRunner.Run(new ExecSubselectIn());
        }
    
        [Test]
        public void TestExecSubselectIndex() {
            RegressionRunner.Run(new ExecSubselectIndex());
        }
    
        [Test]
        public void TestExecSubselectMulticolumn() {
            RegressionRunner.Run(new ExecSubselectMulticolumn());
        }
    
        [Test]
        public void TestExecSubselectMultirow() {
            RegressionRunner.Run(new ExecSubselectMultirow());
        }
    
        [Test]
        public void TestExecSubselectOrderOfEvalNoPreeval() {
            RegressionRunner.Run(new ExecSubselectOrderOfEvalNoPreeval());
        }
    
        [Test]
        public void TestExecSubselectOrderOfEval() {
            RegressionRunner.Run(new ExecSubselectOrderOfEval());
        }
    
        [Test]
        public void TestExecSubselectUnfiltered() {
            RegressionRunner.Run(new ExecSubselectUnfiltered());
        }
    
        [Test]
        public void TestExecSubselectWithinHaving() {
            RegressionRunner.Run(new ExecSubselectWithinHaving());
        }
    
        [Test]
        public void TestExecSubselectWithinPattern() {
            RegressionRunner.Run(new ExecSubselectWithinPattern());
        }
    
        [Test]
        public void TestExecSubselectNamedWindowPerformance() {
            RegressionRunner.Run(new ExecSubselectNamedWindowPerformance());
        }
    
        [Test]
        public void TestExecSubselectInKeywordPerformance() {
            RegressionRunner.Run(new ExecSubselectInKeywordPerformance());
        }
    
        [Test]
        public void TestExecSubselectCorrelatedAggregationPerformance() {
            RegressionRunner.Run(new ExecSubselectCorrelatedAggregationPerformance());
        }
    
        [Test]
        public void TestExecSubselectFilteredPerformance() {
            RegressionRunner.Run(new ExecSubselectFilteredPerformance());
        }
    }
} // end of namespace
