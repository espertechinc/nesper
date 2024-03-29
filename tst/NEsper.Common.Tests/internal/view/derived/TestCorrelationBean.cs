///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class TestCorrelationBean : AbstractCommonTest
    {
        private readonly int PRECISION_DIGITS = 6;

        [Test]
        public void TestCORREL()
        {
            var stat = new BaseStatisticsBean();

            ClassicAssert.AreEqual(double.NaN, stat.Correlation);
            ClassicAssert.AreEqual(0, stat.N);

            stat.AddPoint(1, 10);
            ClassicAssert.AreEqual(double.NaN, stat.Correlation);
            ClassicAssert.AreEqual(1, stat.N);

            stat.AddPoint(2, 20);
            ClassicAssert.AreEqual(1d, stat.Correlation);
            ClassicAssert.AreEqual(2, stat.N);

            stat.AddPoint(1.5, 14);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation, 0.993399268, PRECISION_DIGITS));
            ClassicAssert.AreEqual(3, stat.N);

            stat.AddPoint(1.4, 14);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation, 0.992631989, PRECISION_DIGITS));
            ClassicAssert.AreEqual(4, stat.N);

            stat.RemovePoint(1.5, 14);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation, 1, PRECISION_DIGITS));
            ClassicAssert.AreEqual(3, stat.N);

            stat.AddPoint(100, 1);
            ClassicAssert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation, -0.852632057, PRECISION_DIGITS));
            ClassicAssert.AreEqual(4, stat.N);
        }
    }
} // end of namespace
