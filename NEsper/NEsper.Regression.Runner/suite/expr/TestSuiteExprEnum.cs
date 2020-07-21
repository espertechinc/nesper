///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.enummethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.lrreport;
using com.espertech.esper.regressionlib.support.sales;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprEnum
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
                typeof(SupportBean_ST0_Container),
                typeof(SupportBean),
                typeof(SupportBean_ST0_Container),
                typeof(SupportCollection),
                typeof(PersonSales),
                typeof(SupportBean_A),
                typeof(SupportBean_ST0),
                typeof(SupportSelectorWithListEvent),
                typeof(SupportEnumTwoEvent),
                typeof(SupportSelectorEvent),
                typeof(SupportContainerEvent),
                typeof(Item),
                typeof(LocationReport),
                typeof(Zone),
                typeof(SupportBeanComplexProps),
                typeof(SupportEventWithLongArray),
                typeof(SupportContainerLevelEvent),
                typeof(SupportSelectorWithListEvent),
                typeof(SupportBean_Container),
                typeof(SupportContainerLevel1Event),
                typeof(BookDesc),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportBean_ST0_Container));
            configuration.Common.AddImportType(typeof(LocationReportFactory));
            configuration.Common.AddImportType(typeof(SupportCollection));
            configuration.Common.AddImportType(typeof(ZoneFactory));
            configuration.Compiler.Expression.UdfCache = false;

            var configurationCompiler = configuration.Compiler;
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleList", typeof(SupportBean_ST0_Container).Name, "makeSampleList");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArray", typeof(SupportBean_ST0_Container).Name, "makeSampleArray");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleListString", typeof(SupportCollection).Name, "makeSampleListString");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArrayString", typeof(SupportCollection).Name, "makeSampleArrayString");
            configurationCompiler.AddPlugInSingleRowFunction("convertToArray", typeof(SupportSelectorWithListEvent).Name, "convertToArray");
            configurationCompiler.AddPlugInSingleRowFunction("extractAfterUnderscore", typeof(ExprEnumGroupBy).Name, "extractAfterUnderscore");
            configurationCompiler.AddPlugInSingleRowFunction("extractNum", typeof(ExprEnumMinMax.MyService).Name, "extractNum");
            configurationCompiler.AddPlugInSingleRowFunction("extractBigDecimal", typeof(ExprEnumMinMax.MyService).Name, "extractBigDecimal");
            configurationCompiler.AddPlugInSingleRowFunction("inrect", typeof(LRUtil).Name, "inrect");
            configurationCompiler.AddPlugInSingleRowFunction("distance", typeof(LRUtil).Name, "distance");
            configurationCompiler.AddPlugInSingleRowFunction("getZoneNames", typeof(Zone).Name, "getZoneNames");
            configurationCompiler.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container).Name, "makeTest");
        }

        [Test]
        public void TestExprEnumAggregate()
        {
            RegressionRunner.Run(session, ExprEnumAggregate.Executions());
        }

        [Test]
        public void TestExprEnumAllOfAnyOf()
        {
            RegressionRunner.Run(session, ExprEnumAllOfAnyOf.Executions());
        }

        [Test]
        public void TestExprEnumArrayOf()
        {
            RegressionRunner.Run(session, ExprEnumArrayOf.Executions());
        }

        [Test]
        public void TestExprEnumAverage()
        {
            RegressionRunner.Run(session, ExprEnumAverage.Executions());
        }

        [Test]
        public void TestExprEnumChained()
        {
            RegressionRunner.Run(session, new ExprEnumChained());
        }

        [Test]
        public void TestExprEnumCountOf()
        {
            RegressionRunner.Run(session, ExprEnumCountOf.Executions());
        }

        [Test]
        public void TestExprEnumDataSources()
        {
            RegressionRunner.Run(session, ExprEnumDataSources.Executions());
        }

        [Test]
        public void TestExprEnumDistinct()
        {
            RegressionRunner.Run(session, ExprEnumDistinct.Executions());
        }

        [Test]
        public void TestExprEnumDocSamples()
        {
            RegressionRunner.Run(session, ExprEnumDocSamples.Executions());
        }

        [Test]
        public void TestExprEnumExceptIntersectUnion()
        {
            RegressionRunner.Run(session, ExprEnumExceptIntersectUnion.Executions());
        }

        [Test]
        public void TestExprEnumFirstLastOf()
        {
            RegressionRunner.Run(session, ExprEnumFirstLastOf.Executions());
        }

        [Test]
        public void TestExprEnumGroupBy()
        {
            RegressionRunner.Run(session, ExprEnumGroupBy.Executions());
        }

        [Test]
        public void TestExprEnumInvalid()
        {
            RegressionRunner.Run(session, new ExprEnumInvalid());
        }

        [Test]
        public void TestExprEnumMinMax()
        {
            RegressionRunner.Run(session, ExprEnumMinMax.Executions());
        }

        [Test]
        public void TestExprEnumMinMaxBy()
        {
            RegressionRunner.Run(session, ExprEnumMinMaxBy.Executions());
        }

        [Test]
        public void TestExprEnumMostLeastFrequent()
        {
            RegressionRunner.Run(session, ExprEnumMostLeastFrequent.Executions());
        }

        [Test]
        public void TestExprEnumNamedWindowPerformance()
        {
            RegressionRunner.Run(session, new ExprEnumNamedWindowPerformance());
        }

        [Test]
        public void TestExprEnumNested()
        {
            RegressionRunner.Run(session, ExprEnumNested.Executions());
        }

        [Test]
        public void TestExprEnumNestedPerformance()
        {
            RegressionRunner.Run(session, new ExprEnumNestedPerformance());
        }

        [Test]
        public void TestExprEnumOrderBy()
        {
            RegressionRunner.Run(session, ExprEnumOrderBy.Executions());
        }

        [Test]
        public void TestExprEnumReverse()
        {
            RegressionRunner.Run(session, ExprEnumReverse.Executions());
        }

        [Test]
        public void TestExprEnumSelectFrom()
        {
            RegressionRunner.Run(session, ExprEnumSelectFrom.Executions());
        }

        [Test]
        public void TestExprEnumSequenceEqual()
        {
            RegressionRunner.Run(session, ExprEnumSequenceEqual.Executions());
        }

        [Test]
        public void TestExprEnumSumOf()
        {
            RegressionRunner.Run(session, ExprEnumSumOf.Executions());
        }

        [Test]
        public void TestExprEnumTakeAndTakeLast()
        {
            RegressionRunner.Run(session, ExprEnumTakeAndTakeLast.Executions());
        }

        [Test]
        public void TestExprEnumTakeWhileAndWhileLast()
        {
            RegressionRunner.Run(session, ExprEnumTakeWhileAndWhileLast.Executions());
        }

        [Test]
        public void TestExprEnumToMap()
        {
            RegressionRunner.Run(session, ExprEnumToMap.Executions());
        }

        [Test]
        public void TestExprEnumWhere()
        {
            RegressionRunner.Run(session, ExprEnumWhere.Executions());
        }
    }
} // end of namespace