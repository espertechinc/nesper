///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.suite.epl.database;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLDatabaseWConfig
    {
        private void Run(EPLDatabaseQueryResultCache exec)
        {
            var session = RegressionRunner.Session();
            var configDB = GetDefaultConfig(session.Container);
            if (exec.IsLru) {
                configDB.SetLRUCache(exec.LruSize.Value);
            }
            else {
                configDB.SetExpiryTimeCache(
                    exec.ExpiryMaxAgeSeconds.Value,
                    exec.ExpiryPurgeIntervalSeconds.Value);
            }

            session.Configuration.Common.AddDatabaseReference("MyDB", configDB);
            session.Configuration.Common.AddEventType(typeof(SupportBean_S0));

            RegressionRunner.Run(session, exec);

            session.Destroy();
        }

        private ConfigurationCommonDBRef GetDefaultConfig(IContainer container)
        {
            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                container,
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDB.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            return configDB;
        }

        [Test]
        public void TestEPLDatabaseQueryResultCache()
        {
            Run(new EPLDatabaseQueryResultCache(false, null, 1d, double.MaxValue, 5000L, 1000, false));
            Run(new EPLDatabaseQueryResultCache(true, 100, null, null, 2000L, 1000, false));
            Run(new EPLDatabaseQueryResultCache(true, 100, null, null, 7000L, 25000, false));
            Run(new EPLDatabaseQueryResultCache(false, null, 2d, 2d, 7000L, 25000, false));
            Run(new EPLDatabaseQueryResultCache(false, null, 1d, 1d, 7000L, 25000, true));
        }
    }
} // end of namespace