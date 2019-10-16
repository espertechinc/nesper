///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public partial class ExprDotForgeSumOf : ExprDotForgeEnumMethodBase
    {
        public override EventType[] GetAddStreamTypes(
            string enumMethodUsedName,
            IList<string> goesToNames,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprDotEvalParam> bodiesAndParameters,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            return ExprDotNodeUtility.GetSingleLambdaParamEventType(
                enumMethodUsedName,
                goesToNames,
                inputEventType,
                collectionComponentType,
                statementRawInfo,
                services);
        }

        public override EnumForge GetEnumForge(
            StreamTypeService streamTypeService,
            string enumMethodUsedName,
            IList<ExprDotEvalParam> bodiesAndParameters,
            EventType inputEventType,
            Type collectionComponentType,
            int numStreamsIncoming,
            bool disablePropertyExpressionEventCollCache,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (bodiesAndParameters.IsEmpty()) {
                var aggMethodFactoryInner = GetAggregatorFactory(collectionComponentType);
                TypeInfo = EPTypeHelper.SingleValue(aggMethodFactoryInner.ValueType.GetBoxedType());
                return new EnumSumScalarForge(numStreamsIncoming, aggMethodFactoryInner);
            }

            var first = (ExprDotEvalParamLambda) bodiesAndParameters[0];
            var aggMethodFactory = GetAggregatorFactory(first.BodyForge.EvaluationType);
            var returnType = aggMethodFactory.ValueType.GetBoxedType();
            TypeInfo = EPTypeHelper.SingleValue(returnType);
            if (inputEventType == null) {
                return new EnumSumScalarLambdaForge(
                    first.BodyForge,
                    first.StreamCountIncoming,
                    aggMethodFactory,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            return new EnumSumEventsForge(first.BodyForge, first.StreamCountIncoming, aggMethodFactory);
        }

        private static ExprDotEvalSumMethodFactory GetAggregatorFactory(Type evalType)
        {
            if (evalType.IsDecimal()) {
                return ExprDotEvalSumMethodFactoryDecimal.INSTANCE;
            }

            if (evalType.IsFloatingPointClass()) {
                return ExprDotEvalSumMethodFactoryDouble.INSTANCE;
            }

            if (evalType.IsBigInteger()) {
                return ExprDotEvalSumMethodFactoryBigInteger.INSTANCE;
            }

            if (evalType.GetBoxedType() == typeof(long)) {
                return ExprDotEvalSumMethodFactoryLong.INSTANCE;
            }

            return ExprDotEvalSumMethodFactoryInteger.INSTANCE;
        }

        private static void CodegenReturnSumOrNull(CodegenBlock block)
        {
            block.IfCondition(EqualsIdentity(Ref("cnt"), Constant(0)))
                .BlockReturn(ConstantNull())
                .MethodReturn(Ref("sum"));
        }
    }
} // end of namespace