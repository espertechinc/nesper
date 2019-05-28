///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Numerics;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
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
    public class ExprDotForgeSumOf : ExprDotForgeEnumMethodBase
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
                enumMethodUsedName, goesToNames, inputEventType, collectionComponentType, statementRawInfo, services);
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
                    first.BodyForge, first.StreamCountIncoming, aggMethodFactory,
                    (ObjectArrayEventType) first.GoesToTypes[0]);
            }

            return new EnumSumEventsForge(first.BodyForge, first.StreamCountIncoming, aggMethodFactory);
        }

        private static ExprDotEvalSumMethodFactory GetAggregatorFactory(Type evalType)
        {
            if (evalType.IsFloatingPointClass()) {
                return ExprDotEvalSumMethodFactoryDouble.INSTANCE;
            }

            if (evalType == typeof(decimal)) {
                return ExprDotEvalSumMethodFactoryDecimal.INSTANCE;
            }

            if (evalType == typeof(BigInteger)) {
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

        internal class ExprDotEvalSumMethodFactoryDouble : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryDouble
                INSTANCE = new ExprDotEvalSumMethodFactoryDouble();

            private ExprDotEvalSumMethodFactoryDouble()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodDouble();

            public Type ValueType => typeof(double?);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar(typeof(double), "sum", Constant(0));
                block.DeclareVar(typeof(long), "cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", value);
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", Cast(typeof(double?), value));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }

        internal class ExprDotEvalSumMethodDouble : ExprDotEvalSumMethod
        {
            internal long cnt;
            internal double sum;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsDouble();
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }

        internal class ExprDotEvalSumMethodFactoryDecimal : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryDecimal INSTANCE =
                new ExprDotEvalSumMethodFactoryDecimal();

            private ExprDotEvalSumMethodFactoryDecimal()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodBigDecimal();

            public Type ValueType => typeof(decimal);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar(typeof(decimal), "sum", NewInstance<decimal>(Constant(0d)));
                block.DeclareVar(typeof(long), "cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt")
                    .AssignRef("sum", ExprDotMethod(Ref("sum"), "add", value));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt")
                    .AssignRef("sum", ExprDotMethod(Ref("sum"), "add", Cast(typeof(decimal), value)));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }

        internal class ExprDotEvalSumMethodBigDecimal : ExprDotEvalSumMethod
        {
            internal long cnt;
            internal decimal sum;

            public ExprDotEvalSumMethodBigDecimal()
            {
                sum = decimal.Zero;
            }

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsDecimal();
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }

        internal class ExprDotEvalSumMethodFactoryBigInteger : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryBigInteger INSTANCE =
                new ExprDotEvalSumMethodFactoryBigInteger();

            private ExprDotEvalSumMethodFactoryBigInteger()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodBigInteger();

            public Type ValueType => typeof(BigInteger);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar(typeof(BigInteger), "sum", StaticMethod(typeof(BigInteger), "valueOf", Constant(0)));
                block.DeclareVar(typeof(long), "cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt")
                    .AssignRef("sum", ExprDotMethod(Ref("sum"), "add", value));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt")
                    .AssignRef("sum", ExprDotMethod(Ref("sum"), "add", Cast(typeof(BigInteger), value)));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }

        internal class ExprDotEvalSumMethodBigInteger : ExprDotEvalSumMethod
        {
            internal long cnt;
            internal BigInteger sum;

            public ExprDotEvalSumMethodBigInteger()
            {
                sum = BigInteger.Zero;
            }

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum = BigInteger.Add(sum, (BigInteger) @object);
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }

        internal class ExprDotEvalSumMethodFactoryLong : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryLong INSTANCE = new ExprDotEvalSumMethodFactoryLong();

            private ExprDotEvalSumMethodFactoryLong()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodLong();

            public Type ValueType => typeof(long);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar(typeof(long), "sum", Constant(0));
                block.DeclareVar(typeof(long), "cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", value);
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", Cast(typeof(long), value));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }

        internal class ExprDotEvalSumMethodLong : ExprDotEvalSumMethod
        {
            internal long cnt;
            internal long sum;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsLong();
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }

        internal class ExprDotEvalSumMethodFactoryInteger : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryInteger INSTANCE =
                new ExprDotEvalSumMethodFactoryInteger();

            private ExprDotEvalSumMethodFactoryInteger()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodInteger();

            public Type ValueType => typeof(int);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar(typeof(int), "sum", Constant(0));
                block.DeclareVar(typeof(long), "cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", value);
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.Increment("cnt");
                block.AssignCompound("sum", "+", Cast(typeof(int), value));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }

        internal class ExprDotEvalSumMethodInteger : ExprDotEvalSumMethod
        {
            internal long cnt;
            internal int sum;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsInt();
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }
    }
} // end of namespace