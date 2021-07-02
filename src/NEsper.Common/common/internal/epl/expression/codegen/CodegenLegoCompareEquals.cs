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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoCompareEquals
    {
        public static CodegenExpression CodegenEqualsNonNullNoCoerce(
            CodegenExpression lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType)
        {
            if (lhsType.IsValueType &&
                rhsType.IsValueType &&
                !lhsType.IsFloatingPointClass() &&
                !rhsType.IsFloatingPointClass()) {
                return EqualsIdentity(lhs, rhs);
            }

            if (lhsType.IsValueType && rhsType.IsValueType) {
                return Op(rhs, "==", lhs);
            }

            return StaticMethod<object>("Equals", lhs, rhs);
        }
    }
} // end of namespace