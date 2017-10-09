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
    public class TestCorrelationBean 
    {
        private readonly int PRECISION_DIGITS = 6;
    
        [Test]
        public void TestCORREL()
        {
            BaseStatisticsBean stat = new BaseStatisticsBean();
    
            Assert.AreEqual(Double.NaN, stat.Correlation);
            Assert.AreEqual(0, stat.N);
    
            stat.AddPoint(1, 10);
            Assert.AreEqual(Double.NaN, stat.Correlation);
            Assert.AreEqual(1, stat.N);
    
            stat.AddPoint(2, 20);
            Assert.AreEqual(1d, stat.Correlation);
            Assert.AreEqual(2, stat.N);
    
            stat.AddPoint(1.5, 14);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation,0.993399268, PRECISION_DIGITS));
            Assert.AreEqual(3, stat.N);
    
            stat.AddPoint(1.4, 14);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation,0.992631989, PRECISION_DIGITS));
            Assert.AreEqual(4, stat.N);
    
            stat.RemovePoint(1.5, 14);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation,1, PRECISION_DIGITS));
            Assert.AreEqual(3, stat.N);
    
            stat.AddPoint(100, 1);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stat.Correlation,-0.852632057, PRECISION_DIGITS));
            Assert.AreEqual(4, stat.N);
        }
    }
}
