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

namespace com.espertech.esper.regression.resultset.querytype
{
    [TestFixture]
    public class TestSuiteQuerytype
    {
        [Test]
        public void TestExecQuerytypeRowForAll() {
            RegressionRunner.Run(new ExecQuerytypeRowForAll());
        }
    
        [Test]
        public void TestExecQuerytypeRowForAllHaving() {
            RegressionRunner.Run(new ExecQuerytypeRowForAllHaving());
        }
    
        [Test]
        public void TestExecQuerytypeRowPerEvent() {
            RegressionRunner.Run(new ExecQuerytypeRowPerEvent());
        }
    
        [Test]
        public void TestExecQuerytypeRowPerEventDistinct() {
            RegressionRunner.Run(new ExecQuerytypeRowPerEventDistinct());
        }
    
        [Test]
        public void TestExecQuerytypeRollupDimensionality() {
            RegressionRunner.Run(new ExecQuerytypeRollupDimensionality());
        }
    
        [Test]
        public void TestExecQuerytypeRollupGroupingFuncs() {
            RegressionRunner.Run(new ExecQuerytypeRollupGroupingFuncs());
        }
    
        [Test]
        public void TestExecQuerytypeRollupHavingAndOrderBy() {
            RegressionRunner.Run(new ExecQuerytypeRollupHavingAndOrderBy());
        }
    
        [Test]
        public void TestExecQuerytypeRollupOutputRate() {
            RegressionRunner.Run(new ExecQuerytypeRollupOutputRate());
        }
    
        [Test]
        public void TestExecQuerytypeRollupPlanningAndSODA() {
            RegressionRunner.Run(new ExecQuerytypeRollupPlanningAndSODA());
        }
    
        [Test]
        public void TestExecQuerytypeGroupByEventPerGroup() {
            RegressionRunner.Run(new ExecQuerytypeGroupByEventPerGroup());
        }
    
        [Test]
        public void TestExecQuerytypeGroupByEventPerGroupHaving() {
            RegressionRunner.Run(new ExecQuerytypeGroupByEventPerGroupHaving());
        }
    
        [Test]
        public void TestExecQuerytypeGroupByEventPerRow() {
            RegressionRunner.Run(new ExecQuerytypeGroupByEventPerRow());
        }
    
        [Test]
        public void TestExecQuerytypeGroupByEventPerRowHaving() {
            RegressionRunner.Run(new ExecQuerytypeGroupByEventPerRowHaving());
        }
    
        [Test]
        public void TestExecQuerytypeGroupByReclaimMicrosecondResolution() {
            RegressionRunner.Run(new ExecQuerytypeGroupByReclaimMicrosecondResolution());
        }
    
        [Test]
        public void TestExecQuerytypeWTimeBatch() {
            RegressionRunner.Run(new ExecQuerytypeWTimeBatch());
        }
    
        [Test]
        public void TestExecQuerytypeRowForAllWGroupedTimeWinUnique() {
            RegressionRunner.Run(new ExecQuerytypeRowForAllWGroupedTimeWinUnique());
        }
    
        [Test]
        public void TestExecQuerytypeGetEnumerator() {
            RegressionRunner.Run(new ExecQuerytypeIterator());
        }
    
        [Test]
        public void TestExecQuerytypeHaving() {
            RegressionRunner.Run(new ExecQuerytypeHaving());
        }
    
        [Test]
        public void TestExecQuerytypeRowPerEventPerformance() {
            RegressionRunner.Run(new ExecQuerytypeRowPerEventPerformance());
        }
    }
} // end of namespace
