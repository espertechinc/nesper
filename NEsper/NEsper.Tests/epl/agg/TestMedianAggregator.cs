///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;

using NUnit.Framework;

namespace com.espertech.esper.epl.agg
{
    [TestFixture]
    public class TestMedianAggregator 
    {
        [Test]
        public void TestAggregator()
        {
            var median = new AggregatorMedian();
            Assert.AreEqual(null, median.Value);
            median.Enter(10);
            Assert.AreEqual(10D, median.Value);
            median.Enter(20);
            Assert.AreEqual(15D, median.Value);
            median.Enter(10);
            Assert.AreEqual(10D, median.Value);
    
            median.Leave(10);
            Assert.AreEqual(15D, median.Value);
            median.Leave(10);
            Assert.AreEqual(20D, median.Value);
            median.Leave(20);
            Assert.AreEqual(null, median.Value);
        }
    }
    
    
}
