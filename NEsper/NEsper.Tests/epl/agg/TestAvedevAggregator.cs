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
    public class TestAvedevAggregator
    {
        [Test]
        public void TestAggregateFunction()
        {
            var agg = new AggregatorAvedev();

            Assert.IsNull(agg.Value);

            agg.Enter(82);
            Assert.AreEqual(0D, agg.Value);

            agg.Enter(78);
            Assert.AreEqual(2D, agg.Value);

            agg.Enter(70);
            var result = (double) agg.Value;
            Assert.AreEqual(result.ToString().Substring(0,6),"4.4444");

            agg.Enter(58);
            Assert.AreEqual(8D, agg.Value);

            agg.Enter(42);
            Assert.AreEqual(12.8D, agg.Value);

            agg.Leave(82);
            Assert.AreEqual(12D, agg.Value);

            agg.Leave(58);
            result = (double) agg.Value;
            Assert.AreEqual(result.ToString().Substring(0,7),"14.2222");
        }
    }
}
