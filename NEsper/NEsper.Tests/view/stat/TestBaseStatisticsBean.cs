///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.view.stat
{
    [TestFixture]
    public class TestBaseStatisticsBean 
    {
        private readonly int PRECISION_DIGITS = 6;
    
        [Test]
        public void TestAddRemoveXOnly()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(Double.NaN, stat.XAverage);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(10);
            stat.AddPoint(20);
            Assert.AreEqual(15d, stat.XAverage);
            Assert.AreEqual(0d, stat.YAverage);
            Assert.AreEqual(2, stat.N);
    
            stat.RemovePoint(10);
            Assert.AreEqual(20d, stat.XAverage);
            Assert.AreEqual(0d, stat.YAverage);
            Assert.AreEqual(1, stat.N);
        }
    
        [Test]
        public void TestAverage()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(Double.NaN, stat.XAverage);
            Assert.AreEqual(Double.NaN, stat.YAverage);
            Assert.AreEqual(0, stat.N);
    
            stat.RemovePoint(2, 363636);
            Assert.AreEqual(Double.NaN, stat.XAverage);
            Assert.AreEqual(Double.NaN, stat.YAverage);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(10, -2);
            Assert.AreEqual(10d, stat.XAverage);
            Assert.AreEqual(-2d, stat.YAverage);
            Assert.AreEqual(1, stat.N);
    
            stat.AddPoint(20, 4);
            Assert.AreEqual(15d, stat.XAverage);
            Assert.AreEqual(1d, stat.YAverage);
            Assert.AreEqual(2, stat.N);
    
            stat.AddPoint(1, 4);
            Assert.AreEqual(31d / 3d, stat.XAverage);
            Assert.AreEqual(6d / 3d, stat.YAverage);
            Assert.AreEqual(3, stat.N);
    
            stat.AddPoint(1, -10);
            Assert.AreEqual(8d, stat.XAverage);
            Assert.AreEqual(-4d / 4d, stat.YAverage);
            Assert.AreEqual(4, stat.N);
    
            stat.AddPoint(-32, -11);
            Assert.AreEqual(0d, stat.XAverage);
            Assert.AreEqual(-15d / 5d, stat.YAverage);
            Assert.AreEqual(5, stat.N);
    
            stat.RemovePoint(-32, -10);
            Assert.AreEqual(32d / 4d, stat.XAverage);
            Assert.AreEqual(-5d / 4d, stat.YAverage);
            Assert.AreEqual(4, stat.N);
    
            stat.RemovePoint(8, -5);
            Assert.AreEqual(24d / 3d, stat.XAverage);
            Assert.AreEqual(0d, stat.YAverage);
            Assert.AreEqual(3, stat.N);
    
            stat.RemovePoint(2, 50);
            Assert.AreEqual(22d / 2d, stat.XAverage);
            Assert.AreEqual(-50d / 2d, stat.YAverage);
            Assert.AreEqual(2, stat.N);
    
            stat.RemovePoint(1, 1);
            Assert.AreEqual(21d / 1d, stat.XAverage);
            Assert.AreEqual(-51d, stat.YAverage);
            Assert.AreEqual(1, stat.N);
    
            stat.RemovePoint(3, 3);
            Assert.AreEqual(Double.NaN, stat.XAverage);
            Assert.AreEqual(Double.NaN, stat.YAverage);
            Assert.AreEqual(0, stat.N);
        }
    
        [Test]
        public void TestSum()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(0d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(10, -2);
            Assert.AreEqual(10d, stat.XSum);
            Assert.AreEqual(-2d, stat.YSum);
            Assert.AreEqual(1, stat.N);
    
            stat.AddPoint(3.5, -3);
            Assert.AreEqual(13.5d, stat.XSum);
            Assert.AreEqual(-5d, stat.YSum);
            Assert.AreEqual(2, stat.N);
    
            stat.AddPoint(1,5);
            Assert.AreEqual(14.5d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(3, stat.N);
    
            stat.RemovePoint(9,1.5);
            Assert.AreEqual(5.5d, stat.XSum);
            Assert.AreEqual(-1.5d, stat.YSum);
            Assert.AreEqual(2, stat.N);
    
            stat.RemovePoint(9.5,-1.5);
            Assert.AreEqual(-4d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(1, stat.N);
    
            stat.RemovePoint(1,-1);
            Assert.AreEqual(0d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(0, stat.N);
    
            stat.RemovePoint(1,1);
            Assert.AreEqual(0d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(1.11,-3.333);
            Assert.AreEqual(1.11d, stat.XSum);
            Assert.AreEqual(-3.333d, stat.YSum);
            Assert.AreEqual(1, stat.N);
    
            stat.AddPoint(2.22,3.333);
            Assert.AreEqual(1.11d + 2.22d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(2, stat.N);
    
            stat.AddPoint(-3.32,0);
            Assert.AreEqual(1.11d + 2.22d - 3.32d, stat.XSum);
            Assert.AreEqual(0d, stat.YSum);
            Assert.AreEqual(3, stat.N);
        }
    
        [Test]
        public void TestStddev_STDEVPA()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(Double.NaN, stat.XStandardDeviationPop);
            Assert.AreEqual(Double.NaN, stat.YStandardDeviationPop);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(1,10500);
            Assert.AreEqual(0.0d, stat.XStandardDeviationPop);
            Assert.AreEqual(0.0d, stat.YStandardDeviationPop);
            Assert.AreEqual(1, stat.N);
    
            stat.AddPoint(2, 10200);
            Assert.AreEqual(0.5d, stat.XStandardDeviationPop);
            Assert.AreEqual(150d, stat.YStandardDeviationPop);
            Assert.AreEqual(2, stat.N);
    
            stat.AddPoint(1.5, 10500);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop,0.40824829, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop,141.4213562, PRECISION_DIGITS));
            Assert.AreEqual(3, stat.N);
    
            stat.AddPoint(-0.1,10500);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop,0.777817459, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop,129.9038106, PRECISION_DIGITS));
            Assert.AreEqual(4, stat.N);
    
            stat.RemovePoint(2,10200);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop,0.668331255, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop,0.0, PRECISION_DIGITS));
            Assert.AreEqual(3, stat.N);
    
            stat.AddPoint(0.89,10499);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop,0.580102362, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop,0.433012702, PRECISION_DIGITS));
            Assert.AreEqual(4, stat.N);
    
            stat.AddPoint(1.23,10500);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop,0.543860276, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop,0.4, PRECISION_DIGITS));
            Assert.AreEqual(5, stat.N);
        }
    
        [Test]
        public void TestStddevAndVariance_STDEV()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(Double.NaN, stat.XStandardDeviationSample);
            Assert.AreEqual(Double.NaN, stat.XVariance);
            Assert.AreEqual(Double.NaN, stat.YStandardDeviationSample);
            Assert.AreEqual(Double.NaN, stat.YVariance);
    
            stat.AddPoint(100, -1);
            Assert.AreEqual(Double.NaN, stat.XVariance);
            Assert.AreEqual(Double.NaN, stat.XStandardDeviationSample);
            Assert.AreEqual(Double.NaN, stat.YVariance);
            Assert.AreEqual(Double.NaN, stat.YStandardDeviationSample);
    
            stat.AddPoint(150, -1);
            Assert.AreEqual(1250d, stat.XVariance);
            Assert.AreEqual(0d, stat.YVariance);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample,35.35533906, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample,0, PRECISION_DIGITS));
    
            stat.AddPoint(0,-1.1);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance,5833.33333333, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance,0.003333333, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample,76.37626158, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample,0.057735027, PRECISION_DIGITS));
    
            stat.RemovePoint(100,-1);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance,11250, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance,0.005, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample,106.0660172, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample,0.070710678, PRECISION_DIGITS));
    
            stat.AddPoint(-149, 0);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance,22350.333333, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance,0.37, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample,149.5002787, PRECISION_DIGITS));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample,0.608276253, PRECISION_DIGITS));
        }
    
        [Test]
        public void TestClone()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            stat.AddPoint(100,10);
            stat.AddPoint(200,20);
    
            BaseStatisticsBean cloned = (BaseStatisticsBean) stat.Clone();
            Assert.AreEqual(2, cloned.N);
            Assert.AreEqual(300d, cloned.XSum);
            Assert.AreEqual(150d, cloned.XAverage);
            Assert.AreEqual(30d, cloned.YSum);
            Assert.AreEqual(15d, cloned.YAverage);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(cloned.XStandardDeviationPop,50.0, PRECISION_DIGITS));
        }
    }
}
