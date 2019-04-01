///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.supportunit.epl;

using NUnit.Framework;

namespace com.espertech.esper.epl.expression.ops
{
    [TestFixture]
    public class TestUniqueValueAggregator 
    {
        private AggregatorDistinctValue _agg;
    
        [SetUp]
        public void SetUp()
        {
            _agg = new AggregatorDistinctValue(new SupportAggregator());
        }
    
        [Test]
        public void TestEnter()
        {
            _agg.Enter(1);
            _agg.Enter(10);
            _agg.Enter(null);
        }
    
        [Test]
        public void TestLeave()
        {
            _agg.Enter(1);
            _agg.Leave(1);
        }
    
        [Test]
        public void TestGetValue()
        {
            Assert.AreEqual(0, _agg.Value);
    
            _agg.Enter(10);
            Assert.AreEqual(10, _agg.Value);
    
            _agg.Enter(10);
            Assert.AreEqual(10, _agg.Value);
    
            _agg.Enter(2);
            Assert.AreEqual(12, _agg.Value);
    
            _agg.Leave(10);
            Assert.AreEqual(12, _agg.Value);
    
            _agg.Leave(10);
            Assert.AreEqual(2, _agg.Value);
    
            _agg.Leave(2);
            Assert.AreEqual(0, _agg.Value);
        }
    }
}
