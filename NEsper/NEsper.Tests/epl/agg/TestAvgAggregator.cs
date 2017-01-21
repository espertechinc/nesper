///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.aggregator;

using NUnit.Framework;

namespace com.espertech.esper.epl.agg
{
    [TestFixture]
    public class TestAvgAggregator 
    {
        [Test]
        public void TestResult()
        {
            var agg = new AggregatorAvg();
            agg.Enter(100);
            Assert.AreEqual(100d, agg.Value);
            agg.Enter(150);
            Assert.AreEqual(125d, agg.Value);
            agg.Enter(200);
            Assert.AreEqual(150d, agg.Value);
            agg.Leave(100);
            Assert.AreEqual(175d, agg.Value);
        }
    
    }
}
