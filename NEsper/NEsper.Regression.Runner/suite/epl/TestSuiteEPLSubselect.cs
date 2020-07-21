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

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectUnfiltered()
        {
            RegressionRunner.Run(session, EPLSubselectUnfiltered.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectExists()
        {
            RegressionRunner.Run(session, EPLSubselectExists.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectAllAnySomeExpr()
        {
            RegressionRunner.Run(session, EPLSubselectAllAnySomeExpr.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectIn()
        {
            RegressionRunner.Run(session, EPLSubselectIn.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectFiltered()
        {
            RegressionRunner.Run(session, EPLSubselectFiltered.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectOrderOfEval()
        {
            RegressionRunner.Run(session, EPLSubselectOrderOfEval.Executions());
        }

        [Test]
        public void TestEPLSubselectFilteredPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectFilteredPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectIndex()
        {
            RegressionRunner.Run(session, EPLSubselectIndex.Executions());
        }

        [Test]
        public void TestEPLSubselectInKeywordPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectInKeywordPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectAggregatedSingleValue()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedSingleValue.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectAggregatedInExistsAnyAll()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedInExistsAnyAll.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectMulticolumn()
        {
            RegressionRunner.Run(session, EPLSubselectMulticolumn.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectMultirow()
        {
            RegressionRunner.Run(session, EPLSubselectMultirow.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectAggregatedMultirowAndColumn()
        {
            RegressionRunner.Run(session, EPLSubselectAggregatedMultirowAndColumn.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectCorrelatedAggregationPerformance()
        {
            RegressionRunner.Run(session, new EPLSubselectCorrelatedAggregationPerformance());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectNamedWindowPerformance()
        {
            RegressionRunner.Run(session, EPLSubselectNamedWindowPerformance.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectWithinHaving()
        {
            RegressionRunner.Run(session, EPLSubselectWithinHaving.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLSubselectWithinPattern()
        {
            RegressionRunner.Run(session, EPLSubselectWithinPattern.Executions());
        }
        
        [Test, RunInApplicationDomain]
        public void TestEPLSubselectWithinFilter()
        {
            RegressionRunner.Run(session, EPLSubselectWithinFilter.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new []{
                typeof(SupportBean), 
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBean_S3), 
                typeof(SupportBean_S4),
                typeof(SupportValueEvent),
                typeof(SupportIdAndValueEvent), 
                typeof(SupportBeanArrayCollMap),
                typeof(SupportSensorEvent),
                typeof(SupportBeanRange),
                typeof(SupportSimpleBeanOne), 
                typeof(SupportSimpleBeanTwo),
                typeof(SupportBean_ST0),
                typeof(SupportBean_ST1), 
                typeof(SupportBean_ST2), 
                typeof(SupportTradeEventTwo),
                typeof(SupportMaxAmountEvent),
                typeof(SupportMarketDataBean),
                typeof(SupportEventWithIntArray),
                typeof(SupportEventWithManyArray)
            })
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            configuration.Compiler.AddPlugInSingleRowFunction("supportSingleRowFunction", typeof(EPLSubselectWithinPattern), "SupportSingleRowFunction");
        }
    }
} // end of namespace