///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnumGE
    {
        /// <summary>
        ///     Computer for relational op compare.
        /// </summary>
        public class DecimalComputer : RelationalOpEnumComputer
        {
            public bool Compare(
                object objOne,
                object objTwo)
            {
                var s1 = objOne.AsDecimal();
                var s2 = objTwo.AsDecimal();
                return s1.CompareTo(s2) >= 0;
            }

            public CodegenExpression Codegen(
                CodegenExpression lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType)
            {
                return RelationalOpEnumExtensions.CodegenComparable(lhs, rhs, CodegenExpressionRelational.CodegenRelational.GE);
            }
        }
    }
}
