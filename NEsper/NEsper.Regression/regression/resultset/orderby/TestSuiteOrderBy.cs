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

namespace com.espertech.esper.regression.resultset.orderby
{
    [TestFixture]
    public class TestSuiteOrderBy
    {
        [Test]
        public void TestExecOrderByRowPerEvent() {
            RegressionRunner.Run(new ExecOrderByRowPerEvent());
        }
    
        [Test]
        public void TestExecOrderByGroupByEventPerGroup() {
            RegressionRunner.Run(new ExecOrderByGroupByEventPerGroup());
        }
    
        [Test]
        public void TestExecOrderByGroupByEventPerRow() {
            RegressionRunner.Run(new ExecOrderByGroupByEventPerRow());
        }
    
        [Test]
        public void TestExecOrderByRowForAll() {
            RegressionRunner.Run(new ExecOrderByRowForAll());
        }
    
        [Test]
        public void TestExecOrderBySelfJoin() {
            RegressionRunner.Run(new ExecOrderBySelfJoin());
        }
    
        [Test]
        public void TestExecOrderBySimpleSortCollator() {
            RegressionRunner.Run(new ExecOrderBySimpleSortCollator());
        }
    
        [Test]
        public void TestExecOrderBySimple() {
            RegressionRunner.Run(new ExecOrderBySimple());
        }
    
    }
} // end of namespace
