///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Enumeration for special keywords in crontab timer.
    /// </summary>
    public enum CronOperatorEnum
    {
        /// <summary>Last day of week or month. </summary>
        LASTDAY,

        /// <summary>Weekday (nearest to a date) </summary>
        WEEKDAY,

        /// <summary>Last weekday in a month </summary>
        LASTWEEKDAY
    }

    public static class CronOperatorEnumExtensions
    {
        /// <summary>Returns the syntax string for the operator. </summary>
        /// <returns>syntax string</returns>
        public static string GetSyntax(this CronOperatorEnum value)
        {
            switch (value) {
                case CronOperatorEnum.LASTDAY:
                    return ("last");
                case CronOperatorEnum.WEEKDAY:
                    return ("weekday");
                case CronOperatorEnum.LASTWEEKDAY:
                    return ("lastweekday");
            }

            throw new ArgumentException();
        }
    }
}