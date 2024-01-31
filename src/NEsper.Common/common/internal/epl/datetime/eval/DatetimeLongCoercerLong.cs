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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeLongCoercerLong : DatetimeLongCoercer
    {
        public long Coerce(object value)
        {
            if (value is long l) {
                return l;
            }

            if (value is int i) {
                return i;
            }

            if (value is DateTime time) {
                return time.UtcMillis();
            }

            if (value is DateTimeOffset offset) {
                return offset.UtcMillis();
            }

            if (value is DateTimeEx ex) {
                return ex.UtcMillis;
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