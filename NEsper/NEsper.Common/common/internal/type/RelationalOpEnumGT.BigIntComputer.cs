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

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnumGT
    {
        /// <summary>
        /// Computer for relational op compare.
        /// </summary>
        public class BigIntComputer : RelationalOpEnumComputer
        {
            public bool Compare(
                object objOne,
                object objTwo)
            {
                BigInteger s1 = (BigInteger) objOne;
                BigInteger s2 = (BigInteger) objTwo;
                int result = s1.CompareTo(s2);
                return result > 0;
            }

            public CodegenExpression Codegen(
                CodegenExpression lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType)
            {
                return RelationalOpEnumExtensions.CodegenComparable(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GT);
            }
        }
    }
}
