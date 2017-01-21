///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.datetime.eval
{
    public class DatetimeLongCoercerFactory
    {
        private static readonly DatetimeLongCoercerLong DatetimeLongCoercerLong = new DatetimeLongCoercerLong();
        private static readonly DatetimeLongCoercerDate DatetimeLongCoercerDate = new DatetimeLongCoercerDate();

        public static DatetimeLongCoercer GetCoercer(Type clazz)
        {
            clazz = clazz.GetBoxedType();
            if (clazz == typeof(DateTime?) || clazz == typeof(DateTimeOffset?))
            {
                return DatetimeLongCoercerDate;
            }
            return DatetimeLongCoercerLong;
        }
    }
}