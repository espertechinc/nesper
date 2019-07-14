///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.enummethod;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.lrreport;
using com.espertech.esper.regressionlib.support.sales;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprEnum
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
            RegressionRunner.Run(session, new ExprEnumMinMaxBy());
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
            RegressionRunner.Run(session, new ExprEnumToMap());
        }

        [Test]
        public void TestExprEnumWhere()
        {
            RegressionRunner.Run(session, ExprEnumWhere.Executions());
        }

        private static void Configure(Configuration configuration)
        {
            foreach (Type clazz in new Type[]{
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
                typeof(SupportContainerLevel1Event), typeof(BookDesc)})
            {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportBean_ST0_Container));
            configuration.Common.AddImportType(typeof(LocationReportFactory));
            configuration.Common.AddImportType(typeof(SupportCollection));
            configuration.Common.AddImportType(typeof(ZoneFactory));
            configuration.Compiler.Expression.UdfCache = false;

            ConfigurationCompiler configurationCompiler = configuration.Compiler;
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleList", typeof(SupportBean_ST0_Container), "makeSampleList");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArray", typeof(SupportBean_ST0_Container), "makeSampleArray");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleListString", typeof(SupportCollection), "makeSampleListString");
            configurationCompiler.AddPlugInSingleRowFunction("makeSampleArrayString", typeof(SupportCollection), "makeSampleArrayString");
            configurationCompiler.AddPlugInSingleRowFunction("convertToArray", typeof(SupportSelectorWithListEvent), "convertToArray");
            configurationCompiler.AddPlugInSingleRowFunction("extractAfterUnderscore", typeof(ExprEnumGroupBy), "extractAfterUnderscore");
            configurationCompiler.AddPlugInSingleRowFunction("extractNum", typeof(ExprEnumMinMax.MyService), "extractNum");
            configurationCompiler.AddPlugInSingleRowFunction("extractBigDecimal", typeof(ExprEnumMinMax.MyService), "extractBigDecimal");
            configurationCompiler.AddPlugInSingleRowFunction("inrect", typeof(LRUtil), "inrect");
            configurationCompiler.AddPlugInSingleRowFunction("distance", typeof(LRUtil), "distance");
            configurationCompiler.AddPlugInSingleRowFunction("getZoneNames", typeof(Zone), "getZoneNames");
            configurationCompiler.AddPlugInSingleRowFunction("makeTest", typeof(SupportBean_ST0_Container), "makeTest");
        }
    }
} // end of namespace