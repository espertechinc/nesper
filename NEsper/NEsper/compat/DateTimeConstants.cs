///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
    public class DateTimeConstants
    {
        /// <summary>
        /// Boundary offset represents a number of seconds that represent 24 hours worth of milliseconds.
        /// All times are aligned to UTC time of zero ticks.  However, during conversion, its common for
        /// the time to be shifted due to an offset from UTC.  This causes the number of milliseconds to
        /// actually become negative.  The boundary offset represents a number of milliseconds required
        /// to handle values from zero and greater from a UTC standpoint.
        /// </summary>
        public const long Boundary = 86400 * 1000;
    }
}
