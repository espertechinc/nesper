///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Type of schedule item.
    /// </summary>
    [Serializable]
    public enum ScheduleItemType
    {
        /// <summary>
        /// Wildcard means any value.
        /// </summary>
        WILDCARD,

        /// <summary>
        /// Last day of week or month.
        /// </summary>
        LASTDAY,

        /// <summary>
        /// Weekday (nearest to a date)
        /// </summary>
        WEEKDAY,

        /// <summary>
        /// Last weekday in a month
        /// </summary>
        LASTWEEKDAY
    }

    public static class ScheduleItemTypeExtensions
    {
        /// <summary>
        /// Returns the syntax string.
        /// </summary>
        /// <returns>
        /// syntax
        /// </returns>
        public static string GetSyntax(this ScheduleItemType value)
        {
            switch(value)
            {
                case ScheduleItemType.WILDCARD:
                    return ("*");
                case ScheduleItemType.LASTDAY:
                    return ("last");
                case ScheduleItemType.WEEKDAY:
                    return ("weekday");
                case ScheduleItemType.LASTWEEKDAY:
                    return ("lastweekday");
            }

            throw new ArgumentException();
        }
    }
}
