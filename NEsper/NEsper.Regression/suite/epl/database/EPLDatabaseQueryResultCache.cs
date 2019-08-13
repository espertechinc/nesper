///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseQueryResultCache : RegressionExecution
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly long assertMaximumTime;
        private readonly int numEvents;
        private readonly bool useRandomKeyLookup;

        public EPLDatabaseQueryResultCache(
            bool lru,
            int? lruSize,
            double? expiryMaxAgeSeconds,
            double? expiryPurgeIntervalSeconds,
            long assertMaximumTime,
            int numEvents,
            bool useRandomKeyLookup)
        {
            IsLru = lru;
            LruSize = lruSize;
            ExpiryMaxAgeSeconds = expiryMaxAgeSeconds;
            ExpiryPurgeIntervalSeconds = expiryPurgeIntervalSeconds;
            this.assertMaximumTime = assertMaximumTime;
            this.numEvents = numEvents;
            this.useRandomKeyLookup = useRandomKeyLookup;
        }

        public bool IsLru { get; }

        public int? LruSize { get; }

        public double? ExpiryMaxAgeSeconds { get; }

        public double? ExpiryPurgeIntervalSeconds { get; }

        public void Run(RegressionEnvironment env)
        {
            TryCache(env, assertMaximumTime, numEvents, useRandomKeyLookup);
        }

        private static void TryCache(
            RegressionEnvironment env,
            long assertMaximumTime,
            int numEvents,
            bool useRandomLookupKey)
        {
            var startTime = PerformanceObserver.MilliTime;
            TrySendEvents(env, numEvents, useRandomLookupKey);
            var endTime = PerformanceObserver.MilliTime;
            log.Info(".tryCache delta=" + (endTime - startTime));
            Assert.IsTrue(endTime - startTime < assertMaximumTime);
        }

        private static void TrySendEvents(
            RegressionEnvironment env,
            int numEvents,
            bool useRandomLookupKey)
        {
            var random = new Random();
            var stmtText = "@Name('s0') select myint from " +
                           "SupportBean_S0 as s0," +
                           " sql:MyDB ['select myint from mytesttable where ${Id} = mytesttable.myBigint'] as s1";
            env.CompileDeploy(stmtText).AddListener("s0");

            log.Debug(".trySendEvents Sending " + numEvents + " events");
            for (var i = 0; i < numEvents; i++) {
                var id = 0;
                if (useRandomLookupKey) {
                    id = random.Next(1000);
                }
                else {
                    id = i % 10 + 1;
                }

                var bean = new SupportBean_S0(id);
                env.SendEventBean(bean);

                if (!useRandomLookupKey || id >= 1 && id <= 10) {
                    var received = env.Listener("s0").AssertOneGetNewAndReset();
                    Assert.AreEqual(id * 10, received.Get("myint"));
                }
            }

            log.Debug(".trySendEvents Stopping statement");
            env.UndeployAll();
        }
    }
} // end of namespace