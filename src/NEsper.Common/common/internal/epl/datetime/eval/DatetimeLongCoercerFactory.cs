///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeLongCoercerFactory
    {
        private static readonly DatetimeLongCoercerLong DATETIME_LONG_COERCER_LONG =
            new DatetimeLongCoercerLong();

        private static readonly DatetimeLongCoercerDateTime DATETIME_LONG_COERCER_DATETIME =
            new DatetimeLongCoercerDateTime();

        private static readonly DatetimeLongCoercerDateTimeOffset DATETIME_LONG_COERCER_DATETIME_OFFSET =
            new DatetimeLongCoercerDateTimeOffset();

        private static readonly DatetimeLongCoercerDateTimeEx DATETIME_LONG_COERCER_DTX =
            new DatetimeLongCoercerDateTimeEx();

        public static DatetimeLongCoercer GetCoercer(Type clazz)
        {
            clazz = clazz.GetBoxedType();

            if (clazz == typeof(DateTime?)) {
                return DATETIME_LONG_COERCER_DATETIME;
            }

            if (clazz == typeof(DateTimeOffset?)) {
                return DATETIME_LONG_COERCER_DATETIME_OFFSET;
            }

            if (clazz == typeof(DateTimeEx)) {
                return DATETIME_LONG_COERCER_DTX;
            }

            return DATETIME_LONG_COERCER_LONG;
        }
    }
} // end of namespace