///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    // see INFRA suite for additional Table tests
    [TestFixture]
    public class TestSuiteTable
    {
        [Test]
        public void TestExecTableAccessAggregationState() {
            RegressionRunner.Run(new ExecTableAccessAggregationState());
        }
    
        [Test]
        public void TestExecTableAccessCore() {
            RegressionRunner.Run(new ExecTableAccessCore());
        }
    
        [Test]
        public void TestExecTableNonAccessDotSubqueryAndJoin() {
            RegressionRunner.Run(new ExecTableNonAccessDotSubqueryAndJoin());
        }
    
        [Test]
        public void TestExecTableContext() {
            RegressionRunner.Run(new ExecTableContext());
        }
    
        [Test]
        public void TestExecTableCountMinSketch() {
            RegressionRunner.Run(new ExecTableCountMinSketch());
        }
    
        [Test]
        public void TestExecTableAccessDotMethod() {
            RegressionRunner.Run(new ExecTableAccessDotMethod());
        }
    
        [Test]
        public void TestExecTableDocSamples() {
            RegressionRunner.Run(new ExecTableDocSamples());
        }
    
        [Test]
        public void TestExecTableFAFExecuteQuery() {
            RegressionRunner.Run(new ExecTableFAFExecuteQuery());
        }
    
        [Test]
        public void TestExecTableFilters() {
            RegressionRunner.Run(new ExecTableFilters());
        }
    
        [Test]
        public void TestExecTableInsertInto() {
            RegressionRunner.Run(new ExecTableInsertInto());
        }
    
        [Test]
        public void TestExecTableIntoTable() {
            RegressionRunner.Run(new ExecTableIntoTable());
        }
    
        [Test]
        public void TestExecTableInvalid() {
            RegressionRunner.Run(new ExecTableInvalid());
        }
    
        [Test]
        public void TestExecTableIterate() {
            RegressionRunner.Run(new ExecTableIterate());
        }
    
        [Test]
        public void TestExecTableJoin() {
            RegressionRunner.Run(new ExecTableJoin());
        }
    
        [Test]
        public void TestExecTableLifecycle() {
            RegressionRunner.Run(new ExecTableLifecycle());
        }
    
        [Test]
        public void TestExecTableOnDelete() {
            RegressionRunner.Run(new ExecTableOnDelete());
        }
    
        [Test]
        public void TestExecTableOnMerge() {
            RegressionRunner.Run(new ExecTableOnMerge());
        }
    
        [Test]
        public void TestExecTableOnSelect() {
            RegressionRunner.Run(new ExecTableOnSelect());
        }
    
        [Test]
        public void TestExecTableOnUpdate() {
            RegressionRunner.Run(new ExecTableOnUpdate());
        }
    
        [Test]
        public void TestExecTableOutputRateLimiting() {
            RegressionRunner.Run(new ExecTableOutputRateLimiting());
        }
    
        [Test]
        public void TestExecTablePlugInAggregation() {
            RegressionRunner.Run(new ExecTablePlugInAggregation());
        }
    
        [Test]
        public void TestExecTableRollup() {
            RegressionRunner.Run(new ExecTableRollup());
        }
    
        [Test]
        public void TestExecTableSubquery() {
            RegressionRunner.Run(new ExecTableSubquery());
        }
    
        [Test]
        public void TestExecTableUpdateAndIndex() {
            RegressionRunner.Run(new ExecTableUpdateAndIndex());
        }
    
        [Test]
        public void TestExecTableWNamedWindow() {
            RegressionRunner.Run(new ExecTableWNamedWindow());
        }
    
        [Test]
        public void TestExecTableSelectStarPublicTypeVisibility() {
            RegressionRunner.Run(new ExecTableSelectStarPublicTypeVisibility());
        }
    
        [Test]
        public void TestExecTableMTAccessReadMergeWriteInsertDeleteRowVisible() {
            RegressionRunner.Run(new ExecTableMTAccessReadMergeWriteInsertDeleteRowVisible());
        }
    
        [Test]
        public void TestExecTableMTGroupedAccessReadIntoTableWriteAggColConsistency() {
            RegressionRunner.Run(new ExecTableMTGroupedAccessReadIntoTableWriteAggColConsistency());
        }
    
        [Test]
        public void TestExecTableMTGroupedAccessReadIntoTableWriteNewRowCreation() {
            RegressionRunner.Run(new ExecTableMTGroupedAccessReadIntoTableWriteNewRowCreation());
        }
    
        [Test]
        public void TestExecTableMTGroupedFAFReadFAFWriteChain() {
            RegressionRunner.Run(new ExecTableMTGroupedFAFReadFAFWriteChain());
        }
    
        [Test]
        public void TestExecTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd() {
            RegressionRunner.Run(new ExecTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd());
        }
    
        [Test]
        public void TestExecTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd() {
            RegressionRunner.Run(new ExecTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd());
        }
    
        [Test]
        public void TestExecTableMTGroupedSubqueryReadInsertIntoWriteConcurr() {
            RegressionRunner.Run(new ExecTableMTGroupedSubqueryReadInsertIntoWriteConcurr());
        }
    
        [Test]
        public void TestExecTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd() {
            RegressionRunner.Run(new ExecTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd());
        }
    
        [Test]
        public void TestExecTableMTGroupedWContextIntoTableWriteAsContextTable() {
            RegressionRunner.Run(new ExecTableMTGroupedWContextIntoTableWriteAsContextTable());
        }
    
        [Test]
        public void TestExecTableMTGroupedWContextIntoTableWriteAsSharedTable() {
            RegressionRunner.Run(new ExecTableMTGroupedWContextIntoTableWriteAsSharedTable());
        }
    
        [Test]
        public void TestExecTableMTUngroupedAccessReadInotTableWriteIterate() {
            RegressionRunner.Run(new ExecTableMTUngroupedAccessReadInotTableWriteIterate());
        }
    
        [Test]
        public void TestExecTableMTUngroupedAccessReadIntoTableWriteFilterUse() {
            RegressionRunner.Run(new ExecTableMTUngroupedAccessReadIntoTableWriteFilterUse());
        }
    
        [Test]
        public void TestExecTableMTUngroupedAccessReadMergeWrite() {
            RegressionRunner.Run(new ExecTableMTUngroupedAccessReadMergeWrite());
        }
    
        [Test]
        public void TestExecTableMTUngroupedAccessWithinRowFAFConsistency() {
            RegressionRunner.Run(new ExecTableMTUngroupedAccessWithinRowFAFConsistency());
        }
    
        [Test]
        public void TestExecTableMTUngroupedIntoTableWriteMultiWriterAgg() {
            RegressionRunner.Run(new ExecTableMTUngroupedIntoTableWriteMultiWriterAgg());
        }
    
        [Test]
        public void TestExecTableMTUngroupedJoinColumnConsistency() {
            RegressionRunner.Run(new ExecTableMTUngroupedJoinColumnConsistency());
        }
    
        [Test]
        public void TestExecTableMTUngroupedSubqueryReadMergeWriteColumnUpd() {
            RegressionRunner.Run(new ExecTableMTUngroupedSubqueryReadMergeWriteColumnUpd());
        }
    }
} // end of namespace
