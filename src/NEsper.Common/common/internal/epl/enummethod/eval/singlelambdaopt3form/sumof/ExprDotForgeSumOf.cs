///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf : ExprDotForgeLambdaThreeForm
    {
        private ExprDotEvalSumMethodFactory aggMethodFactory;

        protected override EPChainableType InitAndNoParamsReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            aggMethodFactory = GetAggregatorFactory(collectionComponentType);
            return EPChainableTypeHelper.SingleValue(aggMethodFactory.ValueType.GetBoxedType());
        }

        protected override ThreeFormNoParamFactory.ForgeFunction NoParamsForge(
            EnumMethodEnum enumMethod,
            EPChainableType type,
            StatementCompileTimeServices services)
        {
            return streamCountIncoming => new EnumSumScalarNoParams(streamCountIncoming, aggMethodFactory);
        }

        protected override ThreeFormInitFunction InitAndSingleParamReturnType(
            EventType inputEventType,
            Type collectionComponentType)
        {
            return lambda => {
                var type = ValidateNonNull(lambda.BodyForge.EvaluationType);
                aggMethodFactory = GetAggregatorFactory(type);
                var returnType = aggMethodFactory.ValueType.GetBoxedType();
                return EPChainableTypeHelper.SingleValue(returnType);
            };
        }

        protected override ThreeFormEventPlainFactory.ForgeFunction SingleParamEventPlain(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                typeInfo,
                services) => new EnumSumEvent(lambda, aggMethodFactory);
        }

        protected override ThreeFormEventPlusFactory.ForgeFunction SingleParamEventPlus(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                fieldType,
                numParameters,
                typeInfo,
                services) => new EnumSumEventPlus(lambda, fieldType, numParameters, aggMethodFactory);
        }

        protected override ThreeFormScalarFactory.ForgeFunction SingleParamScalar(EnumMethodEnum enumMethod)
        {
            return (
                lambda,
                eventType,
                numParams,
                typeInfo,
                services) => new EnumSumScalar(lambda, eventType, numParams, aggMethodFactory);
        }

        private static ExprDotEvalSumMethodFactory GetAggregatorFactory(Type evalType)
        {
            if (evalType.IsTypeDecimal()) {
                return ExprDotEvalSumMethodFactoryDecimal.INSTANCE;
            }
            else if (evalType.IsFloatingPointClass()) {
                return ExprDotEvalSumMethodFactoryDouble.INSTANCE;
            }
            else if (evalType.IsTypeBigInteger()) {
                return ExprDotEvalSumMethodFactoryBigInteger.INSTANCE;
            }
            else if (evalType.IsTypeInt64()) {
                return ExprDotEvalSumMethodFactoryLong.INSTANCE;
            }
            else {
                return ExprDotEvalSumMethodFactoryInteger.INSTANCE;
            }
        }

        private static void CodegenReturnSumOrNull(CodegenBlock block)
        {
            block
                .IfCondition(EqualsIdentity(Ref("cnt"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Ref("sum"));
        }
    }
} // end of namespace