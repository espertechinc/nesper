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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeLongCoercerLong : DatetimeLongCoercer
    {
        public long Coerce(object value)
        {
            if (value is long) {
                return (long) value;
            }

            if (value is int) {
                return (int) value;
            }

            if (value is DateTime) {
                return ((DateTime) value).UtcMillis();
            }

            if (value is DateTimeOffset) {
                return ((DateTimeOffset) value).UtcMillis();
            }

            if (value is DateTimeEx) {
                return ((DateTimeEx) value).UtcMillis;
            }

            throw new ArgumentException("invalid value for datetime", "value");
        }

        public CodegenExpression Codegen(
            CodegenExpression value,
            Type valueType,
            CodegenClassScope codegenClassScope)
        {
            return SimpleNumberCoercerFactory.CoercerLong.CodegenLong(value, valueType);
        }
    }
}