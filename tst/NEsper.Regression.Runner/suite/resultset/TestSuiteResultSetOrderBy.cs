///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.resultset.@orderby;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetOrderBy
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
                typeof(SupportBean_A),
                typeof(SupportBeanString),
                typeof(SupportMarketDataBean),
                typeof(SupportHierarchyEvent)
            }) {
                configuration.Common.AddEventType(clazz);
            }
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByAggregateGrouped()
        {
            RegressionRunner.Run(session, ResultSetOrderByAggregateGrouped.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowForAll()
        {
            RegressionRunner.Run(session, ResultSetOrderByRowForAll.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowPerEvent()
        {
            RegressionRunner.Run(session, ResultSetOrderByRowPerEvent.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowPerGroup()
        {
            RegressionRunner.Run(session, ResultSetOrderByRowPerGroup.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderBySelfJoin()
        {
            RegressionRunner.Run(session, ResultSetOrderBySelfJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderBySimple()
        {
            RegressionRunner.Run(session, ResultSetOrderBySimple.Executions());
        }
    }
} // end of namespace