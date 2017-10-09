///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.eval
{
    public class DatetimeLongCoercerLong : DatetimeLongCoercer
    {
        public long Coerce(Object value)
        {
            if (value is long)
                return ((long) value);
            if (value is int)
                return ((int) value);
            if (value is DateTime)
                return ((DateTime) value).UtcMillis();
            if (value is DateTimeOffset)
                return ((DateTimeOffset) value).TimeInMillis();
            if (value is DateTimeEx)
                return ((DateTimeEx) value).TimeInMillis;

            throw new ArgumentException("invalid value for datetime", "value");
        }
    }
}
