///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetOrderBy : AbstractTestBase
    {
        public TestSuiteResultSetOrderBy() : base(Configure)
        {
        }

        public static void Configure(Configuration configuration)
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
            RegressionRunner.Run(_session, ResultSetOrderByAggregateGrouped.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowForAll()
        {
            RegressionRunner.Run(_session, ResultSetOrderByRowForAll.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowPerEvent()
        {
            RegressionRunner.Run(_session, ResultSetOrderByRowPerEvent.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderByRowPerGroup()
        {
            RegressionRunner.Run(_session, ResultSetOrderByRowPerGroup.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderBySelfJoin()
        {
            RegressionRunner.Run(_session, ResultSetOrderBySelfJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOrderBySimple()
        {
            RegressionRunner.Run(_session, ResultSetOrderBySimple.Executions());
        }
    }
} // end of namespace