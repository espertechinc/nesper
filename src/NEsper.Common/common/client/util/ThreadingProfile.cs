///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Threading profile.
    /// </summary>
    public enum ThreadingProfile
    {
        /// <summary>
        ///     Large for use with 100 threads or more. Please see the documentation for more information.
        /// </summary>
        LARGE,

        /// <summary>
        ///     For use with 100 threads or less.
        /// </summary>
        NORMAL
    }
} // end of namespace