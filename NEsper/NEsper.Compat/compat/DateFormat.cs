///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    public interface DateFormat
    {
        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="timeInMillis">The time in millis.</param>
        /// <returns></returns>
        string Format(long? timeInMillis);
        
        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        string Format(DateTime? dateTime);
        
        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        string Format(DateTimeOffset? dateTime);
        
        /// <summary>
        /// Formats the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        string Format(DateTimeEx dateTime);

        /// <summary>
        /// Parses the specified date time string.
        /// </summary>
        /// <param name="dateTimeString">The date time string.</param>
        /// <returns></returns>
        DateTimeEx Parse(string dateTimeString);
    }
}
