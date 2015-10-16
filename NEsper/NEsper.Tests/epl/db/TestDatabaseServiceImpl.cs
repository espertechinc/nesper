///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.schedule;
using com.espertech.esper.support.epl;
using com.espertech.esper.timer;

using NUnit.Framework;


namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestDatabaseServiceImpl 
    {
        private DatabaseConfigServiceImpl _databaseServiceImpl;
    
        [SetUp]
        public void SetUp()
        {
            IDictionary<String, ConfigurationDBRef> configs = new Dictionary<String, ConfigurationDBRef>();
    
            ConfigurationDBRef config = new ConfigurationDBRef();
            config.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative); 
            configs["name1"] = config;
    
            config = new ConfigurationDBRef();
            config.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative, new Properties());
            //config.SetDataSourceConnection("context", new Properties());
            config.LRUCache = 10000;
            configs["name2"] = config;
    
            config = new ConfigurationDBRef();
            config.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative, new Properties());
            config.SetExpiryTimeCache(1, 3);
            configs["name3"] = config;
    
            SchedulingService schedulingService = new SchedulingServiceImpl(new TimeSourceServiceImpl());
            _databaseServiceImpl = new DatabaseConfigServiceImpl(configs, schedulingService, new ScheduleBucket(1));
        }
    
        [Test]
        public void TestGetConnection()
        {
            DatabaseConnectionFactory factory = _databaseServiceImpl.GetConnectionFactory("name1");
            Assert.IsTrue(factory is DatabaseDriverConnFactory);
    
            factory = _databaseServiceImpl.GetConnectionFactory("name2");
            Assert.IsTrue(factory is DatabaseDriverConnFactory);
        }
    
        [Test]
        public void TestGetCache()
        {
            Assert.IsTrue(_databaseServiceImpl.GetDataCache("name1", null) is DataCacheNullImpl);
    
            DataCacheLRUImpl lru = (DataCacheLRUImpl) _databaseServiceImpl.GetDataCache("name2", null);
            Assert.AreEqual(10000, lru.CacheSize);
    
            DataCacheExpiringImpl exp = (DataCacheExpiringImpl) _databaseServiceImpl.GetDataCache("name3", null);
            Assert.AreEqual(1000, exp.MaxAgeMSec);
            Assert.AreEqual(3000, exp.PurgeIntervalMSec);
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                _databaseServiceImpl.GetConnectionFactory("xxx");
                Assert.Fail();
            }
            catch (DatabaseConfigException ex)
            {
                Log.Debug(string.Empty, ex);
                // expected
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
