///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.type
{
    public partial class RelationalOpEnumLT
    {
        /// <summary>
        /// Computer for relational op compare.
        /// </summary>
        public class LongComputer : RelationalOpEnumComputer
        {
            public bool Compare(
                object objOne,
                object objTwo)
            {
                var s1 = objOne;
                var s2 = objTwo;
                return s1.AsInt64() < s2.AsInt64();
            }

            public CodegenExpression Codegen(
                CodegenExpression lhs,
                Type lhsType,
                CodegenExpression rhs,
                Type rhsType, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
            {
                return RelationalOpEnumExtensions.CodegenLong(
                    lhs,
                    lhsType,
                    rhs,
                    rhsType,
                    RelationalOpEnum.LT);
            }
        }
    }
}