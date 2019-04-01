///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestSuiteView
    {
        [Test]
        public void TestExecViewExpiryIntersect() {
            RegressionRunner.Run(new ExecViewExpiryIntersect());
        }
    
        [Test]
        public void TestExecViewExpiryUnion() {
            RegressionRunner.Run(new ExecViewExpiryUnion());
        }
    
        [Test]
        public void TestExecViewExpressionBatch() {
            RegressionRunner.Run(new ExecViewExpressionBatch());
        }
    
        [Test]
        public void TestExecViewExpressionWindow() {
            RegressionRunner.Run(new ExecViewExpressionWindow());
        }
    
        [Test]
        public void TestExecViewExternallyBatched() {
            RegressionRunner.Run(new ExecViewExternallyBatched());
        }
    
        [Test]
        public void TestExecViewGroupLengthWinWeightAvg() {
            RegressionRunner.Run(new ExecViewGroupLengthWinWeightAvg());
        }
    
        [Test]
        public void TestExecViewGroupWin() {
            RegressionRunner.Run(new ExecViewGroupWin());
        }
    
        [Test]
        public void TestExecViewGroupWinReclaimMicrosecondResolution() {
            RegressionRunner.Run(new ExecViewGroupWinReclaimMicrosecondResolution());
        }
    
        [Test]
        public void TestExecViewGroupWinSharedViewStartStop() {
            RegressionRunner.Run(new ExecViewGroupWinSharedViewStartStop());
        }
    
        [Test]
        public void TestExecViewGroupWinTypes() {
            RegressionRunner.Run(new ExecViewGroupWinTypes());
        }
    
        [Test]
        public void TestExecViewGroupWithinGroup() {
            RegressionRunner.Run(new ExecViewGroupWithinGroup());
        }
    
        [Test]
        public void TestExecViewInheritAndInterface() {
            RegressionRunner.Run(new ExecViewInheritAndInterface());
        }
    
        [Test]
        public void TestExecViewInvalid() {
            RegressionRunner.Run(new ExecViewInvalid());
        }
    
        [Test]
        public void TestExecViewKeepAllWindow() {
            RegressionRunner.Run(new ExecViewKeepAllWindow());
        }
    
        [Test]
        public void TestExecViewLengthBatch() {
            RegressionRunner.Run(new ExecViewLengthBatch());
        }
    
        [Test]
        public void TestExecViewLengthWindowStats() {
            RegressionRunner.Run(new ExecViewLengthWindowStats());
        }
    
        [Test]
        public void TestExecViewMultipleExpiry() {
            RegressionRunner.Run(new ExecViewMultipleExpiry());
        }
    
        [Test]
        public void TestExecViewParameterizedByContext() {
            RegressionRunner.Run(new ExecViewParameterizedByContext());
        }
    
        [Test]
        public void TestExecViewPropertyAccess() {
            RegressionRunner.Run(new ExecViewPropertyAccess());
        }
    
        [Test]
        public void TestExecViewRank() {
            RegressionRunner.Run(new ExecViewRank());
        }
    
        [Test]
        public void TestExecViewSimpleFilter() {
            RegressionRunner.Run(new ExecViewSimpleFilter());
        }
    
        [Test]
        public void TestExecViewSize() {
            RegressionRunner.Run(new ExecViewSize());
        }
    
        [Test]
        public void TestExecViewStartStop() {
            RegressionRunner.Run(new ExecViewStartStop());
        }
    
        [Test]
        public void TestExecViewTimeAccum() {
            RegressionRunner.Run(new ExecViewTimeAccum());
        }
    
        [Test]
        public void TestExecViewTimeBatch() {
            RegressionRunner.Run(new ExecViewTimeBatch());
        }
    
        [Test]
        public void TestExecViewTimeBatchMean() {
            RegressionRunner.Run(new ExecViewTimeBatchMean());
        }
    
        [Test]
        public void TestExecViewTimeFirst() {
            RegressionRunner.Run(new ExecViewTimeFirst());
        }
    
        [Test]
        public void TestExecViewTimeInterval() {
            RegressionRunner.Run(new ExecViewTimeInterval());
        }
    
        [Test]
        public void TestExecViewTimeLengthBatch() {
            RegressionRunner.Run(new ExecViewTimeLengthBatch());
        }
    
        [Test]
        public void TestExecViewTimeOrderAndTimeToLive() {
            RegressionRunner.Run(new ExecViewTimeOrderAndTimeToLive());
        }
    
        [Test]
        public void TestExecViewTimeWin() {
            RegressionRunner.Run(new ExecViewTimeWin());
        }
    
        [Test]
        public void TestExecViewTimeWindowMicrosecondResolution() {
            RegressionRunner.Run(new ExecViewTimeWindowMicrosecondResolution());
        }
    
        [Test]
        public void TestExecViewTimeWindowUnique() {
            RegressionRunner.Run(new ExecViewTimeWindowUnique());
        }
    
        [Test]
        public void TestExecViewWhereClause() {
            RegressionRunner.Run(new ExecViewWhereClause());
        }
    
        [Test]
        public void TestExecViewUniqueSorted() {
            RegressionRunner.Run(new ExecViewUniqueSorted());
        }
    
        [Test]
        public void TestExecViewTimeWindowWeightedAvg() {
            RegressionRunner.Run(new ExecViewTimeWindowWeightedAvg());
        }
    
        [Test]
        public void TestExecViewTimeWinMultithreaded() {
            RegressionRunner.Run(new ExecViewTimeWinMultithreaded());
        }
    }
} // end of namespace
