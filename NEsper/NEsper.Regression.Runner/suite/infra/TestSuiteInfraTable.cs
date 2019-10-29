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
using com.espertech.esper.regressionlib.suite.infra.tbl;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.extend.aggfunc;
using com.espertech.esper.regressionlib.support.extend.aggmultifunc;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Table tests
    [TestFixture]
    public class TestSuiteInfraTable
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
                typeof(SupportIntrusionEvent),
                typeof(SupportTrafficEvent),
                typeof(SupportMySortValueEvent),
                typeof(SupportBean_S2),
                typeof(SupportBeanSimple),
                typeof(SupportByteArrEventStringId),
                typeof(SupportBeanRange),
                typeof(SupportTwoKeyEvent),
                typeof(SupportCtorSB2WithObjectArray),
                typeof(Support10ColEvent),
                typeof(SupportTopGroupSubGroupEvent),
                typeof(SupportBeanNumeric)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Compiler.AddPlugInSingleRowFunction(
                "singlerow",
                typeof(InfraTableInvalid),
                "MySingleRowFunction");
            configuration.Compiler.AddPlugInSingleRowFunction(
                "pluginServiceEventBean",
                typeof(InfraTableSelect),
                "MyServiceEventBean");

            configuration.Compiler.AddPlugInAggregationFunctionForge(
                "myaggsingle",
                typeof(SupportCountBackAggregationFunctionForge));
            configuration.Compiler.AddPlugInAggregationFunctionForge("csvWords", typeof(SupportSimpleWordCSVForge));

            var config = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new [] { "referenceCountedMap","referenceCountLookup" },
                typeof(SupportReferenceCountedMapForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(config);
            var configMultiFuncAgg = new ConfigurationCompilerPlugInAggregationMultiFunction(
                new [] { "se1" },
                typeof(SupportAggMFMultiRTForge));
            configuration.Compiler.AddPlugInAggregationMultiFunction(configMultiFuncAgg);

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));

            configuration.Compiler.ByteCode.AllowSubscriber = true;
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableAccessAggregationState()
        {
            RegressionRunner.Run(session, InfraTableAccessAggregationState.Executions());
        }

        [Test]
        public void TestInfraTableAccessCore()
        {
            RegressionRunner.Run(session, InfraTableAccessCore.Executions());
        }

        [Test]
        public void TestInfraTableAccessDotMethod()
        {
            RegressionRunner.Run(session, InfraTableAccessDotMethod.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableContext()
        {
            RegressionRunner.Run(session, InfraTableContext.Executions());
        }

        [Test]
        public void TestInfraTableCountMinSketch()
        {
            RegressionRunner.Run(session, InfraTableCountMinSketch.Executions());
        }

        [Test]
        public void TestInfraTableDocSamples()
        {
            RegressionRunner.Run(session, InfraTableDocSamples.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableFAFExecuteQuery()
        {
            RegressionRunner.Run(session, InfraTableFAFExecuteQuery.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableFilters()
        {
            RegressionRunner.Run(session, new InfraTableFilters());
        }

        [Test]
        public void TestInfraTableInsertInto()
        {
            RegressionRunner.Run(session, InfraTableInsertInto.Executions());
        }

        [Test]
        public void TestInfraTableIntoTable()
        {
            RegressionRunner.Run(session, InfraTableIntoTable.Executions());
        }

        [Test]
        public void TestInfraTableInvalid()
        {
            RegressionRunner.Run(session, InfraTableInvalid.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableIterate()
        {
            RegressionRunner.Run(session, new InfraTableIterate());
        }

        [Test]
        public void TestInfraTableJoin()
        {
            RegressionRunner.Run(session, InfraTableJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTAccessReadMergeWriteInsertDeleteRowVisible()
        {
            RegressionRunner.Run(session, new InfraTableMTAccessReadMergeWriteInsertDeleteRowVisible());
        }

        [Test]
        public void TestInfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedAccessReadIntoTableWriteAggColConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedAccessReadIntoTableWriteNewRowCreation());
        }

        [Test]
        public void TestInfraTableMTGroupedFAFReadFAFWriteChain()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedFAFReadFAFWriteChain());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedSubqueryReadInsertIntoWriteConcurr());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedWContextIntoTableWriteAsContextTable()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedWContextIntoTableWriteAsContextTable());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedWContextIntoTableWriteAsSharedTable()
        {
            RegressionRunner.Run(session, new InfraTableMTGroupedWContextIntoTableWriteAsSharedTable());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadInotTableWriteIterate()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadInotTableWriteIterate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadIntoTableWriteFilterUse()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadIntoTableWriteFilterUse());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessReadMergeWrite()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessReadMergeWrite());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedAccessWithinRowFAFConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedAccessWithinRowFAFConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedIntoTableWriteMultiWriterAgg()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedIntoTableWriteMultiWriterAgg());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedJoinColumnConsistency()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedJoinColumnConsistency());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd()
        {
            RegressionRunner.Run(session, new InfraTableMTUngroupedSubqueryReadMergeWriteColumnUpd());
        }

        [Test]
        public void TestInfraTableNonAccessDotSubqueryAndJoin()
        {
            RegressionRunner.Run(session, new InfraTableNonAccessDotSubqueryAndJoin());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableOnDelete()
        {
            RegressionRunner.Run(session, InfraTableOnDelete.Executions());
        }

        [Test]
        public void TestInfraTableOnMerge()
        {
            RegressionRunner.Run(session, InfraTableOnMerge.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableOnSelect()
        {
            RegressionRunner.Run(session, new InfraTableOnSelect());
        }

        [Test]
        public void TestInfraTableOnUpdate()
        {
            RegressionRunner.Run(session, new InfraTableOnUpdate());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableOutputRateLimiting()
        {
            RegressionRunner.Run(session, new InfraTableOutputRateLimiting());
        }

        [Test]
        public void TestInfraTablePlugInAggregation()
        {
            RegressionRunner.Run(session, InfraTablePlugInAggregation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableRollup()
        {
            RegressionRunner.Run(session, InfraTableRollup.Executions());
        }

        [Test]
        public void TestInfraTableSelect()
        {
            RegressionRunner.Run(session, InfraTableSelect.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableSubquery()
        {
            RegressionRunner.Run(session, InfraTableSubquery.Executions());
        }

        [Test]
        public void TestInfraTableUpdateAndIndex()
        {
            RegressionRunner.Run(session, InfraTableUpdateAndIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestInfraTableWNamedWindow()
        {
            RegressionRunner.Run(session, new InfraTableWNamedWindow());
        }
    }
} // end of namespace