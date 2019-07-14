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
using com.espertech.esper.compat;
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
            session.Destroy();
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
                configuration.Container,
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

        [Test]
        public void TestMultithreadContextCountSimple()
        {
            RegressionRunner.Run(session, new MultithreadContextCountSimple());
        }

        [Test]
        public void TestMultithreadContextInitiatedTerminatedWithNowParallel()
        {
            RegressionRunner.Run(session, new MultithreadContextInitiatedTerminatedWithNowParallel());
        }

        [Test]
        public void TestMultithreadContextPartitioned()
        {
            RegressionRunner.Run(session, new MultithreadContextPartitioned());
        }

        [Test]
        public void TestMultithreadContextTemporalStartStop()
        {
            RegressionRunner.Run(session, new MultithreadContextTemporalStartStop());
        }

        [Test]
        public void TestMultithreadContextUnique()
        {
            RegressionRunner.Run(session, new MultithreadContextUnique());
        }

        [Test]
        public void TestMultithreadDeployAtomic()
        {
            RegressionRunner.Run(session, new MultithreadDeployAtomic());
        }

        [Test]
        public void TestMultithreadDeterminismInsertInto()
        {
            RegressionRunner.Run(session, new MultithreadDeterminismInsertInto());
        }

        [Test]
        public void TestMultithreadedViewTimeWindowSceneTwo()
        {
            RegressionRunner.Run(session, new MultithreadViewTimeWindowSceneTwo());
        }

        [Test]
        public void TestMultithreadMultithreadContextPartitionedWCount()
        {
            RegressionRunner.Run(session, new MultithreadContextPartitionedWCount());
        }

        [Test]
        public void TestMultithreadNamedWindowDelete()
        {
            RegressionRunner.Run(session, new MultithreadNamedWindowDelete());
        }

        [Test]
        public void TestMultithreadStmtDatabaseJoin()
        {
            RegressionRunner.Run(session, new MultithreadStmtDatabaseJoin());
        }

        [Test]
        public void TestMultithreadStmtFilter()
        {
            RegressionRunner.Run(session, new MultithreadStmtFilter());
        }

        [Test]
        public void TestMultithreadStmtFilterSubquery()
        {
            RegressionRunner.Run(session, new MultithreadStmtFilterSubquery());
        }

        [Test]
        public void TestMultithreadStmtInsertInto()
        {
            RegressionRunner.Run(session, new MultithreadStmtInsertInto());
        }

        [Test]
        public void TestMultithreadStmtIterate()
        {
            RegressionRunner.Run(session, new MultithreadStmtIterate());
        }

        [Test]
        public void TestMultithreadStmtJoin()
        {
            RegressionRunner.Run(session, new MultithreadStmtJoin());
        }

        [Test]
        public void TestMultithreadStmtListenerCreateStmt()
        {
            RegressionRunner.Run(session, new MultithreadStmtListenerCreateStmt());
        }

        [Test]
        public void TestMultithreadStmtListenerRoute()
        {
            RegressionRunner.Run(session, new MultithreadStmtListenerRoute());
        }

        [Test]
        public void TestMultithreadStmtMgmt()
        {
            RegressionRunner.Run(session, new MultithreadStmtMgmt());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowConsume()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowConsume());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowDelete()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowDelete());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowFAF()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowFAF());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowIterate()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowIterate());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowJoinUniqueView()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowJoinUniqueView());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowMerge()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowMerge());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowMultiple()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowMultiple());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowSubqueryAgg()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowSubqueryAgg());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowSubqueryLookup()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowSubqueryLookup());
        }

        [Test]
        public void TestMultithreadStmtNamedWindowUpdate()
        {
            RegressionRunner.Run(session, new MultithreadStmtNamedWindowUpdate());
        }

        [Test]
        public void TestMultithreadStmtPattern()
        {
            RegressionRunner.Run(session, new MultithreadStmtPattern());
        }

        [Test]
        public void TestMultithreadStmtStateless()
        {
            RegressionRunner.Run(session, new MultithreadStmtStateless());
        }

        [Test]
        public void TestMultithreadStmtStatelessEnummethod()
        {
            RegressionRunner.Run(session, new MultithreadStmtStatelessEnummethod());
        }

        [Test]
        public void TestMultithreadStmtSubquery()
        {
            RegressionRunner.Run(session, new MultithreadStmtSubquery());
        }

        [Test]
        public void TestMultithreadStmtTimeWindow()
        {
            RegressionRunner.Run(session, new MultithreadStmtTimeWindow());
        }

        [Test]
        public void TestMultithreadStmtTwoPatterns()
        {
            RegressionRunner.Run(session, new MultithreadStmtTwoPatterns());
        }

        [Test]
        public void TestMultithreadUpdate()
        {
            RegressionRunner.Run(session, new MultithreadUpdate());
        }

        [Test]
        public void TestMultithreadUpdateIStreamSubselect()
        {
            RegressionRunner.Run(session, new MultithreadUpdateIStreamSubselect());
        }

        [Test]
        public void TestMultithreadVariables()
        {
            RegressionRunner.Run(session, new MultithreadVariables());
        }

        [Test]
        public void TestMultithreadViewTimeWindow()
        {
            RegressionRunner.Run(session, new MultithreadViewTimeWindow());
        }
    }
} // end of namespace