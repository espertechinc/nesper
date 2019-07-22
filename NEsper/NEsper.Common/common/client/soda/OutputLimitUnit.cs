///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Unit for output rate limiting. </summary>
    public enum OutputLimitUnit
    {
        /// <summary>The time period unit. </summary>
        TIME_PERIOD,

        /// <summary>The number of events unit. </summary>
        EVENTS,

        /// <summary>The unit representing a when-expression. </summary>
        WHEN_EXPRESSION,

        /// <summary>The unit representing a crontab-at-expression. </summary>
        CRONTAB_EXPRESSION,

        /// <summary>The unit representing just after a time period or after a number of events. </summary>
        AFTER,

        /// <summary>The unit representing that output occurs when the context partition terminates. </summary>
        CONTEXT_PARTITION_TERM
    }

    public static class OutputLimitUnitExtensions
    {
        public static string GetText(this OutputLimitUnit value)
        {
            switch (value) {
                case OutputLimitUnit.TIME_PERIOD:
                    return ("timeperiod");

                case OutputLimitUnit.EVENTS:
                    return ("events");

                case OutputLimitUnit.WHEN_EXPRESSION:
                    return ("when");

                case OutputLimitUnit.CRONTAB_EXPRESSION:
                    return ("crontab");

                case OutputLimitUnit.AFTER:
                    return ("after");

                case OutputLimitUnit.CONTEXT_PARTITION_TERM:
                    return ("when terminated");
            }

            throw new ArgumentException();
        }
    }
}