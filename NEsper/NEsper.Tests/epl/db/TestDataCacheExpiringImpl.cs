///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.timer;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestDataCacheExpiringImpl 
    {
        private SupportSchedulingServiceImpl _scheduler;
        private DataCacheExpiringImpl _cache;
        private readonly EventTable[] _lists = new EventTable[10];
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            for (var i = 0; i < _lists.Length; i++)
            {
                _lists[i] = new UnindexedEventTableImpl(0);
            }
        }
    
        [Test]
        public void TestPurgeInterval()
        {
            var scheduler = new SchedulingServiceImpl(new TimeSourceServiceImpl(), _container);
            _cache = new DataCacheExpiringImpl(10, 20, ConfigurationCacheReferenceType.HARD, scheduler, 1, null, TimeAbacusMilliseconds.INSTANCE);   // age 10 sec, purge 1000 seconds
    
            // test single entry in cache
            scheduler.Time = 5000;
            _cache.PutCached(Make("a"), 1, new EventTable[] {_lists[0 ]}); // a at 5 sec
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"), 1)[0]);
    
            scheduler.Time = 26000;
            SupportSchedulingServiceImpl.EvaluateSchedule(scheduler);
            Assert.AreEqual(0, _cache.Count);
    
            // test 4 entries in cache
            scheduler.Time = 30000;
            _cache.PutCached(Make("b"), 1, new EventTable[] { _lists[1] });  // b at 30 sec
    
            scheduler.Time = 35000;
            _cache.PutCached(Make("c"), 1, new EventTable[] { _lists[2] });  // c at 35 sec
    
            scheduler.Time = 40000;
            _cache.PutCached(Make("d"), 1, new EventTable[] { _lists[3] });  // d at 40 sec
    
            scheduler.Time = 45000;
            _cache.PutCached(Make("e"), 1, new EventTable[] { _lists[4] });  // d at 40 sec
    
            scheduler.Time = 50000;
            SupportSchedulingServiceImpl.EvaluateSchedule(scheduler);
            Assert.AreEqual(2, _cache.Count);   // only d and e
    
            Assert.AreSame(_lists[3], _cache.GetCached(Make("d"), 1)[0]);
            Assert.AreSame(_lists[4], _cache.GetCached(Make("e"), 1)[0]);
        }
    
        [Test]
        public void TestGet()
        {
            _scheduler = new SupportSchedulingServiceImpl();
            _cache = new DataCacheExpiringImpl(10, 1000, ConfigurationCacheReferenceType.HARD, _scheduler, 1, null, TimeAbacusMilliseconds.INSTANCE);   // age 10 sec, purge 1000 seconds
    
            Assert.IsNull(_cache.GetCached(Make("a"), 1));
    
            _scheduler.Time = 5000;
            _cache.PutCached(Make("a"), 1, new EventTable[] { _lists[0] }); // a at 5 sec
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"), 1)[0]);
    
            _scheduler.Time = 10000;
            _cache.PutCached(Make("b"), 1, new EventTable[] { _lists[1] }); // b at 10 sec
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"), 1)[0]);
            Assert.AreSame(_lists[1], _cache.GetCached(Make("b"), 1)[0]);
    
            _scheduler.Time = 11000;
            _cache.PutCached(Make("c"), 1, new EventTable[] { _lists[2] }); // c at 11 sec
            _cache.PutCached(Make("d"), 1, new EventTable[] { _lists[3] }); // d at 11 sec
    
            _scheduler.Time = 14999;
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"), 1)[0]);
    
            _scheduler.Time = 15000;
            Assert.AreSame(_lists[0], _cache.GetCached(Make("a"), 1)[0]);
    
            _scheduler.Time = 15001;
            Assert.IsNull(_cache.GetCached(Make("a"), 1));
    
            _scheduler.Time = 15001;
            Assert.IsNull(_cache.GetCached(Make("a"), 1));
    
            _scheduler.Time = 15001;
            Assert.IsNull(_cache.GetCached(Make("a"), 1));
            Assert.AreSame(_lists[1], _cache.GetCached(Make("b"), 1)[0]);
            Assert.AreSame(_lists[2], _cache.GetCached(Make("c"), 1)[0]);
            Assert.AreSame(_lists[3], _cache.GetCached(Make("d"), 1)[0]);
    
            _scheduler.Time = 20000;
            Assert.AreSame(_lists[1], _cache.GetCached(Make("b"), 1)[0]);
    
            _scheduler.Time = 20001;
            Assert.IsNull(_cache.GetCached(Make("b"), 1));
    
            _scheduler.Time = 21001;
            Assert.IsNull(_cache.GetCached(Make("a"), 1));
            Assert.IsNull(_cache.GetCached(Make("b"), 1));
            Assert.IsNull(_cache.GetCached(Make("c"), 1));
            Assert.IsNull(_cache.GetCached(Make("d"), 1));
    
            _scheduler.Time = 22000;
            _cache.PutCached(Make("b"), 1, new EventTable[] { _lists[1] }); // b at 22 sec
            _cache.PutCached(Make("d"), 1, new EventTable[] { _lists[3] }); // d at 22 sec
    
            _scheduler.Time = 32000;
            Assert.AreSame(_lists[1], _cache.GetCached(Make("b"), 1)[0]);
            Assert.AreSame(_lists[3], _cache.GetCached(Make("d"), 1)[0]);
        }
    
        private object[] Make(string key)
        {
            return new object[] {key};
        }
    }
}
