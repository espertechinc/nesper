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

namespace com.espertech.esper.regression.epl.other
{
    [TestFixture]
    public class TestSuiteOther
    {
        [Test]
        public void TestExecEPLComments() {
            RegressionRunner.Run(new ExecEPLComments());
        }
    
        [Test]
        public void TestExecEPLCreateExpression() {
            RegressionRunner.Run(new ExecEPLCreateExpression());
        }
    
        [Test]
        public void TestExecEPLDistinct() {
            RegressionRunner.Run(new ExecEPLDistinct());
        }
    
        [Test]
        public void TestExecEPLDistinctWildcardJoinPattern() {
            RegressionRunner.Run(new ExecEPLDistinctWildcardJoinPattern());
        }
    
        [Test]
        public void TestExecEPLForGroupDelivery() {
            RegressionRunner.Run(new ExecEPLForGroupDelivery());
        }
    
        [Test]
        public void TestExecEPLInvalid() {
            RegressionRunner.Run(new ExecEPLInvalid());
        }
    
        [Test]
        public void TestExecEPLIStreamRStreamKeywords() {
            RegressionRunner.Run(new ExecEPLIStreamRStreamKeywords());
        }
    
        [Test]
        public void TestExecEPLIStreamRStreamConfigSelectorRStream() {
            RegressionRunner.Run(new ExecEPLIStreamRStreamConfigSelectorRStream());
        }
    
        [Test]
        public void TestExecEPLIStreamRStreamConfigSelectorIRStream() {
            RegressionRunner.Run(new ExecEPLIStreamRStreamConfigSelectorIRStream());
        }
    
        [Test]
        public void TestExecEPLLiteralConstants() {
            RegressionRunner.Run(new ExecEPLLiteralConstants());
        }
    
        [Test]
        public void TestExecEPLSchema() {
            RegressionRunner.Run(new ExecEPLSchema());
        }
    
        [Test]
        public void TestExecEPLSplitStream() {
            RegressionRunner.Run(new ExecEPLSplitStream());
        }
    
        [Test]
        public void TestExecEPLUpdate() {
            RegressionRunner.Run(new ExecEPLUpdate());
        }
    
        [Test]
        public void TestExecEPLUpdateMapIndexProps() {
            RegressionRunner.Run(new ExecEPLUpdateMapIndexProps());
        }
    
        [Test]
        public void TestExecEPLStaticFunctions() {
            RegressionRunner.Run(new ExecEPLStaticFunctions());
        }
    
        [Test]
        public void TestExecEPLStaticFunctionsNoUDFCache() {
            RegressionRunner.Run(new ExecEPLStaticFunctionsNoUDFCache());
        }
    
        [Test]
        public void TestExecEPLSelectExpr() {
            RegressionRunner.Run(new ExecEPLSelectExpr());
        }
    
        [Test]
        public void TestExecEPLSelectWildcardWAdditional() {
            RegressionRunner.Run(new ExecEPLSelectWildcardWAdditional());
        }
    
        [Test]
        public void TestExecEPLSelectExprSQLCompat() {
            RegressionRunner.Run(new ExecEPLSelectExprSQLCompat());
        }
    
        [Test]
        public void TestExecEPLSelectExprEventBeanAnnotation() {
            RegressionRunner.Run(new ExecEPLSelectExprEventBeanAnnotation());
        }
    
        [Test]
        public void TestExecEPLSelectExprStreamSelector() {
            RegressionRunner.Run(new ExecEPLSelectExprStreamSelector());
        }
    
        [Test]
        public void TestExecEPLPlanExcludeHint() {
            RegressionRunner.Run(new ExecEPLPlanExcludeHint());
        }
    
        [Test]
        public void TestExecEPLPlanInKeywordQuery() {
            RegressionRunner.Run(new ExecEPLPlanInKeywordQuery());
        }
    
        [Test]
        public void TestExecEPLStreamExpr() {
            RegressionRunner.Run(new ExecEPLStreamExpr());
        }
    
        [Test]
        public void TestExecEPLSelectJoin() {
            RegressionRunner.Run(new ExecEPLSelectJoin());
        }
    
        [Test]
        public void TestExecEPLPatternEventProperties() {
            RegressionRunner.Run(new ExecEPLPatternEventProperties());
        }
    
        [Test]
        public void TestExecEPLPatternQueries() {
            RegressionRunner.Run(new ExecEPLPatternQueries());
        }
    }
} // end of namespace
