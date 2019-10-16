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
using com.espertech.esper.regressionrun.Runner;

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
            _session = RegressionRunner.Session();
            Configure(_session.Configuration);
        }

        [TearDown]
        public void TearDown()
        {
            _session.Destroy();
            _session = null;
        }

        private RegressionSession _session;

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

            configuration.Compiler.AddPlugInSingleRowFunction("doubleInt", typeof(InfraNWTableFAF), "DoubleInt");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "justCount",
                typeof(InfraNWTableFAFIndexPerfWNoQueryPlanLog.InvocationCounter),
                "JustCount");
            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableFAFSubstitutionParams()
        {
            RegressionRunner.Run(_session, InfraNWTableFAFSubstitutionParams.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraComparative()
        {
            RegressionRunner.Run(_session, InfraNWTableComparative.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraContext()
        {
            RegressionRunner.Run(_session, InfraNWTableContext.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraCreateIndex()
        {
            RegressionRunner.Run(_session, InfraNWTableCreateIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraCreateIndexAdvancedSyntax()
        {
            RegressionRunner.Run(_session, new InfraNWTableCreateIndexAdvancedSyntax());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraEventType()
        {
            RegressionRunner.Run(_session, new InfraNWTableEventType());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraFAF()
        {
            RegressionRunner.Run(_session, InfraNWTableFAF.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraFAFIndex()
        {
            RegressionRunner.Run(_session, InfraNWTableFAFIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnDelete()
        {
            RegressionRunner.Run(_session, InfraNWTableOnDelete.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnMerge()
        {
            RegressionRunner.Run(_session, InfraNWTableOnMerge.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnMergePerf()
        {
            RegressionRunner.Run(_session, InfraNWTableOnMergePerf.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnSelect()
        {
            RegressionRunner.Run(_session, InfraNWTableOnSelect.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraOnUpdate()
        {
            RegressionRunner.Run(_session, InfraNWTableOnUpdate.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraStartStop()
        {
            RegressionRunner.Run(_session, InfraNWTableStartStop.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqCorrelCoerce()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqCorrelCoerce.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqCorrelIndex()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqCorrelIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqCorrelJoin()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqCorrelJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubquery()
        {
            RegressionRunner.Run(_session, InfraNWTableSubquery.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqueryAtEventBean()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqueryAtEventBean.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableInfraSubqUncorrel()
        {
            RegressionRunner.Run(_session, InfraNWTableSubqUncorrel.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableJoin()
        {
            RegressionRunner.Run(_session, InfraNWTableJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraNWTableOnSelectWDelete()
        {
            RegressionRunner.Run(_session, InfraNWTableOnSelectWDelete.Executions());
        }
    }
} // end of namespace