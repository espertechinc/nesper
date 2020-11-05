///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.datetimemethod
{
    /// <summary>
    ///     For adding a date-time method that modifies the date-time value and that return a result of the same type
    ///     as the date-time value.
    ///     <para />
    ///     long-type and Date-type values are automatically converted from and to Calendar.
    /// </summary>
    public class DateTimeMethodOpsModify : DateTimeMethodOps
    {
        /// <summary>
        ///     Returns the information how DateTimeEx modify is provided
        /// </summary>
        /// <value>mode</value>
        public DateTimeMethodMode DateTimeExOp { get; set; }

        /// <summary>
        ///     Returns the information how DateTimeOffset modify is provided
        /// </summary>
        /// <value>mode</value>
        public DateTimeMethodMode DateTimeOffsetOp { get; set; }

        /// <summary>
        ///     Returns the information how DateTime modify is provided
        /// </summary>
        /// <value>mode</value>
        public DateTimeMethodMode DateTimeOp { get; set; }
    }
} // end of namespace