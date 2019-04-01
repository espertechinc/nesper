///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeLongCoercerDateTime : DatetimeLongCoercer
    {
        public long Coerce(object date)
        {
            return CoerceToMillis((DateTime) date);
        }

        public CodegenExpression Codegen(
            CodegenExpression value,
            Type valueType,
            CodegenClassScope codegenClassScope)
        {
            if (valueType != typeof(DateTime)) {
                throw new IllegalStateException("Expected a DateTime type");
            }

            return ExprDotMethodChain(value).Add("toInstant").Add("toEpochMilli");
        }

        public static long CoerceToMillis(DateTime dateTime)
        {
            return dateTime.UtcMillis();
        }
    }
} // end of namespace