///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestSuiteDB
    {
        [Test]
        public void TestExecDatabase2StreamOuterJoin() {
            RegressionRunner.Run(new ExecDatabase2StreamOuterJoin());
        }
    
        [Test]
        public void TestExecDatabase3StreamOuterJoin() {
            RegressionRunner.Run(new ExecDatabase3StreamOuterJoin());
        }
    
        [Test]
        public void TestExecDatabaseDMConnFactory() {
            RegressionRunner.Run(new ExecDatabaseDMConnFactory());
        }
    
        [Test]
        public void TestExecDatabaseHintHook() {
            RegressionRunner.Run(new ExecDatabaseHintHook());
        }
    
        [Test]
        public void TestExecDatabaseJoin() {
            RegressionRunner.Run(new ExecDatabaseJoin());
        }
    
        [Test]
        public void TestExecDatabaseJoinInsertInto() {
            RegressionRunner.Run(new ExecDatabaseJoinInsertInto());
        }
    
        [Test]
        public void TestExecDatabaseJoinOptions() {
            RegressionRunner.Run(new ExecDatabaseJoinOptions());
        }
    
        [Test]
        public void TestExecDatabaseJoinOptionUppercase() {
            RegressionRunner.Run(new ExecDatabaseJoinOptionUppercase());
        }
    
        [Test]
        public void TestExecDatabaseJoinOptionLowercase() {
            RegressionRunner.Run(new ExecDatabaseJoinOptionLowercase());
        }
    
        [Test]
        public void TestExecDatabaseJoinPerfNoCache() {
            RegressionRunner.Run(new ExecDatabaseJoinPerfNoCache());
        }
    
        [Test]
        public void TestExecDatabaseJoinPerfWithCache() {
            RegressionRunner.Run(new ExecDatabaseJoinPerfWithCache());
        }
    
        [Test]
        public void TestExecDatabaseNoJoinIterate() {
            RegressionRunner.Run(new ExecDatabaseNoJoinIterate());
        }
    
        [Test]
        public void TestExecDatabaseNoJoinIteratePerf() {
            RegressionRunner.Run(new ExecDatabaseNoJoinIteratePerf());
        }
    
        [Test]
        public void TestExecDatabaseOuterJoinWCache() {
            RegressionRunner.Run(new ExecDatabaseOuterJoinWCache());
        }
    
        [Test]
        public void TestExecDatabaseQueryResultCache() {
            RegressionRunner.Run(new ExecDatabaseQueryResultCache(false, null, 1d, double.MaxValue, 5000L, 1000, false));
            RegressionRunner.Run(new ExecDatabaseQueryResultCache(true, 100, null, null, 2000L, 1000, false));
            RegressionRunner.Run(new ExecDatabaseQueryResultCache(true, 100, null, null, 7000L, 25000, false));
            RegressionRunner.Run(new ExecDatabaseQueryResultCache(false, null, 2d, 2d, 7000L, 25000, false));
            RegressionRunner.Run(new ExecDatabaseQueryResultCache(false, null, 1d, 1d, 7000L, 25000, true));
        }
    }
} // end of namespace
