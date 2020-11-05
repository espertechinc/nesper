///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.suite.epl.database;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLDatabase
    {
        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            Configure(session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            session.Dispose();
            session = null;
        }

        private RegressionSession session;

        private static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBeanTwo),
                typeof(SupportBean_A),
                typeof(SupportBeanRange),
                typeof(SupportBean_S0),
                typeof(SupportBeanComplexProps)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            var common = configuration.Common;
            common.AddVariable("myvariableOCC", typeof(int), 10);
            common.AddVariable("myvariableIPC", typeof(string), "x10");
            common.AddVariable("myvariableORC", typeof(int), 10);

            var configDBWithRetain = new ConfigurationCommonDBRef();
            configDBWithRetain.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDBWithRetain.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configuration.Common.AddDatabaseReference("MyDBWithRetain", configDBWithRetain);

            var configDBWithPooledWithLRU100 = new ConfigurationCommonDBRef();
            configDBWithPooledWithLRU100.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDBWithPooledWithLRU100.ConnectionLifecycleEnum = ConnectionLifecycleEnum.POOLED;
            configDBWithPooledWithLRU100.SetLRUCache(100);
            configuration.Common.AddDatabaseReference("MyDBWithPooledWithLRU100", configDBWithPooledWithLRU100);

            var configDBWithTxnIso1WithReadOnly = new ConfigurationCommonDBRef();
            configDBWithTxnIso1WithReadOnly.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);

            configDBWithTxnIso1WithReadOnly.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configDBWithTxnIso1WithReadOnly.ConnectionCatalog = "test";
            configDBWithTxnIso1WithReadOnly.ConnectionReadOnly = true;
            configDBWithTxnIso1WithReadOnly.ConnectionTransactionIsolation = IsolationLevel.ReadCommitted;
            configDBWithTxnIso1WithReadOnly.ConnectionAutoCommit = true;
            configuration.Common.AddDatabaseReference("MyDBWithTxnIso1WithReadOnly", configDBWithTxnIso1WithReadOnly);

            var dbconfigLowerCase = GetDBConfig(configuration.Container);
            dbconfigLowerCase.ColumnChangeCase = ColumnChangeCaseEnum.LOWERCASE;
            dbconfigLowerCase.AddTypeBinding(typeof(int), typeof(string));
            configuration.Common.AddDatabaseReference("MyDBLowerCase", dbconfigLowerCase);

            var dbconfigUpperCase = GetDBConfig(configuration.Container);
            dbconfigUpperCase.ColumnChangeCase = ColumnChangeCaseEnum.UPPERCASE;
            configuration.Common.AddDatabaseReference("MyDBUpperCase", dbconfigUpperCase);

            var dbconfigPlain = GetDBConfig(configuration.Container);
            configuration.Common.AddDatabaseReference("MyDBPlain", dbconfigPlain);

            var configDBPooled = new ConfigurationCommonDBRef();
            configDBPooled.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDBPooled.ConnectionLifecycleEnum = ConnectionLifecycleEnum.POOLED;
            configuration.Common.AddDatabaseReference("MyDBPooled", configDBPooled);

            var configDBWithLRU100000 = new ConfigurationCommonDBRef();
            configDBWithLRU100000.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDBWithLRU100000.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configDBWithLRU100000.SetLRUCache(100000);
            configuration.Common.AddDatabaseReference("MyDBWithLRU100000", configDBWithLRU100000);

            var configDBWithExpiryTime = new ConfigurationCommonDBRef();
            configDBWithExpiryTime.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDBWithExpiryTime.ConnectionCatalog = "test";
            configDBWithExpiryTime.SetExpiryTimeCache(60, 120);
            configuration.Common.AddDatabaseReference("MyDBWithExpiryTime", configDBWithExpiryTime);

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Common.Logging.IsEnableADO = true;
        }

        internal static ConfigurationCommonDBRef GetDBConfig(IContainer container)
        {
            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDB.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionReadOnly = true;
            configDB.ConnectionAutoCommit = true;
            return configDB;
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabase2StreamOuterJoin()
        {
            RegressionRunner.Run(session, EPLDatabase2StreamOuterJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabase3StreamOuterJoin()
        {
            RegressionRunner.Run(session, EPLDatabase3StreamOuterJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseDataSourceFactory()
        {
            RegressionRunner.Run(session, new EPLDatabaseDataSourceFactory());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseHintHook()
        {
            RegressionRunner.Run(session, EPLDatabaseHintHook.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoin()
        {
            RegressionRunner.Run(session, EPLDatabaseJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinInsertInto()
        {
            RegressionRunner.Run(session, new EPLDatabaseJoinInsertInto());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinOptionLowercase()
        {
            RegressionRunner.Run(session, new EPLDatabaseJoinOptionLowercase());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinOptions()
        {
            RegressionRunner.Run(session, EPLDatabaseJoinOptions.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinOptionUppercase()
        {
            RegressionRunner.Run(session, new EPLDatabaseJoinOptionUppercase());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinPerfNoCache()
        {
            RegressionRunner.Run(session, new EPLDatabaseJoinPerfNoCache());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinPerfWithCache()
        {
            RegressionRunner.Run(session, EPLDatabaseJoinPerfWithCache.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseNoJoinIterate()
        {
            RegressionRunner.Run(session, EPLDatabaseNoJoinIterate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseNoJoinIteratePerf()
        {
            RegressionRunner.Run(session, new EPLDatabaseNoJoinIteratePerf());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseOuterJoinWCache()
        {
            RegressionRunner.Run(session, new EPLDatabaseOuterJoinWCache());
        }
    }
} // end of namespace