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
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;
using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportBeanSimple = com.espertech.esper.regressionlib.support.bean.SupportBeanSimple;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLOther : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
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

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            
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

            configuration.Compiler.ByteCode.IsAllowSubscriber =true;
            configuration.Compiler.AddPlugInSingleRowFunction(
                "sleepme",
                typeof(SupportStaticMethodLib),
                "Sleep",
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.ENABLED);
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherComments()
        {
            RegressionRunner.Run(_session, new EPLOtherComments());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherLiteralConstants()
        {
            RegressionRunner.Run(_session, new EPLOtherLiteralConstants());
        }

        [Test, RunInApplicationDomain]
        public void TestEPLOtherUnaryMinus()
        {
            RegressionRunner.Run(_session, new EPLOtherUnaryMinus());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherPlanInKeywordQuery
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherPlanInKeywordQuery.Executions());
        /// </code>
        /// </summary>

        [Parallelizable(ParallelScope.None)]
        public class TestEPLOtherPlanInKeywordQuery : AbstractTestBase
        {
            public TestEPLOtherPlanInKeywordQuery() : base(Configure)
            {
            }

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
            public TestEPLOtherUpdateIStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithExpression() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithExpression());

            [Test, RunInApplicationDomain]
            public void WithArrayElementInvalid() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElementInvalid());

            [Test, RunInApplicationDomain]
            public void WithArrayElementBoxed() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElementBoxed());

            [Test, RunInApplicationDomain]
            public void WithArrayElement() => RegressionRunner.Run(_session, EPLOtherUpdateIStream.WithArrayElement());

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
            public TestEPLOtherCreateExpression() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithExpressionUse() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithExpressionUse());

            [Test, RunInApplicationDomain]
            public void WithScriptUse() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithScriptUse());

            [Test, RunInApplicationDomain]
            public void WithExprAndScriptLifecycleAndFilter() => RegressionRunner.Run(_session, EPLOtherCreateExpression.WithExprAndScriptLifecycleAndFilter());

            [Test, RunInApplicationDomain]
            public void WithParseSpecialAndMixedExprAndScript() => RegressionRunner.Run(
                _session,
                EPLOtherCreateExpression.WithParseSpecialAndMixedExprAndScript());

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

        /// <summary>
        /// Auto-test(s): EPLOtherAsKeywordBacktick
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherAsKeywordBacktick : AbstractTestBase
        {
            public TestEPLOtherAsKeywordBacktick() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOnSelectProperty() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithOnSelectProperty());

            [Test, RunInApplicationDomain]
            public void WithSubselect() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithSubselect());

            [Test, RunInApplicationDomain]
            public void WithnMergeAndUpdateAndSelect() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithnMergeAndUpdateAndSelect());

            [Test, RunInApplicationDomain]
            public void WithUpdateIStream() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithUpdateIStream());

            [Test, RunInApplicationDomain]
            public void WithOnTrigger() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithOnTrigger());

            [Test, RunInApplicationDomain]
            public void WithFromClause() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithFromClause());

            [Test, RunInApplicationDomain]
            public void WithFAFUpdateDelete() => RegressionRunner.Run(_session, EPLOtherAsKeywordBacktick.WithFAFUpdateDelete());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherCreateIndex
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherCreateIndex.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherCreateIndex : AbstractTestBase
        {
            public TestEPLOtherCreateIndex() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithThreeModule() => RegressionRunner.Run(_session, EPLOtherCreateIndex.WithThreeModule());

            [Test, RunInApplicationDomain]
            public void WithOneModule() => RegressionRunner.Run(_session, EPLOtherCreateIndex.WithOneModule());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherForGroupDelivery
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherForGroupDelivery.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherForGroupDelivery : AbstractTestBase
        {
            public TestEPLOtherForGroupDelivery() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithGroupDeliveryMultikeyWArrayTwoField() => RegressionRunner.Run(
                _session,
                EPLOtherForGroupDelivery.WithGroupDeliveryMultikeyWArrayTwoField());

            [Test, RunInApplicationDomain]
            public void WithGroupDeliveryMultikeyWArraySingleArray() => RegressionRunner.Run(
                _session,
                EPLOtherForGroupDelivery.WithGroupDeliveryMultikeyWArraySingleArray());

            [Test, RunInApplicationDomain]
            public void WithGroupDelivery() => RegressionRunner.Run(_session, EPLOtherForGroupDelivery.WithGroupDelivery());

            [Test, RunInApplicationDomain]
            public void WithDiscreteDelivery() => RegressionRunner.Run(_session, EPLOtherForGroupDelivery.WithDiscreteDelivery());

            [Test, RunInApplicationDomain]
            public void WithSubscriberOnly() => RegressionRunner.Run(_session, EPLOtherForGroupDelivery.WithSubscriberOnly());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherForGroupDelivery.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherInvalid
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherInvalid.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherInvalid : AbstractTestBase
        {
            public TestEPLOtherInvalid() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDifferentJoins() => RegressionRunner.Run(_session, EPLOtherInvalid.WithDifferentJoins());

            [Test, RunInApplicationDomain]
            public void WithLongTypeConstant() => RegressionRunner.Run(_session, EPLOtherInvalid.WithLongTypeConstant());

            [Test, RunInApplicationDomain]
            public void WithInvalidSyntax() => RegressionRunner.Run(_session, EPLOtherInvalid.WithInvalidSyntax());

            [Test, RunInApplicationDomain]
            public void WithInvalidFuncParams() => RegressionRunner.Run(_session, EPLOtherInvalid.WithInvalidFuncParams());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherIStreamRStreamKeywords
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherIStreamRStreamKeywords : AbstractTestBase
        {
            public TestEPLOtherIStreamRStreamKeywords() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithRStreamOutputSnapshot() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamOutputSnapshot());

            [Test, RunInApplicationDomain]
            public void WithIStreamJoin() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithIStreamJoin());

            [Test, RunInApplicationDomain]
            public void WithIStreamInsertIntoRStream() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithIStreamInsertIntoRStream());

            [Test, RunInApplicationDomain]
            public void WithIStreamOnly() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithIStreamOnly());

            [Test, RunInApplicationDomain]
            public void WithRStreamJoin() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamJoin());

            [Test, RunInApplicationDomain]
            public void WithRStreamInsertIntoRStream() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamInsertIntoRStream());

            [Test, RunInApplicationDomain]
            public void WithRStreamInsertInto() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamInsertInto());

            [Test, RunInApplicationDomain]
            public void WithRStreamOnly() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamOnly());

            [Test, RunInApplicationDomain]
            public void WithRStreamOnlyCompile() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamOnlyCompile());

            [Test, RunInApplicationDomain]
            public void WithRStreamOnlyOM() => RegressionRunner.Run(_session, EPLOtherIStreamRStreamKeywords.WithRStreamOnlyOM());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherPatternEventProperties
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherPatternEventProperties.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherPatternEventProperties : AbstractTestBase
        {
            public TestEPLOtherPatternEventProperties() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPropertiesOrPattern() => RegressionRunner.Run(_session, EPLOtherPatternEventProperties.WithPropertiesOrPattern());

            [Test, RunInApplicationDomain]
            public void WithPropertiesSimplePattern() => RegressionRunner.Run(_session, EPLOtherPatternEventProperties.WithPropertiesSimplePattern());

            [Test, RunInApplicationDomain]
            public void WithWildcardOrPattern() => RegressionRunner.Run(_session, EPLOtherPatternEventProperties.WithWildcardOrPattern());

            [Test, RunInApplicationDomain]
            public void WithWildcardSimplePattern() => RegressionRunner.Run(_session, EPLOtherPatternEventProperties.WithWildcardSimplePattern());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherPatternQueries
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherPatternQueries.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherPatternQueries : AbstractTestBase
        {
            public TestEPLOtherPatternQueries() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithPatternWindow() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithPatternWindow());

            [Test, RunInApplicationDomain]
            public void WithFollowedByAndWindow() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithFollowedByAndWindow());

            [Test, RunInApplicationDomain]
            public void WithAggregation() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithAggregation());

            [Test, RunInApplicationDomain]
            public void WithWhere() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithWhere());

            [Test, RunInApplicationDomain]
            public void WithWhereCompile() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithWhereCompile());

            [Test, RunInApplicationDomain]
            public void WithWhereOM() => RegressionRunner.Run(_session, EPLOtherPatternQueries.WithWhereOM());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherPlanExcludeHint
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherPlanExcludeHint.Executions());
        /// </code>
        /// </summary>

        [Parallelizable(ParallelScope.None)]
        public class TestEPLOtherPlanExcludeHint : AbstractTestBase
        {
            public TestEPLOtherPlanExcludeHint() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherPlanExcludeHint.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithJoin() => RegressionRunner.Run(_session, EPLOtherPlanExcludeHint.WithJoin());

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, EPLOtherPlanExcludeHint.WithDocSample());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectExpr
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectExpr.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectExpr : AbstractTestBase
        {
            public TestEPLOtherSelectExpr() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWindowStats() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithWindowStats());

            [Test, RunInApplicationDomain]
            public void WithGetEventType() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithGetEventType());

            [Test, RunInApplicationDomain]
            public void WithEscapeString() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithEscapeString());

            [Test, RunInApplicationDomain]
            public void WithKeywordsAllowed() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithKeywordsAllowed());

            [Test, RunInApplicationDomain]
            public void WithGraphSelect() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithGraphSelect());

            [Test, RunInApplicationDomain]
            public void WithPrecedenceNoColumnName() => RegressionRunner.Run(_session, EPLOtherSelectExpr.WithPrecedenceNoColumnName());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectExprEventBeanAnnotation
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectExprEventBeanAnnotation.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectExprEventBeanAnnotation : AbstractTestBase
        {
            public TestEPLOtherSelectExprEventBeanAnnotation() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWSubquery() => RegressionRunner.Run(_session, EPLOtherSelectExprEventBeanAnnotation.WithWSubquery());

            [Test]
            public void WithSimple() => RegressionRunner.Run(_session, EPLOtherSelectExprEventBeanAnnotation.WithSimple());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectExprSQLCompat
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectExprSQLCompat.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectExprSQLCompat : AbstractTestBase
        {
            public TestEPLOtherSelectExprSQLCompat() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void Withy() => RegressionRunner.Run(_session, EPLOtherSelectExprSQLCompat.Withy());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectExprStreamSelector
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectExprStreamSelector : AbstractTestBase
        {
            public TestEPLOtherSelectExprStreamSelector() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidSelect() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithInvalidSelect());

            [Test, RunInApplicationDomain]
            public void WithAloneJoinNoAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithAloneJoinNoAlias());

            [Test, RunInApplicationDomain]
            public void WithAloneJoinAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithAloneJoinAlias());

            [Test, RunInApplicationDomain]
            public void WithAloneNoJoinAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithAloneNoJoinAlias());

            [Test, RunInApplicationDomain]
            public void WithAloneNoJoinNoAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithAloneNoJoinNoAlias());

            [Test, RunInApplicationDomain]
            public void WithJoinNoAliasWithProperties() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithJoinNoAliasWithProperties());

            [Test, RunInApplicationDomain]
            public void WithNoJoinNoAliasWithProperties() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithNoJoinNoAliasWithProperties());

            [Test, RunInApplicationDomain]
            public void WithJoinWithAliasWithProperties() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithJoinWithAliasWithProperties());

            [Test, RunInApplicationDomain]
            public void WithNoJoinWithAliasWithProperties() => RegressionRunner.Run(
                _session,
                EPLOtherSelectExprStreamSelector.WithNoJoinWithAliasWithProperties());

            [Test, RunInApplicationDomain]
            public void WithJoinWildcardWithAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithJoinWildcardWithAlias());

            [Test, RunInApplicationDomain]
            public void WithNoJoinWildcardWithAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithNoJoinWildcardWithAlias());

            [Test, RunInApplicationDomain]
            public void WithJoinWildcardNoAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithJoinWildcardNoAlias());

            [Test, RunInApplicationDomain]
            public void WithNoJoinWildcardNoAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithNoJoinWildcardNoAlias());

            [Test, RunInApplicationDomain]
            public void WithObjectModelJoinAlias() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithObjectModelJoinAlias());

            [Test, RunInApplicationDomain]
            public void WithInsertFromPattern() => RegressionRunner.Run(_session, EPLOtherSelectExprStreamSelector.WithInsertFromPattern());

            [Test, RunInApplicationDomain]
            public void WithInsertTransposeNestedProperty() => RegressionRunner.Run(
                _session,
                EPLOtherSelectExprStreamSelector.WithInsertTransposeNestedProperty());

            [Test, RunInApplicationDomain]
            public void WithInvalidSelectWildcardProperty() => RegressionRunner.Run(
                _session,
                EPLOtherSelectExprStreamSelector.WithInvalidSelectWildcardProperty());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectJoin
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectJoin.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectJoin : AbstractTestBase
        {
            public TestEPLOtherSelectJoin() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNonUniquePerId() => RegressionRunner.Run(_session, EPLOtherSelectJoin.WithNonUniquePerId());

            [Test, RunInApplicationDomain]
            public void WithUniquePerId() => RegressionRunner.Run(_session, EPLOtherSelectJoin.WithUniquePerId());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSelectWildcardWAdditional
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSelectWildcardWAdditional : AbstractTestBase
        {
            public TestEPLOtherSelectWildcardWAdditional() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidRepeatedProperties() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithInvalidRepeatedProperties());

            [Test, RunInApplicationDomain]
            public void WithWildcardMapEvent() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithWildcardMapEvent());

            [Test, RunInApplicationDomain]
            public void WithCombinedProperties() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithCombinedProperties());

            [Test, RunInApplicationDomain]
            public void WithJoinCommonProperties() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithJoinCommonProperties());

            [Test, RunInApplicationDomain]
            public void WithJoinNoCommonProperties() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithJoinNoCommonProperties());

            [Test, RunInApplicationDomain]
            public void WithJoinInsertInto() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithJoinInsertInto());

            [Test, RunInApplicationDomain]
            public void WithSingleInsertInto() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithSingleInsertInto());

            [Test, RunInApplicationDomain]
            public void WithSingle() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithSingle());

            [Test, RunInApplicationDomain]
            public void WithSingleOM() => RegressionRunner.Run(_session, EPLOtherSelectWildcardWAdditional.WithSingleOM());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherSplitStream
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherSplitStream.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherSplitStream : AbstractTestBase
        {
            public TestEPLOtherSplitStream() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSubqueryMultikeyWArray() => RegressionRunner.Run(_session, EPLOtherSplitStream.WithSubqueryMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void With4Split() => RegressionRunner.Run(_session, EPLOtherSplitStream.With4Split());

            [Test, RunInApplicationDomain]
            public void With3SplitDefaultOutputFirst() => RegressionRunner.Run(_session, EPLOtherSplitStream.With3SplitDefaultOutputFirst());

            [Test, RunInApplicationDomain]
            public void With3SplitOutputAll() => RegressionRunner.Run(_session, EPLOtherSplitStream.With3SplitOutputAll());

            [Test, RunInApplicationDomain]
            public void With2SplitNoDefaultOutputAll() => RegressionRunner.Run(_session, EPLOtherSplitStream.With2SplitNoDefaultOutputAll());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, EPLOtherSplitStream.WithSubquery());

            [Test, RunInApplicationDomain]
            public void With1SplitDefault() => RegressionRunner.Run(_session, EPLOtherSplitStream.With1SplitDefault());

            [Test, RunInApplicationDomain]
            public void WithSplitPremptiveNamedWindow() => RegressionRunner.Run(_session, EPLOtherSplitStream.WithSplitPremptiveNamedWindow());

            [Test, RunInApplicationDomain]
            public void WithFromClause() => RegressionRunner.Run(_session, EPLOtherSplitStream.WithFromClause());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, EPLOtherSplitStream.WithInvalid());

            [Test, RunInApplicationDomain]
            public void With2SplitNoDefaultOutputFirst() => RegressionRunner.Run(_session, EPLOtherSplitStream.With2SplitNoDefaultOutputFirst());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherStaticFunctions
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherStaticFunctions.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherStaticFunctions : AbstractTestBase
        {
            public TestEPLOtherStaticFunctions() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithStaticFuncEnumConstant() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithStaticFuncEnumConstant());

            [Test, RunInApplicationDomain]
            public void WithStaticFuncWCurrentTimeStamp() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithStaticFuncWCurrentTimeStamp());

            [Test, RunInApplicationDomain]
            public void WithPrimitiveConversion() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithPrimitiveConversion());

            [Test, RunInApplicationDomain]
            public void WithPassthru() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithPassthru());

            [Test, RunInApplicationDomain]
            public void WithNestedFunction() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithNestedFunction());

            [Test, RunInApplicationDomain]
            public void WithOtherClauses() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithOtherClauses());

            [Test, RunInApplicationDomain]
            public void WithMultipleMethodInvocations() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithMultipleMethodInvocations());

            [Test, RunInApplicationDomain]
            public void WithComplexParameters() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithComplexParameters());

            [Test, RunInApplicationDomain]
            public void WithUserDefined() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithUserDefined());

            [Test, RunInApplicationDomain]
            public void WithTwoParameters() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithTwoParameters());

            [Test, RunInApplicationDomain]
            public void WithSingleParameter() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithSingleParameter());

            [Test, RunInApplicationDomain]
            public void WithSingleParameterCompile() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithSingleParameterCompile());

            [Test, RunInApplicationDomain]
            public void WithSingleParameterOM() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithSingleParameterOM());

            [Test, RunInApplicationDomain]
            public void WithPerfConstantParametersNested() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithPerfConstantParametersNested());

            [Test, RunInApplicationDomain]
            public void WithPerfConstantParameters() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithPerfConstantParameters());

            [Test, RunInApplicationDomain]
            public void WithNoParameters() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithNoParameters());

            [Test, RunInApplicationDomain]
            public void WithArrayParameter() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithArrayParameter());

            [Test, RunInApplicationDomain]
            public void WithPattern() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithPattern());

            [Test, RunInApplicationDomain]
            public void WithReturnsMapIndexProperty() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithReturnsMapIndexProperty());

            [Test, RunInApplicationDomain]
            public void WithEscape() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithEscape());

            [Test, RunInApplicationDomain]
            public void WithChainedStatic() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithChainedStatic());

            [Test, RunInApplicationDomain]
            public void WithChainedInstance() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithChainedInstance());

            [Test, RunInApplicationDomain]
            public void WithNullPrimitive() => RegressionRunner.Run(_session, EPLOtherStaticFunctions.WithNullPrimitive());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherStreamExpr
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherStreamExpr.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherStreamExpr : AbstractTestBase
        {
            public TestEPLOtherStreamExpr() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalidSelect() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithInvalidSelect());

            [Test, RunInApplicationDomain]
            public void WithPatternStreamSelectNoWildcard() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithPatternStreamSelectNoWildcard());

            [Test, RunInApplicationDomain]
            public void WithJoinStreamSelectNoWildcard() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithJoinStreamSelectNoWildcard());

            [Test, RunInApplicationDomain]
            public void WithStreamInstanceMethodNoAlias() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithStreamInstanceMethodNoAlias());

            [Test, RunInApplicationDomain]
            public void WithStreamInstanceMethodAliased() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithStreamInstanceMethodAliased());

            [Test, RunInApplicationDomain]
            public void WithInstanceMethodStatic() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithInstanceMethodStatic());

            [Test, RunInApplicationDomain]
            public void WithInstanceMethodOuterJoin() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithInstanceMethodOuterJoin());

            [Test, RunInApplicationDomain]
            public void WithStreamFunction() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithStreamFunction());

            [Test, RunInApplicationDomain]
            public void WithChainedParameterized() => RegressionRunner.Run(_session, EPLOtherStreamExpr.WithChainedParameterized());
        }

        /// <summary>
        /// Auto-test(s): EPLOtherNestedClass
        /// <code>
        /// RegressionRunner.Run(_session, EPLOtherNestedClass.Executions());
        /// </code>
        /// </summary>

        public class TestEPLOtherNestedClass : AbstractTestBase
        {
            public TestEPLOtherNestedClass() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithNestedClassEnum() => RegressionRunner.Run(_session, EPLOtherNestedClass.WithNestedClassEnum());
        }
    }
} // end of namespace