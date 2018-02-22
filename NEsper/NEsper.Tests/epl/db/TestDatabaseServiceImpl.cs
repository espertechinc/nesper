///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.support;
using com.espertech.esper.schedule;
using com.espertech.esper.supportunit.epl;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.timer;

using NUnit.Framework;

namespace com.espertech.esper.epl.db
{
    [TestFixture]
    public class TestDatabaseServiceImpl
    {
        private IContainer _container;
        private DatabaseConfigServiceImpl _databaseServiceImpl;
    
        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var configs = new Dictionary<String, ConfigurationDBRef>();
    
            var config = new ConfigurationDBRef();
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
    
            SchedulingService schedulingService = new SchedulingServiceImpl(
                new TimeSourceServiceImpl(), _container.Resolve<ILockManager>());
            _databaseServiceImpl = new DatabaseConfigServiceImpl(
                configs, schedulingService,  new ScheduleBucket(1), 
                SupportEngineImportServiceFactory.Make(_container));
        }
    
        [Test]
        public void TestGetConnection()
        {
            var factory = _databaseServiceImpl.GetConnectionFactory("name1");
            Assert.IsTrue(factory is DatabaseDriverConnFactory);
    
            factory = _databaseServiceImpl.GetConnectionFactory("name2");
            Assert.IsTrue(factory is DatabaseDriverConnFactory);
        }
    
        [Test]
        public void TestGetCache()
        {
            var statementContext = SupportStatementContextFactory.MakeContext(_container);

            var dataCacheFactory = new DataCacheFactory();

            Assert.That(_databaseServiceImpl.GetDataCache("name1", null, null, dataCacheFactory, 0), Is.InstanceOf<DataCacheNullImpl>());

            var lru = (DataCacheLRUImpl)_databaseServiceImpl.GetDataCache("name2", statementContext, null, dataCacheFactory, 0);
            Assert.AreEqual(10000, lru.CacheSize);

            var exp = (DataCacheExpiringImpl)_databaseServiceImpl.GetDataCache("name3", statementContext, null, dataCacheFactory, 0);
            Assert.AreEqual(1.0d, exp.MaxAgeSec);
            Assert.AreEqual(3.0d, exp.PurgeIntervalSec);
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
                Log.Debug(ex.Message, ex);
                // expected
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
