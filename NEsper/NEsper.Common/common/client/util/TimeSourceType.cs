///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Time source type.
    /// </summary>
    public enum TimeSourceType
    {
        /// <summary>
        ///     Millisecond time source type with time originating from System.currentTimeMillis
        /// </summary>
        MILLI,

        /// <summary>
        ///     Nanosecond time source from a wallclock-adjusted System.nanoTime
        /// </summary>
        NANO
    }
} // end of namespace