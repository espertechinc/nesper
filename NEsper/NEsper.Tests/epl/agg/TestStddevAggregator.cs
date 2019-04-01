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
    public class TestStddevAggregator 
    {
        [Test]
        public void TestAggregateFunction()
        {
            AggregationMethod agg = new AggregatorStddev();
    
            Assert.IsNull(agg.Value);
    
            agg.Enter(10);
            Assert.IsNull(agg.Value);
    
            agg.Enter(8);
            double result = agg.Value.AsDouble();
            Assert.AreEqual("1.4142", result.ToString().Substring(0, 6));
    
            agg.Enter(5);
            result = agg.Value.AsDouble();
            Assert.AreEqual("2.5166", result.ToString().Substring(0, 6));
    
            agg.Enter(9);
            result = agg.Value.AsDouble();
            Assert.AreEqual("2.1602", result.ToString().Substring(0, 6));
    
            agg.Leave(10);
            result = agg.Value.AsDouble();
            Assert.AreEqual("2.0816", result.ToString().Substring(0, 6));
        }
    
        [Test]
        public void TestAllOne() {
            AggregationMethod agg = new AggregatorStddev();
            agg.Enter(1);
            agg.Enter(1);
            agg.Enter(1);
            agg.Enter(1);
            agg.Enter(1);
            Assert.AreEqual(0.0d, agg.Value);
        }
    
    }
    
    
}
