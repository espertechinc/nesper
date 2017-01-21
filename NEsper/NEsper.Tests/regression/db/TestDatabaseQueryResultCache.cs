///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseQueryResultCache 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [TearDown]
        public void TearDown()
        {
            _listener = null;
            _epService.Dispose();
        }
    
        [Test]
        public void TestExpireCacheNoPurge()
        {
            ConfigurationDBRef configDB = GetDefaultConfig();
            configDB.SetExpiryTimeCache(1.0d, double.MaxValue);
            TryCache(configDB, 5000, 1000, false);
        }
    
        [Test]
        public void TestLRUCache()
        {
            ConfigurationDBRef configDB = GetDefaultConfig();
            configDB.LRUCache = 100;
            TryCache(configDB, 2000, 1000, false);
        }
    
        [Test]
        public void TestLRUCache25k()
        {
            ConfigurationDBRef configDB = GetDefaultConfig();
            configDB.LRUCache = 100;
            TryCache(configDB, 7000, 25000, false);
        }
    
        [Test]
        public void TestExpireCache25k()
        {
            ConfigurationDBRef configDB = GetDefaultConfig();
            configDB.SetExpiryTimeCache(2, 2);
            TryCache(configDB, 7000, 25000, false);
        }
    
        [Test]
        public void TestExpireRandomKeys()
        {
            ConfigurationDBRef configDB = GetDefaultConfig();
            configDB.SetExpiryTimeCache(1, 1);
            TryCache(configDB, 7000, 25000, true);
        }
    
        private void TryCache(ConfigurationDBRef configDB, long assertMaximumTime, int numEvents, bool useRandomLookupKey)
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
    
            _epService = EPServiceProviderManager.GetProvider("TestDatabaseQueryResultCache", configuration);
            _epService.Initialize();
    
            long startTime = PerformanceObserver.MilliTime;
            TrySendEvents(_epService, numEvents, useRandomLookupKey);
            long endTime = PerformanceObserver.MilliTime;
            Log.Info(".tryCache " + configDB.DataCacheDesc + " delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < assertMaximumTime);
        }
    
        private void TrySendEvents(EPServiceProvider engine, int numEvents, bool useRandomLookupKey)
        {
            Random random = new Random();
            String stmtText = "select myint from " +
                    typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select myint from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = engine.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            Log.Debug(".trySendEvents Sending " + numEvents + " events");
            for (int i = 0; i < numEvents; i++)
            {
                int id = 0;
                if (useRandomLookupKey)
                {
                    id = random.Next(1000);
                }
                else
                {
                    id = i % 10 + 1;
                }
    
                SupportBean_S0 bean = new SupportBean_S0(id);
                engine.EPRuntime.SendEvent(bean);
    
                if ((!useRandomLookupKey) || ((id >= 1) && (id <= 10)))
                {
                    EventBean received = _listener.AssertOneGetNewAndReset();
                    Assert.AreEqual(id * 10, received.Get("myint"));
                }
            }
    
            Log.Debug(".trySendEvents Stopping statement");
            statement.Stop();
        }
    
        private static ConfigurationDBRef GetDefaultConfig()
        {
            var config = new ConfigurationDBRef();
            config.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            config.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            return config;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
