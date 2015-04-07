///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestArrayBackedCollection 
    {
        private ArrayBackedCollection<int?> _coll;
    
        [SetUp]
        public void SetUp()
        {
            _coll = new ArrayBackedCollection<int?>(5);
        }
    
        [Test]
        public void TestGet()
        {
            Assert.AreEqual(0, _coll.Count);
            Assert.AreEqual(5, _coll.Array.Length);
    
            _coll.Add(5);
            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 5, null, null, null, null }, _coll.Array);
            _coll.Add(4);
            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 5, 4, null, null, null }, _coll.Array);
            Assert.AreEqual(2, _coll.Count);
    
            _coll.Add(1);
            _coll.Add(2);
            _coll.Add(3);
            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 5, 4, 1, 2, 3 }, _coll.Array);
            Assert.AreEqual(5, _coll.Count);
    
            _coll.Add(10);
            EPAssertionUtil.AssertEqualsExactOrder(new int?[] { 5, 4, 1, 2, 3, 10, null, null, null, null }, _coll.Array);
            Assert.AreEqual(6, _coll.Count);
    
            _coll.Add(11);
            _coll.Add(12);
            _coll.Add(13);
            _coll.Add(14);
            _coll.Add(15);

            EPAssertionUtil.AssertEqualsExactOrder(_coll.Array, new int?[]{5, 4, 1, 2, 3, 10, 11, 12, 13, 14, 15,
                null, null, null, null, null, null, null, null, null
            });

            Assert.AreEqual(11, _coll.Count);
    
            _coll.Clear();
            Assert.AreEqual(0, _coll.Count);
        }
    }
}
