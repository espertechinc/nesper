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
using com.espertech.esper.regressionlib.suite.resultset.aggregate;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetAggregate
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

        private void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBeanString),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanNumeric),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportEventPropertyWithMethod)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Compiler.ByteCode.IncludeDebugSymbols = true;

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "concatMethodAgg",
                typeof(SupportConcatWManagedAggregationFunctionForge));

            var eventsAsList = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new [] { "eventsAsList" },
                typeof(SupportAggMFEventsAsListForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(eventsAsList);
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateCountSum()
        {
            RegressionRunner.Run(session, ResultSetAggregateCountSum.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFiltered()
        {
            RegressionRunner.Run(session, ResultSetAggregateFiltered.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFilterNamedParameter()
        {
            RegressionRunner.Run(session, ResultSetAggregateFilterNamedParameter.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFirstEverLastEver()
        {
            RegressionRunner.Run(session, ResultSetAggregateFirstEverLastEver.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateFirstLastWindow()
        {
            RegressionRunner.Run(session, ResultSetAggregateFirstLastWindow.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateLeaving()
        {
            RegressionRunner.Run(session, new ResultSetAggregateLeaving());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMaxMinGroupBy()
        {
            RegressionRunner.Run(session, ResultSetAggregateMaxMinGroupBy.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMedianAndDeviation()
        {
            RegressionRunner.Run(session, ResultSetAggregateMedianAndDeviation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateMinMax()
        {
            RegressionRunner.Run(session, ResultSetAggregateMinMax.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateNTh()
        {
            RegressionRunner.Run(session, new ResultSetAggregateNTh());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateRate()
        {
            RegressionRunner.Run(session, ResultSetAggregateRate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetAggregateSortedMinMaxBy()
        {
            RegressionRunner.Run(session, ResultSetAggregateSortedMinMaxBy.Executions());
        }
    }
} // end of namespace