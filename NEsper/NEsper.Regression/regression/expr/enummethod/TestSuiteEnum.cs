///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.expr.enummethod
{
    [TestFixture]
    public class TestSuiteEnum
    {
        [Test]
        public void TestExecEnumAggregate() {
            RegressionRunner.Run(new ExecEnumAggregate());
        }
    
        [Test]
        public void TestExecEnumAllOfAnyOf() {
            RegressionRunner.Run(new ExecEnumAllOfAnyOf());
        }
    
        [Test]
        public void TestExecEnumAverage() {
            RegressionRunner.Run(new ExecEnumAverage());
        }
    
        [Test]
        public void TestExecEnumChained() {
            RegressionRunner.Run(new ExecEnumChained());
        }
    
        [Test]
        public void TestExecEnumCountOf() {
            RegressionRunner.Run(new ExecEnumCountOf());
        }
    
        [Test]
        public void TestExecEnumDataSources() {
            RegressionRunner.Run(new ExecEnumDataSources());
        }
    
        [Test]
        public void TestExecEnumDistinct() {
            RegressionRunner.Run(new ExecEnumDistinct());
        }
    
        [Test]
        public void TestExecEnumDocSamples() {
            RegressionRunner.Run(new ExecEnumDocSamples());
        }
    
        [Test]
        public void TestExecEnumExceptIntersectUnion() {
            RegressionRunner.Run(new ExecEnumExceptIntersectUnion());
        }
    
        [Test]
        public void TestExecEnumFirstLastOf() {
            RegressionRunner.Run(new ExecEnumFirstLastOf());
        }
    
        [Test]
        public void TestExecEnumGroupBy() {
            RegressionRunner.Run(new ExecEnumGroupBy());
        }
    
        [Test]
        public void TestExecEnumInvalid() {
            RegressionRunner.Run(new ExecEnumInvalid());
        }
    
        [Test]
        public void TestExecEnumMinMax() {
            RegressionRunner.Run(new ExecEnumMinMax());
        }
    
        [Test]
        public void TestExecEnumMinMaxBy() {
            RegressionRunner.Run(new ExecEnumMinMaxBy());
        }
    
        [Test]
        public void TestExecEnumMostLeastFrequent() {
            RegressionRunner.Run(new ExecEnumMostLeastFrequent());
        }
    
        [Test]
        public void TestExecEnumNamedWindowPerformance() {
            RegressionRunner.Run(new ExecEnumNamedWindowPerformance());
        }
    
        [Test]
        public void TestExecEnumNested() {
            RegressionRunner.Run(new ExecEnumNested());
        }
    
        [Test]
        public void TestExecEnumNestedPerformance() {
            RegressionRunner.Run(new ExecEnumNestedPerformance());
        }
    
        [Test]
        public void TestExecEnumOrderBy() {
            RegressionRunner.Run(new ExecEnumOrderBy());
        }
    
        [Test]
        public void TestExecEnumReverse() {
            RegressionRunner.Run(new ExecEnumReverse());
        }
    
        [Test]
        public void TestExecEnumSelectFrom() {
            RegressionRunner.Run(new ExecEnumSelectFrom());
        }
    
        [Test]
        public void TestExecEnumSequenceEqual() {
            RegressionRunner.Run(new ExecEnumSequenceEqual());
        }
    
        [Test]
        public void TestExecEnumSumOf() {
            RegressionRunner.Run(new ExecEnumSumOf());
        }
    
        [Test]
        public void TestExecEnumTakeAndTakeLast() {
            RegressionRunner.Run(new ExecEnumTakeAndTakeLast());
        }
    
        [Test]
        public void TestExecEnumTakeWhileAndWhileLast() {
            RegressionRunner.Run(new ExecEnumTakeWhileAndWhileLast());
        }
    
        [Test]
        public void TestExecEnumToMap() {
            RegressionRunner.Run(new ExecEnumToMap());
        }
    
        [Test]
        public void TestExecEnumWhere() {
            RegressionRunner.Run(new ExecEnumWhere());
        }
    }
} // end of namespace
