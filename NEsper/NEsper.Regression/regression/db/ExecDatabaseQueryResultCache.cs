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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseQueryResultCache : RegressionExecution
    {
        private readonly bool _lru;
        private readonly int? _lruSize;
        private readonly double? _expiryMaxAgeSeconds;
        private readonly double? _expiryPurgeIntervalSeconds;
        private readonly long _assertMaximumTime;
        private readonly int _numEvents;
        private readonly bool _useRandomKeyLookup;
    
        public ExecDatabaseQueryResultCache(
            bool lru, 
            int? lruSize, 
            double? expiryMaxAgeSeconds, 
            double? expiryPurgeIntervalSeconds, 
            long assertMaximumTime, 
            int numEvents, 
            bool useRandomKeyLookup)
        {
            _lru = lru;
            _lruSize = lruSize;
            _expiryMaxAgeSeconds = expiryMaxAgeSeconds;
            _expiryPurgeIntervalSeconds = expiryPurgeIntervalSeconds;
            _assertMaximumTime = assertMaximumTime;
            _numEvents = numEvents;
            _useRandomKeyLookup = useRandomKeyLookup;
        }

        public override void Configure(Configuration configuration)
        {
            var configDB = GetDefaultConfig();
            if (_lru)
            {
                configDB.LRUCache = _lruSize.Value;
            }
            else
            {
                configDB.SetExpiryTimeCache(
                    _expiryMaxAgeSeconds.Value,
                    _expiryPurgeIntervalSeconds.Value);
            }

            configuration.AddDatabaseReference("MyDB", configDB);
        }

        public override void Run(EPServiceProvider epService)
        {
            TryCache(epService, _assertMaximumTime, _numEvents, _useRandomKeyLookup);
        }

        private void TryCache(
            EPServiceProvider epService, long assertMaximumTime, int numEvents, bool useRandomLookupKey)
        {
            var startTime = PerformanceObserver.MilliTime;
            TrySendEvents(epService, numEvents, useRandomLookupKey);
            var endTime = PerformanceObserver.MilliTime;
            Log.Info(".tryCache delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < assertMaximumTime);
        }

        private void TrySendEvents(EPServiceProvider engine, int numEvents, bool useRandomLookupKey)
        {
            var random = new Random();
            var stmtText = "select myint from " +
                           typeof(SupportBean_S0).FullName + " as s0," +
                           " sql:MyDB ['select myint from mytesttable where ${id} = mytesttable.mybigint'] as s1";

            var statement = engine.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            listener = new SupportUpdateListener();
            statement.Events += listener.Update;

            Log.Debug(".trySendEvents Sending " + numEvents + " events");
            for (var i = 0; i < numEvents; i++)
            {
                var id = 0;
                if (useRandomLookupKey)
                {
                    id = random.Next(1000);
                }
                else
                {
                    id = i % 10 + 1;
                }

                var bean = new SupportBean_S0(id);
                engine.EPRuntime.SendEvent(bean);

                if ((!useRandomLookupKey) || ((id >= 1) && (id <= 10)))
                {
                    var received = listener.AssertOneGetNewAndReset();
                    Assert.AreEqual(id * 10, received.Get("myint"));
                }
            }

            Log.Debug(".trySendEvents Stopping statement");
            statement.Stop();
        }

        private ConfigurationDBRef GetDefaultConfig()
        {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            return configDB;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
