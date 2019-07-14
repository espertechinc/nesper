///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.infra.nwtable;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionrun.suite.infra
{
    [TestFixture]
    public class TestSuiteInfraNWTable
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
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBeanRange),
                typeof(SupportSimpleBeanOne),
                typeof(SupportSimpleBeanTwo),
                typeof(SupportBean_ST0),
                typeof(SupportSpatialPoint),
                typeof(SupportMarketDataBean),
                typeof(SupportBean_Container),
                typeof(OrderBean)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.Logging.IsEnableQueryPlan = true;

            configuration.Compiler.AddPlugInSingleRowFunction("doubleInt", typeof(InfraNWTableFAF), "doubleInt");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "justCount",
                typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter),
                "justCount");
            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test]
        public void TestInfraNWTableFAFSubstitutionParams()
        {
            RegressionRunner.Run(session, InfraNWTableFAFSubstitutionParams.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraComparative()
        {
            RegressionRunner.Run(session, InfraNWTableComparative.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraContext()
        {
            RegressionRunner.Run(session, InfraNWTableContext.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraCreateIndex()
        {
            RegressionRunner.Run(session, InfraNWTableCreateIndex.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraCreateIndexAdvancedSyntax()
        {
            RegressionRunner.Run(session, new InfraNWTableCreateIndexAdvancedSyntax());
        }

        [Test]
        public void TestInfraNWTableInfraEventType()
        {
            RegressionRunner.Run(session, new InfraNWTableEventType());
        }

        [Test]
        public void TestInfraNWTableInfraFAF()
        {
            RegressionRunner.Run(session, InfraNWTableFAF.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraFAFIndex()
        {
            RegressionRunner.Run(session, InfraNWTableFAFIndex.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraOnDelete()
        {
            RegressionRunner.Run(session, InfraNWTableOnDelete.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraOnMerge()
        {
            RegressionRunner.Run(session, InfraNWTableOnMerge.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraOnMergePerf()
        {
            RegressionRunner.Run(session, InfraNWTableOnMergePerf.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraOnSelect()
        {
            RegressionRunner.Run(session, InfraNWTableOnSelect.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraOnUpdate()
        {
            RegressionRunner.Run(session, InfraNWTableOnUpdate.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraStartStop()
        {
            RegressionRunner.Run(session, InfraNWTableStartStop.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubqCorrelCoerce()
        {
            RegressionRunner.Run(session, InfraNWTableSubqCorrelCoerce.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubqCorrelIndex()
        {
            RegressionRunner.Run(session, InfraNWTableSubqCorrelIndex.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubqCorrelJoin()
        {
            RegressionRunner.Run(session, InfraNWTableSubqCorrelJoin.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubquery()
        {
            RegressionRunner.Run(session, InfraNWTableSubquery.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubqueryAtEventBean()
        {
            RegressionRunner.Run(session, InfraNWTableSubqueryAtEventBean.Executions());
        }

        [Test]
        public void TestInfraNWTableInfraSubqUncorrel()
        {
            RegressionRunner.Run(session, InfraNWTableSubqUncorrel.Executions());
        }

        [Test]
        public void TestInfraNWTableJoin()
        {
            RegressionRunner.Run(session, InfraNWTableJoin.Executions());
        }

        [Test]
        public void TestInfraNWTableOnSelectWDelete()
        {
            RegressionRunner.Run(session, InfraNWTableOnSelectWDelete.Executions());
        }
    }
} // end of namespace