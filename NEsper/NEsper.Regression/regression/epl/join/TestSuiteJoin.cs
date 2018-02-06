///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.epl.join
{
    [TestFixture]
    public class TestSuiteJoin
    {
        [Test]
        public void TestExecJoin20Stream() {
            RegressionRunner.Run(new ExecJoin20Stream());
        }
    
        [Test]
        public void TestExecJoinCoercion() {
            RegressionRunner.Run(new ExecJoinCoercion());
        }
    
        [Test]
        public void TestExecJoinDerivedValueViews() {
            RegressionRunner.Run(new ExecJoinDerivedValueViews());
        }
    
        [Test]
        public void TestExecJoinEventRepresentation() {
            RegressionRunner.Run(new ExecJoinEventRepresentation());
        }
    
        [Test]
        public void TestExecJoinInheritAndInterface() {
            RegressionRunner.Run(new ExecJoinInheritAndInterface());
        }
    
        [Test]
        public void TestExecJoinMultiKeyAndRange() {
            RegressionRunner.Run(new ExecJoinMultiKeyAndRange());
        }
    
        [Test]
        public void TestExecJoinNoTableName() {
            RegressionRunner.Run(new ExecJoinNoTableName());
        }
    
        [Test]
        public void TestExecJoinNoWhereClause() {
            RegressionRunner.Run(new ExecJoinNoWhereClause());
        }
    
        [Test]
        public void TestExecJoinPropertyAccess() {
            RegressionRunner.Run(new ExecJoinPropertyAccess());
        }
    
        [Test]
        public void TestExecJoinSingleOp3Stream() {
            RegressionRunner.Run(new ExecJoinSingleOp3Stream());
        }
    
        [Test]
        public void TestExecJoinStartStop() {
            RegressionRunner.Run(new ExecJoinStartStop());
        }
    
        [Test]
        public void TestExecJoinUniqueIndex() {
            RegressionRunner.Run(new ExecJoinUniqueIndex());
        }
    
        [Test]
        public void TestExecOuterFullJoin3Stream() {
            RegressionRunner.Run(new ExecOuterFullJoin3Stream());
        }
    
        [Test]
        public void TestExecOuterInnerJoin3Stream() {
            RegressionRunner.Run(new ExecOuterInnerJoin3Stream());
        }
    
        [Test]
        public void TestExecOuterInnerJoin4Stream() {
            RegressionRunner.Run(new ExecOuterInnerJoin4Stream());
        }
    
        [Test]
        public void TestExecOuterJoin2Stream() {
            RegressionRunner.Run(new ExecOuterJoin2Stream());
        }
    
        [Test]
        public void TestExecOuterJoin6Stream() {
            RegressionRunner.Run(new ExecOuterJoin6Stream());
        }
    
        [Test]
        public void TestExecOuterJoin7Stream() {
            RegressionRunner.Run(new ExecOuterJoin7Stream());
        }
    
        [Test]
        public void TestExecOuterJoinCart4Stream() {
            RegressionRunner.Run(new ExecOuterJoinCart4Stream());
        }
    
        [Test]
        public void TestExecOuterJoinCart5Stream() {
            RegressionRunner.Run(new ExecOuterJoinCart5Stream());
        }
    
        [Test]
        public void TestExecOuterJoinChain4Stream() {
            RegressionRunner.Run(new ExecOuterJoinChain4Stream());
        }
    
        [Test]
        public void TestExecOuterJoinUnidirectional() {
            RegressionRunner.Run(new ExecOuterJoinUnidirectional());
        }
    
        [Test]
        public void TestExecOuterJoinVarA3Stream() {
            RegressionRunner.Run(new ExecOuterJoinVarA3Stream());
        }
    
        [Test]
        public void TestExecOuterJoinVarB3Stream() {
            RegressionRunner.Run(new ExecOuterJoinVarB3Stream());
        }
    
        [Test]
        public void TestExecOuterJoinVarC3Stream() {
            RegressionRunner.Run(new ExecOuterJoinVarC3Stream());
        }
    
        [Test]
        public void TestExecOuterJoinLeftWWhere() {
            RegressionRunner.Run(new ExecOuterJoinLeftWWhere());
        }
    
        [Test]
        public void TestExecJoinSelectClause() {
            RegressionRunner.Run(new ExecJoinSelectClause());
        }
    
        [Test]
        public void TestExecJoinPatterns() {
            RegressionRunner.Run(new ExecJoinPatterns());
        }
    
        [Test]
        public void TestExecJoinUnidirectionalStream() {
            RegressionRunner.Run(new ExecJoinUnidirectionalStream());
        }
    
        [Test]
        public void TestExecJoin2StreamAndPropertyPerformance() {
            RegressionRunner.Run(new ExecJoin2StreamAndPropertyPerformance());
        }
    
        [Test]
        public void TestExecJoin2StreamExprPerformance() {
            RegressionRunner.Run(new ExecJoin2StreamExprPerformance());
        }
    
        [Test]
        public void TestExecJoin2StreamInKeywordPerformance() {
            RegressionRunner.Run(new ExecJoin2StreamInKeywordPerformance());
        }
    
        [Test]
        public void TestExecJoin2StreamRangePerformance() {
            RegressionRunner.Run(new ExecJoin2StreamRangePerformance());
        }
    
        [Test]
        public void TestExecJoin2StreamSimplePerformance() {
            RegressionRunner.Run(new ExecJoin2StreamSimplePerformance());
        }
    
        [Test]
        public void TestExecJoin2StreamSimpleCoercionPerformance() {
            RegressionRunner.Run(new ExecJoin2StreamSimpleCoercionPerformance());
        }
    
        [Test]
        public void TestExecJoin3StreamAndPropertyPerformance() {
            RegressionRunner.Run(new ExecJoin3StreamAndPropertyPerformance());
        }
    
        [Test]
        public void TestExecJoin3StreamCoercionPerformance() {
            RegressionRunner.Run(new ExecJoin3StreamCoercionPerformance());
        }
    
        [Test]
        public void TestExecJoin3StreamInKeywordPerformance() {
            RegressionRunner.Run(new ExecJoin3StreamInKeywordPerformance());
        }
    
        [Test]
        public void TestExecJoin3StreamOuterJoinCoercionPerformance() {
            RegressionRunner.Run(new ExecJoin3StreamOuterJoinCoercionPerformance());
        }
    
        [Test]
        public void TestExecJoin3StreamRangePerformance() {
            RegressionRunner.Run(new ExecJoin3StreamRangePerformance());
        }
    
        [Test]
        public void TestExecJoin5StreamPerformance() {
            RegressionRunner.Run(new ExecJoin5StreamPerformance());
        }
    }
} // end of namespace
