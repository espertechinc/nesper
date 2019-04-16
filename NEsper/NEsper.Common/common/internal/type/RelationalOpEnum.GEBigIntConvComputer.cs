///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnum
    {
        /// <summary>
        /// Computer for relational op compare.
        /// </summary>
        public class GEBigIntConvComputer : Computer
        {
            private readonly BigIntegerCoercer convOne;
            private readonly BigIntegerCoercer convTwo;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="convOne">convertor for LHS</param>
            /// <param name="convTwo">convertor for RHS</param>
            public GEBigIntConvComputer(
                BigIntegerCoercer convOne,
                BigIntegerCoercer convTwo)
            {
                this.convOne = convOne;
                this.convTwo = convTwo;
            }

            public bool Compare(
                object objOne,
                object objTwo)
            {
                BigInteger s1 = convOne.CoerceBoxedBigInt((object) objOne);
                BigInteger s2 = convTwo.CoerceBoxedBigInt((object) objTwo);
                return s1.CompareTo(s2) >= 0;
            }

            public CodegenExpression Codegen(
                CodegenExpressionRef lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType)
            {
                return CodegenBigIntConv(lhs, lhsType, rhs, rhsType, convOne, convTwo, CodegenExpressionRelational.CodegenRelational.GE);
            }
        }
    }
}