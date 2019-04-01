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

namespace com.espertech.esper.regression.expr.expr
{
    [TestFixture]
    public class TestSuiteExpr
    {
        [Test]
        public void TestExecExprAnyAllSomeExpr() {
            RegressionRunner.Run(new ExecExprAnyAllSomeExpr());
        }
    
        [Test]
        public void TestExecExprArrayExpression() {
            RegressionRunner.Run(new ExecExprArrayExpression());
        }
    
        [Test]
        public void TestExecExprBigNumberSupport() {
            RegressionRunner.Run(new ExecExprBigNumberSupport());
        }
    
        [Test]
        public void TestExecExprBigNumberSupportMathContext() {
            RegressionRunner.Run(new ExecExprBigNumberSupportMathContext());
        }
    
        [Test]
        public void TestExecExprBitWiseOperators() {
            RegressionRunner.Run(new ExecExprBitWiseOperators());
        }
    
        [Test]
        public void TestExecExprCaseExpr() {
            RegressionRunner.Run(new ExecExprCaseExpr());
        }
    
        [Test]
        public void TestExecExprCast() {
            RegressionRunner.Run(new ExecExprCast());
        }
    
        [Test]
        public void TestExecExprCastWStaticType() {
            RegressionRunner.Run(new ExecExprCastWStaticType());
        }
    
        [Test]
        public void TestExecExprCurrentEvaluationContext() {
            RegressionRunner.Run(new ExecExprCurrentEvaluationContext());
        }
    
        [Test]
        public void TestExecExprCurrentTimestamp() {
            RegressionRunner.Run(new ExecExprCurrentTimestamp());
        }
    
        [Test]
        public void TestExecExprDotExpression() {
            RegressionRunner.Run(new ExecExprDotExpression());
        }
    
        [Test]
        public void TestExecExprDotExpressionDuckTyping() {
            RegressionRunner.Run(new ExecExprDotExpressionDuckTyping());
        }
    
        [Test]
        public void TestExecExprExists() {
            RegressionRunner.Run(new ExecExprExists());
        }
    
        [Test]
        public void TestExecExprInBetweenLike() {
            RegressionRunner.Run(new ExecExprInBetweenLike());
        }
    
        [Test]
        public void TestExecExprInstanceOf() {
            RegressionRunner.Run(new ExecExprInstanceOf());
        }
    
        [Test]
        public void TestExecExprLikeRegexp() {
            RegressionRunner.Run(new ExecExprLikeRegexp());
        }
    
        [Test]
        public void TestExecExprMath() {
            RegressionRunner.Run(new ExecExprMath());
        }
    
        [Test]
        public void TestExecExprMathDivisionRules() {
            RegressionRunner.Run(new ExecExprMathDivisionRules());
        }
    
        [Test]
        public void TestExecExprNewInstance() {
            RegressionRunner.Run(new ExecExprNewInstance());
        }
    
        [Test]
        public void TestExecExprNewStruct() {
            RegressionRunner.Run(new ExecExprNewStruct());
        }
    
        [Test]
        public void TestExecExprConcat() {
            RegressionRunner.Run(new ExecExprConcat());
        }
    
        [Test]
        public void TestExecExprCoalesce() {
            RegressionRunner.Run(new ExecExprCoalesce());
        }
    
        [Test]
        public void TestExecExprOpModulo() {
            RegressionRunner.Run(new ExecExprOpModulo());
        }
    
        [Test]
        public void TestExecExprPrevious() {
            RegressionRunner.Run(new ExecExprPrevious());
        }
    
        [Test]
        public void TestExecExprPrior() {
            RegressionRunner.Run(new ExecExprPrior());
        }
    
        [Test]
        public void TestExecExprMinMaxNonAgg() {
            RegressionRunner.Run(new ExecExprMinMaxNonAgg());
        }
    
        [Test]
        public void TestExecExprTypeOf() {
            RegressionRunner.Run(new ExecExprTypeOf());
        }
    
    }
} // end of namespace
