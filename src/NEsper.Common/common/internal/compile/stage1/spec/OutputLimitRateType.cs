///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Enum for the type of rate for output-rate limiting. </summary>
    public enum OutputLimitRateType
    {
        /// <summary>Output by number of events. </summary>
        EVENTS,

        /// <summary>Output following a crontab-like schedule. </summary>
        CRONTAB,

        /// <summary>Output when an expression turns true. </summary>
        WHEN_EXPRESSION,

        /// <summary>Output based on a time period passing. </summary>
        TIME_PERIOD,

        /// <summary>Output after a given time period </summary>
        AFTER,

        /// <summary>Output upon context partition (agent instance) termination </summary>
        TERM
    }
}