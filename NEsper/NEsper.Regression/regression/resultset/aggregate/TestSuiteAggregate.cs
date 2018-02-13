///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.resultset.aggregate
{
    [TestFixture]
    public class TestSuiteAggregate
    {
        [Test]
        public void TestExecAggregateFirstLastWindow() {
            RegressionRunner.Run(new ExecAggregateFirstLastWindow());
        }
    
        [Test]
        public void TestExecAggregateMinMaxBy() {
            RegressionRunner.Run(new ExecAggregateMinMaxBy());
        }
    
        [Test]
        public void TestExecAggregateExtInvalid() {
            RegressionRunner.Run(new ExecAggregateExtInvalid());
        }
    
        [Test]
        public void TestExecAggregateLeaving() {
            RegressionRunner.Run(new ExecAggregateLeaving());
        }
    
        [Test]
        public void TestExecAggregateNTh() {
            RegressionRunner.Run(new ExecAggregateNTh());
        }
    
        [Test]
        public void TestExecAggregateRate() {
            RegressionRunner.Run(new ExecAggregateRate());
        }
    
        [Test]
        public void TestExecAggregateFiltered() {
            RegressionRunner.Run(new ExecAggregateFiltered());
        }
    
        [Test]
        public void TestExecAggregateFilteredWMathContext() {
            RegressionRunner.Run(new ExecAggregateFilteredWMathContext());
        }
    
        [Test]
        public void TestExecAggregateFilterNamedParameter() {
            RegressionRunner.Run(new ExecAggregateFilterNamedParameter());
        }
    
        [Test]
        public void TestExecAggregateLocalGroupBy() {
            RegressionRunner.Run(new ExecAggregateLocalGroupBy());
        }
    
        [Test]
        public void TestExecAggregateFirstEverLastEver() {
            RegressionRunner.Run(new ExecAggregateFirstEverLastEver());
        }
    
        [Test]
        public void TestExecAggregateCount() {
            RegressionRunner.Run(new ExecAggregateCount());
        }
    
        [Test]
        public void TestExexAggregateCountWGroupBy() {
            RegressionRunner.Run(new ExexAggregateCountWGroupBy());
        }
    
        [Test]
        public void TestExecAggregateMaxMinGroupBy() {
            RegressionRunner.Run(new ExecAggregateMaxMinGroupBy());
        }
    
        [Test]
        public void TestExecAggregateMedianAndDeviation() {
            RegressionRunner.Run(new ExecAggregateMedianAndDeviation());
        }
    
        [Test]
        public void TestExecAggregateMinMax() {
            RegressionRunner.Run(new ExecAggregateMinMax());
        }
    
    }
} // end of namespace
