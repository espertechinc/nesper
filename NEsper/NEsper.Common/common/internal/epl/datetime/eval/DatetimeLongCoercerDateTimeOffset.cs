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
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeLongCoercerDateTimeOffset : DatetimeLongCoercer
    {
        public long Coerce(object date)
        {
            return CoerceToMillis((DateTimeOffset) date);
        }

        public CodegenExpression Codegen(
            CodegenExpression value,
            Type valueType,
            CodegenClassScope codegenClassScope)
        {
            if (valueType.GetBoxedType() != typeof(DateTimeOffset?)) {
                throw new IllegalStateException("Expected a DateTimeOffset type");
            }

            return StaticMethod(typeof(DatetimeLongCoercerDateTimeOffset), "CoerceToMillis", value);

            //CodegenExpression timeZoneField =
            //    codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            //return ExprDotMethodChain(value)
            //    .Add("atZone", ExprDotMethod(timeZoneField, "ToZoneId"))
            //    .Add("toInstant")
            //    .Add("toEpochMilli");
        }
        
        /// <summary>NOTE: Code-generation-invoked method, method name and parameter order matters</summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>millis</returns>
        public static long CoerceToMillis(DateTimeOffset? dateTime)
        {
            if (dateTime == null) {
                throw new ArgumentNullException(nameof(dateTime));
            }
            return dateTime.Value.InMillis();
        }
    }
} // end of namespace