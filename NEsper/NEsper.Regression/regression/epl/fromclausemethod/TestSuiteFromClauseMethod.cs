///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    [TestFixture]
    public class TestSuiteFromClauseMethod
    {
        [Test]
        public void TestExecFromClauseMethod() {
            RegressionRunner.Run(new ExecFromClauseMethod());
        }
    
        [Test]
        public void TestExecFromClauseMethodCacheExpiry() {
            RegressionRunner.Run(new ExecFromClauseMethodCacheExpiry());
        }
    
        [Test]
        public void TestExecFromClauseMethodCacheLRU() {
            RegressionRunner.Run(new ExecFromClauseMethodCacheLRU());
        }
    
        [Test]
        public void TestExecFromClauseMethodNStream() {
            RegressionRunner.Run(new ExecFromClauseMethodNStream());
        }
    
        [Test]
        public void TestExecFromClauseMethodOuterNStream() {
            RegressionRunner.Run(new ExecFromClauseMethodOuterNStream());
        }
    
        [Test]
        public void TestExecFromClauseMethodVariable() {
            RegressionRunner.Run(new ExecFromClauseMethodVariable());
        }
    
        [Test]
        public void TestExecFromClauseMethodJoinPerformance() {
            RegressionRunner.Run(new ExecFromClauseMethodJoinPerformance());
        }
    }
} // end of namespace
