///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.epl.other;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.bookexample;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionlib.support.lrreport;
using com.espertech.esper.regressionrun.Runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLOther
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
            configuration.Common.AddEventType("ObjectEvent", typeof(object));

            foreach (var clazz in new[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2),
                typeof(SupportBeanSourceEvent),
                typeof(OrderBean),
                typeof(SupportBeanReadOnly),
                typeof(SupportBeanErrorTestingOne),
                typeof(OrderBean),
                typeof(SupportCollection),
                typeof(SupportBean_A),
                typeof(SupportBean_B),
                typeof(SupportBean_N),
                typeof(SupportChainTop),
                typeof(SupportTemperatureBean),
                typeof(SupportBeanKeywords),
                typeof(SupportBeanSimple),
                typeof(SupportBeanStaticOuter),
                typeof(SupportMarketDataBean),
                typeof(SupportBeanComplexProps),
                typeof(SupportBeanCombinedProps),
                typeof(SupportEventWithIntArray),
                typeof(SupportEventWithManyArray)
            }) {
                configuration.Common.AddEventType(clazz);
            }

            IDictionary<string, object> myMapTypeInv = new Dictionary<string, object>();
            myMapTypeInv.Put("P0", typeof(long));
            myMapTypeInv.Put("P1", typeof(long));
            myMapTypeInv.Put("P2", typeof(long));
            myMapTypeInv.Put("p3", typeof(string));
            configuration.Common.AddEventType("MyMapTypeInv", myMapTypeInv);

            IDictionary<string, object> myMapTypeII = new Dictionary<string, object>();
            myMapTypeII.Put("P0", typeof(long));
            myMapTypeII.Put("P1", typeof(long));
            myMapTypeII.Put("P2", typeof(long));
            configuration.Common.AddEventType("MyMapTypeII", myMapTypeII);

            IDictionary<string, object> myMapTypeIDB = new Dictionary<string, object>();
            myMapTypeIDB.Put("P0", typeof(string));
            myMapTypeIDB.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeIDB", myMapTypeIDB);

            IDictionary<string, object> myMapTypeNW = new Dictionary<string, object>();
            myMapTypeNW.Put("P0", typeof(string));
            myMapTypeNW.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeNW", myMapTypeNW);

            IDictionary<string, object> myMapTypeSR = new Dictionary<string, object>();
            myMapTypeSR.Put("P0", typeof(string));
            myMapTypeSR.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeSR", myMapTypeSR);

            IDictionary<string, object> myMapTypeSODA = new Dictionary<string, object>();
            myMapTypeSODA.Put("P0", typeof(string));
            myMapTypeSODA.Put("P1", typeof(string));
            configuration.Common.AddEventType("MyMapTypeSODA", myMapTypeSODA);

            var configXML = new ConfigurationCommonEventTypeXMLDOM();
            configXML.RootElementName = "MyXMLEvent";
            configuration.Common.AddEventType("MyXmlEvent", configXML);

            var config = new ConfigurationCommonEventTypeXMLDOM();
            config.RootElementName = "simpleEvent";
            configuration.Common.AddEventType("MyXMLEvent", config);

            IDictionary<string, object> myMapTypeSelect = new Dictionary<string, object>();
            myMapTypeSelect.Put("s0", typeof(string));
            myMapTypeSelect.Put("s1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeSelect", myMapTypeSelect);

            IDictionary<string, object> myMapTypeWhere = new Dictionary<string, object>();
            myMapTypeWhere.Put("w0", typeof(int));
            configuration.Common.AddEventType("MyMapTypeWhere", myMapTypeWhere);

            IDictionary<string, object> myMapTypeUO = new Dictionary<string, object>();
            myMapTypeUO.Put("s0", typeof(string));
            myMapTypeUO.Put("s1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeUO", myMapTypeUO);

            var legacy = new ConfigurationCommonEventTypeBean();
            legacy.CopyMethod = "MyCopyMethod";
            configuration.Common.AddEventType("SupportBeanCopyMethod", typeof(SupportBeanCopyMethod), legacy);

            IDictionary<string, object> defMapTypeKVDistinct = new Dictionary<string, object>();
            defMapTypeKVDistinct.Put("k1", typeof(string));
            defMapTypeKVDistinct.Put("v1", typeof(int));
            configuration.Common.AddEventType("MyMapTypeKVDistinct", defMapTypeKVDistinct);

            IDictionary<string, object> typeMap = new Dictionary<string, object>();
            typeMap.Put("int", typeof(int?));
            typeMap.Put("TheString", typeof(string));
            configuration.Common.AddEventType("MyMapEventIntString", typeMap);

            configuration.Common.AddEventType("MapTypeEmpty", new Dictionary<string, object>());

            var testXMLNoSchemaType = new ConfigurationCommonEventTypeXMLDOM();
            testXMLNoSchemaType.RootElementName = "Myevent";
            configuration.Common.AddEventType("TestXMLNoSchemaType", testXMLNoSchemaType);

            IDictionary<string, object> myConfiguredMape = new Dictionary<string, object>();
            myConfiguredMape.Put("bean", "SupportBean");
            myConfiguredMape.Put("beanarray", "SupportBean_S0[]");
            configuration.Common.AddEventType("MyConfiguredMap", myConfiguredMape);

            configuration.Common.Logging.IsEnableQueryPlan = true;
            configuration.Runtime.Execution.IsPrioritized = true;

            configuration.Common.AddVariable("myvar", typeof(int?), 10);

            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.LevelZero));
            configuration.Common.AddImportType(typeof(SupportChainTop));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.NullPrimitive));
            configuration.Common.AddImportType(typeof(EPLOtherStaticFunctions.PrimitiveConversionLib));
            configuration.Common.AddImportType(typeof(Rectangle));

            configuration.Compiler.ByteCode.AllowSubscriber = true;
            configuration.Compiler.AddPlugInSingleRowFunction(
                "sleepme",
                typeof(SupportStaticMethodLib),
                "Sleep",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED);
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherAsKeywordBacktick()
        {
            RegressionRunner.Run(session, EPLOtherAsKeywordBacktick.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherComments()
        {
            RegressionRunner.Run(session, new EPLOtherComments());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherCreateIndex()
        {
            RegressionRunner.Run(session, EPLOtherCreateIndex.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherForGroupDelivery()
        {
            RegressionRunner.Run(session, EPLOtherForGroupDelivery.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherInvalid()
        {
            RegressionRunner.Run(session, EPLOtherInvalid.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherIStreamRStreamKeywords()
        {
            RegressionRunner.Run(session, EPLOtherIStreamRStreamKeywords.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherLiteralConstants()
        {
            RegressionRunner.Run(session, new EPLOtherLiteralConstants());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherPatternEventProperties()
        {
            RegressionRunner.Run(session, EPLOtherPatternEventProperties.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherPatternQueries()
        {
            RegressionRunner.Run(session, EPLOtherPatternQueries.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherPlanExcludeHint()
        {
            RegressionRunner.Run(session, EPLOtherPlanExcludeHint.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectExpr()
        {
            RegressionRunner.Run(session, EPLOtherSelectExpr.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectExprEventBeanAnnotation()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprEventBeanAnnotation.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectExprSQLCompat()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprSQLCompat.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectExprStreamSelector()
        {
            RegressionRunner.Run(session, EPLOtherSelectExprStreamSelector.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectJoin()
        {
            RegressionRunner.Run(session, EPLOtherSelectJoin.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSelectWildcardWAdditional()
        {
            RegressionRunner.Run(session, EPLOtherSelectWildcardWAdditional.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherSplitStream()
        {
            RegressionRunner.Run(session, EPLOtherSplitStream.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherStaticFunctions()
        {
            RegressionRunner.Run(session, EPLOtherStaticFunctions.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherStreamExpr()
        {
            RegressionRunner.Run(session, EPLOtherStreamExpr.Executions());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherUnaryMinus()
        {
            RegressionRunner.Run(session, new EPLOtherUnaryMinus());
        }
        
        [Test, RunInApplicationDomain]
        public void TestEPLOtherNestedClass()
        {
            RegressionRunner.Run(session, EPLOtherNestedClass.Executions());
        }
        
        /// <summary>
        /// Auto-test(s): EPLOtherPlanInKeywordQuery
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherPlanInKeywordQuery : AbstractTestBase
        {
            public TestEPLOtherPlanInKeywordQuery() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithQueryPlan2Stream() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithQueryPlan2Stream());

            [Test, RunInApplicationDomain]
            public void WithQueryPlan3Stream() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithQueryPlan3Stream());

            [Test, RunInApplicationDomain]
            public void WithMultiIdxConstants() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithMultiIdxConstants());

            [Test, RunInApplicationDomain]
            public void WithSingleIdxConstants() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithSingleIdxConstants());

            [Test, RunInApplicationDomain]
            public void WithSingleIdxSubquery() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithSingleIdxSubquery());

            [Test, RunInApplicationDomain]
            public void WithSingleIdxMultipleInAndMultirow() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithSingleIdxMultipleInAndMultirow());

            [Test, RunInApplicationDomain]
            public void WithMultiIdxSubquery() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithMultiIdxSubquery());

            [Test, RunInApplicationDomain]
            public void WithMultiIdxMultipleInAndMultirow() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithMultiIdxMultipleInAndMultirow());

            [Test, RunInApplicationDomain]
            public void WithNotIn() => RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.WithNotIn());
        }
        
        /// <summary>
        /// Auto-test(s): EPLOtherUpdateIStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherUpdateIStream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherUpdateIStream : AbstractTestBase
        {
            public TestEPLOtherUpdateIStream() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithExpression() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithExpression());

            [Test, RunInApplicationDomain]
            public void WithArrayElementInvalid() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElementInvalid());

            [Test, RunInApplicationDomain]
            public void WithArrayElementBoxed() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElementBoxed());

            [Test, RunInApplicationDomain]
            public void WithArrayElement() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElement());

            [Test]
            public void WithMapIndexProps() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithMapIndexProps());

            [Test, RunInApplicationDomain]
            public void WithSubqueryMultikeyWArray() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithSubqueryMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithListenerDeliveryMultiupdateMixed() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithListenerDeliveryMultiupdateMixed());

            [Test, RunInApplicationDomain]
            public void WithListenerDeliveryMultiupdate() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithListenerDeliveryMultiupdate());

            [Test, RunInApplicationDomain]
            public void WithUnprioritizedOrder() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithUnprioritizedOrder());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithCopyMethod() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithCopyMethod());

            [Test, RunInApplicationDomain]
            public void WithSendRouteSenderPreprocess() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithSendRouteSenderPreprocess());

            [Test, RunInApplicationDomain]
            public void WithWrappedObject() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithWrappedObject());

            [Test, RunInApplicationDomain]
            public void WithXMLEvent() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithXMLEvent());

            [Test, RunInApplicationDomain]
            public void WithSODA() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithSODA());

            [Test, RunInApplicationDomain]
            public void WithInsertDirectBeanTypeInheritance() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithInsertDirectBeanTypeInheritance());

            [Test, RunInApplicationDomain]
            public void WithTypeWidener() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithTypeWidener());

            [Test, RunInApplicationDomain]
            public void WithNamedWindow() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFieldsWithPriority() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithFieldsWithPriority());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoWMapNoWhere() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithInsertIntoWMapNoWhere());

            [Test, RunInApplicationDomain]
            public void WithInsertIntoWBeanWhere() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithInsertIntoWBeanWhere());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithFieldUpdateOrder() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithFieldUpdateOrder());

            [Test, RunInApplicationDomain]
            public void WithBean() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithBean());
        }
        
        /// <summary>
        /// Auto-test(s): EPLOtherCreateExpression
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherCreateExpression.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherCreateExpression : AbstractTestBase
        {
            public TestEPLOtherCreateExpression() : base(Configure) { }

            [Test, RunInApplicationDomain]
            public void WithExpressionUse() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithExpressionUse());

            [Test, RunInApplicationDomain]
            public void WithScriptUse() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithScriptUse());

            [Test, RunInApplicationDomain]
            public void WithExprAndScriptLifecycleAndFilter() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithExprAndScriptLifecycleAndFilter());

            [Test, RunInApplicationDomain]
            public void WithParseSpecialAndMixedExprAndScript() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithParseSpecialAndMixedExprAndScript());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherDistinct
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherDistinct.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherDistinct : AbstractTestBase
        {
            public TestEPLOtherDistinct() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDistinctVariantStream() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctVariantStream());

            [Test, RunInApplicationDomain]
            public void WithDistinctOnSelectMultikeyWArray() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctOnSelectMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithDistinctIterateMultikeyWArray() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctIterateMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithDistinctFireAndForgetMultikeyWArray() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctFireAndForgetMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithDistinctOutputLimitMultikeyWArrayTwoArray() => RegressionRunner.Run(
                _session,
                EPLOtherDistinct.WithDistinctOutputLimitMultikeyWArrayTwoArray());

            [Test, RunInApplicationDomain]
            public void WithDistinctOutputLimitMultikeyWArraySingleArray() => RegressionRunner.Run(
                _session,
                EPLOtherDistinct.WithDistinctOutputLimitMultikeyWArraySingleArray());

            [Test, RunInApplicationDomain]
            public void WithDistinctWildcardJoinPatternTwo() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctWildcardJoinPatternTwo());

            [Test, RunInApplicationDomain]
            public void WithDistinctWildcardJoinPatternOne() => RegressionRunner.Run(_session, EPLOtherDistinct.WithDistinctWildcardJoinPatternOne());

            [Test, RunInApplicationDomain]
            public void WithOutputRateSnapshotColumn() => RegressionRunner.Run(_session, EPLOtherDistinct.WithOutputRateSnapshotColumn());

            [Test, RunInApplicationDomain]
            public void WithOutputLimitEveryColumn() => RegressionRunner.Run(_session, EPLOtherDistinct.WithOutputLimitEveryColumn());

            [Test, RunInApplicationDomain]
            public void WithMapEventWildcard() => RegressionRunner.Run(_session, EPLOtherDistinct.WithMapEventWildcard());

            [Test, RunInApplicationDomain]
            public void WithBeanEventWildcardPlusCols() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBeanEventWildcardPlusCols());

            [Test, RunInApplicationDomain]
            public void WithBeanEventWildcardSODA() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBeanEventWildcardSODA());

            [Test, RunInApplicationDomain]
            public void WithBeanEventWildcardThisProperty() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBeanEventWildcardThisProperty());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, EPLOtherDistinct.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithOnDemandAndOnSelect() => RegressionRunner.Run(_session, EPLOtherDistinct.WithOnDemandAndOnSelect());

            [Test, RunInApplicationDomain]
            public void WithBatchWindowInsertInto() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBatchWindowInsertInto());

            [Test, RunInApplicationDomain]
            public void WithBatchWindowJoin() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBatchWindowJoin());

            [Test, RunInApplicationDomain]
            public void WithBatchWindow() => RegressionRunner.Run(_session, EPLOtherDistinct.WithBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithOutputSimpleColumn() => RegressionRunner.Run(_session, EPLOtherDistinct.WithOutputSimpleColumn());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherCreateSchema
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherCreateSchema.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherCreateSchema : AbstractTestBase
        {
            public TestEPLOtherCreateSchema() : base(Configure)
            {
            }

            [Test]
            public void WithCopyFromDeepWithValueObject() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithCopyFromDeepWithValueObject());

            [Test, RunInApplicationDomain]
            public void WithBeanImport() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithBeanImport());

            [Test, RunInApplicationDomain]
            public void WithSameCRC() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithSameCRC());

            [Test, RunInApplicationDomain]
            public void WithVariantType() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithVariantType());

            [Test, RunInApplicationDomain]
            public void WithWithEventType() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithWithEventType());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithCopyFromOrderObjectArray() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithCopyFromOrderObjectArray());

            [Test, RunInApplicationDomain]
            public void WithInherit() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithInherit());

            [Test, RunInApplicationDomain]
            public void WithNestableMapArray() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithNestableMapArray());

            [Test, RunInApplicationDomain]
            public void WithModelPONO() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithModelPONO());

            [Test, RunInApplicationDomain]
            public void WithColDefPlain() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithColDefPlain());

            [Test, RunInApplicationDomain]
            public void WithAvroSchemaWAnnotation() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithAvroSchemaWAnnotation());

            [Test, RunInApplicationDomain]
            public void WithConfiguredNotRemoved() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithConfiguredNotRemoved());

            [Test, RunInApplicationDomain]
            public void WithCopyProperties() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithCopyProperties());

            [Test, RunInApplicationDomain]
            public void WithArrayPrimitiveType() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithArrayPrimitiveType());

            [Test, RunInApplicationDomain]
            public void WithPublicSimple() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithPublicSimple());

            [Test, RunInApplicationDomain]
            public void WithPathSimple() => RegressionRunner.Run(_session, EPLOtherCreateSchema.WithPathSimple());
        }
    }
} // end of namespace