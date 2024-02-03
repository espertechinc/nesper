///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.suite.expr.exprcore;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.epl;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

using SupportBeanComplexProps = com.espertech.esper.regressionlib.support.bean.SupportBeanComplexProps;
using SupportMarkerInterface = com.espertech.esper.regressionlib.support.bean.SupportMarkerInterface;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprCore : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            foreach (var clazz in new[] {
                         typeof(SupportBean),
                         typeof(SupportBean_S0),
                         typeof(SupportBeanArrayCollMap),
                         typeof(SupportBeanComplexProps),
                         typeof(SupportBean_StringAlphabetic),
                         typeof(SupportMarkerInterface),
                         typeof(SupportBeanDynRoot),
                         typeof(SupportMarketDataBean),
                         typeof(SupportBeanWithEnum),
                         typeof(SupportEnumTwo),
                         typeof(SupportEventTypeErasure),
                         typeof(SupportChainTop),
                         typeof(SupportLevelZero),
                         typeof(SupportEventNode),
                         typeof(SupportEventNodeData),
                         typeof(SupportBeanCombinedProps),
                         typeof(SupportBeanNumeric),
                         typeof(ISupportA),
                         typeof(ISupportABCImpl),
                         typeof(ISupportAImpl),
                         typeof(SupportBean_ST0),
                         typeof(SupportBeanObject),
                         typeof(SupportEventWithManyArray),
                         typeof(SupportBeanWithArray)
                     }

                    ) {
                configuration.Common.AddEventType(clazz);
            }

            configuration.Common.EventMeta.AvroSettings.IsEnableAvro = true;
            configuration.Common.AddEventType(
                "MyDateType",
                CollectionUtil.PopulateNameValueMap(
                    "yyyymmdd",
                    typeof(string),
                    "yyyymmddhhmmss",
                    typeof(string),
                    "hhmmss",
                    typeof(string),
                    "yyyymmddhhmmsszz",
                    typeof(string)));

            configuration.Common.AddImportNamespace(typeof(IList<>));
            configuration.Common.AddImportType(typeof(SupportBean));
            configuration.Common.AddImportType(typeof(SupportEnum));
            configuration.Common.AddImportType(typeof(SupportPrivateCtor));
            configuration.Common.AddImportType(typeof(SupportObjectCtor));
            configuration.Common.AddImportType(typeof(SupportEnumTwo));
            configuration.Common.AddImportType(typeof(SupportEnumTwoExtensions));
            configuration.Common.AddImportType(typeof(SupportStaticMethodLib));
            
            var map = new Dictionary<string, object>();
            map.Put("anInt", typeof(string));
            map.Put("anDouble", typeof(string));
            map.Put("anLong", typeof(string));
            map.Put("anFloat", typeof(string));
            map.Put("anByte", typeof(string));
            map.Put("anShort", typeof(string));
            map.Put("IntPrimitive", typeof(int));
            map.Put("IntBoxed", typeof(int?));
            configuration.Common.AddEventType("StaticTypeMapEvent", map);
            configuration.Compiler.ByteCode.IsAllowSubscriber = true;
        }

        [Test, RunInApplicationDomain]
        public void TestExprCoreConcat()
        {
            RegressionRunner.Run(_session, new ExprCoreConcat());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreMath
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreMath.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreMath : AbstractTestBase
        {
            public TestExprCoreMath() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithModulo() => RegressionRunner.Run(_session, ExprCoreMath.WithModulo());

            [Test, RunInApplicationDomain]
            public void WithShortAndByteArithmetic() => RegressionRunner.Run(
                _session,
                ExprCoreMath.WithShortAndByteArithmetic());

            [Test, RunInApplicationDomain]
            public void WithBigIntConv() => RegressionRunner.Run(_session, ExprCoreMath.WithBigIntConv());

            [Test, RunInApplicationDomain]
            public void WithBigInt() => RegressionRunner.Run(_session, ExprCoreMath.WithBigInt());

            [Test, RunInApplicationDomain]
            public void WithDecimalConv() => RegressionRunner.Run(_session, ExprCoreMath.WithDecimalConv());

            [Test, RunInApplicationDomain]
            public void WithDecimal() => RegressionRunner.Run(_session, ExprCoreMath.WithDecimal());

            [Test, RunInApplicationDomain]
            public void WithIntWNull() => RegressionRunner.Run(_session, ExprCoreMath.WithIntWNull());

            [Test, RunInApplicationDomain]
            public void WithFloat() => RegressionRunner.Run(_session, ExprCoreMath.WithFloat());

            [Test, RunInApplicationDomain]
            public void WithLong() => RegressionRunner.Run(_session, ExprCoreMath.WithLong());

            [Test, RunInApplicationDomain]
            public void WithDouble() => RegressionRunner.Run(_session, ExprCoreMath.WithDouble());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreInBetween
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreInBetween.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreInBetween : AbstractTestBase
        {
            public TestExprCoreInBetween() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInBetweenInvalid() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInBetweenInvalid());

            [Test, RunInApplicationDomain]
            public void WithBetweenNumericCoercionDouble() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithBetweenNumericCoercionDouble());

            [Test, RunInApplicationDomain]
            public void WithInRange() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInRange());

            [Test, RunInApplicationDomain]
            public void WithBetweenNumericCoercionLong() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithBetweenNumericCoercionLong());

            [Test, RunInApplicationDomain]
            public void WithInNumericCoercionDouble() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInNumericCoercionDouble());

            [Test, RunInApplicationDomain]
            public void WithInNumericCoercionLong() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInNumericCoercionLong());

            [Test, RunInApplicationDomain]
            public void WithInBoolExpr() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInBoolExpr());

            [Test, RunInApplicationDomain]
            public void WithBetweenNumericExpr() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithBetweenNumericExpr());

            [Test, RunInApplicationDomain]
            public void WithBetweenStringExpr() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithBetweenStringExpr());

            [Test, RunInApplicationDomain]
            public void WithBetweenBigIntBigDecExpr() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithBetweenBigIntBigDecExpr());

            [Test, RunInApplicationDomain]
            public void WithInStringExpr() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInStringExpr());

            [Test, RunInApplicationDomain]
            public void WithInStringExprOM() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInStringExprOM());

            [Test, RunInApplicationDomain]
            public void WithInCollectionArrayConst() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionArrayConst());

            [Test, RunInApplicationDomain]
            public void WithInCollectionObjectArrayProp() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionObjectArrayProp());

            [Test, RunInApplicationDomain]
            public void WithInCollectionMixed() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionMixed());

            [Test, RunInApplicationDomain]
            public void WithInCollectionMaps() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionMaps());

            [Test, RunInApplicationDomain]
            public void WithInCollectionColl() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionColl());

            [Test, RunInApplicationDomain]
            public void WithInCollectionArrays() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionArrays());

            [Test, RunInApplicationDomain]
            public void WithInCollectionArrayProp() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInCollectionArrayProp());

            [Test, RunInApplicationDomain]
            public void WithInArraySubstitution() => RegressionRunner.Run(
                _session,
                ExprCoreInBetween.WithInArraySubstitution());

            [Test, RunInApplicationDomain]
            public void WithInObject() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInObject());

            [Test, RunInApplicationDomain]
            public void WithInNumeric() => RegressionRunner.Run(_session, ExprCoreInBetween.WithInNumeric());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreInstanceOf
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreInstanceOf.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreInstanceOf : AbstractTestBase
        {
            public TestExprCoreInstanceOf() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDynamicSuperTypeAndInterface() => RegressionRunner.Run(
                _session,
                ExprCoreInstanceOf.WithDynamicSuperTypeAndInterface());

            [Test, RunInApplicationDomain]
            public void WithDynamicPropertyNativeTypes() => RegressionRunner.Run(
                _session,
                ExprCoreInstanceOf.WithDynamicPropertyNativeTypes());

            [Test, RunInApplicationDomain]
            public void WithInstanceofStringAndNullCompile() => RegressionRunner.Run(
                _session,
                ExprCoreInstanceOf.WithInstanceofStringAndNullCompile());

            [Test, RunInApplicationDomain]
            public void WithInstanceofStringAndNullOM() => RegressionRunner.Run(
                _session,
                ExprCoreInstanceOf.WithInstanceofStringAndNullOM());

            [Test, RunInApplicationDomain]
            public void WithInstanceofSimple() => RegressionRunner.Run(
                _session,
                ExprCoreInstanceOf.WithInstanceofSimple());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreArrayAtElement
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreArrayAtElement.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreArrayAtElement : AbstractTestBase
        {
            public TestExprCoreArrayAtElement() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWithStringSplit() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithWithStringSplit());

            [Test, RunInApplicationDomain]
            public void WithAdditionalInvalid() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithAdditionalInvalid());

            [Test, RunInApplicationDomain]
            public void WithWithStaticMethodAndUDF() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithWithStaticMethodAndUDF());

            [Test, RunInApplicationDomain]
            public void WithVariableRootedChained() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithVariableRootedChained());

            [Test, RunInApplicationDomain]
            public void WithVariableRootedTopLevelProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithVariableRootedTopLevelProp());

            [Test, RunInApplicationDomain]
            public void WithPropRootedNestedNestedArrayProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithPropRootedNestedNestedArrayProp());

            [Test, RunInApplicationDomain]
            public void WithPropRootedNestedArrayProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithPropRootedNestedArrayProp());

            [Test, RunInApplicationDomain]
            public void WithPropRootedNestedNestedProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithPropRootedNestedNestedProp());

            [Test, RunInApplicationDomain]
            public void WithPropRootedNestedProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithPropRootedNestedProp());

            [Test, RunInApplicationDomain]
            public void WithPropRootedTopLevelProp() => RegressionRunner.Run(
                _session,
                ExprCoreArrayAtElement.WithPropRootedTopLevelProp());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreTypeOf
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreTypeOf.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreTypeOf : AbstractTestBase
        {
            public TestExprCoreTypeOf() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithVariantStream() => RegressionRunner.Run(_session, ExprCoreTypeOf.WithVariantStream());

            [Test, RunInApplicationDomain]
            public void WithDynamicProps() => RegressionRunner.Run(_session, ExprCoreTypeOf.WithDynamicProps());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreTypeOf.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNamedUnnamedPONO() => RegressionRunner.Run(_session, ExprCoreTypeOf.WithNamedUnnamedPONO());

            [Test]
            public void WithFragment() => RegressionRunner.Run(_session, ExprCoreTypeOf.WithFragment());
        }

        /// <summary>
        /// Auto-test(s): ExprCorePrevious
        /// <code>
        /// RegressionRunner.Run(_session, ExprCorePrevious.Executions());
        /// </code>
        /// </summary>
        public class TestExprCorePrevious : AbstractTestBase
        {
            public TestExprCorePrevious() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCorePrevious.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchWindowJoin() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithTimeBatchWindowJoin());

            [Test, RunInApplicationDomain]
            public void WithPrevCountStarWithStaticMethod() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithPrevCountStarWithStaticMethod());

            [Test, RunInApplicationDomain]
            public void WithExtTimedBatch() => RegressionRunner.Run(_session, ExprCorePrevious.WithExtTimedBatch());

            [Test, RunInApplicationDomain]
            public void WithSortWindow() => RegressionRunner.Run(_session, ExprCorePrevious.WithSortWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowDynamic() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithLengthWindowDynamic());

            [Test, RunInApplicationDomain]
            public void WithLengthBatch() => RegressionRunner.Run(_session, ExprCorePrevious.WithLengthBatch());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchWindow() => RegressionRunner.Run(_session, ExprCorePrevious.WithTimeBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithExtTimedWindow() => RegressionRunner.Run(_session, ExprCorePrevious.WithExtTimedWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeWindow() => RegressionRunner.Run(_session, ExprCorePrevious.WithTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithLengthWindowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithExtTimeWindowPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithExtTimeWindowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithTimeWindowPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithTimeWindowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithLengthBatchPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithLengthBatchPerGroup());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithTimeBatchPerGroup());

            [Test, RunInApplicationDomain]
            public void WithSortWindowPerGroup() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithSortWindowPerGroup());

            [Test, RunInApplicationDomain]
            public void WithExprNameAndTypeAndSODA() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithExprNameAndTypeAndSODA());

            [Test, RunInApplicationDomain]
            public void WithPerGroupTwoCriteria() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithPerGroupTwoCriteria());

            [Test, RunInApplicationDomain]
            public void WithPrevCountStar() => RegressionRunner.Run(_session, ExprCorePrevious.WithPrevCountStar());

            [Test, RunInApplicationDomain]
            public void WithTimeBatch() => RegressionRunner.Run(_session, ExprCorePrevious.WithTimeBatch());

            [Test, RunInApplicationDomain]
            public void WithLengthWindow() => RegressionRunner.Run(_session, ExprCorePrevious.WithLengthWindow());

            [Test, RunInApplicationDomain]
            public void WithPrevStream() => RegressionRunner.Run(_session, ExprCorePrevious.WithPrevStream());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowWhere() => RegressionRunner.Run(
                _session,
                ExprCorePrevious.WithLengthWindowWhere());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreNewStruct
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreNewStruct.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreNewStruct : AbstractTestBase
        {
            public TestExprCoreNewStruct() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithWithBacktick() => RegressionRunner.Run(_session, ExprCoreNewStruct.WithWithBacktick());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreNewStruct.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithNewWithCase() => RegressionRunner.Run(_session, ExprCoreNewStruct.WithNewWithCase());

            [Test, RunInApplicationDomain]
            public void WithDefaultColumnsAndSODA() => RegressionRunner.Run(
                _session,
                ExprCoreNewStruct.WithDefaultColumnsAndSODA());

            [Test, RunInApplicationDomain]
            public void WithNewWRepresentation() => RegressionRunner.Run(
                _session,
                ExprCoreNewStruct.WithNewWRepresentation());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreLikeRegexp
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreLikeRegexp.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreLikeRegexp : AbstractTestBase
        {
            public TestExprCoreLikeRegexp() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithLikeRegexNumericAndNull() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithLikeRegexNumericAndNull());

            [Test, RunInApplicationDomain]
            public void WithRegexStringAndNullCompile() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithRegexStringAndNullCompile());

            [Test, RunInApplicationDomain]
            public void WithLikeRegexStringAndNullOM() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithLikeRegexStringAndNullOM());

            [Test, RunInApplicationDomain]
            public void WithLikeRegexEscapedChar() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithLikeRegexEscapedChar());

            [Test, RunInApplicationDomain]
            public void WithLikeRegexInvalid() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithLikeRegexInvalid());

            [Test, RunInApplicationDomain]
            public void WithLikeRegexStringAndNull() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithLikeRegexStringAndNull());

            [Test, RunInApplicationDomain]
            public void WithRegexpWExprs() => RegressionRunner.Run(_session, ExprCoreLikeRegexp.WithRegexpWExprs());

            [Test, RunInApplicationDomain]
            public void WithRegexpWConstants() => RegressionRunner.Run(
                _session,
                ExprCoreLikeRegexp.WithRegexpWConstants());

            [Test, RunInApplicationDomain]
            public void WithLikeWExprs() => RegressionRunner.Run(_session, ExprCoreLikeRegexp.WithLikeWExprs());

            [Test, RunInApplicationDomain]
            public void WithLikeWConstants() => RegressionRunner.Run(_session, ExprCoreLikeRegexp.WithLikeWConstants());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreNewInstance
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreNewInstance.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreNewInstance : AbstractTestBase
        {
            public TestExprCoreNewInstance() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithArrayInvalid() => RegressionRunner.Run(_session, ExprCoreNewInstance.WithArrayInvalid());

            [Test, RunInApplicationDomain]
            public void WithArrayInitTwoDim() => RegressionRunner.Run(
                _session,
                ExprCoreNewInstance.WithArrayInitTwoDim());

            [Test, RunInApplicationDomain]
            public void WithArrayInitOneDim() => RegressionRunner.Run(
                _session,
                ExprCoreNewInstance.WithArrayInitOneDim());

            [Test, RunInApplicationDomain]
            public void WithArraySized() => RegressionRunner.Run(_session, ExprCoreNewInstance.WithArraySized());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreNewInstance.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithStreamAlias() => RegressionRunner.Run(_session, ExprCoreNewInstance.WithStreamAlias());

            [Test, RunInApplicationDomain]
            public void WithKeyword() => RegressionRunner.Run(_session, ExprCoreNewInstance.WithKeyword());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreEqualsIs
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreEqualsIs.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreEqualsIs : AbstractTestBase
        {
            public TestExprCoreEqualsIs() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreEqualsIs.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithIsMultikeyWArray() =>
                RegressionRunner.Run(_session, ExprCoreEqualsIs.WithIsMultikeyWArray());

            [Test, RunInApplicationDomain]
            public void WithIsCoercionSameType() => RegressionRunner.Run(
                _session,
                ExprCoreEqualsIs.WithIsCoercionSameType());

            [Test, RunInApplicationDomain]
            public void WithIsCoercion() => RegressionRunner.Run(_session, ExprCoreEqualsIs.WithIsCoercion());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreDotExpression
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreDotExpression.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreDotExpression : AbstractTestBase
        {
            public TestExprCoreDotExpression() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithToArray() => RegressionRunner.Run(_session, ExprCoreDotExpression.WithToArray());

            [Test, RunInApplicationDomain]
            public void WithCollectionSelectFromGetAndSize() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithCollectionSelectFromGetAndSize());

            [Test, RunInApplicationDomain]
            public void WithNestedPropertyInstanceNW() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithNestedPropertyInstanceNW());

            [Test, RunInApplicationDomain]
            public void WithNestedPropertyInstanceExpr() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithNestedPropertyInstanceExpr());

            [Test, RunInApplicationDomain]
            public void WithArrayPropertySizeAndGetChained() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithArrayPropertySizeAndGetChained());

            [Test, RunInApplicationDomain]
            public void WithArrayPropertySizeAndGet() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithArrayPropertySizeAndGet());

            [Test, RunInApplicationDomain]
            public void WithChainedParameterized() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithChainedParameterized());

            [Test, RunInApplicationDomain]
            public void WithChainedUnparameterized() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithChainedUnparameterized());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreDotExpression.WithInvalid());

            [Test, RunInApplicationDomain]
            public void WithMapIndexPropertyRooted() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithMapIndexPropertyRooted());

            [Test, RunInApplicationDomain]
            public void WithExpressionEnumValue() => RegressionRunner.Run(
                _session,
                ExprCoreDotExpression.WithExpressionEnumValue());

            [Test, RunInApplicationDomain]
            public void WithObjectEquals() => RegressionRunner.Run(_session, ExprCoreDotExpression.WithObjectEquals());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreBigNumberSupport
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreBigNumberSupport.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreBigNumberSupport : AbstractTestBase
        {
            public TestExprCoreBigNumberSupport() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithCastAndUDF() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithCastAndUDF());

            [Test, RunInApplicationDomain]
            public void WithJoin() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithJoin());

            [Test, RunInApplicationDomain]
            public void WithFilterEquals() => RegressionRunner.Run(
                _session,
                ExprCoreBigNumberSupport.WithFilterEquals());

            [Test, RunInApplicationDomain]
            public void WithMinMax() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithMinMax());

            [Test, RunInApplicationDomain]
            public void WithAggregation() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithAggregation());

            [Test, RunInApplicationDomain]
            public void WithMath() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithMath());

            [Test, RunInApplicationDomain]
            public void WithIn() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithIn());

            [Test, RunInApplicationDomain]
            public void WithBetween() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithBetween());

            [Test, RunInApplicationDomain]
            public void WithRelOp() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithRelOp());

            [Test, RunInApplicationDomain]
            public void WithEquals() => RegressionRunner.Run(_session, ExprCoreBigNumberSupport.WithEquals());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreAndOrNot
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreAndOrNot.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreAndOrNot : AbstractTestBase
        {
            public TestExprCoreAndOrNot() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAndOrNotCombined() =>
                RegressionRunner.Run(_session, ExprCoreAndOrNot.WithAndOrNotCombined());

            [Test, RunInApplicationDomain]
            public void WithNotWithVariable() => RegressionRunner.Run(_session, ExprCoreAndOrNot.WithNotWithVariable());

            [Test, RunInApplicationDomain]
            public void WithAndOrNotNull() => RegressionRunner.Run(_session, ExprCoreAndOrNot.WithAndOrNotNull());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreAnyAllSome
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreAnyAllSome.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreAnyAllSome : AbstractTestBase
        {
            public TestExprCoreAnyAllSome() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithAnyAllSomeEqualsAll() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithAnyAllSomeEqualsAll());

            [Test, RunInApplicationDomain]
            public void WithEqualsAllArray() => RegressionRunner.Run(_session, ExprCoreAnyAllSome.WithEqualsAllArray());

            [Test, RunInApplicationDomain]
            public void WithEqualsAny() => RegressionRunner.Run(_session, ExprCoreAnyAllSome.WithEqualsAny());

            [Test, RunInApplicationDomain]
            public void WithEqualsAnyBigInt() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithEqualsAnyBigInt());

            [Test, RunInApplicationDomain]
            public void WithEqualsAnyArray() => RegressionRunner.Run(_session, ExprCoreAnyAllSome.WithEqualsAnyArray());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpAllArray() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithRelationalOpAllArray());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpNullOrNoRows() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithRelationalOpNullOrNoRows());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpAnyArray() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithRelationalOpAnyArray());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpAll() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithRelationalOpAll());

            [Test, RunInApplicationDomain]
            public void WithRelationalOpAny() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithRelationalOpAny());

            [Test, RunInApplicationDomain]
            public void WithEqualsInNullOrNoRows() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithEqualsInNullOrNoRows());

            [Test, RunInApplicationDomain]
            public void WithAnyAllSomeInvalid() => RegressionRunner.Run(
                _session,
                ExprCoreAnyAllSome.WithAnyAllSomeInvalid());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreArray
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreArray.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreArray : AbstractTestBase
        {
            public TestExprCoreArray() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ExprCoreArray.WithSimple());

            [Test, RunInApplicationDomain]
            public void WithMapResult() => RegressionRunner.Run(_session, ExprCoreArray.WithMapResult());

            [Test, RunInApplicationDomain]
            public void WithCompile() => RegressionRunner.Run(_session, ExprCoreArray.WithCompile());

            [Test, RunInApplicationDomain]
            public void WithExpressionsOM() => RegressionRunner.Run(_session, ExprCoreArray.WithExpressionsOM());

            [Test, RunInApplicationDomain]
            public void WithComplexTypes() => RegressionRunner.Run(_session, ExprCoreArray.WithComplexTypes());

            [Test, RunInApplicationDomain]
            public void WithAvroArray() => RegressionRunner.Run(_session, ExprCoreArray.WithAvroArray());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreBitWiseOperators
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreBitWiseOperators.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreBitWiseOperators : AbstractTestBase
        {
            public TestExprCoreBitWiseOperators() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithOp() => RegressionRunner.Run(_session, ExprCoreBitWiseOperators.WithOp());

            [Test, RunInApplicationDomain]
            public void WithOpOM() => RegressionRunner.Run(_session, ExprCoreBitWiseOperators.WithOpOM());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreBitWiseOperators.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreCase
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreCase.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreCase : AbstractTestBase
        {
            public TestExprCoreCase() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSyntax1Sum() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1Sum());

            [Test, RunInApplicationDomain]
            public void WithSyntax1SumOM() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1SumOM());

            [Test, RunInApplicationDomain]
            public void WithSyntax1SumCompile() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1SumCompile());

            [Test, RunInApplicationDomain]
            public void WithSyntax1WithElse() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1WithElse());

            [Test, RunInApplicationDomain]
            public void WithSyntax1WithElseOM() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1WithElseOM());

            [Test, RunInApplicationDomain]
            public void WithSyntax1WithElseCompile() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithSyntax1WithElseCompile());

            [Test, RunInApplicationDomain]
            public void WithSyntax1Branches3() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1Branches3());

            [Test, RunInApplicationDomain]
            public void WithSyntax2() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2());

            [Test, RunInApplicationDomain]
            public void WithSyntax2StringsNBranches() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithSyntax2StringsNBranches());

            [Test, RunInApplicationDomain]
            public void WithSyntax2NoElseWithNull() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithSyntax2NoElseWithNull());

            [Test, RunInApplicationDomain]
            public void WithSyntax1WithNull() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax1WithNull());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithNullOM() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2WithNullOM());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithNullCompile() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithSyntax2WithNullCompile());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithNull() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2WithNull());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithNullBool() =>
                RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2WithNullBool());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithCoercion() =>
                RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2WithCoercion());

            [Test, RunInApplicationDomain]
            public void WithSyntax2WithinExpression() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithSyntax2WithinExpression());

            [Test, RunInApplicationDomain]
            public void WithSyntax2Sum() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2Sum());

            [Test, RunInApplicationDomain]
            public void WithSyntax2EnumChecks() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2EnumChecks());

            [Test, RunInApplicationDomain]
            public void WithSyntax2EnumResult() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2EnumResult());

            [Test, RunInApplicationDomain]
            public void WithSyntax2NoAsName() => RegressionRunner.Run(_session, ExprCoreCase.WithSyntax2NoAsName());

            [Test, RunInApplicationDomain]
            public void WithWithArrayResult() => RegressionRunner.Run(_session, ExprCoreCase.WithWithArrayResult());

            [Test, RunInApplicationDomain]
            public void WithWithTypeParameterizedProperty() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithWithTypeParameterizedProperty());

            [Test, RunInApplicationDomain]
            public void WithWithTypeParameterizedPropertyInvalid() => RegressionRunner.Run(
                _session,
                ExprCoreCase.WithWithTypeParameterizedPropertyInvalid());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreCast
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreCast.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreCast : AbstractTestBase
        {
            public TestExprCoreCast() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithDates() => RegressionRunner.Run(_session, ExprCoreCast.WithDates());

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ExprCoreCast.WithSimple());

            [Test, RunInApplicationDomain]
            public void WithSimpleMoreTypes() => RegressionRunner.Run(_session, ExprCoreCast.WithSimpleMoreTypes());

            [Test, RunInApplicationDomain]
            public void WithAsParse() => RegressionRunner.Run(_session, ExprCoreCast.WithAsParse());

            [Test, RunInApplicationDomain]
            public void WithDoubleAndNullOM() => RegressionRunner.Run(_session, ExprCoreCast.WithDoubleAndNullOM());

            [Test, RunInApplicationDomain]
            public void WithInterface() => RegressionRunner.Run(_session, ExprCoreCast.WithInterface());

            [Test, RunInApplicationDomain]
            public void WithStringAndNullCompile() => RegressionRunner.Run(
                _session,
                ExprCoreCast.WithStringAndNullCompile());

            [Test, RunInApplicationDomain]
            public void WithBoolean() => RegressionRunner.Run(_session, ExprCoreCast.WithBoolean());

            [Test, RunInApplicationDomain]
            public void WithWStaticType() => RegressionRunner.Run(_session, ExprCoreCast.WithWStaticType());

            [Test, RunInApplicationDomain]
            public void WithWArray() => RegressionRunner.Run(_session, ExprCoreCast.WithWArray());

            [Test, RunInApplicationDomain]
            public void WithGeneric() => RegressionRunner.Run(_session, ExprCoreCast.WithGeneric());

            [Test, RunInApplicationDomain]
            public void WithDecimalBigInt() => RegressionRunner.Run(_session, ExprCoreCast.WithDecimalBigInt());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreCoalesce
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreCoalesce.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreCoalesce : AbstractTestBase
        {
            public TestExprCoreCoalesce() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBeans() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithBeans());

            [Test, RunInApplicationDomain]
            public void WithLong() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithLong());

            [Test, RunInApplicationDomain]
            public void WithLongOM() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithLongOM());

            [Test, RunInApplicationDomain]
            public void WithLongCompile() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithLongCompile());

            [Test, RunInApplicationDomain]
            public void WithDouble() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithDouble());

            [Test, RunInApplicationDomain]
            public void WithNull() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithNull());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreCoalesce.WithInvalid());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreCurrentEvaluationContext
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreCurrentEvaluationContext.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreCurrentEvaluationContext : AbstractTestBase
        {
            public TestExprCoreCurrentEvaluationContext() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithEvalCtx() => RegressionRunner.Run(_session, ExprCoreCurrentEvaluationContext.WithEvalCtx());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreCurrentTimestamp
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreCurrentTimestamp.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreCurrentTimestamp : AbstractTestBase
        {
            public TestExprCoreCurrentTimestamp() : base(Configure)
            {
            }
        }

        /// <summary>
        /// Auto-test(s): ExprCoreExists
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreExists.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreExists : AbstractTestBase
        {
            public TestExprCoreExists() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithExistsSimple() => RegressionRunner.Run(_session, ExprCoreExists.WithExistsSimple());

            [Test, RunInApplicationDomain]
            public void WithExistsInner() => RegressionRunner.Run(_session, ExprCoreExists.WithExistsInner());

            [Test, RunInApplicationDomain]
            public void WithCastDoubleAndNullOM() => RegressionRunner.Run(
                _session,
                ExprCoreExists.WithCastDoubleAndNullOM());

            [Test, RunInApplicationDomain]
            public void WithCastStringAndNullCompile() => RegressionRunner.Run(
                _session,
                ExprCoreExists.WithCastStringAndNullCompile());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreMinMaxNonAgg
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreMinMaxNonAgg.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreMinMaxNonAgg : AbstractTestBase
        {
            public TestExprCoreMinMaxNonAgg() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithMinMax() => RegressionRunner.Run(_session, ExprCoreMinMaxNonAgg.WithMinMax());

            [Test, RunInApplicationDomain]
            public void WithMinMaxOM() => RegressionRunner.Run(_session, ExprCoreMinMaxNonAgg.WithMinMaxOM());

            [Test, RunInApplicationDomain]
            public void WithMinMaxCompile() => RegressionRunner.Run(_session, ExprCoreMinMaxNonAgg.WithMinMaxCompile());
        }

        /// <summary>
        /// Auto-test(s): ExprCorePrior
        /// <code>
        /// RegressionRunner.Run(_session, ExprCorePrior.Executions());
        /// </code>
        /// </summary>
        public class TestExprCorePrior : AbstractTestBase
        {
            public TestExprCorePrior() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithBoundedMultiple() => RegressionRunner.Run(_session, ExprCorePrior.WithBoundedMultiple());

            [Test, RunInApplicationDomain]
            public void WithExtTimedWindow() => RegressionRunner.Run(_session, ExprCorePrior.WithExtTimedWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchWindow() => RegressionRunner.Run(_session, ExprCorePrior.WithTimeBatchWindow());

            [Test, RunInApplicationDomain]
            public void WithNoDataWindowWhere() =>
                RegressionRunner.Run(_session, ExprCorePrior.WithNoDataWindowWhere());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowWhere() =>
                RegressionRunner.Run(_session, ExprCorePrior.WithLengthWindowWhere());

            [Test, RunInApplicationDomain]
            public void WithStreamAndVariable() =>
                RegressionRunner.Run(_session, ExprCorePrior.WithStreamAndVariable());

            [Test, RunInApplicationDomain]
            public void WithUnbound() => RegressionRunner.Run(_session, ExprCorePrior.WithUnbound());

            [Test, RunInApplicationDomain]
            public void WithUnboundSceneOne() => RegressionRunner.Run(_session, ExprCorePrior.WithUnboundSceneOne());

            [Test, RunInApplicationDomain]
            public void WithUnboundSceneTwo() => RegressionRunner.Run(_session, ExprCorePrior.WithUnboundSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithBoundedSingle() => RegressionRunner.Run(_session, ExprCorePrior.WithBoundedSingle());

            [Test, RunInApplicationDomain]
            public void WithLongRunningSingle() =>
                RegressionRunner.Run(_session, ExprCorePrior.WithLongRunningSingle());

            [Test, RunInApplicationDomain]
            public void WithLongRunningUnbound() =>
                RegressionRunner.Run(_session, ExprCorePrior.WithLongRunningUnbound());

            [Test, RunInApplicationDomain]
            public void WithLongRunningMultiple() => RegressionRunner.Run(
                _session,
                ExprCorePrior.WithLongRunningMultiple());

            [Test, RunInApplicationDomain]
            public void WithTimewindowStats() => RegressionRunner.Run(_session, ExprCorePrior.WithTimewindowStats());

            [Test, RunInApplicationDomain]
            public void WithTimeWindow() => RegressionRunner.Run(_session, ExprCorePrior.WithTimeWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindow() => RegressionRunner.Run(_session, ExprCorePrior.WithLengthWindow());

            [Test, RunInApplicationDomain]
            public void WithLengthWindowSceneTwo() => RegressionRunner.Run(
                _session,
                ExprCorePrior.WithLengthWindowSceneTwo());

            [Test, RunInApplicationDomain]
            public void WithSortWindow() => RegressionRunner.Run(_session, ExprCorePrior.WithSortWindow());

            [Test, RunInApplicationDomain]
            public void WithTimeBatchWindowJoin() => RegressionRunner.Run(
                _session,
                ExprCorePrior.WithTimeBatchWindowJoin());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreRelOp
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreRelOp.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreRelOp : AbstractTestBase
        {
            public TestExprCoreRelOp() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithTypes() => RegressionRunner.Run(_session, ExprCoreRelOp.WithTypes());

            [Test, RunInApplicationDomain]
            public void WithNull() => RegressionRunner.Run(_session, ExprCoreRelOp.WithNull());
        }

        /// <summary>
        /// Auto-test(s): ExprCoreEventIdentityEquals
        /// <code>
        /// RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.Executions());
        /// </code>
        /// </summary>
        public class TestExprCoreEventIdentityEquals : AbstractTestBase
        {
            public TestExprCoreEventIdentityEquals() : base(Configure)
            {
            }

            [Test, RunInApplicationDomain]
            public void WithSimple() => RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.WithSimple());

            [Test, RunInApplicationDomain]
            public void WithDocSample() => RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.WithDocSample());

            [Test, RunInApplicationDomain]
            public void WithSubquery() => RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.WithSubquery());

            [Test, RunInApplicationDomain]
            public void WithEnumMethod() =>
                RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.WithEnumMethod());

            [Test, RunInApplicationDomain]
            public void WithInvalid() => RegressionRunner.Run(_session, ExprCoreEventIdentityEquals.WithInvalid());
        }
    }
} // end of namespace
