///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.resultset.querytype;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetQueryType
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
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportMarketDataBean),
                typeof(SupportCarEvent),
                typeof(SupportCarInfoEvent),
                typeof(SupportEventABCProp),
                typeof(SupportBeanString),
                typeof(SupportPriceEvent),
                typeof(SupportMarketDataIDBean),
                typeof(SupportBean_A),
                typeof(SupportBean_B)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction(
                "myfunc",
                typeof(ResultSetQueryTypeRollupGroupingFuncs.GroupingSupportFunc),
                "Myfunc");

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "concatstring",
                typeof(SupportConcatWManagedAggregationFunctionForge));

            var mfAggConfig = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new [] { "sc" },
                typeof(SupportAggMFMultiRTForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(mfAggConfig);

            configuration.Common.AddVariable("MyVar", typeof(string), "");
        }

        [Test]
        public void TestResultSetQueryTypeAggregateGrouped()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeAggregateGrouped.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeAggregateGroupedHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeAggregateGroupedHaving.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeGroupByReclaimMicrosecondResolution()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(5000));
        }

        [Test]
        public void TestResultSetQueryTypeHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeHaving.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeEnumerator()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeEnumerator.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeLocalGroupBy()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeLocalGroupBy.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRollupDimensionality()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRollupDimensionality.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRollupGroupingFuncs()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRollupGroupingFuncs.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRollupHavingAndOrderBy()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRollupHavingAndOrderBy.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRollupPlanningAndSODA()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRollupPlanningAndSODA());
        }

        [Test]
        public void TestResultSetQueryTypeRowForAll()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowForAll.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRowForAllHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowForAllHaving.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRowPerEvent()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerEvent.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRowPerEventPerformance()
        {
            RegressionRunner.Run(session, new ResultSetQueryTypeRowPerEventPerformance());
        }

        [Test]
        public void TestResultSetQueryTypeRowPerGroup()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerGroup.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeRowPerGroupHaving()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeRowPerGroupHaving.Executions());
        }

        [Test]
        public void TestResultSetQueryTypeWTimeBatch()
        {
            RegressionRunner.Run(session, ResultSetQueryTypeWTimeBatch.Executions());
        }
    }
} // end of namespace