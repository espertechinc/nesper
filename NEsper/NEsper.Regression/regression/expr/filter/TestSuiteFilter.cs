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

namespace com.espertech.esper.regression.expr.filter
{
    [TestFixture]
    public class TestSuiteFilter
    {
        [Test]
        public void TestExecFilterExpressionsOptimizable() {
            RegressionRunner.Run(new ExecFilterExpressionsOptimizable());
        }
    
        [Test]
        public void TestExecFilterInAndBetween() {
            RegressionRunner.Run(new ExecFilterInAndBetween());
        }
    
        [Test]
        public void TestExecFilterExpressions() {
            RegressionRunner.Run(new ExecFilterExpressions());
        }
    
        [Test]
        public void TestExecFilterLargeThreading() {
            RegressionRunner.Run(new ExecFilterLargeThreading());
        }
    
        [Test]
        public void TestExecFilterWhereClauseNoDataWindowPerformance() {
            RegressionRunner.Run(new ExecFilterWhereClauseNoDataWindowPerformance());
        }
    }
} // end of namespace
