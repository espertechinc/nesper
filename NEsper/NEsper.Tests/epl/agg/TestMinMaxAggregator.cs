///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.type;

using NUnit.Framework;

namespace com.espertech.esper.epl.agg
{
    [TestFixture]
    public class TestMinMaxAggregator 
    {
        [Test]
        public void TestAggregatorMax()
        {
            AggregatorMinMax agg = new AggregatorMinMax(MinMaxTypeEnum.MAX);
            Assert.AreEqual(null, agg.Value);
            agg.Enter(10);
            Assert.AreEqual(10, agg.Value);
            agg.Enter(20);
            Assert.AreEqual(20, agg.Value);
            agg.Enter(10);
            Assert.AreEqual(20, agg.Value);
            agg.Leave(10);
            Assert.AreEqual(20, agg.Value);
            agg.Leave(20);
            Assert.AreEqual(10, agg.Value);
            agg.Leave(10);
            Assert.AreEqual(null, agg.Value);
        }
    
        [Test]
        public void TestAggregatorMin()
        {
            AggregatorMinMax agg = new AggregatorMinMax(MinMaxTypeEnum.MIN);
            Assert.AreEqual(null, agg.Value);
            agg.Enter(10);
            Assert.AreEqual(10, agg.Value);
            agg.Enter(20);
            Assert.AreEqual(10, agg.Value);
            agg.Enter(10);
            Assert.AreEqual(10, agg.Value);
            agg.Leave(10);
            Assert.AreEqual(10, agg.Value);
            agg.Leave(20);
            Assert.AreEqual(10, agg.Value);
            agg.Leave(10);
            Assert.AreEqual(null, agg.Value);
        }
    }
}
