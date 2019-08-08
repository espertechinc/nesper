///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.subselect;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLSubselect
    {
        private RegressionSession session;

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

        [Test]
        public void TestEPLSubselectUnfiltered()
        {
            RegressionRunner.Run(session, EPLSubselectUnfiltered.Executions());
        }

        [Test]
        public void TestEPLSubselectExists()
        {
            RegressionRunner.Run(session, EPLSubselectExists.Executions());
        }

        [Test]
        public void TestEPLSubselectAllAnySomeExpr()
        {
            RegressionRunner.Run(session, EPLSubselectAllAnySomeExpr.Executions());
        }

        [Test]
        public void TestEPLSubselectIn()
        {
            RegressionRunner.Run(session, EPLSubselectIn.Executions());
        }

        [Test]
        public void TestEPLSubselectFiltered()
        {
            RegressionRunner.Run(session, EPLSubselectFiltered.Executions());
        }

        [Test]
        public void TestEPLSubselectOrderOfEval()
        {
            RegressionRunner.Run(session, EPLSubselectOrderOfEval.Executions());
        }

        [Test]
        public void TestEPLSubselectFilteredPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectFilteredPerformance.Executions());
        }

        [Test]
        public void TestEPLSubselectIndex()
        {
            RegressionRunner.Run(session, EPLSubselectIndex.Executions());
        }

        [Test]
        public void TestEPLSubselectInKeywordPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectInKeywordPerformance.Executions());
        }

        [Test]
        public void TestEPLSubselectAggregatedSingleValue()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedSingleValue.Executions());
        }

        [Test]
        public void TestEPLSubselectAggregatedInExistsAnyAll()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedInExistsAnyAll.Executions());
        }

        [Test]
        public void TestEPLSubselectMulticolumn()
        {
            RegressionRunner.Run(session, EPLSubselectMulticolumn.Executions());
        }

        [Test]
        public void TestEPLSubselectMultirow()
        {
            RegressionRunner.Run(session, EPLSubselectMultirow.Executions());
        }

        [Test]
        public void TestEPLSubselectAggregatedMultirowAndColumn()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedMultirowAndColumn.Executions());
        }

        [Test]
        public void TestEPLSubselectCorrelatedAggregationPerformance()
        {
            RegressionRunner.Run(session, new EPLSubselectCorrelatedAggregationPerformance());
        }

        [Test]
        public void TestEPLSubselectNamedWindowPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectNamedWindowPerformance.Executions());
        }

        [Test]
        public void TestEPLSubselectWithinHaving()
        {
            RegressionRunner.Run(session, EPLSubselectWithinHaving.Executions());
        }

        [Test]
        public void TestEPLSubselectWithinPattern()
        {
            RegressionRunner.Run(session, EPLSubselectWithinPattern.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1),
                typeof(SupportBean_S2), typeof(SupportBean_S3), typeof(SupportBean_S4),
                typeof(SupportValueEvent), typeof(SupportIdAndValueEvent), typeof(SupportBeanArrayCollMap),
                typeof(SupportSensorEvent), typeof(SupportBeanRange), typeof(SupportSimpleBeanOne), typeof(SupportSimpleBeanTwo),
                typeof(SupportBean_ST0), typeof(SupportBean_ST1), typeof(SupportBean_ST2), typeof(SupportTradeEventTwo),
                typeof(SupportMaxAmountEvent), typeof(SupportMarketDataBean)})
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            configuration.Compiler.AddPlugInSingleRowFunction("supportSingleRowFunction", typeof(EPLSubselectWithinPattern), "SupportSingleRowFunction");
        }
    }
} // end of namespace