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

namespace com.espertech.esper.regression.resultset.outputlimit
{
    [TestFixture]
    public class TestSuiteOutputLimit
    {
        [Test]
        public void TestExecOutputLimitAfter() {
            RegressionRunner.Run(new ExecOutputLimitAfter());
        }
    
        [Test]
        public void TestExecOutputLimitAggregateAll() {
            RegressionRunner.Run(new ExecOutputLimitAggregateAll());
        }
    
        [Test]
        public void TestExecOutputLimitChangeSetOpt() {
            RegressionRunner.Run(new ExecOutputLimitChangeSetOpt());
        }
    
        [Test]
        public void TestExecOutputLimitCrontabWhen() {
            RegressionRunner.Run(new ExecOutputLimitCrontabWhen());
        }
    
        [Test]
        public void TestExecOutputLimitGroupByEventPerGroup() {
            RegressionRunner.Run(new ExecOutputLimitGroupByEventPerGroup());
        }
    
        [Test]
        public void TestExecOutputLimitGroupByEventPerRow() {
            RegressionRunner.Run(new ExecOutputLimitGroupByEventPerRow());
        }
    
        [Test]
        public void TestExecOutputLimitFirstHaving() {
            RegressionRunner.Run(new ExecOutputLimitFirstHaving());
        }
    
        [Test]
        public void TestExecOutputLimitMicrosecondResolution() {
            RegressionRunner.Run(new ExecOutputLimitMicrosecondResolution());
        }
    
        [Test]
        public void TestExecOutputLimitParameterizedByContext() {
            RegressionRunner.Run(new ExecOutputLimitParameterizedByContext());
        }
    
        [Test]
        public void TestExecOutputLimitRowLimit() {
            RegressionRunner.Run(new ExecOutputLimitRowLimit());
        }
    
        [Test]
        public void TestExecOutputLimitRowForAll() {
            RegressionRunner.Run(new ExecOutputLimitRowForAll());
        }
    
        [Test]
        public void TestExecOutputLimitSimple() {
            RegressionRunner.Run(new ExecOutputLimitSimple());
        }
    
    }
} // end of namespace
