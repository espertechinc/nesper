///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.settings;
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
            if (valueType.GetBoxedType() != typeof(DateTime?)) {
                throw new IllegalStateException("Expected a DateTime type, but received \"" + valueType.TypeSafeName() + "\"");
            }

            var timeZoneField = codegenClassScope.AddOrGetDefaultFieldSharable(
                RuntimeSettingsTimeZoneField.INSTANCE);
            return StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", value, timeZoneField);
            //return ExprDotMethodChain(value).Add("toInstant").Add("toEpochMilli");
        }

        public static long CoerceToMillis(DateTime? dateTime)
        {
            if (dateTime == null) {
                throw new ArgumentNullException(nameof(dateTime), nameof(dateTime) + " cannot be null");
            }

            return CoerceToMillis(dateTime.Value);
        }

        public static long CoerceToMillis(DateTime dateTime)
        {
            return dateTime.UtcMillis();
        }

        public static long CoerceToMillis(DateTime? dateTime, TimeZoneInfo timeZoneInfo)
        {
            if (dateTime == null)
            {
                throw new ArgumentNullException(nameof(dateTime), nameof(dateTime) + " cannot be null");
            }

            return dateTime
                .Value
                .ToDateTimeOffset(timeZoneInfo)
                .InMillis();
        }
        
        public static long CoerceToMillis(DateTime dateTime, TimeZoneInfo timeZoneInfo)
        {
            return dateTime
                .ToDateTimeOffset(timeZoneInfo)
                .InMillis();
                
                //.ToUniversalTime()
                //.UtcDateTime
                //.UtcMillis();
        }
    }
} // end of namespace