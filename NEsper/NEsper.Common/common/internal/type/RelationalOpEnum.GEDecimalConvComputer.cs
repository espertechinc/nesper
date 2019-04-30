///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnum
    {
        /// <summary>
        ///     Computer for relational op compare.
        /// </summary>
        public class GEDecimalConvComputer : Computer
        {
            private readonly SimpleNumberDecimalCoercer convOne;
            private readonly SimpleNumberDecimalCoercer convTwo;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public GEDecimalConvComputer(
                SimpleNumberDecimalCoercer convOne,
                SimpleNumberDecimalCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public bool Compare(
                object objOne,
                object objTwo)
            {
                var s1 = convOne.CoerceBoxedDecimal(objOne);
                var s2 = convTwo.CoerceBoxedDecimal(objTwo);
                return s1.CompareTo(s2) >= 0;
            }

            public CodegenExpression Codegen(
                CodegenExpressionRef lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType)
            {
                var leftConv = convOne.CoerceBoxedDecimalCodegen(lhs, lhsType);
                var rightConv = convTwo.CoerceBoxedDecimalCodegen(rhs, rhsType);
                return CodegenExpressionBuilder.Relational(
                    CodegenExpressionBuilder.ExprDotMethod(leftConv, "CompareTo", rightConv),
                    CodegenExpressionRelational.CodegenRelational.GE,
                    CodegenExpressionBuilder.Constant(0));
            }
        }
    }
}