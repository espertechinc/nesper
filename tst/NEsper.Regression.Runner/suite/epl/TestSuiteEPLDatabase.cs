///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.suite.epl.database;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    [Category("DatabaseTest")]
    [Category("IntegrationTest")]
    public class TestSuiteEPLDatabase : AbstractTestBase
    {
        public TestSuiteEPLDatabase() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
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

            common.AddImportNamespace(typeof(HookType));
            
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
        public void TestEPLDatabaseDataSourceFactory()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseDataSourceFactory());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinInsertInto()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseJoinInsertInto());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinOptionLowercase()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseJoinOptionLowercase());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinOptionUppercase()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseJoinOptionUppercase());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseJoinPerfNoCache()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseJoinPerfNoCache());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseNoJoinIteratePerf()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseNoJoinIteratePerf());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLDatabaseOuterJoinWCache()
        {
            RegressionRunner.RunPerformanceSensitive(_session, new EPLDatabaseOuterJoinWCache());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabase2StreamOuterJoin
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabase2StreamOuterJoin : AbstractTestBase
        {
            public TestEPLDatabase2StreamOuterJoin() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOuterJoinReversedOnFilter() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinReversedOnFilter());

            [Test, RunInApplicationDomain]
            public void WithRightOuterJoinOnFilter() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithRightOuterJoinOnFilter());

            [Test, RunInApplicationDomain]
            public void WithLeftOuterJoinOnFilter() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithLeftOuterJoinOnFilter());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinLeftS1() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinLeftS1());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinRightS0() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinRightS0());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinFullS1() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinFullS1());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinFullS0() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinFullS0());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinRightS1() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinRightS1());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinLeftS0() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase2StreamOuterJoin.WithOuterJoinLeftS0());
        }

        /// <summary>
        /// Auto-test(s): EPLDatabaseJoin
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabaseJoin : AbstractTestBase
        {
            public TestEPLDatabaseJoin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSimpleJoinRight() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithSimpleJoinRight());

            [Test, RunInApplicationDomain]
            public void WithRestartStatement() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithRestartStatement());

            [Test, RunInApplicationDomain]
            public void WithPropertyResolution() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithPropertyResolution());

            [Test, RunInApplicationDomain]
            public void WithWithPattern() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithWithPattern());

            [Test, RunInApplicationDomain]
            public void WithStreamNamesAndRename() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithStreamNamesAndRename());

            [Test, RunInApplicationDomain]
            public void WithInvalidSubviews() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalidSubviews());

            [Test, RunInApplicationDomain]
            public void WithInvalid1Stream() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalid1Stream());

            [Test, RunInApplicationDomain]
            public void WithInvalidPropertyHistorical() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalidPropertyHistorical());

            [Test, RunInApplicationDomain]
            public void WithInvalidPropertyEvent() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalidPropertyEvent());

            [Test, RunInApplicationDomain]
            public void WithInvalidBothHistorical() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalidBothHistorical());

            [Test, RunInApplicationDomain]
            public void WithInvalidSQL() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithInvalidSQL());

            [Test, RunInApplicationDomain]
            public void WithVariables() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithVariables());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchCompile() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithTimeBatchCompile());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchOM() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithTimeBatchOM());

            [Test, RunInApplicationDomain]
            public void WithTimeBatch() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithTimeBatch());

            [Test, RunInApplicationDomain]
            public void With3Stream() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.With3Stream());

            [Test, RunInApplicationDomain]
            public void With2HistoricalStarInner() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.With2HistoricalStarInner());

            [Test, RunInApplicationDomain]
            public void With2HistoricalStar() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.With2HistoricalStar());

            [Test, RunInApplicationDomain]
            public void WithSimpleJoinLeft() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoin.WithSimpleJoinLeft());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabaseJoinPerfWithCache
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabaseJoinPerfWithCache : AbstractTestBase
        {
            public TestEPLDatabaseJoinPerfWithCache() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithInKeywordMultiIndex() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithInKeywordMultiIndex());

            [Test, RunInApplicationDomain]
            public void WithInKeywordSingleIndex() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithInKeywordSingleIndex());

            [Test, RunInApplicationDomain]
            public void WithOuterJoinPlusWhere() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithOuterJoinPlusWhere());

            [Test, RunInApplicationDomain]
            public void With2StreamOuterJoin() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.With2StreamOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithSelectLargeResultSetCoercion() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithSelectLargeResultSetCoercion());

            [Test, RunInApplicationDomain]
            public void WithSelectLargeResultSet() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithSelectLargeResultSet());

            [Test, RunInApplicationDomain]
            public void WithKeyAndRangeIndex() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithKeyAndRangeIndex());

            [Test, RunInApplicationDomain]
            public void WithRangeIndex() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithRangeIndex());

            [Test, RunInApplicationDomain]
            public void WithConstants() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinPerfWithCache.WithConstants());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabase3StreamOuterJoin
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase3StreamOuterJoin.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabase3StreamOuterJoin : AbstractTestBase
        {
            public TestEPLDatabase3StreamOuterJoin() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOuterJoinLeftS0() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase3StreamOuterJoin.WithOuterJoinLeftS0());

            [Test, RunInApplicationDomain]
            public void WithInnerJoinLeftS0() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabase3StreamOuterJoin.WithInnerJoinLeftS0());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabaseHintHook
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseHintHook.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabaseHintHook : AbstractTestBase
        {
            public TestEPLDatabaseHintHook() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithOutputRowConversion() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseHintHook.WithOutputRowConversion());

            [Test, RunInApplicationDomain]
            public void WithInputParameterConversion() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseHintHook.WithInputParameterConversion());

            [Test, RunInApplicationDomain]
            public void WithOutputColumnConversion() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseHintHook.WithOutputColumnConversion());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabaseJoinOptions
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinOptions.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabaseJoinOptions : AbstractTestBase
        {
            public TestEPLDatabaseJoinOptions() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithPlaceholderWhere() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinOptions.WithPlaceholderWhere());

            [Test, RunInApplicationDomain]
            public void WithNoMetaLexAnalysisGroup() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinOptions.WithNoMetaLexAnalysisGroup());

            [Test, RunInApplicationDomain]
            public void WithNoMetaLexAnalysis() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseJoinOptions.WithNoMetaLexAnalysis());
        }
        
        /// <summary>
        /// Auto-test(s): EPLDatabaseNoJoinIterate
        /// <code>
        /// RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseNoJoinIterate.Executions());
        /// </code>
        /// </summary>

        public class TestEPLDatabaseNoJoinIterate : AbstractTestBase
        {
            public TestEPLDatabaseNoJoinIterate() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithVariablesPoll() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseNoJoinIterate.WithVariablesPoll());

            [Test, RunInApplicationDomain]
            public void WithExpressionPoll() => RegressionRunner.RunPerformanceSensitive(_session, EPLDatabaseNoJoinIterate.WithExpressionPoll());
        }
    }
} // end of namespace