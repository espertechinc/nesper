///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Enumeration for the type of arithmetic to use.
    /// </summary>
    public class MinMaxTypeEnum
    {
        /// <summary>
        ///     Max.
        /// </summary>
        public static readonly MinMaxTypeEnum MAX = new MinMaxTypeEnum("max");

        /// <summary>
        ///     Min.
        /// </summary>
        public static readonly MinMaxTypeEnum MIN = new MinMaxTypeEnum("min");

        private MinMaxTypeEnum(string expressionText)
        {
            ExpressionText = expressionText;
        }

        /// <summary>
        ///     Returns textual representation of enum.
        /// </summary>
        /// <returns>text for enum</returns>
        public string ExpressionText { get; }

        private static CodegenExpression CodegenMinMax(
            bool min, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope, ExprNode[] nodes, Type returnType)
        {
            var r0Type = nodes[0].Forge.EvaluationType;
            var r1Type = nodes[1].Forge.EvaluationType;
            if (r0Type == null || r1Type == null)
            {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                returnType, typeof(MaxComputerDoubleCoerce), codegenClassScope);
            var block = methodNode.Block;

            block.DeclareVar(
                r0Type, "r0", nodes[0].Forge.EvaluateCodegen(r0Type, methodNode, exprSymbol, codegenClassScope));
            if (!r0Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r0");
            }

            block.DeclareVar(
                r1Type, "r1", nodes[1].Forge.EvaluateCodegen(r1Type, methodNode, exprSymbol, codegenClassScope));
            if (!r1Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r1");
            }

            block.DeclareVarNoInit(returnType, "result");
            block.IfCondition(
                    CodegenCompareRelop(
                        returnType, min ? RelationalOpEnum.LT : RelationalOpEnum.GT, Ref("r0"), r0Type, Ref("r1"),
                        r1Type))
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r0"), r0Type, returnType))
                .IfElse()
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r1"), r1Type, returnType))
                .BlockEnd();

            for (var i = 2; i < nodes.Length; i++)
            {
                var nodeType = nodes[i].Forge.EvaluationType;
                var refname = "r" + i;
                block.DeclareVar(
                    nodeType, refname,
                    nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                if (!nodeType.IsPrimitive)
                {
                    block.IfRefNullReturnNull(refname);
                }

                block.IfCondition(
                        Not(
                            CodegenCompareRelop(
                                returnType, min ? RelationalOpEnum.LT : RelationalOpEnum.GT, Ref("result"), returnType,
                                Ref(refname), r1Type)))
                    .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref(refname), nodeType, returnType))
                    .BlockEnd();
            }

            block.MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }

        private static CodegenExpression CodegenCompareRelop(
            Type resultType, RelationalOpEnum op, CodegenExpressionRef lhs, Type lhsType, CodegenExpression rhs,
            Type rhsType)
        {
            return Op(lhs, op.ExpressionText, rhs);
        }

        private static CodegenExpression CodegenCompareCompareTo(CodegenExpression lhs, CodegenExpression rhs, bool max)
        {
            return Relational(
                ExprDotMethod(lhs, "compareTo", rhs),
                max
                    ? CodegenExpressionRelational.CodegenRelational.GT
                    : CodegenExpressionRelational.CodegenRelational.LT, Constant(0));
        }

        /// <summary>
        ///     Executes child expression nodes and compares results.
        /// </summary>
        public interface Computer
        {
            /// <summary>
            ///     Executes child expression nodes and compares results, returning the min/max.
            /// </summary>
            /// <param name="eventsPerStream">events per stream</param>
            /// <param name="isNewData">true if new data</param>
            /// <param name="exprEvaluatorContext">expression evaluation context</param>
            /// <returns>result</returns>
            object Execute(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
        }

        /// <summary>
        ///     Determines minimum using AsDouble.
        /// </summary>
        public class MinComputerDoubleCoerce : Computer
        {
            private readonly ExprEvaluator[] childNodes;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">array of expression nodes</param>
            public MinComputerDoubleCoerce(ExprEvaluator[] childNodes)
            {
                this.childNodes = childNodes;
            }

            public object Execute(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null)
                {
                    return null;
                }

                object result;
                if (valueChildOne.AsDouble() > valueChildTwo.AsDouble())
                {
                    result = valueChildTwo;
                }
                else
                {
                    result = valueChildOne;
                }

                for (var i = 2; i < childNodes.Length; i++)
                {
                    var valueChild = childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null)
                    {
                        return null;
                    }

                    if (valueChild.AsDouble() < result.AsDouble())
                    {
                        result = valueChild;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope, ExprNode[] nodes, Type returnType)
            {
                return CodegenMinMax(true, codegenMethodScope, exprSymbol, codegenClassScope, nodes, returnType);
            }
        }

        /// <summary>
        ///     Determines maximum using AsDouble.
        /// </summary>
        public class MaxComputerDoubleCoerce : Computer
        {
            private readonly ExprEvaluator[] childNodes;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">array of expression nodes</param>
            public MaxComputerDoubleCoerce(ExprEvaluator[] childNodes)
            {
                this.childNodes = childNodes;
            }

            public object Execute(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null)
                {
                    return null;
                }

                object result;
                if (valueChildOne.AsDouble() > valueChildTwo.AsDouble())
                {
                    result = valueChildOne;
                }
                else
                {
                    result = valueChildTwo;
                }

                for (var i = 2; i < childNodes.Length; i++)
                {
                    var valueChild = childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null)
                    {
                        return null;
                    }

                    if (valueChild.AsDouble() > result.AsDouble())
                    {
                        result = valueChild;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope, ExprNode[] nodes, Type returnType)
            {
                return CodegenMinMax(false, codegenMethodScope, exprSymbol, codegenClassScope, nodes, returnType);
            }
        }

        /// <summary>
        ///     Determines minimum/maximum using BigInteger.compareTo.
        /// </summary>
        public class ComputerBigIntCoerce : Computer
        {
            private readonly ExprEvaluator[] childNodes;
            private readonly SimpleNumberBigIntegerCoercer[] convertors;
            private readonly bool isMax;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">expressions</param>
            /// <param name="convertors">convertors to BigInteger</param>
            /// <param name="isMax">true if max, false if min</param>
            public ComputerBigIntCoerce(
                ExprEvaluator[] childNodes, SimpleNumberBigIntegerCoercer[] convertors, bool isMax)
            {
                this.childNodes = childNodes;
                this.convertors = convertors;
                this.isMax = isMax;
            }

            public object Execute(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null)
                {
                    return null;
                }

                var bigIntOne = convertors[0].CoerceBoxedBigInt(valueChildOne);
                var bigIntTwo = convertors[1].CoerceBoxedBigInt(valueChildTwo);

                BigInteger result;
                if (isMax && bigIntOne.CompareTo(bigIntTwo) > 0 ||
                    !isMax && bigIntOne.CompareTo(bigIntTwo) < 0)
                {
                    result = bigIntOne;
                }
                else
                {
                    result = bigIntTwo;
                }

                for (var i = 2; i < childNodes.Length; i++)
                {
                    var valueChild = childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null)
                    {
                        return null;
                    }

                    var bigInt = convertors[i].CoerceBoxedBigInt(valueChild);
                    if (isMax && result.CompareTo(bigInt) < 0 ||
                        !isMax && result.CompareTo(bigInt) > 0)
                    {
                        result = bigInt;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                bool max, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope, ExprNode[] nodes, SimpleNumberBigIntegerCoercer[] convertors)
            {
                var r0Type = nodes[0].Forge.EvaluationType;
                var r1Type = nodes[1].Forge.EvaluationType;
                if (r0Type == null || r1Type == null)
                {
                    return ConstantNull();
                }

                var methodNode = codegenMethodScope.MakeChild(
                    typeof(BigInteger), typeof(ComputerBigIntCoerce), codegenClassScope);
                var block = methodNode.Block;

                block.DeclareVar(
                    r0Type, "r0", nodes[0].Forge.EvaluateCodegen(r0Type, methodNode, exprSymbol, codegenClassScope));
                if (!r0Type.IsPrimitive)
                {
                    block.IfRefNullReturnNull("r0");
                }

                block.DeclareVar(
                    r1Type, "r1", nodes[1].Forge.EvaluateCodegen(r1Type, methodNode, exprSymbol, codegenClassScope));
                if (!r1Type.IsPrimitive)
                {
                    block.IfRefNullReturnNull("r1");
                }

                block.DeclareVar(typeof(BigInteger), "bi0", convertors[0].CoerceBoxedBigIntCodegen(Ref("r0"), r0Type));
                block.DeclareVar(typeof(BigInteger), "bi1", convertors[1].CoerceBoxedBigIntCodegen(Ref("r1"), r1Type));

                block.DeclareVarNoInit(typeof(BigInteger), "result");
                block.IfCondition(CodegenCompareCompareTo(Ref("bi0"), Ref("bi1"), max))
                    .AssignRef("result", Ref("bi0"))
                    .IfElse()
                    .AssignRef("result", Ref("bi1"))
                    .BlockEnd();

                for (var i = 2; i < nodes.Length; i++)
                {
                    var nodeType = nodes[i].Forge.EvaluationType;
                    var refnameNumber = "r" + i;
                    block.DeclareVar(
                        nodeType, refnameNumber,
                        nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                    if (!nodeType.IsPrimitive)
                    {
                        block.IfRefNullReturnNull(refnameNumber);
                    }

                    var refnameBigint = "bi" + i;
                    block.DeclareVar(
                        typeof(BigInteger), refnameBigint,
                        convertors[i].CoerceBoxedBigIntCodegen(Ref(refnameNumber), nodeType));
                    block.IfCondition(Not(CodegenCompareCompareTo(Ref("result"), Ref(refnameBigint), max)))
                        .AssignRef("result", Ref(refnameBigint))
                        .BlockEnd();
                }

                block.MethodReturn(Ref("result"));
                return LocalMethod(methodNode);
            }
        }

        /// <summary>
        ///     Determines minimum/maximum using decimal.compareTo.
        /// </summary>
        public class ComputerBigDecCoerce : Computer
        {
            private readonly ExprEvaluator[] childNodes;
            private readonly SimpleNumberDecimalCoercer[] convertors;
            private readonly bool isMax;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">expressions</param>
            /// <param name="convertors">convertors to decimal</param>
            /// <param name="isMax">true if max, false if min</param>
            public ComputerBigDecCoerce(
                ExprEvaluator[] childNodes, SimpleNumberDecimalCoercer[] convertors, bool isMax)
            {
                this.childNodes = childNodes;
                this.convertors = convertors;
                this.isMax = isMax;
            }

            public object Execute(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null)
                {
                    return null;
                }

                var decimalOne = valueChildOne.AsDecimal();
                var decimalTwo = valueChildTwo.AsDecimal();

                decimal result;
                if (isMax && decimalOne.CompareTo(decimalTwo) > 0 ||
                    !isMax && decimalOne.CompareTo(decimalTwo) < 0)
                {
                    result = decimalOne;
                }
                else
                {
                    result = decimalTwo;
                }

                for (var i = 2; i < childNodes.Length; i++)
                {
                    var valueChild = childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null)
                    {
                        return null;
                    }

                    var decimalResult = valueChild.AsDecimal();
                    if (isMax && result.CompareTo(decimalResult) < 0 ||
                        !isMax && result.CompareTo(decimalResult) > 0)
                    {
                        result = decimalResult;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                bool max, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope, ExprNode[] nodes, SimpleNumberDecimalCoercer[] convertors)
            {
                var r0Type = nodes[0].Forge.EvaluationType;
                var r1Type = nodes[1].Forge.EvaluationType;
                if (r0Type == null || r1Type == null)
                {
                    return ConstantNull();
                }

                var methodNode = codegenMethodScope.MakeChild(
                    typeof(decimal), typeof(ComputerBigDecCoerce), codegenClassScope);
                var block = methodNode.Block;

                block.DeclareVar(
                    r0Type, "r0", nodes[0].Forge.EvaluateCodegen(r0Type, methodNode, exprSymbol, codegenClassScope));
                if (!r0Type.IsPrimitive)
                {
                    block.IfRefNullReturnNull("r0");
                }

                block.DeclareVar(
                    r1Type, "r1", nodes[1].Forge.EvaluateCodegen(r1Type, methodNode, exprSymbol, codegenClassScope));
                if (!r1Type.IsPrimitive)
                {
                    block.IfRefNullReturnNull("r1");
                }

                block.DeclareVar(typeof(decimal), "bi0", convertors[0].CoerceBoxedDecimalCodegen(Ref("r0"), r0Type));
                block.DeclareVar(typeof(decimal), "bi1", convertors[1].CoerceBoxedDecimalCodegen(Ref("r1"), r1Type));

                block.DeclareVarNoInit(typeof(decimal), "result");
                block.IfCondition(CodegenCompareCompareTo(Ref("bi0"), Ref("bi1"), max))
                    .AssignRef("result", Ref("bi0"))
                    .IfElse()
                    .AssignRef("result", Ref("bi1"))
                    .BlockEnd();

                for (var i = 2; i < nodes.Length; i++)
                {
                    var nodeType = nodes[i].Forge.EvaluationType;
                    var refnameNumber = "r" + i;
                    block.DeclareVar(
                        nodeType, refnameNumber,
                        nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                    if (!nodeType.IsPrimitive)
                    {
                        block.IfRefNullReturnNull(refnameNumber);
                    }

                    var refnameBigint = "bi" + i;
                    block.DeclareVar(
                        typeof(decimal), refnameBigint,
                        convertors[i].CoerceBoxedDecimalCodegen(Ref(refnameNumber), nodeType));
                    block.IfCondition(Not(CodegenCompareCompareTo(Ref("result"), Ref(refnameBigint), max)))
                        .AssignRef("result", Ref(refnameBigint))
                        .BlockEnd();
                }

                block.MethodReturn(Ref("result"));
                return LocalMethod(methodNode);
            }
        }
    }
} // end of namespace