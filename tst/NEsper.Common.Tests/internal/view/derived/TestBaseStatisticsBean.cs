///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.supportunit.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.view.derived
{
    [TestFixture]
    public class TestBaseStatisticsBean : AbstractCommonTest
    {
        private readonly int PRECISION_DIGITS = 6;

        [Test]
        public void TestAddRemoveXOnly()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(double.NaN, stat.XAverage);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(10);
            stat.AddPoint(20);
            ClassicAssert.AreEqual(15d, stat.XAverage);
            ClassicAssert.AreEqual(0d, stat.YAverage);
            ClassicAssert.AreEqual(2, stat.N);

            stat.RemovePoint(10);
            ClassicAssert.AreEqual(20d, stat.XAverage);
            ClassicAssert.AreEqual(0d, stat.YAverage);
            ClassicAssert.AreEqual(1, stat.N);
        }

        [Test]
        public void TestAverage()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(double.NaN, stat.XAverage);
            ClassicAssert.AreEqual(double.NaN, stat.YAverage);
            ClassicAssert.AreEqual(0, stat.N);

            stat.RemovePoint(2, 363636);
            ClassicAssert.AreEqual(double.NaN, stat.XAverage);
            ClassicAssert.AreEqual(double.NaN, stat.YAverage);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(10, -2);
            ClassicAssert.AreEqual(10d, stat.XAverage);
            ClassicAssert.AreEqual(-2d, stat.YAverage);
            ClassicAssert.AreEqual(1, stat.N);

            stat.AddPoint(20, 4);
            ClassicAssert.AreEqual(15d, stat.XAverage);
            ClassicAssert.AreEqual(1d, stat.YAverage);
            ClassicAssert.AreEqual(2, stat.N);

            stat.AddPoint(1, 4);
            ClassicAssert.AreEqual(31d / 3d, stat.XAverage);
            ClassicAssert.AreEqual(6d / 3d, stat.YAverage);
            ClassicAssert.AreEqual(3, stat.N);

            stat.AddPoint(1, -10);
            ClassicAssert.AreEqual(8d, stat.XAverage);
            ClassicAssert.AreEqual(-4d / 4d, stat.YAverage);
            ClassicAssert.AreEqual(4, stat.N);

            stat.AddPoint(-32, -11);
            ClassicAssert.AreEqual(0d, stat.XAverage);
            ClassicAssert.AreEqual(-15d / 5d, stat.YAverage);
            ClassicAssert.AreEqual(5, stat.N);

            stat.RemovePoint(-32, -10);
            ClassicAssert.AreEqual(32d / 4d, stat.XAverage);
            ClassicAssert.AreEqual(-5d / 4d, stat.YAverage);
            ClassicAssert.AreEqual(4, stat.N);

            stat.RemovePoint(8, -5);
            ClassicAssert.AreEqual(24d / 3d, stat.XAverage);
            ClassicAssert.AreEqual(0d, stat.YAverage);
            ClassicAssert.AreEqual(3, stat.N);

            stat.RemovePoint(2, 50);
            ClassicAssert.AreEqual(22d / 2d, stat.XAverage);
            ClassicAssert.AreEqual(-50d / 2d, stat.YAverage);
            ClassicAssert.AreEqual(2, stat.N);

            stat.RemovePoint(1, 1);
            ClassicAssert.AreEqual(21d / 1d, stat.XAverage);
            ClassicAssert.AreEqual(-51d, stat.YAverage);
            ClassicAssert.AreEqual(1, stat.N);

            stat.RemovePoint(3, 3);
            ClassicAssert.AreEqual(double.NaN, stat.XAverage);
            ClassicAssert.AreEqual(double.NaN, stat.YAverage);
            ClassicAssert.AreEqual(0, stat.N);
        }

        [Test]
        public void TestClone()
        {
            var stat = new BaseStatisticsBean();

            stat.AddPoint(100, 10);
            stat.AddPoint(200, 20);

            var cloned = (BaseStatisticsBean) stat.Clone();
            ClassicAssert.AreEqual(2, cloned.N);
            ClassicAssert.AreEqual(300d, cloned.XSum);
            ClassicAssert.AreEqual(150d, cloned.XAverage);
            ClassicAssert.AreEqual(30d, cloned.YSum);
            ClassicAssert.AreEqual(15d, cloned.YAverage);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(cloned.XStandardDeviationPop, 50.0, PRECISION_DIGITS));
        }

        [Test]
        public void TestStddev_STDEVPA()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(double.NaN, stat.XStandardDeviationPop);
            ClassicAssert.AreEqual(double.NaN, stat.YStandardDeviationPop);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(1, 10500);
            ClassicAssert.AreEqual(0.0d, stat.XStandardDeviationPop);
            ClassicAssert.AreEqual(0.0d, stat.YStandardDeviationPop);
            ClassicAssert.AreEqual(1, stat.N);

            stat.AddPoint(2, 10200);
            ClassicAssert.AreEqual(0.5d, stat.XStandardDeviationPop);
            ClassicAssert.AreEqual(150d, stat.YStandardDeviationPop);
            ClassicAssert.AreEqual(2, stat.N);

            stat.AddPoint(1.5, 10500);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop, 0.40824829, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop, 141.4213562, PRECISION_DIGITS));
            ClassicAssert.AreEqual(3, stat.N);

            stat.AddPoint(-0.1, 10500);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop, 0.777817459, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop, 129.9038106, PRECISION_DIGITS));
            ClassicAssert.AreEqual(4, stat.N);

            stat.RemovePoint(2, 10200);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop, 0.668331255, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop, 0.0, PRECISION_DIGITS));
            ClassicAssert.AreEqual(3, stat.N);

            stat.AddPoint(0.89, 10499);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop, 0.580102362, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop, 0.433012702, PRECISION_DIGITS));
            ClassicAssert.AreEqual(4, stat.N);

            stat.AddPoint(1.23, 10500);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationPop, 0.543860276, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationPop, 0.4, PRECISION_DIGITS));
            ClassicAssert.AreEqual(5, stat.N);
        }

        [Test]
        public void TestStddevAndVariance_STDEV()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(double.NaN, stat.XStandardDeviationSample);
            ClassicAssert.AreEqual(double.NaN, stat.XVariance);
            ClassicAssert.AreEqual(double.NaN, stat.YStandardDeviationSample);
            ClassicAssert.AreEqual(double.NaN, stat.YVariance);

            stat.AddPoint(100, -1);
            ClassicAssert.AreEqual(double.NaN, stat.XVariance);
            ClassicAssert.AreEqual(double.NaN, stat.XStandardDeviationSample);
            ClassicAssert.AreEqual(double.NaN, stat.YVariance);
            ClassicAssert.AreEqual(double.NaN, stat.YStandardDeviationSample);

            stat.AddPoint(150, -1);
            ClassicAssert.AreEqual(1250d, stat.XVariance);
            ClassicAssert.AreEqual(0d, stat.YVariance);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample, 35.35533906, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample, 0, PRECISION_DIGITS));

            stat.AddPoint(0, -1.1);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance, 5833.33333333, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance, 0.003333333, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample, 76.37626158, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample, 0.057735027, PRECISION_DIGITS));

            stat.RemovePoint(100, -1);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance, 11250, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance, 0.005, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample, 106.0660172, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample, 0.070710678, PRECISION_DIGITS));

            stat.AddPoint(-149, 0);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XVariance, 22350.333333, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YVariance, 0.37, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.XStandardDeviationSample, 149.5002787, PRECISION_DIGITS));
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.YStandardDeviationSample, 0.608276253, PRECISION_DIGITS));
        }

        [Test]
        public void TestSum()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(0d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(10, -2);
            ClassicAssert.AreEqual(10d, stat.XSum);
            ClassicAssert.AreEqual(-2d, stat.YSum);
            ClassicAssert.AreEqual(1, stat.N);

            stat.AddPoint(3.5, -3);
            ClassicAssert.AreEqual(13.5d, stat.XSum);
            ClassicAssert.AreEqual(-5d, stat.YSum);
            ClassicAssert.AreEqual(2, stat.N);

            stat.AddPoint(1, 5);
            ClassicAssert.AreEqual(14.5d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(3, stat.N);

            stat.RemovePoint(9, 1.5);
            ClassicAssert.AreEqual(5.5d, stat.XSum);
            ClassicAssert.AreEqual(-1.5d, stat.YSum);
            ClassicAssert.AreEqual(2, stat.N);

            stat.RemovePoint(9.5, -1.5);
            ClassicAssert.AreEqual(-4d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(1, stat.N);

            stat.RemovePoint(1, -1);
            ClassicAssert.AreEqual(0d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(0, stat.N);

            stat.RemovePoint(1, 1);
            ClassicAssert.AreEqual(0d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(1.11, -3.333);
            ClassicAssert.AreEqual(1.11d, stat.XSum);
            ClassicAssert.AreEqual(-3.333d, stat.YSum);
            ClassicAssert.AreEqual(1, stat.N);

            stat.AddPoint(2.22, 3.333);
            ClassicAssert.AreEqual(1.11d + 2.22d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(2, stat.N);

            stat.AddPoint(-3.32, 0);
            ClassicAssert.AreEqual(1.11d + 2.22d - 3.32d, stat.XSum);
            ClassicAssert.AreEqual(0d, stat.YSum);
            ClassicAssert.AreEqual(3, stat.N);
        }
    }
} // end of namespace
