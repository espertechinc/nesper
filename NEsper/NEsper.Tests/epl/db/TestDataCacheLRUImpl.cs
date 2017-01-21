///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.join.table;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestDataCacheLRUImpl 
    {
        private DataCacheLRUImpl _cache;
        private readonly EventTable[] _lists = new EventTable[10];
    
        [SetUp]
        public void SetUp()
        {
            _cache = new DataCacheLRUImpl(3);
            for (int i = 0; i < _lists.Length; i++)
            {
                _lists[i] = new UnindexedEventTableImpl(0);
            }
        }
    
        [Test]
        public void TestGet()
        {
            Assert.IsNull(_cache.GetCached(Make("a")));
            Assert.IsTrue(_cache.IsActive);

            _cache.PutCached(Make("a"), new EventTable[] { _lists[0] });     // a
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"))[0]);

            _cache.PutCached(Make("b"), new EventTable[] { _lists[1] });     // b, a
            Assert.AreSame(_lists[1], _cache.GetCached(Make("b"))[0]); // b, a

            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"))[0]); // a, b

            _cache.PutCached(Make("c"), new EventTable[] { _lists[2] });     // c, a, b
            _cache.PutCached(Make("d"), new EventTable[] { _lists[3] });     // d, c, a  (b gone)

            Assert.IsNull(_cache.GetCached(Make("b")));

            Assert.AreSame(_lists[2], _cache.GetCached(Make("c"))[0]); // c, d, a
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"))[0]); // a, c, d

            _cache.PutCached(Make("e"), new EventTable[] { _lists[4] }); // e, a, c (d and b gone)
    
            Assert.IsNull(_cache.GetCached(Make("d")));
            Assert.IsNull(_cache.GetCached(Make("b")));
        }
    
        private Object[] Make(String key)
        {
            return new Object[] {key};
        }
    }
}
