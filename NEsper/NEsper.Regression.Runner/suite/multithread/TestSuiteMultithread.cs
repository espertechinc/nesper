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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.suite.multithread;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.regressionlib.support.wordexample;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.multithread
{
    [TestFixture]
    public class TestSuiteMultithread
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
                typeof(SupportMarketDataBean),
                typeof(SupportByteArrEventLongId),
                typeof(SupportBean_A),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportCollection),
                typeof(MultithreadStmtNamedWindowJoinUniqueView.MyEventA),
                typeof(MultithreadStmtNamedWindowJoinUniqueView.MyEventB),
                typeof(MultithreadStmtNamedWindowMultiple.OrderEvent),
                typeof(MultithreadStmtNamedWindowMultiple.OrderCancelEvent),
                typeof(SentenceEvent),
                typeof(SupportTradeEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            var configDB = new ConfigurationCommonDBRef();
            configDB.SetDatabaseDriver(
                SupportDatabaseService.DRIVER,
                SupportDatabaseService.DefaultProperties);
            configDB.ConnectionLifecycleEnum = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionReadOnly = true;
            configDB.ConnectionTransactionIsolation = IsolationLevel.ReadCommitted;
            configDB.ConnectionAutoCommit = true;
            configuration.Common.AddDatabaseReference("MyDB", configDB);

            var common = configuration.Common;
            common.AddVariable("var1", typeof(long?), 0);
            common.AddVariable("var2", typeof(long?), 0);
            common.AddVariable("var3", typeof(long?), 0);

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "intListAgg",
                typeof(SupportIntListAggregationForge));
        }

        private void PerformanceRun(RegressionExecution execution)
        {
            using (new PerformanceContext()) {
                RegressionRunner.Run(session, execution);
            }
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextCountSimple()
        {
            PerformanceRun(new MultithreadContextCountSimple());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextInitiatedTerminatedWithNowParallel()
        {
            PerformanceRun(new MultithreadContextInitiatedTerminatedWithNowParallel());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextPartitioned()
        {
            PerformanceRun(new MultithreadContextPartitioned());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextTemporalStartStop()
        {
            PerformanceRun(new MultithreadContextTemporalStartStop());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadContextUnique()
        {
            PerformanceRun(new MultithreadContextUnique());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeployAtomic()
        {
            PerformanceRun(new MultithreadDeployAtomic());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadDeterminismInsertInto()
        {
            PerformanceRun(new MultithreadDeterminismInsertInto());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadedViewTimeWindowSceneTwo()
        {
            PerformanceRun(new MultithreadViewTimeWindowSceneTwo());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadMultithreadContextPartitionedWCount()
        {
            PerformanceRun(new MultithreadContextPartitionedWCount());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadNamedWindowDelete()
        {
            PerformanceRun(new MultithreadNamedWindowDelete());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtDatabaseJoin()
        {
            PerformanceRun(new MultithreadStmtDatabaseJoin());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtFilter()
        {
            PerformanceRun(new MultithreadStmtFilter());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtFilterSubquery()
        {
            PerformanceRun(new MultithreadStmtFilterSubquery());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtInsertInto()
        {
            PerformanceRun(new MultithreadStmtInsertInto());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtIterate()
        {
            PerformanceRun(new MultithreadStmtIterate());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtJoin()
        {
            PerformanceRun(new MultithreadStmtJoin());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtListenerCreateStmt()
        {
            PerformanceRun(new MultithreadStmtListenerCreateStmt());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtListenerRoute()
        {
            PerformanceRun(new MultithreadStmtListenerRoute());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtMgmt()
        {
            PerformanceRun(new MultithreadStmtMgmt());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowConsume()
        {
            PerformanceRun(new MultithreadStmtNamedWindowConsume());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowDelete()
        {
            PerformanceRun(new MultithreadStmtNamedWindowDelete());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowFAF()
        {
            PerformanceRun(new MultithreadStmtNamedWindowFAF());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowIterate()
        {
            PerformanceRun(new MultithreadStmtNamedWindowIterate());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowJoinUniqueView()
        {
            PerformanceRun(new MultithreadStmtNamedWindowJoinUniqueView());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowMerge()
        {
            PerformanceRun(new MultithreadStmtNamedWindowMerge());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowMultiple()
        {
            PerformanceRun(new MultithreadStmtNamedWindowMultiple());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowSubqueryAgg()
        {
            PerformanceRun(new MultithreadStmtNamedWindowSubqueryAgg());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowSubqueryLookup()
        {
            PerformanceRun(new MultithreadStmtNamedWindowSubqueryLookup());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtNamedWindowUpdate()
        {
            PerformanceRun(new MultithreadStmtNamedWindowUpdate());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtPattern()
        {
            PerformanceRun(new MultithreadStmtPattern());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtStateless()
        {
            PerformanceRun(new MultithreadStmtStateless());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtStatelessEnummethod()
        {
            PerformanceRun(new MultithreadStmtStatelessEnummethod());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtSubquery()
        {
            PerformanceRun(new MultithreadStmtSubquery());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtTimeWindow()
        {
            PerformanceRun(new MultithreadStmtTimeWindow());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadStmtTwoPatterns()
        {
            PerformanceRun(new MultithreadStmtTwoPatterns());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadUpdate()
        {
            PerformanceRun(new MultithreadUpdate());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadUpdateIStreamSubselect()
        {
            PerformanceRun(new MultithreadUpdateIStreamSubselect());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadVariables()
        {
            PerformanceRun(new MultithreadVariables());
        }

        [Test, RunInApplicationDomain]
        public void TestMultithreadViewTimeWindow()
        {
            PerformanceRun(new MultithreadViewTimeWindow());
        }
    }
} // end of namespace