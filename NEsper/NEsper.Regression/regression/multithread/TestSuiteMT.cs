///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.multithread
{
    [TestFixture]
    public class TestSuiteMT
    {
        [Test]
        public void TestExecMTContextNestedNonOverlapAtNow() {
            RegressionRunner.Run(new ExecMTContextNestedNonOverlapAtNow());
        }
    
        [Test]
        public void TestExecMTContextCountSimple() {
            RegressionRunner.Run(new ExecMTContextCountSimple());
        }
    
        [Test]
        public void TestExecMTContextUnique() {
            RegressionRunner.Run(new ExecMTContextUnique());
        }
    
        [Test]
        public void TestExecMTContextDBAccess() {
            RegressionRunner.Run(new ExecMTContextDBAccess());
        }
    
        [Test]
        public void TestExecMTContextInitiatedTerminatedWithNowParallel() {
            RegressionRunner.Run(new ExecMTContextInitiatedTerminatedWithNowParallel());
        }
    
        [Test]
        public void TestExecMTContextListenerDispatch() {
            RegressionRunner.Run(new ExecMTContextListenerDispatch());
        }
    
        [Test]
        public void TestExecMTContextMultiStmtStartEnd() {
            RegressionRunner.Run(new ExecMTContextMultiStmtStartEnd());
        }
    
        [Test]
        public void TestExecMTContextOverlapDistinct() {
            RegressionRunner.Run(new ExecMTContextOverlapDistinct());
        }
    
        [Test]
        public void TestExecMTContextSegmented() {
            RegressionRunner.Run(new ExecMTContextSegmented());
        }
    
        [Test]
        public void TestExecMTContextStartedBySameEvent() {
            RegressionRunner.Run(new ExecMTContextStartedBySameEvent());
        }
    
        [Test]
        public void TestExecMTContextTemporalStartStop() {
            RegressionRunner.Run(new ExecMTContextTemporalStartStop());
        }
    
        [Test]
        public void TestExecMTContextTerminated() {
            RegressionRunner.Run(new ExecMTContextTerminated());
        }
    
        [Test]
        public void TestExecMTDeployAtomic() {
            RegressionRunner.Run(new ExecMTDeployAtomic());
        }
    
        [Test]
        public void TestExecMTDeterminismInsertInto() {
            RegressionRunner.Run(new ExecMTDeterminismInsertInto());
        }
    
        [Test]
        public void TestExecMTDeterminismListener() {
            RegressionRunner.Run(new ExecMTDeterminismListener());
        }
    
        [Test]
        public void TestExecMTInsertIntoTimerConcurrency() {
            RegressionRunner.Run(new ExecMTInsertIntoTimerConcurrency());
        }
    
        [Test]
        public void TestExecMTIsolation() {
            RegressionRunner.Run(new ExecMTIsolation());
        }
    
        [Test]
        public void TestExecMTStmtDatabaseJoin() {
            RegressionRunner.Run(new ExecMTStmtDatabaseJoin());
        }
    
        [Test]
        public void TestExecMTStmtFilter() {
            RegressionRunner.Run(new ExecMTStmtFilter());
        }
    
        [Test]
        public void TestExecMTStmtFilterSubquery() {
            RegressionRunner.Run(new ExecMTStmtFilterSubquery());
        }
    
        [Test]
        public void TestExecMTStmtInsertInto() {
            RegressionRunner.Run(new ExecMTStmtInsertInto());
        }
    
        [Test]
        public void TestExecMTStmtIterate() {
            RegressionRunner.Run(new ExecMTStmtIterate());
        }
    
        [Test]
        public void TestExecMTStmtJoin() {
            RegressionRunner.Run(new ExecMTStmtJoin());
        }
    
        [Test]
        public void TestExecMTStmtListenerAddRemove() {
            RegressionRunner.Run(new ExecMTStmtListenerAddRemove());
        }
    
        [Test]
        public void TestExecMTStmtListenerCreateStmt() {
            RegressionRunner.Run(new ExecMTStmtListenerCreateStmt());
        }
    
        [Test]
        public void TestExecMTStmtListenerRoute() {
            RegressionRunner.Run(new ExecMTStmtListenerRoute());
        }
    
        [Test]
        public void TestExecMTStmtMgmt() {
            RegressionRunner.Run(new ExecMTStmtMgmt());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowConsume() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowConsume());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowDelete() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowDelete());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowFAF() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowFAF());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowIterate() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowIterate());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowJoinUniqueView() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowJoinUniqueView());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowMerge() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowMerge());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowMultiple() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowMultiple());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowPriority() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowPriority());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowSubqueryAgg() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowSubqueryAgg());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowSubqueryLookup() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowSubqueryLookup());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowUniqueTwoWJoinConsumer() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowUniqueTwoWJoinConsumer());
        }
    
        [Test]
        public void TestExecMTStmtNamedWindowUpdate() {
            RegressionRunner.Run(new ExecMTStmtNamedWindowUpdate());
        }
    
        [Test]
        public void TestExecMTStmtPattern() {
            RegressionRunner.Run(new ExecMTStmtPattern());
        }
    
        [Test]
        public void TestExecMTStmtPatternFollowedBy() {
            RegressionRunner.Run(new ExecMTStmtPatternFollowedBy());
        }
    
        [Test]
        public void TestExecMTStmtSharedView() {
            RegressionRunner.Run(new ExecMTStmtSharedView());
        }
    
        [Test]
        public void TestExecMTStmtStateless() {
            RegressionRunner.Run(new ExecMTStmtStateless());
        }
    
        [Test]
        public void TestExecMTStmtStatelessEnummethod() {
            RegressionRunner.Run(new ExecMTStmtStatelessEnummethod());
        }
    
        [Test]
        public void TestExecMTStmtSubquery() {
            RegressionRunner.Run(new ExecMTStmtSubquery());
        }
    
        [Test]
        public void TestExecMTStmtTimeWindow() {
            RegressionRunner.Run(new ExecMTStmtTimeWindow());
        }
    
        [Test]
        public void TestExecMTStmtTwoPatterns() {
            RegressionRunner.Run(new ExecMTStmtTwoPatterns());
        }
    
        [Test]
        public void TestExecMTStmtTwoPatternsStartStop() {
            RegressionRunner.Run(new ExecMTStmtTwoPatternsStartStop());
        }
    
        [Test]
        public void TestExecMTUpdate() {
            RegressionRunner.Run(new ExecMTUpdate());
        }
    
        [Test]
        public void TestExecMTUpdateIStreamSubselect() {
            RegressionRunner.Run(new ExecMTUpdateIStreamSubselect());
        }
    
        [Test]
        public void TestExecMTVariables() {
            RegressionRunner.Run(new ExecMTVariables());
        }
    
    }
} // end of namespace
